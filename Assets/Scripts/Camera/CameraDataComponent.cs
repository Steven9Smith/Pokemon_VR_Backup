using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
namespace Core.Camera
{
	[RequiresEntityConversion]
	public class CameraDataComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			dstManager.AddComponentData(entity, new CameraComponentData { });
			dstManager.SetName(entity,name);
		}
	}

	[System.Serializable]
	public struct CameraComponentData : IComponentData
	{
		public BlittableBool isFree;
		public Entity CameraEntity;
	}

}