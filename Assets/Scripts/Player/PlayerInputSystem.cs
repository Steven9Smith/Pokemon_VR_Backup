using Unity.Mathematics;
using UnityEngine;
using Unity.Entities;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Collections;
using Unity.Physics;
using Pokemon.Move;
using Unity.Transforms;
<<<<<<< Updated upstream
using Unity.Rendering;
using Core.ParentChild;
using Core.Spawning;
using UnityEngine;
using Core;
=======
using UnityEngine.Assertions;
using Pokemon.EntityController;
using UnityEngine;
using Core.GameConfig;
using System;
using Core.Camera;
using Unity.Rendering;
>>>>>>> Stashed changes

namespace Pokemon.Player
{
	public class PlayerInputSystem : JobComponentSystem
	{
<<<<<<< Updated upstream
		public EntityQuery GravityQuery;
		public PhysicsStep step;
		public float3 gravity = -new float3(0, -9.81f, 0);
		private InputDefinitionsClass inputDefinitionsClass;
		private GroupIndexSystem groupIndexSystem;
		protected override void OnCreate()
		{
			GravityQuery = GetEntityQuery(typeof(PhysicsStep));
			inputDefinitionsClass = new InputDefinitionsClass();
			groupIndexSystem = World.GetOrCreateSystem<GroupIndexSystem>();
		}
		struct PlayerInputJob : IJobForEach<PlayerInput, PokemonEntityData, PhysicsVelocity, StateData, PhysicsCollider>
		{
			public float deltaTime;
			public float Horizontal;
			public float Vertical, MouseX, MouseY;
			public float3 gravity;
			public BlittableBool spaceDown;
			public BlittableBool LShiftDown;
			public BlittableBool LCtrlDown;
			public BlittableBool Mouse1Down;
			public BlittableBool Mouse2Down;
			//attack bools
			public BlittableBool attackADown;
			public BlittableBool attackBDown;
			public BlittableBool attackCDown;
			public BlittableBool attackDDown;

