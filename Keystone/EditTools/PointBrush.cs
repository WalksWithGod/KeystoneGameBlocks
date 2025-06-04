using System;
using MTV3D65;


namespace Keystone.EditTools
{
    // a brush that can be variable radius, square or circular
    public class PointBrush : IDisposable
    {
        public enum BrushMode
        {
            Nothing,
            Increase,
            Decrease,
            Flatten,
            Manual,
            Select,
            Smooth,
            Paint
        }

        private TVMesh[] _meshPointer;
        private Enum_BrushSize _eBrushSize;
        private BrushShape _eBrushShape;
        private bool _nPointersLocked;
        private BrushMode _eBrushMode;

        // debateable whether i want to cache a lineList for our brush and render it using Keystone.DebugDraw.DrawLines()
        // our GroupSelectBrush definetly will need to use lines though

        // i think that on BeginApply() this will take over the mouse input
        // and depending on various events, will eventually release the mouse and call IOperationComplete ( IOpResult which is a referece to this probably?)
        // also before callingback, it will push the operation on the Undo stack 

        // but the reason i think we need to take control of the operation once it starts
        // is because of the way we need to handle undo.  We need to treat each operation
        // as an atomic operation even though it make take several frames.  But the "delta"
        // we end up with is always the initial result - final result

        // that said, lets say you're dragging something in the editor and you get to the edge of the screen
        // the mouse should be trapped and the screen should scroll.  Once you release and are done with the
        // operation, the mouse should be free again.

        public PointBrush(TVMesh bristle)
        {
            // setup the meshes that represent the individual bristles of the brush that
            // we use to do things like adjust alltitude of landscape vertices
            _meshPointer = new TVMesh[24];

            _meshPointer[0].CreateSphere(0.3F, 8, 8);
            // TODO: this should be passed in instanced with CreateMeshBuilder and the sphere (or anything else) set
            _meshPointer[0] = bristle;

            // note: SetLightingMode occurs after geometry is loaded because the lightingmode
            // is stored in .tvm, .tva files and so anything you set prior to loading, will be
            // replaced with the lightingmode stored in the file.
            _meshPointer[0].SetLightingMode(CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED);

            _meshPointer[0].Enable(false);
            //_meshPointer[0].SetMaterial(clsMaterials.GetMaterialFromName("brushmiddle"));
            _meshPointer[0].SetCullMode(MTV3D65.CONST_TV_CULLING.TV_BACK_CULL);
            _meshPointer[0].ComputeNormals();
            _meshPointer[0].ComputeBoundings();
            _meshPointer[0].ComputeOctree();
            _meshPointer[0].SetCollisionEnable(false);

            for (int i = 1; i < 24; i ++)
            {
                _meshPointer[i] = _meshPointer[0].Duplicate();
                // _meshPointer[i].SetMaterial(clsMaterials.GetMaterialFromName("brusharound"));
            }
        }

