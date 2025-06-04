using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;


namespace Keystone.Types
{
    // ABOUT THIS CODE:
    // 
    // Developed by user Kojack in January 2012
    // Extended by Beauty and Tubulii
    //
    // Detailed information about usage:
    // http://www.ogre3d.org/tikiwiki/Euler+Angle+Class
    // 
    // "official place" of C# version:
    // http://www.ogre3d.org/tikiwiki/Euler+Angle+Class+Mogre
    // 
    // For questions and bug reports use this forum topic:
    // viewtopic.php?f=8&t=29262
    //
     
    /// <summary>
    /// This struct manages rotations by use of Euler Angles. 
    /// The rotation will be applied in the order Yaw-Pitch-Roll, which are related to the axis order Y-X-Z. 
    /// It's fully compatible with Ogre::Matrix3::FromEulerAnglesYXZ and Ogre::Matrix3::ToEulerAnglesYXZ. 
    /// For the Yaw angle the standard anticlockwise right handed rotation is used (as common in Ogre).
    /// The Yaw-Pitch-Roll ordering is most convenient for upright character controllers and cameras.
    /// </summary>
    public struct Euler
    {

        /// <summary>Base settings (all angles are 0)</summary>
        public static Euler IDENTITY = new Euler(0, 0, 0);

        /// <summary>Rotation around the Y axis in radians.</summary>
        private double mYaw;

        /// <summary>Rotation around the X axis in radians.</summary>
        private double mPitch;

        /// <summary>Rotation around the Z axis in radians.</summary>
        private double mRoll;

        /// <summary>Is the cached quaternion out of date?</summary>
        private bool mChanged;

        /// <summary>Cached quaternion equivalent of this euler object.</summary>
        private Quaternion mCachedQuaternion;

        
         
        /// <summary>
        /// Constructor to create a new Euler Angle struct from angle values. 
        /// The rotation will be applied in the order Yaw-Pitch- Roll, which are related to the axis order Y-X-Z. 
        /// </summary>
        public Euler(double yaw, double pitch, double roll)
        {
            mYaw = yaw;
            mPitch = pitch;
            mRoll = roll;
            mChanged = true;
            mCachedQuaternion = Quaternion.Identity();
        }
         
         
        /// <summary>
        /// Constructor which calculates the Euler Angles from a quaternion.
        /// </summary>
        public Euler(Quaternion oriantation)
        {
            Matrix rotMat;
            rotMat = new Matrix(oriantation);
                     
            // BUGGY METHOD (NaN return in some cases)
            // rotMat.ToEulerAnglesYXZ(out mYaw, out mPitch, out mRoll);
            // Matrix.DecomposeRollPitchYawZXYMatrix

            // WORKAROUND
            Boolean isUnique;
            Matrix3ToEulerAnglesYXZ(rotMat, out mYaw, out mPitch, out mRoll, out isUnique);
         
            mChanged = true;
            mCachedQuaternion = Quaternion.Identity();
        }


        /// <summary>Get or set the Yaw angle.</summary>
        public double Yaw
        {
            get { return mYaw; }
            set
            {
                mYaw = value;
                mChanged = true;
            }
        }

        /// <summary>Get or set the Pitch angle.</summary>
        public double Pitch
        {
            get { return mPitch; }
            set
            {
                mPitch = value;
                mChanged = true;
            }
        }


        /// <summary>Get or set the Roll angle.</summary>
        public double Roll
        {
            get { return mRoll; }
            set
            {
                mRoll = value;
                mChanged = true;
            }
        }

        public void Add(Euler euler)
        {
            mYaw += euler.mYaw;
            mPitch += euler.mPitch;
            mRoll += euler.mRoll;
            mChanged = true;
        }

        /// <summary>
        /// Apply a relative yaw. (Adds the angle to the current yaw value)
        /// </summary>
        /// <param name="yaw">Yaw value as radian</param>
        public void AddYaw(double yaw)
        {
            mYaw += yaw;
            mChanged = true;
        }
         
         
        /// <summary>
        /// Apply a relative pitch. (Adds the angle to the current pitch value)
        /// </summary>
        /// <param name="pitch">Pitch value as radian</param>
        public void AddPitch(double pitch)
        {
            mPitch += pitch;
            mChanged = true;
        }
         
