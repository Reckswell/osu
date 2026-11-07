# YARG and Clone Hero: the fixed-lane perspective highway

Last checked: 2026-11-04

## Summary

YARG (open source, LGPL-3.0) draws each instrument's highway as one textured plane seen by a pinhole camera.
That camera is the player's choice of **camera preset**: field of view, height, distance, pitch, a cross-track "curve" and
the length of the far-end fade. Twelve presets ship. Ten imitate other guitar games, one is orthographic, and the
**Default** preset is FOV 55°, height 2.66, distance 1.14 (minus 6), pitch 24.12°, curve 0.5, fade 1.25. We projected
every preset to the screen.

- **Default at 16:9:** strikeline 0.827H, far edge 0.409H, track 0.292W wide at the line.
- **Far/near width:** about 0.45 for Default; every guitar-game preset sits between 0.39 and 0.47. That is much gentler
  than Chunithm (0.24) or SDVX (0.106).
- **Visible length:** 2.5 track widths. Notes move at a constant speed in track space, so visible time is
  5 × length / speed. The defaults give 833 ms (code default, speed 6) or 1000 ms (new profile, speed 5).
- **Far-end fade:** notes fade in over the far 25% of the track.
- **Readability tools:**
  - colour per lane and coloured frets at the strikeline;
  - for pro keys, 10 white-key lanes on a 0.29W track, with black-key notes between them, white and black notes, and a
    5%-alpha colour tint per half-octave group;
  - three tiers of beat lines;
  - an optional hit-window band drawn on the track.

Clone Hero is closed source. Its documented highway options are note speed ("Hyperspeed") and, since v1, highway
length in the profile. No camera options are documented.

## Scope, evidence and licences

- **YARG** (YARC-Official/YARG), **LGPL-3.0**. We read the `dev` branch at `8e5c4a8` (2026-11-02) [Y1]. `master`
  (`f5bfbe6`, 2026-06-24) differs in note and track elements but not in the camera code we cite.
- **YARG.Core**, **LGPL-3.0**, read at `2f3900c` (2026-11-03) [Y2]. It holds the preset and profile classes.
- **Licence rule.** Both are LGPL, so this repository (MIT) may take ideas and numbers from them, never code. Nothing
  was copied. All numbers marked "projected" come from our own script, which re-implements the published transforms
  (camera position, pitch, vertical FOV, the post-projection fit) in a few lines of Python. That script is in the
  scratchpad only.
- **Clone Hero**: closed source. Sources are the official wiki and release post [C1]-[C4]. Two Clone Hero facts we
  found are struck through in an old wiki revision, and both are marked below.
- **Measurement check.** YARG's README banner [Y3] is a 1920×1080 four-player frame. The drums highway's left rail
  moves 0.286 px sideways per px of height, and the right rail 0.375 px. That is a total width growth of 0.66 px per
  px, against 0.69 projected for Default and 0.76 or more for every other preset. So the banner is consistent with
  the Default camera (pixel measurement with PIL on rows 780-920). The multiplayer layout shifts each camera sideways
  but keeps widths along rows, which is why the two rails differ. Only that ratio is comparable, because multiplayer
  scales each highway down.

## Camera and track

### The rig

All file references are to [Y1] unless marked [Y2].

- **Track** [Y1 `Assets/Script/Gameplay/Player/TrackPlayer.cs:31-35`]:
  - The track lies in the plane y = 0 of its player object, with z running away from the camera.
  - It is `TRACK_WIDTH` = 2 units wide (x ±1).
  - The strikeline is at z = −2 (`STRIKE_LINE_POS`).
  - The far end, where notes become fully transparent, is at z = 3 × HighwayLength (`DEFAULT_ZERO_FADE_POS` = 3).
  - The visible track is therefore **5 units = 2.5 track widths** long at the default length.
  - Players sit 100 units apart at height 100 (`GameManager.Loading.cs:509`).
- **Camera** [`Assets/Script/Gameplay/Visuals/CameraPositioner.cs:69-89`]:
  - Local position (0, PositionY, PositionZ − 6). The default prefab places it at (0, 2.66, −4.86)
    [`Assets/Prefabs/Gameplay/Visual/BaseVisual.prefab:6621`].
  - Pitched down by `Rotation` degrees about x, with no yaw or roll.
  - `Camera.fieldOfView` is Unity's **vertical** FOV; or the camera is orthographic with `OrthographicSize`
    (half-height in units).
