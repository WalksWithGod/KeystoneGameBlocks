using System;
using Keystone.Utilities;

namespace Keystone.Types
{
	/// <summary>
	/// Description of VariableFloat.
	/// </summary>
	public struct VariableFloat
	{
        /// <summary>
        /// The base value for the VariableFloat.
        /// </summary>
        public float Value;

        /// <summary>
        /// The range of the random variation around the base value.
        /// </summary>
        public float Variation;

        /// <summary>
        /// Samples the VariableFloat.
        /// </summary>
        /// <returns>A randomised float value.</returns>
        public float Sample()
        {
            if (Math.Abs(this.Variation) <= float.Epsilon)
                return this.Value;

            return RandomHelper.RandomNumber(this.Value - this.Variation, this.Value + this.Variation);
        }

        /// <summary>
        /// Implicit cast operator from float to VariableFloat.
        /// </summary>
        static public implicit operator VariableFloat(float value)
        {
            return new VariableFloat
            {
                Value     = value,
                Variation = 0f
            };
        }
	}
}
