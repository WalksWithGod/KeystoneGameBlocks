/*
 * Created by SharpDevelop.
 * User: Hypnotron
 * Date: 1/22/2014
 * Time: 1:36 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic ;

namespace keymath.IO
{
	/// <summary>
	/// Description of ConsoleCsvRecordHandler.
	/// </summary>
	/// <remarks>
	/// http://www.codeproject.com/Messages/4302501/Re-Missing-new-line-handling.aspx
	/// </remarks>
	public class ConsoleCsvRecordHandler : ICsvRecordHandler
	{
		private List<string> Fields { get; set; }
        
		public ConsoleCsvRecordHandler()
        { 
        	Fields = new List<string>(); 
        }
 
        public void BeginCsv()
        { 
        	Console.WriteLine("------ CSV Parsing ---------"); 
        }
        
        public void BeginRecord(int recordNr)
        { 
        	Fields.Clear(); 
        }
        
        public void AddField(int fieldNr, string field)
        { 
        	System.Diagnostics.Debug.Assert(fieldNr == Fields.Count); 
        	Fields.Add(field);
        }
        
        public void EndRecord(int recordNr)
        { 
        	Console.WriteLine("Record {0}: '{1}'", recordNr, string.Join("', '", Fields)); 
        }
        
        public void EndCsv(int recordsTotal)
        { 
        	Console.WriteLine("------ Records: {0} ---------", recordsTotal);
        }
	}
	
//	public class DigestCsvRecordHandler : ICsvRecordHandler 
//	{
//		// 
//	}
}