- **Screen fit** [`HighwayCameraRendering.cs:23-33, 177-232, 521-544, 829-841`]:
  - YARG does not show the camera image as is. It measures the track's screen bounding box, from the bottom of the
    camera image (where it hits the track plane) up to the far end.
  - It then scales the image **about the bottom centre of the screen**, by a uniform factor applied after projection.
  - **Single player:** the scale is at most 1 (wider than the screen is never allowed) and keeps the track's height to
    at most **72%** of the screen.
  - **Several players:** each highway is capped at 45% of the screen width, 55% of its height, and 90% of an equal
    share of the width.
  - At 16:9 single player, every preset except the orthographic one fits unscaled (scale 1.00, projected).
- **Multiplayer tilt** (`HighwayTiltMultiplier`, 0-0.85, default 0.5 [`SettingsManager.Settings.cs:642`]):
  - It moves each player's camera sideways, from −m (leftmost) to +m (rightmost) units.
  - The outer highways then lean towards the screen centre [`HighwayCameraRendering.cs:631-644`].

### Camera preset settings (what the player can change)

[Y2 `YARG.Core/Game/Presets/CameraPreset.cs:7-36`]

| Setting | Range | Default | What it does |
|---|---|---|---|
| FieldOfView | 40-150° | 55 | vertical FOV |
| PositionY | −40 to 40 | 2.66 | camera height above the track (units; track width 2) |
| PositionZ | −40 to 40 | 1.14 | camera distance: the camera sits at z = PositionZ − 6 |
| Rotation | 0-180° | 24.12 | pitch down |
| FadeLength | 0-5 | 1.25 | length of the far-end fade, in units (multiplied by HighwayLength when that is above 1) |
| CurveFactor | −3 to 3 | 0.5 | bends the track across its width (below) |
| Orthographic | on/off | off | parallel projection |
| OrthographicSize | 0.1-10 | 2 | half-height of the orthographic view, in units |

Profile settings [Y2 `YargProfile.cs:34-35,174-175`], with the menu's limits
[Y1 `Assets/Script/Menu/ProfileList/ProfileSidebar.cs:449,460`]:

- **NoteSpeed**, 0-100 units per second. The code default is 6. The "New Profile" button creates profiles with 5
  [`ProfileListMenu.cs:162`].
- **HighwayLength**, 0.001-10, default 1.

Other gameplay settings:

- `EnableHighwayAnimation` (default on) and `KickBounceMultiplier` (0-2, default 1) [`SettingsManager.Settings.cs:640-641`].
- The highway preset's **NoteHeight** (0.8-1.2, default 1) scales the note models' height
  [Y2 `HighwayPreset.cs:29-31`].

### Built-in presets, projected to a 1920×1080 screen

Preset values are from [Y2 `CameraPreset.Defaults.cs:63-180`]; the screen columns are projected by us.

- y is measured from the top as a fraction of H; widths are fractions of W.
- "Far" is the zero-fade end (z = 3).
- "Lean" is the angle of the track's outer edge from vertical.
- "Eye" is the camera's height and its distance behind the strikeline, in track widths.
- "Plane angle" is 90° minus the pitch: the angle between the track plane and the image plane.

