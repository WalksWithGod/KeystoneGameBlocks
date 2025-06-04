using Keystone.Physics.Primitives;
using Keystone.Types;
using System;
using System.Collections.Generic;

namespace Keystone.Physics
{
	public sealed class Toolbox
	{
        // Constants
        public const double epsilon = 1E-07;
        public const double bigEpsilon = 1E-05;

        // Statics
        public static readonly Vector3d noVector;
        public static Vector3d zeroVector;
        public static Vector3d rightVector;
        public static Vector3d upVector;
        public static Vector3d forwardVector;
        public static Vector3d leftVector;
        public static Vector3d downVector;
        public static Vector3d backVector;
        public static Matrix zeroMatrix;
        public static Quaternion identityOrientation;

		// Constructors
		static Toolbox ()
		{
            noVector = new Vector3d(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);
		    zeroVector = new Vector3d();
			rightVector = Vector3d.Right();
			upVector = Vector3d.Up();
			forwardVector = Vector3d.Forward();
			leftVector = -Vector3d.Right();
			downVector = -Vector3d.Up();
			backVector = -Vector3d.Forward();
			zeroMatrix = new Matrix(0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d);
			identityOrientation = Quaternion.Identity();
		}
		
		// Methods
        // lineA, lineB, planePtD, planePtE, planePtF
		public static bool getSegmentPlaneIntersection (Vector3d a, Vector3d b, Vector3d d, Vector3d e, Vector3d f, out Vector3d q)
		{
			Plane p = new Plane( d,e,f);
		    double single1;
			return getSegmentPlaneIntersection(a, b, p, out single1, out q);
		}
		
		public static bool getSegmentPlaneIntersection (Vector3d a, Vector3d b, Plane p, out Vector3d q)
		{
			double single1;
			return getSegmentPlaneIntersection(a, b, p, out single1, out q);
		}
		
		public static bool getSegmentPlaneIntersection (Vector3d a, Vector3d b, Plane p, out double t, out Vector3d q)
		{
			q = noVector;
			Vector3d vector1 = b - a;
			t = (p.Distance  - Vector3d.DotProduct(p.Normal, a)) / Vector3d.DotProduct(p.Normal, vector1);
			if ((t >= 0.00d) && (t <= 1.00F))
			{
				q = a + (vector1 * t);
				return true;
			}
			return false;
		}
		
		public static bool getRayPlaneIntersection (Vector3d origin, Vector3d direction, double maximumLength, Vector3d planePosition, Vector3d planeNormal, out Vector3d hitLocation, out double t)
		{
			t = Vector3d.DotProduct(planePosition, planeNormal) - (Vector3d.DotProduct(origin, planeNormal) / Vector3d.DotProduct(planeNormal, direction));
			if ((t >= 0.00d) && (t < maximumLength))
			{
				hitLocation = origin + (direction * t);
				return true;
			}
			hitLocation = noVector;
			return false;
		}

        public static bool findRayTriangleIntersection(Vector3d rayOrigin, Vector3d rayDirection, double maximumLength, Vector3d a, Vector3d b, Vector3d c, out Vector3d hitLocation, out double t)
		{
			hitLocation = noVector;
            t = double.NegativeInfinity;
			Vector3d vector1 = b - a;
			Vector3d vector2 = c - a;
			Vector3d vector4 = Vector3d.CrossProduct(rayDirection, vector2);
            double single1 = Vector3d.DotProduct(vector1, vector4);
			if ((single1 > 0.00d) && (single1 < 0.00d))
			{
				return false;
			}
            double single2 = 1.00F / single1;
			Vector3d vector3 = rayOrigin - a;
            double single3 = Vector3d.DotProduct(vector3, vector4) * single2;
			if ((single3 < 0.00d) || (single3 > 1.00F))
			{
				return false;
			}
			Vector3d vector5 = Vector3d.CrossProduct(vector3, vector1);
            double single4 = Vector3d.DotProduct(rayDirection, vector5) * single2;
			if ((single4 < 0.00d) || ((single3 + single4) > 1.00F))
			{
				return false;
			}
			t = Vector3d.DotProduct(vector2, vector5) * single2;
			if ((t > maximumLength) || (t < 0.00d))
			{
                t = double.NegativeInfinity;
				return false;
			}
			hitLocation = rayOrigin + (rayDirection * t);
			return true;
		}
		
		public static bool isPointInsideTriangle (ref Vector3d vA, ref Vector3d vB, ref Vector3d vC, ref Vector3d p)
		{
			Vector3d vector1 = vC - vA;
			Vector3d vector2 = vB - vA;
			Vector3d vector3 = p - vA;
            double single1 = vector1.LengthSquared();
            double single2 = Vector3d.DotProduct(vector1, vector2);
            double single3 = Vector3d.DotProduct(vector1, vector3);
            double single4 = vector2.LengthSquared();
            double single5 = Vector3d.DotProduct(vector2, vector3);
            double single6 = 1.00F / ((single1 * single4) - (single2 * single2));
            double single7 = ((single4 * single3) - (single2 * single5)) * single6;
            double single8 = ((single1 * single5) - (single2 * single3)) * single6;
            double single9 = (1.00F - single7) - single8;
			if ((single7 > 0.00d) && (single8 > 0.00d))
			{
				return single9 > 0.00d;
			}
			return false;
		}
		
		public static bool isPointInsideTriangle (ref Vector3d vA, ref Vector3d vB, ref Vector3d vC, ref Vector3d p, double margin)
		{
			Vector3d vector1 = vC - vA;
			Vector3d vector2 = vB - vA;
			Vector3d vector3 = p - vA;
            double single1 = vector1.LengthSquared();
            double single2 = Vector3d.DotProduct(vector1, vector2);
            double single3 = Vector3d.DotProduct(vector1, vector3);
            double single4 = vector2.LengthSquared();
            double single5 = Vector3d.DotProduct(vector2, vector3);
            double single6 = 1.00F / ((single1 * single4) - (single2 * single2));
            double single7 = ((single4 * single3) - (single2 * single5)) * single6;
            double single8 = ((single1 * single5) - (single2 * single3)) * single6;
            double single9 = (1.00F - single7) - single8;
			if ((single7 > -margin) && (single8 > -margin))
			{
				return single9 > -margin;
			}
			return false;
		}
		
		public static void getClosestPointOnTriangleToPoint (ref Vector3d a, ref Vector3d b, ref Vector3d c, ref Vector3d p, out Vector3d closestPoint)
		{
		    Vector3d vector1 = b-a;
			Vector3d vector2 = c-a;
			Vector3d vector3 = p-a;
			double single3 = Vector3d.DotProduct(vector1,vector3);
			double single4 = Vector3d.DotProduct(vector2,vector3);
			if ((single3 <= 0.00d) && (single4 < 0.00d))
			{
				closestPoint = a;
			}
			else
			{
			    Vector3d vector4 = p-b;
				double single5 = Vector3d.DotProduct(vector1,vector4);
				double single6 = Vector3d.DotProduct(vector2,vector4);
				if ((single5 >= 0.00d) && (single6 <= single5))
				{
					closestPoint = b;
				}
				else
				{
                    double single1;
                    double single7 = (single3 * single6) - (single5 * single4);
					if (((single7 <= 0.00d) && (single3 >= 0.00d)) && (single5 <= 0.00d))
					{
						single1 = single3 / (single3 - single5);
						closestPoint=vector1* single1;
						closestPoint=closestPoint+a;
					}
					else
					{
					    Vector3d vector5 = p-c;
						double single8 = Vector3d.DotProduct(vector1,vector5);
						double single9 = Vector3d.DotProduct(vector2,vector5);
						if ((single9 >= 0.00d) && (single8 <= single9))
						{
							closestPoint = c;
						}
						else
						{
                            double single2;
                            double single10 = (single8 * single4) - (single3 * single9);
							if (((single10 <= 0.00d) && (single4 >= 0.00d)) && (single9 <= 0.00d))
							{
								single2 = single4 / (single4 - single9);
								closestPoint=vector2* single2;
								closestPoint=closestPoint+a;
							}
							else
							{
                                double single11 = (single5 * single9) - (single8 * single6);
								if (((single11 <= 0.00d) && ((single6 - single5) >= 0.00d)) && ((single8 - single9) >= 0.00d))
								{
									single2 = (single6 - single5) / ((single6 - single5) + (single8 - single9));
									closestPoint=c-b;
									closestPoint=closestPoint* single2;
									closestPoint=closestPoint+b;
								}
								else
								{
								    double single12 = 1.00d / ((single11 + single10) + single7);
									single1 = single10 * single12;
									single2 = single7 * single12;
									Vector3d vector6 = vector1* single1;
									Vector3d vector7 = vector2* single2;
									closestPoint=a+vector6;
									closestPoint=closestPoint+vector7;
								}
							}
						}
					}
				}
			}
		}
		
		public static void getClosestPointOnTriangleToPoint (ref Vector3d a, ref Vector3d b, ref Vector3d c, ref Vector3d p, List<Vector3d> subsimplex, out Vector3d closestPoint)
		{
		    subsimplex.Clear();
			Vector3d vector1 = b-a;
			Vector3d vector2 = c-a;
			Vector3d vector3 = p-a;
			double single3 = Vector3d.DotProduct(vector1,vector3);
			double single4 = Vector3d.DotProduct(vector2,vector3);
			if ((single3 <= 0.00d) && (single4 < 0.00d))
			{
				subsimplex.Add(a);
				closestPoint = a;
			}
			else
			{
			    Vector3d vector4 = p-b;
				double single5 = Vector3d.DotProduct(vector1,vector4);
				double single6 = Vector3d.DotProduct(vector2,vector4);
				if ((single5 >= 0.00d) && (single6 <= single5))
				{
					subsimplex.Add(b);
					closestPoint = b;
				}
				else
				{
                    double single1;
                    double single7 = (single3 * single6) - (single5 * single4);
					if (((single7 <= 0.00d) && (single3 >= 0.00d)) && (single5 <= 0.00d))
					{
						subsimplex.Add(a);
						subsimplex.Add(b);
						single1 = single3 / (single3 - single5);
						closestPoint=vector1* single1;
						closestPoint=closestPoint+a;
					}
					else
					{
					    Vector3d vector5 = p-c;
						double single8 = Vector3d.DotProduct(vector1,vector5);
						double single9 = Vector3d.DotProduct(vector2,vector5);
						if ((single9 >= 0.00d) && (single8 <= single9))
						{
							subsimplex.Add(c);
							closestPoint = c;
						}
						else
						{
							double single2;
							double single10 = (single8 * single4) - (single3 * single9);
							if (((single10 <= 0.00d) && (single4 >= 0.00d)) && (single9 <= 0.00d))
							{
								subsimplex.Add(a);
								subsimplex.Add(c);
								single2 = single4 / (single4 - single9);
								closestPoint=vector2* single2;
								closestPoint=closestPoint+a;
							}
							else
							{
								double single11 = (single5 * single9) - (single8 * single6);
								if (((single11 <= 0.00d) && ((single6 - single5) >= 0.00d)) && ((single8 - single9) >= 0.00d))
								{
									subsimplex.Add(b);
									subsimplex.Add(c);
									single2 = (single6 - single5) / ((single6 - single5) + (single8 - single9));
									closestPoint=c-b;
									closestPoint=closestPoint* single2;
									closestPoint=closestPoint+b;
								}
								else
								{
								    subsimplex.Add(a);
									subsimplex.Add(b);
									subsimplex.Add(c);
									double single12 = 1.00F / ((single11 + single10) + single7);
									single1 = single10 * single12;
									single2 = single7 * single12;
									Vector3d vector6 = vector1* single1;
									Vector3d vector7 = vector2* single2;
									closestPoint=a+vector6;
									closestPoint=closestPoint+vector7;
								}
							}
						}
					}
				}
			}
		}
		
		public static void getClosestPointOnTriangleToPoint (List<Vector3d> q, int i, int j, int k, ref Vector3d p, List<int> subsimplex, List<double> baryCoords, out Vector3d closestPoint)
		{
		    subsimplex.Clear();
			baryCoords.Clear();
			Vector3d vector1 = q[i];
			Vector3d vector2 = q[j];
			Vector3d vector3 = q[k];
			Vector3d vector4 = vector2-vector1;
			Vector3d vector5 = vector3-vector1;
			Vector3d vector6 = p-vector1;
			double single3 = Vector3d.DotProduct(vector4,vector6);
			double single4 = Vector3d.DotProduct(vector5,vector6);
			if ((single3 <= 0.00d) && (single4 < 0.00d))
			{
				subsimplex.Add(i);
				baryCoords.Add(1.00F);
				closestPoint = vector1;
			}
			else
			{
			    Vector3d vector7 = p-vector2;
				double single5 = Vector3d.DotProduct(vector4,vector7);
				double single6 = Vector3d.DotProduct(vector5,vector7);
				if ((single5 >= 0.00d) && (single6 <= single5))
				{
					subsimplex.Add(j);
					baryCoords.Add(1.00F);
					closestPoint = vector2;
				}
				else
				{
					double item;
					double single7 = (single3 * single6) - (single5 * single4);
					if (((single7 <= 0.00d) && (single3 >= 0.00d)) && (single5 <= 0.00d))
					{
						subsimplex.Add(i);
						subsimplex.Add(j);
						item = single3 / (single3 - single5);
						baryCoords.Add(1.00D - item);
						baryCoords.Add(item);
						closestPoint=vector4* item;
						closestPoint=closestPoint+vector1;
					}
					else
					{
					    Vector3d vector8 = p-vector3;
						double single8 = Vector3d.DotProduct(vector4,vector8);
						double single9 = Vector3d.DotProduct(vector5,vector8);
						if ((single9 >= 0.00d) && (single8 <= single9))
						{
							subsimplex.Add(k);
							baryCoords.Add(1.00F);
							closestPoint = vector3;
						}
						else
						{
							double single2;
							double single10 = (single8 * single4) - (single3 * single9);
							if (((single10 <= 0.00d) && (single4 >= 0.00d)) && (single9 <= 0.00d))
							{
								subsimplex.Add(i);
								subsimplex.Add(k);
								single2 = single4 / (single4 - single9);
								baryCoords.Add(1.00D - single2);
								baryCoords.Add(single2);
								closestPoint=vector5* single2;
								closestPoint=closestPoint+vector1;
							}
							else
							{
								double single11 = (single5 * single9) - (single8 * single6);
								if (((single11 <= 0.00d) && ((single6 - single5) >= 0.00d)) && ((single8 - single9) >= 0.00d))
								{
									subsimplex.Add(j);
									subsimplex.Add(k);
									single2 = (single6 - single5) / ((single6 - single5) + (single8 - single9));
									baryCoords.Add(1.00D - single2);
									baryCoords.Add(single2);
									closestPoint=vector3-vector2;
									closestPoint=closestPoint* single2;
									closestPoint=closestPoint+vector2;
								}
								else
								{
								    subsimplex.Add(i);
									subsimplex.Add(j);
									subsimplex.Add(k);
									double single12 = 1.00F / ((single11 + single10) + single7);
									item = single10 * single12;
									single2 = single7 * single12;
									baryCoords.Add((1.00D - item) - single2);
									baryCoords.Add(item);
									baryCoords.Add(single2);
									Vector3d vector9 = vector4* item;
									Vector3d vector10 = vector5* single2;
									closestPoint=vector1+vector9;
									closestPoint=closestPoint+vector10;
								}
							}
						}
					}
				}
			}
		}
		
		public static double getDistanceFromPointToLine (Vector3d p, Vector3d a, Vector3d b)
		{
			Vector3d vector1 = b - a;
			Vector3d vector2 = Vector3d.CrossProduct(vector1, p - a);
			Vector3d vector3 = Vector3d.CrossProduct(vector1,vector2);
			return vector3.Length / vector1.Length;
		}
		
		public static double getSquaredDistanceFromPointToLine (Vector3d p, Vector3d a, Vector3d b)
		{
			Vector3d vector1 = p - a;
			Vector3d vector2 = b - a;
			double single1 = Vector3d.DotProduct(vector1, vector2);
			return Vector3d.DotProduct(vector1, vector1) - ((single1 * single1) / Vector3d.DotProduct(vector2, vector2));
		}
		
		public static void getClosestPointOnSegmentToPoint (ref Vector3d a, ref Vector3d b, ref Vector3d p, out Vector3d closestPoint)
		{
		    Vector3d vector1 = b-a;
			Vector3d vector2 = p-a;
			double single1 = Vector3d.DotProduct(vector2,vector1);
			if (single1 <= 0.00d)
			{
				single1 = 0.00d;
				closestPoint = a;
			}
			else
			{
				double single2 = ((vector1.x * vector1.x) + (vector1.y * vector1.y)) + (vector1.z * vector1.z);
				if (single1 >= single2)
				{
					single1 = 1.00F;
					closestPoint = b;
				}
				else
				{
				    single1 /= single2;
					Vector3d vector3 = vector1* single1;
					closestPoint=a+vector3;
				}
			}
		}
		
		public static void getClosestPointOnSegmentToPoint (ref Vector3d a, ref Vector3d b, ref Vector3d p, List<Vector3d> subsimplex, out Vector3d closestPoint)
		{
		    subsimplex.Clear();
			Vector3d vector1 = b-a;
			Vector3d vector2 = p-a;
			double single1 = Vector3d.DotProduct(vector2,vector1);
			if (single1 <= 0.00d)
			{
				subsimplex.Add(a);
				closestPoint = a;
			}
			else
			{
				double single2 = ((vector1.x * vector1.x) + (vector1.y * vector1.y)) + (vector1.z * vector1.z);
				if (single1 >= single2)
				{
					subsimplex.Add(b);
					closestPoint = b;
				}
				else
				{
				    single1 /= single2;
					subsimplex.Add(a);
					subsimplex.Add(b);
					Vector3d vector3 = vector1* single1;
					closestPoint=a+vector3;
				}
			}
		}
		
		public static void getClosestPointOnSegmentToPoint (List<Vector3d> q, int i, int j, ref Vector3d p, List<int> subsimplex, List<double> baryCoords, out Vector3d closestPoint)
		{
		    Vector3d vector1 = q[i];
			Vector3d vector2 = q[j];
			subsimplex.Clear();
			baryCoords.Clear();
			Vector3d vector3 = vector2-vector1;
			Vector3d vector4 = p-vector1;
			double item = Vector3d.DotProduct(vector4,vector3);
			if (item <= 0.00d)
			{
				subsimplex.Add(i);
				baryCoords.Add(1.00F);
				closestPoint = vector1;
			}
			else
			{
				double single2 = ((vector3.x * vector3.x) + (vector3.y * vector3.y)) + (vector3.z * vector3.z);
				if (item >= single2)
				{
					subsimplex.Add(j);
					baryCoords.Add(1.00F);
					closestPoint = vector2;
				}
				else
				{
				    item /= single2;
					subsimplex.Add(i);
					subsimplex.Add(j);
					baryCoords.Add(1.00d - item);
					baryCoords.Add(item);
					Vector3d vector5 = vector3* item;
					closestPoint=vector1+vector5;
				}
			}
		}
		
		public static bool getIntersectionParallelSegments (Vector3d p1, Vector3d q1, Vector3d p2, Vector3d q2, out Vector3d a, out Vector3d b)
		{
			a = noVector;
			b = noVector;
			Vector3d dirA = q1 - p1;
			Vector3d dirB = q2 - p2;
			if (!areSegmentsParallel(dirA, dirB))
			{
				return false;
			}
			double n = Vector3d.DotProduct(p1, dirA);
			double single2 = Vector3d.DotProduct(q1, dirA);
			double min = Vector3d.DotProduct(p2, dirA);
			double max = Vector3d.DotProduct(q2, dirA);
			if (((n < min) && (n < max)) && ((single2 < min) && (single2 < max)))
			{
				return false;
			}
			double val1 = Clamp(n, min, max);
			double single6 = Clamp(single2, min, max);
			double single7 = Clamp(min, n, single2);
			double val2 = Clamp(max, n, single2);
			double single9 = Math.Min(val1, Math.Min(single6, Math.Min(single7, val2)));
			double single10 = Math.Max(val1, Math.Max(single6, Math.Max(single7, val2)));
			double single11 = 1.00F / dirA.LengthSquared();
			a =  ((single9 * single11) * dirA);
			b =  ((single10 * single11) * dirA);
			return true;
		}
		
		public static double getClosestPointsBetweenSegments (Vector3d p1, Vector3d q1, Vector3d p2, Vector3d q2, out Vector3d c1, out Vector3d c2)
		{
			double single1;
			double single2;
			return getClosestPointsBetweenSegments(p1, q1, p2, q2, out single1, out single2, out c1, out c2);
		}
		
		public static double getClosestPointsBetweenSegments (Vector3d p1, Vector3d q1, Vector3d p2, Vector3d q2, out double s, out double t, out Vector3d c1, out Vector3d c2)
		{
		    Vector3d vector4;
			Vector3d vector1 = q1-p1;
			Vector3d vector2 = q2-p2;
			Vector3d vector3 = p1-p2;
			double single1 = vector1.LengthSquared();
			double single2 = vector2.LengthSquared();
			double single3 = Vector3d.DotProduct(vector2,vector3);
			if ((single1 <= 0.00d) && (single2 <= 0.00d))
			{
				double single8;
				t = single8 = 0.00d;
				s = single8;
				c1 = p1;
				c2 = p2;
				vector4=c1-c2;
				return vector4.LengthSquared();
			}
			if (single1 <= 0.00d)
			{
				s = 0.00d;
                t = Keystone.Utilities.MathHelper.Clamp(single3 / single2, 0.00d, 1.00D);
			}
			else
			{
				double single4 = Vector3d.DotProduct(vector1, vector3);
				if (single2 <= 0.00d)
				{
					t = 0.00d;
                    s = Keystone.Utilities.MathHelper.Clamp(-single4 / single1, 0.00d, 1.00D);
				}
				else
				{
					double single5 = Vector3d.DotProduct(vector1, vector2);
					double single6 = (single1 * single2) - (single5 * single5);
					if (single6 != 0.00d)
					{
                        s = Keystone.Utilities.MathHelper.Clamp(((single5 * single3) - (single4 * single2)) / single6, 0.00d, 1.00d);
					}
					else
					{
						s = 0.50D;
					}
					double single7 = (single5 * s) + single3;
					if (single7 < 0.00d)
					{
						t = 0.00d;
                        s = Keystone.Utilities.MathHelper.Clamp(-single4 / single1, 0.00d, 1.00d);
					}
					else if (single7 > single2)
					{
						t = 1.00D;
                        s = Keystone.Utilities.MathHelper.Clamp((single5 - single4) / single1, 0.00d, 1.00d);
					}
					else
					{
						t = single7 / single2;
					}
				}
			}
			c1 = p1 + (vector1 * s);
			c2 = p2 + (vector2 * t);
			vector4=c1-c2;
			return vector4.LengthSquared();
		}
		
		public static double getDistanceBetweenLines (Vector3d p1, Vector3d p2, Vector3d p3, Vector3d p4)
		{
			Vector3d vector1;
			Vector3d vector2;
			if (getLineLineIntersection(p1, p2, p3, p4, out vector1, out vector2))
			{
				return Vector3d.GetDistance3d(vector1, vector2);
			}
			return 0.00d;
		}
		
		public static bool getLineLineIntersection (Vector3d p1, Vector3d p2, Vector3d p3, Vector3d p4, out Vector3d pa, out Vector3d pb)
		{
			Vector3d vector4;
			pb = vector4 = noVector;
			pa = vector4;
			Vector3d vector1 = p1 - p3;
			Vector3d vector2 = p4 - p3;
			if (vector2.LengthSquared() < 0.00d)
			{
				return false;
			}
			Vector3d vector3 = p2 - p1;
			if (vector3.LengthSquared() < 0.00d)
			{
				return false;
			}
			double single1 = Vector3d.DotProduct(vector1, vector2);
			double single2 = Vector3d.DotProduct(vector2, vector3);
			double single3 = Vector3d.DotProduct(vector1, vector3);
			double single4 = Vector3d.DotProduct(vector2, vector2);
			double single5 = Vector3d.DotProduct(vector3, vector3);
			double value = (single5 * single4) - (single2 * single2);
			if (Math.Abs(value) < 0.00d)
			{
				return false;
			}
			double single8 = (single1 * single2) - (single3 * single4);
			double single6 = single8 / value;
			double single7 = (single1 + (single2 * single6)) / single4;
			pa.x += single6 * vector3.x;
			pa.y += single6 * vector3.y;
			pa.z += single6 * vector3.z;
			pb.x += single7 * vector2.x;
			pb.y += single7 * vector2.y;
			pb.z += single7 * vector2.z;
			return true;
		}
		
		public static bool getLineLineIntersection (Vector3d p1, Vector3d p2, Vector3d p3, Vector3d p4, out Vector3d intersection)
		{
			intersection = noVector;
			Vector3d vector1 = p2 - p1;
			Vector3d vector2 = p4 - p3;
			Vector3d vector3 = p3 - p1;
			Vector3d vector4 = p1 - p3;
			Vector3d vector5 = Vector3d.CrossProduct(vector1, vector2);
			double single1 = vector5.LengthSquared();
			if ((Math.Abs(Vector3d.DotProduct(vector4, Vector3d.CrossProduct(vector1, vector2))) < 0.00d) && (single1 > 0.00d))
			{
				intersection = p1 + ( (vector1 * (Vector3d.DotProduct(Vector3d.CrossProduct(vector3, vector2), Vector3d.CrossProduct(vector1, vector2)) / single1)));
				return true;
			}
			return false;
		}
		
		public static bool isPointCollinear (Vector3d point, Vector3d a, Vector3d b)
		{
			Vector3d vector1 = Vector3d.CrossProduct(a - point, point - b);
			return vector1.LengthSquared() < 0.00d;
		}
		
		public static bool areSegmentsParallel (Vector3d dirA, Vector3d dirB)
		{
			return getSquaredDistanceLinePoint(zeroVector, dirA, dirB) < 0.00d;
		}
		
		public static double getSquaredDistanceLinePoint (Vector3d a, Vector3d b, Vector3d p)
		{
			Vector3d vector1 = b - a;
			Vector3d vector2 = Vector3d.CrossProduct(vector1, a - p);
			return vector2.LengthSquared() / vector1.LengthSquared();
		}


