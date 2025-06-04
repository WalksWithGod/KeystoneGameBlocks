using Keystone.Physics.Primitives;
using Keystone.Types;
using System;
using System.Collections.Generic;

namespace Keystone.Physics
{
	public class Controller
	{
        // Statics
        internal static Vector3d[] testDirections;

        // Instance Fields
        public Vector3d separatingAxis;
        private List<Contact> myPreviousContacts;
        public List<Contact> contacts;
        private List<Vector3d> myContactsLocalPositionsA;
        private List<Vector3d> myContactsLocalPositionsB;
        public PhysicsBody colliderA;
        public PhysicsBody colliderB;
        public PhysicsBody parentA;
        public PhysicsBody parentB;
        public double timeOfImpact;
        public Vector3d nextPositionA;
        public Vector3d nextPositionB;
        public Quaternion nextOrientationA;
        public Quaternion nextOrientationB;
        private bool contactsStable;
        private int numContactsLastFrame;
        internal double margin;
        internal double marginSquared;
        internal double allowedPenetration;
        public float bounciness;
        public float dynamicFriction;
        public float staticFriction;
        public Space space;
        internal ControllerTableKey tableKey;
        private bool wasInDeepPenetration;
        internal bool detectedCollisionInContinuousTest;
        internal int updateIterations;
        private int collisionType;
        private int[] numIterationsAtZeroNormal;
        private int[] numIterationsAtZeroFriction;
        private bool[] calculateNormal;
        private bool[] calculateFriction;

        // Properties
        internal List<Contact> previousContacts
        {
            get { return myPreviousContacts;}
            set{myPreviousContacts = value;}
        }

        public double collisionMargin
        {
            get
            {
                return margin;
            }
        }
		// Constructors
		internal Controller ()
		{
			separatingAxis.ZeroVector();
			myPreviousContacts = new List<Contact>(4);
			contacts = new List<Contact>(4);
			myContactsLocalPositionsA = new List<Vector3d>(4);
			myContactsLocalPositionsB = new List<Vector3d>(4);
			numIterationsAtZeroNormal = new int[4];
			numIterationsAtZeroFriction = new int[4];
			calculateNormal = new bool[4];
			calculateFriction = new bool[4];
		}
		
		internal Controller (PhysicsBody a, PhysicsBody b, Space s) : this()
		{
			space = s;
			contactsStable = false;
			colliderA = a;
			colliderB = b;
			updateBounciness();
			updateFriction();
			updateMargin();
			updateAllowedPenetration();
			parentA = colliderA.myParent;
			parentB = colliderB.myParent;
		}
		
		static Controller ()
		{
			testDirections = new Vector3d[]{
				new Vector3d(1.00F, 0.00d, 0.00d), new Vector3d(0.93F, 0.37F, 0.00d), new Vector3d(0.93F, -0.37F, 0.00d), new Vector3d(0.72F, 0.69F, 0.00d), new Vector3d(0.72F, -0.69F, 0.00d), new Vector3d(0.41F, 0.91F, 0.00d), new Vector3d(0.41F, -0.91F, 0.00d), new Vector3d(0.93F, 0.00d, 0.37F), new Vector3d(0.86F, 0.37F, 0.35F), new Vector3d(0.86F, -0.37F, 0.35F), new Vector3d(0.67F, 0.69F, 0.27F), new Vector3d(0.67F, -0.69F, 0.27F), new Vector3d(0.38F, 0.91F, 0.15F), new Vector3d(0.38F, -0.91F, 0.15F), new Vector3d(0.72F, 0.00d, 0.69F), new Vector3d(0.67F, 0.37F, 0.64F), 
				new Vector3d(0.67F, -0.37F, 0.64F), new Vector3d(0.52F, 0.69F, 0.50F), new Vector3d(0.52F, -0.69F, 0.50F), new Vector3d(0.29F, 0.91F, 0.28F), new Vector3d(0.29F, -0.91F, 0.28F), new Vector3d(0.41F, 0.00d, 0.91F), new Vector3d(0.38F, 0.37F, 0.85F), new Vector3d(0.38F, -0.37F, 0.85F), new Vector3d(0.29F, 0.69F, 0.66F), new Vector3d(0.29F, -0.69F, 0.66F), new Vector3d(0.17F, 0.91F, 0.37F), new Vector3d(0.17F, -0.91F, 0.37F), new Vector3d(0.03F, 0.00d, 1.00F), new Vector3d(0.03F, 0.37F, 0.93F), new Vector3d(0.03F, -0.37F, 0.93F), new Vector3d(0.03F, 0.69F, 0.72F), 
				new Vector3d(0.03F, -0.69F, 0.72F), new Vector3d(0.01F, 0.91F, 0.41F), new Vector3d(0.01F, -0.91F, 0.41F), new Vector3d(-0.34F, 0.00d, 0.94F), new Vector3d(-0.32F, 0.37F, 0.87F), new Vector3d(-0.32F, -0.37F, 0.87F), new Vector3d(-0.25F, 0.69F, 0.68F), new Vector3d(-0.25F, -0.69F, 0.68F), new Vector3d(-0.14F, 0.91F, 0.38F), new Vector3d(-0.14F, -0.91F, 0.38F), new Vector3d(-0.67F, 0.00d, 0.74F), new Vector3d(-0.62F, 0.37F, 0.69F), new Vector3d(-0.62F, -0.37F, 0.69F), new Vector3d(-0.48F, 0.69F, 0.53F), new Vector3d(-0.48F, -0.69F, 0.53F), new Vector3d(-0.27F, 0.91F, 0.30F), 
				new Vector3d(-0.27F, -0.91F, 0.30F), new Vector3d(-0.90F, 0.00d, 0.44F), new Vector3d(-0.83F, 0.37F, 0.41F), new Vector3d(-0.83F, -0.37F, 0.41F), new Vector3d(-0.65F, 0.69F, 0.32F), new Vector3d(-0.65F, -0.69F, 0.32F), new Vector3d(-0.37F, 0.91F, 0.18F), new Vector3d(-0.37F, -0.91F, 0.18F), new Vector3d(0.00d, 1.00F, 0.00d)
			};
		}
		
		
		// Methods
		internal void setup (PhysicsBody a, PhysicsBody b, Space s)
		{
			space = s;
			contacts.Clear();
			previousContacts.Clear();
			contactsStable = false;
			colliderA = a;
			colliderB = b;
			updateBounciness();
			updateFriction();
			updateMargin();
			updateAllowedPenetration();
			parentA = colliderA.myParent;
			parentB = colliderB.myParent;
			for (int i = 0;i < 4; i++)
			{
				calculateNormal[i] = true;
				calculateFriction[i] = true;
				numIterationsAtZeroNormal[i] = 0;
				numIterationsAtZeroFriction[i] = 0;
			}
			numContactsLastFrame = 0;
			myContactsLocalPositionsA.Clear();
			myContactsLocalPositionsB.Clear();
			separatingAxis.ZeroVector();
		}
		
		internal void updateMargin ()
		{
            margin = colliderA.CollisionPrimitive.myCollisionMargin + colliderB.CollisionPrimitive.myCollisionMargin;
			marginSquared = margin * margin;
		}
		
		internal void updateAllowedPenetration ()
		{
            allowedPenetration = colliderA.CollisionPrimitive.myAllowedPenetration + colliderB.CollisionPrimitive.myAllowedPenetration;
		}
		
		internal void updateBounciness ()
		{
			if (space.simulationSettings.bouncinessCombineMethod == PropertyCombineMethod.average)
			{
				bounciness = (colliderA.myBounciness + colliderB.myBounciness) / 2.00F;
			}
			else if (space.simulationSettings.bouncinessCombineMethod == PropertyCombineMethod.max)
			{
				bounciness = Math.Max(colliderA.myBounciness, colliderB.myBounciness);
			}
			else if (space.simulationSettings.bouncinessCombineMethod == PropertyCombineMethod.min)
			{
				bounciness = Math.Min(colliderA.myBounciness, colliderB.myBounciness);
			}
			else if (space.simulationSettings.bouncinessCombineMethod == PropertyCombineMethod.biasHigh)
			{
				bounciness = (Math.Max(colliderA.myBounciness, colliderB.myBounciness) * 0.75F) + (Math.Min(colliderA.myBounciness, colliderB.myBounciness) * 0.25F);
			}
			else
			{
				bounciness = (Math.Max(colliderA.myBounciness, colliderB.myBounciness) * 0.25F) + (Math.Min(colliderA.myBounciness, colliderB.myBounciness) * 0.75F);
			}
		}
		
