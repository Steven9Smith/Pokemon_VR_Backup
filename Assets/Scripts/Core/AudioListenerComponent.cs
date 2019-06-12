using Unity.Entities;
using UnityEngine;
using System;

[RequiresEntityConversion]
public class AudioListenerComponent : MonoBehaviour, IConvertGameObjectToEntity
{
	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		dstManager.AddComponentData(entity, new AudioListenerData { });
	}
}

public struct AudioListenerData : IComponentData { }
