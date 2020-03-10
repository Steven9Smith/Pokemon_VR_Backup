using Core;
using Pokemon;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;

[RequiresEntityConversion]
public class CoreDataComponent : MonoBehaviour, IConvertGameObjectToEntity
{
	public string Name;
	public string BaseName;
	public new Transform transform;
	public MeshFilter meshFilter;
	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		dstManager.SetName(entity, Name + "|" + BaseName);
		//Debug.Log(Name + "|" + BaseName);
		if (meshFilter == null) Debug.LogError("MeshFilter is null for " + Name);
		else if (transform == null) Debug.LogError("Transform is null for "+Name);


		CoreData cd = new CoreData(new ByteString30(Name), new ByteString30(BaseName), transform, meshFilter.sharedMesh,entity);
		if (!cd.isValid) Debug.LogError("failed to successfully create COreData for To-Be Entity \"" + Name + "\":\"" + BaseName + "\"");
		else
		{
			if (dstManager.HasComponent<CoreData>(entity))
			{
				Debug.LogWarning("CoreDataComponent: resetting CoreData using CoreDataCOmponent");
				dstManager.SetComponentData(entity, cd);
			}
			else dstManager.AddComponentData<CoreData>(entity, cd);
		}
	}
}
namespace Core
{
	/// <summary>
	/// Data that all entities should have that make calculations and entity look up mush easier
	/// </summary>
	[Serializable]
	public struct CoreData : IComponentData
	{
		public ByteString30 Name;
		public ByteString30 BaseName;
		public Entity entity;
		public float3 size;
		public float3 scale;
		public BlittableBool isValid;

		public CoreData(ByteString30 name, ByteString30 baseName, float3 mSize, float3 mScale,Entity _entity)
		{
			Name = name;
			BaseName = baseName;
			size = mSize;
			scale = mScale;
			isValid = true;
			entity = _entity;
		}
		/// <summary>
		/// creates a new CoreData
		/// </summary>
		/// <param name="name">Name of Entity</param>
		/// <param name="baseName">BaseName of Entity</param>
		/// <param name="transform">GameOject transform that the entity derives from</param>
		/// <param name="mesh">GameOject mesh that the entity derives from</param>
		public CoreData(ByteString30 name, ByteString30 baseName, Transform transform, Mesh mesh, Entity _entity)
		{
			Name = name;
			BaseName = baseName;
			entity = _entity;
			if (transform != null && mesh != null)
			{
				try
				{
					scale = transform.localScale;
					size = mesh.bounds.size;
					isValid = true;
				}
				catch
				{
					Debug.LogWarning("CoreData: Unknown Exception");
					scale = new float3();
					size = new float3();
					isValid = false;
				}
			}
			else
			{
				Debug.LogWarning("CoreData: Failed to create CoreData, invalid transform or mesh");
				scale = new float3();
				size = new float3();
				isValid = false;
			}
		}
		/// <summary>
		/// Creates a new CoreData
		/// </summary>
		/// <param name="name">name of the Entity</param>
		/// <param name="baseName">base name of the Entity</param>
		/// <param name="go">GameObject (NOT PREFAB) that the Entity comes from</param>
		public CoreData(ByteString30 name, ByteString30 baseName, GameObject go, Entity _entity)
		{
			Name = name;
			BaseName = baseName;
			entity = _entity;
			if (go != null)
			{
				try
				{
					scale = go.GetComponent<Transform>().localScale;
					size = go.GetComponent<MeshFilter>().sharedMesh.bounds.size;
					isValid = true;
				}
				catch
				{
					Debug.LogWarning("CoreData: Failed to generate CoreData due to a posibly null GoameObject, Transform, or MeshFilter");
					scale = new float3();
					size = new float3();
					isValid = false;
				}
			}
			else
			{
				scale = new float3();
				size = new float3();
				isValid = false;
			}
		}

		public bool Equals(CoreData other)
		{
			if (!other.isValid || !isValid) {
				Debug.LogWarning("CoreData: comparing invalid cds");
				return false;
			}
			return Name.Equals(other.Name) && BaseName.Equals(other.BaseName) && size.Equals(other.size) && scale.Equals(other.scale);
		}
		public override string ToString()
		{
			return "CoreData: " + Name + "," + BaseName + "," + size + "," + scale + "," + isValid.Value;
		}
		public string ToNameString() { return Name + ":" + BaseName; }
	}

