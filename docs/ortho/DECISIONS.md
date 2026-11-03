# Decisions

Append-only log of decisions made without asking Alpha. Newest at the bottom. Each entry: date, decision, why,
alternatives considered, how to reverse. If a later decision overrides one, add a new entry that says so.

## 2026-11-03: Separate ruleset that delegates to mania

- **Decision**: `ManiaOrthoRuleset : Ruleset` holds a static `ManiaRuleset` instance (`MANIA`) and forwards
  conversion, difficulty, performance, scoring, health, mods, skins, key bindings and statistics to it. Only the
  drawable ruleset is its own.
- **Why**: subclassing `ManiaRuleset` would inherit `ILegacyRuleset` (legacy ID 3), which would collide with mania in
  the ruleset store and make scores submittable. Delegation keeps every number identical to mania by construction.
- **Alternatives**: subclass (rejected for the reason above); copy mania (rejected: drift).
- **Reverse**: n/a, foundational.

## 2026-11-03: Converter and difficulty calculator get the mania ruleset

- **Decision**: `CreateBeatmapConverter` passes `MANIA`; `CreateDifficultyCalculator` uses `MANIA`'s.
- **Why**: `ManiaBeatmapConverter` treats a beatmap as native only if its ruleset equals the one passed in, and
  `ManiaDifficultyCalculator` compares online IDs. Passing `this` turns every mania map into an osu!standard-style
  convert (wrong columns and notes) and changes star rating. Verified by `RulesetParityTest`.

## 2026-11-03: Working name `maniaortho`

- **Decision**: short name `maniaortho`, description "mania (ortho)", assembly `osu.Game.Rulesets.ManiaOrtho`.
- **Why**: matches the project name; the assembly must start with `osu.Game.Rulesets.` to be discovered.
- **Reverse**: the public name is Alpha's call (QUESTIONS.md). Renaming the short name later orphans local settings and
  scores stored under it, so rename before anyone plays seriously.

## 2026-11-03: Placeholder drawable ruleset subclasses `DrawableManiaRuleset`

- **Decision**: until M1 lands, `DrawableManiaOrthoRuleset` is a plain subclass, so it renders exactly like mania.
- **Why**: gives the harness and parity tests a real target from day one, and keeps mania's visibility mods working.

## 2026-11-03: Settings subsection deferred

- **Decision**: no `CreateSettings()` yet. `ManiaSettingsSubsection` requires a `ManiaRuleset` instance, and passing
  `MANIA` would bind to mania's settings rather than this ruleset's. In-game scroll speed hotkeys still work.
- **Reverse**: backlog M0 "Settings plumbing".

## 2026-11-03: Chunithm is the primary reference; perspective + Chunithm preset by default

- **Decision**: default camera is perspective with the Chunithm preset. SDVX becomes a secondary reference (the
  Voltex preset). Research priority: Chunithm viewfield and Chunithm animations/feedback first.
- **Why**: Alpha, in the project thread: Chunithm is the "game i want to mirror most", including animations and the
  gameplay viewfield. Chunithm's field is a perspective view, so this also answers Q1.
- **Reverse**: the camera mode and preset are settings; only the defaults change.

## 2026-11-03: Mania maps list natively under `maniaortho` via `Ruleset.PlaysBeatmapsOfRuleset`

- **Decision**: a new virtual on `Ruleset` names another ruleset whose beatmaps it plays as its own;
  `AllowGameplayWithRuleset` honours it, and both `PresentBeatmap` paths use `AllowGameplayWithRuleset(..., false)`
  instead of ruleset equality so presenting a mania map keeps `maniaortho` selected.
- **Why**: the alternatives were hard-coding `maniaortho` in osu.Game (worse: leaks our name into core) or relying on
  "show converted beatmaps" (does not work: converts only cover osu!standard maps). The lookup creates each ruleset
  once and caches the answer, so song select filtering stays allocation-free per beatmap.
- **Evidence**: `BeatmapCompatibilityTest` (10 cases, including upstream behaviour unchanged); osu!mania suite 840/840;
  osu.Game filtering + present-beatmap tests 269/269.

## 2026-11-03: Alpha's osu! stable install is the gameplay baseline, read-only

- **Decision**: use `C:\Users\alpha\AppData\Local\osu!` (stable, not lazer) as the reference for how the game should
  run: settings summarised in `research/baseline-alpha.md`; individual `.osu` files staged into the cloud container for
  playtests, never committed (copyright); skins only looked at, never copied. Nothing is ever written to that folder.
