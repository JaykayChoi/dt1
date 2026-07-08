"""02 미니 반송 라인 — 스테이션 3개 + 반송차 2대.

로트가 A → B → C 세 공정을 순서대로 거치고, 공정 사이 이동은 반송차
(OHT/AGV를 추상화한 리소스)가 맡는다. 팹 물류의 뼈대 — "가공하고,
기다리고, 실려 가는" 흐름을 최소 구성으로 재현한다.

관전 포인트:
- 어느 스테이션이 병목인가 (평균 큐 대기가 가장 긴 곳)
- 반송차 2대는 충분한가 (반송 대기, 차량 가동률)

실행: python 02_transport_line.py
"""

import random

import simpy

ARRIVAL_MEAN = 6.0            # 평균 도착 간격 [분]
SERVICE_MEAN = {"A": 4.0, "B": 5.0, "C": 3.0}  # 스테이션별 평균 가공 시간 [분]
TRAVEL_TIME = 2.0             # 스테이션 간 반송 소요 [분]
VEHICLE_COUNT = 2             # 반송차 대수
SIM_TIME = 2_000              # 시뮬레이션 길이 [분]
SEED = 7
LOG_LOTS = 2                  # 이벤트 로그를 출력할 로트 수

lead_times = []                        # 완성 로트의 리드타임
station_waits = {"A": [], "B": [], "C": []}  # 스테이션 큐 대기
transport_waits = []                   # 반송차 배차 대기
busy = {"A": 0.0, "B": 0.0, "C": 0.0, "vehicle": 0.0}


def log(env, lot_id, message):
    """앞쪽 몇 개 로트만 이벤트 로그를 찍는다 — DES의 '사건' 감각용."""
    if lot_id < LOG_LOTS:
        print(f"[t={env.now:7.1f}] lot{lot_id:03d} {message}")


def process_at(env, lot_id, stations, name):
    """스테이션 name에서 큐 대기 후 가공한다."""
    queued = env.now
    with stations[name].request() as req:
        yield req
        station_waits[name].append(env.now - queued)
        log(env, lot_id, f"{name} 가공 시작 (대기 {env.now - queued:.1f}분)")
        service = random.expovariate(1 / SERVICE_MEAN[name])
        busy[name] += service
        yield env.timeout(service)
    log(env, lot_id, f"{name} 가공 완료")


def transport(env, lot_id, vehicles, hop):
    """반송차를 배차받아 다음 스테이션으로 이동한다."""
    queued = env.now
    with vehicles.request() as req:
        yield req
        transport_waits.append(env.now - queued)
        log(env, lot_id, f"{hop} 반송 시작 (배차 대기 {env.now - queued:.1f}분)")
        busy["vehicle"] += TRAVEL_TIME
        yield env.timeout(TRAVEL_TIME)


def lot_flow(env, lot_id, stations, vehicles):
    """로트 하나의 전체 여정: A 가공 → 반송 → B 가공 → 반송 → C 가공."""
    born = env.now
    log(env, lot_id, "투입")
    yield from process_at(env, lot_id, stations, "A")
    yield from transport(env, lot_id, vehicles, "A→B")
    yield from process_at(env, lot_id, stations, "B")
    yield from transport(env, lot_id, vehicles, "B→C")
    yield from process_at(env, lot_id, stations, "C")
    lead_times.append(env.now - born)
    log(env, lot_id, f"완성 (리드타임 {env.now - born:.1f}분)")


def source(env, stations, vehicles):
    lot_id = 0
    while True:
        yield env.timeout(random.expovariate(1 / ARRIVAL_MEAN))
        env.process(lot_flow(env, lot_id, stations, vehicles))
        lot_id += 1


def main():
    random.seed(SEED)
    env = simpy.Environment()
    stations = {name: simpy.Resource(env, capacity=1) for name in SERVICE_MEAN}
    vehicles = simpy.Resource(env, capacity=VEHICLE_COUNT)
    env.process(source(env, stations, vehicles))
    env.run(until=SIM_TIME)

    print()
    print(f"완성 로트 수        : {len(lead_times)}")
    print(f"처리량              : {len(lead_times) / SIM_TIME * 60:.1f} 로트/시간")
    print(f"평균 리드타임       : {sum(lead_times) / len(lead_times):.1f} 분")
    print(f"평균 반송 배차 대기 : {sum(transport_waits) / len(transport_waits):.2f} 분")
    print(f"반송차 가동률       : {busy['vehicle'] / (SIM_TIME * VEHICLE_COUNT):.3f}")
    print()
    print("스테이션    가동률    평균 큐 대기 [분]")
    for name in SERVICE_MEAN:
        waits = station_waits[name]
        print(f"    {name}      {busy[name] / SIM_TIME:6.3f}    {sum(waits) / len(waits):8.1f}")


if __name__ == "__main__":
    main()
