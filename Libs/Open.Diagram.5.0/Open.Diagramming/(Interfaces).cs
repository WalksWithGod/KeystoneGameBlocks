// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Open.Diagramming.Layouts;

namespace Open.Diagramming
{
    public interface IView
    {
        void Suspend();
        void Resume();
        void Refresh();
        void Invalidate();
    }

	public interface IExpandable
	{
		bool Expanded {get; set;}
		GraphicsPath Expander {get;}
		bool DrawExpand {get; set;}
		SizeF ContractedSize {get; set;}
		SizeF ExpandedSize {get; set;}
	}

    //Capture windows forms clicks
	public interface IMouseEvents
	{
        bool ExecuteMouseCommand(MouseCommand command);
	}

	public interface ISelectable
	{
		event EventHandler SelectedChanged;
		bool DrawSelected {get; set;}
		bool Selected {get; set;}
	}

	public interface IUserInteractive
	{
		UserInteraction Interaction {get; set;}
	}

	public interface IPortContainer
	{
		Ports Ports {get; set;}

		void LocatePort(Port port);
		PortOrientation GetPortOrientation(Port port,PointF location);
		bool ValidatePortLocation(Port port,PointF location);
		float GetPortPercentage(Port port,PointF location);
        PointF Intercept(PointF location);
        PointF Forward(PointF location);
        bool Contains(PointF location);
	}

	public interface ILabelContainer
	{
		PointF GetLabelLocation();
		SizeF GetLabelSize();
	}

	public interface ITransformable
	{
		float Rotation {get; set;}
		GraphicsPath TransformPath {get;}
		RectangleF TransformRectangle {get;}
	}

    //Marker interface for any class to signal that it requires rendering
    public interface IRenderable
    {
    }
}

