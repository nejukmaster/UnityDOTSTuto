
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
        //NativeArray의 생성자에서는 할당할 크기와 메모리 할당 방식을 인자로 받습니다.
        //여기서는 생성한 NativeArray의 장시간 유지가 필요하므로 Persistent로 할당합니다.
        //이외에도 Temp(한 프레임 유지), TempJob(Job시스템 전용 한 프레임)이 있습니다.
        TargetPositions = new NativeArray<float3>(spawner.NumTargets, Allocator.Persistent);
        SeekerPositions = new NativeArray<float3>(spawner.NumSeekers, Allocator.Persistent);
        NearestTargetPositions = new NativeArray<float3>(spawner.NumSeekers, Allocator.Persistent);
    }

    public void OnDestroy()
    {
        //NativeArray는 GC에 의해 관리되지 않기 때문에, Persistent로 할당한 경우 Dispose 메서드를 통해 해제해주어야합니다.
        TargetPositions.Dispose();
        SeekerPositions.Dispose();
        NearestTargetPositions.Dispose();
    }

    public void Update()
    {
        
        //각 Seeker, Target들의 Transform을 생성한 NativeArray로 복사합니다.
        for (int i = 0; i < TargetPositions.Length; i++)
        {
            // Vector3->float3로의 암시적 변환 
            TargetPositions[i] = Spawner.TargetTransforms[i].localPosition;
        }
        for (int i = 0; i < SeekerPositions.Length; i++)
        {
            SeekerPositions[i] = Spawner.SeekerTransforms[i].localPosition;
        }

        //이진 분류를 통해 가장 가까운 X거리의 Target을 찾도록 FindNearestJob이 지시하므로, TargetPositions 배열을 X축 기준으로 정렬해주어야 합니다.
        //이 정렬작업 또한 병렬로 처리하기 위해 SortJob 구조체로 수행합니다.
        //NativeArray의 SortJob 메서드는 인자로 받은 IComparer 구조체를 기반으로 정렬작업을 수행하는 SortJob을 반환합니다.
        SortJob<float3, AxisXComparer> sortJob = TargetPositions.SortJob(new AxisXComparer { });
        //SortJob 구조체의 Schedule 메서드는 SegmentSort 작업과 SegmentSortMerge 작업을 작업큐에 삽입합니다.
        //SegmentSort는 배열의 개별 세그먼트를 병렬로 정렬합니다.
        //SegmentSortMerge는 병렬로 정렬된 배열의 각 세그먼트들을 합치는 작업을 합니다. 따라서 해당 작업은 SegmentSort 작업에 종속됩니다.
        JobHandle sortHandle = sortJob.Schedule();

        //복사한 Transform 정보를 바탕으로 FindNearestJob 생성
        FindNearestJob findJob = new FindNearestJob
        {
            TargetPositions = TargetPositions,
            SeekerPositions = SeekerPositions,
            NearestTargetPositions = NearestTargetPositions,
        };
        //Schedule 메서드는 Job인스턴스를 유니티 스케줄러 작업 큐에 삽입합니다.
        //FindNearestJob이 IJob 인터페이스를 구현할 때.
        //JobHandle findHandle = findJob.Schedule();
        //FindNearesetJob이 IJobParallelFor 인터페이스를 구현할 때.
        //이 경우 작업 큐에 등록할 때, 몇번 실행할 건지와 배치 사이즈를 넘겨줍니다.
        //설정한 100의 배치 사이즈는 임의로 설정하였습니다.
        //마지막 인자로 JobHandle을 넘기면 해당 작업을 넘긴 JobHandle의 작업에 종속시킬 수 있습니다.
        //이렇게 할 경우, 현재 Job은 종속된 Job의 작업이 끝나기 전까지 실행되지 않습니다.
        //이 경우, FindNearestJob은 배열의 정렬이 끝난후 이루어져야 하기 때문에 TargetPositions의 SortJob에 종속시킵니다.
        JobHandle findHandle = findJob.Schedule(SeekerPositions.Length, 100, sortHandle);
        //Complete 메서드는 유니티 스케쥴러에 등록된 해당 작업이 완료될 때 까지 기다립니다.
        //Job시스템과 BurstCompiler를 통해 단일 스레드에서 실행한 결과 프로파일링시 130ms의 실행시간을 확인
        //Job시스템과 BurstCompiler를 통해 다중 스레드에서 병렬로 실행한 결과 프로파일링시 18.5ms의 실행시간을 확인
        //Job시스템에 이진분류를 사용하여 알고리즘을 효율화 및, 정렬 알고리즘의 병렬화를 시도한 결과 프로파일링시 1.95ms의 실행시간을 확인
        findHandle.Complete();
        for (int i = 0; i < SeekerPositions.Length; i++)
        {
            //float3->Vector3 암시적 형변환
            Debug.DrawLine(SeekerPositions[i], NearestTargetPositions[i]);
        }

        //거리를 비교하여 Seeker의 위치에서 가장 가까운 Target의 위치를 찾아 실시간으로 DebugLine을 통해 연결
        //모든 Seeker에 대해 모든 Target을 loop : O(N^2)
        //이 로직을 Mono에서 돌릴 경우 CPU 단일 스레드에서 돌아가므로 시간이 오래걸림 : 프로파일링시 124ms의 실행시간 확인
        /*
        foreach (var seekerTransform in Spawner.SeekerTransforms)
        {
            Vector3 seekerPos = seekerTransform.localPosition;
            Vector3 nearestTargetPos = default;
            float nearestDistSq = float.MaxValue;
            foreach (var targetTransform in Spawner.TargetTransforms)
            {
                Vector3 offset = targetTransform.localPosition - seekerPos;
                //거리 비교시 제곱을 활용하는 편이 경제적
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