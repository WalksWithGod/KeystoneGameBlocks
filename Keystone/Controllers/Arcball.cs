//using System;
//using System.Drawing;
//using Keystone.Elements;
//using Keystone.Entities;
//using Microsoft.DirectX;
//using System.Diagnostics;
//using MTV3D65;

//namespace Keystone.Controllers
//{
//    /// <summary>
//    /// Class holds arcball data
//    /// </summary>
//    public class Arcball : StaticEntity
//    {
//        protected Types.Matrix rotation; // Matrix for arc ball's orientation
//        protected TV_3DMATRIX translation; // Matrix for arc ball's position
//        protected TV_3DMATRIX translationDelta; // Matrix for arc ball's position

//        protected int width; // arc ball's viewport width
//        protected int height; // arc ball's viewport height
//        protected Vector2 center; // center of arc ball 
//        protected float radius; // arc ball's radius in screen coords
//        protected float radiusTranslation; // arc ball's radius for translating the target


//        protected TV_3DQUATERNION downQuat; // Quaternion before button down
//        protected TV_3DQUATERNION nowQuat; // Composite quaternion for current drag
//        protected bool isDragging; // Whether user is dragging arc ball

//        protected Point lastMousePosition; // position of last mouse point
//        protected Vector3 downPt; // starting point of rotation arc
//        protected Vector3 currentPt; // current point of rotation arc


//        protected EntityBase _target;

//        /// <summary>
//        /// Create new instance of the arcball class. NOTE: We use a seperate arcball for each viewport 
//        /// we could change this but we'd have to share the Arcball entity with the EditController because right now
//        /// we have seperate EditControllers for each viewport and the EditControllers spawn their own arcballs.
//        /// </summary>
//        public Arcball(string id, ModelBase model, int viewportWidth, int viewportHeight)
//            : base(id, model)
//        {
//            Reset();
//            downPt = Vector3.Empty;
//            currentPt = Vector3.Empty;
//            SetWindow(viewportWidth, viewportHeight);
//        }

//        #region Simple Properties

//        /// <summary>Gets the rotation matrix</summary>
//        public override Types.Matrix RotationMatrix
//        {
//            get
//            {
//                //return rotation = Matrix.RotationQuaternion(nowQuat); 
//   // TODO: Must fix!             Core._CoreClient.Maths.TVMatrixRotationQuaternion(ref rotation, nowQuat);
//                return rotation;
//            }
//        }

//        ///// <summary>Gets the translation matrix</summary>
//        //public Matrix TranslationMatrix
//        //{
//        //    get { return translation; }
//        //}

//        ///// <summary>Gets the translation delta matrix</summary>
//        //public Matrix TranslationDeltaMatrix
//        //{
//        //    get { return translationDelta; }
//        //}

//        /// <summary>Gets the dragging state</summary>
//        public bool IsBeingDragged
//        {
//            get { return isDragging; }
//        }

//        /// <summary>Gets or sets the current quaternion</summary>
//        public TV_3DQUATERNION CurrentQuaternion
//        {
//            get { return nowQuat; }
//            set { nowQuat = value; }
//        }

//        #endregion

//        public void BeginRotation(int mouseX, int mouseY, EntityBase selected)
//        {
//            _target = selected;

//            Reset();
//            TV_3DQUATERNION rotation = new TV_3DQUATERNION();
//    // TODO: must fix!        Core._CoreClient.Maths.TVQuaternionRotationMatrix(ref rotation, _target.RotationMatrix);
//            CurrentQuaternion = rotation;
//            // TODO: as we are moving the mouse over an object (before we've actually picked it)
//            // we shoudl highlight the nearest constrained axis
//            OnBegin(mouseX, mouseY);
//        }

//        public void Rotate(int mouseX, int mouseY)
//        {
//            if (_target != null)
//            {
//                // ROTATION
//                MouseMove(mouseX, mouseY, true, false);
//                _target.RotationMatrix = RotationMatrix;
//                    // this conversion when we pass the RotationMatrix to our selected Entity is causing problems
//                TV_3DVECTOR vec = new TV_3DVECTOR();
//                //Core._CoreClient.Maths.TVEulerAnglesFromMatrix(ref vec, RotationMatrix);
//                //Trace.WriteLine(string.Format("angles x={0}, y={1}, z={2}", vec.x, vec.y, vec.z));
//                //_target.Rotation = new Vector3d (vec);
//                // TRANSLATION
//                //MouseMove( mouseX, mouseY, false, true);
//                //Microsoft.DirectX.Matrix tmp = Microsoft.DirectX.Matrix.Identity;
//                //tmp.M41 = _target.Translation.x;
//                //tmp.M42 = _target.Translation.y;
//                //tmp.M43 = _target.Translation.z;

