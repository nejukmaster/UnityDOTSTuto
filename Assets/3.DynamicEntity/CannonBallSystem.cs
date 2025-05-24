using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct CannonBallSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //EndSimulationEntityCommandBufferSystem�� �̱��� ��ü�� ���ɴϴ�.
        //EndSimulationEntityCommandBufferSystem�� SimulationSystemGroup�� ���� �������� ����Ǵ� System�̸�, �̴� ���� ������ ���� ����˴ϴ�.
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

        var cannonBallJob = new CannonBallJob
        {
            //EndSimulationEntityCommandBufferSystem���� EntityCommandBuffer�� ����� �̸� CannonBallJob�� ����Ͽ� �ش� Job���� ����� �� �ֵ��� �մϴ�.
            //�̷��� EndSimulationEntityCommandBufferSystem�� ����� EntityCommandBuffer�� System�� ����� �� ���������� ����˴ϴ�.
            /****����ó��****/
            //���� ó����, AsParallelWriter �޼��带 ���� ECB�� ParallelWriter ��ü�� �Ѱ��ݴϴ�.
            ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
            //Job�� �����ϰ�, �� ������ DeltaTime�� �Ѱ��ݴϴ�.
            DeltaTime = SystemAPI.Time.DeltaTime
        };

        //cannonBallJob.Schedule();
        /****���� ó��****/
        //��������� cannonBallJob�� ���� ó���� �����մϴ�.
        //�� ��� ����ð��� Tank 10�� ���� 0.03ms���� 0.01ms ���Ϸ� �������� Ȯ���߽��ϴ�.
        cannonBallJob.ScheduleParallel();
    }
}

//IJobEntity�� ��ƼƼ �߽��� Job�Դϴ�.
//������ ������Ʈ�� ��ƼƼ�� �����Ͽ� ����Ǹ�, ����ó���� �����մϴ�.
[BurstCompile]
public partial struct CannonBallJob : IJobEntity
{
    //System���� ����ϴ� EntityManager�� ��Ƽ���������� ����� �Ұ���. ��ſ� EntityCommandBuffer�� ���
    //EntityCommandBuffer�� EntityManager�� ���� ��ƼƼ�� ����/����/������ �� ������, IJobEntity�� ���� ��Ƽ������ ȯ�濡���� ����� �� �ֽ��ϴ�.
    //public EntityCommandBuffer ECB;
    /****����ó��****/
    //IJobEntity�� ����ó���� �����ϰ��� ���, ECB�� EntityCommandBuffer.ParallelWriter�� ����Ͽ��� �մϴ�.
    public EntityCommandBuffer.ParallelWriter ECB;
    public float DeltaTime;

    //IJobEntity�� Execute�� ������ �� IComponentData ������ ���ڷ� ���� �� �ֽ��ϴ�.
    //�̷���� ���ڷ� ���� IComponentData�� ������Ʈ�� ������ �ִ� ��ƼƼ�� ���ؼ� �Ͻ������� �����ϰԵ˴ϴ�.
    /****���� ó��****/
    //IJobEntity�� ����ó���� �����ϰ��� ���, Entity�� ���� ������ �ε����� Execute�� ���ڷ� �Ѱ��ݴϴ�.
    //�̶� [EntityIndexInQuery] ��Ʈ����Ʈ�� ����Ͽ� �Ѱ��־�� Entity�� ���� ������ �ε����� �����ϰԵ˴ϴ�.
    void Execute([EntityIndexInQuery] int index, Entity entity, ref CannonBall cannonBall, ref LocalTransform transform)
    {
        var gravity = new float3(0.0f, -9.82f, 0.0f);

        transform.Position += cannonBall.Velocity * DeltaTime;

        if (transform.Position.y <= 0.0f)
        {
            //EntityCommandBuffer�� ���� ���� CannonBall ��ƼƼ�� �ı��ϴ� �۾��� �߰��մϴ�.
            //EndSimulation ������ EndSimulationEntityCommandBufferSystem�� �ڵ����� ����ǹǷ� PlayBack �޼��� ȣ���� �ʿ����� �ʽ��ϴ�.
            //ECB.DestroyEntity(entity);
            /****���� ó��****/
            //EntityCommandBuffer.ParallelWriter�� ECB�� ����� ���, ���ڿ� ��ƼƼ�� ������ �ε����� �Բ� �Ѱ��־�� �մϴ�.
            ECB.DestroyEntity(index, entity);
        }

        cannonBall.Velocity += gravity * DeltaTime;
    }
}