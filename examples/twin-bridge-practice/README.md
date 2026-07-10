# Twin Bridge — 실습 스켈레톤

`docs/phase8/05-practice.html` 실습의 콘솔 과제(M1). **그대로 컴파일·실행되되**, 두 곳이
`// TODO(실습 1)` 스텁이라 미완성이 드러난다. 정답지는 완성본 `examples/twin-bridge/`다.

## 채울 것 (두 곳)

| 파일 | 메서드 | 할 일 | 완료 신호 |
|---|---|---|---|
| `Recorder/EventRecorder.cs` | `On*` 이벤트 핸들러 | 각 델타 라인에 필드(jid·from·to·vid·eta·node) 추가 | 로그 델타 라인에 필드가 채워진다 |
| `ReplayServer/NdjsonPacer.cs` | `ComputeDelay` | `(curT − prevT) / speed` 반환 | 라인이 타임스탬프 간격대로 페이싱된다 |

## 실행

```
dotnet run --project examples/twin-bridge-practice/Recorder -c Release -- --out /tmp/my.ndjson --until 200
dotnet run --project examples/twin-bridge-practice/ReplayServer -c Release -- --file examples/twin-bridge/sample.ndjson --speed 30
```

## 완료 판정 — 스텁 vs 완성

**스텁 상태(처음)** — 델타 라인이 type·n·t만 있고 **필드가 비어 있다**(반쪽 로그):

```
{"n":1,"t":12.081,"type":"JOB_CREATE"}          ← 필드 없음 = 미완성
{"n":2,"t":12.081,"type":"DISPATCH"}
```

그리고 리플레이 서버는 `ComputeDelay`가 0이라 **전체 로그가 한꺼번에 쏟아진다**(페이싱 없음).

**완성 후** — 완성본 `examples/twin-bridge/sample.ndjson`과 같은 구조:

```
{"n":1,"t":12.081,"type":"JOB_CREATE","jid":0,"from":9,"to":7}   ← 필드 완성
```

리플레이 서버는 라인이 `t` 간격 ÷ 배속대로 도착한다.

## 막히면

- 직렬화: 완성본 `../twin-bridge/Recorder/EventRecorder.cs`의 각 `On*` 메서드가 어떤 필드를
  어떤 `VehicleAgent`/`TransportJob` 필드에서 뽑는지 대조한다.
- 페이싱: 완성본 `../twin-bridge/ReplayServer/NdjsonPacer.cs`의 `ComputeDelay`.
- `diff <(내 로그) ../twin-bridge/sample.ndjson`로 구조를 맞춰 본다(시각·순서는 시드가 같으면 동일).
