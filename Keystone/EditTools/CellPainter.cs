using System;
using Keystone.Events;
using Keystone.Types;
using Keystone.Collision;
using Keystone.Portals;
using Keystone.Entities;

namespace Keystone.EditTools
{
    /// <summary>
    /// Based on 
    /// </summary>
    public class CellPainter : Tool
    {
        // TODO: maybe EntityPlacer is good enough and instead of CellPaintMode
        // we have EntityPlaceMode which can include CellPainting as a flag
        enum CellPaintMode
        {
            Floor,
            Wall,  // what about bottom, mid, upper?
            Ceiling,
            Corner,
            CSG,
            WallFixture,
            CeilingFixture,
            FloorFixture
        }

        public CellPainter(Keystone.Network.NetworkClientBase netClient) : base(netClient)
        {
            mIsActive = true;
            mHasInputCapture = true;
        }

        public override void HandleEvent(EventType type, EventArgs args)
        {
            MouseEventArgs mouseArgs = (MouseEventArgs)args;
            PickResults pickResult = (PickResults)(mouseArgs).Data;

            Entity parentEntity = pickResult.Entity;
            if (parentEntity == null) return;

            System.Diagnostics.Debug.Assert(parentEntity is Interior);

            switch (type)
            {
                // TODO: need an event type to know when we've completed plotting points?

                case EventType.MouseMove:
                    break;

                case EventType.MouseDown:
                    if (mouseArgs.Button == Enums.MOUSE_BUTTONS.XAXIS) // TODO: wtf xasix?  why wont .LEFT work?!! localization testing needed
                    {
                        _viewport = mouseArgs.Viewport;
                        if (_viewport == null)
                        {
                            System.Diagnostics.Debug.WriteLine("No viewport selected");
                            return;
                        }
                    }
                    System.Diagnostics.Debug.WriteLine("CellPainter.HandleEvent() - Mouse Down.");
                    break;
                case EventType.MouseLeave:  // TODO: verify this event occurs when we switch tools
                    System.Diagnostics.Debug.WriteLine("CellPainter.HandleEvent() - Mouse Leave.");
                    break;

       
                default:
                    break;
            }
        }
    }
}
