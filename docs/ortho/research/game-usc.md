# Unnamed SDVX Clone (USC) and K-Shoot MANIA: camera and track code

Last checked: 2026-11-03

## Summary

USC and K-Shoot MANIA (KSM) are the two open-source Sound Voltex clones. Both are MIT. Both draw the SDVX highway as one
textured plane seen by a fixed pinhole camera, and both move notes linearly along that plane with time.

We re-derived each default camera (no chart effects) in a scratch script, from the transforms in the code. The screen
trapezoids are:

| Camera | Judgement line | Far edge | Track width at the line (BT area) | Far/near width | Vanishing point |
|---|---|---|---|---|---|
| USC landscape (16:9, PC default) | 0.941H | 0.284H | 0.435W (0.290W) | 0.106 | 0.206H |
| USC portrait (9:16, tuned to the arcade) | 0.707H | 0.354H | 0.810 of the portrait width (0.540) | 0.089 | 0.320H |
| KSM v2, a port of KSM v1 (16:9) | 0.885H | 0.262H | 0.400W (0.263W) | 0.121 | 0.177H |

y is measured from the top as a fraction of screen height H; widths are fractions of screen width W.

- **The far end is narrow in every case**, about a tenth of the near width; Chunithm's is 0.24 (`game-chunithm.md`).
- **The vanishing point is on screen** in every case, a little above the far edge. Notes enter at a visible far edge
  in the upper third of the screen (landscape) or 35% down (portrait), never at the top edge.
- Neither clone exposes camera settings to the player. All camera motion (zoom, pitch, roll, spin, shake) comes from
  chart data.

`game-sdvx.md` uses these numbers for the Voltex preset.

## Licences and versions read

- **USC**: MIT. Copyright 2016-2018 Guus Waals and 2016-2021 Emil Draws (`LICENSE.md`). Read at commit `6c3a33f`
  (2026-10-24) [U1]. We read the code and re-derived numbers. No code is copied here.
- **KSM v2**: MIT, by masaka and the K-Shoot MANIA developers. Two sound-asset paths are excluded from the licence.
  Read at `37afba9` (2026-11-01) [K1]. It is the official C++ rewrite of KSM v1 and keeps v1's camera, often with
  comments that link the v1 lines.
- **KSM v1**: MIT, © 2020 masaka (HSP source of the 2013-era freeware). Read at `ceb3992` [K2].

## USC: the camera rig

All references are to [U1]. Track units: the whole track is 1 wide (`Track::trackWidth`). One button lane is 1/6 of
that (`Track.cpp:10-14`).

**Matrices.** `Transform` is column-major. `A * B` applies B first. `Rotation({x, 0, 0})` is a right-handed rotation
about +x by x degrees (`Shared/src/Transform.cpp:67-169`). `CreatePerspective(fov, aspect, …)` takes a **vertical**
FOV (`Transform.cpp:5-24`). The camera sits at the origin, and `CreateRenderState` builds a view matrix that is a pure pitch
rotation (`Camera.cpp:357-390`).

**Track placement** (`GetOriginTransform`, `Camera.cpp:87-108`):

- The track is laid on its back: rotated −90° about x.
- It is pushed 0.9 forward, tilted up by 1.5° about the camera, then dropped 0.9 down. Portrait drops it 0.8 instead,
  and adds a 1° pre-rotation there (commit 4ca3559, "reduce rotation radius").
- In track terms, the eye is **0.90 track widths above the plane** and **0.88 behind the judgement line** in landscape.
  In portrait it is 0.80 above and 0.88 behind (our inverse transform).

**Camera pitch** (`Camera.cpp:366-372`):

- First the angle down to the judgement line is computed: 44.25° in landscape, 39.79° in portrait.
- The camera then pitches so that this line sits `fov/2 − fov × pitchOffset` below the view centre.
- The constants are `fovs = {60, 90}` and `pitchOffsets = {0.05, 0.25}` for {landscape, portrait}, with
  `basePitch = 0` (`Camera.hpp:126-131`). Landscape puts the line 27° below centre with a 30° half-FOV; portrait puts
  it 22.5° below with a 45° half-FOV.
- `pitchOffset` is commented "how far from the bottom of the screen should the crit line be", but it is applied as an
  angle. The resulting screen positions are 5.9% (landscape) and 29.3% (portrait) above the bottom, not 5% and 25%.
- The default skin positions its HUD with `critLinePos = {0.95, 0.75}`, which is close to these
  (`bin/skins/Default/scripts/gameplay.lua:229`).
