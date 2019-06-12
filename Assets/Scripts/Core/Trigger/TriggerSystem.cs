using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

namespace Pokemon
{
	public class ATriggerEventSystem : JobComponentSystem
	{
		private int i;
		private NativeArray<ATriggerEvent> oldAtes;
		private const int oldAtesBase = 10;   //changeable
		private bool match = false, alreadyCleared = false;

		private int localAtesRealLength = oldAtesBase;

		private BuildPhysicsWorld buildPhysicsWorld;
		private StepPhysicsWorld stepPhysicsWorld;
		private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

		protected override void OnCreateManager()
		{
			oldAtes = new NativeArray<ATriggerEvent>(oldAtesBase, Allocator.Persistent);
			buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
			stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
			endSimulationEntityCommandBufferSystem = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

		}
		protected override void OnDestroy(){oldAtes.Dispose();}
		public struct TriggerEventsJob : ITriggerEventsJob
		{
			public EntityCommandBuffer ecb;
			public ComponentDataFromEntity<ATriggerEvent> hasATriggerEvent;
			public NativeArray<ATriggerEvent> localAtes;
			public int counter;
			public int currentLength;
			public void Execute(TriggerEvent triggerEvent)
			{
				if (counter >= currentLength) {
					NativeArray<ATriggerEvent> _localAtes = new NativeArray<ATriggerEvent>(localAtes, Allocator.TempJob);
					localAtes = new NativeArray<ATriggerEvent>(currentLength + oldAtesBase, Allocator.TempJob);
					for(int i = 0; i < currentLength; i++)
						localAtes[i] = _localAtes[i];
					currentLength += oldAtesBase;
				}
				if (!hasATriggerEvent.Exists(triggerEvent.Entities.EntityA))
				{
					localAtes[counter] = new ATriggerEvent
					{
						entityA = triggerEvent.Entities.EntityA,
						entityB = triggerEvent.Entities.EntityB,
						valid = true
					};
				}
				counter++;
			}
		}
		public struct UpdateOldAtesJob : IJob
		{
			public NativeArray<ATriggerEvent> oldAtes;
			public NativeArray<ATriggerEvent> localAtes;
			public EntityCommandBuffer ecb;
			private int i,j;
			private bool match;
			public void Execute()
			{
				match = false;
				//remove oldTriggers if not in new triggers
				for (i = 0; i < oldAtes.Length; i++)
				{
					//			Debug.Log("i = "+i);
					if (oldAtes[i].valid)
					{
						for (j = 0; j < localAtes.Length; j++)
						{
							if (oldAtes[i] == localAtes[j])
							{
								match = true;
								break;
							}
						}
						if (!match)
						{
							ecb.RemoveComponent<ATriggerEvent>(oldAtes[i].entityA);
							switch (oldAtes[i].triggerType)
							{
								case TriggerEventClass.Damage:
									Debug.Log("Removing damage trigger on B");
									ecb.RemoveComponent<DamageTrigger>(oldAtes[i].entityB);
									break;
								case TriggerEventClass.Interaction:
									//since interaction trigger is set the entityB of the trigger
									Debug.Log("Removing Interaction trigger on B");
									ecb.RemoveComponent<InteractionTrigger>(oldAtes[i].entityB);
									break;
								default: break;
							}
							oldAtes[i] = new ATriggerEvent { };
						}
						match = false;
					}
					else break;
				}
			}
		}
		public ATriggerEvent processTriggers(ATriggerEvent ate)
		{
			PhysicsCollider physicsCollider = EntityManager.GetComponentData<PhysicsCollider>(ate.entityA);

			switch (physicsCollider.Value.Value.Filter.BelongsTo)
			{
				case TriggerEventClass.Damage:
					if (!EntityManager.HasComponent<DamageTrigger>(ate.entityB))
					{
						Debug.Log("Adding DamageTrigger");
						EntityManager.AddComponentData<DamageTrigger>(ate.entityB,
							new DamageTrigger { Value = EntityManager.GetComponentData<DamageTriggerData>(ate.entityA).Value });
					}
		//			else Debug.Log("The Entity already has a DamageTrigger");
					return new ATriggerEvent
					{
						entityA = ate.entityA,
						entityB = ate.entityB,
						valid = true,
						triggerType = TriggerEventClass.Damage
					};
				case TriggerEventClass.Interaction:
					if (!EntityManager.HasComponent<InteractionTrigger>(ate.entityB))
					{
						Debug.Log("Adding InteractionTrigger");
						EntityManager.AddComponentData<InteractionTrigger>(ate.entityB, new InteractionTrigger { });
						
					}
		//			else Debug.Log("The Entity already has a InteractionTrigger");
					return new ATriggerEvent
					{
						entityA = ate.entityA,
						entityB = ate.entityB,
						valid = true,
						triggerType = TriggerEventClass.Interaction
					};
				default:
	//				Debug.Log("Detected an Unknown TriggerType " + ate.ToString() +
	//					" ABElongsTO = " + physicsCollider.Value.Value.Filter.BelongsTo + ":" + physicsCollider.Value.Value.Filter.CollidesWith);
					break;
			}
			return ate;
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			if (stepPhysicsWorld.Simulation.Type == SimulationType.NoPhysics) return inputDeps;

			NativeArray<ATriggerEvent> localAtes = new NativeArray<ATriggerEvent>(oldAtes.Length,Allocator.TempJob);

			JobHandle jobHandle = new TriggerEventsJob
			{
				ecb = endSimulationEntityCommandBufferSystem.CreateCommandBuffer(),
				hasATriggerEvent = GetComponentDataFromEntity<ATriggerEvent>(),
				counter = 0,
				currentLength = localAtesRealLength,
				localAtes = localAtes
			}.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, inputDeps);
			//must wait for this to finish to continue
			jobHandle.Complete();
			//create/add new trigger events
			for (i = 0; i < localAtes.Length; i++)
			{
				if (localAtes[i].valid)
				{
					match = false;
					localAtes[i] = processTriggers(localAtes[i]);
					//flip the entities
					if (localAtes[i].triggerType == 0)
						localAtes[i] = processTriggers(new ATriggerEvent
					{
							entityA = localAtes[i].entityB,
							entityB = localAtes[i].entityA,
							triggerType = localAtes[i].triggerType,
							valid = localAtes[i].valid
					});
					if (localAtes[i].triggerType == 0) Debug.Log("Unable to update determine triggerType "+localAtes[i].ToString());
				}
				else break;
			}
			//if there are no new trigger events

