# CLAUDE.md: orthographic osu!mania fork

This is a fork of osu! (lazer). The goal is a new ruleset, **maniaortho**, that plays any osu!mania beatmap on a
tilted SDVX/Chunithm-style highway while keeping mania's judgements, scoring, star rating and pp exactly. Everything
outside gameplay (song select, menus, skins system, replays) stays as upstream osu!.

You are expected to work autonomously: research, decide, implement, test, playtest, review and commit without asking
Alpha (the owner) about every step. Read `docs/ortho/README.md` once for the full picture, then follow the work loop.

## Where things are

| Path | What |
|---|---|
| `osu.Game.Rulesets.ManiaOrtho/` | The ruleset. Delegates everything except the playfield to `ManiaRuleset`. |
| `osu.Game.Rulesets.ManiaOrtho.Tests/` | Parity tests, playtest harness (`Playtest/`), visual test scenes. `dotnet run` opens the test browser. |
| `docs/ortho/SPEC.md` | What the gameplay must look and feel like. The source of truth for behaviour. |
| `docs/ortho/BACKLOG.md` | Milestones and tasks with acceptance gates. Work top to bottom. |
| `docs/ortho/DECISIONS.md` | Every non-trivial decision you made alone, with reasons. Append, never rewrite. |
| `docs/ortho/QUESTIONS.md` | Questions only Alpha can answer. Keep working on your best guess meanwhile. |
| `docs/ortho/PLAYTESTING.md` | What each playtest level proves, and the screenshot review rubric. |
| `docs/ortho/research/` | Research notes on reference games, with sources. |
| `docs/ortho/LOG.md` | One short entry per work-loop iteration. |
| `.claude/agents/` | Subagents: `vsrg-researcher`, `gameplay-reviewer`, `playtester`. |
| `.claude/skills/ortho-loop/` | The autonomous work loop. `/ortho-loop` runs it. |

## Commands

```bash
scripts/ortho/check.sh                  # style-enforced build + all ruleset tests (parity, unit, headless smoke). Run before every commit.
scripts/ortho/check.sh --mania          # also run osu!mania's own suite; required if you touch osu.Game or the mania ruleset
scripts/ortho/playtest.sh               # autoplay real + synthetic charts in a real window, mania vs maniaortho screenshots side by side
scripts/ortho/playtest.sh --beatmap path/to/map.osu --times 5000,20000 --scroll-speed 24
scripts/ortho/cloud-setup.sh --force    # install .NET 10, Mesa, Xvfb, ImageMagick on a fresh Linux container
dotnet run --project osu.Game.Rulesets.ManiaOrtho.Tests   # interactive visual test browser (needs a display)
```

Screenshots are PNG files; open them with the Read tool and actually look at them. In a cloud container the game
renders through Xvfb + Mesa (llvmpipe): frames are faithful, frame rates (~10-15 fps) are not. Judge performance only
from runs on Alpha's machine.

## Invariants (a change that breaks one of these is wrong, whatever else it achieves)

1. **Visual only.** Same beatmap + same inputs must produce the same judgements, score, accuracy, star rating and pp as
   osu!mania. `TestSceneReplayParity` and `RulesetParityTest` enforce this; never weaken, skip or delete them.
   Add a parity case whenever you add a code path that could touch input or judgement.
2. **Never submittable.** `ManiaOrthoRuleset` must not implement `ILegacyRuleset` and must keep a negative online ID.
   Never add code that calls osu! score-submission, multiplayer or spectator endpoints.
3. **Settings, not mods.** Camera, walls, lane cover and skin choices are ruleset settings. They must never change
   scoring, so they must never be mods.
4. **Tiny core diff.** Do not edit `osu.Game/` or `osu.Game.Rulesets.Mania/` unless a backlog task explicitly allows it.
   Every allowed core patch is listed in `docs/ortho/README.md#core-patches`; keep each one minimal and add a test.
5. **Mania beatmaps convert as mania.** Anything that compares ruleset identity (beatmap converter, difficulty
   calculator) must be given `ManiaOrthoRuleset.MANIA`, not `this`. Otherwise mania maps are treated as osu!standard
   converts: wrong columns, wrong notes, wrong star rating.

## Work loop (summary; full procedure in `.claude/skills/ortho-loop/SKILL.md`)

