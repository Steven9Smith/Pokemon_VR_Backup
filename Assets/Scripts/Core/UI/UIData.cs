using Pokemon;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI {
	public struct UIComponent : ISharedComponentData, IEquatable<UIComponent>
	{
		public TextMeshProUGUI HealthBarValue;
		public TextMeshProUGUI EnergyBarValue;
		public Image BarBorder;
		public Image HealthBarImage;
		public Image EnergyBarImage;
		public BlittableBool isValid;
		public float3 offset;
		public GameObject UIGameObject;
		public override bool Equals(object obj)
		{
			UIComponent other = (UIComponent)obj;
			return (isValid && other.isValid) && HealthBarImage.Equals(other.HealthBarImage) && HealthBarValue.Equals(other.HealthBarValue);
		}
		public bool Equals(UIComponent other)
		{
			return (isValid && other.isValid) && HealthBarImage.Equals(other.HealthBarImage) && HealthBarValue.Equals(other.HealthBarValue);
		}
		public override int GetHashCode() { return base.GetHashCode(); }
	}
	public struct UIComponentFilter : IComponentData { }
	public struct UIComponentRequest : IComponentData { public BlittableBool addToWorld; }
	public class UIDataClass
	{
		public static GameObject[] FindGameObjectsWithLayer(int layer)
		{
			GameObject[] goArray = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
			List<GameObject> goList = new List<GameObject>();
			for (int i = 0; i < goArray.Length; i++)
				if (goArray[i].layer == layer) goList.Add(goArray[i]);
			if (goList.Count == 0) return null;
			return goList.ToArray();
		}
		
		public static GameObject[] GetGameObjectChildren(GameObject go)
		{
			List<GameObject> gos = new List<GameObject>();
			for (int i = 0; i < go.transform.childCount; i++)
				gos.Add(go.transform.GetChild(i).gameObject);
			return gos.ToArray();
		}
		public static GameObject findGameObjectWithName(GameObject[] go, string name)
		{
			for (int i = 0; i < go.Length; i++)
				if (go[i].name == name) return go[i];
			return null;
		}
		public static void GenerateEntityUIGameObject(EntityManager entityManager,Entity entity,ref GameObject PlayerCanvas,ref GameObject WorldCanvas,bool addToWorld)
		{
			GameObject bob = Resources.Load("Core/UI/InGameUI/PlayerBarContainers") as GameObject;
			GameObject go = GameObject.Instantiate(bob) as GameObject;
			UIComponent uic = new UIComponent { };
			if (go != null)
			{
				GameObject[] containers = GetGameObjectChildren(go);
				for (int i = 0; i < containers.Length; i++)
				{
					GameObject[] containerChildren = GetGameObjectChildren(containers[i]);
					for (int j = 0; j < containerChildren.Length; j++)
					{
					//	Debug.Log("Children: name = \"" + containerChildren[j].name + "\"");
						if (containerChildren[j].name.Contains("BarBorder")) uic.BarBorder = containerChildren[j].GetComponent<Image>();
						else if (containerChildren[j].name == "HealthBar") uic.HealthBarImage = containerChildren[j].GetComponent<Image>();
						else if (containerChildren[j].name == "EnergyBar") uic.EnergyBarImage = containerChildren[j].GetComponent<Image>();
						else if (containerChildren[j].name == "HealthBarValue") uic.HealthBarValue = containerChildren[j].GetComponent<TextMeshProUGUI>();
						else if (containerChildren[j].name == "EnergyBarValue") uic.EnergyBarValue = containerChildren[j].GetComponent<TextMeshProUGUI>();
					}
				}
				if (uic.BarBorder != null && uic.HealthBarImage != null && uic.EnergyBarImage != null && uic.HealthBarValue != null && uic.EnergyBarValue != null)
					uic.isValid = true;
				if (uic.isValid)
				{
					uic.UIGameObject = go;
					//assumes that the entity has CoreData
					uic.offset = GetEntityWorldCanvasUIOffeset(PokemonIO.ByteString30ToString(entityManager.GetComponentData<CoreData>(entity).BaseName));
					if (entityManager.HasComponent<UIComponent>(entity)) entityManager.SetSharedComponentData<UIComponent>(entity, uic);
					else entityManager.AddSharedComponentData(entity, uic);
					if (!entityManager.HasComponent<UIComponentFilter>(entity)) entityManager.AddComponentData<UIComponentFilter>(entity, new UIComponentFilter { });
				}
				else { Debug.LogError("Failed to add UIComponent due to an invalid UIComponent: " + uic.BarBorder + "," + uic.EnergyBarImage + "," + uic.EnergyBarValue + "," + uic.HealthBarImage + "," + uic.HealthBarValue); return; }
				AddToCanvas(go, ref PlayerCanvas, ref WorldCanvas, addToWorld);
				entityManager.RemoveComponent<UIComponentRequest>(entity);
			}
			else { Debug.LogError("Failed to load UI COmponent because go is null"); return; }
		}

		public static void AddToCanvas(GameObject go,ref GameObject PlayerCanvas,ref GameObject WorldCanvas,bool addToWorldCanvas)
		{
			if (WorldCanvas == null || PlayerCanvas == null) { Debug.LogWarning("World and/or player canvas is not ready!"); return; }
			if (addToWorldCanvas) go.transform.SetParent(WorldCanvas.transform,false);
			else go.transform.SetParent(PlayerCanvas.transform,false);
		}
		public static float3 GetEntityWorldCanvasUIOffeset(string name)
		{
			switch (name)
			{
				case "HumanA":return new float3(0,3.3f,0);
				default: return float3.zero; 
			}
		}
	}
}
