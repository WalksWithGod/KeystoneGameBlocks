//using Keystone.Physics.DataStructures;
using Keystone.Physics.Entities;
using Keystone.Physics.Events;
using Keystone.Types;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Keystone.Physics
{
	public class DetectorVolume : Updateable
	{
        // Instance Fields
        //public TriangleMesh triangleMesh;
        //public Dictionary<PhysicsBody, ContainmentState> nearbyEntities;

        // Events
        public event EventHandlerEntityBeginsTouchingVolume eventEntityBeginsTouchingVolume;
        public event EventHandlerEntityStopsTouchingVolume eventEntityStopsTouchingVolume;
        public event EventHandlerVolumeBeginsContainingEntity eventVolumeBeginsContainingEntity;
        public event EventHandlerVolumeStopsContainingEntity eventVolumeStopsContainingEntity;

        //// Constructors
        //public DetectorVolume (TriangleMesh triangleMesh)
        //{
        //    this.nearbyEntities = new Dictionary<PhysicsBody,ContainmentState>();
        //    this.triangleMesh = triangleMesh;
        //}
		
		
		// Methods
		public bool isPointInVolume (Vector3d point)
		{
            //List<Vector3d> hitLocations = ResourcePool.getVectorList();
            //List<Vector3d> hitNormals = ResourcePool.getVectorList();
            //List<float> tois = ResourcePool.getFloatList();
            //this.triangleMesh.rayCast(point, Toolbox.upVector, 10000000.00d, hitLocations, hitNormals, tois, true);
            //int num1 = hitLocations.Count;
            //ResourcePool.giveBack(hitLocations);
            //ResourcePool.giveBack(hitNormals);
            //ResourcePool.giveBack(tois);
            //return (num1 % 2) == 1;
		    return true; // TODO: temp added this hack line 
		}
		
		public bool isEntityWithinVolume (PhysicsBody physicsBody, float margin)
		{
            //List<int> triangleIndices = ResourcePool.getIntList();
            //this.triangleMesh.hierarchy.getNearbyTriangles(ref physicsBody.boundingBox, triangleIndices);
            //foreach (int num1 in triangleIndices)
            //{
            //    Triangle objB = ResourcePool.getStaticTriangle(this.triangleMesh.vertices[this.triangleMesh.indices[num1]].position, this.triangleMesh.vertices[this.triangleMesh.indices[num1 + 1]].position, this.triangleMesh.vertices[this.triangleMesh.indices[num1 + 2]].position);
            //    if (Toolbox.areObjectsColliding(physicsBody, objB, margin, 0.00F))
            //    {
            //        ResourcePool.giveBack(objB);
            //        ResourcePool.giveBack(triangleIndices);
            //        return false;
            //    }
            //    ResourcePool.giveBack(objB);
            //}
            //ResourcePool.giveBack(triangleIndices);
            return this.isPointInVolume(physicsBody.myInternalCenterPosition);
		}
		
		public bool isEntityIntersectingVolume (PhysicsBody physicsBody, float margin)
		{
            //if (this.isPointInVolume(physicsBody.centerPosition))
            //{
            //    return true;
            //}
            //List<int> triangleIndices = ResourcePool.getIntList();
            //this.triangleMesh.hierarchy.getNearbyTriangles(ref physicsBody.boundingBox, triangleIndices);
            //foreach (int num1 in triangleIndices)
            //{
            //    Vector3d v1;
            //    Vector3d v2;
            //    Vector3d v3;
            //    this.triangleMesh.getTransformedPosition(this.triangleMesh.indices[num1], out v1);
            //    this.triangleMesh.getTransformedPosition(this.triangleMesh.indices[num1 + 1], out v2);
            //    this.triangleMesh.getTransformedPosition(this.triangleMesh.indices[num1 + 2], out v3);
            //    Triangle objB = ResourcePool.getStaticTriangle(v1, v2, v3);
            //    if (Toolbox.areObjectsColliding(physicsBody, objB, margin, 0.00F))
            //    {
            //        ResourcePool.giveBack(objB);
            //        ResourcePool.giveBack(triangleIndices);
            //        return true;
            //    }
            //    ResourcePool.giveBack(objB);
            //}
            //ResourcePool.giveBack(triangleIndices);
			return false;
		}
		
		public bool isEntityIntersectingVolume (PhysicsBody physicsBody, float margin, out bool isContained)
		{
            //List<int> triangleIndices = ResourcePool.getIntList();
            //this.triangleMesh.hierarchy.getNearbyTriangles(ref physicsBody.boundingBox, triangleIndices);
            //foreach (int num1 in triangleIndices)
            //{
            //    Vector3d v1;
            //    Vector3d v2;
            //    Vector3d v3;
            //    this.triangleMesh.getTransformedPosition(this.triangleMesh.indices[num1], out v1);
            //    this.triangleMesh.getTransformedPosition(this.triangleMesh.indices[num1 + 1], out v2);
            //    this.triangleMesh.getTransformedPosition(this.triangleMesh.indices[num1 + 2], out v3);
            //    Triangle objB = ResourcePool.getStaticTriangle(v1, v2, v3);
            //    if (Toolbox.areObjectsColliding(physicsBody, objB, margin, 0.00F))
            //    {
            //        ResourcePool.giveBack(objB);
            //        ResourcePool.giveBack(triangleIndices);
            //        isContained = false;
            //        return true;
            //    }
            //    ResourcePool.giveBack(objB);
            //}
            //if (this.isPointInVolume(physicsBody.centerPosition))
            //{
            //    isContained = true;
            //    return true;
            //}
            isContained = false;
            //ResourcePool.giveBack(triangleIndices);
			return false;
		}
		
		public bool isEntityIntersectingVolume (PhysicsBody physicsBody, float margin, List<int> firstVertexIndices, List<int> secondVertexIndices, List<int> thirdVertexIndices)
		{
            //List<int> triangleIndices = ResourcePool.getIntList();
            //this.triangleMesh.hierarchy.getNearbyTriangles(ref physicsBody.boundingBox, triangleIndices);
            //foreach (int num1 in triangleIndices)
            //{
            //    Vector3d v1;
            //    Vector3d v2;
            //    Vector3d v3;
            //    int index = this.triangleMesh.indices[num1];
            //    int item = this.triangleMesh.indices[num1 + 1];
            //    int num4 = this.triangleMesh.indices[num1 + 2];
            //    this.triangleMesh.getTransformedPosition(index, out v1);
            //    this.triangleMesh.getTransformedPosition(item, out v2);
            //    this.triangleMesh.getTransformedPosition(num4, out v3);
            //    Triangle objB = ResourcePool.getStaticTriangle(v1, v2, v3);
            //    if (Toolbox.areObjectsColliding(physicsBody, objB, margin, 0.00F))
            //    {
            //        firstVertexIndices.Add(index);
            //        secondVertexIndices.Add(item);
            //        thirdVertexIndices.Add(num4);
            //    }
            //    ResourcePool.giveBack(objB);
            //}
            //ResourcePool.giveBack(triangleIndices);
            //if (firstVertexIndices.Count <= 0)
            //{
            //    return this.isPointInVolume(physicsBody.centerPosition);
            //}
			return true;
		}
		
		public override void updateAtEndOfUpdate (float dt, float timeScale, float timeSinceLastFrame)
		{
            //List<PhysicsBody> entities = ResourcePool.getEntityList();
            //foreach (PhysicsBody item in this.nearbyEntities.Keys)
            //{
            //    if (!item.boundingBox.Intersects(this.triangleMesh.hierarchy.boundingBox))
            //    {
            //        entities.Add(item);
            //    }
            //}
            //foreach (PhysicsBody entity in entities)
            //{
            //    if (this.nearbyEntities[((TKey) entity)].isContained && (this.eventVolumeStopsContainingEntity != null))
            //    {
            //        this.eventVolumeStopsContainingEntity(this, entity);
            //    }
            //    if (this.nearbyEntities[((TKey) entity)].isTouching && (this.eventEntityStopsTouchingVolume != null))
            //    {
            //        this.eventEntityStopsTouchingVolume(entity, this);
            //    }
            //    this.nearbyEntities.Remove(entity);
            //}
            //entities.Clear();
            //base.mySpace.broadPhase.getEntities(this.triangleMesh.hierarchy.boundingBox, entities);
            //foreach (PhysicsBody key in entities)
            //{
            //    bool flag2;
            //    ContainmentState state1;
            //    bool flag1 = this.isEntityIntersectingVolume(key, key.myCollisionMargin, out flag2);
            //    if (this.nearbyEntities.TryGetValue(key, out state1))
            //    {
            //        if (!state1.isTouching && flag1)
            //        {
            //            if (this.eventEntityBeginsTouchingVolume != null)
            //            {
            //                this.eventEntityBeginsTouchingVolume(key, this);
            //            }
            //        }
            //        else
            //        {
            //            if ((state1.isTouching && !flag1) && (this.eventEntityStopsTouchingVolume != null))
            //            {
            //                this.eventEntityStopsTouchingVolume(key, this);
            //            }
            //        }
            //        if (!state1.isContained && flag2)
            //        {
            //            if (this.eventVolumeBeginsContainingEntity != null)
            //            {
            //                this.eventVolumeBeginsContainingEntity(this, key);
            //            }
            //        }
            //        else
            //        {
            //            if ((state1.isContained && !flag2) && (this.eventVolumeStopsContainingEntity != null))
            //            {
            //                this.eventVolumeStopsContainingEntity(this, key);
            //            }
            //        }
            //        this.nearbyEntities[(TKey) key] = (TValue) new ContainmentState(flag1, flag2);
            //        continue;
            //    }
            //    this.nearbyEntities.Add(key, new ContainmentState(flag1, flag2));
            //    if (flag1 && (this.eventEntityBeginsTouchingVolume != null))
            //    {
            //        this.eventEntityBeginsTouchingVolume(key, this);
            //    }
            //    if (flag2 && (this.eventVolumeBeginsContainingEntity != null))
            //    {
            //        this.eventVolumeBeginsContainingEntity(this, key);
            //    }
            //}
            //base.updateAtEndOfUpdate(dt, timeScale, timeSinceLastFrame);
            //ResourcePool.giveBack(entities);
		}
	}
}
