using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;

public class MoveBulletSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Time.DeltaTime;
        var jobHandle = Entities
            .WithName("MoveBulletSystem")
            .ForEach((ref Translation position, ref Rotation rotation, ref BulletData bulletData) =>
            {
                position.Value += deltaTime * 100f * math.forward(rotation.Value);
            })
            .Schedule(inputDeps);

        jobHandle.Complete();

        Entities.WithoutBurst().WithStructuralChanges()
            .ForEach((Entity entity, ref Translation position,
                                        ref Rotation rotation,
                                        ref BulletData bulletData,
                                        ref LifetimeData lifetimeData) =>
            {
                float distanceToTarget = math.length(GameDataManager.instance.wps[bulletData.waypoint] - position.Value);
                if (distanceToTarget < 27)
                {
                    lifetimeData.lifeLeft = 0;

                    if (UnityEngine.Random.Range(0, 1000) <= 50)
                    {
                        var instance = EntityManager.Instantiate(bulletData.explosionPrefab);
                        EntityManager.SetComponentData(instance, new Translation { Value = position.Value });
                        EntityManager.SetComponentData(instance, new Rotation { Value = rotation.Value });
                        EntityManager.SetComponentData(instance, new LifetimeData { lifeLeft = 0.5f });
                    }
                }

            })
            .Run();


        return jobHandle;
    }
}
