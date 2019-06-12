﻿ using System;
using Unity.Entities;
using UnityEngine;
using Pokemon.Move;

namespace Pokemon
{
	[RequiresEntityConversion]
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
		public PokemonMoveData pokemonMoveData;
		public bool AutoLoadUsingPokemonName = false;
		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			if (AutoLoadUsingPokemonName)
			{
				//get the pokemone entity to get the pokemon name
				PokemonEntity pe = dstManager.GetComponentData<PokemonEntity>(entity);
				//load pokeomn base data
				PokemonData pd = PokemonDataClass.getBasePokemonData(PokemonIO.StringToPokedexEntry(PokemonIO.ByteString30ToString(pe.pokemonName)));
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
								followEntity = true,
								isValid = true,
								name = PokemonIO.StringToByteString30("Tackle")
							},
							pokemonMoveB = new PokemonMove
							{
								isValid = true,
								name = PokemonIO.StringToByteString30("ThunderShock"),
								followEntity = false
							}
						}
						
						jumpHeight = pd.Height*10
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
					SpecialDefense = SpecialDefense
				};
				dstManager.AddComponentData(entity, ped);
			}
		}
	}

	[Serializable]
	///<summary>
	///Holds the data of a pokemon Entity
	///</summary>
	///
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
		public ushort Hp;
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

		public int currentStamina;
		public int maxStamina;
		public int currentHp;
		public float Mass;
		public float jumpHeight;
		public PokemonMoveData pokemonMoveData;
	}

}
