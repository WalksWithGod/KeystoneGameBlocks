// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace Open.Diagramming
{
    public class MouseElements
    {
        public Element MouseStartElement;
        public Element MouseMoveElement;
        public Element InteractiveElement;
        public Origin InteractiveOrigin;
        public Origin MouseStartOrigin;

        public Point StartPoint;
        public Point LastPoint;

        public Handle MouseHandle;

        public MouseElements()
        {
        }

        public MouseElements(MouseElements prototype)
        {
            MouseStartElement = prototype.MouseStartElement;
            MouseMoveElement = prototype.MouseMoveElement;
            MouseStartOrigin = prototype.MouseStartOrigin;
            InteractiveElement = prototype.InteractiveElement;
            InteractiveOrigin = prototype.InteractiveOrigin;
        }

        public void Clear()
        {
            MouseStartElement = null;
            MouseMoveElement = null;
            MouseStartOrigin = null;
            InteractiveElement = null;
            InteractiveOrigin = null;
        }

        public ISelectable MouseStartSelectable
        {
            get
            {
                return (ISelectable)MouseStartElement;
            }
        }

        public ISelectable MouseMoveSelectable
        {
            get
            {
                return (ISelectable)MouseMoveElement;
            }
        }

        public bool IsDockable()
        {
            if (MouseHandle == null) return false;
            if (!MouseHandle.CanDock) return false;
            if (MouseStartElement == null) return false;
            if (MouseMoveElement == null) return false;

            return true;
        }
    }
}
