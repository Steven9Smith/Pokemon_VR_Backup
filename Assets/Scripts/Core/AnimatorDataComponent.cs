using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct AnimatorData : IComponentData
{
	public BlittableBool onGround;
	public BlittableBool TriggerJump;
	public BlittableBool isCreeping;//Walking is default
	public BlittableBool isRunning;
	public BlittableBool isAttacking1;
	public BlittableBool isAttacking2;
	public BlittableBool isAttacking3;
	public BlittableBool isAttacking4;
	public BlittableBool isEmoting1;
	public BlittableBool isEmoting2;
	public BlittableBool isEmoting3;
	public BlittableBool isEmoting4;
	public float3 velocity;
}