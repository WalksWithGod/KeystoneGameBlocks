using System;
using System.Collections.Generic;
using Keystone.Elements;
using Keystone.Portals;
using Keystone.Types;
using KeyCommon.Flags;

namespace Keystone.Entities
{
    /// <summary>
    /// A container class has both an interior and exterior representation in the simulation
    /// but where the interior region is on seperate coordinate system from that of it's exterior which
    /// of course is on the coordinate system of it's own parent region.
    /// 
    /// There can be only one child Region within a container object, however the Container can
    /// contain child Container objects which in turn can host their own child Region as well.
    /// 
    /// When a child Region is added, instead of a sceneNode that links this 
    /// region under the Container's own sceneNode, this interior region's SceneNode is treated as being 
    /// independant like a Zone itself.
    ///
    /// Portals from the child Region to Exterior connect to the root region and during rendering from interior through
    /// an exterior portal, we render in the local space of the Interior region.  We do this by computing the Inverse
    /// of the Vehicle's regionMatrix and applying that to all exterior objects.
    /// 
    /// Portals from the Exterior to the child interior Region are attached as children ofthe Vehicle\Container and point
    /// to a destination that is an interior Region within that Vehicle. 
    /// </summary>
    public abstract class Container : ModeledEntity
    {
        protected Region _interior;       // a child region of a container is one that will host everything in a single coordinate system.  Our decks, rooms, accomodations, computers, generators, etc.        
        
        protected List <Container> _subContainers;  // chunks? turrets, superstructures are built just like any other container and then are added as Children
               

        protected Container(string name) : 
            base(name)
        {
            SetEntityAttributesValue((uint)EntityAttributes.ContainerExterior, true);
        }

        public Region Interior { get { return _interior; } }
        public Container[] SubContainers { get { return _subContainers.ToArray(); } }

        public override void SetProperties(Settings.PropertySpec[] properties)
        {
            base.SetProperties(properties);

            mEntityFlags |= EntityAttributes.ContainerExterior;
        }

        public void AddChild (Container subContainers)
        {
            if (_subContainers == null) _subContainers = new List<Container>();
            if (_subContainers.Contains(subContainers))
                throw new ArgumentException(
                    "Container already exist.  You must remove the existing first if you intend to replace it.");

            base.AddChild(subContainers); // we do NOT implement the base method here because we need a custom SceneNode creation + assignment implementation
            _subContainers.Add(subContainers);
        }

        public virtual void AddChild (Region interior)
        {
            if (_interior == null)
                _interior = interior;
            else if (_interior == interior)
                return; // already added
            else 
                throw new ArgumentException(
                    "Interior already exist.  You must remove the existing first if you intend to replace it.");

            base.AddChild(interior); // we do NOT implement the base method here because we need a custom SceneNode creation + assignment implementation
            SetChangeFlags(Enums.ChangeStates.GeometryAdded | Keystone.Enums.ChangeStates.BoundingBoxDirty, Enums.ChangeSource.Child);
        }

        public override void RemoveChild(Node child)
        {
            if (child is Region && _interior == child)
            {
                _interior = null; 
                SetChangeFlags(Enums.ChangeStates.GeometryRemoved | Keystone.Enums.ChangeStates.BoundingBoxDirty, Enums.ChangeSource.Child);
            }
            if (child is Container && _subContainers != null && _subContainers.Contains((Container)child))
            {
                _subContainers.Remove((Container)child);
                SetChangeFlags(Enums.ChangeStates.GeometryRemoved | Keystone.Enums.ChangeStates.BoundingBoxDirty, Enums.ChangeSource.Child);
            }
            base.RemoveChild(child);
        }


