// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Text;

namespace Open.Diagramming
{
    public sealed class Clipboard
    {
        public Elements Elements; //stores cut or copied shapes
        public bool IsCopy;

        public void Write()
        {
            //Register the data format
            DataFormats.Format format = DataFormats.GetFormat(typeof(Elements).FullName);

            IDataObject ido = new DataObject();

            ido.SetData(format.Name, Elements);

            //True also causes immediate serialization, false delayed until it is needed
            System.Windows.Forms.Clipboard.SetDataObject(ido, true);
        }

        public void Read()
        {
            //Get the data format
            DataFormats.Format format = DataFormats.GetFormat(typeof(Elements).FullName);

            IDataObject ido = System.Windows.Forms.Clipboard.GetDataObject();

            if (ido.GetDataPresent(format.Name)) Elements = ((Elements)(ido.GetData(format.Name)));
        }

        //When loading from a clipboard, some items may need to be modified due to the way
        //.net added them to the clipboard
        public void ResolveItems()
        {
            if (Elements != null)
            {
                foreach (Element element in Elements.Values)
                {
                    //Recreate line points for elements
                    if (element is Link)
                    {
                        Link line = (Link)element;
                        line.DrawPath();
                    }
                }
            }
        }
    }
}
