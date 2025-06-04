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
using System.Text.RegularExpressions ; // req for Match
using System.Linq ;
namespace keymath.IO
{
	/// <summary>
	/// Description of RegexCsvScanner.
	/// </summary>
	/// <remarks>
	/// http://www.codeproject.com/Messages/4302501/Re-Missing-new-line-handling.aspx
	/// </remarks>
	public class RegexCsvScanner : ICsvScanner
	{
		private IEnumerator<Match> Tokenizer { get; set; }

        public string Curr 
        { 
        	get  	
        	{ 
        		return HasData
	            ? Tokenizer.Current.Groups.Cast<Group>().Reverse().First(g => g.Success)
	              .Value.Replace(@"""""", @"""")
	            : string.Empty; 
        	} 
        }
        
        public void Next() 
        { 
        	HasData = HasData && Tokenizer.MoveNext();
        }
        
        public bool HasData { get; private set; }
        
        public bool IsSep { get { return HasData && Tokenizer.Current.Groups[2].Success; } }
        
        public bool IsEol { get { return HasData && Tokenizer.Current.Groups[3].Success; } }
 
        public RegexCsvScanner(string sep, string data)
        {
            string tokens = string.Join("|", new [] {
                    @"""((?:""""|[^""])*)""",   // group 1: content of qstring
                                                //          (not unescaped)
                    @"(" + sep + @")",          // group 2: SEP
                    @"(\n\r?|\r\n?)",           // group 3: EOL = any variant of \n and \r
                    @"([^""\n\r" + sep + @"]+)",// group 4: word of at least one
                                                //          character not one of
                                                //          quote, eol, sep
                });
            Tokenizer = Regex.Matches(data, tokens, RegexOptions.Compiled|RegexOptions.Singleline)
                .Cast<Match>().GetEnumerator();
            HasData = true;
        }
		
	}
}
