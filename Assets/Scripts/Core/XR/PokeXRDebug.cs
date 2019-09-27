using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.XR;

namespace Pokemon
{
	namespace XR {
		public class PokeXRDebug : MonoBehaviour, IConvertGameObjectToEntity
		{
			public bool isEnabled = true;
			public GameViewRenderMode gameViewRenderMode = GameViewRenderMode.BothEyes;
			public float eyeTectureResolutionScale = 1f;
			public float occlusionMaskScale = 1f;
			public float renderViewportScale = 1f;
			public bool useOcclusionMesh = true;
			public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
			{
				XRSettings.enabled = isEnabled;
				XRSettings.gameViewRenderMode = gameViewRenderMode;
				XRSettings.eyeTextureResolutionScale = eyeTectureResolutionScale;
				XRSettings.occlusionMaskScale = occlusionMaskScale;
				XRSettings.renderViewportScale = renderViewportScale;
				XRSettings.useOcclusionMesh = useOcclusionMesh;

				
			}
		}
	}
}
