/*
 * Created by SharpDevelop.
 * User: Hypnotron
 * Date: 1/22/2014
 * Time: 1:35 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace keymath.IO
{
	/// <summary>
	/// Description of ICsvRecordHandler.
	/// </summary>
	/// <remarks>
	/// http://www.codeproject.com/Messages/4302501/Re-Missing-new-line-handling.aspx
	/// </remarks>
	public interface ICsvRecordHandler
	{
		void BeginCsv();
        void BeginRecord(int recordNr);
        void AddField(int fieldNr, string field);
        void EndRecord(int recordNr);
        void EndCsv(int recordsTotal);
    }
	
    public interface ICsvParser
    {
        void ParseRecords();
    }
    
    public interface ICsvScanner
    {
        string Curr { get; }
        void Next();
        bool HasData { get; }
        bool IsSep { get; }
        bool IsEol { get; }
	}
	
}
