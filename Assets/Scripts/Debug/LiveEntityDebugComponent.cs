using System;
using Unity.Mathematics;
using UnityEngine;
using Pokemon;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Core;
using Unity.Collections;
using Core.Spawning;
using Core.GameConfig;

/// <summary>
/// This is the Componet that will appear in the editor in the inspector
/// </summary>
public class LiveEntityDebugComponent : MonoBehaviour
{
	public bool active;						//enable//disable the component
	public string EntityName;				//Name of the specific entity you would like to see properties of
	public EntityDebug Display;				//This is where the Entity's values will be displayed
	public GlobalSettings GlobalSettings;	//This is where global game setts are to be displayed
}
/// <summary>
/// These are the global game settings
/// </summary>
[Serializable]
public class GlobalSettings
{
	public bool applyChange;							//whether or not to apply the change to Global Settings when applyChange is clicked
	public bool editMode;								//enable edit mode for Global Settings
	public PokemonStatSettings mPokmonStatSettings = new PokemonStatSettings();     //Pokemon Stat  Settings
	public GameConfigDebug mGameConfig = new GameConfigDebug();
	public void Update()
	{
		this.mPokmonStatSettings.Update();
		this.mGameConfig.Update();
	}
	public bool IsUpdated()
	{
		if (!this.mPokmonStatSettings.IsUpdated()) return false;
		else if (!this.mGameConfig.IsUpdated()) return false;
		return true;
	}
	public void ApplyChanges()
	{
		this.mPokmonStatSettings.ApplyChanges();
		this.mGameConfig.ApplyChanges();
	}
}
[Serializable]
public class PokemonColliderArrayHolder
{
	public CompoundCollider.ColliderBlobInstance[] Colliders;
}
[Serializable]
public class PokemonColliderArray
{
	public PokemonColliderArrayHolder[] vColliders = null;
	public void Update(CompoundCollider.ColliderBlobInstance[][] _colliders)
	{
		if (vColliders == null || vColliders.Length == 0)
		{
			vColliders = new PokemonColliderArrayHolder[_colliders.Length];
			for (int i = 0; i < _colliders.Length; i++)
				vColliders[i] = new PokemonColliderArrayHolder();
		}
		for (int i = 1; i < _colliders.Length; i++)
		{
			if (_colliders[i] != null)
			{
				vColliders[i] = new PokemonColliderArrayHolder();
				vColliders[i].Colliders = new CompoundCollider.ColliderBlobInstance[_colliders[i].Length];
				vColliders[i].Colliders = _colliders[i];
			}
		}
	}
	public bool IsUpdated(CompoundCollider.ColliderBlobInstance[][] _colliders)
	{
		if (vColliders == null){ vColliders = new PokemonColliderArrayHolder[_colliders.Length]; return false; }
		for (int i = 1; i < _colliders.Length; i++)
			if (vColliders[i].Colliders != _colliders[i]) return false;
		return true;
	}
}
/// <summary>
///	Global Pokemon Settings 
/// </summary>
[Serializable]
public class PokemonStatSettings
{
	public PokemonData[] PokemonBaseDataStats;				//Pokemon Base Stats (these are the base stats new pokemon are given when spawned)
	public PokemonEntityData[] PokemonEntityDataStats;		//Pokemon Entity Data Stats these are the base entity data stats given when a new pokemon is spawned
//	public PokemonColliderArray PokemonColliderDataStats;
	public void Update()
	{
		PokemonBaseDataStats = PokemonDataClass.PokemonBaseData;
		PokemonEntityDataStats = PokemonDataClass.PokemonBaseEntityData;
//		PokemonColliderDataStats.Update(PokemonDataClass.PokemonBaseColliderData);
	}
	public bool IsUpdated()
	{
		if (PokemonDataClass.PokemonBaseData.Length == 0)
		{
			Debug.LogWarning("Pokemon Base Data is Invalid, cannot update it");
			PokemonBaseDataStats = PokemonDataClass.GenerateBasePokemonDatas();
			PokemonDataClass.PokemonBaseData = PokemonBaseDataStats;
			return false;
		}
		if (PokemonDataClass.PokemonBaseEntityData.Length == 0)
		{
			Debug.LogWarning("Pokemon Base Entity Data is Invalid, cannot update it");
			PokemonEntityDataStats = PokemonDataClass.GenerateBasePokemonEntityDatas();
			PokemonDataClass.PokemonBaseEntityData = PokemonEntityDataStats;
			return false;
		}
		if (PokemonDataClass.PokemonBaseColliderData.Length == 0)
		{
			Debug.LogWarning("Pokemon Base Collider Data is Invalid, cannot update it");
			PokemonDataClass.PokemonBaseColliderData = PokemonDataClass.GenerateBasePokemonColliderData();
			return false;
		}
		else if (PokemonBaseDataStats != PokemonDataClass.PokemonBaseData || PokemonEntityDataStats != PokemonDataClass.PokemonBaseEntityData 
			/*!PokemonColliderDataStats.IsUpdated(PokemonDataClass.PokemonBaseColliderData)*/) return false;
		return true;
	}
	public void ApplyChanges()
	{
		PokemonDataClass.PokemonBaseData = PokemonBaseDataStats;
		PokemonDataClass.PokemonBaseEntityData = PokemonEntityDataStats;
//		for(int i = 1; i < PokemonColliderDataStats.vColliders.Length; i++) 
//			PokemonDataClass.PokemonBaseColliderData[i] = PokemonColliderDataStats.vColliders[i].Colliders;
	}
}

