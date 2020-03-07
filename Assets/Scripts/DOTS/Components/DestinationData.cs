using Unity.Entities;
using Unity.Mathematics;

namespace DOTS
{
    /*
     * Copyright (C) Anton Trukhan, 2020.
    */
    public struct DestinationData : IComponentData
    {
        public float3 Destination;
    }
}