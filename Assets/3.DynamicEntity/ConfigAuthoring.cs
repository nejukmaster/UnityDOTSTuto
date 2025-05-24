using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class ConfigAuthoring : MonoBehaviour
{
    public GameObject TankPrefab;
    public GameObject CannonBallPrefab;
    public int TankCount;

    class Baker : Baker<ConfigAuthoring> 
    {
        public override void Bake(ConfigAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new Config {
                TankPrefab = GetEntity(authoring.TankPrefab, TransformUsageFlags.Dynamic),
                CannonBallPrefab = GetEntity(authoring.CannonBallPrefab, TransformUsageFlags.Dynamic),
                TankCount = authoring.TankCount
            });
        }
    }
}

public struct Config : IComponentData
{
    public Entity TankPrefab;
    public Entity CannonBallPrefab;
    public int TankCount;
}
