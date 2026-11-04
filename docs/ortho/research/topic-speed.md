# Speed under perspective

Last checked: 2026-11-04

## Summary

**How the games define speed.** Every perspective game checked defines speed in **track space**: as a visible time,
as track units per second, or as a beat multiplier. The camera then does the rest.

- None of them corrects on-screen speed for the perspective.
- None changes the speed number when the camera changes:
  - YARG keeps units/s and the visible length across camera presets, so visible time stays the same;
  - StepMania keeps field pixels per beat, but draws more field when tilted, so Hallway shows notes 38% longer
    (`game-stepmania-etterna.md`);
  - Chunithm and SDVX have one fixed camera each.
- So SPEC 3's "speed = visible time, linear in track depth" is the reference behaviour, with YARG's rule for camera
  changes.

**The one exception is the approach curve.** The Sonolus Project Sekai engine (MIT, a fan reimplementation) moves notes
**exponentially in screen scale**: y = 1.06^(45·(u − 1)), where u is the fraction of the approach elapsed.

- With linear depth, a note's on-screen speed at the line is 189× its speed at the far end, for Sekai's far/near ratio.
- With Sekai's curve it is 13.8×.
- Notes therefore spend more of their time near the line and rush far less.

**What players feel is the speed near the line.** For a plane homography with far/near width ratio r, that speed is
**1/r track heights per visible time**. That is 4.2× flat mania for Chunithm (r = 0.24) and 9.4× for USC (r = 0.106).
Players of perspective games compensate by choosing longer visible times than flat-game players:

| Game | Typical visible time | Source |
|---|---|---|
| Chunithm | about 645-731 ms | `game-chunithm.md` |
| SDVX | about 585-686 ms (inferred) | `game-sdvx.md` |
| IIDX (flat) | 417-550 ms | `game-iidx.md` |
| Alpha, flat mania | speed 29: labelled 396 ms; real 405 ms with argon, 473 ms with his own 4K skin | `baseline-alpha.md`, `game-osu-mania.md` |

The calculations below use 396 ms as a round reference visible time. In lazer, speed 29 is that only with a legacy
default hit position.

## Camera and track

Geometry per game is in `game-chunithm.md`, `game-sdvx.md` and `game-usc.md`. New numbers in this note (our own
projections, scratch scripts outside the repository):

**YARG default camera.** `game-yarg-clonehero.md` has the full projection: strikeline 0.827H, far end 0.409H,
far/near 0.45, vanishing point 0.070H, lean 18.9°. Our own check of the same transforms [Y1][Y2] agrees: r = 0.448.

**Sonolus Sekai engine** (16:9, stage aspect locked) [P1]:

| Quantity | Value |
|---|---|
| Vanishing point | 4.6% of H above the top edge |
| Lane far end | exactly at the top edge (`lane.t` = 47/850) |
| Judgement line | 0.791H |
| Notes appear at | 0.015H, at 7.3% of the line width |
| Lane width at the line | 0.787W |
| Edge lean | 39.9° |

Computed from `Initialization.ts:24-37` and `lane.ts`. Whether these constants match the official game exactly is
unverified; they look derived from its layout (1176-px lane art).

## Note approach and speed

### How each game defines speed

