namespace FabSim.Sim
{
    /// <summary>
    /// 반송 명령 하나 — Phase 1에서 배운 MCS의 transport job에 해당한다.
    /// 시각들은 시뮬레이션 시간(초)이다.
    /// </summary>
    public sealed class TransportJob
    {
        public int Id;

        /// <summary>픽업 포트 노드 id.</summary>
        public int FromPort;

        /// <summary>드롭 포트 노드 id.</summary>
        public int ToPort;

        public double CreatedAt;
        public double AssignedAt;
        public double CompletedAt;
    }
}
