using System;
using System.Collections.Generic;
using System.Diagnostics;
using Keystone.Cameras;
using Keystone.Collision;
using Keystone.Culling;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.Lights;
using Keystone.Octree;
using Keystone.Portals;
using Keystone.Quadtree;
using Keystone.Types;
using MTV3D65;
using Sector = Keystone.Quadtree.Sector;
using Keystone.Extensions;
using Keystone.QuadTree;

namespace Keystone.Traversers
{
    internal struct SpatialQueryParameters 
    {
    }

    internal struct SpatialQueryResults
    {
        // NOTE: queries can be done against the QueryResult to find
        // closest result entry for example, or farthest, whatever
        Entities.Entity[] Results;

        internal void Add(Entity entity)
        {
            Results = Results.ArrayAppend(entity);
        }

    }

    /// <summary>
    /// Provides Spatial Search of Entities within the scene.
    /// </summary>
    public class EntityQuery : ITraverser
    {

        private SceneNode _startNode;

        private SpatialQueryParameters _parameters;
        private SpatialQueryResults mResult;
        private Stack<IntersectResult> mIntersectResultStack;

        private Entities.Entity mTarget;

        private BoundingSphere _targetSpaceSphere;
        private BoundingSphere _regionSpaceSphere;
        private Vector3d _targetSpaceOffset;

        public EntityQuery()
        {
            mIntersectResultStack = new Stack<IntersectResult>();
        }

        
        internal SpatialQueryResults Query(Entities.Entity target, RegionNode startNode, BoundingSphere targetSpaceSphere, SpatialQueryParameters parameters)
        {
            if (target == null) throw new ArgumentNullException();

            _parameters = parameters;
            mTarget = target;
            _startNode = startNode;

            mResult = new SpatialQueryResults();
            if (startNode == null) return mResult;

            // init the target space sphere in target space.  Eventually we'll compute a
            // a matrix for the target node and entities and then use that
            // to convert the sphere into the model space of each model we test against
            _targetSpaceSphere = targetSpaceSphere;
            _targetSpaceOffset = mTarget.SceneNode.Position;

            // TODO: rather than null, pass the parameters here and dont rely on module level parameters
            // at all.
            _startNode.Traverse(this, null);

            return mResult;
        }

        public void Clear()
        {
            mIntersectResultStack.Clear();
        }

        private void PushVisibilityTestResult(BoundingSphere sphere)
        {
            // if the parent node's bounding sphere is fully inside the target sphere, 
            // then so is the child sphere.  
            // Push the same result and skip the intersection test
            if (mIntersectResultStack.Count > 0 && mIntersectResultStack.Peek() == IntersectResult.INSIDE)
            {
                mIntersectResultStack.Push(IntersectResult.INSIDE);
            }
            else
            {
                // if the parent is INTERSECT then, run a visible test on this child's bounding sphere
                // and push the result
                // NOTE: we dont need to first check for an OUTSIDE result because
                // the callers to this function do that on the parent.  We may decide to 
                // modify this behavior in the future such that its not posssible to forget to do that
                // in future Apply() overload.
                IntersectResult result = _targetSpaceSphere.Intersects(sphere);
                
                mIntersectResultStack.Push(result);
            }
        }

        #region ITraverser Members
        public object Apply(SceneNode node, object data)
        {
            throw new Exception(
                "The method or operation is not valid.  Missing Apply() overload or Traverse() override in derived node type.");
        }

        public object Apply(RegionNode regionNode, object data)
        {
            if (mTarget.Region == null) return null;
            if (regionNode.Region.Enable == false) return null;

            Matrix toRegionSpace;

            // if this scene node does not reference the current camera's Region then we must transform 
            // the ray to the destination region's coordinate system
            if (regionNode.Region != mTarget.Region && (regionNode.Region is Root) == false)
            {

                // compute a new sphere that is in the coordinate system of the destination region
                Matrix root2dest = regionNode.Region.GlobalMatrix;
                Matrix source2root = Matrix.Inverse(mTarget.Region.GlobalMatrix);
                Matrix source2dest = root2dest * source2root;

                toRegionSpace = Matrix.CreateTranslation(_targetSpaceOffset) * Matrix.Inverse(source2dest);
            }
            else
            {
                toRegionSpace = Matrix.Identity();
            }

            _regionSpaceSphere = _targetSpaceSphere.Transform(toRegionSpace);

