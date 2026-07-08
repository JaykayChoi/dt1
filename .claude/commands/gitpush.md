Commit + push the current work to **master** via the `oh-my-claudecode:git-master` agent. Arguments: `$ARGUMENTS`

## Scope (which files to commit)

- **Default (no args):** only the files you created or edited in this conversation. Cross-check against `git status --porcelain` (read-only Bash is allowed) and stage **only** those — do not include unrelated dirty files (pre-existing dirty, the user's own manual edits, other work). If you cannot tell which dirty files are yours, list the candidates and ask rather than guessing broadly.
- **`--all`** (e.g. `/gitpush --all`): every git-dirty file — everything `git status` shows (modified + untracked, respecting `.gitignore`), regardless of origin.

## Commit splitting (both modes)

If the changes span clearly separate concerns, split them into multiple logical commits for a clean history; otherwise one commit. Use the repo's Korean prefix style — `기능:` / `수정:` / `리팩토링:` — and state **why**, not a narrative of how the code changed.

## Commit footer (mandatory)

End every commit message with these two lines, each on its own line:

```
Claude-Session: <session-id>
```

`<session-id>` is the value passed to `--resume` to continue this conversation. Read it from the `$CLAUDE_CODE_SESSION_ID` env var (not `$CLAUDE_SESSION_ID` — that name is unset). If it is empty, use the basename (without the `.jsonl` extension) of this conversation's transcript — the most-recently-modified `~/.claude/projects/<project-slug>/<id>.jsonl`. Resolve the literal value (`echo $CLAUDE_CODE_SESSION_ID`) and put it in the message; do not leave the `<session-id>` placeholder.

## Execution — delegate to git-master

Do not stage, commit, or push yourself. Dispatch the **`oh-my-claudecode:git-master`** agent (Agent tool, `subagent_type: "oh-my-claudecode:git-master"`) and pass it: the per-commit file lists, each full commit message **including the footer**, `branch = master`, and the git rules below. When it returns, report each commit hash + the push result.

## Push policy

- Push to **master**. If master also holds commits that are not yours, push anyway — `git push` publishes whatever master points at.

## Git rules (this repo's hookify guardrail — applies to Bash and PowerShell)

The project is trunk-based: **master only.** These ops are **blocked** on both shells:

- creating or switching branches (`git checkout -b`, `git switch -c`, `git branch <name>`, `git checkout <non-master>`)
- pushing a non-master branch, `gh pr create`, force push (`--force` / `--force-with-lease` / `-f`)
- `git add -f` / `--force`

**Allowed:** `git add <files>`, `git commit`, `git push`, `git push origin master`, `git checkout master`.

Run `git push` with **no trailing redirect or pipe** (no `2>&1 | …` after it): the guardrail's non-master-push pattern reads `2>&1 |` as `<remote> <branch>` and blocks the push. Let the tool capture the output on its own.
