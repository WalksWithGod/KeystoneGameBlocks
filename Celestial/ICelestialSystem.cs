
namespace Celestial
{

    
    // todo: shoudlnt this inherit IEntity?  I think it should.  
    interface ICelestialSystem
    {
        // todo: should use Core Vector3D class because a) we want double precision and b) we want these available on the server
        //       besides, normally the tv3d version is only access when we need to "set" a value
        //       so maybe some conversions during Updates()
        // note: one bummer is that things like boundingbox min,max and stuff will need to be switched to use doubles too :/
        // decimal PositionWorl
        // double PositionLocal  
        // single Rotation (for asteroids this is normal, but for spheres like planets, its for Axial tilt)
        // single Scale
        
        
        string Name{get;set;}
        int ID { get;set;}
        
        float Mass { get;}
        float Radius { get;}
        float Gravity { get;}
        float SurfaceTemperature { get;}  // average
        float Density { get;}
        
        Orbit Orbit{get;}
        ICelestialSystem[] Children { get;}

        void ICelestialSystem(ICelestialSystem parentSystem);

        float DistanceFromParent();
       
        void Add(ICelestialSystem child);
        void Remove(ICelestialSystem child);
        void Remove(int childID);
        void Remove(string childName);
        void Update();
        
    }
}
