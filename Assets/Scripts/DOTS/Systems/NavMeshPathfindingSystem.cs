using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Experimental.AI;

namespace DOTS
{
    /*
     * Copyright (C) Anton Trukhan, 2020.
    */
    [DisableAutoCreation]
    public class NavMeshPathfindingSystem : JobComponentSystem
    {
        private const int MAXIMUM_POOL_SIZE = 50;
        private EntityQuery requests;
        private EntityCommandBufferSystem commandBufferSystem;
        private Dictionary<Entity, NavMeshQuery> navMeshQueries;
        
        protected override void OnCreate()
        {
            navMeshQueries = new Dictionary<Entity, NavMeshQuery>();
            requests = GetEntityQuery(ComponentType.ReadOnly<NavMeshPathfindingRequestData>());
            commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var lookup = GetBufferFromEntity<PathBufferElement>();
            var commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent();
            var world = NavMeshWorld.GetDefaultWorld();
            
            var pathfindingDatas = requests.ToComponentDataArray<NavMeshPathfindingRequestData>(Allocator.TempJob);
            var entities = requests.ToEntityArray(Allocator.TempJob);

            var jobs = new NativeArray<JobHandle>(pathfindingDatas.Length, Allocator.TempJob);
            
            JobHandle jobHandle = inputDeps;

            for ( var i = 0; i < entities.Length; ++i)
            {
                var entity = entities[i];
                
                //Destroy finished requests
                if (pathfindingDatas[i].Status == PathSearchStatus.Finished)
                {
                    navMeshQueries[entity].Dispose();
                    navMeshQueries.Remove(entity);
                    EntityManager.DestroyEntity(entity);
                    continue;
                }
                
                //Process requests in progress
                if (!navMeshQueries.TryGetValue( entity, out var query))
                {
                    query = navMeshQueries[ entity ] =
                        new NavMeshQuery(world, Allocator.Persistent, MAXIMUM_POOL_SIZE);
                }
                var navMeshPathfindingJob = new NavMeshPathfindingJob
                {
                    BuffersLookup = lookup,
                    CommandBuffer = commandBuffer,
                    JobIndex = i,
                    Request = pathfindingDatas[i],
                    Query = query,
                    MaximumPoolSize = MAXIMUM_POOL_SIZE,
                    EntityRequestId = entity
                };

                jobs[i] = navMeshPathfindingJob.Schedule(inputDeps);
            }
            
            jobHandle = JobHandle.CombineDependencies(jobs);

            pathfindingDatas.Dispose();
            entities.Dispose();
            
            jobs.Dispose();
           
            commandBufferSystem.AddJobHandleForProducer(jobHandle);
            return jobHandle;
        }
        
        [BurstCompile]
        struct NavMeshPathfindingJob : IJob
        {
            [NativeDisableParallelForRestriction]
            public BufferFromEntity<PathBufferElement> BuffersLookup;
            
            [ReadOnly]
            public int JobIndex;

            [ReadOnly] 
            public int MaximumPoolSize;

            [ReadOnly] 
            public Entity EntityRequestId;

            public EntityCommandBuffer.Concurrent CommandBuffer;
            
            public NavMeshQuery Query;
            
            public NavMeshPathfindingRequestData Request;

            public void Execute()
            {
                if (Request.Status == PathSearchStatus.Requested )
                {
                    StartPathSearch( JobIndex, EntityRequestId, Query, CommandBuffer, Request );
                }
                else if (Request.Status == PathSearchStatus.Started)
                {
                    var pathfindingStatus = Query.UpdateFindPath(10, out _);
                    if (pathfindingStatus == PathQueryStatus.Success)
                    {
                        Query.EndFindPath(out int pathSize);
                        var pathBuffer = BuffersLookup[Request.Agent];
                        
                        //Path is straight and has no obstacles
                        if (pathSize == 1)
                        {
                            pathBuffer.Add(new PathBufferElement { Value = Request.Destination } );
                            CommandBuffer.DestroyEntity(JobIndex, EntityRequestId);
                            return;
                        }

                        //Path is complex and needs to be properly extracted
                        CompletePathSearch(JobIndex, EntityRequestId, Query, CommandBuffer, pathSize, 
                            MaximumPoolSize, Request, pathBuffer);
                    }
                }
            }

            private static void StartPathSearch(int jobIndex, Entity entityRequest, NavMeshQuery query, 
                EntityCommandBuffer.Concurrent commandBuffer, NavMeshPathfindingRequestData request)
            {
                var from = query.MapLocation (request.Start, request.Extents, request.AgentTypeId);
                var to = query.MapLocation (request.Destination, request.Extents, request.AgentTypeId);
                query.BeginFindPath(from, to);
                request.Status = PathSearchStatus.Started;
                commandBuffer.SetComponent(jobIndex, entityRequest, request);
            }

            private static void CompletePathSearch(int jobIndex, Entity entityRequest, NavMeshQuery query, 
                EntityCommandBuffer.Concurrent commandBuffer, int pathSize, int maximumPoolSize, 
                NavMeshPathfindingRequestData request, DynamicBuffer<PathBufferElement> agentPathBuffer)
            {
                var resultPath = new NativeArray<PolygonId>(pathSize, Allocator.Temp);
                query.GetPathResult(resultPath);

                //Extract path from PolygonId list
                var straightPathCount = 0;
                var straightPath = ExtractPath(query, 
                    request.Start,  request.Destination, 
                    resultPath, maximumPoolSize, ref straightPathCount);

                //Put the result path into buffer
                for (int i = 0; i < straightPathCount; i++)
                {
                    agentPathBuffer.Add(new PathBufferElement { Value = straightPath[i].position } );
                }

                straightPath.Dispose();
                resultPath.Dispose();
                
                request.Status = PathSearchStatus.Finished;
                commandBuffer.SetComponent(jobIndex, entityRequest, request);
            }

            private static NativeArray<NavMeshLocation> ExtractPath(NavMeshQuery query, 
                Vector3 startPosition, Vector3 endPosition,
                NativeArray<PolygonId> calculatedPath, int maxPathLength, ref int straightPathCount)
            {
                var pathLength = calculatedPath.Length;
                var straightPath = new NativeArray<NavMeshLocation>(pathLength, Allocator.Temp);
                var straightPathFlags = new NativeArray<StraightPathFlags>(pathLength, Allocator.Temp);
                var vertexSide = new NativeArray<float>(pathLength, Allocator.Temp);
                
                var pathStatus = PathUtils.FindStraightPath(query, startPosition, endPosition, calculatedPath,
                    pathLength, ref straightPath, ref straightPathFlags, ref vertexSide,
                    ref straightPathCount, maxPathLength);

                straightPathFlags.Dispose();
                vertexSide.Dispose();
                
                return straightPath;
            }
        }
    }
}