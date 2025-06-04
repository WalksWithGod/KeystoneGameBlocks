using System.Xml;
using Keystone.IO;
using Keystone.Elements;

namespace Keystone.Celestial
{
    public class Atmosphere //: Node 
    {
        private ATMOSPHERIC_PRESSURE _pressure;
        private GAS_COMPOSITION[] _composition;
        private byte _primaryContaminant;
        private float _greenHouseFactor;

        public enum GAS_TYPE
        {
            Oxygen,
            CarbonDioxide,
            CarbonMonoxide,
            Nitrogen,
            Hydrogen,
            Helium,
            Methane,
            H20 // water vapor
        }

        public struct GAS_COMPOSITION
        {
            public GAS_TYPE Type;
            public float Percentage;

            public void ReadXml(XmlNode xmlnode)
            {
                Type =
                    (GAS_TYPE)
                    Settings.EnumHelper.EnumeratedMemberValue(typeof (GAS_TYPE),
                                                              XmlHelper.GetAttributeValue(xmlnode, "gastype"));
                Percentage = float.Parse(XmlHelper.GetAttributeValue(xmlnode, "percentage"));
            }

            public void WriteXml(XmlNode xmlnode)
            {
                XmlHelper.CreateAttribute(xmlnode, "gastype", Type.ToString());
                XmlHelper.CreateAttribute(xmlnode, "percentage", Percentage.ToString());
            }
        }

        public enum ATMOSPHERIC_PRESSURE
        {
            None,
            Trace,
            VeryThin,
            Thin,
            Standard,
            Dense,
            VeryDense,
            SuperDense
        }

        public Atmosphere()
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public virtual Settings.PropertySpec[] GetProperties(bool specOnly)
        {
           
            int count;
            if (_composition == null || _composition.Length < 1)
                count = 0;
            else
                count = _composition.Length;

            Settings.PropertySpec[] properties = new Settings.PropertySpec[4 + count ];

            properties[0] = new Settings.PropertySpec("pressure", _pressure.GetType());
            properties[1] = new Settings.PropertySpec("contaminant", _primaryContaminant.GetType());
            properties[2] = new Settings.PropertySpec("greenhousefactor", _greenHouseFactor.GetType());
            properties[3] = new Settings.PropertySpec("compositioncount", count.GetType());
            // note: this is not an issue if the compositions are treated as pure child nodes.
            for (int i = 0; i < count; i++)
            {
                //_composition[i].WriteXml(xmlnode);  uhhh? what to do here?
            }

            if (!specOnly)
            {
                properties[0].DefaultValue = _pressure;
                properties[1].DefaultValue = _primaryContaminant;
                properties[2].DefaultValue = _greenHouseFactor;
                properties[3].DefaultValue = count;
                for (int i = 0; i < count; i++)
                {
                    //_composition[i].WriteXml(xmlnode); // TODO: what about this?
                }
            }

            return properties;
        }

        public virtual void SetProperties(Settings.PropertySpec[] properties)
        {
            if (properties == null) return;

            int count = 0;
            for (int i = 0; i < properties.Length; i++)
            {
                // use of a switch allows us to pass in all or a few of the propspecs depending
                // on whether we're loading from xml or changing a single property via server directive
                switch (properties[i].Name)
                {
                    case "pressure":
                        _pressure = (ATMOSPHERIC_PRESSURE )properties[i].DefaultValue;
                        break;
                    case "contaminant":
                        _primaryContaminant = (byte)properties[i].DefaultValue;
                        break;
                    case "greenhousefactor":
                        _greenHouseFactor = (float)properties[i].DefaultValue;
                        break;
                    case "compositioncount":
                        count =(int)properties[i].DefaultValue;
                        break;

                }
            }
        }

        public GAS_COMPOSITION[] Composition
        {
            get { return _composition; }
            set { _composition = value; }
        }

        public byte Contaminant
        {
            get { return _primaryContaminant; }
            set { _primaryContaminant = value; }
        }

        public float GreenHouseFactor
        {
            get { return _greenHouseFactor; }
            set { _greenHouseFactor = value; }
        }

        public ATMOSPHERIC_PRESSURE Pressure
        {
            get { return _pressure; }
            set { _pressure = value; }
        }
    }
}