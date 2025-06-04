using System;
using Keystone.Elements;
using Keystone.Shaders;
using Keystone.Traversers;
using System.Collections.Generic;
using Keystone.FX;
using System.Diagnostics;

namespace Keystone.Appearance
{

    /// <summary>
    /// Programmable driven appearance.  Fixed Function appearance is now obsolete.
    /// </summary>
    public class GroupAttribute : Group
    {
        // NOTE: no seperate lighting modes per group allowed.  Only one under the primary
        //       Appearance node.
        protected string mTVName; // the group name which can be read/set by Get/SetGroupName 
        protected Shader _shader;
        protected Dictionary<string, object> mParameterValues;
        protected string mPersistedParameterValues;
        protected Texture[] _textures;
        protected int[] _layerAssignments;
        protected int _textureCount = 0;
        internal List<int> mDeletedTextures;
        protected int _hashCode;

        enum Modes // modes to enable in the shader
        {
            DIFFUSE_MAP,
            NORMAL_MAP,
            PARALLAX,
            SPECULAR_MAP,
            EMISSIVE_MAP
        }


        public GroupAttribute(string id)
                    : base(id)
        {
        }

        #region ITraversable Members
        public override void Traverse(ITraverser target)
        {
            target.Apply(this);
        }
        #endregion 

        public override ChildSetter GetChildSetter()
        {
            throw new NotImplementedException();
        }

        public Shader Shader { get { return _shader; } }


        protected virtual byte[] GetHashData()
        {
            if (_children == null || _children.Count == 0)
            {
                return null;
            }

            byte[] data = new byte[4 * _children.Count];
            byte[] tmp;

            for (int i = 0; i < _children.Count; i++)
            {
                // Shader don't need a "GetHashCode()" (OR DOES IT?! How would we know
                // if we made changes to the shader???? <-- todo: 
                // For Material however, changing the underlying material if the TVIndex is same 
                // will have immediate
                // visual change and changing a Shader also immediately updates during render(?)
                if (_children[i] is Shader)
                    tmp = BitConverter.GetBytes(((IPageableTVNode)_children[i]).TVIndex);
                else if (_children[i] is Texture)
                    tmp = BitConverter.GetBytes(_children[i].GetHashCode());
                else if (_children[i] is GroupAttribute)
                    tmp = BitConverter.GetBytes(((GroupAttribute)_children[i]).GetHashCode());
                else
                    throw new Exception("Unexpected child type...");

                Array.Copy(tmp, 0, data, i * 4, 4);
            }
            return data;
        }

        // The purpose of a hashcode
        // is NOT for tracking changes, it's for quickly tracking the difference in
        // apperance on a mesh between one instance render and the next so we know if we
        // have to update the texturse/shaders/materials on a mesh/actor/mini/etc or not.
        protected virtual void ComputeHashCode()
        {
            byte[] data = GetHashData();

            if (data == null || data.Length == 0)
                _hashCode = 0;

            _hashCode = BitConverter.ToInt32(Algorithms.CRC32.Crc32(data), 0);
        }

        public override int GetHashCode()
        {
            if ((_changeStates & Enums.ChangeStates.AppearanceChanged) > 0) ComputeHashCode();
            return _hashCode;
        }


        public void AddChild(Shader child)
        {
            if (_shader != null)
                throw new ArgumentException("Node of type ' " + child.TypeName +
                                            "' already exists. Only one instance of this type allowed under parent type '" +
                                            TypeName);
            base.AddChild(child);
            _shader = child;
            SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
        }

        public void AddChild(Texture child)
        {
            base.AddChild(child);
            AddTexture(child);
            SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
        }

        public override void RemoveChild(Node child)
        {
            if (child is FragmentProgram)
            {
                System.Diagnostics.Debug.Assert(_shader == child);
                _shader = null;
            }
            else if (child is Texture)
            {
                RemoveTexture((Texture)child);
            }
            base.RemoveChild(child);
            SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
        }

        public void RemoveShader() { if (_shader != null) RemoveChild(_shader); }
        

        private void AddTexture(Texture tex)
        {
            Helpers.ExtensionMethods.ArrayAppend(ref _textures, tex);
            _textureCount++;
        }

