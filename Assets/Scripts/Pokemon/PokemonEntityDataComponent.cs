 using System;
using Unity.Entities;
using UnityEngine;
using Pokemon.Move;

namespace Pokemon
{
	[RequiresEntityConversion]
	public class PokemonEntityDataComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		public string pokemonName;
		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			PokemonEntityData ped = PokemonDataClass.GenerateBasePokemonEntityData(PokemonDataClass.StringToPokedexEntry(pokemonName),true);
			dstManager.AddComponentData(entity,ped);
			dstManager.AddComponentData(entity, new ChangeEntityPokemonRequest {
				UseCoreDataName = true
			});
		}
	}
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
		public PokemonEntityData(ushort pokedexNumber = 0,float height = 0,ushort _experienceYield = 0, ushort levelingRate = 0,ushort freindship = 0,float speed = 0,float acceleration = 0,
			float hp = 0,ushort attack = 0,ushort defense = 0,ushort specialAttack = 0, ushort specialDefense = 0, float CurrentStamina = 0,float MaxStamina = 0,float CurrentHp = 0,
			float mass = 0, float JumpHeight = 0,int CurrentLevel = 0, PokemonMoveSet pms = new PokemonMoveSet(),int guiID = 0,char bodyType = (char)0,float jMultiplier = 2f,float ljMultiplier = 2.5f)
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
		}
	}
}
