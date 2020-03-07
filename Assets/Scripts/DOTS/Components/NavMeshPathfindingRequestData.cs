using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS
{
    /*
     * Copyright (C) Anton Trukhan, 2020.
    */
    [Serializable]
    public enum PathSearchStatus
    {
        Requested,
        Started,
        Finished,
    }

    public struct NavMeshPathfindingRequestData : IComponentData
    {
        public float3 Start;
        public float3 Destination;
        public PathSearchStatus Status;
        public Entity Agent;
        public Vector3 Extents;
        public int AgentTypeId;
    }
}