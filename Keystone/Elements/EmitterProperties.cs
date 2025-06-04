using System;
using Keystone.Types;
using Keystone.KeyFrames;
using MTV3D65;

namespace Keystone.Elements
{
    internal struct AttractorProperties
    {
        public int Index;
        public string Name;
        public bool Enable;

        public bool DirectionField;
        public TV_3DVECTOR FieldDirection;
        public TV_3DVECTOR Position;
        public TV_3DVECTOR Attenuation;
        public float Radius;
        public float AttractionRepulsionConstant;
        public CONST_TV_ATTRACTORVELOCITYPOWER VelocityDependancy;

        internal AttractorProperties(int index)
        {
            Index = index;
            Name = "";
            Enable = true;
            DirectionField = true;
            FieldDirection = new TV_3DVECTOR(0, 0, 0);
            Position = new TV_3DVECTOR(0, 0, 0);
            Attenuation = new TV_3DVECTOR(0, 0, 0);
            Radius = 10f;
            AttractionRepulsionConstant = 10f;
            VelocityDependancy = CONST_TV_ATTRACTORVELOCITYPOWER.TV_ATTRACTORPOWER_CONSTANT;
        }

        internal void SetProperty(string propertyName, object newValue)
        {
            switch (propertyName)
            {
                case "name":
                    Name = (string)newValue;
                    break;
                case "enable":
                    Enable = (bool)newValue;
                    break;
                case "index":
                    break;
                case "directionfield":
                    DirectionField = (bool)newValue;
                    break;
                case "fielddirection":
                    FieldDirection = Helpers.TVTypeConverter.ToTVVector((Vector3f)newValue);
                    break;
                case "position":
                    Position = Helpers.TVTypeConverter.ToTVVector((Vector3f)newValue);
                    break;
                case "radius":
                    Radius = (float)newValue;
                    break;
                case "attenuation":
                    Attenuation = Helpers.TVTypeConverter.ToTVVector((Vector3f)newValue);
                    break;
                case "repulsionconstant":
                    AttractionRepulsionConstant = (float)newValue;
                    break;
                case "velocitypower":
                    VelocityDependancy = (CONST_TV_ATTRACTORVELOCITYPOWER)(int)newValue;
                    break;
            }
        }

        internal Settings.PropertySpec[] GetProperties()
        {
            Settings.PropertySpec[] properties = new Settings.PropertySpec[10];

            string category = "general";
            properties[0] = new Settings.PropertySpec("name", typeof(string), category, (object)Name);
            properties[1] = new Settings.PropertySpec("enable", typeof(bool), category, Enable);
            properties[2] = new Settings.PropertySpec("index", typeof(int), category, Index);

            category = "type";
            properties[3] = new Settings.PropertySpec("directionfield", typeof(bool), category, (bool)DirectionField);
            properties[4] = new Settings.PropertySpec("fielddirection", typeof(Vector3f).AssemblyQualifiedName, category, "", Helpers.TVTypeConverter.FromTVVector3f(Position), "", typeof(TypeConverters.Vector3fConverter));

            category = "shape";
            properties[5] = new Settings.PropertySpec("position", typeof(Vector3f).AssemblyQualifiedName, category, "", Helpers.TVTypeConverter.FromTVVector3f(Position), "", typeof(TypeConverters.Vector3fConverter));
            properties[6] = new Settings.PropertySpec("radius", typeof(float), category, (float)Radius);
            properties[7] = new Settings.PropertySpec("attenuation", typeof(Vector3f).AssemblyQualifiedName, category, "", Helpers.TVTypeConverter.FromTVVector3f(Attenuation), "", typeof(TypeConverters.Vector3fConverter));

            category = "behavior";
            properties[8] = new Settings.PropertySpec("repulsionconstant", typeof(float), category, AttractionRepulsionConstant);
            properties[9] = new Settings.PropertySpec("velocitypower", typeof(int), category, (int)VelocityDependancy);

            return properties;
        }

        /// <summary>
        /// Convert the KGB friendly types to TV3D types and after all assigned, Apply() to the particle system.
        /// If necessary destroy and create a new TVParticleSystem.
        /// </summary>
        /// <param name="properties"></param>
        internal void SetProperties(Settings.PropertySpec[] properties)
        {
            if (properties == null) return;


            for (int i = 0; i < properties.Length; i++)
            {
                // use of a switch allows us to pass in all or a few of the propspecs depending
                // on whether we're loading from xml or changing a single property via server directive
                switch (properties[i].Name)
                {
                    case "name":
                        Name = (string)properties[i].DefaultValue;
                        break;
                    case "index":
                        // Index = (int)properties[i].DefaultValue;
                        break;
                    case "enable":
                        Enable = (bool)properties[i].DefaultValue;
                        break;
                    case "directionfield":
                        DirectionField = (bool)properties[i].DefaultValue;
                        break;
                    case "fielddirection":
                        FieldDirection = Helpers.TVTypeConverter.ToTVVector((Vector3f)properties[i].DefaultValue);
                        break;
                    case "position":
                        Position = Helpers.TVTypeConverter.ToTVVector((Vector3f)properties[i].DefaultValue);
                        break;
                    case "radius":
                        Radius = (float)properties[i].DefaultValue;
                        break;
                    case "attenuation":
                        Attenuation = Helpers.TVTypeConverter.ToTVVector((Vector3f)properties[i].DefaultValue);
                        break;
                    case "repulsionconstant":
                        AttractionRepulsionConstant = (float)properties[i].DefaultValue;
                        break;
                    case "velocitypower":
                        VelocityDependancy = (CONST_TV_ATTRACTORVELOCITYPOWER)(int)properties[i].DefaultValue;
                        break;
                }
            }
        }

