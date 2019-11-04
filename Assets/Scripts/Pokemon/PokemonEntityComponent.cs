using System;
using Unity.Entities;
using UnityEngine;

namespace Pokemon
{
	[RequiresEntityConversion]
	public class PokemonEntityComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		public string EntityId;
		public string EntityName;
		public string PokemonName;
	//	public PokemonEntityData PokemonEntityData;
		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			PokemonEntity pe = new PokemonEntity {
				EntityName = PokemonIO.StringToByteString30(EntityName),
				pokemonName = PokemonIO.StringToByteString30(PokemonName)
				//			PokemonEntityData = PokemonEntityData
			};
			dstManager.AddComponentData(entity, pe);
		}
	}

	[Serializable]
	public struct PokemonEntity : IComponentData
	{
		public ByteString30 EntityName;
		public ByteString30 pokemonName;
	//	public PokemonEntityData PokemonEntityData;
	}
//	public class PokemonEntityComponent : ComponentDataProxy<PokemonEntity> { }
}