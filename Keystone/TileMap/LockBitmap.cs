using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Keystone.TileMap
{
	// http://www.codeproject.com/Tips/240428/Work-with-bitmap-faster-with-Csharp
	// Vano Maisuradze, 15 Aug 2011
	public class LockBitmap : IDisposable
	{
	    string mSourcePath;
		Bitmap mSource = null;
	    BitmapData mBitmapData = null;
	 	IntPtr mPtr = IntPtr.Zero;
	 
	 	public string FilePath {get {return mSourcePath;}}
	    public Bitmap Bitmap {get {return mSource;} private set {mSource = value;}}
	    public byte[] Pixels { get; set; }
	    #if USE_MULTI_DIMENSIONAL_ARRAY
	    public byte[,] Pixels {get; set;}
	    #endif
	    
	    public int ColorDepth { get; private set; }
	    public int Width { get; private set; }
	    public int Height { get; private set; }
	 

	    public LockBitmap (string filePath)
	    {
	    	mSourcePath = filePath;
	    	
	    	System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();

			byte[] fileBytes = System.IO.File.ReadAllBytes(mSourcePath);
			memoryStream.Write(fileBytes, 0, fileBytes.Length);
			memoryStream.Position = 0;

	    	mSource = new System.Drawing.Bitmap (memoryStream );
	    	memoryStream.Dispose();
	    	
	    	//System.Drawing.Bitmap tmp = new Bitmap (mSourcePath);
	    	//mSource = new System.Drawing.Bitmap (tmp, tmp.Width, tmp.Height, tmp.PixelFormat);
	 
	    	// Get width and height of bitmap
            Width = mSource.Width;
            Height = mSource.Height;
            
            LockBits();
	    
            //tmp.Dispose();
	    }
	    

	    /// <summary>
	    /// Lock bitmap data so that data can be read from the bitmap.
	    /// </summary>
	    private void LockBits()
	    {
	        try
	        {	 
	            // get total locked pixels count
	            int PixelCount = Width * Height;
	 
	            // Create rectangle to lock
	            Rectangle rect = new Rectangle(0, 0, Width, Height);
	 
	            // get source bitmap pixel format size
	            ColorDepth = System.Drawing.Bitmap.GetPixelFormatSize(mSource.PixelFormat);
	 
	            // Check if bpp (Bits Per Pixel) is 8, 24, or 32
	            if (ColorDepth != 8 && ColorDepth != 24 && ColorDepth != 32)
	            {
	                throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
	            }
	 
	            //http://stackoverflow.com/questions/6020406/travel-through-pixels-in-bmp
	            // Lock bitmap and return bitmap data
	            mBitmapData = mSource.LockBits(rect, ImageLockMode.ReadWrite, 
	                                         mSource.PixelFormat);
	 
	            // create byte array to copy pixel values
	            int step = ColorDepth / 8;
	            Pixels = new byte[PixelCount * step];
	            mPtr = mBitmapData.Scan0;
	 
	            // NOTE: in http://csharpexamples.com/fast-image-processing-c/
	            // using unsafe{} can avoid this .Copy() and allow direct editing of data
	           	// but unsafe{} requires certain access priveledges that I should research since i run
	           	// as guest account but all seems fine.
	            // Copy data from pointer to array
	            Marshal.Copy(mPtr, Pixels, 0, Pixels.Length);
	        }
	        catch (Exception ex)
	        {
	            throw ex;
	        }
	    }
	 
	    /// <summary>
	    /// Unlock bitmap data
	    /// </summary>
	    private void UnlockBits()
	    {
	        try
	        {
	        	System.Diagnostics.Debug.Assert (mPtr == mBitmapData.Scan0);
	            // Copy data from byte array to pointer
	            Marshal.Copy(Pixels, 0, mPtr, Pixels.Length);
	 
	            // Unlock bitmap data
	            mSource.UnlockBits(mBitmapData);
	           
	            // TODO: can i create a version of this class for 32bit bitmaps ONLY so i can always
	            //       store full color value in an int[] instead of byte arrays?
	            // mBitmapData.Scan0 
	        }
	        catch (Exception ex)
	        {
	            throw ex;
	        }
	    }
	    
	    public void Save()
	    {
	    	try
	    	{
	    		System.Diagnostics.Debug.Assert (System.IO.File.Exists (mSourcePath));
	    		UnlockBits();
	    		// we're not allowed to save a bitmap from the stream or file from which it was loaded.
	    		// TODO: we need to verify the contents of this data because for some reason it's just not getting saved
	    		
	    		mSource.Save (mSourcePath);
	    		LockBits();
	    	}
	    	catch (Exception ex)
	    	{
	    		System.Diagnostics.Debug.WriteLine (ex.Message);
	    	}
	    }
	 
	    
	  
	    /// <summary>
	    /// Get the color of the specified pixel
	    /// </summary>
	    /// <param name="x"></param>
	    /// <param name="y"></param>
	    /// <returns></returns>
	    public Pixel GetPixel(int x, int y)
	    {

	    	// TODO: if i know the depth of the bitmap
	    	//       i could avoid the slower code here
	    	//       with just a general GetColor() that returned
	    	//       the specified data at (x,y) directly from lookup
	    	
	    	Pixel pixel = Pixel.GetPixel();
	        
	 
	        // Get color components count
	        int cCount = ColorDepth / 8;
	 
	        // Get start index of the specified pixel
	        int i = ((y * Width) + x) * cCount;
	 
	        if (i > Pixels.Length - cCount)
	            throw new IndexOutOfRangeException();
	 
	        if (ColorDepth == 32) // For 32 bpp get Red, Green, Blue and Alpha
	        {
	            byte b = Pixels[i];
	            byte g = Pixels[i + 1];
	            byte r = Pixels[i + 2];
	            byte a = Pixels[i + 3]; // a
	            pixel.LoadArgb(a, r, g, b);
	        }
	        if (ColorDepth == 24) // For 24 bpp get Red, Green and Blue
	        {
	            byte b = Pixels[i];
	            byte g = Pixels[i + 1];
	            byte r = Pixels[i + 2];
	            pixel.LoadArgb(r, g, b);
	        }
	        if (ColorDepth == 8)
	        // For 8 bpp get color value (Red, Green and Blue values are the same)
	        {
	            byte c = Pixels[i];
	            pixel.LoadArgb(c, c, c);
	        }
	        return pixel;
	    }
	 
	    /// <summary>
	    /// Set the color of the specified pixel
	    /// </summary>
	    /// <param name="x"></param>
	    /// <param name="y"></param>
	    /// <param name="color"></param>
	    public void SetPixel(int x, int y, Pixel pixel)
	    {
	        // Get color components count
	        int cCount = ColorDepth / 8;
	 
	        // Get start index of the specified pixel
	        int i = ((y * Width) + x) * cCount;
	 

	        if (ColorDepth == 32) // For 32 bpp set Red, Green, Blue and Alpha
	        {
	            Pixels[i] = pixel.B;
	            Pixels[i + 1] = pixel.G;
	            Pixels[i + 2] = pixel.R;
	            Pixels[i + 3] = pixel.A;
	        }
	        if (ColorDepth == 24) // For 24 bpp set Red, Green and Blue
	        {
	            Pixels[i] = pixel.B;
	            Pixels[i + 1] = pixel.G;
	            Pixels[i + 2] = pixel.R;
	        }
	        if (ColorDepth == 8)
	        // For 8 bpp set color value (Red, Green and Blue values are the same)
	        {
	            Pixels[i] = pixel.B;
	        }
	    }
	    
	    #region IDisposable
	    public void Dispose()
	    {
	    	if (mSource == null) return;
	    	
	    	mSource.Dispose();
	    	mSource = null;
	    		
	    }
	    #endregion
	}
	
	
	// http://ilab.ahemm.org/tutBitmap.html
	
	
	
	// by snarfblam - 09-17-2006, 01:15 PM 
	// http://www.xtremedotnettalk.com/showthread.php?t=97390
	// Represents a 32-bit ARGB pixel
	// Allows pixel data to be accessed much faster than via
	// the Color struct or the BitConverter class.
	[StructLayout(LayoutKind.Explicit, Size = 4)]
	public struct Pixel
	{
	    // Composite ARGB value
	    [FieldOffset(0)] public int ARGB;
	    // Color components
	    [FieldOffset(3)] public byte A;
	    [FieldOffset(2)] public byte R;
	    [FieldOffset(2)] public ushort High;
	    [FieldOffset(1)] public byte G;
	    [FieldOffset(0)] public byte B;
		[FieldOffset(0)] public ushort Low;
	    // Method to get an instance of this union
	    // NOTE: all fields must still be initialized seperately
	    public static Pixel GetPixel() 
	    {
	        Pixel result;
	
	        result.A = 0;
	        result.R = 0;
	        result.G = 0;
	        result.B = 0;
	        result.High = 0;
	        result.Low = 0;
	        result.ARGB = 0;
	
	        return result;
	    }
	
	    public static Pixel GetPixel (byte a, byte r, byte g, byte b)   
	    {
	    	Pixel result = GetPixel();
	    	result.LoadArgb (a, r, g, b);
	        return result;
	    }
	    
	    public static Pixel GetPixel (int color)   
	    {
	    	Pixel result = GetPixel();
	        result.LoadColor (color);
	        return result;
	    }
	    	    
	    // Set this union to represent the specified color
	    internal void LoadArgb (byte a, byte r, byte g, byte b)
	    {
	    	A = a;
	    	R = r;
	    	G = g;
	    	B = b;
	    }
	    
	    // Set this union to represent the specified color
	    internal void LoadArgb (byte r, byte g, byte b)
	    {
	    	A = 0; // we do not default this to 1 as we would a Color.  This is pixel data.
	    	R = r;
	    	G = g;
	    	B = b;
	    }
	    
	    // Set this union to represent the specified color
	    private void LoadColor (int color)
	    {
	    	ARGB = color;
	    }
	    
	    // Set this union to represent the specified color
	    private void LoadColor(System.Drawing.Color c) 
	    {
	        ARGB = c.ToArgb();
	    }
	
	    // Create a Color struct that represents this union
	    public Color ToColor() 
	    {
	        return Color.FromArgb(ARGB);
	    }
	}
}

