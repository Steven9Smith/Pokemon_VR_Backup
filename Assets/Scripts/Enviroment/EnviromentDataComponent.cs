using Core.Enviroment;
using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;

namespace Core
{
	[RequiresEntityConversion]
	public class EnviromentDataComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		public string EnviromentName = "Forest";
		public string Region = "Kanto";

		public new Transform transform;
		public float3 bounds;
		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			if (transform == null) Debug.LogError("given trans is null or invalid!");
			CoreData cd = new CoreData(new ByteString30(EnviromentName), new ByteString30(Region),bounds,transform.localScale);
			if (!cd.isValid) Debug.LogError("failed to successfully create COreData for To-Be Entity \"" + EnviromentName + "\":\"" + Region + "\"");
			else
			{
				dstManager.SetName(entity, EnviromentName +"|"+Region );
				dstManager.AddComponentData(entity, new EnviromentNotSet { });
				dstManager.AddComponentData(entity, new EnviromentEntityData(cd));
				dstManager.AddComponentData(entity, cd);
			}
		}
	//	public IEnumerator Co()
	//	{
	//		Debug.Log("Waiting 1 second");
	//		yield return new WaitForSeconds(1);
	//	}
	}
	//holds data for a particular enviroment
	[Serializable]
	public struct EnviromentData : IComponentData
	{
		public ByteString30 entityName;
		public int entityId;
		public int entityParentId;
		public ByteString30 pathString;
		public BlittableBool isValid;
		public EnviromentData(string EntityName,int EntityId,int EntityParentId,string EnviromentPath)
		{
			entityName = new ByteString30(EntityName);
			entityId = EntityId;
			entityParentId = EntityParentId;
			pathString = new ByteString30(EnviromentPath);
			isValid = true;
		}
		public EnviromentData(ByteString30 EntityName,int EntityId,int EntityParentId,ByteString30 EnviromentPath)
		{
			entityName = EntityName;
			entityId = EntityId;
			entityParentId = EntityParentId;
			pathString = EnviromentPath;
			isValid = true;
		}
	}
	
	[Serializable]
	public struct EnviromentEntityData : IComponentData
	{
		public CoreData coreData;
		public BlittableBool isValid;
		public EnviromentEntityData(CoreData cd)
		{
			coreData = cd;
			isValid = true;
		}
	}


	public struct EnviromentNotSet : IComponentData { };
	public class EnviromentSystem : JobComponentSystem
	{
		private EntityQuery enviromentQuery;

		protected override void OnCreateManager()
		{
			enviromentQuery = GetEntityQuery(typeof(CoreData),typeof(EnviromentEntityData),typeof(Translation),typeof(EnviromentNotSet));
		}
		protected override void OnStartRunning()
		{
			NativeArray<CoreData> cds = enviromentQuery.ToComponentDataArray<CoreData>(Allocator.TempJob);
			NativeArray<EnviromentEntityData> eds = enviromentQuery.ToComponentDataArray<EnviromentEntityData>(Allocator.TempJob);
			NativeArray<Entity> entities = enviromentQuery.ToEntityArray(Allocator.TempJob);
			NativeArray<Translation> ts = enviromentQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
			for (int i = 0; i < entities.Length; i++)
			{
				if (eds[i].isValid)
				{
				//	EntityManager.SetComponentData(entities[i], new EnviromentData(cds[i].Name, entities[i].Index, -1, cds[i].BaseName));
					EnviromentDataClass.GenerateEnviroment(cds[i].BaseName.ToString(), cds[i].Name.ToString(), EntityManager
							, cds[i].size, ts[i].Value, quaternion.identity, cds[i].scale);
				}
				else Debug.LogWarning("Detected an enviroment that is not valid \"" + EntityManager.GetName(entities[i]) + "\"");
				EntityManager.RemoveComponent<EnviromentNotSet>(entities[i]);
			}
			entities.Dispose();
			cds.Dispose();
			eds.Dispose();
			ts.Dispose();
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			return inputDeps;
		}
	}
}
