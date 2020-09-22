using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
namespace Core.Camera
{
	public class CameraOffsetComponent : MonoBehaviour,IConvertGameObjectToEntity
	{
		public string PokemonName;
		public float3 firstPersonOffset;
		public float3 thridPersonOffset;
		public float smoothingSpeed;
		public bool useGeneratedValues = true;

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			if (!useGeneratedValues)
				dstManager.AddComponentData(entity, new CameraOffsetData
				{
					firstPersonOffset = firstPersonOffset,
					smoothingSpeed = smoothingSpeed,
					thridPersonOffset = thridPersonOffset
				});
	//		else CoreFunctionsClass.SetCameraOffsetData(PokemonName, entity, dstManager);
		}
	}
	[Serializable]
	public struct CameraOffsetData : IComponentData
	{
		public float3 firstPersonOffset;
		public float3 thridPersonOffset;
		public float smoothingSpeed;
	}
}