			public void Execute(ref PlayerInput playerInput, ref PokemonEntityData pokemonEntityData, ref PhysicsVelocity physicsVelocity, ref StateData stateData, ref PhysicsCollider physicsCollider)
			{
				UpdatePlayerInput(ref playerInput);
				PlayerMovement(playerInput, ref pokemonEntityData, ref physicsVelocity, ref stateData);
				gainEnergy(stateData,ref pokemonEntityData, deltaTime);
			}
			/// <summary>
			/// updates the key the player is pressing
			/// </summary>
			/// <param name="playerInput">PLayerInput struct</param>
			private void UpdatePlayerInput(ref PlayerInput playerInput)
			{
				playerInput.SpaceDown = spaceDown;
				playerInput.LShiftDown = LShiftDown;
				playerInput.LCtrlDown = LCtrlDown;
				playerInput.Mouse1Down = Mouse1Down;
				playerInput.Mouse2Down = Mouse2Down;
				playerInput.Move.x = Horizontal;
				playerInput.Move.z = Vertical;
				playerInput.MouseX = MouseX;
				playerInput.MouseY = MouseY;
				playerInput.attackADown = attackADown;
				playerInput.attackBDown = attackBDown;
				playerInput.attackCDown = attackCDown;
				playerInput.attackDDown = attackDDown;
			}
			/// <summary>
			/// executes the player movement per frame
			/// </summary>
			/// <param name="input"></param>
			/// <param name="pokemonEntityData"></param>
			/// <param name="velocity"></param>
			/// <param name="stateData"></param>
			private void PlayerMovement([ReadOnly]PlayerInput input, ref PokemonEntityData pokemonEntityData, ref PhysicsVelocity velocity, ref StateData stateData)
			{
	//			Debug.Log("This is running");
				float3 force = float3.zero;
				//store the the velocity 
				float3 maxVelocity = math.abs(velocity.Linear);
				float acceleration = pokemonEntityData.Acceleration;
		/*		//improve ground detection by using Collision Filters rather than bad math
				if (maxVelocity.y < 1 && maxVelocity.y >= 0) stateData.onGround = true;
				else stateData.onGround = false;

				if (stateData.onGround)
				{
					//entity is running
					if (input.LShiftDown)
					{
						maxVelocity *= 2;
						acceleration *= 2;
						//veruify if the pokemon is moving own its own
						if (maxVelocity.x > 0 || maxVelocity.z > 0) {if (!stateData.isRunning) StateDataClass.SetState(ref stateData, StateDataClass.State.Running);}
						else if (!stateData.isIdle) StateDataClass.SetState(ref stateData, StateDataClass.State.Idle);
					}
					//entity is crouching
					else if (input.LCtrlDown)
					{
						maxVelocity *= 0.5f;
						acceleration *= 0.5f;
						//veruify if the pokemon is moving own its own
						if (maxVelocity.x > 0 || maxVelocity.z > 0) { if (!stateData.isCreeping) StateDataClass.SetState(ref stateData, StateDataClass.State.Creeping); }
						else if (!stateData.isIdle) StateDataClass.SetState(ref stateData, StateDataClass.State.Crouching);
=======
		// This struct was made to mimic some important transform stuff
		public struct ITransform : IComponentData
		{
			public float3 Right;
			public float3 Up;
			public float3 Forward;
			public quaternion Rotation;
			public float3 Position;
			public float3 EulerAngles;
			public ITransform(Transform t)
			{
				Right = t.right;
				Up = t.up;
				Forward = t.forward;
				Rotation = t.rotation;
				Position = t.position;
				EulerAngles = t.eulerAngles;
			}
			public ITransform(float3 right,float3 up,float3 forward,quaternion rotation,float3 position,float3 eulerAngles)
			{
				Right = right;
				Up = up;
				Forward = forward;
				Rotation = rotation;
				Position = position;
				EulerAngles = eulerAngles;
			}
			public ITransform Default()
			{
				return new ITransform(Vector3.right, Vector3.up,Vector3.forward,quaternion.identity,float3.zero,float3.zero);
			}
		}

		public struct EntityControllerStepInput : IComponentData
		{
			public float CurrentRotationAngle;
			public float3 UnsupportedVelocity;
			public StateData stateData;
			public PhysicsVelocity Velocity;
			public PlayerInput input;
			public float DeltaTime;
			public float3 Gravity;
			public int MaxIterations;
			public float Tau;
			public PhysicsDamping Damping;
			public float SkinWidth;
			public float ContactTolerance;
			public float MaxSlope;
			public int RigidBodyIndex;
			public int AffectsPhysicsBodies;
			// Camera Transform Values
			public ITransform CameraTransform;
			// Entity Transform Values
			public ITransform EntityTransform;
			// If the ENtity is Supported than we will get a surface normal
			public float3 CurrentSurfaceNormal;
			public float3 LastSurfaceNormal;
			public void SetPlayerInput(PlayerInput pi)
			{
				input = pi;
			}
		}

		
		//This handles the user input (this gets called before the player movement calculations are done)
		[UpdateBefore(typeof(ExportPhysicsWorld))]
		public class EntityInputSystem : JobComponentSystem
		{
			PlayerInputConfigs PlayerInputConfigs;
			GameConfig gameConfig;

			protected override void OnCreate()
			{
				if (!GameConfigClass.isValid())
					GameConfigClass.LoadGameConfig();
			}
			protected override void OnStartRunning()
			{
				gameConfig = GameConfigClass.mGameConfig;
			}
			protected override void OnStopRunning()
			{
				GameConfigClass.SaveGameConfig(gameConfig);
			}


			protected override JobHandle OnUpdate(JobHandle inputDeps)
			{
				GameConfig tmpConfig = gameConfig;
				Entities.ForEach((ref EntityControllerStepInput stepInput, in PlayerData playerData)=> {
					PlayerInputConfig ic = GameConfigClass.GetPlayerInputConfig(tmpConfig.PlayerInputConfigs, playerData.PlayerNumber);
					stepInput.input = new PlayerInput
					{
						MouseX = Input.GetAxis(ic.LookXAxis),
						MouseY = Input.GetAxis(ic.LookYAxis),
						Move = new float3(Input.GetAxis(ic.MoveXAxis), 0, Input.GetAxis(ic.MoveYAxis)),
						ReqAttack = Input.GetKey(ic.AttackKey),
						ReqAttackA = Input.GetKey(ic.AttackAKey),
						ReqAttackB = Input.GetKey(ic.AttackBKey),
						ReqAttackC = Input.GetKey(ic.AttackCKey),
						ReqAttackD = Input.GetKey(ic.AttackDKey),
						ReqItemA = Input.GetKey(ic.ItemAKey),
						ReqItemB = Input.GetKey(ic.ItemBKey),
						ReqItemC = Input.GetKey(ic.ItemCKey),
						ReqItemD = Input.GetKey(ic.ItemDKey),
						ReqCrouch = Input.GetKey(ic.CrouchKey),
						ReqFocus = Input.GetKey(ic.FocusKey),
						ReqRun = Input.GetKey(ic.RunKey),
						ReqInteract = Input.GetKey(ic.InteractKey),
						ReqJump = Input.GetKey(ic.JumpKey),
						ReqProne = Input.GetKey(ic.ProneKey),
						ReqSwitchAttackLeft = Input.GetKey(ic.SwitchAttackLeftKey),
						ReqSwitchAttackRight = Input.GetKey(ic.SwitchAttackRightKey),
						ReqSwitchItemRight = Input.GetKey(ic.SwitchItemRightKey),
						ReqSwitchItemLeft = Input.GetKey(ic.SwitchItemLeftKey),
						Rotation = stepInput.input.Rotation,
						jumpCount = stepInput.input.jumpCount
					};
			/*		stepInput = new EntityControllerStepInput
					{
						AffectsPhysicsBodies = stepInput.AffectsPhysicsBodies,
						Damping = stepInput.Damping,
						DeltaTime = stepInput.DeltaTime,
						Velocity = stepInput.Velocity,
						stateData = stepInput.stateData,
						SkinWidth = stepInput.SkinWidth,
						MaxSlope = stepInput.MaxSlope,
						ContactTolerance = stepInput.ContactTolerance,
						CurrentRotationAngle = stepInput.CurrentRotationAngle,
						Gravity = stepInput.Gravity,
						MaxIterations = stepInput.MaxIterations,
						RigidBodyIndex = stepInput.RigidBodyIndex,
						Tau = stepInput.Tau,
						UnsupportedVelocity = stepInput.UnsupportedVelocity,
						Up = Vector3.Normalize((Quaternion)rotation.Value * Vector3.up),
						input = new PlayerInput
						{
							
						}
					};*/
				}).WithoutBurst().Run();	
				return inputDeps;
			}
		}
		//This handles the calculations
		[UpdateAfter(typeof(ExportPhysicsWorld)), UpdateBefore(typeof(EndFramePhysicsSystem))]
		public class EntityControllerSystem : JobComponentSystem
		{

			/*	
		//	[BurstCompile]
			struct CharacterControllerJob : IJobChunk
			{
				public float DeltaTime;

				[ReadOnly] public PhysicsWorld PhysicsWorld;
				[ReadOnly] public float3 gravity;
				[ReadOnly] public float MaxSpeedReducer;

				public ArchetypeChunkComponentType<Translation> TranslationType;
				public ArchetypeChunkComponentType<Rotation> RotationType;
				public ArchetypeChunkComponentType<CoreData> coreDataType;
				public ArchetypeChunkComponentType<PokemonEntityData> pedType;
				public ArchetypeChunkComponentType<EntityControllerStepInput> stepInputType;
				public ArchetypeChunkComponentType<PhysicsVelocity> physicsVelocityType;
				public ArchetypeChunkComponentType<PhysicsDamping> physicsDampingType;
				public ArchetypeChunkComponentType<CameraOffsetData> pokemonCameraDataType;

				[ReadOnly] public ArchetypeChunkComponentType<PhysicsCollider> PhysicsColliderType;

				// Stores impulses we wish to apply to dynamic bodies the character is interacting with.
				// This is needed to avoid race conditions when 2 characters are interacting with the
				// same body at the same time.
				public NativeStream.Writer DeferredImpulseWriter;

				public unsafe void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
				{
					float3 up = math.up();

					var chunkPhysicsColliderData = chunk.GetNativeArray(PhysicsColliderType);
					var chunkTranslationData = chunk.GetNativeArray(TranslationType);
					var chunkRotationData = chunk.GetNativeArray(RotationType);
					var chunkPedData = chunk.GetNativeArray(pedType);
					var chunkStepInputData = chunk.GetNativeArray(stepInputType);
					var chunkCoreData = chunk.GetNativeArray(coreDataType);
					var chunkPhysicsVelocityData = chunk.GetNativeArray(physicsVelocityType);
					var chunkPhysicsDampingData = chunk.GetNativeArray(physicsDampingType);
					var chunkPokemonCameraData = chunk.GetNativeArray(pokemonCameraDataType);

					DeferredImpulseWriter.BeginForEachIndex(chunkIndex);


					for (int i = 0; i < chunk.Count; i++)
					{
						var ped = chunkPedData[i];
						var _stepInput = chunkStepInputData[i];
						var collider = chunkPhysicsColliderData[i];
						var position = chunkTranslationData[i];
						var rotation = chunkRotationData[i];
						var coreData = chunkCoreData[i];
						var velocity = chunkPhysicsVelocityData[i];
						var damping = chunkPhysicsDampingData[i];
						var pokemonCameraData = chunkPokemonCameraData[i];

						if (float.IsNaN(position.Value.x)) Debug.LogWarning("position is invalid");
						else
						{
							// Collision filter must be valid
							Assert.IsTrue(!collider.ColliderPtr->Filter.IsEmpty);
							TEntityControllerStepInput TStepInput = new TEntityControllerStepInput
							{
								pw = PhysicsWorld,
								stepInput = new EntityControllerStepInput
								{
									DeltaTime = DeltaTime,
									Up = math.up(),
									Gravity = gravity,
									MaxIterations = _stepInput.MaxIterations,
									Tau = _stepInput.Tau,
									Damping = _stepInput.Damping,
									SkinWidth = _stepInput.SkinWidth,
									ContactTolerance = _stepInput.ContactTolerance,
									MaxSlope = _stepInput.MaxSlope,
									RigidBodyIndex = PhysicsWorld.GetRigidBodyIndex(coreData.entity),
									Velocity = velocity,
									stateData = _stepInput.stateData,
									input = _stepInput.input
								}

							};
							// Character transform
							RigidTransform transform = new RigidTransform
							{
								pos = position.Value,
								rot = rotation.Value
							};
					//		Debug.LogWarning("pos = " + transform.pos.ToString() + "\nrot = " + transform.rot.ToString());
							float3 surfaceNormal, surfaceVelocity;

							// Check support
							CharacterControllerUtilities.CheckSupport(ref PhysicsWorld, TStepInput.pw, ref collider, TStepInput.stepInput, transform, TStepInput.stepInput.MaxSlope,
								ref TStepInput.stepInput.stateData, out surfaceNormal, out surfaceVelocity, ped.Speed);
							//	Debug.Log("ddd"+TStepInput.stepInput.stateData.supported.Value);

							// User input
							float3 desiredVelocity = TStepInput.stepInput.LinearVelocity;
							HandleUserInputA(ref TStepInput.stepInput, ref ped, TStepInput.stepInput.Up, surfaceVelocity, ref desiredVelocity, pokemonCameraData.smoothingSpeed);

							// Calculate actual velocity with respect to surface
							if (TStepInput.stepInput.stateData.supported)
							{
								CalculateMovement(TStepInput.stepInput.CurrentRotationAngle, TStepInput.stepInput.Up, TStepInput.stepInput.stateData.isJumping,
									TStepInput.stepInput.LinearVelocity, desiredVelocity, surfaceNormal, surfaceVelocity, out TStepInput.stepInput.LinearVelocity);
							}
							else
							{
								TStepInput.stepInput.LinearVelocity = desiredVelocity;
							}
							// World collision + integrate
							CharacterControllerUtilities.CollideAndIntegrate(TStepInput.pw, ped, TStepInput.stepInput, ped.Mass, TStepInput.stepInput.AffectsPhysicsBodies > 0,
								collider.ColliderPtr, ref transform, ref TStepInput.stepInput.LinearVelocity, ref DeferredImpulseWriter);

							// Write back and orientation integration
							position.Value = transform.pos;
							rotation.Value = transform.rot;// quaternion.AxisAngle(up, TStepInput.stepInput.CurrentRotationAngle);
							//	Debug.Log(desiredVelocity);
							// Write back to chunk data
							if (!float.IsNaN(position.Value.x))
							{
								chunkStepInputData[i] = TStepInput.stepInput;
								chunkPhysicsVelocityData[i] = new PhysicsVelocity
								{
									Linear = TStepInput.stepInput.LinearVelocity,
									Angular = TStepInput.stepInput.AngularVelocity
								};
								chunkCoreData[i] = new CoreData
								{
									translation = new Translation { Value = transform.pos },
									rotation = new Rotation { Value = transform.rot },
									BaseName = chunkCoreData[i].BaseName,
									damping = chunkCoreData[i].damping,
									entity = chunkCoreData[i].entity,
									isValid = chunkCoreData[i].isValid,
									Name = chunkCoreData[i].Name,
									scale = chunkCoreData[i].scale,
									size = chunkCoreData[i].size
								};

								chunkTranslationData[i] = position;
								chunkRotationData[i] = rotation;

							}
							else Debug.LogWarning("Calculated an invald value with position");
						}
					}
					DeferredImpulseWriter.EndForEachIndex();
				}

				private void HandleUserInputA(ref EntityControllerStepInput stepInput, ref PokemonEntityData ped, float3 up, float3 surfaceVelocity, ref float3 linearVelocity, float smoothingSpeed)
				{
					switch (ped.MovementType) {
						case PokemonDataClass.MOVEMENT_TYPE_SLUSH_SLIDE:
						case PokemonDataClass.MOVEMENT_TYPE_ROLL:
							if (stepInput.stateData.supported)
							{
								stepInput.input.jumpCount = 0;
								stepInput.stateData.isJumping = false;
								//	Debug.Log(stepInput.input.Move.x);
								if (stepInput.input.Move.x != 0 || stepInput.input.Move.y != 0)
								{
									//		Debug.Log(stepInput.input.forward +","+stepInput.input.right+","+stepInput.DeltaTime+","+ped.Acceleration);
									linearVelocity += ((stepInput.input.forward * stepInput.input.Move.y) + (stepInput.input.right * stepInput.input.Move.x) * stepInput.DeltaTime * (stepInput.input.ReqRun ? ped.Acceleration * 2 : stepInput.input.ReqCrouch ? ped.Acceleration / 2 : stepInput.input.ReqProne ? ped.Acceleration / 4 : ped.Acceleration));
									linearVelocity.x = Mathf.Clamp(linearVelocity.x, -ped.Speed/MaxSpeedReducer, ped.Speed/MaxSpeedReducer);
									linearVelocity.z = Mathf.Clamp(linearVelocity.z, -ped.Speed/MaxSpeedReducer, ped.Speed/MaxSpeedReducer);
								//	linearVelocity.y = 0f;
								}
								if (stepInput.input.ReqJump)
								{
									//need jump that goes oppisite of gavity
							//		Debug.Log("this shoulf only run once");
									linearVelocity.y += ped.jumpHeight;
									stepInput.input.jumpCount++;
									stepInput.stateData.supported = false;
									stepInput.stateData.isJumping = true;
								}
							}
							{
								if (stepInput.stateData.isJumping)
								{
									if (!stepInput.input.ReqJump && linearVelocity.y > 0)
										linearVelocity.y += ped.jumpHeight * stepInput.DeltaTime * (ped.jumpMultiplier - 1);
									else
										linearVelocity.y += ped.jumpHeight * stepInput.DeltaTime * (ped.longJumpMultiplier - 1);
								}
							}
						//	Debug.Log(linearVelocity.y);
							break;
						default:// Debug.LogWarning("Failed to find pokemon movement type \""+ped.MovementType+"\"");
							break;
					}
				}

				private void HandleUserInput(ref EntityControllerStepInput stepInput, PokemonEntityData ped, float3 up, float3 surfaceVelocity, ref float3 linearVelocity,float smoothingSpeed)
				{

					// Reset jumping state and unsupported velocity
					if (stepInput.stateData.supported)
					{
						stepInput.stateData.isJumping = false;
						stepInput.UnsupportedVelocity = float3.zero;
>>>>>>> Stashed changes
					}
					//entity is Walking
					else if (maxVelocity.x > 0 || maxVelocity.z > 0){if (!stateData.isWalking) StateDataClass.SetState(ref stateData, StateDataClass.State.Crouching);}
					else if (!stateData.isIdle) StateDataClass.SetState(ref stateData, StateDataClass.State.Idle);
					//add speed if pokemon has not reach its max speed
					if (maxVelocity.x < pokemonEntityData.Speed && maxVelocity.z < pokemonEntityData.Speed)
					{
<<<<<<< Updated upstream
						if (input.Move.z > 0)
						{
							force += input.forward;
						}
						else if (input.Move.z < 0)
						{
							force -= input.forward;
=======
						float3 forward = math.forward(quaternion.identity);
						float3 right = math.cross(up, forward);

						float horizontal = stepInput.input.Move.x;
						float vertical = stepInput.input.Move.y;
						bool jumpRequested = stepInput.input.ReqJump;
						bool haveInput = (math.abs(horizontal) > float.Epsilon) || (math.abs(vertical) > float.Epsilon);
						if (haveInput)
						{
							float3 localSpaceMovement = forward * vertical + right * horizontal;
							float3 worldSpaceMovement = math.rotate(quaternion.AxisAngle(up, stepInput.CurrentRotationAngle), localSpaceMovement);
							requestedMovementDirection = math.normalize(worldSpaceMovement);
						}
						shouldJump = jumpRequested && stepInput.stateData.supported;
					}

					// Turning
					{
						float horizontal = stepInput.input.MouseX;
						float vertical = stepInput.input.MouseY;
						bool haveInput = (math.abs(horizontal) > float.Epsilon);
						//bool haveInput = (math.abs(horizontal) > float.Epsilon) || (math.abs(vertical) > float.Epsilon)
						if (haveInput)
						{
							stepInput.CurrentRotationAngle += horizontal * smoothingSpeed * DeltaTime;
>>>>>>> Stashed changes
						}
						if (input.Move.x > 0)
						{
<<<<<<< Updated upstream
							force += input.right;
						}
						else if (input.Move.x < 0)
						{
							force -= input.right;
						}
						force *= acceleration;
=======
							// Add jump speed to surface velocity and make character unsupported
							stepInput.stateData.isJumping = true;
							stepInput.stateData.supported = true;
							stepInput.UnsupportedVelocity = surfaceVelocity + ped.jumpHeight * up;
						}
						else if (stepInput.stateData.supported)
						{
							// Apply gravity
							stepInput.UnsupportedVelocity += stepInput.Gravity * DeltaTime;
						}
						// If unsupported then keep jump and surface momentum
						linearVelocity = requestedMovementDirection * ped.Speed +
							(stepInput.stateData.supported ? stepInput.UnsupportedVelocity : float3.zero);
>>>>>>> Stashed changes
					}

					


					if (input.SpaceDown.Value == 1 && stateData.onGround)
					{
						//check stamina
						float realJumpheight = pokemonEntityData.jumpHeight < pokemonEntityData.currentStamina ? pokemonEntityData.jumpHeight : pokemonEntityData.currentStamina;
						//apply calculated jumpheight
						velocity.Linear.y += realJumpheight;

						pokemonEntityData.currentStamina = math.clamp(pokemonEntityData.currentStamina - realJumpheight, 0, pokemonEntityData.maxStamina);
						if (!stateData.isJumping) StateDataClass.SetState(ref stateData, StateDataClass.State.Jumping);
					}
				}

				force *= deltaTime;
				force.y = 0f;*/
				//Debug.Log("input "+input.forward+" force = "+force+" velocity = "+velocity.Linear);
			//	if(pokemonEntityData.BodyType != PokemonDataClass.BODY_TYPE_HEAD_ONLY) velocity.Linear += force;
			//	else velocity.Angular += force;
				velocity.Linear += force;
				//Debug.Log("move = "+input.Move+" | acceleration = "+acceleration+" | playerMaxSpeed = "+playerMaxSpeed+" \nvelocity = "+velocity.Linear+" rotation = "+rotation.Value);

				float3 currentVelocity = velocity.Linear;
				//if player is trying to move on x or z
				Vector3 camF = input.forward;
				Vector3 camR = input.right;
				camF.y = 0f;
				camR.y = 0f;
				camF = camF.normalized;
				camR = camR.normalized;
				float3 movement = ((input.Move.x * acceleration * (float3)camR)+ (input.Move.z*acceleration*(float3)camF)) * deltaTime;

				
				if (currentVelocity.y < 1 && input.SpaceDown.Value == 1)
				{
					movement.y = pokemonEntityData.jumpHeight;
				}else if(currentVelocity.y > 1 && input.SpaceDown.Value == 1)
				{
				//	float realJumpheight = pokemonEntityData.jumpHeight < pokemonEntityData.currentStamina ? pokemonEntityData.jumpHeight : pokemonEntityData.currentStamina;

					movement.y = (pokemonEntityData.longJumpMultiplier - 1) * deltaTime;
				}
				else if (currentVelocity.y > 1 && input.SpaceDown.Value == 1)
				{
					movement.y = (pokemonEntityData.jumpMultiplier - 1) * deltaTime;
				}
<<<<<<< Updated upstream

				velocity.Linear += movement;
			//	Debug.Log(velocity.Linear+","+pokemonEntityData.Speed);
				velocity.Linear.x = Mathf.Clamp(velocity.Linear.x, -pokemonEntityData.Speed/ PokemonDataClass.PokemonSpeedStatDivider, pokemonEntityData.Speed/ PokemonDataClass.PokemonSpeedStatDivider);
				velocity.Linear.z = Mathf.Clamp(velocity.Linear.z, -pokemonEntityData.Speed/PokemonDataClass.PokemonSpeedStatDivider, pokemonEntityData.Speed/ PokemonDataClass.PokemonSpeedStatDivider);
		//		Debug.Log(velocity.Linear+","+movement+","+currentVelocity);
			}
			private void gainEnergy(StateData stateData,ref PokemonEntityData ped, float time) { if (ped.currentStamina < ped.maxStamina && ped.currentHp > 0 && !stateData.isRunning ) ped.currentStamina = math.clamp(ped.currentStamina + (ped.Mass / (ped.Hp / ped.currentHp) * time), 0, ped.maxStamina); }
		}
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			NativeArray<PhysicsStep> a = GravityQuery.ToComponentDataArray<PhysicsStep>(Allocator.TempJob);
			if (a.Length > 0) gravity = a[0].Gravity;
			PlayerInputJob moveJob = new PlayerInputJob
