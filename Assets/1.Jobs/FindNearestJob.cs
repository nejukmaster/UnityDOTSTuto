using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
//Vector3 대신 Unity.Mathematics.float3을 사용
//Vector3.sqrMagnitude 대신 Unity.Mathematics.math.distancesq를 사용
using Unity.Mathematics;

//BurstCompiler를 사용합니다.
//BurstCompiler는 각 플랫폼에 맞게 코드를 최적화하여 컴파일을 수행합니다.
[BurstCompile]
//병렬 처리가 필요한 작업의 경우 IJobParallelFor, 그렇지 않은경우 IJob 인터페이스를 구현
public struct FindNearestJob : IJobParallelFor
{
    //NativeArray는 Unity GC에 의해 메모리가 관리되지 않습니다.
    //Job과 BurstCompiler는 관리되는 메모리에 접근할 수 없으므로 이를 대신 사용합니다.
    //Read
    [ReadOnly] public NativeArray<float3> TargetPositions;
    [ReadOnly] public NativeArray<float3> SeekerPositions;

    public NativeArray<float3> NearestTargetPositions;

    
    //Execute 메서드는 IJob 인터페이스에서 구현해야하는 유일한 메서드로
    //작업 스레드가 IJob 객체를 실행할 때 이 메서드를 실행하게됩니다.
    //IJobParallelFor의 Execute 메서드는 정수 index를 인자로 받아 실행됩니다.
    //schedule 메서드로 작업 큐에 등록될 때, 해당 Job을 몇번 실행할 지 설정하고, index는 몇 번째 Job의 실행인지를 나타냅니다.
    public void Execute(int index)
    {
        float3 seekerPos = SeekerPositions[index];
        //seekerPos를 기준으로 X좌표가 가장 가까운 Target의 Transform 인덱스를 이진탐색으로 찾습니다. (O(logN))
        //BinarySearch 메서드는 이진탐색에서 검색할 값과 정렬 기준이 되는 IComparer를 받습니다.
        //BinarySearch는 이진탐색이므로 TargetPositions 배열이 AxisXComparer에 의해 정렬되어 있어야 제대로 작동합니다.
        int startIdx = TargetPositions.BinarySearch(seekerPos, new AxisXComparer());

        //BinarySearch가 정확한 value를 찾지 못했을 경우 마지막으로 검색한 오프셋의 비트의 부정을 반환합니다.
        //따라서 startIdx가 음수라면 해당 비트를 뒤집어 양수로 만들어줍니다.
        if (startIdx < 0) startIdx = ~startIdx;
        //startIdx가 범위내에 있는지 확인하여 ArrayOutOfBoundary 예외 처리를 해줍니다.
        if (startIdx >= TargetPositions.Length) startIdx = TargetPositions.Length - 1;

        //X좌표가 가장 가까운 Target의 Transform과 그때의 Seeker와의 거리(제곱)
        float3 nearestTargetPos = TargetPositions[startIdx];
        float nearestDistSq = math.distancesq(seekerPos, nearestTargetPos);

        //배열의 뒤쪽을 검색하여 좌표가 더 가까운 Target을 찾습니다.(O(N/2))
        Search(seekerPos, startIdx + 1, TargetPositions.Length, +1, ref nearestTargetPos, ref nearestDistSq);
        //배열의 앞쪽을 검색하여 좌표가 더 가까운 Target을 찾습니다.(O(N/2))
        Search(seekerPos, startIdx - 1, -1, -1, ref nearestTargetPos, ref nearestDistSq);

        NearestTargetPositions[index] = nearestTargetPos;
    }

    //IJob 인터페이스를 구현하였을 경우 Execute 메서드
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

            // x거리의 제곱이 현재 가장 가까운 거리보다 크면 검색을 중지합니다.(x거리가 더 먼 Target이 xy거리가 더 가까울 가능성이 없으므로)
            if ((xdiff * xdiff) > nearestDistSq) break;

            float distSq = math.distancesq(targetPos, seekerPos);

            // xy거리가 nearestDistSq보다 가까운 경우 nearestDistSq와 nearestTargetPos를 업데이트
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