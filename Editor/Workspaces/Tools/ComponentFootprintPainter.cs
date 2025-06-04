using System;
using System.Collections.Generic;
using Keystone.EditTools;
using Keystone.Entities;
using Keystone.Collision;
using Keystone.Events;
using Keystone.Types;
using Keystone.Portals;
using Keystone.Cameras;
using Keystone.CSG;
using Keystone.Utilities;
using Keystone.Extensions;

namespace KeyEdit.Workspaces.Tools
{

    public class ComponentFootprintPainter : Tool
    {
        protected ComponentDesignWorkspace mWorkspace;
        protected Keystone.Portals.Interior.TILE_ATTRIBUTES mFlags;
        protected bool BitwiseAdd;
        protected BrushSize mBrushSize;
        protected BrushShape mBrushShape;

        public string LayerName;


        private int mStartPixelX, mStartPixelZ;

        protected System.Drawing.Point[] mSelectedPixels;

        public ComponentFootprintPainter(Keystone.Network.NetworkClientBase netClient, ComponentDesignWorkspace ws)
            : base(netClient)
        {
            if (ws == null) throw new ArgumentNullException();
            mWorkspace = ws;
            mFlags = Interior.TILE_ATTRIBUTES.NONE;

            // http://www.eternal-echo.net/sims/tutorials/footprint/
            //mWorkspace.Component 
            //mWorkspace.Visualization
            // TODO: we need to be able to get the dimensions of the visualization
            // if the dimensions have changed, we need to reset the entire footprint perhaps
        }


        /// <summary>
        /// Assign the value that we should apply to every painted target.
        /// e.g assign true to enable all footprint bits that are painted, assign false
        /// to disable all footprint bits that are painted
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="value"></param>
        public void SetValue(string layerName, Keystone.Portals.Interior.TILE_ATTRIBUTES flags, BrushSize brushSize, bool bitwiseAdd)
        {
            BitwiseAdd = bitwiseAdd;
            LayerName = layerName;
            mFlags = flags;
            //mFlags &= ~Interior.TILE_ATTRIBUTES.WALL;
            mBrushSize = brushSize;
        }

        public System.Drawing.Point[] SelectedPixels { get { return mSelectedPixels; } }


        public override void HandleEvent(EventType type, EventArgs args)
        {
            // keyboard related event
            if (args is KeyboardEventArgs)
            {
                return; // TODO:
            }

            if (type == EventType.KeyboardCancel)
            {
                return; // TODO:
            }

            // mouse related event
            MouseEventArgs mouseArgs = (MouseEventArgs)args;
            MousePosition3D = mouseArgs.UnprojectedCameraSpacePosition;

            // NOTE: the pickParamaters used are from mouseArgs.Viewport.RenderingContext.PickParameters
            mPickResult = Pick(mouseArgs.Viewport, mouseArgs.ViewportRelativePosition.X, mouseArgs.ViewportRelativePosition.Y);

            _viewport = mouseArgs.Viewport;
            if (_viewport == null)
            {
                System.Diagnostics.Debug.WriteLine("ComponentFootprintPainter.HandleEvent() - No viewport selected");
                return;
            }
            
            RenderingContext currentContext = _viewport.Context;
         
            switch (type)
            {
                case EventType.MouseDown:
                    if (mouseArgs.Button == Keystone.Enums.MOUSE_BUTTONS.LEFT || mouseArgs.Button == Keystone.Enums.MOUSE_BUTTONS.XAXIS) // TODO: wtf xasix?  why wont .LEFT work?!! localization testing needed
                    {
                        try
                        {
                            // TODO: there is a footprint visualizer that is pickable but also
                            //       there is the target entity of the workspace that has the footprint we 
                            //       are looking to modify.  I think that the target entity should be passed in
                            //       yes?  or else how do we know what it is

                            // TODO: how do we limit this to only work on the footprint
                            //       mesh?  For starters, we need to disable picking of the Component
                            //       itself.  I think the best way to do that is to disable picking
                            //       of the main scene and limit it in Context to HUD root only.
                            //       This way the only thing that the user can possibly pick is the
                            //       TileMaskGrid
                            // TODO: following assert cannot work because we have seperate Visualizations per viewport!
                            //       this is one of the flaws i think in our use of seperate huds for this particular case
                            //       i think we should only use one HUD and share between all viewports in this workspace
                            //System.Diagnostics.Debug.Assert(mWorkspace.Visualization == mPickResult.Entity); 
                            if (mPickResult.HasCollided)
                                MouseDown(mWorkspace.Visualization);

                            System.Diagnostics.Debug.WriteLine("ComponentFootprintPainter.HandleEvent() - MouseDown...");
                        }
                        catch (Exception ex)
                        {
                            // if the command execute fails we still want to end the input capture
                            System.Diagnostics.Debug.WriteLine("ComponentFootprintPainter.HandleEvent() - Error -" + ex.Message);

                        }
                        finally
                        {
                            // TODO: This i believe is probably false, we don't
                            // need to stop input capture do we?
                            //_hasInputCapture = false;
                            //DeActivate();

                        }
                    }
                    break;
                case EventType.MouseEnter:
                    System.Diagnostics.Debug.WriteLine("ComponentFootprintPainter.HandleEvent() - MouseEnter...");



                    break;
                case EventType.MouseMove:
                    // if (mStartPickResults == null) return; // don't return immediately because for door placer
                    // for instance, on mousemove we want to be able to compute
                    // potential drop locations even if we don't actually decide on them
                    // but our Hud.cs can use the potential locations to draw preview visual

                    MouseMove(mWorkspace.Visualization);
                    System.Diagnostics.Debug.WriteLine("ComponentFootprintPainter.HandleEvent() - Mouse Move...");
                    break;

                // TODO: i think input capture is required... that might be part of the issue
                // with propogation of change flag recursion issues.  we'll have to logically
                // work this out but actually i think thats easy once we're ready toclean that up
                case EventType.MouseUp:
                    System.Diagnostics.Debug.WriteLine("ComponentFootprintPainter.HandleEvent() - Mouse Up.");
                    MouseUp(mWorkspace.Visualization);

                    break;

                case EventType.MouseLeave:  // TODO: verify this event occurs when we switch tools
                    System.Diagnostics.Debug.WriteLine("ComponentFootprintPainter.HandleEvent() - Mouse Leave.");

                    break;

                default:
                    break;
            }
        }


