using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Keystone.Converters
{
    public class Tokenizer
    {
        private const char dash = '-';
        private const char slash = '/';
        private const char backslash = '\\';
        private const char apostrophe = '\'';
        private const char quotation = '\"';
        private const char space = ' ';
        private const char tab = '\t';
        private const char LF = '\n';
        private const char CR = '\r';

        //  Actually we want to work on that in another class because some other code depends on the below.
        public static string[] Tokenize(string s)
        {
            StringBuilder sb = new StringBuilder();
            List<string> tokens = new List<string>();
            int i, length;
            bool inQuote = false;
            char currentchar;
            char? quoteChar = null;

            length = s.Length;
            if (length > 0)
            {
                for (i = 0; i < length; i++)
                {
                    currentchar = s.ToCharArray()[i];
                    switch (currentchar)
                    {
                        case quotation:
                        case apostrophe:
                            if (quoteChar == null)
                            {
                                quoteChar = currentchar;
                                inQuote = !inQuote;
                            }
                            else if (quoteChar == currentchar)
                            {
                                inQuote = !inQuote;
                                quoteChar = null;
                                if (sb.Length > 0)
                                {
                                    tokens.Add(sb.ToString());
                                    // sb = New StringBuilder()
                                    sb.Remove(0, sb.Length);
                                }
                            }
                            else
                            {
                                sb.Append(currentchar);
                            }
                            break;
                        case space:
                        case tab:
                        case LF:
                        case CR:
                            //  I added the CR and LF chars :/  these too are delimiters. Ideally wed want to improve
                            //  this function to be able to configure which delimiters we wanted to use
                            //  if this isnt a quoted argument, spaces are delimiters and so we've completed a token
                            if (!inQuote)
                            {
                                if (sb.Length > 0)
                                {
                                    tokens.Add(sb.ToString());
                                    // sb = New StringBuilder()
                                    sb.Remove(0, sb.Length);
                                }
                            }
                            else
                                sb.Append(currentchar);

                            break;
                        default:
                            sb.Append(currentchar);
                            break;
                    }
                }
            }
            if (sb.Length > 0)
                tokens.Add(sb.ToString());

            return tokens.ToArray();
        }

        //  Actually we want to work on that in another class because some other code depends on the below.
        public static List<string> TokenizeWithSymbols(string s, List<int> debugSymbols)
        {
            List<string> tokens = new List<string>();
            int j = -1;
            bool inQuote = false;
            StringBuilder sb = new StringBuilder();
            char currentchar;
            Int32 length;
            char? quoteChar = null;

            if (!(debugSymbols == null))
            {
                debugSymbols.Clear();
                length = s.Length;
                if (length > 0)
                {
                    for (int i = 0; i < length; i++)
                    {
                        currentchar = s.ToCharArray()[i];
                        switch (currentchar)
                            //this case finds the start and end quote characters of a particular argument that uses them 
                            //i.e. "c:\test\run.exe" would be an arg in a command line for instance that is in qutoes
                        {
                            case quotation:
                            case apostrophe:
                                if (quoteChar == null)
                                {
                                    quoteChar = currentchar;
                                    inQuote = !inQuote;
                                }
                                else if ((quoteChar == currentchar))
                                {
                                    inQuote = !inQuote;
                                    quoteChar = null;
                                    if ((sb.Length > 0))
                                    {
                                        tokens.Add(sb.ToString());
                                        debugSymbols.Add(j);
                                        j = -1;
                                        sb.Remove(0, sb.Length);
                                    }
                                }
                                else
                                {
                                    sb.Append(currentchar);
                                    if ((j == -1))
                                        j = (i + 1);
                                }

                                break;

                                //I added the CR and LF chars :/  these too are delimiters. Ideally wed want to improve
                                // this function to be able to configure which delimiters we wanted to use
                                // if this isnt a quoted argument, spaces are delimiters and so we've completed a token
                            case space:
                            case tab:
                            case LF:
                            case CR:
                            case ';':

                                if (!inQuote)
                                {
                                    if ((sb.Length > 0))
                                    {
                                        tokens.Add(sb.ToString());
                                        debugSymbols.Add(j);
                                        j = -1;
                                        sb.Remove(0, sb.Length);
                                    }
                                }
                                else
                                {
                                    sb.Append(currentchar);
                                    if ((j == -1))
                                    {
                                        j = (i + 1);
                                    }
                                }
                                break;

                            default:
                                sb.Append(currentchar);
                                if ((j == -1))
                                {
                                    j = (i + 1);
                                }
                                break;
                        }
                    }
                    if (sb.Length > 0)
                    {
                        tokens.Add(sb.ToString());
                        debugSymbols.Add(length);
                        j = -1;
                    }
                    return tokens;
                }
            }
            return null;
        }

        public static float ParseFloat(int iOccurance, int iMaxOccurance, string s)
        {
            int j = 0, k = 0;

            for (int i = 1; i <= iOccurance; i++)
            {
                k = 1 + j;
                j = s.IndexOf(" ", j + 1, 1); // Instr(j + 1, s, " ");
            }

            if (iOccurance == iMaxOccurance)
                return float.Parse(s.Substring(k, s.Length - k + 1));
                //   (Mid(s, k, s.Length - k + 1));
            else
                float.Parse(s);

            return float.Parse(s.Substring(k, j - k));
        }
    }
}