        private void RemoveTexture(Texture tex)
        {
            // find the location of the existing texture
            int loc = Helpers.ExtensionMethods.ArrayRemove (ref _textures, tex);
            if (loc == -1) throw new Exception();

            _textureCount = Helpers.ExtensionMethods.ArrayCount(_textures);

            if (mDeletedTextures == null)
                mDeletedTextures = new List<int>();

            // we track deleted layers so that the Appearance will know
            // which textureID to remove from the mesh via setTexture(layer, -1)
            mDeletedTextures.Add(_layerAssignments[loc]);
        }


        protected override void PropogateChangeFlags(global::Keystone.Enums.ChangeStates flags, Enums.ChangeSource source)
        {
            switch (flags)
            {
                case Keystone.Enums.ChangeStates.ScriptLoaded:
                    AssignParamaterValues();
                    // GroupAttribute does need to notify parents but Appearance (aka default/overall) does not
                    NotifyParents(flags);
                    // check registered flag, and if not, but parent != null, then register
                    break;
                default:
                    // if source of the flag is a child or self (and not a parent), notify parents
                    if (_parents != null && source == Enums.ChangeSource.Child || source == Enums.ChangeSource.Self)
                        NotifyParents(flags);
                    break;
            }
        }

        // todo: changing the underlying DomainObject must clear and re-init the mCustomPropertyValues collection
        public Settings.PropertySpec[] GetParameters(bool specOnly)
        {
            Settings.PropertySpec[] specs = null;

            if (_shader != null)
                specs = _shader.Parameters;

            if (!specOnly)
                if (specs != null)
                    for (int i = 0; i < specs.Length; i++)
                        specs[i].DefaultValue = GetParamaterValue(specs[i].Name);

            return specs;
        }

        private object GetParamaterValue(string parameterName)
        {
            if (mParameterValues == null) return null;

            // todo: will this return null if the key doesn't exist?
            if (mParameterValues.ContainsKey(parameterName))
                return mParameterValues[parameterName];

            return null;
        }


        // todo: should SetProperties return list of broken rules?
        // or should we have to call GetBrokenRules()
        public void SetParameterValues(Settings.PropertySpec[] properties)
        {
            // the values are stored in the actual entity
            if (mParameterValues == null) mParameterValues = new Dictionary<string, object>();
            for (int i = 0; i < properties.Length; i++)
            {
                if (!mParameterValues.ContainsKey(properties[i].Name))
                    mParameterValues.Add(properties[i].Name, null);

                string brokenDescription = "";
                bool result = false;
                
                // todo: some kind of range validation
                //if (_shader != null)
                //    result = _shader.Validate(properties[i].Name, properties[i].DefaultValue, out brokenDescription);
                
                if (result)
                {
                    // we only update the property value if the validation result passes
                    // use of a switch allows us to pass in all or a few of the propspecs depending
                    // on whether we're loading from xml or changing a single property via server directive
                    mParameterValues[properties[i].Name] = properties[i].DefaultValue;
                }
                else
                    Debug.WriteLine("Validation failed for property '" + properties[i].Name + "' - " + brokenDescription);
            }
        }

        protected void AssignParamaterValues()
        {
            if (_shader != null && _shader.TVResourceIsLoaded)
            {
                // There's no need to run Initialize since that is only done once per DomainObject
                // and not per Entity that shares it.  Afterall, at this point
                // the mDomainObject.CustomProperties array is already set.
                //mDomainObject.ExecuteScript("Initialize", null);

                Settings.PropertySpec[] custom = _shader.Parameters;

                if (string.IsNullOrEmpty(mPersistedParameterValues))
                {
                    // load the default values
                    mParameterValues = new Dictionary<string, object>();
                    for (int i = 0; i < custom.Length; i++)
                        mParameterValues.Add(custom[i].Name, custom[i].DefaultValue);
                }
                else
                {
                    // load the persited values from the Entity's xml
                    // NOTE: It's guaranteed that if persisted values exist, they will be
                    // restored here by the time any DomainObject or DomainObjectScript loads.
                    // 
                    // with the script loaded, if any custom property values are cached
                    // then we must assign them to the mCustomPropertyValues dictionary
                    // todo: if the script's crc32 doesnt match those of the custom properties
                    // then we must assume the script has changed and the current custom properties
                    // no longer apply...?
                    string[] values = mPersistedParameterValues.Split(new string[] { "," }, StringSplitOptions.None);
                    mParameterValues = new Dictionary<string, object>();
                    for (int i = 0; i < custom.Length; i++)
                        mParameterValues.Add(custom[i].Name, values[i]);

                    mPersistedParameterValues = null;
                }
            }
        }

    }


