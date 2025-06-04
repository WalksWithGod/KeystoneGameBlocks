using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KeyPluginEntityEdit2
{
    public partial class PlaybackControl : UserControl
    {
        private int mFrameCount;
        private int mCurrentFrame;
        public EventHandler PlayBack_Event;


        public PlaybackControl()
        {
            InitializeComponent();

            // init slider to 0 until an animation with a length > 0 is set.
            slider.Minimum = 0; 
            slider.Maximum = 0; 
        }

        public int CurrentFrame 
        {
            get { return mCurrentFrame; }
            set 
            { 
                mCurrentFrame = value;
                slider.Value = mCurrentFrame;
                slider.Text = mCurrentFrame.ToString() + "/" + mFrameCount.ToString();
            }
        }

        public int FrameCount
        {
            get { return mFrameCount; }
            set 
            {
                if (mFrameCount < 0) value = 0;

                mFrameCount = value;
                slider.Text = mCurrentFrame.ToString() + "/" + mFrameCount.ToString ();
                slider.Maximum = value;
            }
        }

        private void slider_ValueChanged(object sender, EventArgs e)
        {
            if (sender == this.slider) return; 
            if (PlayBack_Event != null)
                PlayBack_Event.Invoke(sender, e);
        }

        private void buttonFrameReverse_Click(object sender, EventArgs e)
        {
            if (PlayBack_Event != null)
                PlayBack_Event.Invoke(sender, e);
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            if (PlayBack_Event != null)
                PlayBack_Event.Invoke(sender, e);
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            if (PlayBack_Event != null)
                PlayBack_Event.Invoke(sender, e);
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            if (PlayBack_Event != null)
                PlayBack_Event.Invoke(sender, e);
        }

        private void buttonFrameAdvance_Click(object sender, EventArgs e)
        {
            if (PlayBack_Event != null)
                PlayBack_Event.Invoke(sender, e);
        }
    }
}
