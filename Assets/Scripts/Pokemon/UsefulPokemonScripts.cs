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
using System;
using Core.Spawning;
using Core;
using Core.UI;

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
		public static RenderMesh LoadPokemonRenderMesh(string pokemonName)
		{
		///	string dataPath = Application.dataPath + "/Resources/Pokemon/" + pokemonName + "/" + pokemonName + ".prefab";
		//	if (!File.Exists(dataPath))
		//	{
		//		Debug.LogError(dataPath);
		//	}
			GameObject go = Resources.Load("Pokemon/" + pokemonName + "/" + pokemonName, typeof(GameObject)) as GameObject;
			if (go == null) Debug.LogError("Failed to get the render mesh gameobject");
			else Debug.Log("Succesfully Loaded \""+pokemonName+"\"");
			//verify this works with physics
			RenderMesh renderer = new RenderMesh
			{
				mesh = go.GetComponent<MeshFilter>().sharedMesh,
				material = go.GetComponent<MeshRenderer>().sharedMaterial,
				castShadows = UnityEngine.Rendering.ShadowCastingMode.On,
				receiveShadows = true
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
		//Pokemon Entity Data Related

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
		
		public static void LoadPokemonEntity(Entity entity,EntityManager entityManager,PlayerSaveData psd)
		{
			string pokemonName = ByteString30ToString(psd.playerData.PokemonName);
			string playerName = ByteString30ToString(psd.playerData.Name);
			entityManager.SetName(entity,playerName+":"+pokemonName);
			//get and add renderMesh
			RenderMesh renderMesh = LoadPokemonRenderMesh(pokemonName);
			if (entityManager.HasComponent<RenderMesh>(entity)) entityManager.SetSharedComponentData(entity, renderMesh);
			else entityManager.AddSharedComponentData(entity, renderMesh);
			if (entityManager.HasComponent<PokemonEntityData>(entity)) entityManager.SetComponentData(entity,psd.pokemonEntityData);
			else entityManager.AddComponentData(entity,psd.pokemonEntityData);
			//add this component (filter to seperate living entities from nonliving)
			if (!entityManager.HasComponent<LivingEntity>(entity)) entityManager.AddComponentData<LivingEntity>(entity, new LivingEntity { });
			//add the CoreData
			if (entityManager.HasComponent<CoreData>(entity)) entityManager.SetComponentData(entity, new CoreData
			{
				Name = StringToByteString30(playerName),
				BaseName = StringToByteString30(pokemonName)
			});
			else entityManager.AddComponentData(entity, new CoreData
				{
					Name = StringToByteString30(playerName),
					BaseName = StringToByteString30(pokemonName)
				});
			//	Debug.LogWarning("Player name = "+playerName+", pokemon name = "+pokemonName);
			//add the UI Components(s)
			//		Core.UI.UIDataClass.GenerateEntityUIGameObject(entityManager, entity);
			entityManager.AddComponentData<UIComponentRequest>(entity,new UIComponentRequest { addToWorld = false});
		
			//add third and first person camera offets
			PokemonDataClass.SetPokemonCameraData(pokemonName, entity, entityManager);
			
			//add the state data
			if (!entityManager.HasComponent<StateData>(entity)) entityManager.AddComponentData(entity, new StateData { });
			//add the PhysicsCollider data
			PhysicsCollider ps = PokemonDataClass.getPokemonPhysicsCollider(pokemonName, psd.pokemonEntityData,new CollisionFilter
			{
				BelongsTo = TriggerEventClass.Collidable | TriggerEventClass.Pokemon | TriggerEventClass.Player,
				CollidesWith = TriggerEventClass.Collidable | TriggerEventClass.Pokemon,
				GroupIndex = 1
			});
			Debug.Log("Creating player eith group index of " + ps.Value.Value.Filter.GroupIndex.ToString());
			if(!entityManager.HasComponent<PhysicsCollider>(entity)) entityManager.AddComponentData(entity, ps);
			else entityManager.SetComponentData(entity, ps);

			//add mass
			if (!entityManager.HasComponent<PhysicsMass>(entity)) entityManager.AddComponentData(entity, PhysicsMass.CreateDynamic(MassProperties.UnitSphere, psd.pokemonEntityData.Mass));
			else entityManager.SetComponentData(entity, PhysicsMass.CreateDynamic(MassProperties.UnitSphere, 1f));
			//add physics velocity
			if (!entityManager.HasComponent<PhysicsVelocity>(entity)) entityManager.AddComponentData(entity, new PhysicsVelocity { });
			else entityManager.SetComponentData<PhysicsVelocity>(entity, new PhysicsVelocity { });
			
				//add position and rotation
					if (!entityManager.HasComponent<Translation>(entity))
					{
						Debug.Log("Adding translation!");
						entityManager.AddComponentData(entity, new Translation { Value = float3.zero });
					}
					//	else entityManager.SetComponentData(entity, new Translation { Value = float3.zero });
					if (!entityManager.HasComponent<Rotation>(entity))
					{
						Debug.Log("Adding Rotation!");
						entityManager.AddComponentData(entity, new Rotation { Value = new quaternion() });
					}
					//	else entityManager.SetComponentData(entity, new Rotation { Value = new quaternion() });

					if (!entityManager.HasComponent<Scale>(entity))
					{
						Debug.Log("Adding Scale");
						entityManager.AddComponentData(entity, new Scale { Value = 1f });
					}
				//	else entityManager.SetComponentData(entity, new Scale { Value = 1f });
					//add the group index
					entityManager.AddComponentData(entity, new GroupIndexInfo
					{
						GroupIndex = 1,
						oldIndexGroup = 1
					});
					//add entity to the GroupIndexSystem
					entityManager.AddComponentData(entity, new GroupIndexChangeRequest
					{
						newIndexGroup = 1,
						pokemonName = psd.playerData.PokemonName
					});
		}

		public static void LoadPlayerData(EntityManager entityManager, Entity entity, string playerName)
		{
			FileStream file = null;
			PlayerSaveData psd = new PlayerSaveData { };
			try
			{
				BinaryFormatter bf = new BinaryFormatter();
				file = File.Open(Application.persistentDataPath + "/Players/" + playerName + ".dat", FileMode.Open);
				psd = (PlayerSaveData)bf.Deserialize(file);
			}
			catch (Exception e)
			{
				if (e != null)
				{
					//handle exception
					Debug.LogError("Failed to load the player data");
				}

			}
			finally
			{
				if (file != null) file.Close();
				if (psd.isValid)
				{
					Debug.Log("Loading player data, pokemon = '"+PokemonIO.ByteString30ToString(psd.playerData.Name)+"' pokemon = '"+ PokemonIO.ByteString30ToString(psd.playerData.PokemonName)+"'");
					string pokemonName = ByteString30ToString(psd.playerData.PokemonName);
					if (entityManager.HasComponent<PlayerData>(entity)) entityManager.SetComponentData(entity, psd.isValid ? psd.playerData : new PlayerData { Name = StringToByteString30(playerName), PokemonName = StringToByteString30("Bulbasaur") });
					else entityManager.AddComponentData(entity, psd.isValid ? psd.playerData : new PlayerData { Name = StringToByteString30(playerName), PokemonName = StringToByteString30("Electrode") });
					LoadPokemonEntity(entity, entityManager, psd);
				}
				else
				{
					Debug.LogWarning("Attempting to save new player data and reload");
					string pokemonName = ByteString30ToString(psd.playerData.PokemonName);
					if (pokemonName == "") pokemonName = "Electrode";
					PlayerData pd = new PlayerData { Name = StringToByteString30(playerName), PokemonName = StringToByteString30(pokemonName) };
					PokemonEntityData ped = PokemonDataClass.GenerateBasePokemonEntityData(pokemonName);
					if (SavePlayerData(pd, ped))
						LoadPlayerData(entityManager, entity, playerName);
				}
			}
		}
		
		public static bool SavePlayerData(PlayerData pd,PokemonEntityData ped)
		{
			FileStream file = null;
			PlayerSaveData psd = new PlayerSaveData
			{
				playerData = pd,
				pokemonEntityData = ped,
				isValid = true
			};
			try
			{
				BinaryFormatter bf = new BinaryFormatter();
				file = File.Create(Application.persistentDataPath + "/Players/" + ByteString30ToString(psd.playerData.Name) + ".dat");
				bf.Serialize(file,psd);
				return true;
			}
			catch (Exception e)
			{
				if(e != null)
				{
					//handle exception
					Debug.LogError("Failed to save the player data"+e);
				}
			}
			finally
			{
				if(file!=null)file.Close();
			}
			return false;
		}
		//General stuff
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