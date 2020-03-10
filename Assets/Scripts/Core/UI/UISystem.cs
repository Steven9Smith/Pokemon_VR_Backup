﻿using Unity.Entities;
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
			int i = 0;

			EntityQuery UIComponents,UIComponentRequests,Player;
			public GameObject WorldCanvas,PlayerCanvas;
			private GameObject fakePlayerPosition = new GameObject();

			protected override void OnStartRunning()
			{
				WorldCanvas = Resources.Load("Core/UI/InGameUI/WorldCanvas") as GameObject;
				PlayerCanvas = Resources.Load("Core/UI/InGameUI/PlayerCanvas") as GameObject;
				WorldCanvas = GameObject.Instantiate(WorldCanvas);
				PlayerCanvas = GameObject.Instantiate(PlayerCanvas);
			}
			protected override void OnCreate()
			{
				playerEntityQuery = GetEntityQuery(typeof(PlayerData),typeof(PokemonEntityData),typeof(Translation),typeof(PlayerInput));
				UIComponentRequests = GetEntityQuery(typeof(UIComponentRequest));
				UIComponents = GetEntityQuery(typeof(UIComponentFilter), typeof(PokemonEntityData));
			}
			protected override JobHandle OnUpdate(JobHandle inputDeps)
			{
				NativeArray<Entity> requestEntitites = UIComponentRequests.ToEntityArray(Allocator.TempJob);
				NativeArray<UIComponentRequest> requests = UIComponentRequests.ToComponentDataArray<UIComponentRequest>(Allocator.TempJob);
				for (i = 0; i < requestEntitites.Length; i++)
					UIDataClass.GenerateEntityUIGameObject(EntityManager, requestEntitites[i], ref PlayerCanvas, ref WorldCanvas, requests[i]);
				requestEntitites.Dispose();
				requests.Dispose();
				NativeArray<Entity> entities = UIComponents.ToEntityArray(Allocator.TempJob);
				NativeArray<PokemonEntityData> peds = UIComponents.ToComponentDataArray<PokemonEntityData>(Allocator.TempJob);
				NativeArray<Translation> positions = playerEntityQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
				if (positions.Length > 0)
				{
					fakePlayerPosition.transform.SetPositionAndRotation(positions[0].Value, Quaternion.identity);
					for (int i = 0; i < entities.Length; i++)
					{
						UIComponent uic = EntityManager.GetSharedComponentData<UIComponent>(entities[i]);
						if (uic.toggleVisibility)
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
							if (uic.onWorld)
							{
								uic.UIGameObject.transform.localPosition = EntityManager.GetComponentData<Translation>(entities[i]).Value + uic.positionOffset;
								if (uic.followPlayer)
									uic.UIGameObject.transform.LookAt(fakePlayerPosition.transform);

							}
						}
						//	else Debug.Log("Not Visible");
					}
				}
				entities.Dispose();
				peds.Dispose();
				positions.Dispose();
				return inputDeps;
			}
		}
	}
}