        private void Pointers_SetPosition(float X, float Z)
        {
            //TV_3DVECTOR[] vPointer = null;
            //float fDistance = 0;
            ////  Set distance between pointers
            //switch (_eBrushMode)
            //{
            //    case BrushMode.Paint:
            //        fDistance = 1;
            //        break;
            //    default:
            //        fDistance = 4;
            //        break;
            //}

            //vPointer[0].x = X;
            //vPointer[0].z = Z;


            ////  Set other Pointers - Small
            //if (((_eBrushSize == Enum_BrushSize.Small)
            //     || ((_eBrushSize == Enum_BrushSize.Medium)
            //         || (_eBrushSize == Enum_BrushSize.Large))))
            //{
            //    switch (_eBrushShape)
            //    {
            //        case BrushShape.Square:
            //            vPointer[1].x = (X + fDistance);
            //            vPointer[1].z = Z;
            //            vPointer[2].x = (X + fDistance);
            //            vPointer[2].z = (Z + fDistance);
            //            vPointer[3].x = X;
            //            vPointer[3].z = (Z + fDistance);
            //            break;
            //        case BrushShape.Circle:
            //            vPointer[1].x = (X - fDistance);
            //            vPointer[1].z = Z;
            //            vPointer[2].x = X;
            //            vPointer[2].z = (Z - fDistance);
            //            vPointer[3].x = (X + fDistance);
            //            vPointer[3].z = Z;
            //            vPointer[4].x = X;
            //            vPointer[4].z = (Z + fDistance);
            //            break;
            //        case BrushShape.LineNS:
            //            vPointer[1].x = (X + fDistance);
            //            vPointer[1].z = Z;
            //            break;
            //        case BrushShape.LineWE:
            //            vPointer[1].x = X;
            //            vPointer[1].z = (Z + fDistance);
            //            break;
            //    }
            //}
            ////  Set other Pointers - Medium
            //if (((_eBrushSize == Enum_BrushSize.Medium)
            //     || (_eBrushSize == Enum_BrushSize.Large)))
            //{
            //    switch (_eBrushShape)
            //    {
            //        case BrushShape.Square:
            //            vPointer[4].x = (X - fDistance);
            //            vPointer[4].z = (Z + fDistance);
            //            vPointer[5].x = (X - fDistance);
            //            vPointer[5].z = Z;
            //            vPointer[6].x = (X - fDistance);
            //            vPointer[6].z = (Z - fDistance);
            //            vPointer[7].x = X;
            //            vPointer[7].z = (Z - fDistance);
            //            vPointer[8].x = (X + fDistance);
            //            vPointer[8].z = (Z - fDistance);
            //            break;
            //        case BrushShape.Circle:
            //            vPointer[5].x = (X + fDistance);
            //            vPointer[5].z = (Z - fDistance);
            //            vPointer[6].x = (X + (2*fDistance));
            //            vPointer[6].z = Z;
            //            vPointer[7].x = (X + (2*fDistance));
            //            vPointer[7].z = (Z + fDistance);
            //            vPointer[8].x = (X + fDistance);
            //            vPointer[8].z = (Z + fDistance);
            //            vPointer[9].x = (X + fDistance);
            //            vPointer[9].z = (Z + (2*fDistance));
            //            vPointer[10].x = X;
            //            vPointer[10].z = (Z + (2*fDistance));
            //            vPointer[11].x = (X - fDistance);
            //            vPointer[11].z = (Z + fDistance);
            //            break;
            //        case BrushShape.LineNS:
            //            vPointer[2].x = (X - fDistance);
            //            vPointer[2].z = Z;
            //            break;
            //        case BrushShape.LineWE:
            //            vPointer[2].x = X;
            //            vPointer[2].z = (Z - fDistance);
            //            break;
            //    }
            //}
            ////  Set other Pointers - Large
            //if (_eBrushSize == Enum_BrushSize.Large)
            //{
            //    switch (_eBrushShape)
            //    {
            //        case BrushShape.Square:
            //            vPointer[9].x = (X + (2*fDistance));
            //            vPointer[9].z = (Z - fDistance);
            //            vPointer[10].x = (X + (2*fDistance));
            //            vPointer[10].z = Z;
            //            vPointer[11].x = (X + (2*fDistance));
            //            vPointer[11].z = (Z + fDistance);
            //            vPointer[12].x = (X + (2*fDistance));
            //            vPointer[12].z = (Z + (2*fDistance));
            //            vPointer[13].x = (X + fDistance);
            //            vPointer[13].z = (Z + (2*fDistance));
            //            vPointer[14].x = X;
            //            vPointer[14].z = (Z + (2*fDistance));
            //            vPointer[15].x = (X - fDistance);
            //            vPointer[15].z = (Z + (2*fDistance));
            //            vPointer[16].x = (X - (2*fDistance));
            //            vPointer[16].z = (Z + (2*fDistance));
            //            vPointer[17].x = (X - (2*fDistance));
            //            vPointer[17].z = (Z + fDistance);
            //            vPointer[18].x = (X - (2*fDistance));
            //            vPointer[18].z = Z;
            //            vPointer[19].x = (X - (2*fDistance));
            //            vPointer[19].z = (Z - fDistance);
            //            vPointer[20].x = (X - (2*fDistance));
            //            vPointer[20].z = (Z - (2*fDistance));
            //            vPointer[21].x = (X - fDistance);
            //            vPointer[21].z = (Z - (2*fDistance));
            //            vPointer[22].x = X;
            //            vPointer[22].z = (Z - (2*fDistance));
            //            vPointer[23].x = (X + fDistance);
            //            vPointer[23].z = (Z - (2*fDistance));
            //            vPointer[24].x = (X + (2*fDistance));
            //            vPointer[24].z = (Z - (2*fDistance));
            //            break;
            //        case BrushShape.Circle:
            //            vPointer[12].x = (X - fDistance);
            //            vPointer[12].z = (Z + (2*fDistance));
            //            vPointer[13].x = (X - (2*fDistance));
            //            vPointer[13].z = (Z + fDistance);
            //            vPointer[14].x = (X - (2*fDistance));
            //            vPointer[14].z = Z;
            //            vPointer[15].x = (X - (2*fDistance));
            //            vPointer[15].z = (Z - fDistance);
            //            vPointer[16].x = (X - fDistance);
            //            vPointer[16].z = (Z - fDistance);
            //            vPointer[17].x = (X - fDistance);
            //            vPointer[17].z = (Z - (2*fDistance));
            //            vPointer[18].x = X;
            //            vPointer[18].z = (Z - (2*fDistance));
            //            vPointer[19].x = (X + fDistance);
            //            vPointer[19].z = (Z - (2*fDistance));
            //            vPointer[20].x = (X + (2*fDistance));
            //            vPointer[20].z = (Z - fDistance);
            //            break;
            //        case BrushShape.LineNS:
            //            vPointer[3].x = (X + (2*fDistance));
            //            vPointer[3].z = Z;
            //            break;
            //        case BrushShape.LineWE:
            //            vPointer[3].x = X;
            //            vPointer[3].z = (Z + (2*fDistance));
            //            break;
            //    }
            //}
            ////  Place mesh
            //for (int i = 0; i <= 24; i++)
            //{
            //    // With...
            //    _meshPointer[i].SetPosition(vPointer[i].x, 0, vPointer[i].z);
            //}
        }

