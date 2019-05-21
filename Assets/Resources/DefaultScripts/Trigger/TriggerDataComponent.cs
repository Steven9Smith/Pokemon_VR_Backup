using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

namespace Pokemon
{
	[RequiresEntityConversion]
	public class TriggerDataComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		public enum TriggerType
		{
			Default,
			Floor,
			Area
		}
		public TriggerType triggerType = TriggerType.Default;
		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			dstManager.AddComponentData(entity, new TriggerData
			{
				triggerType = getTriggerType(triggerType)
			});
		}
		private int getTriggerType(TriggerType triggerType)
		{
			switch (triggerType)
			{
				case TriggerType.Default: return 0;
				case TriggerType.Floor: return 1;
				case TriggerType.Area: return 2;
				default: return 0;
			}
		}

	}
	//not in use
	public struct EverythingTrigger : IComponentData{}
	public struct FloorTrigger : IComponentData { }
	public struct PlayerTrigger : IComponentData { }
	public struct PokemonTrigger : IComponentData { }
	public struct StructureTrigger : IComponentData { }
	public struct InteractionTrigger : IComponentData {
	}

	public struct PairOColliders : IComponentData
	{
		public PhysicsCollider entityACollider;
		public PhysicsCollider entityBCollider;
		public ATriggerEvent entityATriggerEvent;
		public BlittableBool aHasInteraction;
		public BlittableBool bHasInteraction;
	}
	public struct NPCTrigger : IComponentData { }


	public class TriggerEventClass
	{
		public const uint Floor = 1;
		public const uint Player = 2;
		public const uint Pokemon = 3;
		public const uint Structure = 4;
		public const uint NPC = 6;
		public const uint Interaction = 16;
		public const uint Everything = uint.MaxValue;
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
				case uint.MaxValue: return physicsCategory + ":Everything";
				default: return physicsCategory + "~:unknown";
			}
		}
		
	}

	[Serializable]
	public struct TriggerData : IComponentData
	{
		public int triggerType;
	}
	[Serializable]
	public struct ATriggerEvent : IComponentData{
		public Entity entityA;
		public Entity entityB;
		public BlittableBool valid;
		public uint triggerType;
		public override string ToString()
		{
			return "ATriggerEvent[EntityA = " + entityA.ToString() + ",EntityB = " + entityB.ToString() + "," + triggerType + "," + valid;
		}
		public static bool operator ==(ATriggerEvent c1, ATriggerEvent c2)
		{
			//since ATrigger can have entityA and entityB is either position we have to test for them both
			return (c1.entityA == c2.entityA && c1.entityB == c2.entityB) || (c1.entityA == c2.entityB && c1.entityB == c2.entityA);
		}

		public static bool operator !=(ATriggerEvent c1, ATriggerEvent c2)
		{
			return (c1.entityA != c2.entityA && c1.entityA != c2.entityB) || (c1.entityB != c2.entityB && c1.entityB == c2.entityA);
		}
	}
	public struct ATriggerEventData : ISharedComponentData
	{
		public NativeArray<ATriggerEvent> ATriggerEvents;
	}
	public struct ATriggerEventDataBut : IComponentData { }
	
}	

	