	public class CoreFunctionsClass {
		static EntityQuery eq;
		static int i;

		public static World DefaultWorld => World.DefaultGameObjectInjectionWorld;
		/// <summary>
		/// Find an Entity within the active game.
		/// </summary>
		/// <param name="em">EntityManager</param>
		/// <param name="entity">an empty Entity</param>
		/// <param name="name">name of the Entity</param>
		/// <param name="baseName">basename of the Entity</param>
		/// <returns></returns>
		public static bool FindEntity(EntityManager em, ref Entity entity, string name, string baseName = "")
		{
			eq = em.CreateEntityQuery(typeof(CoreData));
			NativeArray<Entity> entities = eq.ToEntityArray(Allocator.TempJob);
			NativeArray<CoreData> cds = eq.ToComponentDataArray<CoreData>(Allocator.TempJob);
			bool ok = false;
			for (i = 0; i < entities.Length; i++)
			{
				Debug.Log("aaaaaaaaaaaaTesting \""+name+"\" =? \""+cds[i].Name.ToString()+"\"");
				if (cds[i].Name.ToString() == name)
				{
					if (baseName != "")
					{
						Debug.Log("bbbbbbbbbbbTesting \""+baseName+"\" =? \""+cds[i].BaseName.ToString()+"\"");
						if (baseName == cds[i].BaseName.ToString())
						{
							entity = entities[i];
							ok = true;
							break;
						}
					}
					else
					{
						entity = entities[i];
						ok = true;
						break;
					}
				}
			}
			entities.Dispose();
			cds.Dispose();
			return ok;
		}
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
		/// This spawn the given entity x number of times within a certain bounds
		/// </summary>
		/// <param name="entityManager">EntityManager</param>
		/// <param name="objectToSpawn">entity that you want to be spawned within an area</param>
		/// <param name="areaBounds">bounds of the area</param>
		/// <param name="areaBoundsPosition">position of the area</param>
		/// <param name="amountOfObjectsToSpawn">amount of entities to spawn</param>
		/// <param name="entitiesToExclude">NativeArray of entities to trigger false</param>
		public static NativeArray<Entity> SpawnEntitiesWithinBounds(EntityManager entityManager, Entity objectToSpawn, Bounds areaBounds, int amountOfObjectsToSpawn, NativeArray<CoreData> entitiesToExclude, float[,] heightMap, bool zeroHeight = true)
		{
			if (!objectToSpawn.Equals(new Entity { }))
			{
				//first we get entities within the area
				NativeArray<Bounds> _exclusions = GetEntitiesWIthinArea(entityManager,areaBounds, entitiesToExclude,false);
				NativeArray<Bounds> exclusions = new NativeArray<Bounds>(_exclusions.Length + amountOfObjectsToSpawn, Allocator.TempJob);
				//setup the random number generator
				Unity.Mathematics.Random rand = new Unity.Mathematics.Random((uint)System.Environment.TickCount); //not truly random but we are getting close
																												  //rand.Next(1, 100);
				CoreData cd = entityManager.GetComponentData<CoreData>(objectToSpawn);
				bool useHeightMap = heightMap.Length > 0 && heightMap.GetLength(0) == cd.size.x && heightMap.GetLength(1) == cd.size.z;

				NativeArray<Entity> newEntities = new NativeArray<Entity>(amountOfObjectsToSpawn, Allocator.TempJob);
				bool[] validEntities = new bool[amountOfObjectsToSpawn];
				entityManager.Instantiate(objectToSpawn, newEntities);
				//objectToSpawn.GetComponent<MeshFilter>().sharedMesh.bounds;
				for(int i = 0; i < amountOfObjectsToSpawn; i++)
				{
					int maxTries = 10;
					int counter = 0;
					while (counter < maxTries)
					{
						//first let's generate a position within the bounds
						float3 newPosition = rand.NextFloat3(float3.zero, (float3)areaBounds.size)+(float3)areaBounds.center;
						//remove extra bounds
						newPosition.x -= areaBounds.extents.x;
						newPosition.z -= areaBounds.extents.z;
						if (useHeightMap) newPosition.y = heightMap[(int)newPosition.x,(int)newPosition.z];
						else if (zeroHeight) newPosition.y = 0;
						else newPosition.y -= areaBounds.extents.y;
						Bounds newBounds = new Bounds(newPosition, cd.size*cd.scale);
						Translation t;
						int j = 0;
						bool overlap = false;
						for (; j < exclusions.Length; j++)
						{
							if (exclusions[i] != null)
							{
								if (exclusions[i].Intersects(newBounds))
								{
									overlap = true;
									break;
								}
							}
						}
						if (!overlap)
						{
							//ok now we have to make sure it doesn't intesect the new entities we made
							for (int k = 0; k < i; k++)
							{
								t = entityManager.GetComponentData<Translation>(newEntities[k]);
								if (!t.Value.Equals(new float3()) && new Bounds(t.Value, cd.size * cd.scale).Intersects(newBounds))
								{
									overlap = true;
									break;
								}
							}

						}
						if (!overlap)
						{
							entityManager.SetComponentData(newEntities[i], new Translation { Value = newPosition });
							entityManager.SetName(newEntities[i], cd.Name + "|" + cd.BaseName + i);
							validEntities[i] = true;
							break;
						}
						else counter++;
					}
					if(!validEntities[i])
					{
						//failed to find a location so lets destroy it
						Debug.LogWarning("Failed to generate a safe space for entity "+i);
						validEntities[i] = false;
					}//elswe do nothing
				}
				for(var i = 0; i < validEntities.Length; i++)
					if(!validEntities[i])
						entityManager.DestroyEntity(newEntities[i]);
				_exclusions.Dispose();
				exclusions.Dispose();
				return newEntities;
			}
			else Debug.LogError("Given Object to spawn is null or invalid");
			return new NativeArray<Entity>(0, Allocator.TempJob);
		}
		/// <summary>
		/// get all entities within a given area
		/// </summary>
		/// <param name="entityManager">EntityManager</param>
		/// <param name="bounds">area bounds</param>
		/// <param name="exclude">list of CoreData to not include witht the result</param>
		/// <param name="onlyXZ">set to true if you do not want to deal with the y bounds</param>
		/// <returns></returns>
		public static NativeArray<Bounds> GetEntitiesWIthinArea(EntityManager entityManager, Bounds bounds, NativeArray<CoreData> exclude, bool onlyXZ = true)
		{
			EntityQuery eq = entityManager.CreateEntityQuery(typeof(Translation), typeof(CoreData));
			NativeArray<Entity> entities = eq.ToEntityArray(Allocator.TempJob);
			NativeArray<Translation> translations = eq.ToComponentDataArray<Translation>(Allocator.TempJob);
			NativeArray<CoreData> cds = eq.ToComponentDataArray<CoreData>(Allocator.TempJob);
			NativeArray<Bounds> ValidCubes = new NativeArray<Bounds>(entities.Length, Allocator.TempJob);
			if (ValidCubes.Length > 0)
			{
				ValidCubes[0] = new Bounds(new float3(-1f, -1f, -1f), new float3(-1f, -1f, -1f));
				int count = 0;
				for (int i = 0; i < entities.Length; i++)
				{
					Debug.Log("testing \"" + entityManager.GetName(entities[i]) + "\" " + i);
					if (cds[i].isValid)
					{
						for (int j = 0; j < exclude.Length; j++)
						{
							if (exclude[j].isValid)
							{
								//		Debug.Log(exclude[j] + "|||" + cds[i] + "|||" + !cds[i].Equals(exclude[j]));
								if (!cds[i].Equals(exclude[j]))
								{
									Bounds b = new Bounds(translations[i].Value, cds[i].size);
									if (bounds.Intersects(b))
									{
										Debug.Log("Adding " + entityManager.GetName(entities[i]) + "to aviodables");
										ValidCubes[count] = b;
										count++;
									}
								}
								else Debug.Log("Detected Exclude on " + exclude[j].Name + "|" + exclude[j].BaseName);
							}
							else break;
						}
					}
					else Debug.LogWarning("Invalid or no CcoreData for \"" + entityManager.GetName(entities[i]) + "\" " + cds[i].ToString() + ",  " + i);
				}
				if (entities.Length == 0) Debug.LogWarning("EnviromentData: failed to get wntities that have translation and coredata!");
				entities.Dispose();
				translations.Dispose();
				cds.Dispose();
			}
			return ValidCubes;
		}
		/// <summary>
		/// returns a list of gameobjects within a given layer
		/// </summary>
		/// <param name="layer">layer</param>
		/// <returns></returns>
		public static GameObject[] FindGameObjectsWithLayer(int layer)
		{
			GameObject[] goArray = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
			List<GameObject> goList = new List<GameObject>();
			for (int i = 0; i < goArray.Length; i++)
				if (goArray[i].layer == layer) goList.Add(goArray[i]);
			if (goList.Count == 0) return null;
			return goList.ToArray();
		}
		/// <summary>
		/// returns all the children within a gameObject
		/// </summary>
		/// <param name="go"></param>
		/// <returns></returns>
		public static GameObject[] GetGameObjectChildren(GameObject go)
		{
			List<GameObject> gos = new List<GameObject>();
			for (int i = 0; i < go.transform.childCount; i++)
				gos.Add(go.transform.GetChild(i).gameObject);
			return gos.ToArray();
		}
		/// <summary>
		/// looks for gameobjects within a list with a given namew
		/// </summary>
		/// <param name="go"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static GameObject FindGameObjectWithName(GameObject[] go, string name)
		{
			if (go != null)
			{
				for (int i = 0; i < go.Length; i++)
					if (go[i].name == name) return go[i];
			}
			return null;
		}
		/// <summary>
		/// Reference Article http://www.codeproject.com/KB/tips/SerializedObjectCloner.aspx
		/// Provides a method for performing a deep copy of an object.
		/// Binary Serialization is used to perform the copy.
		/// Perform a deep Copy of the object.
		/// </summary>
		/// <typeparam name="T">The type of object being copied.</typeparam>
		/// <param name="source">The object instance to copy.</param>
		/// <returns>The copied object.</returns>
		public static T Clone<T>(T source)
		{
			if (!typeof(T).IsSerializable)
			{
				throw new ArgumentException("The type must be serializable.", nameof(source));
			}

			// Don't serialize a null object, simply return the default for that object
			if (System.Object.ReferenceEquals(source, null))
			{
				return default(T);
			}

			IFormatter formatter = new BinaryFormatter();
			Stream stream = new MemoryStream();
			using (stream)
			{
				formatter.Serialize(stream, source);
				stream.Seek(0, SeekOrigin.Begin);
				return (T)formatter.Deserialize(stream);
			}
		}

