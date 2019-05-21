using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Pokemon
{
	[RequiresEntityConversion]
	[DisallowMultipleComponent]
	public class SphereRMComponentData : MonoBehaviour, IConvertGameObjectToEntity
	{
		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			dstManager.AddComponentData(entity, new SphereEntityRenderMesh { });
		}
	}
	[Serializable]
	public struct SphereEntityRenderMesh : IComponentData { }

}