using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
namespace Pokemon
{
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
				offset = new float3(0,offset.y,offset.z),
				isCamera = isCamera,
				smoothingSpeed = smoothingSpeed,
				cam = camera
			};
			dstManager.AddSharedComponentData(entity, cd);
			dstManager.SetName(entity, "Camera");
		}
	}
	[Serializable]
	public struct CameraDataComponent : ISharedComponentData
	{
		public float3 offset;
		public float smoothingSpeed;
		public BlittableBool isCamera;
		public Camera cam;
	}

}