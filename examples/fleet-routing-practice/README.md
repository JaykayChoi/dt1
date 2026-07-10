# Fleet Routing — A* 실습 스켈레톤

`docs/phase7/05-practice.html` 실습의 콘솔 과제. **그대로 컴파일·실행되되**, `RailGraph`의
`FindPathAStar`가 `// TODO(실습 3)` 스텁이라 A* 효율 개선이 안 나타나 **미완성이 수치로
드러난다**. 정답지는 완성본 `examples/fleet-routing/`다.

## 채울 것 — `RailGraph.FindPathAStar`

`f = g + h`(g=출발부터의 실비용, h=목표까지 직선거리)로 open 노드 중 최소 f를 골라 확장하는
A*를 구현한다. closed 처리한 노드 수를 `expandedCount`로 센다. 허용 가능 휴리스틱이므로
다익스트라와 **같은 경로를 더 적은 확장으로** 찾아야 한다.

## 실행

```
dotnet run --project examples/fleet-routing-practice -c Release
```

## 완료 판정 — 스텁 vs 완성

**스텁 상태(처음)** — 다익스트라 결과를 그대로 돌려주고 확장을 전체 노드 수로 둬, A*가
다익스트라보다 오히려 많이 확장한다(개선 없음):

```
  다익스트라      18.0    ...
  A*(직선거리)    36.0    ...   ← 개선 없음 = 미완성
```

**완성 후** — 완성본과 같이 A* 확장이 뚜렷이 줄고 경로 길이는 100 % 일치:

```
  다익스트라      18.0    ...
  A*(직선거리)    6.5     ...   100%   ← 약 1/3로 확장 감소
```

실험 B(정적 vs 정체 인지 동적)는 다익스트라만 쓰므로 스텁 상태에서도 완성본과 같은 개선이
나온다 — A*를 채운 뒤 실험 A의 확장 노드가 줄어드는지로 완료를 판정한다.

## 막히면

완성본 `../fleet-routing/RailGraph.cs`의 `FindPathAStar`를 대조한다. 핵심은 open 집합에서
`g + heuristic`이 최소인 노드를 고르는 것과, 확장(closed) 노드만 세는 것이다.
