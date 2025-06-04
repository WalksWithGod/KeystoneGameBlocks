﻿#if ENTITIES
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Common.CommandTrees;
using System.Data.Metadata.Edm;
using System.Linq;

namespace Npgsql.SqlGenerators
{
    internal abstract class SqlBaseGenerator : DbExpressionVisitor<VisitedExpression>
    {
        internal class IdentifierEqualityComparer : IEqualityComparer<string>
        {
            #region IEqualityComparer<string> Members

            public bool Equals(string x, string y)
            {
                // they are equal if they exactly match
                if (x == y)
                    return true;
                // if either of them are null, they are
                // not equal (both are not null because
                // then they would be equal).  Test this
                // early to avoid NullReferenceException.
                if (x == null || y == null)
                    return false;
                // if they are both quoted or unquoted
                // then they are definately different
                if (x[0] != '"' && y[0] != '"' ||
                    x[0] == '"' && y[0] == '"')
                    return false;
                // one is quoted while the other is not
                // simplify to the unquoted form
                return x.Replace("\"", "") == y.Replace("\"", "");
            }

            public int GetHashCode(string obj)
            {
                if (obj == null)
                    throw new ArgumentNullException();
                // normal hashcode if the value is unquoted
                if (obj[0] != '"')
                    return obj.GetHashCode();
                // need to remove quotes to get the right hashcode
                // since the hashcodes need to match for equivalent values.
                return obj.Replace("\"", "").GetHashCode();
            }

            #endregion
        }

        // contains unquoted identifiers, but use a custom IEqualityComparer to allow tests against quoted identifiers
        protected Dictionary<string, string> _variableSubstitution = new Dictionary<string, string>(new IdentifierEqualityComparer());
        protected Stack<string> _projectVarName = new Stack<string>();
        protected Stack<string> _filterVarName = new Stack<string>();
        // store off current projection so the top one is the one being built
        private Stack<ProjectionExpression> _projectExpressions = new Stack<ProjectionExpression>();

        protected SqlBaseGenerator()
        {
        }

        private void SubstituteFilterVar(string value)
        {
            if (_filterVarName.Count != 0)
                _variableSubstitution[_filterVarName.Peek()] = value;
        }

        public override VisitedExpression Visit(DbVariableReferenceExpression expression)
        {
            return new VariableReferenceExpression(expression.VariableName, _variableSubstitution);
        }

        public override VisitedExpression Visit(DbUnionAllExpression expression)
        {
            // UNION ALL keyword
            return new CombinedProjectionExpression(expression.Left.Accept(this),
                "UNION ALL", expression.Right.Accept(this));
        }

        public override VisitedExpression Visit(DbTreatExpression expression)
        {
            throw new NotImplementedException();
        }

        public override VisitedExpression Visit(DbSkipExpression expression)
        {
            // almost the opposite of limit, need to skip first.
            VisitedExpression skip = expression.Input.Expression.Accept(this);
            InputExpression input;
            if (!(skip is ProjectionExpression) || !(((ProjectionExpression)skip).From is FromExpression))
            {
                input = CheckedConvertFrom(skip, expression.Input.VariableName);
                // return this value
                skip = input;
            }
            else
            {
                input = ((ProjectionExpression)skip).From;
                if (_variableSubstitution.ContainsKey(((FromExpression)input).Name))
                    _variableSubstitution[expression.Input.VariableName] = _variableSubstitution[((FromExpression)input).Name];
                else
                    _variableSubstitution[expression.Input.VariableName] = ((FromExpression)input).Name;
            }
            OrderByExpression orderBy = new OrderByExpression();
            foreach (var order in expression.SortOrder)
            {
                orderBy.AppendSort(order.Expression.Accept(this), order.Ascending);
            }
            input.OrderBy = orderBy;
            input.Skip = new SkipExpression(expression.Count.Accept(this));
            // ensure skip variable has the right name
            if (_variableSubstitution.ContainsKey(_projectVarName.Peek()))
                _variableSubstitution[expression.Input.VariableName] = _variableSubstitution[_projectVarName.Peek()];
            return skip;
        }

        public override VisitedExpression Visit(DbSortExpression expression)
        {
            // order by
            PushFilterVar(expression.Input.VariableName);
            VisitedExpression inputExpression = expression.Input.Expression.Accept(this);
            InputExpression from = inputExpression as InputExpression;
            if (from == null)
            {
                from = new FromExpression(inputExpression, expression.Input.VariableName);
                _variableSubstitution[_projectVarName.Peek()] = expression.Input.VariableName;
                SubstituteFilterVar(expression.Input.VariableName);
            }
            else
            {
                if (from is FromExpression)
                {
                    SubstituteFilterVar(((FromExpression)from).Name);
                }
            }
            PopFilterVar();
            from.OrderBy = new OrderByExpression();
            foreach (var order in expression.SortOrder)
            {
                from.OrderBy.AppendSort(order.Expression.Accept(this), order.Ascending);
            }
            return from;
        }

