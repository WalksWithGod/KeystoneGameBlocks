using System;
using System.Collections.Generic;
using System.Diagnostics;
using Keystone.Collision;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.Lights;
using Keystone.Octree;
using Keystone.Portals;
using Keystone.Quadtree;
using Keystone.Resource;
using Keystone.Types;
using MTV3D65;
using Keystone.QuadTree;


namespace Keystone.Traversers
{
    public class Locator : ITraverser
    {
        // stats
        private int _nodesVisited;
        private SceneNode _startNode;
        private Vector3d[] _point;
        private bool _foundInternalSector;
        private RegionNode _root;
        private RegionNode _result;
        private Stack<Vector3d> _relativeTranslations;

        public delegate void PickCallback(PickResults[] results);

        public Locator(RegionNode root)
        {
            _root = root;
            _relativeTranslations = new Stack<Vector3d>();
        }

        public RegionNode Result
        {
            get { return _result; }
        }

        public void Clear()
        {
            _result = null;
            _startNode = null;
            _relativeTranslations.Clear();
        }

        public void BeginSearch(Node startNode, Ray r, PickCallback cb, object context)
        {
        }

        public void EndSearch(object context)
        {
        }

        // pick accuracy is irrelevant to this because a point can only be tested
        // against bounding volumes like the OctreeNodes, Sectors and such.  It's not designed
        // to "pick" things as such.  
        public RegionNode Search(RegionNode startNode, Vector3d point)
        {
            _point = new Vector3d[] {point};
            return Search(startNode, _point);
        }

        public RegionNode Search(RegionNode startNode, BoundingBox box)
        {
            _point = box.Vertices;
            return Search(startNode, _point);
        }

        private RegionNode Search(RegionNode startNode, Vector3d[] point)
        {
            Clear();
            _startNode = startNode;
            _point = point;
            // TODO: here the goal should be to find the inner most Region since Root region which is often the start Region
            // encompasses all of them.  But we do need to dynamically translate these bounding boxes for children right because
            // they are not in world position but relative.  So we could just maintain another stack as we do with Culler _or_ we could
            // always ensure that SceneNode's are always in camera relative coords... and the reason they'd be camera relative and not global
            // is because global values for massive game worlds would get too big.  But moving the spatialgraph like this everyframe
            // would tie the graph to a specific camera and thus we'd need a seperate graph for each viewport...  instead, a spatial graph
            // only responsibility is to manage spatial relationships between game world entities.
            if (startNode.Children != null)
            {
                foreach (SceneNode child in startNode.Children)
                {
                    if (child is RegionNode)
                        child.Traverse(this, null);
                            // TODO: here we need to either translate the search points or translate the SceneNode's

                    // now just translating the points seems like a no brainer, but as far as consistancy is concerned, its not something
                    // as easily done with culling because consider a portal frustum that needs to be updated as you traverse across regions
                    // but maybe that's not so bad afterall..  hrm...
                }
            }
            if (_result == null) _result = _root;

            //CheckSectors();
            return _result;
        }

        //private void CheckSectors()
        //{
        //    foreach (Region sector in C)
        //        if (BoundingBoxContainsAllPoints(sector.BoundingBox))
        //        {
        //            _result = sector.RegionNode ;
        //            break;
        //        }
        //}

        //private bool BoundingBoxContainsAllPoints(BoundingBox box)
        //{
        //    foreach (Vector3d p in _point)
        //        if (!box.Contains(p)) return false;

        //    return true;
        //}

        #region ITraverser Members

        public object Apply(Node node, object data)
        {
            throw new NotImplementedException();
        }

        // the idea is simple, since any internal region that overlaps multiple regions MUST be tracked such that it's represented
        // in _all_ branches to regions part of it's bounds is contained, then no matter which path we follow we are guaranteed to find the inner
        // most region.  This does also mean that if we have a ship within a ship and the larger ship moves and straddles multiple zones, then
        // if the inner ship (that is docked but was moved by virtue of being a child to the parent ship) winds up straddling multiple zones
        // then shouldnt it too have multiple scenenodes?  NO!  WHy not?  Well it should be obvious from the start, regardless of where it's parent is
        // in one zone or multiple, once you traverse to the parent you will always be able to traverse the inner child zones regardless of which branch you took.
        public object Apply(RegionNode node, object data)
        {
            _relativeTranslations.Push(node.Region.Translation);

            Vector3d[] p = _point;
            for (int i = 0; i < p.Length; i++)
            {
                p[i] += _relativeTranslations.Peek();
            }

            if ((node.BoundingBox != null) && (node.BoundingBox.Contains(p)))
            {
                _result = node;
                    // we could push this on a stack, but here we always return the inner most since _result is always overwritten
                if (node.Children != null)
                {
                    foreach (SceneNode child in node.Children)
                    {
                        if (child is RegionNode)
                        {
                            object tmp = child.Traverse(this, data);
                            break;
                                // we want to break out of the foreach the minute we find a workable path because all paths are guaranteed to lead to the deepest
                        }
                    }
                }
            }
            _relativeTranslations.Pop();
            // _result gets overwrritten with deepest

