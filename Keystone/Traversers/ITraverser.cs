using Keystone.Elements;
using Keystone.Entities;
using Keystone.Lights;
using Keystone.Octree;
using Keystone.Portals;
using Keystone.Quadtree;
using Sector = Keystone.Quadtree.Sector;
using Keystone.QuadTree;

namespace Keystone.Traversers
{
    public interface ITraverser
    {
        object Apply(Node scene, object data);
        //object Apply(Quadrant o, object data);
        //object Apply(Sector o, object data); // quadtree Sector, not Portal Sector
        //object Apply(QTreeNode o, object data);
        //object Apply(Branch o, object data);
        //object Apply(Leaf o, object data);


        object Apply(SceneNode node, object data);
        object Apply(OctreeOctant octant, object data);
        object Apply(QuadtreeQuadrant quadrant, object data);
        object Apply(EntityNode node, object data);
        object Apply(GUINode node, object data);
        object Apply(RegionNode node, object data);
        object Apply(TileMap.Structure structure, object data);
        object Apply(Interior interior, object data);
        object Apply(CelledRegionNode node, object data);
        object Apply(CellSceneNode node, object state);
        object Apply(PortalNode node, object data);

        object Apply(Region region, object data);
        //object Apply(Interior interior, object data); // Container
        object Apply(Portal p, object data);
        object Apply(ModeledEntity entity, object data);
        object Apply(DefaultEntity entity, object data);
        object Apply(Controls.Control2D control, object data);
        //   object Apply(Player player, object data);

        object Apply(ModelLODSwitch lod, object data);
        object Apply(Geometry element, object data);

        object Apply(Light light, object data);
    }
}