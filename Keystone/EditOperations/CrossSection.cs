using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keystone.EditOperations
{
    /// <summary>
    /// Finds a 2-d cross section given a triangulated mesh
    /// </summary>
    public class CrossSection
    {
        // http://stackoverflow.com/questions/5695322/calculate-volume-of-3d-mesh
        // http://stackoverflow.com/questions/2797431/generate-2d-cross-section-polygon-from-3d-mesh

        // this is all very tricky.  

        // 1) Our goal is simply to define the coordinates for our cells at various deck levels
        // 2) i think the exterior cross section method is problematic because even after we compute it
        //    we still will end up wanting to make a projection of that in 2d so that we can
        //    then easily determine if a cell will fit within the solid portion and that way we
        //    easily can flood fill the interior
        //    
        // 3) using the projected 2d area is more forgiving with regards to meshes that aren't closed
        //   *eg. have doors and window openings.
        //
        //
        // I really should ignore this for now and focus on just manually creating a floorplan
        // that is multideck for a small-ish test frigate model.
        // 
    }
}
