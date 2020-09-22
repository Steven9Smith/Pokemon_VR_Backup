using Unity.Entities;
using UnityEngine;
using System;

[RequiresEntityConversion]
public class AudioListenerComponent : MonoBehaviour, IConvertGameObjectToEntity
{
	public AudioListener audioListener;
	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		dstManager.AddSharedComponentData(entity, new AudioListenerData {
			audioListener = audioListener
		});
	}
}

public struct AudioListenerData : ISharedComponentData, IEquatable<AudioListenerData> {
	public AudioListener audioListener;

	public bool Equals(AudioListenerData other)
	{
		if (other.audioListener == null && audioListener == null) return true;
		else if (other.audioListener == null) return false;
		else if (audioListener == null) return false;
		return audioListener.Equals(other.audioListener);
	}
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
public struct RequestAudioListenerData : IComponentData { }
