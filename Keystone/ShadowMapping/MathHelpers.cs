using System;
using MTV3D65;


namespace Keystone.PSSM
{
    public class MathHelpers
    {
        private static float fPIdiv180 = (float)Math.PI / 180.0f;

#region General Stuff
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fDegree"></param>
        /// <returns></returns>
        public static float DegreeToRadian(float fDegree)
        {
            return fPIdiv180 * fDegree;
        }

        /// <summary>
        /// Slide off the impacting surface - Aus Quake3 Source Code :)
        /// </summary>
        /// <param name="vVelocity"></param>
        /// <param name="vNormal"></param>
        /// <param name="fOverbounce"></param>
        public static void ClipVelocity(TV_3DVECTOR vNormal, ref TV_3DVECTOR vVelocity)
        {
            float fBackoff = MathHelpers.Vec3DotProduct(vVelocity, vNormal);
            vVelocity = vVelocity - vNormal * (fBackoff * 1.001f);
        }

        /// <summary>
        /// Quake2-Style Acceleration
        /// </summary>
        /// <param name="vWishDirection">Die Richtung, in die gelaufen werden soll</param>
        /// <param name="fAcceleration">Die Beschleunigung für diesen Frame</param>
        public static void AddAcceleration(TV_3DVECTOR vWishVelocity, ref TV_3DVECTOR vVelocity)
        {
            float fWishSpeed = MathHelpers.Vec3Length(vWishVelocity);

            // Y-Achse nicht mit einbeziehen, da sonst der Sprung mit beeinflusst wird.
            // Dies hätte zur Folge, dass man beim Beschleunigen nicht mehr so hoch springen könnte
            TV_3DVECTOR vCurrentVelocity = new TV_3DVECTOR(vVelocity.x, 0.0f, vVelocity.z);
            TV_3DVECTOR vPushDirection = MathHelpers.Vec3Normalize(vWishVelocity - vCurrentVelocity);
            float fPushLength = MathHelpers.Vec3Length(vPushDirection);

            if (fPushLength > 0.0f)
            {
                if (fWishSpeed > fPushLength)
                    fWishSpeed = fPushLength;

                vVelocity = MathHelpers.Vec3MultiplyAdd(vVelocity, vPushDirection, fWishSpeed);
            }
        }

        /// <summary>
        /// Returns the cotangent of the given value.
        /// </summary>
        /// <param name="x">The argument.</param>
        /// <returns>The cotangent.</returns>
        public static float Cot(float x)
        {
            return 1.0f / (float)Math.Tan(x);
        }
        
        /// <summary>
        /// "Normalisiert" einen Winkel zu [0, 359]
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float AngleNormalize360(float x)
        {
            if (x < 0.0f)
                x = (360.0f - x * -1.0f) % 360.0f;
            else 
                x = x % 360.0f;

            return x;
        }
        
