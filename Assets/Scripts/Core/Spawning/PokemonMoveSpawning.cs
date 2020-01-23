using Core.Particles;
using Pokemon;
using Pokemon.Move;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Core.ParentChild;

namespace Core.Spawning {
	public class PokemonMoveSpawn
	{
		/// <summary>
		/// gets the PokemonMove Data associated with the spawning of the PokemonMove
		/// </summary>
		/// <param name="entityManager">EntityManager</param>
		/// <param name="name">PokemonMove name</param>
		/// <param name="entity">PokemonMove Entity</param>
		/// <returns><PokemonMOveDataSpawn/returns>
		public static PokemonMoveDataSpawn getPokemonMoveDataSpawn(EntityManager entityManager,string name, Entity entity,PokemonEntityData pokemonEntityData)
		{
			PokemonMoveDataSpawn pmds = new PokemonMoveDataSpawn { };
			PlayerInput pi = entityManager.GetComponentData<PlayerInput>(entity);
			PhysicsCollider physicsCollider = generatePokemonMoveCollider(name, entityManager, entity,pokemonEntityData,1);
			switch (name)
			{
				case "ThunderBolt":
				
					pmds = new PokemonMoveDataSpawn
					{
						translation = new Translation
						{
							Value = entityManager.GetComponentData<Translation>(entity).Value + new float3(0, 1.5f, 0)
						},
						//use camera direction
						rotation = new Rotation
						{
							//https://answers.unity.com/questions/1353333/how-to-add-2-quaternions.html
							Value = quaternion.LookRotation(pi.forward, new float3(0, 1f, 0))
						},
						hasCollider = true,
						hasDamping = true,
						hasMass = true,
						hasPhysicsVelocity = true,
						hasGravityFactor = false,
						physicsCollider = physicsCollider,
						physicsMass = PhysicsMass.CreateDynamic(physicsCollider.MassProperties, 1),
						physicsDamping = new PhysicsDamping()
						{
							Angular = 0.05f,
							Linear = 0.01f
						},
						physicsVelocity = new PhysicsVelocity
						{
							Angular = float3.zero,
							Linear = float3.zero
						},
						hasParticles = true

					};
					break;
				case "Tackle":
					
					pmds = new PokemonMoveDataSpawn
					{
						hasCollider = true,
						hasDamping = false,
						hasGravityFactor = false,
						hasMass = true,
						hasPhysicsVelocity = false,
						hasParticles = true,
						translation = entityManager.GetComponentData<Translation>(entity),
						rotation = entityManager.GetComponentData<Rotation>(entity),
						physicsCollider = physicsCollider,
						physicsMass = entityManager.GetComponentData<PhysicsMass>(entity),
						hasEntity = true,
						projectOnParentInstead = true
					};
					break;
				default:
					Debug.Log("failed to find a matching pokemon move data spawn");
					break;
			}
			return pmds;
		}
		/// <summary>
		/// Sets the RenderMesh of the given parameters
		/// </summary>
		/// <param name="entityManager">EntityManager</param>
		/// <param name="entity">PokemonMove Entity</param>
		/// <param name="pokemonMoveName">name of the pokemonMove</param>
	/*	public static void SetPokemonMoveRenderMesh(EntityManager entityManager,Entity entity, string pokemonMoveName)
		{
		//	Debug.Log("Pokemon/PokemonMoves/" + pokemonMoveName + "/" + pokemonMoveName);
			GameObject go = Resources.Load("Pokemon/PokemonMoves/"+pokemonMoveName+"/"+pokemonMoveName) as GameObject;
			if (go != null)
			{
				entityManager.SetSharedComponentData(entity, new RenderMesh
				{
					mesh = go.GetComponent<MeshFilter>().sharedMesh,
					castShadows = UnityEngine.Rendering.ShadowCastingMode.On,
					material = go.GetComponent<MeshRenderer>().sharedMaterial,
					receiveShadows = true
				});
			}
			else Debug.LogError("Failed to load pokemon move mesh");
		}*/
		/// <summary>
		/// Initializes the pokemon move entity 
		/// </summary>
		/// <param name="entityManager">EntityManager</param>
		/// <param name="name">name of the pokemon move</param>
		/// <param name="orgEntity">the Entity that executed the move</param>
		/// <param name="entity">the PokemonMove Entity</param>
		/// <param name="ped">PokemonEntityData</param>
		public static void ExecutePokemonMove(EntityManager entityManager,string name,ByteString30 pokemonName, Entity orgEntity,Entity entity, PokemonEntityData ped,GroupIndexSystem groupIndexSystem)
		{
			//test if a move is already being executed on the entity parent
			if (!entityManager.HasComponent<EntityChild>(orgEntity))
			{
				entityManager.AddComponentData<EntityChild>(orgEntity, new EntityChild { entity = entity, isValid = true,followChild = false });
				if (groupIndexSystem == null) Debug.LogError("GroupIndexSystem is null");
				Debug.Log("executing pokemon move \"" + name + "\"");
				PokemonMoveDataSpawn pmds = getPokemonMoveDataSpawn(entityManager, name, orgEntity, ped);
				PlayerInput pi = entityManager.GetComponentData<PlayerInput>(orgEntity);
				PokemonMoveDataEntity pmde = GetPokemonMoveDataEntity(new ByteString30(name), ped, pi.forward);
				if (pmds.hasEntity)
				{
					entityManager.SetComponentData(entity, new EntityParent { entity = orgEntity, isValid = true, followParent = true });
					if (pmds.projectOnParentInstead)
					{
		//				if (pmde.isValid) Debug.Log("It be valid!");
						entityManager.AddComponentData(orgEntity, pmde);
						pmde.preformActionsOn = false;
					}
				}
				CoreData cd = entityManager.GetComponentData<CoreData>(orgEntity);
				entityManager.AddComponentData<CoreData>(entity,new CoreData(new ByteString30(name),new ByteString30("PokemonMove"),cd.size,cd.scale));
				
				//get Pokemon's GroupIndex
				GroupIndexInfo gii = entityManager.GetComponentData<GroupIndexInfo>(orgEntity);
				gii.CurrentGroupIndex = groupIndexSystem.GetNextEmptyGroup();
				gii.Update = true;
				entityManager.SetComponentData(orgEntity, gii);
				int a = groupIndexSystem.ExludeGroupIndexNumber(gii.CurrentGroupIndex);
		//		Debug.Log("AAAAAAAAAAAAA "+a);
				//add group index but we want this to 
				gii = new GroupIndexInfo
				{
					CurrentGroupIndex = gii.CurrentGroupIndex,
					OldGroupIndex = 0,
					OriginalGroupIndex = gii.CurrentGroupIndex,
					Update = true,
				};
				entityManager.AddComponentData(entity, gii);
				//add/set collider 
				if (pmds.hasCollider)
				{
					if (entityManager.HasComponent<PhysicsCollider>(entity)) entityManager.SetComponentData(entity, pmds.physicsCollider);
					else entityManager.AddComponentData(entity, pmds.physicsCollider);
				}
				if (pmds.hasDamping)
				{
					if (entityManager.HasComponent<PhysicsDamping>(entity)) entityManager.SetComponentData(entity, pmds.physicsDamping);
					else entityManager.AddComponentData(entity, pmds.physicsDamping);
				}
				if (pmds.hasGravityFactor)
				{
					if (entityManager.HasComponent<PhysicsGravityFactor>(entity)) entityManager.SetComponentData(entity, pmds.physicsGravityFactor);
					else entityManager.AddComponentData(entity, pmds.physicsGravityFactor);
				}
				if (pmds.hasMass)
				{
					if (entityManager.HasComponent<PhysicsMass>(entity)) entityManager.SetComponentData(entity, pmds.physicsMass);
					else entityManager.AddComponentData(entity, pmds.physicsMass);
				}
				if (entityManager.HasComponent<PokemonMoveDataEntity>(entity)) entityManager.SetComponentData(entity, pmde);
				else entityManager.AddComponentData(entity, pmde);
				if (pmds.hasPhysicsVelocity)
				{
					if (entityManager.HasComponent<PhysicsVelocity>(entity)) entityManager.SetComponentData(entity, pmds.physicsVelocity);
					else entityManager.AddComponentData(entity, pmds.physicsVelocity);
				}
				if (entityManager.HasComponent<Translation>(entity)) entityManager.SetComponentData(entity, pmds.translation);
				else entityManager.AddComponentData(entity, pmds.translation);
				if (entityManager.HasComponent<Rotation>(entity)) entityManager.SetComponentData(entity, pmds.rotation);
				else entityManager.AddComponentData(entity, pmds.rotation);
				//add particle stuff
				ParticleSystemSpawnData pssd = PokemonMoves.getPokemonMoveParticleSystemData(ped.PokedexNumber, name);
				if (pssd.isValid)
				{
					Debug.Log("detected particleSystemspawn data");
					entityManager.AddComponentData(entity, pssd);
				}
				else Debug.LogWarning("ExecutePokemonMove: Failed to get ParticleSystemSpawnData!");
				if (pmds.hasParticles)
				{
					entityManager.AddComponentData(entity, new ParticleSystemRequest { });
					entityManager.AddComponentData(entity, new ParticleSystemData { });
				}
				if (!entityManager.HasComponent<Scale>(entity)) entityManager.AddComponentData<Scale>(entity, new Scale { Value = 1f });
				else entityManager.SetComponentData<Scale>(entity, new Scale { Value = 1f });
				switch (name)
				{
					case "ThunderBolt":
						Debug.Log("Spawning ThunderBolt");
						//set name and render mesg
						entityManager.SetName(entity, name);
						//entityManager.SetSharedComponentData(entity, renderMesh);
						PokemonDataClass.SetRenderMesh(entityManager, entity, pokemonName,1);
						break;
					case "Tackle":
						entityManager.SetName(entity, name);
						break;
					default: Debug.Log("Failed to set pokemon move data for \"" + name + "\""); break;
				}
				//
				PhysicsCollider op = entityManager.GetComponentData<PhysicsCollider>(orgEntity);
				op = entityManager.GetComponentData<PhysicsCollider>(orgEntity);
				PhysicsCollider tmpA = entityManager.GetComponentData<PhysicsCollider>(entity);
		//		Debug.Log("Chaned original entities collision filter to " + op.Value.Value.Filter.GroupIndex.ToString() + " with entity index = " + tmpA.Value.Value.Filter.GroupIndex.ToString());
			}
			else
			{
				//maybe cancel but for now we do nothing
			}
		}
		/*public void setMoveEntityData(Entity moveEntity, Entity mEntity, PokemonMove pokemonMove,
					PokemonEntityData pmd, EntityManager entityManager, PlayerInput pi)
				{
					PokemonMoveDataSpawn pmds = Core.Spawning.PokemonMoveSpawn.getPokemonMoveDataSpawn(entityManager,
						pokemonMove.name, mEntity, pmd);
					Debug.Log("Spawning "+ (pokemonMove.name) + pokemonMove.index);
					entityManager.SetName(moveEntity, (pokemonMove.name) + pokemonMove.index);
					entityManager.SetSharedComponentData(moveEntity, renderMeshDefault);
					entityManager.SetComponentData(moveEntity, pmds.translation);
					entityManager.SetComponentData(moveEntity, pmds.rotation);
					entityManager.SetComponentData(moveEntity, GetPokemonMoveDataEntity(pokemonMove.name, pmd, pi.forward));
					if (pmds.hasCollider) entityManager.SetComponentData(moveEntity, pmds.physicsCollider);
					if (pmds.hasPhysicsVelocity) entityManager.SetComponentData(moveEntity, pmds.physicsVelocity);
					if (pmds.hasMass) entityManager.AddComponentData(moveEntity, pmds.physicsMass);
					if (pmds.hasDamping) entityManager.AddComponentData(moveEntity, pmds.physicsDamping);
					if (pmds.hasGravityFactor) entityManager.AddComponentData(moveEntity, pmds.physicsGravityFactor);
					if (pmds.hasParticles)
					{
						entityManager.AddComponentData(moveEntity, new ParticleSystemRequest { });
						entityManager.AddComponentData(moveEntity, new ParticleSystemData { });
						ParticleSystemSpawnData pssd = PokemonMoves.getPokemonMoveParticleSystemData(pmd.PokedexNumber,(pokemonMove.name));
						if (pssd.isValid)
						{
							Debug.Log("detected particleSystemspawn data");
							entityManager.AddComponentData(moveEntity, pssd);
						}
					}
					if (pmds.hasEntity)
					{
						EntityManager.SetComponentData(moveEntity, new EntityParent {isValid = true,entity = pokemonMove.entity });
						EntityManager.AddComponentData(mEntity, new EntityChild { isValid = true, entity = moveEntity });
						EntityManager.AddComponentData(mEntity, GetPokemonMoveDataEntity(pokemonMove.name, pmd, pi.forward));
					}
				}*/
		public static PokemonMoveDataEntity GetPokemonMoveDataEntity(ByteString30 name, PokemonEntityData ped, float3 forward)
		{
			PokemonMoveDataEntity pmde = new PokemonMoveDataEntity { };
			switch (name.ToString())
			{
				case "Tackle":
					pmde = new PokemonMoveDataEntity
					{
						name = name,
						accuracy = 1f,
						attackType = PokemonMoves.AttackType.normal,
						damage = calculateDamage(ped.currentLevel, 40f),
						statusType = PokemonMoves.StatusType.none,
						contactType = PokemonMoves.ContactType.Physical,
						isValid = true,
						pokemonMoveAdjustmentData = new PokemonMoveAdjustmentData
						{
							isValid = true,
							pokemonMoveVelocitySet = new PokemonMoveVelocitySet
							{
								value = new PokemonMoveAdjustmentSet
								{
									A = new PokemonMoveAdjustment
									{
										value = new float3 { x = ped.Speed, y = 0f, z = ped.Speed },
										timeLength = -1f,
										useCameraDirection = true,
										staminaCost = 5f
									},
									isValid = true
								}
							}
						},
						forward = forward,
						hasParticles = true,
						preformActionsOn = true
					};
					break;
				case "ThunderBolt":
					pmde = new PokemonMoveDataEntity
					{
						attackType = PokemonMoves.AttackType.electric,
						contactType = PokemonMoves.ContactType.Special,
						name = name,
						isValid = true,
						statusType = PokemonMoves.StatusType.none,
						accuracy = 1f,
						damage = calculateDamage(ped.currentLevel, 40f),
						pokemonMoveAdjustmentData = new PokemonMoveAdjustmentData
						{
							isValid = true,
							pokemonMoveVelocitySet = new PokemonMoveVelocitySet
							{
								value = new PokemonMoveAdjustmentSet
								{
									isValid = true,
									A = new PokemonMoveAdjustment
									{
										timeLength = 1f,
										value = new float3 { x = 50f, y = 1f, z = 50f },
										useCameraDirection = true,
									},
									B = new PokemonMoveAdjustment
									{
										timeLength = 0.5f,
										value = new float3 { x = 0, y = 100f, z = 0 },
										useCameraDirection = false
									},
									C = new PokemonMoveAdjustment
									{
										timeLength = 0.1f,
										value = new float3 { x = 0, y = -1000f, z = 0 },
										useCameraDirection = false
									}
								}

							}
						},
						forward = forward,
						hasParticles = true,
						preformActionsOn = true
					};
					break;
				default:
					Debug.LogError("failed to find a matching pokemon move data entity");
					break;
			}
			return pmde;
		}
		//damage is calculated using the base damage and level
		private static float calculateDamage(float pokemonLevel, float baseDamage)
		{
			return (pokemonLevel / 100) / baseDamage;
		}
		/// <summary>
		/// generates the collider for the pokemon move associated with the given attack
		/// </summary>
		/// <param name="pokemonMoveName">name of the pokemon move</param>
		/// <param name="entityManager">Entity Manager</param>
		/// <param name="pokemon">Entity that executes the move</param>
		/// <returns></returns>
		public static PhysicsCollider generatePokemonMoveCollider(string pokemonMoveName,
			EntityManager entityManager, Entity pokemon,PokemonEntityData pokemonEntityData,int groupIndex = 1)
		{
			PhysicsCollider physicsCollider = new PhysicsCollider { };
			float3 position = entityManager.GetComponentData<Translation>(pokemon).Value;
			switch (pokemonMoveName)
			{
				case "ThunderBolt"://currently uses thunderbolt v1
					position.y = position.y + 2f;
					physicsCollider = new PhysicsCollider
					{
						Value = Unity.Physics.SphereCollider.Create(new SphereGeometry
						{
							Center = new float3(0,2f,0),
							Radius = pokemonEntityData.Height / 2
						},
							new CollisionFilter
							{
								BelongsTo = TriggerEventClass.PokemonMove | TriggerEventClass.Damage,
								CollidesWith = TriggerEventClass.Collidable | TriggerEventClass.Pokemon,
								GroupIndex = groupIndex
							},
							Unity.Physics.Material.Default)
					};
					break;
				case "Tackle":
					switch (pokemonEntityData.BodyType)
					{
						case PokemonDataClass.BODY_TYPE_HEAD_ONLY:
							//in this case the height represents the diameter.
							//we use the scale float to hold the raduis of the head
							physicsCollider = new PhysicsCollider
							{
								Value = Unity.Physics.SphereCollider.Create(new SphereGeometry
								{
									Center = float3.zero,
									Radius = pokemonEntityData.Height / 2
								},
								new CollisionFilter
								{
									BelongsTo = TriggerEventClass.PokemonMove | TriggerEventClass.Damage,
									GroupIndex = groupIndex,
									CollidesWith = TriggerEventClass.Collidable | TriggerEventClass.Pokemon
								//	CollidesWith = TriggerEventClass.Nothing
								},
								Unity.Physics.Material.Default)
							};
							break;
						default:
							Debug.LogError("Failed to get Pokemon's Body Type, craeting dummy size");
							break;
					}
					break;
				default:
					Debug.Log("Failed to generate collider data for \"" + pokemonMoveName + "\"");
					break;
			}
			return physicsCollider;
		}
		

		public static void ExecutePhysicalAttack(EntityManager entityManager,string pokemonName,Entity pokemonEntity,PokemonEntityData ped)
		{
			//change the Entity's CollisionFilter From player/entity to attacking player/ entity
			entityManager.AddComponentData(pokemonEntity, PokemonDataClass.getPokemonPhysicsCollider(pokemonName,
				ped,
				new CollisionFilter {
					BelongsTo = TriggerEventClass.PokemonAttacking,
					CollidesWith = uint.MaxValue,
					GroupIndex = 1
				},new Unity.Physics.Material {
					Flags = Unity.Physics.Material.MaterialFlags.IsTrigger
				})
			);
			//start physical attack animation

			//let the systems do the rest
		}
	
	}
}