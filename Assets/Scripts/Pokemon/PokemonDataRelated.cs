using System;
using Unity.Entities;
using Unity.Transforms;
using Pokemon.Move;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

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
		//Pokemon Body Type
		//0 = Head Only			1 = Head & legs					2 = Pokemon With Fins	3 = insectiod body
		//4 = quadruped body	5 = 2 or more pairs of wings	6 = 2 or more bodies	7 = tecticles or multiped body
		//8 = head with base	9 = biped with tail				10 = biped tailess		11 = single pair of wings
		//12 = serpant body		13 = head with arms
		public char BodyType;


		//base pokemon move data

	}
	public struct PokemonCameraData : IComponentData
	{
		public float3 firstPersonOffset;
		public float3 thridPersonOffset;
	}
	public class PokemonDataClass {
		//use char to save space
		public const char BODY_TYPE_HEAD_ONLY = (char)0;
		public const char BODY_TYPE_HEAD_AND_LEGS = (char)1;
		public const char BODY_TYPE_WITH_FINS = (char)2;
		public const char BODY_TYPE_INSECT_BODY = (char)3;
		public const char BODY_TYPE_QUADRUPED_BODY = (char)4;
		public const char BODY_TYPE_2_OR_MORE_WINGS = (char)5;
		public const char BODY_TYPE_2_OR_MORE_BODIES = (char)6;
		public const char BODY_TYPE_TENTICLES_OR_MULTIPED = (char)7;
		public const char BODY_TYPE_HEAD_WITH_BASE = (char)8;
		public const char BODY_TYPE_BIPED_WITH_TAIL = (char)9;
		public const char BODY_TYPE_BIPED_TAILESS = (char)10;
		public const char BODY_TYPE_SINGLE_PAIR_OF_WINGS = (char)11;
		public const char BODY_TYPE_SERPANT_BODY = (char)12;
		public const char BODY_TYPE_HEAD_WITH_ARMS = (char)13;

		/// <summary>
		/// returns the pokemon's base data
		/// </summary>
		/// <param name="pokedexEntry">pokedex entry number</param>
		/// <returns>PokemonData</returns>
		public static PokemonData getBasePokemonData(uint pokedexEntry = 0)
		{
			PokemonData pd = new PokemonData { };
			switch (pokedexEntry)
			{
				//Bulbasaur
				case 0:
				case 1: //bulbasaur
					pd = new PokemonData
					{
						CatchRate = 11.9f,
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
						BaseSpcialDefense = 65,
						BodyType = PokemonDataClass.BODY_TYPE_QUADRUPED_BODY,
						PokedexNumber = 1
					};
					break;
				case 2: //Ivysaur
					pd = new PokemonData
					{
						CatchRate = 11.9f,
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
						BaseSpcialDefense = 80,
						BodyType = PokemonDataClass.BODY_TYPE_QUADRUPED_BODY,
						PokedexNumber = 2
					};
					break;
				case 3: //Venasaur
					pd = new PokemonData
					{
						CatchRate = 11.9f,
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
						BaseSpcialDefense = 100,
						BodyType = PokemonDataClass.BODY_TYPE_QUADRUPED_BODY,
						PokedexNumber = 3
					};
					break;
				case 101: //Electrode
					pd = new PokemonData {
						CatchRate = 14.8f,
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
						BaseSpcialDefense = 45,
						BodyType = PokemonDataClass.BODY_TYPE_HEAD_ONLY,
						PokedexNumber = 101
					};
					break;
				case 104: //Cubone
					pd = new PokemonData
					{
						CatchRate = 35.2f,
						MaleChance = 50f,//genderless
						EggGroup = 0, //change when format is finished
						MinimumSteps = 5140,
						MaximumSteps = 5396,
						Height = 0.4f,
						HeightVariation = 0.5f,
						Mass = 6.5f,
						MassVariation = 0.5f,
						BaseExpericeYield = 87,
						LevelingRate = 3,
						BaseFriendship = 70,
						BaseHp = 50,
						BaseAttack = 50,
						BaseDefense = 95,
						BaseSpeed = 35f,
						BaseAcceleration = 2.75f,
						BaseSpecialAttack = 40,
						BaseSpcialDefense = 50,
						BodyType = PokemonDataClass.BODY_TYPE_BIPED_WITH_TAIL,
						PokedexNumber = 104
					};
					break;
			}
			return pd;
		}
		/// <summary>
		/// Takes in a pokemon name and returns a corresponding Pokedex Entry name
		/// </summary>
		/// <param name="name">name of the pokemon (First letter capitialized)</param>
		/// <returns>ushort</returns>
		public static ushort StringToPokedexEntry(string name)
		{
			switch (name)
			{
				case "Bulbasaur": return 1;
				case "Ivysaur": return 2;
				case "Venasaur": return 3;
				case "Electrode": return 101;
				case "Cubone": return 104;
				default: return 0;
			}
		}
		/// <summary>
		/// returns the pokemon's camera offset
		/// </summary>
		/// <param name="pokemonName"></param>
		/// <param name="entity"></param>
		/// <param name="entityManager"></param>
		public static void SetPokemonCameraData(string pokemonName,Entity entity,EntityManager entityManager)
		{
			PokemonCameraData pcd = new PokemonCameraData {
				thridPersonOffset = new float3(0,5f,-10f),
				firstPersonOffset = new float3(0,2f,0f)
			};
			switch (pokemonName)
			{
				case "Bulbasaur":
					break;
				case "Ivysaur":
					break;
				case "Venasaur":
					break;
				case "Electrode":
					pcd = new PokemonCameraData
					{
						thridPersonOffset = new float3(0, 2f, 3.5f),
						firstPersonOffset = new float3(0, 0.3f, -0.35f)
					};
					break;
				case "Cubone":
					pcd = new PokemonCameraData {
						firstPersonOffset = new float3(0,0.3f,-0.35f),
						thridPersonOffset = new float3(0,1f,-3f)
					};
					break;
				default:
					Debug.LogError("Failed to get pokemonCameraData!");
					break;
			}
			if (entityManager.HasComponent<PokemonCameraData>(entity)) entityManager.SetComponentData<PokemonCameraData>(entity, pcd);
			else entityManager.AddComponentData(entity, pcd);
		}

		public static float CalculateJumpHeight(float maxHeight,float time,float gravity = -9.81f)
		{
			return (maxHeight - (0.5f * gravity * time * time)) / time;
		}
		/// <summary>
		/// gets the pokemon's base data
		/// </summary>
		/// <param name="pokemonName">the name oof the pokemon</param>
		/// <returns>PokemonEntityData</returns>
		public static PokemonEntityData GenerateBasePokemonEntityData(string pokemonName)
		{
			PokemonData pd = getBasePokemonData(StringToPokedexEntry(pokemonName));
			//add the PokemonEntityData
			if (pd.PokedexNumber == 0) { Debug.LogError("Failed to get basic pokemonData"); return new PokemonEntityData { }; }
			else
			{
				float jumpHeight = 1.0f;
				switch (pokemonName){
					case "Electrode": jumpHeight = 5.0f; break;
					case "Cubone": jumpHeight = 5f;break;
					default: Debug.LogError("Failed to find a jumpHeight multipler for \""+pokemonName+"\"");break;
				}
				Debug.Log("height jump thing = "+ CalculateJumpHeight(pd.Height * 2.5f, 2));
				return new PokemonEntityData
				{
					Acceleration = pd.BaseAcceleration,
					Attack = pd.BaseAttack,
					SpecialAttack = pd.BaseSpecialAttack,
					currentHp = pd.BaseHp,
					SpecialDefense = pd.BaseSpcialDefense,
					Speed = pd.BaseSpeed,
					currentStamina = pd.BaseHp,
					maxStamina = pd.BaseHp,
					Mass = pd.Mass,
					jumpHeight = jumpHeight,
					Hp = pd.BaseHp,
					Defense = pd.BaseDefense,
					currentLevel = 1,
					experienceYield = pd.BaseExpericeYield,
					Height = pd.Height,
					LevelingRate = pd.LevelingRate,
					guiId = 123,
					PokedexNumber = pd.PokedexNumber,
					Freindship = pd.BaseFriendship,
					pokemonMoveSet = getBasePokemonMoveSet(pokemonName)
				};
			}

		}
		/// <summary>
		/// gets the pokemon's default move set
		/// </summary>
		/// <param name="pokemonName">name of the pokemon</param>
		/// <returns>PokemonMOveSet</returns>
		public static PokemonMoveSet getBasePokemonMoveSet(string pokemonName)
		{
			PokemonMoveSet pms = new PokemonMoveSet { };
			switch (pokemonName)
			{
				case "Cubone":
					pms = new PokemonMoveSet
					{
						pokemonMoveA = new PokemonMove { name = PokemonIO.StringToByteString30("Tackle"),isValid = true }
					};
					break;
				case "Electrode":
					pms = new PokemonMoveSet
					{
						pokemonMoveA = new PokemonMove { name= PokemonIO.StringToByteString30("ThunderBolt"), isValid= true},
						pokemonMoveB = new PokemonMove { name = PokemonIO.StringToByteString30("Tackle"), isValid = true}
					};
					break;
			}
			return pms;
		}
		/// <summary>
		/// creates a new PhysicsMass and returns it
		/// </summary>
		/// <param name="physicsCollider">The Physics Collider associated with the Pokemon Entity you are trying to create</param>
		/// <param name="mass">the mass of the pokemon (in kilograms)</param>
		/// <param name="createDynamic">whether or not to create a kinematic or dynamic mass (Note: kinematic masses don't need the mass value)</param>
		/// <returns></returns>
		public static PhysicsMass getPokemonPhysicsMass(PhysicsCollider physicsCollider,float mass,bool createDynamic = true)
		{
			return createDynamic? PhysicsMass.CreateDynamic(physicsCollider.MassProperties, mass) : PhysicsMass.CreateKinematic(physicsCollider.MassProperties);
		}
		/// <summary>
		/// creates and returns the pokemoon's PhysicsCollider
		/// </summary>
		/// <param name="pokemonName">Name of the pokemon</param>
		/// <returns>PhysicsCollider</returns>
		public static PhysicsCollider getPokemonPhysicsCollider(string pokemonName,PokemonEntityData ped,CollisionFilter collisionFilter = new CollisionFilter(),Unity.Physics.Material material = new Unity.Physics.Material()) 
		{
			///FUTURE UPDATE
			///allow specific colliders to recieve specific filters and materials!

			//needs collision groups
			PhysicsCollider physicsCollider = new PhysicsCollider { };
			PhysicsShapeAuthoring physicsShape;
			float radius, height;
			float3 center;
			Quaternion rotation = new quaternion();
			//if default collision filter is detected then create one realted to the pokemon
			if (collisionFilter.Equals(new CollisionFilter()))
			{
				Debug.Log("Creating new Collision Filter");
				collisionFilter = new CollisionFilter
				{
					BelongsTo = TriggerEventClass.Pokemon,
					CollidesWith = uint.MaxValue,
					GroupIndex = 1
				};
			}
			if (material.Equals(new Unity.Physics.Material()))
				material = GetPokemonColliderMaterial(pokemonName);
			switch (pokemonName)
			{
				case "Cubone":
					var colliders = new NativeArray<CompoundCollider.ColliderBlobInstance>(5, Allocator.TempJob);
					colliders[0] = new CompoundCollider.ColliderBlobInstance
					{
						Collider = Unity.Physics.SphereCollider.Create(new SphereGeometry { Center = new float3(0, 0.27f, 0.03f), Radius = 0.225f },collisionFilter,material),
						CompoundFromChild = new RigidTransform{pos = new float3 {x = 0,y = 0,z = 0 },rot = quaternion.identity}
					};
					var a = GenerateCapsuleData(float3.zero,Vector3.right,0.1f,0.3f);
					rotation.SetFromToRotation(Vector3.right, new Vector3(0,90f, 0));
					colliders[1] = new CompoundCollider.ColliderBlobInstance
					{
						Collider = Unity.Physics.CapsuleCollider.Create(new CapsuleGeometry{Vertex0 = a.pointA, Vertex1 = a.pointB, Radius = 0.1f},collisionFilter,material),
						CompoundFromChild = new RigidTransform { pos = new float3(-0.17f, 0.19f, 0), rot = rotation}
					};
					colliders[2] = new CompoundCollider.ColliderBlobInstance
					{
						Collider = Unity.Physics.CapsuleCollider.Create(new CapsuleGeometry { Vertex0 = a.pointA, Vertex1 = a.pointB, Radius = 0.1f },collisionFilter,material),
						CompoundFromChild = new RigidTransform { pos = new float3(0.17f, 0.19f, 0), rot = rotation }
					};
					colliders[3] = new CompoundCollider.ColliderBlobInstance
					{
						Collider = Unity.Physics.SphereCollider.Create(new SphereGeometry { Center = float3.zero, Radius = 0.23f },collisionFilter,material),
						CompoundFromChild = new RigidTransform { pos = new float3(0, 0.75f, 0.03f), rot = rotation }
					};
					a = GenerateCapsuleData(float3.zero, Vector3.right,0.1f, 0.3f);
					rotation = Quaternion.Euler(0,90f,26f);
					colliders[4] = new CompoundCollider.ColliderBlobInstance
					{
						Collider = Unity.Physics.CapsuleCollider.Create(new CapsuleGeometry{Vertex0 = a.pointA, Vertex1 = a.pointB, Radius = 0.1f },collisionFilter,material),
						CompoundFromChild = new RigidTransform { pos = new float3(0, 0.63f, 0.33f),rot = rotation }
					};
					physicsCollider = new PhysicsCollider { Value = CompoundCollider.Create(colliders) };
					colliders.Dispose();
					break;
				case "Electrode":
					physicsCollider = new PhysicsCollider { Value = Unity.Physics.SphereCollider.Create(new SphereGeometry
						{
							Center = float3.zero,
							Radius = ped.Height/2
						},
						collisionFilter,
						material
						
					)};
					break;
			}
			Debug.Log("Returning Physics Collide for \""+pokemonName+"\"");
			return physicsCollider; 
		}

		public static Unity.Physics.Material GetPokemonColliderMaterial(string pokemonName,
			Unity.Physics.Material.MaterialFlags materialFlags = Unity.Physics.Material.MaterialFlags.EnableMassFactors)
		{
			Unity.Physics.Material material = Unity.Physics.Material.Default;
			switch (pokemonName)
			{
				case "Electrode":
					material = Unity.Physics.Material.Default;
					break;
				default:
					Debug.Log("PokemonDataRealted: GetPokemonColliderMaterial: Failed to get a ColliderMaterial for \""+pokemonName+"\"");
					break;
			}
			return material;
		}
		/// <summary>
		/// Converts the Physics Material into a readable string
		/// </summary>
		/// <param name="material">material to stringify</param>
		/// <returns>string</returns>
		public string PhysicsMaterialToString(Unity.Physics.Material material)
		{
			return "Unity.Physics.Material: \nEnableCollision: "+material.EnableCollisionEvents+", EnableMassFactors: "+material.EnableMassFactors+", EnableSurfaceVelocity: "+material.EnableSurfaceVelocity+", IsTrigger: "+material.IsTrigger+
				"\nCustom Tags: "+material.CustomTags.ToString()+"\nFriction: "+material.Friction.ToString()+"\nRestitution: "+material.Restitution.ToString();
		}

		public struct CapsuleData
		{
			public float3 pointA;
			public float3 pointB;
			public float radius;
			public override string ToString(){
				return new Vector3(pointA.x,pointA.y,pointA.z).ToString("F4") + "," +
					new Vector3(pointB.x, pointB.y, pointB.z).ToString("F4")  + "," + radius;
			}
		}
		//	sin(a) = o/h
		//  cos(a) = a/h
		private static CapsuleData GenerateCapsuleData(Vector3 center, Vector3 direction,float length,
			float radius)
		{
			Vector3 d2 = direction.normalized * length / 2f;
			return new CapsuleData
			{
				pointA = center - d2,
				pointB = center + d2,
				radius = radius
			};
		}
	}
	//# Pokémon 	HP 	Attack 	Defense 	Sp. Attack 	Sp. Defense 	Speed 	Total 	Average
	
}