| Game | Setting | Meaning | Visible time | BPM | Track or screen space | Source |
|---|---|---|---|---|---|---|
| osu!mania (lazer) | Scroll speed 1-40 (default 8), F3/F4 ±1 | time range | T = (11485 / speed) × (768 − H) / 643.2 ms: argon +2.3% over the label | relative to the most common beat length; BPM changes scale it (Sequential) | flat | `game-osu-mania.md` |
| beatmania IIDX | NHS 1-20 / CHS / FHS; green number | GN = visible time in 1/600 s | T = 291,213 / (BPM × HS) × visible fraction ms | FHS fixed at the start BPM; changes with BPM | flat | `game-iidx.md` |
| Chunithm | speed 1.00-15.00, 16-19, SONIC | visible time | T = 5486 / s ms | none ("BPM basically does not affect scroll speed") | **track** (constant world speed) | `game-chunithm.md` |
| SDVX | III: Hi-speed, saved as Hi-speed × BPM; EG: lane speed | visible time (mapping unverified) | — | III: BPM × HS; EG: BPM-independent | track (clones) | `game-sdvx.md` |
| USC / KSM | hi-speed or mod-speed | beats in view | T = 480,000 / (BPM × HS); KSM about 448,900 / (BPM × HS) | mod-speed normalised | **track** | `game-usc.md` |
| Project Sekai (Sonolus) | note speed 1-12, default 6, 0.01 steps | approach duration | T = lerp(350, 4000, ((12 − s)/11)^1.31) ms | none (scaled time only) | **exponential in screen scale** | [P1] |
| YARG | note speed (units/s, code default 6; new profiles 5) and highway length (default 1) | track units per second | T = 5 × length / speed s: 833 ms or 1000 ms; slower on lower difficulties by default | none | **track** | `game-yarg-clonehero.md`, [Y1][Y2] |
| ArcCreate (Arcaea editor/player, GPL-3.0) | drop rate (default 150) | z = time × rate / 1000 at the base BPM | 100 track units forward, so T = 100,000 / rate ms; default 667 ms | relative to the chart's base BPM | **track** | [A1] |
| StepMania / Etterna | x-mod, C-mod, M-mod | field pixels: 64 px per beat × x; C: 64 px × C/60 per s | visible field px / speed: Overhead 360/C s, Hallway 38% longer | x: BPM; C: none; M: max BPM | flat field, then rotated (**track**) | `game-stepmania-etterna.md`, [SM1] |
| Quaver | scroll speed 5.0-100.0 (default 15.0), F3/F4 ±1.0, small ±0.1 | px per ms = (S/10) / (20 × rate) × scaling | lane px / speed | none | flat | [Q1] |

Sekai durations, from the Sonolus formula:

| Note speed | 6 | 8 | 9 | 10 | 10.5 | 11 | 11.5 | 12 |
|---|---|---|---|---|---|---|---|---|
| ms | 2000 | 1320 | 1015 | 741 | 618 | 508 | 414 | 350 |
| Mania speed with the same visible time (label, 11485 / T; argon needs about 2.3% more) | 5.7 | 8.7 | 11.3 | 15.5 | 18.6 | 22.6 | 27.8 | 32.8 |

The official game's range and typical speeds are unverified.

### How the camera changes the feel

**Linear depth under a plane homography.** Take a track whose far/near width ratio is r, with notes moving linearly in
depth, which is what Chunithm, USC, KSM, YARG, StepMania tilt, ArcCreate and our ruleset do. Let p be the position as
a fraction of the track's on-screen height. Then, exactly:

- p(d) = (1/(1 + k·d) − r) / (1 − r), where k = 1/r − 1 and d is the fraction of the visible time still to go;
- share of the visible time in the **near half** of the on-screen track = **r / (1 + r)**;
- on-screen speed **at the line** = **1/r** track heights per visible time;
- speed at the far end = r, so the line/far speed ratio is **1/r²**.

| Camera | r | Near half | Speed at the line (track heights / T) | Line / far |
|---|---|---|---|---|
| Flat (mania, IIDX) | 1 | 50% | 1.0 | 1 |
| YARG default | 0.448 | 30.9% | 2.2 | 5 |
| **Chunithm** (our default preset) | 0.24 | 19.4% | 4.2 | 17 |
| KSM v2 | 0.121 | 10.8% | 8.3 | 68 |
| USC landscape (our Voltex preset) | 0.106 | 9.6% | 9.4 | 89 |
| USC portrait (arcade proxy) | 0.089 | 8.2% | 11.2 | 126 |
| Sekai far ratio, *if* linear | 0.073 | 6.8% | 13.8 | 189 |
| **Sekai (Sonolus), exponential** | 0.073 at spawn | **23.8%** | 2.8 | 13.8 |

The first rows match the existing notes: Chunithm 17× (`game-chunithm.md`), USC 1/90 (`game-sdvx.md`).

**In screen heights per second at a 396 ms visible time:**

