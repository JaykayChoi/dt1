# DT1 — 팹 물류 디지털 트윈 사전 학습

반도체 팹의 물류·반송 시스템을 Unity 기반 3D **디지털 트윈**으로 구현하기 위한
도메인·기술 학습 저장소. 개인 사전 학습용이다.

**📖 온라인 교재 → https://jaykaychoi.github.io/dt1/**

## 무엇을 배우나

두 축을 나란히 학습해 본선(Phase 1~5)의 종점에서 **미니 팹 시뮬레이터**를 만들고,
연장선(Phase 6~10)에서 이를 **실전 트윈** 수준으로 확장한다.

- **도메인 (공장 자동화·물류)** — OHT/AGV/AMR 반송 장비, 반도체 팹 AMHS(FOUP·스토커·
  베이), 제어 계층(MES → MCS → Fleet Management → 장비), 배차·경로·교통제어·데드락,
  이산사건시뮬레이션(DES).
- **Unity 기술 (시각화·성능)** — PBR 렌더링, 라이트 베이킹, 포스트 프로세싱,
  Profiler/Frame Debugger 기반 성능 분석·최적화.
- **실전 보강 (트윈 연결·엔지니어링)** — 자동화 테스트·시뮬레이션 통계 검증, 교통
  제어·데드락, 실시간 데이터 연동(이벤트 스트림·리플레이), Jobs/Burst 대규모 성능,
  CAD 자산 파이프라인.

## 학습 노선 (10 페이즈)

| Phase | 주제 | 산출물 |
|-------|------|--------|
| 1 | 도메인 개념: 공장 자동화·물류 | 반송 장비·AMHS·제어 계층·Fleet 문제·SEMI 표준·용어집 |
| 2 | 이산사건시뮬레이션(DES) | SimPy 예제 + C# DES 엔진(DesCore) |
| 3 | Unity PBR 렌더링 & 라이팅 | PBR·베이킹·포스트 프로세싱 + FabSim 팹 씬 |
| 4 | 성능 분석·최적화 | Profiler·Frame Debugger·배칭·LOD·풀링 실험장 |
| 5 | 통합: 미니 팹 시뮬레이터 | DES 코어 ↔ Unity 브리지, 배차·통계 HUD |
| 6 | 신뢰성: 테스트와 시뮬레이션 검증 | DesCore 단위 테스트 + asmdef + 출력 분석(반복·신뢰구간) |
| 7 | Fleet 심화: 교통·데드락·라우팅·배차 | 구간 점유 교통 제어·데드락 탐지·A*·배차 정책 랩 |
| 8 | 디지털 트윈 연결 | 이벤트 스트림 레코더/리플레이 + 실시간 브리지 + UI Toolkit 대시보드 |
| 9 | 대규모 성능 심화 | Jobs/Burst·메모리/씬 스트리밍·GPU-driven 측정 기록 |
| 10 | 실자산 파이프라인 | CAD/FBX 임포트 최적화 + AssetPostprocessor 자동화 |

## 저장소 구조

```
docs/                    HTML 학습 사이트 (정적, 오프라인 동작 · GitHub Pages 호스팅)
├── index.html           로드맵 허브(노선도)
└── phase*/              페이즈별 교재 스테이션
examples/
├── phase2-simpy/        SimPy DES 예제 3종 (Python)
├── phase2-descore/      C# 미니 DES 엔진 + 콘솔 데모
└── fleet-dispatch/      배차 정책 실험 랩 (DesCore 재사용)
FabSim/                  실습 프로젝트 (Phase 3~5, 직접 수행)
FabSim-P3-Complete/      Phase 3 완성본 — PBR·베이킹·포스트 프로세싱 씬
FabSim-P3Viz-Complete/   Phase 3 완성본 — DT 시각화(상태 셰이더·히트맵·라벨·데칼·카메라)
FabSim-P4-Complete/      Phase 4 완성본 — 대량 반송차 최적화 실험장
FabSim-P5-Complete/      Phase 5 완성본 — DES 구동 미니 팹 시뮬레이터
plans/                   보강 트랙(Phase 6~10) 구현 계획 문서
tools/, serve.py, *.bat  로컬 실행·정리 스크립트
```

## 학습 자료 보기

- **온라인**: https://jaykaychoi.github.io/dt1/ — 어디서나 읽기용. 영상 임베드 정상 재생.
- **로컬**: 저장소를 clone한 뒤 `open-docs.bat` 실행. 로컬 서버로 열려 "유니티
  프로젝트 열기" 버튼과 영상까지 동작한다(원격 사이트에선 유니티 버튼 비활성).

## 실행 가능한 예제

```bash
# SimPy DES 예제 (Python 3 + simpy)
pip install simpy
python examples/phase2-simpy/02_transport_line.py

# C# DES 엔진 데모 (.NET SDK)
dotnet run --project examples/phase2-descore/DesCore.Demo

# 배차 정책 실험 랩 — 반복·95% 신뢰구간·대응 t-검정·warm-up
dotnet run --project examples/fleet-dispatch -c Release

# DesCore 단위·통계 테스트 (Phase 6 완성본 — 전체 통과)
dotnet test examples/phase2-descore/DesCore.Tests

# 같은 테스트의 실습 스켈레톤 (처음엔 5 실패 / 1 통과 → 채워서 6/6)
dotnet test examples/phase2-descore/DesCore.Tests.Practice

# 교통 제어·데드락 실험 랩 (구간 점유 → 처리량 꺾임, 데드락 유발·탐지)
dotnet run --project examples/fleet-traffic -c Release

# 경로 탐색 심화 랩 (다익스트라 vs A*, 정적 vs 정체 인지 라우팅)
dotnet run --project examples/fleet-routing -c Release

# 트윈 브리지 — 시뮬레이션 상태를 NDJSON 이벤트 스트림으로 굽기
dotnet run --project examples/twin-bridge/Recorder -c Release -- --out examples/twin-bridge/sample.ndjson --until 3600 --seed 7

# 트윈 브리지 — 로그를 배속 30x로 TCP 재생(nc localhost 5088로 수신)
dotnet run --project examples/twin-bridge/ReplayServer -c Release -- --file examples/twin-bridge/sample.ndjson --port 5088 --speed 30
```

Phase 7 실습 스켈레톤은 `examples/fleet-traffic-practice`·`fleet-routing-practice`에서
같은 방식으로 실행한다(핵심 로직이 TODO 스텁이라 완성본과 수치가 어긋나 미완성이 드러난다).

각 예제 폴더의 `README.md`에 시나리오·검증된 출력·읽는 법이 정리돼 있다.

## Unity 프로젝트

Unity **6000.3.8f1** + URP에서 연다. `FabSim/`은 직접 실습하는 프로젝트,
`FabSim-P*-Complete/`는 페이즈별 완성 참고 프로젝트다. 각 프로젝트의 씬은
에디터 스크립트(`Assets/Editor/`)로 재생성할 수 있다.

## 정리

학습이 끝나면 `cleanup.bat`으로 로컬 시스템 변경(유니티 열기용 URL 프로토콜 등록 등)을
되돌린다.
