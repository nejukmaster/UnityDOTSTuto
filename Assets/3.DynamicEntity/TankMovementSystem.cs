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
        //SystemAPI.Query�� ���� LocalTransform�� ���� ��� ��ƼƼ�� ��ȸ�Ͽ� Read-Write�� �ش� ������Ʈ�� Ʃ�ÿ� ��� ��ȯ�մϴ�.
        //WithAll<T>�� T ������Ʈ�� ���� ��ƼƼ�� �������ϴ�.
        //WithEntityAccess�� Iterate�� Ʃ�ÿ� ��ȸ�� ��ƼƼ�� �����մϴ�.
        //Player ������Ʈ�� ���� Tank ��ƼƼ�� �÷��̾ ���� ������ ���̹Ƿ� WithNone�� ���� �������� ���ܽ�ŵ�ϴ�.
        foreach (var (transform, entity) in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<Tank>().WithNone<Player>().WithEntityAccess())
        {
            //LocalTransform�� Position�� Read-Only�� �����ɴϴ�.
            var pos = transform.ValueRO.Position; 
            //Index�� �ش� ��ƼƼ�� ������ ID��(����) �Դϴ�.
            //index�� ��ƼƼ ������ ���̹Ƿ� ��ȸ�� ��� ��ƼƼ�� y��ǥ�� �޶����ϴ�.
            pos.y = (float)entity.Index;

            //noise.cnoise�� ���ڸ� ���� Perlin noise�� ���ø��մϴ�.
            var angle = (0.5f + noise.cnoise(pos / 10f)) * 4.0f * math.PI;
            var dir = float3.zero;
            //math.sincos�� ���ڸ� ���� sin���� cos���� ���� ��ȯ�մϴ�.
            math.sincos(angle, out dir.x, out dir.z);

            // LocalTransform�� ������Ʈ�մϴ�. 
            transform.ValueRW.Position += dir * dt * 5.0f;
            transform.ValueRW.Rotation = quaternion.RotateY(angle);
        }

        //Tank ��ƼƼ�� ȸ���� �߰��մϴ�.
        //RotateY �޼���� �־��� ����(����)��ŭ�� Y�� ���� ȸ�� ���ʹϿ��� 
        var spin = quaternion.RotateY(SystemAPI.Time.DeltaTime * math.PI);

        foreach (var tank in SystemAPI.Query<RefRW<Tank>>())
        {
            var trans = SystemAPI.GetComponentRW<LocalTransform>(tank.ValueRO.Turret);

            //���ʹϿ��� ȸ���� �� ���ʹϿ��� ��İ��Ͽ� ����մϴ�.
            trans.ValueRW.Rotation = math.mul(spin, trans.ValueRO.Rotation);
        }
    }
}