        private static void getBoxFaceFaceContacts(ref Vector3d axisLocalA, ref Vector3d axisLocalB, ref Vector3d worldAxis, 
            BoxPrimitive a, BoxPrimitive b, List<Vector3d> manifold, List<int> ids)
		{
			List<Vector3d> face = ResourcePool.getVectorList();
			List<Vector3d> manifoldB = ResourcePool.getVectorList();
			List<int> faceVertexIds = ResourcePool.getIntList();
			List<int> bFaceVertexIds = ResourcePool.getIntList();
			getBoxFace(ref axisLocalA, a, face, faceVertexIds, 0);
			getBoxFace(ref axisLocalB, b, manifoldB, bFaceVertexIds, 8);
			Vector3d pointOnPlane = face[0] + manifoldB[0];
			pointOnPlane=pointOnPlane* 0.50F;
			for (int i = 0;i < 4; i++)
			{
				face[i] = getPointProjectedOnPlane(face[i], worldAxis, pointOnPlane);
				manifoldB[i] = getPointProjectedOnPlane(manifoldB[i], worldAxis, pointOnPlane);
			}
			List<bool> pointIsContained = ResourcePool.getBoolList();
			List<bool> list = ResourcePool.getBoolList();
			arePointsOfManifoldInManifold(face, manifoldB, pointIsContained);
			arePointsOfManifoldInManifold(manifoldB, face, list);
			for (int j = 0;j < 4; j++)
			{
				if (pointIsContained[j] != null)
				{
					manifold.Add(face[j]);
					ids.Add(faceVertexIds[j]);
				}
				else
				{
					if (list[j] != null)
					{
						manifold.Add(manifoldB[j]);
						ids.Add(bFaceVertexIds[j]);
					}
				}
			}
			if (manifold.Count < 4)
			{
				findManifoldIntersections(face, faceVertexIds, manifoldB, bFaceVertexIds, manifold, ids);
				pruneToMaximumSubmanifold(manifold, ids);
			}
			ResourcePool.giveBack(faceVertexIds);
			ResourcePool.giveBack(bFaceVertexIds);
			ResourcePool.giveBack(pointIsContained);
			ResourcePool.giveBack(list);
			ResourcePool.giveBack(face);
			ResourcePool.giveBack(manifoldB);
		}
		
		private static void findManifoldIntersections (List<Vector3d> aFaceVertices, List<int> aFaceVertexIds, List<Vector3d> bFaceVertices, List<int> bFaceVertexIds, List<Vector3d> manifold, List<int> ids)
		{
		    Vector3d vector = aFaceVertices[0] - aFaceVertices[1];
            Vector3d vector2 = aFaceVertices[2] - aFaceVertices[1];
            vector.Normalize();
            vector2.Normalize();
            Vector3d vector3 = aFaceVertices[1];
            Vector3d vector4 = aFaceVertices[3];
            vector4 = vector4 - vector3;
            double num = Vector3d.DotProduct(vector, vector4);
            double num2 = Vector3d.DotProduct(vector2, vector4);
            List<double> list = new List<double>( );  //ResourcePool.getFloatList();
            List<double> list2 = new List<double>( ); //ResourcePool.getFloatList();
            for (int i = 0; i < 4; i++)
            {
                vector4 = bFaceVertices[i];
                vector4 = vector4 - vector3;
                double num3 = Vector3d.DotProduct(vector, vector4);
                double num4 = Vector3d.DotProduct(vector2, vector4);
                list.Add(num3);
                list2.Add(num4);
            }
            double num6 = 0f;
            double num7 = 0f;
            double num8 = 0f;
            double num9 = 0f;
            for (int j = 0; j < 4; j++)
            {
                int num14 = (j + 1) % 4;
                switch (j)
                {
                    case 0:
                        num6 = 0f;
                        num7 = 0f;
                        num8 = num;
                        num9 = 0f;
                        break;

                    case 1:
                        num6 = 0f;
                        num7 = 0f;
                        num8 = 0f;
                        num9 = num2;
                        break;

                    case 2:
                        num6 = 0f;
                        num7 = num2;
                        num8 = num;
                        num9 = num2;
                        break;

                    case 3:
                        num6 = num;
                        num7 = 0f;
                        num8 = num;
                        num9 = num2;
                        break;
                }
                for (int k = 0; k < 4; k++)
                {
                    double num10;
                    double num11;
                    double num12;
                    double num13;
                    int num15 = (k + 1) % 4;
                    if (list[k] < list[num15])
                    {
                        num10 = list[k];
                    }
                    else
                    {
                        num10 = list[num15];
                    }
                    if (list[k] > list[num15])
                    {
                        num12 = list[k];
                    }
                    else
                    {
                        num12 = list[num15];
                    }
                    if (list2[k] < list2[num15])
                    {
                        num11 = list2[k];
                    }
                    else
                    {
                        num11 = list2[num15];
                    }
                    if (list2[k] > list2[num15])
                    {
                        num13 = list2[k];
                    }
                    else
                    {
                        num13 = list2[num15];
                    }
                    if ((((num12 - num6) >= 0f) && ((num8 - num10) >= 0f)) && (((num13 - num7) >= 0f) && ((num9 - num11) >= 0f)))
                    {
                        Vector3d vector6;
                        Vector3d vector7;
                        double num16 = getClosestPointsBetweenSegments(aFaceVertices[j], aFaceVertices[num14], bFaceVertices[k], bFaceVertices[num15], out vector7, out vector6);
                        Vector3d point = (vector6 + vector7) / 2d;
                        if ((num16 < 1E-05f) && !isSimilarPointPresent(manifold, ref point))
                        {
                            ids.Add((((aFaceVertexIds[j] * 3) + (aFaceVertexIds[num14] * 5)) * 0x29) + (((bFaceVertexIds[k] * 3) + (bFaceVertexIds[num15] * 5)) * 0x2b));
                            manifold.Add(point);
                        }
                    }
                }
            }
           // ResourcePool.giveBack(list);
           // ResourcePool.giveBack(list2);

		}
		
		private static bool isSimilarPointPresent (List<Vector3d> manifold, ref Vector3d point)
		{
			int num1 = manifold.Count;
			for (int i = 0;i < num1; i++)
			{
				Vector3d vector1 = manifold[i] - point;
				if (vector1.LengthSquared() < 0.01F)
				{
					return true;
				}
			}
			return false;
		}
		
		private static void arePointsOfManifoldInManifold (List<Vector3d> manifoldA, List<Vector3d> manifoldB, List<bool> pointIsContained)
		{
			Vector3d vector1 = manifoldB[1] - manifoldB[0];
			Vector3d vector2 = manifoldB[2] - manifoldB[1];
			Vector3d vector3 = manifoldB[0];
			double single1 = vector1.Length;
			double single2 = vector2.Length;
			vector1 =  (vector1 / single1);
			vector2 =  (vector2 / single2);
			single1 += 0.01F;
			single2 += 0.01F;
			for (int i = 0;i < 4; i++)
			{
			    Vector3d vector4 = manifoldA[i];
				vector4=vector4-vector3;
				double single3 = Vector3d.DotProduct(vector4,vector1);
				if ((single3 >= -0.01F) && (single3 <= single1))
				{
				    double single4 = Vector3d.DotProduct(vector4,vector2);
					if ((single4 >= -0.01F) && (single4 <= single2))
					{
						pointIsContained.Add(true);
						continue;
					}
					pointIsContained.Add(false);
					continue;
				}
                pointIsContained.Add(false);
			}
		}
		
		private static void pruneToMaximumSubmanifold (List<Vector3d> manifold, List<int> ids)
		{
			if (manifold.Count > 4)
			{
				Vector3d vector1 = manifold[0] - manifold[1];
				Vector3d vector2 = manifold[2] - manifold[1];
				double single1 = float.NegativeInfinity;
				double single2 = float.PositiveInfinity;
				double single4 = float.NegativeInfinity;
				double single5 = float.PositiveInfinity;
				int num1 = -1;
				int num2 = -1;
				int num3 = -1;
				int num4 = -1;
				for (int i = 0;i < manifold.Count; i++)
				{
					double single3 = Vector3d.DotProduct(manifold[i], vector1);
					if (single3 > single1)
					{
						single1 = single3;
						num2 = i;
					}
					if (single3 < single2)
					{
						single2 = single3;
						num1 = i;
					}
					single3 = Vector3d.DotProduct(manifold[i], vector2);
					if (single3 > single4)
					{
						single4 = single3;
						num4 = i;
					}
					if (single3 < single5)
					{
						single5 = single3;
						num3 = i;
					}
				}
				Vector3d item = manifold[num1];
				Vector3d vector4 = manifold[num2];
				Vector3d vector5 = manifold[num3];
				Vector3d vector6 = manifold[num4];
				Vector3d vector7 = item - vector4;
				double single6 = vector7.LengthSquared();
				Vector3d vector8 = item - vector5;
				double single7 = vector8.LengthSquared();
				Vector3d vector9 = item - vector6;
				double single8 = vector9.LengthSquared();
				manifold.Clear();
				manifold.Add(item);
				if (single6 > 0.01F)
				{
					Vector3d vector10 = vector4 - vector5;
					double single9 = vector10.LengthSquared();
					Vector3d vector11 = vector4 - vector6;
					double single10 = vector11.LengthSquared();
					manifold.Add(vector4);
					if ((single7 > 0.01F) && (single9 > 0.01d))
					{
						Vector3d vector12 = vector5 - vector6;
						double single11 = vector12.LengthSquared();
						manifold.Add(vector5);
						if (((single8 > 0.01F) && (single10 > 0.01d)) && (single11 > 0.01d))
						{
							manifold.Add(vector6);
						}
					}
					else
					{
						if ((single8 > 0.01d) && (single10 > 0.01d))
						{
							manifold.Add(vector6);
						}
					}
				}
				else if (single7 > 0.01F)
				{
					manifold.Add(vector5);
					Vector3d vector13 = vector5 - vector6;
					double single12 = vector13.LengthSquared();
					if ((single8 >= 0.01F) || (single12 >= 0.01F))
					{
						return;
					}
					manifold.Add(vector6);
				}
				else
				{
					if (single8 > 0.01F)
					{
						manifold.Add(vector6);
					}
				}
			}
		}
		
		private static void getBoxFace (ref Vector3d localDirection, BoxPrimitive box, List<Vector3d> face, List<int> faceVertexIds, int baseId)
		{
			double value = localDirection.x;
			double single2 = localDirection.y;
			double single3 = localDirection.z;
			double single4 = Math.Abs(value);
			double single5 = Math.Abs(single2);
			double single6 = Math.Abs(single3);

		    double halfWidth = box.BoundingBox.Width*.5d;
            double halfHeight = box.BoundingBox.Height * .5d;
            double halfDepth = box.BoundingBox.Depth * .5d;

			if ((single4 > single5) && (single4 > single6))
			{
				if (value > 0.00d)
				{
                    face.Add(new Vector3d(halfWidth, -halfHeight, -halfDepth));
                    face.Add(new Vector3d(halfWidth, -halfHeight, halfDepth));
                    face.Add(new Vector3d(halfWidth, halfHeight, halfDepth));
                    face.Add(new Vector3d(halfWidth, halfHeight, -halfDepth));
					faceVertexIds.Add(baseId + 4);
					faceVertexIds.Add(baseId + 5);
					faceVertexIds.Add(baseId + 7);
					faceVertexIds.Add(baseId + 6);
				}
				else
				{
                    face.Add(new Vector3d(-halfWidth, -halfHeight, -halfDepth));
                    face.Add(new Vector3d(-halfWidth, -halfHeight, halfDepth));
                    face.Add(new Vector3d(-halfWidth, halfHeight, halfDepth));
                    face.Add(new Vector3d(-halfWidth, halfHeight, -halfDepth));
					faceVertexIds.Add(baseId);
					faceVertexIds.Add(baseId + 1);
					faceVertexIds.Add(baseId + 3);
					faceVertexIds.Add(baseId + 2);
				}
			}
			else if ((single5 > single4) && (single5 > single6))
			{
				if (single2 > 0.00d)
				{
					face.Add(new Vector3d(-halfWidth, halfHeight, -halfDepth));
					face.Add(new Vector3d(-halfWidth, halfHeight, halfDepth));
					face.Add(new Vector3d(halfWidth, halfHeight, halfDepth));
					face.Add(new Vector3d(halfWidth, halfHeight, -halfDepth));
					faceVertexIds.Add(baseId + 2);
					faceVertexIds.Add(baseId + 3);
					faceVertexIds.Add(baseId + 7);
					faceVertexIds.Add(baseId + 6);
				}
				else
				{
					face.Add(new Vector3d(-halfWidth, -halfHeight, -halfDepth));
					face.Add(new Vector3d(-halfWidth, -halfHeight, halfDepth));
					face.Add(new Vector3d(halfWidth, -halfHeight, halfDepth));
					face.Add(new Vector3d(halfWidth, -halfHeight, -halfDepth));
					faceVertexIds.Add(baseId);
					faceVertexIds.Add(baseId + 1);
					faceVertexIds.Add(baseId + 5);
					faceVertexIds.Add(baseId + 4);
				}
			}
			else
			{
				if (single3 > 0.00d)
				{
					face.Add(new Vector3d(-halfWidth, -halfHeight, halfDepth));
					face.Add(new Vector3d(halfWidth, -halfHeight, halfDepth));
					face.Add(new Vector3d(halfWidth, halfHeight, halfDepth));
					face.Add(new Vector3d(-halfWidth, halfHeight, halfDepth));
					faceVertexIds.Add(baseId + 1);
					faceVertexIds.Add(baseId + 5);
					faceVertexIds.Add(baseId + 7);
					faceVertexIds.Add(baseId + 3);
				}
				else
				{
					face.Add(new Vector3d(-halfWidth, -halfHeight, -halfDepth));
					face.Add(new Vector3d(halfWidth, -halfHeight, -halfDepth));
					face.Add(new Vector3d(halfWidth, halfHeight, -halfDepth));
					face.Add(new Vector3d(-halfWidth, halfHeight, -halfDepth));
					faceVertexIds.Add(baseId);
					faceVertexIds.Add(baseId + 4);
					faceVertexIds.Add(baseId + 6);
					faceVertexIds.Add(baseId + 2);
				}
			}
			for (int i = 0;i < 4; i++)
			{
				face[i] = Vector3d.TransformCoord(face[i], box.Body.myInternalOrientationMatrix) + box.CenterPosition;
			}
		}
		
		private static void getBoxFaceEdgeContacts (ref Vector3d axisLocalA, ref Vector3d axisLocalB, ref Vector3d worldAxis, byte edgeDir, BoxPrimitive a, BoxPrimitive b, int aIdOffset, int bIdOffset, List<Vector3d> manifold, List<int> ids)
		{
			Vector3d point;
			Vector3d q2;
			int item;
			int num2;
			bool flag1;
			bool flag2;
			List<Vector3d> face = ResourcePool.getVectorList();
			List<int> faceVertexIds = ResourcePool.getIntList();
			getBoxFace(ref axisLocalA, a, face, faceVertexIds, aIdOffset);
			getBoxEdge(ref axisLocalB, b, edgeDir, out point, out q2, out item, out num2, bIdOffset);
			Vector3d pointOnPlane =  ((face[0] + point) / 2.00F);
			for (int i = 0;i < 4; i++)
			{
				face[i] = getPointProjectedOnPlane(face[i], worldAxis, pointOnPlane);
			}
			point = getPointProjectedOnPlane(point, worldAxis, pointOnPlane);
			q2 = getPointProjectedOnPlane(q2, worldAxis, pointOnPlane);
			areEdgePointsInManifold(ref point, ref q2, face, out flag1, out flag2);
			if (flag1)
			{
				manifold.Add(point);
				ids.Add(item);
			}
			if (flag2)
			{
				manifold.Add(q2);
				ids.Add(num2);
			}
			if (manifold.Count < 2)
			{
				for (int j = 0;j < 4; j++)
				{
					Vector3d vector4;
					Vector3d vector5;
					double single1 = getClosestPointsBetweenSegments(face[j], face[(j + 1) % 4], point, q2, out vector4, out vector5);
					if (single1 < 0.00d)
					{
						manifold.Add((vector4 + vector5) / 2.00d);
						if (aIdOffset == 0)
						{
							ids.Add((((faceVertexIds[j] * 3) + (faceVertexIds[((j + 1) % 4)] * 5)) * 0x29) + (((item * 3) + (num2 * 5)) * 0x2b));
						}
						else
						{
							ids.Add((((item * 3) + (num2 * 5)) * 0x29) + (((faceVertexIds[j] * 3) + (faceVertexIds[((j + 1) % 4)] * 5)) * 0x2b));
						}
						if (manifold.Count == 2)
						{
							break;
						}
					}
				}
			}
			if (manifold.Count == 2)
			{
				Vector3d vector6 = manifold[0] - manifold[1];
				if (vector6.LengthSquared() < 0.01F)
				{
					manifold.RemoveAt(1);
					ids.RemoveAt(1);
				}
			}
			ResourcePool.giveBack(faceVertexIds);
			ResourcePool.giveBack(face);
		}
		
		private static void areEdgePointsInManifold (ref Vector3d v1, ref Vector3d v2, List<Vector3d> manifold, out bool v1IsContained, out bool v2IsContained)
		{
		    double single4;
		    Vector3d vector1 = manifold[1] - manifold[0];
			Vector3d vector2 = manifold[2] - manifold[1];
			Vector3d vector3 = manifold[0];
			double single1 = vector1.Length;
			double single2 = vector2.Length;
			vector1 =  (vector1 / single1);
			vector2 =  (vector2 / single2);
			single1 += 0.01F;
			single2 += 0.01F;
			Vector3d vector4 = v1-vector3;
			double single3 = Vector3d.DotProduct(vector4,vector1);
			if ((single3 >= 0.01F) && (single3 <= single1))
			{
				single4=Vector3d.DotProduct(vector4,vector2);
				if ((single4 >= -0.01F) && (single4 <= single2))
				{
					v1IsContained = true;
				}
				else
				{
					v1IsContained = false;
				}
			}
			else
			{
				v1IsContained = false;
			}
			vector4=v2-vector3;
			single3=Vector3d.DotProduct(vector4,vector1);
			if ((single3 >= -0.01F) && (single3 <= single1))
			{
				single4=Vector3d.DotProduct(vector4,vector2);
				if ((single4 >= -0.01F) && (single4 <= single2))
				{
					v2IsContained = true;
				}
				else
				{
					v2IsContained = false;
				}
			}
			else
			{
				v2IsContained = false;
			}
		}
		
		private static void getBoxEdge (ref Vector3d localDirection, BoxPrimitive box, int edgeDir, out Vector3d a, out Vector3d b, out int idA, out int idB, int baseId)
		{
            a = zeroVector;
            b = zeroVector;
            int i1 = Math.Sign(localDirection.x);
            int i2 = Math.Sign(localDirection.y);
            int i3 = Math.Sign(localDirection.z);
            idA = -1;
            idB = -1;

            switch (edgeDir)
            {
                case 0:
                    a += (float)i2 * box.HalfHeight * upVector + (float)i3 * box.HalfLength * backVector + rightVector * box.HalfWidth;
                    b = a + Toolbox.leftVector * (2.0F * box.HalfWidth);
                    if ((i2 > 0) && (i3 > 0))
                    {
                        idA = baseId + 3;
                        idB = baseId + 7;
                        goto label_1;
                    }
                    if ((i2 < 0) && (i3 > 0))
                    {
                        idA = baseId + 1;
                        idB = baseId + 5;
                        goto label_1;
                    }
                    if ((i2 > 0) && (i3 < 0))
                    {
                        idA = baseId + 2;
                        idB = baseId + 6;
                        goto label_1;
                    }
                    idA = baseId;
                    idB = baseId + 4;
                    break;

                case 1:
                    a += rightVector * ((float)i1 * box.HalfWidth) + backVector * ((float)i3 * box.HalfLength) + upVector * box.HalfHeight;
                    b = a + downVector * (2.0F * box.HalfHeight);
                    if ((i1 > 0) && (i3 > 0))
                    {
                        idA = baseId + 5;
                        idB = baseId + 7;
                    }
                    else if ((i1 < 0) && (i3 > 0))
                    {
                        idA = baseId + 1;
                        idB = baseId + 3;
                    }
                    else if ((i1 > 0) && (i3 < 0))
                    {
                        idA = baseId + 4;
                        idB = baseId + 6;
                    }
                    else
                    {
                        idA = baseId;
                        idB = baseId + 2;
                    }
                    break;

                case 2:
                    a += upVector * (box.HalfHeight * (float)i2) + rightVector * (box.HalfWidth * (float)i1) + backVector * box.HalfLength;
                    b = a + forwardVector * (2.0F * box.HalfLength);
                    if ((i1 > 0) && (i2 > 0))
                    {
                        idA = baseId + 6;
                        idB = baseId + 7;
                    }
                    else if ((i1 < 0) && (i2 > 0))
                    {
                        idA = baseId + 2;
                        idB = baseId + 3;
                    }
                    else if ((i1 > 0) && (i2 < 0))
                    {
                        idA = baseId + 4;
                        idB = baseId + 5;
                    }
                    else
                    {
                        idA = baseId;
                        idB = baseId + 1;
                    }
                    break;
            }
        label_1:
            a = Vector3d.TransformCoord(a, box.Body.myInternalOrientationMatrix);
            a+= box.CenterPosition;
            b = Vector3d.TransformCoord(b, box.Body.myInternalOrientationMatrix);
            b+= box.CenterPosition;
		}

        private static void getBoxFaceVertexContact(ref Vector3d axisLocalA, ref Vector3d axisLocalB, ref Vector3d worldAxis, BoxPrimitive a, BoxPrimitive b, int aIdOffset, int bIdOffset, List<Vector3d> manifold, List<int> ids)
		{
			Vector3d point;
			int item;
			List<Vector3d> face = ResourcePool.getVectorList();
			List<int> faceVertexIds = ResourcePool.getIntList();
			getBoxFace(ref axisLocalA, a, face, faceVertexIds, aIdOffset);
			getBoxVertex(ref axisLocalB, b, out point, out item, bIdOffset);
			Vector3d pointOnPlane =  ((face[0] + point) / 2.00F);
			for (int i = 0;i < 4; i++)
			{
				face[i] = getPointProjectedOnPlane(face[i], worldAxis, pointOnPlane);
			}
			point = getPointProjectedOnPlane(point, worldAxis, pointOnPlane);
			if (isPointInManifold(ref point, face))
			{
				manifold.Add(point);
				ids.Add(item);
			}
			ResourcePool.giveBack(faceVertexIds);
			ResourcePool.giveBack(face);
		}
		
		private static bool isPointInManifold (ref Vector3d p, List<Vector3d> manifold)
		{
		    Vector3d vector1 = manifold[1] - manifold[0];
			Vector3d vector2 = manifold[2] - manifold[1];
			Vector3d vector3 = manifold[0];
			double single1 = vector1.Length;
			double single2 = vector2.Length;
			vector1 =  (vector1 / single1);
			vector2 =  (vector2 / single2);
			Vector3d vector4 = p-vector3;
			double single3 = Vector3d.DotProduct(vector4,vector1);
			if ((single3 >= -0.01F) && (single3 <= (single1 + 0.01F)))
			{
			    double single4 = Vector3d.DotProduct(vector4,vector2);
			    if ((single4 >= -0.01F) && (single4 <= (single2 + 0.01F)))
				{
					return true;
				}
			}
		    return false;
		}
		
		private static void getBoxVertex (ref Vector3d localDirection, BoxPrimitive box, out Vector3d vertex, out int vId, int baseId)
		{
			int num1 = Math.Sign(localDirection.x);
			int num2 = Math.Sign(localDirection.y);
			int num3 = Math.Sign(localDirection.z);
			if (((num1 < 0) && (num2 < 0)) && (num3 < 0))
			{
				vId = baseId;
			}
			else if (((num1 < 0) && (num2 < 0)) && (num3 > 0))
			{
				vId = baseId + 1;
			}
			else if (((num1 < 0) && (num2 > 0)) && (num3 < 0))
			{
				vId = baseId + 2;
			}
			else if (((num1 < 0) && (num2 > 0)) && (num3 > 0))
			{
				vId = baseId + 3;
			}
			else if (((num1 > 0) && (num2 < 0)) && (num3 < 0))
			{
				vId = baseId + 4;
			}
			else if (((num1 > 0) && (num2 < 0)) && (num3 > 0))
			{
				vId = baseId + 5;
			}
			else if (((num1 > 0) && (num2 > 0)) && (num3 < 0))
			{
				vId = baseId + 6;
			}
			else
			{
				vId = baseId + 7;
			}
            vertex = new Vector3d(num1 * box.HalfWidth, num2 * box.HalfHeight, num3 * box.HalfLength);
            vertex = Vector3d.TransformCoord(vertex, box.Body.myInternalOrientationMatrix);
            vertex = vertex + box.Body.myInternalCenterPosition;
		}
		
		private static void sortLines (List<Vector3d> lines, Vector3d origin)
		{
			for (int i = 0;i < lines.Count; i++)
			{
				lines[i] = lines[i] - origin;
			}
			lines.Sort(new Comparison<Vector3d>(compareVectorLengths));
			for (int j = 0;j < lines.Count; j++)
			{
				lines[j] = lines[j] + origin;
			}
		}
		
		internal static int compareVectorLengths (Vector3d v1, Vector3d v2)
		{
			double single1 = v1.LengthSquared();
			double single2 = v2.LengthSquared();
			if (single1 < single2)
			{
				return -1;
			}
			return 1;
		}
		
		public static bool arePointsOnOppositeSidesOfPlane (ref Vector3d o, ref Vector3d p, ref Vector3d a, ref Vector3d b, ref Vector3d c)
		{
		    Vector3d vector1 = b-a;
			Vector3d vector2 = c-a;
			Vector3d vector3 = p-a;
			Vector3d vector4 = o-a;
			Vector3d vector5 = Vector3d.CrossProduct(vector1,vector2);
			double single1 = Vector3d.DotProduct(vector3,vector5);
			double single2 = Vector3d.DotProduct(vector4,vector5);
			if ((single1 * single2) <= 0.00d)
			{
				return true;
			}
			return false;
		}
		
		public static bool isPointWithinFaceExtrusion (Vector3d point, List<Plane> planes, Vector3d centroid)
		{
			foreach (Plane plane1 in planes)
			{
                double single2 = plane1.DistanceToCoordinate(centroid);
                double single1 = plane1.DistanceToCoordinate(point);
				if (((single2 > 0.00d) || (single1 > 0.00d)) && ((single2 < 0.00d) || (single1 < 0.00d)))
				{
					return false;
				}
			}
			return true;
		}
		
