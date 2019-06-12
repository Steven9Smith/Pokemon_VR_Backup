using Unity.Mathematics;
using UnityEngine;
using Unity.Entities;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Collections;
using Unity.Physics;
using Unity.Burst;
using Pokemon.Move;
using Unity.Transforms;

namespace Pokemon.Player
{
	public class PlayerInputSystem : JobComponentSystem
	{
		public EntityQuery GravityQuery;
		public PhysicsStep step;
		public float3 gravity = -new float3(0, -9.81f, 0);
		private InputDefinitionsClass inputDefinitionsClass;
		protected override void OnCreate()
		{
			GravityQuery = GetEntityQuery(typeof(PhysicsStep));
			inputDefinitionsClass = new InputDefinitionsClass();
		}
		[BurstCompile]
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
				gainEnergy(ref pokemonEntityData, deltaTime);
			}
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
			private void PlayerMovement([ReadOnly]PlayerInput input, ref PokemonEntityData pokemonEntityData, ref PhysicsVelocity velocity, ref StateData stateData)
			{
				float3 force = float3.zero;
				float3 maxVelocity = math.abs(velocity.Linear);
				float acceleration = pokemonEntityData.Acceleration;
				//improve ground detection funciton in future
				if (math.abs(velocity.Linear.y) < 1 && math.abs(velocity.Linear.y) > 0) stateData.onGround = true;
				else stateData.onGround = false;
				if (stateData.onGround)
				{
					if (input.LShiftDown)
					{
						maxVelocity *= 2;
						acceleration *= 2;
						stateData.isCreeping = false;
						stateData.isRunning = true;
					}
					else if (input.LCtrlDown)
					{
						maxVelocity *= 0.5f;
						acceleration *= 0.5f;
						stateData.isRunning = true;
					}
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
					if (input.SpaceDown.Value == 1 && stateData.onGround)
					{
						float realJumpheight = pokemonEntityData.jumpHeight < pokemonEntityData.currentStamina ? pokemonEntityData.jumpHeight : pokemonEntityData.currentStamina;
						velocity.Linear.y += realJumpheight;
						pokemonEntityData.currentStamina = math.clamp(pokemonEntityData.currentStamina - realJumpheight, 0, pokemonEntityData.maxStamina);
					}
				}

				force *= deltaTime;
				//			Debug.Log("input "+input.forward+" force = "+force+" velocity = "+velocity.Linear);
				velocity.Linear += force;
				//		Debug.Log("move = "+input.Move+" | acceleration = "+acceleration+" | playerMaxSpeed = "+playerMaxSpeed+" \nvelocity = "+velocity.Linear+" rotation = "+rotation.Value);
			}
			private void gainEnergy(ref PokemonEntityData ped, float time) { if (ped.currentStamina < ped.maxStamina && ped.currentHp > 0) ped.currentStamina = math.clamp(ped.currentStamina + (ped.Mass / (ped.Hp / ped.currentHp) * time), 0, ped.maxStamina); }
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
	public class AttackInputSystem : JobComponentSystem
	{
		private EntityCommandBufferSystem ecbs;
		private EntityQuery pokemonMoveQuery;
		private Entity entity;
		private EntityArchetype pokemonMoveArchtype;
		protected override void OnCreateManager()
		{
			ecbs = World.GetOrCreateSystem<EntityCommandBufferSystem>();
			pokemonMoveQuery = GetEntityQuery(ComponentType.ReadOnly(typeof(PlayerInput)),
				typeof(PokemonEntityData), typeof(Translation), typeof(Rotation),
				typeof(PhysicsVelocity));
			pokemonMoveArchtype = EntityManager.CreateArchetype(
					typeof(Translation),
					typeof(Rotation),
					typeof(PokemonMoveDataEntity),
					typeof(MeshRenderer),
					typeof(LocalToWorld)
				);
			entity = EntityManager.CreateEntity(pokemonMoveArchtype);
		}
		private struct AttackInputJob : IJob
		{
			public EntityCommandBuffer ecb;
			public NativeArray<Entity> entities;
			[DeallocateOnJobCompletion] public NativeArray<PlayerInput> playerInputs;
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
						playerInputs[i].attackDDown ? pokemonEntityDatas[i].pokemonMoveSet.pokemonMoveD : new PokemonMove { isValid = false };
					if (pm.isValid)
					{
						Debug.Log("Detected an attack");
						if (pm.followEntity)
						{
							Debug.Log("add component");
							ecb.AddComponent<PokemonMoveData>(entities[i], PokemonMoves.getPokemonMoveData(pm.name,pokemonEntityDatas[i]));
						}
						else
						{
							Debug.Log("not adding");
							pm = new PokemonMove
							{
								followEntity = pm.followEntity,
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
		}
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			if (pokemonMoveQuery.CalculateLength() == 0) return inputDeps;
			
			NativeArray<PokemonMove> pokemonMoves = new NativeArray<PokemonMove>(pokemonMoveQuery.CalculateLength(), Allocator.TempJob);
			NativeArray<PlayerInput> playerInputs = new NativeArray<PlayerInput>(pokemonMoves.Length, Allocator.TempJob);
			NativeArray<Entity> entities = pokemonMoveQuery.ToEntityArray(Allocator.TempJob);
			JobHandle jh = new AttackInputJob {
				ecb = ecbs.CreateCommandBuffer(),
				entities = entities,
				pokemonEntityDatas = pokemonMoveQuery.ToComponentDataArray<PokemonEntityData>(Allocator.TempJob),
				playerInputs = pokemonMoveQuery.ToComponentDataArray<PlayerInput>(Allocator.TempJob),
				moveSpawnArray = pokemonMoves
			}.Schedule(inputDeps);
			jh.Complete();
			int i = 0, counter = 0;
			for (; i < pokemonMoves.Length; i++)
				if (pokemonMoves[i].isValid) counter++;
			if (counter > 0)
			{
				NativeArray<Entity> pokemonMoveEntities = new NativeArray<Entity>(counter,Allocator.TempJob);
				for (i = 0; i < counter; i++) {
					Entity tempEntity = EntityManager.CreateEntity(pokemonMoveArchtype);
					PokemonMoveDataSpawn pmds = PokemonMoves.getPokemonMoveDataSpawn(pokemonMoves[i].name,
						EntityManager.GetComponentData<PokemonEntityData>(entities[pokemonMoves[i].index]));
					EntityManager.SetComponentData<Translation>(tempEntity,new Translation {
						Value = pmds.TranslationOffset.getFromEntity ? 
							EntityManager.GetComponentData<Translation>(entities[pokemonMoves[i].index]).Value+pmds.TranslationOffset.value : 
							pmds.TranslationOffset.value
						});
					EntityManager.SetComponentData<Rotation>(tempEntity, new Rotation
					{
						//https://answers.unity.com/questions/1353333/how-to-add-2-quaternions.html
						Value = pmds.RotationOffset.getFromEntity ?
							EntityManager.GetComponentData<Rotation>(entities[pokemonMoves[i].index]).Value.value + pmds.RotationOffset.value.value :
							pmds.RotationOffset.value
					});

					EntityManager.SetComponentData<PokemonMoveDataEntity>(tempEntity, PokemonMoves.GetPokemonMoveDataEntity(
						pokemonMoves[i].name,
						EntityManager.GetComponentData<PokemonEntityData>(entities[pokemonMoves[i].index])
					));
					EntityManager.SetName(entities[pokemonMoves[i].index],PokemonIO.ByteString30ToString(pokemonMoves[i].name)+i);

				}
				pokemonMoveEntities.Dispose();
			}
			entities.Dispose();
			pokemonMoves.Dispose();
			
			return jh;

		}
	}

}
