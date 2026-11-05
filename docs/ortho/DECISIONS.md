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

## 2026-11-03: Shift+F3 and Shift+F4 move the wall; F3 and F4 stay mania's scroll speed

- **Decision**: the gameplay hotkeys SPEC 4 asks for are Shift+F3 (wall down) and Shift+F4 (wall up), 5% of the track
  per press, held keys repeating as F3 and F4 do. The scroll speed needed nothing new: mania's F3 and F4 already work
  under maniaortho (inherited from `DrawableManiaRuleset`, same limits: the first 10 seconds of a play and breaks, never
  paused). A toast shows the new wall ("Wall: 5% covered", with the keys named under it), as osu! does for the speed.
- **Why these keys**: next to the speed keys, so a player finds them; Shift+F3/F4 are bound to nothing in osu!
  (global actions match modifiers exactly, so they do not trigger the speed keys either).
- **Input**: a plain component on the drawable ruleset, outside the ruleset's input manager, watches key-downs and
  never handles them (returns false), so every key goes on to exactly what it reaches in mania: a column bound to F3 or
  F4 is still pressed, and nothing about who judges a press changes. The first version used a key binding container
  that handled the keys; review found it would swallow Shift+F4 before mania's input manager (a parity break for anyone
  with a column on F4) and that it never fired while watching a replay, where osu! takes Shift's own key event to
  fast-forward. Shift is now read from the keyboard state. `TestSceneHighwayHotkeys` checks both (a listener inside
  mania's input manager still receives Shift+F4; the keys work in a replay player), plus range limits, other
  modifiers, F3/F4 speed, and that no column is pressed.
- **Fixed, not rebindable**: osu!'s key binding settings list only global actions and each ruleset's own actions.
  Adding wall actions to the ruleset's would change the ruleset's action set and put them into mania replays.
- **Any time**: unlike the speed, the wall moves at any point of a song and while paused, as lane covers do in the
  arcade games (IIDX's SUD+ is moved mid-song). It never changes what is judged, and the ruleset is never submittable.
- **Side effects, accepted**: moving the wall switches the preset to Custom, as moving the slider does; while watching
  a replay, holding Shift also fast-forwards it; a player with a column bound to Shift who presses F3 or F4 while
  holding it moves the wall instead of the speed.
- **How to reverse**: remove `HighwayHotkeys` from `DrawableManiaOrthoRuleset.load`.

## 2026-11-04: Product direction: a lazer game-mode mod later; a self-contained dev build with stable's files now (Alpha)

- **Alpha's direction** (QUESTIONS Q13): development stays inside `C:\Users\alpha\Documents\Projects\osu!`; the osu!
  files themselves (maps, skins, screenshots, user data) come from the original install (stable,
  `C:\Users\alpha\AppData\Local\osu!`) as much as possible. The end product should be a lightweight mod that
  attaches to the main osu! engine, with maniaortho as its own game mode; standalone is fine for now.
- **Now**: the full game runs portable from the clone (its realm and settings next to the build). Maps and skins come in
  through lazer's own "import from osu!stable", which on the same drive creates hard links (`LegacyImportManager`,
  `HardLinkHelper`): no copies, no extra space, and lazer never writes to imported files (its store is content-addressed),
  so stable's files are not changed. Screenshots go to stable's `Screenshots` folder. Alpha's lazer data
  (`%APPDATA%\osu`) is never opened: a dev build can migrate its realm to a newer schema, which the release client then
  refuses.
- **Later (the mod)**: lazer loads custom rulesets as a DLL from its data folder's `rulesets` directory and never submits
  their scores, so maniaortho can ship as one DLL for the official lazer client. Stable cannot load game modes, so the
  mod targets lazer. The blocker is the `PlaysBeatmapsOfRuleset` core patch (README "Core patches"): release lazer
  offers a custom ruleset only its own maps and osu!standard converts. Options to weigh when the time comes: an
  upstream pull request to ppy for that hook; a workaround inside the DLL. M3's "separate client" tasks (rebrand, own
  API endpoints) assumed a forked client and are on hold until that plan is made.
- **Invariant 2 still holds**: a custom ruleset is never submittable, in the fork or as a mod.

## 2026-11-04: Correction to "Gameplay hotkeys": when F3/F4 work

- The hotkeys entry says mania's F3/F4 work in "the first 10 seconds of a play". Precisely, they work until 10 s after
  the gameplay start time, which mania puts 2 s before the first note: so until 8 s after the first note, in breaks,
  and never while paused (`SubmittingPlayer.AllowCriticalSettingsAdjustment`; `research/game-osu-mania.md`).
- SPEC 3 said mania has "settings for constant / overlapping / sequential" scrolling. It has none: always sequential,
  constant only with the Constant Speed mod. SPEC 3 now says so; the highway already followed mania.

## 2026-11-04: A hold held past its end keeps its head on the line

- **Decision**: as in mania (`DrawableHoldNote.Update`: the sizing container keeps the head at the hit target until the
  hold is judged), a native hold note held past its end keeps its head on the judgement line with no body until the
  release is judged (up to 1.5x the Meh window after the end). It used to follow the tail below the line and vanish at
  the end. Found by the osu!mania research note (`research/game-osu-mania.md`, finding 3). The condition is mania's
  own (`Head.IsHit && !DroppedHoldAfter(StartTime)`, from the head's time on), not the note's `IsHolding` proposal, and
  like mania it skips holds with no drawn length (converted maps clamp a hold's end to its start), whose head passes
  the line like a tap's. Tests: `TestHoldHeldPastItsEndKeepsItsHeadOnTheLine` and `TestHoldWithNoLengthPassesTheLine`
  (each fails without its part of the fix), played by a replay so the checks see exact times.
- **Overrides** the 2026-11-03 "Native notes" entry's "follow the tail below it when held past the end".
- **Alternatives**: keep following the tail (looks like the hold ended early, unlike mania); pin while `IsHolding` (also
  pins a hold re-pressed after a drop, which mania does not).
- **How to reverse**: drop the `Math.Max(headDepth, 0)` clamp's past-the-end case in `HighwayNoteLayer.addHold`.

## 2026-11-04: Full game on Alpha's PC: a launcher script, no core patch

- **Context**: Alpha then said the mod has to install on official lazer, sharing files the way lazer does with stable
  (QUESTIONS Q15). Alpha's PC has stable but no lazer (no `%APPDATA%\osu`, no `%LOCALAPPDATA%\osulazer`; names
  listed, nothing opened). Alpha also wants the project kept isolated, with no upstream pull request (Q15 answer), so
  mania maps inside official lazer are parked and this build is how Alpha plays; it shares stable's files through
  exactly lazer's own import.
- **Decision**: `scripts/ortho/play.ps1` builds `osu.Desktop` (Debug) and starts it with its data in
  `Documents\Projects\osu!\ortho-data`. It writes `framework.ini` next to `osu!.exe`, which osu-framework already
  reads as "portable install" (`DesktopGameHost.GetDefaultGameStorage`: storage in the startup folder, so
  `%APPDATA%\osu` and `%APPDATA%\osu-development` are never opened), and `storage.ini` there, osu!'s own custom data
  location (`OsuStorage`), pointing at `ortho-data`. A note file in `ortho-data` keeps osu! from asking about an empty
  data folder (`PlayScriptDataFolderTest`). This replaces the planned portable switch in `osu.Desktop` (Q13 option a):
  same result, no core patch.
- **Data next to the clone, not inside it**: a fresh clone or `git clean -xdf` keeps Alpha's imports, settings and
  replays.
- **Stable's files** come in through osu!'s first-run "Import from osu!stable", which hard-links on the same drive
  (`LegacyModelImporter.PreferHardLinks`) and never changes stable's files. No code of ours.
- **Screenshots are moved, not linked**: PowerShell 5.1's `Remove-Item -Recurse` follows a junction, so removing
  `ortho-data` that way would empty stable's `Screenshots` folder through one, and osu!'s own "move data folder"
  (`MigratableStorage`) would copy all of stable's screenshots into the new folder. The script moves new screenshots
  there when the game closes and at the next start, never replacing a file.
- **Debug, not Release**: a Debug build uses osu!'s development endpoints (`OsuGameBase.UseDevelopmentServer`), so
  no login or score can reach osu.ppy.sh, even if Alpha logs in by mistake (it still downloads osu!'s public beatmap
  metadata file, `online.db`, from assets.ppy.sh, as every lazer does); Release would also refuse to start
  beside a running release lazer (single instance through IPC). The harness's frame times were Release builds of the
  rulesets; if Debug feels slower on Alpha's PC, revisit (Release plus our own endpoints would need a core patch).
- **Environment**: `OSU_EXTERNAL_UPDATE_PROVIDER` (no Velopack hooks, no update checks) and
  `OSU_DISABLE_ERROR_REPORTING`. Known effect outside the folder, as with release lazer: the NVIDIA "threaded
  optimisation" setting for `osu!.exe` is reset to Auto on start.
- **How to reverse**: delete the two ini files next to the build; the game then uses `%APPDATA%\osu-development`.

## 2026-11-04: osu.exe, a launcher for a packaged game (Alpha)

- **Alpha asked** for an .exe to just run, rebuilt when changes are made, and named it: "just call it osu".
- **Decision**: `scripts/ortho/package.ps1` publishes `osu.Desktop` (Debug, as `play.ps1` builds it) into
  `Documents\Projects\osu!\osu\game` and a small launcher, `ManiaOrtho.Launcher` (assembly name `osu`), as
  `osu\osu.exe` (single file, framework-dependent, about 190 KB). Double-clicking it does what `play.ps1` does: the
  framework.ini and storage.ini that keep the data in `ortho-data` (shared with `play.ps1`), temp files in
  `osu\temp`, `OSU_EXTERNAL_UPDATE_PROVIDER` and `OSU_DISABLE_ERROR_REPORTING`, screenshots moved into stable's folder
  after the game closes, a log in `osu\launcher.log`. It starts nothing if that game is already running.
- **Why a launcher and not osu!.exe itself**: the temp folder and the two variables are per process; setting them
  inside the game would mean editing `osu.Desktop/Program.cs` (a core patch). The game's own `osu!.exe` still works on
  its own with the data in `ortho-data`, only without those extras.
- **Why outside osu.sln**: adding a project there is a core-file change. The test project references the launcher, so
  `check.sh` builds it, analyses it and tests its logic (`GameLauncherTest`) without starting a game.
- **Packaging**: built beside the old package and swapped in at the end, so a failed build leaves the last one
  playable; refused while the packaged game runs; `osu\version.txt` names the commit. Framework-dependent because
  the PC already has .NET 10 (it builds the clone); self-contained would add about 70 MB per copy.
- **Name**: `osu.exe` runs as process "osu", the game as "osu!" (as stable does); the launcher tells them apart by path.
  The game's window title stays "osu! (development)" (a Debug build). No icon of osu!'s is copied.
- **How to reverse**: delete `ManiaOrtho.Launcher/`, its reference in the test project and `package.ps1`.

## 2026-11-04: Note thickness up to 8%

- Alpha's 4K note bars are 6.4% of the scroll length (`research/game-osu-mania.md`, finding 7); under the Parallel
  camera the setting is the same measure, so its range now goes to 8% (was 6%). Default and presets unchanged.
  Test: `TestNoteThicknessReachesBarsOfAlphasSkin`.

## 2026-11-04: Held wall keys keep osu-framework's repeat (research C4 dropped)

- **Decision**: no change. `research/topic-covers.md` C4 proposed a repeat timer of our own because the wall keys
  seemed to follow the operating system's key repeat (about 30 per second). They do not: osu-framework drops OS
  repeats and makes its own (`InputManager.updateKeyRepeat`: 250 ms, then every 70 ms, on the input manager's real-time
  clock, so pause and replay fast-forward do not change it). A held Shift+F4 moves the wall about 14 steps (70%) per
  second and crosses 0-90% in about 1.4 s, not half a second. A timer of our own at 100 ms was written, reviewed and
  reverted: it only slowed this to 10 steps a second, for more code.
- **Corrects** C4's premise ("OS key repeat, about 30 Hz", "25-75x beatoraja") and the 2026-11-03 hotkeys entry's
  "repeat on hold" only in that the repeat is osu-framework's, not the OS's.
- **Revisit** if Alpha finds the held rate too fast.

## 2026-11-04: Live pp is lazer's own

- **Decision**: no code for M3's live pp. Lazer's HUD pp counter and the results screen's pp rate a play through the
  score's ruleset, and `ManiaOrthoRuleset` hands difficulty and performance to mania, so both show osu!mania's number.
  Two tests pin it: `TestSceneReplayParity` compares the live counter after each replay, and
  `TestSceneResultsPerformanceParity` the results' pp (through the game's difficulty cache on an imported chart) with
  no mod, DT, HT, HR, NF and FL, against mania's own calculators.