		public static bool isPointWithinFaceExtrusion (Vector3d point, List<Plane> planes, Vector3d centroid, out List<Plane> separatingPlanes)
		{
			separatingPlanes = new List<Plane>();
			bool flag1 = false;
			foreach (Plane item in planes)
			{
                double single2 = item.DistanceToCoordinate(centroid);
                double single1 = item.DistanceToCoordinate(point);
				if (((single2 > 0.00d) || (single1 > 0.00d)) && ((single2 < 0.00d) || (single1 < 0.00d)))
				{
					flag1 = true;
					separatingPlanes.Add(item);
				}
			}
			if (!flag1)
			{
				return true;
			}
			return false;
		}
		
		public static Vector3d getPointProjectedOnPlane (Vector3d point, Vector3d normal, Vector3d pointOnPlane)
		{
		    double single1 = Vector3d.DotProduct(normal,point);
			double single2 = Vector3d.DotProduct(pointOnPlane,normal);
			double single3 = (single1 - single2) / normal.LengthSquared();
			Vector3d vector2 = normal* single3;
			Vector3d vector1 = point-vector2;
			return vector1;
		}
		
		public static double getDistancePointToPlane (Vector3d point, Vector3d normal, Vector3d pointOnPlane)
		{
			return (Vector3d.DotProduct(normal, point) - Vector3d.DotProduct(pointOnPlane, normal)) / normal.LengthSquared();
		}
		
		public static void getClosestPointOnTetrahedronToPoint (ref Vector3d a, ref Vector3d b, ref Vector3d c, ref Vector3d d, ref Vector3d p, out Vector3d closestPoint)
		{
			Vector3d vector1;
			Vector3d vector2;
			closestPoint = p;
			double single1 = float.MaxValue;
			if (arePointsOnOppositeSidesOfPlane(ref p, ref d, ref a, ref b, ref c))
			{
				getClosestPointOnTriangleToPoint(ref a, ref b, ref c, ref p, out vector2);
				vector1=vector2-p;
				double single2 = ((vector1.x * vector1.x) + (vector1.y * vector1.y)) + (vector1.z * vector1.z);
				if (single2 < single1)
				{
					single1 = single2;
					closestPoint = vector2;
				}
			}
			if (arePointsOnOppositeSidesOfPlane(ref p, ref b, ref a, ref c, ref d))
			{
				getClosestPointOnTriangleToPoint(ref a, ref c, ref d, ref p, out vector2);
				vector1=vector2-p;
				double single3 = ((vector1.x * vector1.x) + (vector1.y * vector1.y)) + (vector1.z * vector1.z);
				if (single3 < single1)
				{
					single1 = single3;
					closestPoint = vector2;
				}
			}
			if (arePointsOnOppositeSidesOfPlane(ref p, ref c, ref a, ref d, ref b))
			{
				getClosestPointOnTriangleToPoint(ref a, ref d, ref b, ref p, out vector2);
				vector1=vector2-p;
				double single4 = ((vector1.x * vector1.x) + (vector1.y * vector1.y)) + (vector1.z * vector1.z);
				if (single4 < single1)
				{
					single1 = single4;
					closestPoint = vector2;
				}
			}
			if (arePointsOnOppositeSidesOfPlane(ref p, ref a, ref b, ref d, ref c))
			{
				getClosestPointOnTriangleToPoint(ref b, ref d, ref c, ref p, out vector2);
				vector1=vector2-p;
				double single5 = ((vector1.x * vector1.x) + (vector1.y * vector1.y)) + (vector1.z * vector1.z);
				if (single5 < single1)
				{
					single1 = single5;
					closestPoint = vector2;
				}
			}
		}
		
		public static void getClosestPointOnTetrahedronToPoint (ref Vector3d a, ref Vector3d b, ref Vector3d c, ref Vector3d d, ref Vector3d p, List<Vector3d> subsimplex, out Vector3d closestPoint)
		{
			Vector3d vector1;
			Vector3d vector2;
			subsimplex.Clear();
			subsimplex.Add(a);
			subsimplex.Add(b);
			subsimplex.Add(c);
			subsimplex.Add(d);
			closestPoint = p;
			double single1 = float.MaxValue;
			if (arePointsOnOppositeSidesOfPlane(ref p, ref d, ref a, ref b, ref c))
			{
				getClosestPointOnTriangleToPoint(ref a, ref b, ref c, ref p, subsimplex, out vector2);
				vector1=vector2-p;
				double single2 = ((vector1.x * vector1.x) + (vector1.y * vector1.y)) + (vector1.z * vector1.z);
				if (single2 < single1)
				{
					single1 = single2;
					closestPoint = vector2;
				}
			}
			if (arePointsOnOppositeSidesOfPlane(ref p, ref b, ref a, ref c, ref d))
			{
				getClosestPointOnTriangleToPoint(ref a, ref c, ref d, ref p, subsimplex, out vector2);
				vector1=vector2-p;
				double single3 = ((vector1.x * vector1.x) + (vector1.y * vector1.y)) + (vector1.z * vector1.z);
				if (single3 < single1)
				{
					single1 = single3;
					closestPoint = vector2;
				}
			}
			if (arePointsOnOppositeSidesOfPlane(ref p, ref c, ref a, ref d, ref b))
			{
				getClosestPointOnTriangleToPoint(ref a, ref d, ref b, ref p, subsimplex, out vector2);
				vector1=vector2-p;
				double single4 = ((vector1.x * vector1.x) + (vector1.y * vector1.y)) + (vector1.z * vector1.z);
				if (single4 < single1)
				{
					single1 = single4;
					closestPoint = vector2;
				}
			}
			if (arePointsOnOppositeSidesOfPlane(ref p, ref a, ref b, ref d, ref c))
			{
				getClosestPointOnTriangleToPoint(ref b, ref d, ref c, ref p, subsimplex, out vector2);
				vector1=vector2-p;
				double single5 = ((vector1.x * vector1.x) + (vector1.y * vector1.y)) + (vector1.z * vector1.z);
				if (single5 < single1)
				{
					single1 = single5;
					closestPoint = vector2;
				}
			}
		}
		
		public static void getClosestPointOnTetrahedronToPoint (List<Vector3d> tetrahedron, ref Vector3d p, List<int> subsimplex, List<double> baryCoords, out Vector3d closestPoint)
		{
			Vector3d vector5;
			Vector3d vector6;
			List<int> list = ResourcePool.getIntList();
		    List<double> list2 = new List<double>(); // ResourcePool.getFloatList();
			Vector3d vector1 = tetrahedron[0];
			Vector3d vector2 = tetrahedron[1];
			Vector3d vector3 = tetrahedron[2];
			Vector3d vector4 = tetrahedron[3];
			closestPoint = p;
			double single1 = float.MaxValue;
			subsimplex.Clear();
			subsimplex.Add(0);
			subsimplex.Add(1);
			subsimplex.Add(2);
			subsimplex.Add(3);
			baryCoords.Clear();
			bool flag1 = false;
			if (arePointsOnOppositeSidesOfPlane(ref p, ref vector4, ref vector1, ref vector2, ref vector3))
			{
				getClosestPointOnTriangleToPoint(tetrahedron, 0, 1, 2, ref p, list, list2, out vector6);
				vector5=vector6-p;
				double single2 = vector5.LengthSquared();
				if (single2 < single1)
				{
					single1 = single2;
					closestPoint = vector6;
					subsimplex.Clear();
					baryCoords.Clear();
					for (int i = 0;i < list.Count; i++)
					{
						subsimplex.Add(list[i]);
						baryCoords.Add(list2[i]);
					}
					flag1 = true;
				}
			}
			if (arePointsOnOppositeSidesOfPlane(ref p, ref vector2, ref vector1, ref vector3, ref vector4))
			{
				getClosestPointOnTriangleToPoint(tetrahedron, 0, 2, 3, ref p, list, list2, out vector6);
				vector5=vector6-p;
				double single3 = vector5.LengthSquared();
				if (single3 < single1)
				{
					single1 = single3;
					closestPoint = vector6;
					subsimplex.Clear();
					baryCoords.Clear();
					for (int j = 0;j < list.Count; j++)
					{
						subsimplex.Add(list[j]);
						baryCoords.Add(list2[j]);
					}
					flag1 = true;
				}
			}
			if (arePointsOnOppositeSidesOfPlane(ref p, ref vector3, ref vector1, ref vector4, ref vector2))
			{
				getClosestPointOnTriangleToPoint(tetrahedron, 0, 3, 1, ref p, list, list2, out vector6);
				vector5=vector6-p;
				double single4 = vector5.LengthSquared();
				if (single4 < single1)
				{
					single1 = single4;
					closestPoint = vector6;
					subsimplex.Clear();
					baryCoords.Clear();
					for (int z = 0;z < list.Count; z++)
					{
						subsimplex.Add(list[z]);
						baryCoords.Add(list2[z]);
					}
					flag1 = true;
				}
			}
			if (arePointsOnOppositeSidesOfPlane(ref p, ref vector1, ref vector2, ref vector4, ref vector3))
			{
				getClosestPointOnTriangleToPoint(tetrahedron, 1, 3, 2, ref p, list, list2, out vector6);
				vector5=vector6-p;
				double single5 = vector5.LengthSquared();
				if (single5 < single1)
				{
					single1 = single5;
					closestPoint = vector6;
					subsimplex.Clear();
					baryCoords.Clear();
					for (int k = 0;k < list.Count; k++)
					{
						subsimplex.Add(list[k]);
						baryCoords.Add(list2[k]);
					}
					flag1 = true;
				}
			}
			if (!flag1)
			{
				double single6 = new Matrix(tetrahedron[0].x, tetrahedron[0].y, tetrahedron[0].z, 1.00F, tetrahedron[1].x, tetrahedron[1].y, tetrahedron[1].z, 1.00F, tetrahedron[2].x, tetrahedron[2].y, tetrahedron[2].z, 1.00F, tetrahedron[3].x, tetrahedron[3].y, tetrahedron[3].z, 1.00F).Determinant;
				double single7 = new Matrix(p.x, p.y, p.z, 1.00F, tetrahedron[1].x, tetrahedron[1].y, tetrahedron[1].z, 1.00F, tetrahedron[2].x, tetrahedron[2].y, tetrahedron[2].z, 1.00F, tetrahedron[3].x, tetrahedron[3].y, tetrahedron[3].z, 1.00F).Determinant;
				double single8 = new Matrix(tetrahedron[0].x, tetrahedron[0].y, tetrahedron[0].z, 1.00F, p.x, p.y, p.z, 1.00F, tetrahedron[2].x, tetrahedron[2].y, tetrahedron[2].z, 1.00F, tetrahedron[3].x, tetrahedron[3].y, tetrahedron[3].z, 1.00F).Determinant;
				double single9 = new Matrix(tetrahedron[0].x, tetrahedron[0].y, tetrahedron[0].z, 1.00F, tetrahedron[1].x, tetrahedron[1].y, tetrahedron[1].z, 1.00F, p.x, p.y, p.z, 1.00F, tetrahedron[3].x, tetrahedron[3].y, tetrahedron[3].z, 1.00F).Determinant;
				single6 = 1.00F / single6;
				baryCoords.Add(single7 * single6);
				baryCoords.Add(single8 * single6);
				baryCoords.Add(single9 * single6);
				baryCoords.Add(((1.00F - baryCoords[0]) - baryCoords[1]) - baryCoords[2]);
			}
			ResourcePool.giveBack(list);
			//ResourcePool.giveBack(list2);
		}
		
		public static double getDistanceBetweenObjects (PhysicsBody objA, PhysicsBody objB, double marginA, double marginB)
		{
			List<Vector3d> q = ResourcePool.getVectorList();
			List<Vector3d> subsimplex = ResourcePool.getVectorList();
            q.Add(objA.myInternalCenterPosition - objB.myInternalCenterPosition);
			while (true)
			{
				Vector3d vector1;
				findPointOfMinimumNorm(q, subsimplex, out vector1);
				if (vector1 == zeroVector)
				{
					ResourcePool.giveBack(q);
					ResourcePool.giveBack(subsimplex);
					return 0.00d;
				}
				q.Clear();
				foreach (Vector3d item in subsimplex)
				{
					q.Add(item);
				}
				Vector3d vector2 = findMinkowskiDifferenceExtremePoint(objA, objB, -(vector1), marginA, marginB);
				if (Vector3d.DotProduct(vector2 - vector1, -(vector1)) <= 0.00d)
				{
					ResourcePool.giveBack(q);
					ResourcePool.giveBack(subsimplex);
					return vector1.Length;
				}
				q.Add(vector2);
			}
		}
		
		public static double getDistanceBetweenObjects (PhysicsBody objA, PhysicsBody objB, double marginA, double marginB, out Vector3d separatingDirection)
		{
            return getDistanceBetweenObjects(objA, objB, ref objA.CollisionPrimitive.CenterPosition, ref objB.CollisionPrimitive.CenterPosition, ref objA.myInternalOrientationQuaternion, ref objB.myInternalOrientationQuaternion, marginA, marginB, out separatingDirection);
		}
		
		public static double getDistanceBetweenObjects (PhysicsBody objA, PhysicsBody objB, ref Vector3d positionA, ref Vector3d positionB, ref Quaternion orientationA, ref Quaternion orientationB, double marginA, double marginB, out Vector3d separatingDirection)
		{
			List<Vector3d> q = ResourcePool.getVectorList();
			List<Vector3d> subsimplex = ResourcePool.getVectorList();
			q.Add(positionA - positionB);
			double single1 = 0.00d;
			double single2 = 0.00d;
			int num1 = 0;
			while (true)
			{
				Vector3d vector1;
				findPointOfMinimumNorm(q, subsimplex, out vector1);
				if ((vector1 == zeroVector) || (num1 > 0x32))
				{
					ResourcePool.giveBack(q);
					ResourcePool.giveBack(subsimplex);
					separatingDirection = noVector;
					return 0.00d;
				}
				q.Clear();
				foreach (Vector3d item in subsimplex)
				{
					q.Add(item);
				}
				Vector3d vector4 = -(vector1);
				Vector3d vector2 = findMinkowskiDifferenceExtremePoint(objA, objB, ref vector4, ref positionA, ref positionB, ref orientationA, ref orientationB, marginA, marginB);
				if (Vector3d.DotProduct(vector2 + vector4, vector4) <= (0.00d * single1))
				{
					ResourcePool.giveBack(q);
					ResourcePool.giveBack(subsimplex);
					double single3 = vector1.Length;
					separatingDirection =  (vector1 / single3);
					return single3;
				}
				q.Add(vector2);
                single1 = double.NegativeInfinity;
				foreach (Vector3d vector5 in q)
				{
					single2 = vector5.LengthSquared();
					if (single2 > single1)
					{
						single1 = single2;
					}
				}
				num1++;
			}
		}
		
		public static Vector3d getClosestPointsBetweenObjects (PhysicsBody objA, PhysicsBody objB, double marginA, double marginB, out Vector3d closestA, out Vector3d closestB)
		{
            return getClosestPointsBetweenObjects(objA, objB, ref objA.CollisionPrimitive.CenterPosition, ref objB.CollisionPrimitive.CenterPosition, ref objA.myInternalOrientationQuaternion, ref objB.myInternalOrientationQuaternion, marginA, marginB, out closestA, out closestB);
		}
		
		public static Vector3d getClosestPointsBetweenObjects (PhysicsBody objA, PhysicsBody objB, ref Vector3d positionA, ref Vector3d positionB, ref Quaternion orientationA, ref Quaternion orientationB, double marginA, double marginB, out Vector3d closestA, out Vector3d closestB)
		{
		    closestA = noVector;
			closestB = noVector;
			List<Vector3d> list = ResourcePool.getVectorList();
			List<Vector3d> list2 = ResourcePool.getVectorList();
			List<Vector3d> list3 = ResourcePool.getVectorList();
			List<Vector3d> q = ResourcePool.getVectorList();
			List<Vector3d> list5 = ResourcePool.getVectorList();
			List<Vector3d> list6 = ResourcePool.getVectorList();
			List<int> subsimplex = ResourcePool.getIntList();
            List<double> baryCoords = new List<double>();
			Vector3d vector3 = zeroVector;
            double single2 = double.PositiveInfinity;
			list5.Add(objA.myInternalCenterPosition);
			list6.Add(objB.myInternalCenterPosition);
			Vector3d item = objA.myInternalCenterPosition-objB.myInternalCenterPosition;
			q.Add(item);
			int num1 = 0;
            double single3 = double.NegativeInfinity;
			while (true)
			{
				Vector3d vector1;
				Vector3d vector2;
			    findPointOfMinimumNorm(q, subsimplex, baryCoords, out vector3);
				if (vector3.LengthSquared() < 0.00d)
				{
					ResourcePool.giveBack(list);
					ResourcePool.giveBack(list2);
					ResourcePool.giveBack(list3);
					ResourcePool.giveBack(q);
					ResourcePool.giveBack(list5);
					ResourcePool.giveBack(list6);
					ResourcePool.giveBack(subsimplex);
					return zeroVector;
				}
				list.Clear();
				list2.Clear();
				list3.Clear();
				foreach (int num2 in subsimplex)
				{
					list.Add(q[num2]);
					list2.Add(list5[num2]);
					list3.Add(list6[num2]);
				}
				q.Clear();
				list5.Clear();
				list6.Clear();
				for (int i = 0;i < list.Count; i++)
				{
					q.Add(list[i]);
					list5.Add(list2[i]);
					list6.Add(list3[i]);
				}
                Vector3d vector6 = Vector3d.Negate(vector3);
				objA.CollisionPrimitive.getExtremePoint(ref vector6, ref positionA, ref orientationA, marginA, out vector1);
                objB.CollisionPrimitive.getExtremePoint(ref vector3, ref positionB, ref orientationB, marginB, out vector2);
				item=vector1-vector2;
				Vector3d vector5 = item+vector6;
				double single1 = Vector3d.DotProduct(vector5,vector6);
				if (((single1 <= (0.00d * single3)) || ((num1 > 0x14) && (single1 <= single2))) || (num1 > 0x1e))
				{
					getBarycenter(list5, baryCoords, out closestA);
					getBarycenter(list6, baryCoords, out closestB);
					ResourcePool.giveBack(list);
					ResourcePool.giveBack(list2);
					ResourcePool.giveBack(list3);
					ResourcePool.giveBack(q);
					ResourcePool.giveBack(list5);
					ResourcePool.giveBack(list6);
					ResourcePool.giveBack(subsimplex);
					return vector3;
				}
				single2 = single1;
				q.Add(item);
				list5.Add(vector1);
				list6.Add(vector2);
				num1++;
				single3 = float.NegativeInfinity;
				foreach (Vector3d vector7 in q)
				{
					double single4 = vector7.LengthSquared();
					if (single4 > single3)
					{
						single3 = single4;
					}
				}
			}
		}
		
		public static bool areObjectsColliding (PhysicsBody objA, PhysicsBody objB, double marginA, double marginB, ref Vector3d separatingAxis)
		{
			Vector3d vector1;
			if (separatingAxis == zeroVector)
			{
				vector1 = upVector;
			}
			else
			{
				vector1 = separatingAxis;
			}
			List<int> list = ResourcePool.getIntList();
			List<double> baryCoords = new List<double>( );// ResourcePool.getFloatList();
			List<Vector3d> list3 = ResourcePool.getVectorList();
			List<Vector3d> q = ResourcePool.getVectorList();
			double single1 = 1.00F;
			double single2 = 0.00d;
			int num1 = 0;
			do
			{
				Vector3d item;
			    Vector3d vector4;
                Vector3d vector3 = Vector3d.Negate(vector1);
				findMinkowskiDifferenceExtremePoint(objA, objB, ref vector3, marginA, marginB, out item);
				double single3 = Vector3d.DotProduct(vector1,item);
				if (single3 > (0.00d * single1))
				{
					separatingAxis = vector1;
					ResourcePool.giveBack(list);
					ResourcePool.giveBack(list3);
					ResourcePool.giveBack(q);
					return false;
				}
				findPointClosestToSimplex(q, ref item, out vector4);
				if (item == vector4)
				{
					separatingAxis = vector1;
					ResourcePool.giveBack(list);
					ResourcePool.giveBack(list3);
					ResourcePool.giveBack(q);
					return false;
				}
				q.Clear();
				list3.Add(item);
				for (int i = 0;i < list3.Count; i++)
				{
					q.Add(list3[i]);
				}
				findPointOfMinimumNorm(q, list, baryCoords, out vector1);
				q.Clear();
				for (int j = 0;j < list.Count; j++)
				{
					q.Add(list3[list[j]]);
				}
				list3.Clear();
				foreach (Vector3d vector5 in q)
				{
					list3.Add(vector5);
				}
				single1 = double.NegativeInfinity;
				foreach (Vector3d vector6 in list3)
				{
					single2 = vector6.LengthSquared();
					if (single2 > single1)
					{
						single1 = single2;
					}
				}
				num1++;
			}
			while (((list3.Count != 4) && (vector1.LengthSquared() > (0.00d * single1))) && (num1 <= 0x32));
			ResourcePool.giveBack(list);
			ResourcePool.giveBack(list3);
			ResourcePool.giveBack(q);
			return true;
		}
		
		public static bool areObjectsColliding (PhysicsBody objA, PhysicsBody objB, double marginA, double marginB)
		{
		    Vector3d vector1 = objA.myInternalCenterPosition-objB.myInternalCenterPosition;
			List<int> list = ResourcePool.getIntList();
            List<double> baryCoords = new List<double>();
			List<Vector3d> list3 = ResourcePool.getVectorList();
			List<Vector3d> q = ResourcePool.getVectorList();
			double single1 = 1.00F;
			double single2 = 0.00d;
			int num1 = 0;
			do
			{
				Vector3d item;
			    Vector3d vector4;
				Vector3d vector3 = Vector3d.Negate(vector1);
				findMinkowskiDifferenceExtremePoint(objA, objB, ref vector3, marginA, marginB, out item);
				double single3 = Vector3d.DotProduct(vector1,item);
				if (single3 > (0.00d * single1))
				{
					ResourcePool.giveBack(list);
					ResourcePool.giveBack(list3);
					ResourcePool.giveBack(q);
					return false;
				}
				findPointClosestToSimplex(q, ref item, out vector4);
				if (item == vector4)
				{
					ResourcePool.giveBack(list);
					ResourcePool.giveBack(list3);
					ResourcePool.giveBack(q);
					return false;
				}
				q.Clear();
				list3.Add(item);
				for (int i = 0;i < list3.Count; i++)
				{
					q.Add(list3[i]);
				}
				findPointOfMinimumNorm(q, list, baryCoords, out vector1);
				q.Clear();
				for (int j = 0;j < list.Count; j++)
				{
					q.Add(list3[list[j]]);
				}
				list3.Clear();
				foreach (Vector3d vector5 in q)
				{
					list3.Add(vector5);
				}
				single1 = float.NegativeInfinity;
				foreach (Vector3d vector6 in list3)
				{
					single2 = vector6.LengthSquared();
					if (single2 > single1)
					{
						single1 = single2;
					}
				}
				num1++;
			}
			while (((list3.Count != 4) && (vector1.LengthSquared() > (0.00d * single1))) && (num1 <= 0x32));
			ResourcePool.giveBack(list);
			ResourcePool.giveBack(list3);
			ResourcePool.giveBack(q);
			return true;
		}
		
		internal static bool areObjectsColliding (PhysicsBody objA, PhysicsBody objB, double marginA, double marginB, ref Vector3d backVelocityA, ref Vector3d backVelocityB, ref Vector3d separatingAxis)
		{
			Vector3d vector1;
			if (separatingAxis == zeroVector)
			{
				vector1 = upVector;
			}
			else
			{
				vector1 = separatingAxis;
			}
			List<int> list = ResourcePool.getIntList();
            List<double> baryCoords = new List<double>();
			List<Vector3d> list3 = ResourcePool.getVectorList();
			List<Vector3d> q = ResourcePool.getVectorList();
			double single1 = 1.00F;
			double single2 = 0.00d;
			int num1 = 0;
			do
			{
				Vector3d vector3;
				Vector3d item = findMinkowskiDifferenceExtremePoint(objA, objB, -(vector1), marginA, marginB);
				if (backVelocityA != zeroVector)
				{
					double single3 = Vector3d.DotProduct(-(vector1), backVelocityA);
					if (single3 > 0.00d)
					{
						item += backVelocityA;
					}
					else
					{
						if (single3 == 0.00d)
						{
							item +=  (backVelocityA * 0.50F);
						}
					}
				}
				if (backVelocityB != zeroVector)
				{
					double single4 = Vector3d.DotProduct(vector1, backVelocityB);
					if (single4 > 0.00d)
					{
						item -= backVelocityB;
					}
					else
					{
						if (single4 == 0.00d)
						{
							item -=  (backVelocityB * 0.50F);
						}
					}
				}
				if (Vector3d.DotProduct(vector1, item) > (0.00d * single1))
				{
					separatingAxis = vector1;
					ResourcePool.giveBack(list);
					ResourcePool.giveBack(list3);
					ResourcePool.giveBack(q);
					return false;
				}
				findPointClosestToSimplex(q, ref item, out vector3);
				if (item == vector3)
				{
					separatingAxis = vector1;
					ResourcePool.giveBack(list);
					ResourcePool.giveBack(list3);
					ResourcePool.giveBack(q);
					return false;
				}
				q.Clear();
				list3.Add(item);
				for (int i = 0;i < list3.Count; i++)
				{
					q.Add(list3[i]);
				}
				findPointOfMinimumNorm(q, list, baryCoords, out vector1);
				q.Clear();
				for (int j = 0;j < list.Count; j++)
				{
					q.Add(list3[list[j]]);
				}
				list3.Clear();
				foreach (Vector3d vector4 in q)
				{
					list3.Add(vector4);
				}
				single1 = float.NegativeInfinity;
				foreach (Vector3d vector5 in list3)
				{
					single2 = vector5.LengthSquared();
					if (single2 > single1)
					{
						single1 = single2;
					}
				}
				num1++;
			}
			while (((list3.Count != 4) && (vector1.LengthSquared() > (0.00d * single1))) && (num1 <= 0x32));
			ResourcePool.giveBack(list);
			ResourcePool.giveBack(list3);
			ResourcePool.giveBack(q);
			return true;
		}
		
