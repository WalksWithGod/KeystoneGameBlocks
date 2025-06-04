using Gdk;
using System;
using System.ComponentModel;
using System.Collections.Specialized;

namespace Reasion
{
	namespace Modules
	{
		[DisplayName("Gray-Scott RD")]
		public class GrayScottRD : IReasionModule
		{
			private int width, height;
			private uint[] palette;
			private uint ticks;
			private ModuleParameter[] parameters = {
				new ModuleParameter("Width",  ParameterDisplayType.Slider, 10.0, 400.0, 1.0,   150.0),
				new ModuleParameter("Height", ParameterDisplayType.Slider, 10.0, 400.0, 1.0,   150.0),
				new ModuleParameter("DelU",   ParameterDisplayType.Text,   0.0,  1.0,   0.001, 0.16 ),
				new ModuleParameter("DelV",   ParameterDisplayType.Text,   0.0,  1.0,   0.001, 0.08 ),
				new ModuleParameter("DelF",   ParameterDisplayType.Text,   0.0,  1.0,   0.001, 0.035),
				new ModuleParameter("DelK",   ParameterDisplayType.Text,   0.0,  1.0,   0.001, 0.065),
				new ModuleParameter("DelLU",  ParameterDisplayType.Text,   0.0,  1.0,   0.001, 0.05 ),
				new ModuleParameter("DelLV",  ParameterDisplayType.Text,   0.0,  1.0,   0.001, 0.05 ),
				new ModuleParameter("BoxHSc", ParameterDisplayType.Slider, 0.05, 1.0,   0.01,  0.1  ),
				new ModuleParameter("BoxVSc", ParameterDisplayType.Slider, 0.05, 1.0,   0.01,  0.1  )
			};
			
			private double delU, delV, delF, delK, delLU, delLV;
			private double[] dataU, dataV, genU, genV;
			private double boxXScale, boxYScale;
			private double lowU, highU;
			
			public int Width { get { return this.width; } }
			public int Height { get { return this.height; } }
			public uint Ticks { get { return this.ticks; } }
			public uint[] Palette { get { return this.palette; } set { this.palette = value; } }
			public ModuleParameter[] Parameters { get { return this.parameters; } }
			
			public bool Initialize()
			{
				this.palette = new uint[] {0,0,0, 255,255,255};
				this.ticks = 0;
				
				this.UpdateParameters();
				
				int s = this.width * this.height;
				this.dataU = new double[s];
				this.dataV = new double[s];
				this.genU = new double[s];
				this.genV = new double[s];
			
				this.lowU = 0.0;
				this.highU = 1.0;
			
				Random rand = new Random(DateTime.Now.Millisecond);
				int w = (int)Math.Floor(this.width * this.boxXScale);
				int h = (int)Math.Floor(this.height * this.boxYScale);
				int rx = (int)Math.Floor((this.width / 2.0) - (w / 2.0));
				int ry = (int)Math.Floor((this.height / 2.0) - (h / 2.0));
				for (int y = 0; y < this.height; y++)
				{
					for (int x = 0; x < this.width; x++)
					{
						this.SetCell(this.dataU, x, y, 1.0);
						this.SetCell(this.dataV, x, y, 0.0);
						this.SetCell(this.genU, x, y, 0.0);
						this.SetCell(this.genV, x, y, 0.0);
					}
				}
				for (int y = 0; y < h; y++)
				{
					for (int x = 0; x < w; x++)
					{
						this.SetCell(this.dataU, rx + x, ry + y, 0.5 + ((rand.NextDouble() * 0.02) - 0.01));
						this.SetCell(this.dataV, rx + x, ry + y, 0.25 + ((rand.NextDouble() * 0.02) - 0.01));
					}
				}
				
				return true;
			}
		
			public bool Iterate()
			{
				this.lowU = 1.0;
				this.highU = 0.0;
				for (int y = 0; y < this.height; y++)
				{
					for (int x = 0; x < this.width; x++)
					{
						double u = this.GetCell(this.dataU, x, y);
						double v = this.GetCell(this.dataV, x, y);
						double lu = (u * -4.0) + this.GetCell(this.dataU, x + 1, y) + this.GetCell(this.dataU, x - 1, y) + this.GetCell(this.dataU, x, y + 1) + this.GetCell(this.dataU, x, y - 1);
						double lv = (v * -4.0) + this.GetCell(this.dataV, x + 1, y) + this.GetCell(this.dataV, x - 1, y) + this.GetCell(this.dataV, x, y + 1) + this.GetCell(this.dataV, x, y - 1);
						double nu = u + ((this.delU * (lu * this.delLU)) - (u * v * v) + (this.delF * (1.0 - u)));
						double nv = v + ((this.delV * (lv * this.delLV)) + (u * v * v) - ((this.delF + this.delK) * v));
						if (nu < this.lowU) { this.lowU = nu; }
						if (nu > this.highU) { this.highU = nu; }
						this.SetCell(this.genU, x, y, nu);
						this.SetCell(this.genV, x, y, nv);
					}
				}
			
				this.dataU = this.genU;
				this.dataV = this.genV;
				this.ticks++;
			
				return true;
			}
		
			public void Render(Gdk.Image image)
			{
				for (int y = 0; y < this.height; y++)
				{
					for (int x = 0; x < this.width; x++)
					{
						image.PutPixel(x, y, this.GetRGB(this.GetCell(this.dataU, x, y), this.lowU, this.highU));
					}
				}
			}
			
			public void UpdateParameters()
			{
				this.width = Convert.ToInt16(this.parameters[0].Value);
				this.height = Convert.ToInt16(this.parameters[1].Value);
				this.delU = this.parameters[2].Value;
				this.delV = this.parameters[3].Value;
				this.delF = this.parameters[4].Value;
				this.delK = this.parameters[5].Value;
				this.delLU = this.parameters[6].Value;
				this.delLV = this.parameters[7].Value;
				this.boxXScale = this.parameters[8].Value;
				this.boxYScale = this.parameters[9].Value;
			}
			
			private void SetCell(double[] data, int x, int y, double val)
			{
				if (x < 0 || x >= this.width) { x = (x + this.width) % this.width; }
				if (y < 0 || y >= this.height) { y = (y + this.height) % this.height; }
				data[(y * this.width) + x] = val;
			}
			
			private double GetCell(double[] data, int x, int y)
			{
				if (x < 0 || x >= this.width) { x = (x + this.width) % this.width; }
				if (y < 0 || y >= this.height) { y = (y + this.height) % this.height; }
				return data[(y * this.width) + x];
			}
			
			private uint GetRGB(double val, double low, double high)
			{
				double adjval = (val - low) / (high - low);
				double i = adjval * ((this.palette.Length / 3) - 1);
				double j = i - (int)Math.Floor(i);
				int ca = (int)Math.Ceiling(i) * 3;
				int cb = (int)Math.Floor(i) * 3;
				int r = (int)((j * this.palette[ca    ]) + ((1 - j) * this.palette[cb    ]));
				int g = (int)((j * this.palette[ca + 1]) + ((1 - j) * this.palette[cb + 1]));
				int b = (int)((j * this.palette[ca + 2]) + ((1 - j) * this.palette[cb + 2]));
				return (uint)(r << 16 | g << 8 | b);
			}
		}
	}
}