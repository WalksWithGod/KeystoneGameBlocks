using System;


namespace LibNoise
{
    public interface INoiseBasis : IModule 
    {
        int Seed { get; set; }
        double Frequency { get; set; }
        NoiseQuality NoiseQuality { get; set; }
        int OctaveCount { get; set; }
        double Lacunarity { get; set; }

    }

    public interface IPersistenceNoiseBasis : INoiseBasis
    {
        double Persistence { get; set; }
    }

    public interface ITurbulence : IModule
    {
        IModule SourceModule { get; set; }
        int Seed { get; set; }
        double Power { get; set; }
        double Frequency { get; set; }
        int Roughness { get; set; }
    }
}
