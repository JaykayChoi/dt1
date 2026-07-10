using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TwinBridge.ReplayServer
{
    /// <summary>
    /// 리플레이 TCP 서버 — NDJSON 로그를 타임스탬프 간격 × 배속으로 접속한 클라이언트에게
    /// 흘려보낸다. 고장 주입(--drop 확률 드롭 · --cut 시각에 연결 강제 종료)으로 "화면이
    /// 실물과 어긋나는" sync 상황을 재현할 수 있다.
    /// </summary>
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            string file = ArgValue(args, "--file", "sample.ndjson");
            int port = int.Parse(ArgValue(args, "--port", "5088"));
            double speed = double.Parse(ArgValue(args, "--speed", "30"), CultureInfo.InvariantCulture);
            bool loop = HasFlag(args, "--loop");
            double drop = double.Parse(ArgValue(args, "--drop", "0"), CultureInfo.InvariantCulture);
            double cut = double.Parse(ArgValue(args, "--cut", "0"), CultureInfo.InvariantCulture);

            if (!File.Exists(file))
            {
                Console.Error.WriteLine($"로그 파일 없음: {file}");
                return;
            }

            var listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            Console.WriteLine($"ReplayServer — {file} · 포트 {port} · 배속 {speed:F0}x" +
                (drop > 0 ? $" · drop {drop:P0}" : "") + (cut > 0 ? $" · cut {cut:F0}s" : ""));
            Console.WriteLine("클라이언트 접속 대기 중... (nc localhost " + port + " 또는 Unity TwinFeedClient)");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                Console.WriteLine($"접속: {client.Client.RemoteEndPoint}");
                _ = ServeAsync(client, file, speed, drop, cut, loop);
            }
        }

        private static async Task ServeAsync(
            TcpClient client, string file, double speed, double drop, double cut, bool loop)
        {
            using (client)
            using (var stream = client.GetStream())
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = false })
            {
                var pacer = new NdjsonPacer(file, speed, drop, 20260709);
                using var cts = new CancellationTokenSource();
                if (cut > 0.0)
                {
                    cts.CancelAfter(TimeSpan.FromSeconds(cut));
                }

                try
                {
                    do
                    {
                        await pacer.StreamAsync(writer, cts.Token);
                    }
                    while (loop && !cts.IsCancellationRequested);
                }
                catch (Exception ex) when (ex is OperationCanceledException || ex is IOException)
                {
                    Console.WriteLine($"연결 종료: {ex.GetType().Name}");
                }
            }

            Console.WriteLine("클라이언트 스트림 종료.");
        }

        private static string ArgValue(string[] args, string key, string fallback)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == key)
                {
                    return args[i + 1];
                }
            }

            return fallback;
        }

        private static bool HasFlag(string[] args, string key)
        {
            foreach (string arg in args)
            {
                if (arg == key)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
