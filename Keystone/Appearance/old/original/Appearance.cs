using System;
using Keystone.Algorithms;
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
        protected Appearance(string id) : base(id)
        {
            LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED;
        }

        public abstract int Apply(Terrain land, int appearanceFlags); // chunk?  I might make it so terrains are always handled per chunk
        public abstract int Apply(Mesh3d mesh, int appearanceFlags);
        public abstract int Apply(Actor3d actor, int appearanceFlags);
        public abstract int Apply(Minimesh2 mini, int appearanceFlags);

        #region ITraversable Members
        public override void Traverse(ITraverser target)
        {
            target.Apply(this);
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

            properties[0] = new Settings.PropertySpec("lightingmode", _lightingMode.GetType().Name);

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
        }
        #endregion

        public override ChildSetter GetChildSetter()
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        // todo: I think i need to take into account AlphaBlending and AlphaTestRef value now
        protected override void ComputeHashCode()
        {
            byte[] baseData = base.GetHashData();

            if ((baseData == null || baseData.Length == 0) && _lightingMode == CONST_TV_LIGHTINGMODE.TV_LIGHTING_NONE)
            {
                _hashCode = 0;
                return;
            }

            // combine the data with lighting mode bytes and then compute crc32 for hash
            int length;
            if (baseData == null) 
                length = 0;
            else 
                length = baseData.Length ;

            length += 4; // 4 bytes for the lighting data
            byte[] tmp;
            tmp = BitConverter.GetBytes((int)_lightingMode);

            byte[] data = new byte[length];
            Array.Copy(tmp, 0, data, 0, 4);
            Array.Copy(baseData, 0, data, 4, length - 4);

            _hashCode = BitConverter.ToInt32(CRC32.Crc32(data), 0);
        }


        public virtual CONST_TV_LIGHTINGMODE LightingMode
        {
            get { return _lightingMode; }
            set
            {
                _lightingMode = value;
                SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
            }
        }


        public GroupAttribute[] Groups
        {
            get
            {
                if (_children == null) return null;
                GroupAttribute[] tmp = new GroupAttribute[_children.Count];
                int j = 0;
                for (int i = 0; i < _children.Count; i++)
                    if (_children[i] is GroupAttribute)
                    {
                        tmp[j] = (GroupAttribute) _children[i];
                        j++;
                    }

                // need to resize to just the length of the groups without materials
                GroupAttribute[] final = new GroupAttribute[j];
                Array.Copy(tmp, final, j);
                return final;
            }
        }

        #region IGroup members
        protected override void PropogateChangeFlags(Enums.ChangeStates flags, Enums.ChangeSource source)
        {
            switch (flags)
            {
                case Enums.ChangeStates.AppearanceChanged :
                    // if it's a child or self we notify parent models
                    if (source == Enums.ChangeSource.Self || source == Enums.ChangeSource.Child)
                        NotifyParents(flags);
                    // if a parent for some reason such as having added this appearance, then buck stops here.  Do nothing.
                    
                    break;
            }
            //note: no need to notify parents because our traverser will compare the HashCode of 
            // this appearance with the appearance currently set on the Geometry in question
            // to determine if it needs to be re-Apply(geometry).   Or is this the incorrect way?  Maybe we should
            // not use a HashCode and instead set the actual flag?  

            // NOTE: if the HashCode == 0 then the Apply will be skipped since clearly no children exist
            // (todo: well, lightingmode will?  could maybe special case lightingMode to always
            // apply and to have the Geometry check its previous lighting modes before changing..
        }

        protected override void NotifyParents(Enums.ChangeStates flags)
        {
            if (_parents == null) return;
            foreach (Entity  parent in _parents)
                if (parent != null)
                    parent.SetChangeFlags(flags, Enums.ChangeSource.Child);
        }
        #endregion


        public void AddChild(GroupAttribute child)
        {
            base.AddChild(child);
            SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
        }
    }
}