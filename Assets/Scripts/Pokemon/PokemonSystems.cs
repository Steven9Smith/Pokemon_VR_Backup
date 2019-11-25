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
	public class PokemonMoveDataRemoveSystem : JobComponentSystem
	{
		EntityCommandBufferSystem ecbs;
		EntityQuery pokemonMoveDataQuery;
		EntityQuery pokemonMoveFinishEntities;
		protected override void OnCreate()
		{
			ecbs = World.GetOrCreateSystem<EntityCommandBufferSystem>();
			pokemonMoveDataQuery = GetEntityQuery(typeof(PokemonMoveDataEntity), typeof(PokemonMoveEntity), typeof(EntityParent));
			pokemonMoveFinishEntities = GetEntityQuery(typeof(PokemonMoveEntityRemoveRequest), typeof(PokemonMoveDataEntity));
		}
		private struct RemovePokemonData : IJob
		{
			public EntityCommandBuffer ecb;
			[DeallocateOnJobCompletion] public NativeArray<PokemonMoveDataEntity> pokemonMoveDatas;
			[DeallocateOnJobCompletion] public NativeArray<Entity> pokemonMoveDataEntities;
			[DeallocateOnJobCompletion] public NativeArray<EntityParent> children;
			[ReadOnly] public ComponentDataFromEntity<ParticleSystemRemoveRequest> hasParticleRemoveRequest;
			[DeallocateOnJobCompletion] public NativeArray<Entity> pokemonMoveEntityEntities;
			[DeallocateOnJobCompletion] public NativeArray<PokemonMoveDataEntity> pokemonMoveRemoveDatas;
			private int i;
			public void Execute()
			{
				for (i = 0; i < pokemonMoveDatas.Length; i++)
					if (!pokemonMoveDatas[i].isValid)
					{
						if (children[i].isValid)
						{
							ecb.RemoveComponent<PokemonMoveDataEntity>(children[i].entity);
							ecb.RemoveComponent<EntityChild>(children[i].entity);
						}
						ecb.RemoveComponent<EntityParent>(pokemonMoveDataEntities[i]);
						if (pokemonMoveDatas[i].hasParticles)
							ecb.AddComponent(pokemonMoveDataEntities[i], new ParticleSystemRemoveRequest { });
						ecb.AddComponent(pokemonMoveDataEntities[i], new PokemonMoveEntityRemoveRequest { });
					}
				for (i = 0; i < pokemonMoveEntityEntities.Length; i++)
				{
					if (!pokemonMoveRemoveDatas[i].hasParticles) ecb.DestroyEntity(pokemonMoveEntityEntities[i]);
					else if (!hasParticleRemoveRequest.Exists(pokemonMoveEntityEntities[i])) ecb.DestroyEntity(pokemonMoveEntityEntities[i]);
				}
			}
		}
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			if (pokemonMoveDataQuery.CalculateEntityCount() == 0 && pokemonMoveFinishEntities.CalculateEntityCount() == 0) return inputDeps;

			JobHandle jh = new RemovePokemonData
			{
				pokemonMoveDataEntities = pokemonMoveDataQuery.ToEntityArray(Allocator.TempJob),
				ecb = ecbs.CreateCommandBuffer(),
				pokemonMoveDatas = pokemonMoveDataQuery.ToComponentDataArray<PokemonMoveDataEntity>(Allocator.TempJob),
				children = pokemonMoveDataQuery.ToComponentDataArray<EntityParent>(Allocator.TempJob),
				pokemonMoveEntityEntities = pokemonMoveFinishEntities.ToEntityArray(Allocator.TempJob),
				hasParticleRemoveRequest = GetComponentDataFromEntity<ParticleSystemRemoveRequest>(),
				pokemonMoveRemoveDatas = pokemonMoveFinishEntities.ToComponentDataArray<PokemonMoveDataEntity>(Allocator.TempJob)
			}.Schedule(inputDeps);
			jh.Complete();
			return jh;
		}
	}
	public class PokemoMoveDataEntity : JobComponentSystem
	{
		private struct PokemonMoveEntityJob : IJobForEach<PokemonMoveEntity, PokemonMoveDataEntity, Translation, Rotation, PhysicsVelocity, Scale>
		{
			public float deltaTime;
			public void Execute(ref PokemonMoveEntity pokemonMoveEntity, ref PokemonMoveDataEntity pokemonMoveDataEntity,
				ref Translation translation, ref Rotation rotation, ref PhysicsVelocity velocity, ref Scale scale)
			{
				if (!pokemonMoveDataEntity.isValid)
				{
					return;
				}
				if (pokemonMoveDataEntity.pokemonMoveAdjustmentData.isValid)
				{
					Debug.Log("Doing the execute");
					if (pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveAngularVelocitySet.value.isValid)
					{
						float3 realValue = PokemonMoves.getNextPokemonMoveAdjustment(ref pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveAngularVelocitySet.value,
							 deltaTime, pokemonMoveDataEntity.forward);
						velocity.Angular += realValue;
						Debug.Log("Angular = "+velocity.Angular.ToString());
					}
					if (pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveVelocitySet.value.isValid)
					{
						float3 realValue = PokemonMoves.getNextPokemonMoveAdjustment(
						ref pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveVelocitySet.value,
							 deltaTime, pokemonMoveDataEntity.forward);
						Debug.Log(pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveVelocitySet.value.A.value.ToString()
							+"::::"+velocity.Linear.ToString() + "::" + realValue + ":::" + pokemonMoveDataEntity.forward);

						velocity.Linear += realValue;
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
						rotation.Value.value += new float4 { x = realValue.x, y = realValue.y, z = realValue.z, w = pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveRotationSet.w };
					}
					if (pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveScaleSet.isValid)
					{
						float realValue = PokemonMoves.getNextPokemonMoveAdjustment(
								ref pokemonMoveDataEntity.pokemonMoveAdjustmentData.pokemonMoveScaleSet, deltaTime);
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
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			return new PokemonMoveEntityJob { deltaTime = Time.deltaTime }.Schedule(this, inputDeps);
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