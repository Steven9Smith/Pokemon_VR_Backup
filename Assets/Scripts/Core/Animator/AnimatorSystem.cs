using Unity.Physics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Physics.Systems;
using UnityEngine;
using Unity.Mathematics;
using System;

namespace Pokemon
{
	namespace Animation
	{
		public struct PokemonAnimationData : ISharedComponentData,IEquatable<PokemonAnimationData> {
			public Animator animator;

			public bool Equals(PokemonAnimationData other)
			{
				if (animator == null) return false;
				else return animator.Equals(other);
			}
			public override int GetHashCode()
			{
				return base.GetHashCode();
			}
		}
		public struct PokemonAnimationVerifier : IComponentData
		{
			
		}
		public class AnimatorSystem : ComponentSystem
		{
			EntityQuery animQuery;
			int size;
			protected override void OnCreate()
			{
				animQuery = GetEntityQuery(ComponentType.ReadOnly(typeof(StateData)),ComponentType.ReadOnly(typeof(PokemonAnimationVerifier)));
			}

			protected override void OnUpdate()
			{
				size = animQuery.CalculateEntityCount();
				if (size == 0) return;
				else Debug.Log("Detected an Animation Trigger!");
				NativeArray<StateData> states = animQuery.ToComponentDataArray<StateData>(Allocator.TempJob);
				NativeArray<Entity> entities = animQuery.ToEntityArray(Allocator.TempJob);
				for (int i = 0; i < size; i++)
				{
					PokemonAnimationData pokemonAnimationData = EntityManager.GetSharedComponentData<PokemonAnimationData>(entities[i]);
					Animator anim = pokemonAnimationData.animator;
					if(anim.GetBool("isJumping") != states[i].isJumping)anim.SetBool("isJumping",states[i].isJumping);
					if (anim.GetBool("onGround") != states[i].onGround) anim.SetBool("onGround",states[i].onGround);
					if (anim.GetBool("Idle") != states[i].isIdle) anim.SetBool("Idle",states[i].isIdle);
				}
				states.Dispose();
				entities.Dispose();
			}
		}
	}
}