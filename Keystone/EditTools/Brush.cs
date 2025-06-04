using System;

namespace Keystone.EditTools
{
	/// <summary>
	/// Description of Brush.
	/// </summary>
	public class Brush
	{
		byte[] mMask;
		uint mWidth;
		uint mHeight;
		
		public Brush()
		{
		}
		
		public uint Width {get {return mWidth;}}
		
		public uint Height {get {return mHeight;}}
		
		public byte[] Mask {get{return mMask;}}
	}
}
