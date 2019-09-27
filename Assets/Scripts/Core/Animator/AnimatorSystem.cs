using Unity.Physics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Physics.Systems;
using UnityEngine;
using Unity.Mathematics;

namespace Pokemon
{
	/*
	public struct AnimationEvent : IComponentData {}
	public struct AnimationData : ISharedComponentData { public Animator animator; }
	public class AnimatorSystem : ComponentSystem
	{
		EntityQuery animQuery;
		int i;
		protected override void OnCreate()
		{
			animQuery = GetEntityQuery(ComponentType.ReadOnly(typeof(AnimationEvent)),ComponentType.ReadOnly(typeof(StateData)),ComponentType.ReadOnly(typeof(AnimationData)));
			i = 0;
		}

		protected override void OnUpdate()
		{
			animQuery = GetEntityQuery(ComponentType.ReadOnly(typeof(AnimationEvent)));
			if (animQuery.CalculateLength() < 1)
				return;
			else Debug.Log("Detected an Animation Trigger!");
			NativeArray<StateData> states = animQuery.ToComponentDataArray<StateData>(Allocator.TempJob);
			NativeArray<Entity> entities = animQuery.ToEntityArray(Allocator.TempJob);
			for (i = 0; i < animQuery.CalculateLength();i++)
			{
				adjustState(entities[i],states[i]);
			}
			states.Dispose();
			entities.Dispose();
		}
		private void adjustState(Entity entity, StateData data)
		{
			Animator anim = EntityManager.GetSharedComponentData<AnimationData>(entity).animator;
			if (data.onGround)
			{
				switch (true)
				{

					default:    //play idle if not playing
				//		anim.pl
						break;
				}
			}
			else
			{

			}
		}
	}
	*/
}