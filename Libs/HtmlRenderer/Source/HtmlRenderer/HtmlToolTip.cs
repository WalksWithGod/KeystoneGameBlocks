// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they bagin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using HtmlRenderer.Entities;
using HtmlRenderer.Parse;
using HtmlRenderer.Utils;

namespace HtmlRenderer
{
    /// <summary>
    /// Provides HTML rendering on the tooltips.
    /// </summary>
    public class HtmlToolTip : ToolTip
    {
        #region Fields and Consts

        /// <summary>
        /// the container to render and handle the html shown in the tooltip
        /// </summary>
        private HtmlContainer _htmlContainer;

        /// <summary>
        /// the raw base stylesheet data used in the control
        /// </summary>
        private string _baseRawCssData;

        /// <summary>
        /// the base stylesheet data used in the panel
        /// </summary>
        private CssData _baseCssData;

        /// <summary>
        /// timer used to handle mouse move events when mouse is over the tooltip.<br/>
        /// Used for link handling.
        /// </summary>
        private Timer _linkHandlingTimer;

        /// <summary>
        /// the control that the tooltip is currently showing on.<br/>
        /// Used for link handling.
        /// </summary>
        private Control _associatedControl;

        /// <summary>
        /// the handle of the actual tooltip window used to know when the tooltip is hidden<br/>
        /// Used for link handling.
        /// </summary>
        private IntPtr _tooltipHandle;

        /// <summary>
        /// If to handle links in the tooltip (default: false).<br/>
        /// When set to true the mouse pointer will change to hand when hovering over a tooltip and
        /// if clicked the <see cref="LinkClicked"/> event will be raised although the tooltip will be closed.
        /// </summary>
        private bool _allowLinksHandling = true;

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        public HtmlToolTip()
        {
            OwnerDraw = true;

            _htmlContainer = new HtmlContainer();
            _htmlContainer.IsSelectionEnabled = false;
            _htmlContainer.IsContextMenuEnabled = false;
            _htmlContainer.AvoidGeometryAntialias = true;
            _htmlContainer.AvoidImagesLateLoading = true;
            _htmlContainer.LinkClicked += OnLinkClicked;
            _htmlContainer.RenderError += OnRenderError;
            _htmlContainer.Refresh += OnRefresh;
            _htmlContainer.StylesheetLoad += OnStylesheetLoad;
            _htmlContainer.ImageLoad += OnImageLoad;

            Popup += OnToolTipPopup;
            Draw += OnToolTipDraw;
            Disposed += OnToolTipDisposed;

            _linkHandlingTimer = new Timer();
            _linkHandlingTimer.Tick += OnLinkHandlingTimerTick;
            _linkHandlingTimer.Interval = 40;
        }

        /// <summary>
        /// Raised when the user clicks on a link in the html.<br/>
        /// Allows canceling the execution of the link.
        /// </summary>
        public event EventHandler<HtmlLinkClickedEventArgs> LinkClicked;

        /// <summary>
        /// Raised when an error occurred during html rendering.<br/>
        /// </summary>
        public event EventHandler<HtmlRenderErrorEventArgs> RenderError;

        /// <summary>
        /// Raised when aa stylesheet is about to be loaded by file path or URI by link element.<br/>
        /// This event allows to provide the stylesheet manually or provide new source (file or uri) to load from.<br/>
        /// If no alternative data is provided the original source will be used.<br/>
        /// </summary>
        public event EventHandler<HtmlStylesheetLoadEventArgs> StylesheetLoad;

        /// <summary>
        /// Raised when an image is about to be loaded by file path or URI.<br/>
        /// This event allows to provide the image manually, if not handled the image will be loaded from file or download from URI.
        /// </summary>
        public event EventHandler<HtmlImageLoadEventArgs> ImageLoad;

        /// <summary>
        /// Set base stylesheet to be used by html rendered in the panel.
        /// </summary>
        [Browsable(true)]
        [Description("Set base stylesheet to be used by html rendered in the control.")]
        [Category("Appearance")]
        public string BaseStylesheet
        {
            get { return _baseRawCssData; }
            set
            {
                _baseRawCssData = value;
                _baseCssData = CssParser.ParseStyleSheet(value, true);
            }
        }

        /// <summary>
        /// If to handle links in the tooltip (default: false).<br/>
        /// When set to true the mouse pointer will change to hand when hovering over a tooltip and
        /// if clicked the <see cref="LinkClicked"/> event will be raised although the tooltip will be closed.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [Description("If to handle links in the tooltip.")]
        [Category("Behavior")]
        public bool AllowLinksHandling
        {
            get { return _allowLinksHandling; }
            set { _allowLinksHandling = value; }
        }

        /// <summary>
        /// Gets or sets the max size the tooltip.
        /// </summary>
        /// <returns>An ordered pair of type <see cref="T:System.Drawing.Size"/> representing the width and height of a rectangle.</returns>
        [Browsable(true)]
        [Category("Layout")]
        [Description("If AutoSize or AutoSizeHeightOnly is set this will restrict the max size of the control (0 is not restricted)")]
        public Size MaximumSize
        {
            get { return Size.Round(_htmlContainer.MaxSize); }
            set { _htmlContainer.MaxSize = value; }
        }


        #region Private methods

