//using System;
//using Keystone.Animation;
//using Keystone.Entities;
//using Keystone.Types;
//using MTV3D65;
//using Keystone.Resource;
//using System.Xml;

//namespace Keystone.Elements
//{
//    public class BonedModel : LODModel
//    {
//        private TV_3DMATRIX[] _bones;
//        public AnimationManager _animationManager;

//        internal BonedModel(string id)
//            : base(id)
//        {
//        }
//        public static BonedModel Create(string id)
//        {
//            BonedModel model = (BonedModel)Repository.Get(id);
//            if (model != null) return model;
//            model = new BonedModel(id);
//            return model;
//        }

//        public static BonedModel Create(string id)
//        {
//            BonedModel model;
//            model = (BonedModel)Repository.Get(id);
//            if (model != null) return model;
//            model = new BonedModel(id);

//            return model;
//        }

//        // How do animations and attachments work with LOD?

//        // IMPORTANT: Recall the main purpose of a "model" is to share instances and to have LOD management built in.
//        // For LOD actors, the keyframes have to be the same, but as Sylvain indicated, you can
//        // have fewer bones in the lower LOD actor.  So lower LOD geometry and lower LOD bones but same
//        // number of keyframes and animations.  This is good.  We can do this. HOWEVER, the different bone ID's for attaching things 
//        // could be annoying... but maybe not as bad if we go by bone "name" and then do a mapping across the LODs.

//        // for LOD actors or Sharing Actors, we can sync them by getting/setting the proper KeyFrame
//        // stored in the per instance parent Entity object
//        // NOTE: We would only want to deal directly with Get/Set BoneMatrix if we were animating our actor
//        // through physics or something.  But if we're only using keyframe animation, then this is simplest/best way.
//        //http://www.truevision3d.com/phpBB2/viewtopic.php?t=15261

//        // on the Geometry actor3d's update (EntityBase entity), it should store the resulting
//        // keyframe in the entity.Keyframe = result; 
//        // prior to rendering, the keyframe will be set to the Geometry.
//        // 
//        private Actor3d _actor
//        {
//            get { return (Actor3d) Geometry; }
//            //if (Geometry is LODSwitch) 
//            //        // TODO: this is kinda wrong.
//            //    // we sometimes want the first child but other times for updating
//            //    // animations and such we want the proper LOD _if_ its an Actor3d and say not a Billboard...
//            //    // so how to handle all this?  I suspect for a BoneModel no billboard would ever be used?
//            //    // but assuming this, how do we select the proper child?
//            //    // if we get rid of this private method and force the caller (e.g. Simulation.Update)
//            //    // to select the child and to then handle all the proper animation handling ... then this could work.
//            //else
//            //    return (Actor3d)Geometry;
//        }

//        public AnimationManager AnimationManager
//        {
//            get { return _animationManager; }
//        }


//        // TODO: this should grab the bonematrix out of hte duplicate i think
//        public Matrix GetBoneMatrix(int i, bool modelSpace)
//        {
//            return Helpers.TVTypeConverter.FromTVMatrix(_actor.GetBoneMatrix(i, modelSpace));
//        }

//        public void SetBones()
//        {
//            for (int i = 0; i < _bones.Length; i++)
//            {
//                // TODO: this needs to work with a proper duplicate name
//                _actor.SetBoneMatrix(i, _bones[i], false);
//            }
//        }

//        public int BoneCount
//        {
//            get
//            {
//                if (_actor == null) return 0;
//                return _actor.GetBoneCount();
//            }
//        }

//        public string[] BoneNames
//        {
//            get
//            {
//                int count = BoneCount;
//                if (count == 0) return null;
//                string[] names = new string[count];

//                for (int i = 0; i < count; i++)
//                    names[i] = _actor.GetBoneName(i);

//                return names;
//            }
//        }



//        #region obsolete now that we've given up on Duplicate management in a single Actor3d
//        //
//        //public void RegisterDuplicate(string entityName)
//        //{
//        //    _actor.RegisterDuplicate(entityName);
//        //}

//        //public void UnRegisterDuplicate(string entityName)
//        //{
//        //    _actor.UnRegisterDuplicate(entityName);
//        //}

//        //#region Actor Specific Bounding Volume functions that accomodate duplicates
//        //public bool DuplicateBoundVolumeIsDirty(string entityID)
//        //{
//        //    return _actor.DuplicateBoundVolumeIsDirty(entityID);
//        //}

//        //public BoundingBox DuplicateBoundingBox(string entityID)
//        //{
//        //    return _actor.DuplicateBoundingBox(entityID);
//        //}

//        //public BoundingSphere DuplicateBoundingSphere(string entityID)
//        //{
//        //    return _actor.DuplicateBoundingSphere(entityID);
//        //}
//        //#endregion 

//        //#region IBoundVolume Members
//        //public override bool BoundVolumeIsDirty
//        //{
//        //    get
//        //    {
//        //        throw new Exception("Actor3d.BoundVolumeIsDirty() - Use .BoundVolumeIsDirty(entityID)");
//        //    }
//        //}

//        //public override BoundingBox BoundingBox
//        //{
//        //    get
//        //    {
//        //        throw new Exception("Actor3d.BoundVolumeIsDirty() - Use .BoundVolumeIsDirty(entityID)");
//        //    }
//        //}

//        //public override BoundingSphere BoundingSphere
//        //{
//        //    get
//        //    {
//        //        throw new Exception("Actor3d.BoundVolumeIsDirty() - Use .BoundVolumeIsDirty(entityID)");
//        //    }
//        //}
//        //#endregion 
//        #endregion
//    }
//}