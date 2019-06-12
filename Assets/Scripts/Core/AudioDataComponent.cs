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
public struct AudioSharedData : ISharedComponentData , IEquatable<AudioSharedData>
{
	public AudioSource source;
	public bool playOnStart;
	public override bool Equals(object obj)
	{
		if ((obj == null) || !this.GetType().Equals(obj.GetType())) return false; //Check for null and compare run-time types.
		else
		{
			AudioSharedData other = (AudioSharedData)obj;
			return this.playOnStart == other.playOnStart && this.source == other.source;
		}
	}
	public bool Equals(AudioSharedData other){return this.playOnStart == other.playOnStart && this.source == other.source;}
	public override int GetHashCode(){return base.GetHashCode();}
}

public struct AudioSharedData : ISharedComponentData
{
	public AudioSource source;
	public bool playOnStart;
}
[Serializable]
public struct AudioData : IComponentData { }
