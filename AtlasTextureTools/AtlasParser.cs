using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;

namespace AtlasTextureTools
{
    public struct AtlasRecord
    {
        public string FileName;   // the filename of the sub-texture (mostly used for displaying in image browser)
        public string AtlasFileName;
        public int Index;
        public float WidthOffset;  // ideally when loading the texture, we can  
        public float HeightOffset; // define the atlas items
        public float Width;
        public float Height;
        public float DepthOffset;

        public Rectangle GetRectangle (Image atlas)
        {

            int left, top, width, height;
            width = (int)(atlas.Width * Width);
            height = (int)(atlas.Height * Height);

            left = (int)(atlas.Width * WidthOffset);
            top = (int)(atlas.Height * HeightOffset);

            Rectangle result = new Rectangle (left, top, width, height);
            return result;
            
        }
    }

    public class AtlasParser
    {

        public static AtlasRecord[] Parse(string filename)
        {
            if (string.IsNullOrEmpty(filename)) throw new ArgumentNullException();

            List<AtlasRecord> records = new List<AtlasRecord>();

            using (TextReader rdr = new StreamReader(filename))
            {

                //string[] lines;
                //string text = rdr.ReadToEnd();
                //if (TryParseCSVLine(text, 's', '#', out lines))
                //{
 
                //}

                string line;
                while ((line = rdr.ReadLine()) != null)
                {
                    // use line here
                    if (line.Length > 0)
                    {
                        // find first non white space character
                        int? nonWhiteSpacePosition = line.IndexNotOf("\t\r\n");
                        
                        // begin processing if this line contains information we can use
                        if (nonWhiteSpacePosition != null && line[(int)nonWhiteSpacePosition] != '#')
                        {
                            string[] splitString = line.Split(new char[] {',','\t'}, StringSplitOptions.RemoveEmptyEntries);
                            if (splitString.Length > 8)
                            {
                                // line format:  <original texture filename>/t/t<atlas filename>, <atlas idx>, <atlastype>, <woffset>, <hoffset>, <depth offset>, <width>, <height>
                                //                             0                           1              2            3        4          5          6               7        8

                                AtlasRecord record;
                                record.FileName = splitString[0];
                                record.AtlasFileName = splitString[1];
                                record.Index = int.Parse (splitString[2]);
                                // record.Type = int.Parse(splitString[3]); // atlas type eg. 2d or 3d texture (we only support 2d for now)
                                record.WidthOffset = float.Parse(splitString[4]);
                                record.HeightOffset = float.Parse(splitString[5]);
                                record.DepthOffset = float.Parse(splitString[6]);
                                record.Width = float.Parse(splitString[7]);
                                record.Height = float.Parse(splitString[8]);
                                records.Add(record);
                            }
                        }
                    }
                }
            }


            return records.ToArray();
        }

        // NOTE: Below is a bit too much for what we need for now.  If we really did a lot of
        // CVS manipulation, we'd want a better solution, but for our atlas .tai files, simple split is ok
        // http://code.google.com/p/csharp-csv-reader/
        ///// <summary>
        ///// Read in a line of text, and use the Add() function to add these items to the current CSV structure
        ///// </summary>
        ///// <param name="s"></param>
        //public static bool TryParseCSVLine(string s, char delimiter, char text_qualifier, out string[] array)
        //{
        //    bool success = true;
        //    List<string> list = new List<string>();
        //    StringBuilder work = new StringBuilder();
        //    for (int i = 0; i < s.Length; i++)
        //    {
        //        char c = s[i];

        //        // If we are starting a new field, is this field text qualified?
        //        if ((c == text_qualifier) && (work.Length == 0))
        //        {
        //            int p2;
        //            while (true)
        //            {
        //                p2 = s.IndexOf(text_qualifier, i + 1);

        //                // for some reason, this text qualifier is broken
        //                if (p2 < 0)
        //                {
        //                    work.Append(s.Substring(i + 1));
        //                    i = s.Length;
        //                    success = false;
        //                    break;
        //                }

        //                // Append this qualified string
        //                work.Append(s.Substring(i + 1, p2 - i - 1));
        //                i = p2;

        //                // If this is a double quote, keep going!
        //                if (((p2 + 1) < s.Length) && (s[p2 + 1] == text_qualifier))
        //                {
        //                    work.Append(text_qualifier);
        //                    i++;

        //                    // otherwise, this is a single qualifier, we're done
        //                }
        //                else
        //                {
        //                    break;
        //                }
        //            }

        //            // Does this start a new field?
        //        }
        //        else if (c == delimiter)
        //        {
        //            list.Add(work.ToString());
        //            work.Length = 0;

        //            // Test for special case: when the user has written a casual comma, space, and text qualifier, skip the space
        //            // Checks if the second parameter of the if statement will pass through successfully
        //            // e.g. "bob", "mary", "bill"
        //            if (i + 2 <= s.Length - 1)
        //            {
        //                if (s[i + 1].Equals(' ') && s[i + 2].Equals(text_qualifier))
        //                {
        //                    i++;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            work.Append(c);
        //        }
        //    }
        //    list.Add(work.ToString());

        //    // If we have nothing in the list, and it's possible that this might be a tab delimited list, try that before giving up
        //    if (list.Count == 1 && delimiter != DEFAULT_TAB_DELIMITER)
        //    {
        //        string[] tab_delimited_array = ParseLine(s, DEFAULT_TAB_DELIMITER, DEFAULT_QUALIFIER);
        //        if (tab_delimited_array.Length > list.Count)
        //        {
        //            array = tab_delimited_array;
        //            return success;
        //        }
        //    }

        //    // Return the array we parsed
        //    array = list.ToArray();
        //    return success;
        //}
    }
}