		internal void updateFriction ()
		{
			if (space.simulationSettings.frictionCombineMethod == PropertyCombineMethod.average)
			{
				dynamicFriction = (colliderA.myDynamicFriction + colliderB.myDynamicFriction) / 2.00F;
				staticFriction = (colliderA.myStaticFriction + colliderB.myStaticFriction) / 2.00F;
			}
			else if (space.simulationSettings.frictionCombineMethod == PropertyCombineMethod.max)
			{
				dynamicFriction = Math.Max(colliderA.myDynamicFriction, colliderB.myDynamicFriction);
				staticFriction = Math.Max(colliderA.myStaticFriction, colliderB.myStaticFriction);
			}
			else if (space.simulationSettings.frictionCombineMethod == PropertyCombineMethod.min)
			{
				dynamicFriction = Math.Min(colliderA.myDynamicFriction, colliderB.myDynamicFriction);
				staticFriction = Math.Min(colliderA.myStaticFriction, colliderB.myStaticFriction);
			}
			else
			{
				if (space.simulationSettings.frictionCombineMethod == PropertyCombineMethod.biasHigh)
				{
					dynamicFriction = (Math.Max(colliderA.myDynamicFriction, colliderB.myDynamicFriction) * 0.75F) + (Math.Min(colliderA.myDynamicFriction, colliderB.myDynamicFriction) * 0.25F);
					staticFriction = (Math.Max(colliderA.myStaticFriction, colliderB.myStaticFriction) * 0.75F) + (Math.Min(colliderA.myStaticFriction, colliderB.myStaticFriction) * 0.25F);
				}
				else
				{
					dynamicFriction = (Math.Max(colliderA.myDynamicFriction, colliderB.myDynamicFriction) * 0.25F) + (Math.Min(colliderA.myDynamicFriction, colliderB.myDynamicFriction) * 0.75F);
					staticFriction = (Math.Max(colliderA.myStaticFriction, colliderB.myStaticFriction) * 0.25F) + (Math.Min(colliderA.myStaticFriction, colliderB.myStaticFriction) * 0.75F);
				}
			}
		}
		
		public void updateCollisionDiscreteMPR (double dt)
		{
			if (((colliderA.myIsTangible && colliderB.myIsTangible) && (((!contactsStable || colliderA.myIsDetector)
                || (colliderB.myIsDetector || (!(parentA.myInternalLinearVelocity - parentB.myInternalLinearVelocity).IsNullOrEmpty())))
                || ((!parentA.myInternalAngularVelocity.IsNullOrEmpty()) || (!parentB.myInternalAngularVelocity.IsNullOrEmpty())))) 
                && !updateCollisionSpecialCases(dt))
			{
				Vector3d vector1;
				Vector3d vector2;
				double depth;
				contactsStable = true;
                if (Toolbox.areObjectsCollidingMPR(colliderA, colliderB, colliderA.CollisionPrimitive.myCollisionMargin,
                    colliderB.CollisionPrimitive.myCollisionMargin, out vector1, out vector2, out depth))
				{
					addContact(ref vector1, ref vector2, depth);
					generateFullManifold();
					contactRefresh();
					contactReduction();
				}
				else
				{
					clearContacts();
				}
				numContactsLastFrame = contacts.Count;
			}
		}
		
		internal void updateCollisionDiscreteMPRGJKold (double dt)
		{
			if ((colliderA.myIsTangible && colliderB.myIsTangible) 
                && ((!contactsStable || (!(parentA.myInternalLinearVelocity - parentB.myInternalLinearVelocity).IsNullOrEmpty() )) 
                || ((!parentA.myInternalAngularVelocity.IsNullOrEmpty() ) 
                || (!parentB.myInternalAngularVelocity.IsNullOrEmpty() ))))
			{
				contactsStable = true;
				bool flag1 = false;
                if ((numContactsLastFrame > 0) || Toolbox.areObjectsColliding(colliderA, colliderB, colliderA.CollisionPrimitive.myCollisionMargin,
                    colliderB.CollisionPrimitive.myCollisionMargin, ref separatingAxis))
				{
					Vector3d vector1;
					Vector3d vector2;
					Vector3d vector3;
					Vector3d vector4;
                    double depth;
					Vector3d vector5 = Toolbox.getClosestPointsBetweenObjects(colliderA, colliderB, 0.00d, 0.00d, out vector1, out vector2);
					double single2 = vector5.LengthSquared();
					if ((single2 < marginSquared) && (single2 > 0.00d))
					{
						vector3 = ((vector1 + vector2) / 2.00F);
						vector4 = vector5;
						depth = margin - ( Math.Sqrt( single2));
						addContact(ref vector3, ref vector4, depth);
						flag1 = true;
					}
					else
					{
                        if ((single2 == 0.00d) && Toolbox.areObjectsCollidingMPR(colliderA, colliderB, colliderA.CollisionPrimitive.myCollisionMargin,
                            colliderB.CollisionPrimitive.myCollisionMargin, out vector3, out vector4, out depth))
						{
							addContact(ref vector3, ref vector4, depth);
							flag1 = true;
						}
					}
				}
				if (flag1)
				{
					contactRefresh();
					contactReduction();
				}
				if (!flag1)
				{
					clearContacts();
				}
				numContactsLastFrame = contacts.Count;
			}
		}
		
		internal bool objectsMightBeClose ()
		{
			Vector3d vector2;
			Vector3d vector3;
			Vector3d vector4;
			Vector3d vector1 = colliderB.myInternalCenterPosition - colliderA.myInternalCenterPosition;
			if (separatingAxis.IsNullOrEmpty())
			{
				vector2 = vector1;
				colliderA.CollisionPrimitive.getExtremePoint(ref vector2, 0.00d, out vector3);
				Vector3d vector5 = -(vector2);
				colliderB.CollisionPrimitive.getExtremePoint(ref vector5, 0.00d, out vector4);
			}
			else
			{
				vector2 = separatingAxis;
				if ( Vector3d.DotProduct(vector1, vector2)> 0.00d)
				{
					colliderA.CollisionPrimitive.getExtremePoint(ref vector2, 0.00d, out vector3);
					Vector3d vector6 = -(vector2);
					colliderB.CollisionPrimitive.getExtremePoint(ref vector6, 0.00d, out vector4);
				}
				else
				{
					Vector3d vector7 = -(vector2);
					colliderA.CollisionPrimitive.getExtremePoint(ref vector7, 0.00d, out vector3);
					colliderB.CollisionPrimitive.getExtremePoint(ref vector2, 0.00d, out vector4);
				}
			}
			vector2.Normalize();
			double single1 = Vector3d.DotProduct(vector3, vector2);
            double single2 = Vector3d.DotProduct(vector4, vector2);
			if (Math.Abs( (single2 - single1)) > margin)
			{
				return false;
			}
			return true;
		}
		
		public void updateCollisionDiscreteMPRGJK (double dt)
		{
			if (((colliderA.myIsTangible && colliderB.myIsTangible) && (((!contactsStable || colliderA.myIsDetector)
                || (colliderB.myIsDetector || (!(parentA.myInternalLinearVelocity - parentB.myInternalLinearVelocity).IsNullOrEmpty()))) 
                || ((!parentA.myInternalAngularVelocity.IsNullOrEmpty() ) || (!parentB.myInternalAngularVelocity.IsNullOrEmpty())))) 
                && !updateCollisionSpecialCases(dt))
			{
				contactsStable = true;
				bool flag1 = false;
				if (((numContactsLastFrame > 0) || wasInDeepPenetration) || (detectedCollisionInContinuousTest ||
                    Toolbox.areObjectsColliding(colliderA, colliderB, colliderA.CollisionPrimitive.myCollisionMargin,
                    colliderB.CollisionPrimitive.myCollisionMargin, ref separatingAxis)))
				{
					Vector3d vector3;
					Vector3d vector4;
					double depth;
                    double single2 = 0.00d;
					if (!wasInDeepPenetration)
					{
						Vector3d vector1;
						Vector3d vector2;
						Vector3d vector5 = Toolbox.getClosestPointsBetweenObjects(colliderA, colliderB, 0.00d, 0.00d, out vector1, out vector2);
						single2 = vector5.LengthSquared();
						if ((single2 < marginSquared) && (single2 > 0.00d))
						{
							vector3=vector1+vector2;
							vector3=vector3* 0.50F;
							vector4 = vector5;
							depth = margin - ( Math.Sqrt( single2));
							addContact(ref vector3, ref vector4, depth);
							flag1 = true;
							generateFullManifold();
						}
					}
					if (single2 == 0.00d)
					{
                        if (Toolbox.areObjectsCollidingMPR(colliderA, colliderB, colliderA.CollisionPrimitive.myCollisionMargin,
                            colliderB.CollisionPrimitive.myCollisionMargin, out vector3, out vector4, out depth))
						{
							addContact(ref vector3, ref vector4, depth);
							flag1 = true;
							wasInDeepPenetration = true;
                            if (depth < (colliderA.CollisionPrimitive.myCollisionMargin + colliderB.CollisionPrimitive.myCollisionMargin))
							{
								wasInDeepPenetration = false;
							}
						}
						else
						{
							wasInDeepPenetration = false;
						}
					}
				}
				if (flag1)
				{
					contactRefresh();
					contactReduction();
				}
				if (!flag1)
				{
					clearContacts();
				}
				numContactsLastFrame = contacts.Count;
				detectedCollisionInContinuousTest = false;
			}
		}
		
