using System;
using Keystone.Utilities;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.IO;
using Keystone.Shaders;
using Keystone.Traversers;
using MTV3D65;

namespace Keystone.Appearance
{

    public abstract class Appearance : GroupAttribute // you can switch between multiple appearances if they're available
    {
        protected CONST_TV_LIGHTINGMODE _lightingMode;
        

        // _material for an overall Appearance is always equals to the one and only
        // child of type Material IF ANY in the _children array. 
        // for meshes or landscapes with many groups/chunks, rather than set a material in each
        // groupAttribute object, we can use the default.  If a group/chunk's group attribute
        // already has a material, that will be used instead.
        // a default Material is not required.  If you dont want to have this type of
        // cascading behavior, do not set the defaultMaterial by not adding a Material to an Appearance node.
        internal Appearance(string id) : base(id)
        {
            LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_BUMPMAPPING_TANGENTSPACE;
        }


        #region ITraversable Members
        public override object Traverse(ITraverser target, object data)
        {
            return target.Apply(this, data);
        }
        #endregion 

        #region ResourceBase members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[1 + tmp.Length];
            tmp.CopyTo(properties, 1);

            properties[0] = new Settings.PropertySpec("lightingmode", typeof(int).Name);

            if (!specOnly)
            {
                properties[0].DefaultValue = (int)_lightingMode;
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
                    case "lightingmode":
                        _lightingMode = (CONST_TV_LIGHTINGMODE)((int)properties[i].DefaultValue);
                        break;
                }
            }
            SetChangeFlags(Enums.ChangeStates.AppearanceParameterChanged, Enums.ChangeSource.Self);
        }
        #endregion

        internal override ChildSetter GetChildSetter()
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        
        public virtual CONST_TV_LIGHTINGMODE LightingMode
        {
            get { return _lightingMode; }
            set
            {
                _lightingMode = value;
                SetChangeFlags(Enums.ChangeStates.AppearanceParameterChanged, Enums.ChangeSource.Self);
            }
        }

        public int GroupAttributesCount
        {
            get
            {
                GroupAttribute[] groups = Groups;
                if (groups == null) return 0;
                return groups.Length;
            }
        }

        public GroupAttribute[] Groups
        {
            get
            {
                if (_children == null) return null;
                GroupAttribute[] tmp = new GroupAttribute[_children.Count];
                int j = 0;
                Node[] children = _children.ToArray();
                for (int i = 0; i < children.Length; i++)
                    if (children[i] is GroupAttribute)
                    {
                        tmp[j] = (GroupAttribute) children[i];
                        j++;
                    }

                // need to resize to just the length of the groups without materials
                GroupAttribute[] final = new GroupAttribute[j];
                Array.Copy(tmp, final, j);
                return final;
            }
        }

        #region IGroup members
        protected override void NotifyParents(Enums.ChangeStates flags)
        {
            if (mParents == null) return;
            foreach (Model  parent in mParents)
                if (parent != null)
                    parent.SetChangeFlags(flags, Enums.ChangeSource.Child);
        }
        #endregion


        public void AddChild(GroupAttribute child)
        {
            base.AddChild(child);
            // note: this method of assigning groups is fine so long as there are no bugs
            // with loopback and client both trying to construct a branch that is shared
            // between the two.
            ((GroupAttribute)child).GroupID = GroupAttributesCount - 1;

            SetChangeFlags(Enums.ChangeStates.AppearanceNodeChanged | Enums.ChangeStates.AppearanceParameterChanged, Enums.ChangeSource.Child);
        }

        protected override uint GetHashData()
        {
            uint result = base.GetHashData();

			// TODO: for now only overall appearance will allow different settings for
			// alpha settings
            byte[] bytes = BitConverter.GetBytes((int)_lightingMode);
            Keystone.Utilities.JenkinsHash.Hash(bytes, ref result);
            return result;
        }
    }
}