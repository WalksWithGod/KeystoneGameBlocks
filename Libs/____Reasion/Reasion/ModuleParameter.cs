
using System;

namespace Reasion
{
	public enum ParameterDisplayType { Text, Slider };
	
	public class ModuleParameter
	{
		private string name;
		private ParameterDisplayType displayType;
		private double minValue;
		private double maxValue;
		private double stepping;
		private double defaultValue;
		private double currentValue;
		
		public string Name { get { return this.name; } }
		public ParameterDisplayType DisplayType { get { return this.displayType; } }
		public double MinValue { get { return this.minValue; } }
		public double MaxValue { get { return this.maxValue; } }
		public double Stepping { get { return this.stepping; } }
		public double DefaultValue { get { return this.defaultValue; } }
		public double Value { get { return this.currentValue; } }
		
		public ModuleParameter(string name, ParameterDisplayType displayType, double minValue, double maxValue, double stepping, double defaultValue)
		{
			this.name = name;
			this.displayType = displayType;
			this.maxValue = maxValue;
			this.minValue = minValue;
			this.stepping = stepping;
			this.defaultValue = defaultValue;
			this.SetValue(this.defaultValue);
		}
		
		public void SetValue(string value)
		{
			this.SetValue(Convert.ToDouble(value));
		}
		
		public void SetValue(double value)
		{
			if (value < this.minValue)
			{
				this.currentValue = this.minValue;
			}
			else if (value > this.maxValue)
			{
				this.currentValue = this.maxValue;
			}
			else
			{
				this.currentValue = value;
			}
		}
	}
}
