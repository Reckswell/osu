# StepMania 5 and Etterna: perspective mods on a tilted note field

Last checked: 2026-11-04

## Summary

StepMania's perspective mods are two numbers, **tilt** and **skew**. The five named mods are presets of those two.
They have the same code in StepMania 5.1 and Etterna, both MIT. The rendering works like this:

1. The whole note field is drawn as one flat plane: receptors, notes, holds, explosions and the lane board.
2. The plane is rotated about its horizontal centre line by **∓30° per unit of tilt**, shrunk by up to 10%, and
   shifted 20-45 px.
3. It is seen through a **45° horizontal-FOV** camera (26° vertical at 16:9) whose principal point is the field's
   centre. Skew moves that point from the player's column to the screen centre.

Notes keep moving linearly along the field, and the GPU does the perspective.

- **Hallway** (tilt −1) with Reverse, receptors at the bottom, is the highway look: far/near width **0.80** at the
  screen edge and an outer-edge lean of 4° at 4K.
- **Distant** (tilt +1) does the opposite: the receptor end recedes.
- **Incoming** and **Space** add skew. **Overhead** is flat.

So StepMania's strongest stock perspective is far milder than Chunithm (0.24) or SDVX (0.106).

Readability choices that matter for us:

- Judgement and combo are drawn **outside** the perspective, upright, at fixed screen offsets.
- Hold bodies are quad strips in 16 px steps.
- Speed is pixels of field per beat. **Tilting therefore changes the visible time**: Hallway shows 38% more field than
  Overhead at the same speed mod. SPEC 3 avoids this by defining speed as visible time.

## Scope, evidence and licences

- **StepMania** (stepmania/stepmania), branch `5_1-new` at `825467b` (2026-08-21) [S1]. The README states "All of our
  source code is under the MIT license". Songs are CC-NC, and the MAD/FFmpeg builds are GPL; we read neither.
- **Etterna** (etternagame/etterna), `master` at `2d70614` (2026-09-07) [E1]. MIT: "Copyright (c) 2016-2023
  Etterna", plus StepMania's MIT notice.
- **No code was copied.** The screen numbers below are projected by our own script, which re-implements the
  transforms described in *Camera and track* for a 16:9 screen.
- **The code is the only source.** No official manual describes the mods' geometry. Community pages found by search
  (flashflashrevolution threads) did not describe them either.

## Camera and track

### The two numbers and the named mods

[S1 `src/PlayerOptions.cpp:86-87, 538-560, 1105-1115`]

| Mod | Tilt | Skew | Notes |
|---|---|---|---|
| Overhead (default) | 0 | 0 | flat |
| Hallway | −level | 0 | receptor end near, notes come from far away |
| Distant | +level | 0 | receptor end far, notes come from near |
| Incoming | −level | +level | Hallway, with the vanishing point pulled to the screen centre |
| Space | +level | +level | Distant, with the vanishing point pulled to the screen centre |
| Tilt / Skew | any float | any float | the raw values; mod strings such as "50% hallway" or "200% hallway" scale `level` |

- **Option row.** The player-options row "Perspective" offers exactly these five, with Overhead as the default
  [S1 `Themes/_fallback/metrics.ini:2587-2593`; E1 `Themes/_fallback/metrics.ini:1837-1843`].
- **Tweening.** Each value has a tween speed (`m_SpeedfPerspectiveTilt`), so attacks and mod charts can ease between
  perspectives.

### How the field is placed

[S1 `src/Player.cpp:1632-1636, 1685-1691, 1706-1740`; E1 `src/Etterna/Actor/Gameplay/Player.cpp:1357-1358, 1414-1423, 1452-1468`; identical
logic]

