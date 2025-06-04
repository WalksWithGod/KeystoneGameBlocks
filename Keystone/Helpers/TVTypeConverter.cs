using System;
using Keystone.Types;
using MTV3D65;
using Microsoft.DirectX;

namespace Keystone.Helpers
{
    public class TVTypeConverter
    {

        public static Microsoft.DirectX.Direct3D.Material FromKeystoneMaterial(Appearance.Material mat)
        {
            Microsoft.DirectX.Direct3D.Material result = new Microsoft.DirectX.Direct3D.Material();
            result.AmbientColor = FromKeystoneColor(mat.Ambient);
            result.DiffuseColor = FromKeystoneColor(mat.Diffuse);
            result.EmissiveColor = FromKeystoneColor(mat.Emissive);
            result.SpecularColor = FromKeystoneColor(mat.Specular);
            result.SpecularSharpness = mat.SpecularPower;

            // opacity is normally set independantly in every material color so really, in our own Keystone.Appearance.Material
            // we should not have a seperate Opacity variable and instead that value should be set directly in the colors for each type


            return result;
        }

        //public static Microsoft.DirectX.Direct3D.Material CreateD3DMaterial()
        //{
        //    Microsoft.DirectX.Direct3D.Material mat;
            
        //}

        public static Microsoft.DirectX.Direct3D.Material FromWavefrontObjMaterial(Loaders.WaveFrontObjMaterial mat)
        {
            Microsoft.DirectX.Direct3D.Material result = new Microsoft.DirectX.Direct3D.Material();

            result.AmbientColor = FromKeystoneColor(FromTVColor(mat.Ambient));
            result.DiffuseColor = FromKeystoneColor(FromTVColor(mat.Diffuse));
            result.EmissiveColor = FromKeystoneColor(FromTVColor(mat.Emissive));
            result.SpecularColor = FromKeystoneColor(FromTVColor(mat.Specular));
            result.SpecularSharpness = mat.SpecularPower;

            // opacity is normally set independantly in every material color so really, in our own Keystone.Appearance.Material
            // we should not have a seperate Opacity variable and instead that value should be set directly in the colors for each type


            return result;
        }

        public static Keystone.KeyFrames.EmitterKeyframe[] FromTVEmitterKeyframes(TV_PARTICLEEMITTER_KEYFRAME[] keyframes)
        {
            if (keyframes == null || keyframes.Length == 0) return null;

            KeyFrames.EmitterKeyframe[] results = new KeyFrames.EmitterKeyframe[keyframes.Length];
            for (int i = 0; i < keyframes.Length; i++)
            {
                results[i].Key = keyframes[i].fKey;
                results[i].MainDirection = FromTVVector3f(keyframes[i].vMainDirection);
                results[i].LocalPosition = FromTVVector3f(keyframes[i].vLocalPosition);
                results[i].BoxSize = FromTVVector3f(keyframes[i].vGeneratorBoxSize);
                results[i].Radius = keyframes[i].fGeneratorSphereRadius;
                results[i].Color = FromTVColor(keyframes[i].vDefaultColor);
                results[i].Lifetime = keyframes[i].fParticleLifeTime;
                results[i].Power = keyframes[i].fPower;
                results[i].Speed = keyframes[i].fSpeed;
            }

            return results;
        }

        public static KeyFrames.ParticleKeyframe[] FromTVParticleKeyFrames(TV_PARTICLE_KEYFRAME[] keyframes)
        {
            if (keyframes == null || keyframes.Length == 0) return null;

            KeyFrames.ParticleKeyframe[] results = new KeyFrames.ParticleKeyframe[keyframes.Length];
            for (int i = 0; i < keyframes.Length; i++)
            {
                results[i].Key = keyframes[i].fKey;
                results[i].Color = FromTVColor(keyframes[i].cColor);
                results[i].Size = FromTVVector3f(keyframes[i].fSize);
                results[i].Rotation = FromTVVector3f(keyframes[i].vRotation);
            }

            return results;
        }

