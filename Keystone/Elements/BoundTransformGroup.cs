using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using Keystone.Types;
using Keystone.IO;
using Keystone.Resource;

namespace Keystone.Elements
{
    public abstract class BoundTransformGroup : Transform, IGroup, IBoundVolume
    {
        protected BoundingBox mBox;
        protected BoundingSphere mSphere;
        protected List<Node> mChildren;

        internal BoundTransformGroup(string id) : base(id)
        {
            Shareable = false;
        }

        #region IResource Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties;

            properties = new Settings.PropertySpec[1 + tmp.Length];
            tmp.CopyTo(properties, 1);

            properties[0] = new Settings.PropertySpec("boundingbox", typeof(BoundingBox).Name);

            if (!specOnly)
            {
                properties[0].DefaultValue = mBox;
            }
            return properties;
        }

        public override void SetProperties(Settings.PropertySpec[] properties)
        {
            if (properties == null) return;
            base.SetProperties(properties);

            for (int i = 0; i < properties.Length; i++)
            {
                // use of a switch allows us to pass in all or a few of the propspecs depending
                // on whether we're loading from xml or changing a single property via server directive
                switch (properties[i].Name)
                {
                	// note: boundingbox is primarily required for Server that will not load geometry beyond
                	// basic primitives and hulls, but it's _also_ used for fixed size Zones who's boundingbox
                	// is only ever computed via a fixed box value setting assigned during ctor.
                    case "boundingbox":
                        if (properties[i].DefaultValue != null)
                            mBox = (BoundingBox)properties[i].DefaultValue;
                        break;
                }
            }
        }
        #endregion 

        #region IGroup
        protected void AddChild(Node child)
        {
        	if (child == null) return;
            if (mChildren == null) mChildren = new List<Node>();

            // same node cannot be added twice
            if (mChildren.Contains(child))
                throw new ArgumentException("BoundTransformGroup.AddChild() - ERROR: Node '" + child.ID  + "' of type '" + child.TypeName + "' already exists.");
            
            // ChangeSource.Child will ensure this notification is sent upwards
            // NOTE: always set flags early before any .AddParent/.Add so that if those other methods
            // result in calls to update a bounding box for example, that we then don't return here only
            // to set those flags as dirty again after they've just been updated
            SetChangeFlags(Keystone.Enums.ChangeStates.ChildNodeAdded, Keystone.Enums.ChangeSource.Child);

            child.AddParent(this);
            mChildren.Add(child);
        }
        
        protected void InsertChild(Node child)
        {
            if (mChildren == null) mChildren = new List<Node>();

            // same node cannot be added twice
            if (mChildren.Contains(child))
                throw new ArgumentException("Node '" + child.ID  + "' of type '" + child.TypeName + "' already exists.");
            
            // ChangeSource.Child will ensure this notification is sent upwards
            // NOTE: always set flags early before any .AddParent/.Add so that if those other methods
            // result in calls to update a bounding box for example, that we then don't return here only
            // to set those flags as dirty again after they've just been updated
            SetChangeFlags(Keystone.Enums.ChangeStates.ChildNodeAdded, Keystone.Enums.ChangeSource.Child);

            child.AddParent(this);
            mChildren.Insert(0, child);
        }
                
        public virtual void MoveChildOrder(string childID, bool down)
        {
            Group.MoveChildOrder(mChildren, childID, down);
        }

        public void GetChildIDs(string[] filteredTypes, out string[] childIDs, out string[] childNodeTypes)
        {
            Group.GetChildIDs(mChildren, filteredTypes, out childIDs, out childNodeTypes);
        }

        /// <summary>
        /// Finds the first descendant that matches.
        /// </summary>
        public Node FindDescendant(Predicate<Keystone.Elements.Node> match)
        {
            return Group.FindDescendant(mChildren.ToArray(), match);
        }