| Preset | FOV | Y | Z | Pitch | Curve | Strikeline | Far | Width at line | Far/near | VP | Lean | Eye (h / back) | Plane angle |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| **Default** | 55 | 2.66 | 1.14 | 24.12 | 0.5 | **0.827H** | **0.409H** | **0.292W** | **0.45** | +0.070H | 18.9° | 1.33 / 1.43 | 65.9° |
| Circular | 60 | 2.39 | 1.54 | 24.12 | 0 | 0.816H | 0.404H | 0.302W | 0.41 | +0.112H | 20.9° | 1.20 / 1.23 | 65.9° |
| High FOV | 60 | 2.66 | 1.27 | 24.12 | 0.5 | 0.818H | 0.422H | 0.272W | 0.44 | +0.112H | 18.9° | 1.33 / 1.37 | 65.9° |
| The Band 1 | 47.84 | 2.32 | 1.35 | 26 | 0.5 | 0.806H | 0.319H | 0.373W | 0.43 | −0.050H | 21.2° | 1.16 / 1.33 | 64.0° |
| The Band 2 | 44.97 | 2.72 | 0.72 | 24.12 | 0.5 | 0.836H | 0.374H | 0.331W | 0.47 | −0.041H | 18.5° | 1.36 / 1.64 | 65.9° |
| The Band 3 | 57.29 | 2.22 | 1.61 | 23.65 | 0.5 | 0.819H | 0.389H | 0.334W | 0.40 | +0.099H | 22.4° | 1.11 / 1.19 | 66.3° |
| The Band 4 | 62.16 | 2.56 | 1.20 | 19.43 | 0.5 | 0.852H | 0.482H | 0.267W | 0.43 | +0.207H | 20.2° | 1.28 / 1.40 | 70.6° |
| Hero 2 | 58.15 | 1.82 | 1.26 | 12.40 | 0 | 0.849H | 0.513H | 0.330W | 0.39 | +0.302H | 28.2° | 0.91 / 1.37 | 77.6° |
| Hero 3 | 52.71 | 2.17 | 0.97 | 15.21 | 0 | 0.875H | 0.498H | 0.325W | 0.42 | +0.226H | 24.0° | 1.08 / 1.52 | 74.8° |
| Hero Traveling the World | 53.85 | 1.97 | 1.31 | 16.62 | 0 | 0.851H | 0.461H | 0.353W | 0.40 | +0.206H | 25.9° | 0.98 / 1.34 | 73.4° |
| Hero Live | 62.16 | 2.40 | 1.42 | 21.31 | 0 | 0.829H | 0.446H | 0.285W | 0.41 | +0.176H | 21.2° | 1.20 / 1.29 | 68.7° |
| Clone | 55 | 2.07 | 1.27 | 17.09 | 0 | 0.851H | 0.465H | 0.336W | 0.40 | +0.205H | 24.8° | 1.03 / 1.37 | 72.9° |
| Kinda Orthogonal | ortho, size 1.206 | 19.04 | −13.98 | 44.75 | 0 | 0.935H | 0.280H | 0.209W | 1.00 | none | 0° | — | 45.3° |

Notes on the table:

- **Names.** The names suggest copies of Rock Band 1-4 ("The Band"), Guitar Hero 2, 3, World Tour and Live ("Hero …")
  and Clone Hero ("Clone"). That is an inference from the names only; no source states it.
- **Theme defaults.** The Circular theme asks for the Circular camera
  [Y1 `Assets/Script/Themes/ThemePreset.Defaults.cs:29-39`]. The default theme is "Rectangular" (`:10-12`).
- **The family.** Every perspective preset has far/near **0.39-0.47**, the strikeline **13-19% above the bottom**, the
  far end **32-51% below the top**, and the track **0.27-0.37W** wide. The vanishing point is mostly on screen, just
  above the track's end.
- **Kinda Orthogonal is the only parallel projection.**
  - Pitch 44.75°, so the track is foreshortened by sin 44.75° = 0.70.
  - It has no fade.
  - Its raw image is 1.6 screen-heights tall, so the 72% cap scales it by 0.45. That explains the narrow 0.209W track.

### Depth to screen for the Default preset (projected)

k = 1/r − 1 = 1.23. d is the fraction of the visible track from the strikeline (0) to the far end (1). On-screen speed
is relative to the strikeline.

| d | y | Track width | On-screen speed |
|---|---|---|---|
| 0 | 0.827H | 0.292W | 1.00 |
| 0.10 | 0.744H | 0.260W | 0.79 |
| 0.25 | 0.649H | 0.223W | 0.58 |
| 0.50 | 0.538H | 0.181W | 0.38 |
| 0.75 (fade starts) | 0.464H | 0.152W | 0.27 |
| 1.00 (far end) | 0.409H | 0.131W | 0.20 |

Near the line a note moves 5× its far-end screen speed. In Chunithm the ratio is 17× (`game-chunithm.md`).

### Curve factor (cross-track bend)

[`Assets/Art/Shaders/highways.hlsl:98-125`]

- **What it bends.** The vertex shader bends the whole highway around a cylinder whose axis runs **along** the track.
  Lane lines along z stay straight; lines across the track (strikeline, beat lines, note bars) bow downwards at the
  ends.
