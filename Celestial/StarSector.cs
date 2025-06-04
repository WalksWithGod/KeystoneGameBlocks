using System;
using System.Collections.Generic;
using System.Text;
using Core.Types;

namespace Celestial
{
//    Rendering Planets and Stars
//--------------------------
//- thought about for some far away things that are big enough to get rendered but too far to be included in the cull, 
//   is to flag some objects and during the cull pass, cull them with an overloaded IsVisible test that skips the far plane test.  
//   Then when we render, we draw scale them or draw their imposter.
//- the problem is, if they are in the octree, then its possible that the entire octree node they are in gets culled first... 
//  unless they are in the root node :/
//- But the solution there could be not put them in the octree at all.  Put all stars, planets and other extremely large bodies in the root sector's page.  
//  This way, since all our space "sectors" will contain their own octrees, then we can traverse the sectors and cull those prior to culling the octrees. 
// That way all those other octrees will get culled at the root node since they are likely too far away.
//- we can already then experiment with putting several space sectors into our engine and rendering them INCLUDING the Transform node at 
// their roots and see how that goes.
//- and we dont need an octree or quadtree.  We can have 3d sectors that are simply X,Y,Z as id 0 to N in all three dimensions.  
// To find the neigbor of any sector its as simply as incrementing or decrementing in the various directions. 
    // todo: see notes.doc   could just use portals instead of neighbors. Then all our "paging" could be portal based where we determine what to load based
    // on distance through the various connected regions?
    public class StarSector
    {
        public static StarSector[,,] Sectors;
        
        Vector3d _translation;
        int _xID, _yID, _zID;
        
        private StarSector()
        {
            
        }
        
        // static isn't really required, but i'm sticking to the pattern I use for other entities that make use of repository ref counting
        public static StarSector Create(int xID, int yID, int zID, Vector3d translation)
        {
            StarSector sector = new StarSector();
            sector.Translation = translation;
            return sector;
        }
        
        // todo: as far as i can tell, this doesnt check for sectors that are out of bounds
        public StarSector[] Neighbors()
        {
            StarSector[] results = new StarSector[18];

            //6 cardinals
            results[0] = Sectors[_xID + 1, _yID, _zID]; //+x
            results[1] = Sectors[_xID - 1, _yID, _zID]; //-x
            results[2] = Sectors[_xID, _yID + 1, _zID]; //+y
            results[3] = Sectors[_xID, _yID - 1, _zID]; //-y
            results[4] = Sectors[_xID, _yID, _zID + 1]; //+z
            results[5] = Sectors[_xID, _yID, _zID - 1]; //-z

            //12 edges
            results[6] = Sectors[_xID + 1, _yID + 1, _zID];//+x (+y)
            results[7] = Sectors[_xID + 1, _yID - 1, _zID];//+x (-y)
            
            results[8] = Sectors[_xID - 1, _yID + 1, _zID];//-x (+y)
            results[9] = Sectors[_xID - 1, _yID - 1, _zID];//-x (-y)
            
            results[10] = Sectors[_xID, _yID + 1, _zID + 1];//+y (+z)
            results[11] = Sectors[_xID, _yID + 1, _zID - 1];//+y (-z)
            
            results[12] = Sectors[_xID, _yID - 1, _zID + 1];//-y (+z)
            results[13] = Sectors[_xID, _yID - 1, _zID - 1];//-y (-z)
            
            results[14] = Sectors[_xID + 1, _yID, _zID + 1];//+z (+x)
            results[15] = Sectors[_xID - 1, _yID, _zID + 1];//+z (-x)
            
            results[16] = Sectors[_xID + 1, _yID, _zID - 1];//-z (+x)
            results[17] = Sectors[_xID - 1, _yID, _zID - 1];//-z (-x)

            return results;
        }
        internal Vector3d Translation {get { return _translation; } set {_translation = value;}}
        
    }
}
