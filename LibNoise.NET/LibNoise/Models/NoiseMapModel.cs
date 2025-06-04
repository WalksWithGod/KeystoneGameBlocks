using System;

namespace LibNoise.Models
{
    public enum NoiseModelType
    {
        Line,
        Plane,
        Sphere,
        Cylinder
    }

    public abstract class NoiseMapModel : Math
    {
        /// <summary>
        /// The module from which to retrieve noise.
        /// </summary>
        public IModule SourceModule { get; set; }
        public Palette Palette { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        protected int _minX, _minY, _maxX, _maxY;

        public void SetSize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public void SetCoordinateSystemExtents (int minX, int maxX, int minY, int maxY)
        {
            _minX = minX;
            _minY = minY;
            _maxX = maxX;
            _maxY = maxY;
        }

                /// <summary>
        /// Initialises a new instance of the Sphere class.
        /// </summary>
        /// <param name="sourceModule">The module from which to retrieve noise.</param>
        public NoiseMapModel(IModule sourceModule)
        {
            if (sourceModule == null)
                throw new ArgumentNullException("A source module must be provided.");

            SourceModule = sourceModule;
        }


        public virtual System.Drawing.Bitmap Generate()
        {

            return null;
        }
        
    }
}
