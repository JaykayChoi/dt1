using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using FabSim.Sim;
using UnityEngine;

namespace FabSim.Twin
{
    /// <summary>
    /// 리플레이/라이브 서버에 TCP로 붙어 NDJSON 이벤트를 받아 같은 뷰를 구동하는 트윈 소스.
    /// 소켓 수신·파싱은 백그라운드 스레드에서(ConcurrentQueue), 상태 적용은 메인 스레드(Tick)에서
    /// 한다 — 스레드 경계를 락 프리 큐 하나로 마샬링한다. 0.5초 플레이아웃 버퍼로 지터를 흡수한다.
    /// </summary>
    public sealed class TwinFeedClient : MonoBehaviour, IFleetSource
    {
        [SerializeField]
        private string host = "127.0.0.1";

        [SerializeField]
        private int port = 5088;

        [SerializeField]
        private float playoutDelay = 0.5f;

        private readonly ConcurrentQueue<string> inbound = new ConcurrentQueue<string>();
        private TwinReplaySource source;
        private Thread receiveThread;
        private volatile bool running;
        private volatile bool connected;
        private long lastSequence = -1;
        private int droppedCount;

        /// <summary>서버 접속 여부(대시보드 feed-status가 읽는다).</summary>
        public bool Connected => connected;

        /// <summary>관측된 유실(시퀀스 갭) 누적 수.</summary>
        public int DroppedCount => droppedCount;

        public RailGraph Graph => source?.Graph;

        public IReadOnlyList<VehicleAgent> Vehicles => source != null ? source.Vehicles : System.Array.Empty<VehicleAgent>();

        public double Now => source?.Now ?? 0.0;

        public int CompletedJobs => source?.CompletedJobs ?? 0;

        public int PendingJobCount => source?.PendingJobCount ?? 0;

        public double GetThroughputPerHour(double now)
        {
            return source?.GetThroughputPerHour(now) ?? 0.0;
        }

        public double GetAverageDeliveryTime()
        {
            return source?.GetAverageDeliveryTime() ?? 0.0;
        }

        public double GetFleetUtilization(double now)
        {
            return source?.GetFleetUtilization(now) ?? 0.0;
        }

        /// <summary>수신 스레드를 시작한다(호스트가 소스를 활성화할 때 호출).</summary>
        public void Connect()
        {
            if (running)
            {
                return;
            }

            running = true;
            receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
            receiveThread.Start();
        }

        public void Tick(float deltaTime)
        {
            // 메인 스레드에서만 큐를 드레인해 소스에 적용한다(마샬링 경계).
            while (inbound.TryDequeue(out string line))
            {
                if (source == null)
                {
                    if (line.Contains("\"HELLO\""))
                    {
                        source = new TwinReplaySource(new[] { line }, playoutDelay);
                    }

                    continue;
                }

                TrackSequence(line);
                source.AppendLine(line);
            }

            source?.Tick(deltaTime);
        }

        private void OnDisable()
        {
            running = false;
            connected = false;
            receiveThread?.Join(200);
        }

        private void ReceiveLoop()
        {
            while (running)
            {
                try
                {
                    using var client = new TcpClient();
                    client.Connect(host, port);
                    connected = true;
                    using var stream = client.GetStream();
                    using var reader = new StreamReader(stream);
                    while (running)
                    {
                        string line = reader.ReadLine();
                        if (line == null)
                        {
                            break;
                        }

                        inbound.Enqueue(line);
                    }
                }
                catch (Exception)
                {
                    // 접속 실패·끊김 — 잠시 뒤 재접속(고장 주입 복구 경로).
                }

                connected = false;
                if (running)
                {
                    Thread.Sleep(500);
                }
            }
        }

        // 시퀀스 n의 갭을 유실로 센다(HELLO/SNAPSHOT은 n이 이어지지 않을 수 있어 제외 가능).
        private void TrackSequence(string line)
        {
            int at = line.IndexOf("\"n\":", StringComparison.Ordinal);
            if (at < 0)
            {
                return;
            }

            int start = at + 4;
            int end = start;
            while (end < line.Length && char.IsDigit(line[end]))
            {
                end++;
            }

            if (long.TryParse(line.Substring(start, end - start), out long n))
            {
                if (lastSequence >= 0 && n > lastSequence + 1)
                {
                    droppedCount += (int)(n - lastSequence - 1);
                }

                lastSequence = n;
            }
        }
    }
}
