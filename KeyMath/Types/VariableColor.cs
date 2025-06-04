using System;
using Keystone.Utilities;

namespace Keystone.Types
{
	/// <summary>
	/// Description of VariableColor.
	/// </summary>
	public struct VariableColor
	{
        /// <summary>
        /// The base value for the VariableColor.
        /// </summary>
        public Color Value;

        /// <summary>
        /// The range of the random variation around the base value.
        /// </summary>
        public Color Variation;

        /// <summary>
        /// Samples the VariableColor.
        /// </summary>
        /// <returns>A randomised Color value.</returns>
        public Color Sample()
        {
            Color color;
			color.r = RandomHelper.RandomNumber(this.Value.r - this.Variation.r, this.Value.r + this.Variation.r);
			color.g = RandomHelper.RandomNumber(this.Value.g - this.Variation.g, this.Value.g + this.Variation.g);
			color.b = RandomHelper.RandomNumber(this.Value.b - this.Variation.b, this.Value.b + this.Variation.b);
			color.a = RandomHelper.RandomNumber(this.Value.a - this.Variation.a, this.Value.a + this.Variation.a);
			return color;
        }

        /// <summary>
        /// Implicit cast operator from Color to VariableColor.
        /// </summary>
        static public implicit operator VariableColor(Color value)
        {
            return new VariableColor
            {
                Value     = value,
                Variation = new Color(0.0f, 0.0f, 0.0f, 0.0f)
            };
        }
	}
}
