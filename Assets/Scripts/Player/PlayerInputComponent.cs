using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using static CharacterControllerUtilities;

namespace Pokemon
{
	[RequiresEntityConversion]
	[DisallowMultipleComponent]
	public class PlayerInputComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		public float2 Move;
		public float4 Rotation;
		//bytes replace bool with value of 1 or 0
		public bool SpaceDown;
		public bool LShiftDown;
		public bool RShiftDown;
		public bool Mouse1Down;
		public bool Mouse2Down;
		public bool LCtrlDown;
		public bool attackADown;
		public bool attackBDown;
		public bool attackCDown;
		public bool attackDDown;
		//used for camera stuff
		public float MouseX, MouseY;
		public float smoothingSpeed;
		public float3 forward;
		public float3 right;
		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			dstManager.AddComponentData(entity, new PlayerInput
			{
				Move = Move,
				LCtrlDown = LCtrlDown,
				LShiftDown = LShiftDown,
				SpaceDown = SpaceDown,
				Mouse1Down = Mouse1Down,
				Mouse2Down = Mouse2Down,
				RShiftDown = RShiftDown,
				Rotation = Rotation,
				attackADown = attackADown,
				attackBDown = attackBDown,
				attackCDown = attackCDown,
				attackDDown = attackDDown
			});
		//	dstManager.AddComponentData(entity, new StateData { });
		}
	}
	/// <summary>
	/// Player Input data (stores player input)
	/// </summary>
	[Serializable]
	public struct PlayerInput : IComponentData
	{
		public float2 Move;
		public float4 Rotation;
		//bytes replace bool with value of 1 or 0
		public BlittableBool SpaceDown;
		public BlittableBool LShiftDown;
		public BlittableBool RShiftDown;
		public BlittableBool Mouse1Down;
		public BlittableBool Mouse2Down;
		public BlittableBool LCtrlDown;
		public BlittableBool attackADown;
		public BlittableBool attackBDown;
		public BlittableBool attackCDown;
		public BlittableBool attackDDown;
		//used for camera stuff
		public float MouseX, MouseY;
		public float3 forward;
		public float3 right;
		public int jumpCount;

		public PlayerInput(float2 move,quaternion rotation,bool spaceDown,bool lShiftDown,bool rShiftDown,
			bool mouse1Down,bool mouseDown2,bool lCtrlDown,bool attackaDown, bool attackbDown, bool attackcDown,
			bool attackdDown,int mjumpCount)
		{
			Move = move;
			jumpCount = mjumpCount;
			Rotation = rotation.value;
			SpaceDown = spaceDown;
			LShiftDown = lShiftDown;
			RShiftDown = rShiftDown;
			Mouse1Down = mouse1Down;
			Mouse2Down = mouseDown2;
			LCtrlDown = lCtrlDown;
			attackADown = attackaDown;
			attackBDown = attackbDown;
			attackCDown = attackbDown;
			attackDDown = attackdDown;
			MouseX = 0;
			MouseY = 0;
			forward = float3.zero;
			right = float3.zero;
		}
	}



	public struct CharacterControllerInput : IComponentData
	{
		public float2 Movement;
		public float2 Looking;
		public int Jumped;
	}

	public struct CharacterControllerInternalData : IComponentData
	{
		public float CurrentRotationAngle;
		public CharacterSupportState SupportedState;
		public float3 UnsupportedVelocity;
		public float3 LinearVelocity;
		public Entity Entity;
		public bool IsJumping;
		public CharacterControllerInput Input;
	}

	[Serializable]
	public struct CharacterControllerComponentData : IComponentData
	{
		public float3 Gravity;
		public float MovementSpeed;
		public float MaxMovementSpeed;
		public float RotationSpeed;
		public float JumpUpwardsSpeed;
		public float MaxSlope; // radians
		public int MaxIterations;
		public float CharacterMass;
		public float SkinWidth;
		public float ContactTolerance;
		public int AffectsPhysicsBodies;
	}
}