| Camera | Speed at the line |
|---|---|
| Flat full-height mania | 2.5 H/s |
| Our Chunithm preset (track 0.926H) | 9.7 H/s |
| Chunithm arcade (track 0.763H) | 8.0 H/s |
| Sekai (Sonolus) | 5.5 H/s |
| Our Voltex preset, far edge stretched to the top | 22.4 H/s |
| USC landscape (faithful far edge) | 15.7 H/s |

To give our Chunithm preset flat mania's near-line speed at 396 ms, the visible time would have to be 1.53 s (mania
speed 7.5). Perspective games accept the rush instead, and their players pick longer visible times.

**Sekai's curve** [P1]:

- `approach(from, to, now) = 1.06^(45 × remap(from, to, −1, 0, now))` scales each note's layout about the vanishing
  point (`note.ts:5-6`, `FlatNote.ts:164`).
- Screen distance from the vanishing point is therefore exponential in time. In world terms, depth shrinks
  geometrically, fast far away and slow near the line.
- Position profile, as a fraction of H from the top:

| Time still to go | 0 | 10% | 25% | 50% | 75% | 100% |
|---|---|---|---|---|---|---|
| Sekai curve | 0.791 | 0.598 | 0.389 | 0.179 | 0.071 | 0.015 |
| Linear depth, same screen | 0.791 | 0.322 | 0.154 | 0.067 | 0.033 | 0.015 |

- The note's quad scales with the same factor, so it thickens toward the line.
- Its centre, not its leading edge, reaches the line at the hit time.

**StepMania's perspective mods** (full geometry in `game-stepmania-etterna.md`) [SM1]:

- The field is drawn with a 45° FOV perspective (`Player.cpp:1689`) and rotated ∓30° per unit of tilt
  (`Player.cpp:1707-1739`).
- Note offsets stay in field pixels, 64 px per beat (`ArrowEffects.cpp:470-519`), so **the speed number is kept**.
- The draw distance grows with tilt: `NoteField.cpp:765` multiplies it by (1 + 0.5 × |tilt|), and the field height
  grows by 200 px × |tilt| (`ArrowEffects.cpp:90-93`). So **visible time is not kept**: Hallway shows notes 38% longer
  at the same C-mod (`game-stepmania-etterna.md`).
- SM is a "keep speed when the camera changes" design. We keep visible time instead (S1).

**YARG** (details in `game-yarg-clonehero.md`) [Y1][Y2]:

- Notes sit at z = −2 + (time − now) × note speed (`TrackElement.cs:32-52`).
- Changing the camera preset (FOV, height, rotation) leaves note speed and the visible z range alone, so visible time
  is kept.
- A longer highway keeps units/s, so visible time grows with it.
- Lower difficulties scroll slower by default ("Reduce note speed by difficulty").

### Display

- Chunithm shows a speed number.
- IIDX shows the green number (time) while START is held.
- Etterna's Til' Death shows an effective speed when the cover moves (`topic-covers.md`).
- Quaver and osu! show toasts with the speed number; osu!'s tooltip also gives the ms.
- No game shows an on-screen speed in H/s.

## Judgement line and receptors, covers, notes, feedback

Covers are in `topic-covers.md`; the rest does not concern speed.

## What this means for us

**S1. Keep SPEC 3 (visible time, linear in track depth) as the speed definition. It matches every reference.**

- Chunithm, SDVX's clones, YARG, StepMania and ArcCreate all move notes linearly in track space and leave the camera
  alone.
- Switching preset at the same visible time changes near-line speed by the ratio of 1/r × track height. Chunithm to
  Voltex is 2.3× at our current stretched geometry.
- When the camera changes, YARG keeps visible time as we do. StepMania keeps field speed and lets visible time grow
  with tilt (+38% for Hallway), which would break SPEC 3's "a speed feels the same in every camera mode".
- **No change.**

**S2. Do not auto-compensate for perspective.**

- No reference does it.
- Matching flat mania's near-line speed would mean 1.5 s of visible time for the Chunithm preset at a 396 ms visible time.
  Notes would crawl at the top for over a second.
- A "keep near-line speed when changing camera" option would be our own invention. **Not proposed.** Revisit only if
  Alpha's L4 playtest asks for it.

**S3. Tell the player what perspective does, in numbers.**

