using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Keystone.Appearance;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.Resource;
using Keystone.Shaders;
using Keystone.Types;
using MTV3D65;
using LibNoise.Modifiers;
using System.Drawing;
using Keystone.FX;
using Keystone.Portals;
using KeyCommon.Flags;

namespace Keystone.Celestial
{
    public class ProceduralHelper
    {
        // when using unit radius 1, we must also scale by the stellar objects RADIUS _NOT_ diameter
        private const float UNIT_SPHERE_RADIUS = 1f;
        private const int SLICES = 150;
        private const int STACKS = 150;
        public const float DAYS_TO_SECONDS = 86400f;

        public static Keystone.Celestial.Star CreateStar(Vector3d position)
        {
            Keystone.Celestial.Star star = new Keystone.Celestial.Star(Repository.GetNewName(typeof(Keystone.Celestial.Star)));
            star.Translation = position;
            //star.LatestStepTranslation = position; // not necessary if entity flag "dyanmic" == false
            star.Diameter = 1392000000; // sun diameter in meters
            star.MassKg = 1.9891E30; // required for gravity
            Keystone.Celestial.ProceduralHelper.InitStarVisuals(star);
            return star;
        }

        public static StellarSystem GenerateSolSystem(Vector3d pos, int seed)
        {


            // SEMI-MAJOR AXIS
            const float MERCURY_ORBITAL_RADIUS = 57909100000f;
            const float VENUS_ORBITAL_RADIUS = 108208930000f;
            const float EARTH_ORBITAL_RADIUS = 149598261000f;
            const float MARS_ORBITAL_RADIUS = 227939100000f;
            const float JUPITER_ORBITAL_RADIUS = 778547200000f;
            const float SATURN_ORBITAL_RADIUS = 1433449370000f;
            const float URANUS_ORBITAL_RADIUS = 2876679082000;
            const float NEPTUNE_ORBITAL_RADIUS = 4503443661000f;
            const float PLUTO_ORBITAL_RADIUS = 5874000000000f;
            const float ERIS_ORBITAL_RADIUS = 10123289351640f;// 67.67 AU  
            // Options for scaling the models
            // - apply a scale during traversal to the model just as we apply a scale to the Region during traversal
            //      -   to avoid constant recomputing of bounding box, we should apply the scale to the model's that are loaded
            //          and the ones that are subsequently loaded when in "nav" mode.
            // - we use proxy models (using LOD style switching) for rendering scaled up models, billboards, icons. (icons which 
            //        can have a fixed screenspace size)
            // - what about an LOD switch or a NAV Switch that applies a scaling?  A scaling node of sorts... the question then would be
            // how do you get that scaling node to act more like a permanent toggle so that it doesnt have to scale every frame...
            // thats the problem with Transform nodes in general.  
            // Our current system of scaling the Region allows us to avoid touching the actual entities, but the only way to scale
            // distance down and model's up is to scale Region for distance, and then entity or models for the worlds and ships.
            // - I can render them as Imposters and then scale the imposters up instead!  This way when i render them to a
            // rendersurface, I can render them to 256x256 or whatever and then blit them at whatever actual scale i want.
            // So when in "navigation mode" stars, worlds, ships,s tations, etc will get rendered with the imposter system and the
            // imposter system will have a scale on it.
            //      - BUT HOW DO I HANDLE PICKING THEN?  HUD-ified icons would need a seperate 2d gui handler

            string id = Keystone.Resource.Repository.GetNewName(typeof(StellarSystem));
            StellarSystem solSystem = new StellarSystem(id);
            solSystem.Name = "Sol System";

            // sol
            Keystone.Celestial.Star star = CreateStar(pos);
            star.Name = "Sol";
            solSystem.AddChild(star);

            const int J2000 = 2451545;
            Random r = new Random(seed);

            // MERCURY
            Keystone.Celestial.World world = new Keystone.Celestial.World(Repository.GetNewName(typeof(Keystone.Celestial.World)));
            world.Translation = GetRandomWorldPosition(r, new Vector3d(), MERCURY_ORBITAL_RADIUS); // new Vector3d(0, 0, MERCURY_ORBITAL_RADIUS);
            //world.LatestStepTranslation = world.Translation; // not necessary if entity flag "dyanmic" == false
            world.Diameter = 488; // 0000;
            world.MassKg = 3.3022E23; // required for gravity
            // TODO: I should rename .OrbitalRadius to SemiMajorAxis
            world.OrbitalRadius = MERCURY_ORBITAL_RADIUS;
            world.OrbitalEccentricity = 0.205630f; // Mercury orbital period seconds (87.9691 days)
            world.OrbitalPeriod = 87.9691f * DAYS_TO_SECONDS;
            world.OrbitalEpoch = (long)(DateTime.Today - Keystone.Utilities.TimeDateHelper.FromJulian(J2000)).TotalSeconds;
            world.Name = "Mercury";
            Keystone.Celestial.ProceduralHelper.InitWorldVisuals(star, world, true, false, false, false);


            //return; // TEMP HACK DEBUG CULLING ISSUE WITH JUST MERCURY AND SOL

            // VENUS
            world = new Keystone.Celestial.World(Repository.GetNewName(typeof(Keystone.Celestial.World)));
            world.Translation = GetRandomWorldPosition(r, new Vector3d(), VENUS_ORBITAL_RADIUS); // new Vector3d(0, 0, VENUS_ORBITAL_RADIUS);
            //world.LatestStepTranslation = world.Translation; // not necessary if entity flag "dyanmic" == false
            world.Diameter = 12103600;
            world.MassKg = 4.8685E24; // required for gravity
            world.OrbitalRadius = VENUS_ORBITAL_RADIUS;
            world.OrbitalEccentricity = 0.0068f; // Venus orbital period seconds (224.70069 days)
            world.OrbitalPeriod = 224.70069f * DAYS_TO_SECONDS;
            world.OrbitalEpoch = (long)(DateTime.Today - Keystone.Utilities.TimeDateHelper.FromJulian(J2000)).TotalSeconds;
            world.Name = "Venus";
            Keystone.Celestial.ProceduralHelper.InitWorldVisuals(star, world, true, false, false, false);

            // EARTH
            world = new Keystone.Celestial.World(Repository.GetNewName(typeof(Keystone.Celestial.World)));
            world.Translation = GetRandomWorldPosition(r, new Vector3d(), EARTH_ORBITAL_RADIUS); //new Vector3d(0, 0, EARTH_ORBITAL_RADIUS);
            //world.LatestStepTranslation = world.Translation; // not necessary if entity flag "dyanmic" == false
            world.Diameter = 12756300;
            world.MassKg = 5.9736E24; // required for gravity
            world.OrbitalRadius = EARTH_ORBITAL_RADIUS;
            world.OrbitalEccentricity = 0.01671123f; // Earth orbital period seconds (365.256363004 days)
            world.OrbitalPeriod = 365.256363004f * DAYS_TO_SECONDS;
            world.OrbitalEpoch = (long)(DateTime.Today - Keystone.Utilities.TimeDateHelper.FromJulian(J2000)).TotalSeconds;
            world.AxialRotationRate = 30;// 30 seconds
            world.AxialTilt = 23.5f;
            world.Name = "Earth";
            Keystone.Celestial.ProceduralHelper.InitWorldVisuals(star, world, true, false, true, false);

            Keystone.Celestial.World luna = CreateEarthMoon(r, world);

            // MARS
            world = new Keystone.Celestial.World(Repository.GetNewName(typeof(Keystone.Celestial.World)));
            world.Translation = GetRandomWorldPosition(r, new Vector3d(), MARS_ORBITAL_RADIUS); //new Vector3d(0, 0, MARS_ORBITAL_RADIUS);
            //world.LatestStepTranslation = world.Translation; // not necessary if entity flag "dyanmic" == false
            world.Diameter = 6794000;
            world.MassKg = 6.4185E23; // required for gravity
            world.OrbitalRadius = MARS_ORBITAL_RADIUS;
            world.OrbitalEccentricity = 0.093315f; // Mars orbital period seconds (686.971 days)
            world.OrbitalPeriod = 686.971f * DAYS_TO_SECONDS;
            world.OrbitalEpoch = (long)(DateTime.Today - Keystone.Utilities.TimeDateHelper.FromJulian(J2000)).TotalSeconds;
            world.Name = "Mars";
            Keystone.Celestial.ProceduralHelper.InitWorldVisuals(star, world, true, false, true, false);

            // asteroid belt 1000 km across

            // JUPITER
            world = new Keystone.Celestial.World(Repository.GetNewName(typeof(Keystone.Celestial.World)));
            world.Translation = GetRandomWorldPosition(r, new Vector3d(), JUPITER_ORBITAL_RADIUS); //new Vector3d(0, 0, JUPITER_ORBITAL_RADIUS);
            //world.LatestStepTranslation = world.Translation; // not necessary if entity flag "dyanmic" == false
            world.Diameter = 142844; // 142984000;
            world.MassKg = 1.8986E27; // required for gravity
            world.OrbitalRadius = 1000000000; // JUPITER_ORBITAL_RADIUS;
            world.OrbitalEccentricity = 0.048775f; // Jupiter orbital period seconds (4332.59 days)
            world.OrbitalPeriod = 4332.59f * DAYS_TO_SECONDS;
            world.OrbitalEpoch = (long)(DateTime.Today - Keystone.Utilities.TimeDateHelper.FromJulian(J2000)).TotalSeconds;
            world.Name = "Jupiter";
            Keystone.Celestial.ProceduralHelper.InitWorldVisuals(star, world, false, true, false, false);


            Keystone.Celestial.World[] jupiterMoons = CreateJupiterMoons(r, world);


            // SATURN
            world = new Keystone.Celestial.World(Repository.GetNewName(typeof(Keystone.Celestial.World)));
            world.Translation = GetRandomWorldPosition(r, new Vector3d(), SATURN_ORBITAL_RADIUS); //new Vector3d(0, 0, SATURN_ORBITAL_RADIUS);
            //world.LatestStepTranslation = world.Translation; // not necessary if entity flag "dyanmic" == false
            world.Diameter = 120536000;
            world.MassKg = 5.6846E26; // required for gravity
            world.OrbitalRadius = SATURN_ORBITAL_RADIUS;
            world.OrbitalEccentricity = 0.055723219f; // Saturn orbital period seconds (10759.22 days)
            world.OrbitalPeriod = 10759.22f * DAYS_TO_SECONDS;
            world.OrbitalEpoch = (long)(DateTime.Today - Keystone.Utilities.TimeDateHelper.FromJulian(J2000)).TotalSeconds;
            world.Name = "Saturn";
            Keystone.Celestial.ProceduralHelper.InitWorldVisuals(star, world, false, true, false, false);

            // URANUS
            world = new Keystone.Celestial.World(Repository.GetNewName(typeof(Keystone.Celestial.World)));
            world.Translation = GetRandomWorldPosition(r, new Vector3d(), URANUS_ORBITAL_RADIUS); //new Vector3d(0, 0, URANUS_ORBITAL_RADIUS);
            //world.LatestStepTranslation = world.Translation; // not necessary if entity flag "dyanmic" == false
            world.Diameter = 51118000;
            world.MassKg = 8.6810E25;  // required for gravity
            world.OrbitalRadius = URANUS_ORBITAL_RADIUS;
            world.OrbitalEccentricity = 0.044405586f; // Uranus orbital period seconds (30799.095 days)
            world.OrbitalPeriod = 30799.095f * DAYS_TO_SECONDS;
            world.OrbitalEpoch = (long)(DateTime.Today - Keystone.Utilities.TimeDateHelper.FromJulian(J2000)).TotalSeconds;
            world.Name = "Uranus";
            Keystone.Celestial.ProceduralHelper.InitWorldVisuals(star, world, false, true, false, false);

            // NEPTUNE
            world = new Keystone.Celestial.World(Repository.GetNewName(typeof(Keystone.Celestial.World)));
            world.Translation = GetRandomWorldPosition(r, new Vector3d(), NEPTUNE_ORBITAL_RADIUS); //new Vector3d(0, 0, NEPTUNE_ORBITAL_RADIUS);
            //world.LatestStepTranslation = world.Translation; // not necessary if entity flag "dyanmic" == false
            world.Diameter = 49532000;
            world.MassKg = 1.0243E26; // required for gravity
            world.OrbitalRadius = NEPTUNE_ORBITAL_RADIUS;
            world.OrbitalEccentricity = 0.011214269f; // Neptune orbital period seconds (60190.03 days)
            world.OrbitalPeriod = 60190.03f * DAYS_TO_SECONDS;
            world.OrbitalEpoch = (long)(DateTime.Today - Keystone.Utilities.TimeDateHelper.FromJulian(J2000)).TotalSeconds;
            world.Name = "Neptune";
            Keystone.Celestial.ProceduralHelper.InitWorldVisuals(star, world, false, true, false, false);

            // PLUTO
            world = new Keystone.Celestial.World(Repository.GetNewName(typeof(Keystone.Celestial.World)));
            world.Translation = GetRandomWorldPosition(r, new Vector3d(), PLUTO_ORBITAL_RADIUS); // new Vector3d(0, 0, PLUTO_ORBITAL_RADIUS);
            //world.LatestStepTranslation = world.Translation; // not necessary if entity flag "dyanmic" == false
            world.Diameter = 2274000;
            world.MassKg = 1.305E22; // required for gravity
            world.OrbitalRadius = PLUTO_ORBITAL_RADIUS;
            world.OrbitalEccentricity = 0.24880766f; // Pluto orbital period seconds (90613.305 days)
            world.OrbitalPeriod = 90613.305f * DAYS_TO_SECONDS;
            world.OrbitalInclination = (float)(11.88f * Keystone.Utilities.MathHelper.DEGREES_TO_RADIANS);
            world.OrbitalEpoch = (long)(DateTime.Today - Keystone.Utilities.TimeDateHelper.FromJulian(J2000)).TotalSeconds;
            world.Name = "Pluto";
            Keystone.Celestial.ProceduralHelper.InitWorldVisuals(star, world, true, false, false, false);

            // ERIS
            world = new Keystone.Celestial.World(Repository.GetNewName(typeof(Keystone.Celestial.World)));
            world.Translation = GetRandomWorldPosition(r, new Vector3d(), ERIS_ORBITAL_RADIUS); // new Vector3d(0, 0, ERIS_ORBITAL_RADIUS);
            //world.LatestStepTranslation = world.Translation; // not necessary if entity flag "dyanmic" == false
            world.Diameter = 2326000;
            world.MassKg = 1.67E22; // required for gravity
            world.OrbitalRadius = ERIS_ORBITAL_RADIUS;
            world.OrbitalEccentricity = 0.44177f; // Eris orbital period seconds (90613.305 days)
            world.OrbitalPeriod = 203600f * DAYS_TO_SECONDS;
            world.OrbitalInclination = (float)(44.187f * Keystone.Utilities.MathHelper.DEGREES_TO_RADIANS);
            world.OrbitalEpoch = (long)(DateTime.Today - Keystone.Utilities.TimeDateHelper.FromJulian(2453800)).TotalSeconds;
            world.Name = "Eris";
            Keystone.Celestial.ProceduralHelper.InitWorldVisuals(star, world, true, false, false, false);


            // kupier belt  (pluto is a kupier belt object at the inner edge of the kupier belt
            // appears to be huge radius like all the way out to 500 au+

            // oort cloud 30 trillion km from Sun

            return solSystem;
        }

        private static Vector3d GetRandomWorldPosition (Random r, Vector3d origin, double orbitalRadius) 
        {
            Vector3d result;        
            float angleDegrees = (float)(r.NextDouble() * 360d);
            result =  Keystone.Helpers.TVTypeConverter.FromTVVector(CoreClient._CoreClient.Maths.MoveAroundPoint(new TV_3DVECTOR(), (float)orbitalRadius, angleDegrees, 0f));
            return result;
        }

        private static World CreateEarthMoon(Random r, World planet)
        {
            const float LUNA_ORBITAL_RADIUS = 384403000.0f;
            
            Keystone.Celestial.World moon = new Keystone.Celestial.World(Repository.GetNewName(typeof(Keystone.Celestial.World)));
            moon.Translation = GetRandomWorldPosition(r, new Vector3d(), LUNA_ORBITAL_RADIUS); 
            //moon.LatestStepTranslation = moon.Translation; // not necessary if entity flag "dyanmic" == false
            moon.Diameter = 3476000;
            moon.MassKg = 7.349e22; // required for gravity // todo: i think Luna's mass is wrong. i never updated it
            moon.OrbitalRadius = LUNA_ORBITAL_RADIUS;
            moon.OrbitalEccentricity = 0.0549f;
            moon.OrbitalPeriod = 27.3f * DAYS_TO_SECONDS;
            //moon.OrbitalPeriod = 30f; // temp HACK to make orbit last  1/2 minute. Instead we should have timestep multiplier for physics and animations
            moon.Name = "Moon";
            Keystone.Celestial.ProceduralHelper.InitWorldVisuals(planet, moon, true, false, false, false);

            return moon;
        }

        private static Keystone.Celestial.World[] CreateJupiterMoons(Random r, Keystone.Celestial.World planet)
        {
            const float IO_ORBITAL_RADIUS = 421700000.0f;
            const float EUROPA_ORBITAL_RADIUS = 671034000.0f;
            const float GANYMEDE_ORBITAL_RADIUS = 1070412000.0f;
            const float CALLISTO_ORBITAL_RADIUS = 1882709000.0f;


            Keystone.Celestial.World[] moons = new Keystone.Celestial.World[4];

            moons[0] = new Keystone.Celestial.World(Repository.GetNewName(typeof(Keystone.Celestial.World)));
            moons[0].Translation = GetRandomWorldPosition(r, new Vector3d(), IO_ORBITAL_RADIUS); // new Vector3d(0, 0, IO_ORBITAL_RADIUS);
            //moons[0].LatestStepTranslation = moon.Translation; // not necessary if entity flag "dyanmic" == false
            moons[0].Diameter = 3666000;
            moons[0].MassKg = 8.9319E22; // required for gravity
            moons[0].OrbitalRadius = IO_ORBITAL_RADIUS;
            //moons[0].OrbitalEccentricity = 0.0041f; // Io orbital period seconds (87.9691 days)
            moons[0].OrbitalEccentricity = 0.5f;
            moons[0].OrbitalPeriod = 1.769f * DAYS_TO_SECONDS;
            //moons[0].OrbitalPeriod = 30f; // temp HACK to make orbit last  1/2 minute. Instead we should have timestep multiplier for physics and animations
            moons[0].Name = "Io";
            Keystone.Celestial.ProceduralHelper.InitWorldVisuals(planet, moons[0], true, false, false, false);


            moons[1] = new Keystone.Celestial.World(Repository.GetNewName(typeof(Keystone.Celestial.World)));
            moons[1].Translation = GetRandomWorldPosition(r, new Vector3d(), EUROPA_ORBITAL_RADIUS); //new Vector3d(0, 0, EUROPA_ORBITAL_RADIUS);
            //moons[1].LatestStepTranslation = moon.Translation; // not necessary if entity flag "dyanmic" == false
            moons[1].Diameter = 3121600;
            moons[1].MassKg = 4.80E22; // required for gravity
            moons[1].Name = "Europa";
            moons[1].OrbitalRadius = EUROPA_ORBITAL_RADIUS;
            moons[1].OrbitalEccentricity = 0.0094f; // Europa orbital period seconds (87.9691 days)
            moons[1].OrbitalPeriod = 3.551f * DAYS_TO_SECONDS;
            Keystone.Celestial.ProceduralHelper.InitWorldVisuals(planet, moons[1], true, false, false, false);


            moons[2] = new Keystone.Celestial.World(Repository.GetNewName(typeof(Keystone.Celestial.World)));
            moons[2].Translation = GetRandomWorldPosition(r, new Vector3d(), GANYMEDE_ORBITAL_RADIUS); // new Vector3d(0, 0, GANYMEDE_ORBITAL_RADIUS);
            //moons[2].LatestStepTranslation = moon.Translation; // not necessary if entity flag "dyanmic" == false
            moons[2].Diameter = 5262400;
            moons[2].MassKg = 1.4819E23;  // required for gravity
            moons[2].OrbitalRadius = GANYMEDE_ORBITAL_RADIUS;
            moons[2].OrbitalEccentricity = 0.0011f; // Ganymede orbital period seconds (87.9691 days)
            moons[2].OrbitalPeriod = 7.155f * DAYS_TO_SECONDS;
            moons[2].Name = "Ganymede";
            Keystone.Celestial.ProceduralHelper.InitWorldVisuals(planet, moons[2], true, false, false, false);


            moons[3] = new Keystone.Celestial.World(Repository.GetNewName(typeof(Keystone.Celestial.World)));
            moons[3].Translation = GetRandomWorldPosition(r, new Vector3d(), CALLISTO_ORBITAL_RADIUS);// new Vector3d(0, 0, CALLISTO_ORBITAL_RADIUS);
            //moons[3].LatestStepTranslation = moon.Translation; // not necessary if entity flag "dyanmic" == false
            moons[3].Diameter = 4820600;
            moons[3].MassKg = 1.075938E23;  // required for gravity
            moons[3].OrbitalRadius = CALLISTO_ORBITAL_RADIUS;
            moons[3].OrbitalEccentricity = 0.0074f; // Callisto orbital period seconds (87.9691 days)
            moons[3].OrbitalPeriod = 16.69f * DAYS_TO_SECONDS;
            moons[3].Name = "Callisto";
            Keystone.Celestial.ProceduralHelper.InitWorldVisuals(planet, moons[3], true, false, false, false);

            return moons;
        }

        //private void LoadSkybox()
        //{
        //    string[] textures = new string[6];
        //    textures[0] = _core.DataPath + "pool\\textures\\sky_skylab_01ft.tga"; // "pool\\textures\\Galaxy_BK.bmp";
        //    textures[1] = _core.DataPath + "pool\\textures\\sky_skylab_01bk.tga"; // "pool\\textures\\Galaxy_FT.bmp";
        //    textures[2] = _core.DataPath + "pool\\textures\\sky_skylab_01lf.tga"; // "pool\\textures\\Galaxy_LT.bmp";
        //    textures[3] = _core.DataPath + "pool\\textures\\sky_skylab_01rt.tga"; // "pool\\textures\\Galaxy_RT.bmp";
        //    textures[4] = _core.DataPath + "pool\\textures\\sky_skylab_01up.tga"; // "pool\\textures\\Galaxy_DN.bmp";
        //    textures[5] = _core.DataPath + "pool\\textures\\sky_skylab_01dn.tga"; // "pool\\textures\\Galaxy_UP.bmp";

        //    float radius = 10000;
        //    Keystone.Commands.ICommand addSkybox = new AddSkyBoxTV(textures, radius, null);
        //    QueueCommand(addSkybox);

        //// TODO: this sky dome starfield... wonder why he didnt just use tv3d's skysphere
        //// eg. Core.Atmosphere.SkySphere_Enable(true);plus setscale, texture, etc
        ////TVMesh stars = Core._Core.SceneManager.CreateMeshBuilder("Stars");
        ////stars.LoadTVM("SkySphere.TVM", false, false);
        ////stars.SetTexture(_globals.GetTex("Stars"));
        ////stars.SetScale(earthRadius, 100f, 100f);
        ////stars.SetCollisionEnable(false); 


        //}


        // how do we make a digest as a generic object and not one that is specific to
        // celestial body entities across zones?
        //	- all we really need is a way to know when to disable/enable a digest record so
        //	  that the live entity can take over
        //	- imagine a scenario where a digest is used for flocking far away items
        //  - imagine a scenario where a digest is used to model in an efficient way
        //	  and function as a simulation level of detail where digest can be more abstract
        //	- so it all boils down to how to know when to disable/enable?
        //		- does a digested entity know it's digested?  this is easiest way
        //		- do we use subscription \ publisher model?
        //	- how do we know when a particular entity should subscribe to a digest when it's loaded?
        //	  	- if it's by just adding a FX type\channel subscription it's easy, but honestly i dont really like that model
        //		- during scene load, we need to restore these channels/services/publishers before anything else.
        //        - we can create a dictionary of these services perhaps in the SceneInfo and use unique GUIDs and typenames
        //			and treat them as entitymanager entities.
        //		  - isn't Physics very much like this then?  Physics entities subscribe to physicalSimulation which uses publishser/subscriber model
        //			and during deserialization, if this entity has guid of manager in list, we subscribe it to Scene.EntitySystem
        //  class Digest : EntityPublisher : Channel : Service : EntitySystem
        //	{
        //	}