=======
			}
			*/
			[BurstCompile]
			struct ApplyDefferedPhysicsUpdatesJob : IJob
>>>>>>> Stashed changes
			{
				Horizontal = Input.GetAxis("Horizontal"),
				Vertical = Input.GetAxis("Vertical"),
				spaceDown = Input.GetKeyDown(KeyCode.Space),
				LShiftDown = Input.GetKeyDown(KeyCode.LeftShift),
				LCtrlDown = Input.GetKeyDown(KeyCode.LeftControl),
				Mouse1Down = Input.GetKeyDown(KeyCode.Mouse1),
				Mouse2Down = Input.GetKeyDown(KeyCode.Mouse2),
				MouseX = Input.GetAxis("Mouse X"),
				MouseY = Input.GetAxis("Mouse Y"),
				deltaTime = Time.deltaTime,
				attackADown = inputDefinitionsClass.isAttackA(),
				attackBDown = inputDefinitionsClass.isAttackB(),
				attackCDown = inputDefinitionsClass.isAttackC(),
				attackDDown = inputDefinitionsClass.isAttackD(),
				gravity = gravity
			};
			a.Dispose();
			JobHandle moveHandle = moveJob.Schedule(this, inputDeps);

			return moveHandle;
		}
	}
	public class InputDefinitionsClass
	{
		private KeyCode attackA = KeyCode.Q;
		private KeyCode attackB = KeyCode.E;
		private KeyCode attackC = KeyCode.R;
		private KeyCode attackD = KeyCode.F;
		public bool isAttackA() { return Input.GetKeyDown(attackA); }
		public bool isAttackB() { return Input.GetKeyDown(attackB); }
		public bool isAttackC() { return Input.GetKeyDown(attackC); }
		public bool isAttackD() { return Input.GetKeyDown(attackD); }
	}

	/// <summary>
	/// Handles the Pokemon Attacks
	/// </summary>
	public class AttackInputSystem : JobComponentSystem
	{
		private EntityCommandBufferSystem ecbs;
		private EntityQuery pokemonMoveQuery;
		private EntityArchetype pokemonMoveArchtype;
		private GroupIndexSystem groupIndexSystem;

		protected override void OnCreate()
		{
			ecbs = World.GetOrCreateSystem<EntityCommandBufferSystem>();
			pokemonMoveQuery = GetEntityQuery(ComponentType.ReadOnly(typeof(PlayerInput)),
				typeof(PokemonEntityData), typeof(Translation), typeof(Rotation),
				typeof(PhysicsVelocity));
			pokemonMoveArchtype = EntityManager.CreateArchetype(
					typeof(Translation),
					typeof(Rotation),
					typeof(PokemonMoveDataEntity),
					typeof(RenderMesh),
					typeof(LocalToWorld),
					typeof(PhysicsCollider),
					typeof(PhysicsVelocity),
					typeof(PokemonMoveEntity),
					typeof(EntityParent),
					typeof(TranslationProxy),
					typeof(RotationProxy)
				);
			groupIndexSystem = World.GetExistingSystem<GroupIndexSystem>();
		}
		private struct AttackInputJob : IJob
		{
			public EntityCommandBuffer ecb;
			public NativeArray<Entity> entities;
			public NativeArray<PlayerInput> playerInputs;
			public NativeArray<PokemonEntityData> pokemonEntityDatas;
			public NativeArray<PokemonMove> moveSpawnArray;
			private int counter;
			public void Execute()
			{
				counter = 0;
				for (int i = 0; i < entities.Length; i++)
				{
					PokemonMove pm = playerInputs[i].attackADown ? pokemonEntityDatas[i].pokemonMoveSet.pokemonMoveA :
						playerInputs[i].attackBDown ? pokemonEntityDatas[i].pokemonMoveSet.pokemonMoveB :
						playerInputs[i].attackCDown ? pokemonEntityDatas[i].pokemonMoveSet.pokemonMoveC :
						playerInputs[i].attackDDown ? pokemonEntityDatas[i].pokemonMoveSet.pokemonMoveD :
						new PokemonMove { isValid = false };
					//Debug.Log(pm.isValid.Value+","+(pm.name)+","+pm.isValid.Value);
					if (pm.isValid)
					{
						pm = new PokemonMove
						{
							index = i,
							isValid = true,
							name = pm.name
						};
						moveSpawnArray[counter] = pm;
						counter++;
					}
				}
			}
		}
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			if (pokemonMoveQuery.CalculateEntityCount() == 0) return inputDeps;

<<<<<<< Updated upstream
			NativeArray<PokemonMove> pokemonMoves = new NativeArray<PokemonMove>(pokemonMoveQuery.CalculateEntityCount(), Allocator.TempJob);
			NativeArray<PokemonEntityData> pmds = pokemonMoveQuery.ToComponentDataArray<PokemonEntityData>(Allocator.TempJob);
			NativeArray<PlayerInput> playerInputs = pokemonMoveQuery.ToComponentDataArray<PlayerInput>(Allocator.TempJob);
			NativeArray<Entity> entities = pokemonMoveQuery.ToEntityArray(Allocator.TempJob);
			JobHandle jh = new AttackInputJob
			{
				ecb = ecbs.CreateCommandBuffer(),
				entities = entities,
				pokemonEntityDatas = pmds,
				playerInputs = playerInputs,
				moveSpawnArray = pokemonMoves
			}.Schedule(inputDeps);
			jh.Complete();
			int i = 0, counter = 0;
			for (; i < pokemonMoves.Length; i++) if (pokemonMoves[i].isValid) counter++;
			if (counter > 0)
			{
				NativeArray<Entity> pokemonMoveEntities = new NativeArray<Entity>(counter, Allocator.TempJob);
				EntityManager.CreateEntity(pokemonMoveArchtype, pokemonMoveEntities);
				for (i = 0; i < pokemonMoveEntities.Length; i++)
=======
			BuildPhysicsWorld m_BuildPhysicsWorldSystem;
			ExportPhysicsWorld m_ExportPhysicsWorldSystem;
			EndFramePhysicsSystem m_EndFramePhysicsSystem;

			EntityQuery m_CharacterControllersGroup;

			EntityQuery m_stepInput;
			private float3 gravity;
			public float maxSpeedReducer = 10f;

		//	private float DeltaTime;
		//	private float3 up = math.up();

			protected override void OnStartRunning()
			{
				m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
				m_ExportPhysicsWorldSystem = World.GetOrCreateSystem<ExportPhysicsWorld>();
				m_EndFramePhysicsSystem = World.GetOrCreateSystem<EndFramePhysicsSystem>();

				m_stepInput = GetEntityQuery(typeof(PhysicsStep));

				EntityQueryDesc query = new EntityQueryDesc
>>>>>>> Stashed changes
				{

					if (pokemonMoves[i].name.ToString() != "spawnPoke")
					{
<<<<<<< Updated upstream
						PokemonMoveSpawn.ExecutePokemonMove(EntityManager, pokemonMoves[i].name.ToString(),
							EntityManager.GetComponentData<CoreData>(entities[i]).BaseName, entities[i],
							pokemonMoveEntities[i], pmds[i], groupIndexSystem);
					}
					else
					{
						EntityManager.DestroyEntity(pokemonMoveEntities[i]);
						//ignore this because this is a test anyway and i haven't got to this yet
						PokemonDataClass.GeneratePokemonEntity(new CoreData
						{
							BaseName = new ByteString30("Electrode"),
							Name = new ByteString30("Entity"),
							isValid = true,
							scale = new float3(1f,1f,1f),
							size = new float3(1f,1f,1f)
						},
							EntityManager,
							quaternion.identity,
							EntityManager.GetComponentData<Translation>(entities[i]).Value + new float3(0, 10, 0)
						);
=======
						typeof(PhysicsCollider),
						typeof(Translation),
						typeof(Rotation),
						typeof(PokemonEntityData),
						typeof(CoreData),
						typeof(PhysicsVelocity),
						typeof(PhysicsDamping),
						typeof(EntityControllerStepInput)
>>>>>>> Stashed changes
					}
				}
				pokemonMoveEntities.Dispose();
			}
			entities.Dispose();
			pmds.Dispose();
			playerInputs.Dispose();
			pokemonMoves.Dispose();
			return jh;

<<<<<<< Updated upstream
=======
			public static void HandleUserInputA(ref EntityControllerStepInput stepInput, ref PokemonEntityData ped, ref float3 linearVelocity, float MaxSpeedReducer)
			{
				switch (ped.MovementType)
				{
					case PokemonDataClass.MOVEMENT_TYPE_SLUSH_SLIDE:
						Debug.Log("Slush Slide Detected");
						break;
					case PokemonDataClass.MOVEMENT_TYPE_ROLL:

					//	Debug.Log("Roll Detected");
						//	Debug.LogWarning("jumpcount = " + stepInput.input.jumpCount.ToString() + "\nisJumping = " + stepInput.stateData.isJumping.ToString() + "\nmove = " + stepInput.input.Move.ToString() + "\nspeed = " + 
						//		ped.Speed.ToString() + "\bspeedReducer = " + maxSpeedReducer.ToString()+"\nforward = "+stepInput.input.forward.ToString());

						if (stepInput.stateData.supported)
						{
							if (stepInput.input.ReqJump)
							{
								if (stepInput.input.jumpCount > 0)
								{
								//	Debug.LogWarning("Detected too many jumps!");
								}
								else
								{
									// Since the entity is Rolling it normal to a surface is never the EntityNormal so we must use the surfaceNormal

									linearVelocity += ped.jumpHeight * stepInput.CurrentSurfaceNormal;
									stepInput.input.jumpCount++;
									stepInput.stateData.supported = false;
									stepInput.stateData.isJumping = true;
									stepInput.stateData.isCrouching = false;
									stepInput.stateData.isRunning = false;
								}
							}
							else
							{
								stepInput.input.jumpCount = 0;
								stepInput.stateData.isJumping = false;

								stepInput.stateData.isCrouching = stepInput.input.ReqCrouch;
								stepInput.stateData.isRunning = stepInput.input.ReqRun;


								float acceleration = stepInput.stateData.isCrouching ? ped.Acceleration / 2 : stepInput.stateData.isRunning ? ped.Acceleration * 2 : ped.Acceleration;
								float speed = stepInput.stateData.isCrouching ? ped.Speed/MaxSpeedReducer*0.75f : stepInput.stateData.isRunning ? ped.Speed/MaxSpeedReducer*1.25f : ped.Speed / MaxSpeedReducer;
								// We use the CameraTransForm here since the entity is rolling (this causes the entity Vectors to become unrealiable)
								Vector3 tmp = ((stepInput.CameraTransform.Right * stepInput.input.Move.x) + (stepInput.CameraTransform.Forward * stepInput.input.Move.z)) * acceleration * stepInput.DeltaTime;
								// rolling cannot increase y velocity
								tmp.y = 0;

								linearVelocity += (float3)tmp;
								linearVelocity.x = math.clamp(linearVelocity.x, -speed, speed);
								linearVelocity.z = math.clamp(linearVelocity.z, -speed, speed);
							}
						}
						else {
							//	Debug.Log("Detected jumping!");
							if (stepInput.stateData.isJumping)
							{
								// use last surface normal because the entity is in air and CurrentSurfaceNormal no longer updates
								float3 UpVelocity = linearVelocity * stepInput.LastSurfaceNormal;
								bool isGoingUp = math.lengthsq(UpVelocity) > CharacterControllerUtilities.k_SimplexSolverEpsilonSq;

								if (!stepInput.input.ReqJump && isGoingUp)
									linearVelocity.y += ped.jumpHeight * stepInput.DeltaTime * (ped.jumpMultiplier - 1);
								else
									linearVelocity.y += ped.jumpHeight * stepInput.DeltaTime * (ped.longJumpMultiplier - 1);
							}
						}
						break;
					default:
						Debug.LogWarning("Failed to find pokemon movement type \""+ped.MovementType+"\"");
						break;
				}

				//	Debug.LogWarning("after linearvelocity =" + linearVelocity.ToString());
			}

		
			public void CalculateMovement(float currentRotationAngle, float3 up, bool isJumping,
				float3 currentVelocity, float3 desiredVelocity, float3 surfaceNormal, float3 surfaceVelocity, out float3 linearVelocity)
			{

				float3 forward = math.forward(quaternion.AxisAngle(up, currentRotationAngle));
				//	Debug.LogWarning("cc forward = " + forward);
				Rotation surfaceFrame;
				float3 binorm;
				{
					binorm = math.cross(forward, up);
					//		Debug.LogWarning("cca binorm = " + binorm.ToString());
					binorm = math.normalize(binorm);

					//		Debug.LogWarning("ccb binorm = " + binorm.ToString()+"sruface normal = "+surfaceNormal.ToString());
					float3 tangent = math.cross(binorm, surfaceNormal);
					//		Debug.LogWarning("cc tanget = " + tangent.ToString());
					tangent = math.normalize(tangent);
					if (CoreFunctionsClass.IsNaN(tangent)) tangent = new float3();
					//		Debug.LogWarning("cc tanget = " + tangent.ToString());

					binorm = math.cross(tangent, surfaceNormal);
					//		Debug.LogWarning("ccc binorm = " + binorm.ToString());
					binorm = math.normalize(binorm);
					if (CoreFunctionsClass.IsNaN(binorm)) binorm = new float3();
					//		Debug.LogWarning("ccd binorm = " + binorm.ToString());

					surfaceFrame.Value = new quaternion(new float3x3(binorm, tangent, surfaceNormal));
				}
				//	Debug.LogWarning("cc binorm = " + binorm.ToString());
				float3 relative = currentVelocity - surfaceVelocity;
				//	Debug.LogWarning("cc relative = " + relative.ToString());
				relative = math.rotate(math.inverse(surfaceFrame.Value), relative);
				//	Debug.LogWarning("cc relative = " + relative.ToString());
				float3 diff;
				{
					float3 sideVec = math.cross(forward, up);
					float fwd = math.dot(desiredVelocity, forward);
					float side = math.dot(desiredVelocity, sideVec);
					float len = math.length(desiredVelocity);
					float3 desiredVelocitySF = new float3(-side, -fwd, 0.0f);
					desiredVelocitySF = math.normalizesafe(desiredVelocitySF, float3.zero);
					desiredVelocitySF *= len;
					diff = desiredVelocitySF - relative;
				}
				//	Debug.LogWarning("cc diff = " + diff.ToString());
				relative += diff;
				//	Debug.LogWarning("cc relative = " + relative.ToString());
				//	Debug.LogWarning("cc stuff:\n" + math.rotate(surfaceFrame.Value, relative).ToString() + "\n" + surfaceVelocity + "\n" + (isJumping ? math.dot(desiredVelocity, up) * up : float3.zero).ToString());
				linearVelocity = math.rotate(surfaceFrame.Value, relative) + surfaceVelocity +
					(isJumping ? math.dot(desiredVelocity, up) * up : float3.zero);
			}


			private struct tempData
			{
				public float3 gravity;
				public float DeltaTime;
				public float MaxspeedReducer;

			}

			protected override JobHandle OnUpdate(JobHandle inputDeps)
			{
				
				if (m_CharacterControllersGroup.CalculateEntityCount() == 0)
					return inputDeps;

				//get gravity
				NativeArray<PhysicsStep> ps = m_stepInput.ToComponentDataArray<PhysicsStep>(Allocator.TempJob);
				if (ps.Length > 0) gravity = ps[0].Gravity;
				else gravity = new float3(0, -9.81f, 0);
				ps.Dispose();

				tempData tempData = new tempData
				{
					gravity = gravity,
					DeltaTime = Time.DeltaTime,
					MaxspeedReducer = maxSpeedReducer
				};

			//	PlayerInputClass.physicsWorld = m_BuildPhysicsWorldSystem.PhysicsWorld;

				PhysicsWorld physicsWorld = m_BuildPhysicsWorldSystem.PhysicsWorld;
				float deltaTime = Time.DeltaTime;

				JobHandle handle =
				Entities.ForEach((ref Translation position, ref Rotation rotation, ref CoreData coreData, ref PokemonEntityData ped, ref EntityControllerStepInput _stepInput,
				   ref PhysicsVelocity velocity, ref PhysicsDamping damping, ref PhysicsCollider collider) =>
				{
					PhysicsWorld stepPhysicsWorld = physicsWorld;
					float3 gravity = tempData.gravity;
					//	PhysicsWorld physicsWorld = PlayerInputClass.physicsWorld;
					float DeltaTime = tempData.DeltaTime;
					float maxSpeedReducer = tempData.MaxspeedReducer;

					RigidTransform transform = new RigidTransform
					{
						pos = position.Value,
						rot = rotation.Value
					};

					float3 surfaceNormal, surfaceVelocity;

					_stepInput.Velocity = velocity;
					_stepInput.DeltaTime = deltaTime;
					_stepInput.Damping = damping;
					_stepInput.Gravity = gravity;
					_stepInput.RigidBodyIndex = physicsWorld.GetRigidBodyIndex(coreData.entity);



					// first we need to see if the object is supported
					//	CharacterControllerUtilities.CheckSupport(ref physicsWorld, physicsWorld, ref collider, _stepInput, transform, _stepInput.MaxSlope,
					//		ref _stepInput.stateData, out surfaceNormal, out surfaceVelocity, ped.Speed*2);

					// For now we'll use the camera as the up vector
					CharacterControllerUtilities.CheckSupport(ref physicsWorld, ref stepPhysicsWorld, ref collider, ref _stepInput, transform, out surfaceNormal, out surfaceVelocity, ped.Speed,_stepInput.CameraTransform.Up);

					// If the Entity is supported then update the surface normals
					if (_stepInput.stateData.supported)
					{
						_stepInput.CurrentSurfaceNormal = surfaceNormal;
						_stepInput.LastSurfaceNormal = surfaceNormal;
					}

					float3 desiredVelocity = _stepInput.Velocity.Linear;

					HandleUserInputA(ref _stepInput, ref ped, ref desiredVelocity, maxSpeedReducer);
					if (!float.IsNaN(desiredVelocity.x) && !float.IsNaN(desiredVelocity.y) && !float.IsNaN(desiredVelocity.z))
						_stepInput.Velocity.Linear = desiredVelocity;


					// Write back and orientation integration
					velocity = _stepInput.Velocity;
					coreData = new CoreData
					{
						translation = position,
						rotation = rotation,
						BaseName = coreData.BaseName,
						damping = coreData.damping,
						entity = coreData.entity,
						isValid = coreData.isValid,
						Name = coreData.Name,
						scale = coreData.scale,
						size = coreData.size
					};


				}).WithoutBurst().WithName("EntityMovementSystem").Schedule(inputDeps);
					handle.Complete();
					return handle;
				//return inputDeps;
			}
		}
		

		/*[UpdateAfter(typeof(ExportPhysicsWorld)), UpdateBefore(typeof(EndFramePhysicsSystem))]
		public class AEntityControllerSystem : JobComponentSystem
		{

			[BurstCompile]
			struct CharacterControllerJobB : IJobForEach<CoreData, PokemonEntityData, EntityControllerStepInput, PhysicsCollider,PhysicsVelocity,PokemonCameraData>
			{
				[ReadOnly] public float DeltaTime;

				[ReadOnly] public PhysicsWorld PhysicsWorld;
				[ReadOnly] public float3 gravity;
				[ReadOnly] public float MaxSpeedReducer;

				private float3 up;

				public void Execute(ref CoreData coreData, ref PokemonEntityData ped, ref EntityControllerStepInput _stepInput, ref PhysicsCollider collider,ref PhysicsVelocity velocity,ref PokemonCameraData pokemonCameraData)
				{
					up = math.up();
					// Collision filter must be valid (let's assume this for now)
					//	Assert.IsTrue(collider.ColliderPtr->Filter.IsValid);

					TEntityControllerStepInput TStepInput = new TEntityControllerStepInput
					{
						pw = PhysicsWorld,
						stepInput = new EntityControllerStepInput
						{
							DeltaTime = DeltaTime,
							Up = math.up(),
							Gravity = gravity,
							MaxIterations = _stepInput.MaxIterations,
							Tau = _stepInput.Tau,
							LinearDamping = coreData.damping.Linear,
							AngularDamping = coreData.damping.Angular,
							SkinWidth = _stepInput.SkinWidth,
							ContactTolerance = _stepInput.ContactTolerance,
							MaxSlope = _stepInput.MaxSlope,
							RigidBodyIndex = PhysicsWorld.GetRigidBodyIndex(coreData.entity),
							LinearVelocity = velocity.Linear,
							AngularVelocity = velocity.Angular,
							stateData = _stepInput.stateData,
							input = _stepInput.input
						}
					};
					// Character transform
					RigidTransform transform = new RigidTransform
					{
						pos = coreData.translation.Value,
						rot = coreData.rotation.Value
					};

					float3 surfaceNormal, surfaceVelocity;

					// Check support
					CharacterControllerUtilities.CheckSupport(ref PhysicsWorld, TStepInput.pw, ref collider, TStepInput.stepInput, transform, TStepInput.stepInput.MaxSlope,
						ref TStepInput.stepInput.stateData, out surfaceNormal, out surfaceVelocity, ped.Speed);
					//	Debug.Log("ddd"+TStepInput.stepInput.stateData.supported.Value);

					// User input
					float3 desiredVelocity = TStepInput.stepInput.LinearVelocity;
					HandleUserInputA(ref TStepInput.stepInput, ref ped, TStepInput.stepInput.Up, surfaceVelocity, ref desiredVelocity, pokemonCameraData.smoothingSpeed);

					// Calculate actual velocity with respect to surface
					if (TStepInput.stepInput.stateData.supported)
					{
						CalculateMovement(TStepInput.stepInput.CurrentRotationAngle, TStepInput.stepInput.Up, TStepInput.stepInput.stateData.isJumping,
							TStepInput.stepInput.LinearVelocity, desiredVelocity, surfaceNormal, surfaceVelocity, out TStepInput.stepInput.LinearVelocity);
					}
					else
					{
						TStepInput.stepInput.LinearVelocity = desiredVelocity;
					}

					// World collision + integrate
				//	CharacterControllerUtilities.CollideAndIntegrate(TStepInput.pw, ped, TStepInput.stepInput, ped.Mass, TStepInput.stepInput.AffectsPhysicsBodies > 0,
				//		collider.ColliderPtr, ref transform, ref TStepInput.stepInput.LinearVelocity, ref DeferredImpulseWriter);

					// Write back and orientation integration
					coreData.translation = new Translation { Value = transform.pos };
					coreData.rotation = new Rotation { Value = quaternion.AxisAngle(up, TStepInput.stepInput.CurrentRotationAngle) };
					//	Debug.Log(desiredVelocity);
					// Write back to chunk data
					{
						_stepInput = TStepInput.stepInput;
						velocity = new PhysicsVelocity
						{
							Linear = desiredVelocity,
							Angular = TStepInput.stepInput.AngularVelocity
						};
					}
				/*	{
						chunkStepInputData[i] = TStepInput.stepInput;
						chunkPhysicsVelocityData[0] = new PhysicsVelocity
						{
							Linear = desiredVelocity,
							Angular = TStepInput.stepInput.AngularVelocity
						};

						//	chunkTranslationData[i] = position;
						//	chunkRotationData[i] = rotation;

					}
				}

				private void HandleUserInputA(ref EntityControllerStepInput stepInput, ref PokemonEntityData ped, float3 up, float3 surfaceVelocity, ref float3 linearVelocity, float smoothingSpeed)
				{
					switch (ped.MovementType)
					{
						case PokemonDataClass.MOVEMENT_TYPE_SLUSH_SLIDE:
						case PokemonDataClass.MOVEMENT_TYPE_ROLL:
							if (stepInput.stateData.supported)
							{
								stepInput.input.jumpCount = 0;
								stepInput.stateData.isJumping = false;
								//	Debug.Log(stepInput.input.Move.x);
								if (stepInput.input.Move.x != 0 || stepInput.input.Move.y != 0)
								{
									//		Debug.Log(stepInput.input.forward +","+stepInput.input.right+","+stepInput.DeltaTime+","+ped.Acceleration);
									linearVelocity += ((stepInput.input.forward * stepInput.input.Move.y) + (stepInput.input.right * stepInput.input.Move.x) * stepInput.DeltaTime * (stepInput.input.ReqRun ? ped.Acceleration * 2 : stepInput.input.ReqCrouch ? ped.Acceleration / 2 : stepInput.input.ReqProne ? ped.Acceleration / 4 : ped.Acceleration));
									linearVelocity.x = Mathf.Clamp(linearVelocity.x, -ped.Speed / MaxSpeedReducer, ped.Speed / MaxSpeedReducer);
									linearVelocity.z = Mathf.Clamp(linearVelocity.z, -ped.Speed / MaxSpeedReducer, ped.Speed / MaxSpeedReducer);
									//	linearVelocity.y = 0f;
								}
								if (stepInput.input.ReqJump)
								{
									//need jump that goes oppisite of gavity
									//		Debug.Log("this shoulf only run once");
									linearVelocity.y += ped.jumpHeight;
									stepInput.input.jumpCount++;
									stepInput.stateData.supported = false;
									stepInput.stateData.isJumping = true;
								}
							}
							{
								if (stepInput.stateData.isJumping)
								{
									if (!stepInput.input.ReqJump && linearVelocity.y > 0)
										linearVelocity.y += ped.jumpHeight * stepInput.DeltaTime * (ped.jumpMultiplier - 1);
									else
										linearVelocity.y += ped.jumpHeight * stepInput.DeltaTime * (ped.longJumpMultiplier - 1);
								}
							}
							//	Debug.Log(linearVelocity.y);
							break;
						default:// Debug.LogWarning("Failed to find pokemon movement type \""+ped.MovementType+"\"");
							break;
					}
				}

				private void CalculateMovement(float currentRotationAngle, float3 up, bool isJumping,
					float3 currentVelocity, float3 desiredVelocity, float3 surfaceNormal, float3 surfaceVelocity, out float3 linearVelocity)
				{
					float3 forward = math.forward(quaternion.AxisAngle(up, currentRotationAngle));

					Rotation surfaceFrame;
					float3 binorm;
					{
						binorm = math.cross(forward, up);
						binorm = math.normalize(binorm);

						float3 tangent = math.cross(binorm, surfaceNormal);
						tangent = math.normalize(tangent);

						binorm = math.cross(tangent, surfaceNormal);
						binorm = math.normalize(binorm);

						surfaceFrame.Value = new quaternion(new float3x3(binorm, tangent, surfaceNormal));
					}

					float3 relative = currentVelocity - surfaceVelocity;
					relative = math.rotate(math.inverse(surfaceFrame.Value), relative);

					float3 diff;
					{
						float3 sideVec = math.cross(forward, up);
						float fwd = math.dot(desiredVelocity, forward);
						float side = math.dot(desiredVelocity, sideVec);
						float len = math.length(desiredVelocity);
						float3 desiredVelocitySF = new float3(-side, -fwd, 0.0f);
						desiredVelocitySF = math.normalizesafe(desiredVelocitySF, float3.zero);
						desiredVelocitySF *= len;
						diff = desiredVelocitySF - relative;
					}

					relative += diff;

					linearVelocity = math.rotate(surfaceFrame.Value, relative) + surfaceVelocity +
						(isJumping ? math.dot(desiredVelocity, up) * up : float3.zero);
				}

			}

			[BurstCompile]
			struct ApplyDefferedPhysicsUpdatesJob : IJob
			{
				// Chunks can be deallocated at this point
				[DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> Chunks;

				public NativeStream.Reader DeferredImpulseReader;

				public ComponentDataFromEntity<PhysicsVelocity> PhysicsVelocityData;
				public ComponentDataFromEntity<PhysicsMass> PhysicsMassData;
				public ComponentDataFromEntity<Translation> TranslationData;
				public ComponentDataFromEntity<Rotation> RotationData;

				public void Execute()
				{
					int index = 0;
					int maxIndex = DeferredImpulseReader.ForEachCount;
					DeferredImpulseReader.BeginForEachIndex(index++);
					while (DeferredImpulseReader.RemainingItemCount == 0 && index < maxIndex)
					{
						DeferredImpulseReader.BeginForEachIndex(index++);
					}

					while (DeferredImpulseReader.RemainingItemCount > 0)
					{
						// Read the data
						var impulse = DeferredImpulseReader.Read<DeferredCharacterControllerImpulse>();
						while (DeferredImpulseReader.RemainingItemCount == 0 && index < maxIndex)
						{
							DeferredImpulseReader.BeginForEachIndex(index++);
						}

						PhysicsVelocity pv = PhysicsVelocityData[impulse.Entity];
						PhysicsMass pm = PhysicsMassData[impulse.Entity];
						Translation t = TranslationData[impulse.Entity];
						Rotation r = RotationData[impulse.Entity];

						// Don't apply on kinematic bodies
						if (pm.InverseMass > 0.0f)
						{
							// Apply impulse
							pv.ApplyImpulse(pm, t, r, impulse.Impulse, impulse.Point);

							// Write back
							PhysicsVelocityData[impulse.Entity] = pv;
						}
					}
				}
			}

			BuildPhysicsWorld m_BuildPhysicsWorldSystem;
			ExportPhysicsWorld m_ExportPhysicsWorldSystem;
			EndFramePhysicsSystem m_EndFramePhysicsSystem;

			EntityQuery m_CharacterControllersGroup;

			EntityQuery m_stepInput;
			private float3 gravity;
			public float maxSpeedReducer = 10f;
			protected override void OnCreate()
			{
				m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
				m_ExportPhysicsWorldSystem = World.GetOrCreateSystem<ExportPhysicsWorld>();
				m_EndFramePhysicsSystem = World.GetOrCreateSystem<EndFramePhysicsSystem>();

				m_stepInput = GetEntityQuery(typeof(PhysicsStep));

				EntityQueryDesc query = new EntityQueryDesc
				{
					All = new ComponentType[]
					{
						typeof(PhysicsCollider),
						typeof(Translation),
						typeof(Rotation),
						typeof(PokemonEntityData),
						typeof(CoreData),
						typeof(PhysicsVelocity),
						typeof(PhysicsDamping),
						typeof(PokemonCameraData),
						typeof(EntityControllerStepInput)
					}
				};
				m_CharacterControllersGroup = GetEntityQuery(query);
			}

			protected override JobHandle OnUpdate(JobHandle inputDeps)
			{

				if (m_CharacterControllersGroup.CalculateEntityCount() == 0)
					return inputDeps;
				//get gravity
				NativeArray<PhysicsStep> ps = m_stepInput.ToComponentDataArray<PhysicsStep>(Allocator.TempJob);
				if (ps.Length > 0) gravity = ps[0].Gravity;
				else gravity = new float3(0, -9.81f, 0);
				ps.Dispose();
				
				var ccJob = new CharacterControllerJobB
				{
					// Archetypes
					gravity = gravity,
					// Input
					DeltaTime = UnityEngine.Time.fixedDeltaTime,
					PhysicsWorld = m_BuildPhysicsWorldSystem.PhysicsWorld,
					MaxSpeedReducer = maxSpeedReducer
				};
				inputDeps = JobHandle.CombineDependencies(inputDeps, m_ExportPhysicsWorldSystem.FinalJobHandle);
				inputDeps = ccJob.Schedule(m_CharacterControllersGroup, inputDeps);

				/*		var applyJob = new ApplyDefferedPhysicsUpdatesJob()
						{
							Chunks = chunks,
							DeferredImpulseReader = deferredImpulses.AsReader(),
							PhysicsVelocityData = GetComponentDataFromEntity<PhysicsVelocity>(),
							PhysicsMassData = GetComponentDataFromEntity<PhysicsMass>(),
							TranslationData = GetComponentDataFromEntity<Translation>(),
							RotationData = GetComponentDataFromEntity<Rotation>()
						};

						inputDeps = applyJob.Schedule(inputDeps);
					
				
				// Must finish all jobs before physics step end
				m_EndFramePhysicsSystem.HandlesToWaitFor.Add(inputDeps);

				return inputDeps;
			}
>>>>>>> Stashed changes
		}
		*/

	}
}