1. Read `docs/ortho/LOG.md` (last 3 entries), `BACKLOG.md`, `QUESTIONS.md` (any answers?).
2. Take the first unchecked task whose dependencies are met.
3. If the task needs facts about reference games, delegate to `vsrg-researcher` first and cite its note.
4. Write or update the failing test / visual scene first, then implement.
5. `scripts/ortho/check.sh` until green. Then `scripts/ortho/playtest.sh` for anything visible, and have
   `playtester` review the screenshots against `SPEC.md` and the rubric.
6. Have `gameplay-reviewer` review the diff. Fix what it finds or record why not in `DECISIONS.md`.
7. Commit (one task per commit), tick the task, append to `LOG.md`, continue with the next task.

Stop and report to Alpha only when: a milestone gate is reached, a decision is listed under "Ask Alpha" below, the same
task failed three iterations in a row, or invariants conflict with the spec.

## Autonomy policy

**Decide alone** (log non-obvious ones in `DECISIONS.md`): implementation approach, file layout, shader and math
details, default values within the ranges in `SPEC.md`, test design, naming of internal types, refactors inside the
ruleset projects, which reference behaviour to copy when sources agree.

**Ask Alpha** (write to `QUESTIONS.md`, keep going on your best guess where reversible): anything that changes the
spec's intent (camera style, wall semantics, default feel), branding and the public name, patches to core files not
already listed, anything touching official osu! services or accounts, merging to `master`, releases, deleting
user-visible features.

**Never**: push to `master`, force-push shared branches, disable or skip tests to get green, commit beatmaps, audio or
skins you did not create (copyright), copy code from non-MIT/BSD sources into this repo (reading for reference is fine;
note the licence in the research note).

## Gotchas learned so far

- `ManiaBeatmapConverter(beatmap, ruleset)` decides "is this a mania map" by comparing the source ruleset with the one
  passed in. `ManiaDifficultyCalculator` compares online IDs. Both must get the mania ruleset (invariant 5).
- `AssemblyRulesetStore` only lists legacy rulesets; the game's `RealmRulesetStore` lists ours. Test discovery inside a
  running game (`[Resolved] RulesetStore`), not with `AssemblyRulesetStore`.
- `AddUntilStep` times out after 10s of game time. For long maps, wait in checkpoints (see `TestSceneReplayParity`).
- The playfield draws from `DrawableRuleset.FrameStableClock`, not the gameplay clock. After a seek it catches up a few
  frames per update and can lag by tens of seconds on a slow renderer. Wait on the frame-stable clock before judging
  or capturing anything (see `TestScenePlaytest`); otherwise two "identical" captures show different notes.
- Mods like Hidden, FadeIn, Cover and Flashlight cast to `DrawableManiaRuleset` / `ManiaPlayfield`. While
  `DrawableManiaOrthoRuleset` subclasses `DrawableManiaRuleset` they work; once the playfield is replaced, each needs a
  projected equivalent or must be hidden from this ruleset's mod list (record which in `DECISIONS.md`).
- A container hidden with `Alpha = 0` does not update its children. Pooled drawables added to it (mania's explosions,
  our bursts) never prepare or expire and pile up: clear them, do not just hide them. A hidden skinnable misses skin
  changes and key releases and leaves the input queue; set `AlwaysPresent` on anything hidden that must stay current.
- The warp's buffer holds premultiplied colour (it starts transparent). It is drawn back with premultiplied blending;
  anything else drawn from a transparent buffer needs the same, or see-through content comes out at alpha squared.
- Seeking back drops pooled effects and judgements that ended in between; they do not come back. Captures must reach
  their time from below (the playtest harness stops short and seeks forward; `overshoot` in the summary should be 0).
- osu-framework transforms are affine. Perspective needs a custom `DrawNode` emitting arbitrary quads; textured
  trapezoids need perspective-correct UVs (a plain two-triangle quad shows a visible bend).

## Code conventions (from upstream; CI enforces most)

- Every `.cs` file starts with the ppy MIT licence header (copy from any existing file). Files are UTF-8 with BOM and
  CRLF on checkout (`.gitattributes` handles line endings).
- Drawables are `partial` classes (source generators). Nullable is on.
- No allocations (LINQ, closures, `new` lists) in `Update()` or draw paths. Cache and reuse.
- Match upstream naming: private fields `camelCase`, constants `UPPER_SNAKE`, test scenes `TestScene*`.
