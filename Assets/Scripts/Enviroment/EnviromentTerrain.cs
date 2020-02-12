using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Entities;
using Pokemon;


[RequiresEntityConversion]
public class EnviromentTerrain : MonoBehaviour, IConvertGameObjectToEntity
{
	public UnityEngine.Mesh mesh;
	public UnityEngine.Material material;
	public TerrainData terrainData;
	public string name;


	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		if (mesh == null) Debug.LogError("Failed to load given mesh");
		else if (material == null) Debug.LogError("Given Material is null");
		else
		{
	//		Debug.Log(mesh.bounds.size);
			entity = Core.Terrain.TerrianBuilder.generateTerrianEntity(dstManager, material,mesh,terrainData);;
			dstManager.SetName(entity,name);
			
		}
	}
}
namespace Core.Terrain
{
	public class TerrianBuilder
	{

		public static Unity.Physics.TerrainCollider.CollisionMethod Method;
		public static unsafe float * HeightsToFloatArray(TerrainData terrainData)
		{
			float* heights = null;
			if (terrainData.size != Vector3.zero)
			{
				heights = (float*)UnsafeUtility.Malloc((int)terrainData.size.x * (int)terrainData.size.z * sizeof(float), 4, Allocator.Temp);
				float[,] a = terrainData.GetHeights(0, 0, (int)terrainData.size.x, (int)terrainData.size.z);
				for(int i = 0; i < (int)terrainData.size.x; i++)
					for(int j = 0; j < (int)terrainData.size.z; j++) 
						heights[j + (i *(int)terrainData.size.x)] = a[i,j];
				
			}
			else Debug.LogError("failed to get terrian size");

			return heights;
		}
		public static NativeArray<float> HeightsToFloatNativeArray(TerrainData terrainData)
		{
			NativeArray<float> heights = new NativeArray<float>(0,Allocator.Temp);
			if (terrainData.size != Vector3.zero)
			{
				heights = new NativeArray<float>((int)(terrainData.size.x * terrainData.size.z), Allocator.Temp);
				float[,] a = terrainData.GetHeights(0, 0, (int)terrainData.size.x, (int)terrainData.size.z);
				for (int i = 0; i < (int)terrainData.size.x; i++)
					for (int j = 0; j < (int)terrainData.size.z; j++)
						heights[j + (i * (int)terrainData.size.x)] = a[i, j];
			}
			else Debug.LogError("failed to get terrain size");
			return heights;
		}
		public static unsafe Entity generateTerrianEntity(EntityManager entityManager, UnityEngine.Material mat,Mesh mesh,TerrainData terrainData)
		{
			//	float* heights = GenerateMeshHeights(mesh);
			//	float* heights = HeightsToFloatArray(terrainData);
			NativeArray<float> heights = HeightsToFloatNativeArray(terrainData);
			int2 size = new int2((int)terrainData.size.x,(int) terrainData.size.z);
			
			float3 scale = terrainData.heightmapScale;
	//		Debug.Log(terrainData.size);
			Entity staticEntity = new Entity { };
			if (heights.Length == 0)
			{
				Debug.LogError("Invalid height was given");
				//	UnsafeUtility.Free(heights, Allocator.Temp);
				heights.Dispose();
				return staticEntity;
			}
			else
			{
				Debug.Log("Creating terrain...");
				BlobAssetReference<Unity.Physics.Collider> collider = Unity.Physics.TerrainCollider.Create(heights,size, scale, Method,
					new CollisionFilter {
						BelongsTo = TriggerEventClass.Floor,
						CollidesWith = TriggerEventClass.Pokemon | TriggerEventClass.Player | TriggerEventClass.NPC | TriggerEventClass.Collidable,
						GroupIndex = 0
					}
			);
				float3 position = new float3(size.x - 1, 0f, size.y - 1) * scale * -0.5f;
				staticEntity = Bodies.Bodies.CreateStaticBodyWithMesh(entityManager, mat,mesh, position, quaternion.identity, collider);
			}

			//	UnsafeUtility.Free(heights, Allocator.Temp);
			heights.Dispose();
			return staticEntity;
		}
	}

}