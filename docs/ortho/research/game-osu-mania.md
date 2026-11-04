# osu!mania (lazer)

Last checked: 2026-11-04

Everything below is read from the code in this repository (osu! lazer, MIT), at commit `85131eb`. Path prefixes:
`M/` = `osu.Game.Rulesets.Mania/`, `S/` = `osu.Game/Rulesets/UI/Scrolling/`, `G/` = `osu.Game/`,
`O/` = `osu.Game.Rulesets.ManiaOrtho/`. Lengths are in lazer playfield units: the playfield is scaled so that its height
is 768 units (`M/UI/ManiaPlayfieldAdjustmentContainer.cs:23-32,54-60`, `DrawSizePreservingFillContainer`, target
1024x768, strategy Minimum), which makes a 16:9 screen 1366 units wide. Stable's 640x480 skin units are multiplied by
1.6 (`G/Skinning/LegacySkin.cs:39`, `STABLE_MAGIC_SCALE_FACTOR`). Nothing here was observed on screen; where a claim is
a reading of code that a capture should confirm, it says so.

## Summary

Mania's scroll speed setting (1-40, default 8) maps to a time range of `11485 / speed` ms, which is then scaled by the
skin's hit position and by the playback rate. Visible time (the time a note's leading edge takes from the top of the
column to the judgement line) is that time range, so it depends on the skin: about 2.3% longer than the settings label
with argon or triangles, and 19.4% longer with Alpha's own 4K skin (HitPosition 480). Mania always uses the
**sequential** scroll algorithm (SV changes accumulate distance); the Constant Speed mod switches to **constant**;
**overlapping** is taiko's, not mania's. Hold heads are pinned to the line while held, and the body is masked from the
head's middle. Judgements are one at a time per stage, about 180 units above the line on argon and triangles. F3/F4
change the speed by 1 per press, but in normal play only until 8 s after the first note or during breaks, never
while paused; replays are not limited.

## Camera and track (stage layout)

- **Flat, full height.** Each stage is a column flow, centred horizontally in its cell of a one-row grid
  (`M/UI/ManiaPlayfield.cs:63-83`, `M/UI/Stage.cs:70-73`), filling the playfield's height minus the skin's stage padding
  (`M/UI/Stage.cs:174-184`). Padding: argon 30 top and 30 bottom (`M/Skinning/Argon/ManiaArgonSkinTransformer.cs:163-165`),
  triangles and legacy 0 (no lookup answers, `?? 0`).
- **Columns are masked** to the stage on both axes (`M/UI/ColumnFlow.cs:51`), so notes keep scrolling below the hit
  position into the key area until they expire, and are cut at the stage's bottom edge.
- **Draw order inside a column**: hit object area (hit target, then notes, then explosions), then the key area on top
  (`M/UI/Column.cs:93-106`). So argon's and triangles' key areas cover notes that have passed the line; legacy skins can
  put keys under notes (`KeysUnderNotes`, `M/Skinning/Legacy/LegacyKeyArea.cs:72`). Column backgrounds are proxied
  below everything (`M/UI/Stage.cs:149-150`).
- **Dual stages** are two stages side by side, each centred in half the playfield width (`M/UI/ManiaPlayfield.cs:63-83`).
  They come from native maps with more than 10 keys (12K-18K become two 6K-9K stages,
  `M/Beatmaps/ManiaBeatmapConverter.cs:67-73`, `MAX_STAGE_KEYS = 10` at `M/ManiaRuleset.cs:54`) or the Dual Stages mod
  (`M/Mods/ManiaModDualStages.cs:33`). Stable's `StageSeparation`, `ColumnStart` and `ColumnRight` are parsed but not
  implemented (`G/Skinning/LegacyManiaSkinConfiguration.cs:46-60`): lazer always centres the stage.
- **Portrait** (mobile) scales a single stage up by 1.25 (`M/UI/ManiaPlayfieldAdjustmentContainer.cs:42-53`); not
  relevant on desktop.

## Note approach and speed

### Scroll speed to time range

| Step | Code | Value |
|---|---|---|
| Setting | `M/Configuration/ManiaRulesetConfigManager.cs:24` | `ScrollSpeed` default 8.0, range 1-40, precision 0.1 |
| Speed to time | `M/UI/DrawableManiaRuleset.cs:183` | `ComputeScrollTime(s) = 11485 / s` ms (`MAX_TIME_RANGE`, line 43) |
| Settings label and toast | `G/Localisation/RulesetSettingsStrings.cs:110`, `ManiaRulesetConfigManager.cs:44-53` | "`{11485/s}`ms (speed `{s}`)": the unscaled value |
| Applied every frame | `M/UI/DrawableManiaRuleset.cs:167-176` | `TimeRange = TargetTimeRange × tempo × frequency × (768 − H) / (768 − 124.8)` |
| Clamp | `M/UI/DrawableManiaRuleset.cs:74-75` | `TimeRange` in [1, 11485] game ms |

