using Core.Camera;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Pokemon.Player
{
	public class CameraMovementSystem : JobComponentSystem
	{

		//0 = third person
		//1 = first person
		public int viewMode = 0;
		private bool invertY = true, offsetSet = false;

		// GameObjects
		public GameObject DummyEmptyGameObject = null;


		public GameObject[] CameraParents;
		public GameObject[] PlayerCameras;

		EntityQuery cameraQuery;
		// Setup the camera gameobjects and oder them accordingly
		protected override void OnStartRunning()
		{
			cameraQuery = GetEntityQuery(typeof(CameraComponentData));

			// Create Dummy GameObject
			DummyEmptyGameObject = Resources.Load("Core/Prefabs/DummyEmptyGameObject") as GameObject;
			DummyEmptyGameObject = GameObject.Instantiate(DummyEmptyGameObject);
			if (DummyEmptyGameObject == null)
				Debug.LogError("Failed to get dummy prefab");

			GameObject[] tempPlayerCameras = GameObject.FindGameObjectsWithTag("PlayerCameras");
			GameObject[] tempCameraParents = GameObject.FindGameObjectsWithTag("CameraParents");
			// each camera has a number after it so lets us use the indexes to signify the one we want in terms of order
			if (tempPlayerCameras.Length != tempCameraParents.Length)
				Debug.LogError("Failed to find as many parents as cameras");
			else if (tempPlayerCameras.Length == 0)
				Debug.LogError("Failed to find any cameras");
			CameraParents = new GameObject[tempPlayerCameras.Length];
			PlayerCameras = new GameObject[tempPlayerCameras.Length];
			
			for(int i =0; i < PlayerCameras.Length; i++)
			{
				//	format:
				// Blah001
				int tmp = 0,length = tempPlayerCameras[i].name.Length;
				string name = tempPlayerCameras[i].name;
				string number = name.Substring(length-3,3);
				if (int.TryParse(number, out tmp))
				{
					PlayerCameras[tmp] = tempPlayerCameras[i];
				}
				else Debug.LogError("Failed to convert number of player camera name, make sure ");
				length = tempCameraParents[i].name.Length;
				name = tempCameraParents[i].name;
				number = name.Substring(length-3,3);
				if (int.TryParse(number, out tmp))
				{
					CameraParents[tmp] = tempCameraParents[i];
				}
				else Debug.LogError("Failed to convert number of camera parent name, make sure ");
			}
			//now all availible cameras are organized by index so now we have to update the IComponentData
			NativeArray<CameraComponentData> ccds = cameraQuery.ToComponentDataArray<CameraComponentData>(Allocator.TempJob);
			if (ccds.Length == 0)
				Debug.LogError("Failed to find camera entities");
			// don't forget each camera has a CameraCopmonentData
			for(int i = 0; i < ccds.Length; i++)
			{
				string name = EntityManager.GetName(ccds[i].CameraEntity);
				if(name == PlayerCameras[i].name)
				{
					EntityManager.SetComponentData(ccds[i].CameraEntity, new CameraComponentData {
						CameraEntity = ccds[i].CameraEntity,
						isFree = true
					});
				}
			}
			ccds.Dispose();
			Debug.Log("Camera System Startup Successfully completed");
		}


		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			EntityQuery _cameraQuery = cameraQuery;

			GameObject[] tempPlayerCameras = PlayerCameras;
			GameObject[] tempCameraParents = CameraParents;
			// Setup camera with entities
			Entities.ForEach((Entity entity, ref PokemonEntityData ped,ref Translation position,ref EntityControllerStepInput stepInput, ref PlayerCameraComponentData ccdr, ref PlayerCameraComponentDataRequest pccdr) =>
			{
				Debug.Log("Attempting to setup camera for "+EntityManager.GetName(entity));
				NativeArray<CameraComponentData> cameraComponentDatas = _cameraQuery.ToComponentDataArray<CameraComponentData>(Allocator.TempJob);
				NativeArray<Entity> cameraComponentEntities = _cameraQuery.ToEntityArray(Allocator.TempJob);
				for (int i = 0; i < cameraComponentDatas.Length; i++)
				{
					bool foundMatch = false;
					Debug.Log("Testing if camera "+i+" is free");
					if (!cameraComponentDatas[i].isFree)
					{
						// get the right index
						string name = EntityManager.GetName(cameraComponentEntities[i]);
						for(int j = 0; j < tempPlayerCameras.Length; i++)
						{
							if(name == tempPlayerCameras[j].name)
							{
								Debug.Log("setting Camera with entity " + EntityManager.GetName(entity));
								ccdr =  new PlayerCameraComponentData
								{
									cameraOffsetComponent = PokemonDataClass.GetCameraOffsetData(ped.PokedexNumber),
									ViewMode = ccdr.ViewMode,
									CameraEntity = cameraComponentEntities[i],
									index = i,
									invertY = ccdr.invertY
								};
								EntityManager.SetComponentData(cameraComponentEntities[i], new CameraComponentData
								{
									CameraEntity = cameraComponentEntities[i],
									isFree = false
								});
								EntityManager.RemoveComponent<PlayerCameraComponentDataRequest>(entity);
							
								foundMatch = true;
								break;
							}

						}
						if (foundMatch)
							break;
							

					}

				}
				cameraComponentDatas.Dispose();
				cameraComponentEntities.Dispose();
			}).WithStructuralChanges().WithoutBurst().WithName("CameraComponentSetup").Run();

			// Now all the cameras are set up with players so lets move the camera and stuff


			Entities.WithNone<PlayerCameraComponentDataRequest>().ForEach((Translation position, ref Rotation rotation, ref PlayerCameraComponentData playerCameraComponentData,ref EntityControllerStepInput stepInput) => {
				GameObject PlayerCamera = tempPlayerCameras[playerCameraComponentData.index];
				GameObject CameraParent = tempCameraParents[playerCameraComponentData.index];
				// we must calculate the new position of the camera

				// Deal with position
				if (!playerCameraComponentData.offsetSet)
				{
					if (playerCameraComponentData.ViewMode == Core.CoreFunctionsClass.CameraViewMode.ThirdPerson)
						playerCameraComponentData.CurrentCameraOffset = playerCameraComponentData.cameraOffsetComponent.thridPersonOffset;
					else
						playerCameraComponentData.CurrentCameraOffset = playerCameraComponentData.cameraOffsetComponent.firstPersonOffset;
					playerCameraComponentData.offsetSet = true;
				}
				Quaternion mouseY = Quaternion.AngleAxis(playerCameraComponentData.invertY ? stepInput.input.MouseY : stepInput.input.MouseY * playerCameraComponentData.cameraOffsetComponent.smoothingSpeed, Vector3.right);
				mouseY = Quaternion.Euler(mouseY.eulerAngles.x, 0,0);

				playerCameraComponentData.CurrentCameraOffset = mouseY * Quaternion.AngleAxis(stepInput.input.MouseX * playerCameraComponentData.cameraOffsetComponent.smoothingSpeed, Vector3.up) * playerCameraComponentData.CurrentCameraOffset;
				

				CameraParent.transform.position = position.Value + playerCameraComponentData.CurrentCameraOffset;
				if (CameraParent.transform.position.y < position.Value.y)
					CameraParent.transform.position = new float3(CameraParent.transform.position.x,position.Value.y,CameraParent.transform.position.z);
				// Deal with rotation
				CameraParent.transform.LookAt(position.Value);

				stepInput.CameraTransform = new ITransform(CameraParent.transform);
				DummyEmptyGameObject.transform.SetPositionAndRotation(position.Value, rotation.Value);
				stepInput.EntityTransform = new ITransform(DummyEmptyGameObject.transform);


			}).WithoutBurst().WithName("CameraComponentMovementSystem").Run();
			
			return inputDeps;
		}
	}


