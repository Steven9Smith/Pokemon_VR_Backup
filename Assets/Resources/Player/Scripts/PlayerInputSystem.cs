using Unity.Mathematics;
using UnityEngine;
using Unity.Entities;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Collections;
using Unity.Physics;
using Unity.Burst;

namespace Pokemon.Player
{
	public class PlayerInputSystem : JobComponentSystem
	{
		public EntityQuery GravityQuery;
		public PhysicsStep step;
		public float3 gravity = -new float3(0,-9.81f,0);
		protected override void OnCreate()
		{
			GravityQuery = GetEntityQuery(typeof(PhysicsStep));
		}
		[BurstCompile]
		struct PlayerInputJob : IJobForEach<PlayerInput,PokemonEntityData, PhysicsVelocity, StateData, PhysicsCollider>
		{
			public float deltaTime;
			public float Horizontal;
			public float Vertical,MouseX,MouseY;
			public float3 gravity;
			public BlittableBool spaceDown;
			public BlittableBool LShiftDown;
			public BlittableBool LCtrlDown;
			public BlittableBool Mouse1Down;
			public BlittableBool Mouse2Down;
			public BlittableBool EDown;

			public void Execute(ref PlayerInput playerInput,ref PokemonEntityData pokemonEntityData, ref PhysicsVelocity physicsVelocity, ref StateData stateData,ref PhysicsCollider physicsCollider)
			{
				UpdatePlayerInput(ref playerInput);
				PlayerMovement(playerInput,pokemonEntityData,ref physicsVelocity,ref stateData);
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
				playerInput.EDown = EDown;
			}
			private void PlayerMovement([ReadOnly]PlayerInput input, [ReadOnly] PokemonEntityData pokemonEntityData, ref PhysicsVelocity velocity,ref StateData stateData)
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
						maxVelocity *= 2;
						acceleration *= 2;
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
				//		Debug.Log("attempting to jump! "+pokemonEntityData.jumpHeight);
						velocity.Linear.y += pokemonEntityData.jumpHeight;
					}
				}

				force *= deltaTime;
	//			Debug.Log("input "+input.forward+" force = "+force+" velocity = "+velocity.Linear);
				velocity.Linear += force;
		//		Debug.Log("move = "+input.Move+" | acceleration = "+acceleration+" | playerMaxSpeed = "+playerMaxSpeed+" \nvelocity = "+velocity.Linear+" rotation = "+rotation.Value);
			}

		}
		/// <summary>
		/// preforms boolean tests
		/// </summary>
		/// <param name="a">a float3</param>
		/// <param name="b">another float3</param>
		/// <param name="mode">which test to preform
		/// 1 && ==
		/// 2 && <
		/// 3 && >
		/// 4 && <=
		/// 5 && >=
		/// 6 || ==
		/// 7 || <
		/// 8 || >
		/// 9 || <=
		/// 10 || >=
		/// </param>
		/// <returns></returns>
		public static bool CompareFloat3(float3 a, float3 b,int mode=0)
		{
			switch (mode)
			{
				case 1: return a.x == b.x && a.y == b.y && a.z == b.z;
				case 2: return a.x < b.x && a.y < b.y && a.z < b.z;
				case 3: return a.x > b.x && a.y > b.y && a.z > b.z;
				case 4: return a.x <= b.x && a.y <= b.y && a.z <= b.z;
				case 5: return a.x >= b.x && a.y >= b.y && a.z >= b.z;
				case 6: return a.x == b.x || a.y == b.y || a.z == b.z;
				case 7: return a.x < b.x || a.y < b.y || a.z < b.z;
				case 8: return a.x > b.x || a.y > b.y || a.z > b.z;
				case 9: return a.x <= b.x || a.y <= b.y || a.z <= b.z;
				case 10: return a.x >= b.x || a.y >= b.y || a.z >= b.z;
				default: return a.x == b.x || a.y == b.y || a.z == b.z;
			}
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
				EDown = Input.GetKeyDown(KeyCode.E),
				gravity = gravity
			};
			a.Dispose();
			JobHandle moveHandle = moveJob.Schedule(this, inputDeps);

			return moveHandle;
		}
	}
}
