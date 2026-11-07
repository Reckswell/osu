# Chunithm: viewfield, camera and track geometry

Last checked: 2026-11-03

## Summary

Chunithm draws a **true planar perspective** track: one 16-cell plane seen by a fixed look-at camera. Three
independent sources agree on its geometry to within about 0.3% of screen height:

- a SUN screenshot we measured;
- SEGA's official play-guide image;
- camera and option tables decoded from the current executable by a fan project.

At 16:9 the geometry is:

| Feature | Value |
|---|---|
| Judgement line | **0.926H** from the top (7.4% above the bottom) |
| Inner track width at the line | **0.611W**, centred |
| Vanishing point | **0.079H above** the top edge |
| Far edge | **0.163H** from the top, which is exactly the bottom of the top-centre HUD frame |
| Far/near width ratio | **0.24** |

Speed number *s* maps to visible time **T = 5486 / s ms** (s ≤ 15). The default 6.0 is 914 ms, a common MASTER speed of
8.0 is 686 ms, and BPM does not change scroll speed.

The **field wall** (AMAZON PLUS and later, 0-16, default 0) is an opaque vertical panel standing on the track. It
**keeps note speed**, so visible time shrinks as it moves. It hides 5.8% of the track per step up to step 11.

A TAP is 10.1 world units deep, **2.55% of the visible track length**. That is about 8% of screen height at the
judgement line and 5 px at the far edge (1080p).

Several of SPEC.md's assumptions about Chunithm need correcting; see the end of *What this means for us*:

- the far end is narrower than SPEC's range, not "wider";
- the wall is off by default, not "on";
- the judgement line sits at 7.4%, not 12%;
- notes appear at 16% below the top, not at the top edge;
- the wall keeps speed.

Animations, judgement text, combo and hit effects are in `game-chunithm-feedback.md` and are not repeated here.

Conventions in this note:

- y is measured from the top of the screen as a fraction of screen height H.
- x is a fraction of screen width W.
- "px" means pixels of a 1920×1080 frame unless stated otherwise.
- "units" are Chunithm world units: the 16-cell inner track is 64 units wide, so one cell is 4 units.

## Scope, evidence and versions

**Evidence used, strongest first.**

1. **Pixel measurements of real game frames (observation).** We loaded each PNG with PIL. We located track edges
   and guide lines as intensity peaks along image rows, then fitted straight lines to them.
   - The chunithm.org SUN-era Japanese 1920×1080 screenshot (speed 10.50, 8-split guides) [C3].
   - SEGA's official play-guide image 06.png at speed 4.00 with 4-split guides, measured on a 600 px copy whose game
     area is 600×338 and scaled ×3.2 [C7].
   - chunithm.org's international "judgement line" image at speed 9.5 [C4].
   - A PARADISE-era 65-frame GIF [C22].
2. **Chunithm-Studio decodes (strong, but not checked on hardware) [C21].** The project disassembled `chusanApp.exe`
   (the executable since NEW) and read its option tables, read-only. Its data includes a VERSE stage, so the decoded
   build is probably VERSE or later (inference from `game-scene-assets.md:15`). The authors state that "the original
   game has never been launched" (`native-scroll-speed.md:62-64`).
   - **Licensing:** the repository has no licence file and redistributes SEGA assets. We read its documents for
     numbers only. Nothing from it may be copied into this repo.
   - **Agreement with the screenshots:** its camera, projected by us, reproduces the measured screenshot geometry
     and note thicknesses within 1-2 px.
3. **Official SEGA pages [C7][C8], community guides and wikis [C1][C9]-[C14][C19], and Q&A posts [C15]-[C18].**
4. **Fan reimplementations, as proxies only.** Seaurchin (MIT, 2017) [C20] set its camera by measuring screenshots,
   per its own comment "スクショから計測" (ScenePlayer.h:70). Kounithm (no licence) [C23] is also covered.
5. **Could not be read:**
   - The proxy blocks chunithm.sega.jp/.com images, wikiwiki's ゲームシステム1 page, YouTube and most image hosts. That
     wikiwiki page is the source of chunithm.org's option table [C1:34,73].
   - chunithm.fandom, argw.miraheze and a goo blog failed to load.
   - The official Japanese option page [C8] renders with JavaScript and only its title was readable.

**Versions** (JP release dates from otoge-db [C26]):

| Version | Viewfield change | Source |
|---|---|---|
| CHUNITHM 2015-07-16, PLUS 2016-02-04, AIR 2016-08-25, AIR PLUS 2017-02-09, STAR 2017-08-24, STAR PLUS 2018-03-08, AMAZON 2018-10-25 | No field wall. Speed 1 to 20 (SONIC) in 0.5 steps. Guide lines, field colour and judgement position are listed on a pre-NEW options page (which version added each is unverified). **Geometry not measured directly**: Seaurchin's screenshot-tuned camera (2016-17) is within 2% of the current geometry (see Camera and track). | [C9][C20] |
| AMAZON PLUS 2019-04-11 | **Field wall added** (Options → 背景 → フィールドウォール, off by default). Judgement timing adjustable. | [C10][C15] |
| CRYSTAL 2019-10-24, CRYSTAL PLUS 2020-07-16 | None found. | — |
| PARADISE 2021-01-21, PARADISE LOST 2021-05-13 | Last versions with 0.5 speed steps and a single timing offset (A). GIF geometry matches today's (half-width/(J−VP) = 0.535). | [C1:41,44][C22] |
| NEW 2021-11-04 | Gold cabinets run at 120 fps, silver at 60. Speed, timing and wall are **saved separately per cabinet type**. Speed steps become 0.25 up to 15, then 16, 17, 18, 19 and SONIC. Timing split into A (chart position) and B (input). | [C11][C12][C18][C1:41,44-45] |
| NEW PLUS 2022-04-14, SUN 2022-10-13, SUN PLUS 2023-05-11 | SUN screenshot measured: same geometry. | [C3] |
| LUMINOUS 2023-12-14, LUMINOUS PLUS 2024-06-20 | Notes of any width 1-16 (see the feedback note). No geometry change found. | `game-chunithm-feedback.md` |
| VERSE 2024-12-12, X-VERSE 2025-07-16, X-VERSE-X 2025-12-11, Mate 2026-07-02 | X-VERSE adds selectable stage backgrounds (feedback note). SEGA's new-function page lists no field changes for the current version. No geometry change found (unverified for VERSE and later: no screenshot measured). | [C26][C8] |

**International vs Japanese.**

- The international judgement-line image [C4] measures half-width/(J−VP) ≈ 0.55. The JP SUN frame gives 0.54, so the
  geometry is the same within measurement error.
- chunithm.org documents a single option set for both versions [C1].
- No source documents a viewfield difference. International releases follow JP with a delay (unverified).

## Camera and track

### Measured geometry (observation)

