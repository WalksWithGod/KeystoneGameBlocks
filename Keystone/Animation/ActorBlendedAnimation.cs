using System;
using System.Collections.Generic;
using MTV3D65;
using Keystone.Elements;

namespace Keystone.Animation
{
//    // http://www.truevision3d.com/forums/tv3d_sdk_65/code_for_the_problem-t11663.0.html;msg81578#msg81578

//    // Hello all,
//    //I am having an interesting problem with using the 3 blended layers (on top of the standard animation layer).
//    //In our project, we are "cycling animation layers" so basically you start by placing an animation on BlendedID = 0:
//    //tvActor.SetBlendedAnimationID(animationID, weight, 0);
//    //We then play this blended layer.
//    //As layers are added we set BlendedID= 1 and BlendedID = 2 and play those as well. When a fourth layer is needed we use the standard layer:
//    //tvActor.SetAnimationID(animationID);

//    //Now here comes the problem:
//    //When we try to add a fifth layer, we want to replace the BlendedID=0 layer. So we stop that blended layer and call SetBlendedAnimationID again (except this time with a different animation ID. We then play the layer.
//    //Now when we do this, it causes BlendedID 1 and 2 to stop playing and will also start playing the animation with ID = 0.
//    //I am not sure if I am misusing the blended animation layers but I would like to know if this is a problem with TV or me.
//    //BTW, I ended up shifting animations to lower blended levels if we needed to swap. While this didn't cause animation 0 to play, it does make you reset all animations.
//    //If anyone can help me with this I would appreciate it.

//    // ----------------
//    // Reply: From SYlvain
//    // BTW : it's better to always use first SetAnimationID for the "first" layer. - Sylvain
//    // 
//    // ------------------

//    // Reply: Sample code to show problme
//    //(c#)
//    //        private TVActor tvActor;
//    //        int anim1, anim2, anim3, anim4, anim5, anim6;

//    //public void TestTVActor()
//    //        {
//    //            tvActor = TVScene.CreateActor();
//    //            tvActor.LoadTVA("crazyBoxes.TVA");

//    //            //create new animations
//    //            anim1 = tvActor.AddAnimationRange(0, 35, 65);
//    //            anim2 = tvActor.AddAnimationRange(0, 70, 100);
//    //            anim3 = tvActor.AddAnimationRange(0, 105, 135);
//    //            anim4 = tvActor.AddAnimationRange(0, 140, 170);
//    //            anim5 = tvActor.AddAnimationRange(0, 175, 205);
//    //            anim6 = tvActor.AddAnimationRange(0, 210, 240);

//    //            LoadAnimations1();
//    //            LoadAnimations2();
//    //        }
//    //        public void LoadAnimations1()
//    //        {
//    //            tvActor.SetAnimationID(anim1); 
    // //           Hypnotron - I think that these sorts of blends
    // //           allow you to do things like have your turret rotation animation
    // //           play in conjunction with your turret barrel's elevation animations and with
    // //           a recoil animation when shooting.  
    // //           So in this case, all the weights will remain 1.
    // //
//    //                                           id,  weight, layer
//    //            tvActor.SetBlendedAnimationID(anim2, 1, 0);
//    //            tvActor.SetBlendedAnimationID(anim3, 1, 1);
//    //            tvActor.SetBlendedAnimationID(anim4, 1, 2);

//    //            tvActor.PlayAnimation();
//    //            tvActor.PlayBlendedAnimation(1, 0);
//    //            tvActor.PlayBlendedAnimation(1, 1);
//    //            tvActor.PlayBlendedAnimation(1, 2);
//    //        }
//    //        public void LoadAnimations2()
//    //        {
    // 
     ////    When TestTVActor() is called, it will try to play animations on the SetAnimationID 
     ////and the 3 blended layers (this is done inside LoadAnimations1). Then LoadAnimations2() 
     ////will try to change the first blended layer (BlendedID = 0) and will cause BlendedID=1
     ////and BlendedID=2 to stop playing and start playing animation 0 from the actor.
 


//    //            tvActor.SetBlendedAnimationID(anim5, 1, 0);
//    //            tvActor.PlayBlendedAnimation(1, 0);
//    //        }