        /// <summary>
        /// Clamp'ed einen Wert zwischen Min und Max
        /// </summary>
        /// <param name="x"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>true if clamped</returns>
        public static bool Clamp(ref float x, float min, float max)
        {
            if (x < min)
            {
                x = min;
                return true;
            }
            else if (x > max)
            {
                x = max;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool Clamp(ref int x, int min, int max)
        {
            if (x < min)
            {
                x = min;
                return true;
            }
            else if (x > max)
            {
                x = max;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Distance1D(float a, float b)
        {
            return (a > b) ? a - b : b - a;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Distance2D(TV_3DVECTOR a, TV_3DVECTOR b)
        {
            return (float)Math.Sqrt(Math.Pow(a.x - b.x, 2) + Math.Pow(a.z - b.z, 2));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="fDamping"></param>
        public static void DampToZero(ref float x, float fDamping)
        {
            if (x != 0.0f)
            {
                if (x > 0.0f)
                {
                    x -= fDamping;

                    if (x < 0.0f)
                        x = 0.0f;
                }
                else
                {
                    x += fDamping;

                    if (x > 0.0f)
                        x = 0.0f;
                }
            }
        }
#endregion

#region Vector Stuff
        /// <summary>
        /// Transformiert einen Vektor anhand einer Matrix
        /// </summary>
        /// <param name="m">Matrix</param>
        /// <param name="v">Vektor</param>
        /// <returns>Transformierter Vektor</returns>
        public static TV_3DVECTOR Vec3TransformCoord(TV_3DMATRIX m, TV_3DVECTOR v)
        {
            TV_3DVECTOR vNewVector = new TV_3DVECTOR();

            vNewVector.x = m.m11 * v.x + m.m21 * v.y + m.m31 * v.z + m.m41;
            vNewVector.y = m.m12 * v.x + m.m22 * v.y + m.m32 * v.z + m.m42;
            vNewVector.z = m.m13 * v.x + m.m23 * v.y + m.m33 * v.z + m.m43;

            return vNewVector;
        }

        /// <summary>
        /// Gibt das Kreuzprodukt zweier Vektoren zurück
        /// </summary>
        /// <param name="v1">Vektor 1</param>
        /// <param name="v2">Vektor 2</param>
        /// <returns>Kreuzprodukt</returns>
        public static TV_3DVECTOR Vec3CrossProduct(TV_3DVECTOR v1, TV_3DVECTOR v2)
        {
            TV_3DVECTOR vNewVector = new TV_3DVECTOR();

            vNewVector.x = v1.y * v2.z - v1.z * v2.y;
            vNewVector.y = v1.z * v2.x - v1.x * v2.z;
            vNewVector.z = v1.x * v2.y - v1.y * v2.x;

            return vNewVector;
        }

        /// <summary>
        /// Gibt das Punktprodukt zweier Vektoren zurück
        /// </summary>
        /// <param name="v1">Vektor 1</param>
        /// <param name="v2">Vektor 2</param>
        /// <returns>Punktprodukt</returns>
        public static float Vec3DotProduct(TV_3DVECTOR v1, TV_3DVECTOR v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }

        /// <summary>
        /// Gibt die Länge eines Vektors zurück
        /// </summary>
        /// <param name="v">Vektor</param>
        /// <returns>Länge</returns>
        public static float Vec3Length(TV_3DVECTOR v)
        {
            return (float)Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
        }

        /// <summary>
        /// Normalisiert einen Vektor
        /// </summary>
        /// <param name="v">Der Vektor, der Normalisiert werden soll</param>
        /// <returns>Der Normalisierte Vektor</returns>
        public static TV_3DVECTOR Vec3Normalize(TV_3DVECTOR v)
        {
            TV_3DVECTOR vNewVector = new TV_3DVECTOR();
            float fLength = Vec3Length(v);

            if (fLength > 0.0f)
            {
                vNewVector.x = v.x / fLength;
                vNewVector.y = v.y / fLength;
                vNewVector.z = v.z / fLength;
            }

            return vNewVector;
        }

        /// <summary>
        /// out = v + b * s
        /// </summary>
        /// <param name="v"></param>
        /// <param name="b"></param>
        /// <param name="s"></param>
        public static TV_3DVECTOR Vec3MultiplyAdd(TV_3DVECTOR v, TV_3DVECTOR b, float s)
        {
            TV_3DVECTOR vOut = new TV_3DVECTOR();

            vOut.x = v.x + b.x * s;
            vOut.y = v.y + b.y * s;
            vOut.z = v.z + b.z * s;

            return vOut;
        }

        /// <summary>
        /// "Bereinigt" einen normalisierten Vektor um numerische Ungenauigkeiten
        /// </summary>
        /// <param name="vVector"></param>
        public static void CleanNormalizedVector(ref TV_3DVECTOR vVector)
        {
            float[] faComponents = new float[3];

            faComponents[0] = vVector.x;
            faComponents[1] = vVector.y;
            faComponents[2] = vVector.z;

            for (int i = 0; i < 3; i++)
            {
                if (faComponents[i] < 0.0f)
                {
                    if (faComponents[i] < -0.999f)
                        faComponents[i] = -1.0f;
                    else if (faComponents[i] > -0.001f)
                        faComponents[i] = 0.0f;
                }
                else
                {
                    if (faComponents[i] > 0.999f)
                        faComponents[i] = 1.0f;
                    else if (faComponents[i] < 0.001f)
                        faComponents[i] = 0.0f;
                }
            }

            vVector.x = faComponents[0];
            vVector.y = faComponents[1];
            vVector.z = faComponents[2];
        }

#endregion

#region Matrix Stuff
        /// <summary>
        /// Builds a left-handed orthographic projection matrix.
        /// http://msdn2.microsoft.com/en-us/library/bb205346(VS.85).aspx
        /// </summary>
        /// <param name="w">Width of the view volume.</param>
        /// <param name="h">Height of the view volume.</param>
        /// <param name="zn">Minimum z-value of the view volume which is referred to as z-near.</param>
        /// <param name="zf">Maximum z-value of the view volume which is referred to as z-far. </param>
        /// <returns></returns>
        public static TV_3DMATRIX MatrixOrthoLH(float w, float h, float zn, float zf)
        {
            return new TV_3DMATRIX(2.0f / w,  0.0f,      0.0f,              0.0f,
                                   0.0f,      2.0f / h,  0.0f,              0.0f,
                                   0.0f,      0.0f,      1.0f / (zf - zn),  0.0f,
                                   0.0f,      0.0f,      -zn / (zf - zn),   1.0f);
        }

        /// <summary>
        /// Builds a left-handed, look-at matrix.
        /// http://msdn2.microsoft.com/en-us/library/bb205342.aspx
        /// </summary>
        /// <param name="vEye">defines the eye point. This value is used in translation.</param>
        /// <param name="vAt">defines the camera look-at target</param>
        /// <param name="vUp">structure that defines the current world's up, usually [0, 1, 0].</param>
        /// <returns></returns>
        public static TV_3DMATRIX MatrixLookAtLH(TV_3DVECTOR vEye, TV_3DVECTOR vAt, TV_3DVECTOR vUp)
        {
            TV_3DVECTOR z = Vec3Normalize(vAt - vEye);
            TV_3DVECTOR x = Vec3Normalize(Vec3CrossProduct(vUp, z));
            TV_3DVECTOR y = Vec3CrossProduct(z, x);

            return new TV_3DMATRIX(x.x,  y.x,  z.x,  0.0f,
                                   x.y,  y.y,  z.y,  0.0f,
                                   x.z,  y.z,  z.z,  0.0f,
                                   -Vec3DotProduct(x, vEye), 
                                   -Vec3DotProduct(y, vEye), 
                                   -Vec3DotProduct(z, vEye), 1.0f);
        }

        /*
        [20:18]	SylvainTV: 	float fAspect, fW, fH;
        [20:18]	SylvainTV: 	if(cam->fAspectRatio > 0.0f)
        [20:18]	SylvainTV: 	{
        [20:18]	SylvainTV: 		fAspect = cam->fAspectRatio;
        [20:18]	SylvainTV: 		fW = fZoom * fAspect;
        [20:18]	SylvainTV: 		fH = fZoom;
        [20:18]	SylvainTV: 	}
        [20:18]	SylvainTV: 	else
        [20:18]	SylvainTV: 	{
        [20:18]	SylvainTV: 		fAspect = 1;
        [20:18]	SylvainTV: 		fW = fZoom ;
        [20:18]	SylvainTV: 		fH = fZoom * (float)cam->viewportheight / (float)cam->viewportwidth ;
        [20:18]	SylvainTV: 	}
        [20:18]	SylvainTV: 	D3DXMatrixOrthoLH(&cam->mProjMatrix, fW, fH, fNearPlane,  fFarPlane);
        [20:18]	SylvainTV: 	cam->mOldProjection = cam->mProjMatrix;
        [20:18]	SylvainTV: 	if(cam->inuse)
        [20:18]	SylvainTV: 	{
        [20:18]	SylvainTV: 		CameraManager_ApplyCamera(iCameraIndex);
        [20:18]	SylvainTV: 	}
        */
        /// <summary>
        /// Builds a left-handed perspective projection matrix based on a field of view.
        /// http://msdn2.microsoft.com/en-us/library/bb205350(VS.85).aspx
        /// </summary>
        /// <param name="fovY">Field of view in the y direction, in radians.</param>
        /// <param name="aspect">Aspect ratio, defined as view space width divided by height.</param>
        /// <param name="zn">Z-value of the near view-plane.</param>
        /// <param name="zf">Z-value of the far view-plane.</param>
        /// <returns></returns>
        public static TV_3DMATRIX MatrixPerspectiveFovLH(float fovY, float aspect, float zn, float zf)
        {
            float yScale = (float)Cot(fovY * 0.5f);
            float xScale = yScale / aspect;

            return new TV_3DMATRIX(xScale,   0.0f,        0.0f,            0.0f,
                                   0.0f,   yScale,        0.0f,            0.0f,
                                   0.0f,     0.0f,  zf/(zf-zn),            1.0f,
                                   0.0f,     0.0f,        -zn*zf/(zf-zn),  0.0f);
        }

        /// <summary>
        /// Returns a standard matrix identitiy.
        /// </summary>
        /// <returns></returns>
        public static TV_3DMATRIX MatrixIdentity()
        {
            return new TV_3DMATRIX(1.0f, 0.0f, 0.0f, 0.0f,
                                   0.0f, 1.0f, 0.0f, 0.0f,
                                   0.0f, 0.0f, 1.0f, 0.0f,
                                   0.0f, 0.0f, 0.0f, 1.0f);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mMatrix"></param>
        /// <returns></returns>
        public static TV_3DMATRIX MatrixInverse(TV_3DMATRIX mMatrix)
        {
            float a = 0.0f;
            CoreClient._CoreClient.Maths.TVMatrixInverse(ref mMatrix, ref a, mMatrix);

            return mMatrix;
        }


        /// <summary>
        /// Kombiniert zwei Matrixen via Multiplikation
        /// </summary>
        /// <param name="m1">Matrix 1</param>
        /// <param name="m2">Matrix 2</param>
        /// <returns>Kombinierte Matrix</returns>
        public static TV_3DMATRIX MatrixMultiply(TV_3DMATRIX m1, TV_3DMATRIX m2)
        {
            TV_3DMATRIX mMatrix = new TV_3DMATRIX();

            mMatrix.m11 = m1.m11 * m2.m11 + m1.m12 * m2.m21 + m1.m13 * m2.m31 + m1.m14 * m2.m41;
            mMatrix.m12 = m1.m11 * m2.m12 + m1.m12 * m2.m22 + m1.m13 * m2.m32 + m1.m14 * m2.m42;
            mMatrix.m13 = m1.m11 * m2.m13 + m1.m12 * m2.m23 + m1.m13 * m2.m33 + m1.m14 * m2.m43;
            mMatrix.m14 = m1.m11 * m2.m14 + m1.m12 * m2.m24 + m1.m13 * m2.m34 + m1.m14 * m2.m44;

            mMatrix.m21 = m1.m21 * m2.m11 + m1.m22 * m2.m21 + m1.m23 * m2.m31 + m1.m24 * m2.m41;
            mMatrix.m22 = m1.m21 * m2.m12 + m1.m22 * m2.m22 + m1.m23 * m2.m32 + m1.m24 * m2.m42;
            mMatrix.m23 = m1.m21 * m2.m13 + m1.m22 * m2.m23 + m1.m23 * m2.m33 + m1.m24 * m2.m43;
            mMatrix.m24 = m1.m21 * m2.m14 + m1.m22 * m2.m24 + m1.m23 * m2.m34 + m1.m24 * m2.m44;

            mMatrix.m31 = m1.m31 * m2.m11 + m1.m32 * m2.m21 + m1.m33 * m2.m31 + m1.m34 * m2.m41;
            mMatrix.m32 = m1.m31 * m2.m12 + m1.m32 * m2.m22 + m1.m33 * m2.m32 + m1.m34 * m2.m42;
            mMatrix.m33 = m1.m31 * m2.m13 + m1.m32 * m2.m23 + m1.m33 * m2.m33 + m1.m34 * m2.m43;
            mMatrix.m34 = m1.m31 * m2.m14 + m1.m32 * m2.m24 + m1.m33 * m2.m34 + m1.m34 * m2.m44;

            mMatrix.m41 = m1.m41 * m2.m11 + m1.m42 * m2.m21 + m1.m43 * m2.m31 + m1.m44 * m2.m41;
            mMatrix.m42 = m1.m41 * m2.m12 + m1.m42 * m2.m22 + m1.m43 * m2.m32 + m1.m44 * m2.m42;
            mMatrix.m43 = m1.m41 * m2.m13 + m1.m42 * m2.m23 + m1.m43 * m2.m33 + m1.m44 * m2.m43;
            mMatrix.m44 = m1.m41 * m2.m14 + m1.m42 * m2.m24 + m1.m43 * m2.m34 + m1.m44 * m2.m44;

            return mMatrix;
        }

        /// <summary>
        /// Applies translation to a matrix.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        /// <returns></returns>
        public static TV_3DMATRIX MatrixTranslation(float x, float y, float z)
        {
            TV_3DMATRIX mMatrix = MatrixIdentity();

            mMatrix.m31 += x;
            mMatrix.m32 += y;
            mMatrix.m33 += z;

            return mMatrix;
        }
#endregion
    }
}