            //  even though we have OctreeHost and OctreeNode here, i think what we're really "locating" are
            // things and not spatial areas. So ultimately we want a "Region" and not a "regionNode"?  So our "start" should
            // be a region or entity and SceneNode's should be strictly an internally helpful construct.
            // Now typically with "portals" if we have an entity that is in one sector and moves, we just need to test if 
            // the entity has moved through a portal and if so test if its entirely moved or partially and then add it to both 
            // until it's fully in one or the other.  This is annoying but simple still.  But the difference here is our sectors dont
            // overlap.  
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
                
        public object Apply(SceneNode node, object data)
        {
            throw new Exception("Method not implemented");
        }

        public object Apply(OctreeOctant octant, object data)
        {
            throw new NotImplementedException();
        }

        public object Apply(QuadtreeQuadrant quadrant, object data)
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
            //if ((region.BoundingBox != null) && (BoundingBoxContainsAllPoints (region.BoundingBox)))
            //{
            //    _result = region;
            //    _foundInternalSector = true;
            //    return; 
            //}

            //if (region.Portals != null)
            //    foreach (Portal p in region.Portals)
            //        p.Destination.Traverse(this); //TODO: we cant just traverse this way or we'll recurse forever.
            // seems our best bet is to simply itterate through a list of all internal sectors instead :/  hrm.
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

        public object Apply(Quadrant o, object data)
        {
            throw new NotImplementedException();
        }

        public object Apply(Quadtree.Sector o, object data)
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

        #endregion
    }
}

//Public Class QTreeFind : Implements IQTreeTraverser

//    Delegate Sub NodeFound(ByVal node As QTreeNode)
//    Private _nodePickedCallback As NodeFound
//    Private _progressCallback As IQTreeTraverser.ProgressUpdate
//    Private _progresscallbackIsAvailable As Boolean = False
//    Private _nodesTraversed As Int32
//    Private _pick As PointF
//    Private _found As Boolean = False
//    Private _depth As Int32 = 0

//    Public Sub New(ByVal pickedCallback As NodeFound, Optional ByVal progressCallback As IQTreeTraverser.ProgressUpdate = Nothing)
//        If pickedCallback Is Nothing Then Throw New ArgumentNullException
//        _nodePickedCallback = pickedCallback
//        If progressCallback IsNot Nothing Then
//            _progressCallback = progressCallback
//            _progresscallbackIsAvailable = True
//        End If
//    End Sub

//    Public ReadOnly Property NodesTraversed() As Integer Implements IQTreeTraverser.NodesTraversed
//        Get
//            Return _nodesTraversed
//        End Get
//    End Property

//    Public WriteOnly Property Find() As String
//        Set(ByVal value As String)
//            Dim s() As String = Split(value, " ", 2)

//            _pick = New PointF(CSng(Val(s(0))), CSng(Val(s(1))))
//        End Set
//    End Property

//    Public Sub Apply(ByRef node As QTreeNodeBranch) Implements IQTreeTraverser.Apply
//        If _progresscallbackIsAvailable Then _progressCallback.Invoke(_nodesTraversed)
//        _nodesTraversed += 1

//        If Not _found Then
//            If node.Center = _pick Then
//                _nodePickedCallback.Invoke(node)
//                _found = True
//                Exit Sub
//            End If

//            _depth += 1
//            If node.Bounds.Contains(_pick) Then
//                For Each child As QTreeNode In node.Children
//                    child.traverse(Me)
//                Next
//            End If
//            _depth -= 1
//        End If
//    End Sub

//    Public Sub Apply(ByRef node As QTreeNodeLeaf) Implements IQTreeTraverser.Apply
//        If _progresscallbackIsAvailable Then _progressCallback.Invoke(_nodesTraversed)
//        _nodesTraversed += 1

//        If node.Center = _pick Then
//            _nodePickedCallback.Invoke(node)
//            _found = True
//        End If

//    End Sub

//    Public Sub Apply(ByVal node As MiniMeshSysBranch) Implements IQTreeTraverser.Apply
//        If _progresscallbackIsAvailable Then _progressCallback.Invoke(_nodesTraversed)
//        _nodesTraversed += 1

//        If Not _found Then
//            If node.Center = _pick Then
//                _nodePickedCallback.Invoke(node)
//                _found = True
//                Exit Sub
//            End If

//            _depth += 1
//            If node.Bounds.Contains(_pick) Then
//                For Each child As QTreeNode In node.Children
//                    child.traverse(Me)
//                Next
//            End If
//            _depth -= 1
//        End If
//    End Sub

//    Public Sub Apply(ByVal node As MiniMeshSysLeaf) Implements IQTreeTraverser.Apply
//        If _progresscallbackIsAvailable Then _progressCallback.Invoke(_nodesTraversed)
//        _nodesTraversed += 1

//        If node.Center = _pick Then
//            _nodePickedCallback.Invoke(node)
//            _found = True
//        End If
//    End Sub

//End Class