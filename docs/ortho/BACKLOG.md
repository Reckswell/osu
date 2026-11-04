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
- [x] Research baseline (delegate to `vsrg-researcher`): one note per reference game in `research/`, covering camera,
  note approach, judgement line position, lane covers/walls, speed definition, hold rendering, key-count handling.
  SDVX, Chunithm, osu!mania first, then Project Sekai, IIDX, StepMania/Etterna, Quaver, YARG/Clone Hero, USC,
  Arcaea, DJMAX. Gate: each note has sources and a "what this means for us" section; `research/INDEX.md` updated.
  (Quaver, Arcaea and DJMAX finished 2026-11-04; their proposals are in M6 below and Q19.)
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
- [x] Highway playfield: replace mania's column layout with projected lanes, lane dividers, judgement line, receptors.
  Notes still drawn by mania for now if that is simpler. Gate: screenshots for 4K/7K in perspective and flat.
  (First version: `HighwayWarpContainer` warps mania's whole stage; replay parity green. Done since: receptor area
  below the line drawn at flat height, upright judgements (`HighwayJudgementLayer`), field wall drawn (`HighwayWall`,
  M2 wall task's L0 test and screenshots still open), Chunithm judgement line and lane guides (`HighwayGuides`,
  skin hit target hidden when tilted, settings "Lane guides" and "Use skin hit target"). Flat camera: captures of
  Armageddon 4K, C18H27NO3 7K and synthetic 7K (both SV sections) differ from mania only in random judgement
  particles and HUD text, the stage pixel for pixel; `/mnt/project-files/ortho-agent/flat/`.)
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

