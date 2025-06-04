using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keystone.Types;
using Keystone.Immediate_2D;

namespace Keystone.Hud
{
    public class AxisIndicator
    {
        public bool Enable;

        // @"E:\dev\c#\KeystoneGameBlocks\Data\pool\meshes\axis.x"
        public static Renderable3DLines[] Update(Vector3d cameraSpacePosition, float columnSpacing, float halfColumns)
        {
            //if (Enable == false) return null;

            return GeneratedLines(cameraSpacePosition, columnSpacing, halfColumns);
        }

        // TODO: i could create this a TVMesh as well...  that would be faster.
        //       Then to position the tvmesh in a fixed screen position, take a 2d spot, unproject it some close distance
        //       and use that as our world position

        // OPTION 0 
        // Use a 3D HUD rendering pass that just looks at HUD items.
        // Because the camera never moves and the world objects never move, there is no need to compute
        // the position with respect to camera's position and rotation every frame.
        //
        
        // OPTION 1
        // http://xboxforums.create.msdn.com/forums/p/41571/244967.aspx
        // take your camera view matrix and Matrix.Invert it.  
        // You now have a matrix ( call it Camera_world) that will rotate an object to align its 
        // .Forward with the camera's lookat vector and Up and right etc..
        // Use viewFrustum = new BoundingFrustum( cameraView * cameraProjecton) to create a bounding frustum for the camera
        // Use viewfrustum.GetCorners to return the 3d coordinates of the four corners of each of the Near 
        // and Far clip planes.
        // Set camera_world.translation to the corner where you want to place your axes object and use 
        // camera_world as your axes object's world matrix.

        private static Renderable3DLines[] GeneratedLines(Vector3d offset, float columnSpacing, float halfColumns)
        {

            Line3d currentLine = new Line3d((float)offset.x, (float)((halfColumns * columnSpacing) + offset.y), (float)offset.z,
                                                          (float)offset.x, (float)((-halfColumns * columnSpacing) + offset.y), (float)offset.z); //  green for up axis
            
            Renderable3DLines axisG = new Renderable3DLines(new Line3d[] {currentLine}, new Color(0f, 1.0f, 0f, 1.0f));
          
            
            currentLine = new Line3d((float)((halfColumns * columnSpacing) + offset.x), (float)offset.y, (float)+offset.z,
                                                 (float)((-halfColumns * columnSpacing) + offset.x), (float)offset.y, (float)+offset.z); //  red for right axis

            Renderable3DLines axisR = new Renderable3DLines(new Line3d[] { currentLine }, new Color(1.0f, 0.0f, 0.0f, 1f));


            
            currentLine = new Line3d((float)offset.x, (float)offset.y, (float)((halfColumns * columnSpacing) + offset.z),
                                                (float)offset.x, (float)offset.y, (float)((-halfColumns * columnSpacing) + offset.z)); //  blue for front axis

            Renderable3DLines axisB = new Renderable3DLines(new Line3d[] { currentLine }, new Color(0.0f, 0.0f, 1.0f, 1f));


            return new Renderable3DLines[] { axisR, axisG, axisB };
        }


        //        //public static void DrawAxisIndicator(Vector3d offset, Camera camera, float length)
        //        public static void DrawAxisIndicator(Vector3d offset, float columnSpacing, float halfColumns)
        //        {
        //            //// using 2d X,Y offset for the axis indicator on our screen, we want
        //            //// to take these projected coords and turn them into world coords

        //            //// typically you want the axis indicator in the same spot relative to the camera.  So to get
        //            //// the actual position of the indicator it's 
        //            //Vector3d position = camera.Position + offset;

        //            //// simply, the axis indicator is 3 lines that meet at a common point.  Each of the three lines
        //            //// only has one axis value different for its other endpoint.
        //            //// so if the common point is 0,0,0
        //            //// then endX = 10,0,0  (position.x + length, position.y, position.z)
        //            ////      endY = 0,10,0  (position.x, position.y + length, position.z)
        //            ////      endZ = 0,0,10  (position.x, position.y, position.z + length)
        //            //Core._CoreClient.Screen2D.Draw_Line3D((float)position.x, (float)position.y, (float)position.z,
        //            //                                (float)position.x + length, (float)position.y,
        //            //                                (float)position.z);
        //            //Core._CoreClient.Screen2D.Draw_Line3D((float)position.x, (float)position.y, (float)position.z,
        //            //                                (float)position.x, (float)position.y + length,
        //            //                                (float)position.z);
        //            //Core._CoreClient.Screen2D.Draw_Line3D((float)position.x, (float)position.y, (float)position.z,
        //            //                                (float)position.x, (float)position.y,
        //            //                                (float)position.z + length);

        //            //float r = 64;
        //            //CoreClient._CoreClient.Screen2D.Draw_Line3D(0, 0, 0, 10 * r, 0, 0, CoreClient._CoreClient.Globals.RGBA(1, 0, 0, 1));
        //            //CoreClient._CoreClient.Screen2D.Draw_Line3D(0, 0, 0, 0, 10 * r, 0, CoreClient._CoreClient.Globals.RGBA(0, 1, 0, 1));
        //            //CoreClient._CoreClient.Screen2D.Draw_Line3D(0, 0, 0, 0, 0, 10 * r, CoreClient._CoreClient.Globals.RGBA(0, 0, 1, 1));

        //            // TODO: i think i just used halfCOlumns and columnSpacing for all when i should have
        //            // used appropriate ones where applicable?
        //            Line3d currentLine = new Line3d((float)offset.x, (float)((halfColumns * columnSpacing) + offset.y), (float)offset.z,
        //                                                          (float)offset.x, (float)((-halfColumns * columnSpacing) + offset.y), (float)offset.z); //  green for up axis
        //            DrawLines(new Line3d[] { currentLine }, new Color(0f, 1.0f, 0f, 1.0f));
        //            currentLine = new Line3d((float)((halfColumns * columnSpacing) + offset.x), (float)offset.y, (float)+offset.z,
        //                                                 (float)((-halfColumns * columnSpacing) + offset.x), (float)offset.y, (float)+offset.z); //  red for right axis
        //            DrawLines(new Line3d[] { currentLine }, new Color(1.0f, 0.0f, 0.0f, 1f));
        //            currentLine = new Line3d((float)offset.x, (float)offset.y, (float)((halfColumns * columnSpacing) + offset.z),
        //                                                (float)offset.x, (float)offset.y, (float)((-halfColumns * columnSpacing) + offset.z)); //  blue for front axis
        //            DrawLines(new Line3d[] { currentLine }, new Color(0.0f, 0.0f, 1.0f, 1f));
        //        }
    }
}