- **Proposal:** the scroll-speed or camera settings hint shows "near the line ≈ N× flat speed", where
  N = track height / r relative to the flat stage. That is about 3.9× for the Chunithm preset and 8.9× for Voltex as
  stretched now.
- Keep game-chunithm's "≈ CHUNITHM s" hint (s = 5486 / T).
- **Playtest speeds** (real visible times with argon, from `game-osu-mania.md`):

| Mania speed | Visible time | Matches |
|---|---|---|
| 29 | 405 ms | Alpha's speed |
| 21.4 | about 550 ms | IIDX GN 330 |
| 16.7 | 704 ms | about Chunithm 7.8 |

  State the skin with every capture, since Alpha's own 4K skin makes speed 29 read 473 ms.

**S4. Optional "Approach curve: Linear / Eased" setting, later, off by default.**

- Sekai's exponential approach, if the Sonolus engine is faithful, makes notes spend 23.8% of their time in the near
  half of the track and cuts the line/far speed ratio from 189 to 14 at the same far ratio.
- It is a readability tool for strong perspective (Voltex), not the Chunithm or SDVX look, so **not the default**.
- **Implementation as a pure depth remap** after mania's scroll algorithm:
  - d' = (r^(−d) − 1) / k with k = 1/r − 1. This makes the screen scale exponential in d.
  - For r = 0.24: d = 0.25 → 0.135, 0.5 → 0.329, 0.75 → 0.605.
  - d' = 0 at the line, so notes still cross the line at their hit time.
  - Holds stay aligned, because head and tail go through the same function.
  - The remap applies after mania's own positions, so SV still behaves as in mania.
