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

## 2026-11-03: The highway draws its own judgement line and lane guides; the skin's hit target is hidden when tilted

- **Decision**: under tilted cameras, a Chunithm-style judgement line (white core 1.0% H, gold glow 3.3% H) and teal
  guide lines on every lane boundary (#45D9C8, stage edges #1F6763, 0.22% of the track wide, at least 1 px, fading from
  80% to 30% alpha with distance) are drawn in the highway's screen-space overlay. Mania's skin hit target is hidden.
  Two preferences, outside presets: "Lane guides" (on) and "Use skin hit target" (off; on brings back the skin's hit
  target and drops the highway line). The flat camera shows mania's own look, untouched.
- **Why**: the warp magnified the skin's hit target about 4.6x near the line (a 23 px band became about 110 px at
  720p), because it sits just above the hit position where the perspective is strongest. Values are from
  research/game-chunithm.md ("Judgement line and receptors", P4). Guides are placed on the measured column boundaries,
  not at k/N, so they stay on legacy skins' uneven columns and on dual stages.
- **Deviation from the arcade**: Chunithm draws its judgement line and guides under the notes. Ours are drawn over them
  (the notes are inside the warped buffer, under one texture with the column backgrounds), so the core is kept to 1% H
  and the glow fades out; the leading edge of a note at its hit time is covered by half the core (about 3-5 px), and a
  divider covers about 1 px of the edges of the notes beside it.
- **Legacy skins**: with key textures they leave the column hit target empty and draw their own (`LegacyHitTarget`:
  stage hint and the skin's judgement line) inside the stage background, which is rebuilt on a skin change. Both are
  hidden, and the search runs again after every skin change (found by review; `TestLegacySkinHitTargetHidden`).
- **Minimum width** is 1 screen pixel, converted through the draw matrix (gameplay sits in a scaling container).
- **Known leftover**: the default skin's key area has a 7 px grey bar just above the hit position, which the warp also
  magnifies (about 20 px above the line at 720p). It is part of the key area, not the hit target, so it stays for now.
- **Asked**: whether the default should be the Chunithm line or the skin's hit target goes to Alpha with the MG1 report.

## 2026-11-03: Native notes, opt-in, by proxying mania's note containers away

- **Decision**: a "Notes" setting, Skin (tilted, default for now) or Highway. With Highway, notes are drawn natively on
  the track (`HighwayNoteLayer`): flat bars 0.0255 track depth thick (Chunithm's TAP, research P5), 98% of the lane,
  a white rim on the leading edge; hold bodies 80% wide, brighter while held, grey once broken; tails half a note thick.
  Heads stop at the line while held, as in mania, and follow the tail below it when held past the end. Notes are
  clipped at the judgement line, as in Chunithm.
- **Colours**: the skin's column colour when it is a note colour (opaque and not dark: argon and triangles colour notes
  with it); otherwise built-in colours after Alpha's skin, white outer lanes and pink #FF8290 inner lanes counted in
  from both edges, amber for mania's special centre column. That is legacy mania's own 4K/6K/7K pattern (1-2-2-1,
  1-2-1-1-2-1, 1-2-1-S-1-2-1). Legacy skins' column colour is the lane background (black in classic and in Alpha's
  skin), which made every native note black before review. With timing-based colouring on, a note takes its snap colour
  instead of the column colour (replacing, not multiplying: mania multiplies it into the skin's piece, which for the
  white note textures snap colouring is used with is the same thing, and multiplying into pink lanes would muddy it).
