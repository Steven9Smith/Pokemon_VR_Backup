using Pokemon;
using Pokemon.Animation;
using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Pokemon
{
	[RequiresEntityConversion]
	public class PlayerDataComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		public string playerName;
		public string pokemonName;
		//		public AnimatorController anim;
		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			if (playerName != "")
			{
				Debug.Log(Application.persistentDataPath);
				PlayerData pd = new PlayerData
				{
					Name = PokemonIO.StringToByteString30(playerName),
					PokemonName = PokemonIO.StringToByteString30(pokemonName)
				};
				dstManager.AddComponentData(entity, pd);
				//add controls
				dstManager.AddComponentData(entity, new PlayerInput { });
				dstManager.AddComponentData(entity, new StateData { });

				PlayerSaveData psd = new PlayerSaveData { };
				dstManager.SetName(entity, "Player \"" + playerName + "\"");
				PokemonIO.LoadPlayerData(dstManager, entity, playerName);

				
				//load the animator data if there is any
				
		/*		if (anim != null)
				{
					Debug.Log("discovered an aniator");
					if (!dstManager.HasComponent<PokemonAnimationData>(entity)) dstManager.AddSharedComponentData(entity, new PokemonAnimationData
					{
						animator = anim
					});
					else dstManager.SetSharedComponentData(entity, new PokemonAnimationData
					{
						animator = anim
					});
					if (!dstManager.HasComponent<PokemonAnimationVerifier>(entity)) dstManager.AddComponentData(entity, new PokemonAnimationVerifier { });
				}
				else Debug.LogError("Cannot get Animator because the anim was not set");	*/
			}
			else Debug.LogError("Cannot load player data without a name");
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
	[Serializable]
	public struct PlayerSaveData
	{
		public PlayerData playerData;
		public PokemonEntityData pokemonEntityData;
		public bool isValid;
	}
}