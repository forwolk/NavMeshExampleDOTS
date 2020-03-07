using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace DOTS.Systems
{
    /*
     * Copyright (C) Anton Trukhan, 2020.
    */
    [DisableAutoCreation]
    public class MoveToDestinationSystem : JobComponentSystem
    {

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var moveForwardRotationJob = new MoveToDestinationJob ();
            moveForwardRotationJob.DeltaTime = Time.DeltaTime;

            var jobHandle = moveForwardRotationJob.Schedule(this, inputDeps);

            return jobHandle;
        }
        
        [BurstCompile]
        struct MoveToDestinationJob : IJobForEachWithEntity<SpeedData, DestinationData, Translation>
        {

            public float DeltaTime;
            
            public void Execute(Entity entity, int index, [ReadOnly] ref SpeedData speedData, 
                [ReadOnly] ref DestinationData destinationData, ref Translation translation)
            {
                var currentDistance = math.distance(translation.Value.xz, destinationData.Destination.xz);
                if (currentDistance < 0.1f)
                {
                    return;
                }

                var distVector = destinationData.Destination.xz - translation.Value.xz;
                var distVectorNormalized = math.normalize(distVector);

                var vec2 = translation.Value.xz + distVectorNormalized * speedData.Speed * DeltaTime;
                translation.Value = new float3(vec2.x, translation.Value.y, vec2.y);
            }

        }
    }
}