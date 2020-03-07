using Unity.Entities;
using UnityEngine;

namespace DOTS
{
    /*
     * Copyright (C) Anton Trukhan, 2020.
    */
    public class FollowPath : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            manager.AddComponentData(entity, new FollowPathData());
            manager.AddBuffer<PathBufferElement>(entity);

        }
    }
}