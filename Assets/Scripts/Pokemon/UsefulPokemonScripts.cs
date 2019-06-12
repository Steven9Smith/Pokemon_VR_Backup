using Pokemon.Player;

using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Physics;
using Unity.Mathematics;

namespace Pokemon
{
	public class PokemonIO
	{
		public const byte TYPE_ENVIROMENT = 0;
		public const byte TYPE_POKEMON = 1;
		public const byte TYPE_PLAYER = 2;
		public static ushort currentId = 0;

		//create changePokemon
		//create generateId
		/// <summary>
		/// generates a new Load RenderMesh value using the given values
		/// </summary>
		/// <param name="pokemonName"></param>
		/// <returns></returns>
		public static RenderMesh LoadMeshInstaceRenderer(string pokemonName)
		{
			string dataPath = Application.dataPath + "/Resources/Pokemon/" + pokemonName + "/" + pokemonName + ".prefab";
			if (!File.Exists(dataPath))
			{
				Debug.LogError(dataPath);
			}
			Debug.Log("pokemonName = " + pokemonName);
			GameObject go = Resources.Load("Pokemon/" + pokemonName + "/" + pokemonName, typeof(GameObject)) as GameObject;
			//verify this works with physics
			UnityEngine.Material _mat = Resources.Load("Pokemon/" + pokemonName + "/" + pokemonName, typeof(UnityEngine.Material)) as UnityEngine.Material;
			UnityEngine.Mesh _mesh = go.GetComponent<MeshFilter>().sharedMesh;
			RenderMesh renderer = new RenderMesh
			{
				mesh = _mesh,
				material = _mat
			};

			return renderer;
		}
		public static RenderMesh LoadEnviromentRenderMesh(EnviromentData enviromentData, string startPath)
		{
			startPath += ByteString30ToString(enviromentData.entityName) + "/" + ByteString30ToString(enviromentData.entityName);
			Debug.Log("adasdadasd" + startPath);
			GameObject go = Resources.Load("Enviroment/Models/" + startPath, typeof(GameObject)) as GameObject;
			UnityEngine.Material _mat = Resources.Load("Enviroment/Models/" + startPath, typeof(UnityEngine.Material)) as UnityEngine.Material;
			UnityEngine.Mesh _mesh = go.GetComponent<MeshFilter>().sharedMesh;
			RenderMesh renderer = new RenderMesh
			{
				mesh = _mesh,
				material = _mat
			};

			return renderer;
		}


		/// <summary>
		///	Returns the path of the entities base on the given type
		/// </summary>
		/// <param name="type">type of entity to laod
		/// <para>0 = enviroment entity   1 = pokemon entity   2 = player entity</para>
		/// </param>
		/// <returns></returns>
		public static List<string> GetStoredEntitiesPath(byte type)
		{
			string path = "";
			List<string> entities = new List<string>();
			switch (type)
			{
				case TYPE_ENVIROMENT: //enviroment
					path = Application.dataPath + "/Resources/Scenes/" + GetCurrentSceneName() + "/IEnviroment";
					PrintString("LoadStoredEnviromentEntities", "Loading Enviroment Entities with starting Path: \"" + path + "\"");
					break;
				case TYPE_POKEMON://Pokemon
					path = Application.dataPath + "/Resources/Scenes/" + GetCurrentSceneName() + "/IPokemon";
					PrintString("LoadStoredEnviromentEntities", "Loading Pokemon Entities with string: \"" + path + "\"");
					break;
				case TYPE_PLAYER: //Player
					PrintString("LoadStoredEnviromentEntities", "Loading Player Pokemon Entities");
					path = Application.dataPath + "/Resources/Player/Data";
					try
					{
						List<string> IPokemon = new List<string>(Directory.EnumerateFiles(path));
						//Debug.Log(IPokemonI.Count);
						for (int j = 0; j < IPokemon.Count; j++)
						{
							//file is either:
							//Player.dat
							//Player.data.meta
							if (!IPokemon[j].Contains("meta"))//pokemon is meta
							{
								//	entities.Add(IPokemon[j].Substring(IPokemon[j].LastIndexOf('\\')).Remove(0, 1));
								entities.Add(IPokemon[j].Replace('\\', '/'));
							}
						}

					}
					catch (System.UnauthorizedAccessException UAEx)
					{
						Debug.LogError(UAEx.Message);
					}
					catch (PathTooLongException PathEx)
					{
						Debug.LogError(PathEx.Message);
					}
					return entities;
				default:
					PrintString("LoadStoredEntities", "Failed to load stored entities with type: " + type, 2);
					return entities;
			}
			try
			{
				List<string> IPokemon = new List<string>(Directory.EnumerateDirectories(path));
				for (int i = 0; i < IPokemon.Count; i++)
				{
					List<string> IPokemonI = new List<string>(Directory.EnumerateFiles(IPokemon[i].Replace('\\', '/')));
					for (int j = 0; j < IPokemonI.Count; j++)
					{
						if (!IPokemonI[j].Contains("meta"))//pokemon is meta
						{
							//	entities.Add(IPokemon[i].Substring(IPokemon[i].LastIndexOf('\\')).Remove(0, 1) + ":" + IPokemonI[j].Substring(IPokemonI[j].LastIndexOf("\\")).Remove(0, 1));
							entities.Add(IPokemonI[j].Replace('\\', '/'));
						}
					}
				}
			}
			catch (System.UnauthorizedAccessException UAEx)
			{
				Debug.LogError(UAEx.Message);
			}
			catch (PathTooLongException PathEx)
			{
				Debug.LogError(PathEx.Message);
			}
			return entities;
		}

