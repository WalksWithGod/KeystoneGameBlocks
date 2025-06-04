using Keystone.Types;

namespace Keystone.KeyFrames
{

    public struct EmitterKeyframe
    {
        //private int _emitterIndex;
        //private TVParticleSystem  _particle;
        //private TV_PARTICLEEMITTER_KEYFRAME _tvEmitterKeyFrame;
        //private int mUseage;
        private float _key;
        private Vector3f _direction;
        public Vector3f _position;
        public Vector3f _boxSize;
        private Color _color;
        private float _radius;
        private float _lifetime;
        private float _power;
        private float _speed;

        // scale
        // rotation

        private void Test()
        {
            //_tvEmitterKeyFrame.fGeneratorSphereRadius;
            //_tvEmitterKeyFrame.fKey;
            //_tvEmitterKeyFrame.fParticleLifeTime;
            //_tvEmitterKeyFrame.fPower;
            //_tvEmitterKeyFrame.fSpeed;
            //_tvEmitterKeyFrame.vGeneratorBoxSize;
            //_tvEmitterKeyFrame.vLocalPosition;
            //_tvEmitterKeyFrame.vMainDirection;
            //_tvEmitterKeyFrame.vDefaultColor;


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

        public Vector3f MainDirection
        {
            get { return _direction; }
            set { _direction = value; }
        }

        public Vector3f LocalPosition
        {
            get { return _position; }
            set { _position = value; }
        }

        public Vector3f BoxSize
        {
            get { return _boxSize; }
            set { _boxSize = value; }
        }

        public float Radius
        {
            get { return _radius; }
            set { _radius = value; }
        }

        public float Lifetime
        {
            get { return _lifetime; }
            set { _lifetime = value; }
        }

        public float Power
        {
            get { return _power; }
            set { _power = value; }
        }

        public float Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }

    }
}