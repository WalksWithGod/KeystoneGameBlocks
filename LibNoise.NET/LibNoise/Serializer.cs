using System;
using System.Collections.Generic;
using System.Xml;

namespace LibNoise
{
    public class Serializer
    {
        /// <summary>
        /// Takes the path to an xml file 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Models.NoiseMapModel FromXMLFile(string path)
        {
            try
            {
                string text = System.IO.File.ReadAllText(path);
                return FromXMLText(text);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null; 
            } 
        }

        public static Models.NoiseMapModel FromXMLText(string xmltext)
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            } 
        }


        private void Process()
        {
 
        }

    }
}
