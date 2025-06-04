using System;
using System.ComponentModel;
using MTV3D65;

namespace Keystone.Devices
{
    public enum FSAASamples
    {
        F_8X,
        F_6X,
        F_4X,
        F_2X
    }

    public interface IWindow
    {
        void Show();
        void Hide();
        IntPtr Handle { get; }
        CancelEventHandler Closing { get; }
    }

    public class GraphicsDevice
    {
        public bool Fullscreen;
        public string DisplayMode;

        public CONST_TV_DEPTHBUFFERFORMAT DepthBuffer;

        public bool IsInitialized;
        private IntPtr _handle;

        public bool Vsync;

        public float Gamma = 1.5F;
        // 0 to 10 and for fullscreen only. Typically anything over 2.0 is going to look severely over exposed

        public bool EnableTNL = true; // for older cards, you might need to excplicitly enable this
        public bool FSAA;
        public FSAASamples FsaaSamples;
        public uint Adapter;
        public int BPP;
        public int Width;
        public int Height;

        public GraphicsDevice(IntPtr handle)
        {
            _handle = handle;
            DepthBuffer = CONST_TV_DEPTHBUFFERFORMAT.TV_DEPTHBUFFER_BESTBUFFER;
        }


        public void SwitchFullScreen()
        {
            Width = FullscreenWidth;
            Height = FullscreenHeight;
            BPP = ColorDepth;

            if (IsInitialized)
                CoreClient._CoreClient.Engine.SwitchFullscreen(Width, Height, BPP, DepthBuffer, Handle);
            else
                //NOTE: you really shouldn't set gamma in the init call.  Best time would be after the scene is loaded. (even if its a GUI screen)
                CoreClient._CoreClient.Engine.Init3DFullscreen(Width, Height, BPP, EnableTNL, Vsync,
                                                   DepthBuffer, Gamma, Handle);


            TVGraphicEffect gamma = new TVGraphicEffect();
            gamma.ChangeGamma(Gamma);

            Validate();
            IsInitialized = true;
            Fullscreen = true;
        }

        public void SwitchWindow()
        {
            if (IsInitialized)
                CoreClient._CoreClient.Engine.SwitchWindowed(Handle);
            else if (IntPtr.Zero == Handle)
                return;
            else
            {
                // note: an access violation on Init3dWindowed is either TVLicense Sign needing to be re-run 
                // or the desktop color depth is too low.
                //CoreClient._CoreClient.Engine.SetMainBufferAlpha(true); 
                CoreClient._CoreClient.Engine.Init3DWindowed(Handle, EnableTNL);
                CoreClient._CoreClient.Engine.GetViewport().SetAutoResize(false);
                //TODO: Width and Height must be set here.  ISSUE: GetViewportMode returns actual renderable area
                // and does not take into account the client window's full rect.  Which do we really care about though?
                CoreClient._CoreClient.Engine.GetVideoMode(ref Width, ref Height, ref BPP);
            }

            Validate();
            IsInitialized = true;
            Fullscreen = false;
        }

        public void Toggle()
        {
            if (IsInitialized)
            {
                if (Fullscreen)
                    SwitchWindow();
                else
                    SwitchFullScreen();
            }
        }

        public void SetAdapter(int id)
        {
            CoreClient._CoreClient.Engine.SetInitAdapter(id);
        }

        private void Validate()
        {
            int h = 0, w = 0, color = 0;
            CoreClient._CoreClient.Engine.GetVideoMode(ref w, ref h, ref color);
            //Trace.Assert(BPP == color);
            //Trace.Assert(Width == w);
            //Trace.Assert(Height == h);
        }

        public IntPtr Handle
        {
            get { return _handle; }
        }

        // reads the color depth from our display mode string in our settings dialog.
        private int ColorDepth
        {
            get
            {
                int delim = DisplayMode.IndexOf("=", 0);
                int depth = Convert.ToInt32(DisplayMode.Substring(delim + 1));
                return depth;
            }
        }

        private int FullscreenWidth
        {
            get
            {
                int delim = DisplayMode.IndexOf("x", 0);
                int width = Convert.ToInt32(DisplayMode.Substring(0, delim - 1));
                return width;
            }
        }

        private int FullscreenHeight
        {
            get
            {
                int firstX = DisplayMode.IndexOf("x", 0);
                int secondX = DisplayMode.IndexOf(" ", firstX + 2);
                int height = Convert.ToInt32(DisplayMode.Substring(firstX + 1, secondX - firstX));
                return height;
            }
        }

        public CONST_TV_MULTISAMPLE_TYPE MultisampleType
        {
            get
            {
                switch (FsaaSamples)
                {
                    case FSAASamples.F_2X:
                        return CONST_TV_MULTISAMPLE_TYPE.TV_MULTISAMPLE_2_SAMPLES;
                    case FSAASamples.F_4X:
                        return CONST_TV_MULTISAMPLE_TYPE.TV_MULTISAMPLE_4_SAMPLES;
                    case FSAASamples.F_6X:
                        return CONST_TV_MULTISAMPLE_TYPE.TV_MULTISAMPLE_6_SAMPLES;
                    default:
                        return CONST_TV_MULTISAMPLE_TYPE.TV_MULTISAMPLE_8_SAMPLES;
                }
            }
        }
    }
}