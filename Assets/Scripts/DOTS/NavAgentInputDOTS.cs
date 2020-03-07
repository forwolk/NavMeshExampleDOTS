using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using Ray = UnityEngine.Ray;

namespace DOTS
{
    /*
     * Copyright (C) Anton Trukhan, 2020.
    */
    public class NavAgentInputDOTS : MonoBehaviour
    {
        [SerializeField] private Camera Camera;

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.ScreenPointToRay(Input.mousePosition);
                
                var world = World.DefaultGameObjectInjectionWorld;
                PhysicsWorld physicsWorld = world.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;

                var raycastInput = new RaycastInput
                {
                    Start = ray.origin,
                    End = ray.origin + ray.direction * int.MaxValue,
                    Filter = CollisionFilter.Default
                };
                if (!physicsWorld.CastRay(raycastInput, out var result))
                {
                    return;
                }
                

                EntityManager manager = world.EntityManager;
                
                //Search path
                var buffer = manager.GetBuffer<PathBufferElement>(DOTSLocator.AgentEntity);
                var pathData = manager.GetComponentData<FollowPathData>(DOTSLocator.AgentEntity);
                if (pathData.PathStatus == PathStatus.EndOfPathReached)
                {
                    buffer.Clear();
                    manager.SetComponentData(DOTSLocator.AgentEntity, new FollowPathData { PathIndex = 0, PathStatus = PathStatus.Calculated});
                }
                
                var translation = manager.GetComponentData<Translation>(DOTSLocator.AgentEntity);
                var requestEntity = manager.CreateEntity();
                manager.AddComponentData(requestEntity, new NavMeshPathfindingRequestData
                {
                    Start = translation.Value, 
                    Destination = result.Position, 
                    Status = PathSearchStatus.Requested, 
                    Agent = DOTSLocator.AgentEntity,
                    Extents = Vector3.one * 2,
                    AgentTypeId = 0
                });
            }
        }
    }
}