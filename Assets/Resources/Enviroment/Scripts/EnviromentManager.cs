using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Pokemon.Player;
using System.Collections.Generic;

namespace Pokemon.JobSystem
{

   /* public class EnviromentManager : MonoBehaviour
    {
        private static EntityManager manager;
        private static EntityArchetype EnviromentArchetype;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            manager = World.Active.GetOrCreateSystem<EntityManager>();
			EnviromentArchetype = PokemonArchetypes.GenerateArchetype(manager, PokemonArchetypes.ENVIROMENT_ARCHTYPE);
            
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void InitializeWithScene()
        {
			CreateIEnviroments();
			LoadEnviroment(PokemonIO.GetCurrentSceneName());
        }

        public static void LoadEnviroment(string sceneName)
        {
			PokemonIO.PrintString("LoadEnviroment","spawning Enviroment Entities");

			List<string> entityIds = PokemonIO.GetStoredEntitiesPath(PokemonIO.TYPE_ENVIROMENT);
	//		if (entityIds.Count == 0) entityIds = Create
			//PokemonEntity Array
			NativeArray<Entity> entities = new NativeArray<Entity>(entityIds.Count, Allocator.Temp);
			Entity enviromentEntity = manager.CreateEntity(EnviromentArchetype);
			manager.Instantiate(enviromentEntity, entities);
			for (int i = 0; i < entityIds.Count; i++)
			{
				Entity entity = entities[i];
				PokemonIO.LoadEntity(manager, ref entity, entityIds[i], PokemonIO.TYPE_ENVIROMENT);
			}
			entities.Dispose();
			PokemonIO.PrintString("LoadEnviroment","spawning done! spawned "+entityIds.Count);
			//Generate new pokemon i'm missing
			PokemonIO.createPokemonData();
		}
		public static void CreateIEnviroments()
		{
			PokemonIO.CreateNewEnviromentData(0, PokemonIO.StringToByteString30("Test001"), PokemonIO.StringToByteString30("Test_Land"),
				new ByteString30(), new ByteString30() { A = 0, B = 0, C = 0},new Translation(),new Rotation(),new Scale() { Value = new float3(1f,1f,1f)});

		}
    }*/
}
