using System;
using Keystone.Animation;
using Keystone.Elements;
using Keystone.Traversers;
using Keystone.Types;


namespace Keystone.Entities
{
    public class BonedEntity : SteerableEntity // ModeledEntity
    {

        //When using TVActor.BindToActor() this actor's bone's must match
        // with corresponding bones in the parent.
        private int[] _correspondingParentBones; 

        
        internal BonedEntity(string id)
            : base(id)
        {
            Visible = true;

        }


        #region ITraversable Members
        public override object Traverse(ITraverser target, object data)
        {
            return target.Apply(this, data);
        }
        #endregion 

        public int[] CorrespondingParentBones
        {
            get { return _correspondingParentBones; }
        }

        
        public void AddChild(BonedEntity child)
        {
            base.AddChild(child);

            // When using TVActor.BindToActor() this actor's bone's must match
            // with corresponding bones in the parent.
            // fortunately here, since the child is of same type we can directly access the private variables directly
            //string[] parentBoneNames = BonedModel.BoneNames;
            //string[] childBoneNames = child.BonedModel.BoneNames;
            //bool found = false;


            // NOTE: bone names are tied to the actor geometry.  This should be obvious.
            // So when changing a bone name, it's changed for all duplicates as well
            // and if that's not automatic, we need to verify that such a change does occur
            // in all duplicate actors.
            //child._correspondingParentBones = new int[child.BonedModel.BoneCount];
            //for (int i = 0; i < child._correspondingParentBones.Length; i ++)
            //{
            //    // itterate through every parent bone name and find a match we can assign to that child
            //    for (int j = 0; j < parentBoneNames.Length; j ++)
            //    {
            //        if (childBoneNames[i] == parentBoneNames[j])
            //        {
            //            child._correspondingParentBones[i] = j;
            //            found = true;
            //            break;
            //        }
            //    }
            //    if (!found) throw new Exception("No matching parent bone.");
            //    found = false;
            //}
        }

//        public void AddChild(Actor3d geometry)
//        {
//            base.AddChild(geometry);
//            //_animationController.Initialize(this);
//        }

        public void AddChild(Entity child, int boneID)
        {
            if (boneID >= 0)
                child.AttachedToBoneID = boneID;

            base.AddChild(child);
        }

        public override void RemoveChild(Node child)
        {
            base.RemoveChild(child);
            //if (child is Actor3d) // OBSOLETE - Actor3d must be child of Model.
            //{
               // _animationController.UnInitialize();
                //((Actor3d)child).UnRegisterDuplicate(_id);
            //}
        }
       
    }
}