| Quantity | SUN JP screenshot [C3] | Official 06.png [C7] | Decoded camera, projected by us [C21] |
|---|---|---|---|
| Judgement line centre y | 1000 px = **0.926H** (white core 995-1005) | 0.924H (core rows 311-313 of 338) | 1000.2 px = 0.926H |
| Inner track width at the line | ≈1170 px = **0.609W** | 1168 px (x 374-1542 = 0.195-0.803W, per `game-scene-reference.md:25,38`) | 1172.9 px = 0.611W |
| Vanishing point y | ≈ −83 px = −0.077H | −85 px (our fit); −88 px in `game-scene-reference.md:48` | −85.5 px = **−0.079H** |
| Edge lean from vertical | 28.4° (width grows 1.079 px per px of y) | 28.2° (`game-scene-reference.md:51`) | 28.4° |
| Half-width / (J − VP) | 0.54 | 0.54 | 0.540 |
| Far edge y | hidden under the HUD frame, which ends at y ≈ 176 = 0.163H | not visible | wall-0 plane z = −460 → 175.7 px = **0.163H**; mesh end z = −470 → 0.158H |
| Far edge width | ≈ 280 px at y 176 (0.146W) | — | 282 px = 0.147W |
| Far/near width ratio | ≈ 0.239 | — | **0.241** |

Inner-edge x positions in the SUN frame:

| y (px) | Left x | Right x |
|---|---|---|
| 400 | 699 | 1221 |
| 600 | 591 | 1329 |
| 800 | 483 | 1437 |
| 980 | 386 | 1534 |

The guide lines at y 800 are evenly spaced, 119.5 px apart (observation [C3]).

**The camera is centred** (vanishing-point x = W/2) and does not move during play:

- The decoded camera has states 1-7 and 101-107 that shift X by only ±0.1 to ±1.3 units. Their purpose is unknown
  (`game-camera-decoding.md`).
- No source describes camera motion in normal play.

### Decoded camera [C21]

- **Values.** `nf_gamecamera.afb` node 1, state 0 (`game-camera-decoding.md:154`):
  - eye (0, 57, 44);
  - target (0, 3, −146.5);
  - vertical FOV 27.5° (horizontal 47.0° at 16:9);
  - near/far clip 1/1500.

  The stage camera uses the same eye and target with FOV 27° (`:153`).
- **Field names are code-confirmed.** The `SglCamera` code path shows that selector 1 is the eye, selector 2 is the
  target, and the FOV is vertical (`game-camera-decoding.md:149`).
- **Coordinate frame.** The judgement line is at z = −65 (`native-scroll-speed.md:13`).
  - The inner track spans X ±32, and two 1-cell side bands span X ±32..±36 (`game-scene-assets.md:47`).
  - The mesh runs from Z −470 to 0, so it continues past the judgement line toward the camera and off the bottom of
    the screen.
- **Values we derived**, using a standard look-at projection at 1920×1080 (our script):
  - Pitch is 15.83° down (`game-camera-candidate.md:48` gives 15.826°), and focal length 2207 px.
  - The angle between the track plane and the image plane is **74.2°**.
  - In track widths (64 units), the eye is **0.89** above the track and **1.70** behind the judgement line, and the
    visible track (z −65 to −460) is **6.17** long.
- **The FOV is not unique.** Any camera that reproduces the trapezoid (VP, judgement y, width, far y) gives the same
  image of the track plane and the same depth-to-y mapping. We solved for that camera at other FOVs:

  | Vertical FOV | Pitch | Tilt (90° − pitch) | Eye height | Judgement ahead of eye | Visible track length |
  |---|---|---|---|---|---|
  | 27.5° (Chunithm) | 15.8° | 74.2° | 0.89 | 1.70 | 6.17 |
  | 45° | 25.6° | 64.4° | 0.84 | 0.83 | 3.89 |
  | **60°** | **33.8°** | **56.2°** | **0.77** | **0.45** | **3.03** |
  | 90° | 49.2° | 40.8° | 0.61 | 0.00 | 2.22 |

  Eye height, judgement distance and track length are in track widths. The camera only matters for things off the
  track plane: the wall panel's top edge (always off-screen), AIR notes, and any note height.
- **Independent check.** Seaurchin uses DxLib's default 60° FOV ("初期設定では 60度" [C30]). Its screenshot-tuned camera
  (eye (0, 620, −340), target (0, 0, 580), track ±400 wide; ScenePlayer.h:15-21,70, ScenePlayer.Draw.cpp:593) has:

  | | Seaurchin | Our FOV-60 solution |
  |---|---|---|
  | Pitch | 34.0° | 33.8° |
  | Eye height (track widths) | 0.78 | 0.77 |
  | Judgement ahead of eye (track widths) | 0.43 | 0.45 |

  This independently confirms both the geometry and the equivalence.

### True perspective, and how note spacing changes with depth

The track is a true perspective plane, and notes move linearly in world depth. We checked this in four ways:

- **Note positions.** In the SUN frame, a 16th-note stream of 4-cell TAPs has centres at y ≈ 200.5, 251, 322, 432,
  623.5 and about 1040. The values 1/(y + 83) are evenly spaced, 0.000527 apart, which is what linear motion in depth
  under perspective produces. The on-screen gaps per 93.75 ms are **416, 191, 110, 71, 50 px**, from near to far
  (observation [C3]).
- **Note thickness.** The same notes are 6, 9, 11, 19 and 36 px thick (rim included), which fits
  T ≈ 7.2·10⁻⁵·(y + 83)² px. That is the squared depth scaling a flat note on the plane must show. The decoded
  camera with the decoded TAP depth predicts 6.1, 8.4, 12.4, 20.0 and 37.6 px.
- **Straight edges.** The edges and guide lines are straight and meet at one point (all frames).
- **The code.** It moves z linearly with time (`native-scroll-speed.md:7-18`).

**Depth mapping implied by these numbers.** Let d be the fraction of visible time remaining (d = 1 at the far edge,
0 at the line), k = 1/r − 1 = 3.16, J = 0.926H and VP = −0.079H. Then:

- y(d) = VP + (J − VP)/(1 + k·d)
- width(d) = 0.611W/(1 + k·d)

| d | y | Track width | On-screen speed relative to the line | 2.55%-deep TAP thickness |
|---|---|---|---|---|
| 0 (line) | 0.926H | 0.611W | 1.00 | 87 px |
| 0.10 | 0.685H | 0.464W | 0.58 | 50 px |
| 0.25 | 0.483H | 0.341W | 0.31 | 27 px |
| 0.50 | 0.311H | 0.237W | 0.15 | 13 px |
| 1.00 (far edge) | 0.163H | 0.147W | 0.058 (1/17) | 5 px |

So a note spends **the first half of its visible time between 16% and 31% of screen height**. It enters the lower
half of the screen (y > 0.5H) only for the **last 23%** of its visible time.

Near the line it moves **3.17 H per visible time**, 17× its far-edge speed. A flat mania stage that spans the screen
moves 1.0 H per visible time.

### 16:9 layout: what occupies the sides

- **Top centre** [C21 `game-scene-assets.md:75-76,92`, `game-scene-reference.md:38`]:
  - A score/gauge frame spans x 578-1342, y 36-176 (0.30-0.70W, 0.033-0.163H).
  - A judgement-count strip sits above it (y 0-36).
  - The far end of the track is hidden exactly behind this frame's bottom edge.