[Serializable]
public class GameConfigDebug
{
	public GameConfig gameConfig;
	public bool saveConfig;
	public void Update()
	{
		gameConfig = GameConfigClass.mGameConfig;
	}
	public bool IsUpdated()
	{
		if (saveConfig)
		{
			SaveConfig();
			saveConfig = false;
		}
		return GameConfigClass.isValid();
	}
	public void ApplyChanges()
	{
		GameConfigClass.mGameConfig = gameConfig;
	}
	public void SaveConfig()
	{
		GameConfigClass.SaveGameConfig(gameConfig);
	}
}
[Serializable]
public class EntityDebug
{
	public bool applyChange;							//click to apply any changes
	public bool constantChange;							//click this to have the changes be constant

	public bool EditMode;								//click this to enable edit mode

	public bool DoNotChangePosition = true;				//click this to keep the postiion the same (this gets overwritten if the scale is changed)
	public float3 position;								//position of the entity
	public bool DoNotChangeScale = true;				//click this to not change the scale when applyChanges is clicked
	public float scale;									//scale of the entity
	public DebugPhysicsVelocity physicsVelocity;		//PhysicsVelocity components of the Entity	(if it has one)
	public DebugPhysicsDamping physicsDamping = new DebugPhysicsDamping() { enable = false };          //PhysicsDamping component of the Entity (if it has one)
	public DebugCoreData coreData;						//CoreData component of the Entity (this game is being made with every entity having a CoreData component)
	public DebugPhysicsMass physicsMass;                //PhysicsMass of the Entity (cannot be edited due to its reliance on the PokemonEntityData) (if it has one)
	public DebugPokemonEntityData pokemonEntityData;    //PokemonEntityData of the Entity (if it has one)

