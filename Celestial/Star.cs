
using Celestial;

namespace Celestial
{
    public class Star : Body  
    {
        
        LUMINOSITY    _luminosityClass ;
        SPECTRAL_TYPE _spectralType ;
        SPECTRAL_SUB_TYPE _spectralSubType;
        
        int _temperature ;
        float _luminosity;
        float _age ;
        float _innerLimitDistance ;
        float _outerLimitDistance;
        float _lifeZoneInnerEdge ;
        float _lifeZoneOuterEdge ;
        float _snowLine;
               
        public Star (string name) : base(name)
        {
            
        }

        public LUMINOSITY LuminosityClass { get { return _luminosityClass; } set { _luminosityClass = value; } }
        public SPECTRAL_TYPE SpectralType { get { return _spectralType; } set { _spectralType = value; } }
        public SPECTRAL_SUB_TYPE SpectralSubType { get { return _spectralSubType; } set { _spectralSubType = value; } }

        public int Temperature { get { return _temperature; } set { _temperature = value; } }
        public float Luminosity { get { return _luminosity; } set { _luminosity = value; } }
        public float Age { get { return _age; } set { _age = value; } }
        
        // todo: these may not be needed necessarily.  Usually are just for generation.
        public float InnerLimitDistance { get { return _innerLimitDistance; } set { _innerLimitDistance = value; } }
        public float OuterLimitDistance { get { return _outerLimitDistance; } set { _outerLimitDistance = value; } }
        public float LifeZoneInnerEdge { get { return _lifeZoneInnerEdge; } set { _lifeZoneInnerEdge = value; } }
        public float LifeZoneOuterEdge { get { return _lifeZoneOuterEdge; } set { _lifeZoneOuterEdge = value; } }
        public float SnowLine { get { return _snowLine; } set { _snowLine = value; } }


        public override void Traverse(Core.Traversers.ITraverser target)
        {
            throw new global::System.Exception("The method or operation is not implemented.");
        }
    }
}