        public static void InitStarVisuals(Star star)
        {
            Types.Color diffuseColor, ambient, specular, emissive;
            diffuseColor = Keystone.Types.Color.White; ; // we dont want solely diffuseColor
            // because a billboard will rotate to face camera and that can affect it's color shading.
            // we want fullbright star billboards, not shaded
            //diffuseColor = Keystone.Types.Color.White; //  Keystone.Types.Color.Random();
            ambient = Keystone.Types.Color.White;
            specular = Keystone.Types.Color.Black; // if this does not use Black (0,0,0, 255) our specular ruins the alpha of the corona billboards
            emissive = Keystone.Types.Color.White; //TODO: get star color from temperature;
            // TODO: create a shared material here using a name based on Material.GetDefaultName ();
            Material emissiveMaterial = Material.Create(Repository.GetNewName(typeof(Material)),
                										diffuseColor, ambient, specular, emissive);
            // TODO: we should generate the color and range from the passed star stats
            // add a light as a child of the star
            // TODO: TV doesn't like multiple directional lights. Only DirLight0 get's passed into semantic.
            // You must pass the directional light and color manually via TVShader.SetEffectParamVector3 and SetEffectParamColor
            string id = Repository.GetNewName(typeof(Lights.DirectionalLight));
            Lights.Light light = new Lights.DirectionalLight(id);
            ((Lights.DirectionalLight)light).IsBillboard = true;
            // March.6.2017 - Range no longer needed. Switched Light from PointLight to DirectionalLight with Billboarding
            // March.7.2017 - Range _IS_ needed for determination of whether light source intersects Entity 
            light.Range = 74799000000000f / 2f;
            light.Diffuse = diffuseColor;
            light.InheritRotation = false;
            star.AddChild(light);


           // // osiris real time star rendering
           // // http://www.youtube.com/watch?v=AD5BQJe8T_I
           // // http://www.youtube.com/watch?v=eWtug51MQO8
           // //
           // // http://www.entheosweb.com/photoshop/shiny_starburst_effect.asp

           string starPath = @"caesar\shaders\Planet\brighterstartextures\medres\mstar.png";
            
           // // The problem with my star textures is the transparency info should be in the alpha channel
           // // and the rgb channels should be only contain the actual halo and NOT any back color.
           // // Thus we have a situation where we're grabbing the black background color and rendering it.
            starPath = @"caesar\shaders\Planet\sinstar_surface_green.png";
           
           // //starPath = Core._Core.DataPath + @"\pool\Shaders\Planet\sun.dds"; //flare.jpg";    
           // starPath = @"E:\dev\_projects\_TV\Zak_HLSLSkyDemo\HLSLSkyDemo\Media\Textures\Stars\sun.dds";
           // //starPath = Core._Core.DataPath + @"\pool\shaders\Planet\sinstar_halo.png";
           // //starPath = Core._Core.DataPath + @"\pool\shaders\Planet\sinstar_halo_alpha5.png";
           // //starPath = Core._Core.DataPath + @"\pool\shaders\Planet\starcorona.dds";
           // //starPath = Core._Core.DataPath + @"\pool\Shaders\Planet\corona_yellow.jpg";
           //// starPath = Core._Core.DataPath + @"\pool\shaders\Planet\SunGlow.dds";
           // // NOTE: loading a texture like a star texture that uses an alpha channel you MUST
           // // load the texture with colorkey even if that key is not correctly specified.  It must be done
           // // to inform tv to not ignore the alpha channel in the texture.

            double radius = star.Radius;
            // OBSOLETE - we should typically set scales on Models not Entities unless we intend for all child entities
            //            such as planets and asteroids to adopt the scale of the parent
			// star.Scale = new Vector3d(radius, radius, radius);
            star.InheritRotation = false;
            star.InheritScale = true;

//            appearance = CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL, null, starPath, null, null, null);
//            appearance.RemoveMaterial();
//            appearance.AddChild(emissiveMaterial);
//
//            Model starModel = new Model(Repository.GetNewName(typeof(Model)));
//            Mesh3d mesh = Mesh3d.CreateSphere(UNIT_SPHERE_RADIUS, SLICES, STACKS, CONST_TV_MESHFORMAT.TV_MESHFORMAT_TEX1, true);
//            starModel.AddChild(mesh);
//            starModel.AddChild(appearance);

        
            // try dividing by the texture dimensions to see if this scales the corna to the proper size
// BEGIN TEMP: Lets not add any sphere model, just corona only
            ModelSelector selector = new ModelSequence(Repository.GetNewName(typeof(ModelSequence)));
            star.AddChild (selector);
            selector.Scale = new Vector3d (radius, radius, radius);
            selector.InheritScale = true;

            // TODO: should coronaModel scale be 1.5x the star radius to reflect the fact that
            // a corona represents the part of a star that extends beyond the surface?
            // TODO: move the .InheritScale to CreateCorona along with above note
            // but also note how here we have a CoronaModel with .InheritScale whereas clouds, rings, atmosphere
            // do NOT inheritscale.  So why here?  It makes it difficult to recurse through a stellarsystem
            // to scale down everything.
            string billboardTexturePath = @"caesar\Shaders\Planet\SunGlow.dds";
            billboardTexturePath = @"caesar\textures\SunWhite.dds";
            
            // NOTE: Fog Enable = true will cause individual star billboards to appear white
            Model coronaModel = CreateBillboardModel(CONST_TV_BILLBOARDTYPE.TV_BILLBOARD_NOROTATION, billboardTexturePath, "tvdefault", emissiveMaterial, CONST_TV_BLENDINGMODE.TV_BLEND_ADD, false);
            coronaModel.InheritScale = true;
            coronaModel.Name = "corona1"; //ok to recycle friendly names since only need be unique per entity

            selector.AddChild (coronaModel);


            // add animation for rotating both coronas in opposite direction
            string clip1_name = Repository.GetNewName(typeof(Animation.KeyframeInterpolator<Quaternion>));
            // TODO: must these AnimationClips always be unique?  The main thing that makes them
            //       unable to be used uniquely is the "target" that is specified and is used to wire up
            //       the animation with the Entity or Model that it's animating.  
            Animation.KeyframeInterpolator<Quaternion> clip = MakeAnimation(clip1_name, coronaModel.Name);
            Animation.KeyframeInterpolator<Quaternion> clip2 = null;
            
            Quaternion q1, q2, q3;
            // yaw, pitch, roll quaternion instantiation
            q1 = new Quaternion(0, 0, Utilities.MathHelper.DEGREES_TO_RADIANS * 0);
            q2 = new Quaternion(0, 0, Utilities.MathHelper.DEGREES_TO_RADIANS * 179);
            q3 = new Quaternion(0, 0, Utilities.MathHelper.DEGREES_TO_RADIANS * 359.99);
            clip.AddKeyFrame(q1);
            clip.AddKeyFrame(q2);
            clip.AddKeyFrame(q3);
            clip.Duration = 60f; //seconds, 
            //anim.Speed =   // length in seconds is effectively the speed right unless we want to slow it down or speed it up?

          

            bool doubleCorona = true;
            if (doubleCorona)
            {
            	// NOTE: Fog Enable = true will cause individual star billboards to appear white
                coronaModel = CreateBillboardModel(CONST_TV_BILLBOARDTYPE.TV_BILLBOARD_NOROTATION , billboardTexturePath, "tvdefault", emissiveMaterial, CONST_TV_BLENDINGMODE.TV_BLEND_ADD, false);
                coronaModel.InheritScale = true;
                coronaModel.Name = "corona2"; //ok to recycle friendly names since only need be unique per entity

                selector.AddChild(coronaModel);  // testing add of 2nd corona model

                // add animation for rotating both coronas in opposite directions
                string clip2_name = Repository.GetNewName(typeof(Animation.KeyframeInterpolator<Quaternion>));
                clip2 = MakeAnimation(clip2_name, coronaModel.Name);
                // add the keyframes in reverse order
                clip2.AddKeyFrame(q3);
                clip2.AddKeyFrame(q2);
                clip2.AddKeyFrame(q1);
                clip2.Duration = 60f; //seconds

                // obsolete - now a single Animation.cs stores both clips 
                //string animation2_name = Repository.GetNewName(typeof(Animation.KeyframeInterpolator<Quaternion>));
                //star.Animations.Play(animation2_name, true);
            }


//            selector.AddChild (starModel); 

            id = Repository.GetNewName(typeof(Keystone.Animation.Animation));

            Animation.Animation anim = new Keystone.Animation.Animation(id);
            anim.Looping = true;
            string animationName = star.ID + "_corona_animation"; // named so we can access from script
            anim.Name = animationName;
            //anim.Speed = 1f;
            
            anim.AddChild(clip);
            if (clip2 != null)
                anim.AddChild(clip2);
            
            star.AddChild(anim);
            
            
            // TODO: I should not have to .Play them now.  It should play
            // when deserialized.
            star.Animations.Play(animationName, true);
// ELSE TEMP
            //star.AddChild(starModel);
//              star.AddChild(coronaModel);
// END TEMP:


            // Obsolete? Instead of CreateFlare() we now use OnRender() VisualFX.DrawTexturedQuad
            // MUST SET DOMAINOBJECT DESIGN VARIABLE "hexflare texture"
            //ModeledEntity flare = CreateFlare();          
        	string scriptPath = @"caesar\scripts_entities\star.css";
            MakeDomainObject(star, scriptPath);
        }

        public static void MakeDomainObject(Entity parent, string scriptPath)
        {
            if (string.IsNullOrEmpty(scriptPath)) return;
//
//            DomainObjects.DomainObject domainObj = (DomainObjects.DomainObject)Keystone.Resource.Repository.Create (scriptPath, "DomainObject");
//            // TODO: verify this .LoadTVResource() is just not necessary. 
//			//       in fact, we only need to assign the scriptpath and not even create the domainObj itself.  
//			//       TODO: unfortunately this is not so... .LoadTVResource is necessary still... we need to really clean this up
//			//       and figure out sln that works for all use cases .  one thing we need to answer is if entity.ResourcePath has script assigned
//			//       and we xml deserialize that entity and AddChild() it to some parent, the script will not have loaded yet so when do
//			//       we assign production?  well in LoadTVResource() then right?  and then remove productions when .Dispose()		
//			// 		TODO: further, the entity should not be considered "active" until any script assigned is loaded (what about child entities? and their scripts?)		
//			//       - when node is added to parent, incrementref occurs and that triggers QueuePageableResource() but what if we want to turn pager off
//			//      such as when generating a galaxy we dont intend to render? ideally if i could ensure the pager will only skip paging for that scene
//			if (domainObj.PageStatus == PageableNodeStatus.NotLoaded) 
//			{
//				// WARNING: During multi-zone stellar system generation, as each Zone is completed and paged out
//				//          so will any DomainObjects!  This means they end up having to be reloaded.  The trick would be
//				//          to cache these scripts until all generation is complete so that they never have to be loaded.
//				//          The other trick is to not load these scripts during generation since we only intend to generate the XML.
//				// TODO: i suspect the same thing is happening with shaders, meshes and textures.  I have one thread trying to load
//				// and share, while another is unloading and disposing.
//				//domainObj.ResourceStatus = PageableNodeStatus.Loading;
//            	//domainObj.LoadTVResource();
//            	//if (domainObj.PageStatus != PageableNodeStatus.Error)
//				//	domainObj.PageStatus = PageableNodeStatus.Loaded;
//				Keystone.IO.PagerBase.LoadTVResourceSychronously (domainObj, false);
//			}
//           	else if (domainObj.PageStatus == PageableNodeStatus.Error)
//            	throw new Exception ("ProceduralHelper.MakeDomainObject() - Error in script.");
//            
//            Keystone.Traversers.SuperSetter setter = new Keystone.Traversers.SuperSetter(parent);
//            setter.Apply (domainObj);
        
            parent.ResourcePath  = scriptPath ;
        }

        private static Keystone.Animation.KeyframeInterpolator<Quaternion> MakeAnimation(string clipName, string targetName)
        {
            Keystone.Animation.KeyframeInterpolator<Quaternion> clip = new Keystone.Animation.KeyframeInterpolator<Quaternion>(clipName, "rotation");
            clip.Name = clipName;
            clip.TargetName = targetName; // note uses friendly name not .ID 
            //anim.Duration = 2f; // interpolaters have a rate of speed...hrm

            return clip;
        }
        
        public static Model CreateInstancedMeshModel (string geometryID, string texturePath, string shaderPath, Material emissiveMaterial)
        {
        	uint maxInstances = 1000;
		
            InstancedGeometry instancedGeometry = (InstancedGeometry)Repository.Create (geometryID, "InstancedGeometry");

            // TODO: this needs to load not a path, but a quad set of vertices
			instancedGeometry.SetProperty ("meshortexresource", typeof(string), instancedGeometry.ID);
		    instancedGeometry.SetProperty ("isbillboard", typeof(bool), false);
		    instancedGeometry.SetProperty ("maxcount", typeof(uint), maxInstances);
		    // precache is required to avoid random crashes.
//		    instancedGeometry.SetProperty ("precache", typeof(bool), true); // TODO: precache is for minimeshes only not InstancedGeometry?

    		// the MaxInstancesCount starts small, but can be increased withing TVMinimesh without losing existing element data
    		if (instancedGeometry.MaxInstancesCount < maxInstances)
    			instancedGeometry.MaxInstancesCount = maxInstances;
	    		
            Model model = new Model(Repository.GetNewName(typeof(Model)));
        	// billboards generally wont cast or receive shadows
			model.CastShadow = false;
        	model.ReceiveShadow = false;
        	
            Appearance.Appearance appearance = CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL, shaderPath, texturePath, null, null, null, true);
 
		
            Diffuse layer = (Diffuse)appearance.Layers[0];
            Keystone.IO.PagerBase.LoadTVResource (layer.Texture, false);

            
            // TODO: as a generic method, having a default material automatically sucks... 
            // what i dont want any material added? (eg not doing a Star material or any that requires default diffuse material)
            // use passed in material which is same as main star sphere
            //if (emissiveMaterial != null)
            //{
            //	appearance.RemoveMaterial();
            //	appearance.AddChild(emissiveMaterial);
            //}
            
            model.AddChild(appearance);
            model.AddChild(instancedGeometry);
            return model;
        }
        
        public static Model CreateInstancedBillboardModel (string geometryID, 
                                                           CONST_TV_BILLBOARDTYPE billboardType,
                                                           string billboardTexturePath, 
                                                           string shaderPath, 
                                                           Material emissiveMaterial, 
                                                           CONST_TV_BLENDINGMODE blendMode, 
                                                           bool alphaTest)
        {
        	uint maxInstances = 1000;
		
            InstancedBillboard instancedGeometry = (InstancedBillboard)Repository.Create (geometryID, "InstancedBillboard");

            // TODO: this needs to load not a path, but a quad set of vertices
			instancedGeometry.SetProperty ("meshortexresource", typeof(string), instancedGeometry.ID);
		    instancedGeometry.SetProperty ("maxcount", typeof(uint), maxInstances);
		    // precache is required to avoid random crashes.
//		    instancedGeometry.SetProperty ("precache", typeof(bool), true); // TODO: precache is for minimeshes only not InstancedGeometry?

    		// the MaxInstancesCount starts small, but can be increased withing TVMinimesh without losing existing element data
    		if (instancedGeometry.MaxInstancesCount < maxInstances)
    			instancedGeometry.MaxInstancesCount = maxInstances;
	    		
            Model model = new Model(Repository.GetNewName(typeof(Model)));
        	// billboards generally wont cast or receive shadows
			model.CastShadow = false;
        	model.ReceiveShadow = false;
        	
            Appearance.Appearance appearance = CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL, shaderPath, billboardTexturePath, null, null, null, true);
            // TV_BLEND_ALPHA - simple alpha blending of source with whatever already exists in the backbuffer using material opacity value
            // TV_BLEND_ADD   - adds pixels where alpha component in texture > 0.  This is used as preferred alternative to color keying
            // TV_BLEND_ADDALPHA - adds pixels where alpha component in texture > 0 and blends. Used often for particle textures like smoke
            
            // TV_BLEND_ADD is correct so that clouds when rendering after atmosphere (which hosts nighttime starmap)
            // will allow starmap to be visible through clouds.
            appearance.BlendingMode = blendMode;  
		
            Diffuse layer = (Diffuse)appearance.Layers[0];
            Keystone.IO.PagerBase.LoadTVResource (layer.Texture, false);

        	// NOTE: if there is z-fighting between mostly non-transparent quads, then depthwriteneable = true might help
        	// however, most of the time (eg like in particle systems) we would rather have z-fighting with good alpha blending
        	// between all layers, rather than top most z quad billboard to render and any billboard particles behind it to be ignored.
        	// That results in farther z billboards to just suddenly appear if they move out from under the top most billboards.
        	layer.AlphaTestDepthWriteEnable = false; // could prevent some zfighting
        	layer.AlphaTest = alphaTest;
        	// ignore pixels when the alpha in the texture <= the ref value
        	layer.AlphaTestRefValue = 1;
            
            // TODO: as a generic method, having a default material automatically sucks... 
            // what i dont want any material added? (eg not doing a Star material or any that requires default diffuse material)
            // use passed in material which is same as main star sphere
            //if (emissiveMaterial != null)
            //{
            //	appearance.RemoveMaterial();
            //	appearance.AddChild(emissiveMaterial);
            //}
            
            model.AddChild(appearance);
            model.AddChild(instancedGeometry);
            return model;
        }
        
        // TODO: this can be turned into a 
        // Create_Transparent_Billboard / Create_Flare / etc 
        // 
        // Sub entity parts should be 1:1 with main entity so corona should be attached to an entity
        // that is added to the main StarEntity which willc ontain the sphere model.  And this way during traversal
        // it's easier to sort alphas and solids.
        public static Model CreateBillboardModel(CONST_TV_BILLBOARDTYPE billboardType, string billboardTexturePath, string shaderPath, Material emissiveMaterial, CONST_TV_BLENDINGMODE blendMode, bool alphaTest)
        {
            // half the width or half height of a square billboard is not the radius.
            // the radius would be the hypotenus from center to corner.  Using Sqrt(8) and divide by 2
            // gives us the unit 
            float billboardUnitRadius = (float)Math.Sqrt(8) / 2f; 
            // note: below we use the *4 only because the image file we use
            //       doesn't fit the rect fully (well it does but just visually doesn't appear to do so)
            //       because there's so much empty alpha area along the outer edges.  
            //       It ends up being 25% smaller than the it's sphere radius
            float billboardUnitWidth = billboardUnitRadius * 2 * 4; 
            float billboardUnitHeight = billboardUnitWidth;
            
            string shareableBillboardID = Billboard.GetCreationString (billboardType,
               								true, billboardUnitWidth, billboardUnitHeight);
            Mesh3d mesh = (Billboard)Repository.Create (shareableBillboardID, "Billboard");
            Model model = new Model(Repository.GetNewName(typeof(Model)));

			
            Appearance.Appearance appearance = CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL, shaderPath, billboardTexturePath, null, null, null);
            // TV_BLEND_ALPHA - simple alpha blending of source with whatever already exists in the backbuffer using material opacity value
            // TV_BLEND_ADD   - adds pixels where alpha component in texture > 0.  This is used as preferred alternative to color keying
            // TV_BLEND_ADDALPHA - adds pixels where alpha component in texture > 0 and blends. Used often for particle textures like smoke
            
            // TV_BLEND_ADD is correct so that clouds when rendering after atmosphere (which hosts nighttime starmap)
            // will allow starmap to be visible through clouds.
            appearance.BlendingMode = blendMode;  
			
            Diffuse layer = (Diffuse)appearance.Layers[0];
            // TODO: I should not have to LoadTVResource() here.  It should load when deserialized.
