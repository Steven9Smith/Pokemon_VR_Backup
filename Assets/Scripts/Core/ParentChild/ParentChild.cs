using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace Core
{
	namespace ParentChild
	{
		/// <summary>
		/// This holds an Entity that represents a Entity's child entity
		/// </summary>
		public struct EntityChild : IComponentData
		{
			public Entity entity;
			public BlittableBool isValid;
			//maybe add offset
		}
		/// <summary>
		/// This holds an Entity that represents a Entity's parent entity
		/// </summary>
		public struct EntityParent : IComponentData
		{
			public Entity entity;
			public BlittableBool isValid;
		}
		/// <summary>
		/// moves the child with the parent
		/// </summary>
		public class ParentChildSystem : JobComponentSystem
		{
			//dem queries
			private EntityQuery parents;
			private EntityQuery childs;
			protected override void OnCreate()
			{
				parents = GetEntityQuery(typeof(EntityChild));
				childs = GetEntityQuery(typeof(EntityParent));
			}
			protected override JobHandle OnUpdate(JobHandle inputDeps)
			{
				/*Note I made EntityParent and Entity Child to support backward Entity interation.
				 * if your having trouble understanding then think of it like a LinkedList.
				 * so you can iterate childern by getting the EntityChild and vise vera by getting the EntityParent.
				 * BUT BE WARNED! EntityParent and EntityChild are set PER UPDATE. So if you give a parent EntityChild and
				 * a child an EntityParent then the child's Translation will be set TWICE. USE WITH CAUTION
				 * (maybe update EntityParent and EntityChild with a bool that checks whether or not to update Translation)
				 */
				if (parents.CalculateEntityCount() > 0)
				{
					Debug.Log("Settiing Parent to Child");
					NativeArray<EntityChild> entityChildren = parents.ToComponentDataArray<EntityChild>(Allocator.TempJob);
					NativeArray<Entity> entities = parents.ToEntityArray(Allocator.TempJob);
					for (int i = 0; i < entityChildren.Length; i++)
					{
						if (entityChildren[i].isValid)
							if(EntityManager.Exists(entityChildren[i].entity))
								EntityManager.SetComponentData(entities[i], EntityManager.GetComponentData<Translation>(entityChildren[i].entity));
					}
					entityChildren.Dispose();
					entities.Dispose();
				}
				if (childs.CalculateEntityCount() > 0)
				{
					NativeArray<EntityParent> entityParents = childs.ToComponentDataArray<EntityParent>(Allocator.TempJob);
					NativeArray<Entity> entities = childs.ToEntityArray(Allocator.TempJob);
					for (int i = 0; i < entityParents.Length; i++)
					{
						if (entityParents[i].isValid)
							if (EntityManager.Exists(entityParents[i].entity))
								EntityManager.SetComponentData(entities[i], EntityManager.GetComponentData<Translation>(entityParents[i].entity));
					}
					entityParents.Dispose();
					entities.Dispose();
				}

				return inputDeps;
			}
		}
	}
}
