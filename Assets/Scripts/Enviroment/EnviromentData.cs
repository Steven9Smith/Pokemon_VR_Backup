using Pokemon;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Core;

namespace Pokemon
{
    [Serializable]
    public struct EnviromentEntityDataSave
    {
        public ByteString30 EntityName;
		public ByteString30 EntityId;
		public ByteString30 EntityParentId;
		public ByteString30 PathString;
        public byte BoundType;
        public Translation Position;
        public Rotation Rotation;
        public Scale Scale;
    }
}
namespace Core.Enviroment
{
	public class EnviromentDataClass
	{

		public static void GenerateEnviroment(string region, string enviroment, EntityManager entityManager,
			float3 bounds, float3 position, quaternion rotation, float3 scale, float bevelRadius = 0)
		{
			EntityArchetype defaultArchtype = entityManager.CreateArchetype(
				typeof(Translation),
				typeof(Rotation),
				typeof(Scale),
				typeof(LocalToWorld),
				//		typeof(PhysicsCollider),
				typeof(AudioSharedData),
				typeof(AudioData),
				typeof(CoreData)
			);
			Entity entity = entityManager.CreateEntity(defaultArchtype);
			Translation trans = new Translation { Value = position };
			entityManager.SetComponentData(entity, trans);
			entityManager.SetComponentData(entity, new Scale { Value = scale.x });
			CoreData _cd = new CoreData(new ByteString30(enviroment), new ByteString30(region), bounds, new float3(1f, 1f, 1f));
			entityManager.SetComponentData(entity, _cd);
			/*	entityManager.SetComponentData(entity, new PhysicsCollider
			{
				Value = Unity.Physics.BoxCollider.Create(new BoxGeometry
				{
					Center = position+(bounds/2),
					Size = bounds,
					Orientation = rotation,
					BevelRadius = bevelRadius
				})
			});*/
			entityManager.SetSharedComponentData(entity, GetEnviromentSoundInfo(region, "Forest"));
			switch (enviroment)
			{
				case "Forest":
					//populate forest entities
					//we gonna need some trees, and bushes
					GameObject ForestTreeGO = Resources.Load("Enviroment/Kanto/Forest/Landscapes/Tree/Tree") as GameObject;
				//	ForestTreeGO = GameObject.Instantiate(ForestTreeGO);
				//	CoreFunctionsClass.WaitFor(2);
					if (ForestTreeGO != null)
					{
						//create the exclude list
						NativeArray<CoreData> excludes = new NativeArray<CoreData>(1, Allocator.TempJob);
						excludes[0] = _cd;
						excludes.Dispose();

						Entity e = GenerateEntity(entityManager, 0, ForestTreeGO);
							NativeArray<Entity> generatedEntities = CoreFunctionsClass.SpawnEntitiesWithinBounds(entityManager, e, new Bounds(trans.Value, _cd.size), 30, excludes, new float[0, 0], true);
							entityManager.AddComponentData(e, new DestroyMe { });

							generatedEntities.Dispose();
					}
					else Debug.LogError("Failed to successfully load ForestTree");
				//	GameObject.Destroy(ForestTreeGO);
					

					break;
				default: Debug.LogError("cannot create enviroment with name \"" + enviroment + "\""); break;
			}
		}
		public static AudioSharedData GetEnviromentSoundInfo(string region, string EnviromentName)
		{
			switch (EnviromentName)
			{
				case "Forest":
					try
					{
						GameObject go = Resources.Load("Enviroment/" + region + "/Forest/ForestEnviroment") as GameObject;
						AudioSource a = go.GetComponent<AudioSource>();
						AudioClip b = Resources.Load("Enviroment/" + region + "/Forest/Sounds/KantoForestStart.mp3") as AudioClip;
						AudioClip c = Resources.Load("Enviroment/" + region + "/Forest/Sounds/KantoForestStart.mp3") as AudioClip;
						return new AudioSharedData
						{
							isValid = true,
							musicStart = c,
							music = a,
							musicLoop = b,
							ambientSounds = a,
							playOnStart = true
							//currently missing ambient music
						};
					}
					catch
					{
						Debug.LogWarning("Failed to get sound info for forest");
					}
					break;
				default: Debug.LogWarning("failed to get \"" + EnviromentName + "\"'s SoundData"); break;
			}
			return new AudioSharedData { };
		}

