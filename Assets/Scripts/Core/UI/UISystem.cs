using Unity.Entities;
using UnityEngine;
using Pokemon;
using UnityEngine.UI;
using Unity.Collections;
using Unity.Jobs;

namespace Core {
	namespace UI {
		public class UISystem : JobComponentSystem
		{
			EntityQuery playerEntityQuery;
			BarDataComponent healthBarDataComponent;
			BarDataComponent staminaBarDataComponent;
			int i = 0;
			protected override void OnStartRunning()
			{
				healthBarDataComponent = GameObject.FindWithTag("HealthBar").GetComponent<BarDataComponent>();
				staminaBarDataComponent = GameObject.FindWithTag("EnergyBar").GetComponent<BarDataComponent>();
			}
			protected override void OnCreate()
			{
				playerEntityQuery = GetEntityQuery(typeof(PlayerData),typeof(PokemonEntityData));
			}
			protected override JobHandle OnUpdate(JobHandle inputDeps)
			{
				NativeArray<PokemonEntityData> peds = playerEntityQuery.ToComponentDataArray<PokemonEntityData>(Allocator.TempJob);
				for (i = 0; i < peds.Length; i++)
				{
					if (peds[i].guiId == healthBarDataComponent.guiId)	healthBarDataComponent.barImage.fillAmount = peds[i].currentHp/peds[i].Hp;
					if (peds[i].guiId == staminaBarDataComponent.guiId) staminaBarDataComponent.barImage.fillAmount = peds[i].currentStamina/peds[i].maxStamina;
				}
				peds.Dispose();

				return inputDeps;
			}
		}
	}
}