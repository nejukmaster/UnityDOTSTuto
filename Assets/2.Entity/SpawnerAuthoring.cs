using Unity.Entities;
using UnityEngine;

public class SpawnerAuthoring : MonoBehaviour
{
    public GameObject CubePrefab;

    //SpawnerAuthoring을 베이킹할 Baker클래스
    //보통 Mono클래스의 해당 Baker클래스는 그 Mono클래스의 하위 클래스로 작성하는 것이 관례이다.
    class Baker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            //CubeSpawner 엔티티는 게임상에서 표시되지 않게 할것이므로 TransformUsageFlags를 None으로 설정합니다. 
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            var spawner = new CubeSpawner
            {
                //반면, RotatingCube엔티티는 게임상에서 표시되고, 또 돌아가야하므로 TransformUsageFlags를 Dynamic으로 설정합니다.
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