		public static void SetEnviromentEntityData(EntityManager entityManager, GameObject go, NativeArray<Entity> entities,
			NativeArray<Bounds> entitiesToAviod, bool disposeOnCompletion, string region, string name, float3 entitySize, float3 center, float3 bounds)
		{
			Unity.Mathematics.Random rand = new Unity.Mathematics.Random(0x6E624EB7u);
			float3 newPosition;
			Debug.Log("entitySize = " + entitySize);
			for (int i = 0; i < entities.Length; i++)
			{
				switch (name)
				{
					case "Tree":
						center.y = 0;                   //	bounds -= entitySize;
						bool overlap = false;
						float3 scale = go.GetComponent<Transform>().localScale;
						Bounds _bounds = go.GetComponent<MeshFilter>().sharedMesh.bounds;
						Debug.Log("Tree stuff = " + scale.ToString() + "," + _bounds.ToString() + "," + (_bounds.size * scale));
						int attempt = 0;
						while (true)
						{
							newPosition = center + rand.NextFloat3(-bounds / 2, bounds / 2);
							newPosition.y = -0.1f; //spawning on flat land for now
							for (int j = 0; j < i; j++)
							{
								float3 entityPosition = entityManager.GetComponentData<Translation>(entities[j]).Value;
								//Note that this treats the coreData position and size as cube which may not be tree for all entities (we just want the AABBs)
								Bounds a = new Bounds(newPosition, _bounds.size * scale);
								if (!a.Intersects(new Bounds(entityPosition, _bounds.size * scale)))
								{
									if (entitiesToAviod.Length > 0 && !entitiesToAviod[0].size.Equals(new float3(-1f, -1f, -1f)))
									{
										for (int k = 0; k < entitiesToAviod.Length; k++)
										{
											if (a.Intersects(entitiesToAviod[k]))
											{
												Debug.LogWarning("AAA");
												overlap = true;
												break;
											}
										}
									}
									else Debug.LogWarning("No entities to compare");
								}
								else
								{
									Debug.LogWarning("B");
									overlap = true;
									break;
								}

							}
							if (!overlap) break;
							else attempt++;
							if (attempt == 50)
							{
								Debug.LogWarning("Failed to create entity " + i + " for Tree");

								break;
							}
							else overlap = false;
						}
						if (!overlap)
						{
							entityManager.SetComponentData(entities[i], new Translation { Value = newPosition });
							entityManager.SetComponentData(entities[i], new Scale { Value = scale.x });
							entityManager.SetSharedComponentData(entities[i], new RenderMesh
							{
								mesh = go.GetComponent<MeshFilter>().sharedMesh,
								castShadows = UnityEngine.Rendering.ShadowCastingMode.On,
								material = go.GetComponent<MeshRenderer>().sharedMaterial,
								receiveShadows = true
							});
							entityManager.SetComponentData(entities[i], GetEnviromentPhysicsCollider(region + "|" + name, scale.x));
							entityManager.SetName(entities[i], region + "|" + name + i);
							entityManager.SetComponentData(entities[i], new CoreData(new ByteString30(name), new ByteString30(region), bounds, scale));
						}

						break;
					default:
						Debug.LogWarning("Failed to set data for entity with name \"" + region + "|" + name + "\"");
						break;
				}
			}
			if (disposeOnCompletion) entitiesToAviod.Dispose();
		}

		public static void GenerateFloraAroundEntities(EntityManager entityManager, NativeArray<Entity> spawnAround, float3 spawnAroundSize, string floraPath, string floraName, int floraPerEntity, int shape)
		{
			GameObject go = Resources.Load(floraPath) as GameObject;
			if (go != null)
			{
				go = GameObject.Instantiate(go);
				NativeArray<Entity> entities = new NativeArray<Entity>(spawnAround.Length * floraPerEntity, Allocator.Temp);
				entityManager.CreateEntity(GetEnviromentArchtype(entityManager, floraName), entities);
				for (int i = 0; i < spawnAround.Length; i++)
				{
					float3 position = entityManager.GetComponentData<Translation>(spawnAround[i]).Value;
					CoreData coreData = entityManager.GetComponentData<CoreData>(spawnAround[i]);
					for (int j = i * floraPerEntity; j < floraPerEntity; j++)
					{
						switch (shape)
						{
							case 0://box

								break;
						}

					}
					//	entityManager.SetComponentData()
				}
				GameObject.Destroy(go);
				entities.Dispose();
			}
			else
			{
				Debug.LogWarning("Failed to get flora gameobject using path \"" + floraPath + "\"");
			}
		}