		public static bool areSweptObjectsColliding (PhysicsBody objA, PhysicsBody objB, double marginA, double marginB, ref Vector3d sweepA, ref Vector3d sweepB, out Vector3d hitLocation, out Vector3d hitNormal, out double toi)
		{
            return areSweptObjectsColliding(objA, objB, ref objA.CollisionPrimitive.CenterPosition, ref objB.CollisionPrimitive.CenterPosition, ref objA.myInternalOrientationQuaternion, ref objB.myInternalOrientationQuaternion, marginA, marginB, ref sweepA, ref sweepB, out hitLocation, out hitNormal, out toi);
		}
		
		public static bool areSweptObjectsColliding (PhysicsBody objA, PhysicsBody objB, ref Vector3d positionToUseA, ref Vector3d positionToUseB, ref Quaternion orientationToUseA, ref Quaternion orientationToUseB, double marginA, double marginB, ref Vector3d sweepA, ref Vector3d sweepB, out Vector3d hitLocation, out Vector3d hitNormal, out double toi)
		{
		    Vector3d vector9;
			Vector3d vector10;
		    toi = 0.00d;
			double single1 = 0.00d;
			hitLocation = zeroVector;
			hitNormal = zeroVector;
			Vector3d vector5 = sweepB-sweepA;
			Vector3d vector6 = positionToUseA-positionToUseB;
			List<Vector3d> list = ResourcePool.getVectorList();
			List<Vector3d> list2 = ResourcePool.getVectorList();
			List<Vector3d> list3 = ResourcePool.getVectorList();
			List<int> subsimplex = ResourcePool.getIntList();
            List<double> baryCoords = new List<double>();
			List<Vector3d> q = ResourcePool.getVectorList();
			List<Vector3d> list7 = ResourcePool.getVectorList();
			List<Vector3d> list8 = ResourcePool.getVectorList();
			int num1 = 0;
            double single4 = double.NegativeInfinity;
			double single5 = 0.00d;
			while ((vector6.LengthSquared() > (0.00d * single4)) || (num1 < 1))
			{
			    Vector3d vector2;
				Vector3d vector3;
			    objA.CollisionPrimitive.getExtremePoint(ref vector6, ref positionToUseA, ref orientationToUseA, marginA, out vector2);
				Vector3d vector7 = Vector3d.Negate( vector6);
                objB.CollisionPrimitive.getExtremePoint(ref vector7, ref positionToUseB, ref orientationToUseB, marginB, out vector3);
				Vector3d item = vector2-vector3;
				Vector3d vector1 = hitLocation-item;
				double single2 = Vector3d.DotProduct(vector6,vector1);
				if (single2 > 0.00d)
				{
				    double single3 = Vector3d.DotProduct(vector6,vector5);
					if (single3 >= 0.00d)
					{
						hitLocation = noVector;
						hitNormal = noVector;
						toi = float.NaN;
						ResourcePool.giveBack(list);
						ResourcePool.giveBack(list2);
						ResourcePool.giveBack(list3);
						ResourcePool.giveBack(subsimplex);
						ResourcePool.giveBack(q);
						ResourcePool.giveBack(list7);
						ResourcePool.giveBack(list8);
						return false;
					}
					single1 -= single2 / single3;
					if (single1 > 1.00F)
					{
						hitLocation = noVector;
						hitNormal = noVector;
						toi = float.NaN;
						ResourcePool.giveBack(list);
						ResourcePool.giveBack(list2);
						ResourcePool.giveBack(list3);
						ResourcePool.giveBack(subsimplex);
						ResourcePool.giveBack(q);
						ResourcePool.giveBack(list7);
						ResourcePool.giveBack(list8);
						return false;
					}
					hitLocation=vector5* single1;
					toi = single1;
					hitNormal = vector6;
				}
				q.Clear();
				list7.Clear();
				list8.Clear();
				list.Add(item);
				list2.Add(vector2);
				list3.Add(vector3);
				for (int i = 0;i < list.Count; i++)
				{
					q.Add(hitLocation - list[i]);
				}
				findPointOfMinimumNorm(q, subsimplex, baryCoords, out vector6);
				q.Clear();
				for (int j = 0;j < subsimplex.Count; j++)
				{
					q.Add(list[subsimplex[j]]);
					list7.Add(list2[subsimplex[j]]);
					list8.Add(list3[subsimplex[j]]);
				}
				list.Clear();
				list2.Clear();
				list3.Clear();
				for (int z = 0;z < q.Count; z++)
				{
					list.Add(q[z]);
					list2.Add(list7[z]);
					list3.Add(list8[z]);
				}
				num1++;
				if (num1 > 0x32)
				{
					hitLocation = noVector;
					hitNormal = noVector;
					toi = double.NaN;
					ResourcePool.giveBack(list);
					ResourcePool.giveBack(list2);
					ResourcePool.giveBack(list3);
					ResourcePool.giveBack(subsimplex);
					ResourcePool.giveBack(q);
					ResourcePool.giveBack(list7);
					ResourcePool.giveBack(list8);
					return false;
				}
				single4 = float.NegativeInfinity;
				foreach (Vector3d vector8 in list)
				{
					single5 = vector8.LengthSquared();
					if (single5 > single4)
					{
						single4 = single5;
					}
				}
			}
			getBarycenter(list2, baryCoords, out vector9);
			getBarycenter(list3, baryCoords, out vector10);
			Vector3d vector11 = sweepA* toi;
			Vector3d vector12 = sweepB* toi;
			Vector3d vector13 = vector9+vector10;
			vector13=vector13+vector11;
			vector13=vector13+vector12;
			hitLocation=vector13* 0.50F;
			ResourcePool.giveBack(list);
			ResourcePool.giveBack(list2);
			ResourcePool.giveBack(list3);
			ResourcePool.giveBack(subsimplex);
			ResourcePool.giveBack(q);
			ResourcePool.giveBack(list7);
			ResourcePool.giveBack(list8);
			return true;
		}
		
		public static bool areSweptObjectsColliding (PhysicsBody objA, PhysicsBody objB, double marginA, double marginB, ref Vector3d sweepA, ref Vector3d sweepB, out double toi)
		{
            return areSweptObjectsColliding(objA, objB, ref objA.CollisionPrimitive.CenterPosition, ref objB.CollisionPrimitive.CenterPosition, ref objA.myInternalOrientationQuaternion, ref objB.myInternalOrientationQuaternion, marginA, marginB, ref sweepA, ref sweepB, out toi);
		}
		
		public static bool areSweptObjectsColliding (PhysicsBody objA, PhysicsBody objB, ref Vector3d positionToUseA, ref Vector3d positionToUseB, ref Quaternion orientationToUseA, ref Quaternion orientationToUseB, double marginA, double marginB, ref Vector3d sweepA, ref Vector3d sweepB, out double toi)
		{
		    toi = 0.00d;
			double single1 = 0.00d;
			Vector3d vector3 = zeroVector;
			Vector3d vector4 = sweepB-sweepA;
			Vector3d vector5 = objA.myInternalCenterPosition-objB.myInternalCenterPosition;
			List<Vector3d> list = ResourcePool.getVectorList();
			List<int> subsimplex = ResourcePool.getIntList();
            List<double> baryCoords = new List<double>();
			List<Vector3d> q = ResourcePool.getVectorList();
			int num1 = 0;
			double single4 = double.NegativeInfinity;
			double single5 = 0.00d;
			while ((vector5.LengthSquared() > (0.00d * single4)) || (num1 < 1))
			{
			    Vector3d item = findMinkowskiDifferenceExtremePoint(objA, objB, ref vector5, ref positionToUseA, ref positionToUseB, ref orientationToUseA, ref orientationToUseB, marginA, marginB);
				Vector3d vector1 = vector3-item;
				double single2 = Vector3d.DotProduct(vector5,vector1);
				if (single2 > 0.00d)
				{
					double single3;
					single3=Vector3d.DotProduct(vector5,vector4);
					if (single3 >= 0.00d)
					{
						toi = float.NaN;
						ResourcePool.giveBack(list);
						ResourcePool.giveBack(subsimplex);
						ResourcePool.giveBack(q);
						return false;
					}
					single1 -= single2 / single3;
					if (single1 > 1.00F)
					{
						toi = float.NaN;
						ResourcePool.giveBack(list);
						ResourcePool.giveBack(subsimplex);
						ResourcePool.giveBack(q);
						return false;
					}
					vector3=vector4* single1;
					toi = single1;
				}
				q.Clear();
				list.Add(item);
				for (int i = 0;i < list.Count; i++)
				{
					q.Add(vector3 - list[i]);
				}
				findPointOfMinimumNorm(q, subsimplex, baryCoords, out vector5);
				q.Clear();
				for (int j = 0;j < subsimplex.Count; j++)
				{
					q.Add(list[subsimplex[j]]);
				}
				list.Clear();
				for (int z = 0;z < q.Count; z++)
				{
					list.Add(q[z]);
				}
				num1++;
				if (num1 > 0x32)
				{
                    toi = double.NaN;
					ResourcePool.giveBack(list);
					ResourcePool.giveBack(subsimplex);
					ResourcePool.giveBack(q);
					return false;
				}
				single4 = double.NegativeInfinity;
				foreach (Vector3d vector6 in list)
				{
					single5 = vector6.LengthSquared();
					if (single5 > single4)
					{
						single4 = single5;
					}
				}
			}
			ResourcePool.giveBack(list);
			ResourcePool.giveBack(subsimplex);
			ResourcePool.giveBack(q);
			return true;
		}
		
		[Obsolete]
		internal static bool areObjectsCollidingReference (PhysicsBody objA, PhysicsBody objB, double marginA, double marginB)
		{
			List<Vector3d> q = ResourcePool.getVectorList();
			List<Vector3d> subsimplex = ResourcePool.getVectorList();
			q.Add(objA.myInternalCenterPosition - objB.myInternalCenterPosition);
			int num1 = 0;
			double single2 = float.PositiveInfinity;
			while (true)
			{
				Vector3d vector1;
				findPointOfMinimumNorm(q, subsimplex, out vector1);
				if (vector1.LengthSquared() < 0.00d)
				{
					ResourcePool.giveBack(q);
					ResourcePool.giveBack(subsimplex);
					return true;
				}
				q.Clear();
				foreach (Vector3d item in subsimplex)
				{
					q.Add(item);
				}
				Vector3d vector2 = findMinkowskiDifferenceExtremePoint(objA, objB, -(vector1), marginA, marginB);
				Vector3d vector3 = vector2 - vector1;
				double single1 = Vector3d.DotProduct(vector3, -(vector1));
				if (((single1 <= 0.00d) || ((num1 > 10) && (single1 <= single2))) || (num1 > 0x14))
				{
					ResourcePool.giveBack(q);
					ResourcePool.giveBack(subsimplex);
					return false;
				}
				single2 = single1;
				q.Add(vector2);
				num1++;
			}
		}
		
		public static void findPointClosestToSimplex (List<Vector3d> q, ref Vector3d p, out Vector3d closestPoint)
		{
			if (q.Count == 4)
			{
				Vector3d vector1 = q[0];
				Vector3d vector2 = q[1];
				Vector3d vector3 = q[2];
				Vector3d vector4 = q[3];
				if (((vector1 == vector2) || (vector1 == vector3)) || (vector1 == vector4))
				{
					getClosestPointOnTriangleToPoint(ref vector2, ref vector3, ref vector4, ref p, out closestPoint);
				}
				else if ((vector2 == vector3) || (vector2 == vector4))
				{
					getClosestPointOnTriangleToPoint(ref vector1, ref vector3, ref vector4, ref p, out closestPoint);
				}
				else if (vector3 == vector4)
				{
					getClosestPointOnTriangleToPoint(ref vector1, ref vector2, ref vector3, ref p, out closestPoint);
				}
				else
				{
					getClosestPointOnTetrahedronToPoint(ref vector1, ref vector2, ref vector3, ref vector4, ref p, out closestPoint);
				}
			}
			else if (q.Count == 3)
			{
				Vector3d vector5 = q[0];
				Vector3d vector6 = q[1];
				Vector3d vector7 = q[2];
				if ((vector5 == vector6) || (vector5 == vector7))
				{
					getClosestPointOnSegmentToPoint(ref vector6, ref vector7, ref p, out closestPoint);
				}
				else if (vector6 == vector7)
				{
					getClosestPointOnSegmentToPoint(ref vector5, ref vector6, ref p, out closestPoint);
				}
				else
				{
					getClosestPointOnTriangleToPoint(ref vector5, ref vector6, ref vector7, ref p, out closestPoint);
				}
			}
			else if (q.Count == 2)
			{
				Vector3d vector8 = q[0];
				Vector3d vector9 = q[1];
				if (vector8 == vector9)
				{
					closestPoint = vector8;
				}
				else
				{
					getClosestPointOnSegmentToPoint(ref vector8, ref vector9, ref p, out closestPoint);
				}
			}
			else if (q.Count == 1)
			{
				closestPoint = q[0];
			}
			else
			{
				closestPoint = noVector;
			}
		}
		
		public static void findPointOfMinimumNorm (List<Vector3d> q, out Vector3d closestPoint)
		{
			if (q.Count == 4)
			{
				Vector3d vector1 = q[0];
				Vector3d vector2 = q[1];
				Vector3d vector3 = q[2];
				Vector3d vector4 = q[3];
				if (((vector1 == vector2) || (vector1 == vector3)) || (vector1 == vector4))
				{
					getClosestPointOnTriangleToPoint(ref vector2, ref vector3, ref vector4, ref zeroVector, out closestPoint);
				}
				else if ((vector2 == vector3) || (vector2 == vector4))
				{
					getClosestPointOnTriangleToPoint(ref vector1, ref vector3, ref vector4, ref zeroVector, out closestPoint);
				}
				else if (vector3 == vector4)
				{
					getClosestPointOnTriangleToPoint(ref vector1, ref vector2, ref vector3, ref zeroVector, out closestPoint);
				}
				else
				{
					getClosestPointOnTetrahedronToPoint(ref vector1, ref vector2, ref vector3, ref vector4, ref zeroVector, out closestPoint);
				}
			}
			else if (q.Count == 3)
			{
				Vector3d vector5 = q[0];
				Vector3d vector6 = q[1];
				Vector3d vector7 = q[2];
				if ((vector5 == vector6) || (vector5 == vector7))
				{
					getClosestPointOnSegmentToPoint(ref vector6, ref vector7, ref zeroVector, out closestPoint);
				}
				else if (vector6 == vector7)
				{
					getClosestPointOnSegmentToPoint(ref vector5, ref vector6, ref zeroVector, out closestPoint);
				}
				else
				{
					getClosestPointOnTriangleToPoint(ref vector5, ref vector6, ref vector7, ref zeroVector, out closestPoint);
				}
			}
			else if (q.Count == 2)
			{
				Vector3d vector8 = q[0];
				Vector3d vector9 = q[1];
				if (vector8 == vector9)
				{
					closestPoint = vector8;
				}
				else
				{
					getClosestPointOnSegmentToPoint(ref vector8, ref vector9, ref zeroVector, out closestPoint);
				}
			}
			else if (q.Count == 1)
			{
				closestPoint = q[0];
			}
			else
			{
				closestPoint = noVector;
			}
		}
		
		public static void findPointOfMinimumNorm (List<Vector3d> q, List<Vector3d> subsimplex, out Vector3d closestPoint)
		{
			subsimplex.Clear();
			if (q.Count == 4)
			{
				Vector3d vector1 = q[0];
				Vector3d vector2 = q[1];
				Vector3d vector3 = q[2];
				Vector3d vector4 = q[3];
				if (((vector1 == vector2) || (vector1 == vector3)) || (vector1 == vector4))
				{
					getClosestPointOnTriangleToPoint(ref vector2, ref vector3, ref vector4, ref zeroVector, subsimplex, out closestPoint);
				}
				else if ((vector2 == vector3) || (vector2 == vector4))
				{
					getClosestPointOnTriangleToPoint(ref vector1, ref vector3, ref vector4, ref zeroVector, subsimplex, out closestPoint);
				}
				else if (vector3 == vector4)
				{
					getClosestPointOnTriangleToPoint(ref vector1, ref vector2, ref vector3, ref zeroVector, subsimplex, out closestPoint);
				}
				else
				{
					getClosestPointOnTetrahedronToPoint(ref vector1, ref vector2, ref vector3, ref vector4, ref zeroVector, subsimplex, out closestPoint);
				}
			}
			else if (q.Count == 3)
			{
				Vector3d vector5 = q[0];
				Vector3d vector6 = q[1];
				Vector3d vector7 = q[2];
				if ((vector5 == vector6) || (vector5 == vector7))
				{
					getClosestPointOnSegmentToPoint(ref vector6, ref vector7, ref zeroVector, subsimplex, out closestPoint);
				}
				else if (vector6 == vector7)
				{
					getClosestPointOnSegmentToPoint(ref vector5, ref vector6, ref zeroVector, subsimplex, out closestPoint);
				}
				else
				{
					getClosestPointOnTriangleToPoint(ref vector5, ref vector6, ref vector7, ref zeroVector, subsimplex, out closestPoint);
				}
			}
			else if (q.Count == 2)
			{
				Vector3d item = q[0];
				Vector3d vector9 = q[1];
				if (item == vector9)
				{
					subsimplex.Add(item);
					closestPoint = item;
				}
				else
				{
					getClosestPointOnSegmentToPoint(ref item, ref vector9, ref zeroVector, subsimplex, out closestPoint);
				}
			}
			else if (q.Count == 1)
			{
				subsimplex.Add(q[0]);
				closestPoint = q[0];
			}
			else
			{
				closestPoint = noVector;
			}
		}
		
		public static void findPointOfMinimumNorm (List<Vector3d> q, List<int> subsimplex, List<double> baryCoords, out Vector3d closestPoint)
		{
			subsimplex.Clear();
			baryCoords.Clear();
			if (q.Count == 4)
			{
				if (((q[0] == q[1]) || (q[0] == q[2])) || (q[0] == q[3]))
				{
					getClosestPointOnTriangleToPoint(q, 1, 2, 3, ref zeroVector, subsimplex, baryCoords, out closestPoint);
				}
				else if ((q[1] == q[2]) || (q[1] == q[3]))
				{
					getClosestPointOnTriangleToPoint(q, 0, 2, 3, ref zeroVector, subsimplex, baryCoords, out closestPoint);
				}
				else if (q[2] == q[3])
				{
					getClosestPointOnTriangleToPoint(q, 0, 1, 2, ref zeroVector, subsimplex, baryCoords, out closestPoint);
				}
				else
				{
					getClosestPointOnTetrahedronToPoint(q, ref zeroVector, subsimplex, baryCoords, out closestPoint);
				}
			}
			else if (q.Count == 3)
			{
				if ((q[0] == q[1]) || (q[0] == q[2]))
				{
					getClosestPointOnSegmentToPoint(q, 1, 2, ref zeroVector, subsimplex, baryCoords, out closestPoint);
				}
				else if (q[1] == q[2])
				{
					getClosestPointOnSegmentToPoint(q, 0, 1, ref zeroVector, subsimplex, baryCoords, out closestPoint);
				}
				else
				{
					getClosestPointOnTriangleToPoint(q, 0, 1, 2, ref zeroVector, subsimplex, baryCoords, out closestPoint);
				}
			}
			else if (q.Count == 2)
			{
				if (q[0] == q[1])
				{
					subsimplex.Add(0);
					baryCoords.Add(1.00F);
					closestPoint = q[0];
				}
				else
				{
					getClosestPointOnSegmentToPoint(q, 0, 1, ref zeroVector, subsimplex, baryCoords, out closestPoint);
				}
			}
			else if (q.Count == 1)
			{
				subsimplex.Add(0);
				baryCoords.Add(1.00F);
				closestPoint = q[0];
			}
			else
			{
				closestPoint = noVector;
			}
		}
		
		public static bool isPointInsideEntity (Vector3d p, PhysicsBody e, double margin)
		{
			return isPointInsideEntity(p, e, e.myInternalCenterPosition, e.myInternalOrientationQuaternion, margin);
		}
		
		public static bool isPointInsideEntity (Vector3d p, PhysicsBody e, Vector3d positionToUse, Quaternion orientationToUse, double margin)
		{
			Vector3d vector3;
			Vector3d vector4;
			Vector3d vector6;
			Vector3d vector7;
			Vector3d vector1 = e.myInternalCenterPosition - p;
			if (vector1 == zeroVector)
			{
				return true;
			}
			Vector3d vector2 = -(vector1);
            e.CollisionPrimitive.getExtremePoint(ref vector2, ref positionToUse, ref orientationToUse, margin, out vector3);
			vector3 -= p;
			if ( Vector3d.DotProduct(vector3, vector2)<= 0.00d)
			{
				return false;
			}
			vector2 = Vector3d.CrossProduct(vector3, vector1);
			if (vector2 == zeroVector)
			{
				return true;
			}
            e.CollisionPrimitive.getExtremePoint(ref vector2, ref positionToUse, ref orientationToUse, margin, out vector4);
			vector4 -= p;
			if ( Vector3d.DotProduct(vector4, vector2)<= 0.00d)
			{
				return false;
			}
			vector2 = Vector3d.CrossProduct(vector3 - vector1, vector4 - vector1);
			double single1 = Vector3d.DotProduct(vector2, vector1);
			if (single1 > 0.00d)
			{
				Vector3d vector5 = vector3;
				vector3 = vector4;
				vector4 = vector5;
				vector2 = -(vector2);
			}
			else
			{
				if (single1 == 0.00d)
				{
					return true;
				}
			}
			int num1 = 0;
		Label_00CF:
			num1++;
            e.CollisionPrimitive.getExtremePoint(ref vector2, ref positionToUse, ref orientationToUse, margin, out vector6);
			vector6 -= p;
			single1 = Vector3d.DotProduct(vector6, vector2);
			if ( Vector3d.DotProduct(vector6, vector2)<= 0.00d)
			{
				return false;
			}
			single1 = Vector3d.DotProduct(Vector3d.CrossProduct(vector3, vector6), vector1);
			if ((num1 < 0x14) && (Vector3d.DotProduct(Vector3d.CrossProduct(vector3, vector6), vector1) < 0.00d))
			{
				vector4 = vector6;
				vector2 = Vector3d.CrossProduct(vector3 - vector1, vector6 - vector1);
				goto Label_00CF;
			}
			single1 = Vector3d.DotProduct(Vector3d.CrossProduct(vector6, vector4), vector1);
			if ((num1 < 0x14) && (Vector3d.DotProduct(Vector3d.CrossProduct(vector6, vector4), vector1) < 0.00d))
			{
				vector3 = vector6;
				vector2 = Vector3d.CrossProduct(vector6 - vector1, vector4 - vector1);
				goto Label_00CF;
			}
		Label_0197:
			vector2 = Vector3d.CrossProduct(vector4 - vector3, vector6 - vector3);
			if ( Vector3d.DotProduct(vector2, vector3)>= 0.00d)
			{
				return true;
			}
            e.CollisionPrimitive.getExtremePoint(ref vector2, ref positionToUse, ref orientationToUse, margin, out vector7);
			vector7 -= p;
			if ((-Vector3d.DotProduct(vector7, vector2) >= 0.00d) || (Vector3d.DotProduct(vector7 - vector6, vector2) <= 0.00d))
			{
				return false;
			}
			Vector3d vector8 = Vector3d.CrossProduct(vector7, vector1);
			if ( Vector3d.DotProduct(vector3, vector8)> 0.00d)
			{
				if ( Vector3d.DotProduct(vector4, vector8)> 0.00d)
				{
					vector3 = vector7;
					goto Label_0197;
				}
				vector6 = vector7;
				goto Label_0197;
			}
			if (Vector3d.DotProduct(vector6, vector8)> 0.00d)
			{
				vector4 = vector7;
				goto Label_0197;
			}
			vector3 = vector7;
			goto Label_0197;
		}

        public static bool areBoxesColliding(BoxPrimitive a, BoxPrimitive b, double marginA, double marginB, out double distance, out Vector3d axis)
        {
            Vector3d vector1 = b.Body.myInternalCenterPosition - a.Body.myInternalCenterPosition;
            Matrix matrix1 = Matrix.Transpose(a.Body.myInternalOrientationMatrix);
            Vector3d vector2 = Vector3d.TransformCoord(vector1, matrix1);
            Matrix matrix2 = b.Body.myInternalOrientationMatrix * matrix1;
            Matrix matrix3 = Matrix.Transpose(matrix2);
            Vector3d vector3 = new Vector3d(a.HalfWidth + marginA, a.HalfHeight + marginA, a.HalfLength + marginA);
            Vector3d vector4 = new Vector3d(b.HalfWidth + marginB, b.HalfHeight + marginB, b.HalfLength + marginB);
            List<Vector3d> list = ResourcePool.getVectorList();
            list.Add(new Vector3d(a.HalfWidth + marginA, 0.00d, 0.00d));
            list.Add(new Vector3d(0.00d, a.HalfHeight + marginA, 0.00d));
            list.Add(new Vector3d(0.00d, 0.00d, a.HalfLength + marginA));
            List<Vector3d> list2 = ResourcePool.getVectorList();
            list2.Add(Vector3d.TransformCoord(new Vector3d(b.HalfWidth + marginB, 0.00d, 0.00d), matrix2));
            list2.Add(Vector3d.TransformCoord(new Vector3d(0.00d, b.HalfHeight + marginB, 0.00d), matrix2));
            list2.Add(Vector3d.TransformCoord(new Vector3d(0.00d, 0.00d, b.HalfLength + marginB), matrix2));
            distance = double.NegativeInfinity;
            axis = noVector;
            double single1 = double.NegativeInfinity;
            Vector3d vector5 = noVector;
            for (int i = 0; i < 3; i++)
            {
                Vector3d vector6 = list[i];
                vector6.Normalize();
                Vector3d vector8 = Vector3d.TransformCoord(vector6, matrix3);
                vector8.x = Math.Abs(vector8.x);
                vector8.y = Math.Abs(vector8.y);
                vector8.z = Math.Abs(vector8.z);
                distance = (Math.Abs(Vector3d.DotProduct(vector6, vector2)) - Vector3d.DotProduct(new Vector3d(Math.Abs(vector6.x), Math.Abs(vector6.y), Math.Abs(vector6.z)), vector3)) - Vector3d.DotProduct(vector8, vector4);
                if (distance > 0.00d)
                {
                    axis = vector6;
                    ResourcePool.giveBack(list);
                    ResourcePool.giveBack(list2);
                    return false;
                }
                if (distance > single1)
                {
                    single1 = distance;
                    vector5 = vector6;
                }
            }
            for (int j = 0; j < 3; j++)
            {
                Vector3d vector9 = list2[j];
                vector9.Normalize();
                Vector3d vector11 = Vector3d.TransformCoord(vector9, matrix3);
                vector11.x = Math.Abs(vector11.x);
                vector11.y = Math.Abs(vector11.y);
                vector11.z = Math.Abs(vector11.z);
                distance = (Math.Abs(Vector3d.DotProduct(vector9, vector2)) - Vector3d.DotProduct(new Vector3d(Math.Abs(vector9.x), Math.Abs(vector9.y), Math.Abs(vector9.z)), vector3)) - Vector3d.DotProduct(vector11, vector4);
                if (distance > 0.00d)
                {
                    axis = vector9;
                    ResourcePool.giveBack(list);
                    ResourcePool.giveBack(list2);
                    return false;
                }
                if (distance > single1)
                {
                    single1 = distance;
                    vector5 = vector9;
                }
            }
            for (int z = 0; z < 3; z++)
            {
                for (int k = 0; k < 3; k++)
                {
                    Vector3d vector12 = Vector3d.CrossProduct(list[z], list2[k]);
                    if (vector12 != zeroVector)
                    {
                        vector12.Normalize();
                        Vector3d vector14 = Vector3d.TransformCoord(vector12, matrix3);
                        vector14.x = Math.Abs(vector14.x);
                        vector14.y = Math.Abs(vector14.y);
                        vector14.z = Math.Abs(vector14.z);
                        distance = (Math.Abs(Vector3d.DotProduct(vector12, vector2)) - Vector3d.DotProduct(new Vector3d(Math.Abs(vector12.x), Math.Abs(vector12.y), Math.Abs(vector12.z)), vector3)) - Vector3d.DotProduct(vector14, vector4);
                        if (distance > 0.00d)
                        {
                            axis = vector12;
                            ResourcePool.giveBack(list);
                            ResourcePool.giveBack(list2);
                            return false;
                        }
                        if (distance > single1)
                        {
                            single1 = distance;
                            vector5 = vector12;
                        }
                    }
                }
            }
            distance = single1;
            axis = vector5;
            ResourcePool.giveBack(list);
            ResourcePool.giveBack(list2);
            return true;
        }

