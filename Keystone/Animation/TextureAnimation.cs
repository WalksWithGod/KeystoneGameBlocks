using System;
using System.Collections.Generic;
using Keystone.Resource;
using Keystone.Extensions;

namespace Keystone.Animation
{
    public class TextureAnimation : AnimationClip  
    {
        //private Dictionary<string, System.Drawing.Rectangle> mRectangles;

        // given a texture sheet target, stores an array of
        // the different texture indices in the spritesheet
        // which make up this animation.

        // this type of animation can be sourced to a 
        // SpriteSheet texture object from an Appearance layer 
        // and the TextureAnimation can simply
        // modify the UV of a quad for instance 
        // so that the textureID never has to change (cuz that is slow...)
        private System.Drawing.RectangleF[] mUVKeyFrames; // rectangular areas of the texture
                                  
        // TODO:  serializing a rectangle or a vector3d as an "attribute"
        // is easily done just as in x3d

        /// <summary>
        /// TODO: this cannot yet be shared so long as the base class AnimationClip.cs
        /// has non-shareable properties for assigning the Target of the animation!  
        /// Shareable UV Texture animation. AnimationTrack will persist state.
        /// </summary>
        /// <param name="id"></param>
        protected TextureAnimation (string id)
            : base(id)
        {
        }

        public static TextureAnimation Create(string id)
        {
            TextureAnimation anim;
            anim = (TextureAnimation)Repository.Get(id);
            if (anim != null) return anim;
            anim = new TextureAnimation(id);
            return anim;
        }

        #region ResourceBase members
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[1 + tmp.Length];
            tmp.CopyTo(properties, 1);

            properties[0] = new Settings.PropertySpec("keyframes", typeof(System.Drawing.RectangleF[]).Name);

            if (!specOnly)
            {
                properties[0].DefaultValue = mUVKeyFrames;
            }

            return properties;
        }

        public override void SetProperties(Settings.PropertySpec[] properties)
        {
            if (properties == null) return;
            base.SetProperties(properties);

            for (int i = 0; i < properties.Length; i++)
            {
                switch (properties[i].Name)
                {
                    // TODO: I think for the plugin, we need to pass in all keyframes at once when any single keyframe is modified 
                    case "keyframes":
                       mUVKeyFrames = (System.Drawing.RectangleF[])properties[i].DefaultValue;
                       // note: we propgate here and do not set our own. 
                       PropogateChangeFlags(Keystone.Enums.ChangeStates.TargetChanged, Keystone.Enums.ChangeSource.Child);
                       break;
                       // case "speed": // is speed apart of the overall animation and not just this interpolatin "clip"?
                       //   break;
                }
            }
        }
        #endregion

        public override int KeyFrameCount
        {
            get { if (mUVKeyFrames== null) return 0; return mUVKeyFrames.Length; }
        }

        public void DefineSpriteRectangle(System.Drawing.RectangleF rect)
        {
            // is the offset within the bounds of the texture?
            //if (string.IsNullOrEmpty(name)) throw new ArgumentNullException();
            //if (mRectangles == null) mRectangles = new Dictionary<string, System.Drawing.Rectangle>();
            //if (mRectangles.ContainsKey(name)) throw new Exception("Duplicate keys not allowed.");

            //mRectangles.Add(name, rect);

            if (mUVKeyFrames == null) mUVKeyFrames = new System.Drawing.RectangleF[0];
            mUVKeyFrames = mUVKeyFrames.ArrayAppend(rect);

        }

        public void RemoveKeyFrame(int index)
        {
            if (mUVKeyFrames == null || mUVKeyFrames.Length < index + 1) throw new ArgumentOutOfRangeException();

            mUVKeyFrames.RemoveAt(index);
        }

        //public void UnDefineSpriteRectangle(string name)
        //{
        //    if (string.IsNullOrEmpty(name)) throw new ArgumentNullException();
        //    if (mRectangles == null) throw new ArgumentOutOfRangeException();

        //    if (mRectangles.ContainsKey(name) == false) throw new ArgumentOutOfRangeException("Sprite not found.");

        //    mRectangles.Remove(name);

        //}

        public System.Drawing.RectangleF GetSpriteRect(int index)
        {
            if (mUVKeyFrames == null || mUVKeyFrames.Length < index + 1) throw new ArgumentOutOfRangeException();
            return mUVKeyFrames[index];
        }

        //public System.Drawing.Rectangle GetSpriteRect(string name)
        //{
        //    if (mRectangles == null) throw new ArgumentOutOfRangeException("Sprite not found.");
        //    System.Drawing.Rectangle result;
        //    bool found = mRectangles.TryGetValue(name, out result);

        //    return result;
        //}

        // requires a shader that supports "keyframe" and "sprite_rectangle" parameters
        internal override void Update(AnimationTrack track, object target)
        {
            if (target is Appearance.GroupAttribute == false) throw new ArgumentOutOfRangeException();
            if (target == null)throw new ArgumentNullException ();

            Appearance.GroupAttribute appearance = (Appearance.GroupAttribute)target;

    //        System.Diagnostics.Debug.Assert(track.KeyFrame >= 0.0 && track.KeyFrame <= 1.0);
            // compute new UV rect index
            int index = (int)Math.Floor(mUVKeyFrames.Length * track.Weight);
            //if (index == 0)
            //    System.Diagnostics.Debug.WriteLine("test");
            // this will result in a flag that shader parameters have changed 
            // once we go beyond HashCode and use better series of bitflags to denote exactly what
            // changed in appearance and what has not.
            //Clamp(index, mUVKeyFrames.Length - 1);
            if (index > mUVKeyFrames.Length - 1)
                index = mUVKeyFrames.Length - 1;
            System.Drawing.RectangleF rect = this.mUVKeyFrames[index];
            Keystone.Types.Vector4 v;
            v.x = rect.Left;
            v.y = rect.Top;
            v.z = rect.Width;
            v.w = rect.Height;

            // NOTE: this parameter must be set before Render() or else all instances
            //       will use the final value that was set in the shader.
            //       But we avoid this here by calling APPEARANCE.SetShaderParameterValue() 
            //       and NOT SHADER.SetShaderParameterValue() 
            //       APPEARANCE.SetShaderParameterValue() caches the changes
			//       until they can be applied just prior to render via Appearance or GroupAttribute
			//System.Diagnostics.Debug.WriteLine ("TextureAnimation.Update() - Updating Shader Parameter Value for Appearance " + appearance.ID);
            appearance.SetShaderParameterValue("textureUVInfo", v);


            // TODO: get the rect for that keyframe as current
            //    - eventually the rect UV needs to be updated on geometry.SetVertexUV()
            // so this target then has to be the 
            // class AnimatedTextureLayer : Layer {}?
            // since that will store UV coords that get set on apply
            // Vector2f[] UVCoordinates;

            // what if the object target is a shader?
            // computing the location of the next sprite in a spritesheet
            // and passing those UV offsets as shader params is easier
            // than trying to do all that in the shader i think....
            // 


        }
    }
}
