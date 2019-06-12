using Pokemon;
using System;
using Unity.Entities;
using UnityEngine;

namespace Pokemon
{
	[RequiresEntityConversion]
	public class PlayerDataComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		public string playerName;
		public string pokemonName;
		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			PlayerData pd = new PlayerData
			{
				Name = PokemonIO.StringToByteString30(playerName),
				PokemonName = PokemonIO.StringToByteString30(pokemonName)
			};
			dstManager.AddComponentData(entity, pd);
			dstManager.SetName(entity, "Player \"" + playerName + "\"");
		}
	}
	/// <summary>
	/// Players current data in the scene while its active remeber that the data must be blittable
	/// </summary>
	[Serializable]
	public struct PlayerData : IComponentData
	{
		public ByteString30 Name;
		public ByteString30 PokemonName;
		//   public PokemonEntityData PokemonEntityData;
	}
}