- Relative to the track plane, the view axis points 18.75° down in landscape and 19.79° in portrait. The track plane
  is therefore **71.25°** (landscape) and **70.2°** (portrait) from the image plane.
- `Track.hpp:236-240` still declares older camera fields (`m_basePitch = {−35, −47}`, `m_fov = {70, 90}`). Nothing
  reads them; the live values are in `Camera.hpp`.

**Visible track** (`Track.cpp:16-23,181`):

- `trackLength` is **10** track widths in landscape and **12** in portrait.
- The mesh runs from 1 unit before the line to `trackLength`, so the track continues below the line toward the
  camera, off the bottom of the screen.
- Notes sit at `trackLength × position`, where position is view distance / view range, between 0 and 1
  (`Track.cpp:489`, `Game.cpp:3079-3080`). They appear at the far edge.
- The texture is opaque to the far end (`textures/track.png`, a 1200×150 channel mask; `shaders/track.fs`). In
  landscape the far edge is a visible edge against the background. In portrait the skin draws a banner across the top
  and an "IO console" below the line (`gameplay.lua:380-383,632-633`), copying the arcade's layout.

**Derived screen geometry.** We projected the track corners with the matrices above (our own script, numbers only):

| Quantity | Landscape 16:9 | Portrait 9:16 |
|---|---|---|
| Judgement line y | **0.941H** | 0.707H |
| Far edge y (`trackLength`) | **0.284H** | 0.354H |
| Width at the line: whole track | **0.435W** (0.774H) | 0.810 portrait W (0.456H) |
| Width at the line: 4 BT lanes (4/6) | **0.290W** (0.516H) | 0.540 portrait W |
| Far-edge width | 0.046W | 0.072 portrait W |
| Far/near width ratio r | **0.106** | 0.089 |
| Vanishing point y | 0.206H (on screen, above the far edge) | 0.320H |
| Edge lean from vertical | 27.8° | 30.5° |
| Vertical FOV, track-to-image-plane angle | 60°, 71.25° | 90°, 70.2° |

**Check.** The same projection formula applied to Chunithm's numbers reproduces that note's 28.4° lean
(`game-chunithm.md`).

**Aspect ratio.** The FOV is vertical, so the track keeps its size relative to the screen height. Its fraction of the
screen width changes with the aspect ratio: at 4:3 the whole track is 0.58W.

**How the tuning got there** (git history of `Camera.hpp`/`Camera.cpp`):

| Commit | Change |
|---|---|
| 38d544b, 2018-06-15, "Better camera (#71)" | FOV {70, 90} became {60, 90}; min/max pitch replaced by `basePitch 0` |
| b1b699d, 2019-03-02 | "Update how the highway looks. This is better in portrait than in landscape, will need adjusting." |
| 6e2af64, 2019-03-31, "Update critical line position" | Portrait offset 0.27 → 0.25 |
| e33b837, 2020-02-01, "Adjust camera values to better match SDVX when in portrait mode" | Portrait anchor −0.8 → −0.775 |
| d344d56, 2020-06-18, "Fix default highway pitch in portrait mode" | Anchor back to −0.8 |

The author of the 2020 work states in PR #267 that "almost all aspects of SDVX's camera have been fully replicated",
with roll speed and angle matched "close enough" [U2]. That PR is about roll and tilt. No commit says how the static
geometry was compared with the arcade (unverified).

## USC: lanes, notes, speed, covers

**Lanes** (`Track.cpp:10-14,428-470`):

| Lane | Width (track units) | Position |
|---|---|---|
| BT A-D | 1/6 each | the central 4/6 |
| FX L, FX R | 2/6 each | FX L over BT A+B, FX R over BT C+D |
| Lasers (2) | — | the full width; cursor centres travel ±5/12 (`Game.cpp:832`), so each laser reaches into a 1/6 margin outside the BT area |

**Chips** (`Track.cpp:131-132,518-531`; `textures/button.png` is 64×48 and fully opaque):

- A BT chip is 1/6 wide and **0.125** long in track units: 1.25% of the visible track at the line.
- Chips are lengthened linearly with position up to `distantButtonScale = 2.0` at the far edge (`Track.hpp:80`), so
  they stay visible far away.
- The chip runs from its time position away from the camera. Its **leading edge crosses the line at the hit time**,
  the same as mania.
- On screen, in landscape at 1080p, a chip whose leading edge is on the line is about **76 px** tall (our exact
  projection). A 2.55%-deep Chunithm TAP placed the same way in Chunithm's geometry is 81 px; `game-chunithm.md`
  gives 87 px for the real mesh, which straddles the line.

