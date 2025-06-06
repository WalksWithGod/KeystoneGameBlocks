using Core.Types;

namespace Physics.Constraints
{
	public class Jacobian
	{
        // Instance Fields
        internal Vector3d vecLinearA;
        internal Vector3d vecAngularA;
        internal Vector3d vecLinearB;
        internal Vector3d vecAngularB;
        internal Matrix matLinearA;
        internal Matrix matAngularA;
        internal Matrix matLinearB;
        internal Matrix matAngularB;

		// Constructors
		internal Jacobian (Matrix linA, Matrix angA, Matrix linB, Matrix angB)
		{
			this.matLinearA = linA;
			this.matAngularA = angA;
			this.matLinearB = linB;
			this.matAngularB = angB;
		}
		
		internal Jacobian (Vector3d linA, Matrix angA, Vector3d linB, Matrix angB)
		{
			this.vecLinearA = linA;
			this.matAngularA = angA;
			this.vecLinearB = linB;
			this.matAngularB = angB;
		}
		
		internal Jacobian (Vector3d linA, Vector3d angA, Vector3d linB, Vector3d angB)
		{
			this.vecLinearA = linA;
			this.vecAngularA = angA;
			this.vecLinearB = linB;
			this.vecAngularB = angB;
		}
		
		internal Jacobian ()
		{
		}
	}
}
