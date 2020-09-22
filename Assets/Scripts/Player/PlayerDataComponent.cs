using Core.UI;
using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Physics;
using Core;
using Unity.Collections;
<<<<<<< Updated upstream
=======
using Pokemon.Player;
using Pokemon.EntityController;
>>>>>>> Stashed changes

namespace Pokemon
{
	[RequiresEntityConversion]
	public class PlayerDataComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		public string playerName;
		public string pokemonName;
		public int PlayerNumber = 1;
		public bool force_save_file_reset = false;
		public bool load_from_file = false;
		//		public AnimatorController anim;
		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			if (PlayerNumber > 4)
				Debug.LogError("Cannot have more than 4 players atm");
			if (playerName != "")
			{
			//	Debug.Log(Application.persistentDataPath);
				PlayerData pd = new PlayerData
				{
					Name = new ByteString30(playerName),
					PokemonName = new ByteString30(pokemonName),
					PlayerNumber = PlayerNumber
				};
				Debug.Log("Adding CoreData with Name: \""+pd.Name+"\" and PokemonName: \""+pd.PokemonName+"\"");
				dstManager.AddComponentData(entity, pd);
				//add controls
<<<<<<< Updated upstream
				dstManager.AddComponentData(entity, new PlayerInput { });
				dstManager.AddComponentData(entity, new StateData { });

			//	PlayerSaveData psd = new PlayerSaveData { };
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
=======
				dstManager.AddComponentData(entity, new EntityControllerStepInput {
					ContactTolerance = CharacterControllerUtilities.ContactTolerance,
					MaxIterations = CharacterControllerUtilities.MaxIterations,
					MaxSlope = CharacterControllerUtilities.MaxSlope,
					Tau = CharacterControllerUtilities.k_DefaultTau,
					SkinWidth = CharacterControllerUtilities.SkinWidth,
					AffectsPhysicsBodies = CharacterControllerUtilities.AffectsPhysicsBodies,
					Gravity = CharacterControllerUtilities.Gravity
					
				});
		
>>>>>>> Stashed changes
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
		public int PlayerNumber;
		//   public PokemonEntityData PokemonEntityData;
	}
	[Serializable]
	public struct PlayerSaveData
	{
		public PlayerData playerData;
		public PokemonEntityData pokemonEntityData;
		public Translation position;
		public Rotation rotation;
		public bool isValid;
	}
}