`H` is the skin's hit position (distance of the judgement line above the bottom of the stage area, in 768 units):
`Stage.HIT_TARGET_POSITION = 110` when the skin gives none (argon and triangles, `M/UI/DrawableManiaRuleset.cs:158-165`,
`M/UI/Stage.cs:38`), else the legacy value; 124.8 is the legacy default (`G/Skinning/LegacyManiaSkinConfiguration.cs:19`,
skin.ini `HitPosition` 402). The code comment says the factor "preserves the scroll speed as the scroll length varies
from changes to the hit position" (`DrawableManiaRuleset.cs:172`): on-screen speed is kept, so visible time grows as
the line goes down. Note that `MIN_TIME_RANGE = 290` (line 38) is documentation only; speed 40 gives 287 ms.

The scroll length is the height of the column's hit object container (`S/ScrollingHitObjectContainer.cs:132`): from the
hit position to the top of the stage. A point at time `t` sits `PositionAt(t, now, TimeRange, scrollLength)` from the
line (`S/ScrollingHitObjectContainer.cs:100-104`), so a note's leading edge enters at the top of the column exactly
`TimeRange` game-ms before its time. Because tempo and frequency multiply `TimeRange`, real-time visible time does not
change with DT/HT/NC. **Visible-time formula** (constant SV, real milliseconds):

```
T_visible = (11485 / speed) × (768 − H) / 643.2        (capped so that T_visible × rate ≤ 11485)
H = 110 (argon, triangles);  H = (480 − clamp(HitPosition, 240, 480)) × 1.6 (legacy, G/Skinning/LegacyManiaSkinDecoder.cs:93-95)
```

| Speed | Label (ms) | Argon / triangles (H 110) | Legacy, HitPosition 402 | Alpha 4K, HitPosition 500→480 (H 0) | Alpha 7K, HitPosition 460 (H 32) |
|---|---|---|---|---|---|
| 8 (default) | 1435 | 1469 | 1436 | 1714 | 1643 |
| 16.7 | 687 | 704 | 688 | 821 | 787 |
| 24 | 478 | 490 | 479 | 571 | 548 |
| 29 (Alpha) | 396 | 405 | 396 | 473 | 453 |
| 40 | 287 | 294 | 287 | 343 | 329 |

