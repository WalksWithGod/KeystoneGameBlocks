using System;


namespace ImageBrowserDotnet
{
    public class ImageViewerEventArgs : EventArgs
    {
        public ImageViewerEventArgs(int size)
        {
            this.Size = size;
        }

        public int Size;
    }

    public delegate void ImageViewerEventHandler(object sender, ImageViewerEventArgs e);

}
