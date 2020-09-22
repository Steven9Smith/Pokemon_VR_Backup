
using UnityEngine;
using Unity.Entities;

[RequiresEntityConversion]
public class TerrainColliderComponent : MonoBehaviour, IConvertGameObjectToEntity
{
	public Transform transform;
	public UnityEngine.Mesh mesh;
	public UnityEngine.Material material;

	public bool ProcedurallyGenerateTerrain = false;

	public TerrainData terrainData;

	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		if (mesh == null) Debug.LogError("Failed to load given mesh");
		else if (material == null) Debug.LogError("Given Material is null");
		else if (transform == null) Debug.LogError("Cannot create the Terrian without the given transform");
		else
		{
			//		Debug.Log(mesh.bounds.size);
			//entity = Core.Terrain.TerrianBuilder.GenerateTerrainEntity(dstManager, material, mesh,new Translation { Value = transform.position},new Rotation {Value = transform.rotation },terrainData);
			Core.Terrain.TerrianBuilder.GenerateTerrainEntityV3(dstManager, entity, mesh,terrainData);
		}
	}
}