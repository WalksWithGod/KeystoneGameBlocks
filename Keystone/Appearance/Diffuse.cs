using System;
using System.Xml;
using Keystone.Resource;
using Keystone.Traversers;

namespace Keystone.Appearance
{

    public class Diffuse : Layer
    {
    	// Diffuse Layer is the only layer that allows AlphaTest = true and for AlphaRef value to be set

        // Alpha testing is a rejection test for source pixels. It's useful for
        // 0) WARNING: PNG textures with alpha channels seem to not work well. DDS is best.
        // G:\Program Files\nvidia corporation\NVIDIA DDS Utilities\nvdxt.exe can be used to batch convert and rescale to powers of 2.
        // 
        // 1) filtering pixels such that if alpha channel value on texture is >= some value 
        //    (eg. must be less than or equal to max value of 255 and is commonly 128) then
        //    that that pixel will get skipped. This is useful for rendering leaves or fences
        //    where high alpha is set on the empty areas of a leaf texture or a chain link fence
        //    texture and we want to skip rendering pixels that are mapped to those alpha texels.
        // 	  NOTE: typically for this case, we disable alpha BLENDING and 
		//    only use alpha for showing/not showing a pixel.  Again, great for imposters or fences, leaves 
		//    since no z is written for the skipped alpha tested (and failed) pixels.
		//    HOWEVER: it is possible to combine alpha testing and alpha blending since alpha values in alpha component
		//    of texture do not have to be 0 or 1.0		
        // 2) texture color keying. (which is really out of fashion and not used anymore. eg. like when you see
        //    UI textures with pink backgrounds
        //    NOTE:  In neither of these 2 cases is it used by Material opacity style apha blending.
        //    But since D3D has no color keying built in, TV3D (or D3DX utility lib) will load such a texture
		//    and set the alpha value to 0 for color keyed areas of the texture so that alphatest will then work with it.  
		//    Otherwise, your artists when making alpha test textures should paint the alpha channel to define the transparent areas.		
        protected bool _alphaTest = false;  // TODO: how to allow assignment of this via plugin?  Is it a property of the texture Layer?
                                    //       since we're not talking about Material style alpha blending.  This is texture alpha blending/testing.
                                    //       if there is no texture, there is no alphaTesting.
        
        // Won’t draw texel where Texture Alpha value < ReferenceValue.
		// Everything above is drawn. (values 0-255, recommended 128).  Thus
		// the effect is the greater the transparency, the less likely it will be drawn depending
		// on where the cutoff value is set.                            
    	protected int _alphaTestRefValue = 128;
    	// You should disable the write to the zbuffer if the mesh you are rendering is a alphablended mesh or an non-solid mesh.
        protected bool _alphaTestDepthBufferWriteEnable = true;
        

        private bool _textureClamp = false;
        
        public Diffuse(string id) : base(id)
        {
            _layerID = 0;
        }

        public bool AlphaTest {get {return _alphaTest;} set {_alphaTest = value;}}
        
        // Won’t draw texel where Texture Alpha value < ReferenceValue.
		// Everything above is drawn. (values 0-255, recommended 128).  Thus
		// the effect is the greater the transparency, the less likely it will be drawn depending
		// on where the cutoff value is set.
        public int AlphaTestRefValue {get {return _alphaTestRefValue;} set {_alphaTestRefValue = value;}}
        // You should disable the write to the zbuffer if the mesh you are rendering is a alphablended mesh or an non-solid mesh.
        // Normally, you are only disabling z-writes so that unsorted
        // alphatested groups in the mesh just render ontop of each other
        // with minimal artifacts. eg. leaves rendered in random order (as opposed to painter algo style) will just cover each other up and reduce visible artifacts around borders)
                // // AlphaTestDepthWriteEnable must default to true or zbuffer will be ignored when 
        // rendering transparent objects making them render over the top when they should 
        // be occluded by an opaque mesh
        /// <summary>
        /// Can be used to solve some z-fighting issues with overlapped billboards 
        /// but can cause other issues involving draw order since no depth information
        /// is ever written.  Using back to front (painters algorithm) helps here but
        /// that of course results in a lot of overdraw and eliminates early z test
        /// optimization in gpu.
        /// </summary>
        /// <remarks>
        /// GetHashCode() does take this var into account when computing.
        /// </remarks>
        public bool AlphaTestDepthWriteEnable {get {return _alphaTestDepthBufferWriteEnable;} set {_alphaTestDepthBufferWriteEnable = value;}}

        public bool TextureClampEnable {get {return _textureClamp;} set{_textureClamp = value;}}
        
        #region ResourceBase members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[4 + tmp.Length];
            tmp.CopyTo(properties, 4);

            properties[0] = new Settings.PropertySpec("alphatest", typeof(bool).Name);
            properties[1] = new Settings.PropertySpec("alphatestrefvalue", typeof(int).Name);
            properties[2] = new Settings.PropertySpec("alphatestdepthwrite", typeof(bool).Name);
			properties[3] = new Settings.PropertySpec("textureclamp", typeof(bool).Name);	

            if (!specOnly)
            {
                properties[0].DefaultValue = _alphaTest;
                properties[1].DefaultValue = _alphaTestRefValue;
                properties[2].DefaultValue = _alphaTestDepthBufferWriteEnable;
                properties[3].DefaultValue = _textureClamp;
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
                    case "alphatest":
                        _alphaTest = (bool)properties[i].DefaultValue;
                        break;
                    case "alphatestrefvalue":
                        _alphaTestRefValue = (int)properties[i].DefaultValue;
                        break;
                    case "alphatestdepthwrite":
                        _alphaTestDepthBufferWriteEnable = (bool)properties[i].DefaultValue;
                        break;
                    case "textureclamp":
                        _textureClamp = (bool)properties[i].DefaultValue;
                        break;
                }
            }
            SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceParameterChanged, Keystone.Enums.ChangeSource.Self);
        }
        #endregion
        
        #region ITraversable Members
        public override object Traverse(ITraverser target, object data)
        {
            return target.Apply(this, data);
        }
        #endregion 
        
        public override int GetHashCode()
		{
			int tmp = base.GetHashCode();
			
			uint result = 0;
            Keystone.Utilities.JenkinsHash.Hash(BitConverter.GetBytes(tmp), ref result);
            
            // take into account alphaTest vars {_alphaTest, _alphaTestRefValue, _alphaTestDepthBufferWriteEnable, _textureClamp}
            Keystone.Utilities.JenkinsHash.Hash(BitConverter.GetBytes(_alphaTest), ref result);
          	Keystone.Utilities.JenkinsHash.Hash(BitConverter.GetBytes(_alphaTestRefValue), ref result);
            Keystone.Utilities.JenkinsHash.Hash(BitConverter.GetBytes(_alphaTestDepthBufferWriteEnable), ref result);
			Keystone.Utilities.JenkinsHash.Hash(BitConverter.GetBytes(_textureClamp), ref result);
                
            return (int)result;
		}
 
    }
}