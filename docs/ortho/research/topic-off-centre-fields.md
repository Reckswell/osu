# Off-centre note fields: where they sit, and where their perspective converges

Last checked: 2026-11-04

## Summary

Alpha's question: when a skin puts its stage off-centre (R Skin left, Aristia right, Starcruiser slightly left; see
`skins-survey.md`), how do other games handle an off-centre field? Alpha wants the tilted lane centred on the same x as
the original stage.

1. **Placement is common.** Most keyboard VSRGs let the field sit off-centre, either by play side or by a setting.
   - IIDX, beatoraja, StepMania and Pump place it by **play side** (1P left, 2P right).
   - DJMAX, Bemuse and Quaver/osu! skins place it by a **setting**: Left/Center/Right, or a pixel offset.
   - StepMania 5, Bemuse and DJMAX default to centred, or offer centring. Etterna centres by default
     (`Center1Player` = true).
2. **Perspective plus off-centre is rare.** Only four games tilt an off-centre field.
   - **StepMania/Etterna**: the vanishing point follows the field (skew 0) or the screen centre (skew 1).
   - **Etterna Til Death**: moving the field leaves the vanishing point at the screen centre.
   - **YARG multiplayer**: each camera moves sideways, so the outer highways lean towards the centre.
   - **Rock Band**: by YARG's account, it does the same.
   - Bemuse offers "3D" only centred. beatoraja has no perspective at all.
3. **No reference keeps the judgement line exactly in place.** StepMania's receptor row drifts 0.1-1.7% W. YARG's
   strikeline moves 1.7-5.7% W towards the centre (our model). Our projection can pin the line exactly at the flat
   centre whatever the vanishing point, which is what Alpha asks for.
4. **When a field does not fit, references keep the side and shrink, or re-centre. None clamps sideways.**
   - StepMania 5 re-centres the field, then shrinks it to fit.
   - YARG caps each highway's width.

## Scope, evidence and licences

- Source code read: StepMania 5.1 and Etterna (MIT), YARG (LGPL-3.0), beatoraja (GPL-3.0), Bemuse (AGPL-3.0) and
  Quaver (MPL-2.0).
  - GPL, LGPL, AGPL and MPL code was read for numbers and behaviour only. **Nothing was copied.**
  - The numbers marked "projected" come from our own re-implementations (scratch scripts, not committed). They follow
    the transforms described here.
- Official text was used for IIDX (KONAMI option page), and earlier notes for DJMAX ([D2], [D6] in `game-djmax.md`).
- **No images or video were downloaded**, so arcade layouts (IIDX, Pump, DJMAX) have no pixel measurements. Their
  fractions of the screen are marked unverified.
- For Pump It Up, no official or wiki text on field position or perspective options was found. Only StepMania's pump
  emulation was read.

## Where the field sits, and who chooses