        /// <summary>
        /// Apply a relative roll. (Adds the angle to the current roll value)
        /// </summary>
        /// <param name="roll">Roll value as degree</param>
        public void AddRoll(double roll)
        {
            mRoll += roll;
            mChanged = true;
        }
         
         
        /// <summary>Get a vector pointing forwards. </summary>
        public Vector3d Forward
        {
            get { return ToQuaternion() * Vector3d.Forward(); }
        }
         
         
        /// <summary>Get a vector pointing to the right.</summary>
        public Vector3d Right
        {
             get { return ToQuaternion() * Vector3d.Right(); }
        }
         
         
        /// <summary> Get a vector pointing up.</summary>
        public Vector3d Up 
        {
             get { return ToQuaternion() * Vector3d.Up(); }
        }
                 
         
         
        /// <summary>
        /// Set the yaw and pitch to face in the given direction. 
        /// The direction doesn't need to be normalised. 
        /// Roll is always unaffected.
        /// </summary>
        /// <param name="directionVector">Vector which points to the wanted direction</param>
        /// <param name="setYaw">if false, the yaw isn't changed.</param>
        /// <param name="setPitch">if false, the pitch isn't changed.</param>
        public void SetDirection(Vector3d directionVector, Boolean setYaw, Boolean setPitch)
        {
            Vector3d d = Vector3d.Normalize (directionVector);
           if(setPitch)
              mPitch = Math.Asin(d.y);
           if(setYaw)
              mYaw = Math.Atan2(d.x, d.z); // TODO: the below i think was in original code but im not sure
                                           // if it doesnt exist to correct a mesh who's "front" is not facing postive Z
              //mYaw = Math.Atan2(d.x, d.z) + Math.PI / 2.0;

           mChanged = setYaw || setPitch;
        }
         
         
         
        /// <summary>
        /// Normalise the selected rotations to be within the +/-180 degree range. 
        /// The normalise uses a wrap around, so for example a yaw of 360 degrees becomes 0 degrees, 
        /// and -190 degrees becomes 170. 
        /// By the parameters it's possible to choose which angles should be normalised.
        /// </summary>
        /// <param name="normYaw">If true, the angle will be normalised.</param>
        /// <param name="normPitch">If true, the angle will be normalised.</param>
        /// <param name="normRoll">If true, the angle will be normalised.</param>
        /// <remarks></remarks>
        public void Normalize(Boolean normYaw, Boolean normPitch, Boolean normRoll)
        {
            const double TWOPI = Math.PI * 2.0;

           if(normYaw)
           {
              double yaw = mYaw;
              if(yaw < -Math.PI)
              {
                  yaw = System.Math.IEEERemainder(yaw, TWOPI);
                 if(yaw < -Math.PI)
                 {
                     yaw += TWOPI;
                 }
                 mYaw = yaw;
                 mChanged = true;
              }
              else if(yaw > Math.PI)
              {
                  yaw = System.Math.IEEERemainder(yaw, TWOPI);
                 if(yaw > Math.PI)
                 {
                     yaw -= TWOPI;
                 }
                 mYaw = yaw;
                 mChanged = true;
              }
           }
         
           if(normPitch)
           {
                 double pitch = mPitch;
              if(pitch < -Math.PI)
              {
                  pitch = System.Math.IEEERemainder(pitch, TWOPI);
                 if(pitch < -Math.PI)
                 {
                     pitch += TWOPI;
                 }
                 mPitch = pitch;
                 mChanged = true;

                 if (double.IsNaN(mPitch * Utilities.MathHelper.RADIANS_TO_DEGREES)) // DEBUGGING
                     { 
                     }  // add breakpoint here
                 }
              else if(pitch > Math.PI)
              {
                  pitch = System.Math.IEEERemainder(pitch, TWOPI);
                 if(pitch > Math.PI)
                 {
                     pitch -= TWOPI;
                 }
                 mPitch = pitch;
                 mChanged = true;
         
                     if (double.IsNaN(mPitch * Utilities.MathHelper.RADIANS_TO_DEGREES)) // DEBUGGING
                     { 
                     }  // add breakpoint here
              }
           }
         
           if(normRoll)
           {
              double roll= mRoll;
              if(roll < -Math.PI)
              {
                  roll = System.Math.IEEERemainder(roll, TWOPI);
                 if(roll < -Math.PI)
                 {
                     roll += TWOPI;
                 }
                 mRoll = roll;
                 mChanged = true;
              }
              else if(roll > Math.PI)
              {
                  roll = System.Math.IEEERemainder(roll, TWOPI);
                 if(roll > Math.PI)
                 {
                     roll -= TWOPI;
                 }
                 mRoll = roll;
                 mChanged = true;
              }
           }
        } // Normalise()
         
         
         
         
        /// <summary>
        /// Return the relative euler angles required to rotate from the current forward direction to the specified direction vector. 
        /// The result euler can then be added to the current euler to immediately face dir. 
        /// Rotation is found to face the correct direction. For example, when false a yaw of 1000 degrees and a dir of
        /// (0,0,-1) will return a -1000 degree yaw. When true, the same yaw and dir would give 80 degrees (1080 degrees faces
        /// the same way as (0,0,-1).
        /// The rotation won't flip upside down then roll instead of a 180 degree yaw.
        /// </summary>
        /// <param name="direction">...TODO...</param>
        /// <param name="shortest">If false, the full value of each angle is used. If true, the angles are normalised and the shortest rotation is found to face the correct direction.</param>
        /// <param name="setYaw">If true the angles are calculated. If false, the angle is set to 0. </param>
        /// <param name="setPitch">If true the angles are calculated. If false, the angle is set to 0. </param>
        public Euler GetRotationTo(Vector3d direction, Boolean setYaw, Boolean setPitch, Boolean shortest)
        {
           Euler t1 = Euler.IDENTITY;
           Euler t2;
           t1.SetDirection(direction, setYaw, setPitch);
           t2 = t1 - this;
           if(shortest && setYaw)
           {
              t2.Normalize(true, true, true);
              t2.mChanged = true; // this wont always be set during Normalize() and Euler2 never uses constructor
           }
           return t2;
        }
         
         
         
