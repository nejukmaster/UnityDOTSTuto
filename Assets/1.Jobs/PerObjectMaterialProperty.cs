using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialProperty : MonoBehaviour
{
    static int baseColorId = Shader.PropertyToID("_BaseColor"),
                cutoffId = Shader.PropertyToID("_Cutoff"),
                //mataliic�� smoothness ������Ƽ �߰�
                metallicId = Shader.PropertyToID("_Metallic"),
		        smoothnessId = Shader.PropertyToID("_Smoothness"),
                emissionColorId = Shader.PropertyToID("_EmissionColor");

    //�� ��ũ��Ʈ�� ����ϴ� ��� ��ü���� ������ �������� ������ MaterialPropertyBlock�Դϴ�
    static MaterialPropertyBlock block;

    [SerializeField]
    Color baseColor = Color.white;

    //ColorUsage �Ӽ��� �ν����Ϳ� Color�� ǥ���ϴ� ����� �����մϴ�.
    //ù��° �Ӽ����� Alpha ä�� ǥ�ÿ���,
    //�ι�° �Ӽ����� HDR�� ��� ���θ� �޽��ϴ�.
    [SerializeField, ColorUsage(false, true)]
    Color emissionColor = Color.black;

    [SerializeField, Range(0f, 1f)]
    float cutoff = 0.5f, metallic = 0f, smoothness = 0.5f;

    //OnValidate�� ������Ұ� �ε�ǰų� ����ɶ� ȣ��˴ϴ�.
    void OnValidate()
    {
        //MaterialPropertyBlock�� ���� �������� �����ϱ� ���� ����� �ʱ�ȭ�մϴ�.
        if (block == null)
        {
            block = new MaterialPropertyBlock();
        }
        block.SetColor(baseColorId, baseColor);
        block.SetFloat(cutoffId, cutoff);
        block.SetFloat(metallicId, metallic);
        block.SetFloat(smoothnessId, smoothness);
        block.SetColor(emissionColorId, emissionColor);
        GetComponent<Renderer>().SetPropertyBlock(block);
    }

    //OnValidate �޼���� ���忡�� ȣ����� �ʱ⿡ �̴� �������� ȣ�����ݴϴ�.
    void Awake()
    {
        OnValidate();
    }
}
