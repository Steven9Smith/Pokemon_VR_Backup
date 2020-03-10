using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Pokemon
{
	/// <summary>
	/// StateData for entities that need it
	/// </summary>
	[Serializable]
	public struct StateData : IComponentData
	{
		//using bools for now but in the future this may change to char
		public BlittableBool isJumping;
		public BlittableBool isSliding;
		public BlittableBool isCreeping;
		public BlittableBool isWalking;
		public BlittableBool isCrouching;
		public BlittableBool isIdle;
		public BlittableBool isRunning;
		public BlittableBool isAttacking1;
		public BlittableBool isAttacking2;
		public BlittableBool isAttacking3;
		public BlittableBool isAttacking4;
		public BlittableBool isEmoting1;
		public BlittableBool isEmoting2;
		public BlittableBool isEmoting3;
		public BlittableBool isEmoting4;
		public BlittableBool supported;
		public StateData(bool _supported = false, bool sliding = false,bool jumping = false, bool creeping = false, bool crouching = false,
			bool walking = false, bool idle = false, bool running = false,
			bool attack1 = false, bool attack2 = false, bool attack3 = false, bool attack4 = false,
			bool emote1 = false, bool emote2 = false, bool emote3 = false, bool emote4 = false)
		{
			isJumping = jumping;
			supported = _supported;
			isCreeping = creeping;
			isWalking = walking;
			isCrouching = crouching;
			isSliding = sliding;
			isIdle = idle;
			isRunning = running;
			isAttacking1 = attack1;
			isAttacking2 = attack2;
			isAttacking3 = attack3;
			isAttacking4 = attack4;
			isEmoting1 = emote1;
			isEmoting2 = emote2;
			isEmoting3 = emote3;
			isEmoting4 = emote4;
		}
		public override string ToString()
		{
			return "isJumping: " + (bool)isJumping +
				"\nisCreeping: " + (bool)isCreeping +
				"\nisSliding: "+(bool)isSliding+
				"\nisWalking: " + (bool)isWalking +
				"\nisCrouching: " + (bool)isCrouching +
				"\nisIdle: " + (bool)isIdle +
				"\nisRunning: " + (bool)isRunning +
				"\nisAttacking1: " + (bool)isAttacking1 +
				"\nisAttacking2: " + (bool)isAttacking2 +
				"\nisAttacking3: " + (bool)isAttacking3 +
				"\nisAttacking4: " + (bool)isAttacking4 +
				"\nisEmoting1: " + (bool)isEmoting1 +
				"\nisEmoting2: " + (bool)isEmoting2 +
				"\nisEmoting3: " + (bool)isEmoting3 +
				"\nisEmoting4: " + (bool)isEmoting4 +
				"\nonGroud: " + (bool)supported;
		}
	}
	/// <summary>
	/// Attach this to all StateData Entities that need to be tracked by the collision syetem
	/// </summary>
	[Serializable]
	public struct StateDataTracking : IComponentData { }

	public class StateDataClass
	{
		public enum State
		{
			Idle = 0,
			Crouching = 1,
			Walking = 2,
			Creeping = 3,
			Running = 4,
			Attack1 = 5,
			Attack2 = 6,
			Attack3 = 7,
			Attack4 = 8,
			Emote1 = 9,
			Emote2 = 10,
			Emote3 = 11,
			Emote4 = 12,
			Jumping = 13
		}
		public static void SetState(ref StateData stateData, State desiredState, bool reset = true, bool supported = true)
		{
			if (reset) ResetStateData(ref stateData);
			switch (desiredState)
			{
				case State.Idle: stateData.isIdle = true; break;
				case State.Crouching: stateData.isCrouching = true; break;
				case State.Walking: stateData.isWalking = true; break;
				case State.Creeping: stateData.isCreeping = true; break;
				case State.Running: stateData.isRunning = true; break;
				case State.Attack1: stateData.isAttacking1 = true; break;
				case State.Attack2: stateData.isAttacking2 = true; break;
				case State.Attack3: stateData.isAttacking3 = true; break;
				case State.Attack4: stateData.isAttacking4 = true; break;
				case State.Emote1: stateData.isEmoting1 = true; break;
				case State.Emote2: stateData.isEmoting2 = true; break;
				case State.Emote3: stateData.isEmoting3 = true; break;
				case State.Emote4: stateData.isEmoting4 = true; break;
				case State.Jumping: stateData.isJumping = true; break;
				default: ResetStateData(ref stateData); stateData.isIdle = true; break;
			}
			if (supported) stateData.supported = true;
		}
		public static void ResetStateData(ref StateData stateData)
		{
			if (stateData.isAttacking1) stateData.isAttacking1 = false;
			if (stateData.isAttacking2) stateData.isAttacking2 = false;
			if (stateData.isAttacking3) stateData.isAttacking3 = false;
			if (stateData.isAttacking4) stateData.isAttacking4 = false;
			if (stateData.isEmoting1) stateData.isEmoting1 = false;
			if (stateData.isEmoting2) stateData.isEmoting2 = false;
			if (stateData.isEmoting3) stateData.isEmoting3 = false;
			if (stateData.isEmoting4) stateData.isEmoting4 = false;
			if (stateData.isCreeping) stateData.isCreeping = false;
			if (stateData.isIdle) stateData.isIdle = false;
			if (stateData.isCrouching) stateData.isCrouching = false;
			if (stateData.isWalking) stateData.isWalking = false;
			if (stateData.isJumping) stateData.isJumping = false;
			if (stateData.isRunning) stateData.isRunning = false;
		}
	}
}