        /// <summary>
        /// Clamp the yaw angle to a range of +/-limit in radians
        /// </summary>
        /// <param name="limit">Wanted co-domain for the Yaw angle in radians.</param>
        public void LimitYaw(double limit)
        {
            if (mYaw > limit)
            {
                mYaw = limit;
                mChanged = true;
            }
            else if (mYaw < -limit)
            {
                mYaw = -limit;
                mChanged = true;
            }
        }
         
         
         
        /// <summary>
        /// Clamp the pitch angle to a range of +/-limit in radians
        /// </summary>
        /// <param name="limit">Wanted co-domain for the Pitch angle in radians.</param>
        public void LimitPitch(double limit)
        {
            if (mPitch > limit)
            {
                mPitch = limit;
                mChanged = true;
            }
            else if (mPitch < -limit)
            {
                mPitch = -limit;
                mChanged = true;
            }
        }
         
         
         
        /// <summary>
        /// Clamp the roll angle to a range of +/-limit.
        /// </summary>
        /// <param name="limit">Wanted co-domain for the Roll angle in radians.</param>
        public void LimitRoll(double limit)
        {
            if (mRoll > limit)
            {
                mRoll = limit;
                mChanged = true;
            }
            else if (mRoll < -limit)
            {
                mRoll = -limit;
                mChanged = true;
            }
        }
         
         
         
         
        /// <summary>
        /// Port of method <c>Matrix3.ToEulerAnglesYXZ()</c>, from MogreMatrix3.cpp as a workaround for a bug in the Math class. 
        /// (The bug was fixed, but is not present in common used binary files.)
        /// </summary>
        /// <param name="matrix">Rotation matrix</param>
        /// <param name="isUnique">If false, the orientation can be described by different angle combinations. 
        ///                        In this case the returned angle values can be different than expected.</param>
        public static void Matrix3ToEulerAnglesYXZ(Matrix matrix,
            out double rfYAngle, out double rfPAngle, out double rfRAngle, out Boolean isUnique)
        {
            const double HALF_PI = Math.PI / 2d;
            rfPAngle = Math.Asin(-matrix.M23);
         
            if (rfPAngle < HALF_PI)
            {
                if (rfPAngle > -HALF_PI)
                {
                    rfYAngle = Math.Atan2(matrix.M13, matrix.M33);
                    rfRAngle = Math.Atan2(matrix.M21, matrix.M22);
                    isUnique = true;
                    return;
                }
                else
                {
                    // WARNING.  Not a unique solution.
                    double fRmY = Math.Atan2(-matrix.M12, matrix.M11);
                    rfRAngle = 0d;  // any angle works
                    rfYAngle = rfRAngle - fRmY;
                    isUnique = false;
                    return;
                }
            }
            else
            {
                // WARNING.  Not a unique solution.
                double fRpY = Math.Atan2(-matrix.M12, matrix.M11);
                rfRAngle = 0d;  // any angle works
                rfYAngle = fRpY - rfRAngle;
                isUnique = false;
                return;
            }
         
         
         
            // "Original" CODE FROM CLASS Matrix3.ToEulerAnglesYXZ()
         
            //rfPAngle = Mogre::Math::ASin(-m12);
            //if ( rfPAngle < Radian(Math::HALF_PI) )
            //{
            //    if ( rfPAngle > Radian(-Math::HALF_PI) )
            //    {
            //        rfYAngle = System::Math::Atan2(m02,m22);
            //        rfRAngle = System::Math::Atan2(m10,m11);
            //        return true;
            //    }
            //    else
            //    {
            //        // WARNING.  Not a unique solution.
            //        Radian fRmY = System::Math::Atan2(-m01,m00);
            //        rfRAngle = Radian(0.0);  // any angle works
            //        rfYAngle = rfRAngle - fRmY;
            //        return false;
            //    }
            //}
            //else
            //{
            //    // WARNING.  Not a unique solution.
            //    Radian fRpY = System::Math::Atan2(-m01,m00);
            //    rfRAngle = Radian(0.0);  // any angle works
            //    rfYAngle = fRpY - rfRAngle;
            //    return false;
            //}
         
         
        } // Matrix3ToEulerAnglesYXZ()

