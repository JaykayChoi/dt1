using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TwinBridge.ReplayServer
{
    /// <summary>
    /// 실습용 NdjsonPacer 스켈레톤. 파일 읽기·라인 송출·드롭 주입은 채워져 있고, 라인 간
    /// 지연을 계산하는 ComputeDelay만 TODO 스텁이다. 스텁은 0을 반환해 전체 로그가 지연 없이
    /// 한꺼번에 쏟아진다(페이싱 미구현이 드러남). 정답지는
    /// examples/twin-bridge/ReplayServer/NdjsonPacer.cs다.
    /// </summary>
    public sealed class NdjsonPacer
    {
        private readonly string logPath;
        private readonly double speed;
        private readonly double dropProbability;
        private readonly Random dropRandom;

        public NdjsonPacer(string logPath, double speed, double dropProbability, int dropSeed)
        {
            this.logPath = logPath;
            this.speed = speed <= 0.0 ? 1.0 : speed;
            this.dropProbability = dropProbability;
            dropRandom = new Random(dropSeed);
        }

        /// <summary>취소될 때까지 라인을 페이싱하며 writer에 쓴다.</summary>
        public async Task StreamAsync(TextWriter writer, CancellationToken token)
        {
            double prevT = 0.0;
            bool first = true;
            foreach (string line in File.ReadLines(logPath))
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                double t = ExtractTimestamp(line);
                if (!first)
                {
                    double delaySeconds = ComputeDelay(prevT, t, speed);
                    if (delaySeconds > 0.0)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), token);
                    }
                }

                first = false;
                prevT = t;

                if (dropProbability > 0.0
                    && !line.Contains("\"HELLO\"") && !line.Contains("\"SNAPSHOT\"")
                    && dropRandom.NextDouble() < dropProbability)
                {
                    continue;
                }

                await writer.WriteAsync(line);
                await writer.WriteAsync('\n');
                await writer.FlushAsync();
            }
        }

        /// <summary>직전 라인과의 시각 차를 배속으로 나눈 대기 시간[초]. 음수는 0으로.</summary>
        public static double ComputeDelay(double prevT, double curT, double speed)
        {
            // TODO(실습 1): (curT - prevT) / speed 초만큼 대기하도록 지연을 계산한다(음수는 0).
            // 스텁 동작: 0을 반환 → 페이싱 없이 전체 로그가 한꺼번에 쏟아진다(미완성).
            return 0.0;
        }

        private static double ExtractTimestamp(string line)
        {
            int key = line.IndexOf("\"t\":", StringComparison.Ordinal);
            if (key < 0)
            {
                return 0.0;
            }

            int start = key + 4;
            int end = start;
            while (end < line.Length && (char.IsDigit(line[end]) || line[end] == '.' || line[end] == '-'))
            {
                end++;
            }

            return double.TryParse(line.Substring(start, end - start),
                NumberStyles.Float, CultureInfo.InvariantCulture, out double value)
                ? value
                : 0.0;
        }
    }
}
