# Sound Voltex: track camera and highway geometry

> **Update (2026-11-07, `topic-speed-scales.md`):** EG's lane speed (1-10) is the older games' BPM × HS over 100;
> by the clones' constant T = 4800 / lane speed ms (the arcade is unmeasured).

Last checked: 2026-11-03

## Summary

Sound Voltex (SDVX) draws one flat highway, seen in perspective by a camera that does not move unless the chart moves
it. The highway has 4 BT lanes in its central two thirds and a laser margin on each side.

**We could not measure an arcade frame.** None of the hosts we found serving SDVX gameplay images could be read from
this environment. The geometry below therefore comes from the two open-source clones, whose code we projected
ourselves (`game-usc.md`):

- **Unnamed SDVX Clone (USC)**, in portrait: a camera its developers tuned to match the arcade. We use it as the
  arcade proxy.
- **USC in landscape**: the same rig, re-aimed for PC windows.
- **K-Shoot MANIA (KSM)**: the classic landscape clone.

All three agree on the look:

| Feature | Value |
|---|---|
| Far end of the track | very narrow, **0.09-0.12 of the near width** (Chunithm 0.24) |
| Track plane vs image plane | about **70-75°** |
| Visible track | 10-13 track widths long |
| Vanishing point | on screen, just above a visible far edge, 26-35% below the top |

**For a landscape window, USC landscape gives:**

| Feature | Value |
|---|---|
| Judgement ("critical") line | **0.941H** |
| Far edge | **0.284H** |
| Whole highway at the line | **0.435W** (BT area 0.290W) |
| Far/near width | **0.106** |

Notes move linearly along the track, so they crawl far away and rush in at the end. A note spends **half its visible
time in the top 10% of the track's screen height**.

The proposed Voltex preset is in *What this means for us* (P1).

## Scope, evidence and versions

**Evidence used, strongest first.**

1. **Open-source code, MIT, projected by us.** USC and KSM v1/v2, read at pinned commits. The method and every
   line reference are in `game-usc.md`. The arcade proxy is USC's portrait camera, which was tuned against the arcade
   in 2019-2020:
   - b1b699d: "better in portrait than in landscape";
   - 6e2af64: "Update critical line position";
   - e33b837: "Adjust camera values to better match SDVX when in portrait mode";
   - d344d56: "Fix default highway pitch in portrait mode";
   - PR #267 claims "almost all aspects of SDVX's camera have been fully replicated" [U2].

   How the static geometry was compared with the arcade is not documented (**unverified**).
2. **Guides and wikis** for options, controls and the cabinet:
   - voltexes.com EG guides [S2][S3];
   - RemyWiki [S1][S7];
   - Wikipedia [S4] and pixiv dictionary [S5];
   - KONAMI's own description [S8].
3. **A Konasute screen-reading tool**, sdvx_helper (Apache-2.0) [S6], for screen orientation and size.
4. **The VOX chart format**, as documented by vox2ksh (MIT) [S9]: only for what the chart-driven camera parameters
   mean. Its evidence folder describes itself as LLM-generated, so it is treated as low confidence.

**Could not be read:**

- konami.com product pages (client error);
- 4gamer.net images (blocked);
- silentblue.remywiki.com (robots);
- giantbomb;
- Wikimedia Commons;
- GitHub issue search;
- GitHub user-attachment images.

No arcade or Konasute gameplay frame was available to measure. **Every arcade geometry number in this note is a
code-derived proxy.**

**Versions.**

- USC's portrait tuning dates from the VIVID WAVE era (2019-2020). `Camera.hpp:3-7` cites "VVD" behaviour.
- The options sources describe EXCEED GEAR (VI, Valkyrie and standard cabinets).
- The current version is ∇ (Nabla) [S10]. vox2ksh reads "NABLA build" DLLs [S9].
- No source found documents a change of highway geometry between versions (**unverified**).

