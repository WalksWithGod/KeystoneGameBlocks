/*
 * Created by SharpDevelop.
 * User: Hypnotron
 * Date: 1/22/2014
 * Time: 2:00 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace keymath.IO
{
	/// <summary>
	/// Description of CsvWriter.
	/// </summary>
	public class CsvWriter
	{
		private string mFilePath;
		
		public CsvWriter(string filepath)
		{
			mFilePath = filepath;
		}
			
		
		/// <summary>
		/// Appends records.
		/// </summary>
		public void WriteRecord ()
		{
			
		}
		
		
		public void WriteRecords (string[] records)
		{
			// records is expected to be an array of cvs lines
			
			var streamWriter = new System.IO.StreamWriter (mFilePath);
			
							
			
		}
		
	}
}
