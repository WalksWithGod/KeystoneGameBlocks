/*
 * Created by SharpDevelop.
 * User: Hypnotron
 * Date: 1/22/2014
 * Time: 1:36 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace keymath.IO
{
	/// <summary>
	/// Description of CsvParser.
	/// </summary>
	/// <remarks>
	/// http://www.codeproject.com/Messages/4302501/Re-Missing-new-line-handling.aspx
	/// </remarks>
	public class CsvParser
	{

		private ICsvScanner Scanner { get; set; }
        private ICsvRecordHandler RecordHandler { get; set; }
 
        public CsvParser(ICsvScanner scanner, ICsvRecordHandler recordHandler)
        {
            Scanner = scanner;
            RecordHandler = recordHandler;
        }
 
        public void ParseRecords()
        {
            RecordHandler.BeginCsv();
            Scanner.Next();
            int recordNr = 0;
            while (Scanner.HasData) 
            	ParseRecord(recordNr++);
            
            RecordHandler.EndCsv(recordNr);
        }
        
        private void ParseRecord(int recordNr)
        {
            RecordHandler.BeginRecord(recordNr);
            int fieldNr = 0;
            ParseField(fieldNr++);
            while (Scanner.IsSep)
            {
                Scanner.Next();
                ParseField(fieldNr++);
            }
            if (Scanner.IsEol) 
            	Scanner.Next();
            
            RecordHandler.EndRecord(recordNr);
        }
        
        private void ParseField(int fieldNr)
        {
            string field = Scanner.Curr;
            if (Scanner.IsSep || Scanner.IsEol) 
            	field = string.Empty;
            else 
            	Scanner.Next();
            
            RecordHandler.AddField(fieldNr, field);
        }
		
	}
}
