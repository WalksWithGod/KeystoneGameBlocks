//using System;
//using Keystone.Commands;
//using Keystone.EditDataStructures;
//using Keystone.EditTools;
//using Keystone.Entities;
//using Keystone.Types;

//namespace Keystone.EditOperations
//{
//    public class MoveOp : SychronousCommand
//    {
//        private PrimitiveInfo[] _primitives;
//        private EditableMesh _mesh;
//        private Vector3d _start, _destination;
//        private Entity _entity;
//        private bool _isEditableMesh;

//        public MoveOp( EditableMesh mesh, PrimitiveInfo[] primitives)
//        {
//            if (mesh == null) throw new ArgumentNullException();
//            if (primitives == null || primitives.Length < 1) throw new ArgumentNullException();
//            _primitives = primitives;
//            _mesh = mesh;
//            _isEditableMesh = true;
//        }

//        public MoveOp (Entity entity, Vector3d start, Vector3d destination)
//        {
//            _isEditableMesh = false;
//            _entity = entity;
//            _start = start;
//            _destination = destination;
//        }

//        #region ICommand Members
//        public override object Execute()
//        {
//            if (_isEditableMesh)
//            {
//                for (int i = 0; i < _primitives.Length; i++)
//                {
//                    if (_primitives[i].Type == PrimitiveType.Vertex)
//                        _mesh.MoveVertex(_primitives[i].ID, _primitives[i].Position[0]);
//                    else if (_primitives[i].Type == PrimitiveType.Edge)
//                    {
//                        _mesh.MoveEdge(_primitives[i].ID, _primitives[i].Position);
//                    }
//                    else if (_primitives[i].Type == PrimitiveType.Face)
//                        _mesh.MoveFace(_primitives[i].ID, _primitives[i].Position);
//                    else
//                        throw new Exception("Unsupported primitive type.");
//                }
//            }
//            else
//            {
//                _entity.Translation = _destination;
//            }
//            // TODO:  should a seperate Entity save directly to the library be done?  not even sure what i mean by "seperate"  or "directly to the library"  do i mean not just the xmldb but the archive for an editablemesh?
//            _state = CommandState.ExecuteCompleted;
//            return null;
//        }

//        // use inverse translation to move back
//        public override void UnExecute()
//        {
//            if (_state == CommandState.ExecuteCompleted )
//            {
//                if (_isEditableMesh)
//                {
//                    for (int i = 0; i < _primitives.Length; i++)
//                    {
//                        if (_primitives[i].Type == PrimitiveType.Vertex)
//                            _mesh.MoveVertex(_primitives[i].ID, _primitives[i].OriginalPosition[0]);
//                        else if (_primitives[i].Type == PrimitiveType.Edge)
//                        {
//                            _mesh.MoveEdge(_primitives[i].ID, _primitives[i].OriginalPosition);
//                        }
//                        else if (_primitives[i].Type == PrimitiveType.Face)
//                            _mesh.MoveFace(_primitives[i].ID, _primitives[i].OriginalPosition);
//                        else
//                            throw new Exception("Unsupported primitive type.");
//                    }
//                }
//                else
//                {
//                    _entity.Translation = _start;
//                }
//                _state = CommandState.ExecuteCompleted;
//            }
//            // else can't unexecute something that isn't completed.
//            // if there was an error in execute, we could either unexecute whatever parts we know didnt fail
//            // or perhaps that should be done automatically on failure in execute since try / catch can be set up to handle this immediately
//            // or in Execute we can add state variables to track exactly what has completed and what not and so we know what to undo
//        }

//        #endregion
//    }
//}
