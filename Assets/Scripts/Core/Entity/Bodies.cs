using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using System;
using Unity.Rendering;
using Unity.Collections;


using Collider = Unity.Physics.Collider;
using Material = UnityEngine.Material;
using Mesh = UnityEngine.Mesh;
using UnityEngine;
using System.Reflection;

namespace Core.Bodies
{

	public class Bodies
	{

		public static World DefaultWorld => World.DefaultGameObjectInjectionWorld;

		//
		// Object creation
		//
/*		public static unsafe Entity CreateBody(EntityManager entityManager,RenderMesh renderMesh, float3 position, quaternion orientation,
		BlobAssetReference<Collider> collider,
		float3 linearVelocity, float3 angularVelocity, float mass, bool isDynamic)
		{
			ComponentType[] componentTypes = new ComponentType[isDynamic ? 7 : 4];

			componentTypes[0] = typeof(RenderMesh);
			componentTypes[1] = typeof(Translation);
			componentTypes[2] = typeof(Rotation);
			componentTypes[3] = typeof(PhysicsCollider);
			if (isDynamic)
			{
				componentTypes[4] = typeof(PhysicsVelocity);
				componentTypes[5] = typeof(PhysicsMass);
				componentTypes[6] = typeof(PhysicsDamping);
			}
			Entity entity = entityManager.CreateEntity(componentTypes);

			entityManager.SetSharedComponentData(entity, renderMesh);

			// Check if the Entity has a Translation and Rotation components and execute the appropriate method
			if (entityManager.HasComponent(entity, typeof(Translation)))
			{
				entityManager.SetComponentData(entity, new Translation { Value = position });
		
	   }
			else
			{
				entityManager.AddComponentData(entity, new Translation { Value = position });
			}
			if (entityManager.HasComponent(entity, typeof(Rotation)))
			{
				entityManager.SetComponentData(entity, new Rotation { Value = orientation });
			}
			else
			{
				entityManager.AddComponentData(entity, new Rotation { Value = orientation });
			}
			entityManager.SetComponentData(entity, new PhysicsCollider { Value = collider });

			if (isDynamic)
			{
				Unity.Physics.Collider* colliderPtr = (Unity.Physics.Collider*)collider.GetUnsafePtr();
				entityManager.SetComponentData(entity, PhysicsMass.CreateDynamic(colliderPtr->MassProperties, mass));
				// Calculate the angular velocity in local space from rotation and world angular velocity
				float3 angularVelocityLocal = math.mul(math.inverse(colliderPtr->MassProperties.MassDistribution.Transform.rot), angularVelocity);
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
*/
		

		/*	public static Entity CreateBodyWithMesh(EntityManager entityManager, UnityEngine.Material mat,UnityEngine.Mesh mesh,float3 position, quaternion orientation, BlobAssetReference<Unity.Physics.Collider> collider,
				float3 linearVelocity, float3 angularVelocity, float mass, bool isDynamic)
			{
				Entity entity = entityManager.CreateEntity(new ComponentType[] { });

				entityManager.AddComponentData(entity, new LocalToWorld { });
				entityManager.AddComponentData(entity, new Translation { Value = position });
				entityManager.AddComponentData(entity, new Rotation { Value = orientation });




	/*
	#pragma warning disable 618
				List<Unity.Physics.Authoring.DisplayBodyColliders.DrawComponent.DisplayResult> meshes;
				unsafe { meshes = Unity.Physics.Authoring.DisplayBodyColliders.DrawComponent.BuildDebugDisplayMesh(colliderComponent.ColliderPtr); }
	#pragma warning restore 618
				CombineInstance[] instances = new CombineInstance[meshes.Count];
				int numVertices = 0;
				for (int i = 0; i < meshes.Count; i++)
				{
					instances[i] = new CombineInstance
					{
						mesh = meshes[i].Mesh,
						transform = Matrix4x4.TRS(meshes[i].Position, meshes[i].Orientation, meshes[i].Scale)
					};
					numVertices += meshes[i].Mesh.vertexCount;
				}
				mesh.indexFormat = numVertices > UInt16.MaxValue ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
				mesh.CombineMeshes(instances);

				entityManager.AddSharedComponentData(entity, new RenderMesh
				{
					mesh = mesh,
					material = mat
				});

				if (isDynamic)
				{
					entityManager.AddComponentData(entity, PhysicsMass.CreateDynamic(colliderComponent.MassProperties, mass));

					float3 angularVelocityLocal = math.mul(math.inverse(colliderComponent.MassProperties.MassDistribution.Transform.rot), angularVelocity);
					entityManager.AddComponentData(entity, new PhysicsVelocity()
					{
						Linear = linearVelocity,
						Angular = angularVelocityLocal
					});
					entityManager.AddComponentData(entity, new PhysicsDamping()
					{
						Linear = 0.01f,
						Angular = 0.05f
					});
				}

				return entity;
			}

			public static Entity CreateStaticBody(EntityManager entityManager,RenderMesh renderMesh,float3 position, quaternion orientation, BlobAssetReference<Unity.Physics.Collider> collider)
			{
				return CreateBody(entityManager,renderMesh,position, orientation, collider, float3.zero, float3.zero, 0.0f, false);
			}
			public static Entity CreateStaticBodyWithMesh(EntityManager entityManager,UnityEngine.Material mat,UnityEngine.Mesh mesh,float3 position, quaternion orientation, BlobAssetReference<Unity.Physics.Collider> collider)
			{
				return CreateBodyWithMesh(entityManager,mat,mesh,position, orientation, collider, float3.zero, float3.zero, 0.0f, false);
			}

			public static Entity CreateDynamicBody(EntityManager entityManager, RenderMesh renderMesh, float3 position, quaternion orientation, BlobAssetReference<Unity.Physics.Collider> collider,
				float3 linearVelocity, float3 angularVelocity, float mass)
			{
				return CreateBody(entityManager,renderMesh,position, orientation, collider, linearVelocity, angularVelocity, mass, true);
			}*/

