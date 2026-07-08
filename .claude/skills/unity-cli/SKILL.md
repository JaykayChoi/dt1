---
name: unity-cli
description: "Control Unity Editor from the terminal using unity-cli. TRIGGER on any mention of '유니티', 'unity', 'Unity' in user messages — whether asking to implement features, fix bugs, create scenes, test gameplay, or any Unity-related work. After writing or editing C# scripts, always use unity-cli to refresh, compile, and check for errors before reporting completion. Also triggers on: compilation check, play mode, run/test game, screenshot, console logs, create scene, create GameObject."
---

# Unity CLI

unity-cli is a command-line tool that controls Unity Editor via HTTP. The Unity connector package runs an HTTP server inside the editor, and the CLI sends commands to it.

## Prerequisites

- Unity Editor must be open with the project loaded
- `com.youngwoocho02.unity-cli-connector` package installed in the Unity project
- `unity-cli` binary installed on the system

## Command Reference

Command syntax: `.claude/rules/UNITY_CLI.md` is authoritative — read it for the full command set.

Two must-know rules before running anything:
- Always run `unity-cli status --ignore-version-mismatch` first — a connection error on one command does not mean the editor is closed.
- Pass `--ignore-version-mismatch` on every command, not only `status`.

## Common Workflows

### After Writing/Editing C# Scripts

1. `unity-cli refresh_unity --compile request`
2. Wait 3-5 seconds
3. `unity-cli console --type error`
4. If no errors, proceed. If errors, fix and repeat.

### Creating a Test Scene Programmatically

1. Write the MonoBehaviour script
2. `unity-cli refresh_unity --compile request` — wait for compilation
3. `unity-cli exec` to create scene, add GameObjects, attach components, save
4. `unity-cli editor play` to test

### Debugging Runtime Issues

1. `unity-cli console --clear true` — clear old logs
2. `unity-cli editor play`
3. Let it run, then: `unity-cli console --type error --stacktrace short`
4. `unity-cli editor stop`

## Related Knowledge Skills

When this skill triggers, also load the relevant knowledge skill based on the task:

| Task | Load Skill |
|------|-----------|
| UI Toolkit work (UXML, USS, VisualElement) | `/unity-dev-toolkit:unity-uitoolkit` |
| UGUI vs UI Toolkit selection | `/unity-dev-toolkit:unity-ui-selector` |
| New script template generation | `/unity-dev-toolkit:unity-template-generator` |
| C# script quality validation | `/unity-dev-toolkit:unity-script-validator` |
| Scene performance analysis | `/unity-dev-toolkit:unity-scene-optimizer` |
| Unity Test Framework execution / result analysis (EditMode/PlayMode) | `/unity-dev-toolkit:unity-test-runner` |
| Compile error diagnosis (when root cause is unclear even after `unity-cli refresh`) | `/unity-dev-toolkit:unity-compile-fixer` |
<!-- | ECS/DOTS patterns | `/game-development:unity-ecs-patterns` | -->

Always load the matching knowledge skill BEFORE starting implementation. Multiple skills can be loaded if the task spans several areas.

## Related Agents (delegate via Agent tool)

For deeper, multi-file Unity work, delegate to a specialist agent instead of doing it inline. Pass relevant file paths and findings in the prompt — agents do not auto-load project rules.

| Situation | Agent (`subagent_type`) | Why |
|------|------|-----|
| New game system / module design, folder structure / namespace / dependency organization | `unity-dev-toolkit:unity-architect` | System design + project structure specialist (read-only) |
| Frame drop / draw call / memory and other performance issue diagnosis and improvement | `unity-dev-toolkit:unity-performance` | Unity Profiler-oriented optimization (rendering, batching, GC) |
| Legacy script refactoring, design pattern application, code duplication removal | `unity-dev-toolkit:unity-refactor` | Code quality / testability improvement specialist |

Routing principles:
- **Simple read/edit work** → handle directly in the current session (delegation overhead is larger).
- **Design / refactor / optimization spanning 3+ files** → delegate to the agents above.
- **Pure knowledge lookups** (UI Toolkit syntax, script validation rules, etc.) → load the Knowledge Skill above.

## Gotchas

- **Input System**: This project uses the new Input System package. Use `UnityEngine.InputSystem` (Keyboard.current, Mouse.current) instead of the legacy `UnityEngine.Input` class.
- **Connection closed before response**: Normal for `refresh_unity` and `exec` during recompilation. Just wait and check results after.
- **exec timeout**: Complex operations may take time. If exec fails, try simpler steps.
- **Newly written scripts**: Must compile before `exec` can reference their types. Always refresh first.