		//Enviroment Related
		/// <summary>
		/// Loads the Envieoment Entity Data with the geiven arguments
		/// </summary>
		/// <param name="enviromentDataPath">path to the enviroment data</param>
		/// <param name="enviromentData">reference to a enviomentData variable</param>
		/// <param name="position">reference to position</param>
		/// <param name="rotation">reference to Rotation</param>
		/// <param name="scale">reference to Scale</param>
		/// <returns>true if successful, false otherwise</returns>
		public static bool LoadEnviromentData(string enviromentDataPath, ref EnviromentData enviromentData, ref Translation position, ref Rotation rotation, ref Scale scale)
		{
			if (File.Exists(enviromentDataPath))
			{
				BinaryFormatter bf = new BinaryFormatter();
				Debug.Log("assaas: " + enviromentDataPath);
				FileStream file = File.Open(enviromentDataPath, FileMode.Open);
				EnviromentEntityDataSave enviromentEntityDataSave = new EnviromentEntityDataSave();
				try { enviromentEntityDataSave = (EnviromentEntityDataSave)bf.Deserialize(file); }
				catch (System.ArgumentNullException ex) { PrintString("LoadEnviromentData", "The given file information was null or invalid:" + ex.Message, 2); file.Close(); return false; }
				catch (SerializationException ex) { PrintString("LoadEnviromentData", "Failed to Deserialize the data with path:" + enviromentDataPath + "," + ex.Message); file.Close(); return false; }
				catch (System.Security.SecurityException ex) { PrintString("LoadEnviromentData", "Deserialization security Exception:" + ex.Message); file.Close(); return false; }
				enviromentData = new EnviromentData() {
					BoundType = enviromentEntityDataSave.BoundType,
					entityName = enviromentEntityDataSave.EntityName,
					entityId = enviromentData.entityId,
					entityParentId = enviromentData.entityParentId,
					pathString = enviromentData.pathString
				};
				position = enviromentEntityDataSave.Position;
				rotation = enviromentEntityDataSave.Rotation;
				scale = enviromentEntityDataSave.Scale;
				file.Close();
				return true;
			}
			else
			{
				PrintString("LoadEnviromentData", "Failed to load enviroment data with path: \"" + enviromentDataPath + "\"", 2);
			}
			return false;
		}
		/// <summary>
		/// Saves the EnviromentData 
		/// </summary>
		/// <param name="enviromentData">Enviroment Data to save</param>
		/// <param name="position">Position</param>
		/// <param name="rotation">Rotation</param>
		/// <param name="scale">Scale</param>
		public static void SaveEnviromentData(EnviromentData enviromentData, Translation position, Rotation rotation,Scale scale)
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			string dataPath = Application.dataPath + "/Resources/Scenes/" + GetCurrentSceneName() + "/IEnviroment/" +
				GenerateObjectName(GetObjectType(enviromentData.pathString)) + "/" + ByteString30ToString(enviromentData.entityId) + ".dat";
			FileStream file = File.Open(dataPath, FileMode.OpenOrCreate);
			EnviromentEntityDataSave enviromentEntityDataSave = new EnviromentEntityDataSave() {
				BoundType = enviromentData.BoundType,
				EntityId = enviromentData.entityId,
				EntityName = enviromentData.entityName,
				EntityParentId = enviromentData.entityParentId,
				PathString = enviromentData.pathString,
				Position = position,
				Rotation = rotation,
				Scale = scale
			};
			binaryFormatter.Serialize(file, enviromentEntityDataSave);
			file.Close();
		}
		/// <summary>
		/// generates a path based on the pathSring
		/// </summary>
		/// <param name="enviromentData">data to process</param>
		/// <returns></returns>
		public static string GenerateEnviromentPath(EnviromentData enviromentData)
		{
			string a = "";
			switch (enviromentData.pathString.A)
			{
				case 0: a += "Kanto/"; break;
				default: PrintString("GenerateEnviromentPath", "unable to determine path A", 1); a += "Kanto"; break;
			}
			switch (enviromentData.pathString.B)
			{
				case 0: a += "Forest/"; break;
				case 1: a += "Desert/"; break;
				case 2: a += "Town/"; break;
				default: PrintString("GenerateEnviromentPath", "unable to determine path B", 1); a += "Forest/"; break;
			}
			//uses Generate ObjectName
			a += GenerateObjectName(enviromentData.pathString.C) + "/";
			if (enviromentData.pathString.length > 3)
			{
				//town specific stuff
				switch (enviromentData.pathString.D)
				{
					case 0: a += "Buildings/"; break;
					case 1: a += "People/"; break;
					default: PrintString("GenerateEnviromentPath", "unable to determine path D", 1); a += "People/"; break;
				}
			}
			return a;
		}
		/// <summary>
		/// Generates the stirng of the object type
		/// </summary>
		/// <param name="type">thing to be converted</param>
		/// <returns></returns>
		private static string GenerateObjectName(byte type)
		{
			switch (type)
			{
				case 0: return "Landscapes";
				case 1: return "Buildings";
				case 255: return "PalletTown";
				default: PrintString("GeneratePbjectType", "unable to determine object type woth byte \"" + type + "\"", 1); return "Landscapes";
			}
		}
		/// <summary>
		/// returns the enviromentPath using the given Path string
		/// </summary>
		/// <param name="pathString">pathString to cbe heck</param>
		/// <returns></returns>
		public static byte GetEnviromentType(ByteString30 pathString) { return pathString.B; }
		/// <summary>
		/// returns the object type using the given path string 
		/// </summary>
		/// <param name="pathString">pathStirng to be checked</param>
		/// <returns></returns>
		public static byte GetObjectType(ByteString30 pathString) { return pathString.C; }
		/// <summary>
		/// creates a new EnviromentDataEntity saves saves it in IEnviroment (For Dev Use Only)
		/// </summary>
		/// <param name="enviromentId">the id of the enviroment entity</param>
		/// <param name="enviromentName">the name</param>
		/// <param name="enviromentParentId">the id of the parent (if it ha one)</param>
		/// <param name="pathString">a path string see GenerateEnviromentPath for more explination</param>
		/// <param name="enviromentType">the type of enviroment</param>
		/// <param name="boundType">the type of bound
		/// <para>0 = floor   1 = wall   2 = roof   3= solid</para>
		/// </param>
		/// <param name="position">the position of the entity</param>
		/// <param name="rotation">the rotation of the eneity</param>
		/// <param name="scale">the scale of the eneity</param>
		public static void CreateNewEnviromentData(byte boundType, ByteString30 enviromentId, ByteString30 enviromentName, ByteString30 enviromentParentId, ByteString30 pathString,
			Translation position = new Translation(), Rotation rotation = new Rotation(), Scale scale = new Scale())
		{
			EnviromentData enviromentData = new EnviromentData()
			{
				BoundType = boundType,
				entityId = enviromentId,
				entityName = enviromentName,
				entityParentId = enviromentParentId,
				pathString = pathString,
			};
			//	if (scale.Value == 0f && scale.Value.y == 0f && scale.Value.z == 0f) scale.Value = new float3(0f,0f,0f); 
			SaveEnviromentData(enviromentData, position, rotation, scale);
		}
		//Player Related
		/// <summary>
		/// Loads the pokemon data using its location which is generated using the scene name, pokemon name, and id of the entity
		/// </summary>
		/// <param name="entityId">id of the entity</param>
		/// <returns></returns>
		public static bool LoadPlayerData(string playerDataPath, ref PlayerData playerData, ref Translation position, ref Rotation rotation)
		{
			//	string dataPath = Application.dataPath + "/Resources/Player/Data/" + playerId+".dat";
			if (File.Exists(playerDataPath))
			{
				BinaryFormatter bf = new BinaryFormatter();
				FileStream file = File.Open(playerDataPath, FileMode.Open);
				PlayerDataSave playerDataSave = new PlayerDataSave();
				try { playerDataSave = (PlayerDataSave)bf.Deserialize(file); }
				catch (System.ArgumentNullException ex) { PrintString("LoadPlayerData", "The given file information was null or invalid:" + ex.Message, 2); return false; }
				catch (SerializationException ex) { PrintString("LoadPlayerData", "Failed to Deserialize the data with path:" + playerDataPath + "," + ex.Message); return false; }
				catch (System.Security.SecurityException ex) { PrintString("LoadPlayerData", "Deserialization security Exception:" + ex.Message); return false; }
				playerData = new PlayerData() {
					Name = playerDataSave.Name,
//					PokemonEntityData = playerDataSave.PokemonEntityData,
					PokemonName = playerDataSave.PokemonName
				};
				position = playerDataSave.Position;
				rotation = playerDataSave.Rotation;
				file.Close();
				return true;
			}
			else
			{
				Debug.LogWarning("Detected player has invliad or missing pokemon data,");
			}
			return false;
		}
		/// <summary>
		/// creates new Player Data
		/// </summary>
		/// <param name="pokemonName">Name of the  pokemon</param>
		/// <param name="playerId">player's name/Id</param>
		/// <param name="pokedexEntry"></param>
		/// <returns></returns>
		public static PlayerData CreateNewPlayerData(string playerId, PokemonEntityData pokemonEntityData, string pokemonName = "_00")
		{
			return new PlayerData
			{
				Name = StringToByteString30(playerId),
				PokemonName = StringToByteString30(pokemonName),
//				PokemonEntityData = pokemonEntityData
			};
		}
		/// <summary>
		/// Saves the player data in the generated path using the given values
		/// </summary>
		/// <param name="data">PokemonData to be saved</param>
		/// <param name="entityId">id of entity</param>
		public static void SavePlayerData(PlayerData playerData, Translation position = new Translation(), Rotation rotation = new Rotation())
		{
			BinaryFormatter bf = new BinaryFormatter();
			string dataPath = Application.dataPath + "/Resources/Player/Data/" + ByteString30ToString(playerData.Name) + ".dat";
			FileStream file;
			file = File.OpenWrite(dataPath);
			PlayerDataSave playerDataSave = new PlayerDataSave() {
				Name = playerData.Name,
	//			PokemonEntityData = playerData.PokemonEntityData,
				Rotation = rotation,
				Position = position,
				PokemonName = playerData.PokemonName
			};
			bf.Serialize(file, playerDataSave);
			file.Close();
		}

