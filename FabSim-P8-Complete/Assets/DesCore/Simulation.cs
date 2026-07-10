using System;

namespace DesCore
{
    /// <summary>
    /// DES 엔진의 심장 — 시뮬레이션 클록과 사건 루프.
    /// "가장 이른 사건을 꺼내고, 클록을 그 시각으로 점프시키고, 실행한다"의
    /// 반복이 엔진의 전부다.
    /// </summary>
    public sealed class Simulation
    {
        private readonly EventQueue eventQueue = new EventQueue();
        private long nextSequence;

        /// <summary>현재 시뮬레이션 시각.</summary>
        public double Now { get; private set; }

        /// <summary>지금까지 실행한 사건 수.</summary>
        public long ExecutedCount { get; private set; }

        /// <summary>delay 시간 뒤에 action이 실행되도록 예약한다.</summary>
        public void Schedule(double delay, Action action)
        {
            if (delay < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(delay), "Cannot schedule an event in the past.");
            }

            eventQueue.Push(new SimEvent(Now + delay, nextSequence++, action));
        }

        /// <summary>until 시각까지의 사건을 모두 실행하고 클록을 until로 맞춘다.</summary>
        public void Run(double until)
        {
            while (!eventQueue.IsEmpty && eventQueue.PeekTime <= until)
            {
                SimEvent next = eventQueue.Pop();
                Now = next.Time;
                ExecutedCount++;
                next.Action();
            }

            Now = until;
        }
    }
}
