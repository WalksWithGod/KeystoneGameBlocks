
namespace Celestial
{
    // world data related to atmosphere, oceans, and fauna and flora
    public class Biosphere
    {
        Atmosphere _atmosphere;
        BiosphereType _biosphereType;
        float _albedo;
        int _oceanCoverage;
        Life _life;
        float _surfaceTemperature;
        
        public Biosphere()
        {
            _atmosphere = new Atmosphere();
        }
        
        public BiosphereType BiosphereType
        {
            get { return _biosphereType; } set { _biosphereType = value; }
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
