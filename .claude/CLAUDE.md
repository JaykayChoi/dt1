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
팀 업무는 공장(반도체 팹 추정)의 물류·반송 시스템을 Unity 기반 3D 디지털 트윈으로 구현하는 것으로 추정되며, 이 프로젝트에서 그 도메인과 기술 스택을 미리 학습한다.

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
- 산출물: `docs/domain-notes.md` — 용어·계층 구조 정리 노트

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

### 참고 자료
- SimPy 공식 문서 (Python DES 프레임워크)
- OpenTCS — 오픈소스 AGV fleet management, 관제 아키텍처 참고
- Unity Learn: Creative Core (Lighting / Shading / Post-processing 코스)
- Unity 매뉴얼: Lightmapping, URP Volumes, Profiler, Frame Debugger
- 반도체 팹 OHT 영상 자료 (Daifuku, Murata Machinery 제품 영상 등)

## DT1 Project Rules

### Git Rules
- **Only commit/push when the user explicitly requests it.** Never auto-commit or push on your own initiative.

### MD File Writing Rules
- **No history**: do not write dates, strikethroughs, "changed from X to Y", "previously", "now uses", commit hashes, or status tables tracking when something was done. If content is now stale, **delete it entirely and write only the current state**. The doc must read as a fresh snapshot of how the system *is*, not a log of how it got there.
- When spawning a subagent for a specific domain, embed relevant rules in the prompt — agents don't auto-load rule files
- Do not create separate `feedback`-type memory files. Record feedback/rules directly in this file

### MD vs Code-Comment Split (read BEFORE writing or updating any MD)
Before adding content to any `.md`, decide **where it belongs** based on how each surface is loaded into context. Putting content in the wrong place silently breaks enforcement — a rule moved into a code comment is invisible to an agent that never opens that file.

### Unity Project Strategy
- **실습 프로젝트는 1개**: `FabSim/` — 사용자가 Phase 3~5 실습을 직접 수행하는 프로젝트. Claude는 사용자가 요청할 때만 이 프로젝트를 수정한다.
- **완성본은 페이즈별 별도 프로젝트**: `FabSim-P3-Complete/` 등 — 각 페이즈의 실습 결과를 Claude가 완성해 둔 참고용 프로젝트. Phase 4·5 시작 시 `FabSim-P4-Complete`, `FabSim-P5-Complete`를 각각 새로 만든다.

### Unity Editor Automation Rules
- Unity Editor tasks (Inspector wiring, scene edits, component setup) are handled directly via `unity-cli` — never ask the user. See `.claude/rules/UNITY_CLI.md` for commands.
- **Always run `unity-cli status --ignore-version-mismatch` first** before assuming the editor is unavailable. A connection error on one command does not mean the editor is closed.