/*	public class CameraMovementSystem : ComponentSystem
	{

		public EntityQuery PlayerQuery,AudioListenerQuery;
		NativeArray<Entity> playerEntities,audioListenerEntities;
		NativeArray<Translation> playerTranslations;
		NativeArray<EntityControllerStepInput> stepInputs;
	//	NativeArray<PlayerInput> playerInputs;
		NativeArray<CameraOffsetData> playerCameras;
	//	public CameraComponent cameraDataComponent;
		public Transform cameraTransform,audioListenerTransform;
		public float3 playerPosition;

	//	public PlayerInput playerInput;
		float3 offset = float3.zero;

		CameraOffsetData pcd1, pcd2, pcd3, pcd4,pcde1;
		EntityControllerStepInput stepInput1,stepInput2,stepInput3,stepInput4;


		//0 = third person
		//1 = first person
		public int viewMode = 0;
		private GameObject mainCamera;
		private GameObject mainCameraParent;
		private bool invertY = true,offsetSet = false;

		// GameObjects
		public GameObject CameraParent;
		public GameObject MainCamera;
	//	public CameraComponent cameraDataCopmonent;


		protected override void OnCreate()
		{
			PlayerQuery = GetEntityQuery(typeof(PlayerData),typeof(Translation),typeof(EntityControllerStepInput),typeof(CameraOffsetData));
			AudioListenerQuery = GetEntityQuery(typeof(AudioListenerData));
		}
		protected override void OnStartRunning()
		{
			if (mainCameraParent == null) Debug.LogError("Failed to get camera parent");
			else if (mainCamera == null) Debug.LogError("Failed to get Main Camera");
			else if (cameraDataComponent == null) Debug.LogError("Failed to get Camera Data Component");
			cameraTransform = mainCameraParent.GetComponent<Transform>();
		//	offsetX = cameraDataComponent.offset;
		//	offsetY = new float3(0, 0, cameraDataComponent.offset.z);
		}
		protected override void OnUpdate()
		{



			//get entity array
			playerEntities = PlayerQuery.ToEntityArray(Allocator.TempJob);
			playerTranslations = PlayerQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
		//	for (int i = 0; i < playerTranslations.Length; i++)
		//		Debug.LogWarning("aaa"+playerTranslations[i].Value.ToString());
			stepInputs = PlayerQuery.ToComponentDataArray<EntityControllerStepInput>(Allocator.TempJob);
			audioListenerEntities = AudioListenerQuery.ToEntityArray(Allocator.TempJob);

			playerCameras = PlayerQuery.ToComponentDataArray<CameraOffsetData>(Allocator.TempJob);

			if (playerCameras.Length > 0) pcd1 = playerCameras[0];
			if (playerEntities.Length > 0 && playerCameras.Length > 0)
			{
				if (!offsetSet)
				{
					
					offset = pcd1.thridPersonOffset.z;
					viewMode = cameraDataComponent.viewMode;
					offset = pcd1.thridPersonOffset.z;
					viewMode = cameraDataComponent.viewMode;
					offsetSet = true;
				}
				//get components
				//Debug.Log("fieldofView = "+cam.fieldOfView);
				playerPosition = playerTranslations[0].Value;
				stepInput1 = stepInputs[0];
		//		playerInput = playerInputs[0];
			//	Debug.Log(playerPosition);
				if (!float.IsNaN(playerPosition.x))
				{
	//				Debug.Log(playerPosition.Value);

					if (playerInput.smoothingSpeed != cameraDataComponent.smoothingSpeed) playerInput.smoothingSpeed = cameraDataComponent.smoothingSpeed;
					if (viewMode == 0)
					{
						offset = Quaternion.AngleAxis(stepInput1.input.MouseX * cameraDataComponent.smoothingSpeed,Vector3.up) 
							* Quaternion.AngleAxis(invertY ? -stepInput1.input.MouseY : stepInput1.input.MouseY*cameraDataComponent.smoothingSpeed,Vector3.right)*offset;
						
						cameraTransform.position = playerPosition + offset;
						cameraTransform.LookAt(playerPosition);
					}
					else
					{
						cameraTransform.position = playerPosition + new float3(0, 0.5f, 0.5f);
					}
					{
						//put character rotation stuff here
					}
					stepInput1.input.SetForward(cameraTransform.forward);
					stepInput1.input.SetRight(cameraTransform.right);
					try
					{
						if (audioListenerEntities.Length > 0)
							EntityManager.GetComponentObject<Transform>(audioListenerEntities[0]).position = playerPosition;
						EntityManager.SetComponentData(playerEntities[0], stepInput1);
					}
					catch
					{
						Debug.LogWarning("failed to get Trasnform for Audio listener component");
					}
				}
				else Debug.LogWarning("Waiting for valid translation "+playerPosition.ToString()+" ...amount of entities = "+playerEntities.Length);
			}
			else
			{
<<<<<<< Updated upstream
				Debug.LogWarning("Failed to find both player and camera components\nplayers = " + playerEntities.Length);
		//		CameraQuery = GetEntityQuery(typeof(CameraDataComponent));
		//		PlayerQuery = GetEntityQuery(typeof(PlayerData));
=======
				if (playerEntities.Length == 0)
					Debug.LogWarning("no players detected");
				if (playerCameras.Length == 0)
					Debug.LogWarning("no cameras detected");
>>>>>>> Stashed changes
			}
			playerEntities.Dispose();
			stepInputs.Dispose();
			playerTranslations.Dispose();
			audioListenerEntities.Dispose();
			playerCameras.Dispose();
		}

	}*/

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