		//Pokemon Entity Data Related

		/// <summary>
		/// Saves a Pokemon Entity
		/// </summary>
		/// <param name="position">current position of entity</param>
		/// <param name="rotation">current rotation of entity</param>
		/// <param name="pokemonEntity">Pokemon Entity to be saved</param>
	/*	public static void SavePokemonEntity(PokemonEntity pokemonEntity, Translation position = new Translation(), Rotation rotation = new Rotation(), Velocity Velocity = new Velocity(), Mass mass = new Mass())
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file;
	//		string dataPath = Application.dataPath + "/Resources/Scenes/" + GetCurrentSceneName() + "/IPokemon/" + PokedexEntryToString(pokemonEntity.PokemonEntityData.PokedexNumber) + "/" + ByteString30ToString(pokemonEntity.EntityId) + ".dat";
//			if (File.Exists(dataPath))
//				File.Create(dataPath);
//			file = File.OpenWrite(dataPath);
			if (mass.value == 0f)
			{
				Debug.LogWarning("Detected an invalid mass setting the mass to 1");
				mass.value = 1f;
			}
			PokemonEntityDataSave pokemonEntityDataSave = new PokemonEntityDataSave()
			{
	//			PokemonEntityData = pokemonEntity.PokemonEntityData,
				entityId = pokemonEntity.EntityId,
				entityName = pokemonEntity.EntityName,
				Position = position,
				Rotation = rotation,
				Velocity = Velocity,
				Mass = mass

			};
//			bf.Serialize(file, pokemonEntityDataSave);
//			file.Close();
		}*/
		/// <summary>
		/// Loads the PokemonEntity with the proper stuff
		/// </summary>
		/// <param name="entityPath">path to the eneity</param>
		/// <param name="pokemonEntity">the pokemonEntity Component</param>
		/// <param name="position">Position Component</param>
		/// <param name="rotation">Rotation Component</param>
		/// <returns></returns>
		public static bool LoadPokemonEntity(string entityPath, ref PokemonEntity pokemonEntity, ref Translation position, ref Rotation rotation)
		{
			if (File.Exists(entityPath))
			{
				BinaryFormatter bf = new BinaryFormatter();
				FileStream file = File.Open(entityPath, FileMode.Open);
				PokemonEntityDataSave pokemonEntityDataSave;
				try { pokemonEntityDataSave = (PokemonEntityDataSave)bf.Deserialize(file); }
				catch (System.ArgumentNullException ex) { PrintString("LoadPokemonEntity", "The given file information was null or invalid:" + ex.Message, 2); file.Close(); return false; }
				catch (SerializationException ex) { PrintString("LoadPokemonEntity", "Failed to Deserialize the data with path:" + entityPath + "," + ex.Message); file.Close(); return false; }
				catch (System.Security.SecurityException ex) { PrintString("LoadPokemonEntity", "Deserialization security Exception:" + ex.Message); file.Close(); return false; }
				pokemonEntity = new PokemonEntity()
				{
					EntityId = pokemonEntityDataSave.entityId,
					EntityName = pokemonEntityDataSave.entityName,
	//				PokemonEntityData = pokemonEntityDataSave.PokemonEntityData
				};
				position = pokemonEntityDataSave.Position;
				rotation = pokemonEntityDataSave.Rotation;
				file.Close();
				return true;
			}
			else
				PrintString("LoadPokemonEntity", "Failed to load Entity with path: \"" + entityPath + "\"");
			return false;
		}
		/// <summary>
		/// Returns Invalid PokemonEntityData (since the structs can't be null)
		/// </summary>
		/// <returns>PokemonEntityData</returns>
		public static PokemonEntityData CreateInvalidPokemonEntityData()
		{
			PokemonEntityData pokemonEntityData = new PokemonEntityData() { PokedexNumber = 0, Height = -1.0f };
			return pokemonEntityData;
		}
		/// <summary>
		/// Creates new PokemonEntityData based on the given Pokedex Entry (Note the Data can point to invalid pokemon data and should be checked after retreival)
		/// </summary>
		/// <param name="pokedexEntry">This is used to retreive the pokemon data</param>
		/// <returns></returns>
		public static PokemonEntityData CreateNewPokemonEntityData(ushort pokedexEntry)
		{
			PokemonEntityData pokemonEntityData = new PokemonEntityData();
			PokemonData pokemonData = new PokemonData();
			if (LoadPokemonData(PokedexEntryToString(pokedexEntry), ref pokemonData))
			{
				pokemonEntityData = new PokemonEntityData()
				{
					Attack = pokemonData.BaseAttack,
					SpecialAttack = pokemonData.BaseSpecialAttack,
					Hp = pokemonData.BaseHp,
					PokedexNumber = pokemonData.PokedexNumber,
					Height = RandomizeHeight(pokemonData.Height, pokemonData.HeightVariation),
					experienceYield = pokemonData.BaseExpericeYield,
					LevelingRate = pokemonData.LevelingRate,
					Freindship = pokemonData.BaseFriendship,
					Speed = pokemonData.BaseSpeed,
					Acceleration = pokemonData.BaseAcceleration,
					SpecialDefense = pokemonData.BaseSpcialDefense,
					Defense = pokemonData.BaseDefense
				};
				PrintString("CreateNewPokemonEntityData", "Pokemon Acceleration = " + pokemonData.BaseAcceleration, 1);
			}
			else PrintString("CreateNewPokemonEntityData", "Failed to Load Pokemon Data", 2);
			return pokemonEntityData;
		}
		/// <summary>
		/// generates and randomized height based on the default height given and the height varibation range
		/// </summary>
		/// <param name="defaultHeight">base height of the pokemon</param>
		/// <param name="heightVariation">height variation of the pokemon (Default is 0.5f)</param>
		/// <returns></returns>
		public static float RandomizeHeight(float defaultHeight, float heightVariation = 0.5f) { return (defaultHeight * UnityEngine.Random.Range((heightVariation * -1), heightVariation)) + defaultHeight; }
		/// <summary>
		/// generates and randomized Mass based on the default Mass given and the Mass varibation range
		/// </summary>
		/// <param name="defaultMass">base Mass of the pokemon</param>
		/// <param name="MassVariation">Mass variation of the pokemon (Default is 0.5f)</param>
		/// <returns></returns>
		public static float RandomizeMass(float defaultMass, float MassVariation = 0.5f) { return (defaultMass * UnityEngine.Random.Range((MassVariation * -1), MassVariation)) + defaultMass; }
		/// <summary>
		/// Loads stored pokemon data (the pokemon data is different from the entity data)
		/// </summary>
		/// <param name="pokemonName">Name of the pokemon data to be loaded</param>
		/// <returns>PokemonData</returns>
		public static bool LoadPokemonData(string pokemonName, ref PokemonData pokemonData)
		{
			PrintString("LoadPokemonData", "LoadPokemonData:\tLoading Data for Pokemon \"" + pokemonName + "\"");
			pokemonData = new PokemonData();
			string dataPath = Application.dataPath + "/Resources/Pokemon/" + pokemonName + "/" + pokemonName + ".dat";
			if (File.Exists(dataPath))
			{
				BinaryFormatter bf = new BinaryFormatter();
				FileStream file = File.Open(dataPath, FileMode.Open);
				try { pokemonData = (PokemonData)bf.Deserialize(file); }
				catch (System.ArgumentNullException ex) { Debug.LogError("The given file information was null or invalid:" + ex.Message); return false; }
				catch (SerializationException ex) { Debug.LogError("Failed to Deserialize the data for  the pokemon:" + pokemonName + ", with path:" + dataPath + "," + ex.Message); return false; }
				catch (System.Security.SecurityException ex) { Debug.LogError("Deserialization security Exception:" + ex.Message); return false; }
				file.Close();
				Debug.LogWarning("LOADED MASS = " + pokemonData.Mass);
				PrintString("LoadPokemonData", "Finished Loading PokemonData");
				return true;
			}
			else
			{
				PrintString("LoadPokemonData", "Failed to Load PokemonData for \"" + pokemonName + "\" with path \"" + dataPath + "\"", 2);
				//create blank data with some easy to test for defaults
				return false;
			}
		}
		/// <summary>
		/// Creates and Saves new pokemon data (For Dev use Only)
		/// </summary>
		/// <param name="pokedexNumber">number entry in the pokedex</param>
		/// <param name="catchRate">pokemon base catch Chance</param>
		/// <param name="maleChance">changce of a male spawning</param>
		/// <param name="eggGroup">pokemon egg group
		/// <para>0 = Monster   1 = Human-Like  2 = Water 1		4 = Water 2			5 = Water 3</para>
		/// <para>6 = Bug		7 = Mineral		8 = Flying		9 = Amorphous		10 = Field</para>
		/// <para>11 = Fairy	12 = Ditto		13 = Grass		14 = Dragon			15 = Undiscovered</para>
		/// </param>
		/// <param name="minimumSteps">minimum steps to hatch pokemon</param>
		/// <param name="maximumSteps">maximim steps to hatch pokemon</param>
		/// <param name="height">pokemon base height</param>
		/// <param name="Mass">pokemon base Mass</param>
		/// <param name="baseExperienceYield">pokemon base experience yield</param>
		/// <param name="levelingRate">pokemon base leveling rate</param>
		/// <param name="baseFriendship">pokemon base freindship</param>
		/// <param name="baseHp">pokemon base HP</param>
		/// <param name="baseAttack">pokemon base Attack</param>
		/// <param name="baseDefense">pokemon baseDefense</param>
		/// <param name="baseSpeed">pokemon base Speed</param>
		/// <param name="baseAcceleration">pokemon base Acceleration</param>
		/// <param name="baseSpecialAttack">pokemon base Special Attack</param>
		/// <param name="baseSpecialDefense">pokemon special Defense</param>
		/// <returns></returns>
		public static void SavePokemonData(string pokemonName, ushort pokedexNumber = 0, float catchRate = 0.01f, float maleChance = 1.0f, ushort eggGroup = 0,
			ushort minimumSteps = 10, ushort maximumSteps = 11, float height = 10f, float heightVariation = 0.5f, float Mass = 1.0f, float wightVariation = 0.5f,
			ushort baseExperienceYield = 1, ushort levelingRate = 1, ushort baseFriendship = 1, ushort baseHp = 1, ushort baseAttack = 1, ushort baseDefense = 1,
			float baseSpeed = 10f, float baseAcceleration = 10f, ushort baseSpecialAttack = 1, ushort baseSpecialDefense = 1)
		{
			PokemonData data = new PokemonData() {
				PokedexNumber = pokedexNumber,
				CatchRate = catchRate,
				MaleChance = maleChance,
				EggGroup = eggGroup,
				MinimumSteps = minimumSteps,
				MaximumSteps = maximumSteps,
				Height = height,
				HeightVariation = heightVariation,
				Mass = Mass,
				MassVariation = wightVariation,
				BaseExpericeYield = baseExperienceYield,
				LevelingRate = levelingRate,
				BaseFriendship = baseFriendship,
				BaseHp = baseHp,
				BaseDefense = baseDefense,
				BaseAttack = baseAttack,
				BaseSpeed = baseSpeed,
				BaseSpecialAttack = baseSpecialAttack,
				BaseSpcialDefense = baseSpecialDefense,
				BaseAcceleration = baseAcceleration
			};
			BinaryFormatter bf = new BinaryFormatter();
			string dataPath = Application.dataPath + "/Resources/Pokemon/" + pokemonName;
			if (File.Exists(dataPath))
			{
				//Folder Exists
				dataPath += "/" + pokemonName + ".dat";
				FileStream file = File.Open(dataPath + "/" + pokemonName + ".dat", FileMode.OpenOrCreate);
				bf.Serialize(file, data);
				file.Close();
			}
			else
			{
				//create missing directory
				Directory.CreateDirectory(dataPath);
				FileStream file = File.Open(dataPath + "/" + pokemonName + ".dat", FileMode.OpenOrCreate); // returns a FileInfo object
				bf.Serialize(file, data);
				file.Close();
			}
		}

