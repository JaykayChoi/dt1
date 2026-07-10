# Fleet Traffic — 교통 제어 실습 스켈레톤

`docs/phase7/05-practice.html` 실습의 콘솔 과제. **그대로 컴파일·실행되되**, `SegmentController`의
핵심 메서드가 `// TODO(실습 N)` 스텁이라 교통 제어가 사실상 꺼진 것처럼 굴러 **미완성이
수치로 드러난다**. 정답지는 완성본 `examples/fleet-traffic/`다 — 베끼지 말고 채운 뒤 대조한다.

## 채울 것 — `SegmentController.cs`

| 순서 | 메서드 | 할 일 | 완료 신호 |
|---|---|---|---|
| 실습 1 | `Acquire`/`Release`/`HolderOf`/`WaitingEdgeOf` | 구간 점유·대기 큐·승계를 구현 | 실험 A ON에서 처리량이 꺾인다 |
| 실습 2 | `DetectDeadlockCycle` | wait-for 그래프 DFS로 사이클 탐지 | 실험 B에서 데드락을 잡는다 |

## 실행

```
dotnet run --project examples/fleet-traffic-practice -c Release
```

## 완료 판정 — 스텁 vs 완성

**스텁 상태(처음)** — 점유를 무시하므로 교통 제어 ON이 OFF와 똑같이 단조 증가하고, 데드락을
못 잡는다:

```
[교통 제어 ON]  12  434.9 ± 9.5   66.0 ± 0.6    ← 꺾이지 않음(= OFF와 동일) = 미완성
[데드락 실험]  양방향(무대책)   아니오   —   —    ← 못 잡음 = 미완성
```

**완성 후** — 완성본 `examples/fleet-traffic/`과 같은 수치가 나온다:

```
[교통 제어 ON]  12  234.1 ± 3.4   176.5 ± 2.5   ← 꺾임(처리량 붕괴 + 반송 폭증)
[데드락 실험]  양방향(무대책)   예   t=4.0 s   [0, 1]   ← 탐지
```

## 막히면

완성본 `../fleet-traffic/SegmentController.cs`의 같은 메서드를 대조한다. `Acquire`는
비었으면 즉시 승인·아니면 대기 큐 + `waitingEdge` 기록, `Release`는 대기자에게 승계,
`DetectDeadlockCycle`은 "대기 차량 → 점유 차량" 간선의 사이클을 DFS로 찾는 것이 핵심이다.