1. **Camera.** `LoadMenuPerspective(45, SCREEN_WIDTH, SCREEN_HEIGHT, vanishX, centerY)`
   [S1 `src/RageDisplay.cpp:573-611`].
   - The camera sits on the screen normal through (vanishX, centerY), at distance D = (W/2)/tan 22.5°.
   - Anything at z = 0 is drawn at its 2D position. Points nearer the camera are magnified away from (vanishX, centerY).
   - At 16:9 (SM5's virtual screen is 854×480): D = 1030 px and the vertical FOV is 26.2°. At 4:3 (640×480): D = 772 px
     and the vertical FOV is 34.5°. **The same mod looks stronger on a 4:3 screen.**
2. **Vanishing point.**
   - vanishX = SCALE(skew, 0.1, 1.0, playerX, SCREEN_CENTER_X). Skew 0 puts it just past the player's own x, by 11% of
     the player-to-centre distance, on the side away from the centre; skew 1 puts it at the screen centre.
   - centerY = the player's y plus the midpoint of the two receptor positions, i.e. the middle of the field.
3. **Rotation.** The note field actor is rotated about x by SCALE(tilt, −1, +1, +30°, −30°), times −1 with Reverse.
   The order is translate, rotate, then scale [S1 `src/Actor.cpp:673-723`; rotation matrix `src/RageMath.cpp:248-258`].
4. **Zoom and offset.**
   - zoom = (1 − 0.5 × mini) × (1 − 0.1 × |tilt|).
   - y offset = −45 px × tilt for tilt > 0, or +20 px × tilt for tilt < 0 (both × −1 with Reverse). A code comment
     says "I don't know why it's done this why, simply preserving old behavior" (`Player.cpp:1714-1717`).
5. **Draw distance.**
   - The field draws notes up to SCREEN_HEIGHT × (1 + 0.5|tilt|) × (1 + |mini|) px before the receptors
     [S1 `src/NoteField.cpp:750-772`; metric `DrawDistanceBeforeTargetsPixels=SCREEN_HEIGHT`, `Themes/_fallback/metrics.ini:1142`].
   - It draws 128 px past them (`DrawDistanceAfterTargetsPixels=-128`, `:1143`).
   - `ArrowEffects` also lengthens its notion of field height by 200 px × |tilt| (`src/ArrowEffects.cpp:90-93`).

### Projected screen geometry (16:9, one centred player, 4 columns)

Receptors are at ±144 px from the field centre in the `_fallback` theme (`metrics.ini:1127-1128`; Etterna `_fallback`
is the same, Til Death uses ±164). A 4K field is 4 × 64 = 256 px.

"Far/near" is measured where notes enter, which is the screen edge in every stock case. "Speed ratio" is on-screen
speed at the receptors divided by speed where notes enter. "Visible field" is how many field pixels fit between the
receptors and the screen edge.

**Reverse** (receptors at the bottom, as in mania):

| Mod | Rotation | Receptors at | 4K width at receptors | Far/near | Speed ratio | Visible field | Visible time vs Overhead |
|---|---|---|---|---|---|---|---|
| Overhead | 0° | 0.800H | 0.300W | 1.00 | 1.00 | 384 px | 1.00 |
| Hallway 50% | −15° | 0.807H | 0.295W | 0.90 | 1.23 | 435 px | 1.13 |
| **Hallway** | **−30°** | **0.794H** | **0.288W** | **0.80** | **1.55** | **530 px** | **1.38** |
| Distant 50% | +15° | 0.811H | 0.276W | 1.10 | 0.82 | 413 px | 1.08 |
| Distant | +30° | 0.808H | 0.254W | 1.24 | 0.65 | 466 px | 1.21 |
| Hallway 200% (mod string) | −60° | 0.725H | 0.266W | 0.58 | 2.94 | 960 px (draw distance; notes pop in at 0.115H) | 2.50 |

- **Standard** (receptors at the top) mirrors these: receptors at about 0.2H, same widths and ratios.
- **Mini 50%** shrinks everything by 0.75 about the field centre (the receptors move to about 0.73H) and doubles the
  draw distance.
- **Skew for an off-centre player.**
  - With P1 at x = W/4, Incoming tilts the field's centre line 6.7° towards the screen centre, and Space 7.0° away.
  - Hallway and Distant (skew 0) stay within 1° of vertical.
  - With one centred player, skew changes nothing.

**Lean of the outer lane edge at the receptors under Hallway:**

| Field | Width | Lean |
|---|---|---|
| 4K | 256 px | 3.7° |
| 7K (Etterna kb7) | 448 px | 6.4° |
| 10K (pump-double) | 480 px | 6.8° |

The lean grows with field width, because SM widens the field with the key count (see *Key count*).

## Note approach and speed

- **Position along the field** [S1 `src/ArrowEffects.cpp:463-516`; `Themes/_fallback/metrics.ini:196`]:
  - y offset = (displayed beat of the note − displayed beat now) × `ArrowSpacing` (64 px) × the scroll speed (XMod).
  - CMod/MMod replace the beat difference with seconds × BPM/60 at a constant or capped BPM.
  - It is linear in field space and is then projected. SM never corrects the screen speed.
- **Visible time.**
  - Overhead, Reverse, CMod C: 384 px / (64 × C / 60) = 360/C s. C600 gives 600 ms and C800 gives 450 ms.
  - Hallway at the same CMod: 530 px → 497/C s, 38% longer.
  - Til Death's ±164 receptors add 5%.
- **Speed is per field pixel, not per visible time**, so changing the perspective, Mini or the receptor position changes
  how long a note is visible. Etterna players set speed per key mode in the theme for this reason (inference).

## Judgement line and receptors

- **Receptors, hit explosions and the lane board are inside the note field** [S1 `src/NoteField.h:110-111`;
  `src/NoteField.cpp:61-64, 136-137, 313-328`].
  - They tilt and shrink with it.
  - At Hallway the receptor row ends up 4% narrower than at Overhead, because the 0.9 zoom outweighs the
    magnification of being nearer the camera. At Distant it is 15% narrower.
- **No separate judgement line** in the stock themes: the receptor row is the line.
- **Judgement and combo text are drawn after the positioner has popped the perspective**, so they stay upright and
  unprojected [S1 `src/Player.cpp:1650-1682`].
  - Default placement: judgement at y = −30 px and combo at +30 px from the player's centre (signs flipped with
    Reverse), i.e. 6% of the screen height above and below mid-screen, nowhere near the receptors
    [S1 `Themes/_fallback/Scripts/02 Other.lua:45-75`].
  - `TapJudgmentsUnderField` and `ComboUnderField` (both false) choose whether they draw under or over the field
    (`metrics.ini:1144-1146`).
