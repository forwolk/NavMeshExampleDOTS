using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS
{
    /*
     * Copyright (C) Anton Trukhan, 2020.
    */
    public class Destination : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData( entity, new DestinationData()
            {
                Destination = transform.position
            });
        }
    }
}