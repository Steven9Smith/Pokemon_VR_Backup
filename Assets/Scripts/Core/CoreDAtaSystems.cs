using Core;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

public class CoreDataSystems : JobComponentSystem
{
	EntityQuery query;

	protected override void OnCreate()
	{
		query = GetEntityQuery(typeof(CoreData));
	}


	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		// This adds a Scale Component to the ENtities with CoreData that donot have it
		Entities.WithNone<Scale>().ForEach((Entity entity, ref CoreData coreData) => {
			EntityManager.AddComponentData(entity, new Scale { Value = coreData.scale.x });
		}).WithoutBurst().WithStructuralChanges().WithName("ScaleVerificationSystem").Run();

		// This handles the CoreData Name Reqquest
		Entities.ForEach((Entity entity,ref CoreDataNameRequest cdnr,ref CoreData cd)=> {
			EntityManager.SetName(entity,
				CoreFunctionsClass.ByteString30ToString(cd.Name)+":"+CoreFunctionsClass.ByteString30ToString(cd.BaseName)
				);
			EntityManager.RemoveComponent<CoreDataNameRequest>(entity);
		}).WithStructuralChanges().WithoutBurst().WithName("CoreDataNameSettingSYstem").Run();


		return inputDeps;
	}
}