(Computed from the formula; Alpha's HitPosition values from `baseline-alpha.md`.) A note's first pixel appears a little
earlier than `T_visible`, because the note piece extends upward from its leading edge (`M/Objects/Drawables/DrawableNote.cs:78-83`):
by note height / on-screen speed, about 100 ms for argon's 42-unit notes at speed 8, 30 ms for Alpha's 49-unit bars at
speed 29.

### Scroll algorithms

- **Control points.** Timing and effect (SV) points are merged into `MultiplierControlPoint`s
  (`S/DrawableScrollingRuleset.cs:106-162`) with `Multiplier = Velocity × SV × BaseBeatLength / BeatLength`
  (`G/Rulesets/Timing/MultiplierControlPoint.cs:24`). Mania sets `RelativeScaleBeatLengths` (`DrawableManiaRuleset.cs:53`),
  so `BaseBeatLength` is the map's most common beat length: **BPM changes scale scroll speed** relative to the main BPM.
  Mania forces `Velocity = 1` and **drops SV for maps not made for the ruleset being played**
  (`DrawableManiaRuleset.cs:88-99`, compares with `Ruleset.RulesetInfo`; this is why maniaortho restores SV,
  `DECISIONS.md` "Restore mania's SV").
- **Sequential** (`S/Algorithms/SequentialScrollAlgorithm.cs`): position is the integral of the multiplier over time.
  Each control point gets a cumulative position (lines 116-136); `PositionAt` is the difference of two such positions
  (41-45, 70-79). Two notes keep their spacing order; an SV section affects everything that has scrolled through it.
  **Mania's default**: `DrawableScrollingRuleset` starts as Sequential (`S/DrawableScrollingRuleset.cs:172`) and mania
  never changes it.
- **Constant** (`ConstantScrollAlgorithm.cs:21-22`): `(t − now) / TimeRange × length`; ignores SV and BPM. Used by
  mania's **Constant Speed** mod (CS, `M/Mods/ManiaModConstantSpeed.cs:27-31`) and the editor with "show speed changes"
  off (`M/Edit/DrawableManiaEditorRuleset.cs:35`).
- **Overlapping** (`OverlappingScrollAlgorithm.cs:36-37`): each object moves at the multiplier of its own start time, so
  objects can overtake each other. Taiko's (`osu.Game.Rulesets.Taiko/UI/DrawableTaikoRuleset.cs:53`); **mania never uses it**.
- There is **no user setting** for the algorithm in mania (`M/ManiaSettingsSubsection.cs:29-69` lists direction,
  speed, timing-based colouring, touch overlay, mobile layout).

### F3 / F4 speed keys

- Bound to `DecreaseScrollSpeed` (F3) and `IncreaseScrollSpeed` (F4) (`G/Input/Bindings/GlobalActionContainer.cs:206-207`).
- Mania changes the **setting by ±1** per press (`DrawableManiaRuleset.cs:139`); the new time range applies the next
  frame with no easing (mania overrides the base class's 200 ms transform, `S/DrawableScrollingRuleset.cs:206-209`).
  The setting change shows osu!'s toast via the tracked setting (`ManiaRulesetConfigManager.cs:44-53`).
- **Blocked** when `AllowScrollSpeedAdjustment` is false (`S/DrawableScrollingRuleset.cs:69-75`): in a
  `SubmittingPlayer` (solo and multiplayer play), whenever it is **not a break and more than 10 s have passed since
  `GameplayStartTime`**, and **always while paused** (`G/Screens/Play/SubmittingPlayer.cs:175-191`).
  `GameplayStartTime` is the first object's time minus 2000 ms (`G/Rulesets/UI/DrawableRuleset.cs:65`), so the window
  ends **8 s after the first note**. Replays and other non-submitting players are never blocked
  (`G/Screens/Play/Player.cs:123`). A blocked press is still consumed (`OnPressed` returns true,
  `S/DrawableScrollingRuleset.cs:215-223`). The comment says these limits "match stable for now" (unverified against stable).

## Judgement line and receptors

| Skin | Hit position source | Line height above screen bottom (16:9) | Scroll length | Hit target |
|---|---|---|---|---|
| Triangles | `Stage.HIT_TARGET_POSITION` 110 | 110/768 = **14.3%** | 658 | 12-unit black band at 60% plus a 2-unit line with a 5-unit glow in the column colour (`M/UI/Components/DefaultHitTarget.cs:21,41-68`) |
| Argon | 110, plus 30 stage padding | 140/768 = **18.2%** | 598 (notes enter 30 units below the top) | 34.4-unit white band at 30%, additive, rounded (`M/Skinning/Argon/ArgonHitTarget.cs:22,32`) |
| Legacy default | HitPosition 402 → 124.8 | **16.25%** | 643.2 | `StageHint` texture inside the stage background (`M/Skinning/Legacy/ManiaLegacySkinTransformer.cs:159-162`) |
| Legacy, any | `(480 − clamp(HitPosition, 240, 480)) × 1.6` | `(480 − HitPosition)/480` | `HitPosition/480 × 768` | as above |
| Alpha's skin | 4K 500→480, 7K 460 | **0%** and **4.2%** | 768 and 736 | none (transparent keys, `baseline-alpha.md`) |

- The hit position is applied as bottom padding on the column's hit object area, the bar line container and the
  judgement container (`M/UI/Components/HitPositionPaddedContainer.cs:33-42`, `M/UI/Stage.cs:106,123`). The hit target
  sits on top of that padding, anchored to its bottom (`M/UI/Components/ColumnHitObjectArea.cs:31,46-54`).
- Below the line is the key area: triangles' and argon's are `HIT_TARGET_POSITION` (110) tall
  (`M/UI/Components/DefaultKeyArea.cs:49`, `M/Skinning/Argon/ArgonKeyArea.cs:57`); argon's press light fades in over
  70 ms and out over 800 ms (`ArgonKeyArea.cs:188,232`).
- A legacy skin's mania config applies only when the skin is legacy and has key textures for drawables
  (`ManiaLegacySkinTransformer.cs:151-152`), but config lookups such as HitPosition always answer: `LegacySkin` creates
  a default configuration for any key count without a `[Mania]` section (`G/Skinning/LegacySkin.cs:146-147`).

## Lane covers / walls (visibility mods)

All three cover mods wrap each column's hit object container in a `PlayfieldCoveringWrapper`
(`M/Mods/ManiaModWithPlayfieldCover.cs:31-47`): a buffer whose alpha is cut by a filled box plus a gradient 25% of the
container tall on its inner edge (`M/UI/PlayfieldCoveringWrapper.cs:76-94`). Coverage changes are damped
(`DampContinuously`, half-life 25 ms, lines 120-141). They are incompatible with Flashlight (`ManiaModWithPlayfieldCover.cs:19`).

| Mod | Covers | Amount |
|---|---|---|
| **Hidden** (HD) | the near end, from the line up ("against scroll", `M/Mods/ManiaModHidden.cs:41`) | filled height `min(400, 160 + 0.5 × combo)` in 768 units measured from the hit position, i.e. ×768/(768 − H) of the container (lines 26-30, 60-65, 94-103); **0 in breaks**. Triangles: 24% of the scroll length at 0 combo, 61% from combo 480, plus the 25% gradient |
| **Fade In** (FI) | the far end ("along scroll", `M/Mods/ManiaModFadeIn.cs:27`) | same numbers as Hidden (subclass) |
| **Cover** (CO) | either end (setting, default **along scroll** = far end, `M/Mods/ManiaModCover.cs:23,44-45`; `CoverExpandDirection` order at `PlayfieldCoveringWrapper.cs:157-169`) | 0.2-0.8 of the container, default 0.5, step 0.1 (`ManiaModCover.cs:35-42`), plus the gradient; no hit-position correction |
| **Flashlight** (FL) | everything outside a full-width band centred on the playfield's vertical middle (`M/Mods/ManiaModFlashlight.cs:47-52`) | size 50 × size multiplier (0.5-3, default 1; lines 18-27), ×2.5 in breaks, combo-based shrinking off by default, 800 ms transitions (`G/Rulesets/Mods/ModFlashlight.cs:56,172-195`) |

- Mania has **no SUDDEN+/LIFT-style adjustable cover outside mods**; Cover (along scroll) is the closest equivalent, and
  it is a ranked mod (`ManiaModCover.cs:31`).
- Flashlight's band is read here as a half height of 50 units with a smoothstep soft edge over 10% of it (from our port,
  `O/Mods/ManiaOrthoModFlashlight.cs:53,156`; the shader itself is in the osu.Game.Resources package, not in this repo,
  so unverified here). On a 16:9 triangles stage that lights notes from about 49% to 34% of their visible time before
  the hit (computed; argon 49% to 32%; Alpha's 4K skin 57% to 44%): **mania's FL hides the last third of the approach**.

## Notes and holds (shapes, rendering)

### Notes

- Anchored at their bottom (down scroll), so the **leading edge** is at the note's time and the piece extends upward
  (`M/Objects/Drawables/DrawableManiaHitObject.cs:68-71`, `DrawableNote.cs:78-83`).
- Heights: triangles 12 units (`M/Skinning/Default/DefaultNotePiece.cs:22`, 1.8% of its scroll length); argon 42 units
  (`M/Skinning/Argon/ArgonNotePiece.cs:21`, 7.0% of 598), coloured part 82%, corner radius 3.4; legacy: texture height ×
  (`WidthForNoteHeightScale`, or the narrowest column) / texture width (`M/Skinning/Legacy/LegacyNotePiece.cs:61-65`,
  `G/Skinning/LegacySkin.cs:155-159`). Alpha's 296x152 bars: about 49 units at 4K (6.4% of 768) and 37 at 7K (5.0% of
  736), assuming the skin does not set `WidthForNoteHeightScale`.
- **Hit notes vanish at once; missed notes keep scrolling past the line while fading out over 150 ms**
  (`DrawableManiaHitObject.cs:73-85`). A note auto-misses once it is later than the Meh window (`DrawableNote.cs:89-95`,
  `G/Rulesets/Scoring/HitWindows.cs:41-50,109`).
- Timing-based colouring (off by default) tints a note by its beat snap (`DrawableNote.cs:126-133`).

### Holds

- Structure (`M/Objects/Drawables/DrawableHoldNote.cs:86-125`): a sizing container holding the head and a masked
  container; the body piece and the tail are proxied into the masked container.
- **Body extent**: from half-way under the head to half-way under the tail (`DrawableHoldNote.cs:250-253`).
- **While held** (`DrawableHoldNote.cs:255-271`): once `Time ≥ StartTime`, if the head was hit and the hold was not
  dropped, the sizing container's height becomes `1 − yOffset / DrawHeight`. Its bottom therefore stays on the hit
  position: **the head is pinned to the line**, and the body and tail are masked from the head's middle
  (mask padding `Head.Height / 2`, lines 242-248). Early hits wait for the start time ("As per stable", line 261).
  The formula is not clamped, so the head stays on the line **even after the tail's time** while the key is still held,
  until the hold is judged (read from code; confirm with a capture).
- **Dropped or head missed**: the sizing height stops updating, so the head and the remaining body scroll on past the
  line together (frozen length). The head never fades on its own (`DrawableHoldNoteHead.cs:56-65`); the whole hold
  fades as one object.
- **Judgement**: head as a note; tail with windows ×1.5 (`TailNote.RELEASE_WINDOW_LENIENCE`, `M/Objects/TailNote.cs:19,23`,
  `DrawableHoldNoteTail.cs:58-60`), capped at Meh if the head was missed or the hold broken (lines 62-71). Keeping the
  key down past the tail's late window (1.5 × Meh) misses the tail (`DrawableNote.cs:89-95`).
- **Body looks**: triangles: additive, column colour at 70% plus a 50% foreground that pulses lighter every 100 ms while
  held (`M/Skinning/Default/DefaultBodyPiece.cs:32,65,124-138`); argon: column colour darkened 0.6 plus a "hitting"
  layer, same width as the head via masking (`ArgonHoldBodyPiece.cs:31-57`); legacy: `NoteImage{n}L` stretched or
  repeated (`NoteBodyStyle`; default RepeatBottom for skin version ≥ 2.5, Stretch below:
  `G/Skinning/LegacySkin.cs:205-213`, `LegacyBodyPiece.cs:77-79,190-212`) and a `lightingL` glow at the hit position
  while held (80 ms in, 120 ms out, lines 116-138).
- **Broken holds dim to dark grey over 60 ms only on legacy skins** (`LegacyBodyPiece.cs:171-178`,
  `LegacyHoldNoteHeadPiece.cs:43`, `LegacyHoldNoteTailPiece.cs:44`); argon and triangles keep their colours.
- Argon's tail is a full note height (`ArgonHoldNoteTailPiece.cs:34`); triangles' and legacy's tails use the note
  piece / `NoteImage{n}T`.

### Bar lines

- One per bar: beat length × time-signature numerator, from each timing point, "major" when the bar index is a multiple
  of the numerator (`G/Rulesets/Objects/BarLineGenerator.cs:51,85-89`). Added to every stage (`M/UI/ManiaPlayfield.cs:97`).
- Drawn in the stage's hit-position-padded container across the column flow's width (`M/UI/Stage.cs:97-114,226`), so
  they cross the line at their time. Triangles/argon (default): 1.2 units at 20% (minor), 1.7 units at 50% plus 10-unit
  additive side extensions (major) (`M/Skinning/Default/DefaultBarLine.cs:41,76-82`). Legacy: `BarlineHeight` (default
  1.2, `LegacyManiaSkinConfiguration.cs:39`) in `ColourBarline` (`LegacyBarLine.cs:19-23`); Alpha's skin uses 0 (hidden).
- They fade out over 150 ms from their time (`M/Objects/Drawables/DrawableBarLine.cs:54`).

## Key count and lane layout

| Skin | Column width | Spacing | 4K stage | 7K stage | 10K stage |
|---|---|---|---|---|---|
| Triangles | 80, odd-count centre column 70 (`M/UI/Column.cs:32-33`, `ColumnFlow.cs:141-146`) | 1 each side (`Stage.cs:36`, `ColumnFlow.cs:127-135`) | 328 = **24.0%** of 16:9 width | 564 = **41.3%** | 820 = **60.0%** |
| Argon | 60, centre column 120 (`ManiaArgonSkinTransformer.cs:167-172`) | 1 each side (lines 159-161) | 248 = **18.2%** | 494 = **36.2%** | 620 = **45.4%** |
| Legacy default | 48 (30 × 1.6, `LegacyManiaSkinConfiguration.cs:17,74`) | `ColumnSpacing`, default 0 (`LegacySkin.cs:271-283`) | 192 = 14.1% | 336 = 24.6% | 480 = 35.1% |
| Alpha's skin | 4K 96, 7K 72 | 0 | 384 = **28.1%** | 504 = **36.9%** | |

- The centre column of an odd key count is "special" (`M/Beatmaps/StageDefinition.cs:32`). The classic skin's own
  skin.ini (in the resources package) was not checked, so its per-key widths may differ from the code default (unverified).
- Column colours: argon has a fixed palette per key count (`ManiaArgonSkinTransformer.cs:184-380`); triangles alternate
  two dark colours counted from both edges, centre column distinct (`ManiaTrianglesSkinTransformer.cs:33-44`); classic
  falls back to black (`ManiaClassicSkinTransformer.cs:30-31`).
- Default keys are laid out from both hands inward/outward around Space (`M/SingleStageVariantGenerator.cs:19-36`);
  input is one action per column, notes in a column must be hit in order (`M/UI/OrderedHitPolicy.cs:34`).

## Feedback (judgement display, hit effects, HUD)

### Hit windows (`M/Scoring/ManiaHitWindows.cs`)

`window = floor(DifficultyRange(OD) × rate / difficulty) + 0.5` ms, ± around the note time (lines 154-159, 58). OD 0 /
5 / 10 anchors (lines 12-17), linear in between (`G/Beatmaps/IBeatmapDifficultyInfo.cs:57-72`):

| OD | Perfect (MAX) | Great (300) | Good (200) | Ok (100) | Meh (50) | Miss |
|---|---|---|---|---|---|---|
| 0 | 22.5 | 64.5 | 97.5 | 127.5 | 151.5 | 188.5 |
| 5 | 19.5 | 49.5 | 82.5 | 112.5 | 136.5 | 173.5 |
| 8 | 16.5 | 40.5 | 73.5 | 103.5 | 127.5 | 164.5 |
| 10 | 13.5 | 34.5 | 67.5 | 97.5 | 121.5 | 158.5 |

- HR divides windows by 1.4, EZ multiplies by 1.4 (`M/Mods/ManiaModHardRock.cs:15`, `ManiaModEasy.cs:16`); rate mods
  multiply by the rate so windows are constant in real time (`M/Mods/IManiaRateAdjustmentMod.cs:27`). Classic mod
  (without ScoreV2) uses stable's integer windows, different for converts (`ManiaHitWindows.cs:129-151`).
- A press between the Meh and Miss windows **early** is a Miss; late, a note is missed as soon as it passes Meh
  (`G/Rulesets/Scoring/HitWindows.cs:82-93,109`).

### Judgement display

- **One judgement per stage at a time**: a new result clears the old (`M/UI/Stage.cs:213-220`). Shown for notes, hold
  heads and tails; not for the hold itself or its body (`DrawableHoldNote.cs:32`, `DrawableHoldNoteBody.cs:12`). It is
  centred over the stage, in the hit-position-padded judgement container (`Stage.cs:123-130`).
- **Triangles**: text size 25, **180 units above the hit position** (`M/UI/DefaultManiaJudgementPiece.cs:16,32-43`);
  hits pop from 0.8 to 1 (250 ms elastic) then shrink to 0.75 and fade over 200 ms; misses drop 100 units, tilt 40° and
  fade over 800 ms (lines 45-77).
- **Argon**: 28-pt additive text with letter spacing 10, also 180 above (`ArgonJudgementPiece.cs:24,62-70`), growing to
  1.4× over 1800 ms while fading over 800 ms, plus small rings bursting outwards for hits (lines 79-104, 108-175).
  **Argon Pro shows nothing for Great and Perfect** (`ManiaArgonSkinTransformer.cs:94-97`).
- **Legacy**: the `mania-hit*` texture at `ScorePosition` (default 300 × 1.6 from the top,
  `LegacyManiaSkinConfiguration.cs:38`, `LegacyManiaJudgementPiece.cs:50-67`); in 20 ms, held 160 ms, out 40 ms, with a
  stable-matching scale sequence (lines 69-99). A transparent texture (Alpha's 300g/300) shows nothing.
- **Hit explosions** appear for every successful result that displays a judgement (`M/UI/Column.cs:176-185`), from a
  pool of 5 per column (line 95).

### HUD defaults

- **Key counter**: in osu!'s global HUD layout at the bottom right (`G/Skinning/ArgonSkin.cs:238-243`,
  `G/Skinning/TrianglesSkin.cs:173-178`); visible only when the "key overlay" setting is on (default off,
  `G/Configuration/OsuConfigManager.cs:158`) or a replay/autoplay is loaded (`G/Screens/Play/HUD/KeyCounterDisplay.cs:53-56,72`).
- **Argon (mania layout)**: combo counter top-centre at Y = 200 without label (`ManiaArgonSkinTransformer.cs:52-58`),
  which is about 26% of the screen height from the top at UI scale 1 (HUD lives in osu!'s 1024x768 scaling container,
  `G/OsuGame.cs:949`; inferred); two bar hit error meters, centre-left and centre-right (lines 62-74, 87-88); leaderboard
  and spectator list on the left.
- **Triangles (ruleset layout)**: same two side hit error meters (`G/Skinning/TrianglesSkin.cs:109-131`).
- **Legacy**: combo at `ComboPosition` (default 111 × 1.6 from the top), one legacy hit error bar at bottom centre
  (`ManiaLegacySkinTransformer.cs:99-142`), shown only if the skin provides legacy resources (line 94).

## Relevant from Alpha's baseline (`baseline-alpha.md`)

- **Speed 29 is not 396 ms in lazer with Alpha's skin**: it is about **473 ms at 4K and 453 ms at 7K** (HitPosition
  480 and 460), and 405 ms with argon or triangles. The baseline note's "≈ 396 ms" is the settings label, which leaves
  out the hit-position factor. Whether stable maps speed 29 the same way is **unverified**: lazer's comment implies
  stable keeps on-screen speed, but stable's source (osu-stable-reference) could not be fetched from this container.
