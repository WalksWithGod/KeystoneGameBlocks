using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.ComponentModel;

using Open.Diagramming.Forms;

namespace Open.Diagramming.Printing
{
	[ToolboxItem(false)]
	public class PrintDocument : System.Drawing.Printing.PrintDocument
	{
		//Property variables
		private View _diagram;
		private PrintRender _render;
		private PageNumberStyle _pageNumberStyle;
		private BorderStyle _borderStyle;
		private float _scale;
		private bool _scaleToFit;
		private bool _clip;

		//Working variables
		private static int _horizonalPages;
		private int _verticalPages;
		private int _lastPage;
		private int _totalPages;

		#region Interface 

		//Constructors
		public PrintDocument(View Diagram)
		{
			this.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(PrintPageHandlerRender);
			
			_diagram = Diagram;
			this.Render = new PrintRender();
			PageNumberStyle = PageNumberStyle.ColumnRow;
			BorderStyle = BorderStyle.Dot;
			Scale = 100;
			_clip = true;

			SetRenderLists();
		}

		//Properties
		//Gets or sets the style for showing printed page numbers
		[Description("Sets or gets the style of the numbers printed on each page.")]
		public virtual PageNumberStyle PageNumberStyle
		{
			get
			{
				return _pageNumberStyle;
			}
			set
			{
				_pageNumberStyle = value;
			}
		}

