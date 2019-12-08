using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using Unity.Jobs;
using Pokemon.Move;
using Unity.Transforms;
using Core.Particles;
using Core.ParentChild;
using Unity.Burst;
using Core.Spawning;

namespace Pokemon
{
	/// <summary>
	/// made to set some varibles in the eneity that has an PokemonEntityData
	/// </summary>
	public class PokemonEntitySpawnSystem : ComponentSystem
	{
		public EntityQuery PokemonEntitySpawnQuery;
		NativeArray<Entity> pokemonEntities;
		protected override void OnCreate()
		{
			PokemonEntitySpawnQuery = GetEntityQuery(typeof(PokemonEntitySpawnData));
		}
		protected override void OnUpdate()
		{
			//get entity array
			pokemonEntities = PokemonEntitySpawnQuery.ToEntityArray(Allocator.TempJob);
			for (int i = 0; i < pokemonEntities.Length; i++)
			{
				PhysicsCollider pc = EntityManager.GetComponentData<PhysicsCollider>(pokemonEntities[i]);
				PokemonEntityData ped = EntityManager.GetComponentData<PokemonEntityData>(pokemonEntities[i]);
				PhysicsMass pm = EntityManager.GetComponentData<PhysicsMass>(pokemonEntities[i]);
				float radius = calculateSphereRadius(pc.Value.Value.MassProperties.Volume);
				//		Debug.Log("Mass = " + ped.Mass + "InverseMAss = " + (1 / ped.Mass) + " inverseInertia ="+ (1/(0.4f*ped.Mass*(radius*radius))));
				EntityManager.SetComponentData<PhysicsMass>(pokemonEntities[i], new PhysicsMass
				{
					AngularExpansionFactor = pm.AngularExpansionFactor,
					Transform = pm.Transform,
					InverseMass = (1 / ped.Mass),
					InverseInertia = (1 / (0.4f * ped.Mass * (radius * radius)))
				});
				EntityManager.RemoveComponent(pokemonEntities[i], typeof(PokemonEntitySpawnData));
			}
			pokemonEntities.Dispose();
		}
		private float calculateSphereRadius(float volume)
		{
			return math.pow((volume / math.PI) * 0.75f, 1f / 3f);
		}
	}
	/// <summary>
	/// This Job listens for PokemonMove Remove Requests and preforms the right sets in order to remove a pokemonMove
	/// </summary>
	public class PokemonMoveDataRemoveSystem : JobComponentSystem
	{
		EntityCommandBufferSystem ecbs;
		EntityQuery pokemonMoveDataQuery;
		EntityQuery pokemonMoveFinishEntities;
		protected override void OnCreate()
		{
			ecbs = World.GetOrCreateSystem<EntityCommandBufferSystem>();
			//The Filter
			pokemonMoveDataQuery = GetEntityQuery(typeof(PokemonMoveDataEntity), typeof(PokemonMoveEntity), typeof(EntityParent));
			pokemonMoveFinishEntities = GetEntityQuery(typeof(PokemonMoveEntityRemoveRequest), typeof(PokemonMoveDataEntity));
		}
		private struct RemovePokemonData : IJob
		{
			public EntityCommandBuffer ecb;
			[DeallocateOnJobCompletion] public NativeArray<PokemonMoveDataEntity> pokemonMoveDatas;
			[DeallocateOnJobCompletion] public NativeArray<Entity> pokemonMoveDataEntities;
			[DeallocateOnJobCompletion] public NativeArray<EntityParent> parents;	//i really should call this parents
			[ReadOnly] public ComponentDataFromEntity<ParticleSystemRemoveRequest> hasParticleRemoveRequest;
			[ReadOnly] public ComponentDataFromEntity<GroupIndexChangeRemoveRequest> hasGroupIndexChangeRemoveRequest;
			[DeallocateOnJobCompletion] public NativeArray<Entity> pokemonMoveEntityEntities;
			[DeallocateOnJobCompletion] public NativeArray<PokemonMoveDataEntity> pokemonMoveRemoveDatas;
			
