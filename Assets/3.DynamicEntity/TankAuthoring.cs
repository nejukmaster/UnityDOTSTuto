using Unity.Entities;
using UnityEngine;

public class TankAuthoring : MonoBehaviour
{
    public GameObject Turret;
    public GameObject Cannon;

    class Baker : Baker<TankAuthoring>
    {
        public override void Bake(TankAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new Tank{
                Turret = GetEntity(authoring.Turret, TransformUsageFlags.Dynamic),
                Cannon = GetEntity(authoring.Cannon, TransformUsageFlags.Dynamic)
            });
        }
    }
}

//모든 탱크의 루트 엔티티에 추가될 구성 요소입니다. 
public struct Tank : IComponentData
{
    public Entity Turret;
    public Entity Cannon;
}