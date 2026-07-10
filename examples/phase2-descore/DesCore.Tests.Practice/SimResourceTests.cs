using System;
using NUnit.Framework;
using DesCore;

namespace DesCore.Tests.Practice
{
    /// <summary>
    /// SimResource 실습. 막히면 완성본 ../DesCore.Tests/SimResourceTests.cs를 대조한다.
    /// </summary>
    [TestFixture]
    public sealed class SimResourceTests
    {
        [Test]
        public void UtilizationMatchesHandComputedValue()
        {
            // TODO(실습 3): 용량 1 자원을 t=0에 Request, t=10에 Release하고 Run(20)한 뒤,
            //   GetUtilization()이 손계산 값 0.5(= busy 10 / (20 * 1))와 같은지 Within(1e-9)로 검증한다.
            Assert.Fail("TODO(실습 3): 아직 구현되지 않음");
        }
    }
}
