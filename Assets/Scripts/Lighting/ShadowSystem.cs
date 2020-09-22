using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Core.Lighting
{
	public class ShadowSystem : JobComponentSystem
	{
		EntityQuery ShadowQuery;
		protected override void OnCreate()
		{
			ShadowQuery = GetEntityQuery(typeof(ShadowSystemData));
		}

		private struct ShadowSystemJob : IJobForEach<ShadowSystemData>
		{
			public float deltaTime;
			public void Execute(ref ShadowSystemData ssd)
			{
				if (ssd.frames > 0)
				{
					if ((uint)ssd.current >= ssd.frames)
					{
						ssd.current = 0;
						ssd.preformCall = true;
					}
					else ssd.current += 1f;
				}
				else if (ssd.millaseconds > 0)
				{
					if (ssd.current >= ssd.millaseconds)
					{
						ssd.current = 0;
						ssd.preformCall = true;
					}
					else ssd.current += deltaTime;
				}
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			if (ShadowQuery.CalculateEntityCount() != 0)
			{
				JobHandle ShadowJob = new ShadowSystemJob {
					deltaTime = Time.DeltaTime
				}.Schedule(ShadowQuery,inputDeps);
				ShadowJob.Complete();

				NativeArray<ShadowSystemData> ssd = ShadowQuery.ToComponentDataArray<ShadowSystemData>(Allocator.TempJob);
				NativeArray<Entity> entities = ShadowQuery.ToEntityArray(Allocator.TempJob);
				for (int i = 0; i < entities.Length; i++)
				{
				//	Debug.Log("ssd[" + i + "] = " + ssd[i].current);
					if (ssd[i].preformCall)
					{
						HDAdditionalLightData hDAdditionalLightData = EntityManager.GetComponentObject<HDAdditionalLightData>(entities[i]);
						
						if (hDAdditionalLightData != null)
							hDAdditionalLightData.RequestShadowMapRendering();
						else Debug.LogWarning("detected a ShadowSystemData component without HDAdditionalLightData Object on entity \"" + EntityManager.GetName(entities[i]) + "\"");
						EntityManager.SetComponentData(entities[i], new ShadowSystemData
						{
							current = ssd[i].current,
							frames = ssd[i].frames,
							millaseconds = ssd[i].millaseconds,
							preformCall = false
						});
					}
				}
				ssd.Dispose();
				entities.Dispose();
				return ShadowJob;
			}
			
			return inputDeps;
		}
	}

	public struct ShadowSystemData : IComponentData
	{
		public uint frames;
		public float millaseconds;
		public float current;
		public BlittableBool preformCall;
	}
}
