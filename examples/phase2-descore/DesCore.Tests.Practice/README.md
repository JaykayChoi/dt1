# DesCore.Tests.Practice — 테스트 실습 스켈레톤

`docs/phase6/03-practice.html` 실습의 콘솔 과제. **그대로 컴파일되고 `dotnet test`가
돌아가되, 스텁 테스트는 실패**해 미완성이 드러나는 골격이다. 정답지는 옆의 완성본
`../DesCore.Tests/`다 — 베끼는 곳이 아니라 막혔을 때 대조하는 곳이다.

## 구성

`EventQueueTests.PopOnEmptyThrows` 하나만 완성 예시로 채워져 있다(처음부터 green).
이 형식을 참고해 나머지 5개 `// TODO(실습 N)` 스텁을 실제 `Assert`로 채운다.

| 순서 | 테스트 | 검증 대상 | 완료 기준 |
|---|---|---|---|
| 실습 1 | `EventQueueTests.PopReturnsEventsInNonDecreasingTime` | FEL이 시각 비내림차순으로 나오는지 | Pop한 Time이 항상 직전 이상 |
| 실습 2 | `EventQueueTests.SameTimeEventsPopInSequenceOrder` | 동시각 tie-break(결정성) | 같은 Time에서 Sequence 오름차순 |
| 실습 3 | `SimResourceTests.UtilizationMatchesHandComputedValue` | 가동률 시간 적분 | 손계산 0.5와 `Within(1e-9)` |
| 실습 4 | `SimulationTests.EventsExecuteAtScheduledTime` | 클록 점프·실행 순서 | 3,5,8 순서·각 시점 Now 정확 |
| 실습 5 | `MM1CrossValidationTests.MeanWaitInQueueMatchesTheoreticalWithin95CI` | 통계적 검증 | 이론 Wq=1.0이 95% CI 안 |

## 실행

```
dotnet test examples/phase2-descore/DesCore.Tests.Practice
```

처음(스텁 그대로):

```
Failed!  - Failed: 5, Passed: 1, Skipped: 0, Total: 6
```

전부 채운 뒤:

```
Passed!  - Failed: 0, Passed: 6, Skipped: 0, Total: 6
```

## 막히면

같은 이름의 완성 테스트가 `../DesCore.Tests/`에 있다. 실습 5(M/M/1)는 반복 루프와
`MeanCi95` 계산이 핵심이니 `../DesCore.Tests/MM1CrossValidationTests.cs`의
`RunReplication`·`MeanCi95`를 대조한다. 통계 테스트가 빨간색이면 반복 수·warm-up
제외·시드를 먼저 의심한다.
