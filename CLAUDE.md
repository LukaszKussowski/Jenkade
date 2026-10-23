# Jenkade

## Checks

`scripts/check.sh` runs every check CI runs on a PR: Release build, all tests, Polish-text placement.

## Agent skills

### Issue tracker

Issues and specs live in GitHub Issues of LukaszKussowski/Jenkade (via `gh`). See `docs/agents/issue-tracker.md`.

### Triage labels

Default vocabulary: needs-triage, needs-info, ready-for-agent, ready-for-human, wontfix. See `docs/agents/triage-labels.md`.

### Domain docs

Single-context: `GLOSSARY.md` and `docs/adr/` at the repo root. See `docs/agents/domain.md`.

## Working with the user

- Implement proposals (retro findings, review suggestions, options) only once the user names the ones to do; a question about them gets a recap ending with the choice.
- Each issue starts on a fresh branch from `origin/main` (`feat/<slug>`), even when the current branch is something else.