- **How**: each column's `HitObjectContainer` gets a framework proxy, in a holder added to the container's own parent
  just behind it (inside a cover mod's wrapper when there is one), shown only under the flat camera. A proxied drawable is not drawn in place, but it stays present: it updates, receives input and judges as
  before (`TestJudgementParityWithNativeNotes`). Positions come from each hit object's time through mania's own scroll
  algorithm (`HighwayScroll.DepthAt`), which matches the warped notes exactly (`TestNotesSitWhereTheWarpedNotesWould`).
  Under the flat camera the proxies are drawn in the container's place, so the flat camera still looks like mania:
  Hidden, Fade In and Cover still cover, the column still masks, and the key area and explosions still draw over notes
  (the first version kept the proxies above the whole stage and lost all three; caught in review).
- **Why**: the warp drew mania's notes about twice Chunithm's thickness near the line (5-6% of the track instead of
  2.55%) and soft, because the skin's note height is a fixed number of pixels in a stage the perspective magnifies
  about 4x there. Native notes are what the rendering card's recommended hybrid ("warp now, native later") planned.
- **Why opt-in for now**: Hidden, Fade In and Cover wrap the note container and Flashlight draws over the stage; none of
  them reach native notes yet (scoring is unaffected). Skin note textures are not used either. Highway becomes the
  default once those are covered and Alpha has seen it.
- **Bar lines** stay warped with the stage (they are in the stage's own container, not the columns').
- **Left from research P5 for later**: hold bodies use normal blending, not additive; thickness is a constant, not the
  0.015-0.06 setting P5 proposes. After a drop, mania freezes the hold's drawn length so the head slides past the line;
  here the head goes back to its unclamped depth and is clipped at once. Only visible around the line.

## 2026-11-03: Hidden, Fade In and Cover on native notes, by reading mania's own cover

- **Decision**: native notes fade exactly where mania's cover fades its own notes. Each frame, for every column, the
  highway reads the two boxes of the column's `PlayfieldCoveringWrapper` (the fully covered part and the gradient,
  opaque at its own bottom edge), maps their flat-stage rows to track depth with the warp's mapping (`DepthCover`),
  and multiplies each note piece's alpha by what the cover leaves. Pieces are cut at the cover's edges, and the fade is
  cut into steps of at most 0.02 depth, because colours are interpolated linearly on screen, which is not linear in
  depth under perspective. Fully covered pieces are not drawn. A cover reaching an end of the column also covers what is
  past that end (mania's column masks it).
- **Why read the drawn boxes**: they already include everything the mods do: Hidden's coverage growing with combo and
  dropping in breaks, the ease-in of coverage, the legacy hit position scaling, Cover's direction and the scroll
  direction. Recomputing any of that would copy mania code and drift from it. Reading needs no core patch.
- **Risk**: it depends on the wrapper's layout (a buffer with the notes and a container of exactly two boxes). If
  upstream changes it, covers silently stop applying to native notes; `TestCoverHidesNativeNotes` (both directions)
  fails in that case. Scoring is never affected: the mods still apply to mania's own drawables.
- **Playtest**: Hidden and Fade In on Armageddon 4K at speed 29, Cover (along scroll) on synthetic 7K: what is covered
  matches mania's capture at the same time, including hold bodies cut off in the fade.
- **Alternatives considered**: recomputing the cover from the mod's `Coverage` and direction (copies mania's easing and
  legacy hit-position maths, drifts when upstream changes them); removing the three mods from this ruleset (SPEC 9
  allows it, but they work and players use them); a core patch exposing the wrapper's state (unlisted core change).
- **Overrides**: the "Why opt-in for now" bullet of the native notes entry above: Hidden, Fade In and Cover now reach
  native notes. Flashlight is still drawn flat over the screen, as with skin notes.
- **How to reverse**: pass no covers to `HighwayNoteLayer.Gather` (drop `measureCovers`); the rest is unchanged.
- **Tests**: `DepthCoverTest` (maths), `TestCoverHidesNativeNotes` (measured cover in depth for both directions, notes
  hidden, fading and clear), `TestJudgementParityWithVisibilityMods` (HD, FI, CO with native notes).

## 2026-11-03: Native notes are the default