        public Node FindNodeAtDescendantIndex(int index)
        {
            int startCount = 0;
            return Group.GetNodeAtDescendantIndex(this, index, ref startCount);
        }

        public Node FindDescendantOfType(string typename, bool recurse)
        {
            return Group.FindDescendantOfType(this, typename, recurse);
        }

        public Node[] FindDescendantsOfType(string typename, bool recurse)
        {
            return Group.FindDescendantsOfType(this, typename, recurse);
        }

        public Node[] Children
        {
            get
            {
                if (mChildren == null) return null;
                return mChildren.ToArray();
            }
        }

        public virtual int ChildCount
        {
            get
            {
                if (mChildren == null) return 0;
                return mChildren.Count;
            }
        }

        public virtual void RemoveChildren()
        {
        	
            while (mChildren != null)
            {
                RemoveChild(mChildren[0]);
            }
        }

        public virtual void RemoveChild(Node child)
        {
            try
            {
                // NOTE: There is no recursive call of child.RemoveChildren()  INSTEAD
                // we call child.RemoveParent(this)  and only from there would it call .RemoveChildren() on it'self 
                // if it had no more parents and was truely detached from the scene.  This is why it works!
                // see Node.cs.RemoveParent for more details.
                Repository.DecrementRef(child);  // 

                // inside of the .RemoveParent() call SceneBase.NodeDetached() is called.  
                // This is ok as it rarely is used even when temporarily Moving a node and having to
                // remove it's old parent before adding the new.  
                child.RemoveParent(this);
                mChildren.Remove(child);
                
                if (mChildren.Count == 0) mChildren = null;
                //SetChangeFlags(global::Keystone.Enums.ChangeStates.GeometryAddedRemoved); // TODO: this is not correct here but notice how there is no flags set at all... not even for io write update.  we need flag for that at least right?
                // ChangeSource.Child will ensure this notification is sent upwards
                SetChangeFlags(Keystone.Enums.ChangeStates.ChildNodeRemoved, Keystone.Enums.ChangeSource.Child);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("BoundTransformGroup.RemoveChild() - " + ex.Message);
            }
        }
#endregion

        #region Disposable
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            RemoveChildren();
        }
        #endregion

        /// <summary>
        /// Region space (meaning final scaling is taken into account) value of the height of this entity
        /// </summary>
        public double Height
        {
            get
            {
                // note: we do not use _box because this way if dirty, the box will update first
                if (BoundingBox == BoundingBox.Initialized()) return 0;
                return BoundingBox.Height;
            }
        }

        /// <summary>
        /// Region space value of the lowest vertex position of entity.
        /// </summary>
        public double Floor
        {
            get
            {
                // note: we do not use _box because this way if dirty, the box will update first
                if (BoundingBox == BoundingBox.Initialized()) return this.Translation.y;
                return BoundingBox.Min.y;
            }
        }

        #region IBoundVolume Members
        public virtual BoundingBox BoundingBox 
        {
            get
            {
                if (BoundVolumeIsDirty) 
                    UpdateBoundVolume();

                return mBox;
            }
        }

        public virtual BoundingSphere BoundingSphere
        {
            get
            {
                if (BoundVolumeIsDirty)
                    UpdateBoundVolume();

                return mSphere;
            }
        }


        public virtual bool BoundVolumeIsDirty
        {
            get
            {
                // TODO: this doesnt include all the | needed for everything, but this function is shared by everything and maybe
                // it should be overriden to only include the relevant subset needed for specifc BoundedElementGroup types...
                return ((mChangeStates & 
                    ( Enums.ChangeStates.BoundingBox_TranslatedOnly | Enums.ChangeStates.BoundingBoxDirty)) !=
                        Enums.ChangeStates.None);
            }
        }

        protected virtual void UpdateBoundVolume()
        {
            DisableChangeFlags (Keystone.Enums.ChangeStates.BoundingBoxDirty);
        }
        #endregion
    }
}