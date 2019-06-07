using System;
using Unity.Entities;
using Unity.Transforms;

namespace Pokemon
{
	[Serializable]
	public struct PokemonEntityDataSave
	{
		public PokemonEntityData PokemonEntityData;
		public ByteString30 entityId;
		public ByteString30 entityName;
		public Translation Position;
		public Rotation Rotation;
	}
	/// <summary>
	/// This holds four pokemon Moves
	/// </summary>
	[Serializable]
	public struct PokemonMoveData : IComponentData
	{
		public PokemonMove pokemonMoveA;
		public PokemonMove pokemonMoveB;
		public PokemonMove pokemonMoveC;
		public PokemonMove pokemonMoveD;
	}

	/// <summary>
	/// Pokemon Move 
	/// </summary>
	[Serializable]
	public struct PokemonMove
	{
		public ByteString30 name;             //name of pokemon move
											  //Attack Type Varibles
		public ushort matterType;         //substance of the type e.g. electric, rock,ground
		public ushort statusType;         //e.g. sleep,paralisys
		public ushort contactType;        //type of move e.g. physical, sspecial attack
										  //Damage Varibles
		public ushort baseDamage;          //base damage of attack
										   //PP varibles
		public ushort basePP;             //base PP for attack
		public ushort pp;                 //current PP 
		public ushort maxPP;              //max PP
	}

	[Serializable]
	public struct PokemonData : IComponentData
	{
		/*
		 Remeber that a ushaort = UInt16
		Type      Capacity

		Int16 -- (-32,768 to +32,767)

		Int32 -- (-2,147,483,648 to +2,147,483,647)

		Int64 -- (-9,223,372,036,854,775,808 to +9,223,372,036,854,775,807)
			 */
		public ushort PokedexNumber;
		//Pokemon's Catch Rate
		public float CatchRate;
		//Pokemon's Chance of being a Male
		public float MaleChance;
		/*pokemon egg group
		0 = Monster   1 = Human-Like  2 = Water 1		4 = Water 2			5 = Water 3
		6 = Bug		7 = Mineral		8 = Flying		9 = Amorphous		10 = Field
		11 = Fairy	12 = Ditto		13 = Grass		14 = Dragon			15 = Undiscovered
		*/
		public ushort EggGroup;
		//minimun amount of steps it takes to hatch the pokemon
		public ushort MinimumSteps;
		//maximum amount of steps it takes to hatch the pokemon
		public ushort MaximumSteps;
		//height in meters
		public float Height;
		//height variation for pokemon
		public float HeightVariation;
		//weight in kilograms
		public float Mass;
		//weight variation for pokemon
		public float MassVariation;
		//Base Experice Yield per battle
		public ushort BaseExpericeYield;
		//Leveling rate 0 = slow	1 = medium slow		2 = medium	3 = medium fast	4 = fast 
		public ushort LevelingRate;
		//Base Freindship
		public ushort BaseFriendship;
		//Base hp stat
		public ushort BaseHp;
		//Base attack stat
		public ushort BaseAttack;
		//base defense stat
		public ushort BaseDefense;
		//base speedStat
		public float BaseSpeed;
		//base acceleration
		public float BaseAcceleration;
		//base special attack stat
		public ushort BaseSpecialAttack;
		//base special defense stat
		public ushort BaseSpcialDefense;


		//base pokemon move data

	}
	//public class PokemonDataComponent : ComponentDataProxy<PokemonData> { }
	public class PokemonDataClass {
		public static PokemonData getBasePokemonData(uint pokedexEntry = 0)
		{
			PokemonData pd = new PokemonData { };
			switch (pokedexEntry)
			{
				//Bulbasaur
				case 0:
				case 1:
					pd = new PokemonData
					{
						CatchRate = 0.059f,
						MaleChance = 0.875f,
						EggGroup = 14, //change when format is finished
						MinimumSteps = 5140,
						MaximumSteps = 5396,
						Height = 0.7f,
						HeightVariation = 0.5f,
						Mass = 6.9f,
						MassVariation = 0.5f,
						BaseExpericeYield = 64,
						LevelingRate = 2,
						BaseFriendship = 70,
						BaseHp = 45,
						BaseAttack = 49,
						BaseDefense = 49,
						BaseSpeed = 45f,
						BaseAcceleration = 7.2f,	//if height under 1 then formula is speed / (mass-height)
						BaseSpecialAttack = 65,
						BaseSpcialDefense = 65
					};
					break;
				case 2:
					pd = new PokemonData
					{
						CatchRate = 0.059f,
						MaleChance = 0.875f,
						EggGroup = 14, //change when format is finished
						MinimumSteps = 5140,
						MaximumSteps = 5396,
						Height = 1.0f,
						HeightVariation = 0.5f,
						Mass = 13.0f,
						MassVariation = 0.5f,
						BaseExpericeYield = 142,
						LevelingRate = 1,
						BaseFriendship = 70,
						BaseHp = 60,
						BaseAttack = 62,
						BaseDefense = 63,
						BaseSpeed = 60f,
						BaseAcceleration = 20f,
						BaseSpecialAttack = 80,
						BaseSpcialDefense = 80
					};
					break;
				case 3:
					pd = new PokemonData
					{
						CatchRate = 0.059f,
						MaleChance = 0.875f,
						EggGroup = 14, //change when format is finished
						MinimumSteps = 5140,
						MaximumSteps = 5396,
						Height = 2.4f,
						HeightVariation = 0.5f,
						Mass = 100.0f,
						MassVariation = 0.5f,
						BaseExpericeYield =236,
						LevelingRate = 1,
						BaseFriendship = 70,
						BaseHp = 80,
						BaseAttack = 82,
						BaseDefense = 83,
						BaseSpeed = 80f,
						BaseAcceleration = 1.05f, //speed / (mass-(height*10))
						BaseSpecialAttack = 100,
						BaseSpcialDefense = 100
					};
					break;
				case 101:
					pd = new PokemonData {
						CatchRate = 0.078f,
						MaleChance = -1f,//genderless
						EggGroup = 14, //change when format is finished
						MinimumSteps = 5140,
						MaximumSteps = 5396,
						Height = 1.2f,
						HeightVariation = 0.5f,
						Mass = 66.6f,
						MassVariation = 0.5f,
						BaseExpericeYield = 150,
						LevelingRate = 3,
						BaseFriendship = 70,
						BaseHp = 60,
						BaseAttack = 50,
						BaseDefense = 70,
						BaseSpeed = 150f,
						BaseAcceleration = 2.75f,
						BaseSpecialAttack = 80,
						BaseSpcialDefense = 45
					};
					break;

			}
			return pd;
		}
	}
	//# Pokémon 	HP 	Attack 	Defense 	Sp. Attack 	Sp. Defense 	Speed 	Total 	Average
	
}
