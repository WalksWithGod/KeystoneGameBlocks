using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keystone.Elements;

namespace Keystone.IK
{

    //Joint -> if the joint is on the Entity, then it could work for all LODs
    //             but if a comparable LOD doesnt exist that would be a problem...
    //            <-- key is that the Joint must modify Transforms... of the bodies it connects
    //            <-- if it modifies a ModelSelector node, and the models in turn use it's parents Matrix
    //            this would then affect all sub-models in any LOD or Sequence
    //            in X3D, these nodes are not connected to a Model... instead they are maintained seperately
    //            as a RigidBodyCollection and connects to the transformable nodes via named keys.
    //            This is similar to how a lot of disconnected physics implementations work.
    //            Thus, ideally, an IK system (or physics system) should be operating on abstract things like
    //            PhysicsBody that are defined seperately, and which in turn affect the transforms of those bodies
    //            and then the Model's use those transforms.
    //            In other words, the Model's will grab the transforms from those Bodies which means we can
    //            easily have a 1:many Body:Models
				
    //        rotation
    //        constraints
    //PhysicalModel
    //    void Update(float elapsed);
    //    Body[] Bodies;  // Body.Transform <-- can be referenced by the Model... thus Model.Transform <-- read only and references Body.Transform 
    //        Body.Hull;
			
    //    Joint[] Joints; Joint.Body1, Joint.Body2
		
		// Everytime a new Joint or new Body is added, or anytime a change in a Joint is made
		// we iterate and re-Apply the transform references of the Models.

    // AnimationController
    //    Animation[] Animations; Animation.Target <-- for intrinsic animations or animation ranges based on intrinsics, this target cannot be changed
    //                                             <-- but for others, yes, we can easily change the target Models or ModelSelectors.
    //                                             So this means then what?  That Models and ModelSelectors do not have to host Animations at all.
    //                                             only the Entity's AnimationController does...
    //                                             and then we bind their targets to them by name.
    //                           
    /// <summary>
    /// The PhysicalModel of an Entity stands in contrast to the Visual Model (Model.cs)
    /// </summary>
    public class PhysicalModel : Group
    {
        Joint[] mJoints;
        Link[] mBodies;   

        public PhysicalModel(string id)
            : base(id)
        {
        }

        #region ITraverser Members
        public override object Traverse(Keystone.Traversers.ITraverser target, object data)
        {
            throw new NotImplementedException();
        }
        #endregion

        internal override Keystone.Traversers.ChildSetter GetChildSetter()
        {
            throw new NotImplementedException();
        }

        #region Group Members
        public void AddChild(Joint joint)
        { 
        }
        #endregion

        public void Update(float elapsedMilliseconds)
        {
 
        }
    }
}
