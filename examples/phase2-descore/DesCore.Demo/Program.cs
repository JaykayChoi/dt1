using System;
using System.Collections.Generic;
using System.Text;

namespace DesCore.Demo
{
    /// <summary>
    /// 미니 반송 라인 데모 — SimPy 예제 02(02_transport_line.py)와 같은 시나리오.
    /// 스테이션 A→B→C를 거치고, 공정 사이 이동은 반송차 2대가 맡는다.
    /// 서로 다른 두 구현(SimPy 코루틴 / DesCore 콜백)이 비슷한 통계를 내는지 비교한다.
    /// </summary>
    internal static class Program
    {
        private const double ArrivalMean = 6.0;
        private const double TravelTime = 2.0;
        private const int VehicleCount = 2;
        private const double SimTime = 2000.0;
        private const int RandomSeed = 7;
        private const int LogLots = 2;

        private static readonly string[] StageNames = { "A", "B", "C" };
        private static readonly double[] ServiceMeans = { 4.0, 5.0, 3.0 };

        private static Simulation simulation;
        private static Random random;
        private static SimResource[] stations;
        private static SimResource vehicles;
        private static int nextLotId;
        private static readonly List<double> leadTimes = new List<double>();
        private static readonly List<double>[] stationWaits =
        {
            new List<double>(), new List<double>(), new List<double>(),
        };
        private static readonly List<double> transportWaits = new List<double>();

        private sealed class Lot
        {
            public int Id;
            public double BornAt;
            public double QueuedAt;
        }

        private static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            simulation = new Simulation();
            random = new Random(RandomSeed);
            stations = new SimResource[StageNames.Length];
            for (int i = 0; i < StageNames.Length; i++)
            {
                stations[i] = new SimResource(simulation, StageNames[i], 1);
            }

            vehicles = new SimResource(simulation, "vehicle", VehicleCount);

            ScheduleNextArrival();
            simulation.Run(SimTime);
            PrintStatistics();
        }

        private static void ScheduleNextArrival()
        {
            simulation.Schedule(random.NextExponential(ArrivalMean), () =>
            {
                var lot = new Lot { Id = nextLotId++, BornAt = simulation.Now };
                Log(lot, "투입");
                EnterStation(lot, 0);
                ScheduleNextArrival();
            });
        }

        private static void EnterStation(Lot lot, int stage)
        {
            lot.QueuedAt = simulation.Now;
            stations[stage].Request(() => BeginService(lot, stage));
        }

        private static void BeginService(Lot lot, int stage)
        {
            double waited = simulation.Now - lot.QueuedAt;
            stationWaits[stage].Add(waited);
            Log(lot, $"{StageNames[stage]} 가공 시작 (대기 {waited:F1}분)");
            double service = random.NextExponential(ServiceMeans[stage]);
            simulation.Schedule(service, () => FinishService(lot, stage));
        }

        private static void FinishService(Lot lot, int stage)
        {
            stations[stage].Release();
            Log(lot, $"{StageNames[stage]} 가공 완료");
            if (stage == StageNames.Length - 1)
            {
                CompleteLot(lot);
            }
            else
            {
                RequestTransport(lot, stage);
            }
        }

        private static void RequestTransport(Lot lot, int stage)
        {
            lot.QueuedAt = simulation.Now;
            vehicles.Request(() => BeginTravel(lot, stage));
        }

        private static void BeginTravel(Lot lot, int stage)
        {
            double waited = simulation.Now - lot.QueuedAt;
            transportWaits.Add(waited);
            Log(lot, $"{StageNames[stage]}→{StageNames[stage + 1]} 반송 시작 (배차 대기 {waited:F1}분)");
            simulation.Schedule(TravelTime, () =>
            {
                vehicles.Release();
                EnterStation(lot, stage + 1);
            });
        }

        private static void CompleteLot(Lot lot)
        {
            double leadTime = simulation.Now - lot.BornAt;
            leadTimes.Add(leadTime);
            Log(lot, $"완성 (리드타임 {leadTime:F1}분)");
        }

        private static void Log(Lot lot, string message)
        {
            if (lot.Id < LogLots)
            {
                Console.WriteLine($"[t={simulation.Now,7:F1}] lot{lot.Id:D3} {message}");
            }
        }

        private static void PrintStatistics()
        {
            Console.WriteLine();
            Console.WriteLine($"실행 사건 수        : {simulation.ExecutedCount:N0}");
            Console.WriteLine($"완성 로트 수        : {leadTimes.Count}");
            Console.WriteLine($"처리량              : {leadTimes.Count / SimTime * 60.0:F1} 로트/시간");
            Console.WriteLine($"평균 리드타임       : {Average(leadTimes):F1} 분");
            Console.WriteLine($"평균 반송 배차 대기 : {Average(transportWaits):F2} 분");
            Console.WriteLine($"반송차 가동률       : {vehicles.GetUtilization():F3}");
            Console.WriteLine();
            Console.WriteLine("스테이션    가동률    평균 큐 대기 [분]");
            for (int i = 0; i < StageNames.Length; i++)
            {
                Console.WriteLine(
                    $"    {StageNames[i]}      {stations[i].GetUtilization(),6:F3}    " +
                    $"{Average(stationWaits[i]),8:F1}");
            }
        }

        private static double Average(List<double> values)
        {
            if (values.Count == 0)
            {
                return 0.0;
            }

            double sum = 0.0;
            foreach (double value in values)
            {
                sum += value;
            }

            return sum / values.Count;
        }
    }
}
