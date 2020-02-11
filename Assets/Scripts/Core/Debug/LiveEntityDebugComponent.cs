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
	public PokemonStatSettings mPokmonStatSettings;     //Pokemon Stat  Settings
	public PokemonCalculationSettings mPokemonCalculationSettings;
	public void Update()
	{
		this.mPokmonStatSettings.Update();
		mPokemonCalculationSettings.Update();
	}
	public bool IsUpdated()
	{
		if (!this.mPokmonStatSettings.IsUpdated()) return false;
		else if (!mPokemonCalculationSettings.IsUpdated()) return false;
		return true;
	}
	public void ApplyChanges()
	{
		this.mPokmonStatSettings.ApplyChanges();
		mPokemonCalculationSettings.ApplyChanges();
	}
}
[Serializable]
public class PokemonCameraOffset
{
	public float3 offset;
	public void Update(PokemonCameraData pcd)
	{
		
	}
	public void ApplyChanges()
	{

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
	public bool DoNotChange = true;
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
		if (!DoNotChange)
		{
			PokemonDataClass.PokemonBaseData = PokemonBaseDataStats;
			PokemonDataClass.PokemonBaseEntityData = PokemonEntityDataStats;
			//		for(int i = 1; i < PokemonColliderDataStats.vColliders.Length; i++) 
			//			PokemonDataClass.PokemonBaseColliderData[i] = PokemonColliderDataStats.vColliders[i].Colliders;
		}
	}
}
[Serializable]
public class PokemonCalculationSettings
{
	public DebugPokemonSpeedStatDivider PokemonSpeedStatDivider;

	public void Update()
	{
		PokemonSpeedStatDivider.Update();
	}
	public bool IsUpdated()
	{
		return PokemonSpeedStatDivider.IsUpdated();
	}
	public void ApplyChanges()
	{
		PokemonSpeedStatDivider.ApplyChange();
	}
}
[Serializable]
public class DebugPokemonSpeedStatDivider
{
	public bool DoNotChange = true;
	public float PokemonSpeedStatDivider;
	public void Update()
	{
		PokemonSpeedStatDivider = PokemonDataClass.PokemonSpeedStatDivider;
	}
	public bool IsUpdated()
	{
		return PokemonSpeedStatDivider == PokemonDataClass.PokemonSpeedStatDivider;
	}
	public void ApplyChange()
	{
		Debug.Log("Chainging");
		if (!DoNotChange)
		{
			Debug.Log("setting to "+PokemonSpeedStatDivider);
			PokemonDataClass.PokemonSpeedStatDivider = PokemonSpeedStatDivider;
		}
		else Debug.Log("chagne failed");
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
	public DebugPhysicsDamping physicsDamping;          //PhysicsDamping component of the Entity (if it has one)
	public DebugCoreData coreData;						//CoreData component of the Entity (this game is being made with every entity having a CoreData component)
	public DebugPhysicsMass physicsMass;                //PhysicsMass of the Entity (cannot be edited due to its reliance on the PokemonEntityData) (if it has one)
	public DebugPokemonEntityData pokemonEntityData;    //PokemonEntityData of the Entity (if it has one)
	public PokemonCameraOffset pokemonCameraOffset; 
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
			if (!coreData.DoNotChange) coreData.Update(entityManager, entity, scale);

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
	public bool DoNotChange = true;
	public float3 Angular;
	public float3 Linear;
	private PhysicsVelocity pv;
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

[Serializable]
public class DebugPhysicsDamping
{
	public bool DoNotChange = true;
	public float Angular;
	public float Linear;
	private PhysicsDamping pd;
	public bool IsUpdated(EntityManager entityManager, Entity entity)
	{
		pd = entityManager.GetComponentData<PhysicsDamping>(entity);
		if (pd.Angular != Angular || pd.Linear != Linear) return false;
		return true;
	}
	public void Update(EntityManager entityManager, Entity entity)
	{
		pd = entityManager.GetComponentData<PhysicsDamping>(entity);
		Angular = pd.Angular;
		Linear = pd.Linear;
	}
	public void ApplyChanges(EntityManager entityManager, Entity entity)
	{
		entityManager.SetComponentData(entity, new PhysicsDamping
		{
			Angular = Angular,
			Linear = Linear
		});
	}
}
[Serializable]
public class DebugPhysicsMass
{
	private PhysicsMass pm;
	private PhysicsCollider pc;
	public bool IsKinematic = false;
	public bool IsDynamic = false;
	public bool DoNotChange = true;
	public float Mass;
	public void ApplyChange(EntityManager entityManager, Entity entity,float mass = 0)
	{
		pm = entityManager.GetComponentData<PhysicsMass>(entity);
		pc = entityManager.GetComponentData<PhysicsCollider>(entity);
		if (IsKinematic) PhysicsMass.CreateKinematic(pc.MassProperties);
		else if (IsDynamic) PhysicsMass.CreateDynamic(pc.MassProperties, mass > 0 ? mass : Mass);
	}
	public void Update(EntityManager entityManager, Entity entity)
	{
		pm = entityManager.GetComponentData<PhysicsMass>(entity);
		Mass = 1 / pm.InverseMass;
		Debug.Log(pm.AngularExpansionFactor + "." + pm.CenterOfMass + "," + pm.InertiaOrientation + "," + pm.InverseInertia + ",iv:" + pm.InverseMass + "," + pm.Transform);
	}
	public bool IsUpdated(EntityManager entityManager, Entity entity)
	{
		return pm.Equals(entityManager.GetComponentData<PhysicsMass>(entity));
	}
}
[Serializable]
public class DebugCoreData
{
	public bool DoNotChange = true;
	public string Name;
	public string BaseName;
	private string[] temp;
	public void ApplyChange(EntityManager entityManager, Entity entity, float scale)
	{
		temp = entityManager.GetName(entity).Split(':');
		CoreData cd = entityManager.GetComponentData<CoreData>(entity);
		entityManager.SetComponentData(entity, new CoreData
		{
			BaseName = temp.Length > 1 ? new ByteString30(temp[1]) : new ByteString30(),
			Name = new ByteString30(temp[0]),
			isValid = true,
			scale = scale,
			size = cd.size
		});
		entityManager.SetName(entity, Name + ":" + BaseName);

	}
	public void Update(EntityManager entityManager,Entity entity, float scale)
	{
		temp = entityManager.GetName(entity).Split(':');
		BaseName = temp.Length > 1 ? temp[1] : "";
		Name = temp[0];
	}
	public bool IsUpdated(EntityManager entityManager, Entity entity)
	{
		temp = entityManager.GetName(entity).Split(':');
		if (Name != temp[0]) return false;
		else if (temp.Length > 1 && BaseName != temp[1]) return false;
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