        public void Apply(ParticleSystem system)
        {
            // todo: we should have seperate right mouse click menu items for creating attractors that use DirectionField or not. Then when populating the propertygrid, if DirectionField == false, we should not show the option to set the FieldDirection
            // DirectionField = false; todo: direction can only be set during system.mSystem.CreateAttractor()
            //Index = system.mSystem.CreateAttractor(DirectionField); // this will result in a change of the Index which we dont want since our AttractorCards in the plugin  will have the wrong index. If you want to create an attractor directionfield parrameter = true, delete the AttractorCard and create a new one
            system.mSystem.SetAttractorFieldDirection(Index, FieldDirection);
            system.mSystem.SetAttractorPosition(Index, Position);
            system.mSystem.SetAttractorRadius(Index, Radius);
            Attenuation = new TV_3DVECTOR(0, 0, 0);
            system.mSystem.SetAttractorAttenuation(Index, Attenuation);
            Enable = true;
            system.mSystem.SetAttractorEnable(Index, Enable);
            
            system.mSystem.SetAttractorParameters(Index, AttractionRepulsionConstant, VelocityDependancy);
        }
    }

    internal struct EmitterProperties
    {
        public int Index;
        public string Name;
        public bool Enable;

        // type
        //CONST_TV_EMITTERTYPE.TV_EMITTER_POINTSPRITE
        //CONST_TV_EMITTERTYPE.BILLBOARD
        //CONST_TV_EMITTERTYPE.TV_EMITTER_MINIMESH
        public CONST_TV_EMITTERTYPE type;
        public int maxParticles;


        // shape 
        // CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_POINT
        //CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_BOXSURFACE
        //CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_SPHERESURFACE
        //CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_SPHEREVOLUME
        //CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_BOXVOLUME
        public CONST_TV_EMITTERSHAPE shape;
        public int systemRadius;
        public TV_3DVECTOR systemBoxSize;

        // visuals
        int textureIndex; // TODO: it doesn't appear we can change the texture during Appearance.Apply() once initial SetBillboard(emitterINdex, textureIndex, sizeX, sizeY) is called.  This could also mean that all duplicates must share same texture
                          //mSystem.SetPointSprite(mEmitters[0], textureIndex);

        // TODO: if i want to store the mShaderPath and mTexturePath, does this preclude us from using different shaders and/or textures on different instances of the same ParticleSystem?  For Actors and TVMesh we can do that because the GroupAttributes are not auto-managed or treated as part of the geometry.
        //       But without those paths, when we LoadTVResource() we cant set the visuals until the user manually does it or we load an entire prefab that just happens to contain the emitter's GroupAttributes too
        //       I think i should just auto-manage it when the emitters are added.
        public string TexturePath;
        public string CubeTextureMaskPath;
        public string ShaderPath;
        public string MeshPath; // relative resource path of the mesh we intend to use for creating the TVMinimesh

        public TVMiniMesh Minimesh;
        public TVMesh mMesh;
        public TVShader mShader;

        public int BillboardWidth;
        public int BillboardHeight;
        public TV_COLOR Color;
        public bool BlendEx;
        public CONST_TV_BLENDEX BlendExSrc;
        public CONST_TV_BLENDEX BlendExDest;
        public CONST_TV_BLENDINGMODE BlendingMode;
        public CONST_TV_PARTICLECHANGE Change;
        public bool AlphaTest;
        public int alphaRefValue;
        public bool depthWrite;


        
        // behavior
        public TV_3DVECTOR gravityVector;
        public bool useGravity;

        public bool EnableMainDirection;
        public TV_3DVECTOR direction;
        public TV_3DVECTOR randomDirectionFactor;

        //float hasLifeTime;
        public float lifeTime;
        public float randomLifeTime;

        public float power;
        public float powerRandom;
        public float Speed;
        public bool Loop;

        public bool EnableSpawnInterpolation;
        public bool BillboardRotationEnable;
        public float AngularSpeed;
        public bool EnableMinimeshRotationScale;

        // animation
        public CONST_TV_PARTICLEEMITTER_KEYUSAGE EmitterKeyUsage;
        public CONST_TV_PARTICLE_KEYUSAGE ParticleKeyUseage;