			private int i;
			public void Execute()
			{
				for (i = 0; i < pokemonMoveDatas.Length; i++)
					if (!pokemonMoveDatas[i].isValid)
					{
						//remove EntityChild from parent
						ecb.RemoveComponent<EntityChild>(parents[i].entity);
						//remove group index
						ecb.AddComponent(pokemonMoveDataEntities[i], new GroupIndexChangeRemoveRequest {
							removeFromArray = true
						});
						//we have a valid pokemon move data
						if (parents[i].isValid)
						{
							//lets remove the PokemonMoveDataEntity Component so it no longer fires the PokemonMoveDataEntity Job
							ecb.RemoveComponent<PokemonMoveDataEntity>(parents[i].entity);
							//		ecb.RemoveComponent<EntityChild>(parents[i].entity); <-outdated (EntityChild was removed)
							ecb.AddComponent(parents[i].entity, new GroupIndexChangeRemoveRequest {removeFromArray = false });
						}
						//remove the EntityParent Component so the Entity no longer follows the Pokemon Entity
						ecb.RemoveComponent<EntityParent>(pokemonMoveDataEntities[i]);
						//if the Pokemon Move has particles then request to get them removed
						if (pokemonMoveDatas[i].hasParticles)
							ecb.AddComponent(pokemonMoveDataEntities[i], new ParticleSystemRemoveRequest { });
						//add the pokemon remove request
						ecb.AddComponent(pokemonMoveDataEntities[i], new PokemonMoveEntityRemoveRequest { });
					}
				for (i = 0; i < pokemonMoveEntityEntities.Length; i++)
				{
					//destroy entities that match these conditions
					if (!pokemonMoveRemoveDatas[i].hasParticles) ecb.DestroyEntity(pokemonMoveEntityEntities[i]);
					else if (!hasParticleRemoveRequest.Exists(pokemonMoveEntityEntities[i]) && !hasGroupIndexChangeRemoveRequest.Exists(pokemonMoveEntityEntities[i])) ecb.DestroyEntity(pokemonMoveEntityEntities[i]);
				}
			}
		}
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			//test if we have any requests
			if (pokemonMoveDataQuery.CalculateEntityCount() == 0 && pokemonMoveFinishEntities.CalculateEntityCount() == 0) return inputDeps;
			//preform the job
			JobHandle jh = new RemovePokemonData
			{
				pokemonMoveDataEntities = pokemonMoveDataQuery.ToEntityArray(Allocator.TempJob),
				ecb = ecbs.CreateCommandBuffer(),
				pokemonMoveDatas = pokemonMoveDataQuery.ToComponentDataArray<PokemonMoveDataEntity>(Allocator.TempJob),
				parents = pokemonMoveDataQuery.ToComponentDataArray<EntityParent>(Allocator.TempJob),
				pokemonMoveEntityEntities = pokemonMoveFinishEntities.ToEntityArray(Allocator.TempJob),
				hasParticleRemoveRequest = GetComponentDataFromEntity<ParticleSystemRemoveRequest>(),
				hasGroupIndexChangeRemoveRequest = GetComponentDataFromEntity<GroupIndexChangeRemoveRequest>(),
				pokemonMoveRemoveDatas = pokemonMoveFinishEntities.ToComponentDataArray<PokemonMoveDataEntity>(Allocator.TempJob)
			}.Schedule(inputDeps);
			jh.Complete();
			return jh;
		}
	}
	/// <summary>
	/// This is a Job. This Job preforms a move adjustment which is either a AngularVelocty, Velocity, Translation, Rotation, and/or Scale
	/// </summary>
	public class PokemoMoveDataEntity : JobComponentSystem
	{
		//NOTE: if you want to debug the values then remove the [BurstCompile] tag to prevent an error
		[BurstCompile]
		private struct PokemonMoveEntityJob : IJobForEachWithEntity<PokemonMoveEntity, PokemonMoveDataEntity, Translation, Rotation, PhysicsVelocity, Scale>
		{
			public float deltaTime;
			[ReadOnly] public ComponentDataFromEntity<GroupIndexChangeRequest> hasGroupIndexChangeRequest;
			public void Execute(Entity entity, int index,ref PokemonMoveEntity pokemonMoveEntity, ref PokemonMoveDataEntity pokemonMoveDataEntity,
				ref Translation translation, ref Rotation rotation, ref PhysicsVelocity velocity, ref Scale scale)
			{
				if (!hasGroupIndexChangeRequest.Exists(entity))
				{
					if (!pokemonMoveDataEntity.isValid)
					{
						return;
					}
					if (pokemonMoveDataEntity.pokemonMoveAdjustmentData.isValid)
					{
						//	Debug.Log("Doing the execute");
						if (pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveAngularVelocitySet.value.isValid)
						{
							float3 realValue = PokemonMoves.getNextPokemonMoveAdjustment(ref pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveAngularVelocitySet.value,
								 deltaTime, pokemonMoveDataEntity.forward);
							if(pokemonMoveDataEntity.preformActionsOn) velocity.Angular += realValue;
							//		Debug.Log("Angular = "+velocity.Angular.ToString());
						}
						if (pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveVelocitySet.value.isValid)
						{
							float3 realValue = PokemonMoves.getNextPokemonMoveAdjustment(
							ref pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveVelocitySet.value,
								 deltaTime, pokemonMoveDataEntity.forward);
							//			Debug.Log(pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveVelocitySet.value.A.value.ToString()
							//				+"::::"+velocity.Linear.ToString() + "::" + realValue + ":::" + pokemonMoveDataEntity.forward);

							if(pokemonMoveDataEntity.preformActionsOn) velocity.Linear += realValue;
							//Debug.Log(pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveVelocitySet.value.A.timeLength);
						}
						if (pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveTranslationSet.value.isValid)
						{
							float3 realValue = PokemonMoves.getNextPokemonMoveAdjustment(ref pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveTranslationSet.value,
								deltaTime, pokemonMoveDataEntity.forward);
							translation.Value += realValue;
						}
						if (pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveRotationSet.value.isValid)
						{
							float3 realValue = PokemonMoves.getNextPokemonMoveAdjustment(ref pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveRotationSet.value,
								 deltaTime, pokemonMoveDataEntity.forward);
							if(pokemonMoveDataEntity.preformActionsOn) rotation.Value.value += new float4 { x = realValue.x, y = realValue.y, z = realValue.z, w = pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveRotationSet.w };
						}
						if (pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveScaleSet.isValid)
						{
							float realValue = PokemonMoves.getNextPokemonMoveAdjustment(
									ref pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveScaleSet, deltaTime);
							if(pokemonMoveDataEntity.preformActionsOn) scale.Value = realValue;
						}
						pokemonMoveDataEntity.pokemonMoveAdjustmentData.isValid = pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveAngularVelocitySet.value.isValid ||
							pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveVelocitySet.value.isValid ||
							pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveTranslationSet.value.isValid ||
							pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveRotationSet.value.isValid;
					}
					else pokemonMoveDataEntity.isValid = false;
				}//else do nothing
			}
		}
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			return new PokemonMoveEntityJob { deltaTime = Time.deltaTime,
				hasGroupIndexChangeRequest = GetComponentDataFromEntity<GroupIndexChangeRequest>(),
			}.Schedule(this, inputDeps);
		}

	}
	public class PokemonMoveParentSystem : JobComponentSystem
	{
		//NOTE: if you want to debug the values then remove the [BurstCompile] tag to prevent an error
		[BurstCompile]
		private struct PokemonMoveEntityJob : IJobForEachWithEntity<PokemonEntityData, PokemonMoveDataEntity, Translation, Rotation, PhysicsVelocity, Scale>
		{
			public float deltaTime;
			[ReadOnly] public ComponentDataFromEntity<GroupIndexChangeRequest> hasGroupIndexChangeRequest;
			public void Execute(Entity entity, int index, ref PokemonEntityData ped, ref PokemonMoveDataEntity pokemonMoveDataEntity,
				ref Translation translation, ref Rotation rotation, ref PhysicsVelocity velocity, ref Scale scale)
			{
				if (!hasGroupIndexChangeRequest.Exists(entity))
				{
					if (!pokemonMoveDataEntity.isValid)return;
					if (pokemonMoveDataEntity.pokemonMoveAdjustmentData.isValid)
					{
						if (pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveAngularVelocitySet.value.isValid)
						{
							float3 realValue = PokemonMoves.getNextPokemonMoveAdjustment(ref pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveAngularVelocitySet.value,
								 ref ped.currentStamina, deltaTime, pokemonMoveDataEntity.forward);
							velocity.Angular += realValue;
						}
						if (pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveVelocitySet.value.isValid)
						{
							float3 realValue = PokemonMoves.getNextPokemonMoveAdjustment(
							ref pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveVelocitySet.value,
								 ref ped.currentStamina, deltaTime, pokemonMoveDataEntity.forward);
							velocity.Linear += realValue;
						}
						if (pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveTranslationSet.value.isValid)
						{
							float3 realValue = PokemonMoves.getNextPokemonMoveAdjustment(ref pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveTranslationSet.value,
								ref ped.currentStamina, deltaTime, pokemonMoveDataEntity.forward);
							translation.Value += realValue;
						}
						if (pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveRotationSet.value.isValid)
						{
							float3 realValue = PokemonMoves.getNextPokemonMoveAdjustment(ref pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveRotationSet.value,
								 ref ped.currentStamina, deltaTime, pokemonMoveDataEntity.forward);
							rotation.Value.value += new float4 { x = realValue.x, y = realValue.y, z = realValue.z, w = pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveRotationSet.w };
						}
						if (pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveScaleSet.isValid)
						{
							float realValue = PokemonMoves.getNextPokemonMoveAdjustment(
									ref pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveScaleSet, deltaTime, ref ped.currentStamina);
							scale.Value = realValue;
						}
						pokemonMoveDataEntity.pokemonMoveAdjustmentData.isValid = pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveAngularVelocitySet.value.isValid ||
							pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveVelocitySet.value.isValid ||
							pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveTranslationSet.value.isValid ||
							pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveRotationSet.value.isValid;
					}
					else pokemonMoveDataEntity.isValid = false;
				}
			}
		}
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			return new PokemonMoveEntityJob
			{
				deltaTime = Time.deltaTime,
				hasGroupIndexChangeRequest = GetComponentDataFromEntity<GroupIndexChangeRequest>(),
			}.Schedule(this, inputDeps);
		}
	}

	/*	public class PokemonMoveData : JobComponentSystem
		{
			private struct PokemonMoveJob : IJobForEach<PokemonMoveDataEntity, Translation, Rotation, PhysicsVelocity, PokemonEntityData,PlayerInput>
			{
				public float deltaTime;
				public void Execute(ref PokemonMoveDataEntity pokemonMoveDataEntity, ref Translation translation,
					ref Rotation rotation, ref PhysicsVelocity velocity, ref PokemonEntityData ped, ref PlayerInput pi)
				{
					if (!pokemonMoveDataEntity.isValid){return; }
					if (pokemonMoveDataEntity.pokemonMoveAdjustmentData.isValid)
					{
						if (pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveAngularVelocitySet.value.isValid)
						{
							float3 realValue = PokemonMoves.getNextPokemonMoveAdjustment(
								ref pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveAngularVelocitySet.value,
								ref ped.currentStamina, deltaTime, pi.forward);
							velocity.Angular += realValue;
						}
						if (pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveVelocitySet.value.isValid)
						{
							float3 realValue = PokemonMoves.getNextPokemonMoveAdjustment(
								ref pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveVelocitySet.value,
								 ref ped.currentStamina, deltaTime, pi.forward);
							velocity.Linear += realValue;
						}
						if (pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveTranslationSet.value.isValid)
						{
							float3 realValue = PokemonMoves.getNextPokemonMoveAdjustment(ref pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveTranslationSet.value,
								ref ped.currentStamina, deltaTime, pi.forward);
							translation.Value += realValue;
						}
						if (pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveRotationSet.value.isValid)
						{
							float3 realValue = PokemonMoves.getNextPokemonMoveAdjustment(ref pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveRotationSet.value,
								 ref ped.currentStamina, deltaTime,  pi.forward);
							rotation.Value.value += new float4 { x = realValue.x, y = realValue.y, z = realValue.z, w = pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveRotationSet.w };
						}
						pokemonMoveDataEntity.pokemonMoveAdjustmentData.isValid = pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveAngularVelocitySet.value.isValid ||
							pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveVelocitySet.value.isValid ||
							pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveTranslationSet.value.isValid ||
							pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveRotationSet.value.isValid;
					}
					else pokemonMoveDataEntity.isValid = false;
				}


			}
			protected override JobHandle OnUpdate(JobHandle inputDeps)
			{
				return new PokemonMoveJob { deltaTime = Time.deltaTime }.Schedule(this, inputDeps);	
			}
		}*/

}