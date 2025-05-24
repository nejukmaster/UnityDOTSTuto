using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

//System���� ������Ʈ ������ �����ϴ� ��Ʈ����Ʈ�Դϴ�.
//UpdateBefore ��Ʈ����Ʈ�� Ư�� System���� �� ���� System�� Update �ǵ��� �մϴ�.
//ShootingSystem�� ��ź�� Local Transform�� �����ϹǷ� ��ƼƼ���� WorldTransform�� �����ϴ� TransformSystemGroup���� ���� �߻��ؾ��մϴ�.
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct ShootingSystem : ISystem
{
    private float timer;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //Ÿ�̸� ����
        timer -= SystemAPI.Time.DeltaTime;
        if (timer > 0)
        {
            //��ź ���� ��Ÿ���� ������ ��� Early Return
            return;
        }
        timer = 0.3f;

        var config = SystemAPI.GetSingleton<Config>();

        var ballTransform = state.EntityManager.GetComponentData<LocalTransform>(config.CannonBallPrefab);

        //��� Tank ��ƼƼ�� ���� Tank, LocalToWorld, URPMaterialPropertyBaseColor ������Ʈ�� �����մϴ�.
        foreach (var (tank, transform, color) in SystemAPI.Query<RefRO<Tank>, RefRO<LocalToWorld>, RefRO<URPMaterialPropertyBaseColor>>())
        {
            //��� Tank ���� ��ź�� �����մϴ�.
            Entity cannonBallEntity = state.EntityManager.Instantiate(config.CannonBallPrefab);

            //��ź(CannonBall)�� URPMaterialPropertyBaseColor�� Tank�� ���� �����մϴ�.
            state.EntityManager.SetComponentData(cannonBallEntity, color.ValueRO);

            //CannonBall�� ��ġ�� Tank ��ƼƼ�� Cannon �κ��� local position���� �����մϴ�.
            var cannonTransform = state.EntityManager.GetComponentData<LocalToWorld>(tank.ValueRO.Cannon);
            ballTransform.Position = cannonTransform.Position;

            //������ LocalToWorld ������Ʈ�� �����մϴ�.
            state.EntityManager.SetComponentData(cannonBallEntity, ballTransform);

            //CannonBall�� ���ư��� �ӵ��� �����մϴ�.
            state.EntityManager.SetComponentData(cannonBallEntity, new CannonBall{
                Velocity = math.normalize(cannonTransform.Up) * 12.0f
            });
        }
    }
}