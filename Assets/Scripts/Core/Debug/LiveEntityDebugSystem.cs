using Pokemon;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Core
{
	namespace _Debug
	{
		

		public class LiveEntityDebugSystem : JobComponentSystem
		{
			private EntityQuery CoreDataQuery;
			private GameObject EntityDebug;
			private LiveEntityDebugComponent DebugComponent;
			private Entity desiredEntity;

			protected override void OnCreateManager()
			{
				CoreDataQuery = GetEntityQuery(typeof(CoreData));
				GameObject a = Resources.Load("Core/Debug/LiveEntityDebugging") as GameObject;
				EntityDebug = GameObject.Instantiate(a);
				if (EntityDebug != null)
				{
					GameObject[] debugs = CoreFunctionsClass.FindGameObjectsWithLayer(8);
					if (debugs != null)
					{
						GameObject parent = CoreFunctionsClass.FindGameObjectWithName(debugs, "GameSettings");
						if (parent != null) EntityDebug.transform.SetParent(parent.transform);
						else Debug.LogWarning("Failed to set parent of the LiveENtityComponent, failed to find GameSettings thing");
					}
					else Debug.LogWarning("Failed to set parent of the LiveEntityComponent");
					DebugComponent = EntityDebug.GetComponent<LiveEntityDebugComponent>();
				}
				else Debug.LogError("EntityDenig is invalid!");
			}
			protected override JobHandle OnUpdate(JobHandle inputDeps)
			{
				//handle the gameobject display and changes
				if (EntityDebug != null)
				{
					if (DebugComponent != null)
					{
						if (DebugComponent.active)
						{
							if (desiredEntity.Equals(new Entity { }) || EntityManager.GetName(desiredEntity) != DebugComponent.EntityName)
							{
								if (DebugComponent.EntityName == "")
								{
									Debug.LogWarning("Cannot display or change entity with a blank Entity name");
								}
								else
								{
									if (CoreDataQuery.CalculateEntityCount() > 0)
									{
										string[] a = DebugComponent.EntityName.Split(':');
										if (!CoreFunctionsClass.FindEntity(EntityManager, ref desiredEntity, a[0], a.Length > 1 ? a[1] : ""))
											Debug.LogWarning("Failed to find Entity that matches \"" + DebugComponent.EntityName + "\"");
									}
									else Debug.LogError("Cannot use LiveEntityDebugging with no Entity with a CoreData Component");
								}
							}
							else
							{
								DebugComponent.Display.Update(EntityManager,desiredEntity);
							}
							//Handle Global Settings
							if (DebugComponent.GlobalSettings.applyChange)
							{
								DebugComponent.GlobalSettings.ApplyChanges();
								DebugComponent.GlobalSettings.Update();
								DebugComponent.GlobalSettings.applyChange = false;
								DebugComponent.GlobalSettings.editMode = false;
							}
							else if (!DebugComponent.GlobalSettings.IsUpdated())
								DebugComponent.GlobalSettings.Update();
						}
					}
				}
				return inputDeps;
			}
		}
	}
}
