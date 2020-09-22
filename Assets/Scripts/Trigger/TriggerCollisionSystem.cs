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

		protected override void OnCreate()
		{
			oldAtes = new NativeArray<ATriggerEvent>(oldAtesBase, Allocator.Persistent);
			buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
			stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
			endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
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

				//make sure the nativelist has enough space for the current amount of trigger events
				if (counter >= currentLength) {
					NativeArray<ATriggerEvent> _localAtes = new NativeArray<ATriggerEvent>(localAtes, Allocator.Temp);
					localAtes = new NativeArray<ATriggerEvent>(currentLength + oldAtesBase, Allocator.Temp);
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
				case TriggerEventClass.Damage | TriggerEventClass.PokemonAttacking:
					if (!EntityManager.HasComponent<DamageTrigger>(ate.entityB))
					{
			//			Debug.Log("Adding DamageTrigger");
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
			//			Debug.Log("Adding InteractionTrigger");
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
					PhysicsCollider pc2 = EntityManager.GetComponentData<PhysicsCollider>(ate.entityB);
				//	Debug.Log("Detected an Unknown TriggerType " + ate.ToString() + " ABElongsTO = \""+ EntityManager.GetName(ate.entityA)+"\"\n"+ 
				//		/*physicsCollider.Value.Value.Filter.BelongsTo+*/"|"+TriggerEventClass.CollisionFilterValueToString(physicsCollider.Value.Value.Filter.BelongsTo) + ":"
				//		+ /*physicsCollider.Value.Value.Filter.CollidesWith+*/"|"+ TriggerEventClass.CollisionFilterValueToString(physicsCollider.Value.Value.Filter.CollidesWith));
			/*	Debug.Log("Detected unknown Trigger Type:\nEntity A = \""+
						EntityManager.GetName(ate.entityA)+"\"\n\tBelongsTo: "+TriggerEventClass.CollisionFilterValueToString(physicsCollider.Value.Value.Filter.BelongsTo)+"\n\tCollidesWith: "+ TriggerEventClass.CollisionFilterValueToString(physicsCollider.Value.Value.Filter.CollidesWith)+
						"\nEntity B = \""+EntityManager.GetName(ate.entityB)+"\"\n\tBelongsTo: " + TriggerEventClass.CollisionFilterValueToString(pc2.Value.Value.Filter.BelongsTo) + "\n\tCollidesWith: " + TriggerEventClass.CollisionFilterValueToString(pc2.Value.Value.Filter.CollidesWith)
					);*/
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
			//verify that ATriggerEntities still exist (thanks a lot particle system)
			for (i = 0; i < localAtes.Length; i++) {
				if (localAtes[i].valid)
				{
					if (!EntityManager.Exists(localAtes[i].entityA) || !EntityManager.Exists(localAtes[i].entityB))
					{
						for (int j = i; j < localAtes.Length; j++)
							localAtes[j] = j==localAtes.Length ? localAtes[j + 1] : new ATriggerEvent { valid = false};
						i--;
					}
				}
				else break;
			}
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
				//	if (localAtes[i].triggerType == 0) Debug.Log("Unable to update determine triggerType for entity "+EntityManager.GetName(localAtes[i].entityA)+"::::"+localAtes[i].ToString());
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
								case TriggerEventClass.Damage | TriggerEventClass.PokemonAttacking:
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
	//The same as the TriggerEventSystem but with Collisions
	public class ACollisionEventSystem : JobComponentSystem
	{
		private int i;
		private NativeArray<ACollisionEvent> oldAtes;
		private const int oldAtesBase = 10;   //changeable
		private bool match = false, alreadyCleared = false;

		private int localAtesRealLength = oldAtesBase;

		private BuildPhysicsWorld buildPhysicsWorld;
		private StepPhysicsWorld stepPhysicsWorld;
		private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

		protected override void OnCreate()
		{
			oldAtes = new NativeArray<ACollisionEvent>(oldAtesBase, Allocator.Persistent);
			buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
			stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
			endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}
		protected override void OnDestroy() { oldAtes.Dispose(); }
		public struct CollisionEventsJob : ICollisionEventsJob
		{
			public EntityCommandBuffer ecb;
			public ComponentDataFromEntity<ACollisionEvent> hasACollisionEvent;
			public NativeArray<ACollisionEvent> localAtes;
			public int counter;
			public int currentLength;
			public void Execute(CollisionEvent CollisionEvent)
			{
				//make sure the nativelist has enough space for the current amount of Collision events
				if (counter >= currentLength)
				{
					NativeArray<ACollisionEvent> _localAtes = new NativeArray<ACollisionEvent>(localAtes, Allocator.TempJob);
					localAtes = new NativeArray<ACollisionEvent>(currentLength + oldAtesBase, Allocator.TempJob);
					for (int i = 0; i < currentLength; i++)
						localAtes[i] = _localAtes[i];
					currentLength += oldAtesBase;
				}
				if (!hasACollisionEvent.Exists(CollisionEvent.Entities.EntityA))
				{
					localAtes[counter] = new ACollisionEvent
					{
						entityA = CollisionEvent.Entities.EntityA,
						entityB = CollisionEvent.Entities.EntityB,
						valid = true
					};
				}
				counter++;
			}
		}
		public struct UpdateOldAtesJob : IJob
		{
			public NativeArray<ACollisionEvent> oldAtes;
			public NativeArray<ACollisionEvent> localAtes;
			public EntityCommandBuffer ecb;
			private int i, j;
			private bool match;
			public void Execute()
			{
				match = false;
				//remove oldCollisions if not in new Collisions
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
							ecb.RemoveComponent<ACollisionEvent>(oldAtes[i].entityA);
							switch (oldAtes[i].CollisionType)
							{
								case CollisionEventClass.Damage:
									Debug.Log("Removing damage Collision on B");
									ecb.RemoveComponent<DamageCollision>(oldAtes[i].entityB);
									break;
								case CollisionEventClass.Interaction:
									//since interaction Collision is set the entityB of the Collision
									Debug.Log("Removing Interaction Collision on B");
									ecb.RemoveComponent<InteractionCollision>(oldAtes[i].entityB);
									break;
								default: break;
							}
							oldAtes[i] = new ACollisionEvent { };
						}
						match = false;
					}
					else break;
				}
			}
		}
		public ACollisionEvent processCollisions(ACollisionEvent ate)
		{
			PhysicsCollider physicsCollider = EntityManager.GetComponentData<PhysicsCollider>(ate.entityA);

			switch (physicsCollider.Value.Value.Filter.BelongsTo)
			{
				case CollisionEventClass.Damage | CollisionEventClass.PokemonAttacking:
					if (!EntityManager.HasComponent<DamageCollision>(ate.entityB))
					{
						//			Debug.Log("Adding DamageCollision");
						EntityManager.AddComponentData<DamageCollision>(ate.entityB,
							new DamageCollision { Value = EntityManager.GetComponentData<DamageCollisionData>(ate.entityA).Value });
					}
					//			else Debug.Log("The Entity already has a DamageCollision");
					return new ACollisionEvent
					{
						entityA = ate.entityA,
						entityB = ate.entityB,
						valid = true,
						CollisionType = CollisionEventClass.Damage
					};
				case CollisionEventClass.Interaction:
					if (!EntityManager.HasComponent<InteractionCollision>(ate.entityB))
					{
						//			Debug.Log("Adding InteractionCollision");
						EntityManager.AddComponentData<InteractionCollision>(ate.entityB, new InteractionCollision { });

					}
					//			else Debug.Log("The Entity already has a InteractionCollision");
					return new ACollisionEvent
					{
						entityA = ate.entityA,
						entityB = ate.entityB,
						valid = true,
						CollisionType = CollisionEventClass.Interaction
					};
				default:
					PhysicsCollider pc2 = EntityManager.GetComponentData<PhysicsCollider>(ate.entityB);
					//		Debug.Log("Detected an Unknown CollisionType " + ate.ToString() + " ABElongsTO = \""+ EntityManager.GetName(ate.entityA)+"\"\n"+ 
					//		/*physicsCollider.Value.Value.Filter.BelongsTo+*/"|"+CollisionEventClass.CollisionFilterValueToString(physicsCollider.Value.Value.Filter.BelongsTo) + ":"
					//	+ /*physicsCollider.Value.Value.Filter.CollidesWith+*/"|"+ CollisionEventClass.CollisionFilterValueToString(physicsCollider.Value.Value.Filter.CollidesWith));
					Debug.Log("Detected unknown Collision Type:\nEntity A = \"" +
						EntityManager.GetName(ate.entityA) + "\"\n\tBelongsTo: " + CollisionEventClass.CollisionFilterValueToString(physicsCollider.Value.Value.Filter.BelongsTo) + "\n\tCollidesWith: " + CollisionEventClass.CollisionFilterValueToString(physicsCollider.Value.Value.Filter.CollidesWith) +
						"\nEntity B = \"" + EntityManager.GetName(ate.entityB) + "\"\n\tBelongsTo: " + CollisionEventClass.CollisionFilterValueToString(pc2.Value.Value.Filter.BelongsTo) + "\n\tCollidesWith: " + CollisionEventClass.CollisionFilterValueToString(pc2.Value.Value.Filter.CollidesWith)
					);
					break;
			}
			return ate;
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			if (stepPhysicsWorld.Simulation.Type == SimulationType.NoPhysics) return inputDeps;

			NativeArray<ACollisionEvent> localAtes = new NativeArray<ACollisionEvent>(oldAtes.Length, Allocator.TempJob);

			JobHandle jobHandle = new CollisionEventsJob
			{
				ecb = endSimulationEntityCommandBufferSystem.CreateCommandBuffer(),
				hasACollisionEvent = GetComponentDataFromEntity<ACollisionEvent>(),
				counter = 0,
				currentLength = localAtesRealLength,
				localAtes = localAtes
			}.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, inputDeps);
			//must wait for this to finish to continue
			jobHandle.Complete();
			//verify that ACollisionEntities still exist (thanks a lot particle system)
			for (i = 0; i < localAtes.Length; i++)
			{
				if (localAtes[i].valid)
				{
					if (!EntityManager.Exists(localAtes[i].entityA) || !EntityManager.Exists(localAtes[i].entityB))
					{
						for (int j = i; j < localAtes.Length; j++)
							localAtes[j] = j == localAtes.Length ? localAtes[j + 1] : new ACollisionEvent { valid = false };
						i--;
					}
				}
				else break;
			}
			//create/add new Collision events
			for (i = 0; i < localAtes.Length; i++)
			{
				if (localAtes[i].valid)
				{
					match = false;
					localAtes[i] = processCollisions(localAtes[i]);
					//flip the entities
					if (localAtes[i].CollisionType == 0)
						localAtes[i] = processCollisions(new ACollisionEvent
						{
							entityA = localAtes[i].entityB,
							entityB = localAtes[i].entityA,
							CollisionType = localAtes[i].CollisionType,
							valid = localAtes[i].valid
						});
					if (localAtes[i].CollisionType == 0) Debug.Log("Unable to update determine CollisionType for entity " + EntityManager.GetName(localAtes[i].entityA) + "::::" + localAtes[i].ToString());
				}
				else break;
			}
			//if there are no new Collision events

			if (i == 0)
			{
				if (!alreadyCleared)
				{
					//			Debug.Log("Clearing STUFF");
					//clear oldAtes
					for (i = 0; i < oldAtes.Length; i++)
					{
						//				Debug.Log("checking "+i+" of "+oldAtes.Length);
						if (oldAtes[i].valid)
						{
							Debug.Log("Attempting to remove..." + oldAtes[i].ToString());
							switch (oldAtes[i].CollisionType)
							{
								case CollisionEventClass.Damage | CollisionEventClass.PokemonAttacking:
									Debug.Log("Removing damage Collision on B");
									EntityManager.RemoveComponent<DamageCollision>(oldAtes[i].entityB);
									break;
								case CollisionEventClass.Interaction:
									//since interaction Collision is set the entityB of the Collision
									Debug.Log("Removing Interaction Collision on B");
									EntityManager.RemoveComponent<InteractionCollision>(oldAtes[i].entityB);
									break;
								default: break;
							}
							EntityManager.RemoveComponent<ACollisionEvent>(oldAtes[i].entityA);
							oldAtes[i] = new ACollisionEvent { };
						}
						else
						{
							//					Debug.Log("Failed to get valid");
							break;
						}
					}
	//				Debug.Log("Cleared the persistant NativeArray");
					alreadyCleared = true;
				}
				localAtes.Dispose();
				return inputDeps;
			}
			//else

			if (localAtes.Length > oldAtes.Length)
			{
				Debug.Log("Adding new persistance values");
				oldAtes = new NativeArray<ACollisionEvent>(localAtes.Length + oldAtesBase, Allocator.Persistent);
			}
			if (alreadyCleared) alreadyCleared = false;

			/////////////////////////////////////////////////////////////////////////
			//clear non-active Collision events
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