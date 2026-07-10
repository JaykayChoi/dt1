using System;
using System.Collections.Generic;

namespace SimStats
{
    /// <summary>
    /// 반복 실험 표본을 평균과 95% 신뢰구간 반폭으로 요약한다.
    /// Phase 6 출력 분석의 보고 형식(20회 반복, 평균 ± t·s/√n)을 코드로 담아
    /// Phase 6·7 콘솔 랩이 공유한다.
    /// </summary>
    public sealed class ExperimentStats
    {
        // t(0.975, df) 소표. 인덱스 = df(1..29). df >= 30이면 정규 근사 1.96.
        private static readonly double[] TCritical95 =
        {
            0.0,
            12.706, 4.303, 3.182, 2.776, 2.571, 2.447, 2.365, 2.306, 2.262, 2.228,
            2.201, 2.179, 2.160, 2.145, 2.131, 2.120, 2.110, 2.101, 2.093, 2.086,
            2.080, 2.074, 2.069, 2.064, 2.060, 2.056, 2.052, 2.048, 2.045,
        };

        /// <summary>표본 요약 결과 — 평균과 신뢰구간 반폭.</summary>
        public readonly struct Summary
        {
            /// <summary>표본 평균.</summary>
            public double Mean { get; }

            /// <summary>95% 신뢰구간 반폭 (평균 ± 이 값).</summary>
            public double Half95 { get; }

            public Summary(double mean, double half95)
            {
                Mean = mean;
                Half95 = half95;
            }

            /// <summary>"평균 ± 반폭" 형식 문자열.</summary>
            public string Format(string numberFormat)
            {
                return $"{Mean.ToString(numberFormat)} ± {Half95.ToString(numberFormat)}";
            }
        }

        /// <summary>표본을 평균과 95% 신뢰구간 반폭으로 요약한다.</summary>
        public static Summary Summarize(IReadOnlyList<double> samples)
        {
            int n = samples.Count;
            if (n == 0)
            {
                return new Summary(0.0, 0.0);
            }

            double sum = 0.0;
            foreach (double x in samples)
            {
                sum += x;
            }

            double mean = sum / n;
            if (n < 2)
            {
                return new Summary(mean, 0.0);
            }

            double squaredDeviation = 0.0;
            foreach (double x in samples)
            {
                double d = x - mean;
                squaredDeviation += d * d;
            }

            double stdDev = Math.Sqrt(squaredDeviation / (n - 1));
            int df = n - 1;
            double t = df < TCritical95.Length ? TCritical95[df] : 1.96;
            double half = t * stdDev / Math.Sqrt(n);
            return new Summary(mean, half);
        }

        /// <summary>선형 보간으로 p 백분위수(0~100)를 구한다.</summary>
        public static double Percentile(IReadOnlyList<double> samples, double p)
        {
            int n = samples.Count;
            if (n == 0)
            {
                return 0.0;
            }

            if (n == 1)
            {
                return samples[0];
            }

            var sorted = new List<double>(samples);
            sorted.Sort();

            double rank = p / 100.0 * (n - 1);
            int low = (int)Math.Floor(rank);
            int high = (int)Math.Ceiling(rank);
            if (low == high)
            {
                return sorted[low];
            }

            double weight = rank - low;
            return sorted[low] * (1.0 - weight) + sorted[high] * weight;
        }
    }
}