| Game | Default | Choices | Who chooses | Source |
|---|---|---|---|---|
| beatmania IIDX (arcade, INFINITAS) | SP: the side you play | SP 1P = lanes at the left of the screen, 2P = at the right; DP: both sides, at the screen edges | the side you start on. **FLIP** (DP) swaps the left and right *charts*, not the fields ("左右の譜面がそのまま入れ替わります"). The official option page lists no lane-position option | [K1]; positions (unverified, no frame measured) |
| beatmania (5-key) | the side you play | **Center** modifier "literally 'centers' the playfield" in 1P; Center Double; Double (fields "on opposite sides of the screen") | modifier | [G1] |
| beatoraja (default skin, IIDX-like) | 1P | skin option "Play Side" 1P / 2P. **1P lanes x 20-410 of 1280: centre 0.168W, width 0.305W. 2P lanes 870-1260: centre 0.832W.** BGA 800×650 fills the other side | skin option | [B1] `play7main.lua:8-12, 147-219` |
| Bemuse | **Center** | Panel: Left / Center / Right / **3D [Beta]**. Note-area centres 0.137W / 0.498W / 0.861W (panel x 0, 455, 919 of 1280, plus the area offset and half its width). 3D is always centred and keyboard-only: choosing it switches to keyboard mode, and leaving keyboard mode turns it back to Center | setting | [BM1] |
| StepMania 5 (`_fallback`) | 1 player on its side | P1 at **0.283W** (`0.85/3`), P2 at **0.717W** (`2.15/3`). Doubles and shared styles at 0.5W. The `Center1Player` preference (default false) centres one player | side and preference; **auto-centres** a style too wide for its half (see *Fitting*) | [S1] |
| Etterna | **centred** (`Center1Player` default **true**) | Til Death: the notefield X is draggable in Customise Gameplay, **saved per key mode**. Its lane cover assumes a centred field at ≥ 6 columns | theme setting | [E1] |
| Pump It Up (arcade) | (unverified) single field on the side of the pad you stand on; Double centred | (unverified) | (unverified) | none found. StepMania's pump emulation: pump-single is one side (0.283W / 0.717W), pump-double centred [S1] |
| DJMAX Respect V | **Center** | GEAR POSITION Left / Center / Right | setting | `game-djmax.md` [D2][D6] |
| Quaver | centred | skin value `ColumnAlignment`: a pixel x offset of the playfield from top centre, default 0 | skin | [Q1] |
| osu! stable | centred (`ColumnStart` 136 at 4K) | skin value `ColumnStart`, per key count: R Skin's 4K stage centre at 0.356W, Aristia 6K-8K at 0.52-0.54W, Starcruiser 0.40-0.45W | skin | `skins-survey.md` |
| osu! (lazer) | always centred | parses `ColumnStart` but ignores it | none | `skins-survey.md` |
| Rock Band, Guitar Hero, Clone Hero, YARG | one highway centred | several highways side by side, one slot each | number of players | [Y1], [Y2] |

**Pattern.** Off-centre placement exists to leave room for something: the BGA (IIDX, beatoraja), a second player
(every game), or the player's preference (DJMAX, Bemuse, Etterna, skins).

- Every game whose default is "your side" also offers a way to centre: Center1Player, beatmania's Center, Bemuse and
  DJMAX's Center.
- **Inference:** off-centre is a deliberate layout choice that players opt into. That supports keeping a skin's
  `ColumnStart` (our current default), since the skin author chose it.

## Perspective on an off-centre field: where is the vanishing point?

### StepMania 5 / Etterna (MIT)

The camera and transforms are described in `game-stepmania-etterna.md` (*How the field is placed*). The positioner is
called with the **Player actor's** x: `PlayerNoteFieldPositioner poser(this, GetX(), ...)`
([S1] `Player.cpp:1645, 1669`; [E1] `Player.cpp:1369-1370, 1392-1393`). Its vanishing x is
SCALE(skew, 0.1, 1.0, playerX, SCREEN_CENTER_X).

Projected for a 4K field with Reverse on an 854×480 screen. Each row gives where the field's centre line is at the
receptor row and at the top screen edge, relative to its flat x.

