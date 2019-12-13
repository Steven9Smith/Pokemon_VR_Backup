using Unity.Entities;
using UnityEngine;
using Pokemon;
using UnityEngine.UI;
using Unity.Collections;
using Unity.Jobs;
using System.Collections.Generic;
using TMPro;
using Unity.Transforms;

namespace Core {
	namespace UI {
		public class UISystem : JobComponentSystem
		{
			EntityQuery playerEntityQuery;
			BarDataComponent healthBarDataComponent;
			BarDataComponent staminaBarDataComponent;
			private GameObject[] UIObjectsLayer;
			int i = 0;

			EntityQuery UIComponents,UIComponentRequests;
			public GameObject WorldCanvas,PlayerCanvas;


			protected override void OnStartRunning()
			{
				//		healthBarDataComponent = GameObject.FindWithTag("HealthBar").GetComponent<BarDataComponent>();
				//		staminaBarDataComponent = GameObject.FindWithTag("EnergyBar").GetComponent<BarDataComponent>();
				//		UIObjectsLayer = UIDataClass.FindGameObjectsWithLayer(5);
				WorldCanvas = Resources.Load("Core/UI/InGameUI/WorldCanvas") as GameObject;
				PlayerCanvas = Resources.Load("Core/UI/InGameUI/PlayerCanvas") as GameObject;
				WorldCanvas = GameObject.Instantiate(WorldCanvas);
				PlayerCanvas = GameObject.Instantiate(PlayerCanvas);
			}
			protected override void OnCreate()
			{
				playerEntityQuery = GetEntityQuery(typeof(PlayerData),typeof(PokemonEntityData));
				UIComponentRequests = GetEntityQuery(typeof(UIComponentRequest));
				UIComponents = GetEntityQuery(typeof(UIComponentFilter), typeof(PokemonEntityData));
			}
			protected override JobHandle OnUpdate(JobHandle inputDeps)
			{
				/*	NativeArray<PokemonEntityData> peds = playerEntityQuery.ToComponentDataArray<PokemonEntityData>(Allocator.TempJob);
					for (i = 0; i < peds.Length; i++)
					{
						if (peds[i].guiId == healthBarDataComponent.guiId)	healthBarDataComponent.barImage.fillAmount = peds[i].currentHp/peds[i].Hp;
						if (peds[i].guiId == staminaBarDataComponent.guiId) staminaBarDataComponent.barImage.fillAmount = peds[i].currentStamina/peds[i].maxStamina;

					}
					peds.Dispose();*/
				NativeArray<Entity> requestEntitites = UIComponentRequests.ToEntityArray(Allocator.TempJob);
				NativeArray<UIComponentRequest> requests = UIComponentRequests.ToComponentDataArray<UIComponentRequest>(Allocator.TempJob);
				for (i = 0; i < requestEntitites.Length; i++)
					UIDataClass.GenerateEntityUIGameObject(EntityManager, requestEntitites[i], ref PlayerCanvas, ref WorldCanvas, requests[i].addToWorld);
				requestEntitites.Dispose();
				requests.Dispose();
				NativeArray<Entity> entities = UIComponents.ToEntityArray(Allocator.TempJob);
				NativeArray<PokemonEntityData> peds = UIComponents.ToComponentDataArray<PokemonEntityData>(Allocator.TempJob);
				for (int i = 0; i < entities.Length; i++) {
					
					UIComponent uic = EntityManager.GetSharedComponentData<UIComponent>(entities[i]);
					if(uic.toggleVisibility)
					{
						if (uic.UIGameObject.activeSelf) uic.UIGameObject.SetActive(false);
						else uic.UIGameObject.SetActive(true);
					}
					if (uic.UIGameObject.activeSelf)
					{
						uic.HealthBarValue.SetText(peds[i].currentHp + "/" + peds[i].Hp);
						uic.HealthBarImage.fillAmount = peds[i].currentHp / peds[i].Hp;
						uic.EnergyBarValue.SetText(peds[i].currentStamina + "/" + peds[i].maxStamina);
						uic.EnergyBarImage.fillAmount = peds[i].currentStamina / peds[i].maxStamina;
						if (uic.onWorld) uic.UIGameObject.transform.localPosition = EntityManager.GetComponentData<Translation>(entities[i]).Value + uic.positionOffset;
					}
					else Debug.Log("Not Visible");
				}
				entities.Dispose();
				peds.Dispose();
				return inputDeps;
			}
		}
	}
}