		public static EntityArchetype GetEnviromentArchtype(EntityManager entityManager, string name)
		{
			EntityArchetype ea = new EntityArchetype { };
			switch (name)
			{
				case "Tree":
					ea = entityManager.CreateArchetype(
						typeof(Translation),
						typeof(Rotation),
						typeof(Scale),
						typeof(RenderMesh),
						typeof(LocalToWorld),
						typeof(PhysicsCollider),
						typeof(CoreData)
						//	typeof(PhysicsMass)
						);
					break;
				case "BushA":
					ea = entityManager.CreateArchetype(
						typeof(Translation),
						typeof(Rotation),
						typeof(Scale),
						typeof(CoreData),
						typeof(RenderMesh),
						typeof(LocalToWorld));
					break;
				default: Debug.LogError("Failed to find a name that matches \"" + name + "\""); break;
			}
			return ea;
		}

		public static PhysicsCollider GetEnviromentPhysicsCollider(string name, float scale = 1f)
		{
			PhysicsCollider pc = new PhysicsCollider { };
			switch (name)
			{
				case "Kanto|Tree":
					NativeArray<CompoundCollider.ColliderBlobInstance> cs = new NativeArray<CompoundCollider.ColliderBlobInstance>(2, Allocator.TempJob);
					cs[0] = new CompoundCollider.ColliderBlobInstance
					{

						Collider = CylinderCollider.Create(new CylinderGeometry
						{
							Orientation = Quaternion.Euler(new float3(270f, 270f, 0)),
							Height = 1f * scale,
							Radius = .25f * scale,
							SideCount = 20,
							Center = new float3(0, .5f, 0) * scale
						}, new CollisionFilter
						{
							BelongsTo = TriggerEventClass.Collidable,
							CollidesWith = TriggerEventClass.Collidable,
							GroupIndex = 1
						}),
						CompoundFromChild = new RigidTransform { rot = quaternion.identity }
					};
					cs[1] = new CompoundCollider.ColliderBlobInstance
					{

						Collider = CylinderCollider.Create(new CylinderGeometry
						{
							Center = new float3(0, 1.7f, 0) * scale,
							Orientation = Quaternion.Euler(new float3(90f, 0, 0)),
							Height = 1.5f * scale,
							Radius = 1f * scale,
							SideCount = 20
						}, new CollisionFilter
						{
							BelongsTo = TriggerEventClass.Collidable,
							CollidesWith = TriggerEventClass.Collidable,
							GroupIndex = 1
						}),
						CompoundFromChild = new RigidTransform { rot = quaternion.identity }
					};
					pc = new PhysicsCollider { Value = CompoundCollider.Create(cs) };
					cs.Dispose();
					break;
				default:
					Debug.LogWarning("Failed to get collider for \"" + name + "\"");
					break;
			}
			return pc;
		}

		public static Entity GenerateEntity(EntityManager entityManager, int entity, GameObject go = null)
		{
			Entity e = new Entity { };
			EntityArchetype ea;
			switch (entity)
			{
				case 0: //Tree
					e = entityManager.CreateEntity(new ComponentType[] {
						typeof(Translation),
						typeof(LocalToWorld),
						typeof(Rotation),
						typeof(RenderMesh),
						typeof(CoreData)
					});
					if (go != null)
					{
						MeshRenderer mr = go.GetComponent<MeshRenderer>();
						MeshFilter mf = go.GetComponent<MeshFilter>();
						entityManager.SetSharedComponentData(e, new RenderMesh
						{
							mesh = mf.sharedMesh,
							material = mr.sharedMaterial,
							receiveShadows = true,
							castShadows = UnityEngine.Rendering.ShadowCastingMode.On
						});
						entityManager.SetComponentData(e, new CoreData
						{
							Name = new ByteString30("Tree"),
							isValid = true,
							scale = 1,
							size = mf.sharedMesh.bounds.size
						});

						entityManager.AddComponentData(e, new NonUniformScale { Value = go.GetComponent<Transform>().localScale });
					}
					else
					{
						entityManager.AddComponentData(e, new Scale { Value = 1f });
					}
					entityManager.SetComponentData(e, new Rotation { Value = quaternion.identity});
				
					break;
				default:
					Debug.LogError("Failed to get entity " + entity);
					break;
			}
			return e;

		}

		/*
			ok so what does a forest have?
			Trees
			bushes
			flowers
			rocks
			Pokemon?
				Grass, Flying, Bug, special pokemon (like pikachu), and forest based pokemon

			forest components?
			Music,
			ambient sound (maybe some chirping or something),
			some  trigger collider to change music and stuff
		*/
		public struct LocationData : IComponentData
		{
			public CoreData locationCoreData; //holds forest "ui" name and base name (forest)
			public ByteString30 region;
			public float3 position;
		}
	}
}