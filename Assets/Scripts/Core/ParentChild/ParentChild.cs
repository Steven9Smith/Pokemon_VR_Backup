using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Core
{
	namespace ParentChild
	{
		public struct EntityChild : IComponentData
		{
			public Entity entity;
			public BlittableBool isValid;
			//maybe add offset
		}
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
			private EntityQuery parents;
			private EntityQuery childs;
			protected override void OnCreate()
			{
				parents = GetEntityQuery(typeof(EntityChild));
				childs = GetEntityQuery(typeof(EntityParent));
			}
			protected override JobHandle OnUpdate(JobHandle inputDeps)
			{
				if (parents.CalculateEntityCount() > 0)
				{
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
