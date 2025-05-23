using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

//ISystem�� �����ϴ� ����ü�� partial�� ����
//ISystem ����ü�� Scene�� ��������� �߰����� ������, Play�ÿ� �ڵ����� �ν��Ͻ̵Ǿ� OnUpdate �޼��尡 �� ������ ȣ��˴ϴ�.
//�� �κ��� �ڵ带 UnityCompiler�� �ڵ� �����ϹǷ� partial�� �����Ͽ� OnUpdate�κи� �ۼ��մϴ�.
public partial struct CubeRotationSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //ECS ����� �ý��ۿ����� DeltaTime
        //Mono�� Time.deltaTime���� �޸� UnityEngine�� GC�� ���� �������� �ʴ� DeltaTime���� BurstCompiler������ ����� �����մϴ�.
        //Time.deltaTime != SystemAPI.Time.DeltaTime
        var deltaTime = SystemAPI.Time.DeltaTime;

        //LocalTransform�� �����ؾ� �ϹǷ� �б�-���Ⱑ ������ ���� ���� Ŭ���� RefRW�� ����
        //RotationSpeed�� �б⸸ �ϸ� �ǹǷ� �б� ������ ���� ���� Ŭ���� RefRO�� ����
        //SystemAPI.Query�� ������ ������Ʈ�� ���� ECS���� ��� ��ƼƼ�� Iterator�� ��ȯ�մϴ�. �̴� foreach ������ in�������� ����� �� �ֽ��ϴ�.
        //Iterate�� ��ƼƼ�� ������ ������Ʈ���� ������ Ʃ�÷� ��ȯ�ǹǷ� �̸� �������ݴϴ�.
        foreach (var (transform, rotationSpeed) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<RotationSpeed>>())
        {
            var radians = rotationSpeed.ValueRO.RadiansPerSecond * deltaTime;
            //LocalTransform ����ü�� RotateY�� ���� ���� �޾Ƽ� Y������ �׸�ŭ ȸ���� ���ο� ȸ������ ��ȯ�մϴ�.
            transform.ValueRW = transform.ValueRW.RotateY(radians);
        }
    }
}
