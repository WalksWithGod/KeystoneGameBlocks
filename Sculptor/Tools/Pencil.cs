using System;
using System.Collections.Generic;
using Keystone.Commands;
using Sculptor.Operations;

namespace Sculptor.Tools
{
    public class Pencil : Tool
    {
        private ToolState _state;
        private bool m_isActive;

        public ToolState State
        { get { return _state; } set { _state = value;} }

        public void Activate()
        {
            m_isActive = true;
        }

        public void DeActivate()
        {
            m_isActive = false;
        }

        public override void HandleEvent(Keystone.Events.EventType type, EventArgs args)
        {
            switch (type)
            {
                case Keystone.Events.EventType.MouseDown :
                    // several things here... one being when we lay the next point we might decide it's close enough to 
                    // another point that they'll be merged?

                    // the args will contain the x,y coords and the viewtype so we'll know which plane to plot the point
                    // our MeshEditable needs a coherent spec on how it fits into a "scene" and with editing ..
                    break;
             
                default:
                    break;
            }
        }

        private void AddPoint()
        {
            // should the pencil determine based on mouse x,y and scene state where the point falls?
            // like how does sketchup determine which plane to plot on?

            // create a command that will perform the operation and allow us tor undo/redo the operation if commanded
            ICommand command = new PlotPoint();
           // _core.CommandProcessor.EnQueue(command);
            
        }
        

    }
}