- Alpha's line at the very bottom (H = 0) is the extreme of mania's range: the clamp at 480 is exactly that
  (`LegacyManiaSkinDecoder.cs:94`).
- Alpha's bars are 6.4% (4K) and 5.0% (7K) of the scroll length thick: about three times triangles' notes and close to
  argon's 7%.
- Alpha's KeyOverlay habit maps to osu!'s key overlay setting; the hit error meter for a legacy skin is the bottom-centre
  legacy bar.

## What this means for us

Checked against `SPEC.md` and `DECISIONS.md` (as of 2026-11-04).

**Matches (no action):**
- SPEC 3 visible time: native notes and the warp both use mania's own `TimeRange` and scroll algorithm
  (`O/UI/HighwayPlayfieldAdjustmentContainer.cs:348`, `O/Highway/HighwayScroll.cs:44-56`), so visible time, rate
  handling, SV, BPM scaling and the Constant Speed mod behave exactly as in mania. Restoring SV for mania maps is
  required by `DrawableManiaRuleset.cs:88-99` (as decided).
- SPEC 6 holds: our body starts half a note above the head (`O/UI/HighwayNoteLayer.cs:225`), as mania's mask does; the
  head stops at the line while held and only after the start time (lines 215-218), as mania's does.
- SPEC 7 judgements: one per stage at mania's height above the line, using the skin's own drawables, so Argon Pro's and
  Alpha's "show nothing for perfect" carry over.
