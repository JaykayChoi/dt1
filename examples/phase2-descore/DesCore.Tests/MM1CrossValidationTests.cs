using System;
using System.Collections.Generic;
using NUnit.Framework;
using DesCore;

namespace DesCore.Tests
{
    /// <summary>
    /// M/M/1 대기행렬을 DesCore로 시뮬레이션해, 큐 대기시간 Wq의 표본이
    /// 이론값 ρ/(μ−λ)를 95% 신뢰구간으로 감싸는지 검정한다. 엔진이 알려진
    /// 정답을 재현함을 보이는 통계적 Verification이며, 시드를 고정해 결정적이다.
    /// </summary>
    [TestFixture]
    public sealed class MM1CrossValidationTests
    {
        private const double ArrivalRate = 0.5;         // λ
        private const double ServiceRate = 1.0;         // μ
        private const int CustomersPerReplication = 60000;
        private const int WarmUpCustomers = 5000;
        private const int Replications = 40;            // 시드 1..40
        private const double TCritical95Df39 = 2.0227;  // t(0.975, 39)

        [Test]
        public void MeanWaitInQueueMatchesTheoreticalWithin95CI()
        {
            double rho = ArrivalRate / ServiceRate;
            double theoreticalWq = rho / (ServiceRate - ArrivalRate);

            var replicationMeans = new List<double>(Replications);
            for (int seed = 1; seed <= Replications; seed++)
            {
                replicationMeans.Add(RunReplication(seed));
            }

            (double mean, double half) = MeanCi95(replicationMeans, TCritical95Df39);
            double low = mean - half;
            double high = mean + half;

            Assert.That(theoreticalWq, Is.GreaterThanOrEqualTo(low),
                $"이론값 Wq={theoreticalWq:F4}가 CI 하한 {low:F4}보다 작다.");
            Assert.That(theoreticalWq, Is.LessThanOrEqualTo(high),
                $"이론값 Wq={theoreticalWq:F4}가 CI 상한 {high:F4}보다 크다.");
        }

        /// <summary>단일 반복을 돌려 warm-up 이후 손님들의 평균 큐 대기시간을 반환한다.</summary>
        private static double RunReplication(int seed)
        {
            var simulation = new Simulation();
            var server = new SimResource(simulation, "server", 1);
            var random = new Random(seed);

            double waitSum = 0.0;
            int recordedCount = 0;
            int arrivedCount = 0;

            Action scheduleArrival = null;
            scheduleArrival = () =>
            {
                simulation.Schedule(random.NextExponential(1.0 / ArrivalRate), () =>
                {
                    int index = arrivedCount;
                    arrivedCount++;
                    double arrivalTime = simulation.Now;

                    server.Request(() =>
                    {
                        if (index >= WarmUpCustomers)
                        {
                            waitSum += simulation.Now - arrivalTime;
                            recordedCount++;
                        }

                        simulation.Schedule(random.NextExponential(1.0 / ServiceRate), () => server.Release());
                    });

                    if (arrivedCount < CustomersPerReplication)
                    {
                        scheduleArrival();
                    }
                });
            };

            scheduleArrival();
            simulation.Run(double.MaxValue);

            return recordedCount > 0 ? waitSum / recordedCount : 0.0;
        }

        /// <summary>표본의 평균과 95% 신뢰구간 반폭(t·s/√n)을 반환한다.</summary>
        private static (double mean, double half) MeanCi95(IReadOnlyList<double> xs, double tCritical)
        {
            int n = xs.Count;
            double sum = 0.0;
            foreach (double x in xs)
            {
                sum += x;
            }

            double mean = sum / n;

            double squaredDeviation = 0.0;
            foreach (double x in xs)
            {
                double d = x - mean;
                squaredDeviation += d * d;
            }

            double stdDev = Math.Sqrt(squaredDeviation / (n - 1));
            double half = tCritical * stdDev / Math.Sqrt(n);
            return (mean, half);
        }
    }
}