//            Keystone.IO.PagerBase.LoadTVResource (layer.Texture, false);

        	// NOTE: if there is z-fighting between mostly non-transparent quads, then depthwriteneable = true might help
        	// however, most of the time (eg like in particle systems) we would rather have z-fighting with good alpha blending
        	// between all layers, rather than top most z quad billboard to render and any billboard particles behind it to be ignored.
        	// That results in farther z billboards to just suddenly appear if they move out from under the top most billboards.
        	layer.AlphaTestDepthWriteEnable = false; // could prevent some zfighting
        	layer.AlphaTest = alphaTest;
        	// ignore pixels when the alpha in the texture <= the ref value
        	layer.AlphaTestRefValue = 128;
            
            // TODO: as a generic method, having a default material automatically sucks... 
            // what i dont want any material added? (eg not doing a Star material or any that requires default diffuse material)
            // use passed in material which is same as main star sphere
            //if (emissiveMaterial != null)
            //{
            	appearance.RemoveMaterial();
            //	appearance.AddChild(emissiveMaterial);
            //}
            model.AddChild(appearance);
            model.AddChild(mesh);
            return model;
        }

        /// <summary>
		/// Replaces a Model's Mesh3d Geometry child with a MinimeshGeometry child instead.
		/// </summary>
		/// <param name="maxMinimeshCount"></param>
		/// <param name="model"></param>
		/// <returns></returns>
		public static InstancedGeometry CreateInstancedGeometryRepresentations(uint initialMinimeshCapacity, Model model, float height)
		{
    		//CoreClient._CoreClient.Settings.settingRead ("sectionname", "keyname");            		
    		// -> Not all models will cast shadow, but we may forget to set .CastShadow=true on ones that do. System.Diagnostics.Debug.Assert (model.CastShadow == true);
    		//System.Diagnostics.Debug.Assert (model.ReceiveShadow == true);
    		
			// set Y value of minimeshGeometry's Model to height appropriate for this Level.  X,Z are always at 0,0 however for all levels of all zone structures.
			model.Translation = new Keystone.Types.Vector3d (0, height, 0);
			
    		// grab the assigned geometry for this sub-model and create a MinimeshGeometry using same Resource
    		Geometry mesh3d = model.Geometry;
    		
    		if (mesh3d == null) // first model may be a "empty" null model
    		{
    			// there's no existing geometry node and we dont need to create an "empty' mmGeometry either
				return null;     			
    		}
    		
    		InstancedGeometry instancedGeometry = null;
    		string id = "InstancedGeometry" + "_" + mesh3d.ID;
    		instancedGeometry = (InstancedGeometry)Resource.Repository.Get (id);

    		if (instancedGeometry == null)
    		{
    			// NOTE: No MinimeshGeometry sharing. The minimeshGeometry is unique for each Level and each Model within a Segment
    			//       But this may need to change for memory requirements.  We may end up sharing afterall and then persisting
    			//       position/scale/rotation/enable arrays during cull() and applying those arrays to shared MinimeshGeometry during Render()
    			instancedGeometry = (InstancedGeometry)Resource.Repository.Create (id, "InstancedGeometry"); // "MinimeshGeometry");
    			instancedGeometry.SetProperty ("meshortexresource", typeof(string), mesh3d.ID);
    		    instancedGeometry.SetProperty ("maxcount", typeof(uint), initialMinimeshCapacity);
    		    // precache is required to avoid random crashes.
    		    instancedGeometry.SetProperty ("precache", typeof(bool), true); 
	    		
	    		// the MaxInstancesCount starts small, but can be increased withing TVMinimesh without losing existing element data
	    		if (instancedGeometry.MaxInstancesCount < initialMinimeshCapacity)
	    			instancedGeometry.MaxInstancesCount = initialMinimeshCapacity;
	    		
	    		// we don't want the Mesh3d falling out of scope before we 
	    		// .PagerBase.LoadTVResource() on the minimesh which needs to gain a reference to the Mesh3d
	    		// TODO: the minimeshGeometry should probably also IncrementRef and DecrementRef that mesh3d 
	    		//       on it's own when it AddParent() it's model
	    		Resource.Repository.IncrementRef (mesh3d); 
	    		
				// in order to deserialize the tile layer data and restore the terrain minimesh elements, the minimesh geometry resource must be loaded
				//Keystone.IO.PagerBase.LoadTVResource (mmGeometry, false);
				
				// BUT we do NOT YET want mmGeometry.PageStatus == PageableNodeStatus.Loaded and the call to IO.PagerBase.LoadTVResource() will do that
				// and we first still need to load mapdata.
				instancedGeometry.LoadTVResource(); 
				
				Resource.Repository.DecrementRef (mesh3d);
			}
    		
    		model.RemoveChild (mesh3d);
    		model.AddChild (instancedGeometry);
			return instancedGeometry;
		}
		
        // TODO: CreateFlare() as entity or model may be obsolete.  
        // Currently we've implemented flares via DomainObject script
        // that calls VisualFX.DrawTexturedQuad() to draw it's own flare
        // in the OnRender() method.
        //internal static ModeledEntity CreateFlare(int width, int height)
        //{          
        //    // TODO: is flare a modelsequence or an entity of it's own added as a child entity?
        //    // as a sequence it must then be added to a modelSequence of itself that is child of
        //    // the squence that holds corona and sphere... because an entity can only have one
        //    // model or modelsequence as direct child.
        //    // but as entity a flareEntity can just be child of Star entity... but
        //    // that makes it not possible for the Star's script to modify the flare models without
        //    // first querying for the first child entity that is the FlareEntity


        //    //ModeledEntity flareEntity = StaticEntity.Create();
        //    //ModelSelector selector = new ModelSequence(Repository.GetNewName(typeof(ModelSequence)));

        //    // TODO: the FlareEntity.Model.Render() should call
        //    // OnRender() script to re-compute all the positions for
        //    // all it's sub-Model flares.

        //    // - This entity is not really attached to scene.  It's simply a FX
        //    //   that is triggered by a Star that is visible.
        //    // FlareEntity <-- entity is still just a system like an FX derived right?
        //    //      ModelSelector[] Children
        //    //            Model[0] Flare1
        //    //            Model[1] Flare2

        //    // common.zip|scripts_entities\star.css
        //    //
        //    // though recall in our star.css we started to write code
        //    // to manipulate flare there as a sub-model.  the key though is we dont want
        //    // flare to contribute to bounding volume.  I think that should be ok as its
        //    // like a convexhull model?  But also somehow if the parent star is visible we want
        //    // to render this un-bounded flare

        //    //// Flare's should be monitored by systems's just as a Particle System
        //    //// manages it's particles, FlareSystem should manage the flares
        //    //// And i think a System is just another Entity in a way... only.. hrm..

        //    Model model = new Model(Repository.GetNewName(typeof(Model)));

        //    TexturedQuad2D quad = TexturedQuad2D.Create(Repository.GetNewName(typeof(TexturedQuad2D)),
        //                                            width, height);

        //    string flarePath = @"pool\Shaders\Sky\HexaFlare.jpg";
        //    ////flarePath = @"pool\Shaders\Sky\glow.dds";

        //    Appearance.Appearance appearance = CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL, 
        //        "", flarePath, "", "", "");
        //    appearance.BlendingMode = CONST_TV_BLENDINGMODE.TV_BLEND_ADD;
        //    appearance.AlphaTestRefValue = 0;
        //    appearance.AlphaTestDepthWriteEnable = false;
        //    appearance.AlphaTest = false;

        //    model.AddChild(appearance);
        //    model.AddChild(quad);

        //    //sequence.AddChild(model);


        //    //flareEntity.AddChild(sequence);
        //    //return flareEntity;

        //    return null;
        //}

        /// <summary>
        /// Accepts a world that is instanced and begins to generate the graphical and animation controllers for rendering this world
        /// </summary>
        /// <param name="newWorld"></param>
        /// <param name="terrestial"></param>
        /// <param name="hasRings"></param>
        public static void InitWorldVisuals(Entity parent, World newWorld, bool terrestial, bool hasRings, bool hasAtmosphere, bool hasClouds)
        {
            CONST_TV_MESHFORMAT format = CONST_TV_MESHFORMAT.TV_MESHFORMAT_TEX1 | CONST_TV_MESHFORMAT.TV_MESHFORMAT_BUMPMAPINFO;

            //world.Biosphere.Atmosphere.Composition.Length;
            // world.Biosphere.OceanCoverage;

            // this post talks about how to generate gas giant textures
            // http://www.gamedev.net/community/forums/mod/journal/journal.asp?jn=263350&cmonth=2&cyear=2007
            // The gas giant texture is procedurally generated by stretching fBm noise with a large non-uniform factor, like ( 1, 30, 1 ).
            // This generates the straight strips. To break the straightness (english?), I am actually using another fBm which returns 
            // the amount of stretching to do. In the end, it looks like this:
            //float res = noise.fBm(ray * (4, 4, 4) );
            //res = noise.fBm(ray * (1, 20 + res * factor, 1));
            // The first factor, (4, 4, 4), determines the size of the turbulence "features"; while the second factor ( factor )
            // determines how stretched the atmosphere looks. Gas giants planets are rotating very quickly around themselves, 
            // which is why their clouds are so stretched. This factor will later be determined by the astrophysical parameters 
            // of the planet like its rotation speed.
            //         GenerateWorldTexture(true, 1);


            newWorld.InheritRotation = false;

            ModelSequence sequence = new ModelSequence("sequence_for_" + parent.TypeName + "_" + newWorld.ID);
            sequence.InheritScale = true;
            Vector3d scale;
            scale.x = scale.y = scale.z = newWorld.Radius;
            sequence.Scale = scale;
            sequence.InheritScale = true; // here all models in sequence inherit scale of world.
            sequence.InheritRotation = false;// here all models wont inherit rotation of world, we rotate (if applicable) each model in sequence independantly
            
            
            // WORLD MODEL
            string diffuse, normalmap, specular, emissive, shader;
            diffuse = "";
            normalmap = "";
            specular = "";
            emissive = "";
            shader = "";

            // generate a texture based on the biosphere type
            // generate an atmosphere if there is one
            // generate a cloud layer if there is one
            if (terrestial)
            {
                // compute the atmosphere color and such and apply shader to the atmosphereent
                //switch (world.Biosphere.BiosphereType)
                //{
                //    case BiosphereType.Earthlike :
                //        break;
                //    case BiosphereType.Desert :
                //        break;
                //    case BiosphereType.Ocean:
                //        break;
                //    case BiosphereType.IcyRockball:
                //        break;
                //    case BiosphereType.Rockball:
                //        break;
                //    case BiosphereType.Hostile_A :
                //        break;
                //    case BiosphereType.Hostile_N :
                //        break;
                //    case BiosphereType.Hostile_SG :
                //        break;
                //    default:
                //        Trace.Assert(false);
                //        break;
                //} 


                // TODO: store in array and randomly select textures of specific types
                if (hasClouds == false && hasAtmosphere == false)
                {
                    diffuse = @"caesar\Shaders\Planet\MoonDiffuse.dds";
                    if (newWorld.Name != "Luna")
                    {
                        diffuse = @"caesar\Shaders\Planet\planets_terrain_lavarock_diff.png";
                        diffuse = @"caesar\Shaders\Planet\planet_Dante.png";
                        diffuse = @"caesar\Shaders\Planet\planet_Miners_Moon_4138.png";
                    }

                    // NOTE: Normal requires a light with a diffuse component 
                    normalmap = @"caesar\Shaders\Planet\MoonNormal.dds";

                    format |= CONST_TV_MESHFORMAT.TV_MESHFORMAT_TEX2;
                    // format |= CONST_TV_MESHFORMAT.TV_MESHFORMAT_TEX3 // normalmap will be TEX2 unless two diffuse halves used in which case normalmap will be TEX3 
                }
                else
                {
                    format |= CONST_TV_MESHFORMAT.TV_MESHFORMAT_TEX2;
                    // format |= CONST_TV_MESHFORMAT.TV_MESHFORMAT_TEX3 // normalmap will be TEX2 unless two diffuse halves used in which case normalmap will be TEX3 
                    //diffuse = @"pool\Shaders\Planet\surface_ganymede_blueyellow_4k.jpg";
                    //diffuse = @"pool\Shaders\Planet\mars_high.jpg";
                    // http://dejankober.com/2010/12/generic-planet-project/   <-- really nice look!
                    // http://forum.unity3d.com/threads/212590-NumberFlow-visual-editor-for-procedural-textures
                    diffuse = @"caesar\Shaders\Planet\Mars - Terraformed.jpg";
                    diffuse = @"caesar\Shaders\Planet\Planet_7_d.png";
                    //diffuse = @"pool\Shaders\Planet\gradius.png";
                    diffuse = @"pool\Shaders\Planet\DiffuseSpecEast.dds";
                    diffuse = @"caesar\Shaders\Planet\libnoisegen.dds";
                    diffuse = @"caesar\Shaders\Planet\2k_earth_daymap.jpg";
                    //diffuse = @"pool\Shaders\Planet\libnoisegen.bmp"; // @"Shaders\Planet\DiffuseSpecWest.dds";
                    //diffuse = @"caesar\Shaders\Planet\libnoisegen1.bmp";
                    //diffuse = @"pool\Shaders\Planet\libnoise_example.jpg";
                    normalmap = @"caesar\Shaders\Planet\flatnormalmap.dds"; //EarthParallax.dds";


                    // emissive = @"pool\Shaders\Planet\EarthEmissive.dds";
                    // CONST_TV_MESHFORMAT.TV_MESHFORMAT_TEX3 | // emissive will be in slot 4 if two diffuse halves used, otherwise slot4
                    // CONST_TV_MESHFORMAT.TV_MESHFORMAT_TEX4 |

                    

                }

                // NOTE: Specular slot is used to store the normal map ONLY IF TWO
                //diffusemaps are used.  This is very important to remember.
                shader = @"caesar\Shaders\Planet\WorldShader.fx";
                //  shader = ""; // Toaster's atmo doesnt have a planet shader
            }
            else
            {
                // NOTE: gas giants use flat normal map... so still requires WorldShader.fx
                diffuse = @"caesar\Shaders\Planet\neptune_current.dds";
                //diffuse = @"caesar\Shaders\Planet\libnoisegen.bmp";
                normalmap = @"caesar\Shaders\Planet\flatnormalmap.dds";
                shader = @"caesar\Shaders\Planet\WorldShader.fx"; 

                format |= CONST_TV_MESHFORMAT.TV_MESHFORMAT_TEX2;
                // format |= CONST_TV_MESHFORMAT.TV_MESHFORMAT_TEX3 // normalmap will be TEX2 unless two diffuse halves used in which case normalmap will be TEX3 
            }


            Model worldModel = GenerateWorldModel(newWorld, newWorld.AxialTilt, format, CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL, shader, diffuse, normalmap, emissive, specular);
            worldModel.Name = newWorld.Name + "_world";

            if (string.IsNullOrEmpty(newWorld.Name))
            {
                newWorld.Name = newWorld.ID;
            }
            

            // CLOUDS, ATMOSPHERE, RINGS
            Model cloudsModel = null;
            Model atmosphereModel = null;
            Model ringModel = null;

            if (terrestial)
            {
                // no atmosphere and cloud layers are needed for Trace or None atmosphere
                //if (newWorld.Biosphere.Atmosphere.Pressure > Atmosphere.ATMOSPHERIC_PRESSURE.Trace)
                //{
                    float ratio = .7f; //GetRatioBasedOnPressureAndAlbedoOrSomething(w));
                    if (hasAtmosphere)
                    {
                        // TODO: here rather than a radius, we send a 1.08 to represent the added
                        //       scale compared to the parent since it inherits parent.  So for now
                        // let's just do this for atmosphere and clouds and have them inherit and then test
                        float atmosphereSphereRadius = newWorld.Radius + (newWorld.Radius * .08f);
                        scale.x = scale.y = scale.z = 1.08;
                        atmosphereModel = CreateAtmosphereLayer(scale, true); 
                        atmosphereModel.Name = newWorld.Name + "_atmosphere";

                        // a terrestial planet with clouds must have an atmosphere however
                        // a gas giant may have clouds but no seperate atmosphere mesh using atmosphere
                        // shader
                        if (hasClouds)
                        {
                            // clouds should be relatively low... just high enough to not cause
                            // z fighting with world sphere.  If you consider outspace to be 80km to
                            // 120km depending on your source since there is no exact dividing line with
                            // atmosphere, just varying degrees of thin until 120km where the detection of 
                            // atmosphere ions is nearly undetectable.
                            // and earth nimbus clouds going up to 15 kilometers then you can see the clouds 
                            // only extend 1/5 to 1/8th the altitude of the total atmosphere.
                            float cloudSphereRadius = newWorld.Radius + (newWorld.Radius * .025f);
                            scale.x = scale.y = scale.z = 1.025;
                            cloudsModel = CreateCloudLayer(scale); 
                            cloudsModel.Name = newWorld.Name + "_clouds"; ;
                        }
                    }
                //}
            }
            else
            {
                // no seperate cloud or atmosphere layers for gas giants or ice giants althought
                // you could maybe try modeling a storm like jupiter's eye using an animated texture map
            }

            // terrestial planets can have rings but it's very unlikely
            // http://answers.yahoo.com/question/index?qid=20071129235852AA9DWWw
            if (hasRings)
            {
                // TODO: if we had a 1 tvunit radius ring mesh we would want radius in the range of 2 - 4x planet radius
                // but i think this ring mesh we're using is already ~5 tvunit radius
                scale.x = scale.y = scale.z = 1.55;  
                //scale.y = 1.0;
                ringModel = ProceduralHelper.GenerateRings(scale, 
                    @"caesar\Shaders\Planet\Rings2.fx", 
                    @"caesar\Shaders\Planet\ring.png", 
                    @"caesar\Shaders\Planet\noise.jpg");

                ringModel.Name = newWorld.Name + "_rings";
            }

            // clouds, atmosphere, rings order is important
            sequence.AddChild(worldModel);
            if (cloudsModel != null) sequence.AddChild(cloudsModel); // NOTE: added after ring and atmosphere
            if (atmosphereModel != null) sequence.AddChild(atmosphereModel); // NOTE: added after any ring
            if (ringModel != null) sequence.AddChild(ringModel);

            newWorld.AddChild(sequence);

            #region orbits
            //// June.21.2017 - Orbit Elliptical Animations feature cut for v1.0.  
            //// ORBIT ANIMATION (used instead of a script or physics modeling)
            //Keystone.Animation.EllipticalAnimation clip = new Keystone.Animation.EllipticalAnimation(Repository.GetNewName(typeof(Keystone.Animation.EllipticalAnimation)));
            //clip.Name = newWorld.ID + "_orbit_clip";
            //clip.TargetName = newWorld.Name;
            //// TODO: if i did not need to specify TargetName in Animation.cs
            //// i could avoid setting length here too and set it dynamically in animation.Update()
            //// and allow the AnimationTrack() to track progress per frame.  This way
            //// the actual Animation could be shared.  For now we wont worry about it.
            //clip.Duration = newWorld.OrbitalPeriod;

            //string id = Repository.GetNewName(typeof(Keystone.Animation.Animation));
            //Animation.Animation anim = new Keystone.Animation.Animation(id);
            //anim.Looping = true;
            //anim.AddChild(clip);
            //anim.Name = newWorld.ID + "_orbit_animation";
            //newWorld.AddChild(anim);

        #endregion // orbits

            // GRAVITY EMISSION & SCRIPT
            // orbit script
            //// load an orbit behavior that we'll assign to everything
            //string orbitScriptPath = @"pool\scripts_behaviors\orbit.css";
            //Keystone.Behavior.Actions.Script orbitScript = Keystone.Behavior.Actions.Script.Create(orbitScriptPath);
            string scriptPath = @"caesar\scripts_entities\world.css";
            MakeDomainObject(newWorld, scriptPath);

            // Add the world directly to whatever system or star it's been created under
            Keystone.Traversers.SuperSetter setter = new Keystone.Traversers.SuperSetter (parent);
            setter.Apply(newWorld);
            
            // play must occur after adding newworld to parent // TODO: i should allow a state to be set so taht when finally adding to a parent, it will begin to play
            // TODO: when deserializing, how do i play these? i mean i wont know the names
            // but because of naming convention i could deduce the name = myID + "_orbit_animation"
            // June.21.2017 - Orbit animations feature cut in v1.0.  All planets are now at fixed locations within their Zones.
            // newWorld.Animations.Play(anim.Name, true);

            //ModeledEntity orbit = InitOrbitLineStrip(newWorld.ID + "_orbit", newWorld.Name + "_orbit", newWorld.OrbitalRadius, newWorld.OrbitalEccentricity, newWorld.OrbitalInclination, newWorld.OrbitalProcession);
            //// orbit line mesh has same parent as newworld,
            //setter = new Keystone.Traversers.SuperSetter(parent);
            //setter.Apply(orbit);

            //Hypnotron says:
            //was the seperation of the diffuse east/west and clouds east/west  just to keep the texture's square?
            //zaknafein. says:
            //yep because i thought (i'm not so sure anymore) that the max resolution is 2048 x 2048
            //so i made a shader that takes two of those textures and "splats" them side by side
            //zaknafein. says:
            //resulting in 4096x2048
            // Actually, I think Zak might be right... for low end sm2.0 cards we shouldnt go lower than 2048x2048
            // talked to Zak about this atmosphereBack and it seems completely unnecessary.  There's no visual quality difference.
            //TVMesh atmosphereBack = Core._Core.SceneManager.CreateMeshBuilder("AtmosphereBack");
            //atmosphereBack.CreateSphere(108f, 100, 100);
            //atmosphereBack.SetLightingMode(CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL);
            //atmosphereBack.SetMeshFormat((int)(CONST_TV_MESHFORMAT.TV_MESHFORMAT_NOTEX));
            //atmosphereBack.SetCollisionEnable(false);
            //atmosphereBack.SetCullMode(CONST_TV_CULLING.TV_FRONT_CULL); // FRONT CULLED.  This is the key.
            //atmosphereBack.Shader = atmosphereShader;

            //planetModel.Rotation = new Vector3d(23f, 0, 0); // Axial tilt?

            //cloudModel.Rotation = new Vector3d(23f, 0, 0);
        }

        // TODO: planet atmosphere is alpha so to blend with the back portion of a ring, the ring needs to be drawn first
        // so the back ring needs to be a seperate entity and front and back need to rotate with respect to the camera
                        

        //http://local.wasp.uwa.edu.au/~pbourke/modelling_rendering/asteroid/
        private void AddImpactCraters()
        {
            //In order to create realistic looking craters, the fractal generation is insufficient, 
            // or generates only ugly landscapes. For this, additional craters are added on some 
            // place by multiplying the heightvalues at some area of a heihgtmap with a factor, 
            // calculated depending on their distances to the crater-center and some variables
            // set within the Crater-class. 
        }

        private static Model GenerateWorldModel(World world, float tilt, CONST_TV_MESHFORMAT format, CONST_TV_LIGHTINGMODE lightingMode, string shader, string diffuseTex, string normalmapTex, string emissiveTex, string specularTex)
        {
            
            Model model = new Model(Repository.GetNewName(typeof(Model)));

            //Hypnotron: was the seperation of the diffuse east/west and clouds east/west  just to keep the texture's square?
            //zaknafein: yep because i thought (i'm not so sure anymore) that the max resolution is 2048 x 2048
            //so i made a shader that takes two of those textures and "splats" them side by syde
            //resulting in 4096x2048
            Mesh3d mesh = Mesh3d.CreateSphere(UNIT_SPHERE_RADIUS, SLICES, STACKS, format, false);

            // use LIGHTING_NORMAL if using WorldShader.fx
            Appearance.Appearance appearance = CreateAppearance(lightingMode, shader, diffuseTex, normalmapTex, emissiveTex, specularTex);
            if (string.IsNullOrEmpty(normalmapTex) == false)
            	appearance.AddDefine ("NORMALMAP", null);
            if (string.IsNullOrEmpty (specularTex) == false)
            	appearance.AddDefine ("SPECULARMAP", null);
            if (string.IsNullOrEmpty (emissiveTex) == false)
            	appearance.AddDefine ("EMISSIVEMAP", null);
            
            model.AddChild(appearance);
            model.AddChild(mesh);

            model.Name = world.Name + "_Model";
            // TODO: I'm not rotating other planets besides "Earth"
            // TODO: why dont i just pass in the Entity name?
            if (world.Name == "Earth")
            {
                // let's test a spherical rotation for the model                
                Vector3d axis;
                axis.x = 0.39875;
                axis.y = 0;
                axis.z = 0.91706; // i believe the reason this is flipped compared to my calc below
                // is a gl vs dx thing?

                // http://stackoverflow.com/questions/1568568/how-to-convert-euler-angles-to-directional-vector
                // TODO: the following seems to work for 
                double yaw = 0;
                double pitch = 1.5 * Utilities.MathHelper.DEGREES_TO_RADIANS;

                //axis.x = Math.Cos(yaw) * Math.Cos(pitch);
                //axis.y = Math.Sin(yaw) * Math.Cos(pitch);
                //axis.z = Math.Sin(pitch);

                axis.x = Math.Sin(pitch) * Math.Cos(yaw);
                axis.y = Math.Sin(pitch) * Math.Sin(yaw);
                axis.z = Math.Cos(pitch);
                //Quaternion qaxis = new Quaternion(0, 23.5 * Utilities.MathHelper.DEGREES_TO_RADIANS, 0);
                //double angleRadians = 0;
                //Vector3d axis2 = qaxis.GetAxisAngle(ref angleRadians);

                Quaternion q1 = new Quaternion(axis, 0.01 * Utilities.MathHelper.DEGREES_TO_RADIANS);
                // TODO: if i normalize all of these quats, can i skip normalize in the anim's slerp?
                Quaternion q2 = new Quaternion(axis, 359.99 * Utilities.MathHelper.DEGREES_TO_RADIANS);
                Keystone.Animation.KeyframeInterpolator<Quaternion> clip = MakeAnimation( world.ID + "_Axis_Rotation_Clip", world.Name);
                clip.AddKeyFrame(q1);
                clip.AddKeyFrame(q2);
                clip.Duration = 30; // 24f * HOURS_TO_SECONDS; //seconds

                string id = Repository.GetNewName(typeof(Keystone.Animation.Animation));
                Animation.Animation anim = new Keystone.Animation.Animation(id);
                anim.AddChild(clip);
                anim.Name = world.ID + "_Axis_Rotation_Animation";
                world.AddChild(anim);
                // TODO: should not .Play() here.  Should auto-play when deserialized
                //       do I need to set "loop" = true in some property?
                world.Animations.Play(anim.Name, true);
            }


            // we inherit parent scale for this so no model scale used
            // TODO: Problem, in order to inherit the World's entity scale
            // and _IF_ this world Model is placed under a ModelSequence, then
            // that sequence MUST ALSO inherit parent's scale!  This is the tricky nature
            // of working with Selector nodes.  So by default we should not rely on a parent's
            // scale...
            // The advantage is however that you can animate just a selector node and allow
            // any LOD to be animated with no extra cost.  So i dont think there's anything broken
            // with selector nodes except that by default these typically 
            // furthermore, if we scale the model 
            model.InheritRotation = false;
            model.InheritScale = true;
            return model;
        }

        // EVE Online talks about their planet rendering improvemetns.
        // They also talk about how they use two half rings to deal with draw order issue
        // http://www.eveonline.com/devblog.asp?a=blog&bid=724
        private static Model GenerateRings(Vector3d scale, string shaderPath, string diffuseTex, string noiseTex)
        {
            string id = Repository.GetNewName(typeof(Model));
            Model model = new Model(id);
            model.InheritRotation = false;
            model.InheritScale = true; // note: i cannot inherit the .Y scale

            // TODO: we should make our ring mesh unit radius 1. Did we make ring.tvm 1 meter radius yet? (unit means 1 radius not .5 radius)
            string path = "caesar\\Meshes\\worlds\\ring.tvm";
			path = Core.FullNodePath(path);
	
            Mesh3d mesh = (Mesh3d)Resource.Repository.Create (path, "Mesh3d");
           
            Appearance.Appearance appearance = CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_NONE, shaderPath, diffuseTex, noiseTex , "", "");
            appearance.BlendingMode = CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA;

            model.AddChild(appearance);
            model.AddChild(mesh);

            // TODO: the problem im having is in our NavView, it'd be nice if i could just
            // scale the root Entity and have everything scale properly.  OR to have coronas and
            // cloud layers, atmosphere layers, planetary rings all follow same rule of scaling
            // where NONE inherit scale and they all have their own scales set.
            // NOTE: i think rings should inherit rotation though, although it probably wouldnt be noticeable.
            // clouds should too along with a slightly additional bit of rotation animation?
            // note: here the scale is just 1.  so the ring is virtually 2D.  That being the case, 
            // if the mesh were truely 2D, we could just scale it
            // note: if we were to inherit scale, then the rings scale would be a percentage increase
            // of the parent.
            model.Name = "rings_" + id;
            model.Scale = scale;
            return model;
        }

        private static Model CreateCloudLayer(Vector3d scale)
        {
            Model model = new Model(Repository.GetNewName(typeof(Model)));
            //clouds.Translation = new TV_3DVECTOR(); // relative position is 0,0,0 with respect to its parent model
            //clouds.Rotation = new TV_3DVECTOR(23f, 0, 0);
            //clouds.Scale = new TV_3DVECTOR(1, 1, 1);
            Mesh3d mesh = Mesh3d.CreateSphere( UNIT_SPHERE_RADIUS, SLICES, STACKS, 
                               CONST_TV_MESHFORMAT.TV_MESHFORMAT_TEX1 |
                               CONST_TV_MESHFORMAT.TV_MESHFORMAT_TEX2 |
                               CONST_TV_MESHFORMAT.TV_MESHFORMAT_TEX3 |
                               CONST_TV_MESHFORMAT.TV_MESHFORMAT_BUMPMAPINFO, false);
            // TODO: Need invidual collision enabling of sub-models
            // and our entity collide needs to test it's own sub-models
  //          model.CollisionEnable = false;

            // TWO DIFFUSE appearance
            //Appearance.Appearance appearance = CreateAppearance (CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL,
            //    @"pool\Shaders\Planet\WorldShader.fx", 
            //    @"pool\Shaders\Planet\CloudsEast.dds",     // using TWO diffuse use DIFFUSE and NORMALMAP
            //    @"pool\Shaders\Planet\CloudsWest.dds",
            //    "",
            //    @"pool\Shaders\Planet\CloudsNormal.dds");  // using TWO Diffuse, normalmap goes in SPECULAR

            // SINGLE DIFFUSE Appearance
            Appearance.Appearance appearance = CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL,
                @"caesar\Shaders\Planet\WorldShader.fx",
                @"caesar\Shaders\Planet\clouds.jpg",
                //@"caesar\Shaders\Planet\CloudsEast.dds",     // using TWO diffuse use DIFFUSE and NORMALMAP
                @"caesar\Shaders\Planet\CloudsNormal.dds",
                "",
                "");  // using TWO Diffuse, normalmap goes in SPECULAR
            

            appearance.BlendingMode = CONST_TV_BLENDINGMODE.TV_BLEND_ADD;
            model.AddChild(mesh);
            model.AddChild(appearance);
            model.InheritRotation = false;
            model.InheritScale = true;
            model.Scale = scale;
            return model;
        }

        private static Model CreateAtmosphereLayer(Vector3d scale, bool Zaks)
        {
            Model model = new Model(Repository.GetNewName(typeof(Model)));
            Appearance.Appearance appearance = null;

            //atmosphereEnt.Rotation = new TV_3DVECTOR(23f, 0, 0);
            //atmosphereEnt.Scale = new TV_3DVECTOR(1, 1, 1);
            CONST_TV_MESHFORMAT format= (CONST_TV_MESHFORMAT)0;
            if (Zaks)
                format = CONST_TV_MESHFORMAT.TV_MESHFORMAT_NOTEX;
            
            Mesh3d mesh = Mesh3d.CreateSphere(UNIT_SPHERE_RADIUS, SLICES, STACKS, format, false);
            
            if (Zaks == false)
                mesh.CullMode = (int)CONST_TV_CULLING.TV_FRONT_CULL ;

           

            //appearance = new DefaultAppearance(Repository.GetNewName(typeof(DefaultAppearance)));
            
            string shaderPath = @"caesar\Shaders\Planet\AtmosphereShader_DirectionalLight.fx"; // AtmospherePS3NEW.fx"; // had a Color parameter
            

            // TODO: these two dont exist, all there is a "color" parameter for Zak's shader
            //atmosphereShader.TVShader.SetEffectParamFloat("Atmosphere Radius", atmosphereRadius );
            //atmosphereShader.TVShader.SetEffectParamFloat("Surface Radius", planetRadius);
            //appearance.AddChild(atmosphereShader);

            if (Zaks == false)
            {
                //GroupAttribute ga = new GroupAttribute(Repository.GetNewName(typeof(GroupAttribute)));
                string gradientPath = @"caesar\Shaders\Planet\AtmosphereGradient3.jpg"; // @"pool\Shaders\Planet\Grad.dds"; // 
                //Diffuse diffuse = Diffuse.Create(gradientPath);
                appearance = CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL, shaderPath, gradientPath, "", "", "");

                // TODO: the following should be set by semantic with TEXTURE0
                //atmosphereShader.TVShader.SetEffectParamTexture("gTex", diffuse.TextureIndex);
                //ga.AddChild (diffuse );
                //appearance.AddChild(ga);
            }
            else
            {
                appearance = CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL, shaderPath, "", "", "", ""); 

            }

            appearance.BlendingMode = CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA;
           // ModelBase atmosphereModelFront = new SimpleModel(Repository.GetNewName(typeof(SimpleModel)));
            
            model.AddChild(mesh);
            model.AddChild(appearance);


            //// BACK ATMOSPHERE - Unneeded?
            // // // talked to Zak about this atmosphere back and it seems completely unnecessary.  There's no visual quality difference.
            //TVMesh atmosphereBack = Core._Core.SceneManager.CreateMeshBuilder("AtmosphereBack");
            //atmosphereBack.CreateSphere(108f, 100, 100);
            //atmosphereBack.SetLightingMode(CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL);
            //atmosphereBack.SetMeshFormat((int)(CONST_TV_MESHFORMAT.TV_MESHFORMAT_NOTEX));
            //atmosphereBack.SetCollisionEnable(false);
            //atmosphereBack.SetCullMode(CONST_TV_CULLING.TV_FRONT_CULL); // FRONT CULLED.  This is the key.
            //atmosphereBack.Shader = atmosphereShader;
            model.InheritRotation = false;
            model.InheritScale = true;
            model.Scale = scale;
  // TODO: Need seperate collisions
            // model.CollisionEnable = false;
            return model;
        }


        public static Background3D CreateCelestialSphere(string id, float radius, Keystone.Types.Color color)
        {
            
            ModelSequence sequence = new ModelSequence (Repository.GetNewName(typeof(ModelSequence)));
			// IMPORTANT: Scale sequence so sphere mesh and BillboardText are all scaled
            sequence.Scale = new Vector3d  (radius, radius, radius);
            			
            Model model = new Model(Repository.GetNewName(typeof(Model)));
            
            // even if texture is not set, appearance allows us to set material 
            Appearance.Appearance appearance = CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL, "tvdefault", "", "", "", "");
            //appearance.BlendingMode = CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA;
            if (appearance.Material != null)
                appearance.RemoveChild(appearance.Material); // the CreateAppearance call above creates a default we want to remove
            Keystone.Types.Color black  = new Keystone.Types.Color(0, 0, 0, 255);

            Material material = Material.Create("emissive_material_" + color.ToInt32().ToString(), black, black, black, color);
            appearance.AddChild(material);
            
            model.AddChild(appearance);

			// 360 / 5 = 72
			//mesh = Mesh3d.CreateSphereWireframeLineList(72, 72);
			Mesh3d mesh = Mesh3d.CreateLatLongSphere (72, 72);
			mesh.Name = "celestial_sphere_" + Repository.GetNewName(typeof(Mesh3d));
			

            model.AddChild(mesh);
            sequence.AddChild (model);
            
            Background3D backgroundEntity = new Background3D(id);
            backgroundEntity.Name = "";
            backgroundEntity.AddChild(sequence);

            
            Model textModel = new Model(Repository.GetNewName(typeof(Model)));
            textModel.Translation = new Vector3d (0, 0, 0.0025);
		    textModel.Name = "";
		    
            BillboardText text = new BillboardText (Repository.GetNewName(typeof(BillboardText)));
            text.Text = "00";

            
            textModel.AddChild (text);
            sequence.AddChild (textModel);
            
            // create the billboardtext and add it to the sequence
            for (int i = 0; i < 360; i+= 15)
            {
            	
            	// at 0, 90, 180, 270 angles, we'll add verticle degrees 
            	// along the equator, we'll add horizontal degrees 
            	if (i == 0) // equator
            	{
            		
            	}
            	
            	for (int j = 0; j < 360; j+= 15)
            	{
            		//Vector3d position = 
            		
            		// at 0, 90, 180, 270 angles, we'll add verticle degrees 
            		
            		// Model textModel = new Model();
		            
		            // BillboardText text = new BillboardText ();
		            // textModel.AddChild (text);
		            // sequence.AddChild (textModel);
            	}
            }
            
            
            
            return backgroundEntity;
        }
        
        public static ModeledEntity CreateCylinder(string id, string texturePath, float radius, float height, int slices, Keystone.Types.Color color)
        {
            
            // even if texture is not set, appearance allows us to set material 
            Appearance.Appearance appearance = CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL, "tvdefault", texturePath, "", "", "");
            //appearance.BlendingMode = CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA;
            appearance.RemoveChild(appearance.Material); // the CreateAppearance call above creates a default we want to remove
            Keystone.Types.Color black  = new Keystone.Types.Color(0, 0, 0, 255);

            Material material = Material.Create("emissive_material_" + color.ToInt32().ToString(), black, black, black, color);
            appearance.AddChild(material);
            
            Model model = new Model(Repository.GetNewName(typeof(Model)));
            model.AddChild(appearance);


        	float halfHeight = height * .5f;
            Mesh3d	mesh = Mesh3d.CreateCone(new Vector3d(0, -halfHeight, 0), new Vector3d(0, halfHeight, 0), radius, radius, slices, 0, Utilities.MathHelper.TWO_PI);
			mesh.Name = "cylinder_" + Repository.GetNewName(typeof(Mesh3d));
            // TODO: a cone with stacks and slices overload

            model.AddChild(mesh);
            
            ModeledEntity entity = new ModeledEntity(id);
            entity.AddChild(model);

            return entity;
        }

        public static ModeledEntity CreateTriangleStripTrail()
        {
            // http://www1.palestar.com/palestar/index.htm?module=forums.php&page=/viewtopic.php?topic=25516&forum=17&4
            // milo from starshatter wrote
            //
            //// If you are just looking for a way to calculate vY, here is the equivalent code from my game engine: 
            //Vector3d  head = trail[1] + loc; 
            //Vector3d tail = trail[0] + loc; 
            //Vector3d vcam = camview->Pos() - head; 
            //Vector3d vtmp = vcam.cross(head-tail); 
            //vtmp.Normalize(); 
            //Vector3d vlat = vtmp * (width + (0.1 * width * ntrail)); 

            //verts->loc[0] = tail - vlat; 
            //verts->loc[1] = tail + vlat; 

            //// My "vlat" is your "vY'. Depending on your conventions, you may need to change
            //// the signs to ensure that the polygon faces directly at the camera instead of
            //// directly away from it. 

            //// Basically, the idea is to take a vector from the trail to the camera position 
            //// and then compute the cross-product of that with a vector that is parallel to 
            //// the length of the trail. The result of the cross-product is always perpendicular 
            //// to both input vectors. This guarantees that the resulting polygon is always normal 
            //// to the camera. 

            return null;
        }

        public static void InitOrbitLineStrip(ModeledEntity orbit, int segmentCount, 
                                              double orbitalRadius, 
                                              float orbitalEccentricity, 
                                              float orbitalInclination, 
                                              float orbitalProcession,
                                              Keystone.Types.Color color, bool quadLines)
        {
            // TODO: perform a test where i speed up jupiters orbit as well as jupiters moons
            // and verify that the moon orbits and jupiters orbits all appropriately inherit hierarchical
            // rotation
            // NOTE: semi-major axis is hardcoded to 1.0f because we use model.Scale (orbitalRadius, orbitalRadius, orbitalRadius); to resize each orbit line list.
            Model model = ProceduralHelper.CreateSmoothElipse(segmentCount, (float)orbitalRadius, 1.0f, orbitalEccentricity, color, quadLines);
            orbit.AddChild (model);
            
            orbit.InheritScale = true; // if we scale the entire solar system, why shouldn't the orbit's inherit that scale?  
            orbit.InheritRotation = false;

            // moon orbital radius = 384,400,000 meters
            // earth orbital radius = 
            // pluto max 7.4 billion km 7,400,000,000,000 meters
            //  384400000.0f; // meters
            Vector3d scale;
            scale.x = orbitalRadius;
            scale.y = orbitalRadius;
            scale.z = orbitalRadius;

            model.Scale = scale;  
            
            if (orbitalInclination != 0f || orbitalProcession != 0f)
                orbit.Rotation = new Quaternion(orbitalProcession, 0, orbitalInclination);
        }

        
        public static Model CreateLine(Vector3d start, Vector3d end, Keystone.Types.Color color, bool quadLines)
        {
            Model model = new Model(Repository.GetNewName(typeof(Model)));
            
            Mesh3d mesh =  null;
            string shaderPath = "tvdefault";
            string texturePath = "";
            
            if (quadLines)
            {
	        	shaderPath = @"pool\shaders\VolumeLines.fx"; // todo: MoveMotionField to caesar\\shaders path
           
               	//texturePath = @"pool\Shaders\Planet\ring.png";
           		//texturePath = @"pool\textures\neutron_bitmap.dds"; // thruster02-01.dds";
            }
            
            Appearance.Appearance appearance = CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL, shaderPath, texturePath, null, null, null);
            
            if (quadLines)
            	appearance.BlendingMode = CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA;
			
            if (appearance.Material != null)
                appearance.RemoveChild(appearance.Material); // the CreateAppearance call above creates a default we want to remove

            Keystone.Types.Color black  = new Keystone.Types.Color(0, 0, 0, 255);

            Material material = Material.Create("emissive_material_" + color.ToInt32().ToString(), black, black, black, color);
            appearance.AddChild(material);

            bool dashedLines = false;
            mesh = Mesh3d.Create3DLineList(Repository.GetNewName(typeof(Mesh3d)), new Vector3d[] {start, end}, dashedLines, quadLines); // linestrip method
    
            model.AddChild(appearance);
            model.AddChild(mesh);
            return model;
        }
        
        public static Model CreateSmoothElipse(int segments, float radius, float semimajoraxis, float eccentricity, Keystone.Types.Color color, bool quadLines)
        {
            Model model = new Model(Repository.GetNewName(typeof(Model)));
            model.InheritScale = true;

            // texturePath and shader are for the shader version of lines, not for 3d primitive mesh using linelist
            string texturePath = "";
            string shaderPath = "tvdefault";
            if (quadLines)
            {
            	texturePath = @"caesar\Shaders\Planet\ring.png";
            	texturePath = @"caesar\textures\neutron_bitmap.dds"; // thruster02-01.dds";
            	shaderPath = @"caesar\shaders\VolumeLines.fx";
            }

            Appearance.Appearance appearance = CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL, shaderPath, texturePath, null, null, null);
            if (quadLines)
            	appearance.BlendingMode = CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA;

            if (appearance.Material != null)
                appearance.RemoveChild(appearance.Material);
            Keystone.Types.Color black  = new Keystone.Types.Color(0, 0, 0, 255);

            Material material = Material.Create("emissive_material_" + color.ToInt32().ToString(), black, black, black, color);
            appearance.AddChild(material);

            model.AddChild(appearance);

            Mesh3d mesh = Mesh3d.CreateEllipse(semimajoraxis, eccentricity, segments, quadLines); // linestrip method    
            model.AddChild(mesh);
            return model;
        }

        public static Model CreateSmoothCircle(int segments, Keystone.Types.Color color, bool dashedSegments, bool quadLines)
        {
            Model model = new Model(Repository.GetNewName(typeof(Model)));
                        
            float unitCircleRadius = 1.0f;

            string shaderPath = "tvdefault";
            string texturePath = "";
            if (quadLines)
            {
	            texturePath = @"caesar\Shaders\Planet\ring.png";
                texturePath = @"caesar\textures\laser.png"; // neutron_trail.dds"; // thruster02-01.dds";
	            shaderPath = @"caesar\shaders\VolumeLines.fx";
            }
                       
            Appearance.Appearance appearance = CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL, shaderPath, texturePath, "", "", "");
            if (quadLines)
	        	appearance.BlendingMode = CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA;

            if (appearance.Material != null)
			    appearance.RemoveChild(appearance.Material); // the CreateAppearance call above creates a default we want to remove

            Keystone.Types.Color black  = new Keystone.Types.Color(0, 0, 0, 255);
            Material material = Material.Create("emissive_material_" + color.ToInt32().ToString(), black, black, black, color);
            appearance.AddChild(material);
            
            model.AddChild(appearance);
            
            // TODO: a billboard option would be nice
            Mesh3d mesh = Mesh3d.CreateCircle(segments, unitCircleRadius, dashedSegments, quadLines); // linestrip method
            model.AddChild(mesh);
            return model;
        }

        public enum MOTION_FIELD_TYPE 
        {
            POINT,
            SPRITE,
            LINE,
            QUAD
        }

        public static ModeledEntity CreateMotionField(string id, MOTION_FIELD_TYPE fieldType, int particleCount, string texturePath, float spriteSize, int color)
        {
        	// Foreground3d entities get bound to camera and are rendered after primary scene elements
            // and using the preserved (not cleared)  zbuffer 
       //     Foreground3d field = new Foreground3d(id);
       //     Background3D field = new Background3D(id); // TODO: TEMP use Background3d 
            // NOTE: the Background3D does not work if this field is attached to a HUD Root element
            // but placing this motion field onto the hud makes it easy to get rid of it when in other workspaces
            ModeledEntity field = new ModeledEntity(id);
            field.UserTypeID = (uint)fieldType; // very important to set this as MoveMotionField requires it to determine how to modify mesh vertices
            //    Background3D field = new Background3D(id);
            field.SetEntityAttributesValue((uint)EntityAttributes.AllEntityAttributes, false);
            //field.SetEntityAttributesValue((uint)EntityAttributes.HUD, true);
            field.SetEntityAttributesValue((uint)EntityAttributes.PickableInGame, false);
            field.CollisionEnable = false;
            field.Dynamic = false;
            field.Enable = true;
            field.Pickable = false;


            int particleTexture = -1;
            // TODO: is loading the texture here mandatory?

            // texture is only relevant for FIELD_TYPE POINT or QUAD
            if (fieldType == MOTION_FIELD_TYPE.POINT || fieldType == MOTION_FIELD_TYPE.SPRITE || fieldType == MOTION_FIELD_TYPE.QUAD)
                if (!string.IsNullOrEmpty(texturePath))
                    particleTexture = CoreClient._CoreClient.TextureFactory.LoadTexture(Core.FullNodePath(texturePath), texturePath);

            Mesh3d mesh = null;
            string relativePath = null;

            // TODO: maybe pass in a FIELD_TYPE (point, sprite, line, quad)
            // if use point sprites
            switch (fieldType)
            {
                case MOTION_FIELD_TYPE.POINT:
                    Vector3d[] p = Utilities.RandomHelper.RandomSphericalPoints(particleCount, 250f);
                    int[] rc = new int[p.Length];
                    for (int i = 0; i < rc.Length; i++)
                        rc[i] = Utilities.RandomHelper.RandomColor().ToInt32();
                        //rc[i] = new Keystone.Types.Color(178, 153, 110, 255).ToInt32(); // dust color
                    relativePath = "caesar\\meshes\\pointsprites\\motionfield_points.tvm";
                    mesh = Mesh3d.CreatePointSprite(relativePath, p, rc, false, 0.1f);
                    break;

                case MOTION_FIELD_TYPE.SPRITE :
                    Vector3d[] points = Utilities.RandomHelper.RandomSphericalPoints(particleCount, 250f);
                    int[] randomColors = new int[points.Length];
                    for (int i = 0; i < randomColors.Length; i++)
                        //randomColors[i] = Utilities.RandomHelper.RandomColor().ToInt32();
                        randomColors[i] = new Keystone.Types.Color(178, 153, 110, 255).ToInt32(); // dust color

                    relativePath = "caesar\\meshes\\pointsprites\\motionfield.tvm";
                    mesh = Mesh3d.CreatePointSprite(relativePath, points, randomColors, particleTexture >= 0, spriteSize);
                    
                    break;
                case MOTION_FIELD_TYPE.LINE :
                    Vector3d[] linePoints = new Vector3d[particleCount * 2]; // each line is composed of 2 vertices
                    for (int i = 0; i < linePoints.Length; i++)
                        linePoints[i] = new Vector3d();
                    relativePath = "caesar\\meshes\\pointsprites\\motionfield_linelist.tvm";
                    mesh = Mesh3d.Create3DLineList("test motion field with lines", linePoints, false, false, 10f);
                    break;
                case MOTION_FIELD_TYPE.QUAD :
                    Vector3d[] lp = new Vector3d[particleCount * 2]; // each line is generated from 2 points. a start and an end
                    for (int i = 0; i < lp.Length; i++)
                        lp[i] = new Vector3d();
                    relativePath = "caesar\\meshes\\pointsprites\\motionfield_quad_linelist.tvm";
                    mesh = Mesh3d.Create3DLineList("test motion field with quad lines", lp, false, true);
                    break;
                default :
                    break;
            }
                       
            
            Model model = new Model(Repository.GetNewName(typeof(Model)));
            if (fieldType == MOTION_FIELD_TYPE.QUAD || particleTexture >= 0)
            {
                string shaderPath = "tvdefault";
                if (fieldType == MOTION_FIELD_TYPE.QUAD)
                {
                    texturePath = @"caesar\Shaders\Planet\ring.png";
                    texturePath = @"caesar\textures\neutron_trail.dds"; // thruster02-01.dds";
                    shaderPath = @"caesar\shaders\VolumeLines.fx";
                }


                Appearance.Appearance appearance = CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED, shaderPath, texturePath, "", "", "");
                appearance.BlendingMode = CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA;

                // remove material if we're using vertex coloring (eg color array is passed into Mesh.CreatePointSprite())
                //                appearance.RemoveMaterial();
                //                Keystone.Types.Color matColor = new Keystone.Types.Color(color);
                //                Material material = Material.Create(Repository.GetNewName(typeof(Material)), matColor, matColor, matColor, matColor);
                //                appearance.AddChild(material);
                model.AddChild(appearance);
            }

            if (!string.IsNullOrEmpty(relativePath))
            {
                string fullPath = System.IO.Path.Combine(CoreClient._Core.ModsPath, relativePath);
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fullPath));
                mesh.SaveTVResource(fullPath);
            }

            model.AddChild(mesh);
            field.AddChild(model);

            // load a field script that will get called every frame
            // similar to a star lens flare

            return field;
        }


        // major missiles/trails/billboards/fx vid xna
        //http://www.youtube.com/watch?v=ihCbCgS7nTU&feature=related
        public static void MoveMotionField(ModeledEntity field, Mesh3d mesh, Vector3d cameraPosition, Vector3d lastCameraPosition, Vector3d forward, double elapsedSeconds)
        {
            if (mesh == null) return;
            // NOTE: if mMotionField is NOT a Background3D attached to main scene
            //       then it will not get rendered properly.  But if we make it a ModeledEntity
            //       then it doesnt get placed at camera automatically so we do it here.
            field.Translation =  cameraPosition; 

            // TODO: I think the motion field should just moves at a fixed rate regardless of the speed of the vehicle or camera
            //       or perhaps we can scale the speed of the movement within reason. at some point there has to be a max speed
            //       but the position of the particles should be irrelevant in terms of speed of the camera or vehicle
            double MAX_VISIBLE_RANGE = 250d;
            double FIXED_SPEED = 40;
            double speed;
            Vector3d delta = Vector3d.Normalize(forward, out speed);

            // todo: we need to scale the speed to a range based on the actual length of the un-normalized forward vector
            //speed = Math.Min(speed, FIXED_SPEED);
            delta *= (FIXED_SPEED * elapsedSeconds);

            double distanceTraveled = delta.Length;
            
            if (distanceTraveled == 0) 
            {
            	field.Enable = false;
        		return;
            }

            field.Visible = true;
            field.Enable = true;
            
            Vector3d[] data = null;
            Vector3d[] normals = null;
            if (field.UserTypeID == (uint)MOTION_FIELD_TYPE.QUAD)
            {
                mesh.GetVertices(out data, out normals);
                //TV_SVERTEX[] verts = mesh.GetVertices();
            }
            else
                data = mesh.GetVertexPositions();

            // we don't want to draw motion field in front of camera if we are no longer moving at all
            // TODO: in the future maybe we'll switch to rendering of a dust field who's particles
            // move slightly even after the camera is stopped
            // rather than relying on camera to provide all of the "animation." 
            if (delta.IsNullOrEmpty())
            {
                for (int i = 0; i < data.Length; i++)
                {
                	// put it behind the camera
                	// if i calc bbox here, ill never see the field
                	// because i dont trigger dirty box upon moving these vertices..
                	// and i dont know i want to have to calc bbox everytime...
                    data[i] = -forward;
                }
            }
            else
            {
                int step = 1;
                if (field.UserTypeID == (uint)MOTION_FIELD_TYPE.LINE)
                    step = 2;
                else if (field.UserTypeID == (uint)MOTION_FIELD_TYPE.QUAD)
                    step = 4;

            	// G:\Games\_space sims\procUniverse_xna\Reflector_Src\ProceduralUniverse\MotionField.cs
                // we're still moving so update the field
                for (int i = 0; i < data.Length; i+= step)
                {
                    data[i] -= delta;
                    if (step == 2)
                        data[i + 1] -= delta;
                    else if (step == 4)
                    {
                        // v1 - position => start, normal => end
                        normals[i] -= delta;

                        // v2 = position => end, normal => start
                        data[i + 1] = normals[i];
                        normals[i + 1] = data[i];

                        // v3 - position => start, normal => end
                        data[i + 2] = data[i];
                        normals[i + 2] = normals[i];

                        // v4 = position => end, normal => start
                        data[i + 3] = normals[i];
                        normals[i + 3] = data[i];
                    }
                    Vector3d direction = data[i]; 
                    // direction.Normalize() both Normalizes direction and returns length
                    double distance = direction.Normalize();

                    // if the particle is no longer in front of the camera, move it out to the front 
                    // and it will behave as a new particle
                    // TODO: if we are facing one way but moving sideways, then we still just end up
                    //       putting new stars in front of camera which looks weird.  We need to have
                    //       in cases where the cameraForward does not match the direction of camera delta
                    //       that more stars are created at just beyond direction of travel.  Indeed if our
                    //       FORWARD_SPAWN_DISTANCE = 260f and our REVERSE_SPAWN_DISTANCE = -40f then
                    //       our other difference ratios of look vs movement direction should be a
                    //       percentage of that range.
                    if (Vector3d.DotProduct(direction, forward) < 0d)
                    {
                        Vector3d randomPosition = Utilities.RandomHelper.RandomUnitSphere(); // Random returns normalized random vector
                        double num3 = 32f + (204.8f * Utilities.RandomHelper.RandomFloat());
                        //data[i] = Utilities.RandomHelper.RandomUnitSphere() * 250d; // MAX_VISIBLE_RANGE;
                        data[i] = (forward * MAX_VISIBLE_RANGE) + (randomPosition * num3);
                        data[i] = (forward + randomPosition) * (MAX_VISIBLE_RANGE * Utilities.RandomHelper.RandomFloat());
                        if (step == 2)
                            data[i + 1] = data[i] + (forward * 10);
                        else if (step == 4)
                        {
                            // v1 - position => start, normal => end
                            normals[i] = data[i] + (forward * 10);

                            // v2 = position => end, normal => start
                            data[i + 1] = normals[i];
                            normals[i + 1] = data[i];

                            // v3 - position => start, normal => end
                            data[i + 2] = data[i];
                            normals[i + 2] = normals[i];

                            // v4 = position => end, normal => start
                            data[i + 3] = normals[i];
                            normals[i + 3] = data[i];
                        }
                    }
                    // if the particle is still in front of the camera BUT beyond visible range
                    // (eg if we're moving backwards) then create a particle behind the direction we're facing
                    // so particles are still spawning in the direction we're headed
                    else if (distance > MAX_VISIBLE_RANGE)
                    {
                        Vector3d randomPosition = Utilities.RandomHelper.RandomUnitSphere(); // Random returns normalized random vector
                        // TODO: 32f + (32f... what is that? is that narrowing the field of view that they are generated?)
                        double num4 = 32 + (32 * Utilities.RandomHelper.RandomFloat());
                        data[i] =  (forward * -40) + (randomPosition * num4);
                        if (step == 2)
                            data[i + 1] = data[i] + (forward * -10);
                        else if (step == 4)
                        {
                            // v1 - position => start, normal => end
                            normals[i] = data[i] + (forward * -10);

                            // v2 = position => end, normal => start
                            data[i + 1] = normals[i];
                            normals[i + 1] = data[i];

                            // v3 - position => start, normal => end
                            data[i + 2] = data[i];
                            normals[i + 2] = normals[i];

                            // v4 = position => end, normal => start
                            data[i + 3] = normals[i];
                            normals[i + 3] = data[i];
                        }
                    }
                }
            }


            // NOTE: If call to .SetVertices does not internally use .GetVertex to 
            //       retreive and cache the previous vertex color, it will set a color of 0
            //       making the field invisible. 
            if (field.UserTypeID == (uint)MOTION_FIELD_TYPE.QUAD)
                mesh.SetVertices(data, normals, null);
            else
                mesh.SetVertices(data, null, null);
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="texturePath"></param>
        /// <param name="starCount"></param>
        /// <param name="radius"></param>
        /// <param name="spriteSize">
        /// The sprite texture size is based on the radius of the field so that the resulting 
        /// star textures will take up just the right number of pixels on screen (aka just 
        /// right amount of screenspace) however, since we can lay down multiple seperate fields, 
        /// some small variation between stars which are supposed to be closer having larger 
        /// texture sizes and those that are far away have slightly smaller sizes than the norm
        /// </param>
        /// <returns></returns>
        public static Entity CreateRandomStarField(string id, string[] texturePath, float radius, int[] starCount, float[] spriteSize, int[] starColors)
        {
        	// NOTE: Fog Enable = true will cause individual star billboards to appear white
            System.Diagnostics.Debug.Assert(starCount.Length == spriteSize.Length && spriteSize.Length == starColors.Length);
            int layersCount = starCount.Length;

            Background3D field = new Background3D(id);
            field.Dynamic = false;

            int starTex = -1;
            string name = "";

            Elements.ModelSelector sequence = null;
            if (layersCount > 0)
            {
                // we must add multiple models to a sequence node first
                sequence = new Elements.ModelSequence(Repository.GetNewName(typeof(Elements.ModelSequence)));
                field.AddChild(sequence);
            }

            // We can add multiple Models each with their own Mesh3d so that our Field can have
            // layers of different types of stars with different Appearances.
            for (int i =0 ; i < layersCount; i++)
            {

                if (!string.IsNullOrEmpty(texturePath[i]))
                	starTex = CoreClient._CoreClient.TextureFactory.LoadTexture(System.IO.Path.Combine (Core.FullNodePath(texturePath[i])), texturePath[i]);
                else
                    starTex = -1;

                Model model = new Model(Repository.GetNewName(typeof(Model)));
                if (starTex >= 0)
                {
                    Appearance.Appearance appearance = CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED, "tvdefault", texturePath[i], null, null, null); 
                    appearance.BlendingMode = CONST_TV_BLENDINGMODE.TV_BLEND_ADD;
                    // remove material if we're using vertex coloring (eg color array is passed into Mesh.CreatePointSprite())
                    appearance.RemoveMaterial();
                    //id = Repository.GetNewName(typeof(Appearance.Material));
                    //Keystone.Types.Color color = new Keystone.Types.Color(starColors[i]);
                    //Material material = Material.Create(id, color, color, color, color);
                    //appearance.AddChild(material);
                    model.AddChild(appearance);
                }


                Vector3d[] points = Utilities.RandomHelper.RandomSphericalPoints(starCount[i], radius);

                // NOTE: HACK: the following random colors generation may seem confusing since we pass in
                // var "starColors" but the problem is, here we pass in 3 LAYERS of starfields
                // so our starColors array is only length 3 it does not contains as many colors
                // as starPoints!  so we'd either have to make a jagged array or have it contain
                // in a single array, all colors for all 3 layers.
                int[] randomColors = new int[points.Length];
                for (int j = 0; j < randomColors.Length; j++)
                    randomColors[j] = Utilities.RandomHelper.RandomColor().ToInt32();
                
               
                string relativePath =  string.Format ("caesar\\meshes\\pointsprites\\starfield{0}.tvm", i.ToString());
                Mesh3d mesh = Mesh3d.CreatePointSprite(relativePath, points, randomColors, starTex >= 0, spriteSize[i]);
                string fullPath = System.IO.Path.Combine(CoreClient._Core.ModsPath, relativePath);
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fullPath));
                mesh.SaveTVResource(fullPath);

                // TODO: we need to save this .tvm to file and use its resourcePath when re-loading the scene that contains it
                mesh.Name = "starfield_" + Repository.GetNewName(typeof(Mesh3d));

                model.AddChild(mesh);
                if (sequence != null)
                {
                	if (i > 0)
                		model.Pickable = false;
                	
                    sequence.AddChild(model);
                }
                else
                    field.AddChild(model);
            }
            
            return field;
        }
                


        // TODO: how about potentially creating a "Digest" entity
        // and then creating it during galaxy generation
        // and storing custom digest properties
        // then have some methods where proxy entities can be retrieved
        // from a Digest such that in gameplay matters, a digest is treated as a collection
        // of entities, but for visibility, picking, rendering, its treated as a single
        // entity.  We will insert it onto the Root.  
        // And allow for some elements to be disabled/enabled and in terms of pointsprite mesh
        // we can move disabled vertices to double.MaxValue 
        public static Entity CreateStarAtlas(Keystone.Celestial.Star[] stars, double viewDepth)
        {
            if (stars == null || stars.Length == 0) throw new ArgumentOutOfRangeException();

            ModeledEntity atlas = new ModeledEntity("nav_workspace_galaxy");
            //atlas.Pickable = false;
            // currently, a entity should only be of one type (eg exist in one layer)
            atlas.SetEntityAttributesValue((uint)EntityAttributes.AllEntityAttributes, false);
            atlas.SetEntityAttributesValue((uint)EntityAttributes.HUD, true);
            atlas.Dynamic = false;

            float spriteSize = 0.1f;
            int[] randomColors = new int[stars.Length];

            for (int i = 0; i < randomColors.Length; i++)
                randomColors[i] = Utilities.RandomHelper.RandomColor().ToInt32();

            string texturePath = @"caesar\Shaders\Planet\stardx7.png";

            Model model = new Model(Repository.GetNewName(typeof(Model)));

            Appearance.Appearance appearance = CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_NONE, "tvdefault", texturePath, null, null, null);
            appearance.BlendingMode = CONST_TV_BLENDINGMODE.TV_BLEND_ADD;
            // we dont want material if using vertex coloring
            appearance.RemoveMaterial(); 
            model.AddChild(appearance);
            

            // TODO: stars[0] may be null if it's not paged in.  This is why I think
            // this sort of size data needs to be passed in here not computed here
            //Portals.ZoneRoot root = (Portals.ZoneRoot)stars[0].Region.Region;
            //Vector3d size;
            //size.x = root.RegionDiameterX;
            //size.y = root.RegionDiameterY;
            //size.z = root.RegionDiameterZ;

            //Vector3d zoneCount;
            //zoneCount.x = root.RegionsAcross;
            //zoneCount.y = root.RegionsHigh;
            //zoneCount.z = root.RegionsDeep;

            //// this gives us complete dimensions of the universe
            //size *= zoneCount;

                        

            double positionScale = 1d / viewDepth; // / size.z;

            Vector3d[] points = new Vector3d[stars.Length];
            for (int i = 0; i < stars.Length; i++)
            {
                // in order to scale the star positions and pointsprite sclaes so that 
                // 1) they all fit on screen at once
                // 2) are of accurate scale sizes relative to one another
                //
                //
                // the dimensions of each region
                // the total dimensions of universe
                // a scale factor by 
                // the regional offset for each region
                // 
                // When we recurse and instance our zones, stellar systems, stars, worlds
                // to create our navigation digest entities, we never actually fully load those
                // entities such that we rebuild the hierarchy.  In fact we explicitly avoid 
                // having to recreate the parent/child relationships.  However this means attempting to
                // read the .Region of a star here will fail.
                Portals.Zone zone =  (Portals.Zone)stars[i].Region;
                //points[i].x = zone.Offset[0] * 1000;
                //points[i].y = zone.Offset[1] * 1000;
                //points[i].z = zone.Offset[2] * 1000;

                // TODO: the star's position and zone offset and zone size data
                // should all be passed in as args to this function so we can avoid these sorts
                // of issues where these entities are not paged in and we only intended to read
                // them from xmldb temporarily to get the info we needed.
                // TODO: then we should automatically scale our galaxy view to LY.
                //
                // a star's translation is always with respect to it's current StellarSystem and not
                // the Zone.  Further we'll scale the positions down by same amount so all are visible on screen at once
                points[i] = (stars[i].Translation + zone.ZoneTranslation) * positionScale; // TODO: instead of + zone.ZoneTranslation wouldnt stars[i].GlobalTranslation work?
                System.Diagnostics.Debug.WriteLine("Star translation = " + stars[i].Translation.ToString());

                //points[i].x *= zone.Offset[0];
                //points[i].y *= zone.Offset[1];
                //points[i].z *= zone.Offset[2]; 
            }
            // 0) it could be that a single tvmesh will not due because of issues
            //    with the draw order of the vertices.  Or maybe i can disable depth testing
            //    so we always blend
            // 1) get proper locations for each star in diget
            // 2) get proper color to work based on the star's temperature
            // 3) get proper scale of the star to work
            // 4) Create digest Entity
            //      - can we associate custom info for the digest so it's always accessible immediately?
            //        eg so we can in an array that corresponds to each vertex, store a hashtable of data
            //        by some custom property name for each?  eg PropertySpecs?
            // 5) create a better grid or add a tvmesh circle grid

            // 6) lines for stars above / below grid
            //
            // TODO: how do we associate lables and an Entity ID with each pointsprite so that when picking
            //       we can fill in the appropriate EntityID when a Digest/Atlas entity is the underlying
            //       node.  I think the only real way is to create a special DigestEntity
            //       that can contain arrays of the names for each vertex and which can be used
            //       in the PickResult to refer to the appropriate underlying entities so we can
            //       instance that entity if does not exist in the Repository or query the server
            //       for info about it if it does not exist in the xmldb too (eg has not been
            //       surveyed)
            string relativePath = "caesar\\meshes\\pointsprites\\staratlas.tvm";
            Mesh3d mesh = Mesh3d.CreatePointSprite(relativePath, points, randomColors, true, spriteSize);
            string fullPath = System.IO.Path.Combine(CoreClient._Core.ModsPath, relativePath);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fullPath));
            mesh.SaveTVResource(fullPath);

            
            mesh.Name = "nav_workspace_galaxy_stars" + Repository.GetNewName(typeof(Mesh3d));

            model.AddChild(mesh);
            atlas.AddChild(model);

            return atlas;
        }

        public static Entity CreateStarAtlas(string[] starIDs, double viewDepth)
        {
            if (starIDs == null || starIDs.Length == 0) throw new ArgumentOutOfRangeException();

            Celestial.Star[] stars = new Celestial.Star[starIDs.Length];

            for (int i = 0; i < stars.Length; i++)
                stars[i] = (Celestial.Star)Repository.Get(starIDs[i]);

            return CreateStarAtlas(stars, viewDepth);
        }

        public static Entity CreateStarAtlas(string[] starIDs, Vector3d[] positions, double[] radii, int[] color)
        {
            return null;
            
            // during world generation, do we create an atlas by creating an atlas entity
            // taht will be a starfield type entity?


            // start with proper atlas based on all stars and planets 


            // itterate add name, zone offset, and position to each star and world / moon added to the atlas

            // get normalized dir vector to each star and world

            // get vertex point on starfield sphere by multiplying by radius
            Vector3d cameraPosition = Vector3d.Zero();
            Vector3d dir;

            // update vertex position


            // add several random fields for far away systems that are backdrop only
            // and non pickable.

        }

        //public static Vector3d[] CreateElipsePoints(int numSegments)
        //{
            /// <summary>/// Creates an ellipse starting from 0, 0 with the given width and height.
            /// Vectors are generated using the parametric equation of an ellipse.
            /// </summary>/// <param name="semimajor_axis">The width of the ellipse at its center.</param>
            /// <param name="semiminor_axis">The height of the ellipse at its center.</param>
            /// <param name="angle_offset">The counterlockwise rotation in radians.</param>
            /// <param name="sides">The number of sides on the ellipse (a higher value yields more resolution).</param>
        //}

        

        //public static Portals.Region GeneratePlanetoidBelt( BoundingBox fieldDimensions, int count, uint octreeDepth)
        //{
        //        // then we have to itterate through each one in a circle using MoveAroundPoint
        //        // todo; But the most difficult thing is not having these asteroids intersect!
        //       //Vector3d position = Utilities.MathHelper.MoveAroundPoint(origin, radius, horizontalAngle, verticleAngle);
        //        // 
        //        // all of these "asteroids" will have no atmosphere and will be rockyball or icyrockball
        //        // radius can require some simple random generation, but density and mass and gravity can
        //        // then use typical calcs

        //        // once we have a specific Planetoid position plotted, we should recall this function recursively
        //        // and using a new worldType.Planetoid (as opposed to PlanetoidBelt)


        //        // TODO: for asteroids, first we'll create an octree region to contain this asteroid belt
        //        // 
        //        // now in Infinity, he claims to procedurally positions asteroids and that they remain unique each time they are
        //        // recreated.  Im not sure that this is a good idea if you start with the basis that this is multiplayer.  Why?  because
        //        // in our game, asteroids do orbit and i'd rather have a hundreds to low thousands of them speckled in an octree
        //        // that are sent in an array and which are individual entities, and then to totally just debris field fudge the rest
        //        // 
        //        // Issue - would this be too sparse?  Well... i think it should be as sparse as it needs to be because _if_ we want
        //        // asteroids to be useful in physics calcs, sending them on trajectories to hit other planets and such things...
        //        // then there just can't be that many really large ones.  So let's figure this... if the asteroid is >100 meters
        //        // it's unique and tracked.  And we'll have a fixed limit of say 1000 in the 100 - 1000 meter range, and then 100 in the
        //        // 1000 - 10000 meter range.... and the rest we totally fudge as just decorations.  We can automatically still
        //        // determine where a ship is in a field server side and start applying damage if they are in a really dense section
        //        // and moving too fast.

        //        //
        //        // Can we rotate an entire octree to simulate orbiting of asteroids?
        //        // I dont think that buys us anything because the octree wouldn't be axis aligned and so we'd need
        //        // object oriented bounding octree
        //        // WAIT:  We would just need to orient the frustum to the octree's rotation instead.
        //        // so it could work.
        //
        //        // ugh because my code still has the problem of the few asteroids i actually create that arent fake, of
        //        // having to move within the octree.  I want to avoid that.  I guess initially i'll just have
        //        // my regular grid octree and have moving items check their bounds and update themselves dyanmically.

        //        Random rand = new Random();

        //        Portals.Region octreeRegion = new Keystone.Portals.Region(Resource.Repository.GetNewName(typeof(Portals.Region)), fieldDimensions, false, octreeDepth);
        //        Appearance.DefaultAppearance app;
        //        string asteroidPath = @"E:\dev\c#\KeystoneGameBlocks\Data\pool\meshes\wavefront_obj\asteroid_small_1.obj";
        //        string name = Resource.Repository.GetNewName(typeof(Minimesh));
        //        Minimesh mini = Minimesh.Create(name, asteroidPath, true, true, out app);
        //        mini.MaxInstancesCount = (uint)count;
        //        // TODO: more initial options need to be set
        //        //_resource.SetAlphaTest(true, 200, true);
        //        //_resource.SetFadeOptions(true, 3000, 2000);
        //        ((FX.FXInstanceRenderer)Core._Core.SceneManager.FXProviders[(int)FX.FX_SEMANTICS.FX_INSTANCER]).AddMinimesh(mini);

        //        for (int i = 0; i < mini.MaxInstancesCount; i++)
        //        {
        //            // place a seperate new entity into the world
        //            StaticEntity planetoid = new StaticEntity(Resource.Repository.GetNewName(typeof(StaticEntity)), mini.Model);
        //            planetoid.Enable = true;
        //            planetoid.Translation = Utilities.MathHelper.RandomVector(rand, fieldDimensions.Min, fieldDimensions.Max); 

        //            double d = rand.NextDouble();
        //            // produce some scale variance
        //            double sx = 1 + 1 * d;
        //            double sy = 1 * d;
        //            double sz = 1 + 1 * d;

        //            planetoid.Scale = new Vector3d(sx, sy, sz);
        //            planetoid.Rotation = Utilities.MathHelper.RandomRotation(rand);
        //            octreeRegion.AddChild(planetoid);
        //        }

        //        return octreeRegion;
        //}

        ////// for now we just use this to create a random field of asteroids around player
        ////// but eventually importing a minimesh either starts with 0 elements and user must click it
        ////// and modify it, or perhaps creating a minimesh is more like creating an FXMinimesh where
        ////// future meshes of that type will subscribe and use for rendering.
        ////// So what this temp hack should do perhaps is add regular Entities and subscribe them to FXMinimesh
        ////// and then allow that FX to handle the rendering for visible Entities.


        ////    //(ByVal tvland As MTV3D65.TVLandscape, ByVal meshcount As Int32, ByRef position() As MTV3D65.TV_3DVECTOR, Optional ByVal sx As Single = 1, Optional ByVal sy As Single = 1, Optional ByVal sz As Single = 1) As MTV3D65.TVMiniMesh

        ////    _positions = new Vector3d[elementCount];
        ////    _rotations = new Vector3d[elementCount];
        ////    _scales = new Vector3d[elementCount];

        ////    // note: in a previous version of this function i allowed for the option of an existing position array to be passed in
        ////    // in which case those position values would be used
        ////    for (int i = 0; i < elementCount; i++)
        ////    {
        ////        _rotations[i] = Utilities.MathHelper.RandomRotation(_random);
        ////        _positions[i] = Utilities.MathHelper.RandomVector(_random, area.Min, area.Max);
        ////        //position[i].y = tvland.GetHeight(position(i).x, position(i).z);

        ////        //tvmini.SetPosition(position(i).x, position(i).y, position(i).z, i);
        ////        //dummy.SetPosition(position(i).x, position(i).y, position(i).z);
        ////        //tvmini.SetRotation(1, 1, 1, i); // rndm.Next(0, 360), 1, i) //<-- rotation only effects billboard with BillBoardType <> rotating

        ////        double d = _random.NextDouble();
        ////        // produce some scale variance
        ////        double sx = 1 + 1 * d;
        ////        double sy = 1 * d;
        ////        double sz = 1 + 1 * d;
        ////        _scales[i] = new Vector3d(sx, sy, sz);
        ////        // dummy is used as an easy way to grab a final matrix since you can't grab a matrix from an individual minimesh element
        ////        //dummy.SetScale(sx, sy, sz);
        ////        //tvmini.SetMatrix(dummy.GetMatrix, i);

        ////    }
        ////}


        /// http://en.wikipedia.org/wiki/Asteroid_belt
        /// In Sol System's main belt, more than half the mass of the main belt is contained in the four largest
        /// objects: Ceres, 4 Vesta, 2 Pallas, and 10 Hygiea. These have mean diameters of more than 400 km, 
        /// while Ceres, the main belt's only dwarf planet, is about 950 km in diameter.[1][2][3][4] 
        /// The remaining bodies range down to the size of a dust particle. The asteroid material is so thinly 
        /// distributed that multiple unmanned spacecraft have traversed it without incident.  
        ///
        /// Journal of Infinity dev's asteroid notes
        /// http://www.gamedev.net/community/forums/mod/journal/journal.asp?jn=263350&cmonth=1&cyear=2008&cday=31
        /// also July.2007  http://www.gamedev.net/community/forums/mod/journal/journal.asp?jn=263350&cmonth=7&cyear=2007
        public static void InitAsteroidField(Entity field, float fieldRadius, int count)
        {
            
                //// determine how many tiny, small, medium, large, enormous asteroids this field will have
                //int tinyCount;
                //int smallCount;
                //int mediumCount;
                //int largeCount;
                //int enormousCount;
                //Vector3d origin = worldInfo.ParentBody.Translation;

                //// then itterate and create the entities
                //BoundingBox fieldDimensions = new BoundingBox(origin, 100000);
                //Portals.Region octreeRegion = ProceduralHelper.GeneratePlanetoidBelt( fieldDimensions, 100, (uint)8);
                
                //// and then add the new region to the scene
                //StellarSystem system = null;
                //Entities.EntityBase parent = worldInfo.ParentBody;
                //while (system == null)
                //{
                //    if (parent is StellarSystem) system = (StellarSystem)parent;
                //    parent = parent.Parent;
                //}

                //system.AddChild(octreeRegion);
                // I believe the above is more correct (adding to system) than below
                // where we attemt to just add the planetoid to the main sector Region and 
                // dont even bother with having this planetoid orbit anything.

                ////worldInfo.ParentBody.Parent.Parent.AddChild(octreeRegion);
                //// w.Region.AddChild(octreeRegion);

            // hierarchically speaking, a planetoid field is relative to either a planet, a star
            // or a star system...  
            // note; an octreeRegion is just a normal Region that has "octreePartitioned=true" set
            // and then all children added to it get OctreeSceneNode's instead of regular SceneNodes.
            // Region octreeRegion = new Region(Resource.Repository.GetNewName(typeof(Portals.Region)), fieldDimensions, false, octreeDepth);
            bool isRegion = field is Portals.Region;

            Vector3d origin = field.Translation;

            double radius = fieldRadius;  // for non region fields, we dont need a fixed radius as the entity will auto size
            if (isRegion)
                radius = field.BoundingBox.Radius * .5d;
            double outerRadius = radius;// 12756200;  // the radius of the planet to which this field will orbit
            double innerRadius = outerRadius * .85d; // 750000;
            double width = outerRadius - innerRadius; // 1620; // 16250;

            // first we should load an array of asteroid meshes
            // add each to our Instancer
            // then as we randomly place asteroids, select a random one.  Then select the scale and
            // select the resolution of the texture based on that scale.
            // TODO: we should also generate lod's for some of the really detailed asteroid meshes.
                        
            string path = @"caesar\meshes\asteroids\MajorAsteroids_1.3.1\tvm\";
            //string asteroidPath = path + "kleopatra.tvm"; 
            //path = @"E:\dev\c#\KeystoneGameBlocks\Data\pool\meshes\wavefront_obj\";
            //string asteroidPath = path + "asteroid_small_1.obj";
            //path = @"E:\dev\c#\KeystoneGameBlocks\Data\pool\meshes\Misc\";
            //string asteroidPath = path + "pawn.tvm";

            path = @"caesar\meshes\rocketcommander\";
            string asteroidPath = path + "asteroid1Low.tvm"; 
            string name = Keystone.Resource.Repository.GetNewName(typeof(Mesh3d));
            
            //Mesh3d temp = Mesh3d.Create ("00", path + "eros.obj" , false, false, out app);
            //temp = Mesh3d.Create("000", path + "bacchus.obj", false, false, out app);
            //temp = Mesh3d.Create("01", path + "castalia.obj", false, false, out app);
            //temp = Mesh3d.Create("02", path + "ceres.obj", false, false, out app);
            //temp = Mesh3d.Create("03", path + "dactyl.obj", false, false, out app);
            //temp = Mesh3d.Create("04", path + "geographos.obj", false, false, out app);
            //temp = Mesh3d.Create("05", path + "golevka.obj", false, false, out app);
            //temp = Mesh3d.Create("06", path + "ida.obj", false, false, out app);
            //temp = Mesh3d.Create("07", path + "juno.obj", false, false, out app);
            //temp = Mesh3d.Create("08", path + "kleopatra.obj", false, false, out app);
            //temp = Mesh3d.Create("09", path + "ky26.obj", false, false, out app);
            //temp = Mesh3d.Create("10", path + "mathilde.obj", false, false, out app);
            //temp = Mesh3d.Create("11", path + "pallas.obj", false, false, out app);
            //temp = Mesh3d.Create("12", path + "toutatis.obj", false, false, out app);
            //temp = Mesh3d.Create("13", path + "vesta.obj", false, false, out app);

           // TODO: when creating a Mesh3d that already exists, the appearance will not be set.
            // however we should attempt to grab the previous appearance but how considering
            // the mini does not retain app reference... it's model does however, but it may have
            // multiple parent models with different appearances.... appearances are not shared though..
            // so we should re-create the appearance too.  That is the proper answer
            DefaultAppearance app = Mesh3d.GetAppearance (asteroidPath);
            Mesh3d mesh = (Mesh3d)Repository.Create(asteroidPath, "Mesh3d");
            mesh.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL;

       //     mini.MaxInstancesCount = (uint)count;
            // TODO: more initial options need to be set
            //_resource.SetAlphaTest(true, 200, true);
            //_resource.SetFadeOptions(true, 3000, 2000);
          //  ((FXInstanceRenderer)((Keystone.Scene.ClientSceneManager)Core._Core.SceneManager).FXProviders[(int)FX_SEMANTICS.FX_INSTANCER]).AddMinimesh(mini);

            int skipCount = 0;
            for (int i = 0; i < count; i++)
            {
                // place a seperate new entity into the world.  Note the minimesh.Model which
                // is an InstancedModel is passed to the StaticEntity so that minimesh rendering
                // will be used.
                ModeledEntity planetoid = new ModeledEntity(Keystone.Resource.Repository.GetNewName(typeof(ModeledEntity)));
                planetoid.Dynamic = false; // will follow eliptical path or perhaps just a rotation of the minimesh root
                planetoid.Enable = true;
                Model model = new Model(Repository.GetNewName(typeof(Model)));
                model.AddChild(mesh);
                model.UseInstancing = true;
                model.AddChild(app);


                
                double x = Utilities.RandomHelper.RandomDouble() * Math.PI * 2 - Math.PI;
                x = Math.Tan(x) * width;
                Vector3d position = Vector3d.Forward() * (innerRadius + x);
                double angle1 = Utilities.RandomHelper.RandomDouble() * Math.PI * 2;
                position = Vector3d.TransformCoord(position, Matrix.CreateRotationY(angle1));

                // TODO: taper out astroid size 

                double z = Utilities.RandomHelper.RandomDouble() * Math.PI * 2 - Math.PI;
                z = Math.Tan(z) * width;
                position += Vector3d.Up() * z;
                //position.y += origin.y;

                //position += origin; // the positions are relative to the field!  We do not need origin added

                planetoid.AddChild(model);
                planetoid.Translation = position;

                // TODO: the bounding (picking?) issue with my minimeshes is caused by the rotation i think      
                planetoid.Rotation = Utilities.RandomHelper.RandomQuaternion();
                Vector3d scale;

                // uniform scaling
                scale.x = scale.y = scale.z = Utilities.RandomHelper.RandomNumber(0.01, .2);

                // non uniform scaling
                //scale.x = Utilities.MathHelper.RandomNumber(rnd, 0.01, 1);
                //scale.y = Utilities.MathHelper.RandomNumber(rnd, 0.01, 1);
                //scale.z = Utilities.MathHelper.RandomNumber(rnd, 0.01, 1);
                               
                planetoid.Scale = scale; 
                
                //planetoid.AstroidNumber = (int)rnd.NextDouble() * 3;
                //planetoid.Origin = position;

                // TODO: CAN WE IMPROVE FXINSTANCERENDER to accept an animation that will animate
                // all of these minimesh elements for us?  If we store within the minimesh renderer
                // every rotation axis and rotation speed, then we can in fact animate the visible ones
                // ourselves which might be faster and more efficient than seperate animation.cs for
                // each asteroid
                //planetoid.RotationSpeed = new Vector3d((rnd.NextDouble() * 2 - 1) / Scale.X / 30, (rnd.NextDouble() * 2 - 1) / Scale.X / 30, (rnd.NextDouble() * 2 - 1) / Scale.X / 30); 
                //    Debug.Assert(field.BoundingBox.Contains(position ), "InitAsteroidField() - Must not attempt to insert that is outside of the bounding volume of the Octree.");
                if (!isRegion)
                    field.AddChild(planetoid);
                else if (field.BoundingBox.Contains(position )) // we do need to test transformed if we use field.BoundingBox.Contains even though the asteroids are relative when inserted
                    field.AddChild(planetoid);
                else
                {
                    skipCount++;
                    Debug.Write("ProceduralHelper.InitAsteroidField() - Skipping adding Asteroid index '" + i.ToString() + "' Entity.  Must not attempt to insert that is outside of the bounding volume of the Octree.");
                    
                    Debug.WriteLine("  " + skipCount + " asteroids skipped so far...");
                }
            }
        }

        

        public static void CreateGasField()
        {
            // a gas nebula type field can be created just like our motion field
            // where we spawn the gas cloud values in front of the ship as it's traveling
            // in fact the only difference is we use a minimesh system instead of point sprites
            // right?  or maybe point sprites could work... i dont know.
        }
        
        /// <summary>
		/// Minimesh Billboards
		/// </summary>
		/// <param name="maxMinimeshCount"></param>
		/// <param name="model"></param>
		/// <returns></returns>
		public static Model CreateMinimeshBillboard(string billboardTexturePath, uint initialMinimeshCapacity, CONST_TV_BLENDINGMODE blendMode)
		{
			Model model = new Model(Repository.GetNewName(typeof(Model)));
			model.CastShadow = false;
			model.ReceiveShadow = false;
			
            Appearance.Appearance appearance = CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL, null, billboardTexturePath, null, null, null);
            // TV_BLEND_ADD is correct so that clouds when rendering after atmosphere (which hosts nighttime starmap)
            // will allow starmap to be visible through clouds.
            appearance.BlendingMode = blendMode;  

            Diffuse layer = (Diffuse)appearance.Layers[0];
            layer.AlphaTestDepthWriteEnable = true; // could prevent some zfighting
            layer.AlphaTest = true;
            layer.AlphaTestRefValue = 128;
            
    		MinimeshGeometry mmGeometry = (MinimeshGeometry)Repository.Create (billboardTexturePath, "MinimeshGeometry");

    		// TODO: do we re-use minimesh if existing?  ideally no.  ideally the ParticleSystem manager should handle that.
    		if (mmGeometry == null)
    		{
    			// NOTE: the minimeshGeometry is unique for each Level and each Model within a Segment
    			mmGeometry = (MinimeshGeometry)Resource.Repository.Create ("MinimeshGeometry");
    			mmGeometry.AxialRotationEnable = false;
    			mmGeometry.SetProperty ("meshortexresource", typeof(string), billboardTexturePath);
	        	mmGeometry.SetProperty ("maxcount", typeof(uint), initialMinimeshCapacity);
	        	mmGeometry.SetProperty ("billboardwidth", typeof(float), 0.1f);
	        	mmGeometry.SetProperty ("billboardheight", typeof(float), 1.0f);
	        	mmGeometry.SetProperty ("isbillboard", typeof(bool), true);    		    
    		}

    		// the MaxInstancesCount starts small, but can be increased withing TVMinimesh without losing existing element data
    		if (mmGeometry.MaxInstancesCount < initialMinimeshCapacity)
	    		mmGeometry.MaxInstancesCount = initialMinimeshCapacity;
    		

    		model.AddChild (mmGeometry);
    		model.AddChild (appearance);
    		
			// in order to deserialize the tile layer data and restore the terrain minimesh elements, the minimesh geometry resource must be loaded
			Keystone.IO.PagerBase.LoadTVResource (mmGeometry, false);

			return model;
            
		}
		
        public static void InitiTVParticleSystem(Model model, string uniqueAnimationName, string uniqueTargetName, float durationInSeconds)
        {
        	// Geonordo's volumetric fog using billboard particles
        	// http://www.youtube.com/watch?v=C7zs8GTuyMs
        
        	// Keystone.Celestial.Nebula
        	//
        
        	// consider the types of particle systems we'll mostly need
            // - explosion
            // - engine exhaust
            // - potentially a turret muzzle blast
            // - sparks indoors
            // - fire indoors and potentially exterior of ships
            // - re-entry particles around ship, craft, asteroid
            // 
            // So what is a "system"  
            //  - i think a system is a collection of particles to represent something like fire + smoke
            //  - to represent an explosion plus shock wave
            // 

            // So we can have a "model"  but no entity... except hrm... a "Instance" info type entity... 
            // As far as placing them in the scene, attaching them.... well... we do want that so we get the culling
            // and where we can set an "isVisible" to potentially ignore updating it and rendering it.  So that's required clearly.
            // I think that when we designed our Entity->Model  relationship we decided that an Entity was in affect
            // an "instance" container, something that can be scripted, etc....  So in this sense, it's an entity no?
            // 
            //
            // TODO: i dont think a particle system is an entity.  Im already noticing some issues with regards to
            // .Update() of our explosion.Model and ModelBase has no .Update() to override.  
            // (ALTHOUGH there is the ModelBase.Render(instance, ) which in turn calls the underlying Geometry's Render()
            //
            // A particle system
            // is more of an FX that i think we should be able to attach to things so their movement is updated
            // that we should be able to call/spawn easily from our scripts, and once spawned our FXParticle 
            // should manage the lifecycles of any spawned particles.
            //
            //
            // Indeed, also looking at Zak's bullet demo with refracted bullet trails... got me thinking.
            // It's true i dont need a dedicated "bullet" class since my entities are designed... however
            // the re-using of bullets by sharing them with new Entity instances is definetly what im all about
            // so what's the difference with ParticleSystems?  Only one... we use VisualFX api to handle some of these things
            // We do need to be clear though as far as explosions and damage stimuli being released and that sort of thing.
            // I mean i think in our script we would for instance play a sound, create the stimulus, create the visual.
            // 
            // But again, let's just take a exhaust plume particle system.... does this get attached to an engine nozzle?  
            // How do we update it's position otherwise?  How do we get the exact location to attach it relative to the 
            // engine nozzle?


          
            // TODO: we switch to a ParticleSysModel which can use either Absolute or Relative Position for Cull, Render (no picking of particle systems? what about edit mode?) 

            // AWESOME emitter!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // http://memo.tv/files/memotv/StarryMouse3D_0.swf
            // plus source code cache\downloads\StarryMouse3d.zip
            // - good for like simulating debris in both exhaust and in damaged areas
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            string diffuseTex = @"caesar\particles\Smokey.dds";
            Keystone.Appearance.Layer textureLayer = (Layer)Keystone.Resource.Repository.Create("Diffuse");
            Keystone.Appearance.Texture tex = (Texture)Keystone.Resource.Repository.Create(diffuseTex, "Texture");
            tex.TextureType = Texture.TEXTURETYPE.Default;
            textureLayer.AddChild(tex);

            Appearance.DefaultAppearance appearance =(DefaultAppearance)Repository.Create("DefaultAppearance");
            //appearance.AddChild(textureLayer);
            GroupAttribute ga = (GroupAttribute)Repository.Create("GroupAttribute");
            ga.AddChild(textureLayer);
            appearance.AddChild(ga);

            // todo: what happens when we clone a particleSystem and then want to edit some parameters independantly?  Changing visual properties should be ok, but not type(pointsprite vs billboard vs minimesh)\geometry and behavior
            string id = "caesar\\particles\\default.tvp"; 
            ParticleSystem ps = ParticleSystem.Create(id);

            //           ps.AddEmitter("test", CONST_TV_EMITTERTYPE.TV_EMITTER_BILLBOARD, 32);
            //           ps.AddAttractor(); // TODO: Temp hack to test attractors

            //            string fullPath = Core.FullNodePath(id);
            //            ps.SaveTVResource(fullPath);

            //Repository.IncrementRef(ps);
            //Repository.DecrementRef(ps);
            // ps = null;

            //ps = ParticleSystem.Create(id);
            //IO.PagerBase.LoadTVResource(ps, true);


            model.AddChild(ps);
            model.AddChild(appearance);           
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="animationParent"></param>
        /// <param name="model"></param>
        /// <param name="uniqueAnimationName">Each animation added to a specific Entity must have a unique name.</param>
        /// <param name="uniqueTargetName">Each instance of this explosion animation must have a unique "friendly name" for the TARGET if more than 
        /// one explosion model + animation pair is to exist under the same Entity.</param>
        /// <param name="durationInSeconds"></param>
        public static void InitExplosion(ModeledEntity animationParent, Model model, string uniqueAnimationName, string uniqueTargetName, float durationInSeconds)
        {
            Types.Color diffuseColor, ambient, specular, emissive;
            diffuseColor = Keystone.Types.Color.White; //  Keystone.Types.Color.Random();
            ambient = new Types.Color(1.0f, 1.0f, 0.0f, 1.0f);
            specular = new Types.Color(1.0f, 1.0f, 0.0f, 1.0f);
            emissive = new Types.Color(1.0f, 1.0f, 0.0f, 1.0f);
            // TODO: create a shared material here using a name based on Material.GetDefaultName ();
            Material emissiveMaterial = Material.Create(Repository.GetNewName(typeof(Material)),
                										diffuseColor, ambient, specular, emissive);

            // half the width or half height of a square billboard is not the radius.
            // the radius would be the hypotenus from center to corner.  Using Sqrt(8) and divide by 2
            // gives us the unit 
            float billboardUnitRadius = (float)Math.Sqrt(8) / 2f;

            string shareableBillboardID = Billboard.GetCreationString (CONST_TV_BILLBOARDTYPE.TV_BILLBOARD_FREEROTATION,
               														   true, billboardUnitRadius, billboardUnitRadius);
            Mesh3d mesh = (Billboard)Repository.Create (shareableBillboardID, "Billboard");

            string explosionSprites = @"caesar\textures\exp05_atlas.dds";
			string shaderPath = @"caesar\shaders\AnimatedTextured\AnimatedTexture.fx";
            Appearance.Appearance appearance = CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL, shaderPath, explosionSprites, "", "", "");
            appearance.BlendingMode = CONST_TV_BLENDINGMODE.TV_BLEND_ADD;  // TV_BLEND_ADD is correct so that starfield shines through
            
            appearance.Name = uniqueTargetName; // friendly name
            // use passed in material which is same as main star sphere
            appearance.RemoveMaterial();
            appearance.AddChild(emissiveMaterial);

            Settings.PropertySpec[] specs = new Settings.PropertySpec[1];
            specs[0] = new Settings.PropertySpec("textureUVInfo", typeof(Vector4).Name, Vector4.Zero());
            appearance.SetShaderParameterValues(specs);

            string textureAnimationClipID = Repository.GetNewName (typeof(Keystone.Animation.TextureAnimation));
            
            // add animated texture animation to Entity
            Keystone.Animation.TextureAnimation clip;
            object cachedAnimation = Repository.Get(textureAnimationClipID);
            if (cachedAnimation == null)
            {
                clip = Keystone.Animation.TextureAnimation.Create(textureAnimationClipID);
				clip.Duration = durationInSeconds; // seconds
				 // note uses friendly name not .ID, but this is why clip cannot be shared
				 // across different parents! the targetname is specific per instance
                clip.TargetName = appearance.Name;


                // add the keyframe rects
                float textureWidth = 4096;
                float textureHeight = 768;
                // UV Width and UV Height
                float width = 256f / textureWidth;
                float height = 256f / textureHeight;
                clip.DefineSpriteRectangle(new RectangleF(0, 0, width, height));
                clip.DefineSpriteRectangle(new RectangleF(256f / textureWidth, 0f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(512f / textureWidth, 0f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(768f / textureWidth, 0f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(1024f / textureWidth, 0f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(1280f / textureWidth, 0f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(1536f / textureWidth, 0f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(1792f / textureWidth, 0f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(2048f / textureWidth, 0f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(2304f / textureWidth, 0f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(2560f / textureWidth, 0f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(2816f / textureWidth, 0f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(3072f / textureWidth, 0f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(3328f / textureWidth, 0f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(3584f / textureWidth, 0f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(3840f / textureWidth, 0f / textureHeight, width, height));

                clip.DefineSpriteRectangle(new RectangleF(256f / textureWidth, 256f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(512f / textureWidth, 256f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(768f / textureWidth, 256f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(1024f / textureWidth, 256f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(1280f / textureWidth, 256f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(1536f / textureWidth, 256f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(1792f / textureWidth, 256f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(2048f / textureWidth, 256f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(2304f / textureWidth, 256f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(2560f / textureWidth, 256f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(2816f / textureWidth, 256f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(3072f / textureWidth, 256f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(3328f / textureWidth, 256f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(3584f / textureWidth, 256f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(3840f / textureWidth, 256f / textureHeight, width, height));

                clip.DefineSpriteRectangle(new RectangleF(256f / textureWidth, 512f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(512f / textureWidth, 512f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(768f / textureWidth, 512f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(1024f / textureWidth, 512f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(1280f / textureWidth, 512f / textureHeight, width, height));
                // below for ex5
                clip.DefineSpriteRectangle(new RectangleF(1536f / textureWidth, 512f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(1792f / textureWidth, 512f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(2048f / textureWidth, 512f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(2304f / textureWidth, 512f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(2560f / textureWidth, 512f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(2816f / textureWidth, 512f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(3072f / textureWidth, 512f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(3328f / textureWidth, 512f / textureHeight, width, height));
                clip.DefineSpriteRectangle(new RectangleF(3584f / textureWidth, 512f / textureHeight, width, height));

                // final clip is for empty texture.  
                clip.DefineSpriteRectangle(new RectangleF(0, 0, 0, 0));

                //anim.DefineSpriteRectangle(new RectangleF(3840f / textureWidth, 512f / textureHeight, width, height));
            }
            else
            {
            	throw new Exception ("Have we made AnimationClip.cs shareable yet?  We cannot have non-shareable target properties inside it.");
                clip = (Keystone.Animation.TextureAnimation)cachedAnimation;
            }
            string id = Repository.GetNewName(typeof(Keystone.Animation.Animation));
            Animation.Animation anim = new Keystone.Animation.Animation(id);
            anim.Looping = true;
            anim.Name = uniqueAnimationName;
            anim.AddChild(clip);
            
            
            model.AddChild(appearance);
            model.AddChild(mesh);
            
            // TODO: what if we want to add model under a ModelSelector
            //       does the "anim" still go under parent? I think so because
            //       being able to play any animation under an Entity should just be
            //       entity.Animations.Play(name) so long as all names are unique

            animationParent.AddChild(anim);

        }

        private static ModeledEntity CreateModeledEntity(string path, string id)
        {
            Keystone.Appearance.DefaultAppearance app = Mesh3d.GetAppearance (path);
            // TODO: all i need to do here is if we want textures and materials, is to load those in .LoadTVResource()
            //       and do Factor.Create() calls to share existing.   but question is, how do they get added
            //       to Model?  they could maybe retained by Mesh3d as the defaults and adding 
            ModeledEntity entity = new ModeledEntity(id); ;
            Model model = new Model(Repository.GetNewName(typeof(Model)));
			Keystone.Elements.Mesh3d resource = (Mesh3d)Repository.Create(path, "Mesh3d");

            if (app == null)
            {
                app = new Keystone.Appearance.DefaultAppearance(Keystone.Resource.Repository.GetNewName(typeof(Keystone.Appearance.DefaultAppearance)));
                app.AddChild(Keystone.Appearance.Material.Create(Keystone.Appearance.Material.DefaultMaterials.matte));
                // app.LightingMode = MTV3D65.CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL;
            }
            model.AddChild(resource);
            model.AddChild(app);
            entity.AddChild(model);
            return entity;
        }

        public static Appearance.Appearance CreateAppearance(CONST_TV_LIGHTINGMODE lightingMode, 
                                                             string shaderPath, 
                                                             string diffuseTex, 
                                                             string normalmapTex, 
                                                             string emissiveTex, 
                                                             string specularTex, 
                                                             bool createMaterial = false)
        {

        	// NOTE: by default, Repository.Create() will assign pssm.fx shader unless shaderPath argument
        	//       results in a change subsequently.
        	DefaultAppearance appearance = (DefaultAppearance)Repository.Create (typeof(DefaultAppearance).Name);

            if (!string.IsNullOrEmpty(shaderPath))
            {
                string id = Repository.GetNewName(typeof(Shader));
                appearance.ResourcePath = shaderPath;

                // TODO: our Plugin needs to take this into account when we want to add a shader path to an Appearance.  
                // obsolete - July.14.2013 - Shader nodes are no longer directly added as child nodes. 
                // We only assign Apearance.Resource path and allow pager to load it
                //Shader genericShader = Shader.Create(id, shaderPath);
                //appearance.AddChild(genericShader);                
            }
        	// else appearance.ResourcePath = "tvdefault";
        	
            //diffuse + gloss/specular map split in two chunks, resulting in a 4096×2048 resolution - shader requires store in diffuse layer 
            if (!string.IsNullOrEmpty(diffuseTex))
            {
            	Keystone.Appearance.Layer textureLayer = (Layer)Keystone.Resource.Repository.Create("Diffuse");
            	Keystone.Appearance.Texture tex = (Texture)Keystone.Resource.Repository.Create (diffuseTex, "Texture");
            	tex.TextureType = Texture.TEXTURETYPE.Default;
				textureLayer.AddChild (tex);
                appearance.AddChild(textureLayer);
                //appearance.AddDefine("DIFFUSEMAP", null);
            }
            // the second half of the diffuse hemisphere texture must store in normal map layer1
            if (!string.IsNullOrEmpty(normalmapTex))
            {
                Keystone.Appearance.Layer textureLayer = (Layer)Keystone.Resource.Repository.Create("NormalMap");
				Keystone.Appearance.Texture tex = (Texture)Keystone.Resource.Repository.Create (normalmapTex, "Texture");
				tex.TextureType = Texture.TEXTURETYPE.Default;
				textureLayer.AddChild (tex);
                appearance.AddChild(textureLayer);
                appearance.AddDefine("NORMALMAP", null);
            }

            // parallax is normalmap w/ heightmap in alpha channel - store in specular layer2
            if (!string.IsNullOrEmpty(specularTex))
            {
                Keystone.Appearance.Layer textureLayer = (Layer)Keystone.Resource.Repository.Create("Specular");
				Keystone.Appearance.Texture tex = (Texture)Keystone.Resource.Repository.Create (specularTex, "Texture");
				tex.TextureType = Texture.TEXTURETYPE.Default;
				textureLayer.AddChild (tex);
                appearance.AddChild(textureLayer);
                //appearance.AddDefine("SPECULARMAP", null);
            }

            // store in emissive layer3
            if (!string.IsNullOrEmpty(emissiveTex))
            {
                Keystone.Appearance.Layer textureLayer = (Layer)Keystone.Resource.Repository.Create("Emissive");
				Keystone.Appearance.Texture tex = (Texture)Keystone.Resource.Repository.Create (emissiveTex, "Texture");
				tex.TextureType = Texture.TEXTURETYPE.Default;
				textureLayer.AddChild (tex);
                appearance.AddChild(textureLayer);
                //appearance.AddDefine("EMISSIVEMAP", null);
            }

            
            // Our default shaders DO still make use of materials but some of our Celestial shaders do not
            if (createMaterial)
            {
                Material material = (Material)Repository.Create("Material"); 
                
                // NOTE: Using .Create(Material.DefaultMaterials) will share whenever same material is used
                //       and that may not be intended.  Instead we just use Repository.Create("Material") and
                //       a unique material is used.
                //Material material = Material.Create(Material.DefaultMaterials.matte);
                appearance.AddChild(material);
            }


            appearance.LightingMode = lightingMode;

            return appearance;
        }


        #region VEHICLE TEMPORARY TEST CODE
        //    // obsolete function that created a vehicle with interior and eterior for testing portals
        //public static void InitVehicleInterior(Vehicles.Vehicle vehicle)
        //{
        //    // add the exterior geometry
        //    Model model = new Model(Repository.GetNewName(typeof(Model)));
        //    Keystone.Appearance.DefaultAppearance app;
        //    string path = @"E:\dev\c#\KeystoneGameBlocks\Data\pool\meshes\Test Frigate\test_frigate.obj";
        //    Keystone.Elements.Mesh3d exteriorGeometry = Keystone.Elements.Mesh3d.Create(path, true, true, out app);
        //    if (app == null)
        //    {
        //        app = new Keystone.Appearance.DefaultAppearance(Keystone.Resource.Repository.GetNewName(typeof(Keystone.Appearance.DefaultAppearance)));
        //        app.AddChild(Keystone.Appearance.Material.Create(Keystone.Appearance.Material.DefaultMaterials.matte));
        //        // app.LightingMode = MTV3D65.CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED;
        //    }
        //    model.AddChild(exteriorGeometry);
        //    vehicle.AddChild(model);

        //    // TODO: normally a NewVehicle would not start with any Interior Mesh or Portals
        //    // or the interior or exterior would be generated to match the bounds of the other
        //    // only user vehicles or unowned vehicles\stations that are being boarded will load interiors
        //    Vector3d[] points = new Vector3d[4];
        //    // - add an Interior region
        //    Keystone.Portals.Region interior = new Keystone.Portals.Region("Interior_" + vehicle.ID);


        //    // - load and add a mesh to the interior region that will have our floors, ceilings and walls as viewed from inside
        //    //   and most importantly, the hole that we'll look thru to the exterior
        //    string path = @"E:\dev\c#\KeystoneGameBlocks\Data\pool\meshes\Test Frigate\test_frigate_interior.obj";
        //    model = new Model(Repository.GetNewName(typeof(Model)));
        //    StaticEntity interiorModeledEntity = CreateStaticEntity(path, "interior");
        //    interior.AddChild(interiorModeledEntity);

        //    // add the portal to the Interior region that will look to the exterior
        //    points[0] = new Vector3d(54.6024, 7.09593, -2.50019);  // top left viewed from interior
        //    points[1] = new Vector3d(54.6024, 7.09593, 2.49981);  // top right viewed from interior
        //    points[2] = new Vector3d(54.6024, 4.59593, 2.49981); // bottom right viewed from interior
        //    points[3] = new Vector3d(54.6024, 4.59593, -2.50019);  // bottom left viewed from interior

        //    //  Keystone.Portals.Portal interiorPortal = new Keystone.Portals.Portal( "interiorPortal", _core.SceneManager.ActiveScene.Root, points);
        //    //  interior.AddChild(interiorPortal);

        //    // add a portal to the Vehicle that will look to the interior region
        //    points[0] = new Vector3d(54.6024, 7.09593, 2.49981);  // top left viewed from exterior
        //    points[1] = new Vector3d(54.6024, 7.09593, -2.50019);  // top right viewed from exterior
        //    points[2] = new Vector3d(54.6024, 4.59593, -2.50019);  // bottom right
        //    points[3] = new Vector3d(54.6024, 4.59593, 2.49981); // bottom left

        //    Keystone.Portals.Portal exteriorPortal = new Keystone.Portals.Portal("exteriorPortal", interior, points);

        //    // finally add the interior to the Entity after the interior has geometry added to it elses it's bounds is null
        //    // TODO: small dilemna, i dont want to traverse Interior's during culling as children of the Vehicle but only through
        //    // exterior portals if we're outside the vehicle, or as a starting node if we're inside the vehicle.  The reason
        //    // i do still want it as a child of the Vehicle though is so we can get world transformation info from the vehicle
        //    // so that we can apply the transform during rendering of the geometry.  NOTE: culling and picking we can
        //    // transform the camera and mouse pick vectors to the object's local space first.
        //    // 
        //    vehicle.AddChild(interior);
        //    vehicle.AddChild(exteriorPortal);

        //    // - add a chair mesh too
        //    path = @"E:\dev\c#\KeystoneGameBlocks\Data\pool\meshes\Test Frigate\component_objs\galcap-[jasonduskey5-15.11.2009-e1527]_tslocator_gmdc.obj";
        //    StaticEntity captainsChair = CreateStaticEntity(path, "Captain's Chair");
        //    interior.AddChild(captainsChair);
        //    //   - chair mesh cures +1 fatigue/exhaustion every 5 minutes up to maximum
        //    //      - chair does not cure sleepiness
        //    // - bed/bunk cures +1 sleep every 5 minutes up to maximum.  After 72 hours with little or no sleep
        //    //   npc's and player too, will start to lose reaction time and make mistakes and even collapse.

        //    // set the camera into this interior region (TODO: our camera bounds check must be adaptable for the region it's in)
        //    // TODO: i think basically this means add a viewpoint to the SceneInfo that
        //    // has a camera as being in the ship and then we can use our Viewpoint selector to switch
        //    // viewpoints (like drop down box or menu within the 3d viewport toolbar)
        //    //
        //    // TODO: Now we have ability to drag and drop nodes in treeview to reparent them.
        //    //   be nice if I could load my "room" minimeshes and select a wall, delete it and
        //    //   replace it with a portal that uses the bounds of that wall segment
        //    //   Then add a "Vehicle" and reparent the entire room.

        //    //
        //    // TODO: temp set this up to use iimposter rendering
        //    //        entity.Subscribe(((Keystone.Scene.ClientSceneManager)_core.SceneManager).FXProviders[(int)Keystone.FX.FX_SEMANTICS.FX_IMPOSTER]);

        //}

    
        #endregion // VEHICLE TEMPORARY TEST CODE

#region Terrain Tile Segment
	public static ModeledEntity CreateTerrainTileSegment(bool textured)
	{		
		// model selector
		//		model[0] = null model
		//      model[1...]  (castshadow true/false)
		//			appearance
		//				material
		//				diffuselayer
		//					texture
		//					
		//			geometry
 	
		ModeledEntity result = (ModeledEntity)Resource.Repository.Create ("ModeledEntity");
		result.ResourcePath = @"\caesar\scripts\tile_floor.css";
		result.Name = "Dirt";
		result.Dynamic = false;
		result.Enable = true;
		result.UseFixedScreenSpaceSize = false;
		
		// WARNING: When modifying this segment prefab, we must re-save as prefab in the MOD database in order
		// to see these changes reflected in the game.
		ModelSequence selector = (ModelSequence)Resource.Repository.Create ("ModelSequence");
				
		// 0 - null model
		//------------------------------------------------------
		Model nullModel = (Model)Resource.Repository.Create ("Model");
		nullModel.CastShadow = false;
		nullModel.ReceiveShadow = false;
		selector.AddChild (nullModel);
		
		// cap
		// floor
		// column_uncapped
		// column_capped
		//
		// wall_thick_inner
		// wall_thick_inner_overhang
		// wall_thick_inner_underhang
		// wall_thick_inner_overhang_underhang
		// wall_thick_corner
		// wall_thick_corner_overhang
		// wall_thick_corner_underhang
		// wall_thick_corner_overhang_underhang
		//
		// wall_thin_inner_##
		// wall_thin_corner
		// wall_thin_end
		//
		// t-junction

	
		// 1 - top cap
		//------------------------------------------------------
		string HACK_MESH = @"caesar\meshes\structures\box_2.5_x_3_x_2.5.obj";
		string meshPath = @"caesar\meshes\terrain\default_cap.obj";
		string texturePath = null;
		if (textured)
			texturePath = @"caesar\textures\terrain\marsslope.jpg"; // paving 5.png";
		string shaderPath = @"caesar\shaders\PSSM\pssm.fx";
    	
    	
		Model model = CreateSegmentModel (meshPath, texturePath, shaderPath);
		selector.AddChild (model);
		

		// 2 - bottom cap
		//------------------------------------------------------		
		meshPath = @"caesar\meshes\terrain\default_floor.obj";    	
		model = CreateSegmentModel (meshPath, texturePath, shaderPath);
		selector.AddChild (model);
		
		// 3 - four-sided column variant 00
		//------------------------------------------------------
		meshPath = @"caesar\meshes\terrain\default_column_capped.obj";
		model = CreateSegmentModel (meshPath, texturePath, shaderPath);
		selector.AddChild (model);

		// 4 - three-sided - end walls [+z (0), -z (180), +x (270), -x(90)]
		//------------------------------------------------------
		meshPath = @"caesar\meshes\terrain\default_wall_thin_end_capped.obj";
		model = CreateSegmentModel (meshPath, texturePath, shaderPath);
		selector.AddChild (model);
		
		// 5, 6 - two-sided variant 00 && 01 [+/-z (0), +/-x (270)]
		//------------------------------------------------------
		meshPath = @"caesar\meshes\terrain\dirt_two_sided_00.obj";
		model = CreateSegmentModel (HACK_MESH, texturePath, shaderPath);
		selector.AddChild (model);
		
		meshPath = @"caesar\meshes\terrain\dirt_two_sided_01.obj";
		model = CreateSegmentModel (HACK_MESH, texturePath, shaderPath);
		selector.AddChild (model);

		// 7 - two-sided corner 
		//------------------------------------------------------
		meshPath = @"caesar\meshes\terrain\dirt_two_sided_corner.obj";
		model = CreateSegmentModel (HACK_MESH, texturePath, shaderPath);
		selector.AddChild (model);
		
		// 8, 9, 10, 11 - one-sided variant 00, 01, 02, 03 
		//------------------------------------------------------
		meshPath = @"caesar\meshes\terrain\default_wall_thin_inner.obj";
		model = CreateSegmentModel (meshPath, texturePath, shaderPath);
		selector.AddChild (model);
		
		meshPath = @"caesar\meshes\terrain\default_wall_thin_inner.obj";
		model = CreateSegmentModel (meshPath, texturePath, shaderPath);
		selector.AddChild (model);
		
		meshPath = @"caesar\meshes\terrain\default_wall_thin_inner.obj";
		model = CreateSegmentModel (meshPath, texturePath, shaderPath);
		selector.AddChild (model);
		
		meshPath = @"caesar\meshes\terrain\default_wall_thin_inner.obj";
		model = CreateSegmentModel (meshPath, texturePath, shaderPath);
		selector.AddChild (model);

		// one-sided T-junction
		//------------------------------------------------------
		
		
		result.AddChild (selector);  
		return result;
	}
	
	private static Model CreateSegmentModel (string meshPath, string texturePath, string shaderPath)
	{
		Model model = (Model)Repository.Create ("Model");
	
		model.CastShadow = true;
		model.ReceiveShadow = true;
		Keystone.Elements.Mesh3d mesh = (Mesh3d)Repository.Create(meshPath, "Mesh3d");
		
		Keystone.IO.PagerBase.LoadTVResource (mesh, false );
		

		// NOTE: a default material is added automatically in CreateAppearance()
		Keystone.Appearance.Appearance appearance = CreateAppearance (CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL, shaderPath, texturePath, null, null, null, true);
    	
    	model.AddChild (mesh);
    	model.AddChild (appearance);
    	return model;
	}
	
	
	public static int GenerateMapLayerBitmap(int seed, 
	                                         int locationX, int locationZ, 
	                                         int imageWidth, int imageHeight,
	                                         uint structureLevelsHigh, int bottomLevel, int topLevel, 
	                                         out int minFloorLevel, out int maxFloorLevel)
	{
		int imageSize = 8192;
		int imageScale = 1;
		LibNoise.IModule module = GetNoiseModule(seed, true, 0.0, 8.0);
    	LibNoise.Models.Plane plane = new LibNoise.Models.Plane(module);
    	plane.SetSize (imageSize * imageWidth * imageScale, 
    	               imageSize * imageHeight * imageScale);

        System.Drawing.Color start = System.Drawing.Color.FromArgb (0, 0,0,0);      
        System.Drawing.Color end = System.Drawing.Color.FromArgb (0, (int)structureLevelsHigh - 1, 0, 0);

        
        plane.Palette = new LibNoise.Palette (start, end);
		// track min/max altitude across entire Structure (imageWidth x imageHeight)
		minFloorLevel = int.MaxValue;
		maxFloorLevel = int.MinValue;       
		
		// sample noise
		int[,] heightmap = new int[imageWidth, imageHeight];
		
		for (int i = 0; i < imageWidth; i++)
		{
			for (int j = 0; j < imageHeight; j++)
			{
				int x = (locationX * imageWidth) + i;
				int z = (locationZ * imageHeight) + j;
			
				double value = plane.GetValue (x, z);				
				System.Drawing.Color color = plane.Palette.GetColor (value);
				
				// remap 0 to N to -halfN/+halfN range with sealevel inbetween floorLevel -1 and floorLevel 0
				int height = (int)(color.R - topLevel);
				if (height < bottomLevel) height = bottomLevel; 
				if (height > topLevel) height = topLevel;
				
				minFloorLevel = Math.Min (height, minFloorLevel);
				maxFloorLevel = Math.Max (height, maxFloorLevel); 
				//System.Diagnostics.Debug.WriteLine ("Level = " + level.ToString());
	
				heightmap[i, j] = height;
			}
		}
		
		bool tempHack = false;
		// TEMP HACK - generate a heightmap in a simpler way since LibNoise method is produce bad results
		if (tempHack)
		{
			minFloorLevel = int.MaxValue;
			maxFloorLevel = int.MinValue;      
			for (int i = 0; i < imageWidth; i++)
			{
				for (int j = 0; j < imageHeight; j++)
				{
					// NOTE: the first level we see is actually the top of the lowest floor
					// so visually it might seem we're missing a floor, but we're not. 
					// but wait, isnt the lowest floor always fully covered with terrain? 
					int level = Utilities.RandomHelper.RandomNumber (-1, 2);
					minFloorLevel = Math.Min (level, minFloorLevel);
					maxFloorLevel = Math.Max (level, maxFloorLevel);
					heightmap[i,j] = level;
				}
			}
		}
		// END HACK
		
		
		
		// IMPORTANT NOTE: top most level to have geometry data is maxLevel - 1.  The level exists but is not allowed to have geometry
		//       since that is reserved for entities being placed on top of the 2nd level from the top. In other words
		//       those non-structure entities will extend into the top most level.
		
		
		// verify that all levels BELOW the first level that is not empty, are also not empty.
		// (in other words, nothing is floating in our realm.)
		
		// similarly, verify all levels ABOVE the first level that is FULLY TERRAIN, are not fully terrain themselves.  
		// *no subterranian caverns assumed
		
		for (int i = bottomLevel; i <= maxFloorLevel; i++)
		{
			// create the bitmap layers for all levels from minFloorLevel to maxFloorLevel
			int initializationValue = 0;
			InitializeMapLayerBitmap ("layout", i, locationX, locationZ, imageWidth, imageHeight, initializationValue);
            initializationValue = 0;
			InitializeMapLayerBitmap ("obstacles", i, locationX, locationZ, imageWidth, imageHeight, initializationValue);
			initializationValue = -1;
			InitializeMapLayerBitmap ("style", i, locationX, locationZ, imageWidth, imageHeight, initializationValue);
		}
		
		
		Dictionary <string, Bitmap> bitmaps = new Dictionary<string, Bitmap>();
		
		string[] layerNames = new string[] {"obstacles", "layout", "style"};
		const int TERRAIN_SEGMENT_INDEX = 1;
		const int EMPTY_SEGMENT_INDEX = 0;
		
		byte segmentIndex = TERRAIN_SEGMENT_INDEX;
        	
		Keystone.TileMap.Pixel emptyPixel = Keystone.TileMap.Pixel.GetPixel();
		emptyPixel.B = EMPTY_SEGMENT_INDEX;
		Keystone.TileMap.Pixel pixel = Keystone.TileMap.Pixel.GetPixel(); 
		pixel.B = segmentIndex;
		System.Drawing.Color pixelColor = pixel.ToColor();
				
		// write segment data to each level's layout bmp		
		for (int i = 0; i < imageWidth; i++)
		{
			for (int j = 0; j < imageHeight; j++)
			{
				int maxLevel = heightmap[i, j];
				// NOTE: lowest level will always be full covered since every pixel in minFloorLevel will have segmentIndex set
				for (int level = maxLevel; level >= bottomLevel; level--)
			    {
					// for layout - assign the segmentIndex value to the x,z location
					// of ALL CREATED LEVELS <= TO THIS ONE since by definition (assuming no subterranian caverns) 
					// all levels beneath this must be terrained as well
					System.Drawing.Bitmap bmp = null;
					string path = Keystone.TileMap.Structure.GetLayerPath (locationX, locationZ, level, "layout");
					if (bitmaps.ContainsKey (path)  == false)
					{
                        string tempFile = Keystone.IO.XMLDatabase.GetTempFileName();
						System.IO.File.Copy (path, tempFile);
						
						// note: in order to save bmp to a certain path, we cannot open with that path - must open a copy
						//       perhaps we can create an entire temp folder 
						bmp = new System.Drawing.Bitmap(tempFile);
						bitmaps.Add (path, bmp);
					}
					else 
						bmp = bitmaps[path];
					
					if (level == maxLevel)
						// IMPORTANT: we must never allow structure geometry (i.e terrain) 
						//            to exist at top most level.  The top most level is reserved
						//            for placing entities and for those entities to walk through.
						bmp.SetPixel (i, j, emptyPixel.ToColor());
					else
						bmp.SetPixel (i, j, pixelColor);
				
					// for obstalces - segments on this level must affect obstacle map on level beneath this one?
					//               - must look at paint raise/lower and see where obstacle data is written
				
					// for style - we skip. autotile must compute 					
				}
			}
		}
		

		foreach (string key in bitmaps.Keys)
		{
			bitmaps[key].Save (key);
			bitmaps[key].Dispose();
		}
		
		// NOTE: the problem here is, if we use the bottomLevel as the minFloorLevel, we create a maplayer when it may not be necessary
		//       and which will only use up memory.  The alternative solution to the AutoTile creating walls between zones where one side
		//       has no maplayers present and the other does, is to have a rule where if the adjacent is NULL, then if the first MapLayer above it
		//       that is not NULL, if it's tiletype is terrain, then we treat the NULL as terrain too.
		minFloorLevel = bottomLevel;
		
		// level count
		return (maxFloorLevel - minFloorLevel) + 1;

	}
	
	public static void InitializeMapLayerBitmap(string layerName, int floorLevel, int locationX, int locationZ, int imageWidth, int imageHeight, int initializationValue)
	{
		// note: style gets ignored so it's ok to just create it here and write to it

		string path = Keystone.TileMap.Structure.GetLayerPath (locationX, locationZ, floorLevel, layerName);
		if (System.IO.File.Exists (path) == false)
    	{
        	System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(imageWidth, imageHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        	for( int x = 0; x <  bmp.Height; ++x )
        		for( int y = 0; y < bmp.Width; ++y )								        		
    			{		
        			Keystone.TileMap.Pixel pixel = Keystone.TileMap.Pixel.GetPixel(); 
        			// use the B to set segmentIndex value
        			// use the G to set modelIndex value
        			// use the High (RA) to hold minimeshElement index value
        			if (layerName == "obstacles")
        			{
        				pixel.B = (byte)initializationValue; // segments placed on this LEVEL affect obstacle map of the level BELOW it!
        			}
        			else if (layerName == "layout")
        			{
        				pixel.B = (byte)initializationValue;
        			}
        			else if (layerName == "style") // "style"
        			{
        				// style has to be discovered during autotile
        				pixel.ARGB = initializationValue;
        			}
        			else throw new NotImplementedException();
        			
        			// cannot use .SetPixel (int) with Format8bppIndexed so we will use Format24bppRgb format for this test lookup image
        			// if we use the LockBitmap here, we can use 8bit (1 byte) color values
        			bmp.SetPixel(x, y, pixel.ToColor());
        		}
        	
        	bmp.Save (path);
        	bmp.Dispose();
    	} 
	}
#endregion
        // http://www.gamedev.net/community/forums/topic.asp?topic_id=444935&whichpage=1&#2996279
        // The stars in the background:
        //- I Disable the Z-buffer
        //- I use a big sphere around the player, that is textured with a procedural texture, using very low shades of blue colors for the gradient.
        //- Next I render a set of random points for the static stars far away
        //- Next I render the same set of points rotated differently with pointsize 2.0f for the bigger stars
        //- I post process the scene using a blur on a high blur value (I take the backbuffer, scale it down to quarter screen, blur horizontally, then vertically, then add it back to the backbuffer. This is done with a pixel shader fx file from nVidia's website + some tweaking for it)
        //- Next i draw the closer stars, with slow rotation to give the feel of movement
        //- I post process again with a lower blur value, so it adds the moving stars to the effect
        //- I re-enable the Z-buffer

        // E:\dev\_projects\GalaxyEngine

        // a very very very good thread for generating normal maps for these planet textures (which are just gradient colored heightmaps) we gen.
        // how to generate a normal map for our planet's heightmap
        // http://www.gamedev.net/community/forums/topic.asp?topic_id=475213
        //
        // check the src ive already downloaded the complex planet
        // http://libnoise.sourceforge.net/examples/complexplanet/index.html
        // from that page.  see KeystoneGameBlocks\LibNoise.NET\examples\complex_planet
        //
        // Also infinity's notes on sub-tile texturing
        // http://www.gamedev.net/community/forums/mod/journal/journal.asp?jn=263350&reply_id=3198944
        //
        // also check out the hlsl used in
        // F:\games\_demos\MooxSpace_r16\media\programs
        // thats much nicer i think than Zaks
        public static string GenerateWorldTexture(LibNoise.Models.NoiseModelType modelType, int textureHeight, int textureWidth, bool fast, string palettePath, int seed)
        {
            LibNoise.IModule module = GetNoiseModule(seed, fast, 0, 1000);
            int[] colors = null;

            
            
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // generate the perlin texture
            if (modelType == LibNoise.Models.NoiseModelType.Sphere)
            {

                LibNoise.Models.Sphere sphere = new LibNoise.Models.Sphere(module);
                Bitmap lookupBitmap = new Bitmap(palettePath);
                sphere.Palette = new LibNoise.Palette(lookupBitmap);
                // sphere.Palette = new LibNoise.Palette() ; 

                // // === wood palette
                //  // Create a dark-stained wood palette (oak?)
                //sphere.Palette.Clear();
                //sphere.Palette.AddColor (0, 189, 94, 4);
                //sphere.Palette.AddColor(0.50, 144, 48, 6);
                //sphere.Palette.AddColor(1.00, 60, 10, 8);
                //// == end wood palette
                //// == a nice jade palette.
                //  sphere.Palette.Clear();
                //  sphere.Palette.AddColor (-1.000, 24, 146, 102);
                //  sphere.Palette.AddColor ( 0.000, 78, 154, 115);
                //  sphere.Palette.AddColor ( 0.250, 128, 204, 165);
                //  sphere.Palette.AddColor ( 0.375, 78, 154, 115);
                //  sphere.Palette.AddColor ( 1.000, 29, 135, 102);
                //// == end jade palette
                //  // Create a gray granite palette.  Black and pink appear at either ends of
                //  // the palette; those colors provide the charactistic black and pink flecks
                //  // in granite.
                //  sphere.Palette.Clear();
                //  sphere.Palette.AddColor (-1.0000, 0,   0,   0);
                //  sphere.Palette.AddColor (-0.9375, 0,   0,   0);
                //  sphere.Palette.AddColor (-0.8750, 216, 216, 242);
                //  sphere.Palette.AddColor ( 0.0000, 191, 191, 191);
                //  sphere.Palette.AddColor ( 0.5000, 210, 116, 125);
                //  sphere.Palette.AddColor ( 0.7500, 210, 113,  98);
                //  sphere.Palette.AddColor ( 1.0000, 255, 176, 192);
                //// == end granite
                //  // == water palette
                //  sphere.Palette.Clear();
                //  sphere.Palette.AddColor(-1.00, 48, 64, 192, 255);
                //  sphere.Palette.AddColor(0.50, 96, 192, 255, 255);
                //  sphere.Palette.AddColor(1.00, 255, 255, 255, 255);
                //  // == end water palette
                //  // = cloud palette - note it has two fully transparent areas!
                //  sphere.Palette.Clear();
                //  sphere.Palette.AddColor(-1.00, 255, 255, 255, 0);
                //  sphere.Palette.AddColor(-0.50, 255, 255, 255, 0);
                //  sphere.Palette.AddColor(1.00, 255, 255, 255, 255);
                //  // = end cloud palette
                //// == slime
                //sphere.Palette.Clear();
                //sphere.Palette.AddColor (-1.0000, 160,  64,  42);
                //sphere.Palette.AddColor ( 0.0000, 64, 192,  64);
                //sphere.Palette.AddColor ( 1.0000, 128, 255, 128);
                //// == end slime
                sphere.SetCoordinateSystemExtents(-180, 180, -90, 90);
               
                colors = sphere.Generate((uint)textureWidth, (uint)textureHeight);
            }

            // create the tv texture and save it
            bool hasAlpha = true; // used in the TextureFactory.CreateTexture() call.  cloud layers would have alpha but surface terrain + ocean layers would not.
            string name = "libnoise";
            CoreClient._CoreClient.TextureFactory.SetTextureMode(CONST_TV_TEXTUREMODE.TV_TEXTUREMODE_32BITS);
            int textureIndex = CoreClient._CoreClient.TextureFactory.CreateTexture(textureWidth, textureHeight, hasAlpha);
            CoreClient._CoreClient.TextureFactory.LockTexture(textureIndex, false);
            CoreClient._CoreClient.TextureFactory.SetPixelArray(textureIndex, 0, 0, textureWidth, textureHeight, colors);
            CoreClient._CoreClient.TextureFactory.UnlockTexture(textureIndex);
            string filename = Core._Core.DataPath + "\\caesar\\Shaders\\Planet\\libnoisegen.bmp";
            CoreClient._CoreClient.TextureFactory.SaveTexture(textureIndex, filename, CONST_TV_IMAGEFORMAT.TV_IMAGE_BMP);
            stopWatch.Stop();
            System.Diagnostics.Trace.WriteLine(string.Format("Procedural texture '{0}' generated in {1} seconds", filename, stopWatch.Elapsed.TotalSeconds));
            return filename;
        }

        private static LibNoise.IModule GetNoiseModule (int seed, bool fast, double lowerBounds, double upperBounds)
        {
        	if (fast)
            {
        		LibNoise.FastNoise fastPerlin = new LibNoise.FastNoise(seed);
        		fastPerlin.Frequency =  0.1; // 0.015;
                fastPerlin.NoiseQuality = LibNoise.NoiseQuality.Standard;
                fastPerlin.Seed = 13;
                fastPerlin.OctaveCount = 2;
                fastPerlin.Lacunarity = 0.5;
                fastPerlin.Persistence = 0.15;
                return fastPerlin;
                    
                LibNoise.FastNoise fastPlanetContinents = new LibNoise.FastNoise(seed);
                fastPlanetContinents.Frequency = 0.5;

                LibNoise.FastBillow fastPlanetLowlands = new LibNoise.FastBillow();
                fastPlanetLowlands.Frequency = 2; // 4
                ScaleBiasOutput fastPlanetLowlandsScaled = new ScaleBiasOutput(fastPlanetLowlands);
                fastPlanetLowlandsScaled.Scale = 0.2;
                fastPlanetLowlandsScaled.Bias = 0.5;
				
                
                LibNoise.FastRidgedMultifractal fastPlanetMountainsBase = new LibNoise.FastRidgedMultifractal(seed);
                fastPlanetMountainsBase.Frequency = 1; // 4

                ScaleBiasOutput fastPlanetMountainsScaled = new ScaleBiasOutput(fastPlanetMountainsBase);
                fastPlanetMountainsScaled.Scale = 0.4;
                fastPlanetMountainsScaled.Bias = 0.85;

                LibNoise.FastTurbulence fastPlanetMountains = new LibNoise.FastTurbulence(fastPlanetMountainsScaled);
                fastPlanetMountains.Power = 0.1;
                fastPlanetMountains.Frequency = 5; // 50

                LibNoise.FastNoise fastPlanetLandFilter = new LibNoise.FastNoise(seed + 1); // increment seed for subsequent occurrences of the same noise type
                fastPlanetLandFilter.Frequency = 3; //6

                Select fastPlanetLand = new Select(fastPlanetLandFilter, fastPlanetLowlandsScaled, fastPlanetMountains);
                fastPlanetLand.SetBounds(lowerBounds, upperBounds);
                fastPlanetLand.EdgeFalloff = 0.5;

                LibNoise.FastBillow fastPlanetOceanBase = new LibNoise.FastBillow(seed);
                fastPlanetOceanBase.Frequency = 5; // 15
                ScaleOutput fastPlanetOcean = new ScaleOutput(fastPlanetOceanBase, 0.1);

                Select fastPlanetFinal = new Select(fastPlanetContinents, fastPlanetOcean, fastPlanetLand);
                fastPlanetFinal.SetBounds(lowerBounds, upperBounds);
                fastPlanetFinal.EdgeFalloff = 0.5;

//                return fastPlanetFinal;

                // This texture map is made up two layers.  The bottom layer is a wavy water
                // texture.  The top layer is a cloud texture.  These two layers are
                // combined together to create the final texture map.

                //  // Lower layer: water texture
                //  // --------------------------

                //  // Base of the water texture.  The Voronoi polygons generate the waves.  At
                //  // the center of the polygons, the values are at their lowest.  At the edges
                //  // of the polygons, the values are at their highest.  The values smoothly
                //  // change between the center and the edges of the polygons, producing a
                //  // wave-like effect.
                //  LibNoise.Voronoi baseWater = new LibNoise.Voronoi ();
                //  baseWater.Seed = 0;
                //  baseWater.Frequency = 8.0;
                //  baseWater.DistanceEnabled = true;
                //  baseWater.Displacement = 0.0;

                //  // Stretch the waves along the z axis.
                //// TODO: verify this is ScaleInput and not ScaleOutput
                //  LibNoise.Modifiers.ScaleInput baseStretchedWater = new ScaleInput (baseWater, 1.0, 1.0, 3.0);

                //  // Smoothly perturb the water texture for more realism.
                //  LibNoise.Turbulence finalWater = new LibNoise.Turbulence (baseStretchedWater );
                //  finalWater.Seed = 1;
                //  finalWater.Frequency = 8.0;
                //  finalWater.Power = 1.0 / 32.0;
                //  finalWater.Roughness = 1;
                //  module = finalWater;

                //// Upper layer: cloud texture - this needs to be rendered to a second sphere over the world
                //// --------------------------

                //// Base of the cloud texture.  The billowy noise produces the basic shape
                //// of soft, fluffy clouds.
                //LibNoise.Billow cloudBase = new LibNoise.Billow();
                //cloudBase.Seed = 2;
                //cloudBase.Frequency = 2.0;
                //cloudBase.Persistence = 0.375;
                //cloudBase.Lacunarity = 2.12109375;
                //cloudBase.OctaveCount = 4;
                //cloudBase.NoiseQuality = LibNoise.NoiseQuality.High;

                //// Perturb the cloud texture for more realism.
                //LibNoise.Turbulence finalClouds = new LibNoise.Turbulence(cloudBase);
                //finalClouds.Seed = 3;
                //finalClouds.Frequency = 16.0;
                //finalClouds.Power = 1.0 / 64.0;
                //finalClouds.Roughness = 2;
                //module = finalClouds;


                //  // Large slime bubble texture.
                //LibNoise.Billow largeSlime = new  LibNoise.Billow();
                //largeSlime.Seed =0;
                //largeSlime.Frequency =4.0;
                //largeSlime.Lacunarity =2.12109375;
                //largeSlime.OctaveCount =1;
                //largeSlime.NoiseQuality = LibNoise.NoiseQuality.High ;

                //// Base of the small slime bubble texture.  This texture will eventually
                //// appear inside cracks in the large slime bubble texture.
                //LibNoise.Billow smallSlimeBase = new LibNoise.Billow ();
                //smallSlimeBase.Seed =1;
                //smallSlimeBase.Frequency =24.0;
                //smallSlimeBase.Lacunarity =2.14453125;
                //smallSlimeBase.OctaveCount =1;
                //smallSlimeBase.NoiseQuality = LibNoise.NoiseQuality.High;

                //// Scale and lower the small slime bubble values.
                //LibNoise.Modifiers.ScaleBiasOutput smallSlime = new ScaleBiasOutput (smallSlimeBase );
                //smallSlime.Scale =0.5;
                //smallSlime.Bias =-0.5;

                //// Create a map that specifies where the large and small slime bubble
                //// textures will appear in the final texture map.
                //LibNoise.RidgedMultifractal slimeMap = new LibNoise.RidgedMultifractal ();
                //slimeMap.Seed =0;
                //slimeMap.Frequency =2.0;
                //slimeMap.Lacunarity =2.20703125;
                //slimeMap.OctaveCount =3;
                //slimeMap.NoiseQuality = LibNoise.NoiseQuality.Standard;

                //// Choose between the large or small slime bubble textures depending on the
                //// corresponding value from the slime map.  Choose the small slime bubble
                //// texture if the slime map value is within a narrow range of values,
                //// otherwise choose the large slime bubble texture.  The edge falloff is
                //// non-zero so that there is a smooth transition between the two textures.
                //LibNoise.Modifiers.Select slimeChooser = new Select (slimeMap, largeSlime , smallSlime );
                //slimeChooser.SetBounds (-0.375, 0.375);
                //slimeChooser.EdgeFalloff =0.5;

                //// Finally, perturb the slime texture to add realism.
                //LibNoise.Turbulence finalSlime = new LibNoise.Turbulence (slimeChooser );
                //finalSlime.Seed =2;
                //finalSlime.Frequency =8.0;
                //finalSlime.Power =1.0 / 32.0;
                //finalSlime.Roughness =2;
                //module = finalSlime;
                //// =================
                // Primary granite texture.  This generates the "roughness" of the texture
                // when lit by a light source.
                LibNoise.Billow primaryGranite = new LibNoise.Billow ();
                primaryGranite.Seed = 0;
                primaryGranite.Frequency = 8.0;
                primaryGranite.Persistence = 0.625;
                primaryGranite.Lacunarity = 2.18359375;
                primaryGranite.OctaveCount =6 ;
                primaryGranite.NoiseQuality = LibNoise.NoiseQuality.Standard;

                // Use Voronoi polygons to produce the small grains for the granite texture.
                LibNoise.Voronoi  baseGrains = new LibNoise.Voronoi ();
                baseGrains.Seed = 1;
                baseGrains.Frequency = 16.0;
                baseGrains.DistanceEnabled = true;

                // Scale the small grain values so that they may be added to the base
                // granite texture.  Voronoi polygons normally generate pits, so apply a
                // negative scaling factor to produce bumps instead.
                LibNoise.Modifiers.ScaleBiasOutput scaledGrains = new ScaleBiasOutput (baseGrains);
                scaledGrains.Scale =-0.5;
                scaledGrains.Bias =0.0;

                // Combine the primary granite texture with the small grain texture.
                LibNoise.Modifiers.Add combinedGranite = new Add (primaryGranite , scaledGrains );

                // Finally, perturb the granite texture to add realism.
                LibNoise.Turbulence finalGranite = new LibNoise.Turbulence (combinedGranite );
                finalGranite.Seed =2;
                finalGranite.Frequency =4.0;
                finalGranite.Power =1.0 / 8.0;
                finalGranite.Roughness = 6;
                //module = finalGranite;
//                return finalGranite;
                //// =================
                //// Primary jade texture.  The ridges from the ridged-multifractal module produces the veins.
                LibNoise.RidgedMultifractal primaryJade = new LibNoise.RidgedMultifractal ();
                primaryJade.Seed = 0;
                primaryJade.Frequency = 0.1;
                primaryJade.Lacunarity = 2.20703125;
                primaryJade.OctaveCount = 1;
                primaryJade.NoiseQuality = LibNoise.NoiseQuality.Standard;
                

				return primaryJade;
				
                //// Base of the secondary jade texture.  The base texture uses concentric
                //// cylinders aligned on the z axis, which will eventually be perturbed.
                //LibNoise.Cylinders baseSecondaryJade = new LibNoise.Cylinders ();
                //baseSecondaryJade.Frequency =2.0;

                //// Rotate the base secondary jade texture so that the cylinders are not
                //// aligned with any axis.  This produces more variation in the secondary
                //// jade texture since the texture is parallel to the y-axis.
                //LibNoise.Modifiers.RotateInput  rotatedBaseSecondaryJade = new RotateInput (baseSecondaryJade , 90.0, 25.0, 5.0);

                //// Slightly perturb the secondary jade texture for more realism.
                //LibNoise.Turbulence perturbedBaseSecondaryJade = new LibNoise.Turbulence (rotatedBaseSecondaryJade );
                //perturbedBaseSecondaryJade.Seed = 1;
                //perturbedBaseSecondaryJade.Frequency = 4.0;
                //perturbedBaseSecondaryJade.Power = 1.0 / 4.0;
                //perturbedBaseSecondaryJade.Roughness = 4;

                //// Scale the secondary jade texture so it contributes a small part to the
                //// final jade texture.
                //LibNoise.Modifiers.ScaleBiasOutput secondaryJade = new ScaleBiasOutput (perturbedBaseSecondaryJade);
                //secondaryJade.Scale =0.25;
                //secondaryJade.Bias = 0.0;

                //// Add the two jade textures together.  These two textures were produced
                //// using different combinations of coherent noise, so the final texture will
                //// have a lot of variation.
                //LibNoise.Modifiers.Add combinedJade = new Add (primaryJade , secondaryJade );

                //// Finally, perturb the combined jade textures to produce the final jade
                //// texture.  A low roughness produces nice veins.
                //LibNoise.Turbulence finalJade = new LibNoise.Turbulence (combinedJade );
                //finalJade.Seed =2;
                //finalJade.Frequency = 4.0;
                //finalJade.Power = 1.0 / 16.0;
                //finalJade.Roughness = 2;
                //module = finalJade;

                //// =================
                //  // Base wood texture.  The base texture uses concentric cylinders aligned
                //  // on the z axis, like a log.
                //  LibNoise.Cylinders baseWood = new LibNoise.Cylinders();
                //  baseWood.Frequency =  16.0;

                //  // Perlin noise to use for the wood grain.
                //  LibNoise.Perlin woodGrainNoise = new LibNoise.Perlin ();
                //  woodGrainNoise.Seed = 0;
                //  woodGrainNoise.Frequency = 48.0f;
                //  woodGrainNoise.Persistence = 0.5f;
                //  woodGrainNoise.Lacunarity = 2.20703125f;
                //  woodGrainNoise.OctaveCount = 3;
                //  woodGrainNoise.NoiseQuality = LibNoise.NoiseQuality.Standard;

                //  // Stretch the Perlin noise in the same direction as the center of the
                //  // log.  This produces a nice wood-grain texture.
                //  LibNoise.Modifiers.ScaleOutput  scaledBaseWoodGrain = new ScaleOutput(woodGrainNoise, .25f);
                //  //scaledBaseWoodGrain.SourceModule = woodGrainNoise;
                //  //scaledBaseWoodGrain.Scale = .25f;

                //  // Scale the wood-grain values so that they may be added to the base wood
                //  // texture.
                //   LibNoise.Modifiers.ScaleBiasOutput woodGrain = new ScaleBiasOutput (scaledBaseWoodGrain );
                //  woodGrain.Scale = 0.25f;
                //  woodGrain.Bias = 0.125f;

                //  // Add the wood grain texture to the base wood texture.
                //  LibNoise.Modifiers.Add  combinedWood = new Add (baseWood , woodGrain);

                //  // Slightly perturb the wood texture for more realism.
                //  LibNoise.Turbulence perturbedWood = new LibNoise.Turbulence (combinedWood );
                //  perturbedWood.Seed = 1;
                //  perturbedWood.Frequency = 4.0;
                //  perturbedWood.Power = 1.0 / 255.0;
                //  perturbedWood.Roughness = 4;

                //  // Cut the wood texture a small distance from the center of the "log".
                //  LibNoise.Modifiers.TranslateInput translatedWood = new TranslateInput (perturbedWood ,0, 0,0);
                //  translatedWood.Z = 1.48f;

                //  // Cut the wood texture on an angle to produce a more interesting wood texture.
                //  LibNoise.Modifiers.RotateInput rotatedWood = new RotateInput (translatedWood, 84.0, 0f, 0f);

                //  // Finally, perturb the wood texture to produce the final texture.
                //  LibNoise.Turbulence  finalWood = new LibNoise.Turbulence (rotatedWood);
                //  finalWood.SourceModule = rotatedWood;
                //  finalWood.Seed = 2;
                //  finalWood.Frequency = 2.0f;
                //  finalWood.Power = 1.0 / 64.0;
                //  finalWood.Roughness = 4;
                //  module = finalWood;
                // // =================

            }
            else // slow
            {
                LibNoise.Perlin slowPlanetContinents = new LibNoise.Perlin();
                slowPlanetContinents.Frequency = 0.5; // 1.5;

                LibNoise.Billow slowPlanetLowlands = new LibNoise.Billow();
                slowPlanetLowlands.Frequency = 4;
                LibNoise.Modifiers.ScaleBiasOutput slowPlanetLowlandsScaled = new ScaleBiasOutput(slowPlanetLowlands);
                slowPlanetLowlandsScaled.Scale = 0.2;
                slowPlanetLowlandsScaled.Bias = 0.5;

                LibNoise.RidgedMultifractal slowPlanetMountainsBase = new LibNoise.RidgedMultifractal();
                slowPlanetMountainsBase.Frequency = 4;

                ScaleBiasOutput slowPlanetMountainsScaled = new ScaleBiasOutput(slowPlanetMountainsBase);
                slowPlanetMountainsScaled.Scale = 0.4;
                slowPlanetMountainsScaled.Bias = 0.85;

                LibNoise.FastTurbulence slowPlanetMountains = new LibNoise.FastTurbulence(slowPlanetMountainsScaled);
                slowPlanetMountains.Power = 0.1;
                slowPlanetMountains.Frequency = 50;

                LibNoise.Perlin slowPlanetLandFilter = new LibNoise.Perlin();
                slowPlanetLandFilter.Frequency = 6;

                Select slowPlanetLand = new Select(slowPlanetLandFilter, slowPlanetLowlandsScaled, slowPlanetMountains);
                slowPlanetLand.SetBounds(lowerBounds, upperBounds);
                slowPlanetLand.EdgeFalloff = 0.5;

                LibNoise.Billow slowPlanetOceanBase = new LibNoise.Billow();
                slowPlanetOceanBase.Frequency = 15;
                ScaleOutput slowPlanetOcean = new ScaleOutput(slowPlanetOceanBase, 0.1);

                Select slowPlanetFinal = new Select(slowPlanetContinents, slowPlanetOcean, slowPlanetLand);
                slowPlanetFinal.SetBounds(lowerBounds, upperBounds);
                slowPlanetFinal.EdgeFalloff = 0.5;

                return slowPlanetFinal;
            }

        }
#region CellGrid
        /// <summary>
        /// Finds the TVMesh triangle indices for a particular cell in 
        /// a grid mesh created with Mesh3d.CreateCellGrid() and a 
        /// corresponding celledRegion with equal cell dimensions.
        /// </summary>
        /// <param name="widthX">Number of elements in the grid along X axis</param>
        /// <param name="depthZ">Number of elements in the grid along Z axis</param>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="triangle1"></param>
        /// <param name="triangle2"></param>
        internal static void CellGrid_GetCellTriangleIndices(uint widthX, uint depthZ, uint x, uint z, out int triangle1, out int triangle2)
        {

            // deduce the mesh triangle indices for the triangles that comprise the picked quad
            uint deckTileCount = widthX * depthZ;
            triangle1 = (int)(2 * ((deckTileCount - 1) - (x + z * widthX)));// * 2 because there are two triangles per tile or cell 
            triangle2 = triangle1 + 1;
            //     System.Diagnostics.Debug.WriteLine(string.Format("T1 = {0}  T2 = {1}", triangle1, triangle2));
        }

        
        public static void CellGrid_CellSetCollapseState(uint widthX, uint depthZ, uint x, uint z, Mesh3d mesh, bool collapse)
        {
            // TODO: i think that if i start doing this before our tvmesh is loaded
            //       we will have completely screwed up floors during load of saved cell data
            int triangle1, triangle2;
            CellGrid_GetCellTriangleIndices(widthX, depthZ, x, z, out triangle1, out triangle2);

            // vertices 2 and 3 will be at indices 0 and 1
            int[] triangleVertexIndices = mesh.GetTriangleIndices(triangle2);
            int a = triangleVertexIndices[0];
            int b = triangleVertexIndices[1];
            // vertices 0 and 1 from first triangle
            triangleVertexIndices = mesh.GetTriangleIndices(triangle1);
            int c = triangleVertexIndices[0];
            int d = triangleVertexIndices[1];

            Vector3d min, max = Vector3d.Zero();

            if (collapse == false)
            {
                // TODO: if ceiling and using reversed vertex ordering,
                //       do we need to use b and c instead?  I wouldn't think so
                //       because the verts are added in same order but to different
                //       values
                min = mesh.GetVertex(a);
                max = mesh.GetVertex(c);
                double zTemp = min.z;
                min.z = max.z;
                max.z = zTemp;

                mesh.SetVertex(b, min);
                mesh.SetVertex(d, max);
            }
            else
            {
                // make degenerate triangles by moving verts b and d to same coords as a and c respectively
                min = mesh.GetVertex(a);
                max = mesh.GetVertex(c);
                mesh.SetVertex(b, min);
                mesh.SetVertex(d, max);
            }
        }

        public static bool CellGrid_IsCellCollapsed(uint widthX, uint depthZ, uint x, uint z, Mesh3d mesh)
        {
            int triangle1, triangle2;
            CellGrid_GetCellTriangleIndices(widthX, depthZ, x, z, out triangle1, out triangle2);

            // vertices 2 and 3 will be at indices 0 and 1
            int[] triangleVertexIndices = mesh.GetTriangleIndices(triangle2);
            int a = triangleVertexIndices[0];
            int b = triangleVertexIndices[1];
            // vertices 0 and 1 from first triangle
            triangleVertexIndices = mesh.GetTriangleIndices(triangle1);
            int c = triangleVertexIndices[0];
            int d = triangleVertexIndices[1];

            Vector3d vA = mesh.GetVertex(a);
            Vector3d vC = mesh.GetVertex(c);

            if (vA == mesh.GetVertex(b) && vC == mesh.GetVertex(d)) return true;

            return false;

        }

        public static void CellGrid_SetCellUV(uint widthX, uint depthZ, uint x, uint z, Mesh3d mesh, Keystone.Appearance.TextureAtlas atlas, uint tileIndex)
        {
            if (tileIndex < 0) return;

            int triangle1, triangle2;
            // deduce the mesh triangle indices for the triangles that comprise the picked quad
            CellGrid_GetCellTriangleIndices(widthX, depthZ, x, z, out triangle1, out triangle2);

            Vector2f[] textureDimensions = atlas.GetTileDimensions(tileIndex);

            // MODIFY THE VERTICES OF THIS QUAD
            // http://www.truevision3d.com/forums/tv3d_sdk_65/setgeometry_optimization_and_face_order-t14255.0.html
            // work around for the index re-ordering bug is to select indices
            // 0 and 1 from the first triangle as the right most vertices of the quad
            // and 2 and 3 from the second triangle as the left most vertices 

            // vertices 2 and 3 will be at indices 0 and 1
            int[] triangle = mesh.GetTriangleIndices(triangle2);
            int vertex0 = triangle[0];
            int vertex1 = triangle[1];
            // vertices 0 and 1 from first triangle
            triangle = mesh.GetTriangleIndices(triangle1);
            int vertex2 = triangle[0];
            int vertex3 = triangle[1];

            // make modifications to vertex UVs
            mesh.SetVertexUV(vertex0, textureDimensions[0].x, textureDimensions[1].y); // bottom left vertex of quad
            mesh.SetVertexUV(vertex1, textureDimensions[0].x, textureDimensions[0].y); // top left vertex of quad
            mesh.SetVertexUV(vertex2, textureDimensions[1].x, textureDimensions[0].y); // top right vertex of quad
            mesh.SetVertexUV(vertex3, textureDimensions[1].x, textureDimensions[1].y); // bottom right vertex of quad
        }

        public static void CellGrid_SetCellUV(uint widthX, uint depthZ,  uint x, uint z, Mesh3d mesh, int atlasTileIndex, float textureCountX, float textureCountY)
        {
            // NOTE: THis specifically alters the UVs, it does not directly
            //       modify the boundary layer data mask in the CellMap db layer

     
            // TODO:  i think create a CelledGrid as a type of Mesh3d (like Billboard) 
            //       that has extra methods for collapsing and setting UVs based on an atlas index.

            int triangle1, triangle2;
            // deduce the mesh triangle indices for the triangles that comprise the picked quad
            CellGrid_GetCellTriangleIndices(widthX, depthZ, x, z, out triangle1, out triangle2);
          

            // compute the width ratio of each sub-texture in the mini atlas 
            float tileWidth = 1 / textureCountX; // uv width per tile (assuming all tiles are same width)
            float textureHeight = 1 / textureCountY; // uv height per tile (assuming all tiles are same height)


            float u1 = atlasTileIndex * tileWidth;
            float u2 = u1 + tileWidth;
            float v1 = 0f; // TODO: if our atlas contains > 1 row along Y, our v1 coord of 0 will not work
            float v2 = v1 + textureHeight;
            
            // MODIFY THE VERTICES OF THIS QUAD
            // http://www.truevision3d.com/forums/tv3d_sdk_65/setgeometry_optimization_and_face_order-t14255.0.html
            // work around for the index re-ordering bug is to select indices
            // 0 and 1 from the first triangle as the right most vertices of the quad
            // and 2 and 3 from the second triangle as the left most vertices 

            // vertices 2 and 3 will be at indices 0 and 1
            int[] triangle = mesh.GetTriangleIndices(triangle2);
            int vertex0 = triangle[0];
            int vertex1 = triangle[1];
            // vertices 0 and 1 from first triangle
            triangle = mesh.GetTriangleIndices(triangle1);
            int vertex2 = triangle[0];
            int vertex3 = triangle[1];

            // make modifications to vertex UVs
            mesh.SetVertexUV(vertex0, u1, v2); // bottom left vertex of quad
            mesh.SetVertexUV(vertex1, u1, v1); // top left vertex of quad
            mesh.SetVertexUV(vertex2, u2, v1); // top right vertex of quad
            mesh.SetVertexUV(vertex3, u2, v2); // bottom right vertex of quad

            //mesh.SetVertexUV(vertex0, uvDimensions[0].x, uvDimensions[1].y); // bottom left vertex of quad
            //mesh.SetVertexUV(vertex1, uvDimensions[0].x, uvDimensions[0].y); // top left vertex of quad
            //mesh.SetVertexUV(vertex2, uvDimensions[1].x, uvDimensions[0].y); // top right vertex of quad
            //mesh.SetVertexUV(vertex3, uvDimensions[1].x, uvDimensions[1].y); // bottom right vertex of quad
        }

        
        
        // TODO: OBSOLETE -  Our OLD PRE SHADER visual grid we generated and then drew in FloorPlanHud.cs  
        // resulted in out of memory error.  The new version does the entire tilemask grid using just a single large quad
        // where the pixels match our desired grid resolution and we use SetPixelArray on a texture to store
		// our atlas lookup texture values.        
        public static void ToggleTileMaskUVs(float cellSizeX, float cellSizeZ, int cellCountX, int cellCountZ, int innerCellCountX, int innerCellCountZ, Mesh3d mesh, TextureAtlas atlas, uint atlasIndex, Vector3d impactPoint)
        {
            float minX = cellSizeX / -2f;
            float maxX = -minX;
            float minZ = cellSizeZ / -2f;
            float maxZ = -minZ;

            // TODO: if impactpoint parameters are not between the min and max this will fail
            int xResult = (int)(innerCellCountX * Utilities.InterpolationHelper.LinearMapValue(minX, maxX, (float)impactPoint.x));
            int zResult = (int)(innerCellCountZ * Utilities.InterpolationHelper.LinearMapValue(minZ, maxZ, (float)impactPoint.z));

            // compute the triangles of the mouse hit quad
            int triangle1 = 2 * (xResult + (zResult * innerCellCountX)); // * 2 because there are two triangles per mask tile
            int triangle2 = triangle1 + 1;
            // System.Diagnostics.Debug.WriteLine(string.Format("T1 = {0}  T2 = {1}", triangle1, triangle2));

            //NOTE: mesh only represents one floor so the y component of impactPoint is ignored 

            Vector2f[] uvDimensions = atlas.GetTileDimensions(0f, 0f,1f,1f, atlasIndex);

            // http://www.truevision3d.com/forums/tv3d_sdk_65/setgeometry_optimization_and_face_order-t14255.0.html
            // work around for the index re-ordering bug is to select indices
            // 0 and 1 from the first triangle as the right most vertices of the quad
            // and 2 and 3 from the second triangle as the left most vertices 

            // vertices 2 and 3 will be at indices 0 and 1
            // NOTE: GetTriangleIndices() which calls tvmesh.GetTriangleInfo() is a VERY expensive call
            //       and cannot be done every frame!  The .SetVertexUV in comparison are not that expensive.
            // TODO: is there  away to deduce the triangle indices?
            int[] triangle = mesh.GetTriangleIndices(triangle2);
          //  triangle = new int[] { triangle2 * 2, triangle2 * 2 + 1, triangle2 * 2 - 2 };
            if (triangle == null) return;
            mesh.SetVertexUV(triangle[0], uvDimensions[0].x, uvDimensions[1].y); // bottom left vertex of quad
            mesh.SetVertexUV(triangle[1], uvDimensions[0].x, uvDimensions[0].y); // top left vertex of quad

            // vertices 0 and 1 from first triangle
            triangle = mesh.GetTriangleIndices(triangle1);
         //   triangle = new int[] { triangle1 * 2, triangle1 * 2 + 1, triangle1 * 2 + 2 };
            mesh.SetVertexUV(triangle[0], uvDimensions[1].x, uvDimensions[0].y); // top right vertex of quad
            mesh.SetVertexUV(triangle[1], uvDimensions[1].x, uvDimensions[1].y); // bottom right vertex of quad
        }


        public static Vector3d PixelCoordinateToPosition (float gridWidth, float gridDepth,
                                                            uint tileCountX, uint tileCountZ,
                                                            int pixelX, int pixelZ)
        {
        	Vector3d result;
        	double tileSizeX = gridWidth / (double)tileCountX;
        	double tileSizeZ = gridDepth / (double)tileCountZ;
        	
        	double left = -gridWidth / 2d;
        	double bottom = -gridDepth / 2d;
        	
        	result.x = left + (tileSizeX * pixelX);
        	result.y = 0d;
        	result.z = bottom + (tileSizeZ * pixelZ);
        	
        	// find center
        	result.x += tileSizeX / 2d;
        	result.z += tileSizeZ / 2d;
        	
        	return result;
        }
        
        public static void MapImpactPointToTileCoordinate(float gridWidth, float gridDepth,
                                                            uint tileCountX, uint tileCountZ,
                                                            float impactPointX, float impactPointZ,
                                                            out int pixelX, out int pixelZ)
        {
 
            MapImpactPointToPixelCoordinate(gridWidth, gridDepth, tileCountX, tileCountZ,
                                           impactPointX, impactPointZ,
                                           out pixelX, out pixelZ);

            // to map from texture pixel coords to tilemap coords, we need to 
            // reverse the Z pixel.
            pixelZ = Math.Abs(pixelZ - (int)tileCountZ + 1);
        }

        /// <summary>
        /// Maps a point that is in local space of grid mesh with origin at center of mesh
        /// and converts to pixel coordinate where pixel 0,0 is bottom, left of image.
        /// </summary>
        /// <param name="gridWidth">Range across X axis in local space</param>
        /// <param name="gridDepth">Range across Z axis in local space</param>
        /// <param name="tileCountX">Remapped range across X</param>
        /// <param name="tileCountZ">Remapped range across Z</param>
        /// <param name="impactPointX">X Point relative to origin that is at center of grid.</param>
        /// <param name="impactPointZ">Z Point relative to origin that is at center of grid</param>
        /// <param name="pixelX">Remapped result X</param>
        /// <param name="pixelZ">Remapped result Z</param>
        public static void MapImpactPointToPixelCoordinate(float gridWidth, float gridDepth,
                                                            uint tileCountX, uint tileCountZ, 
                                                            float impactPointX, float impactPointZ, 
                                                            out int pixelX, out int pixelZ)
        {
            // compute local space min/max dimensions of the quad
            float minX = gridWidth / -2f;
            float maxX = -minX;
            float minZ = gridDepth / -2f;
            float maxZ = -minZ;

            // NOTE: impactPointX and impactPointZ must be in local space 
            if (impactPointX < minX || impactPointZ < minZ || impactPointX > maxX || impactPointZ > maxZ)
            {
                pixelX = -1;
                pixelZ = -1;
                return;
            }

            // x pixel at index 0 == left most pixel, that is why we must convert it from bottom most pixel by reversing
            pixelX = (int)(tileCountX * Utilities.InterpolationHelper.LinearMapValue(minX, maxX, impactPointX));
            pixelX = Math.Abs(pixelX - (int)tileCountX + 1); 

            // z pixel at index 0 == top most pixel, whereas in our tile, z is bottom most
            pixelZ = (int)(tileCountZ * Utilities.InterpolationHelper.LinearMapValue(minZ, maxZ, impactPointZ));
                         
        }

        public static void TileMaskGrid_SetRandomData(int textureIndex, int width, int depth)
        {

            //  iterate and set random values to the atlas lookup
            CoreClient._CoreClient.TextureFactory.LockTexture(textureIndex, false);
            for (int i = 0; i < width; i++)
                for (int j = 0; j < depth; j++)
                {
                    // TODO: here the function to deermine r,g,b a, could be a function
                    float r = 0, g = 0, b = 0, a = 1.0f;
                    r = Utilities.RandomHelper.RandomNumber(0, 7) / 255f;
                    int color = CoreClient._CoreClient.Globals.RGBA(r, g, b, a);
                    CoreClient._CoreClient.TextureFactory.SetPixel(textureIndex, i, j, color);
                    // System.Diagonostics.Debug.Assert (color == CoreClient._CoreClient.TextureFactory.GetPixel(textureIndex, i, j));
                }
            CoreClient._CoreClient.TextureFactory.UnlockTexture(textureIndex);
        }
        #endregion
    }
}