- **Why**: Alpha offered it as the baseline. It is stable, so its mania settings are translated to lazer equivalents.

## 2026-11-03: Camera is set in screen terms (far-edge width, heights, track width), not tilt + FOV

- **Decision**: the highway is defined by where it lands on screen: judgement-line height, far-edge height, track width
  at the line and far/near width ratio. Tilt and field of view are not settings.
- **Why**: once the track's near and far edges are pinned to screen positions (SPEC 2: notes enter at a visible far
  edge), a pinhole camera's tilt and FOV only matter through one number, `tan(tilt) / focal length`, which fixes the
  far/near width ratio. Two sliders for one degree of freedom would confuse; the width ratio is also what research can
  measure directly from Chunithm/SDVX frames. The maths is the plane homography (see `HighwayProjection`).
- **Reverse**: a tilt/FOV pair can be added later as a derived input that sets the ratio.

## 2026-11-03: Highway settings live in a second config manager under the same ruleset

- **Decision**: `ManiaOrthoRulesetConfigManager` extends mania's (so `DrawableManiaRuleset` keeps working) and owns a
  `HighwayConfigManager` with its own enum. Choosing a preset writes its values; editing a value makes it Custom; a named
  preset re-applies its values on load so tuning a preset reaches everyone using it.
- **Why**: a `RulesetConfigManager` has one enum, and mania's cannot be extended. Keys are stored by name and the two
  enums share none.

## 2026-11-03: First highway renders by warping mania's stage (hybrid option, pending Alpha's card)

- **Decision**: `HighwayWarpContainer` renders mania's playfield flat into a frame buffer and draws the stage's part of
  it onto the highway with `TrackDrawing` (grid-subdivided quads, perspective-correct to under a pixel). The stage is
  stretched sideways inside the buffer to about its width at the judgement line so the near end is not magnified.
- **Why**: Alpha has not yet picked between warp / native / hybrid; the hybrid (warp first, flat judgements and combo,
  native notes later) was recommended and is reversible. Warping keeps every mania feature working on day one (skins,
  hold bodies, bar lines, SV, Hidden/Fade In/Cover), and replay parity stays green because nothing in input or judgement
  changed.
- **Known costs**: one full-playfield buffer redraw per frame (measure on device); the area below the judgement line is
  magnified by perspective (receptors look large); judgements inside the stage tilt until moved to the overlay.
- **Reverse**: switch `CreatePlayfieldAdjustmentContainer` back to mania's, or replace the warp with native drawing.

## 2026-11-03: Chunithm preset uses the measured arcade geometry

- **Decision**: far/near width ratio 0.24, judgement line 7.4% of screen height above the bottom, track 0.61 of screen
  width at the line for every key count (`research/game-chunithm.md`: three sources agree to about 0.3% of screen
  height). The far edge is put at the top of the screen instead of the arcade's 16% (pending Q7).
- **Why**: Alpha named Chunithm as the game to mirror. The old values (0.45 / 12% / 0.6) were placeholders. SPEC's
  description of Chunithm ("gentler tilt, wider far end, wall on") is contradicted by the measurements, so the preset
  follows the measurements and the conflicting points are asked in Q7 and Q8 rather than changed in SPEC alone.
- **Effect**: the time-to-depth mapping is the arcade's, so a note spends half its visible time in the top fifth of the
  track's screen height and accelerates near the line, as in the arcade. The stretch to the top edge only changes vertical scale.
- **Reverse**: edit `HighwayPresets.Get(Chunithm)`; named presets re-apply their values on load.

## 2026-11-03: Judgements are drawn upright, at mania's height above the line, shrunk by the perspective

- **Decision**: each stage's judgements move to `HighwayJudgementLayer` in the highway's flat overlay. Mania's own
  display is hidden (not disabled: `DisplayJudgements` also gates hit explosions). Judgements are mania's drawables in a
  frame that copies the stage's flat layout, so skins place them as in mania; the frame is then placed so the judgement
  keeps its screen height above the judgement line, sits over the stage's lanes at that height, and is scaled by the
  track's width scale there (about 0.8 at the usual height with the Chunithm preset).
