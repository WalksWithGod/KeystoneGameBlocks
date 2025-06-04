using System;
using System.Collections.Generic;
using Keystone.Types;
using Keystone.Cameras;

namespace Keystone.FX
{
    public struct KeyFrame
    {
        public float Time;
        
    }
    
    // E:\dev\_projects\_AI\SharpSteer2-master\SharpSteer2\trail.cs  

    // http://hollowfear.com/development/the-engine/#!prettyPhoto/10/
    // Hollow Fear Ribbon Trails
    // For melee weapon trails primarily(like a very obvious sword swing trail), I 
    // developed a ribbon trail system. The base implementation is rather simple.
    // You define the length of a ribbon with a number of frames of duration, 20 
    // frames for example. Then you create a triangle list(or strip) of 20 quads. 
    // Then each frame, you shift each pair of vertices(top and bottom of the trail)
    // over one, and you’re done. For trails of slow objects, that should do it. 
    // However, a swing of a sword for example, is a very fast movement, finished 
    // in only a few frames. That creates a very angular trail, rather than a smooth, 
    // circular one, as we’d expect in this case. So, on top of our base system, we 
    // have to implement some kind of smoothing, by inserting points between our 
    // base(control) points. First, I implemented Catmull-Rom spline smoothing, and 
    // although it worked well, the trail of sword swing looked unnatural. Next I 
    // smoothed it with Bezier spline. It looked better but still not perfect. 
    // Finally, I used B splines, and I was satisfied with the result. So now I use 
    // all 3 of them for different situations, as they all have a bit different 
    // characteristics. 

    // CONSIDER THE SCRIPT
    // Exhaust.cs
    // {
    //      // Initialize()
    //      // {
    //      //      // add properties and assign values
    //      //      
    //      //      // build our model in script
    //      //      // TODO: but shouldn't we want to build it using parameters stored
    //      //      // when changing parameters we want to change the underlying textures and such for instance
    //      //      // 
    //      //      // These core parameters for a given billboard, appearance or whatever
    //      //      // should be built in parameters for the most part.  
    //      //      // However this script represents a short cut way to do that
    //      //      // Sort of like our ProceduralHelper where we init star and pass in a bunch of
    //      //      // params, and internally it just builds up the proper series of models, meshes and appearances, textures, etc
    //      //      // 
    //      // }
    //      // this type of script we can design various parameters
    //      // that the plugin can modify... including the setting of 
    //      // various textures...
    //      // base_radius
    //      // tail_radius
    //      // length (at full throttle)
    //      // color
    //      // glow_texture
    //      // glow_halo_texture
    //      // thrust_texture[]
    //      // thrust_halo_texture
    //      // 
    //      // NOTE: we can load prefabs obviously, so what we need is to create these "Components"
    //      // and then allow user to select them from Components hierarchy in archive.
    //      // So this is how we have to start off I suppose... our engine exhaust plumes
    //      // must be entities and user can set parameters on it, but they cannot edit the model
    //      // unless they go outside of the game and into true edit mode and create a modded 
    //      // component that has the visuals they want.

    // 
    //      // seems to me what im describing really is a particle system
    //      // with for instance trail emitters
    //      // plume emitters systems that includes the glow, etc
    //      // with every single emitter type having ability to alter colors, max particles
    //      // etc.
    // }

    //  Keystone\Celestial\ProceduralHelper\CreateSmoothCircle(string id, int segments, float radius)
    // - uses mesh3d built with linestrip primitive and
    // @"pool\shaders\VolumeLines.fx";  
    // to create thick connected list of lines.  trianglestrip seems to not work on tvmesh.
    // - treat this like our Starfield and MotionFields where
    // ProceduralHelper.MoveMotionField()  moves the individual vertices and where a single Model\Mesh
    // represents an entire system.  So our "Trail" comprised of a list of volume lines should also
    // be simply a single Mesh3d that gets managed by a function to update the vertex locations and to
    // enable/disable/expand/contract the trail as needed.
    //      - NOTE: MoveMotionField() is currently called by Hud.cs as part of Hud.Render()
    //      - WARNING: but isn't motion field incredibly slow?  moving tvmesh verts seems to be very bad
    //       for performance.  maybe using minimesh is better.
    // ---
    // E:\dev\cpp\Projects\HomeWorldSrc\src\Win32\trails.c
    // ---
    //E:\dev\cpp\Projects\Freespace2_Open\code\weapon\trails.cpp
    //E:\dev\cpp\Projects\Freespace2_Open\code\ship\shipcontrails.cpp
    // --
    // TODO: rename ribbon trail since ribbons are continuous and unbroken whereas other types of smoke trails
    // may not be.
    // treat this as a MinimeshRender.cs  ?  Where we manage all Minimesh instances
    // and when we traverse, we add a new instance?  
    // The main question is, how to keep invidiual lasers and explosions and exhaust plumes
    // and ribbon trails, etc  as light weight as possible.  

    public class FXExhaustTrail : FXBase 
    {
        // uses a queue with fading done  
        private Queue <KeyFrame> _queue;
        private int _sampleFrequency; // the rate in updates per second that we will advance the circular queue
        private float _maxAge; // maximum age in ms before this part of the keyframe is removed
        private bool _fadeEnable; //will fade each portion in a linear way from 0 age to _maxAge


        public override void Update(double elapsedSeconds, RenderingContext context)
        {
            base.Update(elapsedSeconds, context);
        }

        public override void Render(RenderingContext context)
        {
            base.Render(context);
        }
    }
}