    //// todo: I should probably just rename this to GroupAppearance
    ////       The main difference here is there is no Apply functions for GroupAppearances
    ////        but the types ofchild nodes are similar except for other GroupAppearances
    //// GroupAttribute objects are added to Appearance nodes.
    //// Appearance node's can contain multiple GroupAttribute nodes.
    //// A single GroupAttribute node corresponds to a single Mesh or Actor group or Terrain chunk
    //public class GroupAttribute : Group
    //{
    //    // NOTE: no seperate lighting modes per group allowed.  Only one under the primary
    //    //       Appearance node.
    //    protected string mTVName; // the group name which can be read/set by Get/SetGroupName 
    //    protected Shader _shader;

    //    enum Modes // modes to enable in the shader
                   
    //    {
    //        DIFFUSE_MAP,
    //        NORMAL_MAP,
    //        PARALLAX,
    //        SPECULAR_MAP,
    //        EMISSIVE_MAP
    //    }
    //    // NOTE: Material and Layers are incompatible with Shader.  That is to say, if a Shader
    //    // child exists, not Material and no layers can exist.  If Material and/or Layers exist
    //    // no Shader child can exist.  So in other words, the appearance can either be
    //    // fixed function and use material and layers, or it can be Programmable and just rely
    //    // on the Shader and it's internal parameters and texture slots which we'll expose 
    //    // via the plugin.
    //    // In a sense, Material + Layer == Shader  == Appearance vs ProgrammableAppearance
    //    //                                             - although, we still need Textures placed
    //    //                                             - into correct slots of programmable to work
    //    // How about we divert to ProgrammableGroup and FixedFunctionGroupAttribute?

    //    protected Material _material;  // todo: material should be similar to type of 
    //                                   // fragmentProgram shader whereby we can only ever have
    //                                   // either a shader or material or neither but never both
    //                                   // Are they both types of Material?
    //                                   // ProgrammableMaterial, FixedFunctionMaterial

    //    // let's assume this is all ok... so how then do i get textures associated with
    //    // proper layer index for shader?  for fixed function its by the layerID in the layer
    //    // struct.  For programmable materials?
    //    //   - specifically, consider a GUI that shows us the paramters for each
    //    //     shader including the texture semantics that are listed..


    //    protected Layer[] _layers; // can also be thought of as simply TexureMods
    //    // todo: layers are ok, but they should be structs and they should not be GROUPS.
    //    //  we should keep textures under AppearanceGroup directly and have them at
    //    // same array indices as the textures.
    //    // Then have layer.ToString() for saving and constructor that accepts string to recreate.
    //    // HOWEVER, for Shader appearances, no layers are required and instead texture mod
    //    // data is stored as parameters in the shader who's persistant values are stored
    //    // here in the appearance and set against the Shader if the shader's current values
    //    // don't match... how do we know that tho?  The shader's current parameter values i suppose
    //    // we can track.
    //    //
    //    // eg. "diffuse_scale" "diffuse_offset" "diffuse_rotation"
    //    //     "normalmap_scale" "normalmap_offset" "normalmap_rotation"  etc
    //    // 
    //    protected int _layersCount = 0;
    //    internal List<int> mDeletedLayers;


    //    protected int _hashCode;
        

    //    public GroupAttribute(string id) : base(id)
    //    {
    //    }

    //    #region ITraversable Members
    //    public override void Traverse(ITraverser target)
    //    {
    //        target.Apply(this);
    //    }
    //    #endregion 

    //    public override ChildSetter GetChildSetter()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public Shader Shader { get { return _shader; } }
    //    public Material Material { get { return _material; } }

    //    public Layer[] Layers
    //    {
    //        get { return _layers; }
    //    }

    //    protected virtual byte[] GetHashData()
    //    {
    //        if (_children == null || _children.Count == 0)
    //        {
    //            return null;
    //        }

