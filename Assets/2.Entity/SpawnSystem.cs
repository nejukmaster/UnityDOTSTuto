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
        //RequireForUpdate�� �� �ý����� ������Ʈ�� �䱸�Ǵ� ������Ʈ�� �����մϴ�.
        //�� ���, ���� ���� CubeSpawner ������Ʈ�� ���� ������Ʈ�� �������� �ʴ´ٸ� �� �ý����� ������Ʈ�� �Ͼ�� �ʽ��ϴ�.
        state.RequireForUpdate<CubeSpawner>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //SystemAPI.GetSingleton �޼���� ���� ������ �ϳ��ۿ� �������� �ʴ� ������Ʈ�� ���
        var prefab = SystemAPI.GetSingleton<CubeSpawner>().CubePrefab;
        var instances = state.EntityManager.Instantiate(prefab, 10, Allocator.Temp);

        var random = new Random(123);
        foreach (var entity in instances)
        {
            var transform = SystemAPI.GetComponentRW<LocalTransform>(entity);
            //NextFloat3�� ������Ұ� [0, max)�� ������ �Ǽ����� float3�� ��ȯ�մϴ�.
            transform.ValueRW.Position = random.NextFloat3(new float3(10, 10, 10));
        }

        //SpawnSystem���� ���� ť�� ��ƼƼ �ν��Ͻ��� �ѹ��� �Ͼ�� �ϹǷ� ���� ������ ���̻��� Update�� �Ͼ�� �ʰ� �մϴ�.
        state.Enabled = false;
    }
}