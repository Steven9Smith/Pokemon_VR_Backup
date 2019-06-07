using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Pokemon
{
	[RequiresEntityConversion]
	[DisallowMultipleComponent]
	public class PlayerInputComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		public float3 Move;
		public float4 Rotation;
		//bytes replace bool with value of 1 or 0
		public bool SpaceDown;
		public bool LShiftDown;
		public bool RShiftDown;
		public bool Mouse1Down;
		public bool Mouse2Down;
		public bool LCtrlDown;
		public bool EDown;
		//max speed the player can acheive on their own using the player acceleration
		public float MaxInputVelocity;
		//acceleration the player is currently allowed to move
		public float PlayerCurrentAcceleration;
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
				MaxInputVelocity = MaxInputVelocity,
				Mouse1Down = Mouse1Down,
				Mouse2Down = Mouse2Down,
				RShiftDown = RShiftDown,
				Rotation = Rotation,
				PlayerCurrentAcceleration = PlayerCurrentAcceleration,
				EDown = EDown
			});
			dstManager.AddComponentData(entity, new StateData { });
		}
	}
	/// <summary>
	/// Player Input data (stores player input)
	/// </summary>
	[Serializable]
	public struct PlayerInput : IComponentData
	{
		public float3 Move;
		public float4 Rotation;
		//bytes replace bool with value of 1 or 0
		public BlittableBool SpaceDown;
		public BlittableBool LShiftDown;
		public BlittableBool RShiftDown;
		public BlittableBool Mouse1Down;
		public BlittableBool Mouse2Down;
		public BlittableBool LCtrlDown;
		public BlittableBool EDown;
		//max speed the player can acheive on their own using the player acceleration
		public float MaxInputVelocity;
		//acceleration the player is currently allowed to move
		public float PlayerCurrentAcceleration;
		//used for camera stuff
		public float MouseX, MouseY;
		public float smoothingSpeed;
		public float3 forward;
		public float3 right;
	}
}