        //public EmitterKeyframe[] mEmitterFrames;
        //public ParticleKeyframe[] mParticleFrames;
        public TV_PARTICLEEMITTER_KEYFRAME[] mEmitterFrames;
        public TV_PARTICLE_KEYFRAME[] mParticleFrames;

        internal EmitterProperties(int index)
        {
            Name = "";
            Index = index;

            MeshPath = null;
            TexturePath = null;
            CubeTextureMaskPath = null;
            ShaderPath = null;

            Enable = true;
            mShader = null;
            Minimesh = null;
            mMesh = null;

            // type
            //CONST_TV_EMITTERTYPE.TV_EMITTER_BILLBOARD
            //CONST_TV_EMITTERTYPE.TV_EMITTER_POINTSPRITE
            //CONST_TV_EMITTERTYPE.TV_EMITTER_MINIMESH
            type = CONST_TV_EMITTERTYPE.TV_EMITTER_BILLBOARD; // NOTE: I think minimesh is only if you want 3D geometric particles instead of 2d points and quads
            maxParticles = 32;
            //Index = mSystem.CreateEmitter(CONST_TV_EMITTERTYPE.TV_EMITTER_POINTSPRITE, maxParticles);
            //Index = mSystem.CreateEmitter(CONST_TV_EMITTERTYPE.TV_EMITTER_MINIMESH, maxParticles);

            // shape 
            shape = CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_POINT;
            systemRadius = 128; // System.SetEmitterSphereRadius(Index, systemSize);
            systemBoxSize = new TV_3DVECTOR(systemRadius, systemRadius, systemRadius);

            // visuals
            textureIndex = 4; // TODO: it doesn't appear we can change the texture during Appearance.Apply() once initial SetBillboard(emitterINdex, textureIndex, sizeX, sizeY) is called.  This could also mean that all duplicates must share same texture
                              //mSystem.SetPointSprite(mEmitters[0], textureIndex);
            Minimesh = null;
            BillboardWidth = 128;
            BillboardHeight = 128;
            Color = new TV_COLOR(1f, 0f, 0f, 1f);
            BlendEx = false;
            BlendExSrc = CONST_TV_BLENDEX.TV_BLENDEX_SRCALPHA;
            BlendExDest = CONST_TV_BLENDEX.TV_BLENDEX_DESTALPHA;
            BlendingMode = CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA;
            Change = CONST_TV_PARTICLECHANGE.TV_CHANGE_NO;
           

            AlphaTest = false;
            alphaRefValue = 128;
            depthWrite = false;

            // behavior
            gravityVector = new TV_3DVECTOR(0, 0, 0);
            useGravity = false;

            EnableMainDirection = true;
            direction = new MTV3D65.TV_3DVECTOR(0, 1, 0);
            randomDirectionFactor = new TV_3DVECTOR(0, 0, 0);

            //hasLifeTime = true;
            lifeTime = 10f;
            randomLifeTime = 0f;

            power = 10f;
            powerRandom = 0f;
            Speed = 10f;
            Loop = true;
            EnableSpawnInterpolation = false;
            BillboardRotationEnable = false;
            AngularSpeed = 0;
            EnableMinimeshRotationScale = false;

            // animation
            EmitterKeyUsage = CONST_TV_PARTICLEEMITTER_KEYUSAGE.TV_EMITTER_KEYUSAGE_LOCALPOSITION;
            

            mEmitterFrames = null; ;
            //CONST_TV_PARTICLEEMITTER_KEYUSAGE.TV_EMITTER_KEYUSAGE_BOXSIZE;
            //CONST_TV_PARTICLEEMITTER_KEYUSAGE.TV_EMITTER_KEYUSAGE_DEFAULTCOLOR;
            //CONST_TV_PARTICLEEMITTER_KEYUSAGE.TV_EMITTER_KEYUSAGE_LIFETIME;
            //CONST_TV_PARTICLEEMITTER_KEYUSAGE.TV_EMITTER_KEYUSAGE_LOCALPOSITION;
            //CONST_TV_PARTICLEEMITTER_KEYUSAGE.TV_EMITTER_KEYUSAGE_MAINDIRECTION;
            //CONST_TV_PARTICLEEMITTER_KEYUSAGE.TV_EMITTER_KEYUSAGE_POWER;
            //CONST_TV_PARTICLEEMITTER_KEYUSAGE.TV_EMITTER_KEYUSAGE_RADIUS;
            //CONST_TV_PARTICLEEMITTER_KEYUSAGE.TV_EMITTER_KEYUSAGE_SPEED;
            //emitterFrame.fGeneratorSphereRadius
            //emitterFrame.fParticleLifeTime;
            //emitterFrame.fPower;
            //emitterFrame.fSpeed;
            //emitterFrame.vDefaultColor;
            //emitterFrame.vGeneratorBoxSize;
            //emitterFrame.vLocalPosition;
            //emitterFrame.vMainDirection;

            ParticleKeyUseage = CONST_TV_PARTICLE_KEYUSAGE.TV_PARTICLE_KEYUSAGE_COLOR;
            //CONST_TV_PARTICLE_KEYUSAGE.TV_PARTICLE_KEYUSAGE_ANGLE_XYZ =1
            //CONST_TV_PARTICLE_KEYUSAGE.TV_PARTICLE_KEYUSAGE_COLOR = 2
            //CONST_TV_PARTICLE_KEYUSAGE.TV_PARTICLE_KEYUSAGE_SIZE = 4
            //CONST_TV_PARTICLE_KEYUSAGE.TV_PARITCLE_KEYUSAGE_ANGLE_Z = 8

            mParticleFrames = null;
            //tmpFrame.cColor
            //tmpFrame.fKey
            //tmpFrame.fSize
            //tmpFrame.vRotation
        }

