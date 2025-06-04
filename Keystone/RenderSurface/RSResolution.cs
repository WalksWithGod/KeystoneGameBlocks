namespace Keystone.RenderSurfaces
{
    public enum RSResolution
    {
        R_32x32 = 0, //  first element must equal 0 since in ShadowMap.vb we assume lowest version = 0
        //  do not assign values to the rest.  we assume in ShadowMap.vb that they all increment by 1 from the initial 0
        R_64x64,
        R_128x128,
        R_256x256,
        R_384x384,
        R_512x512,
        R_768x768,
        R_896x896,
        R_1024x1024,
        R_1152x1152,
        R_1280x1280,
        R_1536x1536,
        R_2048x2048
    }
}