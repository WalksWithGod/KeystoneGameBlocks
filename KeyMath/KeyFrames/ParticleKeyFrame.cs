using Keystone.Types;

namespace Keystone.KeyFrames
{
    // meh these just get set directly onto the particle system... wrapping this
    // seems only good for saving the results.. but actually wrapping isnt necessary
    // since if we wanted, we could just add them directly to the save/load for the Emitter 
    // although, we lose our AddChild() support.. but perhaps we dont want that either for ParticleSystems.
    // todoo: i think wrapping keyframes is only good for displaying and editing in the plugin propertygrid since the plugin knows nothing about TV3D types, but does have access to KeyStandardLibrary and KeyMath types
    public struct ParticleKeyframe
    {
        //private int _emitterIndex;
        //private TVParticleSystem  _particle;
        //private TV_PARTICLE_KEYFRAME _tvParticleKeyFrame;
        //private int mUseage;

        private float _key;
        private Color _color;
        private Vector3f _size;
        public Vector3f _rotation;

        private void Test()
        {
            //_tvParticleKeyFrame.cColor;
            //_tvParticleKeyFrame.fKey;
            //_tvParticleKeyFrame.fSize;
            //_tvParticleKeyFrame.vRotation

        }

        //public int Useage
        //{
        //    get { return mUseage; }
        //    set { mUseage = value; }
        //}

        public float Key
        {
            get { return _key; }
            set { _key = value; }
        }

        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }

        public Vector3f Size
        {
            get { return _size; }
            set { _size = value; }
        }

        public Vector3f Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }
    }
}