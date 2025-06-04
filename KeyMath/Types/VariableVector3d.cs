using System;
using Keystone.Utilities;

namespace Keystone.Types
{
	/// <summary>
	/// Description of VariableVector3d.
	/// </summary>
	public struct VariableVector3d
	{
        /// <summary>
        /// The base value for the VariableVector3d.
        /// </summary>
        public Vector3d Value;

        /// <summary>
        /// The range of the random variation around the base value.
        /// </summary>
        public double Variation;

        /// <summary>
        /// Samples the VariableVector3d.
        /// </summary>
        /// <returns>A randomised Vector3d value.</returns>
        public Vector3d Sample()
        {
            if (Math.Abs(this.Variation) <= double.Epsilon)
                return this.Value;

            Vector3d result;
			result.x = RandomHelper.RandomNumber(this.Value.x - this.Variation, this.Value.x + this.Variation);
			result.y = RandomHelper.RandomNumber(this.Value.y - this.Variation, this.Value.y + this.Variation);
			result.z = RandomHelper.RandomNumber(this.Value.z - this.Variation, this.Value.z + this.Variation);
			return result;
        }

        /// <summary>
        /// Implicit cast operator from Vector3d to VariableVector3d.
        /// </summary>
        static public implicit operator VariableVector3d(Vector3d value)
        {
            return new VariableVector3d
            {
                Value     = value,
                Variation = 0d
            };
        }
	}
}
