
namespace Celestial
{
    public class Atmosphere
    {
        Pressure _pressure ;
        GAS_COMPOSITION[] _composition;
        byte _primaryContaminant ;
        float _greenHouseFactor;

        
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
        }
        
        public Atmosphere() {}
        
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

        public Pressure Pressure
        {
            get { return _pressure; }
            set { _pressure = value; }
        }
    }
}
