using System;
using Keystone.Modeler;

namespace Keystone.EditTools
{
    public class RectangleTool : Tool
    {
     
        public RectangleTool(Keystone.Network.NetworkClientBase netClient) : base(netClient)
        {}

        public override void HandleEvent(Keystone.Events.EventType type, EventArgs args)
        {

            if (_mesh == null)
            {
                DeActivate();
                return;
            }

            switch (type)
            {
                case Keystone.Events.EventType.MouseDown:
                    // several things here... one being when we lay the next point we might decide it's close enough to 
                    // another point that they'll be merged?

                    // the args will contain the x,y coords and the viewtype so we'll know which plane to plot the point
                    // our MeshEditable needs a coherent spec on how it fits into a "scene" and with editing ..

                    // get the args

                    // determine 3d points based on the current view

                    // add the point to the target 

                    break;

                default:
                    break;
            }
        }

        private void AddRectangle()
        {
            // should the pencil determine based on mouse x,y and scene state where the point falls?
            // like how does sketchup determine which plane to plot on?

            // create a command that will perform the operation and allow us tor undo/redo the operation if commanded
       //     Commands.ICommand command = new EditOperations.PlotPoint(new Amib.Threading.PostExecuteWorkItemCallback(WorkItemCompleted));
       //     Core._Core.CommandProcessor.EnQueue(command);

        }

        private void WorkItemCompleted(Amib.Threading.IWorkItemResult result)
        {
        }
    }
}
