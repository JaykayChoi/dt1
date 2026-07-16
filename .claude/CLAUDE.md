<!-- OMC:START -->
<!-- OMC:VERSION:4.14.3 -->

# oh-my-claudecode - Intelligent Multi-Agent Orchestration

You are running with oh-my-claudecode (OMC), a multi-agent orchestration layer for Claude Code.
Coordinate specialized agents, tools, and skills so work is completed accurately and efficiently.

<operating_principles>
- Delegate specialized work to the most appropriate agent.
- Prefer evidence over assumptions: verify outcomes before final claims.
- Choose the lightest-weight path that preserves quality.
- Consult official docs before implementing with SDKs/frameworks/APIs.
</operating_principles>

<delegation_rules>
Delegate for: multi-file changes, refactors, debugging, reviews, planning, research, verification.
Work directly for: trivial ops, small clarifications, single commands.
Route code to `executor` (use `model=opus` for complex work). Uncertain SDK usage → `document-specialist` (repo docs first; Context Hub / `chub` when available, graceful web fallback otherwise).
</delegation_rules>

<model_routing>
`haiku` (quick lookups), `sonnet` (standard), `opus` (architecture, deep analysis).
Direct writes OK for: `~/.claude/**`, `.omc/**`, `.claude/**`, `CLAUDE.md`, `AGENTS.md`.
</model_routing>

<skills>
Invoke via `/oh-my-claudecode:<name>`. Trigger patterns auto-detect keywords.
Tier-0 workflows include `autopilot`, `ultrawork`, `ralph`, `team`, and `ralplan`.
Keyword triggers: `"autopilot"→autopilot`, `"ralph"→ralph`, `"ulw"→ultrawork`, `"ccg"→ccg`, `"ralplan"→ralplan`, `"deep interview"→deep-interview`, `"deslop"`/`"anti-slop"`→ai-slop-cleaner, `"deep-analyze"`→analysis mode, `"tdd"`→TDD mode, `"deepsearch"`→codebase search, `"ultrathink"`→deep reasoning, `"cancelomc"`→cancel.
Team orchestration is explicit via `/team`.
Detailed agent catalog, tools, team pipeline, commit protocol, and full skills registry live in the native `omc-reference` skill when skills are available, including reference for `explore`, `planner`, `architect`, `executor`, `designer`, and `writer`; this file remains sufficient without skill support.
</skills>

<verification>
Verify before claiming completion. Size appropriately: small→haiku, standard→sonnet, large/security→opus.
If verification fails, keep iterating.
</verification>

<execution_protocols>
Broad requests: explore first, then plan. 2+ independent tasks in parallel. `run_in_background` for builds/tests.
Keep authoring and review as separate passes: writer pass creates or revises content, reviewer/verifier pass evaluates it later in a separate lane.
Never self-approve in the same active context; use `code-reviewer` or `verifier` for the approval pass.
Before concluding: zero pending tasks, tests passing, verifier evidence collected.
</execution_protocols>

<hooks_and_context>
Hooks inject `<system-reminder>` tags. Key patterns: `hook success: Success` (proceed), `[MAGIC KEYWORD: ...]` (invoke skill), `The boulder never stops` (ralph/ultrawork active).
Persistence: `<remember>` (7 days), `<remember priority>` (permanent).
Kill switches: `DISABLE_OMC`, `OMC_SKIP_HOOKS` (comma-separated).
</hooks_and_context>

<cancellation>
`/oh-my-claudecode:cancel` ends execution modes. Cancel when done+verified or blocked. Don't cancel if work incomplete.
</cancellation>

<worktree_paths>
State: `.omc/state/`, `.omc/state/sessions/{sessionId}/`, `.omc/notepad.md`, `.omc/project-memory.json`, `.omc/plans/`, `.omc/research/`, `.omc/logs/`
</worktree_paths>

## Setup

Say "setup omc" or run `/oh-my-claudecode:omc-setup`.

<!-- OMC:END -->

<!-- User customizations -->
## Project Overview (프로젝트 성격)

