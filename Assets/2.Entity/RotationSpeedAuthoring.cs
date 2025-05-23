using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public struct RotationSpeed : IComponentData
{
    public float RadiansPerSecond; // 엔티티가 회전하는 속도
}
public class RotationSpeedAuthoring : MonoBehaviour
{
    public float DegreesPerSecond = 360.0f;
}

//Baker<T : MonoBehaviour> 클래스는 T 컴포넌트가 Subscene Entity에 베이킹 될때의 방법을 정의합니다.
//이 예제에서는 RotationSpeedAuthoring Mono클래스를 RotationSpeed IComponentData 클래스로 Bake하도록 정의합니다.
class RotationSpeedBaker : Baker<RotationSpeedAuthoring>
{
    //Baker클래스에서 반드시 구현해야되는 메서드입니다.
    //authoring은 현재 Bake되고 있는 RotationSpeedAuthoring 인스턴스를 받습니다.
    public override void Bake(RotationSpeedAuthoring authoring)
    {
        //GetEntity를 통해 현재 베이킹되는 엔티티를 들고옵니다.
        //authoring은 현재 Bake 되고있는 인스턴스의 GameObject를 명시적으로 지정합니다. 생략해도 무방합니다.
        //TransformUsageFlags는 해당 게임오브젝트가 Bake될 때 추가할 Transform관련 컴포넌트들을 플래그로 지정합니다. 여기서는 큐브의 회전을 구현하므로 Dynamic으로 설정합니다.
        Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

        RotationSpeed rotationSpeed = new RotationSpeed
        {
            //RotationSpeedAuthoring 클래스에선 회전각을 도(°)단위로 지정하지만, RotationSpeed에선 라디안으로 지정하므로 이를 변환하여 RotationSpeed 구조체를 생성합니다.
            RadiansPerSecond = math.radians(authoring.DegreesPerSecond)
        };
        //생성한 IComponentData 구조체를 엔티티에 컴포넌트로 추가합니다.
        AddComponent(entity, rotationSpeed);
    }
}