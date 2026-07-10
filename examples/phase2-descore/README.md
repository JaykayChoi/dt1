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
DesCore.Tests/           NUnit 단위·통계 테스트 (완성본, Phase 6)
DesCore.Tests.Practice/  같은 테스트의 실습 스켈레톤 — TODO 스텁을 직접 채운다
```

## 실행

```
dotnet run --project DesCore.Demo
```

## 테스트 (Phase 6)

DesCore 엔진의 정렬 불변식·클록 정확성·가동률 적분을 자동화 테스트로 못 박고,
M/M/1 대기행렬을 시뮬레이션해 이론값을 95% 신뢰구간으로 재현하는지 검정한다.

```
dotnet test DesCore.Tests             # 완성본 — 전체 통과
dotnet test DesCore.Tests.Practice    # 실습 스켈레톤 — 처음엔 5 실패 / 1 통과
```

`DesCore.Tests`는 12개 테스트로, `EventQueue`가 (시각, 일련번호) 오름차순을 지키는지,
`Simulation` 클록이 예약 시각에 정확히 사건을 실행하는지, `SimResource` 가동률이
손계산 값과 일치하는지, 그리고 `MM1CrossValidationTests`가 λ=0.5·μ=1.0 M/M/1의
이론 대기시간 Wq=1.0을 40회 반복의 95% CI로 감싸는지를 검증한다(시드 1..40 고정으로
결정적). 통계 테스트지만 시드를 고정해 플레이키하지 않다.

`DesCore.Tests.Practice`는 같은 테스트의 뼈대다 — `EventQueueTests.PopOnEmptyThrows`
하나만 완성 예시로 green이고, 나머지 5개는 `// TODO(실습 N)` 스텁이라 처음 실행하면
"5 실패 / 1 통과"로 미완성이 드러난다. 채워 나가며 완성본과 대조하는 것이 실습이다.
해설·실습 안내는 `docs/phase6/`에 있다.

## 설계 노트

- SimPy는 코루틴(제너레이터) 기반, DesCore는 **콜백 기반**이다 — "가공이 끝나면
  이걸 실행해라"를 `Schedule(delay, action)`으로 예약하는 연쇄로 흐름을 만든다.
  해설은 `docs/phase2/04-descore.html`.
- `SimResource.Release()`는 대기자가 있으면 점유 수를 유지한 채 다음 대기자에게
  바로 넘긴다(핸드오프). 가동률은 "사용 중 수 × 경과 시간"의 시간 적분으로 계산.
- 같은 파라미터의 SimPy 구현과 통계(처리량·리드타임·병목 위치)가 유사하게 나오는
  것으로 엔진을 검증한다.
