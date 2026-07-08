"""01 단일 스테이션 — M/M/1 대기행렬.

로트가 랜덤하게 도착하고(포아송 과정), 장비 1대가 랜덤한 시간(지수분포)으로
처리한다. DES의 최소 단위 실험 — 시뮬레이션 측정값이 큐잉 이론값과
일치하는지 확인하는 것이 목적이다.

실행: python 01_single_station.py
"""

import random

import simpy

ARRIVAL_MEAN = 1.25  # 평균 도착 간격 [분] → 도착률 λ = 1/1.25 = 0.8 /분
SERVICE_MEAN = 1.0   # 평균 처리 시간 [분] → 서비스율 μ = 1.0 /분
SIM_TIME = 20_000    # 시뮬레이션 길이 [분]
SEED = 42

wait_times = []  # 로트별 큐 대기시간
busy_time = 0.0  # 장비가 실제로 일한 총 시간


def lot(env, machine):
    """로트 하나의 생애: 도착 → 큐 대기 → 가공 → 퇴장."""
    global busy_time
    arrived = env.now
    with machine.request() as req:
        yield req  # 장비가 빌 때까지 큐에서 대기
        wait_times.append(env.now - arrived)
        service = random.expovariate(1 / SERVICE_MEAN)
        busy_time += service
        yield env.timeout(service)  # 가공


def source(env, machine):
    """포아송 도착원: 지수분포 간격으로 로트를 계속 투입한다."""
    while True:
        yield env.timeout(random.expovariate(1 / ARRIVAL_MEAN))
        env.process(lot(env, machine))


def main():
    random.seed(SEED)
    env = simpy.Environment()
    machine = simpy.Resource(env, capacity=1)
    env.process(source(env, machine))
    env.run(until=SIM_TIME)

    lam = 1 / ARRIVAL_MEAN
    mu = 1 / SERVICE_MEAN
    rho = lam / mu
    measured_wq = sum(wait_times) / len(wait_times)
    theory_wq = rho / (mu - lam)  # M/M/1: Wq = ρ / (μ - λ)

    print(f"처리 로트 수          : {len(wait_times):,}")
    print(f"가동률 ρ              : 측정 {busy_time / SIM_TIME:.3f} | 이론 {rho:.3f}")
    print(f"평균 큐 대기 Wq [분]  : 측정 {measured_wq:.2f} | 이론 {theory_wq:.2f}")


if __name__ == "__main__":
    main()