        internal void SetProperty(string propertyName, object newValue)
        {
            switch (propertyName)
            {
                case "name":
                    Name = (string)newValue;
                    break;
                case "type":
                    type = (CONST_TV_EMITTERTYPE)(int)newValue;
                    break;
                case "index":
                    // Index = (int)newValue;
                    break;
                case "enable":
                    Enable = (bool)newValue;
                    break;
                case "maxparticles":
                    maxParticles = (int)newValue;
                    break;
                case "shape":
                    shape = (CONST_TV_EMITTERSHAPE)(int)newValue;
                    break;
                case "systemsize":
                    systemRadius = (int)newValue;
                    break;
                case "boxsize":
                    systemBoxSize = Helpers.TVTypeConverter.ToTVVector((Vector3f)newValue);
                    break;
                case "billboardwidth":
                    BillboardWidth = (int)newValue;
                    break;
                case "billboardheight":
                    BillboardHeight = (int)newValue;
                    break;
                case "color":
                    Color = Helpers.TVTypeConverter.ToTVColor((Color)newValue);
                    break;
                case "blendex":
                    BlendEx = (bool)newValue;
                    break;
                case "blendexsrc":
                    BlendExSrc = (CONST_TV_BLENDEX)(int)newValue;
                    break;
                case "blendexdest":
                    BlendExDest = (CONST_TV_BLENDEX)(int)newValue;
                    break;
                case "blendingmode":
                    BlendingMode = (CONST_TV_BLENDINGMODE)(int)newValue;
                    break;
                case "change":
                    Change = (CONST_TV_PARTICLECHANGE)(int)newValue;
                    break;
                case "alphatest":
                    AlphaTest = (bool)newValue;
                    break;
                case "alpharefvalue":
                    alphaRefValue = (int)newValue;
                    break;
                case "depthwrite":
                    depthWrite = (bool)newValue;
                    break;
                case "gravityvector":
                    gravityVector = Helpers.TVTypeConverter.ToTVVector((Vector3f)newValue);
                    break;
                case "usegravity":
                    useGravity = (bool)newValue;
                    break;
                case "enablemaindirection":
                    EnableMainDirection = (bool)newValue;
                    break;
                case "direction":
                    direction = Helpers.TVTypeConverter.ToTVVector((Vector3f)newValue);
                    break;
                case "randomdirectionfactor":
                    randomDirectionFactor = Helpers.TVTypeConverter.ToTVVector((Vector3f)newValue);
                    break;
                case "lifetime":
                    lifeTime = (float)newValue;
                    break;
                case "randomlifetime":
                    randomLifeTime = (float)newValue;
                    break;
                case "power":
                    power = (float)newValue;
                    break;
                case "powerrandom":
                    powerRandom = (float)newValue;
                    break;
                case "speed":
                    Speed = (float)newValue;
                    break;
                case "loop":
                    Loop = (bool)newValue;
                    break;
                case "enablespawninterpolation":
                    EnableSpawnInterpolation = (bool)newValue;
                    break;
                case "enablebillboardrotation":
                    BillboardRotationEnable = (bool)newValue;
                    break;
                case "angularspeed":
                    AngularSpeed = (float)newValue;
                    break;
                case "enablemnimeshrotationscale":
                    EnableMinimeshRotationScale = (bool)newValue;
                    break;
                case "emitterkeyuseage":
                    //CONST_TV_PARTICLEEMITTER_KEYUSAGE.TV_EMITTER_KEYUSAGE_LOCALPOSITION = 1;
                    //CONST_TV_PARTICLEEMITTER_KEYUSAGE.TV_EMITTER_KEYUSAGE_MAINDIRECTION = 2;
                    //CONST_TV_PARTICLEEMITTER_KEYUSAGE.TV_EMITTER_KEYUSAGE_DEFAULTCOLOR = 4;
                    //CONST_TV_PARTICLEEMITTER_KEYUSAGE.TV_EMITTER_KEYUSAGE_RADIUS = 8;
                    //CONST_TV_PARTICLEEMITTER_KEYUSAGE.TV_EMITTER_KEYUSAGE_SPEED = 16;
                    //CONST_TV_PARTICLEEMITTER_KEYUSAGE.TV_EMITTER_KEYUSAGE_LIFETIME = 32;
                    //CONST_TV_PARTICLEEMITTER_KEYUSAGE.TV_EMITTER_KEYUSAGE_POWER = 64;
                    //CONST_TV_PARTICLEEMITTER_KEYUSAGE.TV_EMITTER_KEYUSAGE_BOXSIZE = 128;
                    EmitterKeyUsage = (CONST_TV_PARTICLEEMITTER_KEYUSAGE)(int)newValue;
                    break;
                case "particlekeyuseage":
                    //CONST_TV_PARTICLE_KEYUSAGE.TV_PARTICLE_KEYUSAGE_ANGLE_XYZ = 1;
                    //CONST_TV_PARTICLE_KEYUSAGE.TV_PARTICLE_KEYUSAGE_COLOR = 2;
                    //CONST_TV_PARTICLE_KEYUSAGE.TV_PARTICLE_KEYUSAGE_SIZE = 4;
                    //CONST_TV_PARTICLE_KEYUSAGE.TV_PARITCLE_KEYUSAGE_ANGLE_Z = 8
                    ParticleKeyUseage = (CONST_TV_PARTICLE_KEYUSAGE)(int)newValue;
                    break;
                case "emitterkeyframes":
                    mEmitterFrames = Helpers.TVTypeConverter.ToTVEmitterKeyFrames( (Keystone.KeyFrames.EmitterKeyframe[])newValue); // TEMP: Helpers.TVTypeConverter.ToTVEmitterKeyFrames((EmitterKeyframe[])properties[i].DefaultValue);
                    break;
                case "particlekeyframes":
                    mParticleFrames = Helpers.TVTypeConverter.ToTVParticleKeyFrames( (Keystone.KeyFrames.ParticleKeyframe[])newValue); //TEMP Helpers.TVTypeConverter.ToTVParticleKeyFrames ((ParticleKeyframe[])properties[i].DefaultValue);
                    break;
            }
        }

