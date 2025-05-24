using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public partial struct TankSpawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        //�� �ý����� ������Ʈ���� ���� Config ������Ʈ�� �־�� �մϴ�.
        state.RequireForUpdate<Config>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //�� ������Ʈ�� �ѹ��� �Ͼ �� �ְ� ������Ʈ �޼ҵ� ���� �� �� �̻��� ������Ʈ�� �����ϴ�.
        state.Enabled = false;

        var config = SystemAPI.GetSingleton<Config>();

        var random = new Random(123);

        for (int i = 0; i < config.TankCount; i++)
        {
            var tankEntity = state.EntityManager.Instantiate(config.TankPrefab);

            if (i == 0)
            {
                state.EntityManager.AddComponent<Player>(tankEntity);
            }

            var color = new URPMaterialPropertyBaseColor { Value = RandomColor(ref random) };
            //����ũ�� ��Ʈ ��ƼƼ�� GO�϶��� ���� ������Ʈ���� ��ƼƼ�� LinkedEntityGroup ������Ʈ�� ���·� ������ �ֽ��ϴ�.
            //LinkedEntityGroup�� IBufferElementData ������ ������Ʈ�� DynamicBuffer���·� �����͸� �����ϸ�, GetBuffer�� ���� �� DynamicBuffer�� ���� �� �ֽ��ϴ�. 
            var linkedEntities = state.EntityManager.GetBuffer<LinkedEntityGroup>(tankEntity);
            foreach (var entity in linkedEntities)
            {
                //��ƼƼ�� URPMaterialPropertyBaseColor ������Ʈ�� ����ִ��� Ȯ���ϰ� �̸� ���� �������� ������ color�� �����մϴ�.
                if (state.EntityManager.HasComponent<URPMaterialPropertyBaseColor>(entity.Value))
                {
                    //IComponentData�� DOP�� �����ϴ� ����ü�̹Ƿ� ����� ���� �ʽ��ϴ�.
                    //���� URPMaterialPropertyBaseColor�� ������ �ٲٷ��� �� URPMaterialPropertyBaseColor�� �����Ͽ� EntityManager�� SetComponentData �޼��带 ���� ������ݴϴ�.
                    state.EntityManager.SetComponentData(entity.Value, color);
                }
            }
        }
    }

    static float4 RandomColor(ref Random random)
    {
        //0.618034005f�� Ȳ�� ������ �����Դϴ�. 
        var hue = (random.NextFloat() + 0.618034005f) % 1;
        return (Vector4)Color.HSVToRGB(hue, 1.0f, 1.0f);
    }
}
