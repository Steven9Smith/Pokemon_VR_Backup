
using System;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace Core
{
	namespace UI
	{
		[RequiresEntityConversion]
		public class UIDataComponent : MonoBehaviour, IConvertGameObjectToEntity
		{
			public Material material;
			public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
			{
				
			}
		}
		//parent plane
		public struct UIData : IComponentData
		{
			Point centerPoint;
			Point dimension;

		}


		[Serializable]
		public struct UIChild : ISharedComponentData
		{
			UIData uiData;
			Point positionToParent;
		}
		public struct Point : IComponentData
		{
			uint x, y;

			public static bool operator ==(Point c1, Point c2)
			{
				//since ATrigger can have entityA and entityB is either position we have to test for them both
				return c1.x == c2.x && c1.y == c2.y;
			}

			public static bool operator !=(Point c1, Point c2)
			{
				return c1.x != c2.x && c1.y != c2.y; ;
			}
		}
	}
}
