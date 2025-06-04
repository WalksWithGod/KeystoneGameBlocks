// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;

using Open.Diagramming.Collections;

namespace Open.Diagramming.Serialization
{
    //Serializes or deserializes any Elements<T> based collection
	public class LayersSerialize: ISerializationSurrogate
	{
		public virtual void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
            Layers layers = obj as Layers;

            //Add a reference to the container
            info.AddValue("CurrentLayer", layers.CurrentLayer);
            info.AddValue("Model", layers.Model);
		}

		public virtual object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
            Layers layers = obj as Layers;
            SerializationInfoEnumerator enumerator = info.GetEnumerator();

            //Reset nullable properties
            layers.SetCurrentLayer(null);

            //Enumerate the info object and apply to the appropriate properties
            while (enumerator.MoveNext())
            {
                if (enumerator.Name == "CurrentLayer") layers.SetCurrentLayer(info.GetValue("CurrentLayer", typeof(Layer)) as Layer);
                else if (enumerator.Name == "Model") layers.SetModel(info.GetValue("Model", typeof(Model)) as Model);
            }

            return layers;
		}
	}
}