AX사업본부 **DT개발팀(Digital Twin 개발팀)** 입사 전 사전 학습용 프로젝트.
팀 업무는 공장의 물류·반송 시스템을 Unity 기반 3D 디지털 트윈으로 구현하는 것이며, 이 프로젝트에서 그 도메인과 기술 스택을 미리 학습한다.

**학습 축 1 — 도메인 (공장 자동화·물류)**
- 반송 장비: OHT(천장 반송), AGV(무인 운반차), AMR(자율주행 로봇)
- 제어 계층: MES(생산 실행) → MCS(반송 제어) → Factory Fleet Management(배차·교통제어) → 장비
- 방법론: 이산사건시뮬레이션(DES) — 물류 시뮬레이션의 핵심 기법

**학습 축 2 — Unity 기술 (시각화·성능)**
- PBR(Physically Based Rendering), 라이트 베이킹, 포스트 프로세싱
- 내장 Profiler / Frame Debugger를 이용한 성능 분석·최적화

**학습 목표 프로젝트**: 미니 팹/공장 물류 시뮬레이터 — 공장 레이아웃에 OHT 레일·AGV 경로를 구성하고, DES 로직으로 반송을 시뮬레이션하며, PBR + 구운 라이트로 씬을 구성하고, 장비 수를 늘려가며 Profiler로 병목을 최적화한다.

## Learning Roadmap (학습 로드맵)

### Phase 1 — 도메인 개념: 공장 자동화·물류 (약 1주)
- AMR / AGV / OHT의 차이와 각각의 역할, 적용 환경
- 반도체 팹 AMHS 구조: FOUP, 스토커(Stocker), 인터베이/인트라베이 반송
- 제어 계층 흐름: MES(생산 지시) → MCS(반송 제어) → Fleet Management(배차·교통제어) → 장비
- Fleet Management 핵심 문제: 배차(dispatching), 경로 계획, 교통 제어, 데드락 방지, 충전 스케줄링
- SEMI 표준은 이름과 역할만: E84(반송 핸드오프), E88(스토커), E82(IBSEM)
- 산출물: `docs/phase1/` 스테이션 6페이지(용어집 포함) + `docs/assets/terms.js` 용어 툴팁 사전

### Phase 2 — 이산사건시뮬레이션(DES) (약 1주)
- 핵심 개념: 시뮬레이션 클록, 이벤트 큐(FEL), 상태 변수, 리소스 점유/대기, 통계 수집
- 큐잉 이론은 감만: 도착률/서비스율, 병목, 대기시간-가동률 관계
- SimPy(Python)로 실습: 스테이션 3~4개 + 반송 장비 2~3대 모델 — 처리량, 평균 대기시간 측정
- C#으로 미니 DES 엔진 작성(우선순위 큐 기반 이벤트 스케줄러) — Phase 5에서 Unity 코어로 재사용
- 산출물: SimPy 예제 스크립트 + C# DES 코어 라이브러리

### Phase 3 — Unity PBR 렌더링 & 라이팅 (약 1~2주)
- URP 프로젝트 셋업, PBR 머티리얼 이해: Base Map / Metallic / Smoothness / Normal / AO
- 라이팅 모드(Realtime / Mixed / Baked) 차이, 라이트맵 베이킹, Light Probes, Reflection Probes
- 포스트 프로세싱(URP Volume): Bloom, Tonemapping(ACES), Color Adjustments, SSAO, Vignette
- 실습: 무료 공장/창고 에셋으로 팹 내부 씬 구성 → 라이트 베이크 → 포스트 프로세싱으로 마감
- 산출물: 라이팅·포스트 적용 전/후 비교 가능한 공장 씬

### Phase 4 — Unity 성능 분석·최적화 (약 1주)
- Profiler: CPU 타임라인 읽기, GC Alloc 추적, Memory Profiler
- Frame Debugger: 드로우콜 단위 렌더링 관찰, SRP Batcher / GPU Instancing / Static Batching 비교
- 최적화 기법: LOD, 오클루전 컬링, 오브젝트 풀링, 대량 Transform 갱신(Jobs/Burst 맛보기)
- 실습: 장비 500~1000대 씬에서 60fps 목표로 병목 제거
- 산출물: 최적화 전/후 프로파일링 기록

