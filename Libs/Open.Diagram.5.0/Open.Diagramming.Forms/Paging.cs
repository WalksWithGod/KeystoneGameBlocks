// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Drawing;
using System.Runtime.Serialization;
using System.ComponentModel;

using Open.Diagramming.Forms.Rendering;

namespace Open.Diagramming.Forms
{
	public class Paging
	{
		//Property variables
        private bool _enabled;
        private Margin _margin;
        private Color _workspacecolor;
        private Size _workspaceSize;
        private SizeF _pageSize;
        private Point _workspaceOffset;
        private Point _pageOffset;
        private SizeF _padding;
        private Render _render;
        private int _page;

		//Constructors
		public Paging()
		{
            Margin = new Margin();
            WorkspaceColor = SystemColors.AppWorkspace;
            Enabled = true;
            Padding = new SizeF(40, 40);
            Page = 1;
		}

		//Properties
        [Description("Determines whether the view is drawn as a set of pages.")]
        public virtual bool Enabled
        {
            get 
            { 
                return _enabled; 
            }
            set 
            {
                _enabled = value;
                if (Render != null) Render.DisposeGraphicsStateBitmap();  
            }
        }

        [Description("Gets Origin sets the current page displayed.")]
        public virtual int Page
        {
            get
            {
                return _page;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("Page may not be less than 1.");
                _page = value;
            }
        }

        [Description("Defines the distance away from the edge of the page that elements should not be placed in.")]
        public virtual Margin Margin
        {
            get
            {
                return _margin;
            }
            set
            {
                if (value == null) throw new ArgumentNullException();
                _margin = value;
                if (Render != null) Render.DisposeGraphicsStateBitmap(); 
            }
        }

        [Description("Sets or gets the color used to draw the application workspace.")]
        public virtual Color WorkspaceColor
        {
            get
            {
                return _workspacecolor;
            }
            set
            {
                _workspacecolor = value;
                if (Render != null) Render.DisposeGraphicsStateBitmap(); 
            }
        }

        [Description("Sets or gets the overall workspace rectangle for a paged render.")]
        public virtual Size WorkspaceSize
        {
            get
            {
                return _workspaceSize;
            }
        }

        [Description("Defines the size of the page displayed when pagging is enabled.")]
        public virtual SizeF PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = value;
                if (Render != null) Render.DisposeGraphicsStateBitmap();
            }
        }

        public virtual Point WorkspaceOffset
        {
            get
            {
                return _workspaceOffset;
            }
        }

        public virtual Point PageOffset
        {
            get
            {
                return _pageOffset;
            }
        }

        public virtual SizeF Padding
        {
            get
            {
                return _padding;
            }
            set
            {
                _padding = value;
                if (Render != null) Render.DisposeGraphicsStateBitmap();
            }
        }

        protected internal Render Render
        {
            get
            {
                return _render;
            }
            set
            {
                _render = value;
            }
        }

        protected internal void SetWorkspaceSize(Size value)
        {
            _workspaceSize = value;
            if (Render != null) Render.DisposeGraphicsStateBitmap();
        }

        protected internal void SetWorkspaceOffset(Point value)
        {
            _workspaceOffset = value;
            if (Render != null) Render.DisposeGraphicsStateBitmap();
        }

        protected internal void SetPageOffset(Point value)
        {
            _pageOffset = value;
            if (Render != null) Render.DisposeGraphicsStateBitmap();
        }
	}
}
