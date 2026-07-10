using System;
using System.Collections.Generic;
using NUnit.Framework;
using DesCore;

namespace DesCore.Tests.Practice
{
    /// <summary>
    /// M/M/1 교차검증 실습(통계). 막히면 완성본
    /// ../DesCore.Tests/MM1CrossValidationTests.cs의 반복·CI 계산을 대조한다.
    /// </summary>
    [TestFixture]
    public sealed class MM1CrossValidationTests
    {
        [Test]
        public void MeanWaitInQueueMatchesTheoreticalWithin95CI()
        {
            // TODO(실습 5): λ=0.5, μ=1.0인 M/M/1을 SimResource(용량 1)로 시뮬레이션한다.
            //   손님이 NextExponential(1/λ) 간격으로 도착해 server.Request; onGranted에서
            //   대기 = Now − 도착시각을 기록하고 NextExponential(1/μ) 뒤 Release한다.
            //   시드 1..40으로 반복해 warm-up(앞 5,000명) 제외 평균 Wq의 표본을 모으고,
            //   x̄ ± t(0.975,39)·s/√40 신뢰구간이 이론값 ρ/(μ−λ)=1.0을 감싸는지 검증한다.
            Assert.Fail("TODO(실습 5): 아직 구현되지 않음");
        }
    }
}
