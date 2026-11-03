# Orthographic osu!mania: how this fork and its agent work

## What is being built

A new ruleset, `maniaortho`, inside a fork of osu! (lazer). It plays every osu!mania beatmap, but draws the playfield
as a tilted highway in the style of Sound Voltex and Chunithm: fixed lanes, notes travelling from the top of the screen
to a judgement line near the bottom, and an adjustable "wall" that sets where notes appear. Judgements, scoring, star
rating and pp are taken unchanged from osu!mania, and scores never reach the official servers.

Background and the reasoning behind these choices: the design doc
"Orthographic osu!mania: design considerations" (https://claude.ai/artifact/GKF11mXTNjtLSg3aWkochk).

## How the agent works

The agent is Claude Code running in this repository with the instructions in `/CLAUDE.md`. It runs a loop
(`.claude/skills/ortho-loop/SKILL.md`): pick the next task from `BACKLOG.md`, research if needed, write a test,
implement, run the checks, playtest, get a review, commit, log, repeat. Three subagents help:

| Subagent | Job | Output |
|---|---|---|
| `vsrg-researcher` | Find out how osu!mania, SDVX, Chunithm, Project Sekai, IIDX, StepMania/Etterna, Quaver, YARG and others handle a specific gameplay element. Prefers primary sources and open-source code. | A note in `research/` with sources and "what this means for us". |
| `playtester` | Run the playtest harness, look at the screenshots, judge them against `SPEC.md` and the rubric in `PLAYTESTING.md`. | Pass/fail per rubric item, with the screenshots it looked at. |
| `gameplay-reviewer` | Review a diff against the invariants, the spec and osu! conventions. | Findings ranked by severity. |

Alpha is asked only at milestone gates and for the decisions listed under "Ask Alpha" in `CLAUDE.md`. Everything the
agent decided alone is in `DECISIONS.md`, and every iteration is one line in `LOG.md`, so it can be reviewed later
instead of supervised live.

## What "playtesting" means here

Honest summary (details in `PLAYTESTING.md`):

| Level | What runs | Where | Proves | Does not prove |
|---|---|---|---|---|
| L0 Unit | Projection maths, settings, parity of SR/pp | Anywhere, seconds | Geometry and numbers are right | Anything visual |
| L1 Replay parity | Real and synthetic charts played by a humanised replay in mania and maniaortho | Anywhere, headless, ~1 min | Gameplay is identical to mania | How it looks |
| L2 Screenshots | Autoplay in a real window, frames frozen at set times, mania vs maniaortho side by side | Cloud (Xvfb + software GL) or Alpha's PC | What a player would see | Frame rate, input feel |
| L3 Device run | Same harness plus the full game on Alpha's PC via Remote Control | Alpha's PC | Real GPU performance, real maps from Alpha's library | Feel under human hands |
| L4 Human | Alpha plays, exports replays and notes | Alpha's PC | Feel, readability at speed | (it is the ground truth) |

The agent does L0 to L2 on every change by itself. L3 happens at milestone gates through Remote Control on Alpha's
PC. L4 is the only step that needs Alpha's hands; replays Alpha exports can be fed back into the parity tests.

## Running it

- **In a cloud session** (claude.ai/code): the container needs `scripts/ortho/cloud-setup.sh` to have run once
  (installs .NET 10, Mesa, Xvfb). Then ask for `/ortho-loop` or "continue the backlog".
- **On a schedule**: a routine that starts a cloud session on this repo every day with the prompt
  `/ortho-loop 3` (three tasks per run) keeps it moving without anyone watching.
- **On Alpha's Windows PC**: run Claude Code in the repo and use `/ortho-loop`; `scripts/ortho/playtest.ps1` runs the
  harness natively on the GPU.

### One-time setup still to do (needs Alpha)

These touch Claude Code's own configuration, so they were left for Alpha to approve:

1. Register `scripts/ortho/cloud-setup.sh` as a SessionStart hook, so every cloud session builds without a manual step.
   Add to `.claude/settings.json`:
   ```json
   { "hooks": { "SessionStart": [ { "hooks": [ { "type": "command", "command": "$CLAUDE_PROJECT_DIR/scripts/ortho/cloud-setup.sh" } ] } ] } }
   ```
2. Optionally allow the loop's routine commands without prompts (`dotnet build`, `dotnet test`, `scripts/ortho/*`).

## Core patches

Changes outside the two ruleset projects. Keep this list short; each needs a reason and a test.

| File | Change | Why | Status |
|---|---|---|---|
| `osu.sln`, `osu.Desktop.slnf` | Add the two ruleset projects | Build and CI | Done |
| `osu.Desktop/osu.Desktop.csproj` | Reference `osu.Game.Rulesets.ManiaOrtho` | Ship the ruleset with the game | Done |
| `osu.Game/Rulesets/Ruleset.cs` | New virtual `PlaysBeatmapsOfRuleset` (null by default) | Lets a ruleset declare another ruleset's beatmaps as its own | Done, `BeatmapCompatibilityTest` |
| `osu.Game/Beatmaps/BeatmapInfoExtensions.cs` | `AllowGameplayWithRuleset` also accepts beatmaps of the ruleset named by `PlaysBeatmapsOfRuleset` (looked up once per ruleset and cached) | Song select must offer every mania map under `maniaortho` without "show converts" | Done, `BeatmapCompatibilityTest` |
| `osu.Game/Screens/Select/SongSelect.cs`, `osu.Game/OsuGame.cs` | `PresentBeatmap` keeps the current ruleset when it plays the beatmap natively (`AllowGameplayWithRuleset(..., false)` instead of ruleset equality; identical for upstream rulesets) | Importing or opening a mania map must not switch the player back to mania | Done |
| API endpoints, app name, data folder | Point away from official services | Never share login or database with the official client | Backlog M3 |

## Branches

Work happens on feature branches off `master` and reaches `master` through pull requests that Alpha merges. The agent
never pushes to `master`.