- **Etterna's Til Death** moves the receptors to ±164 px. It lets the player drag judgement, combo, error bar and note
  field to any position and size, **saved per key mode** [E1 `Themes/Til Death/metrics.ini:107-108`;
  `Themes/Til Death/Scripts/02 CustomizeGameplay.lua:10-45`].

## Lane covers / walls

- **Sudden and Hidden** are appearance mods defined in field space [S1 `src/ArrowEffects.cpp:1018-1100`].
  - A centre line 160 px of field from the receptors (divided by the Mini zoom), with a 40 px fade.
  - Sudden hides notes farther than 200 px and fades them in between 200 and 160 px. Hidden fades them out between
    160 and 120 px and hides them nearer than 120 px.
  - SuddenOffset and HiddenOffset move the line by fractions of 160 px.
  - Because they are in field space, they tilt and foreshorten with the field. They are the same kind of thing as our
    track-space wall.
- **Etterna Til Death** adds a theme lane cover: sudden or hidden, a height setting (default 10), and a BPM/height
  readout [E1 `Themes/Til Death/Scripts/01 player_config.lua:87-88`; `BGAnimations/ScreenGameplay overlay/lanecover.lua`].
  It is drawn by the theme as a screen-space overlay (its geometry under tilt was not checked; unverified).
- **Screen filter.** Til Death draws a darkening "screen filter" behind the note field (default 1) [`01 player_config.lua:72`].

## Notes and holds (shapes, rendering)

- **Tap notes** are noteskin sprites or models drawn flat on the rotated plane. The GPU does perspective-correct
  texturing.
- **Tiny** shrinks each note by 0.5^tiny and pulls the columns together by the same factor; it never pushes them apart
  (`TinyPercentGate=1`) [S1 `src/ArrowEffects.cpp:855-866, 1340-1360`; `metrics.ini:249-250`].
- **Mini** scales the whole field instead (zoom 1 → 0.5).
- **Hold bodies** are built as one quad strip per hold part (top cap, body, bottom cap) [S1 `src/NoteDisplay.cpp:726-747, 760-1090`].
  - The strip is stepped every **16 px of field**, or every 4 px when wave-type mods bend the path (`:1136`).
  - Each row has three vertices (left, centre, right), sent as a symmetric quad strip.
  - Heads, bodies and caps share the plane, so they line up at every depth.

## Key count and lane layout