        /// <summary>
        /// On tooltip appear set the html by the associated control, layout and set the tooltip size by the html size.
        /// </summary>
        private void OnToolTipPopup(object sender, PopupEventArgs e)
        {
            //Create fragment container
            _htmlContainer.SetHtml("<div class=htmltooltip>" + GetToolTip(e.AssociatedControl) + "</div>", _baseCssData);

            //Measure bounds of the container
            using (var g = e.AssociatedControl.CreateGraphics())
            {
                _htmlContainer.PerformLayout(g);
            }

            //Set the size of the tooltip
            e.ToolTipSize = new Size((int)Math.Ceiling(_htmlContainer.ActualSize.Width), (int)Math.Ceiling(_htmlContainer.ActualSize.Height));

            // start mouse handle timer
            if( _allowLinksHandling )
            {
                _associatedControl = e.AssociatedControl;
                _linkHandlingTimer.Start();
            }
        }

        /// <summary>
        /// Draw the html using the tooltip graphics.
        /// </summary>
        private void OnToolTipDraw(object sender, DrawToolTipEventArgs e)
        {
            if(_allowLinksHandling && _tooltipHandle == IntPtr.Zero)
            {
                // get the handle of the tooltip window using the graphics device context
                var hdc = e.Graphics.GetHdc();
                _tooltipHandle = Win32Utils.WindowFromDC(hdc);
                e.Graphics.ReleaseHdc(hdc);
            }

            e.Graphics.Clear(Color.White);
            _htmlContainer.PerformPaint(e.Graphics);
        }

        /// <summary>
        /// Propagate the LinkClicked event from root container.
        /// </summary>
        private void OnLinkClicked(object sender, HtmlLinkClickedEventArgs e)
        {
            if (LinkClicked != null)
            {
                LinkClicked(this, e);
            }
        }

        /// <summary>
        /// Propagate the Render Error event from root container.
        /// </summary>
        private void OnRenderError(object sender, HtmlRenderErrorEventArgs e)
        {
            if (RenderError != null)
            {
                RenderError(this, e);
            }
        }

        /// <summary>
        /// Propagate the stylesheet load event from root container.
        /// </summary>
        private void OnStylesheetLoad(object sender, HtmlStylesheetLoadEventArgs e)
        {
            if (StylesheetLoad != null)
            {
                StylesheetLoad(this, e);
            }
        }

        /// <summary>
        /// Propagate the image load event from root container.
        /// </summary>
        private void OnImageLoad(object sender, HtmlImageLoadEventArgs e)
        {
            if (ImageLoad != null)
            {
                ImageLoad(this, e);
            }
        }

        /// <summary>
        /// Handle html renderer invalidate and re-layout as requested.
        /// </summary>
        private void OnRefresh(object sender, HtmlRefreshEventArgs e)
        {
//            if (e.Layout)
//            {
//                if (InvokeRequired)
//                    Invoke(new MethodInvoker(PerformLayout));
//                else
//                    PerformLayout();
//            }
//            if (InvokeRequired)
//                Invoke(new MethodInvoker(Invalidate));
//            else
//                Invalidate();
        }

        /// <summary>
        /// Raised on link handling timer tick, used for:
        /// 1. Know when the tooltip is hidden by checking the visibility of the tooltip window.
        /// 2. Call HandleMouseMove so the mouse cursor will react if over a link element.
        /// 3. Call HandleMouseDown and HandleMouseUp to simulate click on a link if one was clicked.
        /// </summary>
        private void OnLinkHandlingTimerTick(object sender, EventArgs eventArgs)
        {
            try
            {
                var handle = _tooltipHandle;
                if (handle != IntPtr.Zero && Win32Utils.IsWindowVisible(handle))
                {
                    var mPos = Control.MousePosition;
                    var mButtons = Control.MouseButtons;
                    var rect = Win32Utils.GetWindowRectangle(handle);
                    if( rect.Contains(mPos) )
                    {
                        _htmlContainer.HandleMouseMove(_associatedControl, new MouseEventArgs(mButtons, 0, mPos.X - rect.X, mPos.Y - rect.Y, 0));
                    }
                }
                else
                {
                    _linkHandlingTimer.Stop();
                    _tooltipHandle = IntPtr.Zero;

                    var mPos = Control.MousePosition;
                    var mButtons = Control.MouseButtons;
                    var rect = Win32Utils.GetWindowRectangle(handle);
                    if( rect.Contains(mPos) )
                    {
                        if( mButtons == MouseButtons.Left )
                        {
                            var args = new MouseEventArgs(mButtons, 1, mPos.X - rect.X, mPos.Y - rect.Y, 0);
                            _htmlContainer.HandleMouseDown(_associatedControl, args);
                            _htmlContainer.HandleMouseUp(_associatedControl, args);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                OnRenderError(this, new HtmlRenderErrorEventArgs(HtmlRenderErrorType.General, "Error in link handling for tooltip", ex));                
            }
        }

        /// <summary>
        /// Unsubscribe from events and dispose of <see cref="_htmlContainer"/>.
        /// </summary>
        private void OnToolTipDisposed(object sender, EventArgs eventArgs)
        {
            Popup -= OnToolTipPopup;
            Draw -= OnToolTipDraw;
            Disposed -= OnToolTipDisposed;

            if(_htmlContainer != null)
            {
                _htmlContainer.LinkClicked -= OnLinkClicked;
                _htmlContainer.RenderError -= OnRenderError;
                _htmlContainer.Refresh -= OnRefresh;
                _htmlContainer.StylesheetLoad -= OnStylesheetLoad;
                _htmlContainer.ImageLoad -= OnImageLoad;
                _htmlContainer.Dispose();
                _htmlContainer = null;
            }

            if( _linkHandlingTimer != null )
            {
                _linkHandlingTimer.Dispose();
                _linkHandlingTimer = null;
            }
        }

        #endregion
    }
}