    public class ActorBlendedAnimation : BonedAnimation
    {
        public ActorBlendedAnimation(string id) : base(id)
        {
            TVActor actor;
            //actor.SetBlendedAnimationID (0, weight, layer)
            
        }


//        public void SetBlendedAnimationByName(string sAnimationName)
//        {
//            _actor._actor.SetBlendedAnimationByName(sAnimationName);
//        }

//        public void SetBlendedAnimationByName(string sAnimationName, float fWeight)
//        {
//            _actor._actor.SetBlendedAnimationByName(sAnimationName, fWeight);
//        }

//        public void SetBlendedAnimationByName(string sAnimationName, float fWeight, int iBlendedAnimationLayer)
//        {
//            _actor._actor.SetBlendedAnimationByName(sAnimationName, fWeight, iBlendedAnimationLayer);
//        }

//        public string BlendedAnimationByName
//        {
//            get { return mBlendedAnimationByName; }
//            set
//            {
//                mBlendedAnimationByName = value;
//                _actor._actor.SetBlendedAnimationByName(value);
//            }
//        }

//        public int BlendedAnimationID
//        {
//            get { return mBlendedAnimationID; }
//            set
//            {
//                mBlendedAnimationID = value;
//                _actor._actor.SetBlendedAnimationID(value);
//            }
//        }

//        public void SetBlendedAnimationID(int iAnimationID)
//        {
//            _actor._actor.SetBlendedAnimationID(iAnimationID);
//        }

//        public int GetBlendedAnimationID(int iBlendedAnimationLayerID)
//        {
//            return _actor._actor.GetBlendedAnimationID(iBlendedAnimationLayerID);
//        }

//        public void SetBlendedAnimationID(int iAnimationID, float fWeight)
//        {
//            _actor._actor.SetBlendedAnimationID(iAnimationID, fWeight);
//        }

//        public void SetBlendedAnimationID(int iAnimationID, float fWeight, int iBlendedAnimationLayer)
//        {
//            _actor._actor.SetBlendedAnimationID(iAnimationID, fWeight, iBlendedAnimationLayer);
//        }

//        public float BlendedAnimationKeyFrame
//        {
//            get { return _actor._actor.GetBlendedAnimationKeyFrame(); }
//            set { _actor._actor.SetBlendedAnimationKeyFrame(value); }
//        }

//        public float GetBlendedAnimationKeyFrame()
//        {
//            return _actor._actor.GetBlendedAnimationKeyFrame();
//        }

//        public void SetBlendedAnimationKeyFrame(float fKeyFrame)
//        {
//            _actor._actor.SetBlendedAnimationKeyFrame(fKeyFrame);
//        }

//        public void SetBlendedAnimationKeyFrame(float fKeyFrame, int iBlendedAnimationLayer)
//        {
//            _actor._actor.SetBlendedAnimationKeyFrame(fKeyFrame, iBlendedAnimationLayer);
//        }

//        public float GetBlendedAnimationKeyFrame(int iBlendedAnimationID)
//        {
//            return _actor._actor.GetBlendedAnimationKeyFrame(iBlendedAnimationID);
//        }


//        public float BlendedAnimationWeight
//        {
//            get { return mBlendedAnimationWeight; }
//            set
//            {
//                mBlendedAnimationWeight = value;
//                _actor._actor.SetBlendedAnimationWeight(value);
//            }
//        }

//        public void SetBlendedAnimationWeight(float fWeight)
//        {
//            _actor._actor.SetBlendedAnimationWeight(fWeight);
//        }

//        public void SetBlendedAnimationWeight(float fWeight, int iBlendedAnimationLayer)
//        {
//            _actor._actor.SetBlendedAnimationWeight(fWeight, iBlendedAnimationLayer);
//        }

//        public bool BlendedAnimationLoop
//        {
//            get { return mBlendedAnimationLoop; }
//            set
//            {
//                mBlendedAnimationLoop = value;
//                _actor._actor.SetBlendedAnimationLoop(value);
//            }
//        }

//        public void SetBlendedAnimationLoop(bool bLooping)
//        {
//            _actor._actor.SetBlendedAnimationLoop(bLooping);
//        }

//        public void SetBlendedAnimationLoop(bool bLooping, int iBlendedAnimationLayer)
//        {
//            _actor._actor.SetBlendedAnimationLoop(bLooping, iBlendedAnimationLayer);
//        }


//        public void PlayBlendedAnimation(float fAnimationSpeed)
//        {
//            _actor._actor.PlayBlendedAnimation(fAnimationSpeed);
//        }

//        public void PlayBlendedAnimation(float fAnimationSpeed, int iBlendedAnimationLayer)
//        {
//            _actor._actor.PlayBlendedAnimation(fAnimationSpeed, iBlendedAnimationLayer);
//        }

//        public void PauseBlendedAnimation()
//        {
//            _actor._actor.PauseBlendedAnimation();
//        }

//        public void PauseBlendedAnimation(int iBlendedAnimationLayer)
//        {
//            _actor._actor.PauseBlendedAnimation(iBlendedAnimationLayer);
//        }

//        public void StopBlendedAnimation()
//        {
//            _actor._actor.StopBlendedAnimation();
//        }

//        public void StopBlendedAnimation(int iBlendedAnimationLayer)
//        {
//            _actor._actor.StopBlendedAnimation(iBlendedAnimationLayer);
//        }

    }
}