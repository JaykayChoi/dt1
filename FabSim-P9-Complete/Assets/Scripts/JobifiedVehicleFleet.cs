using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Jobs;

namespace FabSim
{
    /// <summary>
    /// VehicleFleet의 대량 이동을 C# Job System + Burst로 워커 스레드에 분산한다. Phase 4의
    /// "호출 96번 → 1번"이 "코어 1개 → N개"로 올라선 것. useJobs를 끄면 원본과 동일한 매니저
    /// 루프로 되돌려 측정 비교에 쓴다(두 경로가 정확히 같은 위치를 낸다).
    /// </summary>
    public sealed class JobifiedVehicleFleet : MonoBehaviour
    {
        private static readonly ProfilerMarker MoveMarker = new ProfilerMarker("JobifiedVehicleFleet.Move");

        [Tooltip("이동시킬 비히클 트랜스폼들. 씬 빌더가 채운다.")]
        [SerializeField]
        private Transform[] vehicles;

        [Tooltip("주행 속도 [m/s].")]
        [SerializeField]
        private float speed = 6f;

        [Tooltip("순환 트랙의 X 반폭 [m].")]
        [SerializeField]
        private float halfWidth = 20f;

        [Tooltip("순환 트랙의 Z 반길이 [m].")]
        [SerializeField]
        private float halfLength = 30f;

        [Tooltip("켜면 잡 경로(워커 스레드), 끄면 기존 매니저 루프(메인 스레드).")]
        [SerializeField]
        private bool useJobs = true;

        private NativeArray<float> offsets;
        private TransformAccessArray transformAccess;
        private float perimeter;

        private void Start()
        {
            perimeter = 4f * (halfWidth + halfLength);
            offsets = new NativeArray<float>(vehicles.Length, Allocator.Persistent);
            for (int i = 0; i < vehicles.Length; i++)
            {
                offsets[i] = perimeter / vehicles.Length * i;
            }

            transformAccess = new TransformAccessArray(vehicles);
        }

        private void Update()
        {
            using (MoveMarker.Auto())
            {
                float travel = Time.time * speed;
                if (useJobs)
                {
                    var job = new MoveJob
                    {
                        Offsets = offsets,
                        Travel = travel,
                        Perimeter = perimeter,
                        HalfWidth = halfWidth,
                        HalfLength = halfLength,
                    };
                    JobHandle handle = job.Schedule(transformAccess);
                    handle.Complete();
                }
                else
                {
                    for (int i = 0; i < vehicles.Length; i++)
                    {
                        Transform vehicle = vehicles[i];
                        float distance = TrackMath.Repeat(travel + offsets[i], perimeter);
                        TrackMath.EvaluateTrack(
                            distance, vehicle.localPosition.y, halfWidth, halfLength,
                            out float x, out float y, out float z);
                        vehicle.localPosition = new Vector3(x, y, z);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (offsets.IsCreated)
            {
                offsets.Dispose();
            }

            if (transformAccess.isCreated)
            {
                transformAccess.Dispose();
            }
        }

        /// <summary>씬 빌더가 트랙 파라미터와 차량 목록을 주입한다.</summary>
        public void Initialize(Transform[] fleet, float trackHalfWidth, float trackHalfLength)
        {
            vehicles = fleet;
            halfWidth = trackHalfWidth;
            halfLength = trackHalfLength;
        }

        [BurstCompile]
        private struct MoveJob : IJobParallelForTransform
        {
            [ReadOnly] public NativeArray<float> Offsets;
            public float Travel;
            public float Perimeter;
            public float HalfWidth;
            public float HalfLength;

            public void Execute(int index, TransformAccess transform)
            {
                float distance = TrackMath.Repeat(Travel + Offsets[index], Perimeter);
                TrackMath.EvaluateTrack(
                    distance, transform.localPosition.y, HalfWidth, HalfLength,
                    out float x, out float y, out float z);
                transform.localPosition = new Vector3(x, y, z);
            }
        }
    }
}