- **Decision**: "Notes" defaults to Highway (native notes); Skin stays available and is listed second.
- **Why**: SPEC 6 asks for notes with a fixed thickness in track units, which only native notes have (warped skin notes
  are about twice Chunithm's thickness near the line and soft). The reason they were opt-in is gone: Hidden, Fade In and
  Cover now fade them as mania does (entry above), and Flashlight behaves the same for both styles. Alpha was sent the
  comparison sheets; this is reversible with one default.
- **Cost**: a legacy skin's note textures are not used with native notes (M6 task); its look is approximated by the
  white/pink lane colours from Alpha's skin. Players who want their textures pick Skin.
- **How to reverse**: `SetDefault(HighwaySetting.NoteStyle, HighwayNoteStyle.Skin)` and `HighwaySettings` default;
  `TestNotesDrawnOnTheHighwayByDefault` pins the current choice.

## 2026-11-03: Flashlight lights the track rows mania lights (projected, not removed)

- **Decision**: the ruleset lists `ManiaOrthoModFlashlight` in place of mania's Flashlight. It inherits
  `ManiaModFlashlight` (same acronym, settings, rank adjustment, incompatibilities) and only replaces the drawable:
  mania's lit band (a full-width band at the playfield's vertical centre, soft over 10% of its half height) is drawn
  with each of its rows moved to where the highway draws that row of the flat stage: on the track at the row's depth
  above the line, at flat height below it, unchanged under the flat camera
  (`HighwayPlayfieldAdjustmentContainer.DrawnYOfFlatY`). Notes are lit for exactly the same part of their approach as
  in mania. The soft edge is drawn in 6 strips per side approximating mania's smoothstep; the band's size still eases
  with combo and breaks as in mania (the base class drives it), and is measured in playfield units as mania's draw node
  does (the stage is scaled up in portrait; caught in review, `TestBandIsManiasSize(portrait)`).
- **Why projected**: SPEC 9 asks for projected equivalents first. Mania's screen-space band over the tilted track
  lit a different, nearer part of each note's approach, so the mod showed notes for a different time than mania.
  With the Chunithm camera the lit band sits far up the track and is thin on screen: that is the same window of time
  mania lights, compressed by the perspective. Alternatives: keep the screen band (wrong time window), or remove FL
  from the mod list (loses a mod for no reason).
- **Why a subclass, not hiding mania's drawable**: the flashlight needs the mod's combo binding and an internal tracker
  only the base mod can set up; a subclass gets both and reads the eased band size, which is protected.
- **Risk**: score multipliers are looked up by exact mod type. Mania gives Flashlight none today; if it adds one, ours
  would miss it. `ModListTest.TestFlashlightMultiplierIsMania` (alone and with HR, DT, EZ, NF) and
  `TestJudgementParityWithVisibilityMods("FL")` fail in that case.
- **How to reverse**: drop the replacement in `ManiaOrthoRuleset.withHighwayVisuals`.
- **Tests**: `ModListTest` (same mods as mania by acronym and type, FL replaced, multipliers equal),
  `TestSceneHighwayFlashlight` (notes lit in mania are drawn inside the drawn lit band, dark ones outside it; the band is
  mania's under the flat camera), replay parity with FL. Playtest: Armageddon 4K at speed 29 and synthetic 7K, the same
  notes lit as in mania's capture.

## 2026-11-03: The HUD's key counter stays where the skin puts it

- **Decision**: osu!'s HUD layout is left alone. Argon puts the key counter at the bottom right of the screen (global
  HUD layout); the Chunithm track is wider near the line than mania's stage, so on 4K at 16:9 the counter overlaps
  the right edge of the track's near end (playtest v11 captures), and on 7K it covers the near end of the three
  right lanes (v15). The counter only shows in replays and autoplay unless the player turns on osu!'s key overlay
  setting (`OsuSetting.KeyOverlay`, off by default; `KeyCounterDisplay`), so normal play is not affected.
- **Why**: SPEC 7 keeps the osu! HUD unchanged, and lazer skins already have per-ruleset HUD layouts, so a player can
  move the counter in the skin editor for maniaortho alone without touching mania's layout. Alternatives: ship a
  maniaortho default layout that moves it (changes the HUD the spec keeps, and needs a skin transformer of our own
  for every built-in skin), or shrink the track (changes the camera presets research set).