- **Radius.** R = sign(c) × 15 − c × 13/3 units, so c = 0.5 gives R = 12.8, c = 3 gives R = 2, and c = −3 bends the
  other way.
- **Size of the bow** (projected for the Default camera at 1080p; track half-width 1):

  | Curve | Edge drops (units) | Edge drops (% of width) | Strikeline bow | Bow at the far end |
  |---|---|---|---|---|
  | 0.5 | 0.039 | 1.9% | 8 px | 5 px |
  | 3 | 0.245 | 12% | 52 px | 29 px |

- **Before.** An older parabolic version is left in the shader under `#else`, unused.

### Camera motion

YARG moves the camera itself during play [`CameraPositioner.cs:14-36, 162-296`], unless `EnableHighwayAnimation` is
off:

| Motion | Trigger | What happens |
|---|---|---|
| Highway raise | start of play | the camera pitch starts 60° up and swings to +2° past the preset in 0.33 s, then settles in 0.17 s |
| Kick bounce | drum kick [`DrumsPlayer.cs:866, 1114`] | a 0.03-unit vertical punch × `KickBounceMultiplier` |
| Punch | a combo break after a combo of 10 or more [`TrackPlayer.cs:1366-1368, 1388-1390`] | 0.03 units sideways, left or right at random, for 0.25 s |
| Scoop | star power activation [`TrackPlayer.cs:729-731`] | the pitch dips 1° down and 0.25° up over 0.083 s |

The strikeline group has its own 0.25 s z "zoom-in" at start [`StrikelinePositioner.cs:14-22`]. All of these move the
strikeline on screen.

## Note approach and speed

- **Speed.** Notes move at **NoteSpeed units per second** in track space. The camera does the rest, so the screen
  speed profile is the depth table above [`TrackElement.cs:37-47`; `TrackEffectElement.cs:636`].
  - Visible time = 5 × HighwayLength / NoteSpeed seconds.
  - Speed 6 gives 833 ms and speed 5 gives 1000 ms, at Expert and length 1.
- **Lower difficulties.** When "Reduce note speed by difficulty" is on (default), lower difficulties scroll slower:
  Hard ×0.75, Medium ×0.5625, Easy and Beginner ×0.421875 [`Helpers/Extensions/DifficultyExtensions.cs:58-75`;
  `SettingsManager.Settings.cs:279`].
- **Song speed.** Speed divides by the song speed, except in practice below 100%. So notes keep their spacing in
  track units per beat of real time [`BasePlayer.cs:26-45`].
- **Highway length** moves the far end to z = 3L.
  - It keeps NoteSpeed, so it changes visible time; it is the "keep speed" behaviour of Chunithm's wall.
  - Above 1 it also stretches the fade.
  - The HUD anchor at the far end moves with it [`TrackPlayer.cs:282-293`].
- **Fade.** Notes are spawned beyond the far end and are invisible there. A screen-space alpha mask fades every highway
  element linearly from z = 3L − FadeLength (fully visible) to z = 3L (invisible)
  [`HighwayCameraRendering.cs:139-145`; `Assets/Resources/HighwaysAlphaMask.shader:41-60`]. With the defaults the
  fade covers the far **25%** of the visible track, between 0.464H and 0.409H on screen.

## Judgement line and receptors

- **Strikeline.** At z = −2, a 3D row of frets: one coloured fret per lane, built from theme models
  [`Assets/Script/Gameplay/Visuals/Fret/Fret.cs`].
  - Inactive frets are grey (0.321).
  - A press plays an animation and particles in the lane's colour, with a 0.25 s fade
    [`Fret.cs:56-63, 249-270, 304-320`].
  - The track continues below the strikeline to the screen bottom. At 0.83H the strikeline leaves 17% of the screen
    below it for the frets and the combo/star-power meters.
- **Hit-window display** (setting `ShowHitWindow`, **off by default** [`SettingsManager.Settings.cs:644`]):
  - A translucent full-width quad lies on the track at the strikeline [`BaseVisual.prefab` "HitWindow", scale x 2].
  - Its depth is (back-end − front-end window) × NoteSpeed.
  - It is offset by half the window asymmetry and by the video and input calibration
    [`Assets/Script/Gameplay/Visuals/HitWindowDisplay.cs:40-71`].
  - Because it lives in track space, it foreshortens with the track and grows with note speed.
