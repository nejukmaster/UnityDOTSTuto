using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine;

public partial struct TankMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var dt = SystemAPI.Time.DeltaTime;
        //SystemAPI.Query를 통해 LocalTransform을 가진 모든 엔티티를 순회하여 Read-Write로 해당 컴포넌트를 튜플에 담아 반환합니다.
        //WithAll<T>는 T 컴포넌트를 가진 엔티티만 가려냅니다.
        //WithEntityAccess는 Iterate된 튜플에 순회한 엔티티를 포함합니다.
        //Player 컴포넌트가 붙은 Tank 엔티티는 플레이어가 직접 조종할 것이므로 WithNone을 통해 쿼리에서 제외시킵니다.
        foreach (var (transform, entity) in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<Tank>().WithNone<Player>().WithEntityAccess())
        {
            //LocalTransform의 Position을 Read-Only로 가져옵니다.
            var pos = transform.ValueRO.Position; 
            //Index는 해당 엔티티의 고유한 ID값(정수) 입니다.
            //index는 엔티티 고유의 값이므로 순회된 모든 엔티티의 y좌표는 달라집니다.
            pos.y = (float)entity.Index;

            //noise.cnoise는 인자를 통해 Perlin noise를 샘플링합니다.
            var angle = (0.5f + noise.cnoise(pos / 10f)) * 4.0f * math.PI;
            var dir = float3.zero;
            //math.sincos는 인자를 통해 sin값과 cos값을 각각 반환합니다.
            math.sincos(angle, out dir.x, out dir.z);

            // LocalTransform을 업데이트합니다. 
            transform.ValueRW.Position += dir * dt * 5.0f;
            transform.ValueRW.Rotation = quaternion.RotateY(angle);
        }

        //Tank 엔티티에 회전을 추가합니다.
        //RotateY 메서드는 주어직 각도(라디안)만큼의 Y축 기준 회전 쿼터니온을 
        var spin = quaternion.RotateY(SystemAPI.Time.DeltaTime * math.PI);

        foreach (var tank in SystemAPI.Query<RefRW<Tank>>())
        {
            var trans = SystemAPI.GetComponentRW<LocalTransform>(tank.ValueRO.Turret);

            //쿼터니온의 회전은 두 쿼터니온을 행렬곱하여 계산합니다.
            trans.ValueRW.Rotation = math.mul(spin, trans.ValueRO.Rotation);
        }
    }
}
