using Unity.Entities;
using UnityEngine;
using System;

[RequiresEntityConversion]
public class AudioDataComponent : MonoBehaviour, IConvertGameObjectToEntity
{
	public AudioSource music;
	public AudioClip musicLoop;
	public AudioClip musicStart;
	public AudioSource ambientSounds;
	public bool playOnStart;
	public bool toggleMusicStart;
	public bool toggleMusicLoop;
	public bool toggleAmbientSounds;

	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		dstManager.AddSharedComponentData(entity,new AudioSharedData {
			music = music,
			ambientSounds = ambientSounds,
			isValid = true,
			musicLoop = musicLoop,
			playOnStart = playOnStart,
			musicStart = musicStart,
			toggleMusicAmbientSounds = toggleAmbientSounds,
			toggleMusicLoop = toggleMusicLoop,
			toggleMusicStart = toggleMusicStart
		});
		dstManager.AddComponentData(entity, new AudioData { });
	}
}
[Serializable]

public struct AudioSharedData : ISharedComponentData , IEquatable<AudioSharedData>
{
	public bool playOnStart;
	public bool toggleMusicLoop;
	public bool toggleMusicStart;
	public bool toggleMusicAmbientSounds;
	public AudioClip musicLoop;
	public AudioClip musicStart;
	public AudioSource music;
	public AudioSource ambientSounds;
	public bool isValid;

	public bool Equals(AudioSharedData other)
	{
		if (!isValid || !other.isValid) return false;
		else if (music == other.music && ambientSounds == other.ambientSounds && musicLoop == other.musicLoop && musicStart == other.musicStart) return true;
		return false;
	}
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}

[Serializable]
public struct AudioData : IComponentData { }
