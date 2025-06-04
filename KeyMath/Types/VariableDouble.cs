using System;
using Keystone.Utilities;

namespace Keystone.Types
{
	/// <summary>
	/// Description of VariableDouble.
	/// </summary>
	public struct VariableDouble
	{
		/// <summary>
        /// The base value for the VariableDouble.
        /// </summary>
        public double Value;

        /// <summary>
        /// The range of the random variation around the base value.
        /// </summary>
        public double Variation;

        /// <summary>
        /// Samples the VariableDouble.
        /// </summary>
        /// <returns>A randomised double value.</returns>
        public double Sample()
        {
            if (Math.Abs(this.Variation) <= float.Epsilon)
                return this.Value;

            return RandomHelper.RandomNumber(this.Value - this.Variation, this.Value + this.Variation);
        }

        /// <summary>
        /// Implicit cast operator from double to VariableDouble.
        /// </summary>
        static public implicit operator VariableDouble(double value)
        {
            return new VariableDouble
            {
                Value     = value,
                Variation = 0d
            };
        }
	}
}
