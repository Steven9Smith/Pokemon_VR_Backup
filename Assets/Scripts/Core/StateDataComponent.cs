using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Pokemon
{
	public struct StateData : IComponentData
	{
		//using bools for now but in the future this may change to char
		public BlittableBool isJumping;
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
		public BlittableBool onGround;
	}
	public class StateDataClass
	{
		public enum State{
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
		public static void SetState(ref StateData stateData, State desiredState, bool reset = true, bool onGround = true)
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
			if (onGround) stateData.onGround = true;
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