			if (i == 0)
			{
				if (!alreadyCleared){
		//			Debug.Log("Clearing STUFF");
					//clear oldAtes
					for (i = 0; i < oldAtes.Length; i++)
					{
		//				Debug.Log("checking "+i+" of "+oldAtes.Length);
						if (oldAtes[i].valid)
						{
							Debug.Log("Attempting to remove..."+oldAtes[i].ToString());
							switch (oldAtes[i].triggerType)
							{
								case TriggerEventClass.Damage:
									Debug.Log("Removing damage trigger on B");
									EntityManager.RemoveComponent<DamageTrigger>(oldAtes[i].entityB);
									break;
								case TriggerEventClass.Interaction:
									//since interaction trigger is set the entityB of the trigger
									Debug.Log("Removing Interaction trigger on B");
									EntityManager.RemoveComponent<InteractionTrigger>(oldAtes[i].entityB);
									break;
								default: break;
							}
							EntityManager.RemoveComponent<ATriggerEvent>(oldAtes[i].entityA);
							oldAtes[i] = new ATriggerEvent { };
						}
						else
						{
		//					Debug.Log("Failed to get valid");
							break;
						}
					}
					Debug.Log("Cleared the persistant NativeArray");
					alreadyCleared = true;
				}
				localAtes.Dispose();
				return inputDeps;
			}
			//else

			if (localAtes.Length > oldAtes.Length)
			{
				Debug.Log("Adding new persistance values");
				oldAtes = new NativeArray<ATriggerEvent>(localAtes.Length + oldAtesBase, Allocator.Persistent);
			}
			if (alreadyCleared) alreadyCleared = false;

			/////////////////////////////////////////////////////////////////////////
			//clear non-active trigger events
			JobHandle cleanJobHandle = new UpdateOldAtesJob
			{
				ecb = endSimulationEntityCommandBufferSystem.CreateCommandBuffer(),
				oldAtes = oldAtes,
				localAtes = localAtes
			}.Schedule(inputDeps);
			cleanJobHandle.Complete();

