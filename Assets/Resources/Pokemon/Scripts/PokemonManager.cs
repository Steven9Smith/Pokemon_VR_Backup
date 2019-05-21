using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Collections;
using System.Collections.Generic;
namespace Pokemon.SpawnSystem {
/*	public class PokemonManager : MonoBehaviour
	{

	}
    public class PokemonManagerA : MonoBehaviour
    {
        private static EntityManager manager;
        private static GameObject pokemonPrefab;
        //Pure Way
        private static EntityArchetype pokemonArchtype;
        private static RenderMesh pokemonRenderer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
		{
			manager = World.Active.GetOrCreateSystem<EntityManager>();
			pokemonArchtype = PokemonArchetypes.GenerateArchetype(manager,PokemonArchetypes.POKEMON_ENTITY_ARCHTYPE);
         
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void InitializeWithScene()
        {
			//    Debug.Log("scene loaded!");
			PokemonIO.createPokemonData();
			SpawnPokemon();
        }
        public static void SpawnPokemon()
        {
			//Generate new pokemon i'm missing
			PokemonIO.createPokemonData();
            Debug.Log("spawning Pokemon...");

            List<string> entityIds = PokemonIO.GetStoredEntitiesPath(1);
			if (entityIds.Count == 0) entityIds = CreateNewPokemonData();
			//PokemonEntity Array
            NativeArray<Entity> entities = new NativeArray<Entity>(entityIds.Count, Allocator.Temp);

            Entity pokemonEntity = manager.CreateEntity(pokemonArchtype);
            manager.Instantiate(pokemonEntity, entities);
            for (int i = 0; i < entityIds.Count; i++)
            {
				Entity entity = entities[i];
				PokemonIO.LoadEntity(manager, ref entity, entityIds[i], PokemonIO.TYPE_POKEMON);
            }
            entities.Dispose();
            PokemonIO.PrintString("SpawnPokemon","spawning done! spawned "+entityIds.Count);
        }
		public static List<string> CreateNewPokemonData()
		{
			PokemonIO.SavePokemonData("Bulbasaur");
			
			PokemonIO.PrintString("CreateNewPokemonData","Detectged no valid pokemon in this scenece, creating new pokemon assuming debug mode!",1);
			//create new pokenmon save Data
			PokemonEntity pokemonEntity = new PokemonEntity() {
				EntityId = PokemonIO.StringToByteString30("Test"),
				EntityName = PokemonIO.StringToByteString30("ATest"),
				PokemonEntityData = PokemonIO.CreateNewPokemonEntityData(0)
			};
			PokemonIO.SavePokemonEntity( pokemonEntity,new Translation(),new Rotation());
			return PokemonIO.GetStoredEntitiesPath(1);
		}
    }*/
}
