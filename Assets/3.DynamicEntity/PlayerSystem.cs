using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

public partial struct PlayerSystem : ISystem
{
    //PlayerSystem의 OnUpdate는 관리되는 객체(카메라)에 접근해야 하므로 BurstCompile을 사용할 수 없습니다.
    public void OnUpdate(ref SystemState state)
    {
        //키보드 입력을 벡터로 바꾼후 DeltaTime을 곱하여 프레임 보정
        var movement = new float3(Input.GetAxis("Horizontal"),0,Input.GetAxis("Vertical"));
        movement *= SystemAPI.Time.DeltaTime;

        //Player 컴포넌트를 가진 엔티티들을 쿼리
        foreach (var playerTransform in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<Player>())
        {
            //Player를 가진 탱크를 움직입니다.
            playerTransform.ValueRW.Position += movement;

            //플레이어를 따라서 카메라를 움직입니다.
            var cameraTransform = Camera.main.transform;
            cameraTransform.position = playerTransform.ValueRO.Position;
            cameraTransform.position -= 10.0f * (Vector3)playerTransform.ValueRO.Forward();
            cameraTransform.position += new Vector3(0, 5f, 0);
            //LookAt은 LocalTransform 구조체의 확장 메서드로 특정 위치를 보는 회전을 반영합니다.
            cameraTransform.LookAt(playerTransform.ValueRO.Position);
        }
    }
}