        // NOTE: we always include the default values and dont just return empty PropertySpec[]
        internal Settings.PropertySpec[] GetProperties()
        {
            Settings.PropertySpec[] properties = new Settings.PropertySpec[38];
            
            string category = "general";
            properties[0] = new Settings.PropertySpec("name", typeof(string), category, (object)Name);
            properties[1] = new Settings.PropertySpec("enable", typeof(bool), category, Enable);
            properties[2] = new Settings.PropertySpec("index", typeof(int), category, Index);

            category = "type";
            properties[3] = new Settings.PropertySpec("type", typeof(int), category, (int)type);
                        
            properties[4] = new Settings.PropertySpec("maxparticles", typeof(int), category, maxParticles);

            category = "shape";
            properties[5] = new Settings.PropertySpec("shape", typeof(int), category, (int)shape);
            properties[6] = new Settings.PropertySpec("systemsize", typeof(int), category, systemRadius);// sphere radius
            properties[7] = new Settings.PropertySpec("boxsize", typeof(Vector3f).AssemblyQualifiedName, category, "", Helpers.TVTypeConverter.FromTVVector3f(systemBoxSize), "", typeof(TypeConverters.Vector3fConverter));
            properties[8] = new Settings.PropertySpec("billboardwidth", typeof(int), category, BillboardWidth);
            properties[9] = new Settings.PropertySpec("billboardheight", typeof(int), category, BillboardHeight);

            category = "visuals";
            properties[10] = new Settings.PropertySpec("color", typeof(Keystone.Types.Color).AssemblyQualifiedName, category,"", Helpers.TVTypeConverter.FromTVColor(Color), "", typeof(TypeConverters.ColorConverter));
            properties[11] = new Settings.PropertySpec("blendex", typeof(bool), category, BlendEx);
            properties[12] = new Settings.PropertySpec("blendexsrc", typeof(int), category, (int)BlendExSrc);
            properties[13] = new Settings.PropertySpec("blendexdest", typeof(int), category, (int)BlendExDest);
            properties[14] = new Settings.PropertySpec("blendingmode", typeof(int), category, (int)BlendingMode);
            properties[15] = new Settings.PropertySpec("change", typeof(int), category, (int)Change);
            properties[16] = new Settings.PropertySpec("alphatest", typeof(bool), category, AlphaTest);
            properties[17] = new Settings.PropertySpec("alpharefvalue", typeof(int), category, alphaRefValue);
            properties[18] = new Settings.PropertySpec("depthwrite", typeof(bool), category, depthWrite);
            category = "behavior";
            properties[19] = new Settings.PropertySpec("gravityvector", typeof(Vector3f).AssemblyQualifiedName, category, "", Helpers.TVTypeConverter.FromTVVector3f(gravityVector), "", typeof(TypeConverters.Vector3fConverter));
            properties[20] = new Settings.PropertySpec("usegravity", typeof(bool), category, useGravity);
            properties[21] = new Settings.PropertySpec("enablemaindirection", typeof(bool), category, EnableMainDirection);
            properties[22] = new Settings.PropertySpec("direction", typeof(Vector3f).AssemblyQualifiedName, category,"", Helpers.TVTypeConverter.FromTVVector3f(direction), "", typeof(TypeConverters.Vector3fConverter));
            properties[23] = new Settings.PropertySpec("randomdirectionfactor", typeof(Vector3f).AssemblyQualifiedName, category, "", Helpers.TVTypeConverter.FromTVVector3f(randomDirectionFactor), "", typeof(TypeConverters.Vector3fConverter));
            properties[24] = new Settings.PropertySpec("lifetime", typeof(float), category, lifeTime);
            properties[25] = new Settings.PropertySpec("randomlifetime", typeof(float), category, randomLifeTime);
            properties[26] = new Settings.PropertySpec("power", typeof(float), category, power);
            properties[27] = new Settings.PropertySpec("powerrandom", typeof(float), category, powerRandom);
            properties[28] = new Settings.PropertySpec("speed", typeof(float), category, Speed);
            properties[29] = new Settings.PropertySpec("loop", typeof(bool), category, Loop);
            properties[30] = new Settings.PropertySpec("enablespawninterpolation", typeof(bool), category, EnableSpawnInterpolation);
            properties[31] = new Settings.PropertySpec("enablebillboardrotation", typeof(bool), category, BillboardRotationEnable);
            properties[32] = new Settings.PropertySpec("angularspeed", typeof(float), category, AngularSpeed);
            properties[33] = new Settings.PropertySpec("enablemnimeshrotationscale", typeof(bool), category, EnableMinimeshRotationScale);

            category = "keyframes";
            properties[34] = new Settings.PropertySpec("emitterkeyuseage", typeof(int), category, (int)EmitterKeyUsage);
            properties[35] = new Settings.PropertySpec("particlekeyuseage", typeof(int), category, (int)ParticleKeyUseage);
            properties[36] = new Settings.PropertySpec("emitterkeyframes", typeof(EmitterKeyframe[]), category, Helpers.TVTypeConverter.FromTVEmitterKeyframes(mEmitterFrames));
            properties[37] = new Settings.PropertySpec("particlekeyframes", typeof(ParticleKeyframe[]), category, Helpers.TVTypeConverter.FromTVParticleKeyFrames(mParticleFrames));

            //Minimesh = null // this should probably just be a path to a MinimeshGeometry;     

            //hasLifeTime = true;

            return properties;
        }

