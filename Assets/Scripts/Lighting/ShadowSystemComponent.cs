using UnityEngine;
using Unity.Entities;
using Core.Lighting;

public class ShadowSystemComponent : MonoBehaviour, IConvertGameObjectToEntity
{
	public uint frames;
	public float millaseconds;
	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		dstManager.AddComponentData(entity, new ShadowSystemData {
			current = 0,
			frames = frames,
			millaseconds = millaseconds
		});
	}

}
