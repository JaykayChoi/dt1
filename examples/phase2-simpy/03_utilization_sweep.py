"""03 가동률 스윕 — 대기시간은 왜 비선형으로 폭발하는가.

M/M/1 스테이션의 도착률 λ를 조금씩 올리며(서비스율 μ=1 고정) 평균 큐
대기시간을 측정한다. 가동률 ρ가 1에 다가갈수록 대기가 완만히 늘지 않고
'폭발'하는 것 — 팹이 장비 가동률 100%를 목표로 하지 않는 이유 — 을
숫자로 확인한다.

실행: python 03_utilization_sweep.py
"""

import random

import simpy

SERVICE_MEAN = 1.0    # μ = 1.0 /분 (고정)
LAMBDAS = [0.5, 0.6, 0.7, 0.8, 0.9, 0.95]
SIM_TIME = 200_000    # ρ가 높을수록 수렴이 느려 길게 굴린다
SEED = 42


def run_mm1(lam):
    """도착률 lam인 M/M/1을 굴려 평균 큐 대기시간을 반환한다."""
    random.seed(SEED)
    env = simpy.Environment()
    machine = simpy.Resource(env, capacity=1)
    waits = []

    def lot(env):
        arrived = env.now
        with machine.request() as req:
            yield req
            waits.append(env.now - arrived)
            yield env.timeout(random.expovariate(1 / SERVICE_MEAN))

    def source(env):
        while True:
            yield env.timeout(random.expovariate(lam))
            env.process(lot(env))

    env.process(source(env))
    env.run(until=SIM_TIME)
    return sum(waits) / len(waits)


def main():
    mu = 1 / SERVICE_MEAN
    print("    λ       ρ     측정 Wq [분]   이론 Wq [분]")
    for lam in LAMBDAS:
        rho = lam / mu
        measured = run_mm1(lam)
        theory = rho / (mu - lam)
        print(f"  {lam:4.2f}   {rho:4.2f}   {measured:10.1f}   {theory:10.1f}")
    print()
    print("ρ 0.5→0.9 는 1.8배지만 대기시간은 9배 — 마지막 여유가 사라질 때 대기가 폭발한다.")


if __name__ == "__main__":
    main()
