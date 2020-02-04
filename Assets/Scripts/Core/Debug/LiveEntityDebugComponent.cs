using Core;
using Pokemon;
using System;
using Unity.Mathematics;
using Unity.Physics;
using UnityEditor;
using UnityEngine;

public class LiveEntityDebugComponent : MonoBehaviour
{
	public bool active;
	public string EntityName;
	public EntityDebug Display;
	public EntityDebug Change;

/*	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		if (EditorApplication.isPlaying)
		{
			Repaint();
		}
	}*/ 
}


[Serializable]
public class EntityDebug
{
	public bool applyChange;
	public bool constantChange;
	public bool copyFromDisplay;
	public float3 position;
	public float scale;
	public DebugPhysicsVelocity physicsVelocity;
	public DebugPhysicsDamping physicsDamping;
	public DebugCoreData coreData;
	public DebugStateData stateData;
	public DebugPokemonEntityData pokemonEntityData;
}
[Serializable]
public class DebugPhysicsVelocity
{
	public float3 Angular;
	public float3 Linear;
}
[Serializable]
public class DebugPhysicsDamping
{
	public float Angular;
	public float Linear;
}
[Serializable]
public class DebugCoreData
{
	public string Name;
	public string BaseName;
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
	public ushort PokedexNumber;
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
	public char BodyType;
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