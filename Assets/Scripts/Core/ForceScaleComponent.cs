using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

public class ForceScaleComponent : MonoBehaviour,IConvertGameObjectToEntity
{
	public Transform transform = null;

	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		if (transform != null)
			dstManager.AddComponentData(entity, new NonUniformScale { Value = transform.localScale });
		else Debug.LogWarning("Cannot add scake to Entity \""+entity.ToString()+"\" because transform is invalid. please fiz this");
	}
}
