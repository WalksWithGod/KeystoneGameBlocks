using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keystone.Scene
{
    /// <summary>
    /// Partial Scenes are subset of a Primary Scene.  
    /// They rely on the fact that there is already a primary scene
    /// that contains paging, simulation, XMLDB references, etc.  This scene is only a 
    /// sort of isolated view of a specific part of a scene.  Thus it's useful for things like
    /// Solar System views, planetary surveys, target zoom mfds, etc.
    /// </summary>
    public class PartialScene
    {
        public Scene mSourceScene;

        // TODO: but wait, what if we want to view a part of the scene that goes out of
        // scope or isn't even in scope?  Like viewing various solar systems?
        // Maybe the only important thing is that instead of loading a scene
        // here we pass in an existing one
        public PartialScene(Scene sourceScene)
        {

        }
    }
}
