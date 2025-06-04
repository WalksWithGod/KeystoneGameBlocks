using System;
using Keystone.Elements;
using Keystone.Extensions;

namespace Keystone.TileMap
{
    /// <summary>
    /// Description of StructureLevel.
    /// </summary>
    internal class StructureLevel
    {

        public int FloorLevel;   // FloorLevel is not the same as the array index this StructureLevel is placed.
                                 // This is because the array order can change as new StructureLevels are added or deleted
                                 // from the Structure and by tracking the FloorIndex seperately, a floor designed to be at
                                 // "ground level" will always be at ground level no matter how many new floors we insert
                                 // above or below it or how many we delete.

        public MapLayer[] mMapLayers;
        // TODO: mStructureMinimeshes need to be per level because we need to be able to enable/disable by floor
        //       and because it fascilitates picking a minimesh structural element by floor
        public MinimeshGeometry[] mStructureMinimeshes;

        public StructureLevel(int floorLevel)
        {
            FloorLevel = floorLevel;
        }

        public void AddMapLayer(MapLayer layer)
        {
            mMapLayers = mMapLayers.ArrayAppend(layer);
        }


        public void CreateMinimeshRepresentations(ModelSelector segmentModelSelector, uint initialMinimeshCapacity, double floorHeight)
        {
            MinimeshGeometry[] minimeshes = new MinimeshGeometry[segmentModelSelector.Children.Length];

            // recall any segment can have multiple visual styles or auto-tile versions we must loop through
            for (uint i = 0; i < segmentModelSelector.Children.Length; i++)
            {
                Model model = segmentModelSelector.Select(i);

                double height = this.FloorLevel * floorHeight;

                minimeshes[i] = CreateMinimeshRepresentations(initialMinimeshCapacity, model, (float)height);
            }
        }

        /// <summary>
        /// Replaces a Model's Mesh3d Geometry child with a MinimeshGeometry child instead.
        /// </summary>
        /// <param name="maxMinimeshCount"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private MinimeshGeometry CreateMinimeshRepresentations(uint initialMinimeshCapacity, Model model, float height)
        {
            //CoreClient._CoreClient.Settings.settingRead ("sectionname", "keyname");            		
            // -> Not all models will cast shadow, but we may forget to set .CastShadow=true on ones that do. System.Diagnostics.Debug.Assert (model.CastShadow == true);
            //System.Diagnostics.Debug.Assert (model.ReceiveShadow == true);

            // grab the assigned geometry for this sub-model and create a MinimeshGeometry using same Resource
            Geometry geometry = model.Geometry;

            if (geometry == null) // first model may be a "empty" null model
            {
                // there's no existing geometry node and we dont need to create an "empty' mmGeometry either
                return null;
            }

            MinimeshGeometry mmGeometry = null;

            // Re-use minimesh if possible.  
            // NOTE: every StructureLevel has it's own mStructureMinimeshes array.
            // That is to say, MinimeshGeometry is not shared even between different levels in the same Structure.cs let alone
            // between different Structures. This gives us much better control on height based rendering order (higher levels render
            // before lower levels to take advantage of early Z test) and entire levels can be disabled/enabled easily depending on
            // our view settings. (eg. hiding / showing various dungeon levels)
            if (mStructureMinimeshes != null)
            {
                for (int i = 0; i < mStructureMinimeshes.Length; i++)
                    if (mStructureMinimeshes[i].Mesh3d.ID == geometry.ID)
                    {
                        // NOTE: We should always use same geometry even for multiple rotations within a given StructureLevel.

                        // NOTE: but we should NOT allow this MinimeshGeometry to be shared by multiple Models since it will result in the minimeshGeometry
                        //       being rendered twice.  Instead we should create a new minimesh in the cases where a user does
                        //       specify the exact same geometry file twice and this way elements are added to one or the other
                        //       and are each only rendered once.
                        System.Diagnostics.Debug.Assert(true, "TileMap.StructureLevel.CreateMinimeshRepresentation() - Sharing between multiple Models not allowed.  Use a single Model instead.");
                        // No Sharing! -> mmGeometry = mStructureMinimeshes[i];
                        break;
                    }
            }


            if (mmGeometry == null)
            {
                // NOTE: the minimeshGeometry is unique for each Level and each Model within a Segment
                mmGeometry = (MinimeshGeometry)Resource.Repository.Create("MinimeshGeometry");
                mmGeometry.SetProperty("meshortexresource", typeof(string), geometry.ID);
                mmGeometry.SetProperty("isbillboard", typeof(bool), false);
                mStructureMinimeshes = mStructureMinimeshes.ArrayAppend(mmGeometry);
            }

            // set Y value of minimeshGeometry's Model to height appropriate for this Level.  X,Z are always at 0,0 however for all levels of all zone structures.
            model.Translation = new Keystone.Types.Vector3d(0, height, 0);

            // the MaxInstancesCount starts small, but can be increased withing TVMinimesh without losing existing element data
            if (mmGeometry.MaxInstancesCount < initialMinimeshCapacity)
                mmGeometry.MaxInstancesCount = initialMinimeshCapacity;


            // we don't want the Mesh3d falling out of scope before we 
            // .PagerBase.LoadTVResource() on the minimesh which needs to gain a reference to the Mesh3d
            // TODO: the minimeshGeometry should probably also IncrementRef and DecrementRef that mesh3d 
            //       on it's own when it AddParent() it's model
            Resource.Repository.IncrementRef(geometry);
            model.RemoveChild(geometry);
            model.AddChild(mmGeometry);

            // in order to deserialize the tile layer data and restore the terrain minimesh elements, the minimesh geometry resource must be loaded
            Keystone.IO.PagerBase.LoadTVResource(mmGeometry, false);
            Resource.Repository.DecrementRef(geometry);
            return mmGeometry;
        }

        // TODO: i added incrementRef above and now i think i also need to decrementref and then to ensure
        // that we do in fact cause the minimeshes disposedmanagedresources to fire
        internal void Dispose()
        {

            // TODO: Doesn't MapGrid.Destroy() screw up ClientPager.LoadZoneMapLayers() where we want to load mapdata
            // that is +1 further out from our page_in value so that auto-tiling works at boundaries?
            // In other  words, i dont think our page-out distance can be less than or equal to layer-page-in distance
            //Core._Core.MapGrid.Destroy(LocationX, LocationY, LocationZ);
        }
    }
}
