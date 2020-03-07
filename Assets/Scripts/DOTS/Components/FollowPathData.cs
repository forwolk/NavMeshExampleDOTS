using Unity.Entities;

namespace DOTS
{
    /*
     * Copyright (C) Anton Trukhan, 2020.
    */
    public enum PathStatus
    {
        Calculated,
        Following,
        EndOfPathReached
    }
    
    public struct FollowPathData : IComponentData
    {
        public int PathIndex;
        public PathStatus PathStatus;
    }
}