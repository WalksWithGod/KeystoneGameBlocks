using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.IO;

namespace ImageBrowserDotnet 
{
    public partial class ImageViewer : UserControl
    {
        private ToolTip myToolTip; 
        private Image m_Image;
        private string mImagePath;

        private bool m_IsThumbnail;
        private bool m_IsActive;
        private string mCaption;
        private bool mShowCaption;
        private Font mFont; 

        public ImageViewer()
        {
            m_IsThumbnail = false;
            m_IsActive = false;

            InitializeComponent();
        }

        ~ImageViewer()
        {
            if (m_Image != null)
                try
                {
                    m_Image.Dispose();
                    myToolTip = null;
                }
                catch { }
        }

        /// <summary>
        /// Caption font
        /// </summary>
        public Font Font 
        {
            get { return mFont; }
            set { mFont = value;}
        }

        public Image Image
        {
            set { m_Image = value; }
            get { return m_Image; }
        }

        /// <summary>
        /// For Archived Mods, this is the full path within the zip archive including the filename
        /// In otherwords, this is the EntryName
        /// For File System Mods, it's the full path including filename.
        /// </summary>
        public string ImagePath
        {
            set 
            { 
                mImagePath = value;
                Caption = ImageFileName;
            }
            get 
            { 
            	return mImagePath; 
            }
        }
        
        string mModFolder;
        public string ImageModFolder
        {
        	get {return mModFolder;}
        	set {mModFolder = value;}
        }
        
        public string ImageFileName
        {
        	get 
        	{
        		if (m_Image == null) return null;
        		return Path.GetFileName (mImagePath);
        	}
        }

        /// <summary>
        /// This is equivalent to just a filename.
        /// </summary>
        public string Caption
        {
            get { return mCaption; }
            set 
            { 
                mCaption = value; 
                //
            }
        }

        /// <summary>
        /// If not showing the caption, we do still update the caption
        /// </summary>
        public bool ShowCaption
        {
            get { return mShowCaption; }
            set { mShowCaption = value; }
        }

        public bool IsActive
        {
            set 
            { 
                m_IsActive = value;
                this.Invalidate();
            }
            get { return m_IsActive; }
        }

        public bool IsThumbnail
        {
            set { m_IsThumbnail = value; }
            get { return m_IsThumbnail; }
        }

        public void ImageSizeChanged(object sender, ImageViewerEventArgs e)
        {
            this.Width = e.Size;
            this.Height = e.Size;
            this.Invalidate();
        }


