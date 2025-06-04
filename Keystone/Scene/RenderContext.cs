using System;
using Keystone.Cameras;
using Keystone.FX;

namespace Keystone.Scene
{
    //todo: WindowManager class shoudl contain a Viewport class that holds the Width/Heights and these need to be updated for anytype of viewport upon resize
// todo: I can add the Brushes and AxisIndicator meshes and such here and track their states here 
//   e.g. BeginArcBall when the user holds down the R(rotate key) and EndArcBall on release.
    ///  same with other Edit states such as BeginRaiseTerrain()  EndRaiseTerrain() etc
// 
// todo: RenderScene.Invoke  should accept a "name" of the pass being used so we can track / profile seperately
// todo: My Culler and Render's now will accept a context and use those values for determining what to draw
// todo: i suppose then that Pick() and  Element _Selected; 
// and such should all just 
    public class RenderContext
    {
        private Viewport _viewport;
        //Brush _currentBrush;

        private bool _wireframe;
        private bool _showBoundingBoxes;
        private bool _drawLightBoxes;

        private bool _showGrid;
        private bool _showAxisIndicator;
        private bool _previewMode; // disables texturing
        private bool _forceLowestLOD; //

        private bool _showFPS; // can enable or disable TVEngine's ShowFPS while running
        private bool _showProfiler; // can enable or disable TVEngine's profile debug output

        private bool _showRenderingStatistics;
        //todo: or add a "RenderingStatistics class that lists the things to output to debug along with position offsets for each

        private bool _totalPasses; // tracks how many sceneRender's are called.
        private bool[] _meshesRendered; // indices relate to the pass


        private bool[] _semanticsEnabled;
        private bool _throttleframerate;
        private int _frameRateLimit; // e.g. 15 fps

        public RenderContext(Viewport vp)
        {
            if (vp == null) throw new ArgumentNullException();
            _viewport = vp;
        }

        public bool WireFrame
        {
            get { return _wireframe; }
            set { _wireframe = value; }
        }

        public void EnableSemantic(FX_SEMANTICS semantic, bool value)
        {
        }
    }
}