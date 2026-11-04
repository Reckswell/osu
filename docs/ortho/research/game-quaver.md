# Quaver

Last checked: 2026-11-04

## Summary

Quaver is a flat keyboard VSRG (1K-10K) with **no camera, tilt or perspective option** and **no visibility mods** (no
Hidden, Fade In or Flashlight). It matters to us for speed, layout, covers, holds and feedback on a flat stage, which is
our Classic preset and the baseline every tilted preset is compared with.

- **Speed is an on-screen speed.** The number (5.0-100.0, default 15.0, one value per key count) is a fixed number of
  pixels per millisecond in a 1080-tall virtual screen. Its visible time therefore depends on where the skin puts the
  line: T = **13528 / S ms** for the default 4K skin, 13392 / S at 7K. Quaver 15.0 is 902 ms; Alpha's 405 ms is Quaver
  **33.4**. Rate mods keep the real-time visible time, as in mania.
- **Layout of the default "Bar" skin**: judgement line **12.0%** of the screen height above the bottom at 4K (12.9% at
  7K), with an opaque key panel below it drawn over the notes. The stage is 24.9% of a 16:9 screen wide at 4K and 29.7%
  at 7K. Notes are solid full-lane bars, 4.3% of the screen tall at 4K.
- **Lane covers** are options-menu settings. The top cover hides the top 0-75% **of the screen**, with a soft lower edge
  16% of the screen tall in the default image. The bottom cover does the same from the bottom. Both are built when the
  play loads and keep speed.
- **Holds behave like mania's and ours.** The body runs from the head's middle to the tail's middle, and the head is
  pinned to the line while held, also past the tail's time. Missed or dropped holds are tinted grey and scroll on. In
  the default skin bodies are opaque, full width and the same colour as the head.
- **osu! maps scroll as in lazer mania.** BPM changes become SV relative to the most common BPM, and positions are SV
  integrals. The No SV mod is mania's Constant Speed.

## Scope, evidence and licences

Everything below was read from code at the commits in **Sources**, or measured from the default skin files. Nothing
was observed in a running Quaver. Where a claim is a reading of code that a capture should confirm, it says so.

| Repository | Licence | Used for |
|---|---|---|
| Quaver/Quaver ([Q1]) | **MPL-2.0** (weak copyleft) | gameplay, skinning and config code. Read for ideas and numbers only. |
| Quaver/Quaver.API ([Q2]) | **MPL-2.0** | map model, SV normalisation, osu! import, mods. Read only. |
| Quaver/Wobble ([Q3]) | **MIT** | the engine: sprite drawing, virtual resolution |
| Quaver.Resources, a folder in [Q1] ([Q4]) | **CC BY-NC 4.0** (art) | default skin `skin.ini` values; image sizes and colours measured with ImageMagick. Nothing copied or committed. |

No code from any of them is in this repository.

Units: Quaver lays out gameplay in a virtual screen 1080 px tall. At 16:9 or wider it is 1920 wide or more
(`QuaverGame.cs:417,1265-1281`; Wobble `WindowManager.cs:26-37`). Skin lengths are written in 1366-wide units and
multiplied by `SkinScalingFactor` = 1920/1366 = 1.4056 at load (`QuaverGame.cs:142`, `SkinKeys.cs:550-563`, every
`[FixedScale]` property). All pixel numbers below are in that 1080-tall space. Fractions of H are of the screen height.

## Camera and track

- **Flat, full height. No perspective in any form.** The playfield is a column of sprites in screen space
  (`GameplayPlayfieldKeys.cs:212-249`).
  - There is no camera, tilt, skew, FOV or 3D setting in `ConfigManager.cs` or the gameplay code. A search for
    "perspective" or "tilt" in `Quaver.Shared` and Wobble finds only a FontAwesome icon name.
  - The mod list ([Q2] `ModIdentifier.cs:19-65`) has rate mods, Strict/Chill windows, No Fail, No LN, Inverse, Full LN,
    Mirror, Randomize, No SV, No Mines, Autoplay, Coop and No Miss. None is visual.
  - **"Paused"** (`ModIdentifier.cs:40`) is a flag recorded on a score when the player paused, not a visual mod.
- **Visual-only settings.** All are flat and none changes judgement:
  - Playfield Scale 25-100% scales the whole playfield about its centre and masks outside it
    (`ConfigManager.cs:1152`, `GameplayPlayfieldKeys.cs:204-205,232-246`).
  - Note & Receptor Size Scale 25-100% narrows notes and receptors inside their lanes (`ConfigManager.cs:1284`,
    `GameplayHitObjectKeys.cs:169-170,226-230`).
  - Visual Offset −500..500 ms shifts drawing only (`ConfigManager.cs:1307`, `HitObjectManagerKeys.cs:791`).
  - Lane covers, Long Note Shrink, timing lines, and the scroll direction per key count.
- **Stage position**: centred, moved sideways by the skin's `ColumnAlignment` in pixels
  (`GameplayPlayfieldKeys.cs:215-230`). Default 0 in all three default skins.

## Note approach and speed

### Scroll speed to on-screen speed

