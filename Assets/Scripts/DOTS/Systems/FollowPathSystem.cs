using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace DOTS
{
    /*
     * Copyright (C) Anton Trukhan, 2020.
    */
    public class FollowPathSystem : JobComponentSystem
    {
        private const float STOPPING_RANGE = 0.5f;
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var lookup = GetBufferFromEntity<PathBufferElement>();
            var job = new SetPathJob { buffersLookup = lookup, StoppingDistance = STOPPING_RANGE };
            var jobHandle = job.Schedule(this, inputDeps);
            return jobHandle;
        }
    }

    [BurstCompile]
    struct SetPathJob : IJobForEachWithEntity<FollowPathData, DestinationData, Translation>
    {
        [ReadOnly]
        public BufferFromEntity<PathBufferElement> buffersLookup;

        public float StoppingDistance;

        public void Execute(Entity entity, int index, ref FollowPathData pathData,
            ref DestinationData destinationData, [ReadOnly] ref Translation translation)
        {
            if (pathData.PathStatus == PathStatus.EndOfPathReached)
            {
                return;
            }

            if (pathData.PathStatus == PathStatus.Calculated)
            {
                pathData.PathStatus = PathStatus.Following;
                pathData.PathIndex = 0;
            }

            if ( !buffersLookup.Exists(entity))
            {
                return;
            }
            
            var path = buffersLookup[entity];
            if (path.Length == 0)
            {
                return;
            }
            
            var pos = path[pathData.PathIndex].Value;
            var distance = math.distance(pos.xz, translation.Value.xz);
            var pointReached = distance < StoppingDistance;
            if (pointReached && pathData.PathIndex == path.Length - 1)
            {
                pathData.PathStatus = PathStatus.EndOfPathReached;
                return;
            }
            
            if (pointReached)
            {
                pathData.PathIndex++;
                pos = path[pathData.PathIndex].Value;
            }

            destinationData.Destination = pos;
        }
    }
}