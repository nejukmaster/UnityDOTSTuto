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

//��� ��ũ�� ��Ʈ ��ƼƼ�� �߰��� ���� ����Դϴ�. 
public struct Tank : IComponentData
{
    public Entity Turret;
    public Entity Cannon;
}