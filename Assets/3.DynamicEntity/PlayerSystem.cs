using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

public partial struct PlayerSystem : ISystem
{
    //PlayerSystem�� OnUpdate�� �����Ǵ� ��ü(ī�޶�)�� �����ؾ� �ϹǷ� BurstCompile�� ����� �� �����ϴ�.
    public void OnUpdate(ref SystemState state)
    {
        //Ű���� �Է��� ���ͷ� �ٲ��� DeltaTime�� ���Ͽ� ������ ����
        var movement = new float3(Input.GetAxis("Horizontal"),0,Input.GetAxis("Vertical"));
        movement *= SystemAPI.Time.DeltaTime;

        //Player ������Ʈ�� ���� ��ƼƼ���� ����
        foreach (var playerTransform in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<Player>())
        {
            //Player�� ���� ��ũ�� �����Դϴ�.
            playerTransform.ValueRW.Position += movement;

            //�÷��̾ ���� ī�޶� �����Դϴ�.
            var cameraTransform = Camera.main.transform;
            cameraTransform.position = playerTransform.ValueRO.Position;
            cameraTransform.position -= 10.0f * (Vector3)playerTransform.ValueRO.Forward();
            cameraTransform.position += new Vector3(0, 5f, 0);
            //LookAt�� LocalTransform ����ü�� Ȯ�� �޼���� Ư�� ��ġ�� ���� ȸ���� �ݿ��մϴ�.
            cameraTransform.LookAt(playerTransform.ValueRO.Position);
        }
    }
}
