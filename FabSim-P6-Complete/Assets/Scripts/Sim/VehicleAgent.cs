namespace FabSim.Sim
{
    /// <summary>비히클의 작업 단계.</summary>
    public enum VehiclePhase
    {
        Idle,
        ToPickup,
        Carrying,
    }

    /// <summary>
    /// OHT 한 대의 시뮬레이션 상태. 위치는 좌표가 아니라
    /// "어느 엣지를 언제 출발해 언제 도착하는가"로 표현된다 —
    /// 뷰(Unity)는 이 시각들로 렌더 시점의 위치를 보간한다.
    /// </summary>
    public sealed class VehicleAgent
    {
        public int Id;
        public VehiclePhase Phase;

        /// <summary>정지 중이거나 마지막으로 도착한 노드.</summary>
        public int NodeId;

        /// <summary>이동 중 여부. true면 아래 엣지 필드들이 유효하다.</summary>
        public bool IsMoving;

        public int EdgeFromNode;
        public int EdgeToNode;
        public double EdgeDepartAt;
        public double EdgeArriveAt;

        /// <summary>현재 수행 중인 반송 명령 (없으면 null).</summary>
        public TransportJob CurrentJob;

        /// <summary>가동률 계산용 — 현재 작업의 시작 시각과 누적 가동 시간.</summary>
        public double BusyStartedAt;
        public double TotalBusyTime;
    }
}
