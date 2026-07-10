using System;
using System.Collections.Generic;

namespace DesCore
{
    /// <summary>
    /// 용량이 제한된 자원(장비, 반송차 등). SimPy의 Resource에 해당하되
    /// 코루틴 대신 콜백 방식이다 — Request에 넘긴 콜백이 자원을 얻는 순간
    /// 호출되고, 사용이 끝나면 Release를 호출해야 한다.
    /// </summary>
    public sealed class SimResource
    {
        private readonly Simulation simulation;
        private readonly Queue<Action> waiters = new Queue<Action>();
        private double busyArea;
        private double lastChangeTime;

        /// <summary>통계 출력용 이름.</summary>
        public string Name { get; }

        /// <summary>동시에 사용할 수 있는 최대 수.</summary>
        public int Capacity { get; }

        /// <summary>현재 사용 중인 수.</summary>
        public int InUse { get; private set; }

        /// <summary>자원을 기다리는 요청 수.</summary>
        public int QueueLength => waiters.Count;

        public SimResource(Simulation simulation, string name, int capacity)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            this.simulation = simulation;
            Name = name;
            Capacity = capacity;
        }

        /// <summary>
        /// 자원을 요청한다. 여유가 있으면 onGranted가 즉시 호출되고,
        /// 없으면 대기열에 들어가 차례가 왔을 때 호출된다.
        /// </summary>
        public void Request(Action onGranted)
        {
            if (onGranted == null)
            {
                throw new ArgumentNullException(nameof(onGranted));
            }

            if (InUse < Capacity)
            {
                AccumulateBusyArea();
                InUse++;
                onGranted();
            }
            else
            {
                waiters.Enqueue(onGranted);
            }
        }

        /// <summary>
        /// 자원을 반납한다. 대기자가 있으면 점유 수를 유지한 채
        /// 다음 대기자에게 바로 넘겨준다.
        /// </summary>
        public void Release()
        {
            if (InUse == 0)
            {
                throw new InvalidOperationException($"{Name}: released more than requested.");
            }

            if (waiters.Count > 0)
            {
                Action next = waiters.Dequeue();
                next();
            }
            else
            {
                AccumulateBusyArea();
                InUse--;
            }
        }

        /// <summary>시작부터 지금까지의 평균 가동률(용량 기준)을 반환한다.</summary>
        public double GetUtilization()
        {
            AccumulateBusyArea();
            if (simulation.Now <= 0.0)
            {
                return 0.0;
            }

            return busyArea / (simulation.Now * Capacity);
        }

        private void AccumulateBusyArea()
        {
            // 가동률은 "사용 중 수 × 경과 시간"의 시간 적분으로 계산한다.
            busyArea += InUse * (simulation.Now - lastChangeTime);
            lastChangeTime = simulation.Now;
        }
    }
}
