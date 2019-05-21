using Unity.Entities;
using UnityEngine;
using System;

[RequiresEntityConversion]
public class AudioDataComponent : MonoBehaviour, IConvertGameObjectToEntity
{
	public AudioSource source;
	public bool playOnStart;
	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		dstManager.AddSharedComponentData(entity,new AudioSharedData {
			source = source,
			playOnStart = playOnStart
		});
		dstManager.AddComponentData(entity, new AudioData { });
	}
}



[Serializable]
public struct AudioSharedData : ISharedComponentData
{
	public AudioSource source;
	public bool playOnStart;
}
[Serializable]
public struct AudioData : IComponentData { }