		/// <summary>
		/// wait for x seconds
		/// </summary>
		/// <param name="seconds"></param>
		/// <returns></returns>
		public static IEnumerator WaitFor(int seconds)
		{
			yield return new WaitForSeconds(seconds);
		}
	}

	//Core structs
	[Serializable]
	public struct StringFloat3
	{
		public string _string;
		public float3 _float3;
	}
	[Serializable]
	public struct String30Float3 : IComponentData
	{
		public ByteString30 string30;
		public float3 _float3;
		public override string ToString()
		{
			return string30.ToString() + " | " + _float3.ToString();
		}
	}
	[Serializable]
	public struct ByteString30 : IComponentData
	{
		public byte A;
		public byte B;
		public byte C;
		public byte D;
		public byte E;
		public byte F;
		public byte G;
		public byte H;
		public byte I;
		public byte J;
		public byte K;
		public byte L;
		public byte M;
		public byte N;
		public byte O;
		public byte P;
		public byte Q;
		public byte R;
		public byte S;
		public byte T;
		public byte U;
		public byte V;
		public byte W;
		public byte X;
		public byte Y;
		public byte Z;
		public byte AA;
		public byte AB;
		public byte AC;
		public byte AD;
		public int length;
		public ByteString30(string a)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(a);
			length = a.Length;
			A = 0;
			B = 0;
			C = 0;
			D = 0;
			E = 0;
			F = 0;
			G = 0;
			H = 0;
			I = 0;
			J = 0;
			K = 0;
			L = 0;
			M = 0;
			N = 0;
			O = 0;
			P = 0;
			Q = 0;
			R = 0;
			S = 0;
			T = 0;
			U = 0;
			V = 0;
			W = 0;
			X = 0;
			Y = 0;
			Z = 0;
			AA = 0;
			AB = 0;
			AC = 0;
			AD = 0;
			for (int i = 0; i < length; i++)
			{
				switch (i)
				{
					case 0:
						A = bytes[i];
						break;
					case 1:
						B = bytes[i];
						break;
					case 2:
						C = bytes[i];
						break;
					case 3:
						D = bytes[i];
						break;
					case 4:
						E = bytes[i];
						break;
					case 5:
						F = bytes[i];
						break;
					case 6:
						G = bytes[i];
						break;
					case 7:
						H = bytes[i];
						break;
					case 8:
						I = bytes[i];
						break;
					case 9:
						J = bytes[i];
						break;
					case 10:
						K = bytes[i];
						break;
					case 11:
						L = bytes[i];
						break;
					case 12:
						M = bytes[i];
						break;
					case 13:
						N = bytes[i];
						break;
					case 14:
						O = bytes[i];
						break;
					case 15:
						P = bytes[i];
						break;
					case 16:
						Q = bytes[i];
						break;
					case 17:
						R = bytes[i];
						break;
					case 18:
						S = bytes[i];
						break;
					case 19:
						T = bytes[i];
						break;
					case 20:
						U = bytes[i];
						break;
					case 21:
						V = bytes[i];
						break;
					case 22:
						W = bytes[i];
						break;
					case 23:
						X = bytes[i];
						break;
					case 24:
						Y = bytes[i];
						break;
					case 25:
						Z = bytes[i];
						break;
					case 26:
						AA = bytes[i];
						break;
					case 27:
						AB = bytes[i];
						break;
					case 28:
						AC = bytes[i];
						break;
					case 29:
						AD = bytes[i];
						break;
				}
			}
		}
		//if you get A and exspect something else then check your values
		public byte Get(int index)
		{
			switch (index)
			{
				case 0: return A;
				case 1: return B;
				case 2: return C;
				case 3: return D;
				case 4: return E;
				case 5: return F;
				case 6: return G;
				case 7: return H;
				case 8: return I;
				case 9: return J;
				case 10: return K;
				case 11: return L;
				case 12: return M;
				case 13: return N;
				case 14: return O;
				case 15: return P;
				case 16: return Q;
				case 17: return R;
				case 18: return S;
				case 19: return T;
				case 20: return U;
				case 21: return V;
				case 22: return W;
				case 23: return X;
				case 24: return Y;
				case 25: return Z;
				case 26: return AA;
				case 27: return AB;
				case 28: return AC;
				case 29: return AD;
				default: return A;
			}
		}
		public bool Equals(ByteString30 bs)
		{
			if (bs.length != length) return false;
			for (int i = 0; i < length; i++)
				if (Get(i) != bs.Get(i)) return false;
			return true;
		}
		public override string ToString()
		{
			string a = "";
			for (int i = 0; i < length; i++)
				a += (char)Get(i);
			return a;
		}
	}


	public struct DestroyMe : IComponentData { }
	//CoreSystems
	public class EntityDestructionSystem : JobComponentSystem
	{
		EntityQuery entityQuery;
		protected override void OnCreate()
		{
			entityQuery = GetEntityQuery(typeof(DestroyMe));
		}
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			if (entityQuery.CalculateEntityCount() == 0) return inputDeps;
			NativeArray<Entity> entities = entityQuery.ToEntityArray(Allocator.TempJob);
			for(int i = 0; i < entities.Length; i++)
			{
				if (EntityManager.Exists(entities[i])) EntityManager.DestroyEntity(entities[i]);
				else Debug.LogWarning("given entity is already been deleted");
			}
			entities.Dispose();
			return inputDeps;
		}
	}

}
