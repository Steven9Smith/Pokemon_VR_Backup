using Pokemon;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;

/// <summary>
/// made to set some varibles in the eneity that has an PokemonEntityData
/// </summary>
public class PokemonEntitySpawntSystem : ComponentSystem
{
	public EntityQuery PokemonEntitySpawnQuery;
	NativeArray<Entity> pokemonEntities;
	protected override void OnCreateManager()
	{
		PokemonEntitySpawnQuery = GetEntityQuery(typeof(PokemonEntitySpawnData));
	}
	protected override void OnUpdate()
	{
		//get entity array
		pokemonEntities = PokemonEntitySpawnQuery.ToEntityArray(Allocator.TempJob);
		for(int i = 0; i < pokemonEntities.Length; i++)
		{
			PhysicsCollider pc = EntityManager.GetComponentData<PhysicsCollider>(pokemonEntities[i]);
			PokemonEntityData ped = EntityManager.GetComponentData<PokemonEntityData>(pokemonEntities[i]);
			PhysicsMass pm = EntityManager.GetComponentData<PhysicsMass>(pokemonEntities[i]);
			float radius = calculateSphereRadius(pc.Value.Value.MassProperties.Volume);
	//		Debug.Log("Mass = " + ped.Mass + "InverseMAss = " + (1 / ped.Mass) + " inverseInertia ="+ (1/(0.4f*ped.Mass*(radius*radius))));
			EntityManager.SetComponentData<PhysicsMass>(pokemonEntities[i], new PhysicsMass {
				AngularExpansionFactor = pm.AngularExpansionFactor,
				Transform = pm.Transform,
				InverseMass = (1 / ped.Mass),
				InverseInertia = (1 / (0.4f * ped.Mass * (radius * radius)))
			});
			EntityManager.RemoveComponent(pokemonEntities[i],typeof(PokemonEntitySpawnData));
		}
		pokemonEntities.Dispose();
	}
	private float calculateSphereRadius(float volume)
	{
		return math.pow((volume / math.PI) * 0.75f, 1f / 3f);
	}
}