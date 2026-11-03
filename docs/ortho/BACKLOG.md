# Backlog

Work top to bottom. A task is done when its **Gate** passes and its commit is in. Tick it (`[x]`), add the commit
hash, and log the iteration in `LOG.md`. Split a task if it will not fit in one iteration; add new tasks where they
belong rather than at the end. Milestone gates (**MG**) are where the loop stops and reports to Alpha.

Status: `[ ]` open, `[~]` in progress (one at a time), `[x]` done, `[!]` blocked (say on what).

## M0: Foundations

- [x] Scaffold `maniaortho` as a separate ruleset delegating to `ManiaRuleset`; wire into solution and osu.Desktop.
- [x] Playtest harness: screenshot runner, humanised replays, synthetic charts, side-by-side compare.
- [x] Parity tests: judgements, score, accuracy (replays); playable beatmap, SR, pp (direct).
- [x] Harness polish: hide the menu cursor that appears in playtest captures; add a `--preset`/settings pass-through
  once settings exist. Gate: captures show no cursor. (Cursor and mouse disabled in the runner; `--screenshot` captures
  any scene. Preset pass-through moves to the highway playfield task.)
- [x] Core patch: `AllowGameplayWithRuleset` accepts mania beatmaps for `maniaortho` (and only for it).
  Gate: unit test in the ruleset tests; song select filter test shows mania maps under `maniaortho`;
  `check.sh --mania` green. (Done via `Ruleset.PlaysBeatmapsOfRuleset`; `PresentBeatmap` no longer switches to mania.)
- [ ] Research baseline (delegate to `vsrg-researcher`): one note per reference game in `research/`, covering camera,
  note approach, judgement line position, lane covers/walls, speed definition, hold rendering, key-count handling.
  SDVX, Chunithm, osu!mania first, then Project Sekai, IIDX, StepMania/Etterna, Quaver, YARG/Clone Hero, USC,
  Arcaea, DJMAX. Gate: each note has sources and a "what this means for us" section; `research/INDEX.md` updated.
- [x] Settings plumbing: `ManiaOrthoRulesetConfigManager` (camera mode, preset, tilt, FOV, wall position, lift,
  wall-keeps-visible-time) and a settings subsection; mania scroll speed still applies.
  Gate: settings round-trip test; settings show in the ruleset settings panel (screenshot via a test scene).
  (Tilt + FOV replaced by far-edge width; see DECISIONS.)

## M1: The highway (prototype)

- [x] `HighwayProjection`: pure maths for `TrackToScreen` in perspective, parallel and flat modes, with the
  invariants in SPEC section 11 as unit tests. Gate: L0 tests green. (`HighwayProjectionTest`, `HighwayScrollTest`.)
- [x] `ProjectedQuad` drawable/DrawNode that draws an arbitrary quad with perspective-correct texture coordinates.
  Gate: a test scene showing a checkerboard on a strong trapezoid; screenshot shows straight checker lines.
  (`TrackDrawing` grid subdivision + `TrackSprite`; `TestSceneTrackSprite` shows subdivided vs one-quad bend.)
- [~] Highway playfield: replace mania's column layout with projected lanes, lane dividers, judgement line, receptors.
  Notes still drawn by mania for now if that is simpler. Gate: screenshots for 4K/7K in perspective and flat.
  (First version: `HighwayWarpContainer` warps mania's whole stage; replay parity green. Done since: receptor area
  below the line drawn at flat height, upright judgements (`HighwayJudgementLayer`), field wall drawn (`HighwayWall`,
  M2 wall task's L0 test and screenshots still open), Chunithm judgement line and lane guides (`HighwayGuides`,
  skin hit target hidden when tilted, settings "Lane guides" and "Use skin hit target"). Open: flat-mode capture.)
- [x] Projected notes and hold notes (heads, bodies, tails) using the scrolling positions mania computes.
  Gate: replay parity still green; screenshots show holds aligned at all depths; flat mode matches mania's layout.
  (`HighwayNoteLayer`, opt-in "Notes: Highway"; parity with native notes green; holds aligned in captures of
  Armageddon 4K, C18H27NO3 7K and synthetic 7K, also with the classic skin; flat camera keeps cover mods; Hidden,
  Fade In and Cover fade native notes as mania does. Default since 2026-11-03; Skin stays as a setting.)
- [x] Bar lines and SV: bar lines projected; SV sections of `synthetic-7k` look right. Gate: screenshots at SV 0.5x and
  1.75x sections. (Warped with the stage. Playtest found SV was dropped under maniaortho; fixed in
  `DrawableManiaOrthoRuleset`, `TestScrollSpeedChangesApply`; captures match mania in Chunithm and Parallel.)
- [x] **MG1**: report to Alpha with a compare sheet (mania vs perspective vs parallel, 4K and 7K, real-4k map) and
  ask for an L3/L4 playtest on device. (Reported 2026-11-03 with five sheets: Armageddon 4K, C18H27NO3 7K, 0.5x SV,
  holds, classic skin. On-device playtest asked; waiting on Alpha.)

## M2: Gameplay settings and feel

- [ ] Visible-time scroll speed across camera modes. Gate: L0 test + screenshot timed at hit time minus visible time
  shows the note at the far edge.
- [ ] Top wall with constant visible time (default) and speed lock toggle. Gate: L0 test; screenshots at wall 0/40/70%.
- [ ] Bottom lift. Gate: screenshot; parity green.
- [ ] Presets (Voltex, Chunithm, Parallel, Classic) with values from research. Gate: compare sheet per preset.
- [ ] Per-key-count camera defaults (reduce tilt at 7K+). Gate: 4K/7K/10K screenshots within the slant limit.
- [ ] Lane beams and hit lighting at the judgement line. Gate: screenshots during dense sections.
- [ ] Built-in highway skin. Gate: rubric pass in screenshots; no texture bends.
- [~] Visibility mods: projected Hidden / Fade In / Cover, or removal. Gate: parity with each mod; decisions logged.
  (Hidden, Fade In, Cover done on native notes: `DepthCover`, `TestCoverHidesNativeNotes`, replay parity with each.
  Open: Flashlight, still a flat band over the screen.)
- [ ] Gameplay hotkeys for wall and speed (like mania's F3/F4). Gate: input test scene.
- [ ] **MG2**: device playtest (L3) with frame times on Alpha's PC; ask Alpha for an L4 session and replays.

## M3: Separate client and local scoring

- [ ] Rebrand app name, icon and data folder **[Alpha: name]**; remove or replace the official API endpoints.
  Gate: the build cannot log in to or contact osu! servers (test); separate data folder.
- [ ] Live pp during play and on results via upstream calculators. Gate: pp equals mania's for the test scores.
- [ ] Append each play to a CSV (columns in the design doc). Gate: file written with correct values in a test.

## M4: Upstream tracking

- [ ] Daily job: read the live `ppy.osu.Game.Rulesets.Mania` version used by `osu-queue-score-statistics`; when it
  changes, diff mania difficulty code and open a PR that bumps it. Gate: a simulated bump opens a PR.
- [ ] Recalculate stored scores when the calculator version changes. Gate: simulated bump recalculates every score.

## M5: Optional self-hosted server

Only after Alpha confirms. See the design doc's "Local-only vs self-hosted server".

## M6: Polish

- [ ] Legacy skin textures on projected notes.
- [ ] Optional cosmetic camera motion (kiai sway), off by default.
- [ ] More presets from research.