		public void updateCollisionDiscreteGJK (double dt)
		{
			updateIterations++;
			if ((updateIterations <= 2) && (((colliderA.myIsTangible && colliderB.myIsTangible) && (((!contactsStable || colliderA.myIsDetector)
                || (colliderB.myIsDetector || (!(parentA.myInternalLinearVelocity - parentB.myInternalLinearVelocity).IsNullOrEmpty())))
                || ((!parentA.myInternalAngularVelocity.IsNullOrEmpty()) || (!parentB.myInternalAngularVelocity.IsNullOrEmpty())))) 
                && !updateCollisionSpecialCases(dt)))
			{
				contactsStable = true;
                if (Toolbox.areObjectsColliding(colliderA, colliderB, colliderA.CollisionPrimitive.myCollisionMargin,
                    colliderB.CollisionPrimitive.myCollisionMargin, ref separatingAxis))
				{
					if (contacts.Count <= 4)
					{
						Vector3d vector1;
						Vector3d vector2;
						Vector3d vector3 = Toolbox.getClosestPointsBetweenObjects(colliderA, colliderB, 0.00d, 0.00d, out vector1, out vector2);
                        if (!vector3.IsNullOrEmpty())
						{
							Vector3d vector4 = ((vector1 + vector2) / 2.00F);
							addContact(ref vector4, ref vector3, margin - vector3.Length);
							generateFullManifold();
						}
						else
						{
							if (colliderA.isPhysicallySimulated) colliderA.activate();
							if (colliderB.isPhysicallySimulated) colliderB.activate();
							
							bool flag1 = true;
							if (numContactsLastFrame == 0)
							{
								Vector3d vector5;
								Vector3d vector6;
								double single1;
								colliderA.move( (colliderA.myInternalLinearVelocity * -dt));
								colliderB.move( (colliderB.myInternalLinearVelocity * -dt));
								Vector3d vector7 =  (colliderA.myInternalLinearVelocity * dt);
								Vector3d vector8 =  (colliderB.myInternalLinearVelocity * dt);
                                bool flag2 = Toolbox.areSweptObjectsColliding(colliderA, colliderB,
                                    Math.Min(0.01F, (colliderA.CollisionPrimitive.myCollisionMargin - 0.01F)),
                                    Math.Min(0.01F, (colliderB.CollisionPrimitive.myCollisionMargin - 0.01F)),
                                    ref vector7, ref vector8, out vector5, out vector6, out single1);
								if (flag2 && (single1 > 0.00d))
								{
									colliderA.move( (vector7 * single1));
									colliderB.move( (vector8 * single1));
									flag1 = false;
								}
							}
							if (flag1 && true)
							{
								Vector3d item;
								List<Vector3d> list = ResourcePool.getVectorList();
								List<Vector3d> list2 = ResourcePool.getVectorList();
								if (colliderA.CollisionPrimitive  is BoxPrimitive)
								{
									item = Vector3d.TransformCoord(Vector3d.Right(), colliderA.myInternalOrientationMatrix);
									if (!list.Contains(item) && !list.Contains(-(item)))
									{
										list.Add(item);
									}
									item = Vector3d.TransformCoord(Vector3d.Up(), colliderA.myInternalOrientationMatrix);
									if (!list.Contains(item) && !list.Contains(-(item)))
									{
										list.Add(item);
									}
									item = Vector3d.TransformCoord(Vector3d.Forward(), colliderA.myInternalOrientationMatrix);
									if (!list.Contains(item) && !list.Contains(-(item)))
									{
										list.Add(item);
									}
								}
                                if (colliderB.CollisionPrimitive is BoxPrimitive)
								{
									item = Vector3d.TransformCoord(Vector3d.Right(), colliderB.myInternalOrientationMatrix);
									if (!list.Contains(item) && !list.Contains(-(item)))
									{
										list.Add(item);
									}
									item = Vector3d.TransformCoord(Vector3d.Up(), colliderB.myInternalOrientationMatrix);
									if (!list.Contains(item) && !list.Contains(-(item)))
									{
										list.Add(item);
									}
									item = Vector3d.TransformCoord(Vector3d.Forward(), colliderB.myInternalOrientationMatrix);
									if (!list.Contains(item) && !list.Contains(-(item)))
									{
										list.Add(item);
									}
								}
                                if (((colliderA.CollisionPrimitive is TrianglePrimitive) && !list.Contains(((TrianglePrimitive)colliderA.CollisionPrimitive).Normal)
                                    && !list.Contains(-((TrianglePrimitive)colliderA.CollisionPrimitive).Normal)))
								{
                                    list.Add((colliderA.CollisionPrimitive as TrianglePrimitive).Normal);
								}
                                if ((colliderB.CollisionPrimitive is TrianglePrimitive) && !list.Contains((colliderB.CollisionPrimitive as TrianglePrimitive).Normal))
								{
                                    list.Add((colliderB.CollisionPrimitive as TrianglePrimitive).Normal);
								}
								Vector3d[] vectorArray1 = Controller.testDirections;
								for (int i = 0;i < vectorArray1.Length; i++)
								{
									Vector3d vector10 = vectorArray1[i];
									list2.Add(vector10);
								}
								foreach (Vector3d vector11 in list)
								{
									if (!list2.Contains(vector11) && !list2.Contains(-(vector11)))
									{
										list2.Add(vector11);
									}
								}
								Vector3d vector16 = new Vector3d( );
                                double single9 = double.PositiveInfinity;
								for (int j = 0;j < list2.Count; j++)
								{
									Vector3d vector12;
									Vector3d vector13;
									Vector3d vector14;
									Vector3d vector15;
									Vector3d vector17 = list2[j];
                                    colliderA.CollisionPrimitive.getExtremePoints(ref vector17, out vector12, out vector14, 0.00d);
                                    colliderB.CollisionPrimitive.getExtremePoints(ref vector17, out vector13, out vector15, 0.00d);
									double single4 = Vector3d.DotProduct(vector12, list2[j]);
                                    double single2 = Vector3d.DotProduct(vector14, list2[j]);
                                    double single5 = Vector3d.DotProduct(vector13, list2[j]);
                                    double single3 = Vector3d.DotProduct(vector15, list2[j]);
                                    double single6 = single3 - single4;
                                    double single7 = single2 - single5;
                                    double value = (single6 < single7) ? -single6 : single7;
									if (Math.Abs(value) < single9)
									{
										single9 = Math.Abs(value);
										vector16 = list2[j];
									}
								}
								ResourcePool.giveBack(list);
								ResourcePool.giveBack(list2);
								Vector3d vector18 = colliderB.myInternalCenterPosition - colliderA.myInternalCenterPosition;
								PhysicsBody entity1 = chooseMovableObject(ref vector16);
								entity1.move((vector16 * single9));
								vector3 = Toolbox.getClosestPointsBetweenObjects(colliderA, colliderB, 0.00d, 0.00d, out vector1, out vector2);
                                if (vector3.IsNullOrEmpty())
								{
									entity1.move((vector16 * Math.Min( 0.01F,  (margin * 0.90F))));
								}
								else
								{
                                    double single10 = Math.Max(Math.Min(0.02F, (margin - 0.00d)), 0.00d) - vector3.Length;
									entity1.move( (vector16 * single10));
								}
							}
							updateCollisionDiscreteGJK(dt);
							return;
						}
					}
					contactRefresh();
					contactReduction();
				}
				else
				{
					clearContacts();
				}
				numContactsLastFrame = contacts.Count;
			}
		}
		
		public bool updateCollisionSpecialCases (double dt)
		{
            if ((space.simulationSettings.useSpecialCaseSphereSphere && (colliderA.CollisionPrimitive is SpherePrimitive)) && (colliderB.CollisionPrimitive is SpherePrimitive))
			{
				updateCollisionDiscreteSphereSphere(dt);
				return true;
			}
			if (space.simulationSettings.useSpecialCaseSphereTriangle && (((colliderA.CollisionPrimitive  is TrianglePrimitive)
                && (colliderB.CollisionPrimitive is SpherePrimitive)) || ((colliderA.CollisionPrimitive is SpherePrimitive) && (colliderB.CollisionPrimitive is TrianglePrimitive))))
			{
				updateCollisionDiscreteSphereTriangle(dt);
				return true;
			}
            if (space.simulationSettings.useSpecialCaseBoxSphere && (((colliderA.CollisionPrimitive is BoxPrimitive) && (colliderB.CollisionPrimitive is SpherePrimitive))
                || ((colliderA.CollisionPrimitive is SpherePrimitive) && (colliderB.CollisionPrimitive is BoxPrimitive))))
			{
				updateCollisionDiscreteSphereBox(dt);
				return true;
			}
            if ((space.simulationSettings.useSpecialCaseBoxBox && (colliderA.CollisionPrimitive is BoxPrimitive)) && (colliderB.CollisionPrimitive is BoxPrimitive))
			{
				updateCollisionDiscreteBoxBox(dt);
				return true;
			}
			return false;
		}
		
