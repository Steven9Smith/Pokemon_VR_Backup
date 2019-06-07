using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
public class SetEntityDebuggerName : MonoBehaviour, IConvertGameObjectToEntity
{
	public string entityName;
	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		dstManager.SetName(entity, entityName);
	}
}