        /// <summary>
        /// Convert the KGB friendly types to TV3D types and after all assigned, Apply() to the particle system.
        /// If necessary destroy and create a new TVParticleSystem.
        /// </summary>
        /// <param name="properties"></param>
        internal void SetProperties(Settings.PropertySpec[] properties)
        {
            if (properties == null) return;


            for (int i = 0; i < properties.Length; i++)
            {
                // use of a switch allows us to pass in all or a few of the propspecs depending
                // on whether we're loading from xml or changing a single property via server directive
                switch (properties[i].Name)
                {
                    case "type":
                        type = (CONST_TV_EMITTERTYPE)(int)properties[i].DefaultValue;
                        break;
                    case "name":
                        Name = (string)properties[i].DefaultValue;
                        break;
                    case "index":
                       // Index = (int)properties[i].DefaultValue;
                        break;
                    case "enable":
                        Enable = (bool)properties[i].DefaultValue;
                        break;
                    case "maxparticles":
                        maxParticles = (int)properties[i].DefaultValue;
                        break;
                    case "shape":
                        shape = (CONST_TV_EMITTERSHAPE)(int)properties[i].DefaultValue;
                        break;
                    case "systemsize":
                         systemRadius = (int)properties[i].DefaultValue;
                        break;
                    case "boxsize":
                        systemBoxSize = Helpers.TVTypeConverter.ToTVVector ((Vector3f)properties[i].DefaultValue);
                        break;
                    case "billboardwidth":
                        BillboardWidth = (int)properties[i].DefaultValue;
                        break;
                    case "billboardheight":
                        BillboardHeight = (int)properties[i].DefaultValue;
                        break;
                    case "color":
                        Color = Helpers.TVTypeConverter.ToTVColor((Color)properties[i].DefaultValue);
                        break;
                    case "blendingmode":
                        //CONST_TV_BLENDINGMODE.TV_BLEND_NO = 0
                        //CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA = 1
                        //CONST_TV_BLENDINGMODE.TV_BLEND_ADD = 2
                        //CONST_TV_BLENDINGMODE.TV_BLEND_COLOR = 3
                        //CONST_TV_BLENDINGMODE.TV_BLEND_ADDALPHA = 4
                        //CONST_TV_BLENDINGMODE.TV_BLEND_MULTIPLY = 5
                        BlendingMode = (CONST_TV_BLENDINGMODE)(int)properties[i].DefaultValue;
                        break;
                    case "blendex":
                        BlendEx = (bool)properties[i].DefaultValue;
                        break;
                    case "blendexsrc":
                        //CONST_TV_BLENDEX.TV_BLENDEX_ZERO = 1
                        //CONST_TV_BLENDEX.TV_BLENDEX_ONE = 2
                        //CONST_TV_BLENDEX.TV_BLENDEX_SRCCOLOR = 3
                        //CONST_TV_BLENDEX.TV_BLENDEX_INVSRCCOLOR = 4
                        //CONST_TV_BLENDEX.TV_BLENDEX_SRCALPHA = 5
                        //CONST_TV_BLENDEX.TV_BLENDEX_INVSRCALPHA = 6
                        //CONST_TV_BLENDEX.TV_BLENDEX_DESTALPHA = 7
                        //CONST_TV_BLENDEX.TV_BLENDEX_INVDESTALPHA = 8
                        BlendExSrc = (CONST_TV_BLENDEX)(int)properties[i].DefaultValue;
                        break;
                    case "blendexdest":
                        BlendExDest = (CONST_TV_BLENDEX)(int)properties[i].DefaultValue;
                        break;
                    case "change":
                        //CONST_TV_PARTICLECHANGE.TV_CHANGE_ALPHA = 1
                        //CONST_TV_PARTICLECHANGE.TV_CHANGE_COLOR = 2
                        //CONST_TV_PARTICLECHANGE.TV_CHANGE_NO = 3
                        Change = (CONST_TV_PARTICLECHANGE)(int)properties[i].DefaultValue;
                        break;
                    case "alphatest":
                        AlphaTest = (bool)properties[i].DefaultValue;
                        break;
                    case "alpharefvalue":
                        alphaRefValue = (int)properties[i].DefaultValue;
                        break;
                    case "depthwrite":
                        depthWrite = (bool)properties[i].DefaultValue;
                        break;
                    case "gravityvector":
                        gravityVector = Helpers.TVTypeConverter.ToTVVector((Vector3f)properties[i].DefaultValue);
                        break;
                    case "usegravity":
                        useGravity = (bool)properties[i].DefaultValue;
                        break;
                    case "enablemaindirection":
                        EnableMainDirection = (bool)properties[i].DefaultValue;
                        break;
                    case "direction":
                        direction =Helpers.TVTypeConverter.ToTVVector((Vector3f)properties[i].DefaultValue);
                        break;
                    case "randomdirectionfactor":
                        randomDirectionFactor = Helpers.TVTypeConverter.ToTVVector ((Vector3f)properties[i].DefaultValue);
                        break;
                    case "lifetime":
                        lifeTime = (float)properties[i].DefaultValue;
                        break;
                    case "randomlifetime":
                        randomLifeTime = (float)properties[i].DefaultValue;
                        break;
                    case "power":
                         power = (float)properties[i].DefaultValue;
                        break;
                    case "powerrandom":
                        powerRandom = (float)properties[i].DefaultValue;
                        break;
                    case "speed":
                        Speed = (float)properties[i].DefaultValue;
                        break;
                    case "loop":
                        Loop = (bool)properties[i].DefaultValue;
                        break;
                    case "enablespawninterpolation":
                        EnableSpawnInterpolation = (bool)properties[i].DefaultValue;
                        break;
                    case "enablebillboardrotation":
                        BillboardRotationEnable = (bool)properties[i].DefaultValue; 
                        break;
                    case "angularspeed":
                        AngularSpeed = (float)properties[i].DefaultValue;
                        break;
                    case "enablemnimeshrotationscale":
                        EnableMinimeshRotationScale = (bool)properties[i].DefaultValue; 
                        break;
                    case "emitterkeyuseage":
                        EmitterKeyUsage = (CONST_TV_PARTICLEEMITTER_KEYUSAGE)(int)properties[i].DefaultValue;
                        break;
                    case "particlekeyuseage":
                        ParticleKeyUseage = (CONST_TV_PARTICLE_KEYUSAGE)(int)properties[i].DefaultValue;
                        break;
                    case "emitterkeyframes":
                        mEmitterFrames = Helpers.TVTypeConverter.ToTVEmitterKeyFrames((EmitterKeyframe[])properties[i].DefaultValue);
                        break;
                    case "particlekeyframes":
                        mParticleFrames = Helpers.TVTypeConverter.ToTVParticleKeyFrames ((ParticleKeyframe[])properties[i].DefaultValue);
                        break;

                }
            }

        }