        public override VisitedExpression Visit(DbScanExpression expression)
        {
            MetadataProperty metadata;
            string tableName;
            string overrideTable = "http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator:Name";
            if (expression.Target.MetadataProperties.TryGetValue(overrideTable, false, out metadata) && metadata.Value != null)
            {
                tableName = metadata.Value.ToString();
            }
            else if (expression.Target.MetadataProperties.TryGetValue("Table", false, out metadata) && metadata.Value != null)
            {
                tableName = metadata.Value.ToString();
            }
            else
            {
                tableName = expression.Target.Name;
            }
            if (_projectVarName.Count != 0) // this can happen in dml
                _variableSubstitution[_projectVarName.Peek()] = tableName;
            SubstituteFilterVar(expression.Target.Name);
            if (expression.Target.MetadataProperties.Contains("DefiningQuery"))
            {
                MetadataProperty definingQuery = expression.Target.MetadataProperties.GetValue("DefiningQuery", false);
                if (definingQuery.Value != null)
                {
                    return new ScanExpression("(" + definingQuery.Value + ")", expression.Target);
                }
            }

            ScanExpression scan;
            string overrideSchema = "http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator:Schema";
            if (expression.Target.MetadataProperties.TryGetValue(overrideSchema, false, out metadata) && metadata.Value != null)
            {
                scan = new ScanExpression(QuoteIdentifier(metadata.Value.ToString()) + "." + QuoteIdentifier(tableName), expression.Target);
            }
            else if (expression.Target.MetadataProperties.TryGetValue("Schema", false, out metadata) && metadata.Value != null)
            {
                scan = new ScanExpression(QuoteIdentifier(metadata.Value.ToString()) + "." + QuoteIdentifier(tableName), expression.Target);
            }
            else
            {
                scan = new ScanExpression(QuoteIdentifier(expression.Target.EntityContainer.Name) + "." + QuoteIdentifier(tableName), expression.Target);
            }

            return scan;
        }

        public override VisitedExpression Visit(DbRelationshipNavigationExpression expression)
        {
            throw new NotImplementedException();
        }

        public override VisitedExpression Visit(DbRefExpression expression)
        {
            throw new NotImplementedException();
        }

        public override VisitedExpression Visit(DbQuantifierExpression expression)
        {
            // TODO: EXISTS or NOT EXISTS depending on expression.ExpressionKind
            // comes with it's built in test (subselect for EXISTS)
            throw new NotImplementedException();
        }

        public override VisitedExpression Visit(DbProjectExpression expression)
        {
            ProjectionExpression project = expression.Projection.Accept(this) as ProjectionExpression;
            // TODO: test if this should always be true
            if (project == null)
                throw new NotSupportedException();
            PushProjectVar(expression.Input.VariableName);
            project.From = CheckedConvertFrom(expression.Input.Expression.Accept(this), expression.Input.VariableName);
            PopProjectVar();

            return project;
        }

        internal InputExpression CheckedConvertFrom(VisitedExpression fromExpression, string variableName)
        {
            InputExpression result = fromExpression as InputExpression;
            if (result == null)
            {
                // if fromExpression is at the top of _projectExpressions, it should be popped
                // so that the previous expression is at the top
                // A projection is either the root VisitedExpression or is a nested select
                // and will always be converted to a from
                // at this point the projection is complete and is no longer considered "current"
                if (object.ReferenceEquals(fromExpression, _projectExpressions.Peek()))
                    _projectExpressions.Pop();
                result = new FromExpression(fromExpression, variableName);
                if (string.IsNullOrEmpty(variableName)) variableName = ((FromExpression)result).Name;
                _variableSubstitution[_projectVarName.Peek()] = variableName;
                SubstituteFilterVar(variableName);
            }
            return result;
        }

        public override VisitedExpression Visit(DbParameterReferenceExpression expression)
        {
            // use parameter in sql
            return new LiteralExpression("@" + expression.ParameterName);
        }

        public override VisitedExpression Visit(DbOrExpression expression)
        {
            return new BooleanExpression("OR", expression.Left.Accept(this), expression.Right.Accept(this));
        }

        public override VisitedExpression Visit(DbOfTypeExpression expression)
        {
            throw new NotImplementedException();
        }

        public override VisitedExpression Visit(DbNullExpression expression)
        {
            // select does something different here.  But insert, update, delete, and functions can just use
            // a NULL literal.
            return new LiteralExpression("NULL");
        }

        public override VisitedExpression Visit(DbNotExpression expression)
        {
            // argument can be a "NOT EXISTS" or similar operator that can be negated.
            // Convert the not if that's the case
            VisitedExpression argument = expression.Argument.Accept(this);
            NegatableExpression negatable = argument as NegatableExpression;
            if (negatable != null)
            {
                negatable.Negate();
                return negatable;
            }
            else
            {
                return new NegateExpression(argument);
            }
        }