		//////////////////////////////////////

		// TODO: add proper utility APIs for converting Collider into buffers usable for UnityEngine.Mesh and for drawing lines
		static readonly Type k_DrawComponent = typeof(Unity.Physics.Authoring.DisplayBodyColliders)
			.GetNestedType("DrawComponent", BindingFlags.NonPublic);

		static readonly MethodInfo k_DrawComponent_BuildDebugDisplayMesh = k_DrawComponent
			.GetMethod("BuildDebugDisplayMesh", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(BlobAssetReference<Collider>) }, null);

		static readonly Type k_DisplayResult = k_DrawComponent.GetNestedType("DisplayResult");

		static readonly FieldInfo k_DisplayResultsMesh = k_DisplayResult.GetField("Mesh");
		static readonly PropertyInfo k_DisplayResultsTransform = k_DisplayResult.GetProperty("Transform");

		static BlobAssetReference<Unity.Physics.Collider> CreateMeshTerrain(NativeArray<float> heights, int2 size, float3 scale)
		{
			var vertices = new NativeList<float3>(Allocator.Temp);
			var triangles = new NativeList<int3>(Allocator.Temp);
			var vertexIndex = 0;
			for (int i = 0; i < size.x - 1; i++)
				for (int j = 0; j < size.y - 1; j++)
				{
					int i0 = i;
					int i1 = i + 1;
					int j0 = j;
					int j1 = j + 1;
					float3 v0 = new float3(i0, heights[i0 + size.x * j0], j0) * scale;
					float3 v1 = new float3(i1, heights[i1 + size.x * j0], j0) * scale;
					float3 v2 = new float3(i0, heights[i0 + size.x * j1], j1) * scale;
					float3 v3 = new float3(i1, heights[i1 + size.x * j1], j1) * scale;

					vertices.Add(v1);
					vertices.Add(v0);
					vertices.Add(v2);
					vertices.Add(v1);
					vertices.Add(v2);
					vertices.Add(v3);

					triangles.Add(new int3(vertexIndex++, vertexIndex++, vertexIndex++));
					triangles.Add(new int3(vertexIndex++, vertexIndex++, vertexIndex++));
				}

			return Unity.Physics.MeshCollider.Create(vertices, triangles);
		}

