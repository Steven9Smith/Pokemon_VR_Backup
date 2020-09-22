using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
namespace Core {
	namespace UI {

		public class BarDataComponent : MonoBehaviour
		{
			public int guiId;
			public Image barImage;

			private void Awake()
			{
				barImage = transform.GetComponent<Image>();
			}
		}
	}
}