- **Top left:** the player card (name plate 576×228 px).
- **Top right:** speed and mirror indicators, and the song card (1334, 0, 586×182 px) [C21][C5 gamescreen.md].
- **Left side:** the character portrait and skill text [C5 gamescreen.md].
- **Everything else beside the track:** the 3D stage background. The default stage "yellowring" is grids, ring
  models and animations, not a picture [C21 `game-scene-assets.md:10-15`]. The official city stage shows LED
  dot-strip side rails (observation [C7]).
- **Side bands.** The track has a 1-cell side band on each side (X ±32..±36), so the full field is 72/64 of the inner
  width, **0.687W** at the line.
  - In the SUN frame the bands measure 6.1% of the inner width each.
  - They are translucent dark teal, e.g. RGB (10-15, 30-52, 30-53), darker towards the far end (observation [C3]).
  - Their textures are stage-specific (`fieldSide`, `sideGlow`, 32×512 [C21 `game-scene-assets.md:28-29`]).
- **Track floor.** It is near-black (6, 6, 6) in the SUN frame. In 06.png it is a grey gradient, about 55-71/255
  near the line, 30-36 at mid-depth and 26 far away (observation). The difference is consistent with the Field
  Brightness option and per-stage textures (inference).

### Other implementations (inferior; for completeness)

- **Seaurchin** (MIT) [C20]:
  - It projects the judgement line at 0.947H with width 0.620W, VP −0.084H, and the far end (z = 3000) at 0.124H
    with r = 0.20.
  - A 1280×106 opaque top cover (14.7% of height) hides everything above 0.147H (Play.as:87-88).
  - It is close to the real geometry, with the line about 2% lower.
  - It renders the whole field into an off-screen 1024×3840 texture (ScenePlayer.h:71-73) and draws that as one 3D
    ground quad (ScenePlayer.Draw.cpp:108). The GPU then gives perspective-correct texturing.
- **Kounithm** (Unity, no licence) [C23]:
  - Its camera has FOV 60 (main.unity:3597), position (8.5, 9.5, −4.21) (main.unity:3620) and a pitch of 42.7°.
  - We computed J 0.874H, VP −0.30H and width 0.817W: much steeper and wider than the real game. Not useful.
- **Chunithm-Studio's own preview** (`player-scene.ts`) scales by 1/(1 + distance·speed/125.7)
  (`native-scroll-speed.md:52-55`). It draws the track with affine Canvas triangles, which it admits are not
  perspective-correct (`game-scene-assets.md:125`). Read for numbers only (no licence).
- **Other repositories** have no camera code to learn from:
  - raiti-chan/CHUNITHM-Emulator (GPL-2.0) is an early stub with no field.
  - chunithm-project/chunithm-clone (GPL-3.0) is empty.
  - PatrickkikALT/ChuniVR (MIT) wraps the real game in VR.
  - UMIGURI is closed source (see the feedback note) [C27].

## Note approach and speed

### Speed number to visible time

**Decoded from the NEW-and-later executable** [C21 `native-scroll-speed.md:7-29,56`]:

- A note's z is −65 − 1.5 × table × t, where t is in 60 Hz frames. The game converts time to frames at 16.6667 ms,
  so the step does not depend on the cabinet's frame rate.
- The speed table value is display × 0.8 for display 1.00-15.00 (0.25 steps). Display 16, 17, 18 and 19 give 14, 16,
  18 and 20, and SONIC gives 60.
- With no wall the visible length is 395 units (−65 to −460), so **T = 395 / (90 × table) s**.
- For display s ≤ 15 that is **T = 5486 / s ms**.