            PushVisibilityTestResult(regionNode.BoundingSphere);
            if (mIntersectResultStack.Peek() != IntersectResult.OUTSIDE)
            {
                //if (regionNode.Region is CelledRegion)
                //    // TODO: can't we take advantage of the Volume's within CelledRegion
                //    // to faster cull rather than go through EVERY entity within?!


                // note: for CelledRegion we also continue to check sub-children
                if (regionNode.Children != null)
                    foreach (SceneNode child in regionNode.Children)
                        child.Traverse(this, data);
            }
            mIntersectResultStack.Pop();
            return null;
        }

        public object Apply(Interior interior, object data)
        {
            throw new NotImplementedException();
        }
        public object Apply(TileMap.Structure structure, object data)
        {
            throw new NotImplementedException();
        }
                
        public object Apply(OctreeOctant octant, object data)
        {
            throw new NotImplementedException();
        }

        public object Apply(QuadtreeQuadrant quadrant, object data)
        {
            throw new NotImplementedException();
        }

        public object Apply(GUINode node, object data)
        {
            return null;
        }

        public object Apply(EntityNode node, object data)
        {
            if (node.Entity == null || node.Entity.Enable == false) return null;
            if (node.Entity is Viewpoint) return null;


            PushVisibilityTestResult(node.BoundingSphere);
            // This is a region space test against the entity's EntityNode sphere
            // which is always in region space
            if (mIntersectResultStack.Pop() != IntersectResult.OUTSIDE)
            {

                mResult.Add(node.Entity);

                // hierarchical child entity traversal.  We can however take advantage
                // of parent intersect results if the parent was fully in or outside of the 
                // regionSpaceSphere
                if (node.Children != null)
                    foreach (SceneNode child in node.Children)
                        child.Traverse(this, data);
            }

            mIntersectResultStack.Pop();
            return null;
        }

        public object Apply(CelledRegionNode node, object data)
        {
            //throw new NotImplementedException();
            return Apply((RegionNode)node, data); // for now just using RegionNode is ok
        }

        public object Apply(CellSceneNode node, object data)
        {
            return Apply((EntityNode)node, data);
        }

        public object Apply(PortalNode node, object data)
        {
            if (node.Entity.Enable && ((Portal)node.Entity).Destination != null)
            {
                // TODO: is pushvisibilityTestResult not useable when traversing a portal?
                // if the portal is fully contained in the target sphere, does that mean
                // so are things on the other side?  absolutely NOT!  I think logically we would
                // be doing a fresh sub-query 
                PushVisibilityTestResult(node.BoundingSphere);

                throw new NotImplementedException();
                //node.Entity.Traverse(this);

                mIntersectResultStack.Pop();
            }
            return null;
        }

        public object Apply(Node node, object data)
        {
            throw new Exception(
                "The method or operation is not valid.  Missing Apply() overload or Traverse() override in derived node type.");
        }

        public object Apply(Portals.Region sector, object data)
        {
            throw new NotImplementedException();
            //if ((sector.BoundingBox != null) && (ModelSpaceRayBBTest(_regionSpaceSphere, sector.BoundingBox)))
            //{
            //    // TODO: why arent we itterating through portals?
            //    // actually i dont think so.  We have to itterate through sectors in list form 

            //    if (sector.Children != null)
            //        foreach (Node child in sector.Children)
            //            child.Traverse(this);
            //}
        }

        public object Apply(Portal p, object data)
        {
            if ((p.Destination != null) && (p.Destination.SceneNode != null))
                return p.Destination.SceneNode.Traverse(this, data);

            return null;
        }

        public object Apply(Controls.Control2D control, object data)
        {
            throw new NotImplementedException();
        }

        public object Apply(DefaultEntity entity, object state)
        {
            throw new NotImplementedException();
        }

        public object Apply(ModeledEntity entity, object data)
        {
            throw new NotImplementedException();
        }

        public object Apply(Terrain terrain, object data)
        {
            throw new NotImplementedException();
            //if (terrain.Enable && terrain.Pickable && terrain.TVResourceIsLoaded)
            //{
            //    DoEntityCollisionTest(_regionSpaceSphere, terrain);
            //}
        }


        public object Apply(ModelLODSwitch lod, object data)
        {
            throw new NotImplementedException();
        }

        public object Apply(Geometry element, object data)
        {
            throw new NotImplementedException();
        }

        public object Apply(Light light, object data)
        {
            throw new NotImplementedException();
            //DoEntityCollisionTest(_regionSpaceSphere, light);
        }


        public object Apply(Appearance.Appearance app, object data)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}