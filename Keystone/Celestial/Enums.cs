namespace Keystone.Celestial
{
    public enum LUMINOSITY : int
    {
        SUPERGIANT_I = 0, // extremely rare and should be placed manually rather than algorithmically or else use extremely low chance value
        GIANT_II,
        GIANT_III,
        SUBGIANT_IV,
        DWARF_V, // main sequence
        SUBDWARF_VI,
        WHITEDWARF_D,
        PULSAR_P,
        NEUTRON_N,
        BLACKHOLE_B
    }

    public enum SPECTRAL_TYPE : int
    {
        O = 0, //     <-- hottest
        B,
        A, // white
        F, // yellow - white
        G, // yellow
        K, // orange
        M, // red  <-- coolest
        SPECIAL
    }

    public enum SPECTRAL_SUB_TYPE : int
    {
        SubType_0 = 0, // hottest
        SubType_1,
        SubType_2,
        SubType_3,
        SubType_4,
        SubType_5,
        SubType_6,
        SubType_7,
        SubType_8,
        SubType_9 // coolest
    }

    public enum WorldSize
    {
        Tiny,
        VerySmall,
        Small,
        Standard,
        Large
    }

    public enum WorldType
    {
        Terrestial,
        GasGiant,
        PlanetoidBelt
    }

    public enum BiosphereType
    {
        Hostile_SG,
        Hostile_N,
        Hostile_A,
        Desert,
        Rockball,
        IcyRockball,
        Ocean,
        Greenhouse,
        Earthlike
    }

    public enum HabitalZones
    {
        Inner,
        Life,
        Middle,
        Outer,
        Forbidden
    }

    public enum Life
    {
        None,
        Protozoa,
        Metazoa,
        SimpleAnimals,
        ComplexAnimals
    }
}