//                //tmp *= TranslationDeltaMatrix;
//                //_target.Translation = new Vector3d(tmp.M41, tmp.M42, tmp.M43);
//            }
//        }

//        public void EndRotation()
//        {
//            OnEnd();
//        }

//        /// <summary>
//        /// Resets the arcball
//        /// </summary>
//        public void Reset()
//        {
//            CoreClient._CoreClient.Maths.TVQuaternionIdentity(ref downQuat);
//            CoreClient._CoreClient.Maths.TVQuaternionIdentity(ref nowQuat);
//             rotation = Types.Matrix.Identity();
//             CoreClient._CoreClient.Maths.TVMatrixIdentity(ref translation);
//             CoreClient._CoreClient.Maths.TVMatrixIdentity(ref translationDelta);
//            isDragging = false;
//            radius = 1.0f;
//            radiusTranslation = 1.0f;
//        }

//        /// <summary>
//        /// Set window paramters
//        /// </summary>
//        public void SetWindow(int w, int h, float r)
//        {
//            width = w;
//            height = h;
//            radius = r;
//            center = new Vector2(w/2.0f, h/2.0f);
//        }

//        public void SetWindow(int w, int h)
//        {
//            SetWindow(w, h, 0.9f); // default radius
//        }

//        /// <summary>
//        /// Convert a screen point to a vector
//        /// </summary>
//        private Vector3 ScreenToVector(float screenPointX, float screenPointY)
//        {
//            float x = -(screenPointX - width/2.0f)/(radius*width/2.0f);
//            float y = (screenPointY - height/2.0f)/(radius*height/2.0f);


//            float z = 0.0f;
//            float mag = (x*x) + (y*y);

//            if (mag > 1.0f)
//            {
//                float scale = 1.0f/(float) Math.Sqrt(mag);
//                x *= scale;
//                y *= scale;
//            }
//            else
//                z = (float) Math.Sqrt(1.0f - mag);

//            return new Vector3(x, y, z);
//        }

//        /// <summary>
//        /// Computes a quaternion from ball points
//        /// </summary>
//        private static TV_3DQUATERNION QuaternionFromBallPoints(Vector3 from, Vector3 to)
//        {
//            float dot = Vector3.Dot(from, to);
//            Vector3 part = Vector3.Cross(from, to);
//            return new TV_3DQUATERNION(part.X, part.Y, part.Z, dot);
//        }

//        private int constrainedAxis;

//        /// <summary>
//        /// Begin the arcball 'dragging'
//        /// </summary>
//        public void OnBegin(int x, int y)
//        {
//            isDragging = true;
//            downQuat = nowQuat;
//            // TODO: the coordinates passed here must be relative coordinates of the viewport and not window client coords or absolute windows coords
//            downPt = ScreenToVector((float) x, (float) y);
//            constrainedAxis = NearestConstraintAxis(downPt, 3);
//            // TODO: as we are moving the mouse over an object (before we've actually picked it)
//            // we shoudl highlight the nearest constrained axis
//            downPt = ConstrainToAxis(downPt, GetVector(constrainedAxis));
//            lastMousePosition = new Point(x, y);
//        }

//        /// <summary>
//        /// The arcball is 'moving'
//        /// </summary>
//        private void OnMove(int x, int y)
//        {
//            if (isDragging)
//            {
//                currentPt = ScreenToVector((float) x, (float) y);
//                currentPt = ConstrainToAxis(currentPt, GetVector(constrainedAxis));
//                nowQuat = QuaternionFromBallPoints(downPt, currentPt)*downQuat;
//            }
//        }

//        /// <summary>
//        /// Done dragging the arcball
//        /// </summary>
//        public void OnEnd()
//        {
//            isDragging = false;
//        }