- **How to reverse**: return a `DefaultSkinComponentsContainer` for `MainHUDComponents` with `Ruleset` set from a
  maniaortho skin transformer that wraps mania's.

## 2026-11-03: The bottom lift is the judgement line height setting, with no cover below the line

- **Decision**: SPEC 4's bottom lift (IIDX's LIFT) is the existing "Judgement line height" setting (0-40% of the
  screen, searchable as "lift"), not a second setting added on top of it. Raising it moves the judgement line,
  mania's hit position, the receptors and key area (drawn at their flat height just below the line), lane lighting
  and judgements up together. The visible time does not change: the track gets shorter on screen, so notes move
  slower on it, as the wall does by default. Below the key area the screen shows the background; there is no lift
  cover graphic.
- **Why one setting**: two sliders that both raise the line would be confusing, and the line height is already part
  of each camera preset like the wall. Chunithm has no lift (research/game-chunithm.md), so "off by default" means the
  preset's own height. Alpha's baseline wants the line down to the bottom edge, which 0% gives.
- **Why no cover**: IIDX shows a cover image below a lifted line (unverified; research/game-iidx.md is still todo).
  Here that area is mostly the receptors and key area, which players read. A cover can come with the built-in highway
  skin if playtests ask for it.
- **Known effects**: like the other camera sliders, the lift does nothing under the flat camera (Classic), where
  mania's own hit position applies, though the slider stays enabled. Changing it switches the preset to Custom, and
  picking a preset resets it; IIDX keeps LIFT independent of anything else, so this goes into the M2 report. At 0% the
  judgement line sits on the screen's bottom edge, so half its core and its lower glow are cut off (cosmetic).
- **How to reverse**: add a `Lift` setting outside the presets and add it to `JudgementLineHeight` in
  `HighwaySettings.ToLayout`.

## 2026-11-03: Voltex preset from USC's camera; note thickness is part of each preset

