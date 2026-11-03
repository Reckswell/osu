---
name: ortho-loop
description: Autonomous work loop for the maniaortho ruleset (orthographic/tilted osu!mania). Picks the next backlog task, researches, writes tests, implements, runs parity checks and screenshot playtests, gets a review, commits and logs, then continues. Use when asked to continue, work on the backlog, run the loop, or /ortho-loop [number of tasks] [branch].
---

# ortho-loop

You are working without supervision. Alpha reviews the results later through commits, `docs/ortho/LOG.md`,
`DECISIONS.md` and the milestone reports, so leave a trail that makes each step checkable.

Arguments: optional number of tasks to finish this run (default 2), optional branch (default `ortho/dev`).
Never work on `master`.

## 0. Prepare

1. `dotnet --version` must print 10.x. If not, on Linux run `scripts/ortho/cloud-setup.sh --force`; on Windows ask
   Alpha to install the .NET 10 SDK and stop.
2. Branch: `git fetch origin`. If the target branch exists on origin, check it out and pull; otherwise create it from
   `origin/master`. If your session was started with a designated branch name, use that name instead, but start it
   from `origin/ortho/dev` when that exists so you continue the latest work.
3. `scripts/ortho/check.sh` once. If it fails before you change anything, fixing that is the task.

## 1. Orient

1. Read the last three entries of `docs/ortho/LOG.md`.
2. Read `docs/ortho/QUESTIONS.md`. For each newly answered question: apply it (SPEC, BACKLOG, DECISIONS), move it to
   *Answered*, and note it in the log.
3. Read `docs/ortho/BACKLOG.md`. Pick the first `[~]` task, else the first `[ ]` task whose prerequisites are done.
   Skip `[!]` tasks. If the pick is a milestone gate (**MG**), go to section 5.
4. Mark it `[~]`.

## 2. Do the task

1. **Understand.** Read the SPEC sections and research notes the task touches, and the code you will change. For
   upstream behaviour (scrolling, skins, mods), read the mania and `osu.Game/Rulesets/UI/Scrolling/` code first.
2. **Research when the task depends on how a reference game behaves** and no `done` note answers it. Spawn
   `vsrg-researcher` with one precise question, e.g. "Where is SDVX's judgement line as a fraction of screen height,
   and how wide is the far end of the track relative to the near end? Give measurements and sources." Use its proposed
   defaults unless they conflict with the SPEC; record the choice in `DECISIONS.md`.
3. **Test first.**
   - Geometry or settings: L0 unit tests in `osu.Game.Rulesets.ManiaOrtho.Tests`.
   - Anything near input, hit objects or timing: a parity case in `Parity/TestSceneReplayParity`.
   - Anything visible: a capture time or a capture scene for the playtest harness (see `docs/ortho/PLAYTESTING.md`).
4. **Implement** in small steps. While iterating, run focused tests:
   `scripts/ortho/check.sh --filter "FullyQualifiedName~YourTest"`. At the end run the full `scripts/ortho/check.sh`
   (add `--mania` if you touched `osu.Game/` or the mania ruleset).
5. **Playtest** anything visible: spawn `playtester` naming the SPEC sections and what changed. Read its report and
   open at least one compare image yourself. Fix failures and repeat (at most three rounds; then see section 4).
6. **Review**: spawn `gameplay-reviewer` with the diff base and task name. Fix every HIGH and MED finding, or record
   in `DECISIONS.md` why not.

## 3. Land it

1. Commit with message `maniaortho: <task summary>`; the body lists the evidence (test counts, playtest verdict,
   screenshot folder name, decisions added). One task per commit (fixups squashed). Do not commit `playtest-output/`.
2. Tick the task `[x]` with the short hash, append a line to `LOG.md`, commit those doc changes too.
3. Push (`git push -u origin <branch>`; retry with backoff on network errors). Ensure a PR from the branch into
   `master` exists; create it if not (title `maniaortho: ongoing work`), and keep its description's checklist in
   step with the backlog.
4. If tasks remain in this run's budget and your context is not nearly full, go back to section 1.3.

## 4. When stuck

- Same failure three times, or a fix would break an invariant: stop the task. Push the partial work to
  `ortho/wip-<task-slug>`, reset your branch to its last good commit, mark the task `[!] <reason, link to wip branch>`,
  log it, and move to the next independent task.
- A question only Alpha can answer: add it to `QUESTIONS.md` with your default, continue with that default if the
  step can be undone, otherwise pick another task.
- Never weaken a test to get green. Never skip the parity test.

## 5. Milestone gate

1. Run `scripts/ortho/playtest.sh` with the milestone's maps and settings; collect the compare images.
2. Write `docs/ortho/reports/<milestone>.md`: what was built, test counts, compare images (paths), decisions since the
   last gate, open questions, known issues, and exactly what you want Alpha to try (for L3/L4).
3. Ask for a device playtest:
   - In a Claude project thread with Remote Control available: ask Alpha to approve a Remote Control session on their
     PC, then run `pwsh scripts/ortho/playtest.ps1` there (and anything else in `PLAYTESTING.md` L3), and add the
     results to the report.
   - Otherwise: ask Alpha to run `pwsh scripts/ortho/playtest.ps1` (or Claude Code with `/ortho-loop`) on their PC and
     to try the build by hand.
4. Mark the MG task `[~] waiting for Alpha`, log it, push, and end the run with the report as your final message.
   Continue past the gate only with tasks that do not depend on Alpha's feedback.

## End of run

Finish with a short report: tasks done (with commits), evidence, decisions made, questions added, what is next. When
running in a project thread, post it there and attach the most informative compare image.