        protected virtual void MouseDown(ModeledEntity visualizer)
        {
            if (mWorkspace.Component == null) return;
            
            // update the snapshot of the footprint of the current target entity
            Settings.PropertySpec footprintProperty = 
                mWorkspace.Component.GetProperty("footprint", false);

            if (footprintProperty.DefaultValue == null)
                return;

            mStartPickResults = mPickResult;
            
            HitPoint((float)mStartPickResults.ImpactPointLocalSpace.x, (float)mStartPickResults.ImpactPointLocalSpace.z,
                       out mStartPixelX, out mStartPixelZ);
        }

        /// <summary>
        /// Since ComponentWorkspace holds our target Component and the
        /// Visualization modeled entity, mouse down and mouse up are only used for
        /// knowing when painting operation is active.
        /// </summary>
        /// <param name="celledRegion"></param>
        protected virtual void MouseMove(ModeledEntity visualizer)
        {
            // if mouse down == false, then mStartPickResults will be null
            if (mStartPickResults == null || mWorkspace.Component == null) return;


            int endPixelX, endPixelZ;

            HitPoint ((float)mPickResult.ImpactPointLocalSpace.x, (float)mPickResult.ImpactPointLocalSpace.z,
                       out endPixelX, out endPixelZ);
            

            System.Diagnostics.Debug.WriteLine("pixelX = " + endPixelX.ToString() + " pixelZ = " + endPixelZ.ToString());


            mSelectedPixels = SelectPixelsUnderBrush(mStartPixelX, endPixelX, mStartPixelZ, endPixelZ);
            int[,] data = ApplyPaintOperationChangesToData(mSelectedPixels, (int)mWorkspace.FootPrintHeight, BitwiseAdd);



            // TODO: as we do for power link painting, here too we must do realtime preview

        }

        protected virtual void MouseUp(ModeledEntity visualizer)
        {

            if (mStartPickResults == null || mWorkspace.Component == null) return;

            int endPixelX, endPixelZ;

            HitPoint((float)mPickResult.ImpactPointLocalSpace.x, (float)mPickResult.ImpactPointLocalSpace.z,
                       out endPixelX, out endPixelZ);

            mSelectedPixels = SelectPixelsUnderBrush(mStartPixelX, endPixelX, mStartPixelZ, endPixelZ);

            // apply the "footprint" bit to our footprint data at every returned pixel coordinate
            int[,] data = ApplyPaintOperationChangesToData(mSelectedPixels, (int)mWorkspace.FootPrintHeight, BitwiseAdd);

            
			// we paint on mouse up which signifies completion of the command
            // TODO: however, I think we may change this such that the network command
            // only gets sent when we "Apply" or "Save" the edits.  
            if (data != null)
                SendPaintChanges(mWorkspace.Component.ID, data);


            mStartPickResults = null;
            mSelectedPixels = null; // clear on mouse up
            mStartPixelX = -1;
            mStartPixelZ = -1;
        }


        public void ClearData()
        {
            int[,] data = mWorkspace.Component.Footprint.Data;
            Array.Clear(data, 0, data.Length);
            SendPaintChanges(mWorkspace.Component.ID, data);
        }