    //        byte[] data = new byte[4 * _children.Count];
    //        byte[] tmp;

    //        for (int i = 0; i < _children.Count; i++)
    //        {
    //            // Shader don't need a "GetHashCode()" (OR DOES IT?! How would we know
    //            // if we made changes to the shader???? <-- todo: 
    //            // For Material however, changing the underlying material if the TVIndex is same 
    //            // will have immediate
    //            // visual change and changing a Shader also immediately updates during render(?)
    //            if (_children[i] is Material || _children[i] is Shader)
    //                tmp = BitConverter.GetBytes(((IPageableTVNode)_children[i]).TVIndex);
    //            else if (_children[i] is Layer)
    //                tmp = BitConverter.GetBytes(_children[i].GetHashCode());
    //            else if (_children[i] is GroupAttribute)
    //                tmp = BitConverter.GetBytes(((GroupAttribute)_children[i]).GetHashCode());
    //            else
    //                throw new Exception("Unexpected child type...");

    //            Array.Copy(tmp, 0, data, i * 4, 4);
    //        }
    //        return data;
    //    }

    //    // The purpose of a hashcode
    //    // is NOT for tracking changes, it's for quickly tracking the difference in
    //    // apperance on a mesh between one instance render and the next so we know if we
    //    // have to update the texturse/shaders/materials on a mesh/actor/mini/etc or not.
    //    protected virtual void ComputeHashCode()
    //    {
    //        byte[] data = GetHashData();

    //        if (data == null || data.Length == 0)
    //            _hashCode = 0;
                        
    //        _hashCode = BitConverter.ToInt32(Algorithms.CRC32.Crc32(data), 0);
    //    }

    //    public override int GetHashCode()
    //    {
    //        if ((_changeStates & Enums.ChangeStates.AppearanceChanged) > 0) ComputeHashCode();
    //        return _hashCode;
    //    }


    //    public void AddChild(Material child)
    //    {
    //        if (_material != null)
    //            throw new ArgumentException("Node of type ' " + child.TypeName +
    //                                        "' already exists. Only one instance of this type allowed under parent type '" +
    //                                        TypeName);

    //        base.AddChild(child);
    //        _material = child;
    //        SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
    //    }

    //    public void AddChild(Shader child)
    //    {
    //        if (_shader != null)
    //            throw new ArgumentException("Node of type ' " + child.TypeName +
    //                                        "' already exists. Only one instance of this type allowed under parent type '" +
    //                                        TypeName);
    //        base.AddChild(child);
    //        _shader = child;
    //        SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
    //    }

    //    public void AddChild(Layer child)
    //    {
    //        base.AddChild(child);
    //        AddTexture(child);
    //        SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
    //    }

    //    public override void RemoveChild(Node child)
    //    {
    //        if (child is Material)
    //        {
    //            System.Diagnostics.Debug.Assert(_material == child);
    //            _material = null;
    //        }
    //        else if (child is Shader)
    //        {
    //            System.Diagnostics.Debug.Assert(_shader == child);
    //            _shader = null;
    //        }
    //        else if (child is Layer)
    //        {
    //            RemoveTexture((Layer)child);
    //        }
    //        base.RemoveChild(child);
    //        SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
    //    }

    //    public void RemoveShader() { if (_shader != null) RemoveChild(_shader); }
    //    public void RemoveMaterial() { if (_material != null) RemoveChild(_material); }


    //    private void AddTexture(Layer tex)
    //    {
    //        Helpers.ExtensionMethods.ArrayAppend(ref _layers, tex);
    //        _layersCount++;
    //    }

    //    private void RemoveTexture(Layer tex)
    //    {
    //        // find the location of the existing texture
    //        int loc = Helpers.ExtensionMethods.ArrayRemove(ref _layers, tex);
    //        if (loc == -1) throw new Exception();

    //        _layersCount = Helpers.ExtensionMethods.ArrayCount(_layers);
            

    //        if (mDeletedLayers == null)
    //            mDeletedLayers = new List<int>();

    //        // we track deleted layers so that the Appearance will know
    //        // which textureID to remove from the mesh via setTexture(layer, -1)
    //        mDeletedLayers.Add(tex.LayerID);
    //    }
    //}
}