        public void Update()
        {
            float fX;
            float fZ;
            //  Reset Pointers
            Pointers_Reset();
            //  Check if we can move the pointers (manual alt. = no)
            if ((_nPointersLocked == false))
            {
                //  Get mouse collision 
                TVCollisionResult col = null;
                ////// col = clsScene.MousePick(2);
                //  ...with landscape
                if (col.IsCollision())
                {
                    //  Place depending the brush
                    switch (_eBrushMode)
                    {
                        case BrushMode.Paint:
                            fX = (int) (col.GetCollisionImpact().x + 0.5F);
                            fZ = (int) (col.GetCollisionImpact().z + 0.5F);
                            Pointers_SetPosition(fX, fZ);
                            break;
                        default:
                            fX = (int) ((col.GetCollisionImpact().x/4)*4);
                            fZ = (int) ((col.GetCollisionImpact().z/4)*4);
                            Pointers_SetPosition((int) fX, (int) fZ);
                            break;
                    }
                }
                else
                {
                    //  No collision
                    Pointers_DisableAll();
                }
            }
            //  Now theyre placed, just set the altitude
            Pointers_CheckAltitude();
        }

        public void Render()
        {
            for (int i = 0; i <= 24; i++)
            {
                _meshPointer[i].Render();
            }
        }


        private void Pointers_CheckAltitude()
        {
            MTV3D65.TV_COLLISIONRESULT res;
            MTV3D65.TV_3DVECTOR vStart;
            MTV3D65.TV_3DVECTOR vEnd;
            for (int i = 0; i <= 24; i++)
            {
                //  Update only enabled pointers
                if (_meshPointer[i].IsEnabled())
                {
                    vStart = _meshPointer[i].GetPosition();
                    vEnd = _meshPointer[i].GetPosition();
                    vStart.y = 1000;
                    vEnd.y = -1000;
                    ////  Check if collision with landscape
                    //res = Core.Core._Core.Scene.AdvancedCollision(vStart, vEnd, MTV3D65.CONST_TV_OBJECT_TYPE.TV_OBJECT_LANDSCAPE);
                    //if (res.bHasCollided) {
                    //    //  Collided, hten its over a landscape
                    //    _meshPointer[i].SetPosition(res.vCollisionImpact.x, res.vCollisionImpact.y, res.vCollisionImpact.z);
                    //}
                    //else {
                    //    //  No collision, Pointer is floating in the nothingness, dont render
                    //    _meshPointer[i].Enable(false);
                    //}
                }
            }
        }

