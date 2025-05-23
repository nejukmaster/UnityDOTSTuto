
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class FindNearest : MonoBehaviour
{
    NativeArray<float3> TargetPositions;
    NativeArray<float3> SeekerPositions;
    NativeArray<float3> NearestTargetPositions;

    public void Start()
    {
        Spawner spawner = Object.FindObjectOfType<Spawner>();
        //NativeArray�� �����ڿ����� �Ҵ��� ũ��� �޸� �Ҵ� ����� ���ڷ� �޽��ϴ�.
        //���⼭�� ������ NativeArray�� ��ð� ������ �ʿ��ϹǷ� Persistent�� �Ҵ��մϴ�.
        //�̿ܿ��� Temp(�� ������ ����), TempJob(Job�ý��� ���� �� ������)�� �ֽ��ϴ�.
        TargetPositions = new NativeArray<float3>(spawner.NumTargets, Allocator.Persistent);
        SeekerPositions = new NativeArray<float3>(spawner.NumSeekers, Allocator.Persistent);
        NearestTargetPositions = new NativeArray<float3>(spawner.NumSeekers, Allocator.Persistent);
    }

    public void OnDestroy()
    {
        //NativeArray�� GC�� ���� �������� �ʱ� ������, Persistent�� �Ҵ��� ��� Dispose �޼��带 ���� �������־���մϴ�.
        TargetPositions.Dispose();
        SeekerPositions.Dispose();
        NearestTargetPositions.Dispose();
    }

    public void Update()
    {
        
        //�� Seeker, Target���� Transform�� ������ NativeArray�� �����մϴ�.
        for (int i = 0; i < TargetPositions.Length; i++)
        {
            // Vector3->float3���� �Ͻ��� ��ȯ 
            TargetPositions[i] = Spawner.TargetTransforms[i].localPosition;
        }
        for (int i = 0; i < SeekerPositions.Length; i++)
        {
            SeekerPositions[i] = Spawner.SeekerTransforms[i].localPosition;
        }

        //���� �з��� ���� ���� ����� X�Ÿ��� Target�� ã���� FindNearestJob�� �����ϹǷ�, TargetPositions �迭�� X�� �������� �������־�� �մϴ�.
        //�� �����۾� ���� ���ķ� ó���ϱ� ���� SortJob ����ü�� �����մϴ�.
        //NativeArray�� SortJob �޼���� ���ڷ� ���� IComparer ����ü�� ������� �����۾��� �����ϴ� SortJob�� ��ȯ�մϴ�.
        SortJob<float3, AxisXComparer> sortJob = TargetPositions.SortJob(new AxisXComparer { });
        //SortJob ����ü�� Schedule �޼���� SegmentSort �۾��� SegmentSortMerge �۾��� �۾�ť�� �����մϴ�.
        //SegmentSort�� �迭�� ���� ���׸�Ʈ�� ���ķ� �����մϴ�.
        //SegmentSortMerge�� ���ķ� ���ĵ� �迭�� �� ���׸�Ʈ���� ��ġ�� �۾��� �մϴ�. ���� �ش� �۾��� SegmentSort �۾��� ���ӵ˴ϴ�.
        JobHandle sortHandle = sortJob.Schedule();

        //������ Transform ������ �������� FindNearestJob ����
        FindNearestJob findJob = new FindNearestJob
        {
            TargetPositions = TargetPositions,
            SeekerPositions = SeekerPositions,
            NearestTargetPositions = NearestTargetPositions,
        };
        //Schedule �޼���� Job�ν��Ͻ��� ����Ƽ �����ٷ� �۾� ť�� �����մϴ�.
        //FindNearestJob�� IJob �������̽��� ������ ��.
        //JobHandle findHandle = findJob.Schedule();
        //FindNearesetJob�� IJobParallelFor �������̽��� ������ ��.
        //�� ��� �۾� ť�� ����� ��, ��� ������ ������ ��ġ ����� �Ѱ��ݴϴ�.
        //������ 100�� ��ġ ������� ���Ƿ� �����Ͽ����ϴ�.
        //������ ���ڷ� JobHandle�� �ѱ�� �ش� �۾��� �ѱ� JobHandle�� �۾��� ���ӽ�ų �� �ֽ��ϴ�.
        //�̷��� �� ���, ���� Job�� ���ӵ� Job�� �۾��� ������ ������ ������� �ʽ��ϴ�.
        //�� ���, FindNearestJob�� �迭�� ������ ������ �̷������ �ϱ� ������ TargetPositions�� SortJob�� ���ӽ�ŵ�ϴ�.
        JobHandle findHandle = findJob.Schedule(SeekerPositions.Length, 100, sortHandle);
        //Complete �޼���� ����Ƽ �����췯�� ��ϵ� �ش� �۾��� �Ϸ�� �� ���� ��ٸ��ϴ�.
        //Job�ý��۰� BurstCompiler�� ���� ���� �����忡�� ������ ��� �������ϸ��� 130ms�� ����ð��� Ȯ��
        //Job�ý��۰� BurstCompiler�� ���� ���� �����忡�� ���ķ� ������ ��� �������ϸ��� 18.5ms�� ����ð��� Ȯ��
        //Job�ý��ۿ� �����з��� ����Ͽ� �˰����� ȿ��ȭ ��, ���� �˰����� ����ȭ�� �õ��� ��� �������ϸ��� 1.95ms�� ����ð��� Ȯ��
        findHandle.Complete();
        for (int i = 0; i < SeekerPositions.Length; i++)
        {
            //float3->Vector3 �Ͻ��� ����ȯ
            Debug.DrawLine(SeekerPositions[i], NearestTargetPositions[i]);
        }

        //�Ÿ��� ���Ͽ� Seeker�� ��ġ���� ���� ����� Target�� ��ġ�� ã�� �ǽð����� DebugLine�� ���� ����
        //��� Seeker�� ���� ��� Target�� loop : O(N^2)
        //�� ������ Mono���� ���� ��� CPU ���� �����忡�� ���ư��Ƿ� �ð��� �����ɸ� : �������ϸ��� 124ms�� ����ð� Ȯ��
        /*
        foreach (var seekerTransform in Spawner.SeekerTransforms)
        {
            Vector3 seekerPos = seekerTransform.localPosition;
            Vector3 nearestTargetPos = default;
            float nearestDistSq = float.MaxValue;
            foreach (var targetTransform in Spawner.TargetTransforms)
            {
                Vector3 offset = targetTransform.localPosition - seekerPos;
                //�Ÿ� �񱳽� ������ Ȱ���ϴ� ���� ������
                float distSq = offset.sqrMagnitude;

                if (distSq < nearestDistSq)
                {
                    nearestDistSq = distSq;
                    nearestTargetPos = targetTransform.localPosition;
                }
            }

            Debug.DrawLine(seekerPos, nearestTargetPos);
        }
        */
    }
}