 using System;
using Unity.Entities;
using UnityEngine;
using Pokemon.Move;

namespace Pokemon
{
/*	[RequiresEntityConversion]
	public class PokemonEntityDataComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		public int guiId;
		public ushort PokedexNumber;
		public float Height;
		public ushort experienceYield;
		public ushort LevelingRate;
		public ushort Freindship;
		public float Speed;
		public float Acceleration;
		public ushort Hp;
		public ushort Attack;
		public ushort Defense;
		public ushort SpecialAttack;
		public ushort SpecialDefense;
		public bool AutoLoadUsingPokemonName = false;
		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			if (AutoLoadUsingPokemonName)
			{
				//get the pokemone entity to get the pokemon name
				PokemonEntity pe = dstManager.GetComponentData<PokemonEntity>(entity);
				//load pokeomn base data
				PokemonData pd = PokemonDataClass.getBasePokemonData(PokemonDataClass.StringToPokedexEntry((pe.pokemonName)));
				//convert base data to pokemon eneity data
				PokemonEntityData ped = new PokemonEntityData {
						Acceleration = pd.BaseAcceleration,
						Attack = pd.BaseAttack,
						SpecialAttack = pd.BaseSpecialAttack,
						SpecialDefense = pd.BaseSpcialDefense,
						Speed = pd.BaseSpeed,
						Defense = pd.BaseDefense,
						PokedexNumber = pd.PokedexNumber,
						LevelingRate = pd.LevelingRate,
						Hp = pd.BaseHp,
						experienceYield = pd.BaseExpericeYield,
						Height = pd.Height,
						Freindship = pd.BaseFriendship,
						Mass = pd.Mass,
						jumpHeight = pd.Height*10,
						guiId = guiId,
						currentHp = pd.BaseHp,
						currentStamina = pd.BaseHp,
						maxStamina = pd.BaseHp,
						pokemonMoveSet = new PokemonMoveSet
						{
							pokemonMoveA = new PokemonMove
							{
								isValid = true,
								name = new ByteString30("Tackle")
							},
							pokemonMoveB = new PokemonMove
							{
								isValid = true,
								name = new ByteString30("ThunderShock")
							}
						}
						
				};
				dstManager.AddComponentData(entity, ped);
			}
			else
			{
				//you are suppose to calculate jump force based on mass and stuff
				PokemonEntityData ped = new PokemonEntityData
				{
					PokedexNumber = PokedexNumber,
					Height = Height,
					experienceYield = experienceYield,
					LevelingRate = LevelingRate,
					Freindship = Freindship,
					Speed = Speed,
					Acceleration = Acceleration,
					Hp = Hp,
					Attack = Attack,
					Defense = Defense,
					SpecialAttack = SpecialAttack,
					SpecialDefense = SpecialDefense,
					guiId = guiId,
					currentHp = Hp
				};
				dstManager.AddComponentData(entity, ped);
			}
		}
	}
	*/
	///<summary>
	///Holds the data of a pokemon Entity
	///</summary>
	///
	[Serializable]
	public struct PokemonEntityData : IComponentData
	{
		public ushort PokedexNumber;
		public float Height;
		public ushort experienceYield;
		public ushort LevelingRate;
		public ushort Freindship;
		public float Speed;
		public float Acceleration;
		public float Hp;
		public ushort Attack;
		public ushort Defense;
		public ushort SpecialAttack;
		public ushort SpecialDefense;
		public float currentStamina;
		public float maxStamina;
		public float currentHp;
		public float Mass;
		public float jumpHeight;
		public int currentLevel;
		public PokemonMoveSet pokemonMoveSet;
		public int guiId;
		public float jumpMultiplier;
		public float longJumpMultiplier;
		public char BodyType;
		public char MovementType;
		public PokemonEntityData(ushort pokedexNumber = 0,float height = 0,ushort _experienceYield = 0, ushort levelingRate = 0,ushort freindship = 0,float speed = 0,float acceleration = 0,
			float hp = 0,ushort attack = 0,ushort defense = 0,ushort specialAttack = 0, ushort specialDefense = 0, float CurrentStamina = 0,float MaxStamina = 0,float CurrentHp = 0,
			float mass = 0, float JumpHeight = 0,int CurrentLevel = 0, PokemonMoveSet pms = new PokemonMoveSet(),int guiID = 0,char bodyType = (char)0,char movementType = (char)0,float jMultiplier = 2f,float ljMultiplier = 2.5f)
		{
			PokedexNumber = pokedexNumber;
			Height = height;
			experienceYield = _experienceYield;
			LevelingRate = levelingRate;
			Freindship = freindship;
			Speed = speed;
			Acceleration = acceleration;
			Hp = hp;
			Attack = attack;
			Defense = defense;
			SpecialAttack = specialAttack;
			SpecialDefense = specialDefense;
			currentStamina = CurrentStamina;
			maxStamina = MaxStamina;
			currentHp = CurrentHp;
			Mass = mass;
			jumpHeight = JumpHeight;
			currentLevel = CurrentLevel;
			pokemonMoveSet = pms;
			guiId = guiID;
			BodyType = bodyType;
			jumpMultiplier = jMultiplier;
			longJumpMultiplier = ljMultiplier;
			MovementType = movementType;
		}
	}
}