        private void HitPoint(float impactPointX, float impactPointZ, out int endPixelX, out int endPixelZ)
        {
            // WARNING: If this seems to be failing, make sure you are picking the Grid and NOT the Component Entity we are 
            //          creating the footprint for!
            Keystone.Celestial.ProceduralHelper.MapImpactPointToPixelCoordinate(
                               mWorkspace.GridWidth, mWorkspace.GridDepth,
                               mWorkspace.FootPrintWidth, mWorkspace.FootPrintHeight,
                               impactPointX, impactPointZ,
                               out endPixelX, out endPixelZ);

        }

        private System.Drawing.Point[] SelectPixelsUnderBrush(int startX, int endX, int startZ, int endZ)
        {
            switch (mBrushShape)
            {
                case BrushShape.Pencil:
                    return SelectCircle(new System.Drawing.Point(endX, endZ) , 1);

                case BrushShape.Rectangle:
                    return SelectRectangle(startX, endX, startZ, endZ);
               
                case BrushShape.Line :
                default:
                    return SelectLine(startX, endX, startZ, endZ); 
            }
        }

        private System.Drawing.Point[] SelectRectangle(int startX, int endX, int startZ, int endZ)
        {
            int width = 1, height = 1;

            switch (mBrushSize)
            {
                case BrushSize.Size_1x1 :
                    width = 1; height = 1;
                    break;
                case BrushSize.Size_2x2:
                    width = 2; height = 2;
                    break;
                case BrushSize.Size_4x4:
                    width = 4; height = 4;
                    break;

                default:
                    break;
            }

            // we need to fill a rectangular area 
            // TODO: i think our "paintline" could also be treated as a rectangle with width or height == 1

            // create a rectangular brush and which travels from the start to the end point touching all pixels inbetween

            // first get the line of pixels from start to end
            System.Drawing.Point[] pixelLine = SelectLine(startX, endX, startZ, endZ);
            System.Drawing.Point[] results = new System.Drawing.Point[0];
            if (pixelLine != null)
                for (int i = 0; i < pixelLine.Length; i++)
                {
                    results = results.ArrayAppendRange(SelectRectangle(pixelLine[i], width, height));
                }

            // remove duplicate's from the result
            return results;

        }

        private System.Drawing.Point[] SelectRectangle(System.Drawing.Point point, int  width, int height)
        {
            if (width == 1 && height == 1) return new System.Drawing.Point[] { point};

            List<System.Drawing.Point> result = new List<System.Drawing.Point>(width * height);

            double halfWidth = width / 2d;
            double halfHeight = height / 2d;

            int startX = -(int)Math.Floor(halfWidth) + point.X;
            int startY = -(int)Math.Floor(halfHeight) + point.Y;

            if (halfWidth % 2 == 0)
                startX -= 1;

            if (halfHeight % 2 == 0)
                startY -= 1;


            for (int x = startX; x < startX + width; x++)
                for (int y = startY; y < startY + height; y++)
                {
                    if (x < 0 || y < 0) continue; // skip pixels that are out of bounds
                    result.Add(new System.Drawing.Point(x, y));
                }

            return result.ToArray();
        }

        private System.Drawing.Point[] SelectCircle(System.Drawing.Point point, int radius)
        {
            System.Drawing.Point[] pixels;
            //radius = 2;
            if (radius == 1)
            {
                pixels = new System.Drawing.Point[radius];
                pixels[0] = point;
            }
            else
            {
                List<System.Drawing.Point> points = new List<System.Drawing.Point>();
                double radiusSquared = radius * radius;
                for (int x = 0; x < radius; x++)
                {
                    for (int y = 0; y < radius; y++)
                    {
                        double dx = x; // - point.X;
                        double dy = y; // - point.Y;
                        double distanceSquared = dx * dx + dy * dy;

                        if (distanceSquared <= radiusSquared)
                        {
                            // NOTE: we do bounds testing in ApplyPaintOperationChangesToData()
                            points.Add(new System.Drawing.Point(x + point.X, y + point.Y));
                            points.Add(new System.Drawing.Point(point.X - x, point.Y - y));
                        }
                    }
                }

                pixels = points.ToArray();
            }

            return pixels;
        }

