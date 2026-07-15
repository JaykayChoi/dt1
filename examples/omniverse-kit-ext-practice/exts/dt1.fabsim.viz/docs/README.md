# dt1.fabsim.viz

USD 팹 스테이지를 순회해 반송 장비 프림(이름에 `OHT` / `AGV` 포함)을 찾고, 각 프림의
`displayColor` 를 상태색으로 칠하는 Omniverse Kit 확장이다.

## 창

확장을 켜면 **DT1 FabSim Viz** 창이 뜬다. 버튼은 두 개.

- **팹 프림 스캔** — 현재 스테이지의 모든 프림을 순회(`stage.Traverse()`)해 이름에 `OHT`/`AGV`가
  들어간 프림을 목록에 모은다. 발견 개수를 상태줄에 표시.
- **상태색 적용** — 스캔한 프림들의 `UsdGeom.Gprim` `displayColor` 를 상태별 색으로 설정한다.
  데모라서 상태(idle/moving/carrying/fault)를 프림 순서로 순환시켜 색이 실제로 바뀌는 것을 보여준다.

## 상태색

| 상태 | 색 | RGB |
| --- | --- | --- |
| idle | 회색 | 0.55, 0.55, 0.58 |
| moving | 파랑 | 0.15, 0.70, 0.95 |
| carrying | 초록 | 0.20, 0.80, 0.35 |
| fault | 빨강 | 0.95, 0.25, 0.20 |

실제 디지털 트윈으로 확장한다면 이 상태를 Phase 8 이벤트 스트림(`examples/twin-bridge`)에서
받아 프림에 반영하면 된다.