- **Decision**: the Voltex preset copies the default camera of USC (the open-source SDVX clone, MIT) in a landscape
  window, projected into our four numbers (`research/game-sdvx.md` P1, `research/game-usc.md`): judgement line 5.9%
  above the bottom, highway 0.435 of the screen wide at the line for every key count, far/near width 0.106, wall off.
  Its far edge (28% below the top in USC) is stretched to the top edge, as Q7's default does for Chunithm; QUESTIONS Q7
  now asks the same for Voltex. Notes in this preset are 1.25% of the track deep (USC's note depth).
- **Why USC**: no arcade frame could be measured (every image source was blocked); USC's portrait camera was tuned
  against the arcade by its developers and its landscape camera is the same rig. K-Shoot MANIA agrees within about 15%
  except for the line height. The arcade numbers stay unverified until one EG or ∇ frame is measured.
- **Note thickness**: notes have a fixed thickness in track units (SPEC 6), but the same depth looks very different
  under different cameras: at Chunithm's 2.55%, SDVX's stronger perspective drew notes about 200 px tall at the line
  at 1080p, twice Chunithm's. So thickness is now a setting that each preset sets ("Note thickness", 0.5-6%, changing
  it makes the preset Custom): Chunithm and Parallel 2.55%, Voltex 1.25%. Alternative rejected: deriving it from the
  camera so notes keep one height on screen at the line, which makes Parallel's notes about 11% of the track deep
  (Parallel draws every depth at the line's scale).
- **Outside SPEC 2's range**: SPEC 2 says the far/near width is "typically 0.3-0.6"; SDVX's 0.106 (and Chunithm's 0.24)
  are below it. The range was a guess made before research; the measured and projected values win, and Alpha can
  change the SPEC text.
- **Known effect**: the thickness slider does nothing with Skin notes, but moving it still makes the preset Custom. Not
  disabled there: disabling a config bindable from the panel would make applying a preset throw.
- **How to reverse**: restore the placeholder values in `HighwayPresets.Get(Voltex)`; drop `HighwaySetting.NoteThickness`
  and use `HighwayNoteLayer.NOTE_THICKNESS` again.

## 2026-11-03: One track width for every key count (Alpha)

- **Decision** (Alpha, Q10 on a decision card): every key count uses the preset's track width, as Chunithm's field
  does. SPEC 5 is updated to match; its earlier plan (45% at 4K to 70% at 10K, less tilt at 7K+) is dropped.
- **Why it covers the slant concern**: with one width, the outer lanes' lean depends only on the camera, not the key
  count; captures of 4K, 7K and 10K with both plans are in
  `/mnt/project-files/ortho-agent/presets/key-counts-same-vs-spec-width.png`.

## 2026-11-03: Hit lighting is drawn by the highway; lane beams stay with the skin

- **Decision**: hits light the judgement line with our own bursts (`HighwayHitEffectLayer`), drawn in screen space at
  each lane's projected point on the line and squashed to 40% height so they read as lying on the track
  (`research/game-chunithm-feedback.md` P1, P2, P6): a ring growing from 0.6 to 1.3 lane widths over 350 ms (`OutQuint`)
  and fading, a centre flash for 120 ms, and rising upright sparkles. Flash and sparkles are additive; the ring is drawn
  normally, because a just-hit lane is lit by the skin's press beam and an additive ring washed out to white there
  (seen in dense captures). Perfect is gold with 8 sparkles,
  Great orange-red with 4, Good/Ok a smaller white-green ring, Meh the same at 60%, a miss nothing. A ring never grows
  past 30% of the track's width, so 4K bursts stay about one lane wide. Bursts appear on exactly the results that make
  mania add an explosion (same event, same conditions as `Column.OnNewResult`).
- **Holds**: while a lane's hold is held, a soft light sits on its line point and a ring pulses outwards at Chunithm's
  hold tick cadence from the beatmap's timing (1/16 notes below 120 BPM, 1/8 up to 240, 1/4 above; research P6 reads
  "1/16 beat", taken as 1/16 notes, which keeps the pulse at 4-8 per second). Needed because native notes hide the hold
  drawable, and with it argon's hold "hitting" glow.
- **Setting, not mod**: "Hit effects: Highway / Skin (tilted)", a preference outside the presets like "Notes", default
  Highway. Under Highway the skin's explosions are hidden (`Column.HitObjectArea.Explosions.Alpha = 0`) and handed back
  to mania's pool as they arrive (hidden, they are never updated, so they would never expire and would pile up); under
  Skin or the flat camera they are shown and no bursts are made.
- **Pool**: 6 bursts per column are made ready when gameplay loads (research P2 suggested 8 per lane; mania readies 5
  explosions per column). Notes 75 ms apart in one lane keep up to 7 alive; the pool grows if needed.
- **Pulse rate**: the hold tick interval is folded by halves or doubles into 100-300 ms, so gimmick timing points
  (6 ms or 60 s beats) neither flicker nor stop the pulse.
- **Why not projected quads**: a disc lying on the track needs the track's real length-to-width ratio, which a 2D
  trapezoid does not fix (the camera's focal length is unknown), and the presets do not describe one consistent world.
  A fixed squash looks the same for an effect this small and costs nothing. *(tune `FLATTEN` by eye)*
- **Lane beams**: not drawn by us. Every skin already lights a pressed lane inside the stage (argon's column
  background gradient over half the column, legacy stage lights), and the warp lays that on the track rising from the
  line, which is research P5's beam. Drawing our own would double it. Revisit with the built-in highway skin.
- **Known overlap**: legacy skins' hold lighting (`LegacyBodyPiece`) is added to the column's internal top-level
  container, which the ruleset cannot reach without reflection, so with a legacy skin a held lane shows both glows.
  With "Notes: Skin", argon's own hold glow (`ArgonHoldBodyPiece`) is drawn too (warped).
- **How to reverse**: set "Hit effects" to Skin; or remove the layer from `HighwayPlayfieldAdjustmentContainer`.

## 2026-11-03: The highway draws its own track; the skin's stage is hidden on tilted cameras

- **Decision** (recommended on Alpha's decision card, QUESTIONS Q11, still open; reversible by changing one default): a new
  setting "Track: Highway / Skin (tilted)", a preference outside the presets like "Notes" and "Hit effects", default
  Highway. Under Highway on a tilted camera, `HighwayTrack` draws, under mania's warped stage:
  - a near-black floor (#060606 at 90%) under each stage, from the judgement line to the far edge and on down to the
    bottom of the screen (research `game-chunithm.md` P4; `game-chunithm-feedback.md` P8 suggests 85% with a later
    "track opacity" setting);
  - a dark teal rail a sixteenth of the stage wide on each side (#0C2A2A at 80%, darker towards the far end),
    Chunithm's side bands. Floor and rails end at the wall when there is one (the wall covers only the lanes). With dual
    stages each stage gets its own, and a rail never takes more than half the gap to the other stage;
  - a white beam in each pressed lane, 30% at the line fading linearly in track depth to nothing half way up, 45% while
    the lane's hold is held, fading in over 30 ms and out over 150 ms easing out (feedback P5, P6), and below the line
    a matching light where the receptor was (SPEC 5 asks for the receptor to light).
  The skin's stage background, stage foreground, lane backgrounds and key areas are hidden (`Alpha = 0` on mania's
  skinnable wrappers). Under Skin, or the flat camera, nothing changes from before.
- **Why**: it is the Chunithm look Alpha asked for and SPEC 8's built-in skin; the warp magnifies skin art several
  times near the line (receptors and stage bottoms looked smeared in every capture); and Alpha's own skin has no
  receptors to lose (`baseline-alpha.md`). The lane beam is now ours, which the hit-lighting decision deferred to this
  task: the skin's press lighting is hidden with its lane backgrounds, so there is no double beam.
- **Input**: which lanes are pressed is read from the key binding container's pressed actions (what mania's columns and
  notes receive, so replays and autoplay light the lanes too). Nothing new handles input. The hidden skin parts are
  kept present (`AlwaysPresent`): hidden with alpha alone they would drop out of the input queue and stop updating,
  so they would miss skin changes (a legacy skin's hit target was never built) and key releases (a stuck light when
  shown again). Present, they stay in the input queue exactly as in mania; every one of them (default, argon, legacy
  key areas and column backgrounds) only lights the skin and returns false, so nothing about who judges a press
  changes. Every replay parity case now runs with the highway track
  (the default) and one runs with the skin's (`TestJudgementParityWithSkinTrack`).
- **Finding the skin parts**: mania builds them in fixed places, so they are told apart by position (stage background
  and foreground in the stage's content container; a column's key area as its own child, its lane background in its
  background container). `TestSceneHighwayTrack` checks that every argon stage background, column background and key
  area in the playfield is inside a hidden part, and that the hit targets are not, so an upstream layout change fails
  the test rather than showing the skin's stage again.
- **Skin hit target**: legacy skins with key textures draw their hit target inside the stage background, which is
  hidden; with "Use skin hit target" on, the highway keeps its own judgement line for them, so the track always has
  one. Per-column hit targets (argon, triangles, other legacy skins) show as before.
- **The warp's transparency** (found by review in the captures): the warp draws mania's stage into a buffer that
  starts transparent, which stores colour already multiplied by its alpha; it was drawn back with normal blending,
  multiplying again. The skin's opaque lane backgrounds used to hide this; on the highway's floor bar lines came out
  about seven times too faint (pixel +3 over the floor against +23 over argon's lanes). The buffer is now drawn with
  premultiplied blending, on every camera (the flat camera's see-through parts, such as argon's 80% lane
  backgrounds, now match mania instead of coming out at 64%).
- **Colour**: the floor is one colour: SPEC 8's "colours per lane type"
  are carried by the notes (native notes use the skin's lane colours or white and pink), not the floor.
- **How to reverse**: set "Track" to Skin, or change the default in `ManiaOrthoRulesetConfigManager` and
  `HighwaySettings`.