- **Hit timing.** Notes are judged as they cross the strikeline (the Clone Hero manual says the same [C2]).

## Lane covers / walls

- **No SUDDEN/HIDDEN-style cover** exists in YARG's gameplay code. We searched `Assets/Script` for sudden, lane cover
  and hidden; the only hit is in career-mode data.
- **What plays the role instead:**
  - HighwayLength (shorter track, same speed);
  - FadeLength (soft far edge).
- **Clone Hero** v1 added "highway length" to player profiles [C3]. No lane cover is documented (unverified negative).

## Notes and holds (shapes, rendering)

- **Notes are theme models** loaded from asset bundles [`ThemePreset.Defaults.cs:10-50`]:
  - "Rectangular" (default): flat bars;
  - "Circular": Rock Band-style gems.
  - Their exact depth on the track was not measured (unverified). NoteHeight 0.8-1.2 scales only the model height.
- **Sustains** are one flat mesh strip on the track [`TrackElements/SustainLine.cs:276-305`].
  - It runs from the strikeline (while held) to the tail, rising 0.01 units at its end.
  - Its width comes from the prefab: 0.8 units for a guitar normal sustain, 2 for open and wildcard sustains, and 0.2
    for a pro keys sustain [`Assets/Prefabs/Gameplay/Visual/TrackElements/*.prefab`, `_sustainWidth`].
  - The visible line is drawn by the sustain texture and shader inside that strip; its painted width is unverified.
  - The whammy bar adds a wave whose amplitude follows the whammy input (`SustainLine.cs:256-271`).
- **Beat lines** run across the full track width in three tiers [`TrackElements/BeatlineElement.cs:10-16`;
  `Beatline.prefab` mesh x 2]:

  | Tier | Depth | Share of the 5-unit visible track | Alpha |
  |---|---|---|---|
  | Weak beat | 0.03 units | 0.6% | 0.3 |
  | Strong beat | 0.05 units | 1.0% | 0.4 |
  | Measure | 0.07 units | 1.4% | 0.6 |

- **Track floor** [`TrackMaterial.cs:179-190`; Y2 `HighwayPreset.cs:9-26`]:
  - Base near-black #0F0F0F, with a 15% #4B4B4B wavy layer.
  - #575757 side-rail pattern.
  - Optional user images for the base and the sides.

## Key count and lane layout

- **Lane centres.** Lanes are equal fractions of the 2-unit track. Lane i of n is centred at
  2(i + 1)/n − 1 − 1/n [`TrackElement.cs:66`]; lane decorations scale to 2/n [`LaneElement.cs:31-46`].

| Instrument | Lanes | Lane width (track fractions) | Readability device |
|---|---|---|---|
| 5-fret guitar | 5 (+ open) | 1/5 | lane colours green, red, yellow, blue, orange [Y2 `ColorProfile.Defaults.cs:12-22`]; open notes are purple and span the track (their sustain strip is the full 2 units) |
| 6-fret guitar | 3 | 1/3 | black (top row) and white (bottom row) notes per lane [C2] |
| 4-lane drums | 4 + kick | 1/4 | kick notes are their own note type, or sit in dedicated kick lanes if the player picks them [`Drums/FourLaneDrumsNoteElement.cs:34-56`]; the regular kick model's width is unverified; separate tom and cymbal notes [C2] |
| 5-lane keys | 5 | 1/5 | same colours as 5-fret |
| **Pro keys** | 25 keys; **10 white keys visible** plus the black keys between | white-key spacing **1/10** (0.2 units) | below |

**Pro keys** is the wide case:

- **Layout** [`ProKeysPlayer.cs:56-57`; `ProKeys/ProKeysTrackOverlay.cs:35-107`; `ProKeysVisual.prefab:146-148`]:
  - White keys sit at a 0.2-unit pitch (white offset −0.913).
  - Black keys sit near the boundaries between white keys (black offset −0.812, i.e. 0.1 units right of the white
    lane centres), with a skipped slot where an octave has no black key.
