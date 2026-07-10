using System;

namespace DesCore
{
    /// <summary>
    /// 확률분포 샘플링 확장 메서드.
    /// </summary>
    public static class RandomExtensions
    {
        /// <summary>
        /// 평균이 mean인 지수분포에서 표본 하나를 뽑는다.
        /// 포아송 과정의 사건 간 간격이 이 분포를 따른다.
        /// </summary>
        public static double NextExponential(this Random random, double mean)
        {
            // 역변환 샘플링: U ~ (0, 1] 에 대해 -mean * ln(U).
            double u = 1.0 - random.NextDouble();
            return -mean * Math.Log(u);
        }
    }
}
