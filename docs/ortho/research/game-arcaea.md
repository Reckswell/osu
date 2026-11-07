# Arcaea (lowiro): ground track

> **Correction (2026-11-07, `topic-speed-scales.md`):** the 3333 / speed ms scale is confirmed by a frame-stepped
> capture of the official game [SS13]; the doubt below no longer stands.

Last checked: 2026-11-04

## Summary

Arcaea draws its 4-lane ground track as one plane under a fixed look-at camera, with notes moving **linearly in
depth** (no eased approach). The geometry below comes from the camera, track and note constants of the open fan
reimplementations ArcCreate and ArcCore, which both inherit them from the fan editor Arcade ("Taken from arcade",
ArcCore). We projected that camera ourselves. **No official frame was measured**, so every number below is the
clones' camera, not a measurement of the game.

At 16:9:

| Feature | Value |
|---|---|
| Judgement line | **0.857H** (14.3% above the bottom) |
| Far end of the track (where notes appear) | **0.070H**, a hard edge |
| Vanishing point | 0.036H **above** the top edge |
| Lane span at the line | **0.849W** (4 lanes; the track's borders add 3.5% of its width on each side) |
| Far/near width ratio | **0.119** |
| Lean of the outer lane edge | 40.2° |

Arcaea is therefore the reference whose track reaches almost to the top of the screen with a small far ratio and
**linear** depth. It is the honest version of what Q7 asks about: no stretching and no eased curve. Its price is the
one SPEC 3 accepts: a note spends the first half of its visible time in the top 15% of the screen, and it rushes
71× faster at the line than at the far end. Players notice this and get used to it [A9].

Two things keep far notes readable:

- **Notes thicken with distance.** Taps are 1.5 units deep at the line and 7.75 units at the far end, growing linearly
  with depth. On screen at 1080p they measure 96 px at the line and 7 px at the far end; a constant 1.5-unit tap would
  be 1.5 px at the far end.
- **Notes fade in** over the far 20% of the track depth.

**Speed and feedback:**

- The official speed setting runs from 1.0 to 6.5 in 0.1 steps [A6]. It follows the chart's BPM relative to a base
  BPM [A6][A8], as lazer mania's speed does.
- The clone's speed scale gives T = 3333 / s ms (6.5 = 513 ms, 4.0 = 833 ms). Its match with the official scale is
  unverified.
- Judgement text is drawn upright at the hit lane's point on the line, rising and fading over 0.5 s (clone). EARLY/LATE
  is shown above the combo at screen centre, or at the top or bottom of the screen if set so [A6][A7].
- Arcaea has no lane cover.

Proposals:

- an "Arcaea" camera preset, every value within today's sliders;
- one new option for it: note thickness that grows with distance (an exception to SPEC 6's wording);
- "Lane (upright)" as a third choice for Q5's judgement placement.

Conventions:

- y is measured from the top as a fraction of screen height H; x is a fraction of screen width W.
- px are pixels of a 1920×1080 frame.
- "Units" are the clones' world units: one lane is 4.25 units, the 4-lane span is 17, and the visible track is 100
  units long.
- d is the fraction of the visible track (or of the visible time) still to go: 0 at the line, 1 at the far end.

## Scope, evidence and versions

**In scope:** the ground track (floor notes, holds, lanes, judgement line). Sky notes, arcs, the sky input and arc
judgement are out of scope. They are a second input plane 5.5 units above the line and do not map to keyboard mania.

**Evidence, strongest first:**

1. **Official and community text.**
   - The Arcaea fandom wiki's Gameplay page [A5]: note types; hold rules; the combo sits under the sky input; holding
     an arc tilts the track.
   - wikiwiki.jp's rules page [A6] has the option list:
     - note speed 1.0-6.5 in 0.1 steps;
     - EARLY/LATE position top, middle or bottom;
     - Pure L/E display;
     - FR/PM marker;
     - touch effects;
     - colour-blind mode;
     - 120 fps.
   - Its FAQ [A7].
   - A note.com charting guide [A8]: hi-speed is linked to BPM.
   - An r/arcaea thread [A9]: notes appear to accelerate.
2. **Fan reimplementations, read for numbers only (nothing copied):**
   - **ArcCreate** [A1], GPL-3.0, commit a00f94ca (2026-09-16): the chart editor and player that replaced Arcade. We
     read its C# scripts, scene (`Gameplay.unity`), materials, shaders and meshes, then parsed the scene's transforms
     and the FBX vertices with our own scratch scripts.
   - **ArcCore** [A2], MIT, 2022: a test player with the same camera constants. It says "Taken from arcade" about the
     arc tilt.
   - **ArcaeaSim2** [A3], BSD-3-Clause-Clear, 2018: an early simulator. Its camera is untuned, but its track border
     ratio of 36/1024 is "measured from track texture" and agrees with ArcCreate's.
   - **pycade** [A4], no licence: aff tools with an initial camera "to be measured". Read only.
3. **Could not be read:**
   - the fandom "Interface and Features" pages;
   - namu.wiki;
   - the miraheze update history;
   - moegirl (JavaScript-only);
   - pixiv dictionary;
   - YouTube.

   No official frame was measured. We did not download screenshots (copyright; this task's rule).

**Versions:**

- Arcaea released on iOS/Android on 2017-03-09 and on Nintendo Switch on 2021-05-18 (developer lowiro) [A10].
- 120 fps mode and the current option set come from [A6]. Which version added each option is unverified.
- The Switch's Joy-Con mode splits the 4 lanes into a left pair and a right pair, one per controller [A11].

**How much to trust the clones' camera.** ArcCreate and ArcCore are built to play official-format charts (`.aff`),
including the official chart-driven camera events. Their camera moves those events relative to these constants.
That suggests the constants were matched to the game, but nobody documents how (inference). One official 16:9 frame
would settle the geometry.

## Camera and track

### The rig [A1][A2]

| Item | 16:9 | 4:3 | Source |
|---|---|---|---|
| Camera position | (0, 9, 9) | (0, 9, 8) | `Values.cs:74-78`; `CameraService.cs:70-77`; ArcCore `GameplayCamera.cs:54-72` |
| Looks at | (0, −5.5, −20): pitch 26.565° down at 16:9 (= atan 0.5), 27.4° at 4:3 | | `CameraService.cs:196-198`; ArcCore `:174` |
| Vertical FOV | 50° | 65° | `CameraService.cs:76-77`; scene `field of view: 50` (`Gameplay.unity:4498`) |
| Other aspect ratios | between 16:9 and 4:3 the values interpolate; wider than 16:9 clamps to the 16:9 camera | | Unity's `Mathf.Lerp` clamps; ArcCore clamps explicitly (`:51-52`) |
| Judgement plane | z = 0 on the track plane y = 0; notes come from z = −100 | | `ArcFormula.cs:45-48,66,77`; `Values.cs:8` |
| Lanes | x = ±2.125 and ±6.375; lane 4.25 wide; lane 1 is on the screen's left | | `ArcFormula.cs:30-38`; `Values.cs:11` |
| Track sprite | 10.24 × 153.5, scaled ×1.7896 in x: 18.33 wide, z +53.5 to −100; lane boundaries at 3.5%, 26.75%, 50%, 73.25% and 96.5% of the width | | `Gameplay.unity:11053-11054,11121`; `Track.mat:60-65` |
| Border | 36/1024 of the texture on each side (3.5%): 0.64 units, 3.8% of the 17-unit lane span | | `Track.mat`; ArcaeaSim2 `StageMetrics.cs` ("measured from track texture") |

The track shader has no distance fade (`Track.shader` frag). The tiled track simply ends at z = −100 (clone). Whether
the official track fades into the background is unverified.

### Projected geometry (our computation)

We used a standard pinhole projection of the rig above, with the screen y measured from the top.

| Quantity | 16:9 | 4:3 |
|---|---|---|
| Judgement line (z = 0) | **0.857H** | 0.801H |
| Far end (z = −100) | **0.070H** | 0.173H |
| Vanishing point | **−0.036H** (above the top) | +0.094H (on screen) |
| Lane span at the line | **0.849W** | 0.890W |
| Lane span at the far end | 0.101W | 0.100W |
| Far/near width ratio r | **0.119** | 0.112 |
| Outer-edge lean | 40.2° | 40.0° |
| Track plane vs image plane | 63.4° | 62.6° |
| Lane span where the track meets the bottom edge | 0.985W (at z = +1.86) | 1.14W (off screen) |

**Camera in track widths** (17 units), as `game-chunithm.md` gives it:

- eye height 0.53;
- judgement line 0.53 ahead of the eye;
- visible track 5.88 long;
- vertical FOV 50°.

**Comparison** (the other values come from `game-chunithm.md`, `game-sdvx.md` and `game-sekai.md`):

| | Chunithm (arcade) | USC landscape (SDVX proxy) | Sekai (Sonolus) | **Arcaea (clones)** |
|---|---|---|---|---|
| Judgement line | 0.926H | 0.941H | 0.791H | **0.857H** |
| Far end | 0.163H | 0.284H | 0.015H | **0.070H** |
| Vanishing point | −0.079H | on screen | −0.046H | **−0.036H** |
| Far/near | 0.24 | 0.106 | 0.073 | **0.119** |
| Width at the line | 0.611W | 0.435W | 0.787W | **0.849W** |
| Lean | 28.4° | — | 39.9° | **40.2°** |
| Approach | linear | linear | eased | **linear** |

Arcaea has SDVX's far ratio but a far end 21% of H higher and twice the width. Its lean and its vanishing point just
above the screen are Sekai's, but without Sekai's curve.

### Camera motion

- **Chart-driven camera.** Official charts can move and rotate the camera with `camera(...)` events. They also have
  scene control events (hiding the track, 6-lane widening and others) (`CameraService.cs:205-223`; `TrackController.cs`;
  [A4] `aff.py:14`).
- **Arc tilt.** While an arc is held off-centre the track rolls slightly: "Holding an arc note on the left of the
  screen causes the track to tilt slightly anticlockwise" [A5]; "ホールドする位置によってレーンが傾きます" [A6].
  - In the clones the camera's up vector leans by up to 0.05, a roll of about 3.0°. It eases toward its target at
    6 per second (ArcCreate) or 4-8 per second (ArcCore) (`Values.cs:81-82`; `CameraService.cs:250-273`; ArcCore
    `:165-175`).
  - It is driven by arcs only, so ground notes never move the camera.

## Note approach and speed

### Depth is linear in time [A1]

- z = −(floor position difference) / base BPM × drop rate / 1000, where the floor position is time × BPM summed over
  timing points (`ArcFormula.cs:66,77`; `Note.cs:24-25`).
- At the base BPM a note covers dropRate/1000 units per ms, so the 100-unit track takes **T = 100000 / dropRate ms**.
- This is plane-linear depth, as in Chunithm and SDVX.

**Player reports [A9].** An r/arcaea thread asks how to stop notes "accelerating", as they do not in Project Sekai.
Answers:

- "the notes actually come towards you at a constant speed, but the angle makes the notes appear as if they're
  accelerating";
- there is no setting to change it;
- "most players use 4+".

**Where a note is on screen** (16:9; our projection):

| d (time still to go) | 0 | 0.09 | 0.10 | 0.25 | 0.50 | 0.75 | 1.0 |
|---|---|---|---|---|---|---|---|
| y | 0.857H | 0.500H | 0.477H | 0.277H | 0.154H | 0.100H | 0.070H |
| Lane span | 0.849W | — | 0.488W | 0.298W | 0.181W | 0.130W | 0.101W |

- A note is in the lower half of the screen only for the **last 9%** of its visible time.
- It spends half its time between 7% and 15% of screen height.
- With `topic-speed.md`'s closed forms and r = 0.119:
  - 10.6% of the time is spent in the near half of the on-screen track;
  - the speed at the line is 8.4 track heights per visible time, which is **6.6 H per visible time**;
  - the line/far speed ratio is 71.

### Speed number to visible time

- **Official setting:** "1.0~6.5まで0.1刻みの56段階" (1.0 to 6.5 in 56 steps of 0.1) [A6].
- **BPM:** "基本的にどの譜面も同じ速度で流れてきますが、一部譜面では通常とは異なる速度が設定されている" (charts basically
  scroll at the same speed, but some are set differently) [A6]. "ArcaeaのハイスピードはBPMと連動している" (hi-speed is
  linked to BPM: a new BPM in the timing list changes the scroll speed) [A8].
  - So scroll speed is relative to each chart's base BPM, and BPM changes inside a chart change it.
  - ArcCreate keeps a per-chart base BPM (`Values.cs:102`; `ChartService.cs:539`). That is lazer mania's model
    (speed relative to the most common beat length; `topic-speed.md` S5).
- **Clone scale:**
  - ArcCreate's drop rate defaults to 150 (`Settings.cs:13`). Its menu shows "speed" = drop rate / 30, from 1.0 to
    10.0 in 0.1 steps (`Data/Constants.cs:7-9`; `Selection/Settings/SettingsDialog.cs:151,171,176`).
  - So **T = 3333 / s ms** at the base BPM.
  - Whether ArcCreate's s equals the official number is **unverified**: its range goes past the official 6.5, and no
    source states the mapping.

| s (clone scale) | 1.0 | 2.0 | 3.0 | 4.0 | 5.0 (ArcCreate default) | 6.0 | 6.5 (official max) |
|---|---|---|---|---|---|---|---|
| T (ms) | 3333 | 1667 | 1111 | 833 | 667 | 556 | 513 |
| Mania label speed with the same visible time (11485 / T) | 3.4 | 6.9 | 10.3 | 13.8 | 17.2 | 20.7 | 22.4 |
| Speed at the line (H/s, 16:9) | 2.0 | 4.0 | 5.9 | 7.9 | 9.9 | 11.9 | 12.9 |

- **If the scale is right:**
  - Alpha's 405 ms would be s = 8.2, past the official maximum.
  - Our 840 ms playtest speed is about s = 4.0.
  - Arcaea players at 4.0-6.5 see 8-13 H/s at the line, against 4.3-4.9 H/s for Chunithm and Sekai players
    (`game-sekai.md` K3).
- That gap is large enough to doubt the scale (or Arcaea's very wide, thick near notes make it acceptable). Verify
  before any speed hint uses it.

## Judgement line and receptors

- **One line segment per lane.** Each is a band 1 unit deep, from z = 0 to −1, centred on the lane
  (`Gameplay.unity` CriticalLine0-5 at local y 54 under the track, scale 239 × 3.7 of a 0.01 × 0.27 sprite; our
  reading of the transforms).
  - On screen at 1080p the band is about 67 px tall at the line.
  - Its texture (a thin core with a glow?) is unverified.
- **The near edge of a tap reaches z = 0 at its hit time** (inference).
  - The tap mesh spans z from 0 to −1 in mesh space and is scaled in z only (our parse of `Tap.fbx`; `Tap.cs:65-69`).
  - So at hit time the tap covers z 0 to −1.5, and its near edge meets the band's near edge. That is mania's
    leading-edge anchor.
- **No receptors.** The fandom wiki calls the line the "floor input" [A5]. There are no per-lane receptor graphics in
  the clone's scene.
- **The track continues under the line** to z = +53.5, past the bottom of the screen (z = +1.86 at the bottom edge).

## Lane covers / walls

- **No cover.** The official option list [A6] has none; it has visibility toggles for:
  - the potential display;
  - the skill cut-in;
  - the FR/PM marker;
  - the pause button style;
  - touch effects;
  - colour-blind mode.

  This settles `topic-covers.md`'s "Unverified" bullet for Arcaea.
- **Hiding comes from the chart.** Scene control events (track hide, note group alpha) are chart data, not player
  options (`TrackController.cs`).

## Notes and holds (shapes, rendering)

**Taps** [A1]:

- **Shape.** A flat quad 3.8 units wide (89% of the 4.25-unit lane), lying on the track (our parse of `Tap.fbx`:
  x ±1.9, depth 1 in mesh units).
- **Depth grows linearly with distance:** 1.5 + 6.25 × d units for notes still to come (`ArcFormula.cs:186-194`;
  `Tap.cs:106`). That is 1.5% of the track at the line and 7.75% at the far end, ×5.17.

  | d | 0 | 0.10 | 0.25 | 0.50 | 0.75 | 1.0 |
  |---|---|---|---|---|---|---|
  | Tap depth (units) | 1.50 | 2.12 | 3.06 | 4.62 | 6.19 | 7.75 |
  | On screen, 1080p (px) | **96.5** | 46.0 | 24.9 | 13.9 | 9.6 | **7.3** |
  | A constant 1.5-unit tap (px) | 96.5 | 33.3 | 12.7 | 4.7 | 2.5 | 1.5 |
  | Lane width (px) | 408 | 234 | 143 | 87 | 62 | 48 |

  For comparison, our Chunithm preset's fixed 2.55% notes are about 87 px at the line and 5 px at the far edge
  (`game-chunithm.md` P5).
- **Fade in.** Alpha = (100 + z) / 20, clamped to 0-1 (`ArcFormula.cs:206-214`; `Tap.cs:109-113`). So taps fade in
  linearly in depth over the far **20%** of the track.
- **Colours.** One texture per skin side (Light, Conflict and per-pack variants), not per lane (`Tap.cs:47-50`;
  `ScriptableObjects/SkinOptions/Note/*`). Colour-blind mode changes only arc colours [A6].
- **Missed taps.** A missed tap keeps moving past the line until its window closes (120 ms; `Values.cs:23`;
  `Tap.cs:89-94,145,154-164`), then disappears. It is not drawn after it is judged.

**Holds** [A1]:

- **Shape.** One quad as wide as a tap, from the head's depth to the tail's. The texture is stretched once over the
  whole length, with no separate head or tail sprite (`Hold.cs:163-170`; `HoldNote.shader:70-86`).
- **Alpha.** The body is drawn at 86% alpha (`Values.cs:33`).
- **Before it is grabbed,** it moves past the line like a tap.
- **Once grabbed,** it is clipped at the line, so the body shrinks into the line (`Hold.cs:152-160`). While held:
  - it uses the highlight half of its texture;
  - its alpha drops to 85% every fourth frame, a shimmer (`Values.cs:31,35`; `Hold.cs:180-188`).
- **After its start time, while not held,** it is drawn at 50% (`Values.cs:38`). With the "fading holds" option it
  fades to 50% after 500 ms (`Values.cs:39-40`; `Hold.cs:189-202`).
- **Far end.** Holds do **not** fade in; the shader cuts them at z = −100 (`HoldNote.shader:76`).
- **Rules** [A5][A6]: a hold has no FAR or PURE early/late timing, can be picked up mid-note, and allows a brief
  release. That does not concern us: mania judges holds.

**Bar lines.** Beat lines are drawn across the track. Their thickness grows ×4 from the line to the far end
(`ArcFormula.cs:196-204`; `Values.cs:58`).

## Key count and lane layout

- **Four equal lanes** on one track. Lane dividers are drawn at the three inner boundaries (`Gameplay.unity` Div01-45).
- **Six lanes.** Charts can widen the track to 6 lanes with a scene control event; extra lanes 0 and 5 sit outside
  the borders (`LaneExtraL/R`, `EdgeExtraL/R`). That is chart data, so it is out of scope.
- **Joy-Con mode** splits the 4 lanes into 2 + 2 [A11].
- Nothing scales with key count. Arcaea is a 4K-only reference.

## Feedback (judgement display, hit effects, lane lighting)

- **Judgement text.**
  - On every judged tap or hold tick, a PURE/FAR/LOST sprite is emitted at the hit lane's point on the line
    (`Tap.cs:141-143`).
  - It is moved onto a camera-facing plane 100 units in front of the camera (`ParticleService.cs:180-206,499-511`). So
    it is **upright and the same size in every lane**, not lying on the track.
  - It rises (initial velocity 50, easing to 0) and fades over **0.5 s** (`TextParticle.prefab`: lifetime 0.5,
    velocity-over-lifetime y 50).
  - Individual judgement names can be hidden (`ParticleService.cs:184-189`).
- **EARLY/LATE.**
  - Official: shown above the combo when "Pure L/E display" is on [A7]. The position is top, middle or bottom of the
    screen: "画面上部 or 画面中部 or 画面下部" [A6].
  - ArcCreate anchors these at 0.9, 0.5 and 0.25 of the height from the bottom (default middle). The text eases up and
    fades over 0.5 s, and can show the ms offset instead (`ParticleService.cs:88-101,208-237,443-463`).
- **Combo.** "The combo counter sits under the sky input" [A5]. The sky input line projects to 0.40H, and ArcCreate's
  combo text is anchored at the screen centre (`Gameplay.unity` ComboText). FR/PM markers go under the cover art or
  under the combo [A5][A6].
- **Hit effect.** A tap particle at the projected hit point (`ParticleService.cs:163-178`). The official "touch
  effects" option turns touch effects on or off [A6].
- **Lane flash** (clone).
  - On a hit, the lane lights white at **15%** alpha over a lane-wide quad from the line to **60%** of the track depth,
    fading linearly over **150 ms** (`Values.cs:17-20`; `InputFeedbackService.cs:20-34`).
  - The quad's size comes from `LaneHit.prefab`'s scale of 425 × 23.4 applied to a 0.01 × 2.56 sprite (our reading).
  - On screen it reaches 0.128H.
- **Holds.** A particle at the line while held, as above.

## What this means for us

Everything here is visual: settings, not mods (invariants 1 and 3).

### R1. Where we already match (no change)

- **Linear depth** (SPEC 3, `topic-speed.md` S1). Arcaea is linear, like Chunithm and SDVX. Its players perceive
  acceleration and accept it [A9].
- **Speed relative to the chart's base BPM** (SPEC 3, S5). This is mania's own normalisation.
- **Far fade in track depth.** Our "Far fade" (DECISIONS 2026-11-04) is linear in depth from where notes appear.
  Arcaea's 20% sits inside its 0-30% range.
- **Holds** (SPEC 6).
  - Clipped at the line while held, as our head-pinned holds are.
  - Dimmed when not held: Arcaea 50%, ours 35% dropped.
  - Arcaea's body is 89% of the lane against our 80%. Leave it.
- **Upright judgement text** (SPEC 7). Ours is upright too; it is centred rather than per lane (R5).
- **No camera motion from ground notes** (SPEC 2). Arcaea's only motions are chart-driven or arc-driven.
- **Judgement line height.** Arcaea's 14.3% sits beside SPEC 2's 12% default.

### R2. An "Arcaea" camera preset (SPEC 2; BACKLOG M6 "More presets from research")

Every value is within today's sliders (`ManiaOrthoRulesetConfigManager.cs:118-135`).

| Setting | Chunithm preset now | **Arcaea** |
|---|---|---|
| Camera | Perspective | Perspective |
| Judgement line height | 0.074 | **0.143** |
| Far edge height | 0 (stretched) | **0.070** (faithful; no stretching) |
| Track width | 0.61 | **0.849** |
| Far/near width ratio | 0.24 | **0.119** |
| Wall | 0 | 0 |
| Note thickness | 0.0255 | **0.015 at the line**, growing ×5.17 to the far end (R3) |
| Far fade | 0 (a preference) | 0.20 if presets may set it; otherwise recommend it in the preset's description |

- **What follows from these values.** The vanishing point is −0.036H and the lean 40.2°. These are the clones' camera
  values exactly: our homography with them puts d = 0.5 at 0.154H, matching the projection.
- **Lane widths at 1080p.**
  - 7K: 233 px at the line, 28 px at the far end.
  - 10K: 163 px and 19 px. At every depth that is wider than Chunithm's 1-cell lanes (73 / 17.6 px), so it is safe
    by `topic-wide-keys.md`'s test.
  - 4K lanes are 408 px at the line, as wide as Arcaea's own.
- **Why add it.**
  - It is the one reference that brings notes in from near the top (7% down) with real perspective and linear depth.
  - It avoids both Q7(a)'s stretching (Chunithm's lean falls from 28° to 14°) and Q17's eased curve.
  - It sits between our Chunithm (3.9 H per visible time at the line) and Voltex (8.9) presets, at **6.6 H per visible
    time**.
- **Not a default change.** A new preset is within "default values within ranges" and the backlog's "more presets".
  Record it in DECISIONS.
- **Playtest first.** 4K and 7K at 405 ms and 840 ms, Arcaea against Chunithm.

### R3. Note thickness that grows with distance (SPEC 6)

**Setting:** "Far note size" (growth g), 1-6, **default 1**, which is today's fixed thickness.

- **Definition.** A note whose leading edge is at depth d is T0 × (1 + (g − 1) × d) deep in track units. Arcaea's
  values are T0 = 0.015 and g = 5.17.
- **The leading edge does not move,** so hit times are unchanged (SPEC 6's "leading edge crosses the line at its hit
  time" holds).
- **Holds.** Only head and tail caps grow; bodies run head to tail as now.
- **Why it is needed.** Under the Arcaea camera:
  - a constant 0.015 leaves far notes 1.5 px tall;
  - a constant 0.025 makes near notes 150 px tall;
  - growth gives 96 px near and 7 px far, so both ends stay sane.
- **Gate.**
  - L0: g = 1 is identical to today's notes; thickness is monotonic in d; the leading edge is unchanged.
  - A parity run, like every setting.
- **The SPEC conflict.** SPEC 6 says "a fixed thickness in track units". If the main session reads that as intent
  rather than mechanism, ask Alpha. Proposed text for `QUESTIONS.md`:

  > **Q-Arcaea.** Add an "Arcaea" camera preset (line 14% up, far edge 7% below the top, track 85% wide, far lanes 12%
  > of the near width, linear approach as now), whose notes get thicker with distance (×5 at the far end, as in
  > Arcaea) so far notes stay visible? Options:
  > - (a) the preset with growing notes;
  > - (b) the preset with fixed thickness (far notes about 2 px);
  > - (c) no preset.
  >
  > Best guess (a).

### R4. Speed hint and playtests (SPEC 3)

- **No "≈ Arcaea s" hint yet.** The clone's T = 3333/s may not be the official scale. Add it only once one official
  measurement agrees (for example one frame-stepped capture of the approach at a known speed).
- **Playtests.** The speed-14 playtest (839 ms) equals the clone's s = 4.0, which r/arcaea calls the usual minimum.
  No new speeds are needed.

### R5. Judgement placement: a third option for Q5 (SPEC 7)

- **Three references, three answers:**
  - Chunithm: per lane, lying on the track;
  - Sekai and SPEC 7 today: one text, centred, upright;
  - Arcaea: **per lane, upright**, at the hit lane's point on the line, rising and fading over 0.5 s.
- **Proposal.** If the planned "Judgement placement" setting is built, offer **Lane (upright)** as a third choice. It
  keeps SPEC 7's readability (screen-facing, constant size) and shows where the hit was.
  - Default stays Screen until Q5 is answered.
- **What "Lane (upright)" needs.**
  - One drawable per lane, from a pool.
  - Placed at the lane's projected x on the line, at mania's judgement height above it.
  - Not scaled by the perspective, as Arcaea's is not. That differs from our current centred text, which shrinks with
    the track's width scale.

### R6. Lane flash (SPEC 5): no change

Arcaea's hit flash is fainter and shorter than ours: 15% alpha, 60% of the depth, 150 ms. Our beam is 30% while held,
over 50% of the depth, with a 150 ms fade-out (`HighwayTrack.cs:50-78`). Keep ours: it is the SDVX-style beam SPEC 5
asks for.

### R7. Missed notes (Q14(a)): a data point

In the clone, a missed tap continues past the line until its 120 ms window closes, and an unhit hold keeps scrolling
past the line dimmed to 50%. That joins mania and Sekai's fan engine on "slide past". The official behaviour is
unverified. No default change.

### R8. Not worth copying

- **Chart-driven camera and scene control:** SPEC 2 forbids beatmap-driven camera motion, and mania maps have none.
- **The arc-hold roll.** It rotates the judgement line, which SPEC 2 forbids even for cosmetic motion.
- **The sky input and arcs, and 6-lane widening.**
- **The aspect-dependent camera.** Our presets are fractions of the playfield. On ultrawide screens our track gets
  wider while Arcaea's stays fixed to the screen height. Revisit only if Alpha plays ultrawide.

### R9. Corrections for other notes (for the main session)

- **`topic-covers.md` "Unverified".** "Whether SDVX, Arcaea or DJMAX have any adjustable lane cover": Arcaea has none
  (official option list [A6]). DJMAX: see `game-djmax.md`.
- **`topic-speed.md`, ArcCreate row and "Unverified".**
  - Official speed: 1.0-6.5 in 0.1 steps (56 levels), relative to each chart's base BPM [A6][A8].
  - ArcCreate shows drop rate / 30 (1.0-10.0, default 5.0 = 667 ms).
  - The official visible time is still unverified.

### Where SPEC.md's assumptions about Arcaea hold or fail

1. **SPEC 2, "notes enter at the top edge with the vanishing point at or above it".** Holds closely: far end 0.070H,
   vanishing point −0.036H.
2. **SPEC 2, "far/near typically 0.3-0.6".** Fails: 0.119.
3. **SPEC 2, judgement line 12%.** Close: 14.3%.
4. **SPEC 3, linear in track depth.** Holds.
5. **SPEC 6, "fixed thickness in track units".** Fails: thickness grows ×5.17 with distance (R3).
6. **SPEC 7, screen-space text.** Holds (upright), but per lane rather than centred.

## Unverified / open questions

- **Official geometry.** Every camera number comes from the clones (ArcCreate, ArcCore, both from Arcade). One
  official 16:9 frame would confirm the line, far end, width and far ratio.
- **Official speed scale.** How the official 1.0-6.5 maps to visible time. ArcCreate's drop rate / 30 may be its own
  convention.
- **Official note thickening and far fade.** The ×5.17 growth and the 20% fade are the clones'. Whether the official
  track itself fades at the far end.
- **Textures.** The judgement line texture (the 1-unit band); the divider width as drawn; note and hold colours per
  skin.
- **Judgement text.** The official on-screen size and rise distance.
- **Missed-note behaviour** in the official game (taps past the line for 120 ms; holds scrolling past at 50%).
- **Tablets.** Whether the official tablet layout is the clones' 4:3 camera.

## Sources

Fan reimplementations (read for numbers only; nothing copied):

- [A1] ArcCreate (Arcthesia), **GPL-3.0**, commit a00f94ca602b990bc15d7ca530434da9f875d152 (2026-09-16):
  https://github.com/Arcthesia/ArcCreate/tree/a00f94ca602b990bc15d7ca530434da9f875d152. Paths are under
  `Assets/Scripts/Gameplay/` unless stated.
  - `Utility/Values.cs:8-14` (track length 100 forward, 53.5 back, lane 4.25, fade 20), `:17-20` (lane feedback 15%,
    150 ms), `:23` (miss window 120), `:31-41` (hold flash, alphas, fading holds), `:58` (beat line thickness),
    `:74-82` (camera constants), `:102` (base BPM).
  - `Utility/ArcFormula.cs:30-38` (lane x), `:45-48` (render range), `:66,77` (floor position ↔ z), `:186-194` (tap
    depth growth), `:196-204` (beat line growth), `:206-214` (fade alpha).
  - `Camera/CameraService.cs:61-77` (aspect, reset pose, FOV 50-65), `:194-232` (look-at and chart camera events),
    `:250-273` (arc tilt).
  - `Data/Events/Tap.cs:39,65-69,89-115,138-164`; `Data/Events/Hold.cs:144-205`; `Data/Events/Note.cs:24-25`.
  - `Particle/ParticleService.cs:88-101,163-238,443-463,499-511`; `InputFeedback/InputFeedbackService.cs:20-34`.
  - `Scenecontrol/Controllers/Internal/TrackController.cs` (track parts, lane alphas); `Chart/ChartService.cs:539`.
  - `Assets/Scripts/Settings/Settings.cs:13` (DropRate 150); `Assets/Scripts/Data/Constants.cs:7-9` (scalar 30,
    range 1-10); `Assets/Scripts/Selection/Settings/SettingsDialog.cs:151,171,176`.
  - `Assets/Scenes/Gameplay.unity` (Camera FOV 50 at `:4498`; Track at `:11053-11054,11121`; CriticalLine0-5; Div01-45;
    LaneExtra/EdgeExtra; ComboText; EarlyLateText; LaneHitLayer); `Assets/Materials/Gameplay/Track.mat:60-65`;
    `Assets/Shaders/Track.shader`, `HoldNote.shader:70-86`, `TapNote.shader:69-81`; `Assets/Meshes/Tap.fbx`,
    `Hold.fbx` (vertices parsed by our script); `Assets/Prefabs/Gameplay/TextParticle.prefab`, `LaneHit.prefab`.
- [A2] ArcCore (0thElement), **MIT**, commit 7f63fa08429e670349762d517eeaf0c089e2c73d (2022-02-14):
  https://github.com/0thElement/ArcCore/tree/7f63fa08429e670349762d517eeaf0c089e2c73d.
  `Assets/Scripts/Gameplay/MainHandlers/GameplayCamera.cs:48-72` (same camera, clamped aspect), `:165-175` (tilt,
  "Taken from arcade"); `Assets/Scripts/Utilities/Constants.cs` (lane 4.25, input span ±8.5, windows).
- [A3] ArcaeaSim2 (hozuki / OpenMLTD), **BSD-3-Clause-Clear**, commit 4780dcc5a284837fb17ae5083e7de93304025c2c
  (2018-04-07): https://github.com/hozuki/ArcaeaSim2.
  `ArcaeaView/Subsystems/Scores/Visualization/StageMetrics.cs` (track border 72/1024 "measured from track texture").
- [A4] pycade (Aeterrno-Amora), **no licence; read only**, commit 598a95604235b22668164c27cf30f0e3ac6abb34:
  https://github.com/Aeterrno-Amora/pycade. `aff.py:14` (camera event fields); `coordinate.py:180` (initial camera
  "to be measured").

Official and community:

- [A5] Arcaea fandom wiki, Gameplay: https://arcaea.fandom.com/wiki/Gameplay. Track and lanes, floor and sky input,
  hold rules, combo under the sky input, FR/PM indicator positions, track tilt when holding arcs.
- [A6] Arcaea Wiki* (wikiwiki.jp), ルール: https://wikiwiki.jp/arcaea/%E3%83%AB%E3%83%BC%E3%83%AB. Note speed 1.0-6.5 in
  0.1 steps (56 levels); same speed across charts with exceptions; PURE ±25/50 ms, FAR ±100 ms; EARLY/LATE position
  top/middle/bottom; Pure L/E display; lane tilt with arcs; FR/PM marker; touch effects; colour-blind mode; 120 fps.
- [A7] Arcaea Wiki* (wikiwiki.jp), FAQ: https://wikiwiki.jp/arcaea/FAQ. EARLY/LATE shown above the combo count.
- [A8] nesiddo, 速度変化ギミックの作り方 (note.com): https://note.com/nesiddo/n/nf81263e1d1b1. "Arcaeaのハイスピードは
  BPMと連動している"; notes always come from the far end.
- [A9] r/arcaea, "Any way to make the notes not accelerate" (via a redlib mirror):
  https://lr.sudovanilla.org/r/arcaea/comments/1ghlhd2/any_way_to_make_the_notes_not_accelerate. Perspective, not
  acceleration; no setting; "most players use 4+".
- [A10] Wikipedia, Arcaea: https://en.wikipedia.org/wiki/Arcaea. Developer lowiro; release dates; floor, long, arc
  and sky notes.
- [A11] Arcaea Wiki* (wikiwiki.jp), Nintendo Switch: https://wikiwiki.jp/arcaea/Nintendo%20Switch. Joy-Con split of
  the lanes.

Ours and the existing notes:

- `game-chunithm.md`, `game-sdvx.md`, `game-sekai.md`, `topic-speed.md` (closed forms S1-S5), `topic-covers.md`,
  `topic-wide-keys.md`.
- `osu.Game.Rulesets.ManiaOrtho/Configuration/ManiaOrthoRulesetConfigManager.cs:118-135` (slider ranges);
  `Highway/HighwaySettings.cs` (presets); `UI/HighwayTrack.cs:50-78` (beam).
- Computations: scratch Python scripts outside the repository.
  - A pinhole projection of the rig in [A1]: Unity's vertical FOV, our own clamped lerp for aspect.
  - A binary FBX vertex reader for `Tap.fbx` and `Hold.fbx`.
  - A Unity scene transform reader for `Gameplay.unity`.