- **Column spacing, by game type** [S1 `src/GameManager.cpp:136, 562, 914-934, 1586`; E1
  `src/Etterna/Singletons/GameManager.cpp:163, 395, 559-579`]:

  | Game type | Spacing |
  |---|---|
  | dance | 64 px |
  | pump | 48 px |
  | kb7 | 64 px |
  | beat (IIDX-style) | 34 px |
  | popn | 32 px |

- **StepMania 5's kb7 leaves a gap around the centre column.** Columns sit at ±3.25, ±2.25, ±1.25 and 0 spacings, i.e.
  a quarter-column gap each side of the space-bar column, so the field reads 3 | 1 | 3. **Etterna made kb7 uniform**
  (±3, ±2, ±1, 0).
- **beat** puts the scratch 1.5 spacings from the nearest key, a half-column gap.
- **Fit to the screen.** ScreenGameplay shrinks a field only when it does not fit:
  NotefieldZoom = min(1, available width / style width) [S1 `src/ScreenGameplay.cpp:565-615`]. Otherwise a wider
  style is simply a wider field: 7K is 0.525W (Etterna, uniform) or 0.56W (SM5, with its gaps), and 10K pump-double
  0.56W at 16:9, against 0.30W for 4K.
- **Etterna key modes** (Til Death):
  - 3K-10K, 12K and 16K are mapped from steps types (`Themes/_fallback/Scripts/00 Utility.lua:319-337`).
  - Note field width, height and **spacing** are player settings saved **per key mode** (defaults 1.0, 1.0, 0.0)
    [`01 player_config.lua:55-57`; `02 CustomizeGameplay.lua:37-39`].
- **Combo-break highlight** (Til Death, `CBHighlight`, default off): the column of a combo-breaking judgement flashes
  at 40% alpha in the judgement's colour, faded at both ends, over 0.25 s
  [`Themes/Til Death/Graphics/Notefield board.lua:53-118`].

## Feedback (judgement display, hit effects, lane lighting)

- **Explosions** (GhostArrowRow) are in the field and tilt with it.
- **Judgement and combo** are in screen space (above).
- **Lane lighting** beyond the receptor press animation is theme-specific; Til Death's CB highlight is the only
  per-lane flash we found in these themes.

## What this means for us

All of this is visual: settings, not mods (invariants 1 and 3). These are proposals for the main session to record in
`DECISIONS.md`.

### S1. Keep visible time as the speed (SPEC 3)

- StepMania defines speed in field pixels per beat. Every perspective mod therefore changes visible time: +38% for
  Hallway at the same CMod, +21% for Distant, and Mini changes it too.
- Players compensate with per-key-mode speeds (inference).
- SPEC 3's definition avoids this by construction. **No change**, and this is a good argument to keep it if anyone
  asks for "SM-style" speed.

### S2. Judgements upright and outside the projection (SPEC 7)

- StepMania draws judgement and combo after leaving the perspective, at fixed screen offsets.
- This matches DECISIONS "Judgements are drawn upright, at mania's height above the line". **No change.**
- StepMania puts them at mid-screen (±6% H). Mania's skins put them near the line. Keep mania's.

### S3. Holds as subdivided strips (SPEC 6)

- StepMania steps holds every 16 field px (4 px when bent). With a true 3D plane, heads, bodies and tails line up for
  free.
- **Proposal:** keep our perspective-correct subdivision. If hold bodies are ever textured along their length,
  subdivide in **track depth** so that no segment exceeds about 16 screen px at the line. That is StepMania's step at
  480p; scale it by screen height (36 px at 1080p).

### S4. Receptors: ours stay flat (DECISIONS "The area below the judgement line is drawn at its flat height")

- StepMania tilts receptors with the field. That is harmless at far/near 0.8 over the whole screen: a 64 px receptor
  is distorted by only a few percent between its near and far edge.
- At Chunithm's 0.24 the same approach would magnify the area below the line 4-5×. **Keep our decision.**

### S5. A "mild" perspective recipe (SPEC 2)

- StepMania Hallway with Reverse projects to:
  - judgement line 0.206 above the bottom;
  - far/near 0.80 at the top edge;
  - a 4K field 0.288W wide.
- This is the strongest stock perspective keyboard VSRG players know, and its convergence is far milder than our
  Chunithm (0.24) and Voltex (0.106) presets.
