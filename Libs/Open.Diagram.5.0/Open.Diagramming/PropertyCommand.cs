﻿// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace Open.Diagramming
{
    public class PropertyCommand: Command 
    {
        //Property variables
        private ElementList _elements;
        private List<ElementMomento> _momentos;

        //Constructors
        public PropertyCommand(Element element): base(null)
        {
            _elements = new ElementList(true);
            _elements.Add(element);

            _momentos = new List<ElementMomento>();
            _momentos.Add(new ElementMomento(element));
        }

        public PropertyCommand(ElementList elements): base(null)
        {
            _elements = elements;
            _momentos = new List<ElementMomento>();

            //Loop through and add a momento for each element
            foreach (Element element in elements)
            {
                ElementMomento momento = new ElementMomento(element);
                _momentos.Add(momento);
            }
        }

        //Properties
        public virtual ElementList Elements
        {
            get
            {
                return _elements;
            }
        }

        //Methods
        public override void Execute()
        {
            return; 
        }

        public override void Undo()
        {
            UndoRedo();
        }

        public override void Redo()
        {
            UndoRedo();
        }

        private void UndoRedo()
        {
            int index = 0;

            //Loop through each element and write the contents of the matching momento back into the element
            foreach (Element element in Elements)
            {
                //Get a momento of the element in its current state
                ElementMomento momento = new ElementMomento(element);

                //Write the momento into the element and save the new momento
                _momentos[index].WriteItem(element);
                _momentos[index] = momento;

                index++;
            }
        }
    }
}
