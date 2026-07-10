using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TwinBridge.ReplayServer
{
    /// <summary>
    /// NDJSON 로그를 타임스탬프 간격 × 배속으로 라이터에 흘려보낸다. HELLO는 즉시,
    /// 이후는 직전 라인 t와의 차 (t_i − t_{i-1}) / speed 초만큼 지연해 실물처럼 재생한다.
    /// 파싱 없이 라인을 그대로 재전송하되, t만 뽑아 페이싱한다.
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

                // 고장 주입: 델타 라인만 확률적으로 드롭한다(HELLO·SNAPSHOT은 재동기 기준이라 보존).
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
            double delta = (curT - prevT) / speed;
            return delta > 0.0 ? delta : 0.0;
        }

        // 라인에서 "t":<number> 를 뽑는다(파싱 없이 문자열 스캔).
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
