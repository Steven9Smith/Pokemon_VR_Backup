using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace Pokemon
{
	namespace Move
	{
		/// This holds four pokemon Moves
		/// </summary>
		[Serializable]
		public struct PokemonMoveSet : IComponentData
		{
			public PokemonMove pokemonMoveA;
			public PokemonMove pokemonMoveB;
			public PokemonMove pokemonMoveC;
			public PokemonMove pokemonMoveD;
			
		}
		[Serializable]
		public struct PokemonMove : IComponentData
		{
			public ByteString30 name;
			public BlittableBool isValid;
			public BlittableBool followEntity;
			public int index; //used for spawning moves
		}

		/// <summary>
		/// Pokemon Move 
		/// </summary>
		[Serializable]
		public struct PokemonMoveData : IComponentData
		{
			public ByteString30 name;             //name of pokemon move
												  //Attack Type Varibles
			public PokemonMoves.AttackType attackType;         //substance of the type e.g. electric, rock,ground
			public PokemonMoves.StatusType statusType;         //e.g. sleep,paralisys
			public PokemonMoves.ContactType contactType;
			//	public ushort contactType;        //type of move e.g. physical, sspecial attack
															   //Damage Varibles
			public float damage;          //damage of attack
											  //PP varibles
			public float accuracy;

			public BlittableBool isValid;
			//executing stuff
			public float3 forward;
			public PokemonMoveAdjustmentData pokemonMoveAdjustmentData;
		}
		//this allows for custom stuff. here we go again
		[Serializable]
		public struct PokemonMoveAdjustmentData : IComponentData
		{
			public BlittableBool isValid;
			public PokemonMoveTranslationSet pokemonMoveTranslationSet;
			public PokemonMoveRotationSet pokemonMoveRotationSet;
			public PokemonMoveVelocitySet pokemonMoveVelocitySet;
			public PokemonMoveAngularVelocitySet pokemonMoveAngularVelocitySet;
		}
		//used for multiple things...im too tired to explain
		public struct PokemonMoveAdjustment : IComponentData
		{
			public float3 value;                //how much to adjust 
			public float timeLength;            //how long the affect lasts
			public BlittableBool useCameraDirection;
			public float staminaCost;       //how much stamina does this move cost
			public float3 colliderBounds;
		}
		[Serializable]
		public struct PokemonMoveAdjustmentSet : IComponentData {
			public BlittableBool isValid;
			public PokemonMoveAdjustment A;
			public PokemonMoveAdjustment B;
			public PokemonMoveAdjustment C;
			public PokemonMoveAdjustment D;
			public PokemonMoveAdjustment E;
			public PokemonMoveAdjustment F;
			public PokemonMoveAdjustment G;
			public PokemonMoveAdjustment H;
			public PokemonMoveAdjustment I;
			public PokemonMoveAdjustment J;
			public PokemonMoveAdjustment K;
			public PokemonMoveAdjustment L;
			public PokemonMoveAdjustment M;
			public PokemonMoveAdjustment N;
			public PokemonMoveAdjustment O;
			public PokemonMoveAdjustment P;
			public PokemonMoveAdjustment Q;
			public PokemonMoveAdjustment R;
			public PokemonMoveAdjustment S;
			public PokemonMoveAdjustment T;
			public PokemonMoveAdjustment U;
			public PokemonMoveAdjustment V;
			public PokemonMoveAdjustment W;
			public PokemonMoveAdjustment X;
			public PokemonMoveAdjustment Y;
		}
		[Serializable]
		//allows for up to 30 position adjustments
		/// <summary>
		/// a set of translation adjustments
		/// </summary>
		public struct PokemonMoveTranslationSet : IComponentData{
			public PokemonMoveAdjustmentSet value;
		}
		[Serializable]
		//allows for up to 30 Rotation adjustments
		/// <summary>
		/// a set of rotation adjustments
		/// </summary>
		public struct PokemonMoveRotationSet : IComponentData
		{
			public PokemonMoveAdjustmentSet value;
			public float w; //<- i dont care atm
		}
		[Serializable]
		/// <summary>
		/// a set of linear velocity adjustments
		/// </summary>
		public struct PokemonMoveVelocitySet : IComponentData
		{
			public PokemonMoveAdjustmentSet value;
		}
		[Serializable]
		/// <summary>
		/// a set of angular velocity adjustments
		/// </summary>
		public struct PokemonMoveAngularVelocitySet : IComponentData
		{
			public PokemonMoveAdjustmentSet value;
		}
		public struct TranslationOffsetData : IComponentData
		{
			public float3 value;
			public BlittableBool getFromEntity;
		}
		public struct RotationOffsetData : IComponentData
		{
			public quaternion value;
			public BlittableBool getFromEntity;
			public BlittableBool getCameraForward;
		}
		/// <summary>
		/// spawn data
		/// </summary>
		[Serializable]
		public struct PokemonMoveDataSpawn : IComponentData
		{
			
			public TranslationOffsetData TranslationOffset;
			public RotationOffsetData RotationOffset;
		}
		public struct PokemonMoveDataEntity : IComponentData{
			public ByteString30 name;
			public PokemonMoves.AttackType attackType;
			public PokemonMoves.StatusType statusType;
			public PokemonMoves.ContactType contactType;
			public BlittableBool isValid;
			public PokemonMoveAdjustmentData pokemonMoveAdjustmentData;
			public PokemonMoveParticleDataSet pokemonMoveParticleDataSet;
			public float damage;
			public float accuracy;
			public float3 forward;
		}

		public struct PokemonMoveParticleData
		{
			public float3 velocity;
		}
		public struct PokemonMoveParticleDataSet
		{
			public BlittableBool isValid;
			public PokemonMoveParticleData A;
			public PokemonMoveParticleData B;
			public PokemonMoveParticleData C;
			public PokemonMoveParticleData D;
			public PokemonMoveParticleData E;
			public PokemonMoveParticleData F;
			public PokemonMoveParticleData G;
			public PokemonMoveParticleData H;
			public PokemonMoveParticleData I;
			public PokemonMoveParticleData j;
			public PokemonMoveParticleData K;
			public PokemonMoveParticleData L;
			public PokemonMoveParticleData M;
			public PokemonMoveParticleData N;
			public PokemonMoveParticleData O;
			public PokemonMoveParticleData P;
			public PokemonMoveParticleData Q;
			public PokemonMoveParticleData R;
			public PokemonMoveParticleData S;
			public PokemonMoveParticleData T;
			public PokemonMoveParticleData U;
			public PokemonMoveParticleData V;
			public PokemonMoveParticleData W;
			public PokemonMoveParticleData X;
			public PokemonMoveParticleData Y;
		}

		public class PokemonMoves
		{
			public enum ContactType
			{
				physical = 0,
				phychich = 1,
				emotional = 2,
				philisophical = 3
			}
			public enum AttackType{
				normal = 0,
				phychic = 1,
				electric = 2,
				rock = 3,
				ground = 4,
				water = 5,
				fire = 6,
				grass = 7,
				flying = 8,
				poison = 9,
				bug = 10,
				ghost = 11,
				steel = 12,
				ice = 13,
				dragon = 14,
				dark = 15,
				dairy = 16
			}
			public enum StatusType
			{
				none = 0,
				freeze = 1,
				paralysis = 2,
				poison = 3,
				badlyPoisoned = 4,
				sleep = 5,
				burn = 6
				//add more statuses
			}
	
			public static PokemonMoveDataSpawn getPokemonMoveDataSpawn(ByteString30 name,PokemonEntityData ped)
			{
				PokemonMoveDataSpawn pmds = new PokemonMoveDataSpawn { };
				switch (PokemonIO.ByteString30ToString(name))
				{
					case "ThunderShock":
						pmds = new PokemonMoveDataSpawn
						{
						
							TranslationOffset = new TranslationOffsetData {
								value = new float3 { x = 0, y = 0, z = 0 },
								getFromEntity = true
							},
							RotationOffset = new RotationOffsetData
							{
								value = new float4 { x = 0, y = 0, z = 0,w = 0 },
								getFromEntity = true,
								getCameraForward = true
							}
							

						};
						break;
					default:
						Debug.Log("failed to find a matching pokemon move data spawn");
						break;
				}
				return pmds;
			}
			public static PokemonMoveDataEntity GetPokemonMoveDataEntity(ByteString30 name,PokemonEntityData ped)
			{
				PokemonMoveDataEntity pmde = new PokemonMoveDataEntity { };
				switch (PokemonIO.ByteString30ToString(name))
				{
					case "Tackle":
						pokemonMove = new PokemonMoveData
						{
							name = name,
							accuracy = 1f,
							attackType = AttackType.normal,
							damage = calculateDamage(ped.currentLevel, 40f),
							statusType = StatusType.none,
							contactType = ContactType.physical,
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
											value = new float3 { x = 5f, y = 1f, z = 5f },
											timeLength = -1f,
											useCameraDirection = true,
											staminaCost = 5f
										},
										isValid = true
									}
								}
							}
						};
						break;
					case "ThunderShock" :
						pmde = new PokemonMoveDataEntity
						{
							attackType = AttackType.electric,
							contactType = ContactType.phychich,
							name = name,
							isValid = true,
							statusType = StatusType.none,
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
											colliderBounds = new float3 { x = 1f, y = 1f, z = 1f },
											timeLength = 2f,
											value = new float3 { x = 5f, y = 0, z = 5f },
											useCameraDirection = true
										}
									}
								}
							}
						};
						break;
					default:
						Debug.Log("failed to find a matching pokemon move data entity");
						break;
				}
				return pmde;
			}
			public static RenderMesh getPokemonMoveRenderMesh(ByteString30 name)
			{
				RenderMesh renderMesh = new RenderMesh { };
				GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);

				switch (PokemonIO.ByteString30ToString(name))
				{
					case "ThunderShock":
						renderMesh = new RenderMesh
						{
							mesh = go.GetComponent<MeshFilter>().sharedMesh,
							material = go.GetComponent<MeshRenderer>().material,
							castShadows = UnityEngine.Rendering.ShadowCastingMode.On,
							receiveShadows = true
						};
						break;
					default:
						
						renderMesh = new RenderMesh
						{
							mesh = go.GetComponent<MeshFilter>().sharedMesh,
							material = go.GetComponent<MeshRenderer>().material,
							castShadows = UnityEngine.Rendering.ShadowCastingMode.On,
							receiveShadows = true
						};
						break;
				}
				GameObject.Destroy(go);
				return renderMesh;
			}



			//damage is calculated using the base damage and level
			private static float calculateDamage(float pokemonLevel,float baseDamage)
			{
				return (pokemonLevel / 100) / baseDamage;
			}
		}
	}
}