        public override VisitedExpression Visit(DbNewInstanceExpression expression)
        {
            RowType rowType = expression.ResultType.EdmType as RowType;

            if (rowType != null)
            {
                // should be the child of a project
                // which means it's a select
                ProjectionExpression visitedExpression = new ProjectionExpression();
                _projectExpressions.Push(visitedExpression);
                for (int i = 0; i < rowType.Properties.Count && i < expression.Arguments.Count; ++i)
                {
                    visitedExpression.AppendColumn(new ColumnExpression(expression.Arguments[i].Accept(this), rowType.Properties[i].Name));
                }

                return visitedExpression;
            }
            else if (expression.ResultType.EdmType is CollectionType)
            {
                // TODO: handle no arguments
                VisitedExpression previousExpression = null;
                VisitedExpression resultExpression = null;
                foreach (var arg in expression.Arguments)
                {
                    ProjectionExpression visitedExpression = new ProjectionExpression();
                    var visitedColumn = arg.Accept(this);
                    if (!(visitedColumn is ColumnExpression))
                        visitedColumn = new ColumnExpression(visitedColumn, "C");
                    visitedExpression.AppendColumn((ColumnExpression)visitedColumn);
                    if (previousExpression != null)
                    {
                        resultExpression = new CombinedProjectionExpression(previousExpression, "UNION ALL", visitedExpression);
                    }
                    else
                    {
                        resultExpression = visitedExpression;
                    }
                    previousExpression = visitedExpression;
                }
                return resultExpression;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public override VisitedExpression Visit(DbLimitExpression expression)
        {
            // Need more complex operation where ties are needed
            // TODO: implement WithTies
            if (expression.WithTies)
                throw new NotSupportedException();
            // limit expressions should be structured like where clauses
            // see Visit(DbFilterExpression)
            VisitedExpression limit = expression.Argument.Accept(this);
            InputExpression input;
            if (!(limit is ProjectionExpression))
            {
                input = CheckedConvertFrom(limit, null);
                // return this value
                limit = input;
            }
            else
            {
                input = ((ProjectionExpression)limit).From;
            }
            input.Limit = new LimitExpression(expression.Limit.Accept(this));
            return limit;
        }

        public override VisitedExpression Visit(DbLikeExpression expression)
        {
            // LIKE keyword
            // also uses ESCAPE
            // ESCAPE may be way of identifying wild cards
            // TODO: enhance this.  Only supporting simple case for now
            return new BooleanExpression("LIKE", expression.Argument.Accept(this), expression.Pattern.Accept(this));
        }

        public override VisitedExpression Visit(DbJoinExpression expression)
        {
            // table join
            // the following code works ok, but the rest of the code doesn't work well in a join
            // need to take _projectVarName and append .left then do the same for right
            // use this to do combo variable substitution
            return new JoinExpression(VisitJoinPart(expression.Left),
                expression.ExpressionKind,
                VisitJoinPart(expression.Right),
                expression.JoinCondition.Accept(this));
        }

        private InputExpression VisitJoinPart(DbExpressionBinding joinPart)
        {
            PushProjectVar(joinPart.VariableName);
            string variableName = null;
            VisitedExpression joinPartExpression = null;
            if (joinPart.Expression is DbFilterExpression)
            {
                joinPartExpression = VisitFilterExpression((DbFilterExpression)joinPart.Expression, true);
            }
            else
            {
                joinPartExpression = joinPart.Expression.Accept(this);
            }
            if (joinPartExpression is FromExpression)
            {
                variableName = ((FromExpression)joinPartExpression).Name;
            }
            else if (!(joinPartExpression is JoinExpression)) // don't alias join expressions at all
            {
                joinPartExpression = new FromExpression(joinPartExpression, joinPart.VariableName);
                variableName = joinPart.VariableName;
            }
            PopProjectVar();
            if (variableName != null)
            {
                _variableSubstitution[_projectVarName.Peek()] = variableName;
                string[] dottedNames = _projectVarName.ToArray();
                // reverse because the stack has them last in first out
                Array.Reverse(dottedNames);
                SubstituteAllNames(dottedNames, joinPart.VariableName, variableName);
                //if (_filterVarName.Count != 0)
                //{
                //    dottedNames = _filterVarName.ToArray();
                //    // reverse because the stack has them last in first out
                //    Array.Reverse(dottedNames);
                //    SubstituteAllNames(dottedNames, joinPart.VariableName, variableName);
                //}
                SubstituteFilterNames(joinPart.VariableName, variableName);
                _variableSubstitution[joinPart.VariableName] = variableName;
            }
            return (InputExpression)joinPartExpression;
        }

        private void SubstituteAllNames(string[] dottedNames, string joinPartVariableName, string variableName)
        {
            int nameCount = dottedNames.Length;
            for (int i = 0; i < dottedNames.Length; ++i)
            {
                _variableSubstitution[string.Join(".", dottedNames, i, nameCount - i) + "." + joinPartVariableName] = variableName;
            }
        }

        public override VisitedExpression Visit(DbIsOfExpression expression)
        {
            throw new NotImplementedException();
        }

        public override VisitedExpression Visit(DbIsNullExpression expression)
        {
            return new IsNullExpression(expression.Argument.Accept(this));
        }

        public override VisitedExpression Visit(DbIsEmptyExpression expression)
        {
            // NOT EXISTS
            return new ExistsExpression(expression.Argument.Accept(this)).Negate();
        }

        public override VisitedExpression Visit(DbIntersectExpression expression)
        {
            // INTERSECT keyword
            return new CombinedProjectionExpression(expression.Left.Accept(this),
                "INTERSECT", expression.Right.Accept(this));
        }

        public override VisitedExpression Visit(DbGroupByExpression expression)
        {
            // complicated
            // GROUP BY expression
            // first implementation this is a COUNT(column) query ???
            //EnterNewVariableScope();
            ProjectionExpression projectExpression = new ProjectionExpression();
            _projectExpressions.Push(projectExpression);
            GroupByExpression groupByExpression = new GroupByExpression();
            RowType rowType = ((CollectionType)(expression.ResultType.EdmType)).TypeUsage.EdmType as RowType;
            int columnIndex = 0;
            foreach (var key in expression.Keys)
            {
                VisitedExpression keyColumnExpression = key.Accept(this);
                projectExpression.AppendColumn(new ColumnExpression(keyColumnExpression, rowType.Properties[columnIndex].Name));
                groupByExpression.AppendGroupingKey(keyColumnExpression);
                ++columnIndex;
            }
            foreach (var ag in expression.Aggregates)
            {
                DbFunctionAggregate function = ag as DbFunctionAggregate;
                if (function == null)
                    throw new NotSupportedException();
                VisitedExpression functionExpression = VisitFunction(function);
                projectExpression.AppendColumn(new ColumnExpression(functionExpression, rowType.Properties[columnIndex].Name));
                ++columnIndex;
            }
            PushProjectVar(expression.Input.GroupVariableName);
            PushFilterVar(expression.Input.VariableName);
            projectExpression.From = CheckedConvertFrom(expression.Input.Expression.Accept(this), expression.Input.GroupVariableName);
            projectExpression.From.GroupBy = groupByExpression;
            if (_variableSubstitution.ContainsKey(_projectVarName.Peek()))
            {
                _variableSubstitution[expression.Input.VariableName] = _variableSubstitution[_projectVarName.Peek()];
            }
            if (_variableSubstitution.ContainsKey(_filterVarName.Peek()))
            {
                _variableSubstitution[expression.Input.VariableName] = _variableSubstitution[_filterVarName.Peek()];
            }
            PopProjectVar();
            PopFilterVar();
            //LeaveVariableScope();
            //_variableSubstitution[_projectVarName.Peek()] = expression.Input.VariableName;
            //return new FromExpression(projectExpression, expression.Input.VariableName);
            return projectExpression;
        }

        public override VisitedExpression Visit(DbRefKeyExpression expression)
        {
            throw new NotImplementedException();
        }

        public override VisitedExpression Visit(DbEntityRefExpression expression)
        {
            throw new NotImplementedException();
        }

        public override VisitedExpression Visit(DbFunctionExpression expression)
        {
            // a function call
            // may be built in, canonical, or user defined
            return VisitFunction(expression.Function, expression.Arguments, expression.ResultType);
        }

        public override VisitedExpression Visit(DbFilterExpression expression)
        {
            return VisitFilterExpression(expression, false);
        }

        private VisitedExpression VisitFilterExpression(DbFilterExpression expression, bool partOfJoin)
        {
            // complicated
            // similar logic used for other expressions (such as group by)
            // TODO: this is too simple.  Replace this
            // need to move the from keyword out so that it can be used in the project
            // when there is no where clause
            PushFilterVar(expression.Input.VariableName);
            InputExpression inputExpression;
            if (expression.Input.Expression is DbFilterExpression)
            {
                inputExpression = CheckedConvertFrom(VisitFilterExpression((DbFilterExpression)expression.Input.Expression, partOfJoin), expression.Input.VariableName);
            }
            else
            {
                inputExpression = CheckedConvertFrom(expression.Input.Expression.Accept(this), expression.Input.VariableName);
            }
            if (!(inputExpression is JoinExpression))
            {
                //from = new FromExpression(inputExpression, expression.Input.VariableName);
                FromExpression from = (FromExpression)inputExpression;
                if (from.Where != null)
                {
                    _variableSubstitution[expression.Input.VariableName] = from.Name;
                    from.Where.And(expression.Predicate.Accept(this));
                }
                else
                {
                    _variableSubstitution[_projectVarName.Peek()] = expression.Input.VariableName;
                    if (_variableSubstitution.ContainsKey(_filterVarName.Peek()))
                        _variableSubstitution[_filterVarName.Peek()] = expression.Input.VariableName;
                    from.Where = new WhereExpression(expression.Predicate.Accept(this));
                }
            }
            else
            {
                JoinExpression join = (JoinExpression)inputExpression;

                // optimized query generation for inner joins
                // just make filter part of join condition to avoid building extra
                // nested queries
                // 
                if (partOfJoin && join.JoinType == DbExpressionKind.InnerJoin)
                {
                    System.Diagnostics.Debug.Assert(join.Condition != null);
                    join.Condition = new BooleanExpression("AND", join.Condition, expression.Predicate.Accept(this));
                }
                else
                {
                    VisitedExpression predicate = expression.Predicate.Accept(this);
                    if (join.Where != null)
                        join.Where.And(predicate);
                    else
                        join.Where = new WhereExpression(predicate);
                    if (partOfJoin)
                    {
                        // get the working projection
                        // will use columns from this projection to move
                        // them into a new inner projection (existing ones will be replaced
                        var previousProjection = _projectExpressions.Peek();
                        var projection = new ProjectionExpression();
                        // get the columns to move from previous working projection
                        // to the new projection being built from the join          // call ToArray to avoid problems with changing the list later
                        var movedColumns = GetColumnsForJoin(join, previousProjection).ToArray();
                        // pair up moved column with it's replacement
                        var replacementColumns = movedColumns
                            .Select(c => new { Existing = c, Replacement = GetReplacementColumn(join, c) });
                        // replace moved columns in the previous working projection
                        foreach (var entry in replacementColumns)
                        {
                            previousProjection.ReplaceColumn(entry.Existing, entry.Replacement);
                            projection.AppendColumn(entry.Existing);
                        }
                        // the moved columns need to have their qualification updated since
                        // they moved in a level.
                        AdjustPropertyAccess(movedColumns, _projectVarName.Peek());
                        // for a short duration, now have a new current projection
                        _projectExpressions.Push(projection);
                        projection.From = join;
                        // since this is wrapping a join inside a projection, need to replace all variables
                        // that referenced the inner tables.
                        string searchVar = _projectVarName.Peek() + ".";
                        foreach (var key in _variableSubstitution.Keys.ToArray())
                        {
                            if (key.Contains(searchVar))
                                _variableSubstitution[key] = _projectVarName.Peek();
                        }
                        // can't return a projection from VisitFilterExpression.  Convert to from.
                        inputExpression = CheckedConvertFrom(projection, _projectVarName.Peek());
                    }
                }
            }
            PopFilterVar();
            return inputExpression;
        }

        /// <summary>
        /// Given a join expression and a projection, fetch all columns in the projection
        /// that reference columns in the join.
        /// </summary>
        private IEnumerable<ColumnExpression> GetColumnsForJoin(JoinExpression join, ProjectionExpression projectionExpression)
        {
            List<string> fromNames = new List<string>();
            GetFromNames(join, fromNames);
            foreach (var column in projectionExpression.Columns.OfType<ColumnExpression>())
            {
                foreach (var prop in column.GetAccessedProperties())
                {
                    System.Text.StringBuilder propName = new System.Text.StringBuilder();
                    prop.WriteSql(propName);
                    string table = propName.ToString().Split('.')[0];
                    if (fromNames.Contains(table))
                    {
                        yield return column;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Given an InputExpression append all from names (including nested joins) to the list.
        /// </summary>
        private void GetFromNames(InputExpression input, List<string> fromNames)
        {
            if (input is FromExpression)
            {
                fromNames.Add(QuoteIdentifier(((FromExpression)input).Name));
            }
            else
            {
                var join = (JoinExpression)input;
                GetFromNames(join.Left, fromNames);
                GetFromNames(join.Right, fromNames);
            }
        }

        /// <summary>
        /// Get new ColumnExpression that will be used in projection that had it's existing columns moved.
        /// These should be simple references to the inner column
        /// </summary>
        private ColumnExpression GetReplacementColumn(JoinExpression join, ColumnExpression reassociatedColumn)
        {
            return new ColumnExpression(new LiteralExpression(
                QuoteIdentifier(_projectVarName.Peek()) + "." + QuoteIdentifier(reassociatedColumn.Name)),
                reassociatedColumn.Name);
        }

        /// <summary>
        /// Every property accessed in the list of columns must be adjusted for a new scope
        /// </summary>
        private void AdjustPropertyAccess(ColumnExpression[] movedColumns, string projectName)
        {
            foreach (var column in movedColumns)
            {
                foreach (var prop in column.GetAccessedProperties())
                {
                    prop.AdjustVariableAccess(projectName);
                }
            }
        }

        public override VisitedExpression Visit(DbExceptExpression expression)
        {
            // Except keyword
            return new CombinedProjectionExpression(expression.Left.Accept(this),
                "EXCEPT", expression.Right.Accept(this));
        }

        public override VisitedExpression Visit(DbElementExpression expression)
        {
            // a scalar expression (ie ExecuteScalar)
            // so it will likely be translated into a select
            //throw new NotImplementedException();
            LiteralExpression scalar = new LiteralExpression("(");
            scalar.Append(expression.Argument.Accept(this));
            scalar.Append(")");
            return scalar;
        }

        public override VisitedExpression Visit(DbDistinctExpression expression)
        {
            // the distinct clause for a select
            VisitedExpression distinctArg = expression.Argument.Accept(this);
            ProjectionExpression projection = distinctArg as ProjectionExpression;
            if (projection == null)
                throw new NotSupportedException();
            projection.Distinct = true;
            return new FromExpression(projection, _projectVarName.Peek());
        }

        public override VisitedExpression Visit(DbDerefExpression expression)
        {
            throw new NotImplementedException();
        }

        public override VisitedExpression Visit(DbCrossJoinExpression expression)
        {
            // join without ON
            return new JoinExpression(VisitJoinPart(expression.Inputs[0]),
                expression.ExpressionKind,
                VisitJoinPart(expression.Inputs[1]),
                null);
        }

        public override VisitedExpression Visit(DbConstantExpression expression)
        {
            // literals to be inserted into the sql
            // may require some formatting depending on the type
            //throw new NotImplementedException();
            // TODO: this is just for testing
            return new ConstantExpression(expression.Value, expression.ResultType);
        }

        public override VisitedExpression Visit(DbComparisonExpression expression)
        {
            DbExpressionKind comparisonOperator;
            switch (expression.ExpressionKind)
            {
                case DbExpressionKind.Equals:
                case DbExpressionKind.GreaterThan:
                case DbExpressionKind.GreaterThanOrEquals:
                case DbExpressionKind.LessThan:
                case DbExpressionKind.LessThanOrEquals:
                case DbExpressionKind.Like:
                case DbExpressionKind.NotEquals:
                    comparisonOperator = expression.ExpressionKind;
                    break;
                default:
                    throw new NotSupportedException();
            }
            return new NegatableBooleanExpression(comparisonOperator, expression.Left.Accept(this), expression.Right.Accept(this));
        }

        public override VisitedExpression Visit(DbCastExpression expression)
        {
            return new CastExpression(expression.Argument.Accept(this), GetDbType(expression.ResultType.EdmType));
        }

        protected string GetDbType(EdmType edmType)
        {
            PrimitiveType primitiveType = edmType as PrimitiveType;
            if (primitiveType == null)
                throw new NotSupportedException();
            switch (primitiveType.PrimitiveTypeKind)
            {
                case PrimitiveTypeKind.Boolean:
                    return "bool";
                case PrimitiveTypeKind.Int16:
                    return "int2";
                case PrimitiveTypeKind.Int32:
                    return "int4";
                case PrimitiveTypeKind.Int64:
                    return "int8";
                case PrimitiveTypeKind.String:
                    return "varchar";
                case PrimitiveTypeKind.Decimal:
                    return "numeric";
                case PrimitiveTypeKind.Single:
                    return "float4";
                case PrimitiveTypeKind.Double:
                    return "float8";
                case PrimitiveTypeKind.DateTime:
                    return "timestamp";
                case PrimitiveTypeKind.Binary:
                    return "bytea";
                case PrimitiveTypeKind.Guid:
                    return "uuid";
            }
            throw new NotSupportedException();
        }

        public override VisitedExpression Visit(DbCaseExpression expression)
        {
            LiteralExpression caseExpression = new LiteralExpression(" CASE ");
            for (int i = 0; i < expression.When.Count && i < expression.Then.Count; ++i)
            {
                caseExpression.Append(" WHEN (");
                caseExpression.Append(expression.When[i].Accept(this));
                caseExpression.Append(") THEN (");
                caseExpression.Append(expression.Then[i].Accept(this));
                caseExpression.Append(")");
            }
            if (expression.Else is DbNullExpression)
            {
                caseExpression.Append(" END ");
            }
            else
            {
                caseExpression.Append(" ELSE (");
                caseExpression.Append(expression.Else.Accept(this));
                caseExpression.Append(") END ");
            }
            return caseExpression;
        }

        public override VisitedExpression Visit(DbArithmeticExpression expression)
        {
            LiteralExpression arithmeticOperator;

            switch (expression.ExpressionKind)
            {
                case DbExpressionKind.Divide:
                    arithmeticOperator = new LiteralExpression("/");
                    break;
                case DbExpressionKind.Minus:
                    arithmeticOperator = new LiteralExpression("-");
                    break;
                case DbExpressionKind.Modulo:
                    arithmeticOperator = new LiteralExpression("%");
                    break;
                case DbExpressionKind.Multiply:
                    arithmeticOperator = new LiteralExpression("*");
                    break;
                case DbExpressionKind.Plus:
                    arithmeticOperator = new LiteralExpression("+");
                    break;
                case DbExpressionKind.UnaryMinus:
                    arithmeticOperator = new LiteralExpression("-");
                    break;
                default:
                    throw new NotSupportedException();
            }

            if (expression.ExpressionKind == DbExpressionKind.UnaryMinus)
            {
                System.Diagnostics.Debug.Assert(expression.Arguments.Count == 1);
                arithmeticOperator.Append("(");
                arithmeticOperator.Append(expression.Arguments[0].Accept(this));
                arithmeticOperator.Append(")");
                return arithmeticOperator;
            }
            else
            {
                LiteralExpression math = new LiteralExpression("");
                bool first = true;
                foreach (DbExpression arg in expression.Arguments)
                {
                    if (!first)
                        math.Append(arithmeticOperator);
                    math.Append("(");
                    math.Append(arg.Accept(this));
                    math.Append(")");
                    first = false;
                }
                return math;
            }
        }

        public override VisitedExpression Visit(DbApplyExpression expression)
        {
            // like a join, but used when one of the arguments is a function.
            // it lets you return the results of a function call given values from the
            // other table.
            // sql standard seems to be lateral join
            throw new NotImplementedException();
        }

        public override VisitedExpression Visit(DbAndExpression expression)
        {
            return new BooleanExpression("AND", expression.Left.Accept(this), expression.Right.Accept(this));
        }

        public override VisitedExpression Visit(DbExpression expression)
        {
            // only concrete types visited
            throw new NotSupportedException();
        }

        public abstract void BuildCommand(DbCommand command);

        internal static string QuoteIdentifier(string identifier)
        {
            return "\"" + identifier.Replace("\"", "\"\"") + "\"";
        }

        private VisitedExpression VisitFunction(DbFunctionAggregate functionAggregate)
        {
            if (functionAggregate.Function.NamespaceName == "Edm")
            {
                FunctionExpression aggregate;
                switch (functionAggregate.Function.Name)
                {
                    case "Avg":
                    case "Count":
                    case "Min":
                    case "Max":
                    case "StdDev":
                    case "Sum":
                        aggregate = new FunctionExpression(functionAggregate.Function.Name);
                        break;
                    case "BigCount":
                        aggregate = new FunctionExpression("count");
                        break;
                    default:
                        throw new NotSupportedException();
                }
                System.Diagnostics.Debug.Assert(functionAggregate.Arguments.Count == 1);
                VisitedExpression aggregateArg;
                if (functionAggregate.Distinct)
                {
                    aggregateArg = new LiteralExpression("DISTINCT ");
                    ((LiteralExpression)aggregateArg).Append(functionAggregate.Arguments[0].Accept(this));
                }
                else
                {
                    aggregateArg = functionAggregate.Arguments[0].Accept(this);
                }
                aggregate.AddArgument(aggregateArg);
                return new CastExpression(aggregate, GetDbType(functionAggregate.ResultType.EdmType));
            }
            throw new NotSupportedException();
        }

        private VisitedExpression VisitFunction(EdmFunction function, IList<DbExpression> args, TypeUsage resultType)
        {
            if (function.NamespaceName == "Edm")
            {
                VisitedExpression arg;
                switch (function.Name)
                {
                        // string functions
                    case "Left":
                        System.Diagnostics.Debug.Assert(args.Count == 2);
                        return Substring(args[0].Accept(this), new LiteralExpression(" 1 "), args[1].Accept(this));
                    case "Right":
                        System.Diagnostics.Debug.Assert(args.Count == 2);
                        {
                            var arg0 = args[0].Accept(this);
                            var arg1 = args[1].Accept(this);
                            var start = new FunctionExpression("char_length");
                            start.AddArgument(arg0);
                            // add one before subtracting count since strings are 1 based in postgresql
                            start.Append("+1-");
                            start.Append(arg1);
                            return Substring(arg0, start);
                        }
                    case "Substring":
                        System.Diagnostics.Debug.Assert(args.Count == 3);
                        return Substring(args[0].Accept(this), args[1].Accept(this), args[2].Accept(this));
                    case "Length":
                        FunctionExpression length = new FunctionExpression("char_length");
                        System.Diagnostics.Debug.Assert(args.Count == 1);
                        length.AddArgument(args[0].Accept(this));
                        return new CastExpression(length, GetDbType(resultType.EdmType));
                    case "Concat":
                        System.Diagnostics.Debug.Assert(args.Count == 2);
                        arg = args[0].Accept(this);
                        arg.Append(" || ");
                        arg.Append(args[1].Accept(this));
                        return arg;
                    case "IndexOf":
                        System.Diagnostics.Debug.Assert(args.Count == 2);
                        FunctionExpression indexOf = new FunctionExpression("position");
                        arg = args[0].Accept(this);
                        arg.Append(" in ");
                        arg.Append(args[1].Accept(this));
                        indexOf.AddArgument(arg);
                        return indexOf;
                    case "LTrim":
                        return StringModifier("ltrim", args);
                    case "RTrim":
                        return StringModifier("rtrim", args);
                    case "Trim":
                        return StringModifier("btrim", args);
                    case "ToUpper":
                        return StringModifier("upper", args);
                    case "ToLower":
                        return StringModifier("lower", args);
                    case "Replace":
                        FunctionExpression replace = new FunctionExpression("replace");
                        System.Diagnostics.Debug.Assert(args.Count == 3);
                        replace.AddArgument(args[0].Accept(this));
                        replace.AddArgument(args[1].Accept(this));
                        replace.AddArgument(args[2].Accept(this));
                        return replace;
                        // case "Reverse":

                        // date functions
                    case "Day":
                    case "Hour":
                    case "Minute":
                    case "Month":
                    case "Second":
                    case "Year":
                        return DatePart(function.Name, args);
                    case "Millisecond":
                        return DatePart("milliseconds", args);
                    case "GetTotalOffsetMinutes":
                        VisitedExpression timezone = DatePart("timezone", args);
                        timezone.Append("/60");
                        return timezone;
                    case "CurrentDateTime":
                        return new LiteralExpression("LOCALTIMESTAMP");
                    case "CurrentUtcDateTime":
                        LiteralExpression utcNow = new LiteralExpression("CURRENT_TIMESTAMP");
                        utcNow.Append(" AT TIME ZONE 'UTC'");
                        return utcNow;
                    case "CurrentDateTimeOffset":
                        // TODO: this doesn't work yet because the reader
                        // doesn't return DateTimeOffset.
                        return new LiteralExpression("CURRENT_TIMESTAMP");

                        // bitwise operators
                    case "BitwiseAnd":
                        return BitwiseOperator(args, " & ");
                    case "BitwiseOr":
                        return BitwiseOperator(args, " | ");
                    case "BitwiseXor":
                        return BitwiseOperator(args, " # ");
                    case "BitwiseNot":
                        System.Diagnostics.Debug.Assert(args.Count == 1);
                        LiteralExpression not = new LiteralExpression("~ ");
                        not.Append(args[0].Accept(this));
                        return not;

                        // math operators
                    case "Abs":
                    case "Ceiling":
                    case "Floor":
                    case "Round":
                        return UnaryMath(function.Name, args);

                    case "NewGuid":
                        return new FunctionExpression("uuid_generate_v4");

                    default:
                        throw new NotSupportedException("NotSupported " + function.Name);
                }
            }
            throw new NotSupportedException();
        }

        private VisitedExpression Substring(VisitedExpression source, VisitedExpression start, VisitedExpression count)
        {
            FunctionExpression substring = new FunctionExpression("substr");
            substring.AddArgument(source);
            substring.AddArgument(start);
            substring.AddArgument(count);
            return substring;
        }

        private VisitedExpression Substring(VisitedExpression source, VisitedExpression start)
        {
            FunctionExpression substring = new FunctionExpression("substr");
            substring.AddArgument(source);
            substring.AddArgument(start);
            return substring;
        }

        private VisitedExpression UnaryMath(string funcName, IList<DbExpression> args)
        {
            FunctionExpression mathFunction = new FunctionExpression(funcName);
            System.Diagnostics.Debug.Assert(args.Count == 1);
            mathFunction.AddArgument(args[0].Accept(this));
            return mathFunction;
        }

        private VisitedExpression StringModifier(string modifier, IList<DbExpression> args)
        {
            FunctionExpression modifierFunction = new FunctionExpression(modifier);
            System.Diagnostics.Debug.Assert(args.Count == 1);
            modifierFunction.AddArgument(args[0].Accept(this));
            return modifierFunction;
        }

        private VisitedExpression DatePart(string partName, IList<DbExpression> args)
        {

            FunctionExpression extract_date = new FunctionExpression("cast(extract");
            System.Diagnostics.Debug.Assert(args.Count == 1);
            VisitedExpression arg = new LiteralExpression(partName + " FROM ");
            arg.Append(args[0].Accept(this));
            extract_date.AddArgument(arg);
            // need to convert to Int32 to match cononical function
            extract_date.Append(" as int4)");
            return extract_date;
        }

        private VisitedExpression BitwiseOperator(IList<DbExpression> args, string oper)
        {
            System.Diagnostics.Debug.Assert(args.Count == 2);
            VisitedExpression arg = args[0].Accept(this);
            arg.Append(oper);
            arg.Append(args[1].Accept(this));
            return arg;
        }

        private Stack<Stack<string>> _filterToProject = new Stack<Stack<string>>();
        private void PushProjectVar(string projectVar)
        {
            _projectVarName.Push(projectVar);
            foreach (var stack in _filterToProject)
            {
                stack.Push(projectVar);
            }
        }

        private string PopProjectVar()
        {
            foreach (var stack in _filterToProject)
            {
                stack.Pop();
            }
            return _projectVarName.Pop();
        }

        private void PushFilterVar(string filterVar)
        {
            _filterVarName.Push(filterVar);
            var stack = new Stack<string>();
            stack.Push(filterVar);
            _filterToProject.Push(stack);
        }

        private string PopFilterVar()
        {
            _filterToProject.Pop();
            return _filterVarName.Pop();
        }

        private void SubstituteFilterNames(string joinPartVariableName, string variableName)
        {
            if (_filterVarName.Count != 0)
            {
                foreach (var stack in _filterToProject)
                {
                    string[] dottedNames = stack.ToArray();
                    // reverse because the stack has them last in first out
                    Array.Reverse(dottedNames);
                    SubstituteAllNames(dottedNames, joinPartVariableName, variableName);
                }
            }
        }


        //private Stack<Dictionary<string, string>> _varScopeStack = new Stack<Dictionary<string, string>>();
        //private Stack<Stack<string>> _projectScopeStack = new Stack<Stack<string>>();
        //private Stack<Stack<string>> _filterScopeStack = new Stack<Stack<string>>();

        //private void EnterNewVariableScope()
        //{
        //    _varScopeStack.Push(_variableSubstitution);
        //    _projectScopeStack.Push(_projectVarName);
        //    _filterScopeStack.Push(_filterVarName);
        //    _variableSubstitution = new Dictionary<string, string>();
        //    _projectVarName = new Stack<string>();
        //    _filterVarName = new Stack<string>();
        //}

        //private void LeaveVariableScope()
        //{
        //    _variableSubstitution = _varScopeStack.Pop();
        //    _projectVarName = _projectScopeStack.Pop();
        //    _filterVarName = _filterScopeStack.Pop();
        //}
    }
}
#endif