| Step | Code | Value |
|---|---|---|
| Setting | `ConfigManager.cs:1353` | `ScrollSpeed{n}K` for each key count 1-10: integer 50-1000, default 150, shown ÷10 (5.0-100.0, default 15.0; `OptionsMenu.cs:500-505`) |
| Per-map override | `TimingGroupControllerKeys.cs:42`, `GlobalInputHandler.cs:161-185` | `MapManager.CustomScrollSpeed`, set with the "local" keys (Shift by default) |
| Rate normalisation | `TimingGroupControllerKeys.cs:27-28,38-51` | only if the map has "significant SVs" (any multiplier outside 0.99-1.01 after normalisation, `HitObjectManagerKeys.cs:297`): speed × (1 + (rate − 1) × p), clamped to 50-1000. p is "Normalise Scroll Velocity By Rate Percentage", 0-100, **default 0** (`ConfigManager.cs:1161`). |
| Pixels per audio ms | `TimingGroupControllerKeys.cs:56-72` | (stored / 10) / (20 × rate) × 1.4056 × `BaseToVirtualRatio` (1 at 16:9 or wider) |
| SV and speed factor | `ScrollGroupControllerKeys.cs:29,107-142` | position = ∫ SV dt × 100 (`TrackRounding`, `HitObjectManagerKeys.cs:41`); screen speed × the map's "scroll speed factor" (a Quaver-only map feature, lerped between points, lines 151-162) |
| Note position | `ScrollNoteController.cs:119-124` | y = hit position − (note position − current position) × speed / 100 (downscroll) |

Because the pixels per audio ms are divided by the rate, **on-screen speed in real time does not change with rate
mods** unless p > 0 on an SV map. This matches mania, which multiplies `TimeRange` by the rate (`game-osu-mania.md`).

**Visible time.** In real ms, a speed S (display value) moves notes at S × 1.4056 / 20 = **0.0703·S px/ms**, that is
0.0651·S screen heights per second. A note's leading (bottom) edge reaches the line at its hit time:

- the hit position is the receptor's top + `HitPosOffsetY` − note height, and it is the note's *top* edge
  (`GameplayPlayfieldKeys.cs:315-323`);
- the note's leading edge enters at the screen top `line_y / v` before the hit.

So T = line_y / (0.0703·S), with line_y measured from the top of the screen. Moving the line keeps on-screen speed and
changes visible time, as mania's `(768 − H) / 643.2` factor does.