## Camera and track

### The arcade screen

- **Portrait.** Konasute (the PC version) renders the game as a portrait 1080×1920 image. sdvx_helper normalises
  captures to "the 1080×1920 portrait image after rotation". It handles the image upright, or rotated 90° or 270° in
  a 1920×1080 landscape window (`screen_layout.py:1-4`, `define.py:3`, `classes.py:65-71`,
  `direct_window_capture.py:18-19` [S6]).
  - The Konasute EG version renders at 1080p and 120 fps, and asks for a display of at least 1280×720 [S7].
  - The EG arcade monitor runs at 120 Hz, with a touchscreen below it on the Valkyrie model [S1].
  - **SDVX has no landscape layout** (inference: the PC version itself only rotates the portrait image).
- **Direction of travel.** Objects "come from the back toward the front" and are hit "when they overlap the judgement
  line" [S8]. Wikipedia: "the Critical Line at the bottom of the screen" [S4].
- **Screen layout around the track** (USC's portrait skin, copying the arcade):
  - a banner across the top;
  - the track in the middle;
  - an "IO console" panel below the critical line (`gameplay.lua:380-383,632-633` [U1]).
  - On Konasute ∇/EG, the BPM / LANE-SPEED panel sits at the left, at y 420-508 of 1920 (sdvx_helper's play-screen
    detector crop, `screen_layout.py:187` [S6]). That is above the far part of the track.

### Geometry

Code-derived by projecting each clone's default camera, with no chart effects (method in `game-usc.md`). y is from the
top in screen heights H; widths are fractions of screen width W; "track" is the whole highway including the laser
margins.

| Quantity | Arcade proxy: USC portrait, 9:16 | USC landscape, 16:9 | KSM v2/v1, 16:9 | Chunithm, for comparison |
|---|---|---|---|---|
| Judgement line y | 0.707H (console below) | **0.941H** | 0.885H | 0.926H |
| Far edge y | 0.354H | **0.284H** | 0.262H | 0.163H |
| Track width at the line | 0.810 of portrait width | **0.435W** | 0.400W | 0.611W |
| BT-lane area at the line | 0.540 of portrait width | **0.290W** | 0.263W | — |
| Far/near width ratio | 0.089 | **0.106** | 0.121 | 0.241 |
| Vanishing point y | 0.320H | 0.206H | 0.177H | −0.079H |
| Edge lean from vertical | 30.5° | 27.8° | 26.7° | 28.4° |
| Vertical FOV / track-to-image-plane angle | 90° / 70.2° | 60° / 71.25° | 45° / 75° | 27.5° / 74.2° |
| Eye height and distance behind the line (track widths) | 0.80, 0.88 | 0.90, 0.88 | 0.96, 1.50 | 0.89, 1.70 |
| Visible track length (track widths) | 12 | 10 | 12.7 | 6.17 |

**How much of the track is visible.**

- All of it, from the line to a far edge that is a visible, narrow edge against the background. Nothing hides it.
  The track texture is opaque to the end in USC; whether the arcade's lane fades at its far end is **unverified**.
- Below the line, the track continues off the bottom of the screen in landscape. In the arcade, the console sits
  there.
- The visible length is fixed. Speed changes how fast notes move along it, not where they appear.

**The vanishing point is on screen.** In all three SDVX cameras the lane edges, extended, meet a little above the far
edge: 0.21H in USC landscape, 0.32H in portrait. The far edge cannot reach the top of the screen without changing the
camera.

**Depth profile** (USC landscape; d = fraction of the visible time still to go; speed relative to the speed at the
line):

| d | y | Track width | On-screen speed |
|---|---|---|---|
| 0 (line) | 0.941H | 0.435W | 1.00 |
| 0.10 | 0.604H | 0.236W | 0.29 |
| 0.177 | 0.500H | 0.174W | 0.16 |
| 0.25 | 0.442H | 0.140W | 0.10 |
| 0.50 | 0.347H | 0.083W | 0.037 |
| 1.00 (far edge) | 0.284H | 0.046W | 0.011 (1/90) |

- A note is in the lower half of the screen only for the **last 17.8%** of its visible time. In Chunithm it is the
  last 23%.
- Near the line it moves **6.2 H per visible time**, against 3.2 in Chunithm and 1.0 for a flat full-height mania
  stage.
- The arcade proxy is even more compressed (r 0.089): the first half of the visible time is spent between 0.35H and
  0.38H.

### Default camera and chart-driven motion

- **The rest pose is the camera above.** With no chart effects active, the camera is static: zoom, pitch and offset
  are 0, there is no roll and no spin [U1].
- **Everything else is chart-driven** (details in `game-usc.md`):
  - Lane roll toward active lasers, about 10° (×1.75, ×2.5 in the "bigger" and "biggest" modes).
  - Manual tilt.
  - Spins and swings on slams.
  - Slam shake.
  - Lane rotation about the critical line (VOX `CAM_RotX`, KSH `zoom_top`).
  - Zoom toward the line (`CAM_Radi`, `zoom_bottom`).
  - Lane split (`Morphing2`) and lane hiding (`LaneY`) [S9].
- **All of it is out of scope**: SPEC 2 forbids camera motion from beatmap data, and the roll also rotates the
  judgement line, which SPEC 2 never allows.
- **Unit mappings** (low confidence, from vox2ksh's regression) [S9]:
  - `CAM_RotX` 1.0 ≈ `zoom_top` 140, about 21° of lane rotation in KSM (15° per 100).
  - `CAM_Radi` 1.0 ≈ `zoom_bottom` −125.
  - A tilt value of 1 = 10°.

## Note approach and speed

- **Linear in track space.** Both clones place a note at (time to hit) / (visible range) along a straight track, so
  under perspective notes crawl far away and accelerate near the line. Arcade: unverified, but consistent with
  KONAMI's "from the back toward the front" [S8].
- **Clone speed formulas** (`game-usc.md`):
  - USC: visible time T = 480000 / (BPM × hi-speed) ms, "8 beats (2 measures) in view at 1x".
  - KSM: about 448,900 / (BPM × hi-speed) ms.
- **The arcade.**
  - EG renamed the setting "Lane Speed" (レーンスピード), shown to two decimals [S1][S3].
  - RemyWiki: "Unlike in previous installments, the Hi-SPEED value does not pertain to the BPM" [S1].
  - voltexes suggests 3.00-4.00 for beginners and 7.00-8.20 for advanced players [S2].
  - **How lane speed maps to visible time is unverified.** If lane speed is mod-speed / 100, as the advanced range
    suggests (our inference), USC's constant gives 686-585 ms for 7.00-8.20, and KSM's 641-547 ms. That is mania
    speed 16.7-19.6 or 17.9-21.0, by 11485 / T.
- **BPM changes.** Lane speed is BPM-independent in its display [S1], and SDVX charts change scroll speed with BPM and
  with `ManualSpeed` controls [S9]. Whether EG keeps visible time constant at the main BPM is **unverified**. Mania's
  own SV behaviour stays regardless (SPEC 3).

## Judgement line and receptors

- **The critical line** spans the whole highway, laser margins included. USC draws it at the projected line, with
  laser cursors riding on it (`Game.cpp:826-844` [U1]).
- **Position:** 5.9% above the bottom in USC landscape, 11.4% in KSM. In the arcade proxy it is 29% above the bottom,
  with the console panel filling the space below.
- **No on-screen receptors.** The receptors are the physical buttons: 4 BT in a row, 2 FX below them, and 2 knobs
  [S3][S5].

## Lane covers / walls

- **Arcade.** The EG play-options guide lists no lane cover, HIDDEN or SUDDEN option [S2]. This is an unverified
  negative: we could not read the official option list.
- **Charts can hide the lane** with `LaneY` and shade it with `LANECLEARCOL` [S9]. That is chart-driven, so out of
  scope.
- **USC adds its own HIDDEN/SUDDEN.** Off by default; when on, it fades notes over a fraction of the track length and
  keeps speed (`game-usc.md`).
- **Nothing here argues for a wall in the Voltex preset.** Keep wall 0.

## Notes and holds (shapes, rendering)

- **Two chip kinds.** BT chips are white bars one BT lane wide. FX chips are orange bars two BT lanes wide, drawn on
  the FX lanes. Long BT and long FX are held bars [S3][S5].
- **USC's chip** (`game-usc.md`):
  - 0.125 track units deep, 1.25% of the visible track, about **76 px at 1080p at the line**.
  - Its leading edge is on the line at the hit time.
  - It is stretched up to 2× toward the far edge, a USC choice.
- **The arcade's chip depth is unverified.**
- **Lasers and slams** are curves and horizontal bars that run across the full width. Mania has nothing like them.

## Key count and lane layout

**SDVX, every chart, one layout** [S3][S4][S5], `Track.cpp:10-14,428-470` [U1]:

| Lane | Count | Width | Where | Hit with |
|---|---|---|---|---|
| BT | 4 | 1/6 of the highway each | the central 4/6 | 4 white buttons in a row |
| FX | 2 | 2 BT lanes each | FX-L over BT A+B, FX-R over BT C+D | 2 buttons below the BTs |
| Laser | 2 (blue left, red right) | — | anywhere across the full width; the outer 1/6 on each side exists for them | 2 knobs |

Wikipedia calls the whole thing "the course lane, which consists of 6 columns with rails" [S4]. Since III, lasers can
also leave the lane ("レーンからはみ出た") [S5].

- KSM uses the same layout: BT area 0.656 of the highway width (`game-usc.md`).

**How ours differs:**

- osu!mania has N equal lanes, 1K-10K (and dual stages), all hit with keys. It has no FX lanes, no lasers and no
  laser-only margins.
- SDVX's camera never changes with the chart's layout, because there is only one layout.
- 4K mania maps naturally onto the 4 BT lanes. That is what 4K → KSH converters do (inference; not checked).
- 5K-10K have no SDVX counterpart.
- Our track is the lanes and nothing else. The laser margins (1/6 of the SDVX highway on each side) would be pure
  decoration.

## Feedback (judgement display, hit effects, lane lighting)

Not researched beyond what the clone code shows:

- USC shows button hit effects for 4/60 s (BT) and 3/60 s (FX), plus delayed fades (`Track.hpp:6-9` [U1]).
- Key beams rise from the line when a button is pressed (SPEC 5 already asks for this).
- Judgement text and early/late appear near the line.
- EG adds ±ms and EARLY/LATE display options and a judgement-display height option [S2].

A dedicated feedback note would be needed before copying any of this.

## What this means for us

All of this is visual: settings, not mods (invariants 1 and 3). Values are proposals for the main session to record in
`DECISIONS.md`.

### P1. Voltex preset values (SPEC 2)

Copy **USC's landscape camera**:

- It is SDVX's rig, which USC tuned in portrait against the arcade, re-aimed for a landscape window.
- It is MIT and fully derivable.
- KSM agrees with it within about 15% on every parameter except the judgement line.
- The arcade proxy itself (portrait) cannot be used directly. Its line sits 29% up, over a console, and its track
  would be only 0.26W wide if the portrait frame were fitted to a landscape window's height.

| Setting | Proposed | Evidence |
|---|---|---|
| `CameraMode` | **Perspective** | Every SDVX reference draws one perspective plane (all three projected cameras). |
| `FarWidthRatio` | **0.106** | USC landscape 0.106; arcade proxy 0.089; KSM 0.121. All are well below Chunithm's 0.24, which matches SPEC's "narrow far end". |
| `JudgementLineHeight` | **0.059** | USC landscape line at 0.941H; USC's own HUD assumes 0.95 (`gameplay.lua:229`). KSM is 0.114. The arcade's 0.29 only exists because of the console. |
| `FarEdgeHeight` | **0** now; **0.284** if Alpha accepts the faithful edge (Q-V1) | USC landscape far edge 0.284H (KSM 0.262, arcade proxy 0.354). 0 keeps SPEC 2 ("notes enter at the top edge") and matches the Chunithm preset under Q7's default (a). |
| `TrackWidth` | **0.435** for every key count | The whole SDVX highway at the line in USC landscape, 16:9. It also equals SPEC 5's 45% for 4K. The faithful lane-for-lane alternative is in P2. |
| `Wall` | **0** | No arcade lane cover found [S2]. |
| `LaneGuides` | **on** | SDVX draws thin dividers between BT lanes (USC's track texture). |

**What the far-edge choice costs.** Same J, r and width in all three variants; 1920×1080; mania speed 29 = 396 ms, and
640 ms ≈ an SDVX-typical speed by USC's constant:

| Variant | Far edge | Vanishing point | Edge lean | Speed at the line | Time in the lower half | 2.55% note at the line |
|---|---|---|---|---|---|---|
| F, faithful (USC) | 0.284H | 0.206H, on screen | 27.8° | 6.2 H/VT: 15.7 H/s at 396 ms, 9.7 at 640 | last 17.8%: 70 ms at 396, 114 at 640 | 140 px |
| M, VP at the top edge | 0.100H | 0 | 22.3° | 7.9 H/VT: 20.0 H/s, 12.4 | last 10.5%: 41 ms, 67 | 180 px |
| **S, stretched (proposed now)** | 0 | −0.112H | 20.2° | 8.9 H/VT: 22.4 H/s, 13.9 | last 8.5%: 34 ms, 55 | 201 px |
| Chunithm preset now (stretched) | 0 | −0.292H | 24.0° | 3.9 H/VT: 9.7 H/s, 6.0 | last 17.0%: 67 ms, 109 | 98 px |
| Voltex placeholder now (0.3 / 15% / 0 / 0.5) | 0 | −0.364H | 20.1° | 2.8 H/VT | last 17.4% | 74 px |

H/VT means screen heights per visible time.

- At the far edge, a 4K lane is 22 px wide and a 7K lane 13 px, in every variant. At the line they are 209 px and
  119 px.
- Stretching to the top makes the near-line rush **43% faster** than faithful, and flattens the lean from 28° to 20°.
- The faithful edge keeps SDVX's look, with a visible narrow far end in the upper third. It also leaves a band above
  it for the osu! HUD, as Chunithm's 16% does.
- The current placeholder is far from SDVX: three times the far width, and a gentle profile.

### P2. Lanes (SPEC 5)

- **Default:** mania's N equal lanes fill the whole SDVX highway (0.435W).
  - A 4K lane is then 1.5 SDVX BT lanes wide (0.109W, 209 px at 1080p).
  - A 7K lane is 0.062W (119 px).
  - This is one value for every key count, as in the Chunithm preset, and keeps 7K readable.
- **Optional, cosmetic, later: "SDVX rails".**
  - Draw a blue left and a red right side band, each 1/6 of the highway, with the lanes in the central 2/3.
  - For 4K that is exactly SDVX's BT lanes: lanes 0.290W, outer edge 0.435W.
  - 0.290W is also Alpha's flat 4K stage width (0.28W, `baseline-alpha.md`).
  - Off by default. The bands carry nothing; never fake FX lanes or lasers.
- **7K and above.** Under r ≈ 0.1 the outer lanes lean 28° (faithful) and far lanes are 13 px wide. SPEC 5's
  per-key-count relief, for example raising r toward 0.15 at 7K+, should be allowed in this preset, unlike Chunithm's
  fixed field. The value comes from playtests.

### P3. Speed and readability (SPEC 3)

- **Keep SPEC 3 unchanged.** It matches both clones: visible time is the setting, and notes move linearly in track
  space.
- **Playtest at two speeds.** SDVX's compression is about twice Chunithm's near the line.
  - At Alpha's 396 ms, a note is in the lower half for only 34-70 ms, depending on the far-edge variant (P1 table).
  - Compare sheets for the Voltex preset need mania speed 29 (396 ms) **and** about 18 (640 ms, SDVX-typical by
    USC's constant; arcade mapping unverified).
- **Optional hint.** Show "≈ SDVX mod-speed 480000 / T" next to the speed. It is only a hint, since the arcade's own
  constant is unverified.

### P4. Note thickness (SPEC 6)

- **`HighwayNoteLayer.NOTE_THICKNESS` (0.0255, Chunithm's TAP) is too thick for this camera.** The thickness is a
  fraction of the visible track, and SDVX's near track is far more magnified. The same fraction gives 140 px
  (faithful) to 201 px (stretched) at the line, against 81-98 px in Chunithm.
- **Make thickness per preset.** For Voltex use **0.0125**, USC's chip (0.125 of a 10-unit track). That gives 76 px
  (faithful) or 108 px (stretched) at 1080p, Chunithm-sized on screen.
- Alternatively, define thickness as an on-screen size at the line (*tune*).
- Leading edge on the line at the hit time, as now; USC does the same.

### P5. No camera motion (SPEC 2)

- Use only SDVX's rest pose.
- Laser roll, zoom and spin are chart-driven, and the roll also tilts the judgement line, so none of them may be
  implemented.
- A future "kiai sway" (SPEC 2, optional) must not borrow SDVX's roll, since that moves the line.

### Where SPEC.md's assumptions about SDVX hold or fail

1. **"Voltex: strong tilt, narrow far end" (SPEC 2): holds.** The far end is 0.09-0.12 of the near width, and the
   track plane sits 70-75° from the image plane.
2. **Notes enter at the top edge, with the VP at or above the top (SPEC 2): fails for SDVX.** Notes enter at a narrow
   far edge 26-35% below the top, and the VP is on screen. The same conflict as Chunithm's Q7, but stretching costs
   more here (P1).
3. **Typical far/near width 0.3-0.6 (SPEC 2): SDVX is 0.09-0.12.** The range should be 0.08-0.6. The code's
   `MIN_FAR_WIDTH_RATIO` of 0.05 already allows it.
4. **Judgement line at 12% (SPEC 2):** 5.9% in USC landscape and 11.4% in KSM. The arcade's 29% only works because
   of its console.
5. **"Notes move linearly in track space … it is what SDVX does" (SPEC 3): holds** in both clones' code; arcade
   unverified.
6. **4K track about 45% of screen width (SPEC 5): holds** for the whole SDVX highway (0.435W). The BT area alone is
   0.29W.

### Questions for Alpha (for `QUESTIONS.md`)

- **Q-V1.** In the Voltex preset, may notes enter at SDVX's narrow far edge, 28% below the top (faithful, with the
  vanishing point visible just above it)? Or must they enter at the top edge, which makes notes rush 43% faster near
  the line? Same question as Q7 for Chunithm.
  - **Default until answered:** whatever Q7 resolves to; currently (a), far edge at the top.
  - **Recommendation:** faithful (0.284).

## Unverified / open questions

- **Arcade geometry.** No frame was measured. Every arcade number is USC's portrait camera, whose comparison with the
  arcade is undocumented. Measuring one EG or ∇ frame would settle P1. Needed: the line y, the far edge y, the
  near-edge and far-edge widths, from a 1080×1920 capture.
- **Far end.** Whether the arcade lane fades out at its far end, or ends at a hard edge.
- **Lane speed.** How it maps to visible time in EG and ∇, and whether the main BPM is normalised.
- **Lane covers.** Whether the arcade has any HIDDEN/SUDDEN-style option. None is listed in the EG guide.
- **Chip depth** in the arcade.
- **Version changes.** Whether the static geometry changed between versions (II → ∇).
- **The 4K → KSH converter** lane mapping (inference only).
- **The vox2ksh camera constants** (140, −125), which are regression results in a self-described LLM-generated
  document.

## Sources

- [S1] RemyWiki, SOUND VOLTEX EXCEED GEAR: https://remywiki.com/AC_SDVX_EG. 120 Hz monitor; touchscreen below the
  main screen; Hi-SPEED in hundredths; "does not pertain to the BPM"; red/blue display at BPM changes; VALKYRIE EFFECT;
  background opacity option.
- [S2] voltexes.com, EXCEED GEAR play options guide: https://voltexes.com/exceed-gear-play-options-guide/. Option list
  (lane speed 3.00-4.00 beginners, 7.00-8.20 advanced; timing; EARLY/LATE; ±ms; judgement display position; laser
  colours; background brightness; RANDOM/MIRROR); no lane cover listed.
- [S3] voltexes.com, arcade quick start guide (EG): https://voltexes.com/arcade-quick-start-guide-eg/. CRITICAL LINE,
  BT/FX/lasers/slams, lane speed control.
- [S4] Wikipedia, Sound Voltex: https://en.wikipedia.org/wiki/Sound_Voltex. "course lane, which consists of 6 columns
  with rails"; "Critical Line at the bottom of the screen"; FX buttons and notes.
- [S5] pixiv dictionary, SOUND VOLTEX: https://dic.pixiv.net/a/SOUND_VOLTEX. BT 4 in a row, FX 2, analog devices;
  lasers leaving the lane since III.
- [S6] sdvx_helper (dj-kata, Apache-2.0), commit ce8e32f8d81af3fe04d6aeb6b28cfec28cbaedb7:
  https://github.com/dj-kata/sdvx_helper/tree/ce8e32f8d81af3fe04d6aeb6b28cfec28cbaedb7.
  - `src/screen_layout.py:1-4,187`, `src/define.py:3`, `src/classes.py:65-71`, `src/direct_window_capture.py:18-19`:
    portrait 1080×1920 normalisation, rotation, play-screen crop.
  - `resources/onplay1.png` was looked at in the scratchpad only.
- [S7] RemyWiki, SOUND VOLTEX EXCEED GEAR コナステ: https://remywiki.com/CS_SDVX_EG. 1080p, 120 fps, display
  requirements.
- [S8] KONAMI corporate topic 2022-01-19 (10th anniversary): https://www.konami.com/amusement/corporate/ja/topics/20220119/.
  "奥から手前に向かってくるオブジェクトが判定ラインと重なるタイミング".
- [S9] vox2ksh (Tung Vu, MIT), commit 4fe2ac56eb3b493bc3af46e018cc6cd605947ab7:
  https://github.com/whiteou7/vox2ksh/tree/4fe2ac56eb3b493bc3af46e018cc6cd605947ab7.
  - `specs/vox_format.md` (SPCONTROLER: CAM_RotX, CAM_Radi, Tilt, LaneY, Morphing2, ManualSpeed, LANECLEARCOL).
  - `specs/camera.md:15-18,47-55` (zoom constants, tilt = 10° per unit).
  - `specs/evidence/README.md` calls the evidence folder LLM-generated: low confidence.
- [S10] gamebiz, SOUND VOLTEX ∇ launch article: https://gamebiz.jp/news/418213.
- [U1] [U2] [K1] [K2]: USC and K-Shoot MANIA code and PR; see `game-usc.md` for commits and line references.
- `game-chunithm.md` (Chunithm comparison values), `baseline-alpha.md` (Alpha's speed and stage width).
