using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

namespace Pokemon
{
//	[UpdateBefore(typeof(ATriggerSystem))] doesn't help
	public class TriggerEventSystem : JobComponentSystem
	{
		private BuildPhysicsWorld buildPhysicsWorld;
		private StepPhysicsWorld stepPhysicsWorld;
		private EndSimulationEntityCommandBufferSystem m_EndSimulationEntityCommandBufferSystem;
		private EntityCommandBuffer entityCommandBuffer;
		private int i,j;
		private NativeArray<ATriggerEvent> oldAtes;
		private int oldAtesBase = 10;   //changeable
		private bool match = false;
		private bool alreadyCleared = false;
		protected override void OnCreateManager()
		{
			buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
			stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
			m_EndSimulationEntityCommandBufferSystem = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

			oldAtes = new NativeArray<ATriggerEvent>(oldAtesBase, Allocator.Persistent);
		}
		protected override void OnDestroy()
		{
			oldAtes.Dispose();
		}
		//	[BurstCompile]	//Burst Compile doesn't work for jobs with EntityCommandBuffers
		private struct TriggerJob : IJob
		{
			[ReadOnly] public PhysicsWorld PhysicsWorld;
			[ReadOnly] public TriggerEvents TriggerEvents;
			public NativeArray<ATriggerEvent> localAtes;
//			public Entity entity;
			public EntityCommandBuffer ecb;
			private int counter;
			public void Execute()
			{
				counter = 0;
				//	Debug.Log("attemping to do thing with " + triggerEventsLength);
				//add ATriggers that are missing
				foreach (TriggerEvent triggerEvent in TriggerEvents)
				{
					//		Debug.Log("Messing with Trigger "+counter+" of "+(triggerEventsLength-1));
					Entity entityA = PhysicsWorld.Bodies[triggerEvent.BodyIndices.BodyAIndex].Entity;
					Entity entityB = PhysicsWorld.Bodies[triggerEvent.BodyIndices.BodyBIndex].Entity;
				//	 a = PhysicsWorld.CollisionWorld.Bodies[triggerEvent.BodyIndices.BodyAIndex].Collider->Filter.CategoryBits
					localAtes[counter] = new ATriggerEvent { entityA = entityA, entityB = entityB, valid = true };
					counter++;
				}
			}
		}
		public struct ATriggerJob : IJob
		{
			[DeallocateOnJobCompletion] public NativeArray<PairOColliders> pocs;
			public NativeArray<ATriggerEvent> ates;
			[WriteOnly] public EntityCommandBuffer ecb;
			public void Execute()
			{
				for (int i = 0; i < pocs.Length; i++)
				{
			//		Debug.Log("i ==== " + i);
					if (pocs[i].entityACollider.Value.Value.Filter.MaskBits == pocs[i].entityBCollider.Value.Value.Filter.CategoryBits || pocs[i].entityBCollider.Value.Value.Filter.MaskBits == pocs[i].entityACollider.Value.Value.Filter.CategoryBits)
					{
						switch (pocs[i].entityACollider.Value.Value.Filter.CategoryBits)
						{
							//		case TriggerEventClass.Floor: ecb.AddComponent<FloorTrigger>(entityA, new FloorTrigger { }); break;
							//		case TriggerEventClass.Player: ecb.AddComponent<PlayerTrigger>(entityA, new PlayerTrigger { }); break;
							//		case TriggerEventClass.Pokemon: ecb.AddComponent<PokemonTrigger>(entityA, new PokemonTrigger { }); break;
							//		case TriggerEventClass.Structure: ecb.AddComponent<StructureTrigger>(entityA, new StructureTrigger { }); break;
							//		case TriggerEventClass.NPC: ecb.AddComponent<NPCTrigger>(pocs[i].entityA, new NPCTrigger { }); break;
							case TriggerEventClass.Interaction:
								//			Debug.Log("Atemping to Add an Interaction Trigger!");

								if (!pocs[i].aHasInteraction)
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

								if (!pocs[i].bHasInteraction)
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
							//			if (oldAtes[i].entityA == localAtes[j].entityA) Debug.Log("entityA == entityA");
							//			else Debug.Log("EntityA != EntityA");
							//			Debug.Log("old = "+ oldAtes[i].entityA.ToString()+ "new = "+ localAtes[i].entityA.ToString());
							//			Debug.Log("old = "+ oldAtes[i].entityB.ToString()+ "new = "+ localAtes[i].entityB.ToString());
							//			if (oldAtes[i].entityB == localAtes[j].entityB) Debug.Log("entityB == entityB");
							//			else Debug.Log("EntityA != EntityA");
							//			if (oldAtes[i].triggerType == localAtes[j].triggerType) Debug.Log("triggerType == triggerType");
							//			else Debug.Log("triggerType != triggerType "+oldAtes[i].triggerType+","+localAtes[i].triggerType);
							if (oldAtes[i] == localAtes[j])
							{
								match = true;
								break;
							}
						}
						if (!match)
						{
							//						Debug.Log("Failed to find match!"+match );
							ecb.RemoveComponent<ATriggerEvent>(oldAtes[i].entityA);
							//					PhysicsCollider pc = EntityManager.GetComponentData<PhysicsCollider>(oldAtes[i].entityA);
							//					Debug.Log("Belongs to " + TriggerEventClass.CollisionFilterValueToString(pc.Value.Value.Filter.CategoryBits) + " Collides With " + TriggerEventClass.CollisionFilterValueToString(pc.Value.Value.Filter.MaskBits));
							//					pc = EntityManager.GetComponentData<PhysicsCollider>(oldAtes[i].entityB);
							//					Debug.Log("Belongs to " + TriggerEventClass.CollisionFilterValueToString(pc.Value.Value.Filter.CategoryBits) + " Collides With " + TriggerEventClass.CollisionFilterValueToString(pc.Value.Value.Filter.MaskBits));
							switch (oldAtes[i].triggerType)
							{
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
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			//get trigger events
			TriggerEvents tes = stepPhysicsWorld.Simulation.TriggerEvents;
			i = 0;
			//get amount of triggers
			foreach (TriggerEvent triggerEvents in tes) i++;
			if (i > 0)
			{
				//there are triggers so we continue to create ATriggerEvents
				JobHandle combineDependencies = JobHandle.CombineDependencies(inputDeps, buildPhysicsWorld.FinalJobHandle, stepPhysicsWorld.FinalSimulationJobHandle);
				//create a temp NativeArray to store ATriggerEvents for processing
				NativeArray<ATriggerEvent> localAtes = new NativeArray<ATriggerEvent>(i, Allocator.TempJob);
				//we need this
				entityCommandBuffer = m_EndSimulationEntityCommandBufferSystem.CreateCommandBuffer();
				//run trigger job
				TriggerJob collisionJob = new TriggerJob
				{
					PhysicsWorld = buildPhysicsWorld.PhysicsWorld,
					TriggerEvents = stepPhysicsWorld.Simulation.TriggerEvents,
					ecb = entityCommandBuffer,
					localAtes = localAtes
				};
				JobHandle collisionJobHandle = collisionJob.Schedule(combineDependencies);
				//must wait for this to finish to continue
				collisionJobHandle.Complete();
				/////////////////////////////////////////////////////////////////////////
				//update oldAtes size if nesscary
				if (localAtes.Length > oldAtes.Length)
				{
					Debug.Log("Adding new persistance values");
					oldAtes = new NativeArray<ATriggerEvent>(localAtes.Length + oldAtesBase, Allocator.Persistent);
				}
				if (alreadyCleared) alreadyCleared = false;

				//clear non-active trigger events
				UpdateOldAtesJob uoaj = new UpdateOldAtesJob {
					ecb = m_EndSimulationEntityCommandBufferSystem.CreateCommandBuffer(),
					oldAtes = oldAtes,
					localAtes = localAtes
				};
				JobHandle cleanJobHandle = uoaj.Schedule(collisionJobHandle);
				cleanJobHandle.Complete();
				//////////////////////////////////////////////////////////////////////////
				NativeArray<PairOColliders> poc = new NativeArray<PairOColliders>(localAtes.Length, Allocator.TempJob);
				//create POCS (because you can't get component data inside an IJob )
				for (int i = 0; i < localAtes.Length; i++) //use aTriggerData Native Array
				{
					poc[i] = new PairOColliders
					{
						entityACollider = EntityManager.GetComponentData<PhysicsCollider>(localAtes[i].entityA),
						entityBCollider = EntityManager.GetComponentData<PhysicsCollider>(localAtes[i].entityB),
						entityATriggerEvent = localAtes[i],
						aHasInteraction = EntityManager.HasComponent<InteractionTrigger>(localAtes[i].entityA),
						bHasInteraction = EntityManager.HasComponent<InteractionTrigger>(localAtes[i].entityB)
					};
				}
				/////////////////////////////////////////////////////////////////////////
				JobHandle triggerJobHandle = new ATriggerJob
				{
					ecb = entityCommandBuffer,
					pocs = poc,
					ates = localAtes
				}.Schedule(inputDeps);
				triggerJobHandle.Complete();
				//update oldAtes content
				for (i = 0; i < localAtes.Length; i++)
					oldAtes[i] = localAtes[i];
				localAtes.Dispose();
				return cleanJobHandle;
			}
			else
			{
				if (!alreadyCleared)
				{
					//clear oldAtes
					for (i = 0; i < oldAtes.Length; i++)
					{
						Debug.Log("CLEANN!!!" + oldAtes[i].triggerType);
						if (oldAtes[i].valid)
						{
							switch (oldAtes[i].triggerType)
							{
								case TriggerEventClass.Interaction:
									//since interaction trigger is set the entityB of the trigger
									Debug.Log("Removing INteraction trigger on B");
									EntityManager.RemoveComponent<InteractionTrigger>(oldAtes[i].entityB);
									break;
								default: break;
							}
							EntityManager.RemoveComponent<ATriggerEvent>(oldAtes[i].entityA);
							oldAtes[i] = new ATriggerEvent { };
						}
						else break;
					}
					alreadyCleared = true;
					Debug.Log("Cleared the persistant NativeArray");
				}
				return inputDeps;
			}
		}
	}

}