        public void LoadImage(Stream stream, string resourceDescriptor, int width, int height)
        {
            Image tempImage = null;
            try
            {
                tempImage = Image.FromStream(stream);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }


            if (tempImage != null)
            {
                mImagePath = resourceDescriptor;

                // the width and height of the actual image
                int dw = tempImage.Width;
                int dh = tempImage.Height;

                // the entire thumbnail width and height
                int tw = width;
                int th = height;

                double zw = (tw / (double)dw);
                double zh = (th / (double)dh);
                double z = (zw <= zh) ? zw : zh;
                dw = (int)(dw * z);
                dh = (int)(dh * z);

                Graphics g = null;
                try
                {
                    if (m_Image != null)
                        try
                        {
                            m_Image.Dispose();
                            myToolTip = null;
                        }
                        catch { }

                    m_Image = new Bitmap(dw, dh);
                    g = Graphics.FromImage(m_Image);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(tempImage, 0, 0, dw, dh);

                    // caption gets updated whether we are showing it or not
                    Caption = System.IO.Path.GetFileName(resourceDescriptor);
                    myToolTip = new ToolTip ();
                    //myToolTip.ToolTipTitle = Caption;
                    myToolTip.SetToolTip(this, Caption);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
                finally
                {
                    if (g != null)
                        g.Dispose();
    
                    tempImage.Dispose();
                }
            }
        }

        public void LoadImage(string imageFilename, int width, int height)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(imageFilename, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            if (fs != null)
                try
                {
                    LoadImage(fs, imageFilename, width, height);
                }
                catch (Exception ex )
                {
                	System.Diagnostics.Debug.WriteLine(ex.Message);
                }
                finally 
                {
                    fs.Close();
                    fs.Dispose ();
                }  
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (g == null) return;
            if (m_Image == null) return;

            int dw = m_Image.Width;
            int dh = m_Image.Height;

            int tw = this.Width - 8; // remove border, 4+4 
            int th = this.Height - 8; // remove border, 4+4 

            if (mShowCaption)
            {
                // reserve space for the caption based on the font height
                th = th - mFont.Height - 2;
            }

            double zw = (tw / (double)dw);
            double zh = (th / (double)dh);
            double z = (zw <= zh) ? zw : zh;

            dw = (int)(dw * z);
            dh = (int)(dh * z);
            int dl = 4 + (tw - dw) / 2; // add border 2+2
            int dt = 4 + (th - dh) / 2; // add border 2+2


            g.DrawRectangle(new Pen(Color.Gray), dl, dt, dw, dh);

            if (m_IsThumbnail)
                for (int j = 0; j < 3; j++)
                {
                    g.DrawLine(new Pen(Color.DarkGray),
                        new Point(dl + 3, dt + dh + 1 + j),
                        new Point(dl + dw + 3, dt + dh + 1 + j));
                    g.DrawLine(new Pen(Color.DarkGray),
                        new Point(dl + dw + 1 + j, dt + 3),
                        new Point(dl + dw + 1 + j, dt + dh + 3));
                }

            g.DrawImage(m_Image, dl, dt, dw, dh);

            // draw border if active
            if (m_IsActive)
            {
                g.DrawRectangle(new Pen(Color.White, 1), dl, dt, dw, dh);
                g.DrawRectangle(new Pen(Color.Blue, 2), dl - 2, dt - 2, dw + 4, dh + 4);
            }

            // draw text caption if enabled
            if (mShowCaption)
            {
                string wordWrappedCaption = WordWrap(mCaption, 9);
                //Size size = g.MeasureString(mCaption, mFont,);
                PointF p = new PointF(dl, dt + dh + 2);
                //RectangleF r = new RectangleF(dl, dt + dh + 3, tw, mFont.Height);

                string[] lines = wordWrappedCaption.Split(new string[]{_newline}, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length; i++)
                {
                    g.DrawString(lines[i], mFont, Brushes.Black, p);
                    p.Y = p.Y + (mFont.GetHeight() * .5f);
                }
                //g.DrawString(mCaption, mFont, Brushes.Black, r);
                //DrawMultilineString (g, mCaption, mFont, Brushes.Black, p, dw);
            }

            // Feb.3.2023 - added tooltips
            if (myToolTip == null)
            {
                myToolTip = new ToolTip();
                myToolTip.SetToolTip(this, mCaption);
            }
        }

        protected const string _newline = "\r\n";
        /// <summary>
        /// Word wraps the given text to fit within the specified width.
        /// </summary>
        /// <param name="text">Text to be word wrapped</param>
        /// <param name="width">Width, in characters, to which the text
        /// should be word wrapped</param>
        /// <returns>The modified text</returns>
        public static string WordWrap(string text, int width)
        {
            int pos, next;
            StringBuilder sb = new StringBuilder();
            // Lucidity check
            if (width < 1)
                return text;
            // Parse each line of text
            for (pos = 0; pos < text.Length; pos = next)
            {
                // Find end of line
                int eol = text.IndexOf(_newline, pos);
                if (eol == -1)
                    next = eol = text.Length;
                else
                    next = eol + _newline.Length;
                // Copy this line of text, breaking into smaller lines as needed
                if (eol > pos)
                {
                    do
                    {
                        int len = eol - pos;
                        if (len > width)
                            len = BreakLine(text, pos, width);
                        sb.Append(text, pos, len);
                        sb.Append(_newline);
                        // Trim whitespace following break
                        pos += len;
                        while (pos < eol && Char.IsWhiteSpace(text[pos]))
                            pos++;
                    } while (eol > pos);
                }
                else sb.Append(_newline); // Empty line
            }
            return sb.ToString();
        }
        /// <summary>
        /// Locates position to break the given line so as to avoid
        /// breaking words.
        /// </summary>
        /// <param name="text">String that contains line of text</param>
        /// <param name="pos">Index where line of text starts</param>
        /// <param name="max">Maximum line length</param>
        /// <returns>The modified line length</returns>
        public static int BreakLine(string text, int pos, int max)
        {
            // Find last whitespace in line
            int i = max - 1;
            while (i >= 0 && !Char.IsWhiteSpace(text[pos + i]))
                i--;
            if (i < 0)
                return max; // No whitespace found; break at maximum length
            // Find start of whitespace
            while (i >= 0 && Char.IsWhiteSpace(text[pos + i]))
                i--;
            // Return length of text before whitespace
            return i + 1;
        }
        private static SizeF DrawMultilineString(Graphics g, string s, Font font,
            Brush brush, PointF pos, float wrapWidth)
        {
            //List<string> aboutTexts = new List<string>();
            //for (int i = 0, lastStart = 0; ; )
            //{
            //    if (i >= s.Length)
            //    {
            //        if (lastStart < s.Length)
            //            aboutTexts.Add(s.Substring(lastStart));
            //        break;
            //    }
            //    else if (Environment.NewLine.IndexOf(s[i]) != -1)
            //    {
            //        aboutTexts.Add(s.Substring(lastStart, i - lastStart));
            //        lastStart = ++i;
            //    }
            //    else
            //        ++i;
            //}

            ////Draw each line, one at a time, wrapping it as we go.
            //foreach (string line in aboutTexts)
            //{
            string line = s;
                //Determine where we can wrap.
                List<int> wrapPos = new List<int>();
                for (int last = 0; last != -1; )
                {
                    int wrap = line.IndexOfAny(new char[] { ' ', '\t' }, last);
                    if (wrap != -1)
                        wrapPos.Insert(0, wrap++);
                    last = wrap;
                }

                List<string> wrapLines = new List<string>();
                List<int> reuseIndices = new List<int>();
                int lastIndex = 0;
                do
                {
                    string thisLine = line.Substring(lastIndex);
                    SizeF textSize = g.MeasureString(thisLine, font);
                    while (textSize.Width > wrapWidth)
                    {
                        reuseIndices.Add(wrapPos[0]);
                        thisLine = thisLine.Remove(wrapPos[0] - lastIndex);
                        textSize = g.MeasureString(thisLine, font);
                        wrapPos.RemoveAt(0);
                    }
                    wrapLines.Add(thisLine);
                    lastIndex += thisLine.Length + 1;
                    wrapPos.Clear();
                    wrapPos.AddRange(reuseIndices);
                    reuseIndices.Clear();
                }
                while (lastIndex < line.Length);

                //Then write the strings to screen.
                foreach (string wrapLine in wrapLines)
                {
                    SizeF wrapLineSize = g.MeasureString(wrapLine == string.Empty ?
                        " " : wrapLine, font);
                    g.DrawString(wrapLine, font, brush, pos);
                    pos.Y += wrapLineSize.Height;
                }
            //}

            return new SizeF(wrapWidth, pos.Y);
        }

        private void OnResize(object sender, EventArgs e)
        {
            this.Invalidate();
        }


    }
}
