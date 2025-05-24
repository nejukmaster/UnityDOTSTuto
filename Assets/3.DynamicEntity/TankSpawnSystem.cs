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
        //이 시스템의 업데이트에는 씬에 Config 컴포넌트가 있어야 합니다.
        state.RequireForUpdate<Config>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //이 업데이트가 한번만 일어날 수 있게 업데이트 메소드 실행 후 더 이상의 업데이트를 막습니다.
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
            //베이크된 루트 엔티티는 GO일때의 하위 오브젝트들의 엔티티를 LinkedEntityGroup 컴포넌트의 형태로 가지고 있습니다.
            //LinkedEntityGroup은 IBufferElementData 형태의 컴포넌트로 DynamicBuffer형태로 데이터를 저장하며, GetBuffer를 통해 이 DynamicBuffer를 들고올 수 있습니다. 
            var linkedEntities = state.EntityManager.GetBuffer<LinkedEntityGroup>(tankEntity);
            foreach (var entity in linkedEntities)
            {
                //엔티티가 URPMaterialPropertyBaseColor 컴포넌트를 들고있는지 확인하고 이를 랜덤 색상으로 생성한 color로 설정합니다.
                if (state.EntityManager.HasComponent<URPMaterialPropertyBaseColor>(entity.Value))
                {
                    //IComponentData는 DOP를 만족하는 구조체이므로 기능을 넣지 않습니다.
                    //따라서 URPMaterialPropertyBaseColor의 색상을 바꾸려면 새 URPMaterialPropertyBaseColor를 생성하여 EntityManager의 SetComponentData 메서드를 통해 덮어씌여줍니다.
                    state.EntityManager.SetComponentData(entity.Value, color);
                }
            }
        }
    }

    static float4 RandomColor(ref Random random)
    {
        //0.618034005f는 황금 비율의 역수입니다. 
        var hue = (random.NextFloat() + 0.618034005f) % 1;
        return (Vector4)Color.HSVToRGB(hue, 1.0f, 1.0f);
    }
}
