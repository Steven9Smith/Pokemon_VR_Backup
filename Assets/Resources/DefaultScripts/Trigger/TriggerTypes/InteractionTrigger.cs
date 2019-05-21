
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Pokemon
{
	
	public struct SpawnSphere : IComponentData {
		public float3 position;
		public quaternion quan;
		public BlittableBool valid;
	}
	[UpdateAfter(typeof(TriggerEventSystem))]
	public class InteractionTriggerSystem : JobComponentSystem
	{
		private EntityQuery interactionTriggerQuery;
		private NativeArray<SpawnSphere> spawnSpheres;
		private int spawnSpheresLength;
		private RenderMesh rm;
		private int i;
		protected override void OnCreateManager()
		{
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);

			rm = new RenderMesh
			{
				mesh = go.GetComponent<MeshFilter>().sharedMesh,
				material = go.GetComponent<MeshRenderer>().material,
				castShadows = UnityEngine.Rendering.ShadowCastingMode.On,
				receiveShadows = true
			};
			GameObject.Destroy(go);
			interactionTriggerQuery = GetEntityQuery(typeof(InteractionTrigger),typeof(PlayerInput),typeof(Rotation),typeof(Translation));

		}
		[BurstCompile]
		struct InteractionTriggerJob : IJob
		{
			[WriteOnly] public EntityCommandBuffer ecb;
			[DeallocateOnJobCompletion] public NativeArray<PlayerInput> playerInputs;
			[DeallocateOnJobCompletion] public NativeArray<Translation> translations;
			[DeallocateOnJobCompletion] public NativeArray<Rotation> rotations;
			[DeallocateOnJobCompletion] public NativeArray<Entity> entities;
			public NativeArray<SpawnSphere> spawnSpheres;
			public int spawnSpheresLength;		//passed as an copy
			public int i;
			public void Execute()
			{
				NativeArray<SpawnSphere> a = new NativeArray<SpawnSphere>(spawnSpheresLength, Allocator.Temp);
				spawnSpheresLength = 0;
				for (i = 0; i < entities.Length; i++)
				{
					if (playerInputs[i].EDown)
					{
				//		Debug.Log("Detected an E press!");
						float3 nPosition = translations[i].Value;
						nPosition.y += 2f;
						a[spawnSpheresLength] = new SpawnSphere { position = nPosition, quan = rotations[i].Value,valid = true };
						spawnSpheresLength++;
					}
				}
				if(spawnSpheresLength != 0) a.CopyTo(spawnSpheres);
				a.Dispose();
			}
		}
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			spawnSpheresLength = interactionTriggerQuery.CalculateLength();
			if (spawnSpheresLength == 0) return inputDeps;
	//		Debug.Log("running InteractionTrigger");
			spawnSpheres = new NativeArray<SpawnSphere>(spawnSpheresLength,Allocator.TempJob);
			InteractionTriggerJob ij = new InteractionTriggerJob {
				entities = interactionTriggerQuery.ToEntityArray(Allocator.TempJob),
				playerInputs = interactionTriggerQuery.ToComponentDataArray<PlayerInput>(Allocator.TempJob),
				translations = interactionTriggerQuery.ToComponentDataArray<Translation>(Allocator.TempJob),
				rotations = interactionTriggerQuery.ToComponentDataArray<Rotation>(Allocator.TempJob),
				spawnSpheres = spawnSpheres,	//can change content but not size because you can't use TempJob inside an IJob
				spawnSpheresLength = spawnSpheresLength,	//just using as a temp variable, cannot change the value in the IJob
				i = i
			};
			JobHandle jh = ij.Schedule(inputDeps);
			jh.Complete();
			//can't use length because I can't change the size of the native array in the Job, only the contents it seems

		//	spawnSpheresLength = spawnSpheres.Length;
			if (spawnSpheres.Length == 0  || !spawnSpheres[0].valid)
			{
				spawnSpheres.Dispose();
				return jh;
			}
//			else Debug.Log("Detected some SpawnSpheres! ");
			//"real" size is unknown unless a for loop is make to cout all the valid SpawnSpheres inside the NativeArray but that is not needed due to my skillz
			for (i = 0; i < spawnSpheresLength; i++)
			{
				if (spawnSpheres[i].valid) CreateDynamicSphere(rm, 0.5f, spawnSpheres[i].position, spawnSpheres[i].quan);
				else break;
			}
			spawnSpheres.Dispose();
			return jh;
		}
		public Entity CreateDynamicSphere(RenderMesh displayMesh, float radius, float3 position, quaternion orientation)
		{
			// Sphere with default filter and material. Add to Create() call if you want non default:
			BlobAssetReference<Unity.Physics.Collider> spCollider = Unity.Physics.SphereCollider.Create(float3.zero, radius, new CollisionFilter { CategoryBits = TriggerEventClass.NPC, MaskBits = uint.MaxValue, GroupIndex = 0 });
			return CreateBody(displayMesh, position, orientation, spCollider, float3.zero, float3.zero, 1.0f, true);
		}
		public Entity CreateBody(RenderMesh displayMesh, float3 position, quaternion orientation,
		BlobAssetReference<Unity.Physics.Collider> collider,
		float3 linearVelocity, float3 angularVelocity, float mass, bool isDynamic)
		{

			ComponentType[] componentTypes = new ComponentType[isDynamic ? 9 : 6];

			componentTypes[0] = typeof(RenderMesh);
			componentTypes[1] = typeof(TranslationProxy);
			componentTypes[2] = typeof(RotationProxy);
			componentTypes[3] = typeof(PhysicsCollider);
			componentTypes[4] = typeof(Translation);
			componentTypes[5] = typeof(Rotation);
			if (isDynamic)
			{
				componentTypes[6] = typeof(PhysicsVelocity);
				componentTypes[7] = typeof(PhysicsMass);
				componentTypes[8] = typeof(PhysicsDamping);
			}
			EntityManager entityManager = EntityManager;

			Entity entity = entityManager.CreateEntity(componentTypes);





			entityManager.SetName(entity, "randomSphere");
			entityManager.SetSharedComponentData(entity, displayMesh);

			entityManager.AddComponentData(entity, new LocalToWorld { });
			entityManager.SetComponentData(entity, new Translation { Value = position });
			entityManager.SetComponentData(entity, new Rotation { Value = orientation });
			entityManager.SetComponentData(entity, new PhysicsCollider { Value = collider });
			if (isDynamic)
			{
				MassProperties massProperties = collider.Value.MassProperties;
				entityManager.SetComponentData(entity, PhysicsMass.CreateDynamic(massProperties, mass));

				float3 angularVelocityLocal = math.mul(math.inverse(massProperties.MassDistribution.Transform.rot), angularVelocity);
				entityManager.SetComponentData(entity, new PhysicsVelocity()
				{
					Linear = linearVelocity,
					Angular = angularVelocityLocal
				});
				entityManager.SetComponentData(entity, new PhysicsDamping()
				{
					Linear = 0.01f,
					Angular = 0.05f
				});
			}

			return entity;
		}

	}
}
