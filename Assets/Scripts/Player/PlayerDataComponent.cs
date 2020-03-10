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
using Pokemon.Player;

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
			//	Debug.Log(Application.persistentDataPath);
				PlayerData pd = new PlayerData
				{
					Name = new ByteString30(playerName),
					PokemonName = new ByteString30(pokemonName)
				};
				Debug.Log("aaa"+pd.Name+",,,ss "+pd.PokemonName);
				dstManager.AddComponentData(entity, pd);
				//add controls
				dstManager.AddComponentData(entity, new PlayerInput { });
				dstManager.AddComponentData(entity, new EntityControllerStepInput { });
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
			}
			else Debug.LogError("Cannot load player data without a name");
		}
	}
	public class PlayerSystem : JobComponentSystem
	{
	//	UISystem uiSystem;
		Entity PlayerEntity;
		EntityArchetype PlayerArchetype;

		protected override void OnCreate()
		{
			/*	//	uiSystem = World.GetOrCreateSystem<UISystem>();
					//create type thing
					PlayerArchetype = EntityManager.CreateArchetype(
						typeof(Translation),
						typeof(Rotation),
						typeof(RenderMesh),
						typeof(LocalToWorld),
					//	typeof(PhysicsMass),
						typeof(PhysicsCollider),
					//	typeof(PhysicsVelocity), //<-render and collider delete when this is added or set
						typeof(Scale)
					//	typeof(CoreData),
					//	typeof(LivingEntity),
					//	typeof(PlayerInput),
					//	typeof(StateData),
					);*/
			PlayerArchetype = EntityManager.CreateArchetype(
					typeof(Translation),
					typeof(Rotation),
					typeof(RenderMesh),
					typeof(LocalToWorld),
					typeof(PhysicsCollider),
					typeof(PhysicsVelocity),
					typeof(TranslationProxy),
					typeof(RotationProxy)
				);
		}
		protected override void OnStartRunning()
		{
			//LoadPlayer
		//	NativeArray<Entity> playerEntities = new NativeArray<Entity>(3, Allocator.TempJob);
		//	EntityManager.CreateEntity(PlayerArchetype, playerEntities);
			//		EntityManager.SetComponentData(PlayerEntity, new PlayerData {
			//			PokemonName = new ByteString30("Electrode"),
			//			Name = new ByteString30("Player1")
			//		});
		//	EntityManager.SetComponentData(playerEntities[0], new Scale { Value = 1f });
		//	PokemonIO.LoadPlayerData(EntityManager, playerEntities[0], "Player1");
		//	playerEntities.Dispose();
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			return inputDeps;
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