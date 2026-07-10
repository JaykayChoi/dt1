# Twin Bridge — 이벤트 스트림 레코더와 리플레이 서버

Phase 5의 미니 팹은 시뮬레이션이 Unity 안에 내장돼 있었다. 이 랩은 그 시뮬레이션의 상태
변화를 **NDJSON 이벤트 스트림**으로 밖으로 꺼내(레코더), 그 로그를 실물 피드처럼 **재생**한다
(리플레이 서버). Unity의 `TwinFeedClient`가 이 스트림을 받아 **같은 뷰**를 구동하면, 시뮬레이터가
외부 소스로도 돌아가는 미니 디지털 트윈이 된다.

## 구조

```
twin-bridge/
├── Recorder/         헤드리스 레코더 — FabModel을 Unity 없이 돌려 NDJSON을 굽는다
│   ├── EventRecorder.cs   FabModel 이벤트 → JSON 라인 + 주기 SNAPSHOT
│   └── Program.cs         FabLayout·FabModel 구성 → 구독 → 파일 기록
├── ReplayServer/     리플레이 TCP 서버 — 로그를 타임스탬프 간격 × 배속으로 송출
│   ├── NdjsonPacer.cs     라인 간 지연 = (t_i − t_{i-1}) / 배속
│   └── Program.cs         TcpListener + 고장 주입(--drop/--cut)
├── sample.ndjson     재생 가능한 샘플 로그(레코더 산출물)
└── README.md
```

Recorder는 `FabSim-P8-Complete/Assets/Scripts/Sim/*.cs`를 **링크**(복사 아님)해, 헤드리스가 굽는
로그가 Unity의 소스가 낳는 것과 구조가 동일하도록 단일 진리 원천을 지킨다.

## 실행

```
# 1) 로그 굽기 (같은 시드면 같은 로그 — 결정성)
dotnet run --project examples/twin-bridge/Recorder -c Release -- \
  --out examples/twin-bridge/sample.ndjson --until 3600 --seed 7 --vehicles 4 --snapshot 60

# 2) 로그를 배속 30x로 TCP 송출
dotnet run --project examples/twin-bridge/ReplayServer -c Release -- \
  --file examples/twin-bridge/sample.ndjson --port 5088 --speed 30
```

접속: `nc localhost 5088` 또는 Unity `TwinFeedClient`. HELLO가 즉시 오고, 이후 라인이
타임스탬프 간격(÷배속)대로 도착한다.

## 프로토콜 요약 (NDJSON)

한 줄에 완결된 JSON 객체 하나(`\n` 프레이밍). 공통 봉투: `n`(단조 시퀀스, 유실 탐지),
`t`(시뮬 시각 [초]), `type`.

| type | 뜻 | 필드 | VehicleAgent 매핑 |
|---|---|---|---|
| `HELLO` | 스트림 헤더 | `layout`·`nodeCount`·`portNodes`·`vehicleCount` | 클라이언트가 로컬 그래프와 대조 |
| `JOB_CREATE` | 명령 생성 | `jid`·`from`·`to` | TransportJob.Id/FromPort/ToPort |
| `DISPATCH` | 배차 | `jid`·`vid` | Phase→ToPickup, BusyStartedAt=t |
| `EDGE_DEPART` | 엣지 주행 시작 | `vid`·`from`·`to`·`eta` | EdgeFromNode/EdgeToNode/EdgeArriveAt |
| `EDGE_ARRIVE` | 경로 끝 도착 | `vid`·`node` | NodeId, IsMoving=false |
| `PICKUP` | 픽업 완료 | `vid`·`jid` | Phase→Carrying (FOUP on) |
| `JOB_COMPLETE` | 반송 완료 | `vid`·`jid` | Phase→Idle, CompletedJobs++ |
| `SNAPSHOT` | 전체 상태 | `vehicles[]`·`stats{}` | 재동기화·late-join 기준점 |

## 검증된 출력

```
레코딩 완료 → examples/twin-bridge/sample.ndjson
  파라미터: until=3600s · seed=7 · vehicles=4 · jobInterval=25s · snapshot=60s
  완료 반송 158건 · 라인 2602줄
```

- 첫 줄 `HELLO`, `nodeCount:12`, `portNodes:[1,2,3,7,8,9]`.
- `SNAPSHOT`이 60초 주기로 존재, `n`이 0부터 빠짐없이 단조 증가.
- **결정성**: 같은 시드로 두 번 구우면 바이트 단위로 동일한 파일.
- **일치성**: 로그의 `JOB_COMPLETE` 수(158) = 헤드리스 `FabModel.CompletedJobs`(158).

## 이 로그에서 읽어야 할 것

- **엣지 이벤트가 곧 View 보간 입력** — `EDGE_DEPART`의 `from`/`to`/`eta`는 `VehicleAgent`의
  `EdgeFromNode`/`EdgeToNode`/`EdgeArriveAt`와 1:1이다. Unity의 `EvaluatePosition`이 이 시각들로
  `Vector3.Lerp`하므로, **스트림이 이 필드를 실어 나르면 보간 View는 소스가 바뀌어도 코드 한 줄
  안 고쳐도 된다** — 이것이 트윈 브리지의 설계 축이다.
- **스냅샷 + 델타** — 변화(델타)만 이벤트로 보내 대역폭을 아끼고, 가끔 전체(SNAPSHOT)로 기준을
  맞춘다. 비디오 코덱의 키프레임/P프레임과 같은 구조. SNAPSHOT은 재동기화·뒤늦은 합류의 기준점이다.

## 고장 주입

```
--drop 0.02   # 델타 라인을 2% 확률로 스킵 → 수신 n에 갭 발생(유실 재현)
--cut 20      # 20초 후 연결 강제 종료(서버 다운 재현) → 재접속 시 다음 SNAPSHOT에서 재동기화
```

## 배경 개념

트윈 vs 시뮬레이션·이벤트 스트림·버퍼링·UI Toolkit은 `docs/phase8/`에서 다룬다. 실습 스켈레톤은
`examples/twin-bridge-practice`(직렬화·페이싱이 TODO)에서 출발한다.