			//update oldAtes content
			for (i = 0; i < localAtes.Length; i++)
				oldAtes[i] = localAtes[i];
			localAtes.Dispose();
			return cleanJobHandle;

		}
	}
}
/*
 * public struct ATriggerJob : IJob
		{
			[DeallocateOnJobCompletion] public NativeArray<PairOColliders> pocs;
			public NativeArray<ATriggerEvent> ates;
			[WriteOnly] public EntityCommandBuffer ecb;
			public void Execute()
			{
				for (int i = 0; i < pocs.Length; i++)
				{
			//		Debug.Log("i ==== " + i);
			//also test for everything
					if (pocs[i].entityACollider.Value.Value.Filter.MaskBits == pocs[i].entityBCollider.Value.Value.Filter.CategoryBits ||
						pocs[i].entityBCollider.Value.Value.Filter.MaskBits == pocs[i].entityACollider.Value.Value.Filter.CategoryBits ||
						pocs[i].entityACollider.Value.Value.Filter.CategoryBits == TriggerEventClass.Everything || 
						pocs[i].entityBCollider.Value.Value.Filter.CategoryBits == TriggerEventClass.Everything)
					{
				//		Debug.Log(" a "+ates.ToString());
						switch (pocs[i].entityACollider.Value.Value.Filter.CategoryBits)
						{
							//		case TriggerEventClass.Floor: ecb.AddComponent<FloorTrigger>(entityA, new FloorTrigger { }); break;
							//		case TriggerEventClass.Player: ecb.AddComponent<PlayerTrigger>(entityA, new PlayerTrigger { }); break;
							//		case TriggerEventClass.Pokemon: ecb.AddComponent<PokemonTrigger>(entityA, new PokemonTrigger { }); break;
							//		case TriggerEventClass.Structure: ecb.AddComponent<StructureTrigger>(entityA, new StructureTrigger { }); break;
							//		case TriggerEventClass.NPC: ecb.AddComponent<NPCTrigger>(pocs[i].entityA, new NPCTrigger { }); break;
							case TriggerEventClass.Damage:
								//since damage is removed after addition we don't need to check to see if it's already there
								ecb.AddComponent<DamageTrigger>(ates[i].entityB, new DamageTrigger { });
								break;
							case TriggerEventClass.Interaction:
								//			Debug.Log("Atemping to Add an Interaction Trigger!");

								if (!pocs[i].aHasComponent)
								{
									ecb.AddComponent<InteractionTrigger>(ates[i].entityB, new InteractionTrigger { });
									ates[i] = new ATriggerEvent { entityA = ates[i].entityA, entityB = ates[i].entityB, valid = true, triggerType = TriggerEventClass.Interaction };
								}
								//		else Debug.Log("Detected interaction already exists");
								break;

							default: break;
						}
						switch (pocs[i].entityBCollider.Value.Value.Filter.CategoryBits)
						{
							//		case TriggerEventClass.Floor: ecb.AddComponent<FloorTrigger>(entityA, new FloorTrigger { }); break;
							//		case TriggerEventClass.Player: ecb.AddComponent<PlayerTrigger>(entityA, new PlayerTrigger { }); break;
							//		case TriggerEventClass.Pokemon: ecb.AddComponent<PokemonTrigger>(entityA, new PokemonTrigger { }); break;
							//		case TriggerEventClass.Structure: ecb.AddComponent<StructureTrigger>(entityA, new StructureTrigger { }); break;
							//		case TriggerEventClass.NPC: ecb.AddComponent<NPCTrigger>(pocs[i].entityA, new NPCTrigger { }); break;
							case TriggerEventClass.Interaction:
								//			Debug.Log("Atemping to Add an Interaction Trigger!");

								if (!pocs[i].bHasComponent)
								{
		//										Debug.Log("Entity A CatBits " + TriggerEventClass.CollisionFilterValueToString(pocs[i].entityACollider.Value.Value.Filter.CategoryBits));
		//										Debug.Log("Entity A MaskBits " +  TriggerEventClass.CollisionFilterValueToString(pocs[i].entityACollider.Value.Value.Filter.MaskBits));
		//										Debug.Log("Entity B CatBits " + TriggerEventClass.CollisionFilterValueToString(pocs[i].entityBCollider.Value.Value.Filter.CategoryBits));
		//										Debug.Log("Entity B MaskBits " +  TriggerEventClass.CollisionFilterValueToString(pocs[i].entityBCollider.Value.Value.Filter.MaskBits));
									ecb.AddComponent<InteractionTrigger>(ates[i].entityA, new InteractionTrigger { });
									ates[i] = new ATriggerEvent { entityA = ates[i].entityB, entityB = ates[i].entityA, valid = true, triggerType = TriggerEventClass.Interaction };
								}
		//									else Debug.Log("Detected interaction already exists");
								break;

							default: break;
						}
					}
			//		else Debug.Log("Invalid Trigger Event got through");
				}
				
			}
		}
		
 
	 */