**Speed** (`Game.cpp:866-875`):

- In hi-speed mode the view range is `8 × (1 / hispeed)` beats ("8 beats (2 measures) in view at 1x hi-speed").
  That gives visible time **T = 480000 / (BPM × hispeed) ms**: for example 686 ms at mod-speed 700.
- In C-mod, distance is time / 480000 (`BeatmapPlayback.cpp:514-524`), which gives the same T = 480000 / cmod.
- Notes move linearly in track space, so on screen they crawl far away and accelerate near the line.
- Defaults are hispeed 1.0 and mod-speed 300 (`GameConfig.cpp:81,88`).

**Covers** (`GameConfig.cpp:95-99`, `Game.cpp:510-529`, `Track.hpp:145-149`, `shaders/button.fs:57-60`):

- `EnableHiddenSudden` is off by default.
- When on, a sudden cut-off (default 1.0, meaning no cover) and a hidden cut-off (default 0) fade notes over a 0.2
  window, both measured in fractions of the track length.
- The fade applies to notes only. Speed is unchanged, so the cover shortens visible time.
- This is a USC feature; see `game-sdvx.md` for the arcade.

## USC: chart-driven camera (out of scope for us, recorded for completeness)

SPEC 2 forbids camera motion driven by beatmap data. These effects explain what SDVX's "lane zoom/tilt" is, and why
our preset uses only the rest pose. Values come from KSH files divided by 100 (`BeatmapFromKSH.cpp:816-829`). All are 0
by default (`BeatmapPlayback.cpp:534-560`).

| KSH key | USC field | Effect |
|---|---|---|
| `zoom_bottom` | `pLaneZoom` | Moves the camera along the line from the eye to the judgement line. The offset is `1.65^(−z) − 1` track widths for z ≤ 0, and `distance × (1.65^(−z^1.35) − 1)` for z > 0, approaching the line asymptotically (`Camera.cpp:6,241-252`). |
| `zoom_top` | `pLanePitch` | Rotates the track about the judgement line by `PitchScaleFunc(z) × 15°`, or 7° per unit for charts before KSH version 168 (`Camera.hpp:18-19,124`). `PitchScaleFunc` is a piecewise-linear remap with knots at −4 and +5.59 units, different for landscape and portrait (`Camera.cpp:40-84,236`). |
| `zoom_side` | `pLaneOffset` | Shifts the track sideways by `z × 500/696 / 2` track widths (`Camera.cpp:219`). |
| laser positions | roll | Rolls the track toward the active lasers, up to `MAX_ROLL_ANGLE` = 10° (×1.75 "bigger", ×2.5 "biggest"), with SDVX-like roll speeds, slow tilt and "roll keep" (`Camera.hpp:21`, `Camera.cpp:110-176`). |
| `tilt` | manual roll | One unit is 10° (`BeatmapFromKSH.cpp:854-885`). |
| spins, swings | — | Spins (`Camera.cpp:24-38,186-216`) and swings. |
| slams | shake | Slams shake the camera sideways (`Camera.cpp:221-234`). |

## K-Shoot MANIA: the camera

From KSM v2 [K1] and v1 [K2]:

- **Camera.** It is a Siv3D `BasicCamera3D` with a **vertical FOV of 45°**. The eye is at (0, 45, −366), looking
  **15° down** (`GraphicsMain.cpp:37-42`). This is v1's camera, pitched by −π/12 and placed at (…, −45, 366) in
  v1's axes (`play_graphics.hsp:243-251`).
- **Base screen.** It is 640×480 (`ScreenUtils.hpp:8-9`). The 3D camera uses the real window aspect, so the track keeps
  its size relative to the screen height.
- **Plane** (`GraphicsDefines.hpp:12-26`, `Highway3DGraphics.cpp:13-19,54,87-101`):
  - The highway is one flat plane. It is 46.8 units wide for the 256 px track texture, and 93.5 units in the
    "wide-laser" mesh.
  - Along the track it runs from z −304 to +300.
  - The judgement line is 14 px from the bottom of the 1024 px texture, which puts it at z = −296.
  - BT lanes are 42 px of the 256 px texture, starting at 44 px, so the BT area is **0.656** of the track width. USC
    uses 4/6.
- **Derived geometry** (our projection):
  - At 4:3: judgement line **0.886H**, far edge **0.262H**, track width at the line 0.534W (0.712H).
  - At 16:9 the track is **0.400W**, with the BT area at 0.263W.
  - Far/near width **0.121**, vanishing point 0.177H, edge lean 26.7°.
  - The track plane is 75° from the image plane.
  - The eye is 0.96 track widths high and 1.50 behind the line, and the visible track is 12.7 track widths long.
