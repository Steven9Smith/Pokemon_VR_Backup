using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
namespace Pokemon
{
	public class CameraComponent : MonoBehaviour {
		public float3 offset;
		public float smoothingSpeed;
		public Camera camera;
		public int viewMode = 0;
	}
	/*
	[RequiresEntityConversion]
	public class CameraComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		public float3 offset;
		public float smoothingSpeed;
		public bool isCamera;
		public Camera camera;
		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			CameraDataComponent cd = new CameraDataComponent
			{
				offset = new float3(0, offset.y, offset.z),
				isCamera = isCamera,
				smoothingSpeed = smoothingSpeed,
				cam = camera,
				isValid = true
			};
			dstManager.AddSharedComponentData(entity, cd);
			dstManager.SetName(entity, "Camera");
		}
	}
	[Serializable]
	public struct CameraDataComponent : ISharedComponentData, IEquatable<CameraDataComponent>
	{
		public float3 offset;
		public float smoothingSpeed;
		public BlittableBool isCamera;
		public Camera cam;
		public BlittableBool isValid;
		public override bool Equals(object obj)
		{
			if ((obj == null) || !this.GetType().Equals(obj.GetType())) return false; //Check for null and compare run-time types.
			else
			{
				CameraDataComponent other = (CameraDataComponent)obj;
				return this.offset.Equals(other.offset) && this.smoothingSpeed == other.smoothingSpeed && this.isCamera == other.isCamera && this.cam.Equals(other.cam);
			}
		}
		public bool Equals(CameraDataComponent other) { return this.offset.Equals(other.offset) && this.smoothingSpeed == other.smoothingSpeed && this.isCamera == other.isCamera && this.cam.Equals(other.cam); }
		public override int GetHashCode() { return base.GetHashCode(); }
	}*/
}