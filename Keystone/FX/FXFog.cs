using System;
using Keystone.Types;
using MTV3D65;

namespace Keystone.FX
{
	// TODO: this should become a scene node entity/volume that we can enter and which modifies
	//       the general rendering parameters
    public class FXFog : FXBase
    {
        private Color _color;
        private float _density = .0004f;
        private float _start = 0;
        private float _end = 5000;
        private CONST_TV_FOGTYPE _type;
        private CONST_TV_FOG _algorithm;

        public FXFog(CONST_TV_FOG algo, CONST_TV_FOGTYPE type)
        {
            _algorithm = algo;
            _type = type;
            UpdateType();
            UpdateParameters();
        }

        public CONST_TV_FOG Algorithm
        {
            get { return _algorithm; }
            set
            {
                _algorithm = value;
                UpdateType();
            }
        }

        public CONST_TV_FOGTYPE Type
        {
            get { return _type; }
            set
            {
                _type = value;
                UpdateType();
            }
        }

        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                CoreClient._CoreClient.Atmosphere.Fog_SetColor(_color.r, _color.g, _color.b);
            }
        }

        public float Density
        {
            get { return _density; }
            set
            {
                _density = value;
                UpdateParameters();
            }
        }

        public float Start
        {
            get { return _start; }
            set
            {
                _start = value;
                UpdateParameters();
            }
        }

        public float End
        {
            get { return _end; }
            set
            {
                _end = value;
                UpdateParameters();
            }
        }

        private void UpdateType()
        {
            CoreClient._CoreClient.Atmosphere.Fog_SetType(_algorithm, _type);
        }

        private void UpdateParameters()
        {
            CoreClient._CoreClient.Atmosphere.Fog_SetParameters(_start, _end, _density);
        }

        public override bool Enable
        {
            set
            {
                CoreClient._CoreClient.Atmosphere.Fog_Enable(value);
                UpdateParameters();
            }
        }

        public override void Register(IFXSubscriber subscriber)
        {
            throw new ArgumentException("Subscribers not needed or allowed.");
        }
    }
}