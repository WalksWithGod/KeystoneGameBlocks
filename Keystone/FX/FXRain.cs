using Keystone.Cameras;
using Keystone.Types;
using Keystone.Simulation;
using MTV3D65;

namespace Keystone.FX
{
    public class FXRain : FXBase
    {
        private double _density;
        private int _rainTexture;
        private double _fallSpeed;
        private int _generatorSpeed;
        private double _speed;

        private Weather _weather;
        private double _randomness;
        private double _distance;
        private double _particleSize;
        private double _radius;

        private const int MAX_BILLBOARD_PARTICLES = 10000; //max tv allowed billboard particles.  Use minimesh for more
        private TVParticleSystem pSystem;
        private int iEmt, iTex;

        public FXRain()
        {
            _semantic = FX_SEMANTICS.FX_RAIN;
            //Core._CoreClient.Atmosphere.Rain_Enable(true);
            //Core._CoreClient.Atmosphere.Rain_GetParticleSystem();
            //Core._CoreClient.Atmosphere.Rain_Init(_density, _rainTexture, _fallSpeed, _weather.WindDirection.x, _weather.WindDirection.y,
            //                                _randomness, _distance, _particleSize, _radius, _generatorSpeed, _speed);


            pSystem = CoreClient._CoreClient.Scene.CreateParticleSystem("snow");
            iTex =
                CoreClient._CoreClient.TextureFactory.LoadTexture("particles\\particle.dds", "snowflake", 512, -1,
                                                      CONST_TV_COLORKEY.TV_COLORKEY_BLACK,
                                                      true);


            iEmt = pSystem.CreateEmitter(CONST_TV_EMITTERTYPE.TV_EMITTER_BILLBOARD, MAX_BILLBOARD_PARTICLES);


            pSystem.SetEmitterDirection(iEmt, true, new TV_3DVECTOR(0, -1, 0), new TV_3DVECTOR(-1, -1, -1));

            pSystem.SetEmitterPower(iEmt, 30, 6);
            pSystem.SetEmitterSpeed(iEmt, 1); // Lower# = faster!

            pSystem.SetEmitterShape(iEmt, CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_SPHEREVOLUME);
            pSystem.SetEmitterSphereRadius(iEmt, 80); //70
            //pSystem.SetEmitterShape(iEmt, CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_BOXSURFACE);
            // pSystem.SetEmitterBoxSize(iEmt, new Vector3d(2560, 10, 2560));

            pSystem.SetParticleDefaultColor(iEmt, new TV_COLOR(1, 1, 1, 1));

            pSystem.SetPointSprite(iEmt, iTex); //, 32

            // optional since our direction is already -1?
            pSystem.SetEmitterGravity(iEmt, true, new TV_3DVECTOR(0, -500, 0));

            pSystem.SetEmitterLooping(iEmt, true);
        }

        public override void Update(double elapsedSeconds, RenderingContext context)
        {
            pSystem.Update();
            // Relocate SNOW emittor above player!
            //pSystem.SetEmitterPosition(iEmt, new Vector3d(camera.Translation.x, camera.Translation.y + 100,
            //                                           camera.Translation.z));
            //pSystem.SetEmitterPosition(iEmt, new Vector3d(0, 0, 0));
            pSystem.SetGlobalPosition((float) context.Position.x, (float) context.Position.y, (float) context.Position.z);
        }

        //public override void PreRender(ITraverser traverser)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Rain and Snow should probably be last, even after GLOW 
        /// </summary>
        /// <param name="camera"></param>
        public override void Render(RenderingContext context)
        {
            pSystem.Render();
        }

        //public override void PostRender(ITraverser traverser)
        //{
        //    throw new NotImplementedException();
        //}
    }
}