		public void updateCollisionDiscreteSphereSphere (double dt)
		{
            SpherePrimitive sphere1 = (SpherePrimitive)colliderA.CollisionPrimitive;
            SpherePrimitive sphere2 = (SpherePrimitive)colliderB.CollisionPrimitive;
            double single1 = ((sphere1.Radius + sphere2.Radius) + sphere1.myCollisionMargin) + sphere2.myCollisionMargin;
            Vector3d vector1 = sphere1.CenterPosition - sphere2.CenterPosition;
            double single2 = vector1.Length;
            if ((single2 < single1) && (single2 > 0.00d))
            {
                Vector3d vector2 = vector1 / single2;
                Vector3d vector4 = vector2 * (-sphere1.Radius - sphere1.myCollisionMargin);
                Vector3d vector3 = vector2 * (sphere2.Radius + sphere2.myCollisionMargin);
                vector4 = sphere1.CenterPosition + vector4;
                vector3 = sphere2.CenterPosition + vector3;
                Vector3d vector5 = vector4 + vector3;
                vector5 = vector5 * 0.50F;
                if (contacts.Count == 1)
                {
                    contacts[0].position = vector5;
                    contacts[0].Ra = vector5 - parentA.myInternalCenterOfMass;
                    contacts[0].Rb = vector5 - parentB.myInternalCenterOfMass;
                    contacts[0].penetrationDepth = single1 - single2;
                    contacts[0].normal = vector2;
                }
                else
                {
                    addContact(ref vector5, ref vector2, single1 - single2);
                }
            }
            else
            {
                clearContacts();
            }
		}
		
		public void updateCollisionDiscreteSphereTriangle (double dt)
		{
            throw new NotImplementedException();
            //SpherePrimitive sphere1;
            //Vector3d vector1;
            //Vector3d vector2;
            //TrianglePrimitive triangle1 = (TrianglePrimitive)colliderA.CollisionPrimitive;
            //bool flag1 = true;
            //if (triangle1 == null)
            //{
            //    flag1 = false;
            //    triangle1 = (TrianglePrimitive) colliderB.CollisionPrimitive;
            //    sphere1 = (SpherePrimitive)colliderA.CollisionPrimitive;
            //}
            //else
            //{
            //    sphere1 = (SpherePrimitive)colliderB.CollisionPrimitive;
            //}
            //Toolbox.getClosestPointOnTriangleToPoint(ref triangle1.vertices, ref triangle1.vertices[1], ref triangle1.vertices[2], ref sphere1.CenterPosition, out vector1);
            //if (flag1)
            //{
            //    vector2=vector1-sphere1.CenterPosition;
            //}
            //else
            //{
            //    vector2=sphere1.CenterPosition-vector1;
            //}
            //double single1 = vector2.LengthSquared();
            //double single2 = (sphere1.radius + sphere1.myCollisionMargin) + triangle1.myCollisionMargin;
            //if ((single1 < (single2 * single2)) && (single1 > 0.00d))
            //{
            //    double single3 =  Math.Sqrt( single1);
            //    if (contacts.Count == 1)
            //    {
            //        contacts[0].position = vector1;
            //        contacts[0].Ra=vector1-parentA.myInternalCenterOfMass;
            //        contacts[0].Rb=vector1-parentB.myInternalCenterOfMass;
            //        contacts[0].penetrationDepth = single2 - single3;
            //        if (triangle1.tryToUseFaceNormal)
            //        {
            //            Vector3d vector3 = Vector3d.CrossProduct(contacts[0].normal, triangle1.Normal);
            //            if (Math.Asin(Toolbox.Clamp(vector3.Length, -1.00F, 1.00F)) < triangle1.useFaceNormalWithinAngle)
            //            {
            //                double single4 = Vector3d.DotProduct(contacts[0].normal, triangle1.Normal);
            //                if (single4 < 0.00d)
            //                {
            //                    contacts[0].normal=Vector3d.Inverse(triangle1.Normal);
            //                }
            //                else
            //                {
            //                    contacts[0].normal = triangle1.Normal;
            //                }
            //            }
            //        }
            //        else
            //        {
            //            contacts[0].normal = vector2 /  single3;
            //        }
            //    }
            //    else
            //    {
            //         Vector3d vector4 = vector2 / single3;  
            //        addContact(ref vector1, ref vector4, single2 - single3);
            //    }
            //}
            //else
            //{
            //    clearContacts();
            //}
		}
		
		public void updateCollisionDiscreteSphereBox (double dt)
		{
			SpherePrimitive sphere1;
		    Vector3d vector3;
            BoxPrimitive box1 = colliderA.CollisionPrimitive as BoxPrimitive;
			bool flag1 = true;
			if (box1 == null)
			{
				flag1 = false;
                box1 = colliderB.CollisionPrimitive as BoxPrimitive;
                sphere1 = colliderA.CollisionPrimitive as SpherePrimitive;
			}
			else
			{
				sphere1 = colliderB.CollisionPrimitive as SpherePrimitive;
			}
			Vector3d vector1 = sphere1.CenterPosition-box1.CenterPosition;
            Quaternion quaternion1 = Quaternion.Conjugate( box1.Body.myInternalOrientationQuaternion);
			Vector3d vector2 = Vector3d.TransformCoord(vector1,quaternion1);
			vector2.x = Toolbox.Clamp(vector2.x, -box1.HalfWidth, box1.HalfWidth);
            vector2.y = Toolbox.Clamp(vector2.y, -box1.HalfHeight, box1.HalfHeight);
            vector2.z = Toolbox.Clamp(vector2.z, -box1.HalfLength, box1.HalfLength);
			vector2=Vector3d.TransformCoord(vector2,box1.Body.myInternalOrientationMatrix);
			vector2=vector2+box1.CenterPosition;
			if (flag1)
			{
				vector3=vector2-sphere1.CenterPosition;
			}
			else
			{
				vector3=sphere1.CenterPosition-vector2;
			}
			double single1 = vector3.LengthSquared();
			double single2 = (sphere1.Radius + sphere1.myCollisionMargin) + box1.myCollisionMargin;
			if ((single1 < (single2 * single2)) && (single1 > 0.00d))
			{
				double single3 =  Math.Sqrt( single1);
				if (contacts.Count == 1)
				{
					contacts[0].position = vector2;
					contacts[0].Ra=vector2-parentA.myInternalCenterOfMass;
					contacts[0].Rb=vector2-parentB.myInternalCenterOfMass;
					contacts[0].penetrationDepth = single2 - single3;
					contacts[0].normal = vector3 / single3;
				}
				else
				{
                    Vector3d vector4 = vector3 / single3;
					addContact(ref vector2, ref vector4, single2 - single3);
				}
			}
			else
			{
				clearContacts();
			}
		}
		
		public void updateCollisionDiscreteBoxBox (double dt)
		{
			double single1;
			Vector3d vector1;
            BoxPrimitive  a = (BoxPrimitive)colliderA.CollisionPrimitive;
            BoxPrimitive b = (BoxPrimitive)colliderB.CollisionPrimitive;
			List<Vector3d> manifold = ResourcePool.getVectorList();
			List<int> ids = ResourcePool.getIntList();
			int num1 = collisionType;
			if (Toolbox.areBoxesColliding(a, b, a.myCollisionMargin, b.myCollisionMargin, out single1, out vector1, manifold, ids, out collisionType))
			{
                //System.Diagnostics.Trace.WriteLine("collision...");
				List<Contact> list = ResourcePool.getContactList();
				for (int i = 0;i < contacts.Count; i++)
				{
					bool flag1 = false;
					for (int index = 0;index < manifold.Count; index++)
					{
						Vector3d vector2;
						if (collisionType != num1)
						{
						    vector2 = manifold[index];
							Vector3d vector3 = contacts[i].position-vector2;
							if (vector3.LengthSquared() >= 0.00d) // TODO: wtf?
							{
								continue;
							}
							flag1 = true;
							contacts[i].position = vector2;
							contacts[i].Ra=vector2-parentA.myInternalCenterOfMass;
							contacts[i].Rb=vector2-parentB.myInternalCenterOfMass;
							contacts[i].penetrationDepth = -single1;
							contacts[i].normal = vector1;
							contacts[i].id = ids[index];
							manifold.RemoveAt(index);
							ids.RemoveAt(index);
							break;
						}
						if (contacts[i].id == ids[index])
						{
							flag1 = true;
							vector2 = manifold[index];
							contacts[i].position = vector2;
							contacts[i].Ra=vector2-parentA.myInternalCenterOfMass;
							contacts[i].Rb=vector2-parentB.myInternalCenterOfMass;
							contacts[i].penetrationDepth = -single1;
							contacts[i].normal = vector1;
							manifold.RemoveAt(index);
							ids.RemoveAt(index);
							break;
						}
					}
					if (!flag1)
					{
						list.Add(contacts[i]);
					}
				}
				for (int j = 0;j < manifold.Count; j++)
				{
					int num4 = contacts.Count;
					Vector3d vector4 = manifold[j];
					addContact(ref vector4, ref vector1, -single1);
					if (num4 != contacts.Count)
					{
						contacts[num4].id = ids[j];
					}
				}
				foreach (Contact contact in list)
				{
					removeContact(contact);
				}
				ResourcePool.giveBack(list);
			}
			else
			{
				clearContacts();
			}
			ResourcePool.giveBack(manifold);
			ResourcePool.giveBack(ids);
		}
		
