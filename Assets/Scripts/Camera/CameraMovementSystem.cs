using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Pokemon.Player
{
	public class CameraMovementSystem : ComponentSystem
	{

		public EntityQuery PlayerQuery,AudioListenerQuery;
		NativeArray<Entity> playerEntities,audioListenerEntities;
		public CameraComponent cameraDataComponent;
		public Transform cameraTransform,audioListenerTransform;
		public Translation playerPosition;
		public PlayerInput playerInput;
		float3 offsetX, offsetY;


		//0 = third person
		//1 = first person
		public int viewMode = 0;
		private GameObject mainCamera;
		private GameObject mainCameraParent;
		protected override void OnCreate()
		{
			PlayerQuery = GetEntityQuery(typeof(PlayerData));
			AudioListenerQuery = GetEntityQuery(typeof(AudioListenerData));
		}
		protected override void OnStartRunning()
		{
			mainCameraParent = GameObject.FindWithTag("CameraParent");
			mainCamera = GameObject.FindWithTag("MainCamera");
			cameraDataComponent = mainCamera.GetComponent<CameraComponent>();
			if (mainCameraParent == null) Debug.LogError("Failed to get camera parent");
			else if (mainCamera == null) Debug.LogError("Failed to get Main Camera");
			else if (cameraDataComponent == null) Debug.LogError("Failed to get Camera Data Component");
			cameraTransform = mainCameraParent.GetComponent<Transform>();
			offsetX = cameraDataComponent.offset;
			offsetY = new float3(0, 0, cameraDataComponent.offset.z);
			viewMode = cameraDataComponent.viewMode;
		}
		protected override void OnUpdate()
		{
			//get entity array
			playerEntities = PlayerQuery.ToEntityArray(Allocator.TempJob);

			audioListenerEntities = AudioListenerQuery.ToEntityArray(Allocator.TempJob);
			if (playerEntities.Length > 0)
			{
				//get components
		//		Debug.Log("fieldofView = "+cam.fieldOfView);
				playerPosition = EntityManager.GetComponentData<Translation>(playerEntities[0]);
				playerInput = EntityManager.GetComponentData<PlayerInput>(playerEntities[0]);

				if(playerInput.smoothingSpeed != cameraDataComponent.smoothingSpeed) playerInput.smoothingSpeed = cameraDataComponent.smoothingSpeed;
				if (viewMode == 0)
				{
					offsetX = Quaternion.AngleAxis(playerInput.MouseX * cameraDataComponent.smoothingSpeed, Vector3.up) * offsetX;
					offsetY = Quaternion.AngleAxis(playerInput.MouseY * cameraDataComponent.smoothingSpeed, Vector3.right) * offsetY;

					offsetY.x = 0f;
					offsetY.z = 0f;
			//		Debug.Log("Camera Before: offset = " + offsetX + "," + offsetY + "\nposition = " + cameraTransform.position + "   rotation = " + cameraTransform.rotation + " playterPosition =" + playerPosition.Value);
					cameraTransform.position = playerPosition.Value + offsetX + offsetY;
					cameraTransform.LookAt(playerPosition.Value);
				}
				else
				{
					cameraTransform.position = playerPosition.Value+new float3(0,0.5f,0.5f);

				}
				PlayerInput temp = playerInput;
				temp.forward = cameraTransform.forward;
				temp.right = cameraTransform.right;

				if (audioListenerEntities.Length > 0)
					EntityManager.GetComponentObject<Transform>(audioListenerEntities[0]).position = playerPosition.Value;
				EntityManager.SetComponentData(playerEntities[0], temp);
			}
			else
			{
				Debug.LogError("Failed to find both player and camera components\nplayers = " + playerEntities.Length);
		//		CameraQuery = GetEntityQuery(typeof(CameraDataComponent));
		//		PlayerQuery = GetEntityQuery(typeof(PlayerData));
			}
			audioListenerEntities.Dispose();
			playerEntities.Dispose();
		}

	}

	/**
	 * 
	 using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Pokemon.Player
{
	public class CameraMovementSystem : ComponentSystem
	{

		public EntityQuery CameraQuery, PlayerQuery,AudioListenerQuery;
		NativeArray<Entity> cameraEntities, playerEntities,audioListenerEntities;
		public CameraDataComponent cameraDataComponent;
		public Transform cameraTransform,audioListenerTransform;
		public Translation playerPosition;
		public PlayerInput playerInput;
		bool appliedOffset;
		float3 offsetX, offsetY;
		private GameObject cameraParent;
		//0 = third person
		//1 = first person
		public int viewMode = 0;
		protected override void OnCreate()
		{
			CameraQuery = GetEntityQuery(typeof(CameraDataComponent));
			PlayerQuery = GetEntityQuery(typeof(PlayerData));
			AudioListenerQuery = GetEntityQuery(typeof(AudioListenerData));
			appliedOffset = false;
		}
		protected void 
		protected override void OnUpdate()
		{
			//get entity array
			cameraEntities = CameraQuery.ToEntityArray(Allocator.TempJob);
			playerEntities = PlayerQuery.ToEntityArray(Allocator.TempJob);

			audioListenerEntities = AudioListenerQuery.ToEntityArray(Allocator.TempJob);
			if (cameraEntities.Length > 0 && playerEntities.Length > 0)
			{
				//get components
				Camera cam = EntityManager.GetComponentObject<Camera>(cameraEntities[0]);
		//		Debug.Log("fieldofView = "+cam.fieldOfView);
				cameraTransform = EntityManager.GetComponentObject<Transform>(cameraEntities[0]);
				cameraDataComponent = EntityManager.GetSharedComponentData<CameraDataComponent>(cameraEntities[0]);
				playerPosition = EntityManager.GetComponentData<Translation>(playerEntities[0]);
				playerInput = EntityManager.GetComponentData<PlayerInput>(playerEntities[0]);
				if (!appliedOffset)
				{
					offsetX = cameraDataComponent.offset;
					offsetY = new float3(0,0,cameraDataComponent.offset.z);
					playerInput.smoothingSpeed = cameraDataComponent.smoothingSpeed;
					appliedOffset = true;
				}
				offsetX = Quaternion.AngleAxis(playerInput.MouseX * cameraDataComponent.smoothingSpeed, Vector3.up) * offsetX;
				offsetY = Quaternion.AngleAxis(playerInput.MouseY * cameraDataComponent.smoothingSpeed, Vector3.right) * offsetY;

				offsetY.x = 0f;
				offsetY.z = 0f;
				Debug.Log("Camera Before: offset = "+offsetX+","+offsetY+"\nposition = "+cameraTransform.position+"   rotation = "+cameraTransform.rotation+" playterPosition ="+playerPosition.Value);
				cameraTransform.position = playerPosition.Value + offsetX + offsetY;
				cameraTransform.LookAt(playerPosition.Value);
				PlayerInput temp = playerInput;
				temp.forward = cameraTransform.forward;
				temp.right = cameraTransform.right;

				if (audioListenerEntities.Length > 0)
					EntityManager.GetComponentObject<Transform>(audioListenerEntities[0]).position = playerPosition.Value;
				EntityManager.SetComponentData(playerEntities[0], temp);
			}
			else
			{
				Debug.LogError("Failed to find both player and camera components\ncamera = " + cameraEntities.Length + "   players = " + playerEntities.Length);
		//		CameraQuery = GetEntityQuery(typeof(CameraDataComponent));
		//		PlayerQuery = GetEntityQuery(typeof(PlayerData));
			}
			audioListenerEntities.Dispose();
			cameraEntities.Dispose();
			playerEntities.Dispose();
		}

	}

	/*		//	Debug.Log("cameras = "+camera.Length);
	//		camera = EntityManager.GetSharedComponentData<CameraDataComponent>(cameraEntities[0]);
	//		cam = camera.cam;
			//	if (cam != null) Debug.Log("Oh Shit?");
	//		Debug.Log("offset = " + camera.offset + " smoothingSpeed = " + camera.smoothingSpeed + "active and enabled = s" + cam.isActiveAndEnabled + "\n display =" + cam.targetDisplay + " renderPath = " + cam.actualRenderingPath);
			/*		if (cam.targetDisplay != 1)
					{
						cam.targetDisplay = 1;
						if (cam.activeTexture == null)
							Debug.LogError("Camera has no activeTexture");
						if (!cam.isActiveAndEnabled)
							Debug.LogError("Camera is disabled or inactive!");
						cam.enabled = true;
						cam.clearFlags = CameraClearFlags.Skybox;
						cam.cullingMask = 1 << 0;
				//		cam.fieldOfView = 60f;
				//		cam.nearClipPlane = 0.3f;
				//		cam.farClipPlane = 1000f;
				//		cam.depth = 0;
				//		cam.renderingPath = RenderingPath.UsePlayerSettings;
				//		cam.useOcclusionCulling = true;
				//		cam.allowHDR = false;
				//		cam.allowMSAA = false;

					}
			//		RenderTexture renderTexture = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 24, RenderTextureFormat.ARGB32);
			//		renderTexture.Create();
			//		cam.targetTexture = renderTexture;
			//		cam.Render();
			*/
}