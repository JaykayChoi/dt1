---
paths:
  - "**/*.cs"
  - "**/*.unity"
  - "**/*.prefab"
  - "**/*.asset"
  - "**/*.uxml"
  - "**/*.uss"
---

# Unity CLI Usage

`unity-cli` is installed and configured. Use it to validate Unity-related changes through the Editor, not only by reading source files.

## Basic Workflow

After changing Unity code or assets:

```bash
unity-cli status
unity-cli editor refresh --compile
unity-cli console --type error,warning --stacktrace user
```

If errors appear, fix them and repeat.

## Common Commands

```bash
unity-cli status                                              # Check Unity connection
unity-cli editor refresh                                      # Refresh assets
unity-cli editor refresh --compile                            # Refresh + compile C#
unity-cli console --type error,warning --stacktrace user      # Read console
unity-cli test                                                # Run EditMode tests
unity-cli test --mode PlayMode                                # Run PlayMode tests
unity-cli editor play --wait                                  # Enter Play Mode and wait
unity-cli editor stop                                         # Stop Play Mode
```

## Runtime Validation

Use Play Mode validation when changes affect runtime behavior, scene loading, MonoBehaviour lifecycle, DI, UI binding, animation, physics, or gameplay logic.

```bash
unity-cli editor play --wait
unity-cli console --type error,warning,log --stacktrace user
unity-cli editor stop
```

## Execute Small Unity C# Snippets

Use `exec` for quick inspection only.

```bash
unity-cli exec "return UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name;"
```

For complex snippets, pipe through stdin:

```bash
echo 'Debug.Log("check"); return null;' | unity-cli exec
```

Do not use `exec` for permanent project logic — edit source files normally.

## Reserialize Unity YAML Assets

If `.prefab` / `.unity` / `.asset` / `.mat` are edited as text:

```bash
unity-cli reserialize <asset-path>
unity-cli editor refresh
unity-cli console --type error,warning --stacktrace user
```

## Custom Tools

```bash
unity-cli list                                # List built-in + project tools
unity-cli my_custom_tool                      # Run custom tool
unity-cli my_custom_tool --params '{"k":"v"}' # Run with params
```

## Rules

- Always check `unity-cli status` before relying on Unity commands.
- After code changes: refresh + console error check is mandatory before reporting completion.
- **Exit Play Mode before editing scripts or running `editor refresh --compile`** (run `unity-cli editor stop` first). Recompiling while in Play Mode can wedge Unity's domain reload / Localization / Addressables init so it never completes (boot hangs), forcing an editor restart.
- Unity Editor tasks (Inspector wiring, scene edits, component setup, GUID changes) are handled directly via `unity-cli` — never ask the user to do them manually.