- **Why**: SPEC 1/7 keep judgement text in screen space near the line; keeping mania's screen height (rather than its
  depth) keeps it where the player's eyes are. Q5 (judgements per lane on the track, as in the arcade) stays open.
- **Bug found by playtest**: a custom origin on the frame produced an infinite position after a rewind on Armageddon;
  the frame now scales about its corner and placement skips non-finite values. `TestRewindWhileJudgementShown` covers it.

## 2026-11-03: The area below the judgement line is drawn at its flat height

- **Decision**: the warp draws the receptor/key area below the hit position with no perspective: its flat height, its
  sides following the lane edges' straight lines (`TrackDrawing.DrawBelowLine`).
- **Why**: perspective magnifies the near side; at the Chunithm preset a pixel just below the line would be drawn
  about 4-5 times taller, pushing receptors off screen.

## 2026-11-03: The field wall is a vertical panel, tilted cameras only

- **Decision**: `HighwayWall` is an opaque dark rectangle as wide as the track at the wall's depth, from the top of the
  screen down to that depth, with a teal lower edge where notes appear. Hidden with the Flat camera (mania's Cover mod
  already covers that case).
- **Why**: the arcade's wall is a vertical panel (`research/game-chunithm.md`); SPEC 4 allows either form. A rectangle
  covers the whole track beyond it because the track only narrows with depth. Both wall semantics work without touching
  mania's time range: the warp maps mania's scrolling area onto [0, scroll length].

## 2026-11-03: Review fixes and known limitations of the warp

- **Config type**: the ruleset config is also cached as `ManiaRulesetConfigManager` inside the drawable ruleset. Before,
  mania's notes and columns resolved null and "timing-based note colouring" did nothing here.
- **Scroll direction** is forced to Down and locked in this ruleset's config (SPEC 3: the camera defines direction).
- **Known limitations, not fixed yet**:
  - The warp reads the buffer as an axis-aligned, on-screen rectangle. Gameplay rotated (the fail animation tilts 1°)
    or partly off screen samples slightly wrong texels at the edges.
  - The buffer has no mipmaps; with the Chunithm preset the far end is minified up to about 4x, so thin bar lines and
    hold edges may shimmer in motion. Check in motion captures and on device; fix with extra samples if visible.
  - Dual-stage maps are warped as one highway spanning both stages and the gap between them (Q3 default is two).
  - Flashlight is drawn flat over the highway, so its band is near the line but not time-exact; it needs a projected
    version or removal from this ruleset's mod list (M2 visibility mods task).
  - The Chunithm preset uses the same track width for every key count, as in the arcade, instead of SPEC 5's per-key
    widths; per-key defaults stay an M2 task.

## 2026-11-03: Wall is a fraction of track depth, up to 90%

- **Decision**: the wall setting stays a fraction of the track's depth (not of its screen height), and its range grows
  from 0-80% to 0-90%.
- **Why**: the arcade's wall steps are depth steps (5.8% of the visible track each up to wall 11), which under its strong
  perspective move the wall's base only a little on screen at first (wall 4 hides 23% of the depth but only 5% of the
  screen). The 40% wall playtest shows the same. Players use up to wall 14 (88.5%), past SPEC's 80% (tune).


## 2026-11-03: Restore mania's SV in the ortho drawable ruleset, not in core

- **Decision**: `DrawableManiaOrthoRuleset` re-applies each control point's effect point (scroll speed) after mania's
  loader, for beatmaps made for osu!mania. Converts stay stripped, as in mania.
- **Why**: found by playtest. Mania drops SV unless `Beatmap.BeatmapInfo.Ruleset` equals the ruleset being played, which
  is maniaortho here, so every mania map scrolled at 1x through its SV sections. Fixing it in the ruleset needs no core
  patch: `ControlPoints` is protected, the derived loader runs after mania's and before the scroll algorithm first
  computes positions, and `EffectPointAt(time)` gives the same effect point `DrawableScrollingRuleset` merged in.
  `TestSceneHighwayPlayfield.TestScrollSpeedChangesApply` checks the 0.5x/1.75x sections (it failed with 1.0 before).
- **Also checked, fine**: the hit-window "is convert" check (song-select display goes through `MANIA`, so it compares
  with mania), `ManiaModClassic` (compares with a new `ManiaRuleset`), the converter and difficulty calculator (given
  `MANIA`). Key bindings are stored under maniaortho's own ruleset, on purpose (separate from mania's, like the settings).
