using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct CannonBallSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //EndSimulationEntityCommandBufferSystem의 싱글톤 객체를 들고옵니다.
        //EndSimulationEntityCommandBufferSystem은 SimulationSystemGroup중 가장 마지막에 실행되는 System이며, 이는 보통 프레임 끝에 실행됩니다.
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

        var cannonBallJob = new CannonBallJob
        {
            //EndSimulationEntityCommandBufferSystem에서 EntityCommandBuffer를 만들고 이를 CannonBallJob에 등록하여 해당 Job에서 사용할 수 있도록 합니다.
            //이렇게 EndSimulationEntityCommandBufferSystem에 모아진 EntityCommandBuffer는 System이 실행될 때 순차적으로 실행됩니다.
            /****병렬처리****/
            //병렬 처리시, AsParallelWriter 메서드를 통해 ECB의 ParallelWriter 객체를 넘겨줍니다.
            ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
            //Job을 생성하고, 그 시점의 DeltaTime을 넘겨줍니다.
            DeltaTime = SystemAPI.Time.DeltaTime
        };

        //cannonBallJob.Schedule();
        /****병렬 처리****/
        //명시적으로 cannonBallJob의 병렬 처리를 지시합니다.
        //이 경우 실행시간이 Tank 10개 기준 0.03ms에서 0.01ms 이하로 개선됨을 확인했습니다.
        cannonBallJob.ScheduleParallel();
    }
}

//IJobEntity는 엔티티 중심의 Job입니다.
//지정한 컴포넌트의 엔티티를 쿼리하여 수행되며, 병렬처리를 지원합니다.
[BurstCompile]
public partial struct CannonBallJob : IJobEntity
{
    //System에서 사용하는 EntityManager은 멀티스레딩에서 사용이 불가함. 대신에 EntityCommandBuffer를 사용
    //EntityCommandBuffer는 EntityManager과 같이 엔티티를 생성/삭제/변경할 수 있으며, IJobEntity와 같은 멀티스레딩 환경에서도 사용할 수 있습니다.
    //public EntityCommandBuffer ECB;
    /****병렬처리****/
    //IJobEntity의 병렬처리를 지원하고픈 경우, ECB를 EntityCommandBuffer.ParallelWriter로 사용하여야 합니다.
    public EntityCommandBuffer.ParallelWriter ECB;
    public float DeltaTime;

    //IJobEntity의 Execute를 구현할 때 IComponentData 참조를 인자로 받을 수 있습니다.
    //이럴경우 인자로 받은 IComponentData를 컴포넌트로 가지고 있는 엔티티에 대해서 암시적으로 쿼리하게됩니다.
    /****병렬 처리****/
    //IJobEntity의 병렬처리를 지원하고픈 경우, Entity가 속한 쿼리내 인덱스를 Execute의 인자로 넘겨줍니다.
    //이때 [EntityIndexInQuery] 어트리뷰트를 사용하여 넘겨주어야 Entity가 속한 쿼리내 인덱스를 지시하게됩니다.
    void Execute([EntityIndexInQuery] int index, Entity entity, ref CannonBall cannonBall, ref LocalTransform transform)
    {
        var gravity = new float3(0.0f, -9.82f, 0.0f);

        transform.Position += cannonBall.Velocity * DeltaTime;

        if (transform.Position.y <= 0.0f)
        {
            //EntityCommandBuffer에 땅에 닿은 CannonBall 엔티티를 파괴하는 작업을 추가합니다.
            //EndSimulation 시점에 EndSimulationEntityCommandBufferSystem이 자동으로 실행되므로 PlayBack 메서드 호출이 필요하지 않습니다.
            //ECB.DestroyEntity(entity);
            /****병렬 처리****/
            //EntityCommandBuffer.ParallelWriter로 ECB를 사용할 경우, 인자에 엔티티의 쿼리내 인덱스도 함께 넘겨주어야 합니다.
            ECB.DestroyEntity(index, entity);
        }

        cannonBall.Velocity += gravity * DeltaTime;
    }
}