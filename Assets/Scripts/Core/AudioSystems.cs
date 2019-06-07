
using Unity.Collections;
using Unity.Entities;

public class AudioSystem : ComponentSystem
{
	public EntityQuery AudioQuery;
	NativeArray<Entity> AudioEntities;
	protected override void OnCreateManager()
	{
		AudioQuery = GetEntityQuery(typeof(AudioData));
	}
	protected override void OnUpdate()
	{
//		Debug.Log("Playing sounds...");
		//get entity array
		AudioEntities = AudioQuery.ToEntityArray(Allocator.TempJob);
		for (int i = 0; i < AudioEntities.Length; i++)
		{
			AudioSharedData source = EntityManager.GetSharedComponentData<AudioSharedData>(AudioEntities[i]);
			if (source.playOnStart) source.source.Play();
			EntityManager.RemoveComponent<AudioData>(AudioEntities[i]);
		}
	//	Debug.Log("Finished playing sounds");
		AudioEntities.Dispose();
	}
}