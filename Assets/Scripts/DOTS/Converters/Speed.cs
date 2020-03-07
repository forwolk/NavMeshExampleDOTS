using Unity.Entities;
using UnityEngine;

namespace DOTS
{
    /*
     * Copyright (C) Anton Trukhan, 2020.
    */
    public class Speed : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float SpeedValue;
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData( entity, new SpeedData()
            {
                Speed = SpeedValue,
            });
        }
    }
}