        public static bool areBoxesColliding(BoxPrimitive a, BoxPrimitive b, double marginA, double marginB, out double distance, out Vector3d axis, List<Vector3d> manifold, List<int> ids, out int collisionType)
        {
            byte num11;
            collisionType = -1;
            Vector3d vector1 = b.CenterPosition - a.CenterPosition;
            Matrix matrix1 = Matrix.Transpose(a.Body.myInternalOrientationMatrix);
            Vector3d vector2 = Vector3d.TransformCoord(vector1, matrix1);
            Matrix matrix2 = b.Body.myInternalOrientationMatrix * matrix1;
            Matrix matrix3 = Matrix.Transpose(matrix2);
            Vector3d vector3 = new Vector3d(a.HalfWidth + marginA, a.HalfHeight + marginA, a.HalfLength + marginA);
            Vector3d vector4 = new Vector3d(b.HalfWidth + marginB, b.HalfHeight + marginB, b.HalfLength + marginB);
            List<Vector3d> list = ResourcePool.getVectorList();
            list.Add(rightVector);
            list.Add(upVector);
            list.Add(backVector);
            List<Vector3d> list2 = ResourcePool.getVectorList();
            list2.Add(matrix2.Right);
            list2.Add(matrix2.Up);
            list2.Add(matrix2.Backward);
            distance = double.NegativeInfinity;
            axis = noVector;
            double single1 = double.NegativeInfinity;
            Vector3d vector5 = noVector;
            bool flag1 = false;
            for (int i = 0; i < 3; i++)
            {
                Vector3d vector6 = list[i];
                Vector3d vector8 = Vector3d.TransformCoord(vector6, matrix3);
                vector8.x = Math.Abs(vector8.x);
                vector8.y = Math.Abs(vector8.y);
                vector8.z = Math.Abs(vector8.z);
                double value = Vector3d.DotProduct(vector6, vector2);
                double single3 =
                    Vector3d.DotProduct(new Vector3d(Math.Abs(vector6.x), Math.Abs(vector6.y), Math.Abs(vector6.z)), vector3);
                double single4 = Vector3d.DotProduct(vector8, vector4);
                distance = (Math.Abs(value) - single3) - single4;
                if (distance > 0.00d)
                {
                    axis = vector6;
                    ResourcePool.giveBack(list);
                    ResourcePool.giveBack(list2);
                    return false;
                }
                if (distance > single1)
                {
                    single1 = distance;
                    vector5 = vector6;
                    flag1 = true;
                }
            }
            for (int j = 0; j < 3; j++)
            {
                Vector3d vector9 = list2[j];
                Vector3d vector11 = Vector3d.TransformCoord(vector9, matrix3);
                vector11.x = Math.Abs(vector11.x);
                vector11.y = Math.Abs(vector11.y);
                vector11.z = Math.Abs(vector11.z);
                double single5 = Vector3d.DotProduct(vector9, vector2);
                double single6 =
                    Vector3d.DotProduct(new Vector3d(Math.Abs(vector9.x), Math.Abs(vector9.y), Math.Abs(vector9.z)), vector3);
                double single7 = Vector3d.DotProduct(vector11, vector4);
                distance = (Math.Abs(single5) - single6) - single7;
                if (distance > 0.00d)
                {
                    axis = vector9;
                    ResourcePool.giveBack(list);
                    ResourcePool.giveBack(list2);
                    return false;
                }
                if (distance > single1)
                {
                    single1 = distance;
                    vector5 = vector9;
                    flag1 = false;
                }
            }
            bool flag2 = false;
            byte edgeDir = 0xff;
            byte num4 = 0xff;
            for (int z = 0; z < 3; z++)
            {
                for (int k = 0; k < 3; k++)
                {
                    Vector3d vector12 = list[z];
                    Vector3d vector13 = list2[k];
                    Vector3d vector14 = Vector3d.CrossProduct(vector12, vector13);
                    if (vector14 != zeroVector)
                    {
                        vector14.Normalize();
                        Vector3d vector16 = Vector3d.TransformCoord(vector14, matrix3);
                        vector16.x = Math.Abs(vector16.x);
                        vector16.y = Math.Abs(vector16.y);
                        vector16.z = Math.Abs(vector16.z);
                        double single8 = Vector3d.DotProduct(vector14, vector2);
                        double single9 =
                            Vector3d.DotProduct(
                                new Vector3d(Math.Abs(vector14.x), Math.Abs(vector14.y), Math.Abs(vector14.z)), vector3);
                        double single10 = Vector3d.DotProduct(vector16, vector4);
                        distance = (Math.Abs(single8) - single9) - single10;
                        if (distance > 0.00d)
                        {
                            axis = vector14;
                            ResourcePool.giveBack(list);
                            ResourcePool.giveBack(list2);
                            return false;
                        }
                        if (distance > (single1 + 0.00d))
                        {
                            single1 = distance;
                            vector5 = vector14;
                            edgeDir = (byte)z;
                            num4 = (byte)k;
                            flag2 = true;
                            flag1 = false;
                        }
                    }
                }
            }
            distance = single1;
            axis = vector5;
            Vector3d vector17 = Vector3d.Negate(axis);
            Vector3d vector18 = Vector3d.TransformCoord(axis, matrix3);
            axis = Vector3d.TransformCoord(axis, a.Body.myInternalOrientationMatrix);
            double single11 = Vector3d.DotProduct(vector1, axis);
            if (single11 > 0.00d)
            {
                vector17 = Vector3d.Negate(vector17);
                vector18 = Vector3d.Negate(vector18);
                axis = Vector3d.Negate(axis);
            }
            if (flag2)
            {
                Vector3d p1;
                Vector3d q1;
                Vector3d p2;
                Vector3d q2;
                Vector3d vector23;
                Vector3d vector24;
                int num7;
                int num8;
                int num9;
                int num10;
                getBoxEdge(ref vector17, a, edgeDir, out p1, out q1, out num7, out num8, 0);
                getBoxEdge(ref vector18, b, num4, out p2, out q2, out num9, out num10, 8);
                getClosestPointsBetweenSegments(p1, q1, p2, q2, out vector23, out vector24);
                Vector3d item = vector23 + vector24;
                item = item * 0.50F;
                manifold.Add(item);
                ids.Add((((num7 * 3) + (num8 * 5)) * 0x29) + (((num9 * 3) + (num10 * 5)) * 0x2b));
                collisionType = 3;

            }
            else if (flag1)
            {
                num11 = getSATFeatureType(b, ref vector18, out num4);
                switch (num11)
                {
                    case 0:
                        {
                            collisionType = 0;
                            getBoxFaceFaceContacts(ref vector17, ref vector18, ref axis, a, b, manifold, ids);
                            break;
                        }
                    case 1:
                        {
                            collisionType = 1;
                            getBoxFaceEdgeContacts(ref vector17, ref vector18, ref axis, num4, a, b, 0, 8, manifold, ids);
                            break;
                        }
                    case 2:
                        {
                            collisionType = 2;
                            getBoxFaceVertexContact(ref vector17, ref vector18, ref axis, a, b, 0, 8, manifold, ids);
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
            else
            {
                num11 = getSATFeatureType(b, ref vector17, out edgeDir);
                switch (num11)
                {
                    case 0:
                        {
                            collisionType = 0;
                            getBoxFaceFaceContacts(ref vector17, ref vector18, ref axis, a, b, manifold, ids);
                            break;
                        }
                    case 1:
                        {
                            collisionType = 1;
                            getBoxFaceEdgeContacts(ref vector18, ref vector17, ref axis, edgeDir, b, a, 8, 0, manifold,
                                                   ids);
                            break;
                        }
                    case 2:
                        {
                            collisionType = 2;
                            getBoxFaceVertexContact(ref vector18, ref vector17, ref axis, b, a, 8, 0, manifold, ids);
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
        Label_063F:
            ResourcePool.giveBack(list);
            ResourcePool.giveBack(list2);
            return true;
        }

        private static byte getSATFeatureType(BoxPrimitive a, ref Vector3d localDirection, out byte edgeDir)
        {
            edgeDir = 0xff;
            double value = localDirection.x;
            double single2 = localDirection.y;
            double single3 = localDirection.z;
            const double single4 = 1.00d;
            const double single5 = 1.00d;
            if ((value < single5) && (value > single4))
            {
                return 0;
            }
            if ((value > -single5) && (value < -single4))
            {
                return 0;
            }
            if ((single2 < single5) && (single2 > single4))
            {
                return 0;
            }
            if ((single2 > -single5) && (single2 < -single4))
            {
                return 0;
            }
            if ((single3 < single5) && (single3 > single4))
            {
                return 0;
            }
            if ((single3 > -single5) && (single3 < -single4))
            {
                return 0;
            }
            bool flag1 = Math.Abs(value) < 0.01F;
            bool flag2 = Math.Abs(single2) < 0.01F;
            bool flag3 = Math.Abs(single3) < 0.01F;
            if ((flag1 && !flag2) && !flag3)
            {
                edgeDir = 0;
                return 1;
            }
            if ((!flag1 && flag2) && !flag3)
            {
                edgeDir = 1;
                return 1;
            }
            if ((!flag1 && !flag2) && flag3)
            {
                edgeDir = 2;
                return 1;
            }
            return 2;
        }
		
		internal static bool areObjectsCollidingMPROld (PhysicsBody objA, PhysicsBody objB, double marginA, double marginB)
		{
			Vector3d axis;
			Vector3d vector8;
			Vector3d vector17;
			double single2;
			Vector3d vector1 = objB.myInternalCenterPosition - objA.myInternalCenterPosition;
			Vector3d rayOrigin = vector1;
			if (rayOrigin != zeroVector)
			{
				axis = -(rayOrigin);
			}
			else
			{
				axis = Vector3d.Up();
			}
			Vector3d a = findMinkowskiDifferenceExtremePoint(objB, objA, axis, marginB, marginA);
			Vector3d vector5 = Vector3d.CrossProduct(a, rayOrigin);
			if (vector5 == zeroVector)
			{
				vector5 = Vector3d.Up();
			}
			Vector3d b = findMinkowskiDifferenceExtremePoint(objB, objA, vector5, marginB, marginA);
			Vector3d vector7 = findMinkowskiDifferenceExtremePoint(objB, objA, Vector3d.CrossProduct(b - rayOrigin, a - rayOrigin), marginB, marginA);
		Label_0085:
			vector8 = a - rayOrigin;
			Vector3d vector9 = b - rayOrigin;
			Vector3d vector10 = vector7 - rayOrigin;
			Vector3d vector14 =  ((((rayOrigin + a) + b) + vector7) / 4.00F);
			Vector3d vector11 = Vector3d.CrossProduct(vector9, vector8);
			Vector3d vector12 = Vector3d.CrossProduct(vector9, vector10);
			Vector3d vector13 = Vector3d.CrossProduct(vector10, vector8);
			if (Vector3d.DotProduct(vector11, rayOrigin) < Vector3d.DotProduct(vector11, vector14))
			{
				vector11 =  (vector11 * -1.00F);
			}
			if (Vector3d.DotProduct(vector12, rayOrigin) < Vector3d.DotProduct(vector12, vector14))
			{
				vector12 =  (vector12 * -1.00F);
			}
			if (Vector3d.DotProduct(vector13, rayOrigin) < Vector3d.DotProduct(vector13, vector14))
			{
				vector13 =  (vector13 * -1.00F);
			}
			if ( Vector3d.DotProduct(vector11, rayOrigin)< 0.00d)
			{
				vector7 = findMinkowskiDifferenceExtremePoint(objB, objA, vector11, marginB, marginA);
				goto Label_0085;
			}
			if ( Vector3d.DotProduct(vector12, rayOrigin)< 0.00d)
			{
				a = findMinkowskiDifferenceExtremePoint(objB, objA, vector12, marginB, marginA);
				goto Label_0085;
			}
			if ( Vector3d.DotProduct(vector13, rayOrigin)< 0.00d)
			{
				b = findMinkowskiDifferenceExtremePoint(objB, objA, vector13, marginB, marginA);
				goto Label_0085;
			}
		Label_01A9:
			vector14 =  ((((rayOrigin + a) + b) + vector7) / 4.00F);
			Vector3d vector15 = Vector3d.CrossProduct(b - a, vector7 - a);
			if (Vector3d.DotProduct(vector15, a) < Vector3d.DotProduct(vector15, vector14))
			{
				vector15 =  (vector15 * -1.00F);
			}
			if ( Vector3d.DotProduct(vector15, a)>= 0.00d)
			{
				return true;
			}
			Vector3d c = findMinkowskiDifferenceExtremePoint(objB, objA, vector15, marginB, marginA);
			double single1 = Vector3d.DotProduct(vector15, c);
			if ((single1 <= 0.00d) || (Math.Abs((float) (single1 - Vector3d.DotProduct(vector15, vector7))) < 0.00d))
			{
				return false;
			}
			if (findRayTriangleIntersection(rayOrigin, axis, float.MaxValue, a, b, c, out vector17, out single2))
			{
				vector7 = c;
				goto Label_01A9;
			}
			if (findRayTriangleIntersection(rayOrigin, axis, float.MaxValue, a, vector7, c, out vector17, out single2))
			{
				b = c;
				goto Label_01A9;
			}
			if (findRayTriangleIntersection(rayOrigin, axis, float.MaxValue, b, vector7, c, out vector17, out single2))
			{
				a = c;
			}
			goto Label_01A9;
		}
		
		public static bool areObjectsCollidingMPR (PhysicsBody objA, PhysicsBody objB, double marginA, double marginB)
		{
            return areObjectsCollidingMPR(objA, objB, ref objA.CollisionPrimitive.CenterPosition, ref objB.CollisionPrimitive.CenterPosition, ref objA.myInternalOrientationQuaternion, ref objB.myInternalOrientationQuaternion, marginA, marginB);
		}
		
		public static bool areObjectsCollidingMPR (PhysicsBody objA, PhysicsBody objB, ref Vector3d positionA, ref Vector3d positionB, ref Quaternion orientationA, ref Quaternion orientationB, double marginA, double marginB)
		{
			Vector3d vector1 = objB.myInternalCenterPosition - objA.myInternalCenterPosition;
			if (vector1 == zeroVector)
			{
				return true;
			}
			Vector3d vector2 = -(vector1);
			Vector3d vector3 = findMinkowskiDifferenceExtremePoint(objB, objA, ref vector2, ref positionB, ref positionA, ref orientationB, ref orientationA, marginB, marginA);
			if ( Vector3d.DotProduct(vector3, vector2)<= 0.00d)
			{
				return false;
			}
			vector2 = Vector3d.CrossProduct(vector3, vector1);
			if (vector2 == zeroVector)
			{
				return true;
			}
			Vector3d vector4 = findMinkowskiDifferenceExtremePoint(objB, objA, ref vector2, ref positionB, ref positionA, ref orientationB, ref orientationA, marginB, marginA);
			if (Vector3d.DotProduct(vector4, vector2)<= 0.00d)
			{
				return false;
			}
			vector2 = Vector3d.CrossProduct(vector3 - vector1, vector4 - vector1);
			if (Vector3d.DotProduct(vector2, vector1)> 0.00d)
			{
				Vector3d vector5 = vector3;
				vector3 = vector4;
				vector4 = vector5;
				vector2 = -(vector2);
			}
			int num1 = 0;

		Label_00BB:
			num1++;
			Vector3d vector6 = findMinkowskiDifferenceExtremePoint(objB, objA, ref vector2, ref positionB, ref positionA, ref orientationB, ref orientationA, marginB, marginA);
			Vector3d.DotProduct(vector6, vector2);
			if (Vector3d.DotProduct(vector6, vector2)<= 0.00d)
			{
				return false;
			}
			Vector3d.DotProduct(Vector3d.CrossProduct(vector3, vector6), vector1);
			if ((num1 < 0x14) && (Vector3d.DotProduct(Vector3d.CrossProduct(vector3, vector6), vector1) < 0.00d))
			{
				vector4 = vector6;
				vector2 = Vector3d.CrossProduct(vector3 - vector1, vector6 - vector1);
				goto Label_00BB;
			}
			Vector3d.DotProduct(Vector3d.CrossProduct(vector6, vector4), vector1);
			if ((num1 < 0x14) && (Vector3d.DotProduct(Vector3d.CrossProduct(vector6, vector4), vector1) < 0.00d))
			{
				vector3 = vector6;
				vector2 = Vector3d.CrossProduct(vector6 - vector1, vector4 - vector1);
				goto Label_00BB;
			}
		Label_017B:
			vector2 = Vector3d.CrossProduct(vector4 - vector3, vector6 - vector3);
			if (Vector3d.DotProduct(vector2, vector3)>= 0.00d)
			{
				return true;
			}
			Vector3d vector7 = findMinkowskiDifferenceExtremePoint(objB, objA, ref vector2, ref positionB, ref positionA, ref orientationB, ref orientationA, marginB, marginA);
			if ((-Vector3d.DotProduct(vector7, vector2) >= 0.00d) || (Vector3d.DotProduct(vector7 - vector6, vector2) <= 0.00d))
			{
				return false;
			}
			Vector3d vector8 = Vector3d.CrossProduct(vector7, vector1);
			if (Vector3d.DotProduct(vector3, vector8)> 0.00d)
			{
				if (Vector3d.DotProduct(vector4, vector8)> 0.00d)
				{
					vector3 = vector7;
					goto Label_017B;
				}
				vector6 = vector7;
				goto Label_017B;
			}
			if (Vector3d.DotProduct(vector6, vector8)> 0.00d)
			{
				vector4 = vector7;
				goto Label_017B;
			}
			vector3 = vector7;
			goto Label_017B;
		}
		
		internal static bool areSweptObjectsCollidingMPR (PhysicsBody objA, PhysicsBody objB, ref Vector3d positionToUseA, ref Vector3d positionToUseB, ref Quaternion orientationToUseA, ref Quaternion orientationToUseB, double marginA, double marginB, ref Vector3d sweepA, ref Vector3d sweepB, out Vector3d location, out Vector3d normal, out double toi)
		{
			Vector3d vector1 = positionToUseA + sweepA;
			Vector3d vector2 = positionToUseB + sweepB;
			Vector3d vectorRef1 = positionToUseB - positionToUseA;
			Vector3d vector6 = vector2 - vector1;
			Vector3d vector3 = sweepB - sweepA;
            //KeyboardState state1 = Keyboard.GetState();
            //state1.IsKeyDown(0x50);
			if (areSweptObjectsCollidingMPR(objA, objB, ref positionToUseA, ref positionToUseB, ref orientationToUseA, ref orientationToUseB, marginA, marginB, ref zeroVector, ref vector3))
			{
				double single1 = findSweptPenetrationDepth(objA, objB, ref sweepA, ref sweepB, ref positionToUseA, ref positionToUseB, ref orientationToUseA, ref orientationToUseB, marginA, marginB, vector3);
				double single2 = vector3.Length;
				if (single2 > 0.00d)
				{
					toi = 1.00F - (single1 / single2);
				}
				else
				{
					toi = 0.00d;
				}
				Vector3d vector4 = positionToUseA + ( sweepA * toi);
				Vector3d vector5 = positionToUseB + ( sweepB * toi);
				areObjectsCollidingMPR(objA, objB, ref vector4, ref vector5, ref orientationToUseA, ref orientationToUseB, marginA * 2.00F, marginB * 2.00F, out location, out normal, out single1);
				return true;
			}
			toi = float.NaN;
			location = noVector;
			normal = noVector;
			return false;
		}
			
		public static bool areSweptObjectsCollidingMPR (PhysicsBody objA, PhysicsBody objB, double marginA, double marginB, ref Vector3d sweepA, ref Vector3d sweepB)
		{
            return areSweptObjectsCollidingMPR(objA, objB, ref objA.CollisionPrimitive.CenterPosition, ref objB.CollisionPrimitive.CenterPosition, ref objA.myInternalOrientationQuaternion, ref objB.myInternalOrientationQuaternion, marginA, marginB, ref sweepA, ref sweepB);
		}
		
		public static bool areSweptObjectsCollidingMPR (PhysicsBody objA, PhysicsBody objB, ref Vector3d positionToUseA, ref Vector3d positionToUseB, ref Quaternion orientationToUseA, ref Quaternion orientationToUseB, double marginA, double marginB, ref Vector3d sweepA, ref Vector3d sweepB)
		{
			Vector3d vector1 = objB.myInternalCenterPosition - objA.myInternalCenterPosition;
			if (vector1 == zeroVector)
			{
				return true;
			}
			Vector3d vector2 = -(vector1);
			Vector3d vector3 = findMinkowskiDifferenceExtremePoint(objB, objA, ref sweepB, ref sweepA, ref vector2, ref positionToUseB, ref positionToUseA, ref orientationToUseB, ref orientationToUseA, marginB, marginA);
			if ( Vector3d.DotProduct(vector3, vector2) <= 0.00d)
			{
				return false;
			}
			vector2 = Vector3d.CrossProduct(vector3, vector1);
			if (vector2 == zeroVector)
			{
				return true;
			}
			Vector3d vector4 = findMinkowskiDifferenceExtremePoint(objB, objA, ref sweepB, ref sweepA, ref vector2, ref positionToUseB, ref positionToUseA, ref orientationToUseB, ref orientationToUseA, marginB, marginA);
			if ( Vector3d.DotProduct(vector4, vector2)<= 0.00d)
			{
				return false;
			}
			vector2 = Vector3d.CrossProduct(vector3 - vector1, vector4 - vector1);
			if ( Vector3d.DotProduct(vector2, vector1)> 0.00d)
			{
				Vector3d vector5 = vector3;
				vector3 = vector4;
				vector4 = vector5;
				vector2 = -(vector2);
			}
			int num1 = 0;
		Label_00C3:
			num1++;
			Vector3d vector6 = findMinkowskiDifferenceExtremePoint(objB, objA, ref sweepB, ref sweepA, ref vector2, ref positionToUseB, ref positionToUseA, ref orientationToUseB, ref orientationToUseA, marginB, marginA);
			Vector3d.DotProduct(vector6, vector2);
			if ( Vector3d.DotProduct(vector6, vector2)<= 0.00d)
			{
				return false;
			}
			Vector3d.DotProduct(Vector3d.CrossProduct(vector3, vector6), vector1);
			if ((num1 < 0x14) && (Vector3d.DotProduct(Vector3d.CrossProduct(vector3, vector6), vector1) < 0.00d))
			{
				vector4 = vector6;
				vector2 = Vector3d.CrossProduct(vector3 - vector1, vector6 - vector1);
				goto Label_00C3;
			}
			Vector3d.DotProduct(Vector3d.CrossProduct(vector6, vector4), vector1);
			if ((num1 < 0x14) && (Vector3d.DotProduct(Vector3d.CrossProduct(vector6, vector4), vector1) < 0.00d))
			{
				vector3 = vector6;
				vector2 = Vector3d.CrossProduct(vector6 - vector1, vector4 - vector1);
				goto Label_00C3;
			}
		Label_0187:
			vector2 = Vector3d.CrossProduct(vector4 - vector3, vector6 - vector3);
			if ( Vector3d.DotProduct(vector2, vector3)>= 0.00d)
			{
				return true;
			}
			Vector3d vector7 = findMinkowskiDifferenceExtremePoint(objB, objA, ref sweepB, ref sweepA, ref vector2, ref positionToUseB, ref positionToUseA, ref orientationToUseB, ref orientationToUseA, marginB, marginA);
			if ((-Vector3d.DotProduct(vector7, vector2) >= 0.00d) || (Vector3d.DotProduct(vector7 - vector6, vector2) <= 0.00d))
			{
				return false;
			}
			Vector3d vector8 = Vector3d.CrossProduct(vector7, vector1);
			if ( Vector3d.DotProduct(vector3, vector8)> 0.00d)
			{
				if ( Vector3d.DotProduct(vector4, vector8)> 0.00d)
				{
					vector3 = vector7;
					goto Label_0187;
				}
				vector6 = vector7;
				goto Label_0187;
			}
			if ( Vector3d.DotProduct(vector6, vector8)> 0.00d)
			{
				vector4 = vector7;
				goto Label_0187;
			}
			vector3 = vector7;
			goto Label_0187;
		}
        internal static bool areObjectsCollidingMPR(PhysicsBody a, PhysicsBody b, out double distance, out Vector3d normal)
        {
            return areObjectsCollidingMPR(a, b, ref a.CollisionPrimitive.CenterPosition, ref b.CollisionPrimitive.CenterPosition, ref a.myInternalOrientationQuaternion, ref b.myInternalOrientationQuaternion, out distance, out normal);
        }

        internal static bool areObjectsCollidingMPR(PhysicsBody a, PhysicsBody b, ref Vector3d positionA, ref Vector3d positionB, ref Quaternion orientationA, ref Quaternion orientationB, out double distance, out Vector3d normal)
        {
            distance = findConservativeDistanceEstimate(a, b, ref positionA, ref positionB, ref orientationA, ref orientationB, 0.00d, 0.00d, out normal);
            if (distance == 0.00d)
            {
                normal = noVector;
                return true;
            }
            return false;
        }

        public static bool areSweptObjectsCollidingCA(PhysicsBody objA, PhysicsBody objB, double dt, out Vector3d nextPositionA, out Vector3d nextPositionB,
            out Quaternion nextOrientationA, out Quaternion nextOrientationB, out double timeOfImpact, out Vector3d location, out Vector3d normal)
        {
            Vector3d vector1;
            Vector3d vector2;
            Vector3d vector3;
            Vector3d vector4;
            Quaternion quaternion1;
            Quaternion quaternion2;
            Quaternion quaternion3;
            Quaternion quaternion4;
            integrateLinearVelocity(objA, objB, dt, out vector1, out vector2, out vector3, out vector4);
            integrateAngularVelocity(objA, objB, dt, out quaternion1, out quaternion2, out quaternion3, out quaternion4);
            if (areSweptObjectsCollidingCA(objA, objB, ref vector1, ref vector2, ref quaternion1, ref quaternion2, ref vector3, ref vector4, ref quaternion3, ref quaternion4, out nextPositionA, out nextPositionB, out nextOrientationA, out nextOrientationB, out timeOfImpact))
            {
                double single1;
                areObjectsCollidingMPR(objA, objB, ref nextPositionA, ref nextPositionB, ref nextOrientationA, ref nextOrientationB, objA.collisionMargin, objB.collisionMargin, out location, out normal, out single1);
                return true;
            }
            location = noVector;
            normal = noVector;
            return false;
        }

        public static bool areSweptObjectsCollidingCA(PhysicsBody objA, PhysicsBody objB, ref Vector3d originalPositionA, ref Vector3d originalPositionB,
            ref Quaternion originalOrientationA, ref Quaternion originalOrientationB, ref Vector3d finalPositionA, ref Vector3d finalPositionB,
            ref Quaternion finalOrientationA, ref Quaternion finalOrientationB, out Vector3d nextPositionA, out Vector3d nextPositionB,
            out Quaternion nextOrientationA, out Quaternion nextOrientationB, out double timeOfImpact, out Vector3d location, out Vector3d normal)
        {
            if (areSweptObjectsCollidingCA(objA, objB, ref originalPositionA, ref originalPositionB, ref originalOrientationA, ref originalOrientationB, ref finalPositionA, ref finalPositionB, ref finalOrientationA, ref finalOrientationB, out nextPositionA, out nextPositionB, out nextOrientationA, out nextOrientationB, out timeOfImpact))
            {
                double single1;
                areObjectsCollidingMPR(objA, objB, ref nextPositionA, ref nextPositionB, ref nextOrientationA, ref nextOrientationB, objA.collisionMargin, objB.collisionMargin, out location, out normal, out single1);
                return true;
            }
            location = noVector;
            normal = noVector;
            return false;
        }

        public static bool areObjectsCollidingCA(PhysicsBody objA, PhysicsBody objB, double dt, out Vector3d nextPositionA, out Vector3d nextPositionB,
            out Quaternion nextOrientationA, out Quaternion nextOrientationB, out double timeOfImpact)
        {
            Vector3d vector1;
            Vector3d vector2;
            Vector3d vector3;
            Vector3d vector4;
            Quaternion quaternion1;
            Quaternion quaternion2;
            Quaternion quaternion3;
            Quaternion quaternion4;
            integrateLinearVelocity(objA, objB, dt, out vector1, out vector2, out vector3, out vector4);
            integrateAngularVelocity(objA, objB, dt, out quaternion1, out quaternion2, out quaternion3, out quaternion4);
            return areSweptObjectsCollidingCA(objA, objB, ref vector1, ref vector2, ref quaternion1, ref quaternion2, ref vector3, 
                ref vector4, ref quaternion3, ref quaternion4, out nextPositionA, out nextPositionB, out nextOrientationA, out nextOrientationB, 
                out timeOfImpact);
        }

        public static bool areSweptObjectsCollidingCA(PhysicsBody objA, PhysicsBody objB, ref Vector3d originalPositionA, ref Vector3d originalPositionB,
            ref Quaternion originalOrientationA, ref Quaternion originalOrientationB, ref Vector3d finalPositionA, ref Vector3d finalPositionB,
            ref Quaternion finalOrientationA, ref Quaternion finalOrientationB, out Vector3d nextPositionA, out Vector3d nextPositionB,
            out Quaternion nextOrientationA, out Quaternion nextOrientationB, out double timeOfImpact)
        {
            double single1;
            Vector3d vector9;
            bool flag1 = true;
            Vector3d vector1 = finalPositionA - originalPositionA;
            Vector3d vector2 = finalPositionB - originalPositionB;
            Vector3d vector3 = originalPositionA;
            Vector3d vector4 = originalPositionB;
            Vector3d vector5 = Vector3d.TransformCoord(objA.myCenterOfMassOffset, objA.myInternalOrientationMatrix);
            Vector3d vector6 = Vector3d.TransformCoord(objB.myCenterOfMassOffset, objB.myInternalOrientationMatrix);
            Vector3d vector7 = originalPositionA - vector5;
            Vector3d vector8 = originalPositionB - vector6;

            double single2 = 2.00d * (Math.Acos(Math.Min(1.00d, Math.Abs(Quaternion.DotProduct(originalOrientationA, finalOrientationA)))));
            double single3 = 2.00d * (Math.Acos(Math.Min(1.00d, Math.Abs(Quaternion.DotProduct(originalOrientationB, finalOrientationB)))));
            Quaternion quaternion1 = originalOrientationA;
            Quaternion quaternion2 = originalOrientationB;
            timeOfImpact = 0.00d;

            if (!areObjectsCollidingMPR(objA, objB, ref vector7, ref vector8, ref quaternion1, ref quaternion2, out single1, out vector9))
            {
                Vector3d vector10 = vector1 - vector2;
                double single5 = Vector3d.DotProduct(vector10, vector9);
                double single4 = Math.Abs(((single5 + (single2 * objA.CollisionPrimitive.myMaximumRadius)) + (single3 * objB.CollisionPrimitive.myMaximumRadius)));
                do
                {
                    if (single1 < 0.01F)
                    {
                        single1 = 0.01F;
                    }
                    timeOfImpact += single1 / single4;
                    if (timeOfImpact >= 1.00F)
                    {
                        nextPositionA = finalPositionA;
                        nextPositionB = finalPositionB;
                        nextOrientationA = finalOrientationA;
                        nextOrientationB = finalOrientationB;
                        timeOfImpact = 1.00F;
                        flag1 = false;
                        break;
                    }
                    vector3 = Vector3d.Lerp(originalPositionA, finalPositionA, timeOfImpact);
                    vector4 = Vector3d.Lerp(originalPositionB, finalPositionB, timeOfImpact);

                    quaternion1 = Quaternion.Slerp(originalOrientationA, finalOrientationA, timeOfImpact);
                    quaternion2 = Quaternion.Slerp(originalOrientationB, finalOrientationB, timeOfImpact);
                    vector5 = Vector3d.TransformCoord(objA.myCenterOfMassOffset, quaternion1);
                    vector6 = Vector3d.TransformCoord(objB.myCenterOfMassOffset, quaternion2);
                    vector7 = vector3 - vector5;
                    vector8 = vector4 - vector6;
                }
                while (!areObjectsCollidingMPR(objA, objB, ref vector7, ref vector8, ref quaternion1, ref quaternion2, out single1, out vector9));
                
                if (timeOfImpact <= 1.00F)
                {
                    double single6 = timeOfImpact;
                    double single7 = 1.00F - single6;
                    vector3 = (originalPositionA * single7) + (finalPositionA * single6);
                    vector4 = (originalPositionB * single7) + (finalPositionB * single6);
                    quaternion1 = Quaternion.Slerp(originalOrientationA, finalOrientationA, single6);
                    quaternion2 = Quaternion.Slerp(originalOrientationB, finalOrientationB, single6);
                }
            }
            nextPositionA = vector3;
            nextPositionB = vector4;
            nextOrientationA = quaternion1;
            nextOrientationB = quaternion2;
            return flag1;
        }

        public static bool areSweptObjectsCollidingCA(PhysicsBody objA, PhysicsBody objB, ref Vector3d originalPositionA, ref Vector3d originalPositionB,
            ref Quaternion originalOrientationA, ref Quaternion originalOrientationB, ref Vector3d finalPositionA, ref Vector3d finalPositionB, 
            out Vector3d nextPositionA, out Vector3d nextPositionB, out double timeOfImpact)
        {
            double single1;
            Vector3d vector9;
            bool flag1 = true;
            Vector3d vector1 = finalPositionA - originalPositionA;
            Vector3d vector2 = finalPositionB - originalPositionB;
            Vector3d vector3 = originalPositionA;
            Vector3d vector4 = originalPositionB;
            Vector3d vector5 = Vector3d.TransformCoord(objA.myCenterOfMassOffset, objA.myInternalOrientationMatrix);
            Vector3d vector6 = Vector3d.TransformCoord(objB.myCenterOfMassOffset, objB.myInternalOrientationMatrix);
            Vector3d vector7 = originalPositionA - vector5;
            Vector3d vector8 = originalPositionB - vector6;
            timeOfImpact = 0.00d;
            
            if (!areObjectsCollidingMPR(objA, objB, ref vector7, ref vector8, ref originalOrientationA, ref originalOrientationB, out single1, out vector9))
            {
                Vector3d vector10 = vector1 - vector2;
                double value = Vector3d.DotProduct(vector10, vector9);
                double single2 = Math.Abs(value);
                do
                {
                    if (single1 < 0.01F)
                    {
                        single1 = 0.01F;
                    }
                    timeOfImpact += single1 / single2;
                    if (timeOfImpact >= 1.00F)
                    {
                        nextPositionA = finalPositionA;
                        nextPositionB = finalPositionB;
                        timeOfImpact = 1.00F;
                        flag1 = false;
                        break;
                    }
                    vector3 = Vector3d.Lerp(originalPositionA, finalPositionA, timeOfImpact);
                    vector4 = Vector3d.Lerp(originalPositionB, finalPositionB, timeOfImpact);
                    vector7 = vector3 - vector5;
                    vector8 = vector4 - vector6;
                }
                while (!areObjectsCollidingMPR(objA, objB, ref vector7, ref vector8, ref originalOrientationA, ref originalOrientationB, out single1, out vector9));
                if (timeOfImpact <= 1.00F)
                {
                    vector3 = Vector3d.Lerp(originalPositionA, finalPositionA, timeOfImpact);
                    vector4 = Vector3d.Lerp(originalPositionB, finalPositionB, timeOfImpact);
                }
            }
            nextPositionA = vector3;
            nextPositionB = vector4;
            return flag1;
        }

		public static bool areObjectsCollidingMPR (PhysicsBody objA, PhysicsBody objB, double marginA, double marginB, out Vector3d contactLocation, 
            out Vector3d contactNormal, out double depth)
		{
            return areObjectsCollidingMPR(objA, objB, ref objA.CollisionPrimitive.CenterPosition, ref objB.CollisionPrimitive.CenterPosition, ref objA.myInternalOrientationQuaternion, ref objB.myInternalOrientationQuaternion, marginA, marginB, out contactLocation, out contactNormal, out depth);
		}
		
		public static bool areObjectsCollidingMPR (PhysicsBody objA, PhysicsBody objB, ref Vector3d positionA, ref Vector3d positionB, 
            ref Quaternion orientationA, ref Quaternion orientationB, double marginA, double marginB, out Vector3d contactLocation, 
            out Vector3d contactNormal, out double depth)
		{
		    Vector3d vector6;
			Vector3d vector7;
		    Vector3d vector11;
			Vector3d vector12;
		    Vector3d vector16;
		    Vector3d vector19;
			Vector3d vector20;
		    Vector3d vector24;
			Vector3d vector25;
		    contactLocation = noVector;
			contactNormal = noVector;
			depth = double.NegativeInfinity;
			bool flag1 = false;
			Vector3d vector1 = positionA;
			Vector3d vector2 = positionB;
			Vector3d vector3 = vector2-vector1;
			if (vector3 == zeroVector)
			{
				flag1 = true;
				vector3 = new Vector3d(0.00d, 0.00d, 0.00d);
			}
            Vector3d vector4 = -vector3;
			Vector3d vector5 = vector3;
            objA.CollisionPrimitive.getExtremePoint(ref vector5, ref positionA, ref orientationA, marginA, out vector6);
            objB.CollisionPrimitive.getExtremePoint(ref vector4, ref positionB, ref orientationB, marginB, out vector7);
			Vector3d vector8 = vector7-vector6;
			if (!flag1 && (Vector3d.DotProduct(vector8, vector4) <= 0.00d))
			{
				return false;
			}
			vector4=Vector3d.CrossProduct(vector8,vector3);
			if (vector4 == zeroVector)
			{
			    Vector3d vector9 = vector8-vector3;
				vector4 = Vector3d.Normalize(vector9);
				contactNormal = vector4;
				Vector3d vector10 = vector6+vector7;
				contactLocation=vector10* 0.50F;
				depth=Vector3d.DotProduct(vector4,vector8);
				return true;
			}
            vector5 = Vector3d.Negate(vector4);
            objA.CollisionPrimitive.getExtremePoint(ref vector5, ref positionA, ref orientationA, marginA, out vector11);
            objB.CollisionPrimitive.getExtremePoint(ref vector4, ref positionB, ref orientationB, marginB, out vector12);
			Vector3d vector13 = vector12-vector11;
			if (!flag1 && (Vector3d.DotProduct(vector13, vector4) <= 0.00d))
			{
				return false;
			}
			Vector3d vector14 = vector8-vector3;
			Vector3d vector15 = vector13-vector3;
			vector4=Vector3d.CrossProduct(vector14,vector15);
			double distance = Vector3d.DotProduct(vector4,vector3);
			if (distance > 0.00d)
			{
				Vector3d vector18 = vector8;
				vector8 = vector13;
				vector13 = vector18;
				vector18 = vector6;
				vector6 = vector11;
				vector11 = vector18;
				vector18 = vector7;
				vector7 = vector12;
				vector12 = vector18;
                vector4 = Vector3d.Negate( vector4);
			}
		Label_01B4:
            vector5 = Vector3d.Negate(vector4);
            objA.CollisionPrimitive.getExtremePoint(ref vector5, ref positionA, ref orientationA, marginA, out vector19);
            objB.CollisionPrimitive.getExtremePoint(ref vector4, ref positionB, ref orientationB, marginB, out vector20);
			Vector3d vector21 = vector20-vector19;
			if (!flag1 && (Vector3d.DotProduct(vector21, vector4) <= 0.00d))
			{
				return false;
			}
			Vector3d vector17 = Vector3d.CrossProduct(vector8,vector21);
			distance=Vector3d.DotProduct(vector17,vector3);
			if (distance < 0.00d)
			{
				vector13 = vector21;
				vector11 = vector19;
				vector12 = vector20;
				vector14=vector8-vector3;
				vector16=vector21-vector3;
				vector4=Vector3d.CrossProduct(vector14,vector16);
				goto Label_01B4;
			}
			vector17=Vector3d.CrossProduct(vector21,vector13);
			distance=Vector3d.DotProduct(vector17,vector3);
			if (distance < 0.00d)
			{
				vector8 = vector21;
				vector6 = vector19;
				vector7 = vector20;
				vector15=vector13-vector3;
				vector16=vector21-vector3;
				vector4=Vector3d.CrossProduct(vector15,vector16);
				goto Label_01B4;
			}
		    while(true)
		    {
		        Vector3d vector22 = vector13 - vector8;
		        Vector3d vector23 = vector21 - vector8;
		        vector4 = Vector3d.CrossProduct(vector22, vector23);
		        if (!flag1)
		        {
		            distance = Vector3d.DotProduct(vector4, vector8);
		            flag1 = distance >= 0.00d;
		        }
		        vector5 = Vector3d.Negate(vector4);
		        objA.CollisionPrimitive.getExtremePoint(ref vector5, ref positionA, ref orientationA, marginA, out vector24);
		        objB.CollisionPrimitive.getExtremePoint(ref vector4, ref positionB, ref orientationB, marginB, out vector25);
		        Vector3d vector26 = vector25 - vector24;
		        if (!flag1 && (-Vector3d.DotProduct(vector26, vector4) >= 0.00d))
		        {
		            return false;
		        }
		        if (Vector3d.DotProduct(vector26 - vector21, vector4) <=
		            Math.Max((float) (0.00d*vector4.LengthSquared()), (float) 0.00d))
		        {
		            if (flag1)
		            {
		                contactNormal = Vector3d.Normalize(vector4);
		                vector14 = vector8 - vector3;
		                vector15 = vector13 - vector3;
		                vector16 = vector21 - vector3;
		                double single2 = Vector3d.DotProduct(Vector3d.CrossProduct(vector14, vector15), vector16);
		                double single3 = Vector3d.DotProduct(Vector3d.CrossProduct(vector8, vector13), vector21);
		                double single4 = Vector3d.DotProduct(Vector3d.CrossProduct(-(vector3), vector15), vector16);
		                double single5 = Vector3d.DotProduct(Vector3d.CrossProduct(vector14, -(vector3)), vector16);
		                double single6 = 1.00F/single2;
		                double single7 = single3*single6;
		                double single8 = single4*single6;
		                double single9 = single5*single6;
		                double single10 = ((1.00F - single7) - single8) - single9;
		                Vector3d vector27 = (single7*vector1) + (single8*vector6) + (single9*vector11) + (single10*vector19);
		                Vector3d vector28 = (single7*vector2) + (single8*vector7) + (single9*vector12) + (single10*vector20);
		                contactLocation = ((vector27 + vector28)/2.00d);
		                depth = findPenetrationDepth(objB, objA, ref positionB, ref positionA, ref orientationB,
		                                             ref orientationA,
		                                             objB.CollisionPrimitive.myCollisionMargin,
		                                             objA.CollisionPrimitive.myCollisionMargin, contactNormal);
		                return true;
		            }
		            return false;
		        }
		        vector17 = Vector3d.CrossProduct(vector26, vector3);
		        distance = Vector3d.DotProduct(vector8, vector17);
		        if (distance > 0.00d)
		        {
		            distance = Vector3d.DotProduct(vector13, vector17);
		            if (distance > 0.00d)
		            {
		                vector8 = vector26;
		                vector6 = vector24;
		                vector7 = vector25;
                        continue;
		            }
		            vector21 = vector26;
		            vector19 = vector24;
		            vector20 = vector25;
                    continue;
		        }
		        distance = Vector3d.DotProduct(vector21, vector17);
		        if (distance > 0.00d)
		        {
		            vector13 = vector26;
		            vector11 = vector24;
		            vector12 = vector25;
		            continue;
		        }
		        vector8 = vector26;
		        vector6 = vector24;
		        vector7 = vector25;
		    }
		}

        private static double findSweptPenetrationDepth(PhysicsBody objA, PhysicsBody objB, ref Vector3d sweepA, ref Vector3d sweepB, 
            ref Vector3d positionA, ref Vector3d positionB, ref Quaternion orientationA, ref Quaternion orientationB, double marginA, 
            double marginB, Vector3d normal)
        {
            normal.Normalize();
            Vector3d vector1 = objB.myInternalCenterPosition - objA.myInternalCenterPosition;
            Vector3d vector2 = normal;
            Vector3d vector3 = findMinkowskiDifferenceExtremePoint(objB, objA, ref sweepB, ref sweepA, ref vector2, ref positionB, ref positionA, ref orientationB, ref orientationA, marginB, marginA);
            if (Vector3d.DotProduct(vector3, vector2) <= 0.00d)
            {
                return double.NegativeInfinity;
            }
            vector2 = Vector3d.CrossProduct(vector3, vector1);
            if (vector2 == zeroVector)
            {
                vector3 += new Vector3d(0.00d, 0.00d, 0.00d);
                vector2 = Vector3d.CrossProduct(vector3, vector1);
                if (vector2 == zeroVector)
                {
                    vector3 += new Vector3d(0.00d, 0.00d, 0.00d);
                    vector2 = Vector3d.CrossProduct(vector3, vector1);
                }
            }
            Vector3d vector4 = findMinkowskiDifferenceExtremePoint(objB, objA, ref sweepB, ref sweepA, ref vector2, ref positionB, ref positionA, ref orientationB, ref orientationA, marginB, marginA);
            if (Vector3d.DotProduct(vector4, vector2) <= 0.00d)
            {
                return double.NegativeInfinity;
            }
            vector2 = Vector3d.CrossProduct(vector3 - vector1, vector4 - vector1);
            if (Vector3d.DotProduct(vector2, vector1) > 0.00d)
            {
                Vector3d vector5 = vector3;
                vector3 = vector4;
                vector4 = vector5;
                vector2 = -(vector2);
            }
        Label_010D:
            Vector3d vector6 = findMinkowskiDifferenceExtremePoint(objB, objA, ref sweepB, ref sweepA, ref vector2, ref positionB, ref positionA, ref orientationB, ref orientationA, marginB, marginA);
            if (Vector3d.DotProduct(vector6, vector2) <= 0.00d)
            {
                return double.NegativeInfinity;
            }
            if (Vector3d.DotProduct(Vector3d.CrossProduct(vector3, vector6), vector1) < 0.00d)
            {
                vector4 = vector6;
                vector2 = Vector3d.CrossProduct(vector3 - vector1, vector6 - vector1);
                goto Label_010D;
            }
            if (Vector3d.DotProduct(Vector3d.CrossProduct(vector6, vector4), vector1) < 0.00d)
            {
                vector3 = vector6;
                vector2 = Vector3d.CrossProduct(vector6 - vector1, vector4 - vector1);
                goto Label_010D;
            }
        Label_019C:
            vector2 = Vector3d.CrossProduct(vector4 - vector3, vector6 - vector3);
            vector2.Normalize();
            double single1 = Math.Abs(Vector3d.DotProduct(vector2, normal));
            if ((single1 > 1.00F) && (single1 < 1.00F))
            {
                vector2.Normalize();
                single1 = Vector3d.DotProduct(vector6, vector2);
                if (single1 < 0.00d)
                {
                    return -single1;
                }
                return single1;
            }
            Vector3d vector7 = findMinkowskiDifferenceExtremePoint(objB, objA, ref sweepB, ref sweepA, ref vector2, ref positionB, ref positionA, ref orientationB, ref orientationA, marginB, marginA);
            if (Vector3d.DotProduct(vector7, vector2) >= 0.00d)
            {
                return double.NegativeInfinity;
            }
            if (Vector3d.DotProduct(vector7 - vector6, vector2) <= 0.00d)
            {
                single1 = Vector3d.DotProduct(vector6, vector2);
                if (single1 < 0.00d)
                {
                    return -single1;
                }
                return single1;
            }
            Vector3d vector8 = Vector3d.CrossProduct(vector7, vector1);
            if (Vector3d.DotProduct(vector3, vector8) > 0.00d)
            {
                if (Vector3d.DotProduct(vector4, vector8) > 0.00d)
                {
                    vector3 = vector7;
                    goto Label_019C;
                }
                vector6 = vector7;
                goto Label_019C;
            }
            if (Vector3d.DotProduct(vector6, vector8) > 0.00d)
            {
                vector4 = vector7;
                goto Label_019C;
            }
            vector3 = vector7;
            goto Label_019C;
        }

		public static double findPenetrationDepth (PhysicsBody objA, PhysicsBody objB, double marginA, double marginB, Vector3d normal)
		{
            return findPenetrationDepth(objA, objB, ref objA.CollisionPrimitive.CenterPosition, ref objB.CollisionPrimitive.CenterPosition, 
                ref objA.myInternalOrientationQuaternion, ref objB.myInternalOrientationQuaternion, marginA, marginB, normal);
		}
		
		public static double findPenetrationDepth (PhysicsBody objA, PhysicsBody objB, ref Vector3d positionA, ref Vector3d positionB,
            ref Quaternion orientationA, ref Quaternion orientationB, double marginA, double marginB, Vector3d normal)
		{
		    Vector3d vector7;
		    Vector3d vector1 = objB.myInternalCenterPosition-objA.myInternalCenterPosition;
			Vector3d vector2 = normal;
			Vector3d vector3 = findMinkowskiDifferenceExtremePoint(objB, objA, ref vector2, ref positionB, ref positionA, ref orientationB, ref orientationA, marginB, marginA);
			double value = Vector3d.DotProduct(vector3,vector2);
			if (value <= 0.00d)
			{
                return double.NegativeInfinity;
			}
			vector2=Vector3d.CrossProduct(vector3,vector1);
			if (vector2 == zeroVector)
			{
				vector1.x -= 0.00d;
				vector1.y -= 0.00d;
				vector1.z -= 0.00d;
				vector3.x += 0.00d;
				vector3.y += 0.00d;
				vector3.z += 0.00d;
				vector2=Vector3d.CrossProduct(vector3,vector1);
				if (vector2 == zeroVector)
				{
					vector1.x += 0.00d;
					vector1.y += 0.00d;
					vector1.z += 0.00d;
					vector3.x -= 0.00d;
					vector3.y -= 0.00d;
					vector3.z -= 0.00d;
					vector3 += new Vector3d(0.00d, 0.00d, 0.00d);
					vector2=Vector3d.CrossProduct(vector3,vector1);
				}
			}
			Vector3d vector4 = findMinkowskiDifferenceExtremePoint(objB, objA, ref vector2, ref positionB, ref positionA, ref orientationB, ref orientationA, marginB, marginA);
			value=Vector3d.DotProduct(vector4,vector2);
			if (value <= 0.00d)
			{
				return double.NegativeInfinity;
			}
			Vector3d vector5 = vector3-vector1;
			Vector3d vector6 = vector4-vector1;
			vector2=Vector3d.CrossProduct(vector5,vector6);
			value=Vector3d.DotProduct(vector2,vector1);
			if (value > 0.00d)
			{
				Vector3d vector8 = vector3;
				vector3 = vector4;
				vector4 = vector8;
                vector2 = Vector3d.Negate( vector2);
			}
		Label_01F8:
			Vector3d vector10 = findMinkowskiDifferenceExtremePoint(objB, objA, ref vector2, ref positionB, ref positionA, ref orientationB, ref orientationA, marginB, marginA);
			value=Vector3d.DotProduct(vector10,vector2);
			if (value <= 0.00d)
			{
                return double.NegativeInfinity;
			}
			Vector3d vector9 = Vector3d.CrossProduct(vector3,vector10);
			value=Vector3d.DotProduct(vector9,vector1);
			if (value < 0.00d)
			{
				vector4 = vector10;
				vector5=vector3-vector1;
				vector7=vector10-vector1;
				vector2=Vector3d.CrossProduct(vector5,vector7);
				goto Label_01F8;
			}
			vector9=Vector3d.CrossProduct(vector10,vector4);
			value=Vector3d.DotProduct(vector9,vector1);
			if (value < 0.00d)
			{
				vector3 = vector10;
				vector7=vector10-vector1;
				vector6=vector4-vector1;
				vector2=Vector3d.CrossProduct(vector7,vector6);
				goto Label_01F8;
			}
		Label_02B2:
			Vector3d vector11 = vector4-vector3;
			Vector3d vector12 = vector10-vector3;
			vector2=Vector3d.CrossProduct(vector11,vector12);
			vector2.Normalize();
			value=Vector3d.DotProduct(vector2,normal);
			value = Math.Abs(value);
			if ((value > 1.00F) && (value < 1.00F))
			{
				value=Vector3d.DotProduct(vector10,vector2);
				if (value < 0.00d)
				{
					return -value;
				}
				return value;
			}
			Vector3d vector13 = findMinkowskiDifferenceExtremePoint(objB, objA, ref vector2, ref positionB, ref positionA, ref orientationB, ref orientationA, marginB, marginA);
			value=Vector3d.DotProduct(vector13,vector2);
			if (-value >= 0.00d)
			{
                return double.NegativeInfinity;
			}
			Vector3d vector14 = vector13-vector10;
			value=Vector3d.DotProduct(vector14,vector2);
			if (value <= 0.00d)
			{
				value=Vector3d.DotProduct(vector10,vector2);
				if (value < 0.00d)
				{
					return -value;
				}
				return value;
			}
			vector9=Vector3d.CrossProduct(vector13,vector1);
			value=Vector3d.DotProduct(vector3,vector9);
			if (value > 0.00d)
			{
				value=Vector3d.DotProduct(vector4,vector9);
				if (value > 0.00d)
				{
					vector3 = vector13;
					goto Label_02B2;
				}
				vector10 = vector13;
				goto Label_02B2;
			}
			value=Vector3d.DotProduct(vector10,vector9);
			if (value > 0.00d)
			{
				vector4 = vector13;
				goto Label_02B2;
			}
			vector3 = vector13;
			goto Label_02B2;
		}
		
		public static double findConservativeDistanceEstimate (PhysicsBody objA, PhysicsBody objB, double marginA, double marginB, 
            out Vector3d separatingDirection)
		{
            return findConservativeDistanceEstimate(objA, objB, ref objA.CollisionPrimitive.CenterPosition, ref objB.CollisionPrimitive.CenterPosition,
                ref objA.myInternalOrientationQuaternion, ref objB.myInternalOrientationQuaternion, marginA, marginB, out separatingDirection);
		}
		
		public static double findConservativeDistanceEstimate (PhysicsBody objA, PhysicsBody objB, ref Vector3d positionA, ref Vector3d positionB, 
            ref Quaternion orientationA, ref Quaternion orientationB, double marginA, double marginB, out Vector3d separatingDirection)
		{
		    Vector3d vector7;
		    separatingDirection = noVector;
            double num = 0d;
            Vector3d vector = positionB - positionA;
            if (vector == zeroVector)
            {
                return 0f;
            }
            Vector3d vector2 = -vector;
            Vector3d vector3 = findMinkowskiDifferenceExtremePoint(objB, objA, ref vector2, ref positionB, ref positionA, ref orientationB, ref orientationA, marginB, marginA);
            double num2 = Vector3d.DotProduct(vector3, vector2);
            if (num2 <= 0.0001d)
            {
                double num3 = vector2.Length;
                vector2 = vector2 / num3;
                separatingDirection =  -vector2;
                if (num2 > 0d)
                {
                    return 0d;
                }
                return (-num2 / num3);
            }
            vector2 = Vector3d.CrossProduct(vector3, vector);
            if (vector2.LengthSquared() < 1E-07f)
            {
                return 0d;
            }
            Vector3d vector4 = findMinkowskiDifferenceExtremePoint(objB, objA, ref vector2, ref positionB, ref positionA, ref orientationB, ref orientationA, marginB, marginA);
            num2 = Vector3d.DotProduct(vector4, vector2);
            if (num2 <= 0f)
            {
                vector2.Normalize();
                separatingDirection = vector2;
                num = Vector3d.DotProduct(vector4, vector2);
                if (num < 0f)
                {
                    num *= -1f;
                    separatingDirection =  -separatingDirection;
                }
                return num;
            }
            Vector3d vector5 = vector3 - vector;
            Vector3d vector6 = vector4 - vector;
            vector2 = Vector3d.CrossProduct(vector5, vector6);
            if (vector2 == zeroVector)
            {
                return 0f;
            }
            num2 = Vector3d.DotProduct(vector, vector2);
            if (num2 > 0f)
            {
                Vector3d vector8 = vector3;
                vector3 = vector4;
                vector4 = vector8;
                vector2 = -vector2;
            }


        Label_016A:
            Vector3d vector10 = findMinkowskiDifferenceExtremePoint(objB, objA, ref vector2, ref positionB, ref positionA, ref orientationB, ref orientationA, marginB, marginA);
            num2 =Vector3d.DotProduct(vector10, vector2);
            if (num2 <= 0f)
            {
                vector2.Normalize();
                separatingDirection = vector2;
                num = Vector3d.DotProduct(vector10, vector2);
                if (num < 0f)
                {
                    num *= -1f;
                    separatingDirection = -separatingDirection;
                }
                return num;
            }
            Vector3d vector9 = Vector3d.CrossProduct( vector3, vector10);
            num2 =Vector3d.DotProduct(vector9, vector);
            if (num2 < -1E-07f)
            {
                vector4 = vector10;
                vector5 =vector3 - vector;
                vector7 = vector10 - vector;
                vector2 =Vector3d.CrossProduct(vector5, vector7);
                goto Label_016A;
            }
            vector9 =Vector3d.CrossProduct(vector10, vector4 );
            num2 =Vector3d.DotProduct(vector9, vector);
            if (num2 < -1E-07f)
            {
                vector3 = vector10;
                vector6 = vector4 -  vector;
                vector7 =vector10 - vector;
                vector2 = Vector3d.CrossProduct(vector6, vector7);
                goto Label_016A;
            }

            while (true)
            {
                Vector3d vector11;
                Vector3d vector12;
                Vector3d vector13;
                bool flag = false;
                vector11= vector4 - vector3;
                vector12  = vector10 - vector3;
                vector2 =Vector3d.CrossProduct(vector11, vector12);
                double num4 = vector2.LengthSquared();
                if (!flag && (Vector3d.DotProduct(vector2, vector3) >= 0f))
                {
                    return 0f;
                }
                Vector3d vector14 = findMinkowskiDifferenceExtremePoint(objB, objA, ref vector2, ref positionB, ref positionA, ref orientationB, ref orientationA, marginB, marginA);
                if (!flag && (-Vector3d.DotProduct(vector14, vector2) >= 0f))
                {
                    flag = -num2 >= 0f;
                }
                vector13 = vector14 - vector10;
                num2 =Vector3d.DotProduct(vector13, vector2);
                if (num2 <= Math.Max((1E-05f * num4), 1E-05f))
                {
                    if (num4 > 0f)
                    {
                        vector2 = vector2 / (Math.Sqrt(num4));
                    }
                    separatingDirection = vector2;
                    num = Vector3d.DotProduct(vector10, vector2 );
                    if (num < 0f)
                    {
                        num *= -1f;
                        separatingDirection = -separatingDirection;
                    }
                    return num;
                }
                vector9 =Vector3d.CrossProduct(vector14, vector);
                num2 =Vector3d.DotProduct(vector3, vector9 );
                if (num2 > 0f)
                {
                    num2 =Vector3d.DotProduct(vector4, vector9);
                    if (num2 > 0f)
                    {
                        vector3 = vector14;
                    }
                    else
                    {
                        vector10 = vector14;
                    }
                }
                else
                {
                    num2 =Vector3d.DotProduct(vector10, vector9 );
                    if (num2 > 0f)
                    {
                        vector4 = vector14;
                    }
                    else
                    {
                        vector3 = vector14;
                    }
                }
            }


        //    Vector3d vector7;
        //    separatingDirection = noVector;
        //    double single1 = 0.00d;
        //    Vector3d vector1 = positionB-positionA;
        //    if (vector1 == zeroVector)
        //    {
        //        return 0.00d;
        //    }
        //    Vector3d vector2 = Vector3d.Inverse(vector1);
        //    Vector3d vector3 = findMinkowskiDifferenceExtremePoint(objB, objA, ref vector2, ref positionB, ref positionA, ref orientationB, ref orientationA, marginB, marginA);
        //    double single2 = Vector3d.DotProduct(vector3,vector2);
        //    if (single2 <= 0.00d)
        //    {
        //        double single3 = vector2.Length;
        //        vector2 =  (vector2 / single3);
        //        separatingDirection = Vector3d.Inverse(vector2);
        //        if (single2 > 0.00d)
        //        {
        //            return 0.00d;
        //        }
        //        return -single2 / single3;
        //    }
        //    vector2=Vector3d.CrossProduct(vector3,vector1);
        //    if (vector2.LengthSquared() < 0.00d)
        //    {
        //        return 0.00d;
        //    }
        //    Vector3d vector4 = findMinkowskiDifferenceExtremePoint(objB, objA, ref vector2, ref positionB, ref positionA, ref orientationB, ref orientationA, marginB, marginA);
        //    single2=Vector3d.DotProduct(vector4,vector2);
        //    if (single2 <= 0.00d)
        //    {
        //        vector2.Normalize();
        //        separatingDirection = vector2;
        //        single1=Vector3d.DotProduct(vector4,vector2);
        //        if (single1 < 0.00d)
        //        {
        //            single1 *= -1.00F;
        //            separatingDirection = Vector3d.Inverse(separatingDirection);
        //        }
        //        return single1;
        //    }
        //    Vector3d vector5 = vector3-vector1;
        //    Vector3d vector6 = vector4-vector1;
        //    vector2=Vector3d.CrossProduct(vector5,vector6);
        //    if (vector2 == zeroVector)
        //    {
        //        return 0.00d;
        //    }
        //    single2=Vector3d.DotProduct(vector1,vector2);
        //    if (single2 > 0.00d)
        //    {
        //        Vector3d vector8 = vector3;
        //        vector3 = vector4;
        //        vector4 = vector8;
        //        vector2 = Vector3d.Inverse(vector2);
        //    }
        //Label_016A:
        //    Vector3d vector10 = findMinkowskiDifferenceExtremePoint(objB, objA, ref vector2, ref positionB, ref positionA, ref orientationB, ref orientationA, marginB, marginA);
        //    single2=Vector3d.DotProduct(vector10,vector2);
        //    if (single2 <= 0.00d)
        //    {
        //        vector2.Normalize();
        //        separatingDirection = vector2;
        //        single1 = Vector3d.DotProduct(vector10, vector2);
        //        if (single1 < 0.00d)
        //        {
        //            single1 *= -1.00F;
        //            separatingDirection = Vector3d.Inverse(separatingDirection );
        //        }
        //        return single1;
        //    }
        //    Vector3d vector9 = Vector3d.CrossProduct(vector3,vector10);
        //    single2=Vector3d.DotProduct(vector9,vector1);
        //    if (single2 < 0.00d)
        //    {
        //        vector4 = vector10;
        //        vector5=vector3-vector1;
        //        vector7=vector10-vector1;
        //        vector2=Vector3d.CrossProduct(vector5,vector7);
        //        goto Label_016A;
        //    }
        //    vector9=Vector3d.CrossProduct(vector10,vector4);
        //    single2=Vector3d.DotProduct(vector9,vector1);
        //    if (single2 < 0.00d)
        //    {
        //        vector3 = vector10;
        //        vector6=vector4-vector1;
        //        vector7=vector10-vector1;
        //        vector2=Vector3d.CrossProduct(vector6,vector7);
        //        goto Label_016A;
        //    }
        //Label_0257:
        //    bool flag1 = false;
        //    Vector3d vector11 = vector4-vector3;
        //    Vector3d vector12 = vector10-vector3;
        //    vector2=Vector3d.CrossProduct(vector11,vector12);
        //    double single4 = vector2.LengthSquared();
        //    if (!flag1 && (Vector3d.DotProduct(vector2, vector3) >= 0.00d))
        //    {
        //        return 0.00d;
        //    }
        //    Vector3d vector14 = findMinkowskiDifferenceExtremePoint(objB, objA, ref vector2, ref positionB, ref positionA, ref orientationB, ref orientationA, marginB, marginA);
        //    if (!flag1 && (-Vector3d.DotProduct(vector14, vector2) >= 0.00d))
        //    {
        //        flag1 = -single2 >= 0.00d;
        //    }
        //    Vector3d vector13 = vector14-vector10;
        //    single2=Vector3d.DotProduct(vector13,vector2);
        //    if (single2 <= Math.Max( (0.00d * single4),  0.00d))
        //    {
        //        if (single4 > 0.00d)
        //        {
        //            vector2 =  vector2 /  Math.Sqrt( single4);
        //        }
        //        separatingDirection = vector2;
        //        single1=Vector3d.DotProduct(vector10,vector2);
        //        if (single1 < 0.00d)
        //        {
        //            single1 *= -1.00F;
        //            separatingDirection = Vector3d.Inverse(separatingDirection );
        //        }
        //        return single1;
        //    }
        //    vector9=Vector3d.CrossProduct(vector14,vector1);
        //    single2=Vector3d.DotProduct(vector3,vector9);
        //    if (single2 > 0.00d)
        //    {
        //        single2=Vector3d.DotProduct(vector4,vector9);
        //        if (single2 > 0.00d)
        //        {
        //            vector3 = vector14;
        //            goto Label_0257;
        //        }
        //        vector10 = vector14;
        //        goto Label_0257;
        //    }
        //    single2=Vector3d.DotProduct(vector10,vector9);
        //    if (single2 > 0.00d)
        //    {
        //        vector4 = vector14;
        //        goto Label_0257;
        //    }
        //    vector3 = vector14;
        //    goto Label_0257;
		}
		
		
		public static Vector3d findExtremePointWithOffset (PhysicsBody a, PhysicsBody b, Vector3d axis, double marginA, double marginB,
            Vector3d offsetA, Vector3d offsetB)
		{
            Vector3d vector1 = Vector3d.Negate(axis);
			Vector3d vector2;
			Vector3d vector3;
            a.CollisionPrimitive.getExtremePoint(ref axis, marginA, out vector2);
            b.CollisionPrimitive.getExtremePoint(ref vector1, marginB, out vector3);
            vector2 += offsetA;
			vector3 += offsetB;
			return vector2 - vector3;
		}
		
		public static Vector3d findMinkowskiDifferenceExtremePoint (PhysicsBody a, PhysicsBody b, Vector3d axis, double marginA, double marginB)
		{
            Vector3d vector1 = Vector3d.Negate(axis);
			Vector3d vector2;
			Vector3d vector3;
            a.CollisionPrimitive.getExtremePoint(ref axis, marginA, out vector2);
            b.CollisionPrimitive.getExtremePoint(ref vector1, marginB, out vector3);
			Vector3d vector4 = vector2-vector3;
			return vector4;
		}
		
		public static void findMinkowskiDifferenceExtremePoint (PhysicsBody a, PhysicsBody b, ref Vector3d axis, double marginA, double marginB, 
            out Vector3d extremePoint)
		{
		    extremePoint = findMinkowskiDifferenceExtremePoint(a, b, axis, marginA, marginB);
		}
		
		public static Vector3d findMinkowskiDifferenceExtremePoint (PhysicsBody a, PhysicsBody b, ref Vector3d axis, ref Vector3d positionA, 
            ref Vector3d positionB, ref Quaternion orientationA, ref Quaternion orientationB, double marginA, double marginB)
		{
			Vector3d vector1 = Vector3d.Negate(axis);
			Vector3d vector2;
			Vector3d vector3;

            a.CollisionPrimitive.getExtremePoint(ref axis, ref positionA, ref orientationA, marginA, out vector2);
            b.CollisionPrimitive.getExtremePoint(ref vector1, ref positionB, ref orientationB, marginB, out vector3);
			return vector2 - vector3;
		}
		
		public static Vector3d findMinkowskiDifferenceExtremePoint (PhysicsBody a, PhysicsBody b, ref Vector3d sweepA, ref Vector3d sweepB, 
            ref Vector3d axis, ref Vector3d positionA, ref Vector3d positionB, ref Quaternion orientationA, ref Quaternion orientationB, 
            double marginA, double marginB)
		{
			Vector3d vector1 = Vector3d.Negate(axis);
			Vector3d vector2;
			Vector3d vector3;
            a.CollisionPrimitive.getExtremePoint(ref axis, ref positionA, ref orientationA, marginA, out vector2);
			double single1 = Vector3d.DotProduct(axis, sweepA);
			if (single1 > 0.00d)
			{
				vector2 += sweepA;
			}
			else
			{
				if (single1 == 0.00d)
				{
					vector2 +=  ( sweepA * .5D);
				}
			}
            b.CollisionPrimitive.getExtremePoint(ref vector1, ref positionB, ref orientationB, marginB, out vector3);
			single1 = Vector3d.DotProduct(vector1, sweepB);
			if (single1 > 0.00d)
			{
				vector3 += sweepB;
			}
			else
			{
				if (single1 == 0.00d)
				{
					vector3 +=  (sweepB * .5d);
				}
			}
			return vector2 - vector3;
		}
		
		internal static void getBarycenter (List<Vector3d> q, List<double> baryCoords, out Vector3d barycenter)
		{
			barycenter = zeroVector;
			for (int i = 0;i < q.Count; i++)
			{
				Vector3d vector1 = q[i];
				vector1=vector1* baryCoords[i];
				barycenter=barycenter+vector1;
			}
		}
		
		public static bool rayCastGJKInfinite (Vector3d origin, Vector3d direction, PhysicsBody target, bool withMargin, 
            out Vector3d hitLocation, out Vector3d hitNormal, out double t)
		{
			if (target is CompoundBody)
			{
				return ((CompoundBody) target).rayTestInfinite(origin, direction, withMargin, out hitLocation, out hitNormal, out t);
			}
			t = 0.00d;
			hitLocation = origin;
			hitNormal = zeroVector;
			Vector3d vector3 = hitLocation - target.myInternalCenterPosition;
			List<Vector3d> list = ResourcePool.getVectorList();
			List<int> subsimplex = ResourcePool.getIntList();
            List<double> baryCoords = new List<double>();
			List<Vector3d> q = ResourcePool.getVectorList();
            double single3 = double.NegativeInfinity;
			double single4 = 0.00d;
			int num1 = 0;
			while (vector3.LengthSquared() > (single3 * 0.00d))
			{
				Vector3d item;
				num1++;
				if (num1 > 0x32)
				{
					hitLocation = noVector;
					hitNormal = noVector;
					ResourcePool.giveBack(list);
					ResourcePool.giveBack(subsimplex);
					ResourcePool.giveBack(q);
					return false;
				}
				if (withMargin)
				{
                    target.CollisionPrimitive.getExtremePoint(ref vector3, target.CollisionPrimitive.myCollisionMargin, out item);
				}
				else
				{
                    target.CollisionPrimitive.getExtremePoint(ref vector3, 0.00d, out item);
				}
				Vector3d vector1 = hitLocation - item;
				double single1 = Vector3d.DotProduct(vector3, vector1);
				if (single1 > 0.00d)
				{
					double single2 = Vector3d.DotProduct(vector3, direction);
					if (single2 >= 0.00d)
					{
						ResourcePool.giveBack(list);
						ResourcePool.giveBack(subsimplex);
						ResourcePool.giveBack(q);
						return false;
					}
					t -= single1 / single2;
					hitLocation = origin + (direction * t);
					hitNormal = vector3;
				}
				q.Clear();
				list.Add(item);
				for (int i = 0;i < list.Count; i++)
				{
					q.Add(hitLocation - list[i]);
				}
				findPointOfMinimumNorm(q, subsimplex, baryCoords, out vector3);
				q.Clear();
				for (int j = 0;j < subsimplex.Count; j++)
				{
					q.Add(list[subsimplex[j]]);
				}
				list.Clear();
				foreach (Vector3d vector4 in q)
				{
					list.Add(vector4);
				}
				single3 = float.NegativeInfinity;
				foreach (Vector3d vector5 in list)
				{
					Vector3d vector6 = vector5 - origin;
					single4 = vector6.LengthSquared();
					if (single4 > single3)
					{
						single3 = single4;
					}
				}
			}
			ResourcePool.giveBack(list);
			ResourcePool.giveBack(subsimplex);
			ResourcePool.giveBack(q);
			return true;
		}
		
		public static bool rayCastGJK (Vector3d origin, Vector3d direction, double length, PhysicsBody target, bool withMargin, 
            out Vector3d hitLocation, out Vector3d hitNormal, out double t)
		{
			if (target is CompoundBody)
			{
				return (target as CompoundBody).rayTest(origin, direction, length, withMargin, out hitLocation, out hitNormal, out t);
			}
			t = 0.00d;
			hitLocation = origin;
			hitNormal = zeroVector;
			Vector3d vector3 = hitLocation - target.myInternalCenterPosition;
			List<Vector3d> list = ResourcePool.getVectorList();
			List<int> subsimplex = ResourcePool.getIntList();
            List<double> baryCoords = new List<double>();
			List<Vector3d> q = ResourcePool.getVectorList();
			double single3 = double.NegativeInfinity;
			double single4 = 0.00d;
			int num1 = 0;
			while (vector3.LengthSquared() > (single3 * 0.00d))
			{
				Vector3d item;
				num1++;
				if (num1 > 0x32)
				{
					hitLocation = noVector;
					hitNormal = noVector;
					ResourcePool.giveBack(list);
					ResourcePool.giveBack(subsimplex);
					ResourcePool.giveBack(q);
					return false;
				}
				if (withMargin)
				{
					target.CollisionPrimitive.getExtremePoint(ref vector3, target.CollisionPrimitive.myCollisionMargin, out item);
				}
				else
				{
                    target.CollisionPrimitive.getExtremePoint(ref vector3, 0.00d, out item);
				}
				Vector3d vector1 = hitLocation - item;
				double single1 = Vector3d.DotProduct(vector3, vector1);
				if (single1 > 0.00d)
				{
					double single2 = Vector3d.DotProduct(vector3, direction);
					if (single2 >= 0.00d)
					{
						hitLocation = noVector;
						hitNormal = noVector;
						ResourcePool.giveBack(list);
						ResourcePool.giveBack(subsimplex);
						ResourcePool.giveBack(q);
						return false;
					}
					t -= single1 / single2;
					if (t > length)
					{
						hitLocation = noVector;
						hitNormal = noVector;
						ResourcePool.giveBack(list);
						ResourcePool.giveBack(subsimplex);
						ResourcePool.giveBack(q);
						return false;
					}
					hitLocation = origin + (direction * t);
					hitNormal = vector3;
				}
				q.Clear();
				list.Add(item);
				for (int i = 0;i < list.Count; i++)
				{
					q.Add(hitLocation - list[i]);
				}
				findPointOfMinimumNorm(q, subsimplex, baryCoords, out vector3);
				q.Clear();
				for (int j = 0;j < subsimplex.Count; j++)
				{
					q.Add(list[subsimplex[j]]);
				}
				list.Clear();
				foreach (Vector3d vector4 in q)
				{
					list.Add(vector4);
				}
				single3 = double.NegativeInfinity;
				foreach (Vector3d vector5 in list)
				{
					Vector3d vector6 = vector5 - origin;
					single4 = vector6.LengthSquared();
					if (single4 > single3)
					{
						single3 = single4;
					}
				}
			}
			ResourcePool.giveBack(list);
			ResourcePool.giveBack(subsimplex);
			ResourcePool.giveBack(q);
			return true;
		}

        // True Axis has swept collisions = http://www.trueaxis.com/
        // Solid collision library is at http://www.dtecta.com/
        // only costs 2k Euro for per game title.
        // GJK = Gilbert, Johnson, Keerthi algorithm is an itterative method for computing the distance between convex objects.
        // http://www.win.tue.nl/~gino/solid/jgt98convex.pdf  <-- Gino Van Den Berge 1999 presents a paper that improves performance
        // versaility and robustness of the algorithm
        public static bool rayCastGJK (Vector3d origin, Vector3d direction, PhysicsBody target, bool withMargin, out Vector3d hitLocation, 
            out Vector3d hitNormal, out double t)
		{
			if (target is CompoundBody)
			{
				return ((CompoundBody) target).rayTest(origin, direction, 1.00F, withMargin, out hitLocation, out hitNormal, out t);
			}
			t = 0.00d;
			hitLocation = origin;
			hitNormal = zeroVector;
			Vector3d vector3 = hitLocation - target.myInternalCenterPosition;
			List<Vector3d> list = ResourcePool.getVectorList();
			List<int> subsimplex = ResourcePool.getIntList();
            List<double> baryCoords = new List<double>();
			List<Vector3d> q = ResourcePool.getVectorList();
			double single3 = float.NegativeInfinity;
			double single4 = 0.00d;
			int num1 = 0;
			while (vector3.LengthSquared() > (single3 * 0.00d))
			{
				Vector3d item;
				num1++;
				if (num1 > 0x32)
				{
					hitLocation = noVector;
					hitNormal = noVector;
					ResourcePool.giveBack(list);
					ResourcePool.giveBack(subsimplex);
					ResourcePool.giveBack(q);
					return false;
				}
				if (withMargin)
				{
                    target.CollisionPrimitive.getExtremePoint(ref vector3, target.CollisionPrimitive.myCollisionMargin, out item);
				}
				else
				{
                    target.CollisionPrimitive.getExtremePoint(ref vector3, 0.00d, out item);
				}
				Vector3d vector1 = hitLocation - item;
				double single1 = Vector3d.DotProduct(vector3, vector1);
				if (single1 > 0.00d)
				{
					double single2 = Vector3d.DotProduct(vector3, direction);
					if (single2 >= 0.00d)
					{
						hitLocation = noVector;
						hitNormal = noVector;
						ResourcePool.giveBack(list);
						ResourcePool.giveBack(subsimplex);
						ResourcePool.giveBack(q);
						return false;
					}
					t -= single1 / single2;
					if (t > 1.00F)
					{
						hitLocation = noVector;
						hitNormal = noVector;
						ResourcePool.giveBack(list);
						ResourcePool.giveBack(subsimplex);
						ResourcePool.giveBack(q);
						return false;
					}
					hitLocation = origin + (direction * t);
					hitNormal = vector3;
				}
				q.Clear();
				list.Add(item);
				for (int i = 0;i < list.Count; i++)
				{
					q.Add(hitLocation - list[i]);
				}
				findPointOfMinimumNorm(q, subsimplex, baryCoords, out vector3);
				q.Clear();
				for (int j = 0;j < subsimplex.Count; j++)
				{
					q.Add(list[subsimplex[j]]);
				}
				list.Clear();
				foreach (Vector3d vector4 in q)
				{
					list.Add(vector4);
				}
				single3 = float.NegativeInfinity;
				foreach (Vector3d vector5 in list)
				{
					Vector3d vector6 = vector5 - origin;
					single4 = vector6.LengthSquared();
					if (single4 > single3)
					{
						single3 = single4;
					}
				}
			}
			ResourcePool.giveBack(list);
			ResourcePool.giveBack(subsimplex);
			ResourcePool.giveBack(q);
			return true;
		}
		
		public static bool rayCast (Vector3d origin, Vector3d direction, double maximumLength, PhysicsBody target, bool withMargin, 
            out Vector3d hitLocation, out Vector3d hitNormal, out double t)
		{
			return target.rayTest(origin, direction, maximumLength, withMargin, out hitLocation, out hitNormal, out t);
		}
		
		public static bool rayCast (Vector3d origin, Vector3d direction, double maximumLength, PhysicsBody target, double margin, 
            out Vector3d hitLocation, out Vector3d hitNormal, out double t)
		{
			if (target is CompoundBody)
			{
				return ((CompoundBody) target).rayTest(origin, direction, 1.00D, margin, out hitLocation, out hitNormal, out t);
			}
			t = 0.00d;
			hitLocation = origin;
			hitNormal = zeroVector;
			Vector3d vector3 = hitLocation - target.myInternalCenterPosition;
			List<Vector3d> list = ResourcePool.getVectorList();
			List<int> subsimplex = ResourcePool.getIntList();
		    List<double> baryCoords = new List<double>();
			List<Vector3d> q = ResourcePool.getVectorList();
            double single3 = double.NegativeInfinity;
			double single4 = 0.00d;
			int num1 = 0;
			while ((vector3.LengthSquared() > (single3 * 0.00d)) || (num1 > 0x32))
			{
				Vector3d item;
				num1++;
                target.CollisionPrimitive.getExtremePoint(ref vector3, margin, out item);
				Vector3d vector1 = hitLocation - item;
				double single1 = Vector3d.DotProduct(vector3, vector1);
				if (single1 > 0.00d)
				{
                    double single2 = Vector3d.DotProduct(vector3, direction);
					if (single2 >= 0.00d)
					{
						hitLocation = noVector;
						hitNormal = noVector;
						ResourcePool.giveBack(list);
						ResourcePool.giveBack(subsimplex);
						ResourcePool.giveBack(q);
						return false;
					}
					t -= single1 / single2;
					if (t > maximumLength)
					{
						hitLocation = noVector;
						hitNormal = noVector;
						ResourcePool.giveBack(list);
						ResourcePool.giveBack(subsimplex);
						ResourcePool.giveBack(q);
						return false;
					}
					hitLocation = origin + (direction * t);
					hitNormal = vector3;
				}
				q.Clear();
				list.Add(item);
				for (int i = 0;i < list.Count; i++)
				{
					q.Add(hitLocation - list[i]);
				}
				findPointOfMinimumNorm(q, subsimplex, baryCoords, out vector3);
				q.Clear();
				for (int j = 0;j < subsimplex.Count; j++)
				{
					q.Add(list[subsimplex[j]]);
				}
				list.Clear();
				foreach (Vector3d vector4 in q)
				{
					list.Add(vector4);
				}
				single3 = float.NegativeInfinity;
				foreach (Vector3d vector5 in list)
				{
					single4 = vector5.LengthSquared();
					if (single4 > single3)
					{
						single3 = single4;
					}
				}
			}
			ResourcePool.giveBack(list);
			ResourcePool.giveBack(subsimplex);
			ResourcePool.giveBack(q);
			return true;
		}
		
		public static Vector3d getVelocityOfPoint (Vector3d p, PhysicsBody obj)
		{
			return obj.myInternalLinearVelocity + Vector3d.CrossProduct(obj.myInternalAngularVelocity, p - obj.myInternalCenterPosition);
		}
		
		public static Matrix getCrossProductMatrix (Vector3d v)
		{
			return new Matrix(0.00d, v.z, -v.y, 0.00d, -v.z, 0.00d, v.x, 0.00d, v.y, -v.x, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d);
		}
		
		public static Matrix getTransposedVector (Vector3d v)
		{
			return new Matrix(v.x, 0.00d, 0.00d, 0.00d, v.y, 0.00d, 0.00d, 0.00d, v.z, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d);
		}
		
		public static Matrix getMatrixFromVector (Vector3d v)
		{
			return new Matrix(v.x, v.y, v.z, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d, 0.00d);
		}
		
		public static bool areVectorsSimilar (Vector3d a, Vector3d b)
		{
			double single1 = 0.00d;
			if (((Math.Abs((float) (a.x - b.x)) < single1) && (Math.Abs((float) (a.y - b.y)) < single1)) && (Math.Abs((float) (a.z - b.z)) < single1))
			{
				return true;
			}
			return false;
		}
		
		public static bool areVectorsEqual (Vector3d a, Vector3d b)
		{
			if (((a.x == b.x) && (a.y == b.y)) && (a.z== b.z))
			{
				return true;
			}
			return false;
		}
		
		public static bool containsSimilarAxis (List<Vector3d> list, Vector3d toCheck)
		{
			foreach (Vector3d b in list)
			{
				if (areVectorsSimilar(toCheck, b))
				{
					return true;
				}
				if (areVectorsSimilar(-(toCheck), b))
				{
					return true;
				}
			}
			return false;
		}
		
		public static double Clamp (double n, double min, double max)
		{
			if (n < min)
			{
				return min;
			}
			if (n > max)
			{
				return max;
			}
			return n;
		}
		
		public static void pruneDirectionalDuplicates (List<Vector3d> a, List<Vector3d> b)
		{
			List<Vector3d> list = new List<Vector3d>();
			List<Vector3d> list2 = new List<Vector3d>();
			List<Vector3d> list3 = new List<Vector3d>();
			foreach (Vector3d toCheck in a)
			{
				if (!containsSimilarAxis(list, toCheck))
				{
					list.Add(toCheck);
					continue;
				}
				list2.Add(toCheck);
			}
			foreach (Vector3d item in b)
			{
				if (!containsSimilarAxis(list, item))
				{
					list.Add(item);
					continue;
				}
				list3.Add(item);
			}
			foreach (Vector3d vector3 in list2)
			{
				a.Remove(vector3);
			}
			foreach (Vector3d vector4 in list3)
			{
				b.Remove(vector4);
			}
		}
		
		public static Matrix getOuterProduct (Vector3d a, Vector3d b)
		{
			return new Matrix(a.x * b.x, a.x * b.y, a.x * b.z, 0.00D, a.y * b.x, a.y * b.y, a.y * b.z, 0.00d, a.z * b.x, a.z * b.y, a.z * b.z, 0.00d, 0.00d, 0.00d, 0.00d, 1.00d);
		}
		
		public static void getConvexHull (List<Vector3d> points, List<int> indices, List<Vector3d> hullVertices)
		{
			int num3;
			int num4;
			double single1;
			double single2;
			List<int> outsidePoints = ResourcePool.getIntList();
			List<int> edges = ResourcePool.getIntList();
			List<int> list3 = ResourcePool.getIntList();
			for (int item = 0;item < points.Count; item++)
			{
				outsidePoints.Add(item);
			}
			List<int> list = ResourcePool.getIntList();
			getExtremePointsOfSet(Vector3d.Up(), points, out num3, out num4);
			if (num3 == num4)
			{
				throw new ArgumentException("Point set is degenerate.");
			}
			list.Add(num3);
			list.Add(num4);
			Vector3d direction = noVector;
			for (int i = 0;i < points.Count; i++)
			{
				if ((i != num3) && (i != num4))
				{
					direction = Vector3d.CrossProduct(points[num3] - points[i], points[num4] - points[i]);
					if (direction.LengthSquared() > 0.00d)
					{
						break;
					}
				}
			}
			double single3 = Vector3d.DotProduct(direction, points[num3]);
			getExtremePointsOfSet(direction, points, out num3, out num4, out single1, out single2);
			if (Math.Abs((float) (single1 - single3)) < 0.00d)
			{
				if (Math.Abs((float) (single2 - single3)) < 0.00d)
				{
					throw new ArgumentException("Point set is degenerate.");
				}
				list.Add(num4);
			}
			else
			{
				list.Add(num3);
			}
			direction = Vector3d.CrossProduct(points[list[1]] - points[list[0]], points[list[2]] - points[list[0]]);
			single3 = Vector3d.DotProduct(direction, points[list[0]]);
			getExtremePointsOfSet(direction, points, out num3, out num4, out single1, out single2);
			if (Math.Abs((float) (single1 - single3)) < 0.00d)
			{
				if (Math.Abs((float) (single2 - single3)) < 0.00d)
				{
					throw new ArgumentException("Point set is degenerate.");
				}
				list.Add(num4);
			}
			else
			{
				list.Add(num3);
			}
			if (list.Count != 4)
			{
				throw new ArgumentException("Could not form an initial tetrahedron from the input points; ensure that the input point set has volume.");
			}
			indices.Add(list[0]);
			indices.Add(list[1]);
			indices.Add(list[2]);
			indices.Add(list[1]);
			indices.Add(list[2]);
			indices.Add(list[3]);
			indices.Add(list[2]);
			indices.Add(list[3]);
			indices.Add(list[0]);
			indices.Add(list[3]);
			indices.Add(list[0]);
			indices.Add(list[1]);
			for (int j = 0;j < 4; j++)
			{
				outsidePoints.Remove(list[j]);
			}
			verifyWindings(indices, points);
			removePointsInPolyhedronIfInside(outsidePoints, points, indices);
			ResourcePool.giveBack(list);
		Label_0448:
			while (outsidePoints.Count > 0)
			{
				for (int startIndex = 0;startIndex < indices.Count; startIndex += 3)
				{
					int num1;
					Vector3d vector2 = findNormal(points, indices, startIndex);
					Vector3d vector1 = getExtremePointOfSet(vector2, outsidePoints, points, out num1);
					if ((Vector3d.DotProduct(vector1, vector2) - Vector3d.DotProduct(points[indices[startIndex]], vector2)) > 0.00d)
					{
						outsidePoints.Remove(num1);
						edges.Clear();
						list3.Clear();
						for (int index = 0;index < indices.Count; index += 3)
						{
							vector2 = findNormal(points, indices, index);
							if ((Vector3d.DotProduct(vector1, vector2) - Vector3d.DotProduct(points[indices[index]], vector2)) > 0.00d)
							{
								maintainEdge(indices[index], indices[index + 1], edges);
								maintainEdge(indices[index], indices[index + 2], edges);
								maintainEdge(indices[index + 1], indices[index + 2], edges);
								indices.RemoveAt(index);
								indices.RemoveAt(index);
								indices.RemoveAt(index);
								index -= 3;
							}
						}
						for (int z = 0;z < edges.Count; z += 2)
						{
							indices.Add(edges[z]);
							indices.Add(edges[z + 1]);
							indices.Add(num1);
						}
						verifyWindings(indices, points);
						removePointsInPolyhedronIfInside(outsidePoints, points, indices);
						goto Label_0448;
					}
				}
			}
			for (int k = 0;k < indices.Count; k++)
			{
				if (!hullVertices.Contains(points[indices[k]]))
				{
					hullVertices.Add(points[indices[k]]);
				}
			}
			ResourcePool.giveBack(outsidePoints);
			ResourcePool.giveBack(edges);
			ResourcePool.giveBack(list3);
		}
		
		private static void maintainEdge (int a, int b, List<int> edges)
		{
			bool flag1 = false;
			int index = 0;
			for (int i = 0;i < edges.Count; i += 2)
			{
				if (((edges[i] != a) || (edges[(i + 1)] != b)) && ((edges[i] != b) || (edges[(i + 1)] != a)))
				{
					continue;
				}
				flag1 = true;
				index = i;
			}
			if (!flag1)
			{
				edges.Add(a);
				edges.Add(b);
			}
			else
			{
				edges.RemoveAt(index);
				edges.RemoveAt(index);
			}
		}
		
		private static void removePointsInPolyhedronIfInside (List<int> outsidePoints, List<Vector3d> points, List<int> indices)
		{
			List<int> list = ResourcePool.getIntList();
			for (int i = 0;i < outsidePoints.Count; i++)
			{
				list.Add(outsidePoints[i]);
			}
			outsidePoints.Clear();
			for (int startIndex = 0;(startIndex < indices.Count) && (list.Count > 0); startIndex += 3)
			{
				Vector3d vector2 = findNormal(points, indices, startIndex);
				for (int index = 0;index < list.Count; index++)
				{
					Vector3d vector1 = points[list[index]];
					if ((Vector3d.DotProduct(vector1, vector2) - Vector3d.DotProduct(points[indices[startIndex]], vector2)) > 0.00d)
					{
						outsidePoints.Add(list[index]);
						list.RemoveAt(index);
						index--;
					}
				}
			}
			ResourcePool.giveBack(list);
		}
		
		private static void verifyWindings (List<int> indices, List<Vector3d> points)
		{
			Vector3d vector1 = zeroVector;
			for (int i = 0;i < indices.Count; i++)
			{
				vector1 += points[indices[i]];
			}
			vector1 =  (vector1 / ((float) indices.Count));
			for (int startIndex = 0;startIndex < indices.Count; startIndex += 3)
			{
				if (Vector3d.DotProduct(points[indices[startIndex]] - vector1, findNormal(points, indices, startIndex)) < 0.00d)
				{
                    // swap
                    int num3 = indices[startIndex + 1];
					indices[startIndex + 1] = indices[startIndex + 2];
					indices[startIndex + 2] = num3;
				}
			}
		}
		
		private static Vector3d findNormal (List<Vector3d> points, List<int> indices, int startIndex)
		{
			return Vector3d.CrossProduct(points[indices[(startIndex + 2)]] - points[indices[startIndex]], points[indices[(startIndex + 1)]] - points[indices[startIndex]]);
		}
		
		public static Vector3d getExtremePointOfSet (Vector3d direction, List<int> pointIndices, List<Vector3d> points, out int maxIndex)
		{
            double single2 = double.MinValue;
			Vector3d vector1 = zeroVector;
			maxIndex = 0;
			for (int i = 0;i < pointIndices.Count; i++)
			{
				double single1 = Vector3d.DotProduct(points[pointIndices[i]], direction);
				if (single1 > single2)
				{
					single2 = single1;
					vector1 = points[pointIndices[i]];
					maxIndex = pointIndices[i];
				}
			}
			return vector1;
		}
		
		public static void getExtremePointsOfSet (Vector3d direction, List<Vector3d> points, out int minimum, out int maximum)
		{
            double single2 = double.MinValue;
            double single3 = double.MaxValue;
			minimum = 0;
			maximum = 0;
			for (int i = 0;i < points.Count; i++)
			{
				double single1 = Vector3d.DotProduct(points[i], direction);
				if (single1 > single2)
				{
					single2 = single1;
					maximum = i;
				}
				if (single1 < single3)
				{
					single3 = single1;
					minimum = i;
				}
			}
		}
		
		internal static void getExtremePointsOfSet (Vector3d direction, List<Vector3d> points, out int minimum, out int maximum, out double min, out double max)
		{
			max = double.MinValue;
            min = double.MaxValue;
			minimum = 0;
			maximum = 0;
			for (int i = 0;i < points.Count; i++)
			{
				double single1 = Vector3d.DotProduct(points[i], direction);
				if (single1 > max)
				{
					max = single1;
					maximum = i;
				}
				if (single1 < min)
				{
					min = single1;
					minimum = i;
				}
			}
		}
		
		internal static void differentiateQuaternion (ref Quaternion orientation, ref Matrix localInertiaTensorInverse, ref Vector3d angularMomentum, out Quaternion orientationChange)
		{
		    Quaternion quaternion1 = Quaternion.Normalize(orientation);
            Matrix matrix1 = new Keystone.Types.Matrix(quaternion1);
			Matrix matrix3 = Matrix.Transpose(matrix1);
			Matrix matrix2 = matrix3*localInertiaTensorInverse;
			matrix2=matrix2*matrix1;
            Vector3d vector1 = Vector3d.TransformCoord(angularMomentum, matrix2);
			vector1=vector1* 0.50d;
            orientationChange = Quaternion.Multiply(new Quaternion(vector1.x, vector1.y, vector1.z, 0.00d), quaternion1);
		}
		
        // TODO: create class Integrator and move it out of Toolbox
        // Here is how numerical integration works. First, start at a certain initial position and velocity, 
        // then take a small step forward in time to find the velocity and position at the next time value. 
        // Do this repeatedly to go forward in time in small increments, each time taking the results of 
        // the previous integration as the starting point for the next. 
        // Eular Integrators have way too much error that just grows worse over time. Thus we use RK4 integrator.
        // This integrator is called the Runge Kutta order 4 integrator aka RK4. This is the standard integrator used for
        // numerical integration these days and is sufficiently accurate for just about anything required in game physics,
        // given an appropriate timestep
        // Technically RK4 has error O(5) (read “order 5″) in the Taylor’s Series expansion of the solution to the differential equation.
        // I’m not going to go into the details of deriving or explaining how an RK4 integrator is derived, but I will say intuitively 
        // that what it is doing is detecting curvature (change over time) when integrating down to the fourth derivative.
        // RK4 is not completely accurate, but its order of accuracy is extremely good and this is what counts.
        // An RK4 integrator works by evaluating the derivatives at several points in the timestep to detect this curvature, 
        // then it combines these sampled derivatives with a weighted average to get the single best approximation of the
        // derivative that it can provide over the timestep.
        // source Glenn Fiedler - http://gafferongames.com/game-physics/integration-basics/
		internal static void updateOrientationRK4 (ref Quaternion q, ref Matrix localInertiaTensorInverse, ref Vector3d angularMomentum, double dt, out Quaternion newOrientation)
		{
			Quaternion quaternion1;
		    Quaternion quaternion3;
		    Quaternion quaternion5;
		    Quaternion quaternion7;
		    differentiateQuaternion(ref q, ref localInertiaTensorInverse, ref angularMomentum, out quaternion1);
            Quaternion quaternion2 = Quaternion.Scale(quaternion1, dt * 0.50d);
			quaternion2=q+quaternion2;
			differentiateQuaternion(ref quaternion2, ref localInertiaTensorInverse, ref angularMomentum, out quaternion3);
            Quaternion quaternion4 = Quaternion.Scale(quaternion3, dt * 0.50d);
			quaternion4=q+quaternion4;
			differentiateQuaternion(ref quaternion4, ref localInertiaTensorInverse, ref angularMomentum, out quaternion5);
            Quaternion quaternion6 = Quaternion.Scale(quaternion5, dt);
			quaternion6=q+quaternion6;
			differentiateQuaternion(ref quaternion6, ref localInertiaTensorInverse, ref angularMomentum, out quaternion7);
			quaternion1=Quaternion.Scale(quaternion1, dt / 6.00d);
            quaternion3 = Quaternion.Scale(quaternion3, dt / 3.00d);
            quaternion5 = Quaternion.Scale(quaternion5, dt / 3.00d);
            quaternion7 = Quaternion.Scale(quaternion7, dt / 6.00d);
			Quaternion quaternion8 = q+quaternion1;
			quaternion8=quaternion8+quaternion3;
			quaternion8=quaternion8+quaternion5;
			quaternion8=quaternion8+quaternion7;
            newOrientation = Quaternion.Normalize(quaternion8);
		}
		
		internal static Vector3d differentiatePosition (ref Vector3d position, double invMass, ref Vector3d linearMomentum)
		{
		    return linearMomentum*invMass;
		}
		
		internal static Vector3d updatePositionRK4 (ref Vector3d position, double mass, ref Vector3d linearMomentum, double dt)
		{
			double invMass = 1.00d / mass;
			Vector3d vector1 = differentiatePosition(ref position, invMass, ref linearMomentum);
			Vector3d vector2 =  (position + (vector1 * (dt / 2.00F)));
			Vector3d vector3 = differentiatePosition(ref vector2, invMass, ref linearMomentum);
			Vector3d vector4 =  (position + (vector3 * (dt / 2.00F)));
			Vector3d vector5 = differentiatePosition(ref vector4, invMass, ref linearMomentum);
			Vector3d vector6 =  (position + (vector5 * dt));
			Vector3d vector7 = differentiatePosition(ref vector6, invMass, ref linearMomentum);
			return  ((((position + (vector1 * (dt / 6.00F))) + (vector3 * (dt / 3.00F))) + (vector5 * (dt / 3.00F))) + (vector7 * (dt / 6.00F)));
		}
		
		public static CompoundBody getHighestRankCompoundBody (PhysicsBody p)
		{
			if (p.isSubBodyOfCompound)
			{
				return getHighestRankCompoundBody(p.compoundBody);
			}
			if (p is CompoundBody)
			{
				return (CompoundBody) p;
			}
			return null;
		}
		
		public static void getCollidedEntities (PhysicsBody e, List<PhysicsBody> collidedEntities)
		{
			foreach (Controller controller1 in e.controllers)
			{
				if (controller1.colliderA == e)
				{
					collidedEntities.Add(controller1.colliderB);
					continue;
				}
				collidedEntities.Add(controller1.colliderA);
			}
		}
		
		internal static void integrateLinearVelocity (PhysicsBody e, double dt, out Vector3d initial, out Vector3d final)
		{
			initial = e.myInternalCenterOfMass;
			final=e.myInternalLinearVelocity* dt;
			final=initial+final;
		}
		
		internal static void integrateLinearVelocity (PhysicsBody a, PhysicsBody b, double dt, out Vector3d aInitial, out Vector3d bInitial, out Vector3d aFinal, out Vector3d bFinal)
		{
            try
            {
                aInitial = a.myInternalCenterOfMass;
                bInitial = b.myInternalCenterOfMass;
                aFinal = a.myInternalLinearVelocity * dt;
                aFinal = aInitial + aFinal;
                bFinal = b.myInternalLinearVelocity * dt;
                bFinal = bInitial + bFinal;
                //if (aFinal.IsNullOrEmpty() || bFinal.IsNullOrEmpty() || aFinal.x == double.NaN || bFinal.x == double.NaN || aFinal.y == double.NaN || bFinal.y == double.NaN || aFinal.z == double.NaN || bFinal.z == double.NaN)
                //    System.Diagnostics.Debug.WriteLine("wtf");
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("wtf");
                aFinal = new Vector3d();
                bFinal = new Vector3d();
                bInitial = new Vector3d();
                aInitial = new Vector3d();
            }
            
		}
		
		internal static void integrateAngularVelocity (PhysicsBody e, double dt, out Quaternion initial, out Quaternion final)
		{
			initial = e.myInternalOrientationQuaternion;
			if ((e.space != null) && e.space.simulationSettings.useRK4AngularIntegration)
			{
				updateOrientationRK4(ref initial, ref e.localInertiaTensorInverse, ref e.myInternalAngularMomentum, dt, out final);
			}
			else
			{
			    Vector3d vector1 = e.myInternalAngularVelocity* dt * 0.50F;
                final = Quaternion.Multiply(new Quaternion(vector1.x, vector1.y, vector1.z, 0.00d), initial);
			    final = initial + final; 
				final = Quaternion.Normalize(final);
			}
		}
		
		internal static void integrateAngularVelocity (PhysicsBody a, PhysicsBody b, double dt, out Quaternion aInitial, out Quaternion bInitial, out Quaternion aFinal, out Quaternion bFinal)
		{
			aInitial = a.myInternalOrientationQuaternion;
			bInitial = b.myInternalOrientationQuaternion;
			if ((a.space != null) && a.space.simulationSettings.useRK4AngularIntegration)
			{
				updateOrientationRK4(ref aInitial, ref a.localInertiaTensorInverse, ref a.myInternalAngularMomentum, dt, out aFinal);
				updateOrientationRK4(ref bInitial, ref b.localInertiaTensorInverse, ref b.myInternalAngularMomentum, dt, out bFinal);
			}
			else
			{
			    Vector3d vector1 = a.myInternalAngularVelocity* dt * 0.50F;
				Vector3d vector2 = b.myInternalAngularVelocity* dt * 0.50F;
                aFinal = Quaternion.Multiply(new Quaternion(vector1.x, vector1.y, vector1.z, 0.00d), aInitial);
                bFinal = Quaternion.Multiply(new Quaternion(vector2.x, vector2.y, vector2.z, 0.00d), bInitial);
			    aFinal = aInitial + aFinal; 
			    bFinal = bInitial + bFinal; 
				aFinal = Quaternion.Normalize(aFinal);
				bFinal = Quaternion.Normalize(bFinal);
			}
		}
		
		public static void getSphereVolumeSplitByPlane (Vector3d spherePosition, double sphereVolume, double radius, Vector3d p, Vector3d norm, out double volume)
		{
			Vector3d vector1 = getPointProjectedOnPlane(spherePosition, norm, p);
			Vector3d vector2 = vector1 - spherePosition;
			double single1 = vector2.Length;
			double single2 = radius - single1;
			double single3 = ((3.14F * single2) * single2) * (radius - (0.33F * single2));
            //double single3 = ((Math.PI * single2) * single2) * (radius - (0.33F * single2)); // these magic numbers suck.  I assume 3.14 means pi?  what's .33?
			if ( Vector3d.DotProduct(vector2, norm) > 0.00d)
			{
				volume = sphereVolume - single3;
			}
			else
			{
				volume = single3;
			}
		}
	}
}