- **Range shifts.** The track slides sideways in 0.25 s when the chart shifts range [`ProKeysPlayer.cs:440-447`].
- **Notes.** White notes for white keys, black notes for black keys. Both have a white metal rim, and star-power notes
  are #FFF2AA / #AA8F00 [Y2 `ColorProfile.ProKeys.cs:70-83`].
- **Tint.** Each key carries a lane overlay tinted by its **half-octave group** (C-E, F-B), cycling red, yellow, blue,
  green, orange. The tint is at **5% alpha** [`ProKeysTrackOverlay.cs:43-79, 109-118`].
- **Group edges.** The overlays on C and F use an edge-gradient texture (one side), and those on E and B the same
  texture mirrored. That outlines each group's edges [`:120-138`].
- **Press highlight.** A held key shows a 30%-alpha highlight in its group colour, rising along the track [`:81-107`].
- **Projected size.** At the Default camera on 1920×1080, a white-key lane is **56 px at the strikeline and 25 px at
  the far end**.
- **In the banner frame** [Y3] the pro keys highway shows the coloured group bands and small white and black note bars
  (observation).

## Feedback (judgement display, hit effects, lane lighting)

- **Star power** recolours the whole highway with the highway preset's StarPowerColor, default **#FF9800**, through a
  shader state [Y2 `HighwayPreset.cs:9-10`; `TrackMaterial.cs:126-145, 207`]. Star-power notes switch to the
  star-power note colours (white by default for guitar) [Y2 `ColorProfile.Defaults.cs:50`].
