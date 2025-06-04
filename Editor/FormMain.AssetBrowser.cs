using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Keystone.Cameras;
using Keystone.Commands;
using Keystone.Controllers;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.Types;
using Keystone.Appearance;
using Keystone.IO;
using Keystone.Resource;
using Keystone.Scene;
using Lidgren.Network;
using MTV3D65;
using System.Collections.Generic;
using Ionic.Zip;

namespace KeyEdit
{
    partial class FormMain : FormMainBase
    {


        // TODO: I should move this into a seperate ImageUtils dll
        /// Creates a resized bitmap from an existing image on disk.
        /// Call Dispose on the returned Bitmap object
        /// Public Domain
        /// - Rick Strahl
        ///
        /// Bitmap or null
        public static Bitmap CreateThumbnail(string lcFilename, int lnWidth, int lnHeight)
        {
            System.Drawing.Bitmap bmpOut = null;

            try
            {
                Bitmap loBMP = new Bitmap(lcFilename);
                System.Drawing.Imaging.ImageFormat loFormat = loBMP.RawFormat;

                decimal lnRatio;
                int lnNewWidth = 0;
                int lnNewHeight = 0;


                //*** If the image is smaller than a thumbnail just return it
                if (loBMP.Width < lnWidth && loBMP.Height < lnHeight)
                    return loBMP;

                if (loBMP.Width > loBMP.Height)
                {
                    lnRatio = (decimal)lnWidth / loBMP.Width;
                    lnNewWidth = lnWidth;
                    decimal lnTemp = loBMP.Height * lnRatio;
                    lnNewHeight = (int)lnTemp;
                }
                else
                {
                    lnRatio = (decimal)lnHeight / loBMP.Height;
                    lnNewHeight = lnHeight;
                    decimal lnTemp = loBMP.Width * lnRatio;
                    lnNewWidth = (int)lnTemp;
                }

                // GetThumbnailImage does use the CreateThumbnail method of GDI+ because it 
                //doesn’t properly convert transparent GIF images as it draws the background color black. 
                // The other code compensates for this by first drawing the canvas white then loading the GIF
                // image on top of it. Transparency is lost – unfortunately GDI+ does not handle transparency 
                // automatically and keeping Transparency intact requires manipulating the palette of the image 
                // which is beyond this demonstration.
                // - Rick Strahl

                // System.Drawing.Image imgOut =
                //      loBMP.GetThumbnailImage(lnNewWidth,lnNewHeight, null,IntPtr.Zero);


                // *** This code creates cleaner (though bigger) thumbnails and properly
                // *** and handles GIF files better by generating a white background for
                // *** transparent images (as opposed to black)

                bmpOut = new Bitmap(lnNewWidth, lnNewHeight);
                Graphics g = Graphics.FromImage(bmpOut);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.FillRectangle(Brushes.White, 0, 0, lnNewWidth, lnNewHeight);
                g.DrawImage(loBMP, 0, 0, lnNewWidth, lnNewHeight);
                loBMP.Dispose();


            }
            catch
            {
                return null;
            }
            return bmpOut;
        }
    }
}