		private PhysicsBody chooseMovableObject (ref Vector3d norm)
		{
			PhysicsBody entity1 = null;
			Vector3d vector1 = colliderB.myInternalCenterPosition - colliderA.myInternalCenterPosition;
			if (colliderA.isPhysicallySimulated && colliderB.isPhysicallySimulated)
			{
				Vector3d vector3;
				Vector3d vector4;
				Vector3d vector2 = space.simulationSettings.gravity;
				if (colliderA.volume > (colliderB.volume * 15.00F))
				{
					if (Vector3d.DotProduct(norm, vector1)< 0.00d)
					{
						norm =  (norm * -1.00F);
					}
					return colliderB;
				}
				if (colliderB.volume > (colliderA.volume * 15.00F))
				{
					if (Vector3d.DotProduct(norm, vector1)> 0.00d)
					{
						norm =  (norm * -1.00F);
					}
					return colliderA;
				}
				if (parentA.mass > (parentB.mass * 50.00F))
				{
					if (Vector3d.DotProduct(norm, vector1)< 0.00d)
					{
						norm = (norm * -1.00F);
					}
					return colliderB;
				}
				if (parentB.mass > (parentA.mass * 50.00F))
				{
					if (Vector3d.DotProduct(norm, vector1)> 0.00d)
					{
						norm =  (norm * -1.00F);
					}
					return colliderA;
				}
				colliderB.CollisionPrimitive.getExtremePoint(ref vector2, 0.00d, out vector4);
                colliderA.CollisionPrimitive.getExtremePoint(ref vector2, 0.00d, out vector3);
				if (Vector3d.DotProduct(vector4, vector2) > Vector3d.DotProduct(vector3, vector2))
				{
					if (Vector3d.DotProduct(norm, vector1)> 0.00d)
					{
						norm =  (norm * -1.00F);
					}
					return colliderA;
				}
				if (Vector3d.DotProduct(norm, vector1)< 0.00d)
				{
					norm =  (norm * -1.00F);
				}
				return colliderB;
			}
			if (colliderA.isPhysicallySimulated)
			{
				if (Vector3d.DotProduct(norm, vector1)> 0.00d)
				{
					norm =  (norm * -1.00F);
				}
				return colliderA;
			}
			if (Vector3d.DotProduct(norm, vector1)< 0.00d)
			{
				norm =  (norm * -1.00F);
			}
			return colliderB;
		}
		
		internal void contactRefreshOld ()
		{
			List<Contact> list = ResourcePool.getContactList();
			for (int i = 0;i < contacts.Count; i++)
			{
			    double single1;
			    Vector3d vector5 = myContactsLocalPositionsA[i];
				Vector3d vector3 = Vector3d.TransformCoord(vector5,colliderA.myInternalOrientationMatrix);
				vector5 = myContactsLocalPositionsB[i];
				Vector3d vector4 = Vector3d.TransformCoord(vector5,colliderB.myInternalOrientationMatrix);
				Vector3d vector1 = parentA.myInternalCenterOfMass+vector3;
				Vector3d vector2 = parentB.myInternalCenterOfMass+vector4;
				if ((contacts.Count == 4) && ((colliderA.controllers.Count == 1) || (colliderB.controllers.Count == 1)))
				{
					single1 = 0.00d;
				}
				else
				{
					single1 = 0.01F;
				}
				vector5=vector2-vector1;
				double value = Vector3d.DotProduct(vector5,contacts[i].normal);
				if ((Math.Abs(value) > single1) || (vector5.LengthSquared() > 0.01F))
				{
					list.Add(contacts[i]);
					continue;
				}
				Vector3d vector6 = vector1+vector2;
				vector6=vector6* 0.50F;
				contacts[i].position = vector6;
				contacts[i].Ra=vector6-parentA.myInternalCenterOfMass;
				contacts[i].Rb=vector6-parentB.myInternalCenterOfMass;
			}
			foreach (Contact contact in list)
			{
				if (contacts.Count == 1)
				{
					break;
				}
				removeContact(contact);
			}
			ResourcePool.giveBack(list);
		}
		
		public void contactRefresh ()
		{
			List<Contact> list = ResourcePool.getContactList();
			for (int i = 0;i < contacts.Count; i++)
			{
			    Vector3d vector5 = myContactsLocalPositionsA[i];
				Vector3d vector3 = Vector3d.TransformCoord(vector5,colliderA.myInternalOrientationMatrix);
				vector5 = myContactsLocalPositionsB[i];
				Vector3d vector4 = Vector3d.TransformCoord(vector5,colliderB.myInternalOrientationMatrix);
				Vector3d vector1 = parentA.myInternalCenterOfMass+vector3;
				Vector3d vector2 = parentB.myInternalCenterOfMass+vector4;
				vector5=vector2-vector1;
				double single1 = Vector3d.DotProduct(vector5,contacts[i].normal);
				Vector3d vector6 = vector5* single1;
				vector5=vector5-vector6;
				single1 = vector5.LengthSquared();
				if (single1 > space.simulationSettings.contactInvalidationLengthSquared)
				{
					list.Add(contacts[i]);
				}
				else
				{
					contacts[i].penetrationDepth = contacts[i].baseDepth + Vector3d.DotProduct(vector2 - vector1, contacts[i].normal);
					if (contacts[i].penetrationDepth < 0.00d)
					{
						list.Add(contacts[i]);
					}
					else
					{
					    Vector3d vector7 = vector1+vector2;
						vector7=vector7* 0.50F;
						contacts[i].position = vector7;
						contacts[i].Ra=vector7-parentA.myInternalCenterOfMass;
						contacts[i].Rb=vector7-parentB.myInternalCenterOfMass;
					}
				}
			}
			foreach (Contact contact in list)
			{
				if (contacts.Count == 1)
				{
					break;
				}
				removeContact(contact);
			}
			ResourcePool.giveBack(list);
		}
		
		private void contactReductionOld ()
		{
			if (contacts.Count > 4)
			{
				int num1 = 0;
				List<int> list = ResourcePool.getIntList();
				for (int item = 0;item < contacts.Count; item++)
				{
					if (contacts[num1].penetrationDepth < contacts[item].penetrationDepth)
					{
						num1 = item;
					}
					list.Add(item);
				}
				list.Remove(num1);
				Vector3d vector1 = contacts[list[0]].position;
				Vector3d vector2 = contacts[list[1]].position;
				Vector3d vector3 = contacts[list[2]].position;
				Vector3d vector4 = vector2 - vector1;
				Vector3d vector5 = vector3 - vector1;
				Vector3d vector6 = Vector3d.CrossProduct(vector4, vector5);
				double val1 = vector6.LengthSquared();
				vector1 = contacts[list[0]].position;
				vector2 = contacts[list[2]].position;
				vector3 = contacts[list[3]].position;
				vector4 = vector2 - vector1;
				vector5 = vector3 - vector1;
				Vector3d vector7 = Vector3d.CrossProduct(vector4, vector5);
				double single2 = vector7.LengthSquared();
				vector1 = contacts[list[1]].position;
				vector2 = contacts[list[2]].position;
				vector3 = contacts[list[3]].position;
				vector4 = vector2 - vector1;
				vector5 = vector3 - vector1;
				Vector3d vector8 = Vector3d.CrossProduct(vector4, vector5);
				double single3 = vector8.LengthSquared();
				vector1 = contacts[list[0]].position;
				vector2 = contacts[list[1]].position;
				vector3 = contacts[list[3]].position;
				vector4 = vector2 - vector1;
				vector5 = vector3 - vector1;
				Vector3d vector9 = Vector3d.CrossProduct(vector4, vector5);
				double val2 = vector9.LengthSquared();
				double single5 = Math.Max(val1, Math.Max(single2, Math.Max(single3, val2)));
				if (single5 == val1)
				{
					removeContact(list[3]);
				}
				else if (single5 == single2)
				{
					removeContact(list[1]);
				}
				else if (single5 == single3)
				{
					removeContact(list[0]);
				}
				else
				{
					if (single5 == val2)
					{
						removeContact(list[2]);
					}
				}
				ResourcePool.giveBack(list);
			}
		}
		
		private void contactReductionBad ()
		{
			if (contacts.Count > 4)
			{
				int num1 = 0;
				for (int i = 0;i < contacts.Count; i++)
				{
					if (contacts[num1].penetrationDepth < contacts[i].penetrationDepth)
					{
						num1 = i;
					}
				}
				if (num1 == 0)
				{
					removeContact(1);
				}
				else
				{
					removeContact(0);
				}
			}
		}
		
