# Gameplay spec: maniaortho

The source of truth for what the playfield must look and feel like. Numbers marked *(tune)* are starting points to be
refined by research and playtesting; record changes in `DECISIONS.md`. Items marked **[Alpha]** are pending Alpha's
answer in `QUESTIONS.md` and use the stated default until then.

Alpha's own words, which every item below must respect:

> The actual game itself should behave very similarly to the designs of SDVX, Project Sekai, Chunithm, and other
> orthographic VSRGs. I include SDVX as the primary choice since it uses actual buttons and fixed laning (which is the
> feature I want to preserve from osu mania!). Each note should approach from the top of the screen towards the bottom,
> rather than from the center, but add an option to add walls (similar to chunithm) where you can adjust where the
> notes actually come from on the screen. The general user interface (song selection, menu, etc.) can stay the same.

## 1. Model

The playfield is mania's lane-and-time model with a camera on top. Mania already turns each note's time into a
distance along its lane (including scroll velocity changes). This ruleset keeps that distance exactly and only changes
how a point at (lane position, distance) is placed on screen.

- **Track space**: x = across lanes (0 at the left edge of lane 0, 1 at the right edge of the last lane),
  d = distance from the judgement line along the track (0 at the line, 1 at the far edge of the visible track).
- **Projection**: a pure function `TrackToScreen(x, d) -> screen point` with no game state, unit-tested on its own.
- Everything drawn on the track (notes, holds, bar lines, lane dividers, the wall) is defined in track space and
  projected. HUD, judgement text and combo stay in screen space.

## 2. Camera

| Mode | Look | Use |
|---|---|---|
| **Perspective** (default **[Alpha]**) | True 3D projection. Lanes converge toward a vanishing point above the top of the screen; far notes are smaller and move slower on screen. | SDVX / Chunithm look |
| **Parallel tilt** | Track foreshortened vertically, lanes stay parallel (no convergence). Literally orthographic. | Readability at high key counts |
| **Flat** | Identical layout to osu!mania. | Baseline, and a sanity check that the projection path is correct |

Requirements:

- Notes enter at the **top edge of the screen** (or at the wall, section 4), never from a point in the middle of the
  screen. In perspective mode the vanishing point therefore sits at or above the top of the screen, so the far end of
  the track is a visible edge with real width, not a point.
- The judgement line sits near the bottom: default 12% of screen height above the bottom edge *(tune)*.
- Parameters: tilt angle (0-75 deg, default 55 *(tune)*), field of view (30-90 deg, default 60 *(tune)*), far-edge
  width as a fraction of near-edge width (perspective only, derived from tilt/FOV, typically 0.3-0.6), track width at
  the judgement line (fraction of screen width; default from key count, section 5).
- Presets: **Voltex** (perspective, strong tilt, narrow far end), **Chunithm** (perspective, gentler tilt, wider far
  end, wall on), **Parallel** (parallel tilt), **Classic** (flat). Preset values come from the research notes and are
  recorded in `DECISIONS.md`.
- No camera motion driven by beatmap data. Optional cosmetic motion (for example a small sway in kiai) is allowed later,
  off by default, and must never move the judgement line.

## 3. Scroll speed

- Speed is defined as **visible time**: the milliseconds a note takes from appearing (far edge or wall) to reaching
  the judgement line. This matches mania's existing scroll speed setting (which maps to a time range) and IIDX's
  "green number", so a speed feels the same in every camera mode.
- Notes move linearly in **track space** (d is linear in time at constant SV). Under perspective they therefore move
  slowly far away and accelerate near the line. That is correct and expected (it is what SDVX does); do not "correct"
  it in screen space.
- Scroll velocity (SV) changes behave exactly as in mania (same distance algorithm, same settings for constant /
  overlapping / sequential).
- Upscroll is not offered in this ruleset (the camera defines direction). Mania's scroll direction setting is ignored
  here, and that is recorded as a decision.

## 4. Walls

- **Top wall** (Chunithm's field wall, IIDX's SUDDEN+): an opaque cover over the far part of the track. Notes appear
  at the wall's edge. Position: 0-80% of the track length *(tune)*. Drawn in track space (it is a plane on the track
  or a vertical panel at that depth; pick whichever reads better in playtests).
- **Bottom lift** (IIDX's LIFT): optional, raises the judgement line. Off by default.
- **Constant visible time by default**: moving the wall changes on-screen speed so that visible time stays the same.
  A toggle "keep speed when moving the wall" switches to keeping on-screen speed instead (visible time shrinks).
- Walls are ruleset **settings**, adjustable from settings and with a hotkey during gameplay (Shift+F3 / Shift+F4).
  They never change judgement or scoring, so they are not mods.

## 5. Lanes and key modes

- 1K-10K single stage are required. Dual stage (mania's 2x stages) is drawn as two highways side by side *(tune)*.
- The track keeps one width for every key count, as Chunithm's field does: more keys means narrower lanes, and the
  outermost lanes lean the same at 4K and 10K (Alpha, QUESTIONS Q10). Each preset sets the width.
- Fixed lanes, one key per lane, same default bindings as mania. Key presses light the lane (SDVX-style beam rising
  from the judgement line) and the receptor at the line.

## 6. Notes, holds, bar lines

- Notes are rectangles in track space with a fixed thickness in track units, drawn as projected trapezoids. They shrink
  with distance in perspective mode.
- Hold bodies are one projected quad from head to tail (subdivided if needed for texture correctness). Heads, bodies
  and tails must line up exactly at every depth: a misaligned hold is a visible bug.
- Textures on trapezoids use perspective-correct mapping. Two plain triangles show a bend along the diagonal.
- Bar lines are projected across the full track width.
- A note's leading edge must cross the judgement line exactly at its hit time (same as mania). Unit-test this for
  every camera mode.

## 7. Feedback and HUD

- Judgement text, combo and hit error meter stay in screen space near the judgement line, readable at all tilts.
- Hit lighting and explosions are drawn at the judgement line in the lane's projected position.
- The osu! HUD (score, accuracy, pp counter, progress) is unchanged.

## 8. Skins

- Ship a built-in highway skin first (simple, high contrast, SDVX-like colours per lane type).
- Later, map legacy mania skin note and hold textures onto projected quads. Stage decorations that cannot be projected
  are ignored.

## 9. Mods

- Conversion and difficulty mods (Mirror, Random, key mods, DT/HT, HR/EZ, etc.) work unchanged.
- Visibility mods (Hidden, Fade In, Cover, Flashlight) need projected equivalents or are removed from this ruleset's
  mod list. Whichever is chosen per mod goes in `DECISIONS.md`. They must still score exactly like mania.

## 10. Performance

- Target: no dropped frames at 240 Hz at 1080p on Alpha's PC on dense 7K maps. No allocations per frame in the
  playfield. Measured only on device (L3), never in the cloud.

## 11. Acceptance checks (how each section is verified)

| Section | Check | Level |
|---|---|---|
| 1, 6 | `TrackToScreen` unit tests: lane edges, judgement line, far edge, monotonic depth, note crosses line at hit time | L0 |
| 3 | Visible time equals the configured time range in every camera mode (unit + screenshot at t = hit - visible time) | L0, L2 |
| 4 | Moving the wall keeps visible time constant (default) or speed constant (toggle) | L0, L2 |
| All | Replay parity: identical judgements, score and accuracy to mania | L1 |
| 2, 5, 6 | Screenshots of 4K and 7K at fixed times pass the rubric in `PLAYTESTING.md` | L2 |
| 10 | Frame times on Alpha's PC | L3 |
| Feel | Alpha's playtest notes | L4 |