- **No new preset** (SPEC lists four). Documenting it as a Custom recipe is enough: Perspective, far/near 0.8, line
  0.2, far edge 0, width as the preset's.

### S6. Dual stages share one vanishing point (SPEC 5)

- StepMania's skew 1 (Incoming, Space) and YARG's multiplayer tilt both bend side-by-side fields towards one centre.
- Our current single highway spanning both stages (DECISIONS, review fixes) already does this. That supports keeping
  it as the dual-stage default.

### S7. Covers in track space (SPEC 4)

- StepMania's Sudden and Hidden are defined in field space (160 px ± a 40 px fade) and foreshorten with the tilt, as
  our wall does.
- One difference worth copying as an option: StepMania's covers **fade over a band** (40 px, 25% of the 160 px line
  distance), while our wall is a hard edge.
- See `game-yarg-clonehero.md` Y3 for a far fade. It should start at the wall's edge when the wall is on.

## Unverified / open questions

- How the stock noteskins' receptor and explosion sprites look under tilt. Not measured: no frame was rendered.
- Til Death's lane cover under a tilted field (screen space or field space).
- That players actually set speeds per key mode to keep visible time. No source found; it is an inference from the
  per-key-mode settings.
- Community descriptions of the mods: none found that state the geometry. The directions given here are derived from
  the code.

## Sources

Commit hashes are the versions read. Both projects are MIT. Nothing copied.

- [S1] StepMania, MIT (README "Licensing Terms"), `5_1-new` at `825467bcd81c812b33ad684dc04dd151b2d5dec3`:
  https://github.com/stepmania/stepmania/tree/825467bcd81c812b33ad684dc04dd151b2d5dec3.
  - `src/PlayerOptions.cpp:86-87, 538-560, 861-864, 1105-1115, 1290-1294`: mods.
  - `src/Player.cpp:1619-1682, 1685-1691, 1706-1740`: draw order, camera push, positioner.
  - `src/RageDisplay.cpp:573-611`: LoadMenuPerspective. `src/RageMath.cpp:248-258`: rotation.
    `src/Actor.cpp:673-723`: transform order.
  - `src/ArrowEffects.cpp:90-93, 463-516, 855-866, 1018-1100, 1340-1360`: field height, y offset, Tiny, Sudden/Hidden,
    zoom.
  - `src/NoteField.cpp:61-64, 136-137, 313-328, 750-772`: board, receptor and ghost rows, draw distance.
    `src/NoteField.h:110-111`.
  - `src/NoteDisplay.cpp:726-747, 760-1090, 1136`: hold strips.
  - `src/ScreenGameplay.cpp:565-615`: field zoom to fit.
  - `src/GameManager.cpp:136-166, 562, 914-934, 1586-1640`: column layouts.
  - `Themes/_fallback/metrics.ini:23-24, 188-196, 249-250, 1126-1146, 2587-2593`: metrics.
  - `Themes/_fallback/Scripts/02 Other.lua:45-75`: judgement and combo transforms.
- [E1] Etterna, MIT (`LICENSE`), `master` at `2d706147cc4bcf95a8ceb24dc4f4129be207f803`:
  https://github.com/etternagame/etterna/tree/2d706147cc4bcf95a8ceb24dc4f4129be207f803.
  - `src/Etterna/Actor/Gameplay/Player.cpp:1357-1358, 1414-1468`: same positioner.
  - `src/Etterna/Actor/Gameplay/ArrowEffects.cpp:120`; `src/Etterna/Actor/Gameplay/NoteField.cpp:711`.
  - `src/Etterna/Singletons/GameManager.cpp:47-60, 163, 395, 559-579`: steps types, spacing, uniform kb7.
  - `Themes/_fallback/metrics.ini:686-687, 1837-1843`.
  - `Themes/Til Death/metrics.ini:107-108`.
  - `Themes/Til Death/Scripts/01 player_config.lua:55-57, 72, 87-95`; `02 CustomizeGameplay.lua:10-45`;
    `02 ThemePrefs.lua:9-90, 615-645`.
  - `Themes/Til Death/Graphics/Notefield board.lua:1-118`; `Themes/_fallback/Scripts/00 Utility.lua:319-335`.
