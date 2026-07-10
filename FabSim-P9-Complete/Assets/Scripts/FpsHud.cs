using System.Text;
using UnityEngine;

namespace FabSim
{
    /// <summary>
    /// 프레임 타임과 FPS를 화면 좌상단에 표시한다.
    /// 문자열 생성은 GC를 만들므로 0.25초마다 한 번만 갱신한다 —
    /// "매 프레임 문자열 만들기"가 왜 나쁜지의 반면교사이자 대응책.
    /// </summary>
    public sealed class FpsHud : MonoBehaviour
    {
        private const float UpdateInterval = 0.25f;

        private readonly StringBuilder builder = new StringBuilder(64);
        private string display = "";
        private float accumulated;
        private int frames;
        private float timer;

        private void Update()
        {
            accumulated += Time.unscaledDeltaTime;
            frames++;
            timer += Time.unscaledDeltaTime;
            if (timer < UpdateInterval)
            {
                return;
            }

            float averageMs = accumulated / frames * 1000f;
            float fps = frames / accumulated;
            builder.Length = 0;
            builder.Append("Frame ").Append(averageMs.ToString("F2")).Append(" ms  (")
                .Append(fps.ToString("F0")).Append(" FPS)");
            display = builder.ToString();
            accumulated = 0f;
            frames = 0;
            timer = 0f;
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(12f, 8f, 320f, 24f), display);
        }
    }
}
