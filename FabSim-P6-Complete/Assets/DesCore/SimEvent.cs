using System;

namespace DesCore
{
    /// <summary>
    /// 시뮬레이션에서 특정 시각에 실행될 하나의 사건.
    /// </summary>
    public sealed class SimEvent
    {
        /// <summary>사건이 실행될 시뮬레이션 시각.</summary>
        public double Time { get; }

        /// <summary>같은 시각에 예약된 사건들의 실행 순서를 보장하는 일련번호.</summary>
        public long Sequence { get; }

        /// <summary>사건이 실행할 동작.</summary>
        public Action Action { get; }

        public SimEvent(double time, long sequence, Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            Time = time;
            Sequence = sequence;
            Action = action;
        }

        /// <summary>이 사건이 other보다 먼저 실행되어야 하면 true를 반환한다.</summary>
        public bool IsBefore(SimEvent other)
        {
            if (Time != other.Time)
            {
                return Time < other.Time;
            }

            return Sequence < other.Sequence;
        }
    }
}
