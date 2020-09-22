using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
namespace Pokemon
{
	public struct DamageTrigger : IComponentData
	{
		public int Value;
	}
	public class DamageTriggerSystem : JobComponentSystem
	{
		public struct DamageTriggerJob : IJobForEach<DamageTrigger, PokemonEntityData>
		{
			public float time;
			public void Execute(ref DamageTrigger c0, ref PokemonEntityData c1)
			{
				//add no hp code later
				if (c1.currentHp > 0) c1.currentHp = math.clamp(c1.currentHp-(c0.Value * time),0,c1.Hp);
				
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			return  new DamageTriggerJob { time = Time.DeltaTime}.Schedule(this,inputDeps);
		}
	}
	public struct DamageTriggerData : IComponentData
	{
		public int Value;
	}
	//Collision Part
	public struct DamageCollision : IComponentData
	{
		public int Value;
	}
	public class DamageCollisionSystem : JobComponentSystem
	{
		public struct DamageCollisionJob : IJobForEach<DamageCollision, PokemonEntityData>
		{
			public float time;
			public void Execute(ref DamageCollision c0, ref PokemonEntityData c1)
			{
				//add no hp code later
				if (c1.currentHp > 0) c1.currentHp = math.clamp(c1.currentHp - (c0.Value * time), 0, c1.Hp);

			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			return new DamageCollisionJob { time = Time.DeltaTime }.Schedule(this, inputDeps);
		}
	}
	public struct DamageCollisionData : IComponentData
	{
		public int Value;
	}
}