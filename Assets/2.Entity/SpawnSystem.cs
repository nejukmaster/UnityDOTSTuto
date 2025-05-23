using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct SpawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        //RequireForUpdate는 이 시스템의 업데이트에 요구되는 컴포넌트를 지정합니다.
        //이 경우, 현재 씬에 CubeSpawner 컴포넌트를 가진 오브젝트가 존재하지 않는다면 이 시스템은 업데이트가 일어나지 않습니다.
        state.RequireForUpdate<CubeSpawner>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //SystemAPI.GetSingleton 메서드는 현재 씬에서 하나밖에 존재하지 않는 컴포넌트를 들고
        var prefab = SystemAPI.GetSingleton<CubeSpawner>().CubePrefab;
        var instances = state.EntityManager.Instantiate(prefab, 10, Allocator.Temp);

        var random = new Random(123);
        foreach (var entity in instances)
        {
            var transform = SystemAPI.GetComponentRW<LocalTransform>(entity);
            //NextFloat3는 구성요소가 [0, max)의 무작위 실수값의 float3를 반환합니다.
            transform.ValueRW.Position = random.NextFloat3(new float3(10, 10, 10));
        }

        //SpawnSystem으로 인한 큐브 엔티티 인스턴싱은 한번만 일어나야 하므로 로직 수행후 더이상의 Update가 일어나지 않게 합니다.
        state.Enabled = false;
    }
}