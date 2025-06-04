using System;
using System.Collections.Generic;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.Lights;
using Keystone.Octree;
using Keystone.Portals;
using Keystone.Quadtree;
using Keystone.Traversers;
using Keystone.Types;
using MTV3D65;
using Sector = Keystone.Quadtree.Sector;
using Keystone.QuadTree;

namespace Keystone.Culling
{
    // http://www.truevision3d.com/forums/tv3d_sdk_65/prerelease_completion_progress_post-t5988.0.html;msg103388#msg103388
    //
    public class OcclusionCuller : ITraverser

    {
        private OcclusionFrustum _frustum;
        private List<int> _visibleList = new List<int>();
        private List<Occluder> _activeOccluders = new List<Occluder>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frustum"></param>
        /// <param name="positionNode">The group node that currently contains the camera.  In this way occlusion can be done 
        /// starting with those occluders that are closest to the camera and then reverse traversing...
        /// up until we get a certain "cutt off" distance away and where occluders screen space starts gettign too small
        /// to be worth running occlusion against </param>
        public OcclusionCuller(OcclusionFrustum frustum, OctreeOctant positionNode)
        {
            // for now f all this tree traversal.  we're just going to use the below Culler method on a list of mesh indices. 
            _frustum = frustum;


            // if shadowmapping is to be used, we should move this culler out
            // to the program's level so that it can be used with shadowmapping
            // Then, when occluding testing, we test the shadow's volume and the caster
            // and determine if an item is to be added to the casters, shadows or both lists.
        }

        public void Clear()
        {
            _visibleList.Clear();
            _activeOccluders.Clear();
        }

        public OcclusionFrustum Frustum
        {
            set { _frustum = value; }
        }

        public int[] Cull(Geometry[] meshlist)
        {
            _visibleList.Clear();
            if (meshlist != null)
            {
                for (int i = 0; i < meshlist.Length; i++)
                {
                    // compared to the bottom overloaded Culler() this implementation uses cached bounding volumes
                    // and does not need to call tvMesh.GetBoundingBox every itteration 
                    bool result = _frustum.IsVisible(meshlist[i]);

                    if (result)
                    {
                        // these meshes are still visible (ie. not occluded) so we add them to the updated list
                        _visibleList.Add(meshlist[i].TVIndex);
                    }
                }
                return _visibleList.ToArray();
            }
            return null;
        }

        public int[] Cull(int[] meshlist)
        {
            _visibleList.Clear();
            if (meshlist != null)
            {
                for (int i = 0; i < meshlist.Length; i++)
                {
                    TV_3DVECTOR min, max;
                    min = new TV_3DVECTOR(0, 0, 0);
                    max = new TV_3DVECTOR(0, 0, 0);

                    TVMesh m;
                    m = CoreClient._CoreClient.Globals.GetMeshFromID(meshlist[i]);
                    m.GetBoundingBox(ref min, ref max, false);
                    BoundingBox box = new BoundingBox(min.x, min.y,min.z, max.x, max.y, max.z);

                    bool result = _frustum.IsVisible(box);

                    if (result)
                    {
                        // these meshes are still visible (ie. not occluded) so we add them to the updated list
                        _visibleList.Add(meshlist[i]);
                    }
                }
                return _visibleList.ToArray();
            }
            return null;
        }

        #region ITraverser Members

        public object Apply(Node o, object data)
        {
            throw new NotImplementedException();
        }

        public object Apply(SceneNode node, object data)
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

        public object Apply(RegionNode node, object data)
        {
            throw new NotImplementedException();
        }

        public object Apply(Interior interior, object data)
        {
            throw new NotImplementedException();
        }

        public object Apply(TileMap.Structure structure, object data)
        {
            throw new NotImplementedException();
        }
                
        public object Apply(CelledRegionNode node, object data)
        {
            throw new NotImplementedException();
        }
        public object Apply(CellSceneNode node, object state)
        {
            throw new NotImplementedException();
        }
        public object Apply(PortalNode node, object data)
        {
            throw new NotImplementedException();
        }

        public object Apply(GUINode node, object data)
        {
            return null;
        }

        public object Apply(EntityNode node, object data)
        {
            throw new NotImplementedException();
        }

        public object Apply(Portals.Region region, object data)
        {
            throw new NotImplementedException();
        }

        //public object Apply(Interior interior)
        //{
        //    throw new NotImplementedException();
        //}

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

        public object Apply(Portal p, object data)
        {
            throw new NotImplementedException();
        }


        public object Apply(Quadrant o, object data)
        {
            throw new NotImplementedException();
        }

        public object Apply(Sector o, object data)
        {
            throw new NotImplementedException();
        }

        public object Apply(QTreeNode o, object data)
        {
            throw new NotImplementedException();
        }

        public object Apply(Branch o, object data)
        {
            throw new NotImplementedException();
        }

        public object Apply(Leaf o, object data)
        {
            throw new NotImplementedException();
        }

        public object Apply(Terrain terrain, object data)
        {
            throw new NotImplementedException();
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
        }

        public object Apply(Appearance.Appearance app, object data)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}