using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Pokemon;
namespace Core
{
	namespace Particles
	{
		/// <summary>
		/// this creates and manages PArticleSystems as well as giving and removing them from entities 
		/// </summary>
		class ParticleEntitySystem : JobComponentSystem
		{
			private EntityQuery particleSystemRemoveRequestQuery;
			private EntityQuery particleSystemRequestQuery;
			private EntityQuery particleSystemEntityQuery;

			private int i = 0, j = 0;

			private GameObject particlePrefab = null;
			private bool haveParticlePrefab = false,requiresNewParticleSystems = false;
			private GameObject createParticleSystemGameObject(string name, float3 translation, float4 rotation)
			{
				GameObject a = particlePrefab;
				a.name = name;
				Debug.Log("creating "+a.name);
				//not going to use the parent transform for now
				return GameObject.Instantiate(a, translation, (quaternion)rotation);
			}
			protected override void OnCreate()
			{
				particleSystemRemoveRequestQuery = GetEntityQuery(typeof(ParticleSystemRemoveRequest),typeof(ParticleSystemData));
				particleSystemRequestQuery = GetEntityQuery(typeof(ParticleSystemRequest));
				particleSystemEntityQuery = GetEntityQuery(typeof(ParticleSystemEntity));
				//load the prefab
				particlePrefab = Resources.Load("Core/ParticleSystem/ParticleSystem") as GameObject;
				if (particlePrefab == null) Debug.LogError("ParticleEntitySystem: Failed to load particle prefab");
				else
				{
					Debug.Log("Loaded Particle Entity System Prefab Successfully");
					haveParticlePrefab = true;
					for (i = 0; i < 1; i++)
					{
						GameObject go = createParticleSystemGameObject("particleSystem" + i, new float3(), new float4());
					}
				}
			}
			/// <summary>
			/// filters out the active (in use) ParticleSystems
			/// </summary>
			protected override JobHandle OnUpdate(JobHandle inputDeps)
			{
				i = particleSystemRequestQuery.CalculateEntityCount();
				if (i == 0 && particleSystemRemoveRequestQuery.CalculateEntityCount() == 0) return inputDeps;
				NativeArray<Entity> particleSystemEntityEntities = particleSystemEntityQuery.ToEntityArray(Allocator.TempJob);
				NativeArray<ParticleSystemEntity> pses = particleSystemEntityQuery.ToComponentDataArray<ParticleSystemEntity>(Allocator.TempJob);
				
				//test if any one needs new particles
				if (i > 0) {
					if (haveParticlePrefab)
					{
						NativeArray<Entity> particleSystemRequestEntities = particleSystemRequestQuery.ToEntityArray(Allocator.TempJob);
						j = getActiveParticleSystems(pses);
						requiresNewParticleSystems = j + particleSystemRequestEntities.Length > particleSystemEntityEntities.Length;
						Debug.Log("detected "+j+" active particleSystems. current max: " + particleSystemEntityEntities.Length + " current requested: "+ particleSystemRequestEntities .Length+ " require new spawns = "+requiresNewParticleSystems);
						//test to see if we need to make new particle systems
						if (requiresNewParticleSystems)
						{
				//			Debug.Log("Detected a request in new entities!");
							for(i = 0; i < j + particleSystemRequestEntities.Length - particleSystemEntityEntities.Length; i++)
								createParticleSystemGameObject("ParticleSystem" + (i+ particleSystemEntityEntities.Length), new float3(), new float4());
							particleSystemEntityEntities.Dispose();
							pses.Dispose();
							particleSystemEntityEntities = particleSystemEntityQuery.ToEntityArray(Allocator.TempJob);
							pses = particleSystemEntityQuery.ToComponentDataArray<ParticleSystemEntity>(Allocator.TempJob);
							
						}
						Debug.Log("Filling in Requests...");
						//now we fill the requests
						for (i = 0; i < particleSystemRequestEntities.Length;  i++)
						{
							for (j = 0; j < pses.Length;j++) {
								if (!pses[j].isActive)
								{
									pses[j] = new ParticleSystemEntity
									{
										id = pses[j].id,
										isActive = true
									};
									Debug.Log("setting particle system "+pses[j].id+" to a entity");
									//assumes the eneity already has the ParticleSystemData
									EntityManager.RemoveComponent<ParticleSystemRequest>(particleSystemRequestEntities[i]);
									ParticleSystem ps = EntityManager.GetComponentObject<ParticleSystem>(particleSystemEntityEntities[j]) as ParticleSystem;
									if (ps == null) Debug.LogError("FAILED TO GET PARTICLE SYSTEM");
						//			else Debug.Log("particle position = "+ps.transform.position);
									if(EntityManager.HasComponent<ParticleSystemSpawnData>(particleSystemRequestEntities[i]))
									{
										ParticleSystemSpawnData pssd = EntityManager.GetComponentData<ParticleSystemSpawnData>(particleSystemRequestEntities[i]);
										if (pssd.paticleSystemName.A != 0)
										{
											GameObject temp = Resources.Load("Pokemon/PokemonMoves/" + (pssd.paticleSystemName)+"/"+ (pssd.paticleSystemName)+"ParticleSystem") as GameObject;
											if (temp != null) ps = temp.GetComponent<ParticleSystem>();
											else Debug.LogWarning("Failed to load ParticleSystem Preset: invalid ParticleSystem, got \"Pokemon/PokemonMoves/" + (pssd.paticleSystemName) + "ParticleSystem\"");
										}
										else Debug.LogWarning("Failed to load ParticleSystem Preset: invalid Name");
										if (pssd.particleSystemSpawnDataShape.isValid)
										{
											var shape = ps.shape;
											shape.position = pssd.particleSystemSpawnDataShape.offsetPostiion;
											shape.rotation = pssd.particleSystemSpawnDataShape.offsetRotation;
											shape.scale = pssd.particleSystemSpawnDataShape.offsetScale;
										}
										
									}
									else
									{
										Debug.LogWarning("No ParticleSystemDataSpawnData found, setting some defaults."+EntityManager.GetName(particleSystemRequestEntities[i]));
										var shape = ps.shape;
										shape.position = new float3();
										shape.rotation = new float3();
										shape.scale = new float3(1f,1f,1f);
									}
									ps.Play();
									EntityManager.AddComponentObject(particleSystemRequestEntities[i], ps);
									EntityManager.SetComponentData(particleSystemRequestEntities[i],
										new ParticleSystemData
										{
											isFinished = false,
											particleSystemEntity = pses[j]
										}
									);

									EntityManager.SetComponentData<ParticleSystemEntity>(particleSystemEntityEntities[j], pses[j]);
									break;
								}
							}
						}
						particleSystemRequestEntities.Dispose();
					}
					else Debug.LogError("Cannot create new prefabs becuase we failed to laod thr original");
				}
				//test for any removal request
				if (particleSystemRemoveRequestQuery.CalculateEntityCount() > 0)
				{
	//				Debug.Log("Detected "+ particleSystemRemoveRequestQuery.CalculateEntityCount()+" remove requests");
					//now we test if there are any particlesystems that are finished being used
					NativeArray<Entity> psds = particleSystemRemoveRequestQuery.ToEntityArray(Allocator.TempJob);
					for (i = 0; i < psds.Length; i++)
					{
						ParticleSystemData psd = EntityManager.GetComponentData<ParticleSystemData>(psds[i]);
						for (j = 0; j < pses.Length; j++)
						{
			//				Debug.Log("testing "+pses[j].id +" with "+ psd.particleSystemEntity.id);
							if (pses[j].id == psd.particleSystemEntity.id)
							{
			//					Debug.Log("Preforming remove request on id " + pses[j].id);
								EntityManager.SetComponentData(psds[i], new ParticleSystemData { });
								EntityManager.RemoveComponent<ParticleSystemRemoveRequest>(psds[i]);
								EntityManager.SetComponentData(particleSystemEntityEntities[j], new ParticleSystemEntity
								{
									id = pses[j].id,
									isActive = false
								});
								break;
							}
						}
					}
					psds.Dispose();
					
				}
				pses.Dispose();
				particleSystemEntityEntities.Dispose();
				return inputDeps;

			}
			/// <summary>
			/// returns the total amount of active particle systems
			/// </summary>
			/// <returns>int of active particle systems</returns>
			private int getActiveParticleSystems(NativeArray<ParticleSystemEntity> pses)
			{
				int jj = 0;
				for (i = 0; i < pses.Length; i++)
					if (pses[i].isActive) jj++;
				return jj;
			}
		
		}

		class ParticleDataMovementSystem : JobComponentSystem
		{
			private EntityQuery entityQuery;
			private NativeArray<Entity> particleEntities;
			private NativeArray<Translation> translations;
			private int i;
			protected override void OnCreate()
			{
				entityQuery = GetEntityQuery(typeof(ParticleSystemData),typeof(Translation));
			}
			protected override JobHandle OnUpdate(JobHandle inputDeps)
			{
				if (entityQuery.CalculateEntityCount() == 0) return inputDeps;
				particleEntities = entityQuery.ToEntityArray(Allocator.TempJob);
				translations = entityQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
			//	Debug.Log("Got "+translations.Length+" TRANSFORM entities");
				for(i = 0; i < particleEntities.Length; i++)
				{
					if (EntityManager.HasComponent<ParticleSystem>(particleEntities[i]))
					{
						ParticleSystem ps = EntityManager.GetComponentObject<ParticleSystem>(particleEntities[i]);
				//		Debug.Log("ps  ==== = " + ps.transform.position);
						ps.transform.position = translations[i].Value;
					}
				}
				translations.Dispose();
				particleEntities.Dispose();
				return inputDeps;
			}
		}
	}
}