- [x] Visible-time scroll speed across camera modes. Gate: L0 test + screenshot timed at hit time minus visible time
  shows the note at the far edge. (`HighwayScrollTest`; in game `TestNoteAppearsOneVisibleTimeBeforeItsHit` for
  Perspective and Parallel tilt, also checked on screen against the far edge and the drawn wall. Armageddon 4K at
  speed 29, time range 405.1 ms: the notes hit at 33446 ms are exactly at the far edge at 33041 ms (not yet in view,
  as at the top of mania's stage, which shows the same notes), just inside it at 33061 ms, on the line at 33446 ms.)
- [x] Top wall with constant visible time (default) and speed lock toggle. Gate: L0 test; screenshots at wall 0/40/70%.
  (Same captures at walls 0, 40% and 70%: the same notes on screen as mania, entering at the wall. With the speed lock
  at 40% the visible time is 243.1 ms and they reach the wall at 33203 ms. Playtest `--speed-lock`; summaries record
  the time range and visible time.)
- [x] Bottom lift. Gate: screenshot; parity green. (The "Judgement line height" setting, keyword "lift"; see
  DECISIONS. `TestJudgementsFollowTheLift`, `TestJudgementParityWithLift`. Captures at 30% on Armageddon 4K
  (also with HD and FL) and synthetic 7K with the classic skin, and at 0%: the line, receptors, lane lighting and
  judgements move up together; HD and FL cover and light the same notes as mania. Playtest `--set Name=value`.)
- [x] Presets (Voltex, Chunithm, Parallel, Classic) with values from research. Gate: compare sheet per preset.
  (Voltex from USC's camera, `research/game-sdvx.md`; note thickness per preset. Sheets: Armageddon 4K and
  C18H27NO3 7K at speed 29 with mania, Chunithm, Voltex, Parallel, Classic, and Voltex with its faithful far edge;
  Armageddon at speed 18 for mania, Chunithm, Voltex. Classic matches mania.)
- [x] Per-key-count camera defaults (reduce tilt at 7K+). Gate: 4K/7K/10K screenshots within the slant limit.
  (Alpha chose one width for every key count, as in the arcade (Q10): the outer lanes lean the same at 4K, 7K and
  10K, so there is nothing to reduce. Captures in `presets/key-counts-same-vs-spec-width.png`. No code change.)
- [x] Lane beams and hit lighting at the judgement line. Gate: screenshots during dense sections.
  (Bursts lie on the track at each lane's line point, coloured by result, with a glow while a hold is held:
  `HighwayHitEffectLayer`, "Hit effects: Highway / Skin". Lane beams stay the skin's warped press lighting (DECISIONS).
  `TestSceneHighwayHitEffects`, `HitBurstStyleTest`, contact sheet `TestSceneHitBurst`; captures in
  `/mnt/project-files/ortho-agent/hit-effects/`.)
- [x] Built-in highway skin. Gate: rubric pass in screenshots; no texture bends.
  ("Track: Highway / Skin", default Highway pending Q11: `HighwayTrack` draws a near-black floor, teal side rails and
  white beams on pressed lanes under the warped stage, whose skin backgrounds and receptors are hidden. Notes were
  already native. Review found the warp drew see-through content at alpha squared (bar lines ~7x too faint on the new
  floor); it now blends premultiplied, and the flat camera matches mania pixel for pixel on lane backgrounds.
  `TestSceneHighwayTrack` (incl. dual stages), `HighwayTrackTest`, replay parity with both tracks. Captures in
  `/mnt/project-files/ortho-agent/track/`: straight edges and beams, no bends.)
- [x] Visibility mods: projected Hidden / Fade In / Cover, or removal. Gate: parity with each mod; decisions logged.
  (Hidden, Fade In, Cover on native notes: `DepthCover`, `TestCoverHidesNativeNotes`. Flashlight lights the track rows
  mania lights: `ManiaOrthoModFlashlight`, `TestSceneHighwayFlashlight`, `ModListTest`. Replay parity with HD, FI, CO
  and FL.)
- [x] Gameplay hotkeys for wall and speed (like mania's F3/F4). Gate: input test scene.
  (Shift+F3/Shift+F4 move the wall 5% with a toast; mania's F3/F4 speed keys already worked. `TestSceneHighwayHotkeys`:
  keys still reach mania's input manager, work in a replay player, range, modifiers, no column pressed;
  toasts: `TestSceneVisibleTime.TestHotkeysShowTheToasts`.)
- [x] **MG2**: device playtest (L3) with frame times on Alpha's PC; ask Alpha for an L4 session and replays.
  (2026-11-04, RTX 5080 + Ryzen 7 9800X3D, 1920x1080 window, D3D11, uncapped: every run at the 1000 Hz ceiling, median
  frame 0.98 ms in both rulesets; maniaortho's p99 at most 3.93 ms (synthetic 7K; mania 2.67 ms), inside 180 Hz's
  5.56 ms; worst single frames are one-off hitches in both rulesets, mania's larger. Scores identical in all four pairs
  (Armageddon 4K and C18H27NO3 7K played to the end, Chunithm; synthetic 7K; Voltex + wall 40%). GPU captures equal the
  cloud's apart from random particles: `/mnt/project-files/ortho-agent/mg2/`. L4 asked in the thread; Q12, Q13.)

## M2.5: Alpha plays (L4)

- [x] Portable full game: `scripts/ortho/play.ps1` keeps the data next to the clone (framework.ini and storage.ini, no
  core patch; DECISIONS 2026-11-04 "Full game"). Gate: `PlayScriptDataFolderTest`; a PC run leaves
  `%APPDATA%\osu-development` unchanged and fills `ortho-data`. (PC run 2026-11-04: build 25 s, "Storage successfully
  changed to ...\ortho-data", no `%APPDATA%\osu-development`, stable's top level unchanged, nothing sent to
  osu.ppy.sh; `-QuitAfter` needed a second close request, fixed.)
- [ ] Alpha's files from stable: osu!'s first-run import of maps, skins and scores from `AppData\Local\osu!` by hard
  links; screenshots moved into stable's `Screenshots` folder; never open `%APPDATA%\osu`. Gate: on Alpha's PC the
  import reports hard links, stable's files unchanged (sizes and times), a screenshot lands in stable's folder.
- [x] Package for Alpha (asked 2026-11-04): `osu\osu.exe` next to the clone starts the packaged game, repackaged with
  `scripts/ortho/package.ps1`. Gate: `GameLauncherTest`; on Alpha's PC the package builds and `osu.exe` starts the game
  with its data in `ortho-data`. (PC 2026-11-04: packaged in 24 s, game 354 files / 651 MB, osu.exe 186 KB; started
  from the package with "Storage successfully changed to ...\ortho-data", Velopack skipped, no errors; closed cleanly.)
- [ ] L4 session: send Alpha setup steps and a short list of things to try; ask for notes and replays.

## M2.6: Feel follow-ups from the research baseline

From the 2026-11-04 notes (`research/game-osu-mania.md` P1-P7, `topic-covers.md` C3-C4, `topic-wide-keys.md` W1-W3,
`game-yarg-clonehero.md`). All visual: settings, not mods.

- [x] A hold held past its end keeps its head on the line, as in mania (osu!mania P3). Gate: in-game test that fails
  without the fix. (`TestHoldHeldPastItsEndKeepsItsHeadOnTheLine`, `TestHoldWithNoLengthPassesTheLine`.)
- [x] Show the real visible time: the wall toast ("40% covered · 405 ms visible") and the speed setting's hint use the
  highway's own time (mania's `TimeRange` after the skin's hit position, and the wall when speed is kept), not
  `11485 / speed` (osu!mania P1, covers C3). Gate: the shown ms equals the measured time a note takes from the far
  edge or wall to the line, within 1 ms. (`TestSceneVisibleTime`, `TestScrollSpeedShowsTheVisibleTime`; the speed
  toast shows it too; DECISIONS 2026-11-04, "Speed and wall show the real visible time".)
- [x] ~~Held Shift+F3/F4 repeats on its own 100 ms timer, not the OS key repeat (covers C4).~~ Dropped: osu-framework
  already repeats on its own real-time timer (250 ms, then 70 ms), not the OS's (DECISIONS 2026-11-04, "Held wall
  keys keep osu-framework's repeat").
- [x] Note thickness up to 8%, so Alpha's 6.4% bars can be matched under Parallel (osu!mania P7). Gate: settings test.
- [x] Constant Speed on the highway: a test that pins it (osu!mania P2). Gate: notes equally spaced through an SV
  section with the mod on. (`TestConstantSpeedIgnoresSv`: the 0.5x and 1.75x sections scroll at 1x.)
- [x] Group dividers at 6K and up (3|3, 3|1|3, 4|4, 4|1|4, 5|5) and a 1 px on-screen floor on the gap between notes
  (wide keys W1, W3). Gate: tests; 7K and 10K captures reviewed by the playtester. (`WideKeysTest`,
  `TestGroupDividers`, `TestGroupDividersPerStage`, `TestNotesKeepAPixelOfGap`, `TestHoldBodiesStayWithinTheirEnds`;
  captures in `/mnt/project-files/ortho-agent/wide-keys/`, playtester pass.)
- [x] Optional far fade (YARG), default off. Gate: capture; parity. (`TestFarFade`, `TestFarFadeMultipliesFadeIn`,
  `TestJudgementParityWithFarFade`; captures in `/mnt/project-files/ortho-agent/far-fade/`, playtester pass.)
- [x] Playtest at 840 ms visible time too, where Chunithm and Sekai players' near-line speed is (`game-sekai.md` K3).
  Gate: the harness's default capture set includes it. (`playtest.sh` and `playtest.ps1` with no chart or speed: both
  charts at speed 29, 405 ms, and synthetic-7k at 14, 839 ms by the summary; an unset speed is now 29.)
- [ ] **[Alpha: Q17]** A Sekai preset with Sekai's eased approach (`game-sekai.md` K2).
- [ ] **[Alpha: Q14]** Missed notes slide past the line; the wall keys' gate with "keep speed"; lane tint; a wall for
  Voltex at 9K and up.

## M3: Separate client and local scoring

On hold for the "separate client" part (DECISIONS 2026-11-04, "Product direction"): Alpha wants a lazer game-mode mod
in the end. Live pp and the CSV still apply.

- [ ] Rebrand app name, icon and data folder **[Alpha: name]**; remove or replace the official API endpoints.
  Gate: the build cannot log in to or contact osu! servers (test); separate data folder.
- [x] Live pp during play and on results via upstream calculators. Gate: pp equals mania's for the test scores.
  (No code: lazer's HUD pp counter and results pp go through the delegated calculators. `TestSceneReplayParity` checks
  the live counter, `TestSceneResultsPerformanceParity` the results' with no mod, DT, HT, HR, NF and FL. Stable skins
  need the counter added in the skin editor; DECISIONS 2026-11-04, "Live pp is lazer's own".)
- [x] Append each play to a CSV (columns in the design doc). Gate: file written with correct values in a test.
  (`ortho-data\maniaortho\plays.csv`, one row per pass or fail, with the replay saved as a mania `.osr` since osu!
  keeps none for this ruleset; `PlayLogTest`, `TestScenePlayLogRecorder`, `TestScenePlayLogInPlayer`; DECISIONS
  2026-11-04, "Plays go to a CSV".)

## M4: Upstream tracking

- [ ] Daily job: read the live `ppy.osu.Game.Rulesets.Mania` version used by `osu-queue-score-statistics`; when it
  changes, diff mania difficulty code and open a PR that bumps it. Gate: a simulated bump opens a PR.
- [ ] Recalculate stored scores when the calculator version changes. Gate: simulated bump recalculates every score.

## M5: Optional self-hosted server

Only after Alpha confirms. See the design doc's "Local-only vs self-hosted server".

## M6: Polish

- [x] Legacy skin textures on projected notes: the runtime skin porter (Alpha's ask, 2026-11-04; survey in
      `research/skins-survey.md`). Open: the default key-area variant for raised hit positions (asked on a card).
- [x] Optional "Hold opacity" 10-100% and "Track opacity" 0-100% settings, and the judgement line lit per lane on a hit
  in the result's colour (`game-sekai.md` K6, K7, K9). Multipliers, default 100%; slot lights retuned after the
  playtest (DECISIONS 2026-11-04). Great (orange) and Good (green) slot lights seen in a humanised playtest; Meh (dimmer) only in tests, as seed 4 hits none.
- [x] Optional cosmetic camera motion (kiai sway), off by default. ("Kiai sway" 0-100%: the track leans about the
  judgement line with the beat in kiai sections; DECISIONS 2026-11-04.)
- [ ] More presets from research. (No Arcaea preset: Alpha, Q19.)
- [x] Optional "Key beam" length: Long (today) / Half / Quarter of the track's height on screen, default Long
  (`game-djmax.md` R2). Half and Quarter measured to the wall's edge when there is one (DECISIONS 2026-11-04).
- [ ] The beam's release as Quaver's (off in 60-100 ms, `game-quaver.md` QV9), only if Alpha's L4 notes say today's
  150 ms ease-out lingers: single captures cannot show it.
- [ ] Low priority: for `SpecialStyle` (N+1 key) maps, a 1 | N−1 divider marking the special column (`game-djmax.md`
  R3, extends `topic-wide-keys.md` W1).
- [x] "Wall scale": set and step the wall in Chunithm's 0-14, IIDX's white number or osu!mania / Quaver's share of the
  lane, or a flat percent (Alpha, 2026-11-04). Choosing a game gives the wall its speed behaviour. Gate: unit tests per
  scale, panel and hotkey scenes. (DECISIONS 2026-11-04, "Wall scale".)
- [ ] **After skin integration is finished** (Alpha, 2026-11-04): the wall covers only the lanes, not the screen around
  them, and finds where the lanes are, including a ported skin's stage (its column widths, spacing and position), dual
  stages and the kiai sway. Today it is an upright rectangle as wide as the track at its base, so above the base it also
  covers the background beside the converging lanes. Gate: captures with the default and a ported legacy skin at 4K and
  7K, the wall's edges on the lanes' outer edges within 1 px; notes still covered.
