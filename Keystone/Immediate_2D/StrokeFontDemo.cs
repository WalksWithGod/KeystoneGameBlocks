// StrokeFontDemo.cs
// Copyright 2007 Michael Anderson
// Version 1.0 -- January 13, 2007


#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion


namespace StrokeFontDemo
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (StrokeFontDemo game = new StrokeFontDemo())
            {
                game.Run();
            }
        }
    }

    
    /// <summary>
    /// Main "game" class
    /// </summary>
    public class StrokeFontDemo : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        ContentManager content;
        
        List<Line> lineList = new List<Line>();
        List<Line> lineList2 = new List<Line>();
        LineManager lineManager = new LineManager();
        float globalLineRadius = 1.0f;
        
        Matrix viewMatrix;
        Matrix projMatrix;

        const float defaultXCamera = 120.0f;
        const float defaultYCamera = -90.0f;
        const float defaultRotCamera = 0.0f;
        const float defaultZoomCamera = 100.0f;
        float xCamera = defaultXCamera;
        float yCamera = defaultYCamera;
        float rotCamera = defaultRotCamera;
        float zoomCamera = defaultZoomCamera;

        bool wireframeButtonDown = false;
        bool showWireframe = false;

        bool useOtherStyleButtonDown = false;
        bool useOtherStyle = false;


        /// <summary>
        /// Constructor.
        /// </summary>
        public StrokeFontDemo()
        {
            graphics = new GraphicsDeviceManager(this);
            content = new ContentManager(Services);

            StrokeFont.AddString("Four score and seven years ago our fathers \nbrought forth on this continent a new nation, \nconceived in liberty, and dedicated to the \nproposition that all men are created equal.", lineList);

            bool monospace = true; // show how to do monospace (fixed-width) text
            float xOffset = 0;
            float yOffset = 0;
            for (char ch = StrokeFont.minChar; ch <= StrokeFont.maxChar; ch++)
            {
                float xOffsetSave = xOffset;
                StrokeFont.AddCharacter(ch, lineList2, ref xOffset, ref yOffset);
                if (monospace)
                    xOffset = xOffsetSave + StrokeFont.maxWidth;
                if (ch % 16 == 15)
                {
                    xOffset = 0;
                    yOffset -= StrokeFont.lineHeight;
                }
            }
        }


        /// <summary>
        /// Load graphics content.
        /// </summary>
        /// <param name="loadAllContent">Which type of content to load.</param>
        protected override void LoadGraphicsContent(bool loadAllContent)
        {
            if (loadAllContent)
            {
                lineManager.Init(graphics.GraphicsDevice, content);
            }

            Create2DProjectionMatrix();
        }


        /// <summary>
        /// Create a simple 2D projection matrix
        /// </summary>
        public void Create2DProjectionMatrix()
        {
            // Projection matrix ignores Z and just squishes X or Y to balance the upcoming viewport stretch
            float projScaleX;
            float projScaleY;
            float width = graphics.GraphicsDevice.Viewport.Width;
            float height = graphics.GraphicsDevice.Viewport.Height;
            if (width > height)
            {
                // Wide window
                projScaleX = height / width;
                projScaleY = 1.0f;
            }
            else
            {
                // Tall window
                projScaleX = 1.0f;
                projScaleY = width / height;
            }
            projMatrix = Matrix.CreateScale(projScaleX, projScaleY, 0.0f);
            projMatrix.M43 = 0.5f;
        }


        /// <summary>
        /// Unload graphics content.
        /// </summary>
        /// <param name="unloadAllContent">Which type of content to unload.</param>
        protected override void UnloadGraphicsContent(bool unloadAllContent)
        {
            if (unloadAllContent == true)
            {
                content.Unload();
            }
        }


        /// <summary>
        /// Read input and update state
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (gamePadState.Buttons.Back == ButtonState.Pressed ||
                keyboardState.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            if (gamePadState.Buttons.Start == ButtonState.Pressed ||
                keyboardState.IsKeyDown(Keys.Space))
            {
                // Reset camera
                xCamera = defaultXCamera;
                yCamera = defaultYCamera;
                rotCamera = defaultRotCamera;
                zoomCamera = defaultZoomCamera;
            }

            if (gamePadState.Buttons.A == ButtonState.Pressed ||
                keyboardState.IsKeyDown(Keys.W))
            {
                if (!wireframeButtonDown)
                {
                    wireframeButtonDown = true;
                    showWireframe = !showWireframe;
                }
            }
            else
            {
                wireframeButtonDown = false;
            }

            if (gamePadState.Buttons.B == ButtonState.Pressed ||
                keyboardState.IsKeyDown(Keys.B))
            {
                if (!useOtherStyleButtonDown)
                {
                    useOtherStyleButtonDown = true;
                    useOtherStyle = !useOtherStyle;
                }
            }
            else
            {
                useOtherStyleButtonDown = false;
            }

            float leftX = gamePadState.ThumbSticks.Left.X;
            if (keyboardState.IsKeyDown(Keys.Left))
                leftX -= 1.0f;
            if (keyboardState.IsKeyDown(Keys.Right))
                leftX += 1.0f;

            float leftY = -gamePadState.ThumbSticks.Left.Y;
            if (keyboardState.IsKeyDown(Keys.Up))
                leftY -= 1.0f;
            if (keyboardState.IsKeyDown(Keys.Down))
                leftY += 1.0f;

            float rightX = gamePadState.ThumbSticks.Right.X;
            if (keyboardState.IsKeyDown(Keys.A))
                rightX -= 1.0f;
            if (keyboardState.IsKeyDown(Keys.Z))
                rightX += 1.0f;

            float rightY = gamePadState.ThumbSticks.Right.Y;
            if (keyboardState.IsKeyDown(Keys.D))
                rightY -= 1.0f;
            if (keyboardState.IsKeyDown(Keys.C))
                rightY += 1.0f;

            // Adjust line thickness based on right stick vertical axis
            globalLineRadius = MathHelper.Clamp(globalLineRadius + rightY * elapsed, 0.01f, 5.0f);

            // Zoom the camera 
            float fZoom = 0.0f;
            if (gamePadState.Buttons.LeftShoulder == ButtonState.Pressed ||
                keyboardState.IsKeyDown(Keys.S))
            {
                fZoom += 1;
            }
            if (gamePadState.Buttons.RightShoulder == ButtonState.Pressed ||
                keyboardState.IsKeyDown(Keys.X))
            {
                fZoom -= 1;
            }
            zoomCamera *= (1.0f + elapsed * fZoom * 0.5f);

            // Rotate the camera
            rotCamera += -rightX * elapsed;

            // Move the camera, taking current rotation and zoom level into account
            Vector4 motionVector = new Vector4(leftX * elapsed * zoomCamera * 2, leftY * elapsed * zoomCamera * 2, 0, 1);
            Matrix matRot = Matrix.CreateRotationZ(-rotCamera);
            Vector4 rotatedMotionVector4 = Vector4.Transform(motionVector, matRot);
            xCamera += rotatedMotionVector4.X;
            yCamera -= rotatedMotionVector4.Y;

            viewMatrix = Matrix.CreateTranslation(-xCamera, -yCamera, 0) * Matrix.CreateRotationZ(-rotCamera) * Matrix.CreateScale(1.0f / zoomCamera, 1.0f / zoomCamera, 1.0f);

            base.Update(gameTime);
        }


        /// <summary>
        /// Draw the scene.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            if (showWireframe)
                graphics.GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;
            else
                graphics.GraphicsDevice.RenderState.FillMode = FillMode.Solid;

            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            // Use "global world matrices" to translate/rotate/scale an entire lineList
            Matrix globalWorldMatrix1 = Matrix.CreateTranslation(0, 0, 0);
            Matrix globalWorldMatrix2 = Matrix.CreateTranslation(0, -150, 0);

            string lineStyle = useOtherStyle ? "NoBlur" : "Standard";

            lineManager.Draw(lineList, globalLineRadius, Color.Yellow, viewMatrix, projMatrix, time, lineStyle, globalWorldMatrix1);
            lineManager.Draw(lineList2, globalLineRadius / 2, Color.Green, viewMatrix, projMatrix, time, lineStyle, globalWorldMatrix2);

            base.Draw(gameTime);
        }
    }
}