        public static TV_PARTICLEEMITTER_KEYFRAME[] ToTVEmitterKeyFrames(Keystone.KeyFrames.EmitterKeyframe[] keyframes)
        {
            if (keyframes == null || keyframes.Length == 0) return null;

            TV_PARTICLEEMITTER_KEYFRAME[] results = new TV_PARTICLEEMITTER_KEYFRAME[keyframes.Length];
            for (int i = 0; i < keyframes.Length; i++)
            {
                results[i].fKey = keyframes[i].Key;
                results[i].vMainDirection = ToTVVector(keyframes[i].MainDirection);
                results[i].vLocalPosition = ToTVVector(keyframes[i].LocalPosition);
                results[i].vGeneratorBoxSize = ToTVVector(keyframes[i].BoxSize);
                results[i].fGeneratorSphereRadius = keyframes[i].Radius;
                results[i].vDefaultColor = ToTVColor(keyframes[i].Color);
                results[i].fParticleLifeTime = keyframes[i].Lifetime;
                results[i].fPower = keyframes[i].Power;
                results[i].fSpeed = keyframes[i].Speed;
            }

            return results;
        }

        public static TV_PARTICLE_KEYFRAME[] ToTVParticleKeyFrames(Keystone.KeyFrames.ParticleKeyframe[] keyframes)
        {
            if (keyframes == null || keyframes.Length == 0) return null;

            TV_PARTICLE_KEYFRAME[] results = new TV_PARTICLE_KEYFRAME[keyframes.Length];
            for (int i = 0; i < keyframes.Length; i++)
            {
                results[i].fKey = keyframes[i].Key;
                results[i].cColor = ToTVColor(keyframes[i].Color);
                results[i].fSize = ToTVVector(keyframes[i].Size);
                results[i].vRotation = ToTVVector(keyframes[i].Rotation);
            }

            return results;
        }


        public static Microsoft.DirectX.Direct3D.ColorValue FromKeystoneColor(Types.Color color)
        {
            return  new Microsoft.DirectX.Direct3D.ColorValue(color.r, color.g, color.b, color.a);
        }

        public static Vector3d ColorToVector3d (Keystone.Types.Color color)
        {
            Vector3d result;
            result.x = color.r;
            result.y = color.g;
            result.z = color.b;
            return result;
        }
                
        public static Color FromSystemColor(System.Drawing.Color color)
        {
            Color result;
            result.a = color.A;
            result.r = color.R;
            result.g = color.G;
            result.b = color.B;
            return result;
        }

        public static Color FromTVColor (TV_COLOR color)
        {
            Color result;
            result.a = color.a;
            result.r = color.r;
            result.g = color.g;
            result.b = color.b;
            return result;
        }

        public static TV_COLOR ToTVColor (Color color)
        {
            TV_COLOR result;
            result.a = color.a;
            result.r = color.r;
            result.g = color.g;
            result.b = color.b;
            return result;
        }

        public static TV_4DVECTOR ToTVVector4 (Color color)
        {
        	TV_4DVECTOR result;
        	result.x = color.r;
        	result.y = color.g;
        	result.z = color.b;
        	result.w = color.a;
        	return result;
        }


        public static TV_3DVECTOR[] ToTVVector(Vector3d[] vectors)
        {
        	if (vectors == null) throw new ArgumentOutOfRangeException ();
        	
        	TV_3DVECTOR[] result = new TV_3DVECTOR[vectors.Length];
        	
        	for (int i = 0; i < result.Length; i++)
        		result[i] = ToTVVector(vectors[i]);
        	
        	return result;
        }
        
        public static TV_3DVECTOR ToTVVector(Vector3d vector)
        {
            TV_3DVECTOR result;
            result.x = (float)vector.x;
            result.y = (float)vector.y;
            result.z = (float)vector.z;
            return result;
        }

        public static TV_3DVECTOR ToTVVector(Vector3f vector)
        {
            TV_3DVECTOR result;
            result.x = vector.x;
            result.y = vector.y;
            result.z = vector.z;
            return result;
        }

        public static Vector3f FromTVVector3f(TV_3DVECTOR v)
        {
            Vector3f result;
            result.x = v.x;
            result.y = v.y;
            result.z = v.z;
            return result;
        }

        public static Vector3d FromDXVector (Vector3 vector)
        {
            Vector3d result;
            result.x = vector.X;
            result.y = vector.Y;
            result.z = vector.Z;
            return result;
        }
        
        public static Vector3d  FromTVVector(TV_3DVECTOR vector)
        {
            Vector3d result;
            result.x = vector.x;
            result.y = vector.y;
            result.z = vector.z;
            return result;
        }