- **Speed** (`HighwayScroll.cpp:11-16,36-50,317-322`): 540 px per measure at hi-speed factor 1, with factor =
  mod-speed / BPM. The track has 1010 px above the line, which gives **T ≈ 448,900 / (BPM × hispeed) ms**: about 7%
  shorter than USC's at the same number.
- **Chart camera.** `zoom_top` rotates the plane about the judgement line by z × 360/2400 degrees (15° per 100), and
  `zoom_bottom` moves it toward the camera, scaled by 1/450 (positive) or 1/180 (negative)
  (`Highway3DGraphics.cpp:87-101`, `CameraMath.hpp:7-18`). Out of scope, as for USC.

## What this means for us

- **Use USC landscape as the Voltex preset's geometry source** (proposal and reasoning in `game-sdvx.md`). It is the
  arcade's camera rig, which USC tuned in portrait, re-aimed for a landscape window; it is MIT; and KSM agrees with it
  within about 15% on every parameter except the judgement line.
- **Planar homography is enough.** Both clones render one flat plane with a pinhole camera, and their camera effects
  only move that plane. Our `HighwayProjection` model (four screen numbers) reproduces either clone's default view
  exactly. Tilt and FOV only matter for things that stand off the plane.
- **Speed definition.** Both clones scale visible time with BPM (hi-speed × BPM), as mania's default does. Show
  "≈ SDVX mod-speed 480000 / T" only as a hint; the arcade's own constant is unverified.
- **Chip depth.** USC's chip is 1.25% of the visible track at the line. Because SDVX's track is so compressed, that
  is about the same on-screen size as Chunithm's 2.55% TAP (76 px vs 81 px at 1080p). Our fixed 2.55%
  (`HighwayNoteLayer.NOTE_THICKNESS`) would be 140 px under the SDVX camera; see `game-sdvx.md` P4. USC's 2× stretch
  toward the far end is a readability trick we could copy for very compressed presets (optional, *tune*).

## Sources

- [U1] Unnamed SDVX Clone, MIT, commit 6c3a33f9c66460f13934df034dca7ae3f9a130f9:
  https://github.com/Drewol/unnamed-sdvx-clone/tree/6c3a33f9c66460f13934df034dca7ae3f9a130f9.
  - `Main/include/Camera.hpp` (camera constants), `Main/src/Camera.cpp` (rig, zoom, pitch, roll),
    `Main/include/Track.hpp`, `Main/src/Track.cpp` (sizes, view range, notes).
  - `Main/src/Game.cpp` (speed, laser cursors, covers), `Main/src/GameConfig.cpp` (defaults),
    `Shared/src/Transform.cpp` (matrix conventions), `Beatmap/src/BeatmapFromKSH.cpp`,
    `Beatmap/src/BeatmapPlayback.cpp`.
  - Default skin: `bin/skins/Default/scripts/gameplay.lua`, `textures/`, `shaders/`.
  - History: commits 38d544b, b1b699d, 6e2af64, e33b837, d344d56, 4ca3559.
- [U2] USC PR #267 "Camera Changes" (ASleepyCat, 2020): https://github.com/Drewol/unnamed-sdvx-clone/pull/267. Roll
  and tilt matched to SDVX "close enough"; "almost all aspects of SDVX's camera have been fully replicated".
- [K1] K-Shoot MANIA v2, MIT, commit 37afba946f9bf48c0edea15bffc582b0e04ec685:
  https://github.com/kshootmania/ksm-v2/tree/37afba946f9bf48c0edea15bffc582b0e04ec685.
  - `kshootmania/src/MusicGame/Graphics/GraphicsMain.cpp`, `Graphics/GraphicsDefines.hpp`,
    `Graphics/Highway/Highway3DGraphics.cpp`.
  - `Camera/CameraMath.hpp`, `Scroll/HighwayScroll.cpp`, `kshootmania/src/Graphics/ScreenUtils.hpp`.
- [K2] K-Shoot MANIA v1 (HSP), MIT, commit ceb3992c8fac3b141031731926346d9d84138c37:
  https://github.com/kshootmania/ksm-v1/blob/ceb3992c8fac3b141031731926346d9d84138c37/src/scene/play/play_graphics.hsp#L243-L251
  (`initCamera`).
- Method: the transforms above were re-implemented in a throwaway Python script outside the repository. It projected
  the track corners and the judgement line; every number in the derived tables comes from it.