### Phase 5 — 통합: 미니 팹 시뮬레이터 (약 2주)
- 레일/경로를 웨이포인트 그래프로 구성, OHT/AGV 이동 구현
- Phase 2의 C# DES 코어와 Unity 시각화 연결 (시뮬레이션 시간 ↔ 실시간 배속 스케일링)
- 간단한 MCS 흐름: 반송 명령 생성 → 배차 규칙(최근접 유휴 장비 등) → 픽업/이동/드롭
- 통계 HUD: 처리량, 평균 반송 시간, 장비 가동률
- 마무리: Phase 4 기법으로 프로파일링·최적화 패스

### Phase 6 — 신뢰성: 테스트와 시뮬레이션 검증 (약 1주)
- NUnit / Unity Test Framework(EditMode·PlayMode), asmdef 어셈블리 분리(Runtime/Editor/Tests)
- 시뮬레이션 출력 분석: V&V, warm-up 제거, 독립 반복, 95% 신뢰구간, 공통 난수(CRN)·대응 비교
- 산출물: `examples/phase2-descore/DesCore.Tests`(+실습 스켈레톤 `DesCore.Tests.Practice`) + fleet-dispatch 반복·CI 업그레이드 + `FabSim-P6-Complete`(asmdef·테스트) + 실습 가이드 스테이션

### Phase 7 — Fleet 심화: 교통·데드락·라우팅·배차 (약 2~3주)
- 교통 제어: 구간 점유(zone control), 밀도-흐름 관계와 처리량 붕괴
- 데드락: 예방(단방향 루프)·회피(자원 순서화·뱅커스 개념)·탐지(wait-for 사이클 DFS)와 복구
- 경로 탐색: 다익스트라 → A*, 동적 혼잡 비용·재계획 — WHCA*/CBS·시간창 예약은 서베이 수준
- 배차 심화: 대기 aging 가중 점수(기아 방지), P95 지표, 핫 로트 선점, 충전 스케줄링(임계값 vs 기회)
- 산출물: `examples/fleet-traffic`·`examples/fleet-routing` 랩(+각 `-practice` 실습 스켈레톤) + fleet-dispatch 확장 + `FabSim-P7-Complete`(교통 제어 시각화) + 실습 가이드 스테이션

### Phase 8 — 디지털 트윈 연결 (약 2주)
- 시뮬레이션 vs 트윈: 누가 시간을 만드는가, 소스 추상화(IFleetSource), DT 성숙도(미러링→섀도→트윈)
- 이벤트 스트림: NDJSON 프로토콜, 레코더·리플레이 서버(배속/시킹), TCP/WebSocket
- 실시간 브리지: 플레이아웃 버퍼, 순서/유실 처리, 스냅샷+델타, 재접속·재동기화, 고장 주입
- UI Toolkit 운영자 대시보드: UXML/USS·BEM, 런타임 바인딩, ListView 가상화, 시뮬레이션 컨트롤 바
- 산출물: `examples/twin-bridge`(Recorder·ReplayServer, +`twin-bridge-practice` 실습 스켈레톤) + `FabSim-P8-Complete`(트윈 클라이언트·대시보드) + 실습 가이드 스테이션

### Phase 9 — 대규모 성능 심화 (약 2주)
- C# Job System + Burst: NativeArray, IJobParallelForTransform, JobHandle, Unity.Mathematics — ECS/DOTS는 지도만
- Memory Profiler 스냅샷 diff·누수 사냥, additive scene 스트리밍, Addressables 기본
- GPU-driven 맛보기: 컴퓨트 셰이더 위치 갱신, RenderMeshIndirect 대량 인스턴스
- 산출물: `FabSim-P9-Complete` — {500~10,000대} × {Mono/Job/Job+Burst/GPU-driven} 측정표 + 실습 가이드 스테이션(FabSim용 스켈레톤 코드 블록·측정 기록 템플릿)

