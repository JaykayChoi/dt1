using UnityEngine;

namespace FabViz
{
    /// <summary>장비의 운영 상태. 색·라벨로 시각화된다.</summary>
    public enum MachineState
    {
        Running,
        Idle,
        Warning,
        Down,
    }

    /// <summary>
    /// 상태 → 색·라벨 매핑. 팀 표준 색 코드를 한곳에서 관리한다
    /// (정상=청록, 유휴=청색, 경고=황, 정지=적). docs/phase3/06 의 상태색 매핑과 일치.
    /// </summary>
    public static class StatusPalette
    {
        public static Color Color(MachineState state)
        {
            switch (state)
            {
                case MachineState.Running:
                    return new Color(0.10f, 0.85f, 0.55f);
                case MachineState.Idle:
                    return new Color(0.30f, 0.55f, 0.85f);
                case MachineState.Warning:
                    return new Color(0.95f, 0.72f, 0.15f);
                case MachineState.Down:
                    return new Color(0.92f, 0.22f, 0.18f);
                default:
                    return UnityEngine.Color.gray;
            }
        }

        public static string Label(MachineState state)
        {
            switch (state)
            {
                case MachineState.Running:
                    return "RUN";
                case MachineState.Idle:
                    return "IDLE";
                case MachineState.Warning:
                    return "WARN";
                case MachineState.Down:
                    return "DOWN";
                default:
                    return "?";
            }
        }
    }
}
