# Project Sekai (Colorful Stage!)

Last checked: 2026-11-04

## Summary

Project Sekai is a touch-screen game with the steepest perspective of any reference we have studied. It still reads
well, because its notes do not move linearly in depth: they approach on an **exponential curve in screen scale**.

**Geometry** (from the fan engine's constants, at 16:9):

| Feature | Value |
|---|---|
| Judgement line | 0.791H from the top (20.9% above the bottom, high because thumbs need room) |
| Lane width at the line | 0.787W |
| Lane top | exactly at the top edge of the screen |
| Vanishing point | 4.6% of H above the top edge |
| Where notes appear | 0.015H, at 7.3% of the line width |
| Edge lean | 39.9° |

**Speed.** Note speed 1-12 maps to an approach time from 4000 ms down to 350 ms, with
T = 350 + 3650 · ((12 − s)/11)^1.31 ms. A player's frame-stepped captures of the official game fit this formula and
the exponential curve (see the checks below):

- speed 1 takes 4 s;
- four combinations of speed and start position match within 0.1-1.5%;
- a linear-depth approach misses them by about 30%.

Typical speeds are 10.0-11.6 (741-397 ms). Alpha's 405 ms equals Sekai speed 11.55.

**Notes.** They are wide pill-shaped bars, coloured by note type, not by lane. They are 7.4% of the screen height at
the line and a constant 3.4% of the visible time thick.

**Holds.** The body is full width. The head stays on the line while held. The official game has a 10-100%
transparency setting for long notes.

**Hit effects.** Every hit lights the hit cells' slots on the line (fading over 500 ms), sends up an upright glow
(250 ms), flashes the whole lane (300 ms), and adds particles.

**Lane cover.** Since April 2024 the official game has one: **Note Start Point** (ノーツの開始位置), 0-100. It keeps note
speed, so the visible time shrinks. This corrects `topic-covers.md`, which found none.

For us: Sekai shows that notes can enter at the top edge under strong perspective and stay readable, but only with
an eased approach, which SPEC 3 currently rules out. That is a question for Alpha (K2). Several smaller ideas carry
over directly: a hold opacity setting, slot lighting on the line, a track opacity setting, and 3-slice note art.

Conventions:

- y is measured from the top as a fraction of screen height H, x as a fraction of screen width W, at 16:9.
- px means pixels of a 1920×1080 frame.
- A **cell** is 1/12 of the stage width. Charts place notes on 12 cells.
- s is the note's screen scale about the vanishing point (1 at the judgement line).

## Scope, evidence and versions

**Evidence used, strongest first.**

1. **Official SEGA pages.**
   - The English support page lists every show setting: speed, Note Start Point, long-note transparency, lane
     transparency and others [K1].
   - An official @pj_sekai post announces 0.01 steps for speed and timing and an adjustable note start position
     [K2]. It was read from its search-result title only, because X needs a login.
2. **One player's frame-stepped measurements of the official game** [K3][K4] (note.com, ぬるま湯, 2024-10-06):
   - an iPad Pro (2024) captured with a GC550 capture card and stepped frame by frame in MPC-HC;
   - speed tested at 1-12 in steps of 1, and start position at 0-100 in steps of 5;
   - fits are quoted only for speed 9-12 and start position 0-20.

   Only the text and the spreadsheet's example cells could be read. The graph images were blocked.
3. **Fan reimplementations (MIT), read in full.** Both are fan work, not SEGA code; where they may differ from the
   official game is said in each section.
   - [P1] sonolus-pjsekai-engine, NonSpicyBurrito, commit 0f6f37fc (2025-12-18). It is archived and points to its
     successor.
     - Its stage art is 2048×1176 px, with 1420 px at the line and 850 px from the vanishing point to the line.
     - Its options follow the official ones: the note-speed step went from 0.1 to 0.01 on 2024-05-02
       (commit bab50b56), eight days after the official announcement, and its connector alpha has the official
       10-100% range.
   - [P2] sonolus-next-sekai-engine (Next SEKAI), © 2025 Kyle Chang, commit bfe61449 (2026-10-31). It keeps [P1]'s
     constants and speed formula (`layout.py:23-39,1203-1210`) and adds options such as cover speed compensation and
     an alternative approach curve. It calls itself "Project Sekai inspired".
4. **Wikis and guides:**
   - Gamerch [K5][K6][K7];
   - AppMedia [K8];
   - Game8 [K9];
   - Dengeki [K12];
   - a community tips document [K11];
   - wikiwiki's Chunithm page for Sekai players [C14].
5. **Forums:** two r/ProjectSekai speed threads, read through redlib mirrors [K10].
6. **Could not be read:**
   - wikiwiki.jp/pjsekai (refused);
   - YouTube;
   - note.com and Game8 images, and Google Sheets data (blocked here).

   **No official frame was measured.** The geometry rests on [P1]'s constants. Speed and approach curve are
   cross-checked against [K3].

**Versions** (all mobile; dates from [K13] unless noted):

| Date | Change | Source |
|---|---|---|
| 2020-09-30 | JP release (Colorful Palette, a CyberAgent subsidiary, with SEGA; published by SEGA) | [K13] |
| 2021-12-07 | Global (English) release; TW/HK/MO 2021-09-30, KR 2022-05-20, SEA 2023-09-25, CN 2025-03-27 | [K13] |
| 2023-09-30 | APPEND difficulty (3-4 fingers) and trace notes | [K9][K12] |
| 2024-04-24 | Speed and timing in 0.01 steps; Note Start Point added (version 3.5.0 per [K3]) | [K2][K3] |
| current | App Store JP version 7.1.0, SEGA CORPORATION | [K14] |

No source reports a change to the stage geometry across versions (unverified).

## Camera and track

### Stage geometry (P1, 16:9, stage aspect locked)

The transform is `Initialization.ts:24-36`: a skin unit y is the scale s about the vanishing point. Our computation
gives:

| Quantity | Value | How |
|---|---|---|
| Vanishing point | −0.046H (4.6% of H above the top) | t = 0.5 + 1.15875 · 47/1176 of the stage height |
| Judgement line (s = 1) | **0.791H**, 20.9% above the bottom | 1.15875 · 803/1176 below the top |
| Lane top (s = 47/850 = 0.055) | **0.000H**: exactly the top edge | `lane.ts:1-4` |
| Lane bottom (s = 1176/850) | 1.112H, past the bottom edge | the lane continues below the line, off screen |
| Notes appear (s = 1.06^−45 = 0.073) | **0.015H** | `note.ts:5-6` |
| Width of 12 cells at the line | **0.787W** (1511 px, one cell 126 px) | 1.15875 · 1420/1176 of the stage height |
| Width where notes appear | 0.057W (110 px) | × 0.073 |
| Width at the top edge | 0.044W (84 px) | × 0.055 |
| Far/near width ratio | **0.073** where notes appear; 0.055 at the top edge | — |
| Edge lean from vertical | **39.9°** | — |

Comparison with the other references (Chunithm from `game-chunithm.md`, USC from `game-sdvx.md`):

| | Chunithm (arcade) | USC landscape | **Sekai (P1)** |
|---|---|---|---|
| Judgement line | 0.926H | 0.941H | 0.791H |
| Far end | 0.163H | 0.284H | 0.015H |
| Far/near ratio | 0.24 | 0.106 | 0.073 |
| Lean | 28.4° | — | 39.9° |
| Width at the line | 0.611W | 0.435W | 0.787W |

- Sekai is the only reference whose track **natively reaches the top edge**, with no stretching.
- It does this with a very small far ratio and a vanishing point just above the screen.

**How it is drawn** [P1]:

- `perspectiveLayout` (`utils.ts:1-11`) builds a quad whose x is scaled by its y, so edges converge on the vanishing
  point.
- Each note's quad is then scaled as a whole by s (`FlatNote.ts:163-172`).
- The stage art is one flat 2048×1176 sprite with the perspective painted in (`Stage.ts:90-97`). The fallback stage
  draws six 2-cell lane sprites and 0.5-cell borders (`Stage.ts:99-124`).
- There is no 3D camera.

**Aspect ratio.** The stage keeps 16:9 by default ("lock stage aspect ratio"):

- on wider screens it fills the height;
- on narrower ones it fits the width (`Initialization.ts:9-22`).

What the official app does on 4:3 tablets is unverified.

**Touch area.** Each lane's hitbox is a screen rectangle as wide as the lane at the line. It spans from s = 0.567
(0.43H) past the bottom edge (`lane.ts:7-21`). So the lower 57% of the screen is the touch area.

- That explains the high judgement line.
- It has nothing to teach a keyboard game (inference).

## Note approach and speed

### The approach curve

**[P1].** `approach = 1.06^(45 · (u − 1))`, where u is the elapsed fraction of the approach (`note.ts:5-6`). The note
quad is scaled about the vanishing point by that value.

- So s grows exponentially in time.
- The note's centre reaches the line (s = 1) at the hit time.

Position profile (computed):

| Time still to go | 0 | 10% | 25% | 50% | 75% | 100% |
|---|---|---|---|---|---|---|
| y | 0.791H | 0.598H | 0.389H | 0.179H | 0.071H | 0.015H |
| Track width | 0.787W | 0.606W | 0.409W | 0.212W | 0.110W | 0.057W |
| Note height at 1080p | 80 px | 61 px | 41 px | 22 px | 11 px | 6 px |

How this compares with a linear-depth plane at the same far ratio (`topic-speed.md`):

| | Sekai curve | Linear depth, same far ratio |
|---|---|---|
| Line/far on-screen speed | 13.8× | 189× |
| Time in the near half of the on-screen track | 23.8% | 6.8% |

The note is in the lower half of the screen only for the last 16.3% of its time.

**[P2]** keeps this curve as the default. It adds an off-by-default "Alternative Approach Curve"
(`layout.py:1037-1044`, `options.py:285`) that moves notes linearly in depth over part of the range. We computed:
line/far speed 30×, 15.4% of the time in the near half.

**The official game uses an exponential-like curve too** (inference from [K3], our script). [K3] gives four display
times with a start position set. We modelled the start position as a cut at
s = lerp(0.073, 1, p/100) (what [P2] does, `layout.py:720-735`), and used [P1]'s formula and curve:

| Speed, start position | [K3] | Exponential model | Linear depth, same geometry |
|---|---|---|---|
| 10.45, 5 | measured 31.1-31.2 f; their fit 30.84 f | 30.69 f (−1.5%) | 21.9 f |
| 10.2, 6 | their fit 32.43 f | 32.46 f (+0.1%) | — |
| 10.31, 5 | their fit 32.41 f | 32.33 f (−0.3%) | — |
| 9.59, 6 | their fit 39.94 f | 39.91 f (−0.1%) | — |
| 9.44, 15 | "nearly the same as 10.45, 5" | 31.63 f | 15.6 f, half of 10.45/5 |
| 1, 0 | measured 240 f = 4 s | 240 f | 240 f |

- [K3]'s fit is a quadratic fitted to their own captures over speed 9-12 and start position 0-20, so the 0.1-0.3%
  agreements are agreement with smoothed measurements.
- Both the speed formula and the curve are consistent with the official game in that range.
- A linear-depth approach is ruled out by the start-position data.
- At speed 1-8 only the 4 s endpoint is checked.

### Speed number to visible time

[P1] `note.ts:7-9` (identical in [P2]):

> T = lerp(350, 4000, ((12 − s)/11)^1.31) ms. The inverse is s = 12 − 11 · ((T − 350)/3650)^(1/1.31), valid for
> 350-4000 ms.

| Sekai speed | 1 | 5 | 6 | 8 | 9 | 10 | 10.5 | 11 | 11.3 | 11.55 | 12 |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Visible time (ms) | 4000 | 2369 | 2000 | 1320 | 1015 | 741 | 618 | 508 | 449 | **405** | 350 |
| Mania label speed (11485/T) | 2.9 | 4.8 | 5.7 | 8.7 | 11.3 | 15.5 | 18.6 | 22.6 | 25.6 | 28.4 | 32.8 |

Visible time here means from appearing at 0.015H until the note's centre is on the line. The leading edge crosses
1.65% of T earlier: 10 ms at speed 10.5.

**Official range and steps.**

- **Range 1.0-12.0.**
  - [P1] uses it (`options.ts:26-34`).
  - [K3] measured every integer from 1 to 12.
  - A player at 12 "wishes it went to 13" [K10].
  - No official page states the range (unverified).
- **Steps.** 0.1 before 2024-04, 0.01 since [K2][K3]. That "before" is inferred from [P1]'s step change and the
  official wording "±0.01単位で調整可能に".
- **Default 5.0** [K5], called "too slow". Dengeki also calls the default "緩やか" (gentle) [K12].
  - [P1]'s default is 6.
  - The official default is verified by one source only.
- **Adjusting.** The live-start gear icon, then the Live tab, with test taps while adjusting [K9]. **No visible time
  is shown**; [K3] built the measurements because "beatmania shows the display time and Sekai does not".
- **Typical speeds:**
  - beginners 8.0, experienced players 10.0 and up [K5];
  - "Most serious players use ~9 to 11" [K11];
  - two threads put nearly every player at 10.0-11.8, with modal values 10.5-11.3 [K10].
  - In time that is **741-397 ms**, modal 618-449 ms.
- **Cross-game.** Chunithm 8-9 "is close to Sekai 10" [C14]:
  - Chunithm 8-9 is 686-610 ms (`game-chunithm.md`);
  - Sekai 10 is 741 ms.

  Close, given the different curves.

**Players settle on similar near-line speeds** (inference, our numbers):

- On-screen speed at the line is (J − VP) · 2.622 / T = 2.20 H per visible time. That is:
  - **4.32-4.89 H/s** at the modal 11.0-11.3;
  - 5.42 H/s at Alpha's 405 ms.
- Chunithm's arcade geometry gives 3.17 H per visible time (`game-chunithm.md`). At its typical 7.5-8.5 (731-645 ms)
  that is **4.34-4.91 H/s**: the same band.
- Sekai players pick visible times about 30% shorter than Chunithm players. The eased curve lowers the near-line speed
  per visible time by about the same amount (2.20 against 3.17 H, −31%).
- SDVX's inferred numbers (`game-sdvx.md`) do not fit this band. They are inferences themselves, so the pattern is
  shown for two games only.

**BPM and scroll changes.**

- The speed ignores BPM.
- Charts can change scroll speed: [P1] places notes in "scaled time" from time-scale changes (`FlatNote.ts:68-70`),
  and [P2] has a "Disable Timescale" option.
- How common they are in official charts is unverified.

## Judgement line and receptors

- **Position:** 0.791H across the 12 cells, under the notes:
  - [P1]'s judgement line sits on layer 1, notes on 97-102 (`skin.ts:122-143`);
  - its fallback line sprite is one note height tall (7.4% H) and spans the cells (`Stage.ts:119-123`).
- **Look in the official art:** unverified, since no frame was measured. In [P2] (`stage.ts:1074-1091,1254-1330`), the
  fallback stage has:
  - a judgement band coloured purple by default;
  - a thin divider on the line at every cell;
  - lane dividers every 2 cells, which makes **6 drawn lanes**;
  - a 0.08-cell border.
- **Slots.** The line is made of 12 per-cell "slots" that light when hit (Feedback).
- **No receptors.** It is a touch screen; the line is the target.

## Lane covers / walls

**Official: Note Start Point** (JP ノーツの開始位置), added in April 2024 [K1][K2][K3]:

- **Range 0-100**, in **steps of 1 or 10** [K3, quoting the update notes].
- "The higher the number, the closer the note will begin to the line for assessing accuracy" [K1]. The JP notes say
  "値が大きくなるほど、開始位置が判定ライン側に近づきます" [K3].
- **Adjusted before play.** "This setting is available on the preview screen" [K1]. Mid-song adjustment is unverified;
  probably not.
- **Unit: screen distance**, from where notes appear to the line. This is an inference from the fit in "Note approach
  and speed". With the exponential curve, small values hide a lot of time:

  | Start position | 5 | 10 | 20 | 50 |
  |---|---|---|---|---|
  | Visible time hidden | 19% | 31% | 48% | 76% |

- **It keeps note speed.** At a fixed speed the display time falls as the start position rises [K3]. To keep the time,
  players change both, as [K4]'s calculator does (10.2 at 6 → 10.31 at 5, for the same 32.4 f).
- **What is drawn.** [K3] compares it to IIDX's SUDDEN. Whether the official game draws a cover panel or notes simply
  appear later is unverified.
- **Other display options** [K1][K6]:
  - Lane Transparency (レーンの不透明度);
  - Background Brightness (背景の明るさ, "the brightness behind the lane").

  Neither range is stated.

**Fan engines:**

- **[P1] stage cover.** 0-1 in 0.01 steps, drawn from the top of the screen down to lerp(lane top, line, v)
  (`Stage.ts:126-139`, `options.ts:118-128`). It keeps speed.
  - This differs slightly from the official unit. With [P1]'s lane-top start, the 10.45/5 case gives 32.8 f
    (+5.4% vs measured); [P2]'s spawn-point start gives 30.7 f.
- **[P2]:**
  - the cover starts at the note spawn point;
  - "Stage Cover Mode": stage, stage and line, or full width;
  - cover alpha;
  - "Stage Cover Note Speed Compensation" (2026-04-13, commit d43b367), with three values (`layout.py:1203-1210`,
    `localization.toml:63-90`):

    | Value | Effect |
    |---|---|
    | Off | keeps speed |
    | Fixed Only (default) | compensates only note groups that have a fixed speed |
    | Full | "all notes slowed so each note's on-screen time stays as without the cover" (Vietnamese description, translated): **our wall's default** |

  - Its Korean labels call the cover "노트 시작 위치" ("note start position"), the same name as the official option.

## Notes and holds (shapes, rendering)

**Note types and colours.**

| Type | Look | Source |
|---|---|---|
| Tap | cyan | [P1] `skin.ts:16-19` (`Sekai Note Cyan`) |
| Critical | yellow, twice the score | [K9][K13], `skin.ts:33-36` |
| Flick | red/pink, with an arrow above | `skin.ts:27-30,92-104` |
| Slide (hold) head and end | green | `skin.ts:21-25` |
| Slide body | green connector | `skin.ts:66-80` |
| Trace | small diamond-marked notes, judged by contact (2023-09-30) | [K9] |
| Slide tick | diamond | `skin.ts:39-43` |

The exact colours are unverified.

**Shape and width.**

- Notes span their cells exactly: l = lane − size, r = lane + size, so no gap. [P2]'s "note margin" defaults to 0
  (`options.py:266`).
- They are drawn in **3 slices**: a fixed 0.3-cell cap at each end and a stretched middle (`FlatNote.ts:95-104`). A
  slide head held on the line uses 0.25-cell caps (`ActiveSlideConnector.ts:238-257`).
- Widths run from 1 to 12 cells.
- Notes get narrower toward MASTER [K7].

**Thickness.**

- [P1]: h = 75/850 / 2, so the note spans s(1 − h) to s(1 + h) (`note.ts:1-3`, `FlatNote.ts:92-93`).
- That is **0.074H, 80 px at the line**, and 6 px where notes appear.
- Because the whole quad is scaled by s, thickness is proportional to width. It falls as s, not as s² as it would for
  a flat plate on a plane.
- In time it is constant: ln((1 + h)/(1 − h)) / 2.622 = **3.37% of the visible time**. Chunithm's TAP is 2.55% of its
  visible time and 87 px at the line.
- The **centre** is on the line at the hit time, so the leading edge arrives 1.65% of T early.

**Holds (slides)** [P1] `SlideConnector.ts`, `ActiveSlideConnector.ts`:

| Aspect | Behaviour |
|---|---|
| Body width | the note's full width, interpolated head to tail with a linear or quadratic ease, for curved slides |
| Subdivision | 10 segments per connector, each projected (`SlideConnector.ts:202-227`) |
| Clipping | the body starts at the line once the head has passed, held or not (`:197-200`) |
| Opacity | 1, and **0.5 once dropped** (`ActiveSlideConnector.ts:205-207`), times the official **Long Note Transparency, 10-100%** [K1] (`connectorAlpha`, `options.ts:80-89`) |
| While held | the body cross-fades between normal and "active" art once per second (`SlideConnector.ts:235-239`) |
| Head while held | a slide-head note is drawn on the line, following the slide's lane (`renderSlide`) |
| End | a full note with its own judgement (end judgement: [K8]; Chunithm has none [C14]) |

- Guide slides (not judged) fade from 57.5% to 7.5% alpha along their length (`SlideConnector.ts:246-248`).
- The official "Note Start Point" support text says "the beginning of a long note" [K1]. That reads like a translation
  slip for notes in general (inference: [K3] measured taps).

**Flick and trace (no mania equivalent).**

- A flick note carries an arrow of six sizes by note width (`arrowSprites.ts:8-30`), pointing up or diagonally, with
  an optional "marker animation".
- Trace notes need contact, not a fresh tap.
- Mania has neither; skip both.

**Simultaneous line** (同時押しライン, official toggle) [K1][K6]:

- A bar between two simultaneous notes, drawn at note height (`SimLine.ts:50-100`).
- On by default in [P1] (`options.ts:74-79`).

**Missed notes** [P1] `FlatNote.ts:111-118`:

- An unhit note keeps moving past the line until its Good window closes (+125 ms for a tap), then vanishes.
- The lane continues below the line, so this is visible.
- The official behaviour is unverified.

## Key count and lane layout

- **One field for every chart: 12 cells** [K5][K8].
- **Six drawn lanes** of 2 cells each in both fan engines' fallback stages. The official art is drawn into its stage
  sprite (unverified).
- Notes may be any width and position at cell resolution. USC charts allow fractional positions (`lib/src/usc`).
- **There are no key modes.** Difficulty changes note widths and density:
  - widths narrow toward MASTER [K7];
  - APPEND assumes 3-4 fingers [K12];
  - most charts assume 2 fingers [C14].
- **Mapping mania key counts onto 12 cells** (inference):
  - 1, 2, 3, 4, 6 and 12 keys tile evenly. 4K is 3 cells a lane; 6K matches the six drawn lanes.
  - 5K, 7K, 8K and 10K do not tile.
- **Lane widths at Sekai's 0.787W** (computed):

  | Keys | At the line | Where notes appear |
  |---|---|---|
  | 4K | 378 px | 27 px |
  | 7K | 216 px | 16 px |
  | 10K | 151 px | 11 px |

  Every one is above Voltex 10K's 9 px (`topic-wide-keys.md`).

## Feedback (judgement display, hit effects, lane lighting)

**Hit effects** [P1] `FlatNote.ts:175-261`. They play on every hit that is not a miss, coloured by **note type**
(cyan, green, red, yellow), **not by grade**. Only the sound varies with the grade, and only for some notes
(`:182-200`). A miss has no effect.

| Effect | Where | Size | Time | Source |
|---|---|---|---|---|
| Slot light | each hit cell's slot on the line, under the notes (layer 50) | 1 cell × note height | alpha 1 → 0 linearly over **500 ms** | `SlotEffect.ts:16-38` |
| Slot glow | an upright quad rising from the line over the note, **above** the notes (layer 200) | rises to 4 cell widths (0.47H), top 25% wider | height 1 − a³ (fast rise), alpha 1 → 0 over **250 ms** | `SlotGlowEffect.ts:21-66` |
| Lane flash | the whole lane under the note, line to far end, projected | the note's cells | **300 ms** | `FlatNote.ts:250-261` |
| Circular particle | an ellipse at the line | 3.5 cells wide, 60% as tall | 600 ms | `FlatNote.ts:219-230`, `particle.ts:41-58` |
| Linear particle | rising sparks | 2 cells wide, 2 cell widths tall, leaning outward by lane | 500 ms | `FlatNote.ts:207-217`, `particle.ts:60-80` |

- [P2] keeps these timings (`slot_effect.py:14-15`, `note.py:906-959`). It adds an "effect animation speed" option and
  a 1 s lane effect variant.
- Effect sizes are options: note effect 10-200%, slot effect 0-200% (`options.ts:58-117`).
- Particle art comes from the skin and particle packs. The official look is unverified.

**Touches and holds.**

- **Touching a lane with no note** flashes that cell's lane for 300 ms (`Stage.ts:66-88`).
- **While a slide is held** (`ActiveSlideConnector.ts:54-95,209-236`):
  - the slot glow stays over the slide, pulsing at **4 Hz** between 10% and 20% alpha;
  - circular particles (7 cells wide) and linear particles loop at the slide's current position;
  - a hold sound loops.

**Judgement text and combo** ([P1] UI layout, `Initialization.ts:109-136`; the official layout is unverified):

| Element | Position | Size |
|---|---|---|
| Judgement | **one text, centred over the stage at 0.615H** (0.18H above the line) | 4.75% H tall |
| Combo | **to the right of the stage**, at 0.855W, 0.41H | number 14% H tall, "COMBO" label 3.5% H above it |

Score and life sit top left. The menu is top right.

**Official show options** [K1][K6]:

- Note Skin ("Skins have no effect on how accuracy is judged");
- Cut-in effect;
- Display Skill / ON FIRE;
- Multi-Tap Line;
- background brightness and lane transparency;
- Music Video mode: 3D MV, 2D MV or Lite (軽量), with Lite recommended for full-combo attempts [K6].

## What a 4K/7K osu!mania player takes from it

- **Speed.** A Sekai player at speed 11.5 has mania's visible time (about 410 ms). Thanks to the eased curve, notes
  rush only about 5.4 H/s at the line. Under linear depth with the same geometry they would crawl at the top.
- **Readability.** Strong perspective works when notes slow down far away less than plane geometry demands. Sekai
  trades geometric honesty (true depth) for time spent in the readable lower part.
- **Notes.** Big, bright, type-coloured pills with fixed end caps. Thickness tracks width, so far notes stay legible.
  A mania player keeps lane colours, but the shape lessons carry over.
- **Feedback.** One central judgement plus a lot of light at the hit lane: slot, glow, flash and particles, all
  under 600 ms.
- **Covers and options.**
  - A pre-play start-position cover that keeps speed, as Chunithm's does.
  - Per-element opacity sliders (lanes, long notes, background), which a keyboard player uses to tune clutter.
- **Not useful for keyboard play:** the high line and the touch area; flicks, traces and criticals; and 6 drawn
  lanes over 12 cells, which would misalign with 4K and 7K.

## What this means for us

Everything below is visual: settings, not mods (invariants 1 and 3). Our current state comes from `HighwaySettings.cs`,
DECISIONS and `game-chunithm.md`.

### K1. Defaults stay: Chunithm preset, linear approach (SPEC 2, 3)

- Sekai is the exception to SPEC 3. Every other reference moves notes linearly in depth (`topic-speed.md` S1). Sekai's
  readability comes from its curve, which SPEC 3 rules out ("d is linear in time… do not 'correct' it").
- Sekai does confirm SPEC 2's "notes enter at the top edge" for a perspective track without stretching. It needs a
  small far ratio (0.055 at the top) and a vanishing point just above the screen.
  - That is the honest alternative to stretching Chunithm (Q7(a): lean 28° → 14°).
  - It only reads well with the curve.
- **No change to defaults.**

### K2. A "Sekai" preset, only with Sekai's eased approach (SPEC 2, 3): ask Alpha

**Values** (all within our sliders' ranges, `ManiaOrthoRulesetConfigManager.cs:87-92`):

| Setting | Chunithm preset now | Proposed Sekai preset |
|---|---|---|
| Camera | Perspective | Perspective |
| Judgement line height | 0.074 | **0.209** |
| Far edge height | 0 (stretched) | **0.015**: notes appear where Sekai's do. The 1.5% above shows no track; Sekai's lane art continues there |
| Track width | 0.61 | **0.787** |
| Far/near width ratio | 0.24 | **0.073** |
| Wall | 0 | 0 |
| Note thickness | 2.55% of track depth | **3.4% of visible time**, taken in time and remapped like the notes |
| Approach | linear | **eased** |

The vanishing point (−0.046H) and the lean (39.9°) follow from these values.

**The eased approach is `topic-speed.md` S4's depth remap**, d' = (r^(−d) − 1)/k with k = 1/r − 1:

- With r = 0.073 it reproduces Sekai exactly, because the screen scale becomes s = r^d.
- Notes still cross the line at their hit time, and visible time is unchanged (SPEC 3's speed definition holds).
- Holds stay aligned.
- It applies after mania's scroll algorithm, so SV is untouched.
- Implementation:
  - remap note edges in time (head time, head time + thickness), so thickness keeps Sekai's constant ratio;
  - L0 tests: monotonic, d' = 0 at d = 0 and 1 at d = 1;
  - a parity run, like every setting.

**Without the curve: not proposed.** At r = 0.073 under linear depth:

- notes spend 6.8% of their time in the near half of the track;
- line/far speed is 189×;
- a 3.4%-of-time note would be 389 px tall at the line.

**Why ask.** It changes SPEC 3's stated intent. Proposed question for `QUESTIONS.md`:

> **Q-Sekai.** Add a "Sekai" camera preset (line 21% up, far ratio 0.073, notes from the top edge) that uses Project
> Sekai's eased approach (notes slow less far away, so they are not linear in track depth, an exception to SPEC 3)?
>
> Options:
> - (a) Sekai preset with the eased approach; other presets stay linear;
> - (b) (a) plus an "Approach: Linear / Eased" setting on every preset;
> - (c) no Sekai preset.
>
> Best guess (a). Capture first: Chunithm (linear) against Sekai (eased) at 405 ms, 4K and 7K.

### K3. Speed hint and playtest speeds (SPEC 3)

- In the Sekai preset, show "≈ Sekai s" beside the visible time, using s = 12 − 11 · ((T − 350)/3650)^(1/1.31),
  clamped to 1-12. This mirrors game-chunithm's "≈ CHUNITHM s".
- Alpha's real 405 ms reads "≈ Sekai 11.5", a normal Sekai speed.
- **Playtest speed** (inference above): Chunithm and Sekai players settle at about 4.3-4.9 H/s at the line.
  - Our stretched Chunithm preset moves 3.86 H per visible time. It reaches that band at **790-900 ms**: mania label
    speed about 13-14.5, or about 14 for 840 ms on argon (`game-osu-mania.md`).
  - At Alpha's 405 ms it moves 9.5 H/s.
  - **Add an 840 ms capture** next to `topic-speed.md` S3's 405, 550 and 704 ms.
  - Still no auto-compensation (S2).

### K4. Wall (SPEC 4): no change

- The official Note Start Point keeps speed, as Chunithm's field wall does. Our default keeps visible time, as IIDX FHS
  does.
- [P2] added the same option independently ("Full" compensation, April 2026). That is a second design arriving where
  SPEC 4 already is.
- Sekai's unit is screen distance with 1/10 steps, set before play. Ours is track depth in 5% steps, adjustable at any
  time. Keep ours: depth steps are what Chunithm uses (`topic-covers.md` C6).
- In a Sekai preset with the eased curve, a depth step equals a time step, so a 5% wall hides 5% of the visible time.
- **Correction for `topic-covers.md`** (applied 2026-11-04): the row "Project Sekai (official): none found"
  should read: Note Start Point (ノーツの開始位置), 0-100, steps 1 or 10, default 0 (unverified), before play,
  keeps speed, since 2024-04 [K1][K2][K3]. The "Unverified" bullet about Sekai can go.
- **Correction for `topic-speed.md`'s Sekai rows** (applied 2026-11-04):
  - the official default is 5.0, not 6 [K5];
  - the steps are 0.01 since 2024-04;
  - the formula and curve are now cross-checked against official captures ([K3], above).

### K5. Notes (SPEC 6, 8)

- **Keep** 98% width, the leading-edge anchor and thickness in track depth for the existing presets.
  - Sekai's centre anchor puts the leading edge about 10 ms early.
  - Mania's timing, and Alpha's, is trained on the leading edge.
- **Textured notes (M6, SPEC 8):** draw note textures in **3 slices** with fixed end caps, as Sekai does. Size the caps
  in track width, not as a fraction of the lane: Sekai's 0.3-cell cap is **2.5% of the track width**. Then caps look
  the same at 4K and 10K and wide lanes do not stretch the art.
- **Optional "Sekai" palette for the built-in skin**, later: taps cyan, holds green.
  - Not default: Alpha's white/pink lane colours stay (`baseline-alpha.md`).
  - Exact colours need a frame (unverified).

### K6. Holds (SPEC 6)

**Already matching Sekai:**

- the head pinned on the line while held (DECISIONS 2026-11-04);
- the body clipped at the line;
- the body dimmed when dropped.

**Proposal: "Hold opacity", 10-100%, default 100%.**

- It multiplies our current body alphas (0.55 idle, 0.85 held, 0.35 broken; `HighwayNoteLayer.cs:226`).
- It is Sekai's official Long Note Transparency option, with the same range.
- It is a preference outside presets, like "Notes".

**Not proposed:**

- Sekai's full-width body: ours is 80%, which lets the head read over it.
- The 1 Hz art cross-fade.
- A full-size tail: ours is half thickness, and mania judges the release either way.

### K7. Feedback (SPEC 7)

- **Q5 (judgement placement).** Sekai shows one aggregated judgement centred above the line (0.615H, 4.75% H). That is
  SPEC 7's screen-space choice, our current default. Record it as the counter-reference to Chunithm's per-lane text.
  No change.
- **Proposal: a slot light on the line for every hit**, as part of "Hit effects: Highway":
  - The lane's segment of our judgement line lights in the **result** colour (feedback P1) and fades linearly over
    **500 ms**.
  - Colour by result, not note type: with Alpha's Perfect/Great text hidden, colour is the grade cue (feedback P0).
  - It is drawn under the notes' layer, in the highway overlay.
  - It is the hit-time version of feedback P5's "brighten the lane's segment of the judgement line".
  - It is cheap: no pool, one value per lane.
  - *(tune)*: the alpha peak, start at 60%.
- **Not proposed:**
  - Sekai's upright 250 ms glow: it overlaps our ring and sparkles;
  - the empty-press lane flash: our held beam already lights a pressed lane, `HighwayTrack.cs:55-78`;
  - type colours.
- Sekai's effects all end within 600 ms, consistent with our 500 ms burst lifetime.

### K8. Missed notes (Q14(a)): a data point

[P1] draws an unhit note past the line until its Good window closes (+125 ms for a tap), then removes it. That puts
Sekai with mania's "slide past" and against our current clipping at the line. The official behaviour is unverified.
Add it to Q14(a)'s context; no default change.

### K9. Track opacity (SPEC 8)

- Sekai has official "lane transparency" and "background brightness" sliders. osu!'s background dim already covers
  the second.
- **Proposal: "Track opacity", 0-100%, default 90%** (today's fixed `FLOOR_ALPHA`).
- It replaces feedback P8's 60-100% range, because Sekai lets the lane go further transparent.
- It is a preference outside presets.

### K10. Lanes (SPEC 5): no change

- Sekai's six drawn lanes over 12 cells would misalign with 4K and 7K. Our guides stay on the lane boundaries.
- Its width, 0.787W, belongs only in the Sekai preset.

### K11. Not worth copying

- Flick arrows, trace notes, critical notes (a scoring feature).
- Simultaneous-note lines: in 4K and 7K, chord notes in adjacent lanes already read as one bar. Revisit only if a
  playtest asks for split-hand chord cues.
- The 16:9 stage lock (osu! scales its own playfield), the high touch line and the touch hitboxes.

### Where SPEC.md's assumptions about Sekai hold or fail

1. **SPEC 3, "linear in track depth".** Sekai does not do this. It is the reference that eases its approach, in fan
   code and, by the start-position fit, in the official game.
2. **SPEC 2, "notes enter at the top edge".** Holds natively in Sekai.
3. **SPEC 2, "far/near typically 0.3-0.6".** Sekai is 0.073 (0.055 at the top edge), the lowest of all references.
4. **SPEC 2, judgement line 12%.** Sekai is at 20.9%, for thumbs.
5. **SPEC 6, "fixed thickness in track units".** Sekai's thickness is fixed in time (3.4% of the visible time), not as
   a plate on a plane. Under linear depth the two are the same thing; under Sekai's curve they are not.
6. **SPEC 7, judgement text in screen space.** Holds: one centred text.

## Unverified / open questions

- **Official geometry.** Line height, width, vanishing point, note height and the six drawn lanes all come from [P1]'s
  constants. No official frame was measured, because every video and image host was blocked. One 16:9 frame would
  settle it.
- **Speed formula below speed 9.** Only speed 1 = 4 s is checked; [K3]'s fit covers 9-12.
- **Official range, step and default.** 1.0-12.0 is inferred; the default 5.0 comes from Gamerch only.
- **Note Start Point:**
  - its screen unit (inferred from the fit);
  - its default;
  - whether a cover is drawn;
  - whether it can change mid-song.
- **Lane and background brightness:** ranges and defaults.
- **Colours** of notes, judgement line and effects; the official effect art and durations. Fan timings: slot light
  500 ms, glow 250 ms, lane flash 300 ms.
- **Judgement text and combo positions** in the official UI.
- **Missed-note behaviour** in the official game (past the line until the window closes, as in [P1]?).
- **4:3 tablets:** how the official stage scales.
- **Time-scale (scroll speed) changes:** how common they are in official charts.

## Sources

Fan engines (MIT; read in full, nothing copied):

- [P1] sonolus-pjsekai-engine (NonSpicyBurrito), **MIT** (© 2023 NonSpicyBurrito), commit
  0f6f37fc9b512b9d156ec4c00d998fe3916b157d (2025-12-18, archived):
  https://github.com/NonSpicyBurrito/sonolus-pjsekai-engine/tree/0f6f37fc9b512b9d156ec4c00d998fe3916b157d. Paths
  below are under `play/src/engine/playData/` unless they start with `shared/` or `lib/`.
  - `shared/src/engine/data/note.ts:1-6` (note height, approach curve); `shared/src/engine/data/lane.ts:1-4`;
    `shared/src/engine/data/utils.ts:1-11` (perspectiveLayout).
  - `shared/src/engine/configuration/options.ts:26-34` (note speed 1-12, default 6, step 0.01), `:58-117` (effect
    sizes and toggles), `:74-79` (sim line), `:80-89` (connector alpha 0.1-1), `:118-128` (stage cover).
  - `note.ts:7-9` (duration formula); `lane.ts:7-21` (touch hitbox); `skin.ts:3-146` (sprites, layers).
  - `archetypes/Initialization.ts:9-36` (stage transform, aspect lock), `:109-136` (combo and judgement UI).
  - `archetypes/Stage.ts:66-88` (empty-touch lane flash), `:90-124` (stage art, fallback lanes and line),
    `:126-139` (cover).
  - `archetypes/notes/flatNotes/FlatNote.ts:68-118` (timing, slices, despawn), `:163-261` (render, hit effects).
  - `archetypes/slotEffects/SlotEffect.ts:16-38`; `archetypes/slotGlowEffects/SlotGlowEffect.ts:21-66`;
    `particle.ts:41-92`.
  - `archetypes/slideConnectors/SlideConnector.ts:192-268`;
    `archetypes/slideConnectors/activeSlideConnectors/ActiveSlideConnector.ts:54-257`; `archetypes/SimLine.ts:50-100`.
  - `shared/src/engine/data/arrowSprites.ts:8-30`; `lib/src/sus/convert.ts:108-148` (12 cells, widths, slide types).
  - Commit bab50b5602521d7de7ca85be698ef3cdfc65c185 (2024-05-02): note speed step 0.1 → 0.01.
- [P2] sonolus-next-sekai-engine (Next SEKAI), **MIT** (© 2025 Kyle Chang), commit
  bfe61449e20484fd403d5314aba61a740aab3486 (2026-10-31):
  https://github.com/Next-SEKAI/sonolus-next-sekai-engine/tree/bfe61449e20484fd403d5314aba61a740aab3486. Paths are
  under `sekai/lib/` unless stated.
  - `layout.py:23-39` (same constants), `:700-735` (cover depth from the spawn point, compensation),
    `:1037-1044` (alternative approach curve), `:1203-1210` (same speed formula, compensation).
  - `options.py:52-60,129-147,171-215,266-291` (note speed, slide and guide alpha, cover options, note margin,
    alternative curve).
  - `slot_effect.py:14-15`; `note.py:906-959` (effect durations).
  - `stage.py:1074-1091,1254-1330` (fallback stage: 6 lanes, purple line, per-cell dividers).
  - Repository root: `localization.toml:39-90` (cover names and compensation descriptions).
  - Commit d43b367c7b70569d09da483391918da181572b30 (2026-04-13): stage cover note speed compensation.

Official:

- [K1] SEGA, Project SEKAI support (EN), "What are the different show settings":
  https://support-pjsekai.sega.com/hc/en-us/articles/16601102191257-What-are-the-different-show-settings. Every show
  setting: Note Speed, Note Start Point (0-100), Long Note Transparency (10-100%), Lane Transparency, Background
  Brightness, Note Skin, Multi-Tap Line and others. No ranges for speed or lane.
- [K2] @pj_sekai on X, 2024-04-24 (date from the post ID): https://x.com/pj_sekai/status/1783111423092298144. "ノーツの速さと
  タイミングが、±0.01単位で調整可能に… また、ノーツの開始位置も調整可能になります". Read from the search result title; the
  page needs a login.
- [K14] App Store JP listing:
  https://apps.apple.com/jp/app/%E3%83%97%E3%83%AD%E3%82%B8%E3%82%A7%E3%82%AF%E3%83%88%E3%82%BB%E3%82%AB%E3%82%A4-%E3%82%AB%E3%83%A9%E3%83%95%E3%83%AB%E3%82%B9%E3%83%86%E3%83%BC%E3%82%B8-feat-%E5%88%9D%E9%9F%B3%E3%83%9F%E3%82%AF/id1489932710.
  SEGA CORPORATION, version 7.1.0.

Measurements and community:

- [K3] ぬるま湯 (@nurumayuuu_), note.com, 2024-10-06, 「ノーツの表示時間」ってどれくらい？:
  https://note.com/nurumayuuu_/n/n0f910506a55b.
  - Frame-stepped captures: speed 1 = 240 f; speed 10.45 with start 5 = 31.1-31.2 f measured, 30.84 f fitted.
  - Fits are for speed 9-12 and start 0-20.
  - Quotes the v3.5.0 notes (0.01 steps; start position 0-100 in 1 or 10 steps, "判定ライン側に近づきます").
- [K4] The same author's calculator, "プロセカ ノーツ表示時間調整":
  https://docs.google.com/spreadsheets/d/1OzNfYM0qT_vk44p0MZwRvzCICfS148DjFOD74vRzPJY. Only its example cells loaded:
  10.2 at 6 = 32.43 f, 10.31 at 5 = 32.41 f, 9.59 at 6 = 39.94 f; valid for speed 9-12 and start 0-20.
- [K5] Gamerch, おすすめノーツの速さ！判定は厳しい？ (2024-07-23): https://gamerch.com/pjsekai/entry/184303. Default 5.0;
  8.0 for beginners, 10.0 and up for experienced players; 12 lanes.
- [K6] Gamerch, options (2023-01-26): https://gamerch.com/pjsekai/192143. Live settings list (レーンの不透明度,
  背景の明るさ, 同時押しライン…); MV mode 3DMV/2DMV/軽量.
- [K7] Gamerch, beginner guide (2023-10-17): https://gamerch.com/pjsekai/183586. Options path; note width narrows toward
  MASTER.
- [K8] AppMedia, slide notes (2020-08-27, updated 2026-01-15): https://appmedia.jp/?p=5307280. 12 lanes; slide end
  points are judged; yellow notes are criticals.
- [K9] Game8: https://game8.jp/pjsekai/594763 (2025-11-21: note types, critical double score, trace notes since
  2023-09-30); https://game8.jp/pjsekai/594797 (speed via the live-start gear, Live tab, test taps).
- [K10] r/ProjectSekai, via redlib mirrors:
  - "What is everyone's note speeds" (2023-10-26):
    https://axebps-redlib.hf.space/r/ProjectSekai/comments/17haeu7/what_is_everyones_note_speeds;
  - "What speed do y'all play at" (2025; the mirror shows only a relative date):
    https://axebps-redlib.hf.space/r/ProjectSekai/comments/1jgssd8/what_speed_do_yall_play_at.

  Most players at 10.0-11.8; one at 12 wishing for 13.
- [K11] "Project Sekai Tips, Tricks, and Help Compilation" (Ciry, last change 2023-05-08):
  https://docs.google.com/document/d/16ln3Iaxtr0FBF032SOoljvWoiK8hcBJQKzZMKhcMvas/mobilebasic. "Most serious players use
  ~9 to 11 note speed".
- [K12] Dengeki Online: https://dengekionline.com/articles/97140/ (2021-09-30: try 8.0-9.0; the default is gentle);
  https://dengekionline.com/articles/204262/ (3rd anniversary, 2023-09-30: APPEND, trace notes).
- [K13] Wikipedia, "Hatsune Miku: Colorful Stage!": https://en.wikipedia.org/wiki/Hatsune_Miku:_Colorful_Stage!.
  Developer, publisher, release dates, yellow notes score more.
- [C14] wikiwiki CHUNITHM, プロジェクトセカイから来た人へ (2025-12-12):
  https://wikiwiki.jp/chunithmwiki/プロジェクトセカイから来た人へ. Chunithm 8-9 ≈ Sekai 10; Chunithm long notes have no end
  judgement; Sekai is mostly 2-finger.

Ours:

- `osu.Game.Rulesets.ManiaOrtho/Highway/HighwaySettings.cs` (presets);
  `Configuration/ManiaOrthoRulesetConfigManager.cs:83-98` (setting ranges); `UI/HighwayNoteLayer.cs:39-59,226`;
  `UI/HighwayTrack.cs:32-78`.
- Existing notes: `topic-speed.md` (S1-S4, Sekai rows), `topic-covers.md`, `game-chunithm.md`,
  `game-chunithm-feedback.md` (P0, P1, P5, P8), `game-sdvx.md`, `topic-wide-keys.md`.
- Computations: scratch Python scripts outside the repository.
  - Geometry from [P1]'s constants.
  - Start-position model: s = lerp(0.073, 1, p/100), remaining time = −ln(s)/(45 · ln 1.06).
  - Linear-depth comparison: remaining = (1/s − 1)/(1/0.073 − 1).
  - Near-line speed: (J − VP) · 45 · ln 1.06 / T.