- Visible only: no input or judgement path changes. It still needs an L0 test (monotonic, d' = 0 at 0 and 1 at 1) and
  a parity run like every other setting.
- Note thickness would follow the remapped depth.

**S5. BPM: keep mania's behaviour.**

- Lazer mania scales relative to the most common beat length. That is the same as beatoraja's default (main BPM) and
  close to IIDX FHS (start BPM).
- Chunithm, Sekai, YARG and Quaver ignore BPM. Mania's Constant Speed mod already offers that.
- SPEC 3 requires mania's SV algorithms. **No change.**

**S6. Wall and lift (SPEC 4).**

- In our default mode, a wall at a hides the far a of the depth, and notes cover the rest in the same time: track speed
  becomes (1 − a)/T, so near-line speed drops by (1 − a). Wall 40% gives 0.6×.
- That is exactly what Chunithm players do by hand (raise the wall, lower the speed) and what IIDX FHS does
  automatically.
- The lift does the same, as in beatoraja. **No change.** See `topic-covers.md` C3 for showing the resulting ms.

## Unverified

- Sekai: whether the official game uses the Sonolus engine's curve (1.06^45) and layout, and its speed range and
  typical speeds.
- SDVX EG: how lane speed maps to visible time.
- Arcaea: the official speed to drop-rate mapping (ArcCreate's default 150 = 667 ms is the clone's).
- Clone Hero: closed source; not checked.
- YARG's `CurveFactor` bend is ignored in the r = 0.448 used here (`game-yarg-clonehero.md` measures it).

## Sources

- osu! (this repo, MIT): `osu.Game.Rulesets.Mania/UI/DrawableManiaRuleset.cs:36-43,53,167-183`;
  `osu.Game/Rulesets/UI/Scrolling/DrawableScrollingRuleset.cs:111-118,206-223`.
- Existing notes:
  - `game-chunithm.md` (speed table, wall semantics);
  - `game-sdvx.md`, `game-usc.md` (USC/KSM formulas and cameras);
  - `game-iidx.md` (green number);
  - `game-osu-mania.md` (real visible time, F3/F4);
  - `game-stepmania-etterna.md` (tilt geometry, draw distance, visible time under tilt);
  - `game-yarg-clonehero.md` (camera presets, note speed defaults);
  - `topic-covers.md`.
- [P1] sonolus-pjsekai-engine, **MIT**, commit 0f6f37fc9b512b9d156ec4c00d998fe3916b157d:
  https://github.com/NonSpicyBurrito/sonolus-pjsekai-engine/tree/0f6f37fc9b512b9d156ec4c00d998fe3916b157d.
  - `play/src/engine/playData/note.ts:7-9` (duration);
  - `shared/src/engine/data/note.ts:5-6` (approach);
  - `shared/src/engine/configuration/options.ts:26-34` (note speed 1-12, default 6);
  - `play/src/engine/playData/archetypes/Initialization.ts:24-37` (stage transform);
  - `shared/src/engine/data/lane.ts:1-4`, `shared/src/engine/data/utils.ts` (`perspectiveLayout`: x scaled by y);
  - `play/src/engine/playData/archetypes/notes/flatNotes/FlatNote.ts:164`.
- [Y1] YARG, **LGPL-3.0, read only**, commit f5bfbe6996b4af35722d4204a93c6cc5c8f52a45:
  https://github.com/YARC-Official/YARG/tree/f5bfbe6996b4af35722d4204a93c6cc5c8f52a45.
  - `Assets/Script/Gameplay/Player/TrackPlayer.cs:24-32,110-117`;
  - `Assets/Script/Gameplay/Player/BasePlayer.cs:27-46`;
  - `Assets/Script/Gameplay/Visuals/TrackElements/TrackElement.cs:32-52`;
  - `Assets/Script/Gameplay/Visuals/CameraPositioner.cs:69-82`.
- [Y2] YARG.Core, **LGPL-3.0, read only**, commit 2f3900cf91aa141149d60cb122e88876e1709abb:
  https://github.com/YARC-Official/YARG.Core/tree/2f3900cf91aa141149d60cb122e88876e1709abb.
  - `YARG.Core/Game/YargProfile.cs:34-35,174-175` (NoteSpeed 6, HighwayLength 1);
  - `YARG.Core/Game/Presets/CameraPreset.cs:9-36` (FOV 55, Y 2.66, Z 1.14, rotation 24.12, fade 1.25, curve 0.5,
    orthographic option).
- [A1] ArcCreate (Arcthesia), **GPL-3.0, read only**, commit a00f94ca602b990bc15d7ca530434da9f875d152:
  https://github.com/Arcthesia/ArcCreate/tree/a00f94ca602b990bc15d7ca530434da9f875d152.
  - `Assets/Scripts/Gameplay/Utility/ArcFormula.cs:66,77` (floor position ↔ z);
  - `Assets/Scripts/Gameplay/Utility/Values.cs:8,102` (track length 100 forward, base BPM);
  - `Assets/Scripts/Settings/Settings.cs:13` (DropRate 150).
- [SM1] StepMania, MIT-style licence (`Docs/Licenses.txt`), commit 825467bcd81c812b33ad684dc04dd151b2d5dec3:
  https://github.com/stepmania/stepmania/tree/825467bcd81c812b33ad684dc04dd151b2d5dec3.
  - `src/Player.cpp:1685-1739` (45° FOV, tilt ±30°, zoom, y offset);
  - `src/PlayerOptions.cpp:1105-1115` (Overhead, Incoming, Space, Hallway, Distant);
  - `src/ArrowEffects.cpp:470-519` (x/C/M mods, 64 px per beat), `:90-93` (field height + 200 px × |tilt|);
  - `src/NoteField.cpp:765-766` (draw distance × (1 + 0.5 |tilt|) × (1 + |mini|));
  - `Themes/_fallback/metrics.ini:196,1142-1143` (ArrowSpacing 64, draw distance).
- [Q1] Quaver, **MPL-2.0**, commit e9b54a66:
  - `Quaver.Shared/Config/ConfigManager.cs:1201-1202,1353` (F3/F4; ScrollSpeed 150, 50-1000);
  - `Quaver.Shared/Input/Global/GlobalInputHandler.cs:153-196` (±10 or ±1, local with Shift);
  - `Quaver.Shared/Screens/Gameplay/GameplayScreen.cs:724-733` (changes only before 5 s, in breaks or when skippable);
  - `Quaver.Shared/Screens/Gameplay/Rulesets/Keys/HitObjects/TimingGroupControllerKeys.cs:59-71` (px per ms).
- Metrics: the closed forms above are derived from the plane homography used in `HighwayProjection` (DECISIONS,
  "Camera is set in screen terms"). Table values come from a scratch Python script outside the repository.
