using System.Collections;
using System.Collections.Generic;
using Unity.Entities;

using Unity.Physics;
using Unity.Physics.Authoring;
using Pokemon;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Core.Terrain
{
	public static class TerrianBuilder
	{
		public static EntityArchetype TerrainArchtype = new EntityArchetype { };
		private static bool terrainArchtypeSet = false;
		public static Unity.Physics.TerrainCollider.CollisionMethod Method;

		public static unsafe float* HeightsToFloatArray(TerrainData terrainData)
		{
			float* heights = null;
			if (terrainData.size != Vector3.zero)
			{
				heights = (float*)UnsafeUtility.Malloc((int)terrainData.size.x * (int)terrainData.size.z * sizeof(float), 4, Allocator.Temp);
				float[,] a = terrainData.GetHeights(0, 0, (int)terrainData.size.x, (int)terrainData.size.z);
				for (int i = 0; i < (int)terrainData.size.x; i++)
					for (int j = 0; j < (int)terrainData.size.z; j++)
						heights[j + (i * (int)terrainData.size.x)] = a[i, j];

			}
			else Debug.LogError("failed to get terrian size");

			return heights;
		}
		public static NativeArray<float> HeightsToFloatNativeArray(TerrainData terrainData)
		{
			NativeArray<float> heights = new NativeArray<float>(0, Allocator.Temp);
			if (terrainData.size != Vector3.zero)
			{
				heights = new NativeArray<float>((int)(terrainData.size.x * terrainData.size.z), Allocator.Temp);
				float[,] a = terrainData.GetHeights(0, 0, (int)terrainData.size.x, (int)terrainData.size.z);
				for (int i = 0; i < (int)terrainData.size.x; i++)
					for (int j = 0; j < (int)terrainData.size.z; j++)
						heights[j + (i * (int)terrainData.size.x)] = a[i, j];
			}
			else Debug.LogError("failed to get terrain size");
			return heights;
		}

		public static void GenerateTerrainEntityV3(EntityManager entityManager, Entity e, Mesh mesh, TerrainData terrainData)
		{
			//setup terrain data
			NativeArray<float> heights = HeightsToFloatNativeArray(terrainData);
			int2 size = new int2((int)terrainData.size.x, (int)terrainData.size.z);
			float3 scale = terrainData.heightmapScale;
			if (heights.Length == 0)
			{
				Debug.LogError("GenerateTerrainV3: Failed to get the heights using the given Terrain Data!");
				heights.Dispose();
			}
			else
			{
				Debug.Log("GenerateTerrainV3: Successfully Generated Terrrian Collider!");
				PhysicsCollider pc = new PhysicsCollider
				{
					Value = Unity.Physics.TerrainCollider.Create(heights, size, scale, Method,
					new CollisionFilter
					{
						BelongsTo = TriggerEventClass.Floor | TriggerEventClass.Collidable,
						CollidesWith = TriggerEventClass.Pokemon | TriggerEventClass.Player | TriggerEventClass.NPC | TriggerEventClass.Collidable,
						GroupIndex = 0
					})
				};
				if (entityManager.HasComponent<PhysicsCollider>(e))
					entityManager.SetComponentData(e, pc);
				else entityManager.AddComponentData(e, pc);
			}
		}

		public static Entity GenerateTerrainEntity(EntityManager entityManager, UnityEngine.Material mat, Mesh mesh, Translation position, Rotation rotation, TerrainData terrainData)
		{
			if (!terrainArchtypeSet)
			{
				TerrainArchtype = entityManager.CreateArchetype(
					typeof(Translation),
					typeof(Rotation),
					typeof(Scale),
					typeof(RenderBounds),
					typeof(LocalToWorld),
					typeof(PhysicsCollider),
					typeof(CoreData)
					);
			}
			Entity e = entityManager.CreateEntity(TerrainArchtype);
			entityManager.SetComponentData(e, position);
			entityManager.SetComponentData(e, rotation);
			entityManager.SetComponentData(e, new Scale { Value = 1f });
			entityManager.AddSharedComponentData(e, new RenderMesh
			{
				material = mat,
				mesh = mesh,
				castShadows = UnityEngine.Rendering.ShadowCastingMode.On
			});

			//setup terrain data
			NativeArray<float> heights = HeightsToFloatNativeArray(terrainData);
			int2 size = new int2((int)terrainData.size.x, (int)terrainData.size.z);
			float3 scale = terrainData.heightmapScale;
			if (heights.Length == 0)
			{
				Debug.LogError("Failed to get the heights using the given Terrain Data!");
				heights.Dispose();
			}
			else
			{
				entityManager.SetComponentData(e, new PhysicsCollider
				{
					Value = Unity.Physics.TerrainCollider.Create(heights, size, scale, Method,
					new CollisionFilter
					{
						BelongsTo = TriggerEventClass.Floor,
						CollidesWith = TriggerEventClass.Pokemon | TriggerEventClass.Player | TriggerEventClass.NPC | TriggerEventClass.Collidable,
						GroupIndex = 0
					})
				});
			}
			//	entityManager.SetComponentData(e, new CoreData(CoreFunctionsClass.StringToByteString30("TerrainA"),CoreFunctionsClass.StringToByteString30("Terrain"),scale,scale,e,position,rotation,new PhysicsDamping { }));
			return e;
		}

		public static Entity generateTerrainEntity(EntityManager entityManager, UnityEngine.Material mat, Mesh mesh, TerrainData terrainData)
		{
			//	float* heights = GenerateMeshHeights(mesh);
			//	float* heights = HeightsToFloatArray(terrainData);
			NativeArray<float> heights = HeightsToFloatNativeArray(terrainData);
			int2 size = new int2((int)terrainData.size.x, (int)terrainData.size.z);

			float3 scale = terrainData.heightmapScale;
			Entity staticEntity = new Entity { };
			if (heights.Length == 0)
			{
				Debug.LogError("Invalid height was given");
				//	UnsafeUtility.Free(heights, Allocator.Temp);
				heights.Dispose();
				return staticEntity;
			}
			else
			{
				Debug.Log("Creating terrain...");
				/*	BlobAssetReference<Unity.Physics.Collider> collider = Unity.Physics.TerrainCollider.Create(heights,size, scale, Method,
						new CollisionFilter {
							BelongsTo = TriggerEventClass.Floor,
							CollidesWith = TriggerEventClass.Pokemon | TriggerEventClass.Player | TriggerEventClass.NPC | TriggerEventClass.Collidable,
							GroupIndex = 0
						}
				);
					float3 position = new float3(size.x - 1, 0f, size.y - 1) * scale * -0.5f;

				//	staticEntity = Bodies.Bodies.CreateStaticBodyWithMesh(entityManager, mat,mesh, position, quaternion.identity, collider);*/
				staticEntity = Bodies.Bodies.CreateTerrain(entityManager, heights, new int2((int)terrainData.size.x, (int)terrainData.size.z),
					(float3)terrainData.heightmapScale, mat, false, Unity.Physics.TerrainCollider.CollisionMethod.VertexSamples);


			}

			//	UnsafeUtility.Free(heights, Allocator.Temp);
			heights.Dispose();
			return staticEntity;
		}
	}

}
