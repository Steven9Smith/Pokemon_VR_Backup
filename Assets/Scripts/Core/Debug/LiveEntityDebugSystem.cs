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
		public struct EntityChangeRequest : IComponentData
		{
			public float3 position;
			public float scale;
			public PhysicsVelocity physicsVelocity;
			public PhysicsDamping physicsDamping;
			//not do rotation right now		public 
			public CoreData coreData;
			public StateData stateData;
			public PokemonEntityData pokemonEntityData;
		}

		public class LiveEntityDebugSystem : JobComponentSystem
		{
			private EntityQuery ChangeRequestQuery,CoreDataQuery;
			private GameObject EntityDebug;
			private LiveEntityDebugComponent DebugComponent;
			private Entity desiredEntity;

			protected override void OnCreateManager()
			{
				ChangeRequestQuery = GetEntityQuery(typeof(EntityChangeRequest));
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
							if (desiredEntity.Equals(new Entity { }))
							{
								if (DebugComponent.EntityName == "")
								{
									//do nothing
								}
								else
								{
									if (CoreDataQuery.CalculateEntityCount() > 0)
									{
										NativeArray<Entity> _entities = CoreDataQuery.ToEntityArray(Allocator.TempJob);
										NativeArray<CoreData> cds = CoreDataQuery.ToComponentDataArray<CoreData>(Allocator.TempJob);
										for (int i = 0; i < cds.Length; i++)
										{
											if (cds[i].ToNameString() == DebugComponent.EntityName)
											{
												desiredEntity = _entities[i];
												break;
											}
										}
										_entities.Dispose();
										cds.Dispose();
										if (desiredEntity.Equals(new Entity { }))
											Debug.LogError("Failed to find Entity that matches \"" + DebugComponent.EntityName + "\"");
									}
									else Debug.LogError("Cannot use LiveEntityDebugging with no Entity with a CoreData Component");
								}
							}
							else
							{
								//populate the display part of the the debug component
								string tempA = "";
								tempA = EntityManager.GetName(desiredEntity);
								DebugComponent.Display.coreData.Name = tempA.Split(':')[0];
								DebugComponent.Display.coreData.BaseName = tempA.Split(':')[1];
								DebugComponent.Display.position = EntityManager.GetComponentData<Translation>(desiredEntity).Value;
								DebugComponent.Display.scale = EntityManager.GetComponentData<Scale>(desiredEntity).Value;
								if (EntityManager.HasComponent<PhysicsVelocity>(desiredEntity))
								{
									PhysicsVelocity ps = EntityManager.GetComponentData<PhysicsVelocity>(desiredEntity);
									if (DebugComponent.Change.applyChange)
									{
										if (!DebugComponent.Change.physicsVelocity.Angular.Equals(float3.zero) || !DebugComponent.Change.physicsVelocity.Linear.Equals(float3.zero))
										{
											EntityManager.SetComponentData<PhysicsVelocity>(desiredEntity, new PhysicsVelocity
											{
												Angular = !DebugComponent.Change.physicsVelocity.Angular.Equals(float3.zero) ? DebugComponent.Change.physicsVelocity.Angular : ps.Angular,
												Linear = !DebugComponent.Change.physicsVelocity.Linear.Equals(float3.zero) ? DebugComponent.Change.physicsVelocity.Linear : ps.Linear
											});
										}
										DebugComponent.Display.physicsVelocity = DebugComponent.Change.physicsVelocity;
									}
									else
									{
										DebugComponent.Display.physicsVelocity.Angular = ps.Angular;
										DebugComponent.Display.physicsVelocity.Linear = ps.Linear;
									}
								}
								if (EntityManager.HasComponent<PhysicsDamping>(desiredEntity))
								{
									PhysicsDamping pd = EntityManager.GetComponentData<PhysicsDamping>(desiredEntity);
									if (DebugComponent.Change.applyChange)
									{
										if (!DebugComponent.Change.physicsVelocity.Angular.Equals(float3.zero) || !DebugComponent.Change.physicsVelocity.Linear.Equals(float3.zero))
										{
											EntityManager.SetComponentData(desiredEntity, new PhysicsDamping
											{
												Angular = DebugComponent.Change.physicsDamping.Angular > 0 ? DebugComponent.Change.physicsDamping.Angular : pd.Angular,
												Linear = DebugComponent.Change.physicsDamping.Linear > 0 ? DebugComponent.Change.physicsDamping.Linear : pd.Linear
											});
										}
										DebugComponent.Display.physicsDamping = DebugComponent.Change.physicsDamping;
									}
									else
									{
										DebugComponent.Display.physicsDamping.Angular = pd.Angular;
										DebugComponent.Display.physicsDamping.Linear = pd.Linear;
									}
								}
								if (EntityManager.HasComponent<StateData>(desiredEntity))
								{
									StateData sd = EntityManager.GetComponentData<StateData>(desiredEntity);
								/*
								 *	im to lazy to finish ;n;
								 * if (DebugComponent.Change.applyChange)
									{
										if (DebugComponent.Change.stateData.AddToExisting)
										{
											EntityManager.SetComponentData<StateData>(desiredEntity, new StateData
											{
												
											});
										}
										else
										{

										}
									}*/
									DebugComponent.Display.stateData.isAttacking1 = sd.isAttacking1;
									DebugComponent.Display.stateData.isAttacking2 = sd.isAttacking2;
									DebugComponent.Display.stateData.isAttacking3 = sd.isAttacking3;
									DebugComponent.Display.stateData.isAttacking4 = sd.isAttacking4;
									DebugComponent.Display.stateData.isCreeping = sd.isCreeping;
									DebugComponent.Display.stateData.isCrouching = sd.isCrouching;
									DebugComponent.Display.stateData.isEmoting1 = sd.isEmoting1;
									DebugComponent.Display.stateData.isEmoting2 = sd.isEmoting2;
									DebugComponent.Display.stateData.isEmoting3 = sd.isEmoting3;
									DebugComponent.Display.stateData.isEmoting4 = sd.isEmoting4;
									DebugComponent.Display.stateData.isIdle = sd.isIdle;
									DebugComponent.Display.stateData.isJumping = sd.isJumping;
									DebugComponent.Display.stateData.isRunning = sd.isRunning;
									DebugComponent.Display.stateData.isWalking = sd.isWalking;
								}
								if (EntityManager.HasComponent<PokemonEntityData>(desiredEntity))
								{
									PokemonEntityData ped = EntityManager.GetComponentData<PokemonEntityData>(desiredEntity);

									if (DebugComponent.Change.applyChange)
									{
										EntityManager.SetComponentData<PokemonEntityData>(desiredEntity, new PokemonEntityData(
											DebugComponent.Change.pokemonEntityData.PokedexNumber > 0 ? DebugComponent.Change.pokemonEntityData.PokedexNumber : ped.PokedexNumber,
											DebugComponent.Change.pokemonEntityData.Height > 0 ? DebugComponent.Change.pokemonEntityData.Height : ped.Height,
											DebugComponent.Change.pokemonEntityData.experienceYield > 0 ? DebugComponent.Change.pokemonEntityData.experienceYield : ped.experienceYield,
											DebugComponent.Change.pokemonEntityData.LevelingRate > 0 ? DebugComponent.Change.pokemonEntityData.LevelingRate : ped.LevelingRate,
											DebugComponent.Change.pokemonEntityData.Freindship > 0 ? DebugComponent.Change.pokemonEntityData.Freindship : ped.Freindship,
											DebugComponent.Change.pokemonEntityData.Speed > 0 ? DebugComponent.Change.pokemonEntityData.Speed : ped.Speed,
											DebugComponent.Change.pokemonEntityData.Acceleration > 0 ? DebugComponent.Change.pokemonEntityData.Acceleration : ped.Acceleration,
											DebugComponent.Change.pokemonEntityData.Hp > 0 ? DebugComponent.Change.pokemonEntityData.Hp : ped.Hp,
											DebugComponent.Change.pokemonEntityData.Attack > 0 ? DebugComponent.Change.pokemonEntityData.Attack : ped.Attack,
											DebugComponent.Change.pokemonEntityData.Defense > 0 ? DebugComponent.Change.pokemonEntityData.Defense : ped.Defense,
											DebugComponent.Change.pokemonEntityData.SpecialAttack > 0 ? DebugComponent.Change.pokemonEntityData.SpecialAttack : ped.SpecialAttack,
											DebugComponent.Change.pokemonEntityData.SpecialDefense > 0 ? DebugComponent.Change.pokemonEntityData.SpecialDefense : ped.SpecialDefense,
											DebugComponent.Change.pokemonEntityData.currentStamina > 0 ? DebugComponent.Change.pokemonEntityData.currentStamina : ped.currentStamina,
											DebugComponent.Change.pokemonEntityData.maxStamina > 0 ? DebugComponent.Change.pokemonEntityData.maxStamina : ped.maxStamina,
											DebugComponent.Change.pokemonEntityData.currentHp > 0 ? DebugComponent.Change.pokemonEntityData.currentHp : ped.currentHp,
											DebugComponent.Change.pokemonEntityData.Mass > 0 ? DebugComponent.Change.pokemonEntityData.Mass : ped.Mass,
											DebugComponent.Change.pokemonEntityData.jumpHeight > 0 ? DebugComponent.Change.pokemonEntityData.jumpHeight : ped.jumpHeight,
											DebugComponent.Change.pokemonEntityData.currentLevel > 0 ? DebugComponent.Change.pokemonEntityData.currentLevel : ped.currentLevel,
											new Pokemon.Move.PokemonMoveSet
											{
												pokemonMoveA = new Pokemon.Move.PokemonMove
												{
													index = DebugComponent.Change.pokemonEntityData.pokemonMoveSet.pokemonMoveA.index,
													isValid = DebugComponent.Change.pokemonEntityData.pokemonMoveSet.pokemonMoveA.isValid,
													name = new ByteString30(DebugComponent.Change.pokemonEntityData.pokemonMoveSet.pokemonMoveA.name),
												},pokemonMoveB = new Pokemon.Move.PokemonMove
												{
													index = DebugComponent.Change.pokemonEntityData.pokemonMoveSet.pokemonMoveB.index,
													isValid = DebugComponent.Change.pokemonEntityData.pokemonMoveSet.pokemonMoveB.isValid,
													name = new ByteString30(DebugComponent.Change.pokemonEntityData.pokemonMoveSet.pokemonMoveB.name),
												},pokemonMoveC = new Pokemon.Move.PokemonMove
												{
													index = DebugComponent.Change.pokemonEntityData.pokemonMoveSet.pokemonMoveC.index,
													isValid = DebugComponent.Change.pokemonEntityData.pokemonMoveSet.pokemonMoveC.isValid,
													name = new ByteString30(DebugComponent.Change.pokemonEntityData.pokemonMoveSet.pokemonMoveC.name),
												},pokemonMoveD = new Pokemon.Move.PokemonMove
												{
													index = DebugComponent.Change.pokemonEntityData.pokemonMoveSet.pokemonMoveD.index,
													isValid = DebugComponent.Change.pokemonEntityData.pokemonMoveSet.pokemonMoveD.isValid,
													name = new ByteString30(DebugComponent.Change.pokemonEntityData.pokemonMoveSet.pokemonMoveD.name),
												},
											}
											,
											DebugComponent.Change.pokemonEntityData.guiId > 0 ? DebugComponent.Change.pokemonEntityData.guiId : ped.guiId,
											DebugComponent.Change.pokemonEntityData.BodyType > 0 ? DebugComponent.Change.pokemonEntityData.BodyType : ped.BodyType)
											
										);
									}
									ped = EntityManager.GetComponentData<PokemonEntityData>(desiredEntity);



									DebugComponent.Display.pokemonEntityData.Acceleration = ped.Acceleration;
									DebugComponent.Display.pokemonEntityData.Attack = ped.Attack;
									DebugComponent.Display.pokemonEntityData.BodyType = ped.BodyType;
									DebugComponent.Display.pokemonEntityData.currentHp = ped.currentHp;
									DebugComponent.Display.pokemonEntityData.currentLevel = ped.currentLevel;
									DebugComponent.Display.pokemonEntityData.currentStamina = ped.currentStamina;
									DebugComponent.Display.pokemonEntityData.Defense = ped.Defense;
									DebugComponent.Display.pokemonEntityData.experienceYield = ped.experienceYield;
									DebugComponent.Display.pokemonEntityData.Freindship = ped.Freindship;
									DebugComponent.Display.pokemonEntityData.guiId = ped.guiId;
									DebugComponent.Display.pokemonEntityData.Height = ped.Height;
									DebugComponent.Display.pokemonEntityData.Hp = ped.Hp;
									DebugComponent.Display.pokemonEntityData.jumpHeight = ped.jumpHeight;
									DebugComponent.Display.pokemonEntityData.LevelingRate = ped.LevelingRate;
									DebugComponent.Display.pokemonEntityData.Mass = ped.Mass;
									DebugComponent.Display.pokemonEntityData.maxStamina = ped.maxStamina;
									DebugComponent.Display.pokemonEntityData.PokedexNumber = ped.PokedexNumber;
									DebugComponent.Display.pokemonEntityData.SpecialAttack = ped.SpecialAttack;
									DebugComponent.Display.pokemonEntityData.SpecialDefense = ped.SpecialDefense;
									DebugComponent.Display.pokemonEntityData.Speed = ped.Speed;
									DebugComponent.Display.pokemonEntityData.pokemonMoveSet.pokemonMoveA.index = ped.pokemonMoveSet.pokemonMoveA.index;
									DebugComponent.Display.pokemonEntityData.pokemonMoveSet.pokemonMoveA.isValid = ped.pokemonMoveSet.pokemonMoveA.isValid;
									DebugComponent.Display.pokemonEntityData.pokemonMoveSet.pokemonMoveA.name = ped.pokemonMoveSet.pokemonMoveA.name.ToString();
									DebugComponent.Display.pokemonEntityData.pokemonMoveSet.pokemonMoveB.index = ped.pokemonMoveSet.pokemonMoveB.index;
									DebugComponent.Display.pokemonEntityData.pokemonMoveSet.pokemonMoveB.isValid = ped.pokemonMoveSet.pokemonMoveB.isValid;
									DebugComponent.Display.pokemonEntityData.pokemonMoveSet.pokemonMoveB.name = ped.pokemonMoveSet.pokemonMoveB.name.ToString();
									DebugComponent.Display.pokemonEntityData.pokemonMoveSet.pokemonMoveC.index = ped.pokemonMoveSet.pokemonMoveC.index;
									DebugComponent.Display.pokemonEntityData.pokemonMoveSet.pokemonMoveC.isValid = ped.pokemonMoveSet.pokemonMoveC.isValid;
									DebugComponent.Display.pokemonEntityData.pokemonMoveSet.pokemonMoveC.name = ped.pokemonMoveSet.pokemonMoveC.name.ToString();
									DebugComponent.Display.pokemonEntityData.pokemonMoveSet.pokemonMoveD.index = ped.pokemonMoveSet.pokemonMoveD.index;
									DebugComponent.Display.pokemonEntityData.pokemonMoveSet.pokemonMoveD.isValid = ped.pokemonMoveSet.pokemonMoveD.isValid;
									DebugComponent.Display.pokemonEntityData.pokemonMoveSet.pokemonMoveD.name = ped.pokemonMoveSet.pokemonMoveD.name.ToString();
								}

								if (DebugComponent.Change.applyChange)
								{
									if (!DebugComponent.Change.position.Equals(float3.zero))
										EntityManager.SetComponentData<Translation>(desiredEntity, new Translation { Value = DebugComponent.Change.position });
									if (DebugComponent.Change.scale > 0)
										EntityManager.SetComponentData<Scale>(desiredEntity, new Scale { Value = DebugComponent.Change.scale });
									if (DebugComponent.Change.coreData.Name != "" || DebugComponent.Change.coreData.BaseName != "") {
										CoreData cd = EntityManager.GetComponentData<CoreData>(desiredEntity);
										EntityManager.SetComponentData<CoreData>(desiredEntity,new CoreData {
											Name =  DebugComponent.Change.coreData.Name != "" ? new ByteString30(DebugComponent.Change.coreData.Name) : cd.Name,
											BaseName =  DebugComponent.Change.coreData.BaseName != "" ? new ByteString30(DebugComponent.Change.coreData.BaseName) : cd.BaseName,
											isValid = true,
											scale = DebugComponent.Change.scale > 0 ? DebugComponent.Change.scale : EntityManager.GetComponentData<Scale>(desiredEntity).Value,
											size = cd.size
										});
									}


									if (!DebugComponent.Change.constantChange) DebugComponent.Change.applyChange = false;
								}
								if (DebugComponent.Change.copyFromDisplay)
								{
									DebugComponent.Change = CoreFunctionsClass.Clone(DebugComponent.Display);
									DebugComponent.Change.copyFromDisplay = false;
									Debug.Log("ccccc");
								}
							}
						}
					}
				}
				//handle the change request(s)
				if (ChangeRequestQuery.CalculateEntityCount() == 0) return inputDeps;
				NativeArray<Entity> entities = ChangeRequestQuery.ToEntityArray(Allocator.TempJob);
				NativeArray<EntityChangeRequest> requests = ChangeRequestQuery.ToComponentDataArray<EntityChangeRequest>(Allocator.TempJob);

				for (int i = 0; i < entities.Length; i++)
				{
					//apply changes
					if (!requests[i].position.Equals(float3.zero))
						EntityManager.SetComponentData(entities[i], new Translation { Value = requests[i].position });
					if (requests[i].scale > 0)
						EntityManager.SetComponentData(entities[i], new Scale { Value = requests[i].scale });

				}
				entities.Dispose();
				requests.Dispose();
				return inputDeps;
			}
		}
	}
}