        // TODO: I think the cameraSpacePositioning needs to set the System.SetGlobalPosition() each frame
        internal void Apply(ParticleSystem system)
        {

            system.mSystem.SetEmitterEnable(Index, false);
            // type - billboard, pointsprite or minimesh
            // note: WE CAN'T Create a news emitter here because it will screw up the indices used by the EmitterCards. If we
            //       need to create a new emitter, user should delete and then create a new emitter
            //           Index = system.mSystem.CreateEmitter(type, maxParticles);

            
            // NOTE: type is defined on emitter creation. If we want to change the type, we need to destroy and recreate the eentire emitter
            //if (type == CONST_TV_EMITTERTYPE.TV_EMITTER_POINTSPRITE)
            //    system.mSystem.SetPointSprite(Index, textureIndex);
            //else if (type == CONST_TV_EMITTERTYPE.TV_EMITTER_BILLBOARD)
            //{
            //    //system.mSystem.SetBillboard(Index, textureIndex);
            //    system.mSystem.SetBillboard(Index, textureIndex, BillboardWidth, BillboardHeight);
            //}
            //else
            //    system.mSystem.SetMiniMesh(Index, Minimesh);


            // shape
            system.mSystem.SetEmitterShape(Index, shape);

            //MTV3D65.CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_POINT // = 0;
            //MTV3D65.CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_SPHEREVOLUME // =1;
            //MTV3D65.CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_BOXVOLUME // = 2;
            //MTV3D65.CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_SPHERESURFACE // = 3;
            //MTV3D65.CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_BOXSURFACE // = 4;

            if (shape == CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_SPHEREVOLUME)
                system.mSystem.SetEmitterSphereRadius(Index, systemRadius);
            else if (shape == CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_BOXVOLUME)
                system.mSystem.SetEmitterBoxSize(Index, systemBoxSize);

            

            // visuals
            if (BlendEx)
                system.mSystem.SetEmitterAlphaBlendingEx(Index, Change, BlendExSrc, BlendExDest);
            else 
                system.mSystem.SetEmitterAlphaBlending(Index, Change, BlendingMode);

            system.mSystem.SetEmitterAlphaTest(Index, AlphaTest, alphaRefValue, depthWrite);
            system.mSystem.SetParticleDefaultColor(Index, Color);
            //system.mSystem.SetEmitterDirectionCubeMask(Index, !string.IsNullOrEmpty(CubeTextureMaskPath, iCubeTexture); // TODO: this gets set via DefaultAppearance.Apply() if there is a CubeMap texture loaded

            // behavior

            // TODO: I cant find any method that accepts these enum values
            //MTV3D65.CONST_TV_EMITTERMOVEMODE.TV_EMITTER_LERP // = 0;
            //MTV3D65.CONST_TV_EMITTERMOVEMODE.TV_EMITTER_NOLERP//= 1;
          
            system.mSystem.SetEmitterGravity(Index, useGravity, gravityVector);
            system.mSystem.SetEmitterDirection(Index, EnableMainDirection, direction, randomDirectionFactor); // todo: test randomDirectionFactor. I think the TV_3DVECTOR is meant to stores a 0.0 - 1.0 factor for each axis, or maybe its in degrees or radians
            system.mSystem.SetEmitterPower(Index, power, lifeTime, powerRandom, randomLifeTime);
            system.mSystem.SetEmitterSpeed(Index, Speed);
            system.mSystem.SetEmitterLooping(Index, Loop);


            system.mSystem.SetEmitterSpawnInterpolation(Index, EnableSpawnInterpolation);
            if (type == CONST_TV_EMITTERTYPE.TV_EMITTER_BILLBOARD)
                system.mSystem.SetBillboardRotation(Index, BillboardRotationEnable, AngularSpeed);
            else if (type == CONST_TV_EMITTERTYPE.TV_EMITTER_MINIMESH)
                system.mSystem.SetMiniMeshRotationScale(Index, EnableMinimeshRotationScale);

            // animation                        
            // - animate the entire emitter
            if (mEmitterFrames == null)
            {
                //TV_PARTICLEEMITTER_KEYFRAME[] dummy = new TV_PARTICLEEMITTER_KEYFRAME[1];
                //system.mSystem.SetEmitterKeyFrames(Index, 0, 0, dummy);
            }
             else
                system.mSystem.SetEmitterKeyFrames(Index, (int)EmitterKeyUsage, mEmitterFrames.Length, mEmitterFrames);

             // - animate the individual particles
             if (mParticleFrames == null)
             {
                //TV_PARTICLE_KEYFRAME[] dummy = new TV_PARTICLE_KEYFRAME[1];
                //system.mSystem.SetParticleKeyFrames(Index, 0, 0, dummy);

             }
            else
                system.mSystem.SetParticleKeyFrames(Index, (int)ParticleKeyUseage, mParticleFrames.Length, mParticleFrames);


            // system.mSystem.ResetAll();
            system.mSystem.ResetEmitter(Index); // NOTE: this is required for instance when disabling Looping and then trying to re-enable it

            system.mSystem.SetEmitterEnable(Index, Enable);
        }
    }
    
}