- **Not added**: a pp counter in stable skins' layouts. Lazer's own skins show one; a stable skin's default HUD has none
  (`LegacySkin`, as in stable). Adding it would change osu!'s HUD for those skins, and the skin editor does it in a few
  clicks (PLAYTESTING L4). Revisit if Alpha wants it there by default.

## 2026-11-04: Correction to "Converter and difficulty calculator get the mania ruleset" (2026-11-03)

- That entry says the difficulty calculator's ruleset changes star rating. It does not: star rating and pp follow the
  converter, which the calculator reaches through the beatmap (and in game through the difficulty cache's already
  converted beatmap). Given ours, `ManiaDifficultyCalculator` would only treat the map as a convert in
  `CreateDifficultyAdjustmentModCombinations` and list the key mods too. Found by a mutation check for the live pp
  tests and the review. The decision stands (both get `MANIA`); `TestDifficultyModCombinationsMatchMania` now pins the
  calculator's half.

## 2026-11-04: Runtime skin porter (Alpha)

- **Alpha asked** for a porter that turns the flat osu!mania skins in the Skins folder into tilted ones "using the
  skin's base assets to render angled textures", inside the game, with no external software and no texture editing,
  gameplay field only, and several variants for skins whose notes stop above the bottom of the screen (IIDX-style).
- **Read through osu!'s own lookups**: images and per-column values come from `ISkinSource` with mania's legacy lookup
  names and fallbacks (`ManiaSkinConfigurationLookup`, `mania-note1H` → `mania-note1`, tail → head, default classic
  images), so every skin lazer draws flat is drawn tilted. Nothing is written to the skin. The port is built when the
  skin changes (never per frame) and only when mania itself uses its legacy pieces (a legacy stage background is in
  the stage); otherwise, or if reading throws, the highway's own look is used and the error is logged.
- **Measure, do not recompute, the flat layout**: hit position, unit size and stage width come from mania's laid-out
  stage (`FlatStage`), so the port always agrees with the flat game, including lazer's own clamping and scaling.
- **Defaults**: notes, hit effects and track default to "Skin (ported)" (Alpha wants to pick a skin and play); "Skin
  (warped)" keeps the old picture-warp, "Highway" the built-in look.
- **Key area** (`SkinKeyArea`, the IIDX question): five placements, screenshots in the report. Panel (default until
  Alpha picks on the card): the judgement line is lifted to the skin's hit position and the key area stands upright
  under it with vertical sides, as a cabinet panel. Floor: the same line, the key area lies on the floor and widens
  towards the player. Squeezed: the camera's line is kept and the key area is squeezed into the space below it.
  Receptors: the camera's line, only the part of the keys above the line (receptors) is drawn. Hidden: no keys; the
  highway's judgement line is drawn unless the skin draws one (`JudgementLine: 1`), because skins like Minty put
  their receptor line in the key images and leave the stage hint blank. The line is never lowered below the camera's.
- **Stage position** (`SkinStageFit`): SkinPosition (default) keeps the camera's track width and moves its centre to
  where stable centres the stage (`ColumnStart` + half the stage width, 853 units at 16:9), clamped on screen; Camera
  keeps the preset; SkinLayout also takes the skin's stage width. `ColumnStart` is parsed by lazer but not used, so it
  is read by reflection from `LegacySkin`'s internal configuration list (`TestReadingColumnStartStillWorks` fails if
  osu! renames it; the porter then centres); 136, stable's default, means "not set" (centred). Beatmap skins are
  skipped. This reads a core type, it does not patch one.
- **Note shapes**: with "Skin note proportions" (default on), a note's depth makes its image as tall for its width at
  the line as it is flat (Ākala's 296x152 bars stay bars, Flow's circles become ellipses lying on the track), capped at
  15% of the track; off, every note uses the preset's note thickness, scaled per image. Hold bodies tile along the
  track at the image's proportions (`NoteBodyStyle` RepeatBottom/RepeatTop, at most 200 tiles) or stretch (Stretch,
  skins before version 2.5); tails are drawn flipped past the end, as lazer draws them.
- **Sizes**: hit and hold lighting, the hint and the stage bottom keep their flat size on a wider track (lighting is
  drawn three to five lanes wide, growing it with a 2-3x wider track swamped the field) and shrink with a narrower one.
- **Mania's own legacy hit lighting** (explosions, hold light) is hidden and cleared while the ported lighting shows;
  the column containers that hold it are kept present so they still follow skin changes.
- **Not changed**: judgements, score and input. The lighting listens to each column's `NewResult` with mania's own
  conditions; nothing feeds back. Visibility mods work on ported notes because they go through the native note
  layer's cover cut.
- **Existing installs**: osu! stores every setting's default the first time a ruleset runs, so installs that ran
  maniaortho before kept "Highway" for the three styles. A hidden setting runs a one-time move: styles still on
  "Highway" become "Skin (ported)", any other choice is kept, and choosing "Highway" again afterwards sticks
  (`TestOldDefaultsMoveToThePorterOnce`). Q11 (default track) was open; Alpha's request answers it for skins.
- **Review follow-ups** (gameplay-reviewer): the lifted line is capped at the lift setting's 40%; note depth past the
  vanishing point falls back to the cap; body tiles are counted in whole steps and anchored to the scrolling head (a
  held body's pattern keeps moving, as mania's); keys and lane lights run on the game-wide clock as mania's key area
  does, and forget presses while hidden. Left as is: solid fills go through the subdivided grid (a few hundred quads at
  10K, only measurable on Alpha's PC); the hold check walks each column's notes up to three times a frame.
- **Parity**: `TestJudgementParityWithPortedSkin` plays both runs with the classic skin; it compares judgements, score
  and accuracy but not live pp, because the classic skin's HUD has no pp counter (every other case still compares it).
- **Known limitation**: keys keep their flat height and stretch to the lane's width, so round receptors (Mania Flow
  Mode's rings) become wide ellipses on a track wider than the flat stage. Notes keep their proportions.
- **How to reverse**: set the three styles back to Highway or Skin (warped); delete `SkinPort/`.

## 2026-11-04: Plays go to a CSV in the data folder

- **Decision**: `PlayLogRecorder`, added by the drawable ruleset, appends one row to `<data>\maniaortho\plays.csv`
  when a play is passed or failed (`GameplayState.HasPassed` / `HasFailed`, checked each frame). Quits, replays and
  autoplay (`HasReplayLoaded`) are not logged. Columns: the design doc's list plus rank, artist, title and difficulty
  name for reading, `mods_json` (each mod's settings, for exact recalculation) and `score_id` (osu!'s id of the score;
  only passes that reach the results screen are stored by osu!, so for others it points at nothing). Stars and pp come
  from the game's difficulty cache and mania's calculator, as on the results screen. The camera and wall are read at
  the end of the play (the hotkeys can move the wall mid-song). UTF-8 with a byte-order mark, so Excel reads titles.
  Accuracy and stars keep four decimals (osu! rounds down to two, so 99.996% would otherwise read as 100.00).
- **Replays**: osu! keeps no replay for a ruleset that is not one of its four (`Player.ImportScore` only encodes an
  `.osr` for those; `LegacyScoreEncoder` refuses others), so maniaortho plays had none to watch or export. The
  recorder saves each logged play's replay as an osu!mania `.osr` in `<data>\maniaortho\replays` (the `replay`
  column), from the frames recorded up to the pass or fail. It opens in osu! as a mania replay (flat stage); the parity
  tests can replay it through both rulesets (PLAYTESTING L4). Not uploaded anywhere. Reverse: drop `saveReplay`.
  Asked in Q18 whether Alpha wants them.
- **Columns change**: a file whose header differs is kept as `plays until <date>.csv` and a new one started.
- **Known limitation**: Excel reads a title starting with `=`, `+`, `-` or `@` as a formula; the CSV keeps the title as
  it is rather than adding a quote mark other readers would see.
- **Why polled, not `HealthProcessor.Failed`**: that event is a `Func<bool>` whose last handler decides whether the fail
  happens. A handler of ours would override Player's answer (No Fail, for one) and change gameplay (invariant 1).
