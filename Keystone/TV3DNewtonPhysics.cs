using System;
using System.Collections.Generic;
using System.Text;

namespace Keystone
{
    // from baffler on irc aug22,2008
//private void CreateMissile(float PosX, float PosY, float PosZ, TV_3DVECTOR dir)
//        {
//            _missl = _scene.CreateMeshBuilder("a_flak");
//            //_flak.CreateCone(2, 2, 15, true);
//            _missl.CreateSphere(1);
//            _missl.SetLightingMode(CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED);
//            _missl.SetScale(1f, 1f, 1f);
//            _missl.SetPosition(PosX, PosY, PosZ);
//            int mat = _materials.CreateMaterial("MeshMat");
//            _materials.SetAmbient(mat, 1, 1, 1, 1.0f);
//            _materials.SetDiffuse(mat, 0.5f, 0.5f, 0.5f, 1);
//            _materials.SetEmissive(mat, 0, 0, 0, 0);
//            _materials.SetOpacity(mat, 1.0f);
//            _missl.SetMaterial(mat);
//            _missl.SetShadowCast(true, true);
//            _misslBody = _physics.CreateBody(60f);
//            _physics.SetBodyCollidable(_misslBody, true);
//            _physics.AddMesh(_misslBody, _missl, CONST_TV_PHYSICSBODY_BOUNDING.TV_BODY_SPHERE, true);
//            _physics.SetBodyCollidable(_misslBody, false);
//            _physics.AddImpulse(_misslBody, dir, true);

//            //these sets up the collision materials and event for the missle and the land
//            //so we can check if the missile hits the land
//            _physics.SetBodyMaterialGroup(_misslBody, _misslMatGroup);
//            _physics.SetMaterialInteractionCollision(_misslMatGroup, _landMatGroup, true);
//            _physics.SetMaterialInteractionEvents(_misslMatGroup, _landMatGroup, true, false, false);
//        }
//// when you setup the land you need to do this
//            _landBody = _physics.CreateStaticTerrainBody(_land);
//            _landMatGroup = _physics.CreateMaterialGroup("Land_Material1");
//            _physics.SetBodyMaterialGroup(_landBody, _landMatGroup);


//// call this each frame
//        private void CheckCollisions()
//        {
//            int NumEvents = _physics.PollEvents();

//            TV_EVENT_PHYSICSCOLLISION eColl;
//            for (int i = 0; i < NumEvents; i++) {
//                eColl = _physics.GetEventCollisionDesc(i);
//                // this checks if the missle hits the ground, if so it will destroy
//                if ((eColl.iMaterial1 == _misslMatGroup) && (eColl.iMaterial2 == _landMatGroup)) _physics.DestroyBody(eColl.iBody1);
//                // almost the same check but it checks if the ground was hit by the missile (i think, maybe you only need to check once :P)
//                if ((eColl.iMaterial2 == _misslMatGroup) && (eColl.iMaterial1 == _landMatGroup)) _physics.DestroyBody(eColl.iBody2);

////rest of this stuff just checks other collisions for stuff i store in lists
//                if (eColl.iMaterial1 == tankMaterialGroup)
//                {
//                    if (eColl.iMaterial2 == cb_MatGroup)
//                    {
//                        cb_List.ForEach(delegate(uCargoBox _cb) 
//                        {
//                            if (_cb.cb_PhysicBody == eColl.iBody2) {
//                                if (_carrying == "") {
//                                    InsertMessage("Picking up Cargo!");
//                                    _carrying = _cb.cb_Contains;
//                                    cb_List.Remove(_cb);
//                                    _physics.DestroyBody(eColl.iBody2);
//                                }
//                            }
//                        });
//                    }
//                }
//                else if (eColl.iMaterial2 == tankMaterialGroup)
//                {
//                    if (eColl.iMaterial1 == cb_MatGroup)
//                    {
//                        cb_List.ForEach(delegate(uCargoBox _cb)
//                        {
//                            if (_cb.cb_PhysicBody == eColl.iBody1)
//                            {
//                                if (_carrying == "")
//                                {
//                                    InsertMessage("Picking up Cargo!");
//                                    _carrying = _cb.cb_Contains;
//                                    cb_List.Remove(_cb);
//                                    _physics.DestroyBody(eColl.iBody1);
//                                }
//                            }
//                        });
//                    }
//                }
//            }
//        }
}