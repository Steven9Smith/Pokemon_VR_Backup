using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using System;
using Unity.Rendering;

namespace Core.Bodies
{

	public class Bodies
	{
		//
		// Object creation
		//
		public static Entity CreateBody(EntityManager entityManager, UnityEngine.Material mat,float3 position, quaternion orientation, BlobAssetReference<Unity.Physics.Collider> collider,
			float3 linearVelocity, float3 angularVelocity, float mass, bool isDynamic)
		{
			Entity entity = entityManager.CreateEntity(new ComponentType[] { });

			entityManager.AddComponentData(entity, new LocalToWorld { });
			entityManager.AddComponentData(entity, new Translation { Value = position });
			entityManager.AddComponentData(entity, new Rotation { Value = orientation });

			var colliderComponent = new PhysicsCollider { Value = collider };
			entityManager.AddComponentData(entity, colliderComponent);

			UnityEngine.Mesh mesh = new UnityEngine.Mesh();
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
		public static Entity CreateBodyWithMesh(EntityManager entityManager, UnityEngine.Material mat,UnityEngine.Mesh mesh,float3 position, quaternion orientation, BlobAssetReference<Unity.Physics.Collider> collider,
			float3 linearVelocity, float3 angularVelocity, float mass, bool isDynamic)
		{
			Entity entity = entityManager.CreateEntity(new ComponentType[] { });

			entityManager.AddComponentData(entity, new LocalToWorld { });
			entityManager.AddComponentData(entity, new Translation { Value = position });
			entityManager.AddComponentData(entity, new Rotation { Value = orientation });

			var colliderComponent = new PhysicsCollider { Value = collider };
			entityManager.AddComponentData(entity, colliderComponent);

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
		
		public static Entity CreateStaticBody(EntityManager entityManager,UnityEngine.Material mat,float3 position, quaternion orientation, BlobAssetReference<Unity.Physics.Collider> collider)
		{
			return CreateBody(entityManager,mat,position, orientation, collider, float3.zero, float3.zero, 0.0f, false);
		}
		public static Entity CreateStaticBodyWithMesh(EntityManager entityManager,UnityEngine.Material mat,UnityEngine.Mesh mesh,float3 position, quaternion orientation, BlobAssetReference<Unity.Physics.Collider> collider)
		{
			return CreateBodyWithMesh(entityManager,mat,mesh,position, orientation, collider, float3.zero, float3.zero, 0.0f, false);
		}

		public static Entity CreateDynamicBody(EntityManager entityManager,UnityEngine.Material mat,float3 position, quaternion orientation, BlobAssetReference<Unity.Physics.Collider> collider,
			float3 linearVelocity, float3 angularVelocity, float mass)
		{
			return CreateBody(entityManager,mat,position, orientation, collider, linearVelocity, angularVelocity, mass, true);
		}

		public static unsafe Entity CreateJoint(BlobAssetReference<JointData> jointData, Entity entityA, Entity entityB, bool enableCollision = false)
		{
			EntityManager entityManager = World.Active.EntityManager;
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

		//////////////////////////////////////
		


	}
	
	
}