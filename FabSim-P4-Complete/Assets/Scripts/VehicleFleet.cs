using UnityEngine;

namespace FabSim
{
    /// <summary>
    /// 비히클 무리 전체를 매니저 한 개가 일괄 이동시킨다.
    /// 오브젝트마다 Update를 두는 대신 배열 순회 한 번 — 호출 오버헤드와
    /// GC 할당이 사라진다. Phase 4 교재의 "관리 주체를 하나로" 원칙의 구현.
    /// </summary>
    public sealed class VehicleFleet : MonoBehaviour
    {
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

        private float[] offsets;
        private float perimeter;

        private void Start()
        {
            perimeter = 4f * (halfWidth + halfLength);
            offsets = new float[vehicles.Length];
            for (int i = 0; i < vehicles.Length; i++)
            {
                offsets[i] = perimeter / vehicles.Length * i;
            }
        }

        private void Update()
        {
            // 문자열도, 새 배열도, 박싱도 없다 — 이 루프의 GC Alloc은 0이어야 한다.
            // Profiler의 GC Alloc 열에서 직접 확인해 볼 것.
            float travel = Time.time * speed;
            for (int i = 0; i < vehicles.Length; i++)
            {
                Transform vehicle = vehicles[i];
                float distance = Mathf.Repeat(travel + offsets[i], perimeter);
                Vector3 position = vehicle.localPosition;
                vehicle.localPosition = EvaluateTrack(distance, position.y);
            }
        }

        /// <summary>씬 빌더가 트랙 파라미터와 차량 목록을 주입한다.</summary>
        public void Initialize(Transform[] fleet, float trackHalfWidth, float trackHalfLength)
        {
            vehicles = fleet;
            halfWidth = trackHalfWidth;
            halfLength = trackHalfLength;
        }

        private Vector3 EvaluateTrack(float distance, float height)
        {
            // 직사각형 순환 트랙 위의 둘레 거리 → 좌표.
            float w = halfWidth * 2f;
            float l = halfLength * 2f;
            if (distance < l)
            {
                return new Vector3(-halfWidth, height, -halfLength + distance);
            }

            distance -= l;
            if (distance < w)
            {
                return new Vector3(-halfWidth + distance, height, halfLength);
            }

            distance -= w;
            if (distance < l)
            {
                return new Vector3(halfWidth, height, halfLength - distance);
            }

            distance -= l;
            return new Vector3(halfWidth - distance, height, -halfLength);
        }
    }
}
