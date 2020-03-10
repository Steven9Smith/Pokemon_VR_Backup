
using Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Physics.Extensions;
using Unity.Transforms;
using UnityEngine.Assertions;
using Pokemon.EntityController;

namespace Pokemon
{
	namespace Player
	{
		public struct EntityControllerStepInput : IComponentData
		{
		//	public float CurrentRotationAngle;
			public StateData stateData;
			public float3 LinearVelocity;
			public float3 AngularVelocity;
			public PlayerInput input;
			public float DeltaTime;
			public float3 Up;
			public float3 Gravity;
			public int MaxIterations;
			public float Tau;
			public float Damping;
			public float SkinWidth;
			public float ContactTolerance;
			public float MaxSlope;
			public int RigidBodyIndex;
		}
		[UpdateAfter(typeof(ExportPhysicsWorld)), UpdateBefore(typeof(EndFramePhysicsSystem))]
		public class CharacterControllerSystem : JobComponentSystem
		{
			const float k_DefaultTau = 0.4f;
			const float k_DefaultDamping = 0.9f;

			[BurstCompile]
			struct CharacterControllerJob : IJobChunk
			{
				public float DeltaTime;

				[ReadOnly] public PhysicsWorld PhysicsWorld;
				[ReadOnly] public float3 gravity;

				public ArchetypeChunkComponentType<Translation> TranslationType;
				public ArchetypeChunkComponentType<Rotation> RotationType;
				public ArchetypeChunkComponentType<CoreData> coreDataType;
				public ArchetypeChunkComponentType<PokemonEntityData> pedType;
				public ArchetypeChunkComponentType<EntityControllerStepInput> stepInputType;
				public ArchetypeChunkComponentType<PhysicsVelocity> physicsVelocityType;
				public ArchetypeChunkComponentType<PhysicsDamping> physicsDampingType;

				private NativeArray<PhysicsWorld> worlds;

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

					DeferredImpulseWriter.BeginForEachIndex(chunkIndex);

					worlds = new NativeArray<PhysicsWorld>(chunk.Count, Allocator.TempJob);

					for (int i = 0; i < chunk.Count; i++)
					{
						var ped = chunkPedData[i];
						var stepInput = chunkStepInputData[i];
						var collider = chunkPhysicsColliderData[i];
						var position = chunkTranslationData[i];
						var rotation = chunkRotationData[i];
						var coreData = chunkCoreData[i];
						var velocity = chunkPhysicsVelocityData[i];
						var damping = chunkPhysicsDampingData[i];

						// Collision filter must be valid
						Assert.IsTrue(collider.ColliderPtr->Filter.IsValid);
						worlds[i] = PhysicsWorld;
						// Character step input
						stepInput = new EntityControllerStepInput
						{
							DeltaTime = DeltaTime,
							Up = math.up(),
							Gravity = gravity,
							MaxIterations = stepInput.MaxIterations,
							Tau = k_DefaultTau,
							Damping = k_DefaultDamping,
							SkinWidth = stepInput.SkinWidth,
							ContactTolerance = stepInput.ContactTolerance,
							MaxSlope = stepInput.MaxSlope,
							RigidBodyIndex = PhysicsWorld.GetRigidBodyIndex(coreData.entity),
							LinearVelocity = velocity.Linear,
							AngularVelocity = velocity.Angular,
							stateData = stepInput.stateData,
							input = stepInput.input
						};

						// Character transform
						RigidTransform transform = new RigidTransform
						{
							pos = position.Value,
							rot = rotation.Value
						};

						float3 surfaceNormal, surfaceVelocity;

						// Check support
						CharacterControllerUtilities.CheckSupport(ref PhysicsWorld,worlds[i], ref collider, stepInput, transform, stepInput.MaxSlope,
							ref stepInput.stateData, out surfaceNormal, out  surfaceVelocity,ped.Speed);

						// User input
						float3 desiredVelocity = stepInput.LinearVelocity;
						HandleUserInput(ccComponentData, stepInput.Up, surfaceVelocity, ref ccInternalData, ref desiredVelocity);

						// Calculate actual velocity with respect to surface
						if (ccInternalData.SupportedState == CharacterSupportState.Supported)
						{
							CalculateMovement(ccInternalData.CurrentRotationAngle, stepInput.Up, ccInternalData.IsJumping,
								ccInternalData.LinearVelocity, desiredVelocity, surfaceNormal, surfaceVelocity, out ccInternalData.LinearVelocity);
						}
						else
						{
							ccInternalData.LinearVelocity = desiredVelocity;
						}

						// World collision + integrate
						CollideAndIntegrate(stepInput, ccComponentData.CharacterMass, ccComponentData.AffectsPhysicsBodies > 0,
							collider.ColliderPtr, ref transform, ref ccInternalData.LinearVelocity, ref DeferredImpulseWriter);

						// Write back and orientation integration
						position.Value = transform.pos;
						rotation.Value = quaternion.AxisAngle(up, ccInternalData.CurrentRotationAngle);

						// Write back to chunk data
						{
							chunkCCInternalData[i] = ccInternalData;
							chunkTranslationData[i] = position;
							chunkRotationData[i] = rotation;
						}
					}

					worlds.Dispose();

					DeferredImpulseWriter.EndForEachIndex();
				}

