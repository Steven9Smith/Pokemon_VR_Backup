using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System;
using Pokemon;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.VFX;

namespace Core
{
	namespace Particles
	{
		[RequiresEntityConversion]
		public class ParticleEntityComponentData : MonoBehaviour, IConvertGameObjectToEntity
		{
			public GameObject go;
			public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
			{
				go.name = go.name.Replace("(Clone)", "");
				int id = Int32.Parse(go.name[go.name.Length - 1].ToString());
				dstManager.AddComponentData(entity, new ParticleSystemEntity
				{
					id = id,
					isActive = false
				}) ;
				dstManager.SetName(entity,go.name);
			}
		}

		public struct ParticleSystemEntity : IComponentData
		{	
			public BlittableBool isActive;
			public int id;
			public static bool operator ==(ParticleSystemEntity c1, ParticleSystemEntity c2)
			{
				return c1.id == c2.id;
			}

			public static bool operator !=(ParticleSystemEntity c1, ParticleSystemEntity c2)
			{
				return c1.id != c2.id;
			}
		}
		public struct ParticleSystemData : IComponentData
		{
			public ParticleSystemEntity particleSystemEntity;
			public BlittableBool isFinished;
		}
		public struct ParticleSystemRequest : IComponentData{}
		public struct ParticleSystemRemoveRequest: IComponentData{}
		public struct ParticleSystemSpawnData: IComponentData
		{
			public BlittableBool isValid;
			public ParticleSystemSpawnDataShape particleSystemSpawnDataShape;
			public ByteString30 paticleSystemName;
		}
		public struct ParticleSystemSpawnDataShape : IComponentData
		{
			public BlittableBool isValid;
			public float3 offsetPostiion;
			public float3 offsetRotation;
			public float3 offsetScale;
		}
		namespace VFXGraph
		{
			public class VFXGParticlesClass
			{
				public void AddVFXGToPokemonMove(EntityManager entityManager,Entity entity,string pokemonMoveName,Vector3 position,Quaternion rotation)
				{
					GameObject go = null;
					switch (pokemonMoveName)
					{
						case "ThunderBolt":
							go = Resources.Load("Pokemon/PokemonMoves/"+pokemonMoveName+"/"+pokemonMoveName+"VFX") as GameObject;
							break;
						default:
							Debug.LogWarning("Failed to find a VFX Graph for the pokemon move \""+pokemonMoveName+"\"");
							break;
					}
					if (go != null)
					{
						go.name = pokemonMoveName + "VFX";
						GameObject.Instantiate(go, position, rotation);							
					} else Debug.LogWarning("Failed to locate the PokemonMove VFX Object");
				}
			}
			class VFXGParticleSystem : JobComponentSystem
			{
				private EntityQuery VFXGParticleRequestQuery;
				private EntityQuery VFXGParticleEntityQuery;
				private EntityQuery VFXGParticleRemoveRequestQuery;
				private int i,j;
				private GameObject VFXGParticlePrefab;
				protected override void OnCreate()
				{
					VFXGParticleRemoveRequestQuery = GetEntityQuery(typeof(VFXGParticleRemoveRequest), typeof(VFXGParticleSystemData));
					VFXGParticleRequestQuery = GetEntityQuery(typeof(VFXGParticleRequest));
					VFXGParticleEntityQuery = GetEntityQuery(typeof(VFXGParticleEntity));
				}
				/// <summary>
				/// gets the amount of active VFXG Particle Entities
				/// </summary>
				/// <param name="particleEntities">native Array that holds all the VFXG Particles</param>
				/// <returns></returns>
				private int GetActiveVFXGParticleCount(NativeArray<VFXGParticleEntity> particleEntities)
				{
					int jj = 0;
					for (i = 0; i < particleEntities.Length; i++) if (particleEntities[i].isActive) jj++;
					return jj;
				}
				private GameObject CreateVFXGParticle(string name,float3 translation,quaternion rotation)
				{
					GameObject a = Resources.Load("Core/ParticleSystem/VFXGraphParticle") as GameObject;
					a.name = name;
					Debug.Log("Creating "+name);
					return GameObject.Instantiate(a, translation, rotation);
				}
				protected override JobHandle OnUpdate(JobHandle inputDeps)
				{
					i = VFXGParticleRequestQuery.CalculateEntityCount();
					if (i == 0 && VFXGParticleRemoveRequestQuery.CalculateEntityCount() == 0) return inputDeps;
					//get the current particle system entities/gameobject
					NativeArray<Entity> VFXGParticleEntities = VFXGParticleEntityQuery.ToEntityArray(Allocator.TempJob);
					NativeArray<VFXGParticleEntity> particleEntities = VFXGParticleEntityQuery.ToComponentDataArray<VFXGParticleEntity>(Allocator.TempJob);
					//test for new particle requests
					if(i > 0)
					{
						NativeArray<Entity> requestEntities = VFXGParticleRequestQuery.ToEntityArray(Allocator.TempJob);
						//get the currently active/used VFXGParticles
						j = GetActiveVFXGParticleCount(particleEntities);
						//test if the request exceed the current amount of VFXG Particles
						bool requireNewParticles = j + requestEntities.Length > VFXGParticleEntities.Length;
						Debug.Log("detected " + j + " active VFXG Particles. current max: " + VFXGParticleEntities.Length + " current requested: " +requestEntities.Length + " require new spawns = " + requireNewParticles);
						if (requireNewParticles)
						{
							//create needed amount of particles
							for (i = 0; i < j + requestEntities.Length - VFXGParticleEntities.Length; i++)
								CreateVFXGParticle("VFXGParticle" + (i + VFXGParticleEntities.Length), new float3(), new quaternion());
							//"refresh" the current VFXG Particles
							VFXGParticleEntities.Dispose();
							VFXGParticleEntities = VFXGParticleEntityQuery.ToEntityArray(Allocator.TempJob);
							particleEntities.Dispose();
							particleEntities = VFXGParticleEntityQuery.ToComponentDataArray<VFXGParticleEntity>(Allocator.TempJob);
						}
						Debug.Log("VFXG: Filling in requests");
						//now we fill in the requests
						for(i = 0; i < requestEntities.Length; i++)
						{
							for(j = 0; j < particleEntities.Length; j++)
							{
								if (!particleEntities[j].isActive)
								{
									particleEntities[j] = new VFXGParticleEntity
									{
										isActive = true,
										id = particleEntities[j].id
									};
									Debug.Log("setting particle system "+particleEntities[j].id+" to a entity");
									//remove the request
									EntityManager.RemoveComponent<VFXGParticleRequest>(requestEntities[j]);
									VisualEffect vf = EntityManager.GetComponentObject<VisualEffect>(VFXGParticleEntities[j]) as VisualEffect;
									//assume the gameobject has a Visual Effect
									if (EntityManager.HasComponent<VFXGParticleSystemData>(requestEntities[i]))
									{
										//load the particleSystem 
										VFXGParticleSystemData vfxgData = EntityManager.GetComponentData<VFXGParticleSystemData>(requestEntities[i]);
										if (vfxgData.Name.A != 0)
										{
											GameObject temp = Resources.Load("Core/ParticleSystem/VFXGParticles/"+(vfxgData.Name)+"/"+ (vfxgData.Name)) as GameObject;
											if (temp != null) vf = temp.GetComponent<VisualEffect>();
											else Debug.LogError("Failed to get GamObject for particle");
											vf.Play();
											EntityManager.AddComponentObject(requestEntities[i], vf);
											//EntityManager.SetComponentData(requestEntities[i],new ParticleSystemData{ });
											EntityManager.SetComponentData(VFXGParticleEntities[j], particleEntities[j]);
										}
										else Debug.LogError("Cannot found VFXG Particle with the name "+(vfxgData.Name));
									}
									break;
								}
							}
						}
						//Clean Up
						requestEntities.Dispose();
					}
					if(VFXGParticleRemoveRequestQuery.CalculateEntityCount() > 0)
					{
						Debug.Log("Detected Remove request");
						NativeArray<Entity> removeEntities = VFXGParticleRemoveRequestQuery.ToEntityArray(Allocator.TempJob);
						for(i = 0; i < removeEntities.Length; i++)
						{
							VFXGParticleEntity psd = EntityManager.GetComponentData<VFXGParticleEntity>(removeEntities[i]);
							for(j = 0; j < VFXGParticleEntities.Length; j++)
							{
								if(particleEntities[j].id == psd.id)
								EntityManager.SetComponentData(removeEntities[i], new VFXGParticleEntity { });
								EntityManager.RemoveComponent<VFXGParticleRemoveRequest>(removeEntities[i]);
								EntityManager.SetComponentData(VFXGParticleEntities[j], new VFXGParticleEntity {
									id = particleEntities[j].id,
									isActive = false
								});
							}
						}
						removeEntities.Dispose();
					}
					particleEntities.Dispose();
					VFXGParticleEntities.Dispose();
					return inputDeps;
				}
			}

