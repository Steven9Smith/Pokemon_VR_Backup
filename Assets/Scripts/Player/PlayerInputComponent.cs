using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Pokemon
{
<<<<<<< Updated upstream
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
		public bool attackADown;
		public bool attackBDown;
		public bool attackCDown;
		public bool attackDDown;
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
				attackADown = attackADown,
				attackBDown = attackBDown,
				attackCDown = attackCDown,
				attackDDown = attackDDown
			});
		//	dstManager.AddComponentData(entity, new StateData { });
		}
	}
=======
>>>>>>> Stashed changes
	/// <summary>
	/// Player Input data (stores player input)
	/// </summary>
	[Serializable]
	public struct PlayerInput : IComponentData
	{
		public float3 Move;
		public float4 Rotation;
		//bytes replace bool with value of 1 or 0
<<<<<<< Updated upstream
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
		//max speed the player can acheive on their own using the player acceleration
		public float MaxInputVelocity;
		//acceleration the player is currently allowed to move
		public float PlayerCurrentAcceleration;
		//used for camera stuff
		public float MouseX, MouseY;
		public float smoothingSpeed;
		public float3 forward;
		public float3 right;
=======
		public BlittableBool ReqJump;
		public BlittableBool ReqRun;
		public BlittableBool ReqInteract;
		public BlittableBool ReqFocus;
		public BlittableBool ReqCrouch;
		public BlittableBool ReqProne;
		public BlittableBool ReqAttack;
		public BlittableBool ReqSwitchItemLeft;
		public BlittableBool ReqSwitchItemRight;
		public BlittableBool ReqSwitchAttackLeft;
		public BlittableBool ReqSwitchAttackRight;
		public BlittableBool ReqAttackA;
		public BlittableBool ReqAttackB;
		public BlittableBool ReqAttackC;
		public BlittableBool ReqAttackD;
		public BlittableBool ReqItemA;
		public BlittableBool ReqItemB;
		public BlittableBool ReqItemC;
		public BlittableBool ReqItemD;
		//used for camera stuff
		public float MouseX, MouseY;
		public int jumpCount;
	}

	[Serializable]
	public struct PlayerInputConfig
	{
		public string MoveXAxis;
		public string MoveYAxis;
		public string LookXAxis;
		public string LookYAxis;
		public KeyCode JumpKey;
		public KeyCode RunKey;
		public KeyCode InteractKey;
		public KeyCode AttackKey;
		public KeyCode FocusKey;
		public KeyCode CrouchKey;
		public KeyCode ProneKey;
		public KeyCode SwitchItemLeftKey;
		public KeyCode SwitchItemRightKey;
		public KeyCode SwitchAttackLeftKey;
		public KeyCode SwitchAttackRightKey;
		public KeyCode AttackAKey;
		public KeyCode AttackBKey;
		public KeyCode AttackCKey;
		public KeyCode AttackDKey;
		public KeyCode ItemAKey;
		public KeyCode ItemBKey;
		public KeyCode ItemCKey;
		public KeyCode ItemDKey;
		public bool SetKey(int keyReference,KeyCode key)
		{
			if(keyReference > 2)
			{
				switch (keyReference)
				{
					case 4: JumpKey = key; break;
					case 5: RunKey = key; break;
					case 6: InteractKey = key; break;
					case 7: CrouchKey = key; break;
					case 8: ProneKey = key; break;
					case 9: SwitchItemLeftKey = key; break;
					case 10: SwitchItemRightKey = key; break;
					case 11: SwitchAttackLeftKey = key; break;
					case 12: SwitchAttackRightKey = key; break;
					case 13: AttackAKey = key; break;
					case 14: AttackBKey = key; break;
					case 15: AttackCKey = key; break;
					case 16: AttackDKey = key; break;
					case 17: ItemAKey = key; break;
					case 18: ItemBKey = key; break;
					case 19: ItemCKey = key; break;
					case 20: ItemDKey = key; break;
					case 21: FocusKey = key; break;
					case 22: AttackKey = key; break;
				}
				return true;
			}
			else
			{
				Debug.LogWarning("Cannot set Axis using a KeyCode");
				return false;
			}
		}
		public override string ToString()
		{
			return "Jump: " + JumpKey+
				"Run: "+RunKey+
				"Interact: "+RunKey+
				"Crouch: "+RunKey+
				"Prone: "+RunKey
				
				;
		}
	}
	[Serializable]
	public struct PlayerInputConfigs
	{
		public PlayerInputConfig Player1;
		public PlayerInputConfig Player2;
		public PlayerInputConfig Player3;
		public PlayerInputConfig Player4;
>>>>>>> Stashed changes
	}
}
