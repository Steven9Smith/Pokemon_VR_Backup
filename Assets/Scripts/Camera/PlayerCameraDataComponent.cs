using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Core.Camera;
using Core;
using Unity.Mathematics;

namespace Core.Camera
{
	public class PlayerCameraDataComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		[Tooltip("Set starting View mode")]
		public Core.CoreFunctionsClass.CameraViewMode ViewMode;

		public bool InvertYCamera = false;

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			dstManager.AddComponentData(entity, new PlayerCameraComponentDataRequest { });
			dstManager.AddComponentData(entity, new PlayerCameraComponentData
			{
				ViewMode = ViewMode,
				invertY = InvertYCamera
			});
			// Camera data  doesn't have coredata so we can set the name here
			dstManager.SetName(entity, name);
		}
	}
	[System.Serializable]
	public struct PlayerCameraComponentData : IComponentData
	{
		public Entity CameraEntity; // this will have to be found after the system has started
		public CoreFunctionsClass.CameraViewMode ViewMode;
		public CameraOffsetData cameraOffsetComponent;
		public int index;
		public BlittableBool invertY;
		public BlittableBool offsetSet;
		public float3 CurrentCameraOffset;
	}
	[System.Serializable]
	public struct PlayerCameraComponentDataRequest : IComponentData
	{

	}
	
}