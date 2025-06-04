using System;

namespace ImageBrowserDotnet
{
    public class ImageBrowserEventArgs : EventArgs
    {
        public ImageBrowserEventArgs(string imageFilePath, string imageFilename) : this (imageFilePath, imageFilename, true)
        {
        }

        public ImageBrowserEventArgs(string imageFilePath, string imageFilename, bool isImageFile)
        {
        	this.ImageModFolder = imageFilePath;
            this.ImageFilePath = imageFilename;
            this.IsImageFile = isImageFile;
        }

        public string ImageFilePath;
        public string ImageModFolder;
        public bool IsImageFile;
    }

    public delegate void ImageBrowserEventHandler(object sender, ImageBrowserEventArgs e);
}