### Phase 10 — 실자산 파이프라인 (약 1주)
- FBX 임포트 설정(스케일·노멀·압축·Read/Write), 재질 수=서브메시=드로우콜, 텍스처 압축, 프리팹 Variant
- 임포트 자동화: AssetPostprocessor 폴더 규칙, EditorWindow 임포트 리포트, Pixyz류 CAD 변환의 실무 위치
- 산출물: `FabSim-P10-Complete` — 결함 FBX 교보재 + 자동화 스크립트 + 배칭 회복 전/후 수치 + 실습 가이드 스테이션(FabSim에서 직접 교정·자동화)

### Phase 11 — OpenUSD (약 1~2주)
- USD의 두 얼굴: 3D 씬 기술 포맷이자 레이어 합성 컴포지션 엔진. 왜 산업 표준 씬 교환 포맷이 됐나(AOUSD)
- 데이터 모델: Stage · Prim · Property(Attribute/Relationship) · SdfPath · Kind
- 컴포지션 아크: sublayer·inherit·variantSet·reference·payload·specialize와 LIVRPS 강도 순서
- 스키마·지오메트리·머티리얼: UsdGeom, PointInstancer(대량 인스턴싱), UsdShade/UsdPreviewSurface·MaterialX
- 고급: 페이로드 지연 로드, native instancing, flatten 트레이드오프, USD 툴체인(usdcat·usdchecker·usdrecord), 스테이지 순회 최적화
- 관통선: FabSim(C# DES) → USD(.usda) → Omniverse. `.usda`는 텍스트라 SDK 없이 직렬화 가능(쓰기), 검증은 usd-core(읽기)
- 산출물: `docs/phase11/` 7 스테이션(실습·FabSim→USD 익스포트 스테이션 포함) + `examples/openusd-lab`(Python usd-core 정답, 실행·검증됨; `render_scene.py`로 씬 이미지 렌더) + `examples/openusd-lab-practice`(TODO 스켈레톤) + `examples/fabsim-usd-export`(C# 헤드리스 익스포터 — Sim/*.cs 링크, fab.usda 생성·검증됨)

### Phase 12 — NVIDIA Omniverse (약 2주)
- Omniverse = OpenUSD 기반 3D 협업·시뮬레이션 개발 플랫폼(Kit SDK). RTX 실시간 렌더링, 산업 디지털 트윈에서의 위치
- 핵심 구성요소: Kit(확장 기반 앱 프레임워크) · USD Composer · Nucleus(라이브 협업) · RTX Renderer · omni.ui · Connectors
- USD 상호운용: Phase 11의 팹 씬 USD를 Omniverse에서 열기, 라이브 레이어로 다중 클라이언트 편집(Phase 8 트윈과 연결)
- Kit 확장 개발: extension.toml, omni.ext.IExt(on_startup/on_shutdown), omni.ui 창, omni.usd 스테이지 조작
- 고급: OmniGraph(비주얼 스크립팅·데이터 흐름), PhysX 물리, 실시간 외부 데이터 바인딩(텔레메트리 → USD 속성 → 화면)
- 산출물: `docs/phase12/` 6 스테이션(실습 스테이션 포함) + `examples/omniverse-kit-ext`(Kit 확장 소스 + USD 자산 + 설치·실행 가이드). RTX GPU가 필요해 실행·검증은 학습자 몫

Phase 6~10은 구현 완료됐다 — 교재는 `docs/phase6~10/`, 콘솔 랩은 `examples/`(`fleet-traffic`·`fleet-routing`·`twin-bridge`·`common/SimStats` 등), 완성본은 `FabSim-P6~P10-Complete`에 있다.
Phase 11~12(플랫폼선)도 구현 완료됐다 — 교재는 `docs/phase11~12/`, 랩은 `examples/openusd-lab`(+`-practice`)·`examples/omniverse-kit-ext`에 있다. 이 두 페이즈는 FabSim Unity 클론이 아니라 Python usd-core 랩과 Omniverse Kit 확장으로 실습한다.

### 참고 자료
- SimPy 공식 문서 (Python DES 프레임워크)
- OpenTCS — 오픈소스 AGV fleet management, 관제 아키텍처 참고
- Unity Learn: Creative Core (Lighting / Shading / Post-processing 코스)
- Unity 매뉴얼: Lightmapping, URP Volumes, Profiler, Frame Debugger
- 반도체 팹 OHT 영상 자료 (Daifuku, Murata Machinery 제품 영상 등)
- Unity 매뉴얼: Job System·Burst, UI Toolkit, Memory Profiler, AssetPostprocessor
- Law & Kelton, 『Simulation Modeling and Analysis』 — 출력 분석(warm-up·반복·신뢰구간) 장
- OpenUSD 공식 문서(openusd.org) · Pixar USD API · AOUSD(Alliance for OpenUSD)
- `usd-core` (PyPI) — Pixar USD 파이썬 바인딩 경량 배포판
- NVIDIA Omniverse 개발자 문서(docs.omniverse.nvidia.com, developer.nvidia.com/omniverse) — Kit·USD Composer·Nucleus·OmniGraph

## DT1 Project Rules

### Git Rules
- **Only commit/push when the user explicitly requests it.** Never auto-commit or push on your own initiative.

### MD File Writing Rules
- **No history**: do not write dates, strikethroughs, "changed from X to Y", "previously", "now uses", commit hashes, or status tables tracking when something was done. If content is now stale, **delete it entirely and write only the current state**. The doc must read as a fresh snapshot of how the system *is*, not a log of how it got there.
- When spawning a subagent for a specific domain, embed relevant rules in the prompt — agents don't auto-load rule files
- Do not create separate `feedback`-type memory files. Record feedback/rules directly in this file
- **실습(practice) 문서는 학습자가 Claude/unity-cli 없이 혼자 수행한다는 가정**으로 쓴다. 실습 절차·과정 설명에 "Claude에게 시켜라", "unity-cli로 자동화/배치" 같은 위임·자동화 언급을 넣지 않는다 — 반복 작업도 수동 절차(다중 선택, `Ctrl+D` 복제 등)로 안내한다. (완성본을 만들 때 Claude가 unity-cli를 쓰는 것과는 별개다.)

### MD vs Code-Comment Split (read BEFORE writing or updating any MD)
Before adding content to any `.md`, decide **where it belongs** based on how each surface is loaded into context. Putting content in the wrong place silently breaks enforcement — a rule moved into a code comment is invisible to an agent that never opens that file.

### Unity Project Strategy
- **실습 프로젝트는 1개**: `FabSim/` — 사용자가 Phase 3~10 실습을 직접 수행하는 프로젝트. Claude는 사용자가 요청할 때만 이 프로젝트를 수정한다.
- **실습/완성 이원 구조**: 보강 트랙(6~10)의 각 페이즈는 사용자가 직접 만드는 실습 트랙(페이즈 마지막 `NN-practice.html` 실습 가이드 스테이션 + 콘솔 랩은 `examples/<lab>-practice/` TODO 스켈레톤)과 완성본(정답지)을 쌍으로 제공한다.
- **완성본은 페이즈별 별도 프로젝트**: `FabSim-P3-Complete/` 등 — 각 페이즈의 실습 결과를 Claude가 완성해 둔 참고용 프로젝트.
- **보강 트랙(6~10) 완성본 클론 계보**: `FabSim-P6-Complete`(P5 클론 + asmdef·테스트) → `FabSim-P7-Complete`(P6 클론 + 교통 제어) → `FabSim-P8-Complete`(P7 클론 + 트윈 브리지·대시보드), `FabSim-P9-Complete`(P4 클론 + Jobs/Burst·스트리밍·GPU-driven), `FabSim-P10-Complete`(신규 최소 URP + 자산 파이프라인). 클론 시 `Library/`·`Temp/`·`Logs/`·`obj/`는 복사하지 않는다.

### Unity Editor Automation Rules
- Unity Editor tasks (Inspector wiring, scene edits, component setup) are handled directly via `unity-cli` — never ask the user. See `.claude/rules/UNITY_CLI.md` for commands.
- **Always run `unity-cli status --ignore-version-mismatch` first** before assuming the editor is unavailable. A connection error on one command does not mean the editor is closed.
