using Gdk;
using System;
using System.ComponentModel;
using System.Collections.Specialized;

namespace Reasion
{
	namespace Modules
	{
		[DisplayName("Turing RD")]
		public class TuringRD : IReasionModule
		{
			int width, height;
			uint[] palette;
			uint ticks;
			private ModuleParameter[] parameters = {
				new ModuleParameter("Width",  ParameterDisplayType.Slider, 10.0, 400.0,  1.0,   200.0),
				new ModuleParameter("Height", ParameterDisplayType.Slider, 10.0, 400.0,  1.0,   200.0),
				new ModuleParameter("DelU",   ParameterDisplayType.Text,   0.0,  24.0,   0.001, 2.4  ),
				new ModuleParameter("DelV",   ParameterDisplayType.Text, 0.0,  24.0,   0.001, 24.0 )
			};
			
			double delU, delV;
			double[] dataU, dataV, genU, genV;
			double lowU, highU;
			
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
			
				this.lowU = 0.0f;
				this.highU = 14.0f;
			
				Random rand = new Random(DateTime.Now.Millisecond);
				for (int y = 0; y < this.height; y++)
				{
					for (int x = 0; x < this.width; x++)
					{
						this.SetCell(this.dataU, x, y, rand.NextDouble() * 14.0);
						this.SetCell(this.dataV, x, y, rand.NextDouble() * 14.0);
					}
				}
				
				return true;
			}
		
			public bool Iterate()
			{
				this.lowU = 14.0;
				this.highU = 0.0;
				for (int y = 0; y < this.height; y++)
				{
					for (int x = 0; x < this.width; x++)
					{
						double u = this.GetCell(this.dataU, x, y);
						double v = this.GetCell(this.dataV, x, y);
						double diu = this.delU * (this.GetCell(this.dataU, x + 1, y) - (2.0 * u) + this.GetCell(this.dataU, x - 1, y) + this.GetCell(this.dataU, x, y + 1) - (2.0 * u) + this.GetCell(this.dataU, x, y - 1));
						double reu = (u * v) - u - 12.0;
						double nu = u + (0.01 * (reu + diu));
						double div = this.delV * (this.GetCell(this.dataV, x + 1, y) - (2.0 * v) + this.GetCell(this.dataV, x - 1, y) + this.GetCell(this.dataV, x, y + 1) - (2.0 * v) + this.GetCell(this.dataV, x, y - 1));
						double rev = 16.0 - (u * v);
						double nv = v + (0.01 * (rev + div));
						if (nu < 0.0) { nu = 0.0; }
						if (nv < 0.0) { nv = 0.0; }
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
			
			private void UpdateParameters()
			{
				this.width = Convert.ToInt16(this.parameters[0].Value);
				this.height = Convert.ToInt16(this.parameters[1].Value);
				this.delU = this.parameters[2].Value;
				this.delV = this.parameters[3].Value;
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
				double i = adjval * ((this.palette.Length / 3)- 1);
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