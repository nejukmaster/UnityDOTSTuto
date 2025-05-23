using Unity.Entities;
using UnityEngine;

public class SpawnerAuthoring : MonoBehaviour
{
    public GameObject CubePrefab;

    //SpawnerAuthoring�� ����ŷ�� BakerŬ����
    //���� MonoŬ������ �ش� BakerŬ������ �� MonoŬ������ ���� Ŭ������ �ۼ��ϴ� ���� �����̴�.
    class Baker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            //CubeSpawner ��ƼƼ�� ���ӻ󿡼� ǥ�õ��� �ʰ� �Ұ��̹Ƿ� TransformUsageFlags�� None���� �����մϴ�. 
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            var spawner = new CubeSpawner
            {
                //�ݸ�, RotatingCube��ƼƼ�� ���ӻ󿡼� ǥ�õǰ�, �� ���ư����ϹǷ� TransformUsageFlags�� Dynamic���� �����մϴ�.
                CubePrefab = GetEntity(authoring.CubePrefab, TransformUsageFlags.Dynamic)
            };
            AddComponent(entity, spawner);
        }
    }
}

struct CubeSpawner : IComponentData
{
    public Entity CubePrefab;
} 
