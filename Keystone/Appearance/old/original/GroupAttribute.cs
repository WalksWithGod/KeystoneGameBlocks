using System;
using Keystone.Elements;
using Keystone.Shaders;
using Keystone.Traversers;
using System.Collections.Generic;

namespace Keystone.Appearance
{
    // todo: I should probably just rename this to GroupAppearance
    //       The main difference here is there is no Apply functions for GroupAppearances
    //        but the types ofchild nodes are similar except for other GroupAppearances
    // GroupAttribute objects are added to Appearance nodes.
    // Appearance node's can contain multiple GroupAttribute nodes.
    // A single GroupAttribute node corresponds to a single Mesh or Actor group or Terrain chunk
    public class GroupAttribute : Group
    {
        // NOTE: no seperate lighting modes per group allowed.  Only one under the primary
        //       Appearance node.
        protected string mTVName; // the group name which can be read/set by Get/SetGroupName 
        protected Material _material;
        protected Shader _shader;
        protected Layer[] _layers;
        protected int _layersCount = 0;
        protected int _hashCode;
        internal List<int> mDeletedLayers;

        public GroupAttribute(string id) : base(id)
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
        public Material Material { get { return _material; } }

        public Layer[] Layers
        {
            get { return _layers; }
        }

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
                if (_children[i] is Material || _children[i] is Shader)
                    tmp = BitConverter.GetBytes(((IPageableTVNode)_children[i]).TVIndex);
                else if (_children[i] is Layer)
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


        public void AddChild(Material child)
        {
            if (_material != null)
                throw new ArgumentException("Node of type ' " + child.TypeName +
                                            "' already exists. Only one instance of this type allowed under parent type '" +
                                            TypeName);

            base.AddChild(child);
            _material = child;
            SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
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

        public void AddChild(Layer child)
        {
            base.AddChild(child);
            AddTexture(child);
            SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
        }

        public override void RemoveChild(Node child)
        {
            if (child is Material)
            {
                System.Diagnostics.Debug.Assert(_material == child);
                _material = null;
            }
            else if (child is Shader)
            {
                System.Diagnostics.Debug.Assert(_shader == child);
                _shader = null;
            }
            else if (child is Layer)
            {
                RemoveTexture((Layer)child);
            }
            base.RemoveChild(child);
            SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
        }

        public void RemoveShader() { if (_shader != null) RemoveChild(_shader); }
        public void RemoveMaterial() { if (_material != null) RemoveChild(_material); }


        private void AddTexture(Layer tex)
        {
            if (_layersCount == 0)
            {
                _layers = new Layer[1];
                _layers[0] = tex;
            }
            else
            {
                Layer[] tmp = new Layer[_layersCount + 1];
                _layers.CopyTo(tmp, 0);
                tmp[_layersCount] = tex;
                _layers = tmp;
            }
            _layersCount++;
        }

        private void RemoveTexture(Layer tex)
        {
            // find the location of the existing texture
            int loc = 0;
            foreach (Layer t in _layers)
            {
                if (t == tex) break;
                loc++;
            }
            System.Collections.Generic.List<Layer> tmp = new System.Collections.Generic.List<Layer>();
            for (int i = 0; i < _layers.Length; i++)
            {
                if (i != loc) tmp.Add(_layers[i]);
            }
            if (tmp.Count > 0)
            {
                _layers = tmp.ToArray();
                _layersCount = _layers.Length;
            }
            else
            {
                _layers = null;
                _layersCount = 0;
            }

            if (mDeletedLayers == null)
                mDeletedLayers = new List<int>();

            mDeletedLayers.Add(tex.LayerID);
        }
    }
}