		public void contactReduction ()
		{
			if (contacts.Count > 4)
			{
				Vector3d vector2;
				Vector3d vector3;
				int num1 = 0;
				for (int i = 0;i < contacts.Count; i++)
				{
					if (contacts[num1].penetrationDepth < contacts[i].penetrationDepth)
					{
						num1 = i;
					}
				}
				Vector3d vector1 = contacts[num1].normal;
				if (vector1 != Vector3d.Right())
				{
					vector2=Vector3d.CrossProduct(Toolbox.rightVector,vector1);
					vector3=Vector3d.CrossProduct(vector2,vector1);
				}
				else
				{
					vector2 = Toolbox.upVector;
					vector3 = Toolbox.leftVector;
				}
				int index = 0;
				double single1 = double.NegativeInfinity;
				for (int excluded = 0;excluded < 5; excluded++)
				{
					if (excluded != num1)
					{
						double single2 = findManifoldHullVolume(ref vector2, ref vector3, ref vector1, excluded);
						if (single2 > single1)
						{
							single1 = single2;
							index = excluded;
						}
					}
				}
				removeContact(index);
			}
		}
		
		
		public void addContact (Contact contact)
		{
            bool flag1 = true;
            foreach (Contact contact1 in contacts)
            {
                Vector3d vector1 = contact.position - contact1.position;
                if (vector1.LengthSquared() < 0.00d)
                {
                    flag1 = false;
                    break;
                }
            }
            if (flag1)
            {
                if (colliderA.CollisionPrimitive is TrianglePrimitive)
                {
                    TrianglePrimitive triangle1 = (TrianglePrimitive)colliderA.CollisionPrimitive;
                    if (triangle1.tryToUseFaceNormal)
                    {
                        Vector3d vector2 = Vector3d.CrossProduct(contact.normal, triangle1.Normal);
                        if (Math.Asin(Toolbox.Clamp(vector2.Length, -1.00d, 1.00d)) < triangle1.useFaceNormalWithinAngle)
                        {
                            double single1 = Vector3d.DotProduct(contact.normal, triangle1.Normal);
                            if (single1 < 0.00d)
                            {
                                contact.normal = Vector3d.Negate(triangle1.Normal);
                            }
                            else
                            {
                                contact.normal = triangle1.Normal;
                            }
                        }
                    }
                }
                else
                {
                    if (colliderB.CollisionPrimitive is TrianglePrimitive)
                    {
                        TrianglePrimitive triangle2 = (TrianglePrimitive)colliderB.CollisionPrimitive;
                        if (triangle2.tryToUseFaceNormal)
                        {
                            Vector3d vector3 = Vector3d.CrossProduct(contact.normal, triangle2.Normal);
                            if (Math.Asin(Toolbox.Clamp(vector3.Length, -1.00d, 1.00d)) <
                                triangle2.useFaceNormalWithinAngle)
                            {
                                double single2 = Vector3d.DotProduct(contact.normal, triangle2.Normal);
                                if (single2 < 0.00d)
                                {
                                    contact.normal = Vector3d.Negate(triangle2.Normal);
                                }
                                else
                                {
                                    contact.normal = triangle2.Normal;
                                }
                            }
                        }
                    }
                }
                contacts.Add(contact);
                colliderA.onContactCreated(this, contact);
                colliderB.onContactCreated(this, contact);
                if (contacts.Count == 1)
                {
                    colliderA.onInitialCollisionDetected(this);
                    colliderB.onInitialCollisionDetected(this);
                }
                Quaternion quaternion1 = Quaternion.Conjugate(colliderA.myInternalOrientationQuaternion);
                Quaternion quaternion2 = Quaternion.Conjugate(colliderB.myInternalOrientationQuaternion);
                Vector3d item = Vector3d.TransformCoord(contact.Ra, quaternion1);
                Vector3d vector5 = Vector3d.TransformCoord(contact.Rb, quaternion2);
                myContactsLocalPositionsA.Add(item);
                myContactsLocalPositionsB.Add(vector5);
                contactsStable = false;
            }
		}

        public void addContact(ref Vector3d position, ref Vector3d normal, double depth)
		{
            Contact item = ResourcePool.getContact(position, normal, colliderA, colliderB, parentA, parentB, (float)depth);
            addContact(item);
		}
		
		public void removeContact (Contact contact)
		{
			int index = contacts.IndexOf(contact);
			removeContact(index);
		}
		
		public void removeContact (int index)
		{
			contactsStable = false;
			Contact contact = contacts[index];
			colliderA.onContactRemoved(this, contact.position, contact.normal, contact.penetrationDepth);
			colliderB.onContactRemoved(this, contact.position, contact.normal, contact.penetrationDepth);
			ResourcePool.giveBack(contact);
			contacts.RemoveAt(index);
			myContactsLocalPositionsA.RemoveAt(index);
			myContactsLocalPositionsB.RemoveAt(index);
			if (contacts.Count == 0)
			{
				colliderA.onCollisionEnded(this);
				colliderB.onCollisionEnded(this);
			}
		}
		
		public void clearContacts ()
		{
			int num1 = contacts.Count;
			for (int i = 0;i < num1; i++)
			{
				removeContact(0);
			}
		}
		
		private void generateFullManifold ()
		{
			if (space.simulationSettings.useOneShotManifolds && contacts.Count == 1
                && (!(colliderA.CollisionPrimitive is BoundingSphere) && !(colliderB.CollisionPrimitive is BoundingSphere)))
			{
				Contact contact1 = contacts[0];
				Vector3d vector1 = Vector3d.CrossProduct(contact1.Ra, contact1.normal);
                if (!vector1.IsNullOrEmpty())
				{
					Quaternion quaternion1;
					Quaternion quaternion2;
					Quaternion quaternion3;
					Quaternion quaternion4;
					Quaternion quaternion5;
					Quaternion quaternion6;
					Vector3d vector6;
					Vector3d vector7;
					Vector3d vector8;
					Vector3d vector9;
					Vector3d vector10;
					Vector3d vector11;
					Vector3d vector2 = Vector3d.CrossProduct(contact1.normal, vector1);
                    if (contact1.colliderA.isPhysicallySimulated || (!contact1.colliderA.myInternalAngularVelocity.IsNullOrEmpty()))
					{
						quaternion1 = contact1.colliderA.myInternalOrientationQuaternion * new Quaternion(vector1, 0.01F);
						quaternion2 = contact1.colliderA.myInternalOrientationQuaternion *new Quaternion(vector2, 0.01F);
						quaternion3 = contact1.colliderA.myInternalOrientationQuaternion * new Quaternion(vector2, -0.01F);
					}
					else
					{
						quaternion1 = contact1.colliderA.myInternalOrientationQuaternion;
						quaternion2 = contact1.colliderA.myInternalOrientationQuaternion;
						quaternion3 = contact1.colliderA.myInternalOrientationQuaternion;
					}
                    if (contact1.colliderB.isPhysicallySimulated || (!contact1.colliderB.myInternalAngularVelocity.IsNullOrEmpty()))
					{
						quaternion4 = contact1.colliderB.myInternalOrientationQuaternion * new Quaternion(vector1, -0.01F);
						quaternion5 = contact1.colliderB.myInternalOrientationQuaternion * new Quaternion(vector2, -0.01F);
						quaternion6 = contact1.colliderB.myInternalOrientationQuaternion * new Quaternion(vector2, 0.01F);
					}
					else
					{
						quaternion4 = contact1.colliderB.myInternalOrientationQuaternion;
						quaternion5 = contact1.colliderB.myInternalOrientationQuaternion;
						quaternion6 = contact1.colliderB.myInternalOrientationQuaternion;
					}
                    Vector3d vector12 = Toolbox.getClosestPointsBetweenObjects(contact1.colliderA, contact1.colliderB, ref contact1.colliderA.CollisionPrimitive.CenterPosition, ref contact1.colliderB.CollisionPrimitive.CenterPosition, ref quaternion1, ref quaternion4, 0.00d, 0.00d, out vector6, out vector9);
					Vector3d vector3 =  ((vector6 + vector9) / 2.00F);
                    double depth = margin - vector12.Length;
                    Vector3d vector13 = Toolbox.getClosestPointsBetweenObjects(contact1.colliderA, contact1.colliderB, ref contact1.colliderA.CollisionPrimitive.CenterPosition, ref contact1.colliderB.CollisionPrimitive.CenterPosition, ref quaternion2, ref quaternion5, 0.00d, 0.00d, out vector7, out vector10);
					Vector3d vector4 =  ((vector7 + vector10) / 2.00F);
                    double single2 = margin - vector13.Length;
                    Vector3d vector14 = Toolbox.getClosestPointsBetweenObjects(contact1.colliderA, contact1.colliderB, ref contact1.colliderA.CollisionPrimitive.CenterPosition, ref contact1.colliderB.CollisionPrimitive.CenterPosition, ref quaternion3, ref quaternion6, 0.00d, 0.00d, out vector8, out vector11);
					Vector3d vector5 =  ((vector8 + vector11) / 2.00F);
                    double single3 = margin - vector14.Length;
                    if ((!vector12.IsNullOrEmpty()) && (depth > 0.00d))
					{
						addContact(ref vector3, ref vector12, depth);
					}
                    if ((!vector13.IsNullOrEmpty()) && (single2 > 0.00d))
					{
						addContact(ref vector4, ref vector13, single2);
					}
					if ((!vector14.IsNullOrEmpty() ) && (single3 > 0.00d))
					{
						addContact(ref vector5, ref vector14, single3);
					}
				}
			}
		}

