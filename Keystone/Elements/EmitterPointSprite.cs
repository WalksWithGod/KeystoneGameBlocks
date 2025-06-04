//using System;
//using System.Collections.Generic;


//namespace Keystone.Elements
//{
//    public class EmitterPointSprite :Emitter 
//    {
//        // Emitters directly have either a minimesh or diffuse texture assigned to it... no appearance.
//        // 
//        // Since we have to use System.Duplicate() to share an entire system, we'll manage it exactly
//        // the way we duplicate actors.   THe problem is even though we can use 
//        // particleSystem.UpdateEx(iEmitterIndex, fTimeSeconds) 
//        // and even though this allows us to indpendantly update a SPECIFIC EMITTER and 
//        // from a desired keyframe time which we can also track ourselves,
//        // and particleSystem.SetEmitterMatrix()  allows us to restore an emitter at proper position and orientation 
//        // we still can't actually swap emitters between particle systems.  
//        // But at this point we wouldn't have to because our TVParticleSystem would be the only system we used
//        // and it would serve as another TVFactory class.
//
//        // However, two main flaws
//        // FLAW#2: There's no way to assign attractors to work with specific emitters
//        // FLAW#3: (NOT A DEAL BREAKER THOUGH, WE CAN SIMPLY JUST CALL UpdateEx() right before we actually
//        // render the emitter) -   Like TVActor, we would still need to call Emitter.Update(iEmitterIndex, fTimeSeconds) 
//        // in Render() because there's no way to retain the updated position information if we say tried to do a threaded
//        // Update pass where we compute the new vertex locations and then render in the main thread pass.
//        //
//        // Or just make do with TV's system as is.
//        // LIMITATION NOTE: TV's particle systems arent affected by physics. No particles bounding on a floor.
//        // LIMITATTION 2: TV's particles arent pickable
//        //
//        // So our options really are to make our own system using something like TVMinimesh for rendering the individual particles
//        // and for retaining our instance data... and being our "Geometry" node.  Or make do with TV
//        // and if sylvain does allow us the assign attractors to specific emitters, then im set.
//        public static EmitterPointSprite Create (string id) 
//        {
           
//        }

//    }

//    public class EmitterBillboard : Emitter 
//    {
//    }

//    public class EmitterMinimesh : Emitter 
//    {
//    }


//}
