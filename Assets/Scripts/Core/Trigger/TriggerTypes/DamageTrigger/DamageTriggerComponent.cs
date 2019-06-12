using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Pokemon
{
	[RequiresEntityConversion]
	[DisallowMultipleComponent]
	public class DamageTriggerComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		public int damageValue = 1;
		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			dstManager.AddComponentData(entity, new DamageTriggerData { Value = damageValue });
		}
	}
	public struct DamageTriggerData : IComponentData
	{
		public int Value;
	}
}