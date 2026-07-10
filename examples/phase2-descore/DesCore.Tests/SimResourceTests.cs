using System;
using NUnit.Framework;
using DesCore;

namespace DesCore.Tests
{
    /// <summary>
    /// SimResource의 가동률 적분과 대기열 핸드오프를 검증한다.
    /// 가동률은 손으로 계산할 수 있는 값과 대조해 "엔진이 설계대로인가"를 확인한다.
    /// </summary>
    [TestFixture]
    public sealed class SimResourceTests
    {
        [Test]
        public void UtilizationMatchesHandComputedValue()
        {
            var simulation = new Simulation();
            var resource = new SimResource(simulation, "M", 1);

            simulation.Schedule(0.0, () => resource.Request(() => { }));
            simulation.Schedule(10.0, () => resource.Release());
            simulation.Run(20.0);

            // busy 10 / (Now 20 * capacity 1) = 0.5.
            Assert.That(resource.GetUtilization(), Is.EqualTo(0.5).Within(1e-9));
        }

        [Test]
        public void QueuedRequestGrantedOnRelease()
        {
            var simulation = new Simulation();
            var resource = new SimResource(simulation, "M", 1);
            bool firstGranted = false;
            bool secondGranted = false;

            resource.Request(() => firstGranted = true);
            resource.Request(() => secondGranted = true);

            Assert.That(firstGranted, Is.True);
            Assert.That(secondGranted, Is.False);
            Assert.That(resource.QueueLength, Is.EqualTo(1));
            Assert.That(resource.InUse, Is.EqualTo(1));

            resource.Release();

            Assert.That(secondGranted, Is.True);
            Assert.That(resource.QueueLength, Is.EqualTo(0));
            Assert.That(resource.InUse, Is.EqualTo(1));
        }

        [Test]
        public void ReleaseWithoutRequestThrows()
        {
            var simulation = new Simulation();
            var resource = new SimResource(simulation, "M", 1);
            Assert.Throws<InvalidOperationException>(() => resource.Release());
        }
    }
}
