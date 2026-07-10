namespace FabSim
{
    /// <summary>
    /// 순환 트랙의 둘레 거리 → 좌표 계산. UnityEngine 무의존 순수 수학이라 Burst 잡에서도,
    /// 헤드리스 테스트에서도 그대로 쓴다. VehicleFleet.EvaluateTrack과 동일한 직사각 4구간 로직.
    /// </summary>
    public static class TrackMath
    {
        /// <summary>양수 length에 대한 나머지(0 이상 length 미만). Mathf.Repeat의 순수판.</summary>
        public static float Repeat(float value, float length)
        {
            float wrapped = value - Floor(value / length) * length;
            return wrapped < 0f ? wrapped + length : wrapped;
        }

        /// <summary>
        /// 직사각 순환 트랙 위의 둘레 거리 distance(0..둘레)에서의 좌표를 (x, y, z)로 낸다.
        /// y는 입력 height 그대로. 구간 순서: 왼변(l) → 위변(w) → 오른변(l) → 아래변(w).
        /// </summary>
        public static void EvaluateTrack(
            float distance, float height, float halfWidth, float halfLength,
            out float x, out float y, out float z)
        {
            y = height;
            float w = halfWidth * 2f;
            float l = halfLength * 2f;

            if (distance < l)
            {
                x = -halfWidth;
                z = -halfLength + distance;
                return;
            }

            distance -= l;
            if (distance < w)
            {
                x = -halfWidth + distance;
                z = halfLength;
                return;
            }

            distance -= w;
            if (distance < l)
            {
                x = halfWidth;
                z = halfLength - distance;
                return;
            }

            distance -= l;
            x = halfWidth - distance;
            z = -halfLength;
        }

        private static float Floor(float value)
        {
            int i = (int)value;
            return value < i ? i - 1 : i;
        }
    }
}
