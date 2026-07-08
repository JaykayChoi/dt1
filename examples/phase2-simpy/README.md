# Phase 2 — SimPy 예제

이산사건시뮬레이션(DES)을 SimPy로 체득하는 예제 3개. 난이도 순서대로 실행하며
`docs/phase2/03-simpy.html` 해설과 함께 읽는다.

## 준비

```
pip install simpy
```

## 예제

| 파일 | 내용 | 확인 포인트 |
|---|---|---|
| `01_single_station.py` | M/M/1 단일 스테이션 | 측정값이 큐잉 이론값(ρ, Wq)과 일치하는가 |
| `02_transport_line.py` | 스테이션 3개 + 반송차 2대 미니 라인 | 병목은 어디인가(B), 반송차 2대는 충분한가 |
| `03_utilization_sweep.py` | 도착률을 올려가며 ρ–Wq 곡선 측정 | 가동률 90%대에서 대기시간이 폭발하는 비선형성 |

## 실행

```
python 01_single_station.py
python 02_transport_line.py
python 03_utilization_sweep.py   # 6회 반복 실행이라 수십 초 걸린다
```

`02_transport_line.py`는 `examples/phase2-descore`의 C# 데모와 같은 시나리오다 —
두 구현의 통계가 유사하게 나오는지 교차 검증할 수 있다.