- **Alternatives**: watching osu!'s score database (needs a component outside gameplay, so a core patch, and osu!
  keeps a fail only when its replay is saved from the fail screen); writing rows from `osu.exe` after the game closes
  (the launcher would need osu!'s database code).
- **Reverse**: remove the recorder from `DrawableManiaOrthoRuleset.load`.

## 2026-11-04: Speed and wall show the real visible time

- **Decision**: the scroll speed setting's label, mania's speed toast (F3/F4) and the wall toast (Shift+F3/F4) show how
  long notes are visible on the highway, not mania's `11485 / speed` (osu!mania note proposal 1, covers C3). The value
  is mania's own time range, as `DrawableManiaRuleset` sets it every frame (speed, the skin's hit position factor
  `(768 − H) / 643.2`, the rate, and the cap at 11485 game ms), in real time, times `1 − wall` when the wall keeps the
  speed, and without the wall under the flat camera, which draws mania's own stage. Speed itself is unchanged: the same
  skin and speed give the same visible time as in mania, so the coupling between a skin's hit position and the speed
  stays (a lower line shows notes longer, as in mania). Mania's label format is kept ("405ms (speed 29.0)"); the wall
  toast reads "40% covered · 405 ms visible", with "(speed kept)" when the wall shortens it.
- **Where the numbers come from**: `Highway/VisibleTime` repeats mania's three-line formula (its hit position and rate
  are private). In gameplay, `HighwayToasts` reads the hit position from the play's skin (beatmap skins included) and
  the rate from the gameplay clock's mod adjustments; the wall toast uses the play's `TargetTimeRange`, the speed toast
  the new speed (the toast fires before mania applies it). The settings label has no play: it uses the current skin at
  the selected beatmap's key count (and key mods), 4K with none selected; a beatmap's own skin is only in the toast.
- **Toasts moved**: the configs track nothing now (an empty list from `ManiaOrthoRulesetConfigManager`, none from the
  highway config); `HighwayToasts`, on the drawable ruleset, tracks both through small wrappers so each description can
  read the play. Rounded to the nearest ms (mania truncates).