- **"Groove"** (the top multiplier) lerps the floor to a blue palette (#000933 / #23349C / #2C499E) and adds waviness,
  at 5 per second [`TrackMaterial.cs:101-124, 186-191, 222-224`].
- **Hits** trigger the fret's hit animation and particles. Misses trigger miss animations. Drums add a kick flash
  across the strikeline [`Fret.cs:294-350`; `KickFretFlash.cs`].
- **On the track.** The combo and multiplier meter sits on the track below the strikeline ("Multiplier Text",
  "Combo Meter" in `BaseVisual.prefab`). YARG shows no per-note judgement text (unverified).

## Clone Hero (closed source)

What official sources say:

- **Note Speed/Hyperspeed:** "The visual scrolling speed of the note highway. Does not affect the actual song speed.
  Can be changed in the profile menu." [C1]
- **v1.0 release** (2022-11-29):
  - player profiles gained "highway length";
  - players can use their own still or animated highway image;
  - drums and pro drums are playable in the usual 4-lane format, with 5-lane compatibility;
  - Brutal mode makes "Notes rescind from the strike line as your streak gets higher" [C3].
- **Strikeline:** "The bottom part of the highway where the frets and the hit window reside." [C1]
- **5-fret lanes** are green, red, yellow, blue and orange [C2].
- **Struck-through statements** in an old FAQ revision, unverified: "highways look the best at a 512x1024
  resolution", and a hyperspeed-to-note-speed rule of thumb ("Hyperspeed 5 would be around 10") [C4].
- **No camera FOV, angle or height option** is documented (unverified negative). YARG's "Clone" preset is presumably
  a Clone Hero look-alike (inference from the name).

## What this means for us

All of this is visual: settings, not mods (invariants 1 and 3). These are proposals for the main session to record in
`DECISIONS.md`.

### Y1. Keep the camera in screen terms (SPEC 2; DECISIONS "Camera is set in screen terms")

- YARG exposes world-space camera numbers: FOV, height, distance and pitch.
- It then has to auto-fit the resulting image to the screen (the 72% height cap, scale about the bottom centre). So
  what the player sets is not what the player sees.
- FOV and distance trade off: Default and High FOV differ in FOV by 5° but produce almost the same trapezoid.
- This supports our choice of four screen numbers. **No change.**

### Y2. Where the guitar-game family sits (SPEC 2, presets)

| Family | Far/near | Line above bottom | Far edge below top | Lean | Width at line |
|---|---|---|---|---|---|
| YARG Default (and every guitar preset) | 0.45 (0.39-0.47) | 0.17 (0.13-0.19) | 0.41 (0.32-0.51) | 19° (18-28°) | 0.29W (0.27-0.37) |
| Chunithm | 0.24 | 0.074 | 0.163 | 28° | 0.61W |
| SDVX (USC) | 0.106 | 0.059 | 0.284 | 28° | 0.435W |

- The guitar family is the gentle end of perspective: SPEC's old "0.3-0.6" range describes it, not Chunithm or SDVX.
- **No new preset is proposed.** SPEC lists four, and Alpha asked for Chunithm and SDVX. If a "gentle" custom
  recipe is wanted for players coming from guitar games, it is: Perspective, far/near 0.45, judgement line 0.17,
  far edge 0 (or 0.41 faithful), track width as the preset's. A 0.29W track is narrower than the Parallel preset's
  0.45W and would leave a 10K lane only 56 px at the line.

### Y3. Optional far-edge fade (SPEC 2, SPEC 4)

- YARG fades notes in over the far 25% of the track (FadeLength 1.25 of 5).
- Chunithm and SDVX notes appear at full opacity at the far edge (`game-chunithm.md`).
- Our presets stretch the far edge to the top of the screen, so notes pop in at the top edge.
- **Proposal:** a "Far fade" setting: 0-30% of the visible track, measured down from the far edge or the wall's edge.
  - **Default 0** in Chunithm and Voltex (faithful).
  - Linear alpha, like YARG's.
- It is cosmetic. It must stack with (multiply) mania's own Hidden/Fade In covers, never replace them, because those
  are mods that mania scores.

### Y4. Optional hit-window band on the track (SPEC 7)

- YARG draws the hit window as a translucent band **in track space** at the strikeline (`ShowHitWindow`, default off).
  So the band foreshortens with the track and grows with speed.
- **Proposal:** "Show hit window on track", **default off**.
  - It spans the depths of mania's earliest and latest hittable offsets (the Meh window from the beatmap's OD and
    mods, read from mania's `HitWindows`, not re-derived).
  - Full lane width, white at 8-10% alpha.
  - Purely visual. Add a parity case only if the code reads anything input-related (it should not).

### Y5. Bar-line tiers (SPEC 6)

- YARG uses three beat-line tiers (depth 0.6 / 1.0 / 1.4% of the visible track; alpha 0.3 / 0.4 / 0.6). Mania has
  two (major = measure, minor = beat).
- **Proposal** for native bar lines:
  - major: alpha 0.6, depth 0.7% of the track;
  - minor: alpha 0.3, depth 0.35%;
  - full track width, at least 1 px on screen.
- The 2:1 depth ratio is YARG's measure-to-weak ratio rounded. The absolute depths are halved from YARG because our
  notes, at 2.55%, are flat bars, and the lines must stay thinner than notes (*tune*).

### Y6. What not to copy

- **Cross-track curve.** It bows the judgement line (8 px at YARG's default, 52 px at its maximum, 1080p) and breaks
  SPEC 1's planar `TrackToScreen`. Mania judges each lane against a straight line. Not proposed.
- **Camera motion** (raise, kick bounce, punch, scoop) moves the strikeline on screen. SPEC 2 allows optional cosmetic
  motion only if it never moves the judgement line, so YARG's motions are out as they stand.
- **Range-shifting lanes** (pro keys). Mania lanes are fixed (SPEC 5).

### Y7. Speed and length confirm SPEC 3 and SPEC 4

- YARG moves notes at constant speed in track space and lets the camera produce the screen acceleration, as SPEC 3
  says.
- Its HighwayLength, which keeps speed, is the same trade as Chunithm's wall and our "keep speed when moving the wall"
  toggle. **No change.**

## Unverified / open questions

- Exact note-model depth and width on the track (theme asset bundles were not opened), and the painted width of
  sustain lines.
- That the "Hero …", "The Band …" and "Clone" presets reproduce those games' cameras (names only).
- Clone Hero:
  - any camera, cover or lane options beyond note speed and highway length;
  - its note speed units and default (the only rule of thumb found is struck through on the wiki).
- No single-player YARG frame was measured. The banner check confirms only the Default edge slope.

## Sources

Commit hashes are the versions read. YARG and YARG.Core are LGPL-3.0: read for ideas and numbers only, nothing copied.

- [Y1] YARG, LGPL-3.0, `dev` at `8e5c4a81f4ed80ae4360605017d71273584e7c86`:
  https://github.com/YARC-Official/YARG/tree/8e5c4a81f4ed80ae4360605017d71273584e7c86.
  - `Assets/Script/Gameplay/Visuals/CameraPositioner.cs`: preset applied (:69-89), motions (:14-36, :162-296);
    triggers in `Assets/Script/Gameplay/Player/TrackPlayer.cs:729-731, 1366-1390` and `DrumsPlayer.cs:866, 1114`.
  - `Assets/Script/Gameplay/Visuals/HighwayCameraRendering.cs`: screen-fit constants (:23-33), fade parameters
    (:139-145), fit (:177-232), post-projection (:521-544), multiplayer offset (:631-644), track bottom (:829-841).
  - `Assets/Art/Shaders/highways.hlsl:98-130`: curve. `Assets/Resources/HighwaysAlphaMask.shader:41-60`: fade.
  - `Assets/Script/Gameplay/Player/TrackPlayer.cs`: constants (:31-35), length and fade (:282-293).
  - `Assets/Script/Gameplay/Player/BasePlayer.cs:26-45`: note speed and song speed.
  - `Assets/Script/Helpers/Extensions/DifficultyExtensions.cs:58-75`: difficulty scaling.
  - `Assets/Prefabs/Gameplay/Visual/BaseVisual.prefab`: camera (:6606-6621), strikeline (:6863), hit window.
  - `Assets/Script/Gameplay/Visuals/HitWindowDisplay.cs:40-71`: hit window.
  - `Assets/Script/Settings/SettingsManager.Settings.cs:279, 640-644`: settings.
  - `Assets/Script/Gameplay/Visuals/TrackElements/`: `BeatlineElement.cs:10-16`, `SustainLine.cs:256-305`,
    `TrackElement.cs:37-66`, `LaneElement.cs:31-46`, `ProKeys/ProKeysTrackOverlay.cs:35-138`.
  - `Assets/Script/Gameplay/Player/ProKeysPlayer.cs:56-57, 404-447`; `Assets/Prefabs/Gameplay/Visual/ProKeysVisual.prefab:146-148`.
  - `Assets/Script/Gameplay/Visuals/TrackMaterial.cs:101-191`; `Assets/Script/Gameplay/Visuals/Fret/Fret.cs`.
  - `Assets/Script/Themes/ThemePreset.Defaults.cs:10-50`.
  - `Assets/Script/Menu/ProfileList/ProfileListMenu.cs:162-176`; `ProfileSidebar.cs:445-464`.
- [Y2] YARG.Core, LGPL-3.0, `2f3900cf91aa141149d60cb122e88876e1709abb`:
  https://github.com/YARC-Official/YARG.Core/tree/2f3900cf91aa141149d60cb122e88876e1709abb.
  - `YARG.Core/Game/Presets/CameraPreset.cs:7-36` and `CameraPreset.Defaults.cs:63-180`: camera settings and presets.
  - `HighwayPreset.cs:9-45`: highway colours, NoteHeight, waviness.
  - `ColorProfile.Defaults.cs:12-22, 50, 71`; `ColorProfile.ProKeys.cs:14-83`: colours.
  - `YargProfile.cs:34-35, 174-175`: note speed and highway length defaults.
- [Y3] YARG README banner, 1920×1080, four players: https://github.com/YARC-Official/YARG/blob/master/Images/Banner.png
  (Git LFS; fetched from media.githubusercontent.com to the scratchpad only, not committed). Edge-slope check and the
  pro keys observation.
- [C1] Clone Hero wiki, Dictionary: https://wiki.clonehero.net/books/general-info/page/dictionary. Note
  Speed/Hyperspeed, Highway, Strikeline.
- [C2] Clone Hero wiki, How to Play (revision 917):
  https://wiki.clonehero.net/books/clone-hero-manual/page/how-to-play/revisions/917. Lanes, colours, strikeline, drums.
- [C3] Clone Hero v1 release post, 2022-11-29: https://clonehero.net/2022/11/29/v1-release.html. Highway length in
  profiles, custom highways, drums, Brutal mode.
- [C4] Clone Hero wiki, FAQ revision 1058 diff:
  https://wiki.clonehero.net/books/general-info/page/frequently-asked-questions/revisions/1058/changes. Struck-through
  512×1024 highway advice and the hyperspeed rule of thumb.
