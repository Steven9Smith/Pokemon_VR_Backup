using Pokemon;
using System;
using Unity.Entities;
using UnityEngine;

namespace Pokemon
{
	[RequiresEntityConversion]
	public class EnviromentDataComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		public string enviromentName;
		public string enviromentId;
		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			EnviromentData ed = new EnviromentData
			{
				entityId = PokemonIO.StringToByteString30(enviromentId),
				entityName = PokemonIO.StringToByteString30(enviromentName),
				BoundType = 0
			};
			dstManager.AddComponentData(entity, ed);
			dstManager.SetName(entity,"Enviroment \""+enviromentName+"\"");
		}
	}
	//holds data for a particular enviroment
	[Serializable]
	public struct EnviromentData : IComponentData
	{
		public ByteString30 entityName;
		public ByteString30 entityId;
		public ByteString30 entityParentId;
		public ByteString30 pathString;
		public byte BoundType; //0 = floor, 1 = Wall, 2 = both
	}

}