
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class AudioSystem : ComponentSystem
{
	public EntityQuery AudioQuery,EnviromentSoundQuery,AudioListenerRequest;
	NativeArray<Entity> AudioEntities;
	protected override void OnCreate()
	{
		AudioQuery = GetEntityQuery(typeof(AudioData));
		AudioListenerRequest = GetEntityQuery(typeof(RequestAudioListenerData));
	}
	protected override void OnUpdate()
	{
		//this system is currentl not in use due to the lack of Audio Listener support in ECS
		if(AudioListenerRequest.CalculateEntityCount() > 0)
		{
			NativeArray<Entity> requests = AudioListenerRequest.ToEntityArray(Allocator.TempJob);
			for(int i = 0; i < requests.Length; i++)
			{
				EntityManager.AddComponent(requests[i], ComponentType.ReadWrite<AudioListener>());
				EntityManager.AddComponent(requests[i], ComponentType.ReadWrite<Transform>());
				if(!EntityManager.HasComponent<AudioListenerData>(requests[i]))
					EntityManager.AddSharedComponentData(requests[i], new AudioListenerData
					{
						audioListener = EntityManager.GetComponentObject<AudioListener>(requests[i])
					});
				EntityManager.RemoveComponent<RequestAudioListenerData>(requests[i]);
			}
			requests.Dispose();
		}


		AudioEntities = AudioQuery.ToEntityArray(Allocator.TempJob);
		for (int i = 0; i < AudioEntities.Length; i++)
		{
			Debug.Log("starting music for \""+EntityManager.GetName(AudioEntities[i])+"\"");
			AudioSharedData source = EntityManager.GetSharedComponentData<AudioSharedData>(AudioEntities[i]);
			if (source.isValid)
			{
				if (source.playOnStart && source.music.enabled) if (!source.music.isPlaying) source.music.Play();
				if (source.toggleMusicAmbientSounds)
				{ //enabled is used in place of isValid
					if (!source.ambientSounds.isPlaying) source.ambientSounds.Play();
					else source.ambientSounds.Stop();
					if (!source.ambientSounds.enabled) Debug.LogWarning("ambiemt sounds is disabled");
				}
				if (source.toggleMusicStart && source.music.enabled)
				{
					if (!source.music.isPlaying)
					{
						source.music.clip = source.musicStart;
						source.music.Play();
					}
					else source.music.Stop();
					if (!source.music.enabled) Debug.LogWarning("music is disabled");
				}
				if (source.toggleMusicLoop && source.music.enabled)
				{
					if (!source.music.isPlaying)
					{
						source.music.clip = source.musicLoop;
						source.music.Play();
					}
					else source.music.Stop();

					if (!source.music.enabled) Debug.LogWarning("music is disabled");
				}
			}
			
			EntityManager.RemoveComponent<AudioData>(AudioEntities[i]);
		}
	//	Debug.Log("Finished playing sounds");
		AudioEntities.Dispose();
	}
}