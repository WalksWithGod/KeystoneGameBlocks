using System;
using System.Collections.Generic;
using System.Data;

namespace KeyServerCommon
{
    public class TableDefinition
    {
        public string TableName;
        public string[] FieldNames;
        public DbType[] FieldTypes;
        public  List<object[]> FieldValues ;

        public int JoinSourceFieldIndex ; 
        public int JoinTargetFieldIndex;
        public bool OuterJoin  = true ; //  true = include records that have no matches in the joined table

    
        public void Redim (uint count) 
        {
            int i = (int)count;
            FieldNames = new string[i]; 
            FieldTypes = new DbType [i];
            FieldValues = new List<object[]>();        
        }

        public void AddValues(object[] values)
        {
            FieldValues.Add(values);
        }


        public int FieldsCount
        {
            get
            {
                if (FieldNames == null) return 0;
                return FieldNames.Length;
            }
        }
    }
}
