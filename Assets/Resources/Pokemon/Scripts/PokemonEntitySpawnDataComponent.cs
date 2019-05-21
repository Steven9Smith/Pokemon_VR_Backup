using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
namespace Pokemon
{
	[RequiresEntityConversion]
	public class PokemonEntitySpawnDataComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			dstManager.AddComponentData<PokemonEntitySpawnData>(entity,new PokemonEntitySpawnData { });
		}
	}

	public struct PokemonEntitySpawnData : IComponentData
	{
		//literally used just as a filter
	}
}