| Chunithm speed | Visible time | mania ScrollSpeed with the same time (11485/T) |
|---|---|---|
| 2.0 (beginner preset [C1:12]) | 2743 ms | 4.2 |
| 4.0 (intermediate preset) | 1372 ms | 8.4 |
| **6.0 (default, advanced preset [C1:12,41])** | **914 ms** | 12.6 |
| **8.0 ("a common scroll speed for MASTER charts" [C1:41])** | **686 ms** | 16.7 |
| 7.5-8.5 ("7.5~8.5の人が多い" [C9]) | 731-645 ms | 15.7-17.8 |
| 10.0 | 549 ms | 20.9 |
| 12.0 | 457 ms | 25.1 |
| **13.85** | **396 ms** | **29 (Alpha's speed [C32])** |
| 15.0 | 366 ms | 31.4 |
| 16 / 17 / 18 / 19 | 313 / 274 / 244 / 219 ms | 36.7 / 41.9 / 47.1 / 52.4 |
| SONIC | 73 ms | (beyond mania's 1-40 range) |

**Conversions.**

- s_chuni ≈ 0.4776 × mania ScrollSpeed, i.e. mania speed ≈ 2.094 × s_chuni. Mania's constant is 11485/speed, from
  `DrawableManiaRuleset.cs:43,183`.
- Mania's default speed 8 (`ManiaRulesetConfigManager.cs:24`) is 1436 ms, about Chunithm 3.8.
- Project Sekai 10 feels like Chunithm "8~9" according to wikiwiki [C14].

**Cross-checks.**

- *The SUN screenshot.* At speed 10.50 the measured 16th stream is 70.9 units apart (we back-projected the note
  centres through the decoded camera). That gives 70.9/(72 × 10.5) = 93.8 ms. The song is RELOAD at BPM 160 [C26],
  and a 16th there lasts 93.75 ms. This confirms both the speed formula and the camera.
- *A community wall table* [C5] implies a visible length of about 406 units instead of 395. That makes effective
  speed differ by at most 3.6%. The source of the gap is unknown (`native-scroll-speed.md:103`).

**Speed is constant in track space, not screen space.** Notes move a fixed number of world units per second and the
camera does the rest. That is the screen-speed profile in the depth table above.

**Older versions** used 0.5 steps from 1.0 to 15.0 and 1.0 steps to SONIC/20 [C1:41][C9]. The speed-to-time constant
before NEW is not decoded (unverified). Seaurchin's own formula is not evidence for SEGA's: SeenDuration =
6/(HS × bpm/BaseBpm) s (ScenePlayer.cpp:225-240), so it scales with BPM.

### BPM changes and soflan

- "基本的にBPMはスクロール速度に影響しない": BPM basically does not affect scroll speed [C19].
- Speed changes are charted explicitly as **SFL** segments with a six-decimal multiplier. They are purely cosmetic and
  may be negative, which reverses the scroll [C22 Charting.md:90-99].
- Early MASTER charts used stops. WORLD'S END charts have the traits 速 (fast), 止 (stop) and 戻 (reverse) [C19].
- MET (time signature) is cosmetic and draws "thin measure lines on the playfield" [C22 Charting.md:79-88].
- **Contrast with lazer mania.** Its default scrolling divides by the most common beat length, so BPM changes alter
  the scroll rate (`DrawableManiaRuleset.cs:53`, `DrawableScrollingRuleset.cs:111-117`).

### Where notes appear

- Notes exist from z = −550, beyond both the mesh end (−470) and wall 0 (−460). With no wall they emerge from behind
  the wall-0 plane at y 0.163H (`native-scroll-speed.md:47`). On a normal screen that is the bottom edge of the HUD
  frame.
- They appear at full opacity with no fade (see the feedback note).
- "Notes come down from the top of the gameplay field" [C2 gameplay.md:3].
- After the hit time they keep moving past the line, onto the part of the track below it.

## Judgement line and receptors

- **Position.** 0.926H (7.4% above the bottom), measured in two real frames and the decoded camera (table above).
  Width: the inner 16 cells only, x 0.195-0.803W (`game-scene-reference.md:25,38`; observation [C3]: the gold glow
  starts at x 374, not over the side bands).
- **Look** (SUN frame [C3]):

  | Part | Rows (px) | Thickness | Colour |
  |---|---|---|---|
  | Core | 995-1005 | 11 px = 1.0% H | pure white |
  | Glow | 983-1018 | about 36 px = 3.3% H | gold, from (249, 188, 24) to (255, 226, 119) |

  - 06.png gives the same: core at rows 311-313 of 338, glow 307-317, ×3.2 → about 35 px [C7].
  - The feedback note measures "about 2% of frame height" in the GIF, does not see it pulse, and records Seaurchin's
    #FFF289 texture.
  - Stage.xml names `notesFieldLine = Orange` for the default stage [C21 `game-scene-assets.md:10`]. What that colour
    applies to is unverified.
- **Draw order.** The judgement line (`GoalLine`, layer 13) and the in-field combo (14) are drawn *below every note
  layer* [C21 `native-note-assets.md:104,131`]. Notes pass over the line.
- **No on-screen receptors.** The receptor is the physical slider: 16 cells, each with an upper and lower touch block,
  32 sensors in all [C13]. It sits below the landscape monitor (chunithm.org cabinet photo `images/controls.jpg` [C2]).
  - Field cell n corresponds to slider cell n [C22 Charting.md:8].
  - On screen a cell is 1/16 of the inner width: 0.0382W = 73 px at the line.
  - How exactly the physical slider lines up with the screen is unverified.
- **Lane dividers (guidelines).** Options: OFF, 2-, 4- (default), 8- or 16-split [C1:68][C9].
  - In the SUN frame (8-split) the lines at y 800 sit at x 482, 602, 722, 841, 961, 1081, 1200, 1320 and 1438. The
    end values are the edges.
  - Colour is about (64-74, 196-239, 178-215), roughly **#45D9C8**.
  - They are **2-3 px wide near the line, 1 px at mid-screen, and fade to faint and dim at the far end**. That
    behaviour fits lines of fixed track width, about 0.2% of the track width (inference).
  - The field edge line is darker: (31, 103, 99), about #1F6763.
  - 06.png (4-split) shows the same teal, about (55-64, 99-142, 98-128) (observations [C3][C7]).

## Lane covers / walls

### Field wall (AMAZON PLUS, 2019-04-11, and later)

**Option.** Field wall, 0-16, default 0 [C1:70][C10][C15]. It is in the background settings
(Options → 背景 → フィールドウォール) [C15]. All three official presets use 0 [C1:31]. Wiki descriptions:

- "Similar to beatmaniaIIDX's SUDDEN+" [C1:70][C9].
- "At 15, only the judgement line is visible, and at 16, nothing is visible" [C1:70][C9].
- Because of perspective, step 8 does not hide half [C9].
- It does not affect judgement [C17].

**Shape** (decoded [C21 `native-scroll-speed.md:30-45`]):

- An opaque white-tinted **vertical panel** with vertices (±36, 0, z) to (±36, 54, z), standing on the track and
  spanning the inner track and both side bands.
- Its texture is `CHU_UI_Fieldwall_0001.dds`, 640×480 (4:3, like 72×54). Styles 0002 and 0003 exist.
- It has 0.1-unit feathered outer edges.
- Its top edge projects above the screen (wall 0: y = −71 px), so on screen it **covers everything above its base
  line**.
- What the default texture looks like is unverified: only its size is documented, and no screenshot with a wall was
  available.

**Positions.** The table is a z per step [C21 `native-scroll-speed.md:30-34`]. The other columns we computed: hidden
fraction is (z + 460)/395, base y comes from the decoded camera, and "effective speed" is 1/(visible fraction). For
comparison, Chunithm-Studio's own projection gives base y 199, 230, 269, 317, 382, 481, 554, 710 and 1004 px for walls
2, 4, 6, 8, 10, 12, 13, 14 and 15 (`native-scroll-speed.md:72-83`).

| Wall | z | Hidden fraction of the track | Base y | Effective speed ×; community table [C5] |
|---|---|---|---|---|
| 0 | −460 | 0 | 0.163H | 1.00 |
| 1 | −437 | 0.058 | 0.174H | 1.06; 1.06 |
| 2 | −414 | 0.116 | 0.186H | 1.13; 1.1275 |
| 3 | −391 | 0.175 | 0.200H | 1.21; 1.205 |
| 4 | −368 | 0.233 | 0.215H | 1.30; 1.2925 |
| 6 | −322 | 0.349 | 0.250H | 1.54; 1.515 |
| 8 | −276 | 0.466 | 0.295H | 1.87; 1.83 |
| 10 | −230 | 0.582 | 0.354H | 2.39; 2.3075 |
| 11 | −207 | 0.641 | 0.392H | 2.78 |
| 12 | −179.4 | 0.710 | 0.446H | 3.45 |
| 13 | −151.8 | 0.780 | 0.514H | 4.55 |
| 14 | −110.4 | 0.885 | 0.659H | 8.70 |
| 15 | −64.4 | 1.00 (at the line) | 0.931H | — |
| 16 | −55.2 | > 1 | 1.012H | — |

**Steps.** Each step from 0 to 11 hides 23 units, **5.82% of the visible track**. Above 11 the steps grow: 7.0%,
7.0%, 10.5% and 11.6%.

**Speed semantics.** The wall **keeps note speed**:

- Visible time shrinks to T × (1 − hidden).
- Players describe this as the wall shortening the visible time [C17], and the community uses an "effective speed"
  table [C5].
- To compensate, players lower the speed setting when they raise the wall: wall 3 at speed 8.5, wall 4 at 8.0, or
  no wall at 10.0 [C16]. These combinations give 532, 526 and 549 ms of visible time. **Players keep visible time
  about constant by hand and use the wall to make notes larger and slower near the line.**
- Lowering the wall together with the speed "makes notes look bigger" [C13].
- An extreme case is wall 14 at speed 1 [C16]: 631 ms visible, with only the lower 27% of the screen in use.

**Variants.** Challenge (課題) mode uses a moving wall, and another stage type has a half-width of 108
(`native-scroll-speed.md:45`).

**No other covers.** No source mentions a bottom cover, LIFT or HIDDEN-style option. This is an unverified negative.

### All viewfield options

| Option | Range | Default | Effect | Since | Source |
|---|---|---|---|---|---|
| Scroll speed | 1.00-15.00 in 0.5 steps (PARADISE LOST and before) or 0.25 steps (NEW and later), then 16-19 and SONIC (20) | 6.0 | T = 5486/s ms (NEW and later) | 2015 | [C1:41][C9][C11][C21] |
| Mirror | OFF / ON | OFF | chart mirrored horizontally | (unverified) | [C1:42] |
| Judgement timing A | −2.0 to +2.0, step 0.1 | 0.0 | moves the notes relative to the music; the only offset up to PARADISE LOST | AMAZON PLUS | [C1:44][C10] |
| Judgement timing B | −2.0 to +2.0, step 0.1 | 0.0 | moves judgement only | NEW | [C1:45][C11] |
| Guidelines | OFF / 2 / 4 / 8 / 16 split | 4 | teal lines along the track | (unverified) | [C1:68][C9] |
| Field brightness ("field color") | −5 to 0 | 0 | darkens the track; −5 is black | (unverified) | [C1:69][C9][C12] |
| Field wall | 0-16 | 0 | see above | AMAZON PLUS | [C1:70][C10] |
| Judgement display position | Bottom / Middle / Top / Heaven | Bottom | see the feedback note | Heaven: AMAZON PLUS | [C1:62][C10] |
| Field indicator | OFF / COMBO / SCORE … | — | see the feedback note | — | [C1:71] |
| Per-cabinet values | speed, timing and wall saved separately for 120 fps (gold) and 60 fps (silver) | — | — | NEW | [C11][C12][C18] |

Notes on the table:

- The unit of the timing steps is unverified.
- There is no option for camera angle, FOV, track width, lane cover from the bottom, or scroll direction. The
  geometry is fixed for every player.

## Notes and holds (shapes, rendering)

**Colours by type** [C2 gameplay.md:11-96][C22 Charting.md:426-434]:

| Type | Look |
|---|---|
| TAP | red bar |
| ExTAP | yellow |
| HOLD | yellow bar |
| SLIDE | blue bar |
| FLICK | silver/grey with a blue centre 1/3 of its width |
| AIR | green arrow above the note, the same width as the note |
| AIR-action | purple bars |
| AIR-CRUSH | light purple floating bars |
| DAMAGE (`MNE`) | exists [C22 Charting.md:264-271]; look unverified |

**One TAP in the SUN frame**, sampled down a column (observation [C3], rows 606-646 at y ≈ 624):

1. A 3 px white rim on the far edge.
2. A vertical gradient from (255, 125, 126) through (255, 43, 44) down to (180, 30, 31) and (112, 23, 24).
3. A bright white-pink highlight band through the middle of the note.
4. A 3-4 px grey-lilac rim on the near edge, about (186, 169, 183).

It is a filled bar with light rims, not a flat colour.

**Thickness.**

- The decoded TAP head mesh spans Z −4.48 to +5.60 around the note position: **10.09 units deep**, which is 2.5 cells
  of depth and **2.55% of the 395-unit visible track** [C21 `game-scene-reference.md:86`].
- On screen that is about **87 px (8.1% H) at the judgement line**, 1.2 cell-widths tall. It shrinks with the square
  of depth to 5 px at the far edge. This matches the measured 6-36 px (Camera and track).
- The mesh extends about 5.6 units toward the camera and 4.5 away. At the hit time the note therefore straddles the
  line, slightly more of it already past (inference: we assume the model origin is the note's time position).
- Seaurchin's notes are 1 cell deep, 1.7% of its visible track (ScenePlayer.h:71-79). Its approximation is thinner.
- 3D height above the track is unverified. No side faces are visible in the frames we measured.

**Widths.**

- Notes are 1-16 cells wide (c2s Width 1-16 [C22 Charting.md:159]; any width in charts since LUMINOUS, per the
  feedback note).
- There are separate models per width (`NotesTap_01..16` [C21 `native-note-assets.md`]).
- Decoded X spans: 3.92, 15.68 and 63.40 units for 1, 4 and 16 cells, i.e. **98%, 98% and 99%** of the cell span
  [C21 `game-scene-reference.md:86`].

**Draw layers and blending** [C21 `native-note-assets.md:104-110,170-173`]:

- Bottom to top: hold body 15, slide body 16, hold end 17, slide end 18, hold/slide head 20, TAP 23, FLICK 24,
  ExTAP 25, DAMAGE 27, AIR 30-31.
- Within a layer, **narrower notes draw on top** (priority 16 − width).
- Hold and slide bodies use **additive blending** (SRC_ALPHA, ONE, ADD). They are white-tinted normally and 0.4 grey
  in the failed state. While held, their brightness pulses between ×1.25 and ×1.75.
- The look of a hold body in 06/07.png is translucent with a bright centre line (observation [C7]). Body width
  relative to the head is unverified.

**Bar lines.**

- MET draws "thin measure lines" [C22 Charting.md:88].
- In 06.png a thin teal line, the same colour as the guides, spans the inner track at y 211 (×3.2) and passes through
  a TAP (observation [C7]).
- The SUN frame shows none.
- Seaurchin draws white 6 px lines (ScenePlayer.Draw.cpp:576-588). The feedback note describes white lines in the GIF.
- **Colour is therefore uncertain: teal or white.**

**AIR** (not needed for mania, recorded for completeness):

- The arrow model reaches y ≈ 15.57 units, 0.24 track widths above the track (`native-note-assets.md:25,51`).
- At the judgement line its top projects to y 0.678H (our projection).

**120 fps cabinets.** UMIGURI documents that "In 120fps mode, TAP width will be thicker" [C28]. Whether SEGA's gold
cabinet does the same is unverified.

## Key count and lane layout

- **One field for every chart.** The field is always 16 cells across the same 0.611W. Patterns use groups of cells;
  the geometry never changes [C22 Charting.md:8,157-159].
- **The community "4K" pattern** means "The play field is split up into 4 equally sized lanes" [C6 skills.md:13].
  That is exactly what the default 4-split guidelines mark.
- **osu!mania → Chunithm converters** (both MIT):

  | Keys | osu_2_mgxc (cells per lane) [C24] | osu2ugc [C25] |
  |---|---|---|
  | 1K | 16 | — |
  | 2K | 8 + 8 | — |
  | 3K | 4, 8, 4 | — |
  | 4K | 4 × 4 | 4 × 4 |
  | 5K | 3, 3, 4, 3, 3 | 3 each, left-aligned in 15 cells (rightmost cell empty) |
  | 6K | 3, 2, 3, 3, 2, 3 | "full" 3, 2, 3, 3, 2, 3, or "centered" 2 each in 12 cells |
  | 7K | 2, 2, 2, 4, 2, 2, 2 | 2 each, left-aligned in 14 cells |
  | 8K | 2 × 8 | — |

  - osu_2_mgxc: `osu_mania_to_mgxc.py:7-16`.
  - osu2ugc: README.md:19-51. It also converts BPM changes, time signatures and SV (README.md:65).
- **Only 1K, 2K, 4K, 8K and 16K tile 16 cells evenly.** Every converter distorts 5-7K, either with unequal lanes or
  with empty cells.

## Feedback (judgement display, hit effects, lane lighting)

See `game-chunithm-feedback.md`, which covers hit effects, judgement text, combo, lane and slider lighting, field
brightness and the gauge. Two geometry facts it depends on:

- The judgement line is drawn under the notes (layer 13).
- The top HUD frame ends exactly at the far edge of the track (0.163H).

## What this means for us

Everything here is visual: settings, not mods (invariants 1 and 3). Values below are proposals for the main session
to record in `DECISIONS.md`. Questions only Alpha can answer are listed separately at the end.

### P1. Chunithm preset geometry (SPEC 2)

Parameterise presets by the **screen trapezoid**, not by tilt/FOV. The trapezoid alone fixes `TrackToScreen` on the
track plane, and any FOV can then be solved to match it (see the FOV table).

| Parameter | Chunithm (measured) | SPEC now | Proposed Chunithm preset |
|---|---|---|---|
| Judgement line height above the bottom | 7.4% (0.926H) | 12% | **7.4%** |
| Far edge y | 0.163H (under the HUD frame) | top edge | **0.163H**, needs Alpha (Q1); fallback below |
| Vanishing point | 0.079H above the top, centred | at or above the top | **−0.079H**, follows from the other values |
| Far/near width ratio r | 0.241 | 0.3-0.6, derived | **0.24**; widen SPEC's range to 0.15-0.6 |
| Inner track width at the line | 0.611W | 45% (4K) to 70% (10K) | **0.61W for every key count** |
| Side rails | 1 cell (1/16 of the track) each side | — | **1/16 of the track width each side**; outer width 0.687W |
| Edge lean | 28.4° from vertical | — | follows from the values above |
| Equivalent camera | FOV 27.5°, tilt 74.2° | FOV 30-90 (60), tilt 0-75 (55) | FOV 60° → **tilt 56.2°**, eye height 0.77, line 0.45 ahead, track 3.03 long (all in track widths) |

Notes on the table:

- **SPEC's defaults are already near Chunithm.** Reading tilt as 90° − pitch (the angle between the track plane and
  the image plane), SPEC's tilt 55° / FOV 60° places the vanishing point at −0.106H, against Chunithm's −0.079H.
  Matching the line height and track length then gives Chunithm.
- **Fallback if notes must enter at the very top.** Keep r = 0.24 and the line at 0.926H. VP then moves to −0.292H
  and the edge lean falls to 14°, a visibly stretched track. The alternative, keeping VP, gives r = 0.078. We
  recommend the faithful 0.163H and letting the osu! HUD (score, accuracy) use the band above the far edge, as
  Chunithm does.

### P2. Speed (SPEC 3)

- **Keep SPEC 3 as written.** Visible time equals mania's time range, and notes move linearly in track space. That is
  exactly Chunithm's model, with notes moving at constant speed in track space.
- In the Chunithm preset, show the Chunithm equivalent next to mania's value, e.g. "396 ms ≈ CHUNITHM 13.9", using
  s = 5486/T. Above 15 the table steps are 16→313, 17→274, 18→244 and 19→219 ms.
- **Do not change mania's default speed.** For playtests use 686 ms (Chunithm 8.0, mania 16.7), 914 ms (Chunithm
  default, mania 12.6) and Alpha's 396 ms (mania 29).
- **Readability at Alpha's speed.** At 396 ms in Chunithm geometry:
  - A note is in the lower half of the screen for only the last 92 ms.
  - Near the line it moves 8.0 H/s. Alpha's flat full-height stage at the same visible time moves 2.5 H/s.
  - Typical Chunithm players use 645-731 ms instead [C9][C1:41].

  Check readability at mania speed 29 *and* at 16.7 in the compare sheets. Alpha may want a longer visible time in
  perspective (playtest question, not a default change).
- **BPM scaling.** Chunithm ignores BPM; lazer mania's default scales with it. SPEC 3 says to keep mania's behaviour,
  which must stay so for parity. Leave whether to offer a "constant speed" visual option to `topic-speed.md`
  (unverified need).

### P3. Wall (SPEC 4)

- **Shape.** A **vertical opaque panel** at the wall depth, spanning the inner track plus rails, with its top above
  the screen. On screen it covers everything above its base line, including the rails and the stage background. This
  matches Chunithm and reads well: the far part is simply gone.
- **Look.** A plain dark panel with a 1-2 px lighter lower edge (our design; Chunithm's texture look is unverified).
- **Position setting** (hidden fraction of the track):
  - Range **0-0.90**. SPEC's 0-0.80 stops at Chunithm wall 13 (0.78); wall 14 (0.885) is used in practice [C16].
  - Step 0.01, labelled with the equivalent Chunithm step: hidden = 0.0582 × step for steps 0-11, then table values.
  - Default **0**, which is Chunithm's default and all three presets [C1:31].
- **Speed semantics.** Chunithm keeps speed. SPEC 4's default (constant visible time) does automatically what
  Chunithm players do by hand when they lower speed alongside the wall [C16]. Keep SPEC's default globally. Whether
  the Chunithm preset should switch "keep speed when moving the wall" on is Q2.
- **No bottom cover.** Chunithm has no LIFT. Keep SPEC's bottom lift as an extra, off by default. Alpha's baseline
  argues for allowing a lift of 0, which means the line can go down to the bottom edge [C32].

### P4. Lanes and dividers (SPEC 5)

- **Lane widths.** Lanes stay mania's equal fractions of the track. Never use 16-cell groupings like 2-2-2-4-2-2-2:
  equal lanes match the physical keys, which is SPEC 5's "fixed lanes".
- **Guide style, 4K.** Three inner dividers at 1/4, 1/2 and 3/4, which is Chunithm's default 4-split exactly.
  - Colour **#45D9C8** at about 80% alpha.
  - Width in **track space**: 0.22% of the track width (about 2.5 px at the line at 1080p), at least 1 px on screen,
    so the lines thin and dim with distance as in Chunithm.
  - Edge lines are darker, **#1F6763**.
  - Alpha's flat skin uses 2 px lines at 1440p (1.5 px at 1080p) between inner columns [C32]: the same weight.
- **Guide style, 7K.** Six inner dividers at k/7 in the same style. Chunithm has no 7-split, and an 8-split grid
  would misalign with the lanes.
- **Optional overlay.** A "16-cell grid" (2/4/8/16-split, Chunithm style), off by default. Cosmetic only.
- **No per-key-count camera changes in the Chunithm preset.** Chunithm never changes its geometry. At 0.61W a 7K lane
  is 0.087W (167 px at 1920 wide) at the line, and the outer edge leans 28°.
  - SPEC 5's automatic tilt reduction should apply to the other presets only, or be overridable per preset.
  - At 10K (0.061W per lane) the 28° lean needs a playtest check.
- **Rails and floor.**
  - Rails: 1/16 of the track width each side, dark teal (about #0C2A2A at 80% alpha).
  - Floor: near-black (#060606) with a "track brightness" setting, as in the feedback note's proposal P8.

### P5. Notes, holds and bar lines (SPEC 6)

- **Thickness.** **0.0255 of the visible track length**, fixed in track units, which is what SPEC 6 already demands.
  It gives about 87 px (8.1% H) at the line at 1080p and 5 px at the far edge.
  - Alpha's flat notes are 6.4% of screen height (152/296 of a 60/480 column) [C32]. The two look alike at the line.
  - Make thickness a setting from 0.015 to 0.06, default 0.0255 (*tune*).
- **Width.** **98% of the lane**, a 1% gap on each side.
- **Note skin.** Default: Alpha's flat bars (white outer lanes, pink #FF8290 inner) [C32]. An optional "Chunithm"
  skin: a red gradient bar with a white far rim and a light near rim, and a central highlight band (colours above).
- **Holds.**
  - Body below every head, additively blended, brightening while held and greying when broken (Chunithm's layer
    order and blend).
  - Narrower notes on top of wider ones.
  - Notes drawn **over** the judgement line.
- **Bar lines.** Thin lines across the inner track only, in track space. Off in an Alpha-style preset [C32]; on in the
  Chunithm preset. Colour is open (teal or white, see above); use white at about 35% alpha as the feedback note
  proposes, until verified.
- **Note anchor.** Chunithm roughly centres the note on the line at hit time (inference). **Keep SPEC 6's leading
  edge**, which is mania's behaviour and what Alpha's timing is trained on.

### P6. HUD (SPEC 7)

- In the Chunithm preset the band above the far edge (y < 0.163H) holds no track. The osu! HUD's top elements fit
  there without covering notes, as Chunithm's own HUD frame does.
- No change to SPEC 7.

### Where SPEC.md's assumptions about Chunithm look wrong

1. **"Wider far end" (SPEC 2).** Chunithm's far/near ratio is 0.24, below SPEC's "typical 0.3-0.6". The far end is
   narrow; it only *looks* moderate because it is cut off at 16% from the top.
2. **"Gentler tilt" (SPEC 2).** In 3D terms Chunithm's camera has a 27.5° FOV and the track plane sits 74° from the
   image plane: at SPEC's 75° tilt limit and below its 30° FOV minimum. "Gentle" describes only the screen result.
   Presets should be specified as trapezoids (P1).
3. **"Wall on" (SPEC 2).** The wall defaults to 0 and all official presets leave it off [C1:31]. With wall 0, the
   far edge already coincides with the HUD bottom.
4. **Judgement line 12% (SPEC 2).** Chunithm puts it at 7.4%; Alpha's habit is 0% [C32].
5. **Notes enter at the top edge (SPEC 2).** In Chunithm they enter at 16.3% below the top, under the HUD frame.
6. **Wall range 0-80% (SPEC 4).** Chunithm reaches 100%, in steps of 5.8% up to 64%, and players use 88.5%.
7. **Wall keeps visible time by default (SPEC 4).** Chunithm keeps note speed. SPEC's default is still a sound
   convenience (P3), but the spec should not describe it as Chunithm's behaviour.
8. **Width grows with key count, tilt reduced at 7K+ (SPEC 5).** Chunithm uses one 0.61W field and one camera for
   every pattern.
9. **Speed.** SPEC 3 is right: notes move linearly in track space with no screen-space correction. But Chunithm's
   speed ignores BPM, unlike lazer mania's default. Mania's behaviour must stay; just do not cite Chunithm for it.

### Questions for Alpha (for `QUESTIONS.md`)

- **Q1.** In the Chunithm preset, may notes enter at 16% below the top, as in Chunithm, or must they enter at the
  very top edge, which stretches the track? Best guess: 16%, faithful.
- **Q2.** Should the Chunithm preset's wall keep speed, as in Chunithm, or keep visible time (SPEC's default)? Best
  guess: keep visible time, the SPEC default, which automates what players do by hand.
- **Q3.** Should the Chunithm preset turn the wall on (as SPEC says) or default it to 0 (as Chunithm does)? Best
  guess: 0.

## Unverified / open questions

- **Geometry before PARADISE.** No pre-2021 frame was measured. Only Seaurchin's 2016-17 screenshot-tuned camera
  suggests it is within 2%.
- **VERSE, X-VERSE and Mate.** No frame was measured; only the absence of documented changes.
- **Hardware checks.** The decoded camera and tables have never been compared with the running game by their
  authors. Our screenshot measurements agree with them, but at only two real frames (SUN, official 06.png).
- **Old speed formula.** The speed-to-time constant before NEW.
- **The 395 vs ~406 unit gap.** The community wall table implies a visible length of about 406 units (≤3.6% effect).
- **Wall texture.** What the default field-wall texture looks like.
- **Bar line colour** (teal or white), and the bar-line width in units.
- **Stage-dependent colours.** Floor, rails and judgement glow per stage; `notesFieldLine = Orange` meaning.
- **Notes.** TAP 3D height, hold-body width relative to the head, DAMAGE look.
- **120 fps.** Whether gold cabinets draw thicker TAPs (only UMIGURI documents this).
- **Timing option unit.** The unit of the timing A/B steps.
- **International.** Version lag and any viewfield difference beyond one measured image.
- **Slider alignment.** How the physical slider aligns with the on-screen cells.

## Sources

Commit hashes are the versions read. "No licence" means read for facts only; nothing copied.

- [C1] chunithm.org guide, settings page (DarinMao/chunithm-org, no licence file):
  https://github.com/DarinMao/chunithm-org/blob/e9a8ff5/docs/basic/settings.md. Presets (:8-31); scroll speed range,
  steps and "common … around 8.00" (:41); mirror (:42); timing A/B (:44-45); judgement position (:62); guidelines,
  field brightness, field wall (:68-70); field indicator (:71); wall-table credit (:121); table adapted from wikiwiki
  (:34, :73).
- [C2] chunithm.org, gameplay page: https://github.com/DarinMao/chunithm-org/blob/e9a8ff5/docs/basic/gameplay.md.
  Note colours and types (:3-96). Cabinet photo `docs/basic/images/controls.jpg`.
- [C3] chunithm.org, SUN JP gameplay screenshot, 1920×1080, speed 10.50, RELOAD:
  https://github.com/DarinMao/chunithm-org/blob/e9a8ff5/docs/basic/images/gameplay.png. All "SUN frame" measurements.
  The image was downloaded to the scratchpad only.
- [C4] chunithm.org, international judgement-line image:
  https://github.com/DarinMao/chunithm-org/blob/e9a8ff5/docs/basic/images/judgement-line.png. Half-width/(J−VP) ≈ 0.55.
- [C5] chunithm.org, field wall / speed table by shirohara_64qam (tweet, January 2024):
  https://github.com/DarinMao/chunithm-org/blob/e9a8ff5/docs/basic/images/field-wall-table.png. Effective-speed factors
  for walls 1-10. Also gamescreen.md (:51-77) for HUD regions.
- [C6] chunithm.org, skills page: https://github.com/DarinMao/chunithm-org/blob/e9a8ff5/docs/intermediate/skills.md#L13.
  "4 equally sized lanes".
- [C7] SEGA official play guide, https://chunithm.sega.com/play, images 06/07/09.png. The image host is blocked here;
  we measured the copies in Chunithm-Studio `docs/references/official-gameplay-tap.png`, `-long.png` and `-air.png`.
  Judgement line, guides, bar line, floor gradient, VP.
- [C8] SEGA JP options page, https://chunithm.sega.jp/play/option/ (renders with JavaScript; only the title was
  readable). SEGA new-function page, https://chunithm.sega.com/play/new-function.html (no field changes listed).
- [C9] Gamerch CHUNITHM wiki, options: https://gamerch.com/chunithm-puls/286117. Speed 1 to SONIC in 0.5 steps;
  presets; "7.5~8.5の人が多い"; wall 0-16 as SUDDEN+, 15/16 behaviour, perspective; guide lines; field colour −5;
  judgement positions.
- [C10] silentblue RemyWiki, CHUNITHM AMAZON PLUS: https://silentblue.remywiki.com/CHUNITHM:AMAZON_PLUS. Release
  2019-04-11; field wall added; judgement timing adjustable.
- [C11] silentblue RemyWiki, CHUNITHM NEW changes: https://silentblue.remywiki.com/CHUNITHM:NEW/Changes. 0.25 speed
  steps to 15, then 1.0 steps; separate 120/60 Hz settings; timing A/B.
- [C12] wikiwiki, これからCHUNITHMを始める人へ: https://wikiwiki.jp/chunithmwiki/これからCHUNITHMを始める人へ. Speed
  1.0~20.0 (SONIC); presets; per-cabinet speed, timing and wall; guidelines; field colour.
- [C13] wikiwiki, よくある質問1: https://wikiwiki.jp/chunithmwiki/よくある質問1. 16 columns, 32 sensors; lowering the
  wall and the speed makes notes look bigger; gold 120 fps, silver 60 fps.
- [C14] wikiwiki, プロジェクトセカイから来た人へ: https://wikiwiki.jp/chunithmwiki/プロジェクトセカイから来た人へ.
  "スピード8~9がプロセカにおける速さ10に近い".
- [C15] Yahoo! Chiebukuro, 2019-04-12: https://detail.chiebukuro.yahoo.co.jp/qa/question_detail/q12206276807. Wall
  newly added, off by default, Options → 背景 → フィールドウォール.
- [C16] Yahoo! Chiebukuro, 2021-03: https://detail.chiebukuro.yahoo.co.jp/qa/question_detail/q12240073227. Players'
  speed/wall pairs (8.5/3, 8.0/4, 10.0/0, 1/14).
- [C17] Yahoo! Chiebukuro, 2022-07: https://detail.chiebukuro.yahoo.co.jp/qa/question_detail/q13265592998. The wall
  shortens visible time, covers from the top down, and does not affect judgement.
- [C18] Yahoo! Chiebukuro, 2025-01: https://detail.chiebukuro.yahoo.co.jp/qa/question_detail/q14308843209. Gold
  120 fps vs silver 60 fps; options saved separately.
- [C19] アニヲタWiki, ソフラン: https://w.atwiki.jp/aniwotawiki/pages/32872.html. "基本的にBPMはスクロール速度に影響しない";
  early stops; WORLD'S END 速/止/戻.
- [C20] Seaurchin 0.82, MIT, © 2016 Yuu Kobayashi: https://github.com/nazawa/Seaurchin/tree/fffa136. ScenePlayer.h:15-26
  (lane constants), :70 (camera, "measured from screenshots"), :71-79 (lane buffer, note scale), :106-107;
  ScenePlayer.cpp:225-240 (SeenDuration, BPM-scaled); ScenePlayer.Draw.cpp:108 (ground quad), :576-588 (guide and
  measure lines), :593 (camera call); Resources/Skins/Default/Scripts/Play.as:87-88 (top cover).
- [C21] Chunithm-Studio 0.1.26 (Nastu-Koi/Chunithm-Studio, **no licence, contains SEGA assets; numbers only**):
  https://github.com/Nastu-Koi/Chunithm-Studio/tree/a5ade8d/docs.
  - `game-camera-decoding.md:149-154`: camera fields, eye/target/FOV.
  - `game-camera-candidate.md:48`: pitch, focal length.
  - `native-scroll-speed.md`: note z (:7-18), speed table (:20-28), wall table and panel (:30-45), note range (:47),
    visible time (:56), unverified items (:62-108).
  - `game-scene-assets.md`: stage (:10-15), textures (:27-29), field mesh (:43-47), HUD (:75-92), Canvas affine note
    (:125).
  - `game-scene-reference.md`: screenshot calibration (:25-51), TAP extents (:86).
  - `native-note-assets.md`: models (:22-35), AIR height (:51), layers (:104-131), hold blend (:170-173).
- [C22] Chunithm-Research (Suprnova, Unlicense, written for PARADISE):
  https://github.com/Suprnova/Chunithm-Research/blob/6604cc1/Charting.md. 16 columns (:8); MET (:79-88); SFL (:90-99);
  width (:157-159); slides (:204-209); mines (:264-271); note cosmetics (:426-434). GIF `_assets/cyaegha example.gif`
  (PARADISE-era geometry).
- [C23] Kounithm (emubure, no licence): https://github.com/emubure/Kounithm/blob/15c02d3/Assets/main.unity. Camera FOV
  (:3597) and position (:3620).
- [C24] osu_2_mgxc (Kenton1989, MIT):
  https://github.com/Kenton1989/osu_2_mgxc/blob/8464605/osu_mania_to_mgxc.py#L7-L16. Key count → cell mapping.
- [C25] osu2ugc (SnowyRinlya, MIT, 2026): https://github.com/SnowyRinlya/osu2ugc/blob/eaf81dd/README.md. 4K-7K layouts
  (:19-51); BPM, metre and SV conversion (:65).
- [C26] otoge-db (zvuc), `chunithm/data/music-ex.json`:
  https://github.com/zvuc/otoge-db/blob/cbbf30f/chunithm/data/music-ex.json. Version release dates (first song added
  per version); RELOAD id 2327, BPM 160, SUN.
- [C27] Repositories checked with no usable camera code: https://github.com/raiti-chan/CHUNITHM-Emulator (GPL-2.0,
  stub), https://github.com/chunithm-project/chunithm-clone (GPL-3.0, empty), https://github.com/PatrickkikALT/ChuniVR
  (MIT, VR wrapper of the real game).
- [C28] UMIGURI documentation, 60/120 fps mode: https://umgr.inonote.jp/en/docs/basic/60-120fps-mode. "In 120fps mode,
  TAP width will be thicker".
- [C29] note.com, Project Sekai speed: https://note.com/nurumayuuu_/n/n0f910506a55b. Sekai 10.45 ≈ 514 ms (context
  only).
- [C30] DxLib 3D camera reference: https://dxlib.xsrv.jp/function/dxfunc_3d_camera.html. Default FOV 60°.
- [C31] osu! (this repo): `osu.Game.Rulesets.Mania/UI/DrawableManiaRuleset.cs:43,53,183`;
  `osu.Game.Rulesets.Mania/Configuration/ManiaRulesetConfigManager.cs:24`;
  `osu.Game/Rulesets/UI/Scrolling/DrawableScrollingRuleset.cs:111-117`.
- [C32] `docs/ortho/research/baseline-alpha.md` (Alpha's stable setup) and `docs/ortho/research/game-chunithm-feedback.md`.
