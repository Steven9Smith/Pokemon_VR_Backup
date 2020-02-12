using Unity.Mathematics;
using UnityEngine;
using Unity.Entities;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Collections;
using Unity.Physics;
using Pokemon.Move;
using Unity.Transforms;
using Unity.Rendering;
using Core.ParentChild;
using Core.Spawning;
using Core;

namespace Pokemon.Player
{
	public class PlayerInputSystem : JobComponentSystem
	{
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
				//improve ground detection by using Collision Filters rather than bad math
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
					}
					//entity is Walking
					else if (maxVelocity.x > 0 || maxVelocity.z > 0){if (!stateData.isWalking) StateDataClass.SetState(ref stateData, StateDataClass.State.Crouching);}
					else if (!stateData.isIdle) StateDataClass.SetState(ref stateData, StateDataClass.State.Idle);
					//add speed if pokemon has not reach its max speed
					if (maxVelocity.x < pokemonEntityData.Speed && maxVelocity.z < pokemonEntityData.Speed)
					{
						if (input.Move.z > 0)
						{
							force += input.forward;
						}
						else if (input.Move.z < 0)
						{
							force -= input.forward;
						}
						if (input.Move.x > 0)
						{
							force += input.right;
						}
						else if (input.Move.x < 0)
						{
							force -= input.right;
						}
						force *= acceleration;
					}

			//		if(maxVelocity.y < 0)
			//		{
//
//					}


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
				force.y = 0f;
				//Debug.Log("input "+input.forward+" force = "+force+" velocity = "+velocity.Linear);
			//	if(pokemonEntityData.BodyType != PokemonDataClass.BODY_TYPE_HEAD_ONLY) velocity.Linear += force;
			//	else velocity.Angular += force;
				velocity.Linear += force;
				//Debug.Log("move = "+input.Move+" | acceleration = "+acceleration+" | playerMaxSpeed = "+playerMaxSpeed+" \nvelocity = "+velocity.Linear+" rotation = "+rotation.Value);
			}
			private void gainEnergy(StateData stateData,ref PokemonEntityData ped, float time) { if (ped.currentStamina < ped.maxStamina && ped.currentHp > 0 && !stateData.isRunning ) ped.currentStamina = math.clamp(ped.currentStamina + (ped.Mass / (ped.Hp / ped.currentHp) * time), 0, ped.maxStamina); }
		}
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			NativeArray<PhysicsStep> a = GravityQuery.ToComponentDataArray<PhysicsStep>(Allocator.TempJob);
			if (a.Length > 0) gravity = a[0].Gravity;
			PlayerInputJob moveJob = new PlayerInputJob
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
				deltaTime = Time.DeltaTime,
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
				{

					if (pokemonMoves[i].name.ToString() != "spawnPoke")
					{
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
					}
				}
				pokemonMoveEntities.Dispose();
			}
			entities.Dispose();
			pmds.Dispose();
			playerInputs.Dispose();
			pokemonMoves.Dispose();
			return jh;

		}
	}
}
