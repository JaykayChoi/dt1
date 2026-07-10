using System;
using System.Collections.Generic;
using NUnit.Framework;
using DesCore;

namespace DesCore.Tests.Practice
{
    /// <summary>
    /// EventQueue 실습. PopOnEmptyThrows는 형식을 보여 주는 완성 예시다.
    /// 나머지 TODO 스텁을 실제 Assert로 채워 green으로 만드는 것이 과제다.
    /// 막히면 완성본 ../DesCore.Tests/EventQueueTests.cs의 같은 이름 테스트를 대조한다.
    /// </summary>
    [TestFixture]
    public sealed class EventQueueTests
    {
        // 완성 예시 — 처음부터 green. 이 형식을 참고해 아래 스텁들을 채운다.
        [Test]
        public void PopOnEmptyThrows()
        {
            var queue = new EventQueue();
            Assert.Throws<InvalidOperationException>(() => queue.Pop());
        }

        [Test]
        public void PopReturnsEventsInNonDecreasingTime()
        {
            // TODO(실습 1): 고정 시드 Random으로 무작위 Time·Sequence=i인 SimEvent를 1,000개
            //   Push한 뒤, 연속 Pop한 Time이 비내림차순(이전 <= 다음)인지 Assert로 검증한다.
            Assert.Fail("TODO(실습 1): 아직 구현되지 않음");
        }

        [Test]
        public void SameTimeEventsPopInSequenceOrder()
        {
            // TODO(실습 2): 같은 Time(예: 5.0)에 Sequence 0..9를 셔플해 Push한 뒤,
            //   Pop 순서가 Sequence 오름차순인지 검증한다(DES 결정성의 tie-break).
            Assert.Fail("TODO(실습 2): 아직 구현되지 않음");
        }
    }
}
