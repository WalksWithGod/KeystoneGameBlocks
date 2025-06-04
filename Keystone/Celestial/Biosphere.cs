using System.Xml;
using Keystone.IO;

namespace Keystone.Celestial
{
    // world data related to atmosphere, oceans, and fauna and flora
    public class Biosphere
    {
        private Atmosphere _atmosphere;
        private BiosphereType _biosphereType;
        private float _albedo;
        private int _oceanCoverage;
        private Life _life;
        private float _surfaceTemperature;

        public Biosphere()
        {
            _atmosphere = new Atmosphere();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public virtual Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] properties = new Settings.PropertySpec[5 ];
            
            properties[0] = new Settings.PropertySpec("biosphere", typeof(int).Name);
            properties[1] = new Settings.PropertySpec("albedo", _albedo.GetType());
            properties[2] = new Settings.PropertySpec("oceancoverage", _oceanCoverage.GetType());
            properties[3] = new Settings.PropertySpec("life", typeof(int).Name);
            properties[4] = new Settings.PropertySpec("surfacetemperature", _surfaceTemperature.GetType());
            
            if (!specOnly)
            {
                properties[0].DefaultValue = (int)_biosphereType;
                properties[1].DefaultValue = _albedo;
                properties[2].DefaultValue = _oceanCoverage;
                properties[3].DefaultValue = (int)_life;
                properties[4].DefaultValue = _surfaceTemperature;
            }

            return properties;
        }

        public virtual void SetProperties(Settings.PropertySpec[] properties)
        {
            if (properties == null) return;
           
            for (int i = 0; i < properties.Length; i++)
            {
                // use of a switch allows us to pass in all or a few of the propspecs depending
                // on whether we're loading from xml or changing a single property via server directive
                switch (properties[i].Name)
                {
                    case "biosphere":
                        _biosphereType = (BiosphereType)(int)properties[i].DefaultValue;
                        break;
                    case "albedo":
                        _albedo = (float)properties[i].DefaultValue;
                        break;
                    case "oceancoverage":
                        _oceanCoverage = (int)properties[i].DefaultValue;
                        break;
                    case "life":
                        _life =(Life)(int)properties[i].DefaultValue;
                        break;
                    case "surfacetemperature":
                        _surfaceTemperature = (float)properties[i].DefaultValue;
                        break;
                }
            }
        }

        public BiosphereType BiosphereType
        {
            get { return _biosphereType; }
            set { _biosphereType = value; }
        }

        public Atmosphere Atmosphere
        {
            get { return _atmosphere; }
            set { _atmosphere = value; }
        }

        public float SurfaceTemperature
        {
            get { return _surfaceTemperature; }
            set { _surfaceTemperature = value; }
        }

        public float Albedo
        {
            get { return _albedo; }
            set { _albedo = value; }
        }

        public int OceanCoverage
        {
            get { return _oceanCoverage; }
            set { _oceanCoverage = value; }
        }

        public Life Life
        {
            get { return _life; }
            set { _life = value; }
        }
    }
}