using Keystone.Types;
namespace Keystone
{
    public interface IBoundVolume
    {
        //IBoundVolume Parent { get;} obsolete.  all nodes inherit Node ultimately which contain Parents
        BoundingBox BoundingBox { get; }
        BoundingSphere BoundingSphere { get; }
        bool DrawBoundingBox { get; set; }

        bool BoundVolumeIsDirty { get; }
        void UpdateBoundVolume();
    }
}