// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Drawing;
using System.Runtime.Serialization;

using Open.Diagramming.Forms;
using Open.Diagramming.Layouts;
using Open.Diagramming.Serialization;
using Open.Diagramming.Forms.Rendering;

namespace Open.Diagramming.Serialization
{
	public class ViewSerialize: ISerializationSurrogate
	{
		private Shapes _shapes;
		private Lines _lines;
		private Layers _layers;

		//Constructors
		public ViewSerialize()
		{

		}

		//Properties
		public virtual Shapes Shapes
		{
			get
			{
				return _shapes;
			}
		}

		public virtual Lines Lines
		{
			get
			{
				return _lines;
			}
		}

		public virtual Layers Layers
		{
			get
			{
				return _layers;
			}
		}

		//Implement ISerializeSurrogate
		public virtual void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			View diagram = (View) obj;
			
			info.AddValue("Shapes",diagram.Model.Shapes,typeof(Elements));
			info.AddValue("Lines",diagram.Model.Lines,typeof(Elements));
			info.AddValue("Layers",diagram.Model.Layers,typeof(Layers));
			info.AddValue("Zoom",diagram.Zoom);
			info.AddValue("ShowTooltips",diagram.ShowTooltips);
            info.AddValue("Paged", diagram.Paging.Enabled);
			info.AddValue("Margin",diagram.Margin);
			info.AddValue("WorkspaceColor",diagram.Paging.WorkspaceColor.ToArgb().ToString());
		}

		public virtual object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			View diagram = (View) obj;

			diagram.Suspend();
			
			_shapes = (Shapes) info.GetValue("Shapes",typeof(Shapes));
			_lines = (Lines) info.GetValue("Lines",typeof(Lines));
			_layers = (Layers) info.GetValue("Layers",typeof(Layers));
			
			//Diagram is created without a constructor, so need to do some initialization
			diagram.Render = new ControlRender();

			diagram.Zoom = info.GetSingle("Zoom");
			diagram.ShowTooltips = info.GetBoolean("ShowTooltips");
            diagram.Paging.Enabled = info.GetBoolean("Paged");
            diagram.Paging.WorkspaceColor = Color.FromArgb(Convert.ToInt32(info.GetString("WorkspaceColor")));

			diagram.Resume();
			return diagram;
		}

		public virtual void UpdateObjectReferences()
		{
            //##
            ////Relink layers
            //foreach (Layer layer in _layers)
            //{	
            //    foreach (Element element in layer.Elements.Values)
            //    {
            //        element.SetLayer(layer);
            //    }
            //}
		}
	}
}