			public struct VFXGParticleSystemData : IComponentData
			{
				public ByteString30 Name;
				public Entity VFXGParticleEntity;
			}
			public struct VFXGParticleRequest : IComponentData
			{

			}
			public struct VFXGParticleRemoveRequest : IComponentData
			{

			}
			public struct VFXGParticleEntity : IComponentData
			{
				public BlittableBool isActive;
				public int id;
			}

			class VFXGParticleMoveSystem : JobComponentSystem
			{
				private EntityQuery entityQuery;
				private NativeArray<Entity> particleEntities;
				private NativeArray<Translation> translations;
				private int i;
				protected override void OnCreate()
				{
					entityQuery = GetEntityQuery(typeof(VFXGParticleSystemData), typeof(Translation));
				}
				protected override JobHandle OnUpdate(JobHandle inputDeps)
				{
					if (entityQuery.CalculateEntityCount() == 0) return inputDeps;
					particleEntities = entityQuery.ToEntityArray(Allocator.TempJob);
					translations = entityQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
					//	Debug.Log("Got "+translations.Length+" TRANSFORM entities");
					for (i = 0; i < particleEntities.Length; i++)
					{
						if (EntityManager.HasComponent<VisualEffect>(particleEntities[i]))
						{
							VisualEffect visualEffect = EntityManager.GetComponentObject<VisualEffect>(particleEntities[i]);
							//		Debug.Log("ps  ==== = " + ps.transform.position);
							visualEffect.transform.position = translations[i].Value;
						}
					}
					translations.Dispose();
					particleEntities.Dispose();
					return inputDeps;
				}
			}
		}
	}
}