        /// <summary>
        /// Container overrides this method to avoid notifying a child Region
        /// since child Regions (like a CelledRegion) have their own coordinate systems 
        /// that are unaffected by a parent's transform change.  Notifying them unnecessarily
        /// is a huge waste in framerate.
        /// </summary>
        /// <param name="flags"></param>
        protected override void NotifyChildEntities(Keystone.Enums.ChangeStates flags)
        {
            if (mChildren == null || mChildren.Count == 0) return;
            Node[] children = mChildren.ToArray(); // to avoid issue of iterating _children and another child element being added during that for loop

            if ((flags & (Keystone.Enums.ChangeStates.Translated |
                Keystone.Enums.ChangeStates.BoundingBox_TranslatedOnly |
                Keystone.Enums.ChangeStates.BoundingBoxDirty |
                Keystone.Enums.ChangeStates.GeometryAdded |
                Keystone.Enums.ChangeStates.GeometryRemoved |
                Keystone.Enums.ChangeStates.KeyFrameUpdated |
                Keystone.Enums.ChangeStates.MatrixDirty |
                Keystone.Enums.ChangeStates.RegionMatrixDirty))  != 0)
            {
                // TODO: Problem here is that there may be other flags
                // and those other flags not specified above that if there 
                // maybe we do wish to notify a child Region, but those flags will be ignored
                // if it contains a flag that is being tested for 
                
                for (int i = 0; i < children.Length; i ++)
                {
                    // For movement related messages, child region only needs ONE specific GlobalMatrixDirty
                    // notification.  That will avoid unnecessary Region and local Matrix updates and
                    // only do GlobalMatrix update.
                    if (children[i] is Region)
                        // when parent Container has been transformed, the only notification 
                        // we need to send interior region is GlobalMatrixDirty so that we can
                        // in ScaleCuller and Picker compute a proper Matrix to convert our View matrix
                        children[i].SetChangeFlags(Enums.ChangeStates.GlobalMatrixDirty, Enums.ChangeSource.Parent);

                    else if (children[i] is BoundTransformGroup) 
                        children[i].SetChangeFlags(flags, Enums.ChangeSource.Parent);
                }
            }
            else 
            {
                for (int i = 0; i < children.Length; i++)
                    if (children[i] is BoundTransformGroup) 
                        children[i].SetChangeFlags(flags, Enums.ChangeSource.Parent);
            }
        }

        // note: Below is obsolete since we now know that non region Entities never include
        // their child entities bounding volumes in their own.  Only SceneNode's worry about hierarchical.
        // Thus there is no need to override the EntityBase.UpdateBoundVolume() implementation.
        //#region IBoundVolume
        ///// <summary>
        ///// Container's UpdateBoundVolume is nearly identicle to that of EntityBase however
        ///// Container's _EXCLUDE_ the interior node and any of it's children when iterating.
        ///// Afterall, the interior is a fixed region and it's bounds is incorrect with respect
        ///// to a moving Container and the interior doesn't really exist spatially within the ship
        ///// only logically and rendered with illusion of spatially related
        ///// </summary>
        //protected override void UpdateBoundVolume()
        //{
        //    // see if we can get away with just translating the existing bounding box?
        //    if ((_changeStates & Enums.ChangeStates.BoundingBox_TranslatedOnly & Enums.ChangeStates.BoundingBoxDirty) == Keystone.Enums.ChangeStates.BoundingBox_TranslatedOnly)
        //    {
        //        // TODO: test the above & test to see if it only triggers the following code
        //        // if just BoundingBox_TranslatedOnly is set
        //        DisableChangeFlags(Enums.ChangeStates.BoundingBox_TranslatedOnly);

        //        _box.Max -= _translationDelta;
        //        _box.Min -= _translationDelta;
        //        _sphere = new BoundingSphere(_box);
        //        return;
        //    }

        //    if (_children != null && _children.Count > 0)
        //    {
        //        _box = BoundingBox.Initialized();
        //        BoundingBox childboxes = BoundingBox.Initialized();
        //        for (int i = 0; i < _children.Count; i++)
        //        {
        //            // NOTE: Here we must skip child Region nodes because
        //            // child Region nodes under a Container entity qualify
        //            // as "interior" node's and they are on a seperate coordinate system. 
        //            // ruin the computation of the exterior bounding volume
        //            // since Interior doesn't exist in the same coordinate system.
        //            if (_children[i] is Region) continue;
        //            if (_children[i] is ModeledEntity) // child entities bbox are already in region coords
        //            {
        //                childboxes = BoundingBox.Combine(childboxes, ((ModeledEntity)_children[i]).BoundingBox);
        //            }
        //            else if (_children[i] is Geometry)
        //            {
        //                // child geometry have their bounding boxes box's transformed using their 
        //                // parent entity's RegionMatrix
        //                childboxes = BoundingBox.Combine(childboxes,
        //                                                 BoundingBox.Transform(
        //                                                     ((Geometry)_children[i]).BoundingBox, RegionMatrix));
        //            }
        //        }
        //        _box = BoundingBox.Combine(_box, childboxes);
        //    }

        //    DisableChangeFlags(Keystone.Enums.ChangeStates.BoundingBoxDirty | Enums.ChangeStates.BoundingBox_TranslatedOnly);

        //    if (_box == null) return; // if the box still hasnt been set
        //    _sphere = new BoundingSphere(_box);
        //}
        //#endregion
    }
}
