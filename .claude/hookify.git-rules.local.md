---
name: block-dangerous-git-ops
enabled: true
event: bash
tool_matcher: "Bash|PowerShell"
conditions:
  - field: command
    operator: regex_match
    pattern: git checkout -b\s|git switch -c\s|git branch\s+(?!-)[^\s]|git checkout\s+(?!master(\s|$))(?!\.)(?!--)([a-zA-Z])|git push\s.*--force|git push\s.*--force-with-lease|git push\s+-f(\s|$)|git push\s+(-\S+\s+)*(?!-)\S+\s+(?!master(\s|$))\S+|gh pr create|git add\s+-f(\s|$)|git add\s.*\s-f(\s|$)|git add\s.*--force
action: block
---

BLOCKED: This project's git guardrails.

== Branch / push protection (master-only) ==

Allowed:
  git checkout master
  git push origin master
  git push -u origin master
  git push (no branch specified)

Blocked:
  git checkout -b <branch>         -> creating new branch
  git switch -c <branch>           -> creating new branch
  git branch <name>                -> creating branch
  git checkout <non-master>        -> switching to non-master
  git push origin <non-master>     -> pushing to non-master
  git push -u origin <non-master>  -> pushing to non-master
  git push --force / -f            -> force push
  git push --force-with-lease      -> force push
  gh pr create                     -> PR creation

Note: `git checkout <file>` (without `--`) is blocked as branch-like — use `git checkout -- <file>` for file restores.
Bare `git push` with a trailing pipe (e.g. `2>&1 |`) is misread as remote/branch and blocked — run `git push` without pipes (see gitpush.md).

== .gitignore protection ==

Blocked:
  git add -f / --force             -> force-staging a .gitignore'd path

Never override .gitignore with -f. If a file is ignored but genuinely must
be tracked, fix .gitignore or ask the user first -- do not force-add.

Only proceed if the user explicitly requests it.