Default Bar skin (computed from the layout below; mania columns = the mania speed with the same visible time, from
`game-osu-mania.md`'s formula):

| Quaver speed | 10 | **15 (default)** | 20 | 25 | 30 | **33.4** | 40 |
|---|---|---|---|---|---|---|---|
| 4K visible time (ms), T = 13528 / S | 1353 | **902** | 676 | 541 | 451 | **405** | 338 |
| 7K visible time (ms), T = 13392 / S | 1339 | 893 | 670 | 536 | 446 | 401 | 335 |
| Same time in mania, argon/triangles (11749 / T) | 8.7 | 13.0 | 17.4 | 21.7 | 26.1 | **29.0** | 34.7 |
| Same time in mania, legacy default skin (11485 / T) | 8.5 | 12.7 | 17.0 | 21.2 | 25.5 | 28.4 | 34.0 |

- Rule of thumb: **mania speed on argon ≈ 0.87 × Quaver speed** (4K Bar).
- Alpha's speed 29 on argon (405 ms) is Quaver 33.4.
- Chunithm-like 704 ms is Quaver 19.2. Our slow playtest speed (839 ms) is Quaver 16.1.
- Typical speeds of Quaver players are not sourced here (unverified).
- At aspect ratios narrower than 16:9 the virtual screen grows taller. Speed, lane size and hit offset scale by the
  same `BaseToVirtualRatio`, so visible time should stay about the same; `ReceptorPosOffsetY` is not scaled (read
  from code, not observed).

### Changing speed during play

- F3/F4 change the speed by ±1.0 and the small variants by ±0.1; with the "local" modifier they change a per-map
  override (`ConfigManager.cs:1201-1202`, `GlobalInputHandler.cs:153-196`).
- In gameplay they work only while song time ≤ 5 s, when the intro can be skipped (breaks), in replays and in
  tournament view (`GameplayScreen.cs:724-733`). Mania's window ends 8 s after the first note instead.
- The toast shows the speed number, not a time. Already in `topic-speed.md`.

### SV and BPM (only what decides how osu! maps move)

- **osu! import** ([Q2] `OsuBeatmap.cs:509-536`): inherited timing points become SVs of −100 / beat length, clamped to
  0.01-10. BPM points stay timing points. The map is stored "denormalised" (`BPMDoesNotAffectScrollVelocity` false,
  `Qua.cs:120-128`).
- **At play** (`HitObjectManagerKeys.cs:284`, [Q2] `Qua.cs:1238-1300`): `WithNormalizedSVs()` folds BPM into SV as
  multiplier × current BPM / common BPM. The common BPM is the BPM held longest up to the last note
  (`Qua.cs:623-650`).
  - That is lazer mania's `Multiplier = SV × BaseBeatLength / BeatLength`, with `BaseBeatLength` the most common beat
    length (`osu.Game/Rulesets/UI/Scrolling/DrawableScrollingRuleset.cs:80,111-113`).
  - So **an osu!mania map's notes move the same in Quaver as in lazer mania** (sequential: positions are integrals of
    the multiplier). Edge cases (timing points at the same time, infinite BPM, lazer's own clamps) were not compared
    (unverified).
- **No SV mod** (NSV, `ScrollGroupControllerKeys.cs:122-124`): position = time, ignoring SV and BPM. This is mania's
  Constant Speed.
- Quaver-native charts can have negative SV, several timing groups with their own SVs, and scroll speed factors. osu!
  maps never have them.
- **Correction for `topic-speed.md`** (its table row for Quaver and its S5): "Quaver ignores BPM" is true only for
  Quaver-native charts saved normalised. For osu! maps, BPM changes scale speed relative to the common BPM, exactly as
  in mania.

## Judgement line and receptors

Default skin: **Bar** (`ConfigManager.cs:1164`). Its `skin.ini` [Q4] sets 4K `ColumnSize` 85 and 7K 58, and
`ReceptorPosOffsetY` and `HitPosOffsetY` 0 at both key counts. The receptor image is 256x277 at 4K and 256x436 at 7K.

| | 4K | 7K | How |
|---|---|---|---|
| Lane width | 119.5 px | 81.5 px | `ColumnSize` × 1.4056 (`GameplayPlayfieldKeys.cs:118-131`) |
| Receptor height | 129.3 px | 138.8 px | lane × image height / width (`GameplayPlayfieldKeys.cs:287`, `GameplayPlayfieldKeysStage.cs:424`) |
| **Judgement line, above the screen bottom** | **12.0%** (y = 0.880H) | **12.9%** (0.871H) | receptor top = H − `ReceptorPosOffsetY` − receptor height, + `HitPosOffsetY` (`GameplayPlayfieldKeys.cs:315-332`) |

- **What the receptor looks like** (default images, viewed):
  - a dark grey panel the full lane wide, filling the bottom 12-13% of the screen;
  - a **cyan line along its top edge**, 10/256 of the lane width thick (4.7 px at 4K, 3.2 px at 7K, measured);
  - a grey key square in the middle that turns white with a glow while pressed.
- **The line is the receptor's top edge.**
- **Press animation**: a texture swap between `receptor-up` and `receptor-down`, instant, with no scale or fade
  (`GameplayPlayfieldKeysStage.cs:749-753`, `InputManagerKeys.cs:134`).
- **Draw order**: Bar sets `ReceptorsOverHitObjects = True`, so the opaque panel is drawn **over** notes
  (`GameplayPlayfieldKeysStage.cs:184-192`). Notes disappear under it as they pass the line. A skin with the option
  off shows notes over the receptors.
- **Other default skins** put the line elsewhere. Arrow and Circle set `HitPosOffsetY` 90 at 4K and 58 at 7K, with
  `ReceptorPosOffsetY` 5 [Q4]. Their notes hit inside the receptor art. Their line heights were not computed
  (unverified).
- Hit position is a **skin** value in Quaver, as in mania. No gameplay setting moves it.

## Lane covers / walls

Settings under Options → Gameplay → Lane Cover (`OptionsMenu.cs:242-251`; defaults at `ConfigManager.cs:1244-1249`):

| Setting | Range | Default |
|---|---|---|
| Enable Top Lane Cover / Top Lane Cover Height | 0-75 (%), 1 steps | off / 25 |
| Enable Bottom Lane Cover / Bottom Lane Cover Height | 0-75 (%), 1 steps | off / 25 |
| Display UI Elements Over Lane Covers | on/off | on (combo, judgement, hit error drawn over the cover) |
| Display Receptors Over Lane Covers | on/off | off |

(The code's doc comments say "levels 0-50"; the actual range is 0-75.)

**Geometry** (`GameplayPlayfieldKeysStage.cs:507-519,758-793`):

- **Top cover.** A sprite the playfield's width and the full screen height, bottom-aligned at y = −H + p·H, so its
  lower edge sits at **p of the screen height from the screen top**. It is measured on the screen, not along the
  lane from the line.
- **Bottom cover.** The mirror image: its upper edge sits at (1 − p)·H.
- **Soft edges.** The default `cover-top.png` (300x700, black) is opaque down to row about 590 and fades to transparent
  at row 700 (measured: alpha 0.96 at row 600, 0.67 at 650, 0.16 at 690). Stretched to the screen height, that is a
  soft lower edge about **0.157H** tall. `cover-bottom.png` is the same flipped.
- **4K Bar, top cover p**, as fractions of the visible time (line at 0.880H):

| p | 25% (default) | 50% | 75% |
|---|---|---|---|
| Fully hidden | 10.5% | 38.9% | 67.3% |
| Fading in | 10.5-28.4% | 38.9-56.8% | 67.3-85.2% |
| Fully visible | last 71.6% | last 43.2% | last 14.8% |

- **Bottom cover at the default 25%** begins fading at 0.75H and is about 92% opaque at the 4K line (0.880H). Unless
  receptors are drawn over it, it hides the last ~15% of the approach and the receptors. It is a static Hidden.
- **Adjustment**: options only. Both covers are built once in the stage's constructor. Nothing listens for changes and
  there is no hotkey (only `OptionsMenu.cs` and the V2 `OptionsList.cs:373-375` reference the heights).
- **What moving it keeps**: on-screen speed. Visible time shrinks with the cover. Neither is a mod, and neither changes
  judgement or scoring.

`topic-covers.md`'s Quaver row is right on range, defaults and semantics. Add that the percentage is of the screen
height from the screen edge, and that the default art has a 16%-of-screen soft edge.

## Notes and holds (shapes, rendering)

### Notes

- **Size**: lane width × (`WidthForNoteHeightScale` or lane width) × image height / width
  (`GameplayHitObjectKeys.cs:226-264`).
  - Bar's note images are 256x100 and **solid colour across the whole image** (measured: alpha 1 everywhere, one
    colour per lane). Notes are flat full-lane bars with no rim, no rounding and no gap to the neighbouring lane
    (`NotePadding` 0).
  - Height **46.7 px = 4.3% of H at 4K** (4.9% of the line-to-top distance) and **31.8 px = 2.9% at 7K** (3.4%).
- **Leading edge at the hit time** (see speed).
- **Hit notes vanish at once**: state Removed, unlinked the same frame (`InputManagerKeys.cs:313-324`,
  `HitObjectManagerKeys.cs:513-527`).
- **Missed notes** (late by more than the Okay window, `HitObjectManagerKeys.cs:601-660`) become **Dead**:
  - tinted by the skin's `DeadNoteColor` (200,200,200: a multiply, so about 78% brightness);
  - they keep scrolling at full alpha until one screen height of track past the line, then are removed
    (`GameplayHitObjectKeys.cs:244,571-585`; `ScrollGroupControllerKeys.cs:60-61` render threshold);
  - with Bar's opaque receptor panel drawn over notes, the part past the line is hidden. In the default skin a missed
    note visibly disappears at the line (read from code and the images; not captured).

### Snap colouring

- A **skin** option, `ColorObjectsBySnapDistance`: **off in Bar** (the default), on in Arrow, off in Circle [Q4].
- Snap of each note (`HitObjectManager.cs:42,172-190`):
  - round the offset from its timing point to 1/48 beats;
  - take the first of 1/1, 1/2, 1/3, 1/4, 1/6, 1/8, 1/12, 1/16 that divides it, else "1/48".
- The skin supplies **one texture per snap** (`note-hitobject-N-4th` and so on, or a 9-frame sheet), not a tint
  (`GameplayHitObjectKeys.cs:507-528`, `SkinKeys.cs:49,852-890`). Arrow's palette was not checked (unverified).
- Lazer is close: closest of 1, 2, 3, 4, 5, 6, 7, 8, 9, 12, 16 (`osu.Game/Beatmaps/ControlPoints/ControlPointInfo.cs:193-218`,
  `osu.Game/Screens/Edit/BindableBeatDivisor.cs:17,104-138`), as a tint, off by default.

### Holds

Images per lane: head (`note-holdhitobject`), body (`note-holdbody`, can be animated), end (`note-holdend`, drawn
only if the skin's `DrawLongNoteEnd`, true in all default skins).

- **Body extent**: from the middle of the head to the middle of the end. `LongNoteSizeAdjustment` = (head height − end
  height) / 2 (`GameplayPlayfieldKeys.cs:294-303`, `GameplayHitObjectKeys.cs:42-56,473-485`). This is mania's extent
  (`game-osu-mania.md`, "Holds").
- **Body drawing**: one sprite **stretched** to the body length. Wobble draws a sprite's image into its rectangle;
  there is no tiling (Wobble `Sprite.cs:260`).
  - With Bar's solid images, the head, body and end of a hold read as **one solid bar in the lane's colour, full lane
    width, fully opaque**.
  - There is **no long-note opacity setting in gameplay**. `EditorLongNoteOpacity` 30-100% is editor-only
    (`ConfigManager.cs:774,1283`).
- **While held** (state Held, `HitObjectManagerKeys.cs:741-745`):
  - once the song passes the start time, the head sits on the hit position and the body shortens from the line
    (`GameplayHitObjectKeys.cs:441-495`, `ScrollNoteController.cs:98-113`). An early hit waits for the start time.
  - The body's animation frames loop at 30 fps only while held (`GameplayHitObjectKeys.cs:606-612`,
    `InputManagerKeys.cs:320-321,345`).
- **Held past the end**: the body and end are hidden once the song passes the end time while held, or once the
  remaining body is shorter than half the head. The head stays on the line until the release is judged, at the end
  time + Okay × the release multiplier (`GameplayHitObjectKeys.cs:487-494`, `HitObjectManagerKeys.cs:666-716`).
- **Released early** (before any window), **released with Okay or Miss**, or **head missed**: the whole hold becomes
  Dead (`InputManagerKeys.cs:286-309,395-400,433-435`; `HitObjectManagerKeys.cs:640-660`).
  - It is tinted `DeadNoteColor`.
  - Its body keeps the length it had at release: the held size stops updating (`GameplayHitObjectKeys.cs:380-393`).
  - The head is no longer pinned, so it jumps back to its unpinned position (`GameplayHitObjectKeys.cs:451-461`).
  - It scrolls on and is never faded.
- Released with Marvelous to Good: Removed, gone at once (`InputManagerKeys.cs:395-400`).
- **Long Note Shrink ("Percy")**: −50..200, default 0 (`ConfigManager.cs:236-245,1324`; `OptionsMenu.cs:209-213`).
  - It shortens every body by that many real milliseconds of travel and moves the end sprite toward the head
    (`GameplayHitObjectKeys.cs:409-436,477-485`).
  - The end is therefore drawn crossing the line before the release time. It is a readability aid for LN-heavy
    charts and changes nothing in judgement.

### Bar lines

- One per measure from each timing point, 2 px tall, skin colour (white in Bar), across the stage
  (`TimingLineManager.cs:105-127`, `TimingLine.cs:60-64`).
- "Display Timing Lines" is on by default (`ConfigManager.cs:1181`).

## Key count and lane layout

| Default Bar skin, 16:9 | 4K | 7K |
|---|---|---|
| Stage width | 477.9 px = **24.9%** of W | 570.7 px = **29.7%** of W |
| Lane width | 6.2% of W | 4.2% of W |
| Gaps between lanes | none (`NotePadding` 0) | none |
| Note colours (measured from the images) | white, cyan, cyan, white (1-2-2-1) | white, cyan, white, **pink**, white, cyan, white (1-2-1-S-1-2-1) |
| Column-light colours (`ColumnColor`) | white, light blue (129,196,248), light blue, white | white, cyan, white, pink, white, cyan, white |

- **Widths.** All lanes are the same width, including the 7K centre (`GameplayPlayfieldKeys.cs:67-83,118-131`), and
  the stage grows with key count, as in mania. Quaver has no 7K hand grouping by spacing: the centre is marked only by
  colour.
- **Other key counts** in Bar's `skin.ini` [Q4]:
  - 6K: 1-2-1-1-2-1;
  - 8K: 2-1-2-1-1-2-1-2;
  - 9K: 2-1-2-1-S-1-2-1-2;
  - 10K: 2-1-S-1-2-2-1-S-1-2, two mirrored 5K hands, **each with its own pink centre**.
- **Scroll direction**, per key count (`ConfigManager.cs:1355`; `ScrollDirectionHelper.cs:10-81`):
  - default Down;
  - also Up, Split, Split Reverse, Alternate, Alternate Reverse, Mid and Mid Reverse, which mix up and down lanes.
  - Upscroll moves receptors to the top and can flip note and end images (`GameplayPlayfieldKeys.cs:334-352`,
    `GameplayHitObjectKeys.cs:165-177,292-298`).
- **Scratch lane**: optional 7K+1-style lane with its own width (`GameplayPlayfieldKeys.cs:75-79`,
  `GameplayPlayfieldKeysStage.cs:396-410`). Not relevant to mania maps.

## Feedback (judgement display, hit effects, lane lighting)

Default skins ship **transparent hit lighting**: 1x1 or 2x2 images, or a 256x100 fully transparent image in Circle,
measured [Q4]. So by default the feedback is the receptor, the column light and the judgement.

- **Column lighting** (`ColumnLighting.cs:19-41`, `GameplayPlayfieldKeys.cs:316`, `GameplayPlayfieldKeysStage.cs:432-445`):
  - an image tinted with the column colour, sitting on the receptor's top, `ColumnLightingScale` × lane width × 4 tall
    (Bar's 100x400). That is 0.44H at 4K and 0.30H at 7K.
  - Bar's image is transparent in its upper half and rises to **42% alpha** at the line, so the visible glow is about
    **2 lane widths tall** (0.22H at 4K).
  - Its alpha eases toward 1 when pressed (time constant 2 ms, effectively instant) and toward 0 when released (time
    constant **60 ms**, an exponential decay per frame).
- **Hit lighting**, when a skin provides it (`HitLighting.cs:75-195`, `GameplayPlayfieldKeysStage.cs:640-674`):
  - centred on the receptor, additive;
  - an animated sheet plays once at `HitLightingFps` (60); a single image fades linearly over **120 ms × rate**;
  - while a hold is held, its hold image loops, or a single image pulses its alpha 0 → 1 → 0 every 240 ms × rate;
  - it plays on every non-miss press and again on a hold's release.
  - "Tint Hitlighting Based On Judgement Color" (default **off**, `ConfigManager.cs:1308`) tints it with the
    judgement colour: Marvelous (255,255,200), Perfect (255,255,0), Great (0,255,0), Good (0,168,255), Okay
    (255,0,255), Miss (255,0,0) (`SkinKeys.cs:107-115`).
- **Judgement** (`JudgementHitBurst.cs:72-170`, `GameplayPlayfieldKeysStage.cs:602-635`):
  - one image per stage, centred over the stage at `JudgementBurstPosY` below the screen centre: 108 × 1.4056 =
    152 px, so y = **0.641H**, which is **0.240H above the 4K line**;
  - 140.6 px tall (`JudgementHitBurstScale` 100, 240x240 images with padding);
  - a single-frame image starts 5 px higher and eases down in 183 ms (EaseOutExpo), then fades with a 240 ms time
    constant;
  - **`DisplayJudgementsInEachColumn`** (skin option, default off) shows one judgement per column over its own lane,
    optionally rotated with the column.
- **Combo**: centred over the stage at y = **0.448H** (`ComboPosY` −40, `GameplayPlayfieldKeysStage.cs:524-557`).
  **Hit error bar**: 0.559H.
- **Key labels**: the bound key's name over each receptor, fading out after 2 s, not in replays
  (`GameplayPlayfieldKeysStage.cs:797-824`).

## What this means for us

Checked against `SPEC.md`, `DECISIONS.md` and the code as of 2026-11-04. Quaver has no camera, so it never argues for
or against a camera choice. Its value is as a second flat keyboard reference next to mania. **Most of what it does,
we already do**; the list says so explicitly so nobody rebuilds it.

**QV1. Speed is visible time; no change (SPEC 3).**

- Quaver's speed number is an on-screen speed, so its visible time follows the skin's line. Mania's speed does the
  same through `(768 − H) / 643.2`.
- We show the real visible time in the label and toasts (DECISIONS "Speed and wall show the real visible time"),
  which is what makes Quaver's and mania's numbers comparable.
- **Matches.** For Quaver players and playtests: mania speed on argon ≈ 0.87 × Quaver speed (4K Bar). 405 ms is
  Quaver 33.4; the default 15.0 is 902 ms.
- No "≈ Quaver" hint in the UI. Alpha plays osu!, and the label already gives milliseconds.

**QV2. Rate mods keep visible time; no change (SPEC 3).**

- Quaver keeps real-time visible time under rate mods, like mania and our highway, which uses mania's `TimeRange`.
- Its "Normalise Scroll Velocity By Rate" (default 0%, SV maps only) is a Quaver-only option. **Not proposed.**

**QV3. Speed per key count: not proposed now (SPEC 3, 5).**

- Quaver stores a speed and a direction per key count (1K-10K); mania and we keep one speed. Quaver needs it partly
  because its stage changes width with key count.
- Our track keeps one width for every key count (SPEC 5), and our speed is a visible time, so one speed already reads
  the same at 4K and 7K.
- If Alpha asks, it is a settings-only change that still feeds mania's `TimeRange` (no parity risk).

**QV4. Judgement line at 12% and a panel below it; matches SPEC 2's default and SPEC 8's "Panel".**

- Quaver's default line is 12.0% of the screen above the bottom at 4K (12.9% at 7K), with an opaque key panel under
  it drawn over passing notes.
- That is independent support for SPEC 2's 12% default (mania's own skins sit at 14-18%) and for the Panel key area.
  **No change.**

**QV5. Covers: our wall, "keep speed" and Far fade already reproduce Quaver's top cover (SPEC 4).**

- Quaver's top cover hides the top p of the *screen* (not of the lane), with the default art's soft edge 0.157H tall.
  It is set in options and keeps speed.
- At 4K Bar, its default 25% equals:
  - a wall hiding the first **10.5%** of the visible time, with "keep speed when moving the wall" on;
  - plus a **Far fade of 20%** (17.9% of the visible time over the remaining 89.5%).
- Both settings exist (DECISIONS "Optional far fade, in track depth, on notes drawn on the track"). **No new work.**
- **Bottom cover: not proposed** (`topic-covers.md` C7 stands). Quaver confirms that a keyboard VSRG treats it as a
  non-scoring setting, which invariant 3 already requires if we ever add one.
- `topic-covers.md`'s Quaver row could add the screen-height measure and the soft edge (left to its owner).

**QV6. Holds: ours already match Quaver's behaviour; one data point for M6 "Hold opacity" (SPEC 6).**

Quaver does what mania and our highway do:

- body from the head's middle to the tail's middle;
- head pinned on the line while held, only after the start time;
- head kept on the line when held past the end (DECISIONS 2026-11-04);
- length frozen after a drop, with the head returning to its unpinned position, as ours does (DECISIONS "Native
  notes": "the head goes back to its unclamped depth").

The difference is the look. Quaver's default bodies are **opaque, full lane width and the head's colour**. Ours are 80%
wide at alpha 0.55 idle, 0.85 held and 0.35 broken (`osu.Game.Rulesets.ManiaOrtho/UI/HighwayNoteLayer.cs:69,76,299`).
`game-sekai.md` K6 proposes "Hold opacity" 10-100% as a multiplier of those alphas, which can never reach Quaver's or
argon's solid bodies.

- **Proposal, for when M6 builds it**: let the setting be the idle body alpha itself, 10-100%, **default 55%** (today's
  look).
  - Held = idle + 0.30, capped at 1. Broken = idle × 0.64. At 55% these reproduce today's 0.85 and 0.35.
  - 100% then gives Quaver's and argon's opaque body.
  - Sekai's 10% low end is kept.
- Keep the 80% width, so the head reads over the body (K6's reason still holds).
- **Not proposed: Long Note Shrink.** It draws the tail crossing the line before its release time, which SPEC 6
  treats as a misaligned hold. Mania has no equivalent.

**QV7. Missed notes: no new vote for Q14(a).**

- Quaver's code lets missed notes scroll on, tinted grey (×0.78) and never faded.
- Its default skin's opaque key panel hides them at the line, which in effect is our Chunithm-style clip.
- If Alpha picks "slide like mania", the grey tint is an alternative to mania's 150 ms fade as the late cue. Data
  only.

**QV8. Lanes and colours: matches; no change (SPEC 5, 8).**

- Quaver's default colours have the same mirror structure as our built-in note colours and legacy mania:
  1-2-2-1 at 4K, 1-2-1-S-1-2-1 at 7K.
- Its 10K is two mirrored 5K hands, each with its own centre colour. Our 5|5 group divider says the same
  (DECISIONS "Hand-group dividers from 6K").
- Quaver has no spacing gap at 7K and no gap between notes. It does not need one on a flat 82-119 px lane, while our far
  lanes do (`topic-wide-keys.md` W3). Quaver argues for neither change.

**QV9. Feedback: keyboard numbers for a future lane beam; nothing else to change (SPEC 5, 7).**

- **Judgement height.** Quaver's stage judgement sits 0.240H above the 4K line, mania argon's and triangles' 180 units
  is 0.234H, and ours keeps mania's (DECISIONS "Judgements are drawn upright…"). **Matches.**
- **Per-column judgements.** Quaver offers them as a skin option, off by default. That is a keyboard-VSRG precedent for
  Q5's planned "Judgement placement: Screen / Track" setting, with Screen as the default. **No change.**
- **Lane beam, when the built-in highway skin draws its own** (DECISIONS "Hit lighting is drawn by the highway; lane
  beams stay with the skin" deferred it):
  - start from Quaver's keyboard values rather than argon's 800 ms release: **on at once, off in about 60-100 ms**
    (Quaver's 60 ms time constant), *(tune)*;
  - **20-25% of the track's depth** (Quaver's 2 lane widths are 25% of its 4K lane length and 17% at 7K);
  - **peak alpha ≤ 40%**;
  - in the lane's colour.
  - A short release keeps fast jacks in one lane apart (our inference, unverified by playtest).
- **Hit bursts.** Quaver's hit lighting (120 ms fade, optional judgement tint) is far subtler than our bursts (350 ms
  ring). No change: Chunithm is the reference for those (`game-chunithm-feedback.md`).

**QV10. Camera, mods and visual offset: nothing to take (SPEC 2, 9).**

- Quaver has no perspective, so it says nothing about our camera presets beyond Classic.
- It has no visibility mods, so it says nothing about SPEC 9's projected Hidden, Fade In, Cover and Flashlight.
- Its Visual Offset (drawing only) would break SPEC 6's "leading edge crosses the line at its hit time" check.
  **Not proposed.**

**QV11. SV: osu! maps move the same in Quaver as in mania; no change (SPEC 3).**

- Quaver's normalisation is mania's `RelativeScaleBeatLengths`, and No SV is Constant Speed.
- Correction for `topic-speed.md` (Quaver's row in the speed table, and S5): Quaver ignores BPM only for its own
  normalised charts.

**Suggested defaults from this note:** none change.

- Keep SPEC 2's 12% line, SPEC 4's wall semantics and our hold geometry.
- For M6: "Hold opacity" as the idle body alpha, 10-100%, default 55% (QV6).
- For the built-in skin's lane beam: on 0 ms, off 60-100 ms, 20-25% of depth, ≤ 40% alpha (QV9).

## Unverified / open

- Nothing was observed in a running Quaver. Layout, timing and draw order are read from code and the default skin
  files. These would need a capture to confirm:
  - missed notes vanishing under Bar's key panel;
  - the head jumping back after an early release;
  - the cover's soft edge on screen.
- Visible time at aspect ratios narrower than 16:9 (`ReceptorPosOffsetY` is not scaled).
- Line heights of the Arrow and Circle skins; Arrow's snap palette.
- Typical scroll speeds of Quaver players, and how widely lane covers are used.
- Exact equivalence of Quaver's SV normalisation and lazer mania's in edge cases (same-time timing points, infinite
  BPM, clamps).
- Whether a short beam release reads better than argon's 800 ms on the tilted track (QV9 is an inference).

## Sources

- [Q1] Quaver, **MPL-2.0**, commit e9b54a66c5274b1651328c66e9ea3846f8614508 (develop, 2026-11-02):
  https://github.com/Quaver/Quaver/tree/e9b54a66c5274b1651328c66e9ea3846f8614508. Paths under `Quaver.Shared/`:
  - `Config/ConfigManager.cs:236-256` (Percy, speed and SV-by-rate docs), `:410-440` (lane cover docs), `:1152`,
    `:1161`, `:1164`, `:1181`, `:1201-1202`, `:1244-1249`, `:1283-1284`, `:1307-1308`, `:1324`, `:1353-1355`;
  - `Config/ScrollDirection.cs:14-24`;
  - `QuaverGame.cs:142` (`SkinScalingFactor` 1920/1366), `:417`, `:1265-1281` (virtual screen);
  - `Input/Global/GlobalInputHandler.cs:153-196`; `Screens/Gameplay/GameplayScreen.cs:724-733`;
  - `Screens/Options/OptionsMenu.cs:207-213,242-251,266-281,405-410,500-505`; `Screens/V2/Options/Model/OptionsList.cs:373-375`;
  - `Screens/Gameplay/Rulesets/Keys/HitObjects/TimingGroupControllerKeys.cs:24-72`;
  - `.../HitObjects/ScrollGroupControllerKeys.cs:29,60-61,107-162,258-276`;
  - `.../HitObjects/ScrollNoteController.cs:48-124`; `.../HitObjects/NoteControllerKeys.cs:142-143,191-217`;
  - `.../HitObjects/HitObjectManagerKeys.cs:41,139,284-299,505-527,601-716,741-745,786-792`;
  - `.../HitObjects/GameplayHitObjectKeys.cs:42-56,160-312,371-495,507-528,571-585,606-612`;
  - `Screens/Gameplay/Rulesets/HitObjects/HitObjectManager.cs:42-62,172-190` (snap);
  - `.../Keys/Playfield/GameplayPlayfieldKeys.cs:67-146,193-264,269-360`;
  - `.../Keys/Playfield/GameplayPlayfieldKeysStage.cs:167-258,329-447,452-519,524-674,749-824`;
  - `.../Keys/Playfield/ColumnLighting.cs:19-41`; `.../Keys/Playfield/HitLighting.cs:75-195`;
  - `.../Keys/Playfield/ScrollDirectionHelper.cs:10-81`; `.../Keys/Playfield/Lines/TimingLineManager.cs:105-127`,
    `TimingLine.cs:60-64`;
  - `Screens/Gameplay/Rulesets/Input/InputManagerKeys.cs:134,268-436`;
  - `Screens/Gameplay/UI/JudgementHitBurst.cs:72-170`;
  - `Skinning/SkinKeys.cs:49,70-140,205-218,300-334,520-573,609-715,747-763,852-890`.
- [Q2] Quaver.API, **MPL-2.0**, commit a921d561b2ece7f6bf3682446696c06c17b81649 (the submodule pinned by [Q1]):
  https://github.com/Quaver/Quaver.API/tree/a921d561b2ece7f6bf3682446696c06c17b81649.
  - `Quaver.API/Maps/Qua.cs:120-140,623-650,1238-1300,1495` (SV normalisation, common BPM);
  - `Quaver.API/Maps/Parsers/OsuBeatmap.cs:463-536` (osu! import, SV clamp 0.01-10);
  - `Quaver.API/Enums/ModIdentifier.cs:19-65`; `Quaver.API/Helpers/ModeHelper.cs:15`; `Quaver.API/Enums/GameMode.cs:18-28`.
- [Q3] Wobble, **MIT**, commit 659e78978454173d22c2416ed7286153e7808012:
  https://github.com/Quaver/Wobble/tree/659e78978454173d22c2416ed7286153e7808012.
  - `Wobble/Window/WindowManager.cs:26-37`; `Wobble/Graphics/Sprites/Sprite.cs:260` (image drawn into its rectangle);
    `Wobble/Graphics/Sprites/AnimatableSprite.cs:170-178`.
- [Q4] Quaver.Resources (folder `Quaver.Resources/` of [Q1]), **CC BY-NC 4.0** (`Quaver.Resources/LICENSE.md`):
  `Quaver.Resources/Textures/Skins/{Bar,Arrow,Circle}/skin.ini` and the Bar images under `Keys4/` and `Keys7/`
  (receptors, hit objects, lane covers, lighting).
  - Sizes come from `file`. Colours and alpha profiles come from ImageMagick (`convert -resize 1x1`, single-column
    crops). The two receptor images were viewed.
  - Nothing was copied into this repository.
- osu! lazer (this repo, MIT): `osu.Game/Rulesets/UI/Scrolling/DrawableScrollingRuleset.cs:80,111-113`;
  `osu.Game/Beatmaps/ControlPoints/ControlPointInfo.cs:193-218`; `osu.Game/Screens/Edit/BindableBeatDivisor.cs:17,104-138`.
- Existing notes: `game-osu-mania.md` (visible-time formula, holds, judgement height), `topic-speed.md`,
  `topic-covers.md` (Quaver rows, C7), `topic-wide-keys.md` (W3), `game-sekai.md` (K6),
  `game-chunithm-feedback.md`.
- Ours: `osu.Game.Rulesets.ManiaOrtho/UI/HighwayNoteLayer.cs:69,76,299`;
  `Configuration/ManiaOrthoRulesetConfigManager.cs:122-133`.
- Numbers: computed from the values above with a scratch Python one-liner outside the repository.
  - line_y = 1080 − `ColumnSize` × 1.4056 × receptor height / width;
  - T = line_y / (S × 1.4056 / 20);
  - mania equivalents from `game-osu-mania.md`'s formula.
