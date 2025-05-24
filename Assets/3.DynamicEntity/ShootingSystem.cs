using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

//System들의 업데이트 순서를 설정하는 어트리뷰트입니다.
//UpdateBefore 어트리뷰트는 특정 System보다 더 먼저 System이 Update 되도록 합니다.
//ShootingSystem은 포탄의 Local Transform을 설정하므로 엔티티들의 WorldTransform을 설정하는 TransformSystemGroup보다 먼저 발생해야합니다.
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct ShootingSystem : ISystem
{
    private float timer;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //타이머 진행
        timer -= SystemAPI.Time.DeltaTime;
        if (timer > 0)
        {
            //포탄 생성 쿨타임이 남았을 경우 Early Return
            return;
        }
        timer = 0.3f;

        var config = SystemAPI.GetSingleton<Config>();

        var ballTransform = state.EntityManager.GetComponentData<LocalTransform>(config.CannonBallPrefab);

        //모든 Tank 엔티티에 대해 Tank, LocalToWorld, URPMaterialPropertyBaseColor 컴포넌트를 쿼리합니다.
        foreach (var (tank, transform, color) in SystemAPI.Query<RefRO<Tank>, RefRO<LocalToWorld>, RefRO<URPMaterialPropertyBaseColor>>())
        {
            //모든 Tank 대해 포탄을 생성합니다.
            Entity cannonBallEntity = state.EntityManager.Instantiate(config.CannonBallPrefab);

            //포탄(CannonBall)의 URPMaterialPropertyBaseColor를 Tank와 같게 설정합니다.
            state.EntityManager.SetComponentData(cannonBallEntity, color.ValueRO);

            //CannonBall의 위치를 Tank 엔티티의 Cannon 부분의 local position으로 설정합니다.
            var cannonTransform = state.EntityManager.GetComponentData<LocalToWorld>(tank.ValueRO.Cannon);
            ballTransform.Position = cannonTransform.Position;

            //설정한 LocalToWorld 컴포넌트를 적용합니다.
            state.EntityManager.SetComponentData(cannonBallEntity, ballTransform);

            //CannonBall이 날아가는 속도를 설정합니다.
            state.EntityManager.SetComponentData(cannonBallEntity, new CannonBall{
                Velocity = math.normalize(cannonTransform.Up) * 12.0f
            });
        }
    }
}