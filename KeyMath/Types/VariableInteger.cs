using System;
using Keystone.Utilities;

namespace Keystone.Types
{
	/// <summary>
	/// Description of VariableInteger.
	/// </summary>
	public struct VariableInteger
	{
        /// <summary>
        /// The base value for the VariableInteger.
        /// </summary>
        public int Value;

        /// <summary>
        /// The range of the random variation around the base value.
        /// </summary>
        public int Variation;

        /// <summary>
        /// Samples the VariableInteger.
        /// </summary>
        /// <returns>A randomised integer value.</returns>
        public int Sample()
        {
            if (Math.Abs(this.Variation) <= float.Epsilon)
                return this.Value;

            return RandomHelper.RandomNumber(this.Value - this.Variation, this.Value + this.Variation);
        }

        /// <summary>
        /// Implicit cast operator from float to VariableFloat.
        /// </summary>
        static public implicit operator VariableInteger(int value)
        {
            return new VariableInteger
            {
                Value     = value,
                Variation = 0
            };
        }
	}
}