        private double findManifoldHullVolume(ref Vector3d x, ref Vector3d y, ref Vector3d normal, int excluded)
        {
            Vector3d vector10;
            List<int> list = ResourcePool.getIntList();
            for (int item = 0; item < contacts.Count; item++)
            {
                if (item != excluded)
                {
                    list.Add(item);
                }
            }
            Vector3d vector1 = contacts[list[0]].position;
            Vector3d vector2 = contacts[list[1]].position;
            Vector3d vector3 = contacts[list[2]].position;
            Vector3d vector4 = vector1 + vector2;
            vector4 = vector4 + vector3;
            vector4 = vector4 * 0.33F;
            vector1 = vector1 - vector4;
            vector2 = vector2 - vector4;
            vector3 = vector3 - vector4;
            Vector3d vector5 = contacts[list[3]].position;
            vector5 = vector5 - vector4;
            ResourcePool.giveBack(list);
            bool flag1 = false;
            Vector3d vector6 = vector2 - vector1;
            Vector3d vector7 = Vector3d.CrossProduct(normal, vector6);
            double single1 = Vector3d.DotProduct(vector1, vector7);
            if (single1 < 0.00d)
            {
                single1 *= -1.00F;
                vector7 = Vector3d.Negate(vector7);
            }
            double single2 = Vector3d.DotProduct(vector7, vector5);
            if (single2 > single1)
            {
                flag1 = true;
            }
            bool flag2 = false;
            Vector3d vector8 = vector3 - vector2;
            vector7 = Vector3d.CrossProduct(normal, vector8);
            single1 = Vector3d.DotProduct(vector2, vector7);
            if (single1 < 0.00d)
            {
                single1 *= -1.00F;
                vector7 = Vector3d.Negate(vector7);
            }
            single2 = Vector3d.DotProduct(vector7, vector5);
            if (single2 > single1)
            {
                flag2 = true;
            }
            bool flag3 = false;
            Vector3d vector9 = vector1 - vector3;
            if (!flag1 || !flag2)
            {
                vector7 = Vector3d.CrossProduct(normal, vector9);
                single1 = Vector3d.DotProduct(vector3, vector7);
                if (single1 < 0.00d)
                {
                    single1 *= -1.00F;
                    vector7 = Vector3d.Negate(vector7);
                }
                single2 = Vector3d.DotProduct(vector7, vector5);
                if (single2 > single1)
                {
                    flag3 = true;
                }
            }
            if ((flag1 || flag2) || flag3)
            {
                Vector3d vector11;
                if ((flag1 && !flag2) && !flag3)
                {
                    vector11 = vector5 - vector3;
                    vector10 = Vector3d.CrossProduct(vector6, vector11);
                    return vector10.LengthSquared();
                }
                if ((!flag1 && flag2) && !flag3)
                {
                    vector11 = vector5 - vector1;
                    vector10 = Vector3d.CrossProduct(vector8, vector11);
                    return vector10.LengthSquared();
                }
                if ((!flag1 && !flag2) && flag3)
                {
                    vector11 = vector5 - vector2;
                    vector10 = Vector3d.CrossProduct(vector9, vector11);
                    return vector10.LengthSquared();
                }
                if (flag1 && flag2)
                {
                    vector11 = vector5 - vector1;
                    vector10 = Vector3d.CrossProduct(vector9, vector11);
                    return vector10.LengthSquared();
                }
                if (flag2 && flag3)
                {
                    vector11 = vector5 - vector1;
                    vector10 = Vector3d.CrossProduct(vector6, vector11);
                    return vector10.LengthSquared();
                }
                if (flag3 && flag1)
                {
                    vector11 = vector5 - vector2;
                    vector10 = Vector3d.CrossProduct(vector8, vector11);
                    return vector10.LengthSquared();
                }
            }
            vector10 = Vector3d.CrossProduct(vector6, vector8);
            return vector10.LengthSquared();
        }
		
        // Performs any initial computations required to solve velocities. Called automatically during the velocity solving phase of the space's update.  
		public void preStep (double dt)
		{
			for (int i = 0;i < contacts.Count; i++)
			{
				calculateNormal[i] = true;
				calculateFriction[i] = true;
				numIterationsAtZeroNormal[i] = 0;
				numIterationsAtZeroFriction[i] = 0;
			}
			foreach (Contact contact1 in contacts)
			{
			    Vector3d vector34;
				Vector3d vector35;
				Vector3d vector36;
				if (colliderA.isPhysicallySimulated && colliderB.isPhysicallySimulated)
				{
				    Vector3d vector3 = Vector3d.CrossProduct(contact1.Ra,contact1.normal);
					Vector3d vector4 = Vector3d.CrossProduct(contact1.Rb,contact1.normal);
					Vector3d vector5 = Vector3d.TransformCoord(vector3,parentA.internalInertiaTensorInverse);
					Vector3d vector6 = Vector3d.TransformCoord(vector4,parentB.internalInertiaTensorInverse);
					Vector3d vector7 = Vector3d.CrossProduct(vector5,contact1.Ra);
					Vector3d vector8 = Vector3d.CrossProduct(vector6,contact1.Rb);
					Vector3d vector9 = vector7+vector8;
					double single1 = Vector3d.DotProduct(vector9,contact1.normal);
                    contact1.normalDenominator =  parentA.massReciprocal + parentB.massReciprocal + single1;
				}
				else if (colliderA.isPhysicallySimulated)
				{
				    Vector3d vector10 = Vector3d.CrossProduct(contact1.Ra,contact1.normal);
					Vector3d vector11 = Vector3d.TransformCoord(vector10,parentA.internalInertiaTensorInverse);
					Vector3d vector12 = Vector3d.CrossProduct(vector11,contact1.Ra);
					double single2 = Vector3d.DotProduct(vector12,contact1.normal);
                    contact1.normalDenominator = parentA.massReciprocal + single2;
				}
				else if (colliderB.isPhysicallySimulated)
				{
				    Vector3d vector13 = Vector3d.CrossProduct(contact1.Rb,contact1.normal);
					Vector3d vector14 = Vector3d.TransformCoord(vector13,parentB.internalInertiaTensorInverse);
					Vector3d vector15 = Vector3d.CrossProduct(vector14,contact1.Rb);
					double single3 = Vector3d.DotProduct(vector15,contact1.normal);
					contact1.normalDenominator =parentB.massReciprocal + single3;
					
				}
				Vector3d vector16 = Vector3d.CrossProduct(parentA.myInternalAngularVelocity,contact1.Ra);
				Vector3d vector17 = parentA.myInternalLinearVelocity+vector16;
				vector16=Vector3d.CrossProduct(parentB.myInternalAngularVelocity,contact1.Rb);
				Vector3d vector18 = parentB.myInternalLinearVelocity+vector16;
				Vector3d vector1 = vector17-vector18;
				double single4 = Vector3d.DotProduct(contact1.normal,vector1);
				Vector3d vector19 = contact1.normal* single4;
				Vector3d vector2 = vector1-vector19;
				if (vector2.LengthSquared() > 0.00d)
				{
				    Vector3d vector20 = -vector2;
				    contact1.tangentDirection = Vector3d.Normalize(vector20);
				}
				else
				{
					contact1.tangentDirection.ZeroVector();
				}
				if (colliderA.isPhysicallySimulated && colliderB.isPhysicallySimulated)
				{
				    Vector3d vector21 = Vector3d.CrossProduct(contact1.Ra,contact1.tangentDirection);
					Vector3d vector22 = Vector3d.CrossProduct(contact1.Rb,contact1.tangentDirection);
					Vector3d vector23 = Vector3d.TransformCoord(vector21,parentA.internalInertiaTensorInverse);
					Vector3d vector24 = Vector3d.TransformCoord(vector22,parentB.internalInertiaTensorInverse);
					Vector3d vector25 = Vector3d.CrossProduct(vector23,contact1.Ra);
					Vector3d vector26 = Vector3d.CrossProduct(vector24,contact1.Rb);
					Vector3d vector27 = vector25+vector26;
					double single5 = Vector3d.DotProduct(vector27,contact1.tangentDirection);
                    contact1.frictionDenominator = parentA.massReciprocal + parentB.massReciprocal + single5;
				}
				else if (colliderA.isPhysicallySimulated)
				{
				    Vector3d vector28 = Vector3d.CrossProduct(contact1.Ra,contact1.tangentDirection);
					Vector3d vector29 = Vector3d.TransformCoord(vector28,parentA.internalInertiaTensorInverse);
					Vector3d vector30 = Vector3d.CrossProduct(vector29,contact1.Ra);
					double single6 = Vector3d.DotProduct(vector30,contact1.tangentDirection);
                    contact1.frictionDenominator = parentA.massReciprocal + single6;
				}
				else if (colliderB.isPhysicallySimulated)
				{
				    Vector3d vector31 = Vector3d.CrossProduct(contact1.Rb,contact1.tangentDirection);
					Vector3d vector32 = Vector3d.TransformCoord(vector31,parentB.internalInertiaTensorInverse);
					Vector3d vector33 = Vector3d.CrossProduct(vector32,contact1.Rb);
					double single7 = Vector3d.DotProduct(vector33,contact1.tangentDirection);
					contact1.frictionDenominator =  parentB.massReciprocal + single7;
				}
				if (contact1.normalImpulseTotal != 0.00d)
				{
					if (colliderA.isPhysicallySimulated)
					{
						vector35 = contact1.normal * contact1.normalImpulseTotal;
						vector34=Vector3d.CrossProduct(contact1.Ra,vector35);
						parentA.applyLinearImpulse(vector35);
						parentA.applyAngularImpulse(vector34);
					}
					if (colliderB.isPhysicallySimulated)
					{
						vector35 = contact1.normal * -contact1.normalImpulseTotal;
						vector36=Vector3d.CrossProduct(contact1.Rb,vector35);
						parentB.applyLinearImpulse(vector35);
						parentB.applyAngularImpulse(vector36);
					}
				}
				if (contact1.frictionImpulseTotal != 0.00d)
				{
					if (colliderA.isPhysicallySimulated)
					{
						vector35 = contact1.tangentDirection * -contact1.frictionImpulseTotal;
						vector34=Vector3d.CrossProduct(contact1.Ra,vector35);
						parentA.applyLinearImpulse(vector35);
						parentA.applyAngularImpulse(vector34);
					}
					if (colliderB.isPhysicallySimulated)
					{
						vector35 = contact1.tangentDirection * contact1.frictionImpulseTotal;
						vector36=Vector3d.CrossProduct(contact1.Rb,vector35);
						parentB.applyLinearImpulse(vector35);
						parentB.applyAngularImpulse(vector36);
					}
				}
				if (vector1.LengthSquared() < (space.simulationSettings.staticFrictionVelocityThreshold * space.simulationSettings.staticFrictionVelocityThreshold))
				{
					contact1.friction = staticFriction;
				}
				else
				{
					contact1.friction = dynamicFriction;
				}
                contact1.bounciness = bounciness;
				if ((bounciness != 0.00d) && (single4 < -space.simulationSettings.bouncinessVelocityThreshold))
				{
					vector16=Vector3d.CrossProduct(parentA.myInternalAngularVelocity,contact1.Ra);
					vector17=parentA.myInternalLinearVelocity+vector16;
					vector16=Vector3d.CrossProduct(parentB.myInternalAngularVelocity,contact1.Rb);
					vector18=parentB.myInternalLinearVelocity+vector16;
					vector1=vector17-vector18;
					single4=Vector3d.DotProduct(contact1.normal,vector1);
                    contact1.restitutionBias = -bounciness * single4;
				}
				else
				{
					contact1.restitutionBias = 0.00d;
				}
                if (space.simulationSettings.useSplitImpulsePositionCorrection)
                {
                    contact1.bias = -(1.00d / dt) * Math.Min(0.00d, allowedPenetration - contact1.penetrationDepth);
                }
                else
                {
                    contact1.bias = -((space.simulationSettings.penetrationRecoveryStiffness / dt) * Math.Min(0.00d, (allowedPenetration - contact1.penetrationDepth)));
                    contact1.bias = Toolbox.Clamp(contact1.bias - contact1.restitutionBias, 0.00d, contact1.bias);
                    if (contact1.bias > space.simulationSettings.maximumPositionCorrectionSpeed)
                    {
                        contact1.bias = space.simulationSettings.maximumPositionCorrectionSpeed;
                    }
                }
			}
		}