- F3/F4 limits: inherited from mania, unchanged.

**Mismatches and proposals:**

1. **The speed label is not the visible time** (SPEC 3). Our settings slider copies mania's label
   (`O/ManiaOrthoSettingsSubsection.cs:40`), so it shows `11485 / speed` while the highway's real visible time is
   `× (768 − H) / 643.2`: +2.3% on argon/triangles, +19.4% on Alpha's 4K skin. Under tilted cameras the skin's
   HitPosition no longer moves our judgement line, but it still changes the speed. Proposal: keep using mania's
   `TimeRange` (it is what parity of feel means: the same skin and speed give the same visible time as in mania), but
   show the real visible time where we can (the in-game toast or a line under the highway settings, e.g. "visible time
   405 ms with the current skin"), and record the coupling in `DECISIONS.md`. For playtests that mean "Alpha's speed",
   say which: speed 29 with argon is 405 ms; 396 ms needs speed 29.7 with argon. `baseline-alpha.md` and
   `game-chunithm.md` P2 ("Alpha's 396 ms") should be corrected by whoever owns them.
2. **SPEC 3 mentions "settings for constant / overlapping / sequential"; mania has none.** Mania is always sequential,
   constant only through the CS mod, and never overlapping. Our code already follows mania; the SPEC wording should
   say "same algorithm as mania (sequential; constant with the Constant Speed mod)". No test pins CS on the highway:
   add one (notes at equal spacing through an SV section with CS on).
3. **Held past the tail** (SPEC 6). Mania keeps the head on the line until the hold is judged (up to 1.5 × Meh after the
   tail's time, e.g. 191 ms at OD 8) because its sizing formula is not clamped; ours lets the head follow the tail
   below the line (`O/UI/HighwayNoteLayer.cs:216-218`), so it disappears at the tail's time. Proposal: clamp the head
   at depth 0 while `IsHolding` is true, whatever the tail depth, and draw no body; confirm mania's behaviour with a
   capture first (it is read from code).
4. **Missed notes stop at the line** (recorded as a Chunithm choice). In mania a missed note visibly slides past the
   line for 150 ms while fading, which is the "late" cue mania players read. Proposal: under tilted cameras let missed
   notes (alpha < 1 after a miss) continue into the flat below-line area for their 150 ms fade instead of being clipped;
   keep hit notes clipped. Tune in playtest.
5. **The wall hotkeys bypass mania's critical-settings gate when "keep speed when moving the wall" is on** (SPEC 4).
   With the default (constant visible time) the wall does not change reaction time, so moving it any time is fine. With
   the toggle on, Shift+F3/F4 change visible time mid-song and while paused, which mania forbids for F3/F4. The ruleset
   is never submittable, so this is low risk; proposal: gate the hotkeys with `AllowScrollSpeedAdjustment`'s rule only
   when the toggle is on, or record why not. Also, `DECISIONS.md`'s hotkey entry says F3/F4 work in "the first 10 seconds
   of a play"; precisely it is until 8 s after the first note (10 s after first note − 2 s).
6. **Argon's combo counter sits over the slow part of the track** (SPEC 7). It is centred 200 HUD units from the top
   (about 26% of the screen height). In mania that covers notes at about 72% of their visible time remaining; on the
   Chunithm highway, notes spend the first half of their visible time between 16% and 31% of the screen height
   (`game-chunithm.md`, "Depth mapping", arcade far edge), so with the far edge stretched to the top the combo plausibly
   covers them for a larger share of their approach than in mania (estimate, not measured). Check in compare sheets; if it occludes, a maniaortho HUD default could
   place combo near the line (Chunithm puts it on the field) without touching mania's layout.
7. **Note thickness range** (SPEC 6). Alpha's bars are 6.4% of the scroll length at 4K; our "Note thickness" maximum is
   6% (`O/Configuration/ManiaOrthoRulesetConfigManager.cs:92`). Under perspective the comparison is not one-to-one, but
   under the Parallel preset it is. Proposal: raise the maximum to 8% so Alpha's look can be matched there.
8. **Judgement line height vs mania** (SPEC 2): mania puts the line at 14-18% of the screen height (argon 18.2%,
   triangles 14.3%, legacy default 16.25%) and Alpha at 0-4%. The Chunithm (7.4%) and Voltex (5.9%) presets sit between
   Alpha's habit and mania's defaults; SPEC 2's 12% default is close to triangles. No change; the 0-40% range covers
   all of them.
9. **Track width vs mania** (SPEC 5): the Chunithm preset's 61% of the screen width at the line is 2.5× triangles' 4K
   stage (24%), 3.4× argon's (18.2%) and 2.2× Alpha's (28%). At 7K mania stages are 36-41%. That is
   Alpha's Q10 choice; the readability check at 4K is about lane width at the far edge (61% × 0.24 / 4 = 3.7% of the
   screen per lane), not at the line.
10. **Flashlight**: since mania's band lights notes only between about 49% and 34% of their visible time before the
    hit, our projected band correctly sits far up the track; the compare-sheet check is that notes vanish in the last
    third of their approach on both sides.

**Suggested defaults from this note:** keep mania's speed default (8) and range (1-40); keep scroll algorithm = mania's;
playtest speeds 29 (Alpha), 16.7 and 12.6 (Chunithm-like) with argon, noting the visible times 405, 704 and 933 ms.

## Unverified / open

- Stable's mapping of ManiaSpeed to visible time with a custom HitPosition (lazer's comment implies on-screen speed is
  kept; stable's source was not reachable).
- The classic skin's skin.ini mania values (column widths, hit position) in the resources package.
- The Flashlight shader's exact band extent (taken from our port's reading).
- Behaviours read from code but not captured: head pinned on the line while held past the tail, frozen length after a
  drop, missed notes sliding under the key area.

## Sources

- osu! lazer source in this repository (MIT, https://github.com/ppy/osu): every file and line cited above, at commit
  `85131eb`.
- `docs/ortho/research/baseline-alpha.md`: Alpha's speed, skin HitPosition, column widths and note textures.
- `docs/ortho/research/game-chunithm.md` ("Depth mapping implied by these numbers", P2): Chunithm's depth-to-time
  distribution and speed proposals that item 1 and 6 refer to.
- `docs/ortho/DECISIONS.md` and `docs/ortho/SPEC.md` (2026-11-04): our current choices.
