using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

//ISystem을 구현하는 구조체는 partial로 선언
//ISystem 구조체는 Scene에 명시적으로 추가되진 않으나, Play시에 자동으로 인스턴싱되어 OnUpdate 메서드가 매 프레임 호출됩니다.
//이 부분의 코드를 UnityCompiler가 자동 생성하므로 partial로 선언하여 OnUpdate부분만 작성합니다.
public partial struct CubeRotationSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //ECS 기반의 시스템에서의 DeltaTime
        //Mono의 Time.deltaTime과는 달리 UnityEngine의 GC에 의해 관리되지 않는 DeltaTime으로 BurstCompiler에서도 사용이 가능합니다.
        //Time.deltaTime != SystemAPI.Time.DeltaTime
        var deltaTime = SystemAPI.Time.DeltaTime;

        //LocalTransform은 수정해야 하므로 읽기-쓰기가 가능한 참조 래퍼 클래스 RefRW로 래핑
        //RotationSpeed는 읽기만 하면 되므로 읽기 전용의 참조 래퍼 클래스 RefRO로 래핑
        //SystemAPI.Query는 지정한 컴포넌트를 가진 ECS상의 모든 엔티티의 Iterator를 반환합니다. 이는 foreach 구문의 in절에서만 사용할 수 있습니다.
        //Iterate된 엔티티의 지정한 컴포넌트들은 래퍼의 튜플로 반환되므로 이를 분해해줍니다.
        foreach (var (transform, rotationSpeed) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<RotationSpeed>>())
        {
            var radians = rotationSpeed.ValueRO.RadiansPerSecond * deltaTime;
            //LocalTransform 구조체의 RotateY는 라디안 값을 받아서 Y축으로 그만큼 회전한 새로운 회전값을 반환합니다.
            transform.ValueRW = transform.ValueRW.RotateY(radians);
        }
    }
}