        /// <summary>
        /// Return the Euler rotation state as quaternion.
        /// </summary>
        /// <param name="e">Euler Angle state</param>
        /// <returns>Rotation state as Quaternion</returns>
        public static implicit operator Quaternion(Euler e)
        {
            return e.ToQuaternion();
        }
         
        /// <summary>
        /// Add two euler objects.
        /// </summary>
        /// <returns>Calculation result</returns>
        public static Euler operator +(Euler lhs, Euler rhs)
        {
            return new Euler(lhs.Yaw + rhs.Yaw, lhs.Pitch + rhs.Pitch, lhs.Roll + rhs.Roll);
        }
         
         
        /// <summary>
        /// Subtract two euler objects. This finds the difference as relative angles.
        /// </summary>
        /// <returns>Calculation result</returns>
        public static Euler operator-(Euler lhs, Euler rhs)
        {
             return new Euler(lhs.Yaw - rhs.Yaw, lhs.Pitch - rhs.Pitch, lhs.Roll - rhs.Roll);
        }
         
         
        /// <summary>
        /// Interpolate each euler angle by the given factor. 
        /// (Each angle will be multiplied with the factor.)
        /// </summary>
        /// <returns>Calculation result</returns>
        public static Euler operator *(Euler lhs, Single factor)
        {
            return new Euler(lhs.Yaw * factor, lhs.Pitch * factor, lhs.Roll * factor);
        }
         
         
        /// <summary>
        /// Interpolate the euler angles by lhs. 
        /// (Each angle will be multiplied with the factor.)
        /// </summary>
        /// <returns>Calculation result</returns>
        public static Euler operator *(Single factor, Euler rhs)
        {
            return new Euler(factor * rhs.Yaw, factor * rhs.Pitch, factor * rhs.Roll);
        }
         
         
        /// <summary>
        /// Apply the euler rotation to the vector rhs. 
        /// The calculation is equal to: quaternion*vector
        /// </summary>
        /// <returns>Calculation result</returns>
        public static Vector3d operator *(Euler lhs, Vector3d rhs)
        {
            return lhs.ToQuaternion() * rhs;
        }

        /// <summary>
        /// Calculate the quaternion of the euler object. 
        /// The result is cached. It will only be recalculated when the component euler angles are changed.
        /// </summary>
        public Quaternion ToQuaternion()
        {
            if (mChanged)
            {
                mCachedQuaternion = new Quaternion(Vector3d.Up(), mYaw) * new Quaternion(Vector3d.Right(), mPitch) * new Quaternion(Vector3d.Forward(), mRoll);
                mChanged = false;
            }
            return mCachedQuaternion;
        }


        ///// <summary>
        ///// Return a String with degree values of the axis rotations (human readable style). 
        ///// For example "X-axis: 0°   Y-axis: 36°   Z-axis: 90°"
        ///// </summary>
        //public String ToAxisString()
        //{
        //    return String.Format("X: {0:00}°   Y: {1:00}°   Z: {2:00}°", 
        //        Pitch.ValueDegrees, Yaw.ValueDegrees, Roll.ValueDegrees);
        //}