		//Gets or sets the style for drawing page borders
		[Description("Sets or gets the style of the border around each page.")]
		public virtual BorderStyle BorderStyle
		{
			get
			{
				return _borderStyle;
			}
			set
			{
				_borderStyle = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Description("Sets or gets the Diagram used for printing.")]
		public virtual View Diagram
		{
			get
			{
				return _diagram;
			}
			set
			{
				_diagram = value;
				SetRenderLists();
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Description("Sets or gets the render used to render the print document.")]
		public virtual PrintRender Render
		{
			get
			{
				return _render;
			}
			set
			{
				_render = value;
				SetRenderLists();
			}
		}

		[Description("Sets or gets the scale of the print out.")]
		public virtual float Scale
		{
			get
			{
				return _scale;
			}
			set
			{
				_scale = value;
			}
		}

		[Description("Determines if blank columns and rows are removed from the print out.")]
		public virtual bool Clip
		{
			get
			{
				return _clip;
			}
			set
			{
				_clip = value;
			}
		}

		[Description("Determines whether the document should be made to fit on a page.")]
		public virtual bool ScaleToFit
		{
			get
			{
				return _scaleToFit;
			}
			set
			{
				_scaleToFit = value;
			}
		}

		[Description("Determines whether the document is printed with filled shapes.")]
		public virtual bool DrawBackground 
		{
			get
			{
				return _render.DrawBackground;
			}
			set
			{
				_render.DrawBackground = value;
			}
		}

		[Description("Determines whether the document is printed with shadows.")]
		public virtual bool DrawShadows
		{
			get
			{
				return _render.DrawShadows;
			}
			set
			{
				_render.DrawShadows = value;
			}
		}

		[Description("Determines whether the document is printed without color.")]
		public virtual bool Greyscale
		{
			get
			{
				return _render.Greyscale;
			}
			set
			{
				_render.Greyscale = value;
			}
		}

		//Methods
		[Description("Shows the document in print preview mode .")]
		public virtual void PrintPreview()
		{
			if (_diagram == null) throw new PrintDocumentException("A Diagram reference was not set for this PrintDocument.");

			System.Windows.Forms.PrintPreviewDialog objDialog = new System.Windows.Forms.PrintPreviewDialog();
			objDialog.Document = this;

			//Set the zoom
			_render.Zoom = _diagram.Zoom;

			objDialog.ShowDialog();
			objDialog.Dispose();
		}

		[Description("Shows the document in print preview mode at the specified location and size.")]
		public virtual void PrintPreview(Point Location, Size Size)
		{
			if (_diagram == null) throw new PrintDocumentException("A Diagram reference was not set for this PrintDocument.");

			System.Windows.Forms.PrintPreviewDialog objDialog = new System.Windows.Forms.PrintPreviewDialog();
			objDialog.Document = this;
			objDialog.Location = Location;
			objDialog.Size = Size;

			//Set the zoom
			_render.Zoom = _diagram.Zoom;

			objDialog.ShowDialog();
			objDialog.Dispose();
		}

		#endregion

		#region Implementation

		protected virtual void PrintPageHandlerRender(object sender, System.Drawing.Printing.PrintPageEventArgs e)
		{
			if (_diagram == null) return;

			Rectangle renderRectangle = new Rectangle();

			int column = 0;
			int row = 0;
			float scale = Scale / 100;

			//Calculate defaults if this is the first page we are printing
			if (_lastPage == 0)
			{
				//Set the scale if ScaleToFit is set to true
				if (ScaleToFit)
				{
					float rx = (Convert.ToSingle(e.MarginBounds.Width) / Convert.ToSingle(Diagram.Model.Size.Width));
					float ry = (Convert.ToSingle(e.MarginBounds.Height) / Convert.ToSingle(Diagram.Model.Size.Height));
					
					scale = (rx < ry) ? rx : ry;
					Scale = scale;
				}
								
				SizeF scaledSize = new SizeF(_diagram.Model.Size.Width * scale,_diagram.Model.Size.Height * scale);

				//Consider the visible boundary of the elements in the diagram if truncating
				if (_clip)
				{
					RectangleF bounds = Render.Elements.GetBounds();

                    //Only consider if there are elements
                    if (!bounds.IsEmpty)
                    {
                        RectangleF visible = new RectangleF(0, 0, bounds.Right, bounds.Bottom);

                        scaledSize = new SizeF(visible.Width * scale, visible.Height * scale);
                    }
				}
								
				//Get the horizontal and vertical scaled pages
				_horizonalPages = Convert.ToInt32(Math.Floor(scaledSize.Width / e.MarginBounds.Width));
				if (scaledSize.Width % e.MarginBounds.Width > 0) _horizonalPages += 1;

				_verticalPages = Convert.ToInt32(Math.Floor(scaledSize.Height / e.MarginBounds.Height));
				if (scaledSize.Height % e.MarginBounds.Height > 0) _verticalPages += 1;

				if (PrinterSettings.PrintRange == PrintRange.SomePages)
				{
					if (PrinterSettings.FromPage > 1)
					{
						_lastPage = this.PrinterSettings.FromPage - 1;
					}
				}

				_totalPages = _horizonalPages * _verticalPages;
			}

			column = _lastPage % _horizonalPages;
			row = (_lastPage / _horizonalPages);

			//Calculate the render rectangle for this page
			renderRectangle.Location = new Point(column * e.MarginBounds.Width, row * e.MarginBounds.Height);
			renderRectangle.Width = e.MarginBounds.Width;
			renderRectangle.Height = e.MarginBounds.Height;

			e.Graphics.Clip = new Region(e.MarginBounds);
			e.Graphics.TranslateTransform(-renderRectangle.X + e.MarginBounds.X, -renderRectangle.Y + e.MarginBounds.Y);
			
			//Perform any scaling if required
			e.Graphics.ScaleTransform(scale,scale);

			//_render.MarginBounds = e.MarginBounds;
			_render.RenderRectangle = renderRectangle;
            _render.SetGraphics(e.Graphics);
			_render.RenderDiagram(renderRectangle);

			//Draw row and column information
			StringFormat stringFormat = new StringFormat();
			RectangleF pageNumberRect = new RectangleF(e.MarginBounds.Left, e.MarginBounds.Bottom, e.MarginBounds.Width, e.PageBounds.Height - e.MarginBounds.Top - e.MarginBounds.Height);

			stringFormat.Alignment = StringAlignment.Center;
			stringFormat.LineAlignment = StringAlignment.Center;

			//Add 1 to row and column for printing
			row += 1;
			column += 1;

			e.Graphics.Clip = new Region();
			e.Graphics.ResetTransform();

			string pageNumber = GetPageNumber(row, column, _lastPage + 1);
			if (pageNumber != "")
			{
				e.Graphics.DrawString(pageNumber, _diagram.Font, new SolidBrush(Color.Black), pageNumberRect, stringFormat);
			}

			//Draw a border
			if (_borderStyle != BorderStyle.None)
			{
				Pen pen = GetBorderPen();
				e.Graphics.DrawRectangle(pen, e.MarginBounds);
				pen.Dispose();
			}

			//Calculate if another page needs printing
			if (_lastPage < _totalPages - 1)
			{
				e.HasMorePages = true;
				_lastPage += 1;

				if (this.PrinterSettings.PrintRange == PrintRange.SomePages)
				{
					e.HasMorePages = ! (_lastPage >= this.PrinterSettings.ToPage);
				}
			}
			else
			{
				_lastPage = 0;
				_totalPages = 0;
			}
		}

		protected virtual string GetPageNumber(int intRow, int intColumn, int intSequence)
		{
			switch (_pageNumberStyle)
			{
				case PageNumberStyle.ColumnRow:
					return intColumn.ToString() + "," + intRow.ToString();
				case PageNumberStyle.RowColumn:
					return intRow.ToString() + "," + intColumn.ToString();
				case PageNumberStyle.Sequence:
					return intSequence.ToString();
			}
			return "";
		}

		//Add all shapes and lines to renderlist
		private void SetRenderLists()
		{
			Render.Elements = new ElementList(true);
			Render.Layers = Diagram.Model.Layers;
			
			foreach (Shape shape in Diagram.Model.Shapes.Values)
			{
				Render.Elements.Add(shape);
			}

			foreach (Link line in Diagram.Model.Lines.Values)
			{
				Render.Elements.Add(line);
			}
			Render.Elements.Sort();
		}

		private Pen GetBorderPen()
		{
			Pen pen = new Pen(Color.Black);

			switch (_borderStyle)
			{
				case BorderStyle.Dash:
					pen.DashStyle = DashStyle.Dash;
					break;
				case BorderStyle.Dot:
					pen.DashStyle = DashStyle.Dot;
					break;
				case BorderStyle.Solid:
					pen.DashStyle = DashStyle.Solid;
					break;
			}

			return pen;
		}

		#endregion
	}
}