using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Core.Particles;
using Core;
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
			public int index; //used for spawning moves
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
			public PokemonMoveScaleSet pokemonMoveScaleSet;
		}
		//used for multiple things...im too tired to explain
		public struct PokemonMoveAdjustment : IComponentData
		{
			public float3 value;                //how much to adjust 
			public float timeLength;            //how long the affect lasts
			public BlittableBool useCameraDirection;
			public float staminaCost;       //how much stamina does this move cost
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
		
		public struct PokemonMoveScaleAdjustment : IComponentData
		{
			public float value;
			public float timeLength;
			public float staminaCost;
		}
		/// <summary>
		/// allows for up to 30 scale adjustments
		/// </summary>
		[Serializable]
		public struct PokemonMoveScaleSet : IComponentData
		{
			public PokemonMoveScaleAdjustment A;
			public PokemonMoveScaleAdjustment B;
			public PokemonMoveScaleAdjustment C;
			public PokemonMoveScaleAdjustment D;
			public PokemonMoveScaleAdjustment E;
			public PokemonMoveScaleAdjustment F;
			public PokemonMoveScaleAdjustment G;
			public PokemonMoveScaleAdjustment H;
			public PokemonMoveScaleAdjustment I;
			public PokemonMoveScaleAdjustment J;
			public PokemonMoveScaleAdjustment K;
			public PokemonMoveScaleAdjustment L;
			public PokemonMoveScaleAdjustment M;
			public PokemonMoveScaleAdjustment N;
			public PokemonMoveScaleAdjustment O;
			public PokemonMoveScaleAdjustment P;
			public PokemonMoveScaleAdjustment Q;
			public PokemonMoveScaleAdjustment R;
			public PokemonMoveScaleAdjustment S;
			public PokemonMoveScaleAdjustment T;
			public PokemonMoveScaleAdjustment U;
			public PokemonMoveScaleAdjustment V;
			public PokemonMoveScaleAdjustment W;
			public PokemonMoveScaleAdjustment X;
			public PokemonMoveScaleAdjustment Y;
			public PokemonMoveScaleAdjustment Z;
			public BlittableBool isValid;
		}
		[Serializable]
		/// <summary>
		/// allows for up to 30 position adjustments
		/// </summary>
		public struct PokemonMoveTranslationSet : IComponentData {
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

		public struct PokemonMoveEntity : IComponentData { public char a; }
		/// <summary>
		/// spawn data
		/// </summary>
		[Serializable]
		public struct PokemonMoveDataSpawn : IComponentData
		{
			public Translation translation;
			public Rotation rotation;
			public PhysicsCollider physicsCollider;
			public PhysicsDamping physicsDamping;
			public PhysicsGravityFactor physicsGravityFactor;
			public PhysicsMass physicsMass;
			public PhysicsVelocity physicsVelocity;
			public BlittableBool hasDamping;
			public BlittableBool hasGravityFactor;
			public BlittableBool hasMass;
			public BlittableBool hasCollider;
			public BlittableBool hasPhysicsVelocity;
			public BlittableBool hasEntity;
			public BlittableBool hasParticles;
			public BlittableBool projectOnParentInstead; //used with physical attacks. instead of the PokemonMove Entity moving you have the parent entity move instead
		}
		public struct PokemonMoveDataEntity : IComponentData {
			public ByteString30 name;
			public PokemonMoves.AttackType attackType;
			public PokemonMoves.StatusType statusType;
			public PokemonMoves.ContactType contactType;
			public BlittableBool isValid;
			public BlittableBool hasParticles;
			public BlittableBool preformActionsOn;
			public PokemonMoveAdjustmentData pokemonMoveAdjustmentData;
			public PokemonMoveParticleDataSet pokemonMoveParticleDataSet;
			public float damage;
			public float accuracy;
			public float3 forward;
		}
		public struct PokemonMoveFollowEnityData : IComponentData {
			public Entity entity;
		};
		public struct PokemonMoveEntityRemoveRequest : IComponentData{ };
		
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


		/// <summary>
		/// Data realted to Physical Attacks
		/// </summary>
		public struct PhysicalAttackData : IComponentData
		{
			public char statusEffect;
			public char ability;
		}


		public class PokemonMoves
		{
			public enum ContactType
			{
				Physical = 0,
				Special = 1,
				Emotional = 2,
				Philisophical = 3,
				Invalid = 4,
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
			public static ref PokemonMoveAdjustment getAdjustment(ref PokemonMoveAdjustmentSet set, int index)
			{
				switch (index)
				{
					case 0: return ref set.A;
					case 1: return ref set.B;
					case 2: return ref set.C;
					case 3: return ref set.D;
					case 4: return ref set.E;
					case 5: return ref set.F;
					case 6: return ref set.G;
					case 7: return ref set.H;
					case 8: return ref set.I;
					case 9: return ref set.J;
					case 10: return ref set.K;
					case 11: return ref set.L;
					case 12: return ref set.M;
					case 13: return ref set.N;
					case 14: return ref set.O;
					case 15: return ref set.P;
					case 16: return ref set.Q;
					case 17: return ref set.R;
					case 18: return ref set.S;
					case 19: return ref set.T;
					case 20: return ref set.U;
					case 21: return ref set.V;
					case 22: return ref set.W;
					case 23: return ref set.X;
					case 24: return ref set.Y;
					default: return ref set.A;
				}
			}
			public static ref PokemonMoveScaleAdjustment getScaleAdjustment(ref PokemonMoveScaleSet set, int index)
			{
				switch (index)
				{
					case 0: return ref set.A;
					case 1: return ref set.B;
					case 2: return ref set.C;
					case 3: return ref set.D;
					case 4: return ref set.E;
					case 5: return ref set.F;
					case 6: return ref set.G;
					case 7: return ref set.H;
					case 8: return ref set.I;
					case 9: return ref set.J;
					case 10: return ref set.K;
					case 11: return ref set.L;
					case 12: return ref set.M;
					case 13: return ref set.N;
					case 14: return ref set.O;
					case 15: return ref set.P;
					case 16: return ref set.Q;
					case 17: return ref set.R;
					case 18: return ref set.S;
					case 19: return ref set.T;
					case 20: return ref set.U;
					case 21: return ref set.V;
					case 22: return ref set.W;
					case 23: return ref set.X;
					case 24: return ref set.Y;
					default: return ref set.A;
				}
			}
			public static float3 getNextPokemonMoveAdjustment(ref PokemonMoveAdjustmentSet set, ref float currentStamina, float time, float3 forward)
			{
				for (int i = 0; i < 25; i++)
				{
					ref PokemonMoveAdjustment pma = ref getAdjustment(ref set, i);
					if (pma.timeLength == -1f)
					{
		//				Debug.Log("Detected 1 time addition" + time + " forward = " + forward);
						//one time thing
						pma.timeLength = 0;
						set.isValid = false;
						if (currentStamina - pma.staminaCost < 0)
						{
							float3 temp = pma.useCameraDirection ? pma.timeLength * forward * pma.value : pma.timeLength * pma.value;
							temp *= currentStamina / pma.staminaCost;
							currentStamina = 0;
							return temp;
						}
						else
						{
							currentStamina -= pma.staminaCost;
							return pma.useCameraDirection ? forward * pma.value : pma.value;
						}
					}
					else if (pma.timeLength > 0)
					{
		//				Debug.Log("Detected it's not over");
						if (pma.timeLength - time >= 0)
						{
							pma.timeLength -= time;
							if (currentStamina - pma.staminaCost < 0)
							{
								float3 temp = pma.useCameraDirection ? pma.timeLength * forward * pma.value : pma.timeLength * pma.value;
								temp *= currentStamina / pma.staminaCost;
								currentStamina = 0;
								return temp;
							}
							else
							{
								currentStamina -= pma.staminaCost;
								return pma.useCameraDirection ? time * forward * pma.value : time * pma.value;
							}
						}
						else
						{
							float3 temp = pma.timeLength;
							pma.timeLength = 0;
							if (currentStamina - pma.staminaCost < 0)
							{
								temp = pma.useCameraDirection ? temp * forward * pma.value : time * pma.value;
								temp *= currentStamina / pma.staminaCost;
								currentStamina = 0;
								return temp;
							}
							else
							{
								currentStamina -= pma.staminaCost;
								return pma.useCameraDirection ? temp * forward * pma.value : time * pma.value;
							}
						}
					}
					else if (i == 24) set.isValid = false;
				}
				return float3.zero;
			}
			public static float3 getNextPokemonMoveAdjustment(ref PokemonMoveAdjustmentSet set, float time, float3 forward)
			{
				for (int i = 0; i < 25; i++)
				{
				//	Debug.Log("i = " + i);
					ref PokemonMoveAdjustment pma = ref getAdjustment(ref set, i);
					if (pma.timeLength == -1f)
					{
		//				Debug.Log("AADetected 1 time addition" + time + " forward = " + forward);
						//one time thing
						pma.timeLength = 0;
						set.isValid = false;
						return pma.useCameraDirection ? forward * pma.value : pma.value;
					}
					else if (pma.timeLength > 0)
					{
					//	Debug.Log("AADetected it's not over:" + pma.timeLength.ToString("F3") + "," + time + "," + pma.value+','+forward);
						if (pma.timeLength - time >= 0)
						{
							pma.timeLength -= time;
			//				Debug.Log("AADetected it's not overB:" + pma.timeLength.ToString("F3") + "," + time + "," + pma.value);
							return pma.useCameraDirection ? time * forward * pma.value : time * pma.value;
						}
						else
						{
							float3 temp = pma.useCameraDirection ?
								pma.timeLength * forward * pma.value : time * pma.value;
							pma.timeLength = 0;

			//				Debug.Log("AADetected it's not overBBBBBB:" + pma.timeLength.ToString("F3") + "," + time + "," + pma.value);
							return temp;
						}
					}
					else if (i == 24) set.isValid = false;
				}
				return float3.zero;
			}
			public static float getNextPokemonMoveAdjustment(ref PokemonMoveScaleSet set, float time)
			{
				for (int i = 0; i < 25; i++)
				{
					ref PokemonMoveScaleAdjustment pma = ref getScaleAdjustment(ref set, i);
					if (pma.timeLength == -1f)
					{
						//one time thing
						pma.timeLength = 0;
						set.isValid = false;
						return pma.value;
					}
					else if (pma.timeLength > 0)
					{
						pma.timeLength = pma.timeLength - time >= 0 ? pma.timeLength-time : 0;
						return pma.value;
					}
					else if (i == 24) set.isValid = false;
				}
				return 1f;
			}
			public static float getNextPokemonMoveAdjustment(ref PokemonMoveScaleSet set, float time,ref float currentStamina)
			{
				for (int i = 0; i < 25; i++)
				{
					ref PokemonMoveScaleAdjustment pma = ref getScaleAdjustment(ref set, i);
					currentStamina = math.clamp(currentStamina - pma.staminaCost, 0f, currentStamina);
					if (currentStamina > 0f)
					{
						if (pma.timeLength == -1f)
						{
							//one time thing
							pma.timeLength = 0;
							set.isValid = false;
							return pma.value;
						}
						else if (pma.timeLength > 0)
						{
							pma.timeLength = pma.timeLength - time >= 0 ? pma.timeLength - time : 0;
							return pma.value;
						}
						else if (i == 24) set.isValid = false;
					}
				}
				return 1f;
			}
			public static ParticleSystemSpawnData getPokemonMoveParticleSystemData(ushort pokedexNumber,string pokemonMoveName)
			{

				ParticleSystemSpawnData pssd = new ParticleSystemSpawnData { isValid = false };
				switch (pokemonMoveName)
				{
					case "Tackle":
						switch (pokedexNumber) {
							case 101:
								pssd = new ParticleSystemSpawnData
								{
									particleSystemSpawnDataShape = new ParticleSystemSpawnDataShape
									{
										isValid = true,
										offsetPostiion = new float3(0,-0.6f,-0.34f),
										offsetRotation = new float3(200f,0,0),
										offsetScale = 1f
									},
									paticleSystemName = new ByteString30(pokemonMoveName),
									isValid = true

								};
								break;	
						}
						break;
					case "ThunderBolt":
						switch (pokedexNumber)
						{
							case 101:
								pssd = new ParticleSystemSpawnData
								{
									isValid = true,
									particleSystemSpawnDataShape = new ParticleSystemSpawnDataShape
									{
										isValid = true,
										offsetPostiion = new float3(0, 0, 0),
										offsetRotation = new float3(0, 0, 0),
										offsetScale = 1f
									},
									paticleSystemName = new ByteString30(pokemonMoveName)
								};
								break;
						}
						break;
				}
				return pssd;
			}
			//not currently used but will keep for now
			public static ContactType GetPokemonMoveContactType(string pokemonMoveName)
			{
				switch (pokemonMoveName)
				{
					case "Tackle": return ContactType.Physical;
					case "ThunderBolt":return ContactType.Special;
					default: return ContactType.Invalid;
				}
			}
			public static ContactType GetPokemonMoveContactType(ByteString30 pokemonMoveName)
			{
				string pokemonMoveName_ = pokemonMoveName.ToString();
				switch (pokemonMoveName_)
				{
					case "Tackle": return ContactType.Physical;
					case "ThunderBolt": return ContactType.Special;
					default: return ContactType.Invalid;
				}
			}
			public static BlittableBool GetPokemonMovesHaveParticles(string pokemonMoveName)
			{
				switch (pokemonMoveName)
				{
					case "Tackle": return false;
					case "ThunderBolt": return true;
					default: return false;
				}
			}
			public static BlittableBool GetPokemonMovesHaveParticles(ByteString30 pokemonMoveName)
			{
				string pokemonMoveName_ = pokemonMoveName.ToString();
				switch (pokemonMoveName_)
				{
					case "Tackle": return false;
					case "ThunderBolt": return true;
					default: return false;
				}
			}
		}
	}
}