        ///// <summary>
        ///// Return a String with degree values in the applied rotation order (human readable style).
        ///// For example "Yaw: 0°   Pitch: 36°   Roll: 90°"
        ///// </summary>
        //public String ToYawPitchRollString()
        //{
        //    return String.Format("Yaw: {0:00}°   Pitch: {1:00}°   Roll: {2:00}°", 
        //        Yaw.ValueDegrees, Pitch.ValueDegrees, Roll.ValueDegrees);
        //}

        /// <summary>
        /// Try to parse 3 floating point values from a string, which are seperated by spaces or tabulators. 
        /// Input order for rotation values:: Yaw, Pitch, Roll (all in degree)
        /// If success, an Euler struct will be returned. 
        /// If parsing failed, a FormatException will be thrown. 
        /// Example: "111  .99   -66" 
        /// </summary>
        /// <param name="valueString">String which contains 3 floating point values</param>
        /// <remarks>
        /// Multiple and mixed usage of space/tabulator/commas are possible. 
        /// As decimal seperator a dot "." is expected. 
        /// </remarks>
        /// <returns>Returns an Euler struct or a FormatException</returns>
        public static Euler ParseStringYawPitchRoll(String valueString)
        {
            double[] values = Parse3Params(valueString);

            if (values == null)
                throw new FormatException(String.Format("Can't parse floating point values of string '{0}'", valueString));

            return new Euler(values[0], values[1], values[2]);
        }



        /// <summary>
        /// Try to parse 3 floating point values from a string, which are seperated by spaces or tabulators or comma. 
        /// Input order for rotation values: X-axis, Y-axis, Z-axis (all in degree)
        /// If success, an Euler struct will be returned. 
        /// If parsing failed, a FormatException will be thrown. 
        /// Example: "111  .99   -66" 
        /// </summary>
        /// <param name="valueString">String which contains 3 floating point values</param>
        /// <remarks>
        /// Multiple and mixed usage of space/tabulator/commas are possible. 
        /// As decimal seperator a dot "." is expected. 
        /// </remarks>
        /// <returns>Returns an Euler struct or a FormatException</returns>
        public static Euler ParseStringAxisXYZ(String valueString)
        {
            double[] values = Parse3Params(valueString);

            if (values == null)
                throw new FormatException(String.Format("Can't parse floating point values of string '{0}'", valueString));

            return new Euler(values[1], values[0], values[2]);
        }



        /// <summary>
        /// Try to parse 3 floating point values from a string, which are seperated by spaces or tabulators or comma. 
        /// If parsing failed, null will be returned. 
        /// Example: "111  .99   -66" 
        /// </summary>
        /// <param name="valueString">String which contains 3 floating point values</param>
        /// <remarks>
        /// Multiple and mixed usage of space/tabulator/commas are possible. 
        /// As decimal seperator a dot "." is expected. 
        /// </remarks>
        /// <returns>Returns 3 Single values or null</returns>
        private static double[] Parse3Params(string valueString)
        {
            // Some Regex explanation:
            //
            // The "@" prefix in front of the String means: 
            //         Backslash are processed as Text instead of special symbols.
            //         Advantage: Just write "\" instead of "\\" for each backslash
            // 
            // "^" at first position means:  No text is allowed before
            // "$" at the end means:         No text is allowed after that
            // 
            // Floating point values are matched
            //         Expression: "-?\d*\.?\d+"
            //         Examples:   "111",  "0.111",  ".99",  "-66"
            //
            // Seperator can be tabs or spaces or commas (at least one symbol; mixing is possible)
            //         Expression: "[, \t]+"


            String val = @"[-\d\.]+";     // simplified (faster) floating point pattern (exact pattern would be @"-?\d*\.?\d+" )
            String sep = @"[, \t]+";      // seperator pattern

            // build target pattern
            String searchPattern = "^(" + val + ")" + sep + "(" + val + ")" + sep + "(" + val + ")$";

            Match match = Regex.Match(valueString, searchPattern);

            try
            {
                if (match.Success)
                {

                    System.Globalization.CultureInfo englishCulture = keymath.ParseHelper.English.Culture;

                    double[] result = new double[3];
                    result[0] = Convert.ToDouble(match.Groups[1].Value, englishCulture);
                    result[1] = Convert.ToDouble(match.Groups[2].Value, englishCulture);
                    result[2] = Convert.ToDouble(match.Groups[3].Value, englishCulture);
                    return result;
                }
                else
                    return null;
            }
            catch (FormatException) { return null; }
            catch (OverflowException) { return null; }

        } // Parse3Params()


    }  // struct Euler
}  // namespace

