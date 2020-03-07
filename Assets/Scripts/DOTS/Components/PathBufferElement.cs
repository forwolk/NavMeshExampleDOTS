using Unity.Entities;
using Unity.Mathematics;

namespace DOTS
{
    /*
     * Copyright (C) Anton Trukhan, 2020.
    */
    [InternalBufferCapacity(24)]
    public struct PathBufferElement : IBufferElementData
    {
        public float3 Value;
    }
}