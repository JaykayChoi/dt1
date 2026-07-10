using System;
using System.Collections.Generic;
using NUnit.Framework;
using DesCore;

namespace DesCore.Tests
{
    /// <summary>
    /// Simulation 클록이 예약 시각에 정확히 사건을 실행하고, 단조 증가하며,
    /// Run(until)이 클록을 until로 맞추는지 검증한다.
    /// </summary>
    [TestFixture]
    public sealed class SimulationTests
    {
        [Test]
        public void EventsExecuteAtScheduledTime()
        {
            var simulation = new Simulation();
            var observed = new List<double>();

            simulation.Schedule(5.0, () => observed.Add(simulation.Now));
            simulation.Schedule(3.0, () => observed.Add(simulation.Now));
            simulation.Schedule(8.0, () => observed.Add(simulation.Now));
            simulation.Run(10.0);

            Assert.That(observed, Is.EqualTo(new List<double> { 3.0, 5.0, 8.0 }));
            Assert.That(simulation.ExecutedCount, Is.EqualTo(3));
        }

        [Test]
        public void ClockIsMonotonic()
        {
            var simulation = new Simulation();
            var random = new Random(999);
            var observed = new List<double>();
            for (int i = 0; i < 500; i++)
            {
                simulation.Schedule(random.NextDouble() * 100.0, () => observed.Add(simulation.Now));
            }

            simulation.Run(200.0);

            double previous = double.NegativeInfinity;
            foreach (double now in observed)
            {
                Assert.That(now, Is.GreaterThanOrEqualTo(previous));
                previous = now;
            }
        }

        [Test]
        public void RunSetsClockToUntil()
        {
            var simulation = new Simulation();
            simulation.Run(50.0);
            Assert.That(simulation.Now, Is.EqualTo(50.0));

            bool executed = false;
            simulation.Schedule(100.0, () => executed = true);
            simulation.Run(70.0);

            Assert.That(simulation.Now, Is.EqualTo(70.0));
            Assert.That(executed, Is.False);
            Assert.That(simulation.ExecutedCount, Is.EqualTo(0));
        }

        [Test]
        public void NegativeDelayThrows()
        {
            var simulation = new Simulation();
            Assert.Throws<ArgumentOutOfRangeException>(() => simulation.Schedule(-1.0, () => { }));
        }
    }
}