| Field at | Mod (tilt, skew) | Vanishing x | Receptor centre | Top-edge centre | Centre-line lean |
|---|---|---|---|---|---|
| P1 0.283W | Overhead (0, 0) | (none) | 0 | 0 | 0° |
| P1 0.283W | **Hallway (−1, 0)** | 0.258W (just outside the field's centre) | **+0.16% W** | −0.35% W | 0.7° outward |
| P1 0.283W | Distant (+1, 0) | 0.258W | −0.14% W | +0.40% W | 0.7° inward |
| P1 0.283W | **Incoming (−1, +1)** | **0.5W** | **−1.46% W** (outward) | **+3.14% W** (inward) | **5.9° inward** |
| P1 0.283W | Space (+1, +1) | 0.5W | +1.29% W | −3.57% W | 6.1° outward |
| P1 0.283W | Hallway, skew 0.5 (mod string) | 0.379W | −0.65% W | +1.39% W | 2.6° inward |
| 0.356W (R Skin's centre) | Hallway | 0.340W | +0.11% W | −0.23% W | 0.4° |
| 0.356W | Incoming | 0.5W | −0.97% W | +2.07% W | 3.9° inward |

At P1 = W/4 the same model gives Incoming 6.7° and Space 7.0°, matching `game-stepmania-etterna.md`.

**Answers to the three questions:**

- **Q2.** Skew 0 (Hallway, Distant) is option (a): the vanishing point is above the field's own centre, offset 11% of
  the way outward. Skew 1 (Incoming, Space) is (b): the screen centre. The "50% incoming"-style mod strings give (c),
  in between.
- **Q3. The receptors do not stay put.**
  - The field rotates about its *middle* (`centerY` = the midpoint of the receptor positions), not about the receptor
    row. The receptor end therefore comes off the z = 0 plane and is magnified away from the vanishing point.
  - At skew 0 that drift is ≤ 0.2% W (under 2 px at 854). At skew 1 the receptor row moves 1-1.7% W **outward**,
    while the far end moves 2-4% W inward.
- **Etterna Til Death's custom field position.**
  - Til Death moves the **NoteField inside the Player**: `Notefield:addx(MovableValues.NotefieldX)`
    [E1 `WifeJudgmentSpotting.lua:192-194`]. The Player stays at the screen centre, because `Center1Player` is
    true by default.
  - So a field the player drags sideways keeps the camera at the screen centre. Under plain Hallway it then leans
    towards the centre like Incoming: 3.9° at 0.356W, with the receptors 1% W outward of the dragged position.
  - Til Death's lane cover is a screen-space overlay placed at `SCREEN_CENTER_X + NotefieldX`
    [E1 `lanecover.lua:31-35`], i.e. at the dragged x. It does not follow the lean (inference from the code; not
    rendered).

### YARG multiplayer (LGPL-3.0), and Rock Band by YARG's account

How it works:

- **One camera per highway.** Highway i of n gets a camera moved sideways by `m·(−1 + 2i/(n−1))` world units, with
  m = −`HighwayTiltMultiplier` (0-0.85, default 0.5). A track is 2 units wide.
- **Shift back in screen space.** The image is then shifted back in NDC by the same expression with m/n, as a pure
  screen-space translation [Y1 `HighwayCameraRendering.cs:405-421, 515-538, 625-638`].
- **Result.** Each outer highway's vanishing point moves towards the screen centre (option (b)-like, partial).
- **Rationale.** The PR that added it ([Y2], #1172, 2025-10-20): it "Angles the highways like in Rock Band" to
  "make the spacing between the edges of the screen and each highway roughly equal". The developer adds that at 1.0
  "the leftmost highway's right edge is fully vertical"; at 0 there is no tilt.

Projected with the Default camera (own pinhole model; the screen-fit scale is approximate, the lean is not):

| Players | Tilt | Outer highway: slot centre → strikeline centre → far-end centre | Centre-line lean | Inner highways |
|---|---|---|---|---|
| 2 | 0.5 (default) | 0.250W → **0.307W** → 0.345W | **9.7°** inward | n/a |
| 3 | 0.5 | 0.167W → 0.189W → 0.223W | 9.7° | middle one upright |
| 4 | 0.5 | 0.125W → 0.142W → 0.167W | 9.7° | 3.3°, strikeline +0.6% W |
| 4 | 0.85 (max) | 0.125W → 0.153W → 0.196W | 16.3° | 5.6° |

- **Lean.** It depends only on the camera shift in track widths, not on the screen-fit scale, so every outer highway
  leans the same at a given tilt.
- **Strikeline.** It is **not** pinned: the compensation is per highway count, not per projected width, so it moves
  1.7-5.7% W towards the centre.
- Rock Band's own geometry was not measured (no frame). Only YARG's statement supports "like Rock Band".

### Bemuse (AGPL-3.0)

- "3D [Beta]" is a fourth panel placement next to Left, Center and Right, not a modifier on them.
- Its renderer hard-codes the play area at x = 1280/2 [BM1 `touch3d.js:79-80`, `player-display.js:87, 151`]. So
  Bemuse never draws an off-centre field in perspective.
- The 3D panel is a curved arc projected with a CSS-style perspective of 960 px [BM1 `touch3d.js:5-48`].

### beatoraja (GPL-3.0)

- Lanes are flat 2D skin objects.
- A GitHub code search of exch-bms2/beatoraja for "perspective" returned nothing (2026-11-04). No 3D lane option was
  found.

### Clone Hero, Guitar Hero, Pump, IIDX, DJMAX

- No perspective on an off-centre field was found or sourced.
- IIDX, DJMAX and Pump fields are flat.
- Clone Hero multiplayer geometry is not documented; it is closed source (unverified).

## Readability practices for off-centre fields

- **Judgement and combo sit on the field's centre, not the screen's.**
  - beatoraja: the judgement is 180 px wide at x 115, so its centre is 205 against a lane centre of 215 (1280 wide)
    [B1 `play7main.lua:159, 553-573`].
  - Bemuse: combo at x 141 inside the note area [BM1 `skin_screen.xml:1900`].
  - StepMania: the judgement and combo are the Player's children, at its x (30 px above and below its centre)
    [S1 metrics].
- **The other side gets the BGA and the score panels.**
  - beatoraja: 1P puts the BGA at x 440-1240, the judge count at 476, the judge graph at 740-1190; the 2P layout
    mirrors this [B1 `:169-219`].
  - StepMania 5 default: score at the player's side x (0.283W), or at the screen centre with Center1Player
    [S1 `Themes/default/metrics.ini:1004-1025`].
- **Lane covers follow the field's x** (Etterna Til Death overlay, above).
- **Even margins.** YARG leans outer highways in, so the screen edge and each highway look evenly spaced [Y2].
- **Centring as the fallback for wide fields.** StepMania 5 centres a field that does not fit its side [S1]. Til
  Death's lane cover assumes a centred field at ≥ 6 columns [E1 `lanecover.lua:15`].

## Fitting a field that would leave the screen

- **StepMania 5** [S1 `ScreenGameplay.cpp:565-614`; `_fallback/Scripts/03 Gameplay.lua:41-77`].
  - Each side has a field space of 272 px (the theme's margins around the side's x).
  - If the style is wider than that and one player is playing (`AllowCenter1Player`, true in `_fallback`), the field
    is **moved to the centre**, where the space is 544 px.
  - Then `NotefieldZoom = min(1, field_space / style_width)` **shrinks** it about its own centre.
  - It is never shifted sideways by just enough to fit. dance-single (4 × 64 = 256 px) stays on its side; kb7 and
    solo (≥ 384 px) are centred.
- **Etterna.** `NotefieldZoom` is always 1 [E1 `ScreenGameplay.cpp:244`]. The player sizes and places the field per
  key mode instead.
- **YARG.** With several highways, each one's width is capped at min(0.45W, 0.9 × W/n), and its height at 0.55H, by
  a uniform scale [Y1 `:20-33, 216-248`].
- **osu! stable.** `ColumnStart` is measured from the left edge in 480-unit-high space.
  - The stage's centre as a fraction of the width therefore changes with the aspect ratio. R Skin 4K: 0.475W at 4:3,
    0.356W at 16:9, 0.27W at 21:9.
  - Our `SkinPortLayout.Adjust` reproduces this anchoring.
- **Inference.** No reference keeps a field on its side by sliding it inwards just enough to fit, which is what our
  clamp does now. They keep the position and scale, or they re-centre.

## What this means for us

All of this is visual: these are settings, not mods (invariants 1 and 3).

They extend SPEC 8 ("the skin's stage position (`ColumnStart`) moves the track sideways by default") and the
2026-11-04 `SkinStageFit` decision. Today's behaviour:

- `SkinPosition` moves `CentreX` to the skin's stage centre and clamps it so the track stays on screen
  (`SkinPortLayout.cs:71-78`).
- The trapezoid is symmetric about `CentreX` (`HighwayProjection.cs:103-117`).

### O1. Where the track's centre goes (i)

- **Keep it as now:** the track's centre at the judgement line = the flat stage's centre, computed from
  `ColumnStart` + half the skin's stage width, in the same 480-unit, left-anchored space as stable.
- No reference does better, and Alpha asked for it.
- **Suggested gate.** A unit test that the projected centre of the judgement line (lane 0.5, depth 0) equals the flat
  stage's centre within 0.5 px, for every camera mode and every vanishing-point option below. This holds by
  construction in our projection, unlike StepMania, where the receptors drift 0.1-1.7% W, and YARG, 1.7-5.7% W.

### O2. Which vanishing point (ii): three renderable options

Let k be how far the vanishing point moves from the track's own centre towards the screen centre (0-1).

- **Formula.** With `HighwayLayout.Sway` (already a shear about the judgement line that keeps the line and everything
  below it fixed): **Sway = k · (0.5 − CentreX) · (1 − FarWidthRatio)**.
- **Why it works.** The vanishing point sits at a height 1/(1 − r) far-edge heights above the line, so this puts it
  at lerp(CentreX, 0.5, k).
- **Composition.** Kiai sway adds on top. Parallel (r = 1) has no vanishing point, so every option draws the same
  there.

| Option | k | Reference | R Skin 4K (0.356W), Chunithm 0.61W, r 0.24 | R Skin's widest stage (0.293W), Chunithm | Aristia 8K (0.542W), Chunithm |
|---|---|---|---|---|---|
| **A. Own centre** (now) | 0 | StepMania skew 0 (Hallway/Distant); YARG tilt 0; every flat game | upright centre line; both edges 24.0° | upright; edges 23.2° (at the shrunk width) | upright; edges 24.0° |
| **B. Screen centre** (one shared camera) | 1 | StepMania Incoming/Space; Etterna Til Death dragged field; YARG tilt ≈ 1 | far centre 0.466W, centre line 11.8° inward; outer edge 33.2°, inner 13.3° | far centre 0.450W, 16.8°; outer 36.1°, inner 7.2° | far centre 0.510W, 3.5°; edges 21.0° / 26.8° |
| **C. Halfway** | 0.5 | YARG's default tilt 0.5; StepMania "50% incoming" | far centre 0.411W, 6.0°; edges 28.8° / 18.8° | 8.6°; edges 30.1° / 15.5° | 1.7° |

Voltex (0.435W, r 0.106), for R Skin 4K: B leans the centre line 13.6° (outer edge 31.4°, inner 7.1°), C 6.9°. For
R Skin's widest stage, B leaves the inner edge almost vertical (−1.0°), which is YARG's tilt-1.0 look.

- **Suggested default: A (k = 0).** It is the literal reading of Alpha's request: "the angled lane should have the same
  x coordinates for the centering of the lane". The whole centre line stays at the flat x, at every depth, not only at
  the line. It is also what StepMania's default perspectives do for a side field.
- **Offer B and C** as a setting "Vanishing point: Track centre / Between / Screen centre" (or a 0-100% slider),
  default Track centre. Render A, B and C for R Skin 4K and Aristia 8K with the Chunithm preset, side by side, and
  show Alpha. B is how a single real camera sees a track placed to one side (Rock Band, YARG, Incoming).
- **Ask Alpha (QUESTIONS).** This changes the look's intent, so the default is Alpha's call. Keep A until Alpha answers.
- **Costs of B and C** (inference):
  1. The `Sway` shear leaves the floor and rails below the line unleaned, so lane edges bend at the line (DECISIONS,
     kiai sway). At 3% W that hid under the line's glow. At 5-16% W it will show with the "Floor" and "Squeezed"
     key areas. "Panel" (vertical sides below the line) hides it. Either draw below-line floors with the same lean or
     limit B/C to Panel and Receptors.
  2. Judgements placed above the line follow the leaning centre (`HighwayJudgementLayer` projects them on the track):
     a few px at mania's height. That is fine and matches StepMania.
  3. Lean grows with the distance from the centre, so different key counts of one skin (R Skin 250-320) lean
     differently. That is acceptable for a camera, but say so in the setting's description.

### O3. When the track would leave the screen (iii)

- **The problem.** `SkinPosition` keeps the camera's width (Chunithm 0.61W), which is wider than most skins' stages
  (0.21-0.36W). The track fits with its centre kept only while 2·min(CentreX, 1 − CentreX) ≥ width.
  - Alpha's skins with the Chunithm preset: R Skin 4K (0.356W) needs ≤ 0.713W and fits. Starcruiser (≥ 0.40W),
    Kitsune 10K+ (0.477W) and Aristia 6K-8K (0.52-0.54W) fit.
  - **R Skin's widest-key stages (centre 250/853 = 0.293W) do not fit.** Today the clamp moves them to 0.305W:
    1.2% W, 23 px at 1920, breaking Alpha's "maintain the original position".
  - Voltex (0.435W) and Parallel (0.45W) fit all of them.
- **Proposal: shrink, don't clamp.**
  NearWidth = min(preset width, 2·min(CentreX, 1 − CentreX) − 2·margin). The width scales about the kept centre,
  like StepMania's `NotefieldZoom` and YARG's width cap.
  - Margin 0, or about 0.5% W so the rails' glow stays visible (tune).
  - R Skin at 0.293W then plays at 0.586W instead of 0.61W (−4%, lanes 4% narrower).
  - Visible time is unchanged: it is defined in track space, SPEC 3.
- **Floor for the shrink.** Never narrower than the skin's own flat stage width (`FlatStage.Width`).
  - That stage is on screen in stable by construction, so the floor always fits whenever the skin's own layout does.
  - Only if even that fails (a stage authored off-screen, or an aspect ratio narrower than the author's) clamp as now,
    as a last resort, and log it once.
- **Check the widest drawn row, not the line.** With the "Floor" or "Squeezed" key area the track widens below the
  line: Chunithm's floor at the bottom edge is 1.06× the line width (computed from the homography, line at 0.926H).
  The fit should use the bottom row's width when those key areas are on; Panel's vertical sides need only the line.
- **Not proposed: StepMania's re-centring.** It contradicts "the centre kept". It could be an option "Centre wide
  stages" (off) if Alpha ever wants it.

### O4. Small follow-ups

- **Readability.** The references put judgement, combo and covers on the field's centre. Ours already do:
  `HighwayJudgementLayer` uses the stage centre, and the wall is on the track. **No change.**
- **Background.** IIDX and beatoraja fill the other side with the BGA. Ours keeps osu!'s background dim, so nothing
  to do. A future "fill the free side" idea is not proposed.
- **Dual stage** (`game-stepmania-etterna.md` S6). One shared highway still stands. If dual stages ever become two
  highways, B (or C) is the multi-player convention (YARG, Rock Band, Incoming): the vanishing points lean in towards
  the centre.

## Unverified / open questions

- **IIDX.** The arcade's 1P/2P lane x and width as fractions of the screen; that no centre option exists in recent
  versions (the official option page lists none). DP field positions. Needs one measured arcade frame.
- **Pump It Up.** Single field position (side of the pad) and any perspective option. No source found.
- **DJMAX.** GEAR POSITION fractions of the screen (only the option names are sourced).
- **Rock Band's real highway lean.** Only YARG's PR claims it.
- **Clone Hero's multiplayer layout.**
- **YARG.** The screen-fit scale in our model (lean figures do not depend on it; the strikeline shift does).
- **Etterna Til Death.** That a dragged field leans under perspective is read from the code, not rendered.
- **beatoraja.** That no perspective exists rests on a code search, not a full read.

## Sources

- [S1] StepMania 5.1, MIT, `5_1-new` at `825467bcd81c812b33ad684dc04dd151b2d5dec3`:
  https://github.com/stepmania/stepmania/tree/825467bcd81c812b33ad684dc04dd151b2d5dec3.
  - `src/ScreenGameplay.cpp:97` (`Center1Player` default false), `:565-614` (side and centre placement, centring a
    style too wide for its side, `NotefieldZoom`), `:880-891`.
  - `src/Player.cpp:1645, 1669` (positioner with the Player's x).
  - `src/GameManager.cpp` (style types: pump/dance/kb7/beat single = OnePlayerOneSide, pump-double = OnePlayerTwoSides).
  - `Themes/_fallback/metrics.ini:3503, 3515-3543` (`AllowCenter1Player`, P1 0.85/3, P2 2.15/3, doubles centred);
    `Themes/_fallback/Scripts/03 Gameplay.lua:30-77` (margins, 272/544 px field space);
    `Themes/default/metrics.ini:1000-1025` (score x per side or centred).
  - Camera details: see `game-stepmania-etterna.md`.
- [E1] Etterna, MIT, `master` at `2d706147cc4bcf95a8ceb24dc4f4129be207f803`:
  https://github.com/etternagame/etterna/tree/2d706147cc4bcf95a8ceb24dc4f4129be207f803.
  - `src/Etterna/Screen/Gameplay/ScreenGameplay.cpp:76` (`Center1Player` default true), `:236-249` (zoom 1, placement);
    `src/Etterna/Actor/Gameplay/Player.cpp:1369-1370, 1392-1393, 1414-1460` (positioner).
  - `Themes/Til Death/BGAnimations/ScreenGameplay overlay/WifeJudgmentSpotting.lua:192-194` (NoteField moved inside the
    Player); `.../lanecover.lua:15, 31-35` (centring rule, cover x);
    `Themes/Til Death/Scripts/02 CustomizeGameplay.lua:35-39` (per-key-mode field position and size).
- [Y1] YARG, **LGPL-3.0** (read only), `dev` at `f5bfbe6996b4af35722d4204a93c6cc5c8f52a45` (2026-06-24):
  https://github.com/YARC-Official/YARG/tree/f5bfbe6996b4af35722d4204a93c6cc5c8f52a45.
  - `Assets/Script/Gameplay/Visuals/HighwayCameraRendering.cs:20-33` (caps), `:189-250` (fit), `:405-421` (camera
    shift), `:515-538` (screen shift), `:625-643` (offset formula).
- [Y2] YARG PR #1172, "Angle highways in multiplayer to prevent fail meter icons from getting too close to the
  highways" (commit 21bcebed7e, 2025-10-20): https://github.com/YARC-Official/YARG/pull/1172. Rock Band comparison,
  even spacing, the tilt setting (0 none, 1.0 = leftmost highway's right edge vertical, default 0.5).
- [B1] beatoraja, **GPL-3.0** (read only), commit `9340253a866a2f0091d44c6fa9de7637ce229ef9`:
  https://github.com/exch-bms2/beatoraja/tree/9340253a866a2f0091d44c6fa9de7637ce229ef9.
  - `skin/default/play/play7main.lua:8-12` (Play Side), `:147-219` (1P and 2P geometry), `:553-573` (judgement
    placement).
- [BM1] Bemuse, **AGPL-3.0** (read only), commit `4bf3e7d98a44841b323741cc403ecb7b41618ade` (2026-10-08):
  https://github.com/bemusic/bemuse/tree/4bf3e7d98a44841b323741cc403ecb7b41618ade.
  - `bemuse/src/app/ui/OptionsPlayer.tsx:120-135` (Panel: Left/Center/Right/3D [Beta]);
    `bemuse/src/app/entities/Options.ts:42-43, 90-96, 124` (default center; 3D only in keyboard mode).
  - `public/skins/default/skin_screen.xml:1900, 2041-2079` (combo x, panel x).
  - `bemuse/src/game/display/touch3d.js:1-88` (3D arc, centred at 1280/2, `:79-80`);
    `bemuse/src/game/display/player-display.js:76-90, 151`.
  - `bemuse/src/app/entities/Options.ts:263-271` (choosing 3D switches to keyboard mode).
- [Q1] Quaver, **MPL-2.0** (read only), commit `e9b54a66c5274b1651328c66e9ea3846f8614508`:
  https://github.com/Quaver/Quaver/tree/e9b54a66c5274b1651328c66e9ea3846f8614508.
  - `Quaver.Shared/Screens/Gameplay/Rulesets/Keys/Playfield/GameplayPlayfieldKeys.cs:212-230` (playfield at top centre
    plus `ColumnAlignment`); `Quaver.Shared/Skinning/SkinKeys.cs:98, 616`.
- [K1] KONAMI, beatmania IIDX 32 Pinky Crush, ゲームオプションについて:
  https://p.eagate.573.jp/game/2dx/32/howto/play/option.html. FLIP swaps the left and right charts in DP; no
  lane-position option is listed.
- [G1] Infinite Phantasm, beatmania 5-key guidebook, Modifiers menu:
  https://infinitephantasm.github.io/5key-guidebook/beginner/modifiers.html. Center, Center Double, Double.
- `game-djmax.md` [D2][D6]: GEAR POSITION Left / Center (default) / Right.
- `skins-survey.md`: Alpha's skins' `ColumnStart` and stage centres. `game-stepmania-etterna.md`: perspective
  transforms. `game-yarg-clonehero.md`: YARG Default camera.
- This repository (MIT): `osu.Game.Rulesets.ManiaOrtho/SkinPort/SkinPortLayout.cs:56-81` (the current clamp);
  `Highway/HighwayProjection.cs:103-117` (symmetric trapezoid, sway); `Highway/HighwayLayout.cs:36-51` (`CentreX`,
  `Sway`); `UI/HighwayJudgementLayer.cs:125-166` (judgement on the stage centre).
