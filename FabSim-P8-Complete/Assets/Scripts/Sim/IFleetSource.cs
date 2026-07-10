using System.Collections.Generic;

namespace FabSim.Sim
{
    /// <summary>
    /// 비히클 뷰와 대시보드가 소스를 모른 채 읽는 표면. 내장 시뮬레이션(SimFleetSource)과
    /// 외부 트윈(TwinReplaySource/TwinFeedClient)이 같은 인터페이스를 구현해, 하나의 뷰를
    /// 두 소스가 구동한다. 뷰가 실제로 읽는 것만 노출한다.
    /// </summary>
    public interface IFleetSource
    {
        /// <summary>레일 그래프(노드 좌표 조회용).</summary>
        RailGraph Graph { get; }

        /// <summary>현재 비히클 상태 목록.</summary>
        IReadOnlyList<VehicleAgent> Vehicles { get; }

        /// <summary>현재 소스 시각 [초].</summary>
        double Now { get; }

        /// <summary>완료된 반송 수.</summary>
        int CompletedJobs { get; }

        /// <summary>배차 대기 명령 수.</summary>
        int PendingJobCount { get; }

        /// <summary>now 기준 시간당 처리량 [건/시간].</summary>
        double GetThroughputPerHour(double now);

        /// <summary>완료 기준 평균 반송 시간 [초].</summary>
        double GetAverageDeliveryTime();

        /// <summary>now 기준 함대 평균 가동률(0~1).</summary>
        double GetFleetUtilization(double now);

        /// <summary>실시간 deltaTime만큼 소스를 전진시킨다(시뮬은 클록을 밀고, 트윈은 버퍼를 소비).</summary>
        void Tick(float deltaTime);
    }
}