		static Entity CreateBody(float3 position, quaternion orientation, BlobAssetReference<Collider> collider,
		float3 linearVelocity, float3 angularVelocity, float mass, bool isDynamic, UnityEngine.Material material)
		{
			var entityManager = DefaultWorld.EntityManager;

			Entity entity = entityManager.CreateEntity(new ComponentType[] { });

			entityManager.AddComponentData(entity, new LocalToWorld { });
			entityManager.AddComponentData(entity, new Translation { Value = position });
			entityManager.AddComponentData(entity, new Rotation { Value = orientation });

			var colliderComponent = new PhysicsCollider { Value = collider };
			entityManager.AddComponentData(entity, colliderComponent);

			var mesh = new Mesh();
			var instances = new List<CombineInstance>(8);
			var numVertices = 0;
			foreach (var displayResult in (IEnumerable)k_DrawComponent_BuildDebugDisplayMesh.Invoke(null, new object[] { collider }))
			{
				var instance = new CombineInstance
				{
					mesh = k_DisplayResultsMesh.GetValue(displayResult) as Mesh,
					transform = (float4x4)k_DisplayResultsTransform.GetValue(displayResult)
				};
				instances.Add(instance);
				numVertices += mesh.vertexCount;
			}
			mesh.indexFormat = numVertices > UInt16.MaxValue ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
			mesh.CombineMeshes(instances.ToArray());

			entityManager.AddSharedComponentData(entity, new RenderMesh
			{
				mesh = mesh,
				material = material
			});

			if (isDynamic)
			{
				entityManager.AddComponentData(entity, PhysicsMass.CreateDynamic(colliderComponent.MassProperties, mass));

				float3 angularVelocityLocal = math.mul(math.inverse(colliderComponent.MassProperties.MassDistribution.Transform.rot), angularVelocity);
				entityManager.AddComponentData(entity, new PhysicsVelocity()
				{
					Linear = linearVelocity,
					Angular = angularVelocityLocal
				});
				entityManager.AddComponentData(entity, new PhysicsDamping()
				{
					Linear = 0.01f,
					Angular = 0.05f
				});
			}

			return entity;
		}

		protected static Entity CreateStaticBody(float3 position, quaternion orientation, BlobAssetReference<Collider> collider,UnityEngine.Material material)
		{
			return CreateBody(position, orientation, collider, float3.zero, float3.zero, 0.0f, false,material);
		}

		protected static Entity CreateDynamicBody(float3 position, quaternion orientation, BlobAssetReference<Collider> collider,
			float3 linearVelocity, float3 angularVelocity, float mass,UnityEngine.Material material)
		{
			return CreateBody(position, orientation, collider, linearVelocity, angularVelocity, mass, true,material);
		}

		protected unsafe Entity CreateJoint(BlobAssetReference<JointData> jointData, Entity entityA, Entity entityB, bool enableCollision = false)
		{
			var entityManager = DefaultWorld.EntityManager;
			ComponentType[] componentTypes = new ComponentType[1];
			componentTypes[0] = typeof(PhysicsJoint);
			Entity jointEntity = entityManager.CreateEntity(componentTypes);
			entityManager.SetComponentData(jointEntity, new PhysicsJoint
			{
				JointData = jointData,
				EntityA = entityA,
				EntityB = entityB,
				EnableCollision = (enableCollision ? 1 : 0)
			});
			return jointEntity;
		}

		public static Entity CreateTerrain(EntityManager entityManager, NativeArray<float> heights,
			int2 size, float3 scale, UnityEngine.Material material, bool createMesh = true,
			Unity.Physics.TerrainCollider.CollisionMethod method = Unity.Physics.TerrainCollider.CollisionMethod.VertexSamples)
		{
			var collider = createMesh
				? CreateMeshTerrain(heights, new int2((int)scale.x, (int)scale.z), scale)
				: Unity.Physics.TerrainCollider.Create(heights, size, scale, method);

			bool compound = false;
			if (compound)
			{
				var instances = new NativeArray<CompoundCollider.ColliderBlobInstance>(4, Allocator.Temp);
				for (int i = 0; i < 4; i++)
				{
					instances[i] = new CompoundCollider.ColliderBlobInstance
					{
						Collider = collider,
						CompoundFromChild = new RigidTransform
						{
							pos = new float3((i % 2) * scale.x * (size.x - 1), 0.0f, (i / 2) * scale.z * (size.y - 1)),
							rot = quaternion.identity
						}
					};
				}
				collider = Unity.Physics.CompoundCollider.Create(instances);
				instances.Dispose();
			}

			float3 position = new float3(size.x - 1, 0.0f, size.y - 1) * scale * -0.5f;
			return CreateStaticBody(position, quaternion.identity, collider,material);
		}

	}


}