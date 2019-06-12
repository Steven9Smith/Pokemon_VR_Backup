using Pokemon;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;
using Unity.Jobs;
using Pokemon.Move;
using Unity.Transforms;

namespace Pokemon{
	/// <summary>
	/// made to set some varibles in the eneity that has an PokemonEntityData
	/// </summary>
	public class PokemonEntitySpawntSystem : ComponentSystem
	{
		public EntityQuery PokemonEntitySpawnQuery;
		NativeArray<Entity> pokemonEntities;
		protected override void OnCreateManager()
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
				EntityManager.SetComponentData<PhysicsMass>(pokemonEntities[i], new PhysicsMass {
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
	[UpdateAfter(typeof(PokemonMoveDataWithInputSystem))]
	public class PokemonMoveDataRemoveSystem : JobComponentSystem
	{
		EntityCommandBufferSystem ecbs;
		EntityQuery pokemonMoveDataQuery;
		protected override void OnCreateManager()
		{
			ecbs = World.GetOrCreateSystem<EntityCommandBufferSystem>();
			pokemonMoveDataQuery = GetEntityQuery(typeof(PokemonMoveData));
		}
		private struct RemovePokemonData : IJob
		{
			public EntityCommandBuffer ecb;
			[DeallocateOnJobCompletion] public NativeArray<PokemonMoveData> pmds;
			[DeallocateOnJobCompletion] public NativeArray<Entity> entities;
			public void Execute()
			{
				for (int i = 0; i < entities.Length; i++)
				{
					if (!pmds[i].isValid || !pmds[i].pokemonMoveAdjustmentData.isValid)
						ecb.RemoveComponent<PokemonMoveData>(entities[i]);
				}
			}
		}
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			if (pokemonMoveDataQuery.CalculateLength() == 0) return inputDeps;
			JobHandle jh = new RemovePokemonData
			{
				entities = pokemonMoveDataQuery.ToEntityArray(Allocator.TempJob),
				ecb = ecbs.CreateCommandBuffer(),
				pmds = pokemonMoveDataQuery.ToComponentDataArray<PokemonMoveData>(Allocator.TempJob),
			}.Schedule(inputDeps);
			jh.Complete();
			return jh;
		}
	}
	public class PokemonMoveDataWithInputSystem : JobComponentSystem
	{
		private struct PokemonMoveJob : IJobForEach<PokemonMoveData, Translation, Rotation, PhysicsVelocity, PlayerInput,PokemonEntityData>
		{
			public float deltaTime;
			public void Execute(ref PokemonMoveData pokemonMoveData, ref Translation translation, ref Rotation rotation, ref PhysicsVelocity velocity, ref PlayerInput input,ref PokemonEntityData ped)
			{
				if (!pokemonMoveData.isValid)
				{
					Debug.Log("Detected that pokemonMoveData is invalid");
					return;
				}
				if (pokemonMoveData.pokemonMoveAdjustmentData.isValid)
				{
					if (pokemonMoveData.pokemonMoveAdjustmentData.pokemonMoveAngularVelocitySet.value.isValid)
					{
						float3 realValue = getNextPokemonMoveAdjustment(ref pokemonMoveData.pokemonMoveAdjustmentData.pokemonMoveAngularVelocitySet.value, ref ped.currentStamina, deltaTime, input.forward);
						velocity.Angular += realValue;
					}
					if (pokemonMoveData.pokemonMoveAdjustmentData.pokemonMoveVelocitySet.value.isValid)
					{
						Debug.Log("adding a velocity");
						float3 realValue = getNextPokemonMoveAdjustment(ref pokemonMoveData.pokemonMoveAdjustmentData.pokemonMoveVelocitySet.value, ref ped.currentStamina, deltaTime, input.forward);
						velocity.Linear += realValue;
					}
					if (pokemonMoveData.pokemonMoveAdjustmentData.pokemonMoveTranslationSet.value.isValid)
					{
						float3 realValue = getNextPokemonMoveAdjustment(ref pokemonMoveData.pokemonMoveAdjustmentData.pokemonMoveTranslationSet.value, ref ped.currentStamina, deltaTime, input.forward);
						translation.Value += realValue;
					}
					if (pokemonMoveData.pokemonMoveAdjustmentData.pokemonMoveRotationSet.value.isValid)
					{
						float3 realValue = getNextPokemonMoveAdjustment(ref pokemonMoveData.pokemonMoveAdjustmentData.pokemonMoveRotationSet.value, ref ped.currentStamina, deltaTime, input.forward);
						rotation.Value.value += new float4 { x = realValue.x, y = realValue.y, z = realValue.z, w = pokemonMoveData.pokemonMoveAdjustmentData.pokemonMoveRotationSet.w };
					}
					pokemonMoveData.pokemonMoveAdjustmentData.isValid = pokemonMoveData.pokemonMoveAdjustmentData.pokemonMoveAngularVelocitySet.value.isValid ||
						pokemonMoveData.pokemonMoveAdjustmentData.pokemonMoveVelocitySet.value.isValid ||
						pokemonMoveData.pokemonMoveAdjustmentData.pokemonMoveTranslationSet.value.isValid ||
						pokemonMoveData.pokemonMoveAdjustmentData.pokemonMoveRotationSet.value.isValid;
				}
			}
		}
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			return new PokemonMoveJob { deltaTime = Time.deltaTime }.Schedule(this, inputDeps);
		}
		/// <summary>
		/// returns the calculated value of value of the current PokemonMoveAdjustment
		/// </summary>
		/// <param name="set">se to go through</param>
		/// <param name="time">deltaTime</param>
		/// <param name="forward">camera formard</param>
		/// <returns>float3</returns>
		public static float3 getNextPokemonMoveAdjustment(ref PokemonMoveAdjustmentSet set,ref float currentStamina, float time, float3 forward)
		{
			for (int i = 0; i < 25; i++)
			{
				PokemonMoveAdjustment pma = getAdjustment(set, i);
				if (pma.timeLength == -1f)
				{
					Debug.Log("Detected 1 time addition"+time+" forward = "+forward); 
					//one time thing
					pma.timeLength = 0;
					set.isValid = false;
					if (currentStamina - pma.staminaCost < 0)
					{
						float3 temp = pma.useCameraDirection ? pma.timeLength * forward * pma.value : pma.timeLength * pma.value;
						temp *= currentStamina/pma.staminaCost;
						currentStamina = 0;
						return temp;
					}
					else {
						currentStamina -=pma.staminaCost;
						return pma.useCameraDirection? forward * pma.value : pma.value;
					}
				}
				else if(pma.timeLength > 0)
				{
					Debug.Log("Detected it's not over");
					if (pma.timeLength - time >= 0)
					{
						pma.timeLength -= time;
						if (currentStamina - pma.staminaCost < 0)
						{
							float3 temp = pma.useCameraDirection ? pma.timeLength * forward * pma.value : pma.timeLength * pma.value;
							temp *= currentStamina / pma.staminaCost;
							currentStamina = 0;
							return temp;
						}
						else
						{
							currentStamina -= pma.staminaCost;
							return pma.useCameraDirection ? time * forward * pma.value : time * pma.value;
						}
					}
					else
					{
						float3 temp = pma.timeLength;
						pma.timeLength = 0;
						if (currentStamina - pma.staminaCost < 0)
						{
							temp = pma.useCameraDirection ? temp * forward * pma.value : time * pma.value;
							temp *= currentStamina / pma.staminaCost;
							currentStamina = 0;
							return temp;
						}
						else
						{
							currentStamina -= pma.staminaCost;
							return pma.useCameraDirection ? temp * forward * pma.value : time * pma.value;
						}
					}
				} 
				else if (i == 24) set.isValid = false;
			}
			return float3.zero;
		}
		public static PokemonMoveAdjustment getAdjustment(PokemonMoveAdjustmentSet set, int index)
		{
			switch (index)
			{
				case 0: return set.A;
				case 1: return set.B;
				case 2: return set.C;
				case 3: return set.D;
				case 4: return set.E;
				case 5: return set.F;
				case 6: return set.G;
				case 7: return set.H;
				case 8: return set.I;
				case 9: return set.J;
				case 10: return set.K;
				case 11: return set.L;
				case 12: return set.M;
				case 13: return set.N;
				case 14: return set.O;
				case 15: return set.P;
				case 16: return set.Q;
				case 17: return set.R;
				case 18: return set.S;
				case 19: return set.T;
				case 20: return set.U;
				case 21: return set.V;
				case 22: return set.W;
				case 23: return set.X;
				case 24: return set.Y;
				default: return set.A;
			}
		}

	}
}