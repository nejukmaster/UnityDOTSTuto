using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public struct RotationSpeed : IComponentData
{
    public float RadiansPerSecond; // ��ƼƼ�� ȸ���ϴ� �ӵ�
}
public class RotationSpeedAuthoring : MonoBehaviour
{
    public float DegreesPerSecond = 360.0f;
}

//Baker<T : MonoBehaviour> Ŭ������ T ������Ʈ�� Subscene Entity�� ����ŷ �ɶ��� ����� �����մϴ�.
//�� ���������� RotationSpeedAuthoring MonoŬ������ RotationSpeed IComponentData Ŭ������ Bake�ϵ��� �����մϴ�.
class RotationSpeedBaker : Baker<RotationSpeedAuthoring>
{
    //BakerŬ�������� �ݵ�� �����ؾߵǴ� �޼����Դϴ�.
    //authoring�� ���� Bake�ǰ� �ִ� RotationSpeedAuthoring �ν��Ͻ��� �޽��ϴ�.
    public override void Bake(RotationSpeedAuthoring authoring)
    {
        //GetEntity�� ���� ���� ����ŷ�Ǵ� ��ƼƼ�� ���ɴϴ�.
        //authoring�� ���� Bake �ǰ��ִ� �ν��Ͻ��� GameObject�� ��������� �����մϴ�. �����ص� �����մϴ�.
        //TransformUsageFlags�� �ش� ���ӿ�����Ʈ�� Bake�� �� �߰��� Transform���� ������Ʈ���� �÷��׷� �����մϴ�. ���⼭�� ť���� ȸ���� �����ϹǷ� Dynamic���� �����մϴ�.
        Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

        RotationSpeed rotationSpeed = new RotationSpeed
        {
            //RotationSpeedAuthoring Ŭ�������� ȸ������ ��(��)������ ����������, RotationSpeed���� �������� �����ϹǷ� �̸� ��ȯ�Ͽ� RotationSpeed ����ü�� �����մϴ�.
            RadiansPerSecond = math.radians(authoring.DegreesPerSecond)
        };
        //������ IComponentData ����ü�� ��ƼƼ�� ������Ʈ�� �߰��մϴ�.
        AddComponent(entity, rotationSpeed);
    }
}