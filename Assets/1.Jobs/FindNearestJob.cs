using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
//Vector3 ��� Unity.Mathematics.float3�� ���
//Vector3.sqrMagnitude ��� Unity.Mathematics.math.distancesq�� ���
using Unity.Mathematics;

//BurstCompiler�� ����մϴ�.
//BurstCompiler�� �� �÷����� �°� �ڵ带 ����ȭ�Ͽ� �������� �����մϴ�.
[BurstCompile]
//���� ó���� �ʿ��� �۾��� ��� IJobParallelFor, �׷��� ������� IJob �������̽��� ����
public struct FindNearestJob : IJobParallelFor
{
    //NativeArray�� Unity GC�� ���� �޸𸮰� �������� �ʽ��ϴ�.
    //Job�� BurstCompiler�� �����Ǵ� �޸𸮿� ������ �� �����Ƿ� �̸� ��� ����մϴ�.
    //Read
    [ReadOnly] public NativeArray<float3> TargetPositions;
    [ReadOnly] public NativeArray<float3> SeekerPositions;

    public NativeArray<float3> NearestTargetPositions;

    
    //Execute �޼���� IJob �������̽����� �����ؾ��ϴ� ������ �޼����
    //�۾� �����尡 IJob ��ü�� ������ �� �� �޼��带 �����ϰԵ˴ϴ�.
    //IJobParallelFor�� Execute �޼���� ���� index�� ���ڷ� �޾� ����˴ϴ�.
    //schedule �޼���� �۾� ť�� ��ϵ� ��, �ش� Job�� ��� ������ �� �����ϰ�, index�� �� ��° Job�� ���������� ��Ÿ���ϴ�.
    public void Execute(int index)
    {
        float3 seekerPos = SeekerPositions[index];
        //seekerPos�� �������� X��ǥ�� ���� ����� Target�� Transform �ε����� ����Ž������ ã���ϴ�. (O(logN))
        //BinarySearch �޼���� ����Ž������ �˻��� ���� ���� ������ �Ǵ� IComparer�� �޽��ϴ�.
        //BinarySearch�� ����Ž���̹Ƿ� TargetPositions �迭�� AxisXComparer�� ���� ���ĵǾ� �־�� ����� �۵��մϴ�.
        int startIdx = TargetPositions.BinarySearch(seekerPos, new AxisXComparer());

        //BinarySearch�� ��Ȯ�� value�� ã�� ������ ��� ���������� �˻��� �������� ��Ʈ�� ������ ��ȯ�մϴ�.
        //���� startIdx�� ������� �ش� ��Ʈ�� ������ ����� ������ݴϴ�.
        if (startIdx < 0) startIdx = ~startIdx;
        //startIdx�� �������� �ִ��� Ȯ���Ͽ� ArrayOutOfBoundary ���� ó���� ���ݴϴ�.
        if (startIdx >= TargetPositions.Length) startIdx = TargetPositions.Length - 1;

        //X��ǥ�� ���� ����� Target�� Transform�� �׶��� Seeker���� �Ÿ�(����)
        float3 nearestTargetPos = TargetPositions[startIdx];
        float nearestDistSq = math.distancesq(seekerPos, nearestTargetPos);

        //�迭�� ������ �˻��Ͽ� ��ǥ�� �� ����� Target�� ã���ϴ�.(O(N/2))
        Search(seekerPos, startIdx + 1, TargetPositions.Length, +1, ref nearestTargetPos, ref nearestDistSq);
        //�迭�� ������ �˻��Ͽ� ��ǥ�� �� ����� Target�� ã���ϴ�.(O(N/2))
        Search(seekerPos, startIdx - 1, -1, -1, ref nearestTargetPos, ref nearestDistSq);

        NearestTargetPositions[index] = nearestTargetPos;
    }

    //IJob �������̽��� �����Ͽ��� ��� Execute �޼���
    public void Execute()
    {
        for (int i = 0; i < SeekerPositions.Length; i++)
        {
            float3 seekerPos = SeekerPositions[i];
            float nearestDistSq = float.MaxValue;
            for (int j = 0; j < TargetPositions.Length; j++)
            {
                float3 targetPos = TargetPositions[j];
                float distSq = math.distancesq(seekerPos, targetPos);
                if (distSq < nearestDistSq)
                {
                    nearestDistSq = distSq;
                    NearestTargetPositions[i] = targetPos;
                }
            }
        }
    }
    void Search(float3 seekerPos, int startIdx, int endIdx, int step, ref float3 nearestTargetPos, ref float nearestDistSq)
    {
        for (int i = startIdx; i != endIdx; i += step)
        {
            float3 targetPos = TargetPositions[i];
            float xdiff = seekerPos.x - targetPos.x;

            // x�Ÿ��� ������ ���� ���� ����� �Ÿ����� ũ�� �˻��� �����մϴ�.(x�Ÿ��� �� �� Target�� xy�Ÿ��� �� ����� ���ɼ��� �����Ƿ�)
            if ((xdiff * xdiff) > nearestDistSq) break;

            float distSq = math.distancesq(targetPos, seekerPos);

            // xy�Ÿ��� nearestDistSq���� ����� ��� nearestDistSq�� nearestTargetPos�� ������Ʈ
            if (distSq < nearestDistSq)
            {
                nearestDistSq = distSq;
                nearestTargetPos = targetPos;
            }
        }
    }
}
public struct AxisXComparer : IComparer<float3>
{
    public int Compare(float3 a, float3 b)
    {
        return a.x.CompareTo(b.x);
    }
}