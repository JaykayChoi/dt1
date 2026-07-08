# Phase 2 — DesCore: C# 미니 DES 엔진

Phase 5에서 Unity 시뮬레이터의 코어로 재사용할 이산사건시뮬레이션 엔진.
`UnityEngine` 무의존 + C# 9 문법 + BCL 최소 사용(우선순위 큐 직접 구현)으로
Unity에 폴더째 복사해도 그대로 컴파일된다.

## 구조

```
DesCore/                 클래스 라이브러리 (엔진 본체)
├── SimEvent.cs          사건 = (시각, 일련번호, 실행할 동작)
├── EventQueue.cs        미래 사건 목록(FEL) — 이진 최소 힙
├── Simulation.cs        클록 + 사건 루프 (Schedule / Run)
├── SimResource.cs       용량 제한 자원 — 콜백 기반 Request/Release, 가동률 측정
└── RandomExtensions.cs  지수분포 샘플링
DesCore.Demo/            콘솔 데모 — 미니 반송 라인 (SimPy 예제 02와 동일 시나리오)
```

## 실행

```
dotnet run --project DesCore.Demo
```

## 설계 노트

- SimPy는 코루틴(제너레이터) 기반, DesCore는 **콜백 기반**이다 — "가공이 끝나면
  이걸 실행해라"를 `Schedule(delay, action)`으로 예약하는 연쇄로 흐름을 만든다.
  해설은 `docs/phase2/04-descore.html`.
- `SimResource.Release()`는 대기자가 있으면 점유 수를 유지한 채 다음 대기자에게
  바로 넘긴다(핸드오프). 가동률은 "사용 중 수 × 경과 시간"의 시간 적분으로 계산.
- 같은 파라미터의 SimPy 구현과 통계(처리량·리드타임·병목 위치)가 유사하게 나오는
  것으로 엔진을 검증한다.