        // a line that starts at a point and touches all pixels between it and the end point
        private System.Drawing.Point[] SelectLine(int startX, int endX, int startZ, int endZ)
        {
            // get the tiles within the footprint that are in the path from start and end pick points
            // find the biggest component difference
            int sizeX = Math.Abs(startX - endX);
            int sizeZ = Math.Abs(startZ - endZ);

            int max = Math.Max(sizeX, sizeZ) + 1;
            System.Drawing.Point[] pixels = new System.Drawing.Point[max];

            // if sizeX and sizeZ are the same then the start and end pixels MUST EITHER BE the same point
            // or form a diagonal which is not allowed.
            if (sizeX == 0 && sizeZ == 0)
            {
                // start and end are same point
                pixels[0] = new System.Drawing.Point(startX, startZ);
            }
            else if (sizeX != 0 && sizeZ != 0)
            {
                System.Diagnostics.Debug.WriteLine("Diagonals not allowed.");
                return null;
            }
            else if (sizeX > sizeZ)
            {
                // horizontal line
                int j = 0;
                int start = startX, stop = endX;
                if (startX > endX)
                {
                    start = endX;
                    stop = startX;
                }
                for (int i = start; i <= stop; i ++)
                {
                    pixels[j] = new System.Drawing.Point(i, endZ);
                    j++;
                }
            }
            else if (sizeZ > sizeX)
            {
                // vertical line
                int j = 0;
                int start = startZ, stop = endZ; 
                if (startZ > endZ)
                {
                    start = endZ;
                    stop = startZ;
                }
                for (int i = start; i <= stop; i ++)
                {
                    pixels[j] = new System.Drawing.Point(endX, i);
                    j++;
                }
            }


            if (pixels.Length == 0) return null;
            return pixels;
        }

        /// <summary>
        /// Update the entire footprint data with the pixels that were modified during the paint operation.
        /// </summary>
        /// <param name="pixels"></param>
        /// <param name="zDimension"></param>
        /// <param name="bitwiseAdd"></param>
        /// <returns></returns>
        private int[,] ApplyPaintOperationChangesToData(System.Drawing.Point[] pixels, int zDimension, bool bitwiseAdd)
        {
            // TODO: i think the problem with GetPixels() here is that since the hitpoints we get back are
            // in bitmap space, when we apply these directly to the footprint in bitmap space, we forget that
            // when we apply the footprint we convert whatever is in the footprint to bitmap space also!  So we
            // effectivley undo the mapping
            if (pixels == null) return null;

            // get copy of existing data
            int[,] data = mWorkspace.Component.Footprint.Data;
            for (int i = 0; i < pixels.Length; i++)
            {
                // when applying to the footprint, we must actually UNDO the conversion from
                // footprint to bitmap space which was done by virtue of GetPixel() using bitmap space
                // impact points!  And since the apply of the footprint to our visualization texture
                // will do this conversion again, it ends up getting done twice! (thus canceling out!)
                int reversed = zDimension - 1 - pixels[i].Y; // -1 because we're 0 based index and zDimension is a 1 based count

                // bounds test
                if (pixels[i].X < 0 || pixels[i].X > data.GetLength(0) - 1) continue;
                if (reversed < 0 || reversed > data.GetLength(1) - 1) continue;

                // apply operation
                if (bitwiseAdd)
                    data[pixels[i].X, reversed] |= (int)mFlags;
                else
                    data[pixels[i].X, reversed] &= ~(int)mFlags;
            }

            return data;
        }

        

        protected virtual void SendPaintChanges(string entityID, int[,] newFootprintData )
        {
            System.Diagnostics.Debug.Assert(newFootprintData != null, "Null value not allowed.  A command for painting footprint must have paint value of true or false.");
            System.Diagnostics.Debug.Assert(newFootprintData.GetLength(0) > 0);
            System.Diagnostics.Debug.Assert(newFootprintData.GetLength(1) > 0);

            //for component footprints, we send the entire modified footprint and then
            // change it as a single property as opposed to one cell at a time as in CelledRegion footprint painting
            KeyCommon.Messages.MessageBase mNetworkMessage = new KeyCommon.Messages.Node_ChangeProperty();

            CellFootprint fp = CellFootprint.Create(newFootprintData);



            // DEBUG
            for (int i = 0; i < newFootprintData.GetLength(0); i++)
                for (int j = 0; j < newFootprintData.GetLength(1); j++)
                {
                    if ((newFootprintData[i, j] & (int)Interior.TILE_ATTRIBUTES.WALL) != 0)
                        System.Diagnostics.Debug.WriteLine("Wall detected.");
                }
            // END DEBUG

            string footprintID = fp.ID; // encoded footprint is stored in the .ID.  We don't need to keep the actual CellFootprint node loaded

            Keystone.Resource.Repository.IncrementRef(fp);
            Keystone.Resource.Repository.DecrementRef(fp);

            // when we send the footprint, we only send the encoded string  because this is how footprints
            // are saved in Entity and then entity only creates a Footprint instance based on decoding the string.
            ((KeyCommon.Messages.Node_ChangeProperty)mNetworkMessage).Add("footprint", typeof(string).Name, footprintID);
            ((KeyCommon.Messages.Node_ChangeProperty)mNetworkMessage).NodeID = entityID;
            
            mNetClient.SendMessage(mNetworkMessage);
        }
    }
}
