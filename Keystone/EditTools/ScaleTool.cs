using System;
using System.Collections.Generic;
using Keystone.Collision;
using Keystone.Commands;
using Keystone.Controllers;
using Keystone.Controls;
using Keystone.EditOperations;
using Keystone.Events;
using Keystone.Types;
using System.Diagnostics;

namespace Keystone.EditTools
{
    public class ScaleTool : TransformTool
    {

        public ScaleTool(Keystone.Network.NetworkClientBase netClient) 
            : base(netClient)
        {
            mActiveMode = TransformationMode.ScaleAxis;
            mManipulatorFunctions = new ManipFunctions2(); // only scaleTool still uses mManipulatorFunctions

            mControl = EditTools.Widgets.LoadScalingWidget();
			
            mControl.MouseEnter += OnMouseEnter;
            mControl.MouseLeave += OnMouseLeave;
            mControl.MouseDown += OnMouseDown;
            mControl.MouseUp += OnMouseUp;
            mControl.MouseClick += OnMouseClick;
            mControl.MouseDrag += OnMouseDrag;

            Resource.Repository.IncrementRef(mControl);
        }

    }
}