		//General stuff
		/// <summary>
		/// Loads the entity based on the given parameters
		/// </summary>
		/// <param name="entityManager">an EntityManager</param>
		/// <param name="entity">an Entity</param>
		/// <param name="entityPath">path the the entity to load the data from</param>
		/// <param name="entityType">type of entity<para>0 = enviroment   1 = pokemon   2 = player</para></param>
	/*	public static void LoadEntity(EntityManager entityManager, ref Entity entity, string entityPath, byte entityType)
		{
			if (File.Exists(entityPath))
			{
				BinaryFormatter bf = new BinaryFormatter();
				Translation position = new Translation();
				Rotation rotation = new Rotation();
				Scale scale = new Scale();
				Velocity velocity = new Velocity();
				Mass mass = new Mass();
				switch (entityType)
				{
					case TYPE_ENVIROMENT:
						{
							Debug.Log("LoadEntity:\tLoading Enviroment Entity");
							EnviromentData enviromentData = new EnviromentData();
							string enviromentPath = GenerateEnviromentPath(enviromentData);
							if (LoadEnviromentData(entityPath, ref enviromentData, ref position, ref rotation, ref scale, ref velocity, ref mass))
							{
								velocity.id = currentId;
								currentId++;
								entityManager.AddSharedComponentData(entity, LoadEnviromentRenderMesh(enviromentData, enviromentPath));
								entityManager.SetComponentData(entity, enviromentData);
								entityManager.SetComponentData(entity, position);
								entityManager.SetComponentData(entity, rotation);
								entityManager.SetComponentData(entity, scale);
								entityManager.SetComponentData(entity, velocity);
								entityManager.SetComponentData(entity, mass);
								entityManager.SetComponentData(entity, new Friction() { value = 0.02f });
							}
							break;
						}
					case TYPE_POKEMON:
						{
							Debug.Log("LoadEntity:\tLoading Pokemon Entity");
							//create some place holder variables
							PokemonEntity pokemonEntity = new PokemonEntity();
							if (LoadPokemonEntity(entityPath, ref pokemonEntity, ref position, ref rotation, ref velocity, ref mass))
							{
								velocity.id = currentId;
								currentId++;
								//add the mesh render component
								entityManager.AddSharedComponentData(entity,
				//					 LoadMeshInstaceRenderer(PokedexEntryToString(pokemonEntity.PokemonEntityData.PokedexNumber)));
								//Set PokemonEntity COmponent
								entityManager.SetComponentData(entity, pokemonEntity);
								//Set Position Component
								position = new Translation() { Value = new float3(10f, 0f, 0f) };
								entityManager.SetComponentData(entity, position);
								//Set Rotation Component
								entityManager.SetComponentData(entity, rotation);
								entityManager.SetComponentData(entity, velocity);
								entityManager.SetComponentData(entity, mass);
								entityManager.SetComponentData(entity, new Friction() { value = 0.02f });

							}
							else PrintString("LoadEntity", "Failed to Load Entity with path: \"" + entityPath + "\"", 2);
							break;
						}
					case TYPE_PLAYER:
						Debug.Log("LoadEntity:\tLoading Player Entity");
						PlayerData playerData = new PlayerData();
						if (LoadPlayerData(entityPath, ref playerData, ref position, ref rotation, ref velocity, ref mass))
						{
							velocity.id = currentId;
							currentId++;
							//since we loading pokemon we use pokemon type. player types wiull be added later
		//					entityManager.AddSharedComponentData(entity, LoadMeshInstaceRenderer(PokedexEntryToString(playerData.PokemonEntityData.PokedexNumber)));
							entityManager.SetComponentData(entity, playerData);
							entityManager.SetComponentData(entity, position);
							entityManager.SetComponentData(entity, rotation);
							entityManager.SetComponentData(entity, velocity);
							entityManager.SetComponentData(entity, mass);
			//				entityManager.SetComponentData(entity, new PlayerInput { MaxInputVelocity = playerData.PokemonEntityData.Speed, PlayerCurrentAcceleration = playerData.PokemonEntityData.Acceleration });
							entityManager.SetComponentData(entity, new Friction() { value = 0.02f });
						}
						break;
					default:
						PrintString("LoadEntity", "No Entity with type '" + entityType + "' was found with path: \"" + entityPath + "\"");
						break;
				}
			}
			else Debug.LogError("LoadEntity:\tfailed to open the data for the specific entity with path: \"" + entityPath + "\"");
			PrintString("LoadEntity", "Finished Loading Enitty");
		}
		*/
		/// <summary>
		/// Saves and Entity based on its type
		/// </summary>
		/// <param name="entityManager">the entityt manager</param>
		/// <param name="entity">An Entity</param>
		/// <param name="sceneName">Name of current scene</param>
		/// <param name="enviromentName">name of current envioement</param>
		/// <param name="modelName">name of entity</param>
		/// <param name="entityType">type of entity</param>
	/*	public static void SaveEntity(EntityManager entityManager, ref Entity entity, string sceneName, string enviromentName, string modelName, ushort entityType)
		{
			BinaryFormatter bf = new BinaryFormatter();
			switch (entityType)
			{
				case 0:
					//Enviroment models
					SaveEnviromentData(entityManager.GetComponentData<EnviromentData>(entity), entityManager.GetComponentData<Translation>(entity),
						entityManager.GetComponentData<Rotation>(entity), entityManager.GetComponentData<Scale>(entity),
						entityManager.GetComponentData<Velocity>(entity), entityManager.GetComponentData<Mass>(entity));
					break;
				case 1:
					//pokemon
					PokemonIO.SavePokemonEntity(entityManager.GetComponentData<PokemonEntity>(entity), entityManager.GetComponentData<Translation>(entity),
						entityManager.GetComponentData<Rotation>(entity),
						entityManager.GetComponentData<Velocity>(entity), entityManager.GetComponentData<Mass>(entity));
					break;
				case 2:
					//player
					PokemonIO.SavePlayerData(entityManager.GetComponentData<PlayerData>(entity),
						entityManager.GetComponentData<Translation>(entity), entityManager.GetComponentData<Rotation>(entity), entityManager.GetComponentData<Velocity>(entity), entityManager.GetComponentData<Mass>(entity));
					break;
				default:
					PokemonIO.PrintString("SaveEntity", "Failed to Save Entity with modelName \"" + modelName + "\"", 2);
					break;
			}
		}
		*/
		/// <summary>
		/// returns the name for the current scene
		/// </summary>
		/// <returns></returns>
		public static string GetCurrentSceneName() { return SceneManager.GetActiveScene().name; }
		//Conversions
		/// <summary>
		/// converts a Byte array to a string
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static string ByteAToString(byte[] bytes) { return Encoding.ASCII.GetString(bytes); }
		/// <summary>
		/// converts a string to a byte[]
		/// </summary>
		/// <param name="myString"></param>
		/// <returns></returns>
		public static byte[] StringToByteA(string myString) { return Encoding.ASCII.GetBytes(myString); }
		/// <summary>
		/// converts a ByteStirng30 into a String
		/// </summary>
		/// <param name="bs"></param>
		/// <returns></returns>
		public static string ByteString30ToString(ByteString30 bs)
		{
			byte[] bytes = new byte[bs.length];
			for (int i = 0; i < bs.length; i++)
			{
				switch (i)
				{
					case 0:
						bytes[i] = bs.A;
						break;
					case 1:
						bytes[i] = bs.B;
						break;
					case 2:
						bytes[i] = bs.C;
						break;
					case 3:
						bytes[i] = bs.D;
						break;
					case 4:
						bytes[i] = bs.E;
						break;
					case 5:
						bytes[i] = bs.F;
						break;
					case 6:
						bytes[i] = bs.G;
						break;
					case 7:
						bytes[i] = bs.H;
						break;
					case 8:
						bytes[i] = bs.I;
						break;
					case 9:
						bytes[i] = bs.J;
						break;
					case 10:
						bytes[i] = bs.K;
						break;
					case 11:
						bytes[i] = bs.L;
						break;
					case 12:
						bytes[i] = bs.M;
						break;
					case 13:
						bytes[i] = bs.N;
						break;
					case 14:
						bytes[i] = bs.O;
						break;
					case 15:
						bytes[i] = bs.P;
						break;
					case 16:
						bytes[i] = bs.Q;
						break;
					case 17:
						bytes[i] = bs.R;
						break;
					case 18:
						bytes[i] = bs.S;
						break;
					case 19:
						bytes[i] = bs.T;
						break;
					case 20:
						bytes[i] = bs.U;
						break;
					case 21:
						bytes[i] = bs.V;
						break;
					case 22:
						bytes[i] = bs.W;
						break;
					case 23:
						bytes[i] = bs.X;
						break;
					case 24:
						bytes[i] = bs.Y;
						break;
					case 25:
						bytes[i] = bs.Z;
						break;
					case 26:
						bytes[i] = bs.AA;
						break;
					case 27:
						bytes[i] = bs.AB;
						break;
					case 28:
						bytes[i] = bs.AC;
						break;
					case 29:
						bytes[i] = bs.AD;
						break;
				}
			}
			/*  byte[] bytes = new byte[]{
                  byteString.A,
                  byteString.B,
                  byteString.C,
                  byteString.D,
                  byteString.E,
                  byteString.F,
                  byteString.G,
                  byteString.H,
                  byteString.I,
                  byteString.J,
                  byteString.K,
                  byteString.L,
                  byteString.M,
                  byteString.N,
                  byteString.O,
                  byteString.P,
                  byteString.Q,
                  byteString.R,
                  byteString.S,
                  byteString.T,
                  byteString.U,
                  byteString.V,
                  byteString.W,
                  byteString.X,
                  byteString.Y,
                  byteString.Z,
                  byteString.AA,
                  byteString.AB,
                  byteString.AC,
                  byteString.AD,
              };
            */
			return ByteAToString(bytes);
		}
		/// <summary>
		/// converts a string into a ByteString30
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public static ByteString30 StringToByteString30(string a)
		{
			ByteString30 bs = new ByteString30();
			byte[] bytes = StringToByteA(a);
			//	Debug.LogWarning(a + ":" + a.Length);
			bs.length = a.Length;
			for (int i = 0; i < 30; i++)
			{
				if (i < bytes.Length)
				{
					//    Debug.Log(a+"["+i+"] = "+bytes[i]+":"+(char)bytes[i]);
					switch (i)
					{
						case 0:
							bs.A = bytes[i];
							break;
						case 1:
							bs.B = bytes[i];
							break;
						case 2:
							bs.C = bytes[i];
							break;
						case 3:
							bs.D = bytes[i];
							break;
						case 4:
							bs.E = bytes[i];
							break;
						case 5:
							bs.F = bytes[i];
							break;
						case 6:
							bs.G = bytes[i];
							break;
						case 7:
							bs.H = bytes[i];
							break;
						case 8:
							bs.I = bytes[i];
							break;
						case 9:
							bs.J = bytes[i];
							break;
						case 10:
							bs.K = bytes[i];
							break;
						case 11:
							bs.L = bytes[i];
							break;
						case 12:
							bs.M = bytes[i];
							break;
						case 13:
							bs.N = bytes[i];
							break;
						case 14:
							bs.O = bytes[i];
							break;
						case 15:
							bs.P = bytes[i];
							break;
						case 16:
							bs.Q = bytes[i];
							break;
						case 17:
							bs.R = bytes[i];
							break;
						case 18:
							bs.S = bytes[i];
							break;
						case 19:
							bs.T = bytes[i];
							break;
						case 20:
							bs.U = bytes[i];
							break;
						case 21:
							bs.V = bytes[i];
							break;
						case 22:
							bs.W = bytes[i];
							break;
						case 23:
							bs.X = bytes[i];
							break;
						case 24:
							bs.Y = bytes[i];
							break;
						case 25:
							bs.Z = bytes[i];
							break;
						case 26:
							bs.AA = bytes[i];
							break;
						case 27:
							bs.AB = bytes[i];
							break;
						case 28:
							bs.AC = bytes[i];
							break;
						case 29:
							bs.AD = bytes[i];
							break;
					}
				}
				else break;
			}
			return bs;
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
				default: return 0;
			}
		}
		/// <summary>
		/// Takes in a ushort pokedex entry number and retuns the name of the pokemon
		/// </summary>
		/// <param name="pokedexEntry"></param>
		/// <returns></returns>
		public static string PokedexEntryToString(ushort pokedexEntry)
		{
			switch (pokedexEntry)
			{
				case 1: return "Bulbasaur";
				case 2: return "Ivysaur";
				case 3: return "Venasaur";
				case 101: return "Electrode";
				default: return "_00";
			}
		}
		/// <summary>
		///	Prints the given string in Debug.Log~
		/// </summary>
		/// <param name="a">the string to be passed</param>
		/// <param name="mode">0 = Log, 1 = Warning, 2 = Error, 3 = Assertion</param>
		public static void PrintString(string functionName, string message, ushort mode = 0) {
			switch (mode)
			{
				case 0: Debug.Log(functionName + "\t" + message); break;
				case 1: Debug.LogWarning(functionName + "\t" + message); break;
				case 2: Debug.LogError(functionName + "\t" + message); break;
				case 3: Debug.LogAssertion(functionName + "\t" + message); break;
				default: Debug.Log(functionName + "\t" + message); break;
			}
		}
		//Other

		/// <summary>
		/// returns a List<string> containing all the paths or names of files inside the given DirectoryPath
		/// </summary>
		/// <param name="dir">directory tp look in</param>
		/// <param name="namesOnly">set to true if you only want the filename (includes exxtension)</param>
		/// <returns></returns>
		public static List<string> GetFileNamesInDir(string dir, bool namesOnly = true)
		{
			List<string> fileNames = new List<string>();
			if (Directory.Exists(dir))
			{
				//create new List<String>
				try
				{
					//try to get files in directory
					fileNames = new List<string>(Directory.EnumerateDirectories(dir));
					//if true only save the names of the files
					if (namesOnly)
					{
						for (int i = 0; i < fileNames.Count; i++)
						{
							fileNames[i] = fileNames[i].Substring(fileNames[i].LastIndexOf('\\')).Remove(0, 1);
						}
					}
				}
				catch (System.UnauthorizedAccessException UAEx)
				{
					Debug.LogError(UAEx.Message);
				}
				catch (PathTooLongException PathEx)
				{
					Debug.LogError(PathEx.Message);
				}
			}
			else Debug.LogError("GetDirectoryNamesInDirectory:\tInvalid directory \"" + dir + "\" was given");

			return fileNames;
		}
		/// <summary>
		/// returns a List<string> containing all the paths or names of Directories inside the given DirectoryPath
		/// </summary>
		/// <param name="dir">directory tp look in</param>
		/// <param name="namesOnly">set to true if you only want the dir name</param>
		/// <returns></returns>
		public static List<string> GetDirectoryNamesInDirectory(string dir, bool namesOnly = true)
		{
			List<string> fileNames = new List<string>();
			if (Directory.Exists(dir)) {
				//create new List<String>
				try
				{
					//try to get files in directory
					fileNames = new List<string>(Directory.EnumerateDirectories(dir));
					//if true only save the names of the files
					if (namesOnly)
					{
						for (int i = 0; i < fileNames.Count; i++)
						{
							fileNames[i] = fileNames[i].Substring(fileNames[i].LastIndexOf('\\')).Remove(0, 1);
						}
					}
				}
				catch (System.UnauthorizedAccessException UAEx)
				{
					Debug.LogError(UAEx.Message);
				}
				catch (PathTooLongException PathEx)
				{
					Debug.LogError(PathEx.Message);
				}
			} else Debug.LogError("GetDirectoryNamesInDirectory:\tInvalid directory \"" + dir + "\" was given");

			return fileNames;
		}

		public static void createPokemonData()
		{
			PrintString("CreatePokemonDataABC123", "Creating Missing or new Base Pokemon Data");
			SavePokemonData("Bulbasaur", 1, 0.59f, 0.875f, 14, 5140, 5396, 0.7f, 0.5f, 6.9f, 0.5f, 64, 2, 70, 45, 49, 49, 45f, 5f, 65, 65);
			SavePokemonData("_00", 1, 0.59f, 0.875f, 14, 5140, 5396, 0.7f, 0.5f, 6.9f, 0.5f, 64, 2, 70, 45, 49, 49, 45f, 5f, 65, 65);
			SavePokemonData("Ivysaur", 2, 0.59f, 0.875f, 14, 5140, 5396, 1.0f, 0.5f, 13.0f, 0.5f, 142, 2, 70, 60, 62, 63, 60f, 3f, 80, 80);
			SavePokemonData("Venasaur", 3, 0.59f, 0.875f, 14, 5140, 5396, 1.0f, 0.5f, 13.0f, 0.5f, 142, 2, 70, 80, 82, 83, 80f, 2f, 100, 100);

		}
		public static void GeneratePokemonBounds()
		{
			//FOR DEV USE ONLY
			//STORES BASE COLLIDER INFO FOR EACH POKEMON

		}
	}
	public class PokemonArchetypes
	{
		public const ushort ENVIROMENT_ARCHTYPE = 1;
		public const ushort TREE_ARCHTYPE = 2;
		public const ushort PLAYER_ARCHTYPE = 3;
		public const ushort POKEMON_ENTITY_ARCHTYPE = 4;
		public const ushort ENVIROMENT_ENTITY_ARCHTYPE = 5;

		/// <summary>
		/// Returns the desired Archtype base on the given paramters
		/// </summary>
		/// <param name="entityManager">manager to generate the archtype</param>
		/// <param name="type">the type of archType to generate</param>
		/// <returns></returns>
		public static EntityArchetype GenerateArchetype(EntityManager entityManager, ushort type)
		{
			switch (type)
			{
				case ENVIROMENT_ARCHTYPE:
					return entityManager.CreateArchetype(
							typeof(Translation),
							typeof(Scale),
							typeof(Rotation),
							typeof(TransformMatrix),
							typeof(EnviromentData),
							//collision specific
							typeof(PhysicsCollider)
						);
				case POKEMON_ENTITY_ARCHTYPE:
					return entityManager.CreateArchetype(
							typeof(Translation),
							typeof(TransformMatrix),
							typeof(Rotation),
							typeof(PokemonEntity),
							typeof(PhysicsCollider)
						);
				case PLAYER_ARCHTYPE:
					return entityManager.CreateArchetype(
							typeof(Translation),
							typeof(TransformMatrix),
							typeof(Rotation),
							typeof(PlayerData),
							typeof(PlayerInput),
							typeof(PhysicsCollider)
						);
				default: //No Valid ArchType so create Base
					PokemonIO.PrintString("GenerateArchType", "Failed to find a matching ArchType generating Base ArchType", 1);
					return entityManager.CreateArchetype(
							typeof(TransformMatrix),
							typeof(Translation)
						);
			}
		}

	}
	public class PokemonConversion {
		public static string PlayerInputToString(PlayerInput pi, bool includeAll = false)
		{
			string temp = "Move:\n\tx: " + pi.Move.x + "\n\ty:" + pi.Move.y + "\n\tz: " + pi.Move.z + "\nRotation:\n\tx: " + pi.Rotation.x + "\n\ty: " + pi.Rotation.y + "\n\tz: " + pi.Rotation.z + "\n\tw :" + pi.Rotation.w;
			temp += includeAll ? "\nSpaceDown: " + pi.SpaceDown + "\nLShiftDown: " + pi.LShiftDown + "\nRShiftDown: " +
				pi.RShiftDown + "\nMouse1Down: " + pi.Mouse1Down + "\nMouse2Down: " + pi.Mouse2Down + "\nLCtrlDown: " + pi.LCtrlDown : "";
			return temp;
		}
	}
	public class PokemonDebug{
		/// <summary>
		/// Display Output
		/// </summary>
		/// <param name="message">message to print</param>
		/// <param name="mode">
		/// type of message to print as
		/// 0 = Debug
		/// 1 = Error
		/// 2 = Warning
		/// 3 = Assertion
		/// </param>
		public static void DO(string message,int mode = 0)
		{
			switch (mode)
			{
				case 0: Debug.Log(message); break;
				case 1: Debug.LogError(message); break;
				case 2: Debug.LogWarning(message); break;
				case 3: Debug.LogAssertion(message); break;
				default: Debug.Log(message); break;
			}
		}
	
	}
}