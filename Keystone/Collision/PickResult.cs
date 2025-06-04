using System;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.Types;
using KeyCommon.Traversal;
using MTV3D65;

namespace Keystone.Collision
{
            
    public class PickResults : PickResultsBase
    {
    	public Cameras.RenderingContext Context;
    	public System.Drawing.Point vpRelativeMousePos;
        public Entity Entity { get; private set; }
        public Model Model;
        public Geometry Geometry;
        public int GroupIndex;   // GroupIndex field will also hold MinimeshElement Index
        private static PickResults mEmpty = new PickResults();

        public PickResults() : base ()
        {
        	FaceID = -1;
        	EdgeID = -1;
        	VertexID = -1;
        	GroupIndex = -1; // GroupIndex field will also hold MinimeshElement Index
        	TileVertexIndex = -1;
        	TileLocation.X = -1;
        	TileLocation.Y = -1; // FloorLevel (not array index)
        	TileLocation.Z = -1;
            CellLocation.X = -1;
            CellLocation.Y = -1;
            CellLocation.Z = -1;
        }

        public static PickResults Empty()
        {
            return mEmpty;
        }

        public void SetEntity(string id, string typename)
        {
            EntityID = id;
            EntityTypeName = typename;
        }

        public void SetEntity (Entity entity)
        {
            Entity = entity;

            if (entity is Keystone.Simulation.IEntitySystem)
                EntityID = entity.ReferencedEntityID;
            else if (entity == null)
                EntityID = null;
            else
                EntityID = entity.ID;

            if (entity == null)
                EntityTypeName = null;
            else
                EntityTypeName = entity.TypeName ;
        }
        
        public static CONST_TV_TESTTYPE ToTVTestType(PickAccuracy accuracy)
        {
            switch (accuracy)
            {
                case PickAccuracy.Face:
                    // July.8.2013 - DEFAULT testing seems to work properly whereas ACCURATETESTING seems to fail on BonedActors and
                    //               TVMeshes very often.  This is bizarre.  I wonder if I'm suppose to be
                    //               logical or'ing the test types together?
                    //               Or perhpas ACCURATETESTING requires going through scene.Collide as opposed to actor/mesh.Collide
                    return CONST_TV_TESTTYPE.TV_TESTTYPE_DEFAULT;  //CONST_TV_TESTTYPE.TV_TESTTYPE_ACCURATETESTING;
                case PickAccuracy.BoundingBox:
                    return CONST_TV_TESTTYPE.TV_TESTTYPE_BOUNDINGBOX;
                case PickAccuracy.BoundingSphere:
                    return CONST_TV_TESTTYPE.TV_TESTTYPE_BOUNDINGSPHERE;
                case PickAccuracy.HitBoxes:
                    return CONST_TV_TESTTYPE.TV_TESTTYPE_HITBOXES;
                default:
                    return CONST_TV_TESTTYPE.TV_TESTTYPE_DEFAULT;

            }
        }

        // TODO: i need items here for NearestEdge, NearestVertex,Face, etc from EditableMesh
        // 

        //public PickResults(Entity entity, TV_COLLISIONRESULT result)
        //{
        //    Entity = entity;
        //    Geometry = null;
        //    if (entity is ModeledEntity)
        //        if (((ModeledEntity)entity).Geometry != null)
        //            Geometry = ((ModeledEntity) entity).Geometry;

        //    EdgeOrigin.x = 0;
        //    EdgeOrigin.y = 0;
        //    EdgeOrigin.z = 0;
        //    EdgeDest.x = 0;
        //    EdgeDest.y = 0;
        //    EdgeDest.z = 0;
        //    FacePoints = new Vector3d[0];
        //    FacePointIDs = new uint[0];

        //    Result = result;
        //    isActor = result.eCollidedObjectType == CONST_TV_OBJECT_TYPE.TV_OBJECT_ACTOR;
        //    HasCollided = false;
        //    FaceID = -1;
        //    EdgeID = -1;
        //    VertexIndex = -1;

        //    VertexCoord.x = 0;
        //    VertexCoord.y = 0;
        //    VertexCoord.z = 0;
        //    ImpactPoint.x = 0;
        //    ImpactPoint.y = 0;
        //    ImpactPoint.z = 0;
        //    ImpactNormal.x = 0;
        //    ImpactNormal.y = 0;
        //    ImpactNormal.z = 0;
        //    this.Matrix = Matrix.Identity();
        //    CollidedObjectType = 0;

        //    // debug properties
        //    PickOrigin = new Vector3d();
        //    PickEnd = new Vector3d();
        //}

        // TODO: this should maybe more data
        // PickingResults  that contains different data depending on the pick test
        // {
        //      Nodes[] nodes;
        //      Triangle tri;
        //      GroupIDs[] ids;
        //      Bones[] bone; 

    }
}