        public static Vector3d[] FromTVVector(TV_3DVECTOR[] vectors)
        {
            Vector3d[] results = new Vector3d[vectors.Length];
            for (int i = 0; i < results.Length; i++)
                results[i] = FromTVVector(vectors[i]);

           return results;
        }
        
        public static Keystone.Types.Vector4 FromTVVector(TV_4DVECTOR vector)
        {
            Keystone.Types.Vector4 result;
            result.x = vector.x;
            result.y = vector.y;
            result.z = vector.z;
            result.w = vector.w;

            return result;
        }

        public static TV_3DQUATERNION ToTVQuaternion(Types.Quaternion quat)
        {
            TV_3DQUATERNION result;
            result.x = (float) quat.X;
            result.y = (float)quat.Y;
            result.z = (float)quat.Z;
            result.w = (float)quat.W;
            return result;
        }

        public static TV_3DMATRIX CreateTVMatrix()
        {
            TV_3DMATRIX mat;
            mat.m11 = 0;
            mat.m12 = 0;
            mat.m13 = 0;
            mat.m14 = 0;
            mat.m21 = 0;
            mat.m22 = 0;
            mat.m23 = 0;
            mat.m24 = 0;
            mat.m31 = 0;
            mat.m32 = 0;
            mat.m33 = 0;
            mat.m34 = 0;
            mat.m41 = 0;
            mat.m42 = 0;
            mat.m43 = 0;
            mat.m44 = 0;
            return mat;
        }

        public static TV_3DMATRIX CreateTVIdentityMatrix()
        {
            TV_3DMATRIX mat;
            mat.m11 = 1;
            mat.m12 = 0;
            mat.m13 = 0;
            mat.m14 = 0;
            mat.m21 = 0;
            mat.m22 = 1;
            mat.m23 = 0;
            mat.m24 = 0;
            mat.m31 = 0;
            mat.m32 = 0;
            mat.m33 = 1;
            mat.m34 = 0;
            mat.m41 = 0;
            mat.m42 = 0;
            mat.m43 = 0;
            mat.m44 = 1;
            return mat;
        }

        public static Microsoft.DirectX.Matrix ToDirectXMatrix(TV_3DMATRIX mat)
        {
            Microsoft.DirectX.Matrix result;
            result.M11 = mat.m11;
            result.M12 = mat.m12;
            result.M13 = mat.m13;
            result.M14 = mat.m14;
            result.M21 = mat.m21;
            result.M22 = mat.m22;
            result.M23 = mat.m23;
            result.M24 = mat.m24;
            result.M31 = mat.m31;
            result.M32 = mat.m32;
            result.M33 = mat.m33;
            result.M34 = mat.m34;
            result.M41 = mat.m41;
            result.M42 = mat.m42;
            result.M43 = mat.m43;
            result.M44 = mat.m44;
            return result;

        }

        public static Microsoft.DirectX.Matrix ToDirectXMatrix(Types.Matrix mat)
        {
            Microsoft.DirectX.Matrix result;
            result.M11 = (float)mat.M11;
            result.M12 = (float)mat.M12;
            result.M13 = (float)mat.M13;
            result.M14 = (float)mat.M14;
            result.M21 = (float)mat.M21;
            result.M22 = (float)mat.M22;
            result.M23 = (float)mat.M23;
            result.M24 = (float)mat.M24;
            result.M31 = (float)mat.M31;
            result.M32 = (float)mat.M32;
            result.M33 = (float)mat.M33;
            result.M34 = (float)mat.M34;
            result.M41 = (float)mat.M41;
            result.M42 = (float)mat.M42;
            result.M43 = (float)mat.M43;
            result.M44 = (float)mat.M44;
            return result;

        }