//        public void MouseMove(int mouseX, int mouseY, bool rotating, bool XYTranslation)
//        {
//            const float RATE = 1;

//            // arc ball rotation
//            if (rotating)
//            {
//                OnMove(mouseX, mouseY);
//            }
            
//                //translation
//            else
//            {
//                // Normalize based on size of window and bounding sphere radius
//                float deltaX = (lastMousePosition.X - mouseX)*radiusTranslation/width;
//                float deltaY = (lastMousePosition.Y - mouseY)*radiusTranslation/height;

//                if (XYTranslation)
//                {
//                    CoreClient._CoreClient.Maths.TVMatrixTranslation(ref translationDelta, -RATE * deltaX, RATE * deltaY, 0.0f);
//                    translation *= translationDelta;
//                }
//                else // Middle button
//                {
//                    CoreClient._CoreClient.Maths.TVMatrixTranslation(ref translationDelta, 0.0f, 0.0f, RATE * deltaY);
//                    translation *= translationDelta;
//                }
//                // Store off the position of the cursor
//                lastMousePosition = new Point(mouseX, mouseY);
//            }
//        }


//        //----------------------------------------------------------------
//        // Force sphere point to be perpendicular to axis.
//        private Vector3 ConstrainToAxis(Vector3 loose, Vector3 Axis)
//        {
//            Vector3 onPlane;
//            double norm;

//            onPlane = loose - Vector3.Scale(Axis, Vector3.Dot(Axis, loose));

//            norm = onPlane.X*onPlane.X + onPlane.Y*onPlane.Y + onPlane.Z*onPlane.Z;
//            if (norm > 0)
//            {
//                if (onPlane.Z < 0) onPlane = -onPlane;
//                return Vector3.Scale(onPlane, (float) (1/Math.Sqrt(norm)));
//            }
//            // else drop through
//            if (Axis.Z == 1)
//                onPlane = new Vector3(1, 0, 0);
//            else
//                onPlane = Vector3.Normalize(new Vector3(-Axis.Y, Axis.X, 0));

//            return onPlane;
//        }

//        //----------------------------------------------------------------
//        // Find the index of nearest arc of axis set.
//        private int NearestConstraintAxis(Vector3 loose, int nAxes) // axes As HVect, nAxes&)
//        {
//            Vector3 onPlane;
//            double max, dot;
//            int nearest;

//            max = -1f;
//            nearest = 0;

//            for (int i = 0; i < nAxes; i++)
//            {
//                // wow this is actually simpler than i thought.  He loops through the planes of each
//                // axis and finds which axis plane is the closest to the picked spot.
//                onPlane = ConstrainToAxis(loose, GetVector(i));
//                dot = Vector3.Dot(onPlane, loose);
//                //Trace.WriteLine("Dot for index " + i + " is " + dot);
//                if (dot > max)
//                {
//                    max = dot;
//                    nearest = i;
//                }
//            }
//            //Debug.Print("Nearest = " + nearest);
//            return nearest;
//        }

//        private Vector3 GetVector(int index)
//        {
//            Trace.Assert(index >= 0 && index <= 2);

//            Vector3 v = new Vector3();

//            if (index == 0)
//                v = new Vector3(1, 0, 0); // contrained to X axis rotations
//            if (index == 1)
//                v = new Vector3(0, 1, 0); // constrained to Y axis rotations
//            if (index == 2)
//                v = new Vector3(0, 0, 1); // constrained to Z axis rotations

//            return v;
//        }
//    }
//}

//// the UNION stuff is interesting
////'-------------------------------------------------------------------------
////'User-Defined Types used for ArcBall:
////'-------------------------------------------------------------------------

////   '----------------------------------------------------------------------
////   'Quaternion/Homogeneous Vector:
////   '----------------------------------------------------------------------
////        TYPE tQUAT
////            x    AS GLfloat
////            y    AS GLfloat
////            z    AS GLfloat
////            w    AS GLfloat
////        END TYPE
////        UNION QUAT
////            tQuat
////            v(3) AS GLfloat
////        END UNION
////        MACRO HVect      = QUAT
////        MACRO QUATERNION = QUAT
////        MACRO Vector4f   = QUAT
////        GLOBAL qIDN AS quaternion
////   '----------------------------------------------------------------------