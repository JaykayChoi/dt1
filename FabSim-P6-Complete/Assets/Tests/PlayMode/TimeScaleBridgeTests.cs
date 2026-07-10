using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FabSim;

namespace FabSim.Tests.PlayMode
{
    /// <summary>
    /// 배속 브리지 검증 — timeScale 120에서 프레임이 흐르면 시뮬레이션 클록이
    /// 벽시계 시간의 정확히 120배만큼 전진한다. 프레임이 흘러야 관측되므로 PlayMode다.
    /// </summary>
    public sealed class TimeScaleBridgeTests
    {
        [UnityTest]
        public IEnumerator SimClockAdvancesProportionalToTimeScale()
        {
            var go = new GameObject("sim");
            FabSimulation fab = go.AddComponent<FabSimulation>();

            // Awake가 FabModel을 만들고 Start를 부른다. 한 프레임 흘려 초기화를 마친다.
            yield return null;

            // 런타임 코드 수정 없이 배속을 강제한다(키 입력 대신 리플렉션).
            FieldInfo field = typeof(FabSimulation).GetField(
                "timeScale", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null, "timeScale 필드를 찾아야 한다.");
            field.SetValue(fab, 120f);

            // 배속을 건 직후를 기준선으로 삼는다.
            double before = fab.Model.Simulation.Now;
            double accumulatedWall = 0.0;
            for (int i = 0; i < 30; i++)
            {
                yield return null;
                accumulatedWall += Time.deltaTime;
            }

            // Run(until)이 매 프레임 Now를 정확히 until로 맞추므로, 전진량은 벽시계 × 배속.
            double expected = accumulatedWall * fab.TimeScale;
            double actual = fab.Model.Simulation.Now - before;
            Assert.That(actual, Is.EqualTo(expected).Within(1e-2));

            Object.Destroy(go);
        }
    }
}
