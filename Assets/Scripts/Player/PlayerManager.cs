using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using System.Collections.Generic;
using Pokemon.Player;

namespace Pokemon.JobSystem
{


 /*   public class PlayerManager : MonoBehaviour
    {
        private static EntityManager manager;
        private static EntityArchetype playerArchetype;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            manager = World.Active.GetOrCreateSystem<EntityManager>();
			//create Player Archetype
			playerArchetype = PokemonArchetypes.GenerateArchetype(manager, PokemonArchetypes.PLAYER_ARCHTYPE);
           
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void InitializeWithScene()
        {
            SpawnPlayers();
        }
        public static void SpawnPlayers()
        {
            Debug.Log("loading players...");
			//get all players in game
			List<string> playerIds = PokemonIO.GetStoredEntitiesPath(PokemonIO.TYPE_PLAYER);
			if (playerIds.Count == 0)
			{
				PokemonIO.PrintString("SpawnPlayers", "Creating new Player Data", 1);
				PlayerData playerData = PokemonIO.CreateNewPlayerData("NewPlayer", PokemonIO.CreateNewPokemonEntityData(2));
				PokemonIO.SavePlayerData(playerData, new Translation()
				{
					Value = new float3(0f, 0f, 10f)
				}, new Rotation());
				playerIds.Add("NewPlayer");
			}
			else PokemonIO.PrintString("SpawnPlayers", "Loaded Player data successfully!", 1);
            NativeArray<Entity> entities = new NativeArray<Entity>(playerIds.Count, Allocator.Temp);
            Entity playerEntity = manager.CreateEntity(playerArchetype);
            manager.Instantiate(playerEntity, entities);
            Debug.Log("Initilized player Entityes...now to spawn them....");
            for (int i = 0; i < entities.Length; i++)
            {
				Entity entity = entities[i];
				PokemonIO.LoadEntity(manager, ref entity,playerIds[i],2);
			}
			entities.Dispose();
            PokemonIO.PrintString("SpawnPlayers","Players Loaded! loaded "+entities.Length);
        }
    }*/
}
