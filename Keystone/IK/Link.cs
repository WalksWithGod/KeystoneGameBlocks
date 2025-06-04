using System;
using System.Collections.Generic;
using Keystone.Elements;
using Keystone.Types;


namespace Keystone.IK
{
    /// <summary>
    /// In IK a Link is often referred to as a "Bone" since it's 
    /// commonly used in skeletal animation of avatars.  We'll use
    /// the more general term "Link" since we're not just linking limbs 
    /// and digits on virtual creatures, but also dealing with virtual robots
    /// and doors and machinery.
    /// </summary>
    public class Link : Node
    {
        string mName;  // this is not the same as ID, this is friendly Name and must only be unique within the context of a single entity
        // TODO: rename the class Transform to Transformable or TransformationNode
        //Transformation mTransform; 
        // Constraint[] mConstraints;
        Model mModel;// TODO: In short term just hardcode our Model instead of Transform
        Link mChild;   // if no child this is the end effector

        // can a quat be a goal?  that'd make it easier for some animations where we just rotate
        // and we use analytical lerp...
        Vector3d mGoal; // do we cache the goal here or pass it in to Update() everytime?
                        // we could hardcode a goal in script for  it in 

        Vector3d mEnd;  // used to compute direction and length


        // TODO: I suspect that a Link is our main class and
        // joints are just really constraints (unconstrained is 6DOF)
        // and so each link has a transform applied to it's origin and the effector is the other end
        // 

        // say user issues order to "open / close" engine blast doors
        // the command itself is obviously a game specific command as opposed to a framework command
        // mNetwork.SendMessage (Game_
        // Does it queue the calling of a game script on the server?
        // And where the entity script will know exactly how to manipulate the nodes.
        // Since we would have a bunch of named joints that the script would know the names of
        // since the scripts are tailored for those objects.
        // eg.  "Engine_01" or "Thruster_01" , "BlastDoorLeft" "BlastDoorTop" "BlastDoorBottom" "BlastDoorRight"
        // "Open" or "Close"
        // eg "Engine_01" "Thrust, 0" "Thrust, 1.0"
        // eg "Engine_01" GetState("Damage")  <-- returns our custom parameters but option to filter
        //
        // Message captainsOrder = new SystemOrder();
        // {
        //      Target = "USS Enterprise"; // you can specify other targets in your fleet or even potentially hack enemy ships
        //      SystemName = "Engine01";
        //      ParameterName = "Thrust";
        //      Value = "0.0";
        //      SecurityClearanceFlag;  // maybe Self Destruct command needs this...
        //                              // or if you are issuing orders to another ship where that
        //                              // captain has given you command, you can 
        //      Personell[]             // maybe list of personell carrying out this order is returned?
        //      Granted / Reason        // maybe a reply will include the reason it failed.. eg. no available crew, no qualified crew
        //                              // all crew busy with other tasks, etc., target system inoperative
        // }
        //
        // the above request gets sent and an access granted/denied returned from server.
        // upon which both the server and now your client will execute the named script of that entity.
        // thus... FormMain.Commands.Execute[captainsOrder];
        // which will then call mScene.Simulation.ExecuteClientScript[targetEntity, " ];
        // So we can just have a button on our MainForm that constructs and sends our
        // test command that will open / close the target entity's blast doors
        // this will lead to the execution of a client script
        // that will set an IKGoal on a body
        

        public Link(string id) : base(id) { }

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

        public Link Child { get { return mChild; } }

        public void SetName(string name)
        {
            // name can be Null or Empty however it
            // can NOT match any other non null Body name under this Entity.
            // This is because "Models" will assign themselves to Bodies or Joints
            // based on their friendly name but will not even search if their own
            // assigned body name is null or empty.

            // must revalidate names when
            // - attached to a new parent Entity
            // - when name change occurs

            mName = name;
        }
    }
}
