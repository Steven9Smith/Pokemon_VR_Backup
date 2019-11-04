using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

namespace Pokemon
{
	//not in use
	public struct EverythingTrigger : IComponentData { }
	public struct FloorTrigger : IComponentData { }
	public struct PlayerTrigger : IComponentData { }
	public struct PokemonTrigger : IComponentData { }
	public struct StructureTrigger : IComponentData { }
	public struct InteractionTrigger : IComponentData { }
	public struct NPCTrigger : IComponentData { }

	public class TriggerEventClass
	{
		public const uint Nothing = 0;
		public const uint Floor = 5;
		public const uint Player = 2;
		public const uint Pokemon = 4;
		public const uint Structure = 3;
		public const uint NPC = 6;
		public const uint Interaction = 80;
		public const uint Damage = 64;
		public const uint PokemonMove = 128;
		public const uint PokemonAttacking = 256;
		public const uint Collidable = 512;
		public const uint Everything = uint.MaxValue;

		/// <summary>
		/// converts the given collision filter category to a readable string
		/// </summary>
		/// <param name="physicsCategory">physics category</param>
		/// <returns>string</returns>
		public static string CollisionFilterValueToString(uint physicsCategory)
		{
			//nothing = ? can't seem to test it
			//everything = 4294967295
			switch (physicsCategory)
			{
				case Floor: return physicsCategory + ":floor";
				case Player: return physicsCategory + ":Player";
				case Pokemon: return physicsCategory + ":Pokemon";
				case Structure: return physicsCategory + ":Structure";
				case Interaction: return physicsCategory + ":Interaction";
				case NPC: return physicsCategory + ":NPC";
				case Damage: return physicsCategory + ":Damage";
				case PokemonMove: return physicsCategory + ":PokemonMove";
				case PokemonAttacking: return physicsCategory + ":PokemonAttacking";
				case uint.MaxValue: return physicsCategory + ":Everything";
				case Nothing: return physicsCategory + "Nothing";
				default: return physicsCategory + "~:unknown:" + physicsCategory;
			}
		}
		/// <summary>
		/// checks if the given NativeArray contains the given ATriggerEvent
		/// </summary>
		/// <param name="localAtes">Array to test</param>
		/// <param name="ate">ATriggerEvent</param>
		/// <returns></returns>
		public static bool NativeArrayContainsATriggerEvent(NativeArray<ATriggerEvent> localAtes, ATriggerEvent ate)
		{
			for (int i = 0; i < localAtes.Length; i++)
				if (localAtes[i] == ate) return true;
			return false;
		}

	}
	[Serializable]
	public struct ATriggerEvent : IComponentData{
		public Entity entityA;
		public Entity entityB;
		public BlittableBool valid;
		public uint triggerType;
		public override string ToString()
		{
			return "ATriggerEvent[EntityA = " + entityA.ToString() +
					",EntityB = " + entityB.ToString()+" triggerType = "+triggerType;
			}
		public static bool operator ==(ATriggerEvent c1, ATriggerEvent c2)
		{
			return ((c1.entityA == c2.entityA && c1.entityB == c2.entityB) || (c1.entityA == c2.entityB && c1.entityB == c2.entityA));
		}

		public static bool operator !=(ATriggerEvent c1, ATriggerEvent c2)
		{
			return ((c1.entityA != c2.entityA && c1.entityB != c2.entityB) || (c1.entityA != c2.entityB && c1.entityB != c2.entityA));
		}
		public override bool Equals(object obj)
		{
			if ((obj == null) || !this.GetType().Equals(obj.GetType())) return false; //Check for null and compare run-time types.
			else
			{
				return this == (ATriggerEvent)obj;
			}
		}
	}
	/*
	 
	 
	 */
	//not in use
	public struct EverythingCollision : IComponentData { }
	public struct FloorCollision : IComponentData { }
	public struct PlayerCollision : IComponentData { }
	public struct PokemonCollision : IComponentData { }
	public struct StructureCollision : IComponentData { }
	public struct InteractionCollision : IComponentData { }
	public struct NPCCollision : IComponentData { }

	public class CollisionEventClass
	{
		public const uint Nothing = 0;
		public const uint Floor = 5;
		public const uint Player = 2;
		public const uint Pokemon = 4;
		public const uint Structure = 3;
		public const uint NPC = 6;
		public const uint Interaction = 80;
		public const uint Damage = 64;
		public const uint PokemonMove = 128;
		public const uint PokemonAttacking = 256;
		public const uint Everything = uint.MaxValue;

		/// <summary>
		/// converts the given collision filter category to a readable string
		/// </summary>
		/// <param name="physicsCategory">physics category</param>
		/// <returns>string</returns>
		public static string CollisionFilterValueToString(uint physicsCategory)
		{
			//nothing = ? can't seem to test it
			//everything = 4294967295
			switch (physicsCategory)
			{
				case Floor: return physicsCategory + ":floor";
				case Player: return physicsCategory + ":Player";
				case Pokemon: return physicsCategory + ":Pokemon";
				case Structure: return physicsCategory + ":Structure";
				case Interaction: return physicsCategory + ":Interaction";
				case NPC: return physicsCategory + ":NPC";
				case Damage: return physicsCategory + ":Damage";
				case PokemonMove: return physicsCategory + ":PokemonMove";
				case PokemonAttacking: return physicsCategory + ":PokemonAttacking";
				case uint.MaxValue: return physicsCategory + ":Everything";
				case Nothing: return physicsCategory + "Nothing";
				default: return physicsCategory + "~:unknown:" + physicsCategory;
			}
		}
		/// <summary>
		/// checks if the given NativeArray contains the given ACollisionEvent
		/// </summary>
		/// <param name="localAtes">Array to test</param>
		/// <param name="ate">ACollisionEvent</param>
		/// <returns></returns>
		public static bool NativeArrayContainsACollisionEvent(NativeArray<ACollisionEvent> localAtes, ACollisionEvent ate)
		{
			for (int i = 0; i < localAtes.Length; i++)
				if (localAtes[i] == ate) return true;
			return false;
		}

	}
	[Serializable]
	public struct ACollisionEvent : IComponentData
	{
		public Entity entityA;
		public Entity entityB;
		public BlittableBool valid;
		public uint CollisionType;
		public override string ToString()
		{
			return "ACollisionEvent[EntityA = " + entityA.ToString() +
					",EntityB = " + entityB.ToString() + " CollisionType = " + CollisionType;
		}
		public static bool operator ==(ACollisionEvent c1, ACollisionEvent c2)
		{
			return ((c1.entityA == c2.entityA && c1.entityB == c2.entityB) || (c1.entityA == c2.entityB && c1.entityB == c2.entityA));
		}

		public static bool operator !=(ACollisionEvent c1, ACollisionEvent c2)
		{
			return ((c1.entityA != c2.entityA && c1.entityB != c2.entityB) || (c1.entityA != c2.entityB && c1.entityB != c2.entityA));
		}
		public override bool Equals(object obj)
		{
			if ((obj == null) || !this.GetType().Equals(obj.GetType())) return false; //Check for null and compare run-time types.
			else
			{
				return this == (ACollisionEvent)obj;
			}
		}
	}
}	

	