        public void applyImpulse()
		{
			int num1 = contacts.Count;
			for (int i = 0;i < num1; i++)
			{
				double single3;
				Vector3d vector1;
				Vector3d vector2;
				Vector3d vector3;
				Vector3d vector4;
				Contact contact1 = contacts[i];
				if (calculateNormal[i])
				{
					double value;
				    Vector3d vector9 = Vector3d.CrossProduct(parentA.myInternalAngularVelocity,contact1.Ra);
					Vector3d vector10 = parentA.myInternalLinearVelocity+vector9;
					vector9=Vector3d.CrossProduct(parentB.myInternalAngularVelocity,contact1.Rb);
					Vector3d vector11 = parentB.myInternalLinearVelocity+vector9;
					vector4=vector11-vector10;
					double single4 = Vector3d.DotProduct(contact1.normal,vector4);
					if (space.simulationSettings.useSplitImpulsePositionCorrection)
					{
						value = (single4 + contact1.restitutionBias) / contact1.normalDenominator;
					}
					else
					{
						value = ((single4 + contact1.restitutionBias) + contact1.bias) / contact1.normalDenominator;
					}
					single3 = contact1.normalImpulseTotal;
					contact1.normalImpulseTotal = Math.Max( (contact1.normalImpulseTotal + value),  0.00d);
					value = contact1.normalImpulseTotal - single3;
					if (colliderA.isPhysicallySimulated)
					{
						vector1=contact1.normal* value;
						vector2=Vector3d.CrossProduct(contact1.Ra,vector1);
						parentA.applyLinearImpulse(vector1);
						parentA.applyAngularImpulse(vector2);
					}
					if (colliderB.isPhysicallySimulated)
					{
						vector1=contact1.normal* -value;
						vector3=Vector3d.CrossProduct(contact1.Rb,vector1);
						parentB.applyLinearImpulse(vector1);
						parentB.applyAngularImpulse(vector3);
					}
					if (Math.Abs(value) < space.simulationSettings.minimumImpulse)
					{
						numIterationsAtZeroNormal[i]++;
					}
					else
					{
						numIterationsAtZeroNormal[i] = 0;
					}
					if (numIterationsAtZeroNormal[i] > space.simulationSettings.iterationsBeforeEarlyOut)
					{
						calculateNormal[i] = false;
					}
					if (space.simulationSettings.useSplitImpulsePositionCorrection)
					{
						Vector3d vector5 = new Vector3d();
                        Vector3d vector6 = new Vector3d();
                        Vector3d vector7 = new Vector3d();
                        Vector3d vector8 = new Vector3d();
						if (colliderA.isPhysicallySimulated)
						{
							vector5 = parentA.correctiveLinearVelocity;
							vector7 = parentA.correctiveAngularVelocity;
						}

						if (colliderB.isPhysicallySimulated)
						{
							vector6 = parentB.correctiveLinearVelocity;
							vector8 = parentB.correctiveAngularVelocity;
						}

						vector9=Vector3d.CrossProduct(vector7,contact1.Ra);
						vector10 = vector5 + vector9;
						vector9=Vector3d.CrossProduct(vector8,contact1.Rb);
						vector11 = vector6 + vector9;
						vector4 = vector11 - vector10;
						single4=Vector3d.DotProduct(contact1.normal,vector4);
						double single5 = (single4 + contact1.bias) / contact1.normalDenominator;
						double single6 = contact1.normalImpulseBiasTotal;
						contact1.normalImpulseBiasTotal = Math.Max( (single6 + single5),  0.00d);
						single5 = contact1.normalImpulseBiasTotal - single6;
						vector1 =  (single5 * contact1.normal);
						if (!vector1.IsNullOrEmpty())
						{
							if (colliderA.isPhysicallySimulated)
							{
							    parentA.correctiveLinearVelocity =parentA.correctiveLinearVelocity * parentA.massReciprocal;
								Vector3d vector12 = Vector3d.CrossProduct(contact1.Ra,vector1);
								vector12=Vector3d.TransformCoord(vector12,parentA.internalInertiaTensorInverse);
								parentA.correctiveAngularVelocity +=vector12;
							}
							if (colliderB.isPhysicallySimulated)
							{
                                parentB.correctiveLinearVelocity = parentB.correctiveLinearVelocity * parentB.massReciprocal;
								Vector3d vector13 = Vector3d.CrossProduct(contact1.Rb,vector1);
								vector13=Vector3d.TransformCoord(vector13,parentB.internalInertiaTensorInverse);
								parentB.correctiveAngularVelocity += vector13;
							}
						}
					}
				}
				if (calculateFriction[i])
				{
				    Vector3d vector14 = Vector3d.CrossProduct(parentA.myInternalAngularVelocity,contact1.Ra);
					Vector3d vector15 = parentA.myInternalLinearVelocity+vector14;
					vector14=Vector3d.CrossProduct(parentB.myInternalAngularVelocity,contact1.Rb);
					Vector3d vector16 = parentB.myInternalLinearVelocity+vector14;
					vector4=vector15-vector16;
					double single7 = Vector3d.DotProduct(vector4,contact1.tangentDirection);
					double single1 = single7 / contact1.frictionDenominator;
					single3 = contact1.frictionImpulseTotal;
					contact1.frictionImpulseTotal = Math.Max(Math.Min((contact1.friction * contact1.normalImpulseTotal), (contact1.frictionImpulseTotal + single1)),  (-contact1.friction * contact1.normalImpulseTotal));
					single1 = contact1.frictionImpulseTotal - single3;
					if (colliderA.isPhysicallySimulated)
					{
						vector1=contact1.tangentDirection* -single1;
						vector2=Vector3d.CrossProduct(contact1.Ra,vector1);
						parentA.applyLinearImpulse(vector1);
						parentA.applyAngularImpulse(vector2);
					}
					if (colliderB.isPhysicallySimulated)
					{
						vector1=contact1.tangentDirection* single1;
						vector3=Vector3d.CrossProduct(contact1.Rb,vector1);
						parentB.applyLinearImpulse(vector1);
						parentB.applyAngularImpulse(vector3);
					}
					if (Math.Abs(single1) < space.simulationSettings.minimumImpulse)
					{
						numIterationsAtZeroFriction[i]++;
					}
					else
					{
						numIterationsAtZeroFriction[i] = 0;
					}
					if (numIterationsAtZeroFriction[i] > space.simulationSettings.iterationsBeforeEarlyOut)
					{
						calculateFriction[i] = false;
					}
				}
			}
		}
	}
}