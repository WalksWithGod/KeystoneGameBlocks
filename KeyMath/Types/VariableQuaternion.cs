using System;

namespace Keystone.Types
{
	/// <summary>
	/// Description of VariableQuaternion.
	/// </summary>
	public struct VariableQuaternion
	{
        /// <summary>
        /// The base value for the VariableQuaternion.
        /// </summary>
        public Quaternion Value;

        /// <summary>
        /// The range of the random variation around the base value.
        /// </summary>
        public double Variation;

        /// <summary>
        /// Samples the VariableQuaternion.
        /// </summary>
        /// <returns>A randomised Quaternion value.</returns>
        public VariableQuaternion Sample()
        {
            if (Math.Abs(this.Variation) <= double.Epsilon)
                return this.Value;

            //double x = RandomHelper.RandomNumber(this.Value.X - this.Variation, this.Value.X + this.Variation);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Implicit cast operator from float to VariableQuaternion.
        /// </summary>
        static public implicit operator VariableQuaternion(Quaternion value)
        {
            return new VariableQuaternion
            {
                Value     = value,
                Variation = 0d
            };
        }
	}
}