- **Gate**: `TestSceneVisibleTime` measures, from where the highway draws a note (its depth against the wall's, or its
  height in mania's flat column), the real time it takes to the line, and both toasts match it within 1 ms: default and
  classic skins, DT and HT, wall kept or not, ParallelTilt and Flat, and the cap (speed 1 with DT: 7657 ms, not 11749).
  `TestScrollSpeedShowsTheVisibleTime` checks the settings label (405, 243 with a 40% speed-kept wall, 405 flat, the
  classic skin's value). Mutation: ignoring the hit position fails 4 of the toast cases and the label test.
- **Known limitation**: a speed changed while mania refuses it (paused, or late in a play, from the settings panel) gets
  a toast for the new speed although the play keeps the old one, as osu!'s own toast does. F3 and F4 never do this:
  mania ignores them then and nothing changes.
- **Review follow-ups** (gameplay-reviewer approved; all LOW): the classic skin's label is pinned to 396 ms rather
  than to the label's own lookup; a stub skin with a different 7K line covers the key count (7K and 4K mania maps, an
  osu! map with the 7K mod); the wall toast's name, cover text, keys and "Off" are asserted; the settings label's
  inputs are unbound on dispose; shared settings are reset in teardown; `VisibleTimeTest` pins the formula (cap, rate,
  wall, flat) without a renderer.
- **Reverse**: restore the old `LabelFormat` and the two `CreateTrackedSettings`, drop `HighwayToasts`.

## 2026-11-04: Hand-group dividers from 6K, and a pixel of gap between notes

- **Group dividers** (research/topic-wide-keys.md W1): the lane guides draw the boundaries between mania's default hand
  groups (6K 3|3, 7K 3|1|3, 8K 4|4, 9K 4|1|4, 10K 5|5; one group up to 5K) as a second tier: the same teal, twice a lane
  line's width (0.0044 track widths) and at least 2 px, alpha 1.0 near to 0.45 far (lane lines: 0.8 to 0.3). From the
  stage's key count alone (`LaneGroups`), so dual stages are grouped each on its own and bindings do not matter.
  They are part of the lane guides: off with "Lane guides" off, under the flat camera, and when the ported skin's own
  track is shown (it draws its own column lines; Alpha asked for skins to look like themselves). First-guess values
  from the research, to tune in playtests. Their sides fade over a pixel (`TrackDrawing.Feather`: core a pixel
  narrower, a pixel-wide fading edge each side, same overall width and brightness): a sharp line centred on a pixel
  boundary jumped 2 to 4 to 6 px wide down the 10K split, the most prominent line on screen (playtester). Lane lines
  stay sharp: they are 1-3 px, and a fading edge would grey the 1 px gap beside far notes.
- **Note gap floor** (W3): native note bars keep their 1% gap per side but never less than 1 px on screen, measured at
  the bar's far side (where it is narrowest), and never more than a quarter of the lane, so a note on a tiny lane still
  shows. At the line nothing changes under Chunithm; far away and at 7K+ neighbouring notes no longer merge (at the far
  end the floor applies even at 4K under Chunithm: 1% there is 0.45 px). Hold bodies keep their 80% width, but never
  wider than their head or tail: on lanes under about 9 px (10K Voltex in a small window, dual-stage 10K) the floor is
  more than the body's own inset and the body poked out under its caps (reviewer, SPEC 6). Ported notes are the skin's
  images, whose own margins stay.
- **Gate**: `WideKeysTest` (groups 1K-10K, the gap's three cases), `LaneLineTest.TestFeatheredLineKeepsItsWidth`,
  `TestGroupDividers` (7K and 10K line kinds in game) and `TestGroupDividersPerStage` (two 7K stages from an osu! map with
  7K and Dual Stages), `TestNotesKeepAPixelOfGap` (10K Voltex and 4K Chunithm, once a note is near the line: every bar
  at least 1 px each side, some held at the pixel, and under Chunithm some at 1%), `TestHoldBodiesStayWithinTheirEnds`
  (10K Voltex among the long notes). Mutations: no floor fails both gap cases (0.06 px at 10K Voltex's far end); no 1%
  share fails 4K Chunithm (1 px where 1.58 px was due); no cap limit on the body fails the hold test. Captures at 7K and
  10K (Chunithm, Voltex) reviewed by the playtester: the centre key and the 10K split found at a glance, far notes
  separated by a black pixel; after feathering the split widens smoothly (pixel rows 2, 2+, 4, 4+, 6).
- **Reverse**: `SetLine(..., Lane)` for every inner line; `NoteGap` back to `laneWidth * NOTE_GAP`; the body's inset back
  to its share alone.

## 2026-11-04: The playtest harness leaves no framework.ini in the test build output

- **Problem**: full checks run after a playtest failed a third of the tests with "Could not load file or assembly
  'MessagePack.Annotations'", and the test build output folder was gone afterwards. The harness runs portable from that
  folder, so osu-framework saves `framework.ini` there; a folder with that file is portable for every host started from
  it, NUnit's headless test hosts included, which take it as their storage and delete it when they clean up. Reproduced
  by touching the file and running one test scene: the folder disappeared.
- **Decision**: `PlaytestRunner` deletes the file when a run ends (its framework settings are set every run anyway), and
  `check.sh` deletes a stale one (a run killed by the timeout) before testing. The harness stays portable, so nothing
  of it reaches the user's application data.
- **Alternatives**: running the harness from a copy of the build output (650 MB per run, and the copy could go stale);
  a non-portable harness (would write to `%APPDATA%` on Alpha's PC).
- **Gate**: `PortableMarkerTest`; a playtest run followed by a test scene keeps the folder.
- **Reverse**: drop the `finally` in `PlaytestRunner.Run` and the line in `check.sh`.

## 2026-11-04: Optional far fade, in track depth, on notes drawn on the track

- **What** (research/game-yarg-clonehero.md Y3): a "Far fade" setting, 0 to 30% of the visible track, default 0 (Chunithm
  and SDVX show notes at full strength as soon as they appear). Notes fade in linearly in track depth from nothing where
  they appear (the far edge, or the wall's lower edge) to full at that share below it, as YARG's mask fades linearly in
  z. With a wall the band is a share of the visible part (a 40% wall and a 30% fade span 18% of the track). Beyond where
  notes appear nothing is drawn while it is on (the wall or the far edge hid it anyway).
- **A preference, not part of a preset**: like lane guides, so changing it never turns the preset into Custom and
  choosing a preset leaves it alone.
- **How**: one more `DepthCover` (`DepthCover.FarFade`) applied by the note layer on top of each column's mania cover;
  each quad is cut where either changes and drawn at the product of the two, so Hidden and Fade In (scored mods) still
  hide what they hide. Applies to Highway and Skin (ported) notes, holds included. Not to Skin (warped) notes, which are
  mania's own drawables in the warped stage, nor to mania's bar lines (drawn there too, faint; they show at full
  strength inside the band), nor under the flat camera (no note layer). Fading the warped stage as well would fade the
  skin's whole stage, unlike notes on the highway track; left for if Alpha asks.
- **Gate**: `DepthCoverTest` (the fade's values with and without a wall, past the line, its cuts),
  `TestSceneHighwayNativeNotes.TestFarFade` (every drawn note end at its depth's alpha, walls 0 and 40%, then none),
  `TestFarFadeMultipliesFadeIn` (alpha is Fade In's times the fade's where both fade), `TestJudgementParityWithFarFade`
  (30%, with a check that the run faded). Mutations: the fade skipped fails both scene cases; the covers combined by
  minimum instead of product fails the Fade In case. Captures in `/mnt/project-files/ortho-agent/far-fade/` (off, 25%,
  25% under a 30% wall, holds crossing the band, the classic skin ported); playtester: measured alpha within 0.005 of
  the formula, notes below the band pixel-identical to the reference, alpha 0 at the wall's edge, no banding.
- **Reverse**: drop `HighwaySetting.FarFade` and the `FarFade` cover in `HighwayNoteLayer.add`.

## 2026-11-04: The default playtest set has two speeds, and the speed is always set

- **What**: `playtest.sh` (and `playtest.ps1`) with no chart and no speed play synthetic-7k and real-4k at speed 29
  (Alpha's: 405 ms visible on lazer's skins) and synthetic-7k again at speed 14 (839 ms, where Chunithm and Sekai
  players' notes move at the line, `game-sekai.md` K3). Every other run without `--scroll-speed` plays at 29.
- **Why always set it**: the harness runs portable and keeps its settings between runs, so an unset speed was whatever
  an earlier run used, and two "default" captures could differ. Output folders are now `<chart>-speed<N>`.
- **Why not every chart at both speeds**: each run is 2-4 minutes per ruleset in the cloud; one 840 ms chart shows the
  slower look, and Alpha plays at 405 ms.
- **Reverse**: restore the `beatmaps` default and drop the `runs` list in both scripts.

## 2026-11-04: Hold and track opacity multiply the look as designed; a hit lights its lane's piece of the line

- **What** (BACKLOG M6, `research/game-sekai.md` K6, K7, K9): two preferences outside the presets, "Hold opacity"
  10-100% and "Track opacity" 0-100%, both 100% by default, and with the highway's hit effects each hit lights its
  lane's piece of the judgement line in its result's colour, fading linearly over 300 ms.
- **Opacities are multipliers, not absolute alphas.** K9 proposed "Track opacity" as the floor's alpha with today's 90%
  as the default. A multiplier (default 100%) leaves every look unchanged until it is moved, and the same slider then
  also scales a ported skin's own lane colours (most stable skins set them, with their own alpha) instead of overriding
  them. The cost: the highway floor tops out at today's 90%; osu!'s background dim already darkens what shows through.
  Hold opacity multiplies the body's state alphas (55% idle, 85% held, 35% dropped) and a ported skin's body image, as
  Sekai's Long Note Transparency does its connector; its 10% floor is Sekai's. Heads and tails keep full strength.
- **Not Quaver's idle-alpha reading** (`research/game-quaver.md` QV6: the slider as the idle body alpha, default
  55%, so 100% reaches Quaver's and argon's opaque bodies). One slider would then mean an alpha for Highway notes and a
  factor for a ported skin's body image, whose default (most skins draw opaque bodies) must stay as the skin has it.
  Opaque bodies already come with Skin (ported), Alpha's default. Revisit if Alpha wants solid Highway bodies.
- **What each covers**: Track opacity scales only the lanes' background (the highway floor above and below the line, or
  the skin's `Colour1..N` under Skin (ported)), not the rails, lines, stage sides, beams or lighting, so the lanes stay
  readable at 0%. Neither applies to Skin (warped) parts or the flat camera, which are mania's own drawables.
- **Slot lights**: part of "Hit effects: Highway" (no separate toggle), on the same results as the bursts (none on a
  miss). Colour by result (K7: colour is the grade cue with the top judgement text hidden): the burst's colour (Great
  orange, Good green-white, Meh muted green), except a Perfect's pale gold `#FFF4D0`. Peak 80% times the burst's own
  alpha (Meh dimmer), linear over 300 ms *(tune)*. Drawn as a screen-space band over the guides' judgement line and under
  the notes: solid over the line's glow height (3.3% H) and fading over 1% H beyond it; mixture blending, since additive
  would wash every colour over the gold glow towards the same pale yellow.
- **Tuned after the playtest** (first cut: the burst's gold for Perfect, 60%, Sekai's 500 ms): measured on captures,
  a Perfect's gold on the line's gold glow read as a thicker glow, not as the lane lit, and a 1/4 stream at 170 BPM lit
  5 to 7 of 7 lanes at once, so the whole line just looked thicker. Pale gold keeps the core white and makes the lane's
  piece a flat bright bar; 300 ms lights three or four lanes in such a stream; 80% keeps it visible once the burst over
  it (its first ~150 ms) has faded. Greats and Mehs are only checked in tests so far: autoplay hits only Perfects.
- **Rewinds**: rewound to before a hit, its lane is dark until the replayed hit lights it again; switching the effects
  off clears every light.
- **Two layering exceptions, accepted**: with Skin (ported) track and a key panel, the skin's keys stand in front of the
  line (as they stand in front of notes) and cover the band's lower half; Skin (warped) notes are drawn in the warped
  stage under the overlay, so the band (like the guides' judgement line) lies over them at the line.
- **Gate**: `TestSceneHighwaySettings.TestOpacities` (defaults, the 10% floor, outside presets),
  `TestSceneHighwayNativeNotes.TestHoldOpacity` (a held body at 85% then 40% of it, its head unchanged, the floor),
  `TestSceneHighwayTrack.TestTrackOpacity`, `TestSceneSkinPort.TestOpacitiesReachTheSkinsTrackAndHolds`,
  `HitBurstStyleTest` (slot fade, peak per result, colours), `HighwayTrackTest` (floor and ported lane background as
  drawn, the band across lanes, the slot band centred on the line at the camera's height and a lifted one),
  `TestSceneHighwayHitEffects.TestHitsLightTheirLanesPieceOfTheLine` (exactly the lanes hit in the last 300 ms, at the
  expected strength and colour, between the guides and the notes), `TestSlotsGoDarkWhenTheEffectsAreSwitchedOff` and the
  miss, flat and skin-effects cases; `TestJudgementParityWithNativeNotes` now also checks that slot lights fired during
  the parity run. Mutations: the hold factor dropped from native bodies fails `TestHoldOpacity`; hits not recording
  their time fails the slot scene test. Review (approve with fixes): tests reached only properties (draw values moved
  into tested helpers), the deactivation path was untested, the tail was unchecked; all fixed. Playtester: hold bodies at
  exactly 0.4x over the measured background (native and classic), the floor 0.9 to 0.36, nothing else changed, slot
  alphas within 0.01 of the formula, unlit lanes' line byte-identical; its Perfect and stream notes led to the tuning.
- **Reverse**: drop `HighwaySetting.HoldOpacity` / `TrackOpacity` and their three uses in the container; remove
  `HighwaySlotLights` and the slot arrays in `HighwayHitEffectLayer`.

## 2026-11-04: Key beam length: Long stays as it was; Half and Quarter are shares of the visible track on screen

- **What**: a "Key beam" setting (Long / Half / Quarter, default Long) for the Highway track's beam in a pressed lane,
  after DJMAX's KEYBEAM option (`research/game-djmax.md` R2). A preference outside the presets, like the opacities.
  Ported and warped skins keep their own lane lights' length.
- **Long is unchanged**: half the track's depth, which under perspective is most of the track on screen (81% under
  Chunithm, 90% under Voltex, 50% under Parallel). Nothing changes for anyone who leaves the default.
- **Half and Quarter are measured on screen**, as R2 proposes, so they look the same under every camera. The span is
  from the judgement line to where the track ends: the wall's edge when there is one, else the far edge. With a wall a
  "half" beam measured to the far edge would partly sit behind the wall. The beam still fades linearly in depth over its
  shorter span. Under Parallel without a wall, Half equals Long (depth and screen height are proportional there);
  accepted rather than inventing a longer Long.
- **Not done**: the Quaver-style release (QV9) the backlog item mentioned. It was conditional on a playtest showing the
  150 ms ease-out lingering, and single captures cannot show that; it stays open for Alpha's L4 notes.
- **Gate**: `HighwayTrackTest.TestBeamDepth` (R2's table for Chunithm and Voltex, Parallel, Long),
  `TestBeamReachesItsShareOfTheTrackOnScreen` (with and without a wall), `TestSceneHighwayTrack.TestKeyBeamLength`
  (the setting reaches the track; a quarter and a half of the visible track, with a wall at 30%; back to Long),
  `TestSceneHighwaySettings.TestKeyBeam` (default, persists, outside presets). The draw node reads the tested
  `HighwayTrack.BeamDepth`; the draw call itself is covered by the playtest: on synthetic-7k (Chunithm, 1280x720, line
  at y=664, far edge at 0) the beam faded out at y≈140, ≈340 and ≈490 against 129, 332 and 498 computed. Review
  (approve with fixes): the draw node computed its own depth (now reads the tested property), this entry, the harness
  kept other `--set` preferences between runs (it now resets every preference outside the presets per run), and a
  clearer hint; all fixed.
- **Reverse**: drop `HighwaySetting.KeyBeam`, `KeyBeamLength`, `HighwayTrack.KeyBeam`/`BeamDepth`/`GetBeamDepth`, and
  draw the beam to `BEAM_LENGTH` again.

## 2026-11-04: Kiai sway leans the track about the judgement line, with the beat, off by default

- **What**: a "Kiai sway" setting (0-100%, default 0, outside the presets; tilted cameras only). In kiai sections the far
  end of the track swings from side to side with the beat. SPEC 2 allows optional cosmetic motion that never moves the
  judgement line; no reference game does this (SDVX's roll is chart-driven and tilts the line, so `game-sdvx.md` P5
  rules it out), so the values below are our own.
- **A shear, not a rotation**: `HighwayLayout.Sway` shifts the track sideways in proportion to the height above the
  judgement line (`HighwayProjection.SwayAtY`), nothing on or below it. The line, the judgements, the key area, a ported
  skin's key panel and everything else below the line stay still, and lanes stay straight with one vanishing point (a
  shear composed with the homography is a homography). Everything on the track already draws through the projection,
  so the track, notes, warp, guides, lane lines and lights follow without changes. The wall, an upright panel, is
  sheared about its lower edge to keep covering the leaning lanes behind it. Accepted: the floor and rails below the
  line do not lean, so the track's edges bend slightly at the line (hidden under the line's glow at these sizes), and the
  top edge of bands centred on the line (glow, slot lights) leans by up to ~1 px.
- **Motion**: the far end is at one side on each beat and passes the middle between beats (cos of the swings since the
  first timing point, with the count carried across red lines so an off-beat or gimmick red line never snaps it). One
  swing is the beat, doubled until it is at least 300 ms (above 200 BPM two beats; gimmick timing never shakes). It grows
  over one swing from a section's start and settles over one swing after its end (smoothstep); sections closer than that
  are merged, as are consecutive kiai points. A pure function of the frame-stable time: seeks and captures are exact.
- **Size**: 100% moves the far edge 3% of the screen width (58 px at 1920), about a fifth of the Chunithm preset's
  far-edge width. Visible without hiding where a far note's lane is; tune after Alpha tries it.
- **Test chart**: the synthetic charts gain a kiai section (bars 8-12). Kiai does not touch judgement (only the legacy
  convert generators read it, and these charts are native mania); parity tests are unchanged and pass.
- **Gate**: `KiaiSwayTest` (still outside kiai, sides on the beats, smooth fades, kiai points in a row, merged sections,
  red lines off the beat and gimmick timing without a jump, swing lengths), `HighwayProjectionTest.TestSwayLeansTheTrack
  AboutTheLine` (line and below unchanged, far edge moved by the sway, lanes straight, vanishing point) and
  `TestSwayIsSanitised`, `TestSceneHighwaySettings.TestKiaiSwayIsOffByDefault`, `TestSceneHighwayTrack.TestKiaiSway`
  (none by default; with it, the sway the formula gives, shared by track, warp, guides, notes and slot lights; the line
  unchanged; the wall's corners leaning with the track; still after the section). A flipped wall shear fails the scene
  test. Playtest (synthetic-7k, 1280x720, Chunithm): on two consecutive beats the lane lines moved +37/-36 px at y=40,
  21 px at y=300 and 4 px at y=600 (36.1, 21.0, 3.7 computed), nothing below the line moved, and the score matched the
  run without sway; with the classic skin ported, the key panel stayed still while the stage leaned. Review (approve
  with fixes): the phase restarted at every red line (now carried on), gimmick timing could shake and pop (swing and fade
  at least 300 ms), the wall stayed upright (now sheared), close sections could still jump (merged), more layers in the
  scene test; all fixed. The 1 px band lean is accepted (above).
- **Reverse**: drop `HighwaySetting.KiaiSway`, `KiaiSway`, `HighwayLayout.Sway` and `SwayAtY` (and its three uses in the
  projection), and the wall's shear and bottom-left origin.

## 2026-11-04: Wall scale: the wall set in Chunithm's steps, IIDX's white number or a share of the lane

- **What**: Alpha asked (2026-11-04) for the wall to be settable "scaled to that game specifically or as a flat wall
  slider". A "Wall scale" setting picks the units the wall is set in, moved by in play and shown in: **Percent of the
  track** (default, today's slider, 5% per key press), **Chunithm (0-14)** (the arcade's own wall positions,
  `game-chunithm.md` "Positions", one step per press), **IIDX white number (0-1000)** (thousandths of the lane's height
  on screen, as SUDDEN+ counts, 50 per press) and **osu!mania / Quaver (% of the lane)** (share of the lane's height on
  screen, 5% per press). The wall is still stored as one share of the track's depth (`HighwaySetting.Wall`); a scale
  only converts (`WallScales`), so presets, the drawn wall and the visible time are unchanged. The panel shows one wall
  slider, in the chosen units; the others follow it and set the same wall. A preference outside the presets.
- **Why both depth and screen**: a share of the depth is a share of the visible time (and of Chunithm's steps, which
  are equal in depth up to 11). IIDX's white number and the lane covers of osu!mania and Quaver are drawn in the lane's
  screen space; under a converging camera the far end is squeezed, so 90% of the depth is only about 68% of the lane on
  screen with the Chunithm preset and 49% with Voltex (`topic-covers.md` C6). Each screen-based scale converts with the camera's far width
  ratio, the projection's own formula (`WallScaleTest` checks it against `HighwayProjection.YAt`), so its most follows
  the camera.
- **Speed behaviour comes with the game**: choosing Chunithm or osu!mania / Quaver turns "Wall keeps visible time" off
  (notes keep their speed and come out from under the opaque wall, which covers them, as in Chunithm: wall 5 leaves
  71% of the visible time, why Alpha's 9.0 becomes about 6.0 there); choosing IIDX turns it on (Floating Hi-Speed);
  Percent leaves it. The toggle can still be changed afterwards. Alpha's first question (do notes start at the wall, or
  at the far end and get covered by it?) is exactly this toggle: on, they start at the wall's edge and slow down; off,
  they run the full track under the wall.
- **Range**: Chunithm stops at 14 (88.5%), the last step below our 90%; its 15 and 16 cover the line and beyond. The
  screen scales stop where 90% of the depth lands.
- **Precision**: the stored wall moved from 0.01 to 0.0001 so every scale's units round trip (one white number can be as
  little as 0.00036 of the depth; `WallScaleTest.TestUnitsRoundTrip` rounds to the stored precision).
  The percent slider keeps 1% steps on its own bindable. The play log records the wall to 0.0001 and a new
  `wall_scale` column ("Chunithm 5"); its header changes, so an existing `plays.csv` is kept under a dated name.
- **Keys between units**: a wall between two units (set in another scale) counts from the unit behind it in the key's
  direction, so one press moves at most one step, and never against the key: a 90% wall stays at 90% when raised under
  Chunithm's scale, whose last step is 88.5%.
- **Gate**: `WallScaleTest` (Chunithm's table, screen share against the projection, round trips per scale and camera
  incl. both presets' and the steepest, most units, key steps from between units, ends and never moving back,
  speed behaviour, descriptions; dropping the never-back guard fails 6), `TestSceneHighwaySettings.TestWallScale` (default,
  the speed toggle per game, outside presets, kept) and `TestWallSliderPerScale` (one slider shown, each sets and follows
  the wall and the camera), `TestSceneHighwayHotkeys.TestKeysStepTheChosenScale` (five presses reach Chunithm's wall 5,
  drawn), `TestSceneVisibleTime.TestWallToastInChunithmUnits` ("wall 5 (speed kept) · N ms visible"),
  `TestScenePlayLogRecorder`. Playtest synthetic-7k at speed 14, Chunithm's wall 5 (summary: wall 0.2911, scale
  Chunithm, speed kept): notes come out from under the wall's base at 0.08H, as computed; mania and maniaortho both
  100%, 500000. Review (approve with fixes): the top unit disagreed with the stored most and a raise at the top could
  lower the wall (MED; now the most is the wall's most in units and steps never go back), steps could skip from between
  units, the harness's `--speed-lock` lost to a `--set` scale, a toggle left over between hotkey tests, the flat toast
  ignored the scale, the play log rounded the wall, no try/finally on the sync guard, the IIDX slider's arrow step; all
  fixed. Accepted: a wall under half a Chunithm step reads "wall 0" in Chunithm's units (only reachable from another
  scale).
- **Reverse**: drop `HighwaySetting.WallScale`, `WallScale.cs`, the panel's derived sliders and the coupling in
  `HighwayConfigManager`; hotkeys step `WALL_STEP` again and the toast prints the percent.

## 2026-11-04: A side-placed skin stage keeps its centre exactly; where its lanes converge is a setting (Alpha)

- **Alpha asked** to look at games whose notes are not centred (IIDX, StepMania, Pump It Up and others) and for "the
  angled lane [to] have the same x coordinates for the centering of the lane, to maintain the original position", by
  projection rather than by remaking the skin. Research: `research/topic-off-centre-fields.md`.
- **Centre, exactly**: with "Skin position" (default) and "Skin position and width", the track's centre at the
  judgement line is stable's stage centre (`ColumnStart` + half the skin's stage width). Before, a camera track too wide
  for that spot was slid inwards to stay on screen (the classic skin copied with `ColumnStart` 40 at 4K landed 150 px
  right of where stable puts it at 1280x720; R Skin's widest stages 1.2% of the width). Now it is narrowed about the
  kept centre instead (StepMania's `NotefieldZoom` and YARG's width cap also shrink rather than slide; no reference
  slides), with no margin, never below the skin's own stage width, and with the Floor, Squeezed, Receptors and Hidden
  key areas it fits the bottom row, where the lanes have widened further (Panel's sides are upright).
  `SkinPortLayout.CentreOn`. Deviation from the research note's O3: there is no last-resort clamp. A stage the skin
  itself draws partly off screen keeps its centre and width and hangs off the edge as it does flat in stable, and
  the skin-width floor wins over the bottom-row fit, so a floor key area under a stage at the very edge can widen past
  the screen. Moving the centre would break the one thing Alpha asked for.
- **Convergence** (`SkinConvergence`, "Skin stage lanes converge"): Above the stage (default: no lean, the centre line
  upright at the skin's x at every depth; StepMania skew 0), Halfway, Screen centre (StepMania Incoming/Space, YARG
  multiplayer, Rock Band). Built as `HighwayLayout.Lean`, a lasting shear about the judgement line added to the kiai
  sway, `(target − centre) · (1 − far/near width)`, which puts the vanishing point at the target and leaves the line
  and everything below it where the skin puts them. Parallel cameras have no vanishing point and do not lean. The
  default is the literal reading of Alpha's words; asked as Q20 with screenshots.
- **Accepted**: below the line nothing leans, so with a lean (Halfway or Screen centre, not the default) the lane edges
  bend at the line with the Floor and Squeezed key areas (as with kiai sway, but larger); upright images that span the
  line (the stage hint, key images reaching above it outside Panel) shear above it, and in Panel the key images above
  the line stay upright while the lanes behind them lean. All cosmetic, in non-default options. Panel's sides are
  upright and show no bend.
- **Classic-skin experiment** (copies of osu!'s classic skin in scratch, not committed): notes and heads at 200%
  height, every animation frame and image top labelled, the stage moved left with a raised hit position and right.
  Notes, heads and tails keep the edited proportions; tails flip as lazer flips them; unheld bodies show frame 0 and
  held bodies animate, as lazer's; hit and hold lighting frames matched flat at the same instants. Two differences:
  (1) mania overlaps explosions (a pool per column) while the port restarted one per column, so a release soon after a
  press replaced the earlier explosion; now each column keeps its last four hits (`PortedLighting.HITS_PER_COLUMN`).
  (2) lazer stretches a slice of a legacy hold body over its whole length whatever `NoteBodyStyle` says; the port tiles
  it as stable does. Kept: it is the skin author's intent, and uniform bodies (nearly all) look the same either way.
- **Not changed**: judgements, scoring, input; the porter still never writes to a skin.
- **Reverse**: `SkinPortLayout.CentreOn` back to the clamp, drop `SkinConvergence`, `HighwaySetting.Convergence`,
  `HighwayLayout.Lean`/`LeanTowards`, and `HITS_PER_COLUMN` (back to one slot).

## 2026-11-04: Ported lanes at the skin's own width; side stages lean and pull by sliders (Alpha)

- **Alpha asked** (after the off-centre screenshots) for thinner lanes, "more consistent with the original skin lane
  width", with the classic skin's look as the reference, and for side stages to be "easier for gameplay, rather than a
  complete shift".
- **Width**: the stage fit's default moves from "Skin position" (the preset's track width, 0.61 of the screen on
  Chunithm) to "Skin position and width": every lane is exactly as wide at the judgement line as the skin draws it flat,
  and the key panel matches the flat one pixel for pixel. The preset's far/near ratio still shapes the far end, so a
  4K classic stage is about 3% of the screen wide at the top. Installs that stored the old default move once
  (`SkinLayoutDefaultApplied`, as `SkinPortDefaultsApplied` did for the styles); choosing "Skin position" again sticks.
- **Side stages**: the three-way convergence choice becomes two sliders. "Side stage lean" (0-100%, default 50%) puts
  the vanishing point that far from the stage's centre to the screen's (0% upright, 100% the old "screen centre"),
  still as the shear about the judgement line, so the line and key area stay at the skin's x. "Side stage pull" (0-100%,
  default 0%) moves the whole stage that far towards the screen's centre, for players who prefer it nearer the middle;
  it is the only setting that gives up the exact x Alpha asked for earlier, so it is off by default. Q20 asks which of the
  two Alpha meant; the default is lean only.
- **Accepted, now at the default**: the lean artefacts the convergence entry accepted for non-default options now show
  for every side-placed skin (lean 50%): in Panel (the default key area) key images reaching above the line stay
  upright while the lanes behind them lean; with Floor and Squeezed the lane edges bend at the line; upright images
  spanning the line (the stage hint) shear above it. At half the old Screen centre lean they are half as large, all
  cosmetic, and lean 0% removes them. Centred skins and parallel cameras do not lean at all.
- **Superseded**: `SkinConvergence` and `HighwaySetting.Convergence` (one hour old, never packaged) are replaced by
  `SideStageLean` and `SideStagePull`.
- **Reverse**: set the fit default back to `SkinPosition` and drop `applySkinLayoutDefault`; set the lean default to 0.

## 2026-11-04: The wall covers only the lanes, found from the columns as laid out

- **What**: Alpha asked (2026-11-04), for once skin integration was done, that the wall "covers only the lanes, rather
  than the entire screen, and that it can identify where the lanes are effectively". The skin porter's thread called the
  porter done (lane positions final; Q16 and Q20 are only defaults). This replaces "The field wall is a vertical panel"
  (2026-11-03): the wall was a rectangle as wide as the track at its depth, up to the top of the screen, so above its base
  it also covered the background beside the converging lanes and, with Voltex's lower far edge, above them.
- **Shape**: per stage, the track between its outer columns' edges from the wall's depth to the far edge: a trapezoid on
  the lanes (like IIDX's SUDDEN+ and Quaver's cover, in the lane plane) with the teal edge at its base. It is widened by
  half the guides' edge line (at least half a pixel) and half a pixel, so the stage's edge lines go under it.
- **Where the lanes are**: the stage spans the container already measures each frame from mania's own columns
  (`stageLanes`, the same spans the floor, rails and guides use). So it follows a ported skin's stage position, column
  widths and spacing, `StageFit`, a side stage's lean (Convergence), the kiai sway and dual stages (one cover per stage,
  the gap left open), without code of its own for any of them.
- **What shows now**: the background beside and above the lanes. The highway's floor and rails already ended at the
  wall's base. A ported skin's stage (borders, lane backgrounds, column lines) is drawn up to mania's scrolling area: with
  the default "Wall keeps visible time" that ends at the wall's base too, so the wall stands above the lanes alone; when
  keeping the speed it is the whole track, so the skin's borders run on beside the wall to the far edge. Both kept as
  they are (the porter's layout, not the wall's). Nothing that moves in the lanes runs past the far edge (native and
  ported notes are clipped at depth 1; the warp maps only mania's scrolling area).
- **Gate**: `HighwayWallTest` (lanes and edge lines covered from the wall to the far edge, nothing 2 px beside them, before
  the wall or above the far edge, under Chunithm, Voltex, parallel, a side stage and the sway; the edge band),
  `TestSceneHighwayPlayfield.TestWallStandsWhereNotesAppear` (one cover per stage on the measured lanes, edge lines under
  it, nothing beside or above), `TestSceneHighwayTrack.TestDualStages` (each stage covered, not the gap) and `TestKiaiSway`
  (the cover's corners move with the lanes'). Playtests at wall 40%: synthetic-7k and real-4k with the default skin and
  with the classic skin ported (Chunithm), keeping the speed and (synthetic-7k) the visible time, synthetic-7k under
  Voltex; the wall sits on the lanes only, the rails end at its base, the classic skin's grey borders run on beside it
  only when keeping the speed (`/mnt/project-files/ortho-agent/lane-wall/`). Review (approve with fixes): the default
  mode had not been playtested with a ported skin and this entry overstated the borders (fixed above), the rubric still
  said nothing is drawn beyond the wall (PLAYTESTING item 8 updated), the scene tests compared the wall's stages with the
  same array (now with the first and last columns' edges), the sway check mixed units; all fixed.
- **Also**: `TestKiaiSway` now stops the clock on a beat inside the kiai section. With a running clock it timed out on this
  runner, on the committed code too (the frame-stable clock was past the chart when the first steps ran).
- **Reverse**: restore the 2026-11-03 rectangle (`HighwayWall` as a composite of two boxes, placed by its lower-left corner
  and sheared by the sway) and drop `SetStages`.

## 2026-11-04: Highway notes meet the lane lines, corner by corner (Alpha)

- **Alpha asked** (after a demo): notes had "a tiny bit of clipping/space (not completely flush) with the lines in
  between"; make them "visually as seamless as possible". Close-ups of the default skin at 4K showed a 1-2 px dark gap
  between each note and the line (the 1% margin per side, at least 1 px, from "a pixel of gap between notes").
- **What**: with the lane guides drawn, each plain note bar (taps, hold heads and tails) spans from the solid edge of the
  line on its left to the one on its right (`HighwayNoteLayer.NoteSides`, `HighwayGuides.SolidHalfWidth`): half the
  line's width, at least its pixel floor, and for the feathered group dividers their solid core (a pixel narrower). Hold
  bodies are 80% of the narrowest their column's bars get on the track (at its near or far end), centred, so they keep
  one width as they scroll and never pass their head or tail. Without lane lines (guides off, or the ported skin's own track) the old
  gap stays, so neighbouring notes do not merge into one bar.
- **Why corner by corner**: native notes are drawn over the lines, and a line is often 1 px. Placing the sides by one
  measurement per bar failed both ways in captures: measured at the near side, a note a few hundredths of a pixel too
  wide took the line's only pixel (the centre line vanished between two notes); measured at the far side, the centre
  line (on a pixel boundary at an even window width) had a dark pixel column beside the note on one side. Each corner is
  now placed on the line's edge at its own depth: a line's edge is straight on screen (its width grows with the track,
  linearly in screen height, until its floor, then stays), so the bar's side between two corners is the same straight
  line (within a hundredth of a pixel where a bar straddles the point where the floor takes over). Captures at 4K (pixel rows across the centre line: note, line, note, with nothing between), 7K beside the stage
  edge and a lane line, and 10K Voltex far away beside 1 px lines and the centre divider.
- **Gate**: `TestNotesMeetTheLaneLines` (Chunithm 4K, 7K and 7K dual stages, Voltex 10K: every corner of every bar,
  taps and hold ends with a held head on the line, within 0.001 px of the line's solid edge as `TrackDrawing.GetLaneLine`
  builds it from the guides' own layout and scale, floored lines and group dividers included),
  `TestSolidHalfWidth`, `TestNotesKeepAPixelOfGapWithoutLines` (the old gap rule, guides off),
  `TestHoldBodiesStayWithinTheirEnds` (80% of the narrowest bar, centred, within both ends at every corner, the same
  width a moment later).
- **Supersedes** the "Note gap floor" part of "Hand-group dividers from 6K, and a pixel of gap between notes" wherever
  lines are drawn.
- **Reverse**: pass an empty span of lines to `HighwayNoteLayer.Gather`.

## 2026-11-04: Settings consistent with osu!mania (Alpha)

- **Alpha asked** that every lazer setting that affects gameplay works as in osu!mania, hit lighting and key bindings
  named, "as extensive with each lazer setting as possible". Every setting is listed with where it acts and how it was
  checked in `SETTINGS-PARITY.md`. Most act outside the playfield and run the same code in both modes.
- **maniaortho plays with osu!mania's keys** (`DrawableManiaOrthoRuleset.CreateInputManager` gives mania's input manager
  mania's ruleset info, which is only where it looks its bindings up). Keys map to the same actions, so judgement and
  replays are untouched (invariant 1); `TestSceneKeyBindings` presses a rebound key in both modes. Asked on a decision
  card (Q21); this is its recommended option, easily undone.
- **The maniaortho key section stays, mirrored.** It cannot be hidden without patching osu!: the settings list every
  ruleset, and an empty variant list would also drop song select's key-count grouping for maniaortho. `KeyBindingMirror`
  keeps the stored keys equal to osu!mania's both ways. It lives with `ManiaOrthoSettingsSubsection`, the ruleset's only
  part of the game that exists while keys are changed (settings sections load when the settings first open), and writes
  with `WriteAsync`, which does not need the subsection to be updated while the key binding panel covers it.
  - After the first sync it copies only the columns (variant, action) a change touched, so the echo of its own copy
    finds both sides equal and writes nothing, and an older key is never carried back over one changed meanwhile (review:
    copying the whole side lost a second quick edit). A removed key copies the whole side.
  - The first sync (`Merge`) runs when the settings open and as every maniaortho play loads, so keys set for maniaortho
    before the change reach gameplay without the settings ever opening. Per key count, the side changed from osu!mania's
    defaults wins; when both were changed osu!mania's win (what the player last played osu!mania with) and maniaortho's
    are written to osu!'s log first, so they can be found again.
  - osu!'s key binding panel reads its keys once, so a change copied from the other side shows there after osu!
    restarts. Gameplay uses it at once. Refreshing the panel would need a patch to osu!.
- **Wall keys follow the scroll speed bindings** (Shift with them), read from realm as they change, instead of a fixed
  Shift+F3/F4, and the wall's toast names the keys bound now. Matching uses the binding's own modifiers exactly, so
  Ctrl+Shift+F4 still leaves the wall alone, and a scroll speed binding that itself includes Shift cannot move the wall.
  An action with no key stored keeps osu!'s default.
- **Touch overlay** added to maniaortho's settings (mania shows it on every platform; the config already had it).
- **Hit lighting** is left alone: osu!mania never reads it (only osu! and osu!catch do), so the highway does not either.
  The highway's own effects have "Hit effects".
- **Harness**: `playtest.sh --game-set Name=value,...` sets osu!'s and osu!mania's own settings in both modes, through
  each setting's bindable as osu! reads its settings files; the summary records them.
- **Not changed** (Q22): HUD layouts per mode, the fail animation's tumbling notes, touching lanes directly.

## 2026-11-04: Live osu!mania check (M4 daily job)

- **Decision**: `scripts/ortho/live-mania.sh` compares mania's judgement, scoring and difficulty code with the release
  osu!'s servers use (`osu-queue-score-statistics` pins `ppy.osu.Game.Rulesets.Mania`), and a GitHub Actions workflow
  runs it daily and opens a pull request when that release changes (README "Live osu!mania check").
- **Compared**: mania's rules (`ManiaRuleset.cs`, Beatmaps, Difficulty, Judgements, MathUtils, Mods, Objects with their
  drawables, which judge, Scoring, and `Column.cs` / `OrderedHitPolicy.cs`, the order notes are judged in) and the
  shared code under them (beatmap decoding and conversion, control points, hit objects, mods and `ModUtils`' rates,
  judgements, scoring and health, `IBeatmapDifficultyInfo`'s hit window ranges, `WorkingBeatmap`, `LegacyRandom`,
  difficulty). Wider than "difficulty code", as the backlog put it, because invariant 1 covers judgements, score and
  accuracy too. Files are named one by one where a folder holds other things: `osu.Game/Beatmaps` holds the core patch
  `BeatmapInfoExtensions.cs`, which a proposal must never revert (review: the first list missed `LegacySortHelper`,
  `ModUtils` and the hit policy).
- **Proposal**: when the branch lacks the release, merge it (the fork follows upstream, so this is the bump), then take
  the release's version of any compared file that still differs; a merge that conflicts is aborted and only the files
  are taken. Where the branch already has the release, a difference is a change upstream made after it that osu! has
  not deployed: the proposal reverts it and says to close it if the change cannot affect a result.
- **When**: only when the release differs from `docs/ortho/live-mania-version.txt` (one pull request per release, open
  or closed, its branch `ortho/live-mania-<release>`), not every day a difference stays, so a reviewed difference does
  not come back. A kept difference is taken out of the pull request (or named with `--record`) and noted here; closing
  the pull request instead would leave the release unchecked.
- **GitHub limits** (review): the workflow's token cannot push workflow changes, which merging an upstream release often
  brings, so then it takes only the files; and a pull request it cannot open (Actions may not create them by default)
  removes its branch, so the next run tries again instead of finding the branch and stopping.
- **Why Actions, not a daily Claude routine**: free and deterministic; a routine starts a session every day. It needs
  Actions enabled and the workflow on the default branch (Q23); until then the work loop runs the check.
- **Today**: live is 2026.1026.0, pinned as checked. This branch differs from it in `HealthProcessor.cs` and
  `HealthValue.cs`, upstream's health graph for replays (#39078), merged after that release: it records each health
  value next to the judgement and never changes health, failing, judgements or score, so it stays.
- **Shallow clones** (like this container's): the release is fetched as one commit, so whether the branch has it can be
  "unknown" and only files are taken; a full clone fetches it with its history, so its commit never becomes a shallow
  root that cuts `git log` short.

## 2026-11-04: Saved plays rated again when the calculators change (M4)

- **Stored scores** are the rows of `plays.csv`: osu! stores no maniaortho score of its own with pp (it saves scores
  only for its four rulesets' replays), and its own star ratings are recalculated by osu! itself when a ruleset's
  difficulty version rises (`BackgroundDataStoreProcessor.clearOutdatedStarRatings`).
- **Decision**: `PlayLogRatings` rates every row again (stars, pp, difficulty version) when the calculators differ from
  those named in `maniaortho\plays rated by.txt`: osu!mania's difficulty calculator version, and the osu!mania release
  whose scoring code this build was checked against (`docs/ortho/live-mania-version.txt`, embedded in the DLL). The
  release matters because upstream changes pp without raising the difficulty version, and a live check bump is how this
  fork takes such a change.
- **From what**: the beatmap osu! has under the row's MD5, the row's `mods_json` (each mod's settings) and judgement
  counts, through maniaortho's ruleset (osu!mania's calculators, invariant 5), with the beatmap's maxima from a score
  processor as the play's own score had them. A row whose mods do not read back (a mod this build lacks, a stored
  setting it no longer takes, compared by value so a new order or default of settings does not matter) is not rated:
  it would be rated as another play.
- **Waiting rows**: a row whose beatmap osu! no longer has (or not yet: a map deleted and imported again), or that fails,
  keeps its values and is listed under the stamp's first line; each later play tries those rows again, and they leave
  the list once rated. A failure is said once in osu! (a notification), when the calculators change; retries only go to
  the log file.
- **When**: in the recorder's background write, after the new play's row is added, so the first play after an update
  does it and a long rating never delays (or loses) that row. The log is read and written back under its lock, but
  rated without it; ratings are put back by play (time, beatmap, score, score ID), so rows added meanwhile (rated by
  these calculators) are left as they are and a row removed meanwhile stays removed. One rating at a time: a play ending during another's rating leaves it to that one. A log written before
  the stamp existed has only rows of another difficulty version rated again. The file as it was is kept as
  `plays before rating again <date>.csv` (numbered if taken), the new one written beside it and moved over it. A file
  open in a program that locks it (Excel) cannot be read, keeps the stamp old and is tried again after the next play; a
  file a spreadsheet saved in another encoding than UTF-8 is left alone (rewriting it would lose its accents).
- **Not rated again**: older files kept as `plays until <date>.csv` after a column change; a row the recorder itself
  could not rate (stars empty, osu!'s difficulty lookup gave nothing) until the next calculator change, since finding
  those would mean reading the log after every play (it never happened in testing).
- **Alternatives**: rating at game start (needs a component outside gameplay, a core patch); a settings button (a
  manual step that is easy to forget).

## 2026-11-04: A local score server rates plays with osu!'s published calculators

- **What**: Alpha asked for "my own local server which imitates but does not copy bancho", with "its own separate scoring
  system (that 1:1 matches and updates alongside osu!'s own rework systems)", for personal use. `ortho-server/` is a small
  ASP.NET Core server on 127.0.0.1:7727 with SQLite; the game sends each logged play when the new "Send plays to my score
  server" setting is on (off by default).
- **1:1 with osu!**: the server does not reimplement pp. It references NuGet `ppy.osu.Game.Rulesets.Mania` at the version
  osu!'s live score processor (`ppy/osu-queue-score-statistics`, newest release tag by version) depends on, today
  2026.730.0, and applies `ScorePerformanceProcessor`'s rules for a lazer score: passes only, Ranked or Approved maps,
  every mod `Ranked` (default settings), Classic not allowed. Totals follow `UserTotalPerformanceAggregateHelper` and the
  preserve rule (`CheckIsUserHigh`): best pp and best score per map and mod set kept, best 1000 by pp, best per map,
  0.95^n, bonus (417 - 1/3)(1 - 0.995^n), accuracy the same way.
- **Following reworks**: the server reads the live version at start and every 12 h (git's smart-HTTP ref list and the
  raw csproj on GitHub; api.github.com is not needed); `server-update` writes it to `calculator-version.txt`, rebuilds,
  and the next start rates every play again. A newer live version shows on the page and in the toast.
- **Parity with the game**: `contract/pp-parity.json` is written from the game's calculators (this repo's mania) and
  checked against the server's (osu!'s published ones): six synthetic plays, 4K and 7K, with DT, HT, NF+EZ, HD+MR, all
  equal. The upload and answer types (`contract/Messages/`) are compiled into both, so neither side
  can rename a field alone.
- **Map status**: the game sends the status its local database has (`BeatmapInfo.Status`); a stale status (a map ranked
  since the last update) shows as "not ranked" until osu! refreshes it in the game. The server cannot ask osu! (it never
  contacts osu!'s API).
- **Not Bancho**: no osu! protocol, no logins, no osu! passwords, no beatmap mirroring; map files come from the player's own
  game. `ScoreServerClient` refuses any `ppy.sh` host before sending (invariant 2 reworded to say so). Whether lazer's own
  login and leaderboards should point at it is Q26 (needs core patches; default: no).
- **Delivery**: each play is written to `maniaortho/outbox` first and leaves it only once the server stored it (or had it:
  `score_id` is stored once) or refused it, so nothing is lost to a closed game, a server that is away or a wrong address;
  one send at a time, oldest first. Redirects are never followed (a redirect could carry the play to any host, osu!'s
  included): a 3xx, 404, 405 or an answer that is not a score server's keeps the plays waiting with "is the address
  right?". A 5xx keeps them waiting too, but a play the server fails on five times is set aside so it cannot hold up the
  rest; a 4xx refusal sets the play aside in `outbox refused` with the reason. Disk errors on one queued play skip it, not
  the queue. POSTs carry an `X-ManiaOrtho` header that the server requires, so a web page open in a browser cannot post to
  it (CORS preflight). The toast gives the play's pp, its place among the best plays and the new total with its change.
- **Where it is not exactly osu!**: (1) osu! applies the preserve rule when a score arrives and deletes the others for good;
  the server keeps every play and applies the rule over all of them each time, so after a rework a play osu! would
  already have deleted can count (slightly higher totals, only after a rework). Kept: deleting plays is not reversible and
  a local record is worth more. (2) osu! reads stars and attributes stored as floats by its separate difficulty
  calculator deploy; the server computes them in doubles, so pp can differ in the second decimal. (3) osu!'s pp
  blacklist of a few maps is not known here. (4) Map status is the game's local copy (above).
- **Alternatives**: rating in the game only (no history across reworks, which Alpha wanted); a Bancho-compatible server
  such as a fork of an existing private server (osu! protocol, AGPL code, logins: far more than asked and closer to
  osu!'s services); building the server against this repo's projects (two `osu.Game` assemblies; also drifts with the
  fork rather than osu!'s live version).
- **Reverse**: turn the setting off (nothing is sent); delete `ortho-server/`, `osu.Game.Rulesets.ManiaOrtho/ScoreServer`
  and the recorder's upload block.

## 2026-11-04: Signing in to the score server from the game (Alpha, Q26)

- **What**: Alpha chose "Also log in to it in-game" on the Q26 card, then asked for "the conventional login methods used
  by other 3rd party servers, so that i can ideally connect, then log in through the lazer client, rather than having to
  do finnicking around with files". Lazer private servers are reached by pointing every endpoint at one address; the
  server then answers lazer's OAuth and `/api/v2` shapes. Done the same way here, with the address as a setting
  (Settings > Online > Server) rather than a patched binary, an environment variable or a file to edit.
- **Game (core patches, listed in README)**: `OsuSetting.ServerAddress`, read once at start by `CreateEndpoints`
  (restart to change, as the endpoints are fixed for the game's lifetime and every online component holds them). A
  `ppy.sh` host is never taken there, so the setting cannot be used to talk to osu! under another configuration.
  A saved sign-in belongs to the server that issued it: `OsuSetting.TokenServer` records the API address the saved token
  came from, and at start a token from another server is forgotten before the API reads it (a token saved before this
  patch is osu!'s, so nobody is signed out of osu! by the update). Confirming a different address also signs out at
  once. Found by review: signing in again before restarting, or editing `game.ini`, would otherwise have sent osu!'s token
  to the private server at the next start (or the reverse).
  The sign-in panel names the server, so the private sign-in cannot be mistaken for osu!'s.
- **Plain http, loopback only**: osu-framework upgrades http to https on every request (`AllowInsecureRequests`). The
  server has no certificate and should not need one on 127.0.0.1, so `EndpointConfiguration.AllowsPlainHttp` lets
  requests, sign-in and registration use http only when the custom server is itself http on a loopback address and the
  request goes to that same scheme and authority. A server on another machine is reached over https only: an http
  address there is taken as https (the framework would upgrade API requests anyway, but the SignalR hubs and the play
  uploads would not, and they carry the token). Alternatives: a local certificate (setup Alpha asked to avoid); allowing
  http to any custom address (passwords and tokens in plain text on a network).
- **Server**: accounts of its own (PBKDF2-SHA256, 600k iterations as OWASP gives for SHA-256, each hash keeping its
  count; an unknown name is checked against a dummy hash so timing does not tell which names exist; tokens stored as
  SHA-256; access a day, refresh 90 days, rotated on use), registration through lazer's own Register screen (`POST /users`), the routes lazer calls after
  sign-in answered in the shapes of lazer's types (`LazerApiTest` deserializes each with NuGet `ppy.osu.Game`), empty
  answers for friends, chat, notifications and the like so no screen shows errors. Written from lazer's client code
  (MIT) and observed requests; no code from AGPL private servers (read for reference only).
- **Details lazer forced**: account ids start at 2 (lazer treats user 1 as nobody: no statistics, no avatar); avatars
  are an empty string (any other host triggers lazer's "blocking resource lookup" notification, and null falls back to
  `a.ppy.sh/{id}`, someone else's picture); beatmap covers are `{}` unless the play has an osu! beatmapset id.
- **maniaortho shown as osu!mania**: lazer's profile ruleset selector lists only its four rulesets, so the server's
  maniaortho pp, rank and plays are the user's osu!mania statistics on this server (4K/7K as osu!mania's variants). The
  game never submits osu!mania scores here: lazer's solo submission (`beatmaps/{id}/solo/scores`) is refused with 403,
  and lazer makes no submission token for non-legacy rulesets anyway (invariant 2 unchanged: maniaortho plays still go
  only through `ScoreServerClient`).
- **Plays and accounts**: an upload carrying the game's sign-in (`Authorization: Bearer`) goes to that account;
  `ScoreServerSignIn` hands the token over only when the game is signed in to a custom server at the score server's
  exact scheme and authority, never to a `ppy.sh` host. Plays sent with no sign-in, and those stored before any account
  existed, belong to the first account (the owner), so the old flow keeps working and nothing is orphaned. An upload with a
  sign-in the server does not know (expired, signed out) is refused with 401 and waits in the game's outbox for the
  next send, rather than going to the owner. The score server address setting says whose account plays go to, and
  warns when the game is signed in to another address (`localhost` in one setting and `127.0.0.1` in the other are
  different servers to it: the token is only sent to the exact address that issued it).
- **Web pages**: a request that changes something and carries `Origin` or `Sec-Fetch-Site` (browsers add them, the game
  never does) is refused on every path, so a web page cannot register, sign in or post on Alpha's behalf; the
  `X-ManiaOrtho` rule for `/api` posts stays.
- **Not served**: SignalR hubs (multiplayer, spectating, live metadata; lazer retries them quietly), chat, beatmap
  listing, and beatmap leaderboards (lazer's `LeaderboardManager` refuses online leaderboards for non-legacy rulesets;
  lifting that is another core patch, not asked; backlog M5).
- **Reverse**: leave the address empty (the game is exactly upstream's again); delete the core patches listed in README,
  `ortho-server/OrthoServer/Accounts` and `Lazer`, and `ScoreServerSignIn`.

## 2026-11-04: osu!'s own fix for an intermittent skin editor test, taken early

- **Why**: GitHub's Linux test jobs failed on this branch and on its base the same way:
  `TestSceneSoloResultsScreenV2.TestDisplay` (osu.Game.Tests) hides the skin editor before it has loaded, and
  `SkinEditor.Save` dereferences a bindable that loading sets. osu.Game.Tests failing also stops osu!mania's and
  osu!taiko's suites from running in CI (MSB4181: their projects reference osu.Game.Tests).
- **What**: upstream commit `0e39a5eecf` ("Fix intermittent test", ppy/osu master, 2026-11-03), cherry-picked as is
  with `-x`. It is osu!'s code, not a fork patch: the next upstream merge brings the identical change, so it adds
  nothing to the core diff and is not listed under core patches. Nothing in gameplay, scoring or the ruleset changes.
- **Not taken**: `BeatmapImporterTests.TestImportFailsWithFilenamesDifferingOnlyInCase` also failed once on CI (SharpCompress
  "duplicate key" while the test builds its archive, then the test host crashed); it passed on the base branch's run and
  locally, upstream has no fix, and no code it runs was changed here. Left to upstream.

## 2026-11-05: The score server moves to orthomania, and links with osu! sign-in

- **Why**: Alpha asked to move the server into the new orthomania repository (beside the patcher, in its own folder),
  to let the patcher start it for development, and for "native osu! login ... it only has public login apis/information
  and protects the login users from having their actual passwords leaked".
- **Where**: orthomania `server/` (code, tests, scripts, README). Here, `ortho-server/` keeps only `contract/`, because the
  ruleset compiles it; it is a copy of orthomania's `server/contract`, changed there first. `scripts/ortho/server*` and
  the server step of `check.sh` are gone; the server's tests run in orthomania's CI.
- **Sign-in**: osu!'s public OAuth, authorization code grant, `identify` scope only, as osu!'s API documentation
  describes it. The password is typed only on osu.ppy.sh. The server swaps the one-time code for a token with its own
  client secret (an OAuth application the server's owner registers; kept in the data folder, never in a repository), reads
  `/api/v2/me` (id, username, picture, country), revokes the token at once and keeps nothing of osu!'s. It talks only to
  `https://osu.ppy.sh` and follows no redirect.
- **Not done, on purpose**: sending lazer's own osu! token to the server to learn who is playing. It would be seamless,
  but that token can do anything lazer can on the player's account (chat, score submission), and it would leave the
  player's machine for a third-party server. Neither is acceptable, and invariant 2 already forbids the game sending
  osu!'s sign-in anywhere.
- **The game's side**: a device-style link, like a TV app signing in through a phone. "Link with osu!" asks the server
  for a link, opens its address in the browser (checked to be the score server's own), and polls until the player has
  signed in. The game then holds the server's own token (`ortho_...`), sent with plays to that same server only (scheme,
  host and port, as the fork's sign-in token). It takes precedence over the fork's sign-in to the server. The note under
  the address warns when the link is to another address, or to another osu! account than the one the game is signed in to.
- **A sign-in finishes only in the browser that started it**: each one has its own state, used once and gone after 10
  minutes, tied to the browser by an HttpOnly, SameSite=Lax cookie (checked in a real Chromium, with osu! on another
  site), so a sign-in started elsewhere cannot be finished on someone else's link.
- **Data folder**: `%LOCALAPPDATA%\orthomania\server`, the one the patcher's dev option passes, for the scripts too, so
  starting the server by hand or with the patcher shows the same plays. Plays in the old `../ortho-scores` stay there.
- **Asked**: whether to drop the server's own password accounts and lazer's sign-in imitation (Q27). Kept meanwhile.


## 2026-11-05: osu! sign-in only (Alpha, Q27)

- **What**: Alpha chose "osu! sign-in only" on the Q27 card ("Accounts come only from signing in with osu!. The server
  stores no passwords, and the in-game sign-in to it (fork only) goes."). This reverses the in-game sign-in of
  2026-11-04 ("Signing in to the score server from the game") and Q25's "keep the Server setting".
- **Server (orthomania `server/`)**: registration, password sign-in, its tokens and the whole imitation of osu!'s web API
  for lazer (`OrthoServer/Lazer`, `LazerApiTest`) are gone. A game links only through "Link with osu!" (`/api/link`),
  and a play's bearer token must be a link's. Every request that changes something now needs the game's
  `X-ManiaOrtho` header, on every path (the `/api/v2` exception went with the lazer routes).
- **Data from before**: an account made with a password is removed when the server opens its data, with its password
  hash, e-mail address and sign-in tokens (columns and table dropped, then `VACUUM` with `secure_delete`, so no copy is
  left in the file's free pages; the test reads every file for the hash). Its plays are kept: they belong to nobody, and
  the first osu! account takes them (at start if one exists, else when the first one signs in), as the first registered
  account used to. Alternative: keep those accounts without a way in (plays shown under a name nobody can sign in to,
  and the hash kept); rejected, "the server stores no passwords".
- **Fork**: the Server setting's core patches are reverted to upstream exactly (`OsuConfigManager`,
  `CustomEndpointConfiguration`, `OsuGameBase`, `AllowsPlainHttp` in `EndpointConfiguration`/`APIRequest`/`OAuth`/
  `APIAccess`, `ServerSettings`, `OnlineSection`, `LoginForm`) with their tests. `ScoreServerSignIn` keeps only the
  note for the address setting; plays carry `ScoreServerLinking.TokenFor` and nothing else, and the play recorder no
  longer reads the game's API at all.
- **Someone who used the Server setting**: `game.ini` may still hold `ServerAddress`, `TokenServer` and the private
  server's token; upstream ignores the first two. At the next start lazer tries that token against osu! once,
  osu! refuses it and lazer signs out. The token is dead anyway (the server dropped its tokens table), so nothing of
  value reaches osu!; nobody's osu! sign-in is affected (theirs was forgotten when they switched servers).
