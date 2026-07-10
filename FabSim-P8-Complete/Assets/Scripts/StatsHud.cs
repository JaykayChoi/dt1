using System.Text;
using UnityEngine;

namespace FabSim
{
    /// <summary>
    /// 통계 HUD — 처리량, 평균 반송 시간, 함대 가동률, 대기 명령 수.
    /// Phase 1 Station 04의 지표들이 그대로 화면에 올라온다.
    /// 문자열은 0.25초마다만 갱신한다 (GC 절약 — Phase 4).
    /// </summary>
    public sealed class StatsHud : MonoBehaviour
    {
        private const float UpdateInterval = 0.25f;

        [SerializeField]
        private FabSimulation simulation;

        private readonly StringBuilder builder = new StringBuilder(256);
        private string display = "";
        private float timer;

        /// <summary>씬 빌더가 참조를 주입한다.</summary>
        public void Initialize(FabSimulation owner)
        {
            simulation = owner;
        }

        private void Update()
        {
            timer += Time.unscaledDeltaTime;
            if (timer < UpdateInterval)
            {
                return;
            }

            timer = 0f;
            var model = simulation.Model;
            double now = model.Simulation.Now;

            builder.Length = 0;
            builder.Append("시뮬레이션 시간  ").Append(FormatTime(now))
                .Append("   배속 ×").Append(simulation.TimeScale.ToString("F0"))
                .Append("  (키 0/1/2/3)").AppendLine();
            builder.Append("완료 반송        ").Append(model.CompletedJobs).AppendLine();
            builder.Append("처리량           ")
                .Append(model.GetThroughputPerHour(now).ToString("F1"))
                .Append(" 건/시간").AppendLine();
            builder.Append("평균 반송 시간   ")
                .Append(model.GetAverageDeliveryTime().ToString("F1")).Append(" 초").AppendLine();
            builder.Append("함대 가동률      ")
                .Append((model.GetFleetUtilization(now) * 100.0).ToString("F0"))
                .Append(" %").AppendLine();
            builder.Append("대기 중 명령     ").Append(model.PendingJobCount);
            display = builder.ToString();
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(10f, 10f, 300f, 130f), "");
            GUI.Label(new Rect(22f, 18f, 280f, 120f), display);
        }

        private static string FormatTime(double seconds)
        {
            int total = (int)seconds;
            int hours = total / 3600;
            int minutes = total % 3600 / 60;
            int secs = total % 60;
            return $"{hours:D2}:{minutes:D2}:{secs:D2}";
        }
    }
}
