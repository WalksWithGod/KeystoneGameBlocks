// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Text;

namespace Open.Diagramming
{
    //Command is part of the Command pattern
    public abstract class Command: AbstractCommand
    {
        private Controller _controller;

        //Constructors
        public Command(Controller controller): base()
        {
            _controller = controller;
        }

        //Properties
        public Controller Controller
        {
            get
            {
                return _controller;
            }
        }

        //Methods
        public abstract void Undo();
        public abstract void Redo();
    }
}
