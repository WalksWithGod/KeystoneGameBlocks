using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keystone.Appearance
{
    internal class AnimatedTextureLayer : Layer
    {
        // TODO: i dont think this is necessary at all.
        // I think the TextureAnimation.cs will store the seperate Frames rectangles
        // and those can get passed to the shader prior to render... just not sure yet
        // how we pass those prior to render from a TextureAnimation.cs object
        // the Shader Parameters would have to be updated in the Model 
        // itself.  
        protected AnimatedTextureLayer (string id): base (id)
        {}
    }

    public class SpriteSheet : Diffuse 
    {
        
        private Dictionary<string, System.Drawing.Rectangle> mRectangles;

        public SpriteSheet(string id)
            : base(id)
        { 
        }

        public void DefineSpriteRectangle (string name, System.Drawing.Rectangle rect)
        {
            // is the offset within the bounds of the texture?
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException();
            if (mRectangles == null) mRectangles = new Dictionary<string, System.Drawing.Rectangle>();
            if (mRectangles.ContainsKey(name)) throw new Exception("Duplicate keys not allowed.");

            mRectangles.Add(name, rect);
            
        }

        public void UnDefineSpriteRectangle(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException();
            if (mRectangles == null) throw new ArgumentOutOfRangeException();

            if (mRectangles.ContainsKey(name) == false) throw new ArgumentOutOfRangeException("Sprite not found.");

            mRectangles.Remove(name);

        }

        public System.Drawing.Rectangle GetSpriteRect(string name)
        {
            if (mRectangles == null) throw new ArgumentOutOfRangeException("Sprite not found.");
            System.Drawing.Rectangle result;
            bool found = mRectangles.TryGetValue(name, out result);
            
            return result;
        }
    }
}