				private void HandleUserInput(CharacterControllerComponentData ccComponentData, float3 up, float3 surfaceVelocity,
					ref CharacterControllerInternalData ccInternalData, ref float3 linearVelocity)
				{
					// Reset jumping state and unsupported velocity
					if (ccInternalData.SupportedState == CharacterSupportState.Supported)
					{
						ccInternalData.IsJumping = false;
						ccInternalData.UnsupportedVelocity = float3.zero;
					}

					// Movement and jumping
					bool shouldJump = false;
					float3 requestedMovementDirection = float3.zero;
					{
						float3 forward = math.forward(quaternion.identity);
						float3 right = math.cross(up, forward);

						float horizontal = ccInternalData.Input.Movement.x;
						float vertical = ccInternalData.Input.Movement.y;
						bool jumpRequested = ccInternalData.Input.Jumped > 0;
						bool haveInput = (math.abs(horizontal) > float.Epsilon) || (math.abs(vertical) > float.Epsilon);
						if (haveInput)
						{
							float3 localSpaceMovement = forward * vertical + right * horizontal;
							float3 worldSpaceMovement = math.rotate(quaternion.AxisAngle(up, ccInternalData.CurrentRotationAngle), localSpaceMovement);
							requestedMovementDirection = math.normalize(worldSpaceMovement);
						}
						shouldJump = jumpRequested && ccInternalData.SupportedState == CharacterSupportState.Supported;
					}

					// Turning
					{
						float horizontal = ccInternalData.Input.Looking.x;
						bool haveInput = (math.abs(horizontal) > float.Epsilon);
						if (haveInput)
						{
							ccInternalData.CurrentRotationAngle += horizontal * ccComponentData.RotationSpeed * DeltaTime;
						}
					}

					// Apply input velocities
					{
						if (shouldJump)
						{
							// Add jump speed to surface velocity and make character unsupported
							ccInternalData.IsJumping = true;
							ccInternalData.SupportedState = CharacterSupportState.Unsupported;
							ccInternalData.UnsupportedVelocity = surfaceVelocity + ccComponentData.JumpUpwardsSpeed * up;
						}
						else if (ccInternalData.SupportedState != CharacterSupportState.Supported)
						{
							// Apply gravity
							ccInternalData.UnsupportedVelocity += ccComponentData.Gravity * DeltaTime;
						}
						// If unsupported then keep jump and surface momentum
						linearVelocity = requestedMovementDirection * ccComponentData.MovementSpeed +
							(ccInternalData.SupportedState != CharacterSupportState.Supported ? ccInternalData.UnsupportedVelocity : float3.zero);
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

			protected override void OnCreate()
			{
				m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
				m_ExportPhysicsWorldSystem = World.GetOrCreateSystem<ExportPhysicsWorld>();
				m_EndFramePhysicsSystem = World.GetOrCreateSystem<EndFramePhysicsSystem>();

				EntityQueryDesc query = new EntityQueryDesc
				{
					All = new ComponentType[]
					{
				typeof(CharacterControllerComponentData),
				typeof(CharacterControllerInternalData),
				typeof(PhysicsCollider),
				typeof(Translation),
				typeof(Rotation),
					}
				};
				m_CharacterControllersGroup = GetEntityQuery(query);
			}

			protected override JobHandle OnUpdate(JobHandle inputDeps)
			{
				if (m_CharacterControllersGroup.CalculateEntityCount() == 0)
					return inputDeps;

				var chunks = m_CharacterControllersGroup.CreateArchetypeChunkArray(Allocator.TempJob);

				var ccComponentType = GetArchetypeChunkComponentType<CharacterControllerComponentData>();
				var ccInternalType = GetArchetypeChunkComponentType<CharacterControllerInternalData>();
				var physicsColliderType = GetArchetypeChunkComponentType<PhysicsCollider>();
				var translationType = GetArchetypeChunkComponentType<Translation>();
				var rotationType = GetArchetypeChunkComponentType<Rotation>();

				var deferredImpulses = new NativeStream(chunks.Length, Allocator.TempJob);

				var ccJob = new CharacterControllerJob
				{
					// Archetypes
					CharacterControllerComponentType = ccComponentType,
					CharacterControllerInternalType = ccInternalType,
					PhysicsColliderType = physicsColliderType,
					TranslationType = translationType,
					RotationType = rotationType,
					// Input
					DeltaTime = UnityEngine.Time.fixedDeltaTime,
					PhysicsWorld = m_BuildPhysicsWorldSystem.PhysicsWorld,
					DeferredImpulseWriter = deferredImpulses.AsWriter()
				};

				inputDeps = JobHandle.CombineDependencies(inputDeps, m_ExportPhysicsWorldSystem.FinalJobHandle);
				inputDeps = ccJob.Schedule(m_CharacterControllersGroup, inputDeps);

				var applyJob = new ApplyDefferedPhysicsUpdatesJob()
				{
					Chunks = chunks,
					DeferredImpulseReader = deferredImpulses.AsReader(),
					PhysicsVelocityData = GetComponentDataFromEntity<PhysicsVelocity>(),
					PhysicsMassData = GetComponentDataFromEntity<PhysicsMass>(),
					TranslationData = GetComponentDataFromEntity<Translation>(),
					RotationData = GetComponentDataFromEntity<Rotation>()
				};

				inputDeps = applyJob.Schedule(inputDeps);
				var disposeHandle = deferredImpulses.Dispose(inputDeps);

				// Must finish all jobs before physics step end
				m_EndFramePhysicsSystem.HandlesToWaitFor.Add(disposeHandle);

				return inputDeps;
			}
		}
	}
}