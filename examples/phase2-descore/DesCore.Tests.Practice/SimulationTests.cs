using System;
using System.Collections.Generic;
using NUnit.Framework;
using DesCore;

namespace DesCore.Tests.Practice
{
    /// <summary>
    /// Simulation 클록 실습. 막히면 완성본 ../DesCore.Tests/SimulationTests.cs를 대조한다.
    /// </summary>
    [TestFixture]
    public sealed class SimulationTests
    {
        [Test]
        public void EventsExecuteAtScheduledTime()
        {
            // TODO(실습 4): delay 5,3,8을 Schedule하고 각 Action에서 sim.Now를 기록해,
            //   실행 순서가 3,5,8이고 각 시점 Now가 정확히 그 값이며 ExecutedCount==3인지 검증한다.
            Assert.Fail("TODO(실습 4): 아직 구현되지 않음");
        }
    }
}