        private void Pointers_DisableAll()
        {
            for (int i = 0; i <= 24; i++)
            {
                _meshPointer[i].Enable(false);
            }
        }

        private void Pointers_Reset()
        {
            //int i;
            ////  Disable all
            //for (i = 0; (i <= 24); i++)
            //{
            //    _meshPointer[i].Enable(false);
            //}
            ////  Enable used Pointers depending on the form
            //switch (_eBrushMode)
            //{
            //    case BrushMode.Select:
            //        break;
            //    case BrushMode.Nothing:
            //        break;
            //    case BrushMode.Paint:
            //        switch (_eBrushSize)
            //        {
            //            case Enum_BrushSize.Single:
            //                _meshPointer[0].Enable(true);
            //                break;
            //            case Enum_BrushSize.Small:
            //                switch (_eBrushShape)
            //                {
            //                    case BrushShape.Square:
            //                        for (i = 0; (i <= 3); i++)
            //                        {
            //                            _meshPointer[i].Enable(true);
            //                        }
            //                        break;
            //                    case BrushShape.Circle:
            //                        for (i = 0; (i <= 4); i++)
            //                        {
            //                            _meshPointer[i].Enable(true);
            //                        }
            //                        break;
            //                    case BrushShape.LineNS:
            //                        for (i = 0; (i <= 1); i++)
            //                        {
            //                            _meshPointer[i].Enable(true);
            //                        }
            //                        break;
            //                    case BrushShape.LineWE:
            //                        for (i = 0; (i <= 1); i++)
            //                        {
            //                            _meshPointer[i].Enable(true);
            //                        }
            //                        break;
            //                }
            //                break;
            //            case Enum_BrushSize.Medium:
            //                switch (_eBrushShape)
            //                {
            //                    case BrushShape.Square:
            //                        for (i = 0; (i <= 8); i++)
            //                        {
            //                            _meshPointer[i].Enable(true);
            //                        }
            //                        break;
            //                    case BrushShape.Circle:
            //                        for (i = 0; (i <= 11); i++)
            //                        {
            //                            _meshPointer[i].Enable(true);
            //                        }
            //                        break;
            //                    case BrushShape.LineNS:
            //                        for (i = 0; (i <= 2); i++)
            //                        {
            //                            _meshPointer[i].Enable(true);
            //                        }
            //                        break;
            //                    case BrushShape.LineWE:
            //                        for (i = 0; (i <= 2); i++)
            //                        {
            //                            _meshPointer[i].Enable(true);
            //                        }
            //                        break;
            //                }
            //                break;
            //            case Enum_BrushSize.Large:
            //                switch (_eBrushShape)
            //                {
            //                    case BrushShape.Square:
            //                        for (i = 0; (i <= 15); i++)
            //                        {
            //                            _meshPointer[i].Enable(true);
            //                        }
            //                        break;
            //                    case BrushShape.Circle:
            //                        for (i = 0; (i <= 20); i++)
            //                        {
            //                            _meshPointer[i].Enable(true);
            //                        }
            //                        break;
            //                    case BrushShape.LineNS:
            //                        for (i = 0; (i <= 3); i++)
            //                        {
            //                            _meshPointer[i].Enable(true);
            //                        }
            //                        break;
            //                    case BrushShape.LineWE:
            //                        for (i = 0; (i <= 3); i++)
            //                        {
            //                            _meshPointer[i].Enable(true);
            //                        }
            //                        break;
            //                }
            //                break;
            //        }
            //        break;
            //}
        }

        #region IDisposable Members

        private bool _disposed = false;

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                for (int i = 0; i < 24; i++)
                {
                    _meshPointer[i] = null;
                }
            }
        }

        #endregion
    }
}