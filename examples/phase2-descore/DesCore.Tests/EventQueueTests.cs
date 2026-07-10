using System;
using System.Collections.Generic;
using NUnit.Framework;
using DesCore;

namespace DesCore.Tests
{
    /// <summary>
    /// EventQueue(미래 사건 목록)의 정렬 불변식과 경계 동작을 못 박는다.
    /// DES의 결정성은 "시각 오름차순, 동시각은 일련번호 오름차순"이라는
    /// 이 큐의 계약에서 나온다.
    /// </summary>
    [TestFixture]
    public sealed class EventQueueTests
    {
        [Test]
        public void PopReturnsEventsInNonDecreasingTime()
        {
            var queue = new EventQueue();
            var random = new Random(12345);
            for (int i = 0; i < 1000; i++)
            {
                double time = random.NextDouble() * 1000.0;
                queue.Push(new SimEvent(time, i, () => { }));
            }

            double previousTime = double.NegativeInfinity;
            while (!queue.IsEmpty)
            {
                SimEvent next = queue.Pop();
                Assert.That(next.Time, Is.GreaterThanOrEqualTo(previousTime));
                previousTime = next.Time;
            }
        }

        [Test]
        public void SameTimeEventsPopInSequenceOrder()
        {
            var queue = new EventQueue();
            var sequences = new List<long> { 7, 2, 9, 0, 5, 3, 8, 1, 6, 4 };
            foreach (long sequence in sequences)
            {
                queue.Push(new SimEvent(5.0, sequence, () => { }));
            }

            long previousSequence = -1;
            while (!queue.IsEmpty)
            {
                SimEvent next = queue.Pop();
                Assert.That(next.Time, Is.EqualTo(5.0));
                Assert.That(next.Sequence, Is.GreaterThan(previousSequence));
                previousSequence = next.Sequence;
            }
        }

        [Test]
        public void PopOnEmptyThrows()
        {
            var queue = new EventQueue();
            Assert.Throws<InvalidOperationException>(() => queue.Pop());
        }

        [Test]
        public void PeekTimeOnEmptyThrows()
        {
            var queue = new EventQueue();
            Assert.Throws<InvalidOperationException>(() =>
            {
                double _ = queue.PeekTime;
            });
        }
    }
}