	private string[] temp;
	public void Update(EntityManager entityManager, Entity entity)
	{
		if (!EditMode) {
			position = entityManager.GetComponentData<Translation>(entity).Value;
			scale = entityManager.GetComponentData<Scale>(entity).Value;
			temp = entityManager.GetName(entity).Split(':');
		//	coreData.Name = temp[0];
		//	coreData.BaseName = temp.Length > 1 ? temp[1] : "";
			if (!coreData.IsUpdated(entityManager, entity))
				coreData.Update(entityManager, entity, scale);
			if (entityManager.HasComponent<PhysicsVelocity>(entity) && !physicsVelocity.IsUpdated(entityManager,entity))
				physicsVelocity.Update(entityManager,entity);
			if (entityManager.HasComponent<PhysicsDamping>(entity) && !physicsDamping.IsUpdated(entityManager,entity))
				physicsDamping.Update(entityManager,entity);
			if (entityManager.HasComponent<PhysicsMass>(entity) && !physicsMass.IsUpdated(entityManager, entity))
				physicsMass.Update(entityManager, entity);
			if (entityManager.HasComponent<PokemonEntityData>(entity) && !pokemonEntityData.IsUpdated(entityManager, entity))
				pokemonEntityData.Update(entityManager, entity);
		}
		if (applyChange)
		{
			if(!DoNotChangePosition) entityManager.SetComponentData(entity, new Translation { Value = position });
			if (!DoNotChangeScale)
			{
				entityManager.SetComponentData(entity, new Scale { Value = scale });//got to scale of physics collider as well
				coreData.Update(entityManager, entity, scale);
				PhysicsCollider pc = entityManager.GetComponentData<PhysicsCollider>(entity);
				int groupIndex = 1;
				if (entityManager.HasComponent<GroupIndexInfo>(entity)) groupIndex = entityManager.GetComponentData<GroupIndexInfo>(entity).CurrentGroupIndex;
				entityManager.SetComponentData(entity, PokemonDataClass.getPokemonPhysicsCollider(coreData.BaseName,pokemonEntityData.ped,pc.Value.Value.Filter,scale, PokemonDataClass.GetPokemonColliderMaterial(PokemonDataClass.StringToPokedexEntry(coreData.BaseName)),groupIndex));
				entityManager.SetComponentData(entity, new Translation
				{
					Value = entityManager.GetComponentData<Translation>(entity).Value + ((pokemonEntityData.ped.Height * scale) / 2)
				});
			}
			if (!coreData.DoNotChange) coreData.ApplyChange(entityManager, entity, scale);
			else Debug.LogWarning("Not Chaging CoreData..");

			if(!physicsVelocity.DoNotChange)physicsVelocity.ApplyChanges(entityManager,entity);
			if(!physicsDamping.DoNotChange)physicsDamping.ApplyChanges(entityManager,entity);
			if (!pokemonEntityData.DoNotChange) pokemonEntityData.ApplyChange(entityManager, entity);
			if (!physicsMass.DoNotChange) physicsMass.ApplyChange(entityManager, entity,pokemonEntityData.ped.Mass);
			

			if (!constantChange)
			{
				applyChange = false;
				EditMode = false;
			}
		}
	}
}
[Serializable]
public class DebugPhysicsVelocity
{
	public bool firstCheck = false;
	public bool enable = true;
	public bool hasComponent = false;
	public bool DoNotChange = true;
	public float3 Angular;
	public float3 Linear;
	private PhysicsVelocity pv;
	public bool HasComponent(EntityManager entityManager, Entity entity)
	{
		hasComponent = entityManager.HasComponent<PhysicsDamping>(entity);
		enable = hasComponent ? true : false;
		return hasComponent;
	}
	public bool IsUpdated(EntityManager entityManager,Entity entity)
	{
		pv = entityManager.GetComponentData<PhysicsVelocity>(entity);
		if (!pv.Angular.Equals(Angular) || !pv.Linear.Equals(Linear)) return false;
		return true;
	}
	public void Update(EntityManager entityManager, Entity entity)
	{
		pv = entityManager.GetComponentData<PhysicsVelocity>(entity);
		Angular = pv.Angular;
		Linear = pv.Linear;
	}
	public void ApplyChanges(EntityManager entityManager,Entity entity)
	{
		entityManager.SetComponentData(entity, new PhysicsVelocity
		{
			Angular = Angular,
			Linear = Linear
		});
	}
}
/// <summary>
/// the class that represents an Entity's Physics Damping.
/// Note: the new EntityInputSystem uses the CoreData component to update position, rotation, and PhysicsDamping
/// Future Update Note: add a bool that represents if the entity has core data with physics damping (it will in this project)
/// </summary>
[Serializable]
public class DebugPhysicsDamping
{
	[ConditionalHide("enable")]
	public bool DoNotChange = true;
	[ConditionalHide("enable")]
	public float Angular;
	[ConditionalHide("enable")]
	public float Linear;
	private PhysicsDamping pd;
	public bool enable = true;
	private bool hasComponent = false;
	private bool firstCheck = false;
	public DebugPhysicsDamping() { }
	public DebugPhysicsDamping(PhysicsDamping _pd)
	{
		pd = _pd;
	}
	public bool HasComponent(EntityManager entityManager, Entity entity)
	{
		hasComponent = entityManager.HasComponent<PhysicsDamping>(entity);
		enable = hasComponent ? true : false;
		return hasComponent;
	}
	public bool IsUpdated(EntityManager entityManager, Entity entity)
	{
		if (!firstCheck)
		{
			HasComponent(entityManager, entity);
			firstCheck = true;
		}
		if (hasComponent)
		{
			pd = entityManager.GetComponentData<PhysicsDamping>(entity);
			if (pd.Angular != Angular || pd.Linear != Linear) return false;
			return true;
		}
		//to prevent to many calls to entities will just return true if the entity does not have the component
		else return true;
	}
	public void Update(EntityManager entityManager, Entity entity)
	{
		if (hasComponent)
		{
			pd = entityManager.GetComponentData<PhysicsDamping>(entity);
			Angular = pd.Angular;
			Linear = pd.Linear;
		}
	}
	public void ApplyChanges(EntityManager entityManager, Entity entity)
	{
		HasComponent(entityManager, entity);
		/* new system update cause the physicsDamping to be loaded from the coreData
		if(hasComponent)
			entityManager.SetComponentData(entity, new PhysicsDamping
			{
				Angular = Angular,
				Linear = Linear
			});
		}*/
	}
}
[Serializable]
public class DebugPhysicsMass
{
	public bool enable = true;
	private bool hasComponent = false;
	private bool firstCheck = false;
	private PhysicsMass pm;
	private PhysicsCollider pc;
	[ConditionalHide("enable")]
	public bool IsKinematic = false;
	[ConditionalHide("enable")]
	public bool IsDynamic = false;
	[ConditionalHide("enable")]
	public bool DoNotChange = true;
	[ConditionalHide("enable")]
	public float Mass;
	public bool HasComponent(EntityManager entityManager,Entity entity)
	{
		hasComponent = entityManager.HasComponent<PhysicsMass>(entity);
		enable = hasComponent ? true : false;
		return hasComponent;
	}
	public void ApplyChange(EntityManager entityManager, Entity entity,float mass = 0)
	{
		HasComponent(entityManager, entity);
		if (hasComponent)
		{
			pm = entityManager.GetComponentData<PhysicsMass>(entity);
			pc = entityManager.GetComponentData<PhysicsCollider>(entity);
			if (IsKinematic) PhysicsMass.CreateKinematic(pc.MassProperties);
			else if (IsDynamic) PhysicsMass.CreateDynamic(pc.MassProperties, mass > 0 ? mass : Mass);
		}
	}
	public void Update(EntityManager entityManager, Entity entity)
	{
		if (hasComponent)
		{
			pm = entityManager.GetComponentData<PhysicsMass>(entity);
			Mass = 1 / pm.InverseMass;
			Debug.Log(pm.AngularExpansionFactor + "." + pm.CenterOfMass + "," + pm.InertiaOrientation + "," + pm.InverseInertia + ",iv:" + pm.InverseMass + "," + pm.Transform);
		}
	}
	public bool IsUpdated(EntityManager entityManager, Entity entity)
	{
		if (!firstCheck)
		{
			HasComponent(entityManager, entity);
			firstCheck = true;
		}
		if (hasComponent)
			return pm.Equals(entityManager.GetComponentData<PhysicsMass>(entity));
		//to prevent to many calls to entities will just return true if the entity does not have the component
		else return true;
	}
}
[Serializable]
public class DebugCoreData
{
	[ConditionalHide("enable")]
	public bool DoNotChange = true;
	[ConditionalHide("enable")]
	public string Name;
	[ConditionalHide("enable")]
	public string BaseName;
	[ConditionalHide("enable")]
	public float3 position;
	[ConditionalHide("enable")]
	public quaternion rotation;
	[ConditionalHide("enable")]
	public DebugPhysicsDamping physicsDamping;
	private string[] temp;
	private bool hasComponent = false;
	public bool enable = true;
	private bool firstCheck = false;
	public bool HasComponent(EntityManager entityManager, Entity entity)
	{
		hasComponent = entityManager.HasComponent<CoreData>(entity);
		enable = hasComponent ? true : false;
		return hasComponent;
	}
	public void ApplyChange(EntityManager entityManager, Entity entity, float scale)
	{
		HasComponent(entityManager,entity);
		if (hasComponent)
		{
			Debug.Log("Changing the core");
			temp = entityManager.GetName(entity).Split(':');
			CoreData cd = entityManager.GetComponentData<CoreData>(entity);
			entityManager.SetComponentData(entity, new CoreData
			{
				BaseName = temp.Length > 1 ? new ByteString30(temp[1]) : new ByteString30(),
				Name = new ByteString30(temp[0]),
				isValid = true,
				scale = scale,
				size = cd.size,
				damping = new PhysicsDamping
				{
					Angular = physicsDamping.Angular,
					Linear = physicsDamping.Linear
				},
				entity = entity,
				rotation = new Rotation { Value = rotation },
				translation = new Translation { Value = position }
			});
			entityManager.SetName(entity, Name + ":" + BaseName);
		}
		else Debug.LogWarning("Failed to Apply change: entity does not have the specified component!");
	}
	public void Update(EntityManager entityManager,Entity entity, float scale)
	{
		if (hasComponent)
		{
			Debug.Log("detected component, updating...");
			CoreData cd = entityManager.GetComponentData<CoreData>(entity);
			Name = CoreFunctionsClass.ByteString30ToString(cd.Name);
			BaseName = CoreFunctionsClass.ByteString30ToString(cd.BaseName);
			position = cd.translation.Value;
			rotation = cd.rotation.Value;
			physicsDamping = new DebugPhysicsDamping(cd.damping);
		}
		else Debug.LogWarning("this eneity does not have CoreData");
	}
	public bool IsUpdated(EntityManager entityManager, Entity entity)
	{
		if (!firstCheck)
		{
			HasComponent(entityManager, entity);
			firstCheck = true;
		}
		if (hasComponent)
		{
			//let's keep this void as is for now (may not need rigorous testing)
			temp = entityManager.GetName(entity).Split(':');
			if (Name != temp[0]) return false;
			else if (temp.Length > 1 && BaseName != temp[1]) return false;
			return true;
		}

		else Debug.LogWarning("this eneity does not have CoreData");
		//to prevent to many calls to entities will just return true if the entity does not have the component
		return true;
	}

}
[Serializable]
public class DebugStateData
{
	public bool AddToExisting;
	public bool isJumping;
	public bool isCreeping;
	public bool isWalking;
	public bool isCrouching;
	public bool isIdle;
	public bool isRunning;
	public bool isAttacking1;
	public bool isAttacking2;
	public bool isAttacking3;
	public bool isAttacking4;
	public bool isEmoting1;
	public bool isEmoting2;
	public bool isEmoting3;
	public bool isEmoting4;
	public bool onGround;
}
[Serializable]
public class DebugPokemonEntityData
{
	/*public ushort PokedexNumber;
	public float Height;
	public ushort experienceYield;
	public ushort LevelingRate;
	public ushort Freindship;
	public float Speed;
	public float Acceleration;																    
	public float Hp;
	public ushort Attack;
	public ushort Defense;
	public ushort SpecialAttack;
	public ushort SpecialDefense;
	public float currentStamina;
	public float maxStamina;
	public float currentHp;
	public float Mass;
	public float jumpHeight;
	public int currentLevel;
	public DebugPokemonMoveSet pokemonMoveSet;
	public int guiId;
	public char BodyType;*/

	public bool DoNotChange = true;
	public PokemonEntityData ped;
	private PokemonEntityData mped;

	public void Update(EntityManager entityManager,Entity entity)
	{
		ped = entityManager.GetComponentData<PokemonEntityData>(entity);
	}
	public void ApplyChange(EntityManager entityManager,Entity entity)
	{
		entityManager.SetComponentData(entity, ped);
	}
	public bool IsUpdated(EntityManager entityManager, Entity entity)
	{
		mped = entityManager.GetComponentData<PokemonEntityData>(entity);
		return ped.Equals(mped);
	}
}
[Serializable]
public struct DebugPokemonMoveSet
{
	public DebugPokemonMove pokemonMoveA;
	public DebugPokemonMove pokemonMoveB;
	public DebugPokemonMove pokemonMoveC;
	public DebugPokemonMove pokemonMoveD;
}
[Serializable]
public struct DebugPokemonMove
{
	public string name;
	public bool isValid;
	public int index; //used for spawning moves
}