        public static TV_3DMATRIX[] ToTVMatrix(Types.Matrix[] mat)
        {
            if (mat == null) return null;

            TV_3DMATRIX[] result = new TV_3DMATRIX[mat.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = ToTVMatrix(mat[i]);

            return result;
        }

        public static TV_3DMATRIX ToTVMatrix(Types.Matrix mat)
        {
            TV_3DMATRIX result;
            result.m11 = (float)mat.M11;
            result.m12 = (float)mat.M12;
            result.m13 = (float)mat.M13;
            result.m14 = (float)mat.M14;
            result.m21 = (float)mat.M21;
            result.m22 = (float)mat.M22;
            result.m23 = (float)mat.M23;
            result.m24 = (float)mat.M24;
            result.m31 = (float)mat.M31;
            result.m32 = (float)mat.M32;
            result.m33 = (float)mat.M33;
            result.m34 = (float)mat.M34;
            result.m41 = (float)mat.M41;
            result.m42 = (float)mat.M42;
            result.m43 = (float)mat.M43;
            result.m44 = (float)mat.M44;
            return result;
        }

        public static TV_3DMATRIX ToTVMatrix(Microsoft.DirectX.Matrix m1)
        {
            TV_3DMATRIX m2;
            m2.m11 = m1.M11;
            m2.m12 = m1.M12;
            m2.m13 = m1.M13;
            m2.m14 = m1.M14;
            m2.m21 = m1.M21;
            m2.m22 = m1.M22;
            m2.m23 = m1.M23;
            m2.m24 = m1.M24;
            m2.m31 = m1.M31;
            m2.m32 = m1.M32;
            m2.m33 = m1.M33;
            m2.m34 = m1.M34;
            m2.m41 = m1.M41;
            m2.m42 = m1.M42;
            m2.m43 = m1.M43;
            m2.m44 = m1.M44; //= 1;
            return m2;
        }

        public static Types.Matrix FromTVMatrix(TV_3DMATRIX matrix)
        {
            return new Types.Matrix(matrix.m11, matrix.m12, matrix.m13, matrix.m14,
                              matrix.m21, matrix.m22, matrix.m23, matrix.m24,
                              matrix.m31, matrix.m32, matrix.m33, matrix.m34,
                              matrix.m41, matrix.m42, matrix.m43, matrix.m44);
        }

        public static Types.Matrix FromDXMatrix(Microsoft.DirectX.Matrix matrix)
        {
            return new Types.Matrix(matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                              matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                              matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                              matrix.M41, matrix.M42, matrix.M43, matrix.M44);
        }

        public static Triangle FromTVMeshIndexedFace(TVMesh mesh, int index1, int index2, int index3)
        {

            int[] _indices = new int[3];
            TV_3DVECTOR[] _points = new TV_3DVECTOR[3];

            float tu, tv, tu2, tv2;
            tu = tv = tv2 = tu2 = 0;
            int color = 0;

            TV_3DVECTOR _normal;
            _normal.x = 0; _normal.y = 0; _normal.z = 0;
            _indices[0] = index1;
            _indices[1] = index2;
            _indices[2] = index3;

            TV_3DVECTOR p;
            p.x = 0; p.y = 0; p.z = 0;

            for (int i = 0; i < 3; i++)
            {
                mesh.GetVertex(_indices[i], ref _points[i].x, ref _points[i].y, ref _points[i].z, ref _normal.x,
                               ref _normal.y, ref _normal.z, ref tu, ref tv, ref tu2, ref tv2, ref color);

                CoreClient._CoreClient.Maths.TVVec3TransformCoord(ref p, _points[i], mesh.GetMatrix());
                _points[i] = p;
            }

            // TODO: i should have a Triangle constructor version that can accept a Vertex type

            return new Triangle(Helpers.TVTypeConverter.FromTVVector(_points[0]), 
                Helpers.TVTypeConverter.FromTVVector(_points[1]), 
                Helpers.TVTypeConverter.FromTVVector(_points[2]));
        }

        public static TV_COLLISIONRESULT CreateTVCollisionResult()
        {
            TV_3DVECTOR vec;
            vec.x = 0;
            vec.y = 0;
            vec.z = 0;
            TV_COLLISIONRESULT result;
            result.bHasCollided = false;
            result.eCollidedObjectType = CONST_TV_OBJECT_TYPE.TV_OBJECT_ALL;
            result.fDistance = 0;
            result.fTexU = 0;
            result.fTexV = 0;
            result.fU = 0;
            result.fV = 0;
            result.iBoneID = -1;
            result.iEntityID = -1;
            result.iFaceindex = -1;
            result.iGroupIndex = -1;
            result.iLandscapeID = -1;
            result.iMeshID = -1;
            result.vCollisionImpact = vec;
            result.vCollisionNormal = vec;
            return result;
        }
    }
}
