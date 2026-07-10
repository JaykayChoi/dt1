using System.Collections.Generic;
using NUnit.Framework;
using FabSim.Sim;

namespace FabSim.Tests.EditMode
{
    /// <summary>
    /// SegmentController의 점유 불변식과 데드락 탐지를 검증한다. 한 구간엔 한 대,
    /// 대기·승계, 그리고 wait-for 사이클 탐지가 핵심이다.
    /// </summary>
    public sealed class SegmentControllerTests
    {
        [Test]
        public void AcquireGrantsImmediatelyWhenFree()
        {
            var seg = new SegmentController();
            bool granted = false;
            seg.Acquire(10, 0, () => granted = true);

            Assert.That(granted, Is.True);
            Assert.That(seg.HolderOf(10), Is.EqualTo(0));
        }

        [Test]
        public void SecondAcquireWaitsAndHolderStaysSingle()
        {
            var seg = new SegmentController();
            seg.Acquire(10, 0, () => { });
            bool secondGranted = false;
            seg.Acquire(10, 1, () => secondGranted = true);

            // 한 구간엔 한 대 — 두 번째는 대기, 점유자는 여전히 0.
            Assert.That(secondGranted, Is.False);
            Assert.That(seg.HolderOf(10), Is.EqualTo(0));
            Assert.That(seg.WaitingEdgeOf(1), Is.EqualTo(10));
        }

        [Test]
        public void ReleaseHandsOffToWaiter()
        {
            var seg = new SegmentController();
            seg.Acquire(10, 0, () => { });
            bool secondGranted = false;
            seg.Acquire(10, 1, () => secondGranted = true);

            seg.Release(10, 0);

            // 반납 시 대기자 1에게 승계 — 점유자가 1로 바뀌고 대기 해제.
            Assert.That(secondGranted, Is.True);
            Assert.That(seg.HolderOf(10), Is.EqualTo(1));
            Assert.That(seg.WaitingEdgeOf(1), Is.EqualTo(-1));
        }

        [Test]
        public void UnidirectionalChainHasNoDeadlock()
        {
            // 차량 0이 구간 A, 1이 구간 B를 쥔 채 각자 앞 구간을 기다리지만 순환이 아니다:
            // 0은 1의 구간을, 1은 (아무도 없는) 구간을 기다림 → 사슬(체인), 사이클 아님.
            var seg = new SegmentController();
            seg.Acquire(1, 0, () => { });   // 0이 구간 1 점유
            seg.Acquire(2, 1, () => { });   // 1이 구간 2 점유
            seg.Acquire(2, 0, () => { });   // 0이 구간 2를 대기(1이 점유)
            seg.Acquire(3, 1, () => { });   // 1이 구간 3을 대기(비어 있어 즉시 획득됨)

            Assert.That(seg.DetectDeadlockCycle(), Is.Empty);
        }

        [Test]
        public void CrossOccupancyIsDetectedAsCycle()
        {
            // 마주 보는 두 차량: 0이 구간 1을 쥔 채 2를, 1이 구간 2를 쥔 채 1을 대기 → 순환.
            var seg = new SegmentController();
            seg.Acquire(1, 0, () => { });   // 0이 구간 1 점유
            seg.Acquire(2, 1, () => { });   // 1이 구간 2 점유
            seg.Acquire(2, 0, () => { });   // 0이 구간 2 대기(1 점유)
            seg.Acquire(1, 1, () => { });   // 1이 구간 1 대기(0 점유) → 데드락

            IReadOnlyList<int> cycle = seg.DetectDeadlockCycle();
            Assert.That(cycle, Is.EquivalentTo(new[] { 0, 1 }));
        }
    }
}
