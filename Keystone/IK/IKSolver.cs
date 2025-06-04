using System;
using System.Collections.Generic;
using Keystone.Types;

namespace Keystone.IK
{
    public class IKSolver
    {
        // we should use analytical solver for simple 2 links hinged 
        // but IKCCDSolver for chains

        // thread safe solver so we can do multiples
        // NOTE: for our analytical solution, we only need our interpolation functions
        // there's no need for IK at all... 
        // We simply need the infrastructure to initiate linear interpolation animations from
        // script.
        // We create such an animation instance
        // and now maybe using our "Link" we have a way to assign specific models (and their transforms)
        // to specific types of simple analytical transformations

        // TODO: part of what we should also be doing is seperating some functions from 
        // classes like Animation and making them much more data persisters and then running
        // the computations through thread safe static functions
        //public static void SolveLinearInterpolation (Link link, 
        
            
        public static void SolveIK(Link link, Vector3d goal, float elapsedMilliseconds)
        {
            if (link == null) throw new ArgumentNullException();
            if (elapsedMilliseconds < 0) throw new ArgumentOutOfRangeException();
            if (elapsedMilliseconds == 0) return;

            // find the last link as this will be our end effector
            Link effector = link;
            while (effector.Child != null)
                effector = effector.Child;

            // #if DEBUG
            // if (singleStep) // one step at a time
            //  break;
        }
    }
}
