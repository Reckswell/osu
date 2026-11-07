# Chunithm: animations and gameplay feedback

Last checked: 2026-11-03

## Summary

Chunithm draws almost all of its gameplay feedback **on the field**, not as a flat HUD. Judgement text appears once
per note, in that note's lane, at a depth chosen by the player (Bottom, Middle, Top or Heaven relative to the combo).
It follows the field's perspective, so it is "larger towards the bottom and smaller at the top" [S1][S11]. The combo
counter is also on the field, and the optional score readout sits above it [S1][S7]. Hit effects come from the
judgement line. In the two fan reimplementations we could inspect, Seaurchin (MIT) and UMIGURI, they are gold ripple
rings lying on the floor, rising sparkles, and vertical light curtains. Held slides get a cyan beam with an electric
splash [S23][S24]. ExTAP (the always-JUSTICE-CRITICAL accent note) also triggers a background effect, picked per note
from 8 directional patterns (up, down, centre, left, right, rotate left, rotate right, in/out) [S20][S21][S22]. A miss
shows only a grey MISS label in the lane, with no burst [S19 GIF, observed]. The fine detail of the real arcade
effects is still unverified: per-judgement differences, durations, combo colour states and touch-light decay. Every
frame-accurate source we tried (video, wikiwiki.jp, RemyWiki, image hosts) was blocked from this environment. See
"Unverified" at the end.

## Scope, evidence and versions

**Evidence used, strongest first.** Each claim below is tagged with its source.

1. Official SEGA pages [S6][S7][S8] and the chunithm.org English guide [S1]-[S5].
2. Community wikis and blogs [S9]-[S18][S29].
3. **One real-game capture:** a 65-frame GIF of the MASTER Cyaegha excerpt, recorded with no input (every note is
   missed), in the public-domain Chunithm-Research repository [S19]. The repository was "written with Chunithm
   Paradise in mind" (General.md:5), so the GIF is probably PARADISE-era (inference). The GIF's frame timing is **not
   uniform**: chart beats 0 to 2 span about 30 frames, but beats 2 to 4 span only about 7 (checked against the chart
   in Charting.md:276-297). **No duration in this note comes from the GIF.**
4. **Fan reimplementations, used only as proxies for the arcade look:**
   - Seaurchin 0.82 (2017, MIT) [S23]. It is real code, so its timings are exact, but they are Seaurchin's own values,
     not SEGA's. Its score processor is a stub that awards JUSTICE CRITICAL to everything ("TODO",
     ScoreProcessor.cpp:256). So it has **no per-judgement visuals**.
   - One UMIGURI Beta v2.02.7 gameplay screenshot from its author's MIT repository [S24]. UMIGURI is a closed-source
     Chunithm-style game. It is not SEGA's art.
5. Not used: `UMIGURIDX/UmiguriDX` on GitHub is a deobfuscated runtime dump of UMIGURI's proprietary code. Its licence
   is unclear, so we did not read it for this note. `chunithm-project/chunithm-clone` (GPL-3.0) is an empty Unity
   scaffold.

**Versions and the feedback-relevant changes in each.** Dates are Japanese releases.

| Version | Feedback-relevant change | Source |
|---|---|---|
| CHUNITHM (2015-07-16), PLUS | Black and gold UI theme | [S18][S29] |
| AIR / AIR PLUS (2016-08 / 2017-02) | Sky theme (turquoise, purple). AIR PLUS: the MISS label on the result screen turns grey | [S18][S9] |
| STAR PLUS (2018-03-08) | ExTAPs whose hit "changes the background motion" ("やべーEXTAP", "イカすSLIDE", official names), with more ExTAP effect patterns. Optional TAP success sounds | [S9][S10][S11] |
| AMAZON PLUS (2019-04-11) | Field wall, judgement timing adjustment, judgement position "天" (Heaven) | [S9] |
| CRYSTAL (2019-10-24) | FAST/LATE counts on the result screen | [S9][S7] |
| by PARADISE (exact version unverified) | In-play "judgement detail" (FAST/LATE) for JUSTICE and ATTACK | [S11] |
| NEW (2021-11-04) | FAST/LATE also for JUSTICE CRITICAL. New judgement-detail modes "FAST/LATE only" and "OFF". Optional sound on JUSTICE-or-worse (or ATTACK-or-worse) judgements. Gold cabinet runs at 120 fps (silver: 60). New AIR-SLIDE, AIR-CRUSH and rainbow decorative notes | [S6][S9][S10] |
| 2021-12-16 update | AIR-SLIDE centre line changes from blue to green | [S9] |
| LUMINOUS (2023-12-14) | Notes of any width from 1 to 16 cells. AJC mark. ExTAP stacked on a HOLD or SLIDE start fixed | [S9][S10][S2] |
| VERSE (2024-12-12) | New result layout and skill-dependent clear marks. Later, X-VERSE adds selectable stage backgrounds | [S30][S10] |

No source documents a change to the basic tap/hold hit effects between versions (unverified).

## Camera and track

The camera and track are covered by `game-chunithm.md`. Only what the animations depend on is listed here.

- **Judgement line.** In the GIF it is a bar with a white core and a gold rim, about 2% of frame height thick, across
  the full field width. Its brightness stays at 199-215/255 whenever no note covers it, in all 65 frames, so it does
  **not** pulse with the beat (observed: mean luminance of a 300x4 px strip, per frame). Seaurchin's line texture is
  yellow, `#FFF289`, with a soft alpha falloff, drawn additively [S23 Lane-JudgeLine.png, ScenePlayer.Draw.cpp:86-92].
- **Field brightness** goes from 0 to -5. At 0 the lanes "are transparent enough to see the background"; at -5 they
  are black [S16][S11]. In the GIF the lanes are near-black (luminance 3/255), so a low setting was in use.
- **Guidelines** split the field with OFF, 2, 4 (default), 8 or 16 vertical lines [S1].

## Note approach and speed (spawn behaviour only)

- **No fade-in at the far end.** Notes and hold bodies enter at full opacity from beyond the top edge of the screen,
  clipped by it (GIF frames 21 and 60, observed). Seaurchin draws notes into the field texture, so they simply appear
  at its far edge with no fade either [S23 ScenePlayer.Draw.cpp:81-108].
- **Field wall** (0-16) hides the far part of the field. At 15 only the judgement line is visible; at 16 nothing is
  [S1][S11].
- **Bar lines** are thin white lines across the field ("thin measure lines", Chunithm-Research Charting.md:88). They
  have no animation in the GIF (observed).
- **Unhit long notes continue past the line.** Missed SLIDE bodies and AIR arrows are drawn past the judgement line
  for at least one more frame. Missed taps are gone by the next frame (GIF frames 15-17, 36-38, observed).
- **After a note crosses its judgement time** it moves at 1x speed, whatever the scroll-speed modifier. This is
  UMIGURI's documented behaviour, attributed by its author to the original game ("本家") [S28].

## Feedback (judgement display, hit effects, lane lighting)

### 1. Hit effects at the judgement line

| Note | What is known | Strength |
|---|---|---|
| TAP (red) | Seaurchin: a 256x256 sprite animation of a gold ring that expands and lingers while sparkles rise and fade. 60 frames x 16.6 ms = **1000 ms**: sparkles fill about the first half, the ring fades over the rest. Anchored near its bottom (origin 128,224) at the projected judgement-line point of the note's centre, drawn in **screen space**. The ring itself is drawn pre-flattened, so it reads as lying on the floor. Not scaled by note width [S23 Skin.as:48, ScenePlayer.Draw.cpp:136-150, Effect-Tap.png viewed]. UMIGURI: a gold swirl ring flat on the floor (white core `#FFFFFF`, rim `#EDBE59`/`#F4F19C`), about **2.7 cells** wide and about 0.44 as tall as wide on screen (that is, foreshortened), with gold sparkles above it [S24, measured] | proxy |
| ExTAP (yellow) | Always JUSTICE CRITICAL unless missed, and a "production" (演出) plays on hit [S10][S11][S14]. The visible part is a **background effect** in one of 8 chart-chosen directions: `UP`, `DW`, `CE`, `LC`, `RC`, `LS` (rotate left), `RS` (rotate right), `BS` (in/out) [S20 c2s.md:39-42, 179-190][S21][S22]. Players describe it as "the background is dazzling when you hit them" [S17], and a short background effect, with several patterns, plays on a successful hit [S16]. STAR PLUS added more patterns [S11]. Seaurchin also plays a gold vertical ray curtain that widens upward, 30 frames = **500 ms**, scaled horizontally by note width / 6 cells, on top of the normal tap burst [S23 Skin.as:49, ScenePlayer.Draw.cpp:151-160, ScoreProcessor.cpp:228-232] | official + proxy |
| HOLD (yellow head, yellow-to-purple body) | The head is judged like a TAP. While held, judgements keep arriving at ticks: "JUSTICE CRITICAL keeps appearing while you touch" [S12]. Tick spacing depends on BPM: **1/16 note below 120 BPM, 1/8 from 120 to 240, 1/4 at 240 and above** [S11][S13]. Seaurchin re-spawns the tap burst at every hold tick [S23 ScoreProcessor.cpp:264-288]. UMIGURI shows a gold curtain of vertical rays rising from a ring over a lane group about 4 cells wide, which is consistent with a held HOLD (cannot be confirmed from one frame) [S24] | wiki + proxy |
| SLIDE (blue, moving) | Same tick rules [S11]. Seaurchin plays a one-off slide-start burst (1000 ms), then a **looping** blue scribble (16 frames x 16.6 ms = 266 ms per loop). The loop follows the slide's x position along the judgement line every frame and is dismissed at the slide's end [S23 Skin.as:51-52, ScenePlayer.Draw.cpp:185-256]. UMIGURI: a held slide shows a cyan column (`#2FFFFF`) with a white electric crackle and a translucent blue ellipse at the line [S24] | proxy |
| FLICK | No effect description found (unverified). UMIGURI charts store an "effect direction during autoplay" (auto/left/right), which implies a directional flick effect [S20 ugc.md:126][S22] | weak |
| AIR / AIR-ACTION / AIR-CRUSH | Seaurchin: air-action burst at air height, 48 frames x 8 ms = **384 ms** [S23 Skin.as:50]. UMIGURI stacks an "AIR" tag above the judgement text [S24]. A player calls the AIR-CRUSH hit effect "satisfying" [S17]. Not applicable to osu!mania | proxy |
| DAMAGE (WORLD'S END) | Touching it gives an electric-shock look and a MISS with a different sound [S9][S29] | wiki |
| **MISS** | **No burst.** Only a grey label in the lane (GIF, observed: no flash at the line in any missed frame). EXPERT and lower also flash a bottom-right graphic telling the player to raise their hand higher on a missed AIR; it cannot be turned off [S2] | real capture |

**Per-judgement differences** in the effect itself (JUSTICE CRITICAL vs JUSTICE vs ATTACK) are **unverified**. No
text source describes them, and both fan reimplementations show only the JUSTICE CRITICAL case.

**Plane vs screen.** Seaurchin draws effects as 2D sprites after the 3D field, positioned with
`ConvWorldPosToScreenPos` at the line, so they are billboards [S23 ScenePlayer.Draw.cpp:108-109, 140]. UMIGURI's rings
look like they lie on the plane: they are foreshortened and centred on the line. Its sparkles and curtains rise
vertically in screen space [S24].

### 2. Judgement display

- **One text per note, in the note's lane.** In the GIF, two simultaneous missed notes produce two labels at their two
  lane positions (observed). UMIGURI shows three "JUSTICE! / CRITICAL" texts at three lane positions at once [S24].
- **Depth is chosen relative to the combo display:**
  - Bottom (default): below the combo number.
  - Middle: directly above the word "COMBO".
  - Top: slightly above the combo display.
  - Heaven ("天", AMAZON PLUS): slightly above Top.

  The preset defaults are Bottom for BEGINNER and INTERMEDIATE and Middle for ADVANCED. "Indicators follow perspective,
  and are larger towards the bottom and smaller at the top" [S1][S11][S9]. Players move the text up to Top or Heaven
  when it hides notes [S16].
- **In the GIF**, the (illegible, about 12x4 px) labels sit at a fixed far depth, 18-22% of the frame height from the
  top. That is about 80% of the way from the line to the top edge in screen space, so the capture probably used Top or
  Heaven (inference). Each label is about 0.08 of the field width at that depth. Labels stay at the same depth for at
  least 4 consecutive frames, with no visible rise, and are drawn over hold bodies (observed). Durations: see the GIF
  caveat above.
- **Art.** UMIGURI: a large gold "JUSTICE!" (`#D1B74B`) over a small pale "CRITICAL", upright (not lying on the
  floor), scaled by depth [S24, sampled]. Seaurchin's text colours: J.CRITICAL `#DF932A`, JUSTICE `#DA9A6F`, ATTACK
  `#2CD644`, MISS `#8F8F8F` [S23 Judge-*.png, sampled].
- **Colours in the arcade:** JUSTICE is orange or red. Players call a non-critical JUSTICE "赤J" (red J) [S13], and
  one answer lists "JUSTICE orange, ATTACK green, MISS blue" [S15]. MISS labels in the GIF look grey (observed), and
  the result-screen MISS label has been grey since AIR PLUS [S9]. **MISS in play: grey (GIF) vs blue (one user
  answer), unresolved.**
- **FAST/LATE.** In play, it is shown for JUSTICE and ATTACK through the "judgement detail" option [S11], and for
  JUSTICE CRITICAL as well since NEW. Since NEW the base judgement can also be hidden: show "FAST/LATE only", or "OFF"
  to show nothing [S6][S1]. Players also turn JUSTICE CRITICAL display off when it gets in the way [S16]. The position
  and colours of the FAST/LATE text are **unverified**. The result screen has had FAST/LATE counts since CRYSTAL and a
  FAST/LATE graph since NEW [S9][S6].

### 3. Combo display

- **On the field, centred:** a "COMBO" label above a large number (UMIGURI: silver letters with a dark outline,
  `#EBEDE7`/`#BDADD5`) [S24]. This order matches the judgement-position definitions in [S1]. Notes and slides are
  drawn **over** the combo (UMIGURI, observed) [S24].
- Seaurchin draws the combo **into the field texture**, so it lies on the plane. It is centred, at 16.7% of the field
  length above the line (y 3200 of a 3840 px buffer), with glyph height of about half the field width (font 64 x scale
  8 in a 1024 px-wide buffer) [S23 ScenePlayer.Draw.cpp:60-62, 81-94; ScenePlayer.h:71-73].
- **Increment animation.** Seaurchin: scale 1.05 -> 1.00 over **200 ms**, `out_quad`, restarted on each increment
  [S23 ScenePlayer.cpp:245-251]. The arcade's animation is unverified.
- **Combo 0.** No combo number is visible anywhere in the all-miss GIF, consistent with the combo being hidden at 0
  (observed; the rule itself is unverified). The Field Indicator option "OFF" hides COMBO/CHAIN entirely [S11].
- **Score readout above the combo.** Field Indicator can show SCORE, SCORE counting down from 1,010,000, or the gap to
  a target rank or personal best. The gap display disappears once the target is unreachable [S1][S7][S11].
- **Milestones (every 50/100?), colour changes for ALL JUSTICE / FULL COMBO, and combo-break animation: unverified.**
  No source mentions them; the GIF has no hits.

### 4. Lane and touch feedback; cabinet LEDs

- **Field light under the hand:**
  - UMIGURI: a touched cell shows a pale column of about 30% white over the lane colour. It is one cell wide, runs
    from the line to the far end of the field, and follows the lanes' convergence. The slider LED under it is pale
    pink (`#FFC9FF`) where its neighbours are magenta or orange [S24, measured: lane `#180834` -> band `#61536A`].
  - Seaurchin: while a cell is touched, a white quad is drawn on the field. Alpha falls linearly from 255 at the line
    to 0 at the top, over **26.7% of the field length** (64x512 texture at scale 1x2 in a 3840 px buffer). It switches
    **on and off instantly**, with no fade [S23 ScoreProcessor.cpp:522-535, Lane-HoldLight.png].
  - The arcade's colour and decay are **unverified**.
- **AIR-HOLD hand indicator.** During an AIR-HOLD, a green horizontal line appears on the judgement line. It turns red
  when the hand is below the detection range and blue when above [S12].
- **Cabinet LEDs.** The game drives the slider LEDs itself: 16 cells and 15 dividers, with an RGB value each (BRG
  order on the wire) [S26][S25]. UMIGURI sends them every 33 ms [S25]. In menus the cells show button functions
  (green left/right, orange select, grey level, yellow genre) [S27]. The side AIR-sensor LEDs change from green/white
  to pink (or red) on the last song of a credit [S3][S12]. What the slider LEDs show during play is unverified.

### 5. Field background

- Each song plays over a 3D or animated background scene, by version and label. X-VERSE made it selectable as a
  "stage background" [S10]. At field brightness 0 the lanes are translucent over it [S16].
- **No beat-synced pulse was observed.** Over the clip, the GIF background's mean luminance drifts slowly (62 -> 76 ->
  36), and the field and judgement line stay constant (observed). We found no source for field-edge or wall pulsing.
- **ExTAP background effects** are the main "background reacts to play" mechanism (section 1).

### 6. Gauge, score and end of play

- **Gauge.** A bar at the top of the screen with lamps. The song is cleared when the required lamps light (they turn
  yellow). Good timing raises the gauge and MISS lowers it. Each lamp needs more gauge than the one before [S12][S11].
  A skill that pushes the gauge over the line at the end flips the display from "failed" to "clear" [S12]. Seaurchin's
  own animations: the fill tweens over 100 ms `out_sine`, and a newly lit lamp pops scaleX 1.0 -> 0.8 over 300 ms;
  judgement counters pop scaleX 1.3 -> 1.0 over 300 ms [S23 Play.as:142-174].
- **Score** sits in the top-centre score tracker, with judgement counts above it [S3]. Any roll-up animation is
  unverified.
- **End of chart.** After the last note, the progress bar at the top animates and a **FULL COMBO / ALL JUSTICE
  graphic** appears. The results follow after the audio ends [S19 General.md:13]. ALL JUSTICE shows an "ALL JUSTICE!"
  message [S18]. LUMINOUS added ALL JUSTICE CRITICAL (AJC) [S18][S10]. FULL COMBO means no MISS; ALL JUSTICE means no
  ATTACK and no MISS [S5].

### 7. Skills and characters (excluded)

The character portrait and skill description sit at the left [S3]. Skills only change the gauge, never the score
[S9]. Since NEW, an optional sound (the skill-activation sound) plays on every JUSTICE-or-worse (or ATTACK-or-worse)
judgement [S1][S6]. VERSE's "HOT OVER POWER" effects belong to the menus [S30]. None of this fits an osu! ruleset.

### 8. Sound paired with visuals

- **Guide sound.** A metronome tick at each note's correct time, ignoring offset settings [S1].
- **Hit sounds.** Optional per-type success sounds: TAP (with selectable sound fonts), ExTAP (a burst-like sound
  [S14]), HOLD/SLIDE, AIR and FLICK. The HOLD/SLIDE volume also covers the sound while holding, the end point, and a
  "clicking sound at visible tick points" [S1]. TAP success sounds play on red TAP, ExTAP and HOLD/SLIDE starts [S11].
- These explain the tick visuals (each tick is both heard and shown). osu! keeps its own hitsounds; nothing here
  changes that.

## What this means for us

Everything below is cosmetic. It reads `JudgementResult`s and input state and never feeds back into judgement or
scoring (invariants 1 and 3).

### P0. Alpha's baseline shapes the defaults

How Alpha plays today [S31]:

- osu! stable, 4K first.
- Scroll speed 29, about 396 ms visible.
- Judgement line at the bottom edge.
- The skin's MAX and 300 judgement images are transparent, so **only 200/100/50/miss show text**. In lazer terms,
  Perfect and Great are hidden.
- **Hit lighting on.**
- **Combo burst and combo fire off.**
- **Background dim 100.**
- **Bar lines hidden.**
- Hit error meter on.

Chunithm has the same instinct. Since NEW it can hide the base judgement per tier ("FAST/LATE only" or "OFF"), and
players turn JUSTICE CRITICAL display off when it gets in the way [S6][S16]. So:

- Judgement text gets a per-tier visibility setting (P3). The default hides Perfect and Great.
- Hit bursts stay on for every hit, matching Alpha's hit lighting. With top-tier text hidden, the burst colour is what
  tells Perfect from Great (P2).
- No combo milestone or burst effects, and the combo pop can be turned off (P4).
- Bar lines are optional and off in an Alpha-style preset (P8).
- Background-driven effects such as ExTAP sweeps are invisible at dim 100, which is one more reason to drop them
  (P10).
- FAST/LATE text is off by default. The osu! hit error meter stays in screen space (SPEC 7). Chunithm's own default
  for judgement detail is also OFF [S1].

Visible time matters too. At about 400 ms, notes 60-200 ms from the line sit where on-track judgement text would be
(d = 0.15-0.50). Hiding the top tiers removes nearly all of that clutter on a good play, so P3's Track mode stays
usable at Alpha's speed.

### P1. Mapping mania results to Chunithm-style feedback (SPEC 7)

Chunithm's JUSTICE CRITICAL window (±33.3 ms [S4]) falls between mania's Perfect (±16.5 ms at OD 8) and Great (±40.5
ms at OD 8), computed from `ManiaHitWindows.cs:12-17`. In both games the top two tiers differ by a small bonus:
JUSTICE CRITICAL is worth 101% vs JUSTICE 100% [S5], like mania's Perfect vs Great. ATTACK (50%) keeps the combo, like
mania's Good, Ok and Meh. So we tier by `HitResult`. We do **not** re-time anything, so the display always agrees with
the results screen.

| mania `HitResult` | Chunithm-style tier | Text (default: mania names) | Colour (proposal) | Hit burst |
|---|---|---|---|---|
| Perfect | JUSTICE CRITICAL look | PERFECT (text hidden by default, P0) | gold `#FFD54A` | full: gold ring + core flash + 8 sparkles |
| Great | JUSTICE look ("red J") | GREAT (text hidden by default, P0) | orange-red `#FF7A3D` | ring tinted orange, 4 sparkles |
| Good, Ok | ATTACK look | GOOD / OK | green `#2CD644` | small white-green ring, no sparkles |
| Meh | ATTACK look, dimmed | MEH | `#7FAF7F` | small ring at 60% alpha |
| Miss | MISS | MISS | grey `#8F8F8F` | **none** (same as Chunithm, and the same as mania: `Column.cs:181` skips explosions on misses) |

**Ask Alpha:** should the text use Chunithm's names (JUSTICE CRITICAL, JUSTICE, ATTACK)? That is a branding question
and they are SEGA's terms. Default to mania's names in Chunithm-style art until Alpha answers.

### P2. Hit burst (SPEC 7: "drawn at the judgement line in the lane's projected position")

**Drawn on the track plane:** the ring and core flash, centred on the lane at d = 0. They are projected like notes, so
the tilt flattens them automatically, as in UMIGURI.

- Ring: outer diameter grows from 0.6 to 1.3 lane widths over 350 ms with `OutQuint`, while alpha goes 1 -> 0 over 350
  ms. Additive blending.
- Core flash: 0.8 lane widths, alpha 0 -> 1 -> 0 over 120 ms.
- Upper bound for the ring: 0.3 x the track width, so 4K bursts (where one lane is 0.25 of the track) stay at about
  1.2 lanes and do not swamp the field. For reference, UMIGURI's ring is 0.17 of the field width.
- Perfect and Great bursts must be easy to tell apart (gold vs orange-red, 8 vs 4 sparkles). With Alpha's defaults
  the burst is the only per-note signal for those two tiers (P0).
- This is the highway's equivalent of stable's hit lighting, which Alpha keeps on, so it is on by default.

**Screen space, billboarded at the projected line point:** the sparkles. 4-8 points rise 0.06-0.10 of screen height
over 500 ms, fading out.

**Timing:**

- Total lifetime at most 500 ms. Seaurchin's 1000 ms ring lingers too long for dense 7K streams; mania's default
  explosion lasts 200 ms (`PoolableHitExplosion.cs:16`).
- Pool 8 bursts per lane; mania pools 5 per column (`Column.cs:95`). At 200 BPM 1/16 streams, notes in one lane arrive
  75 ms apart, so a 350 ms ring has at most 5 copies alive.
- Durations are proposals *(tune)*, since the arcade's are unverified.

### P3. Judgement text (SPEC 1 and 7, **conflict to raise**)

The SPEC puts judgement text and combo in screen space ("HUD, judgement text and combo stay in screen space"). Chunithm
puts them on the field, per note, in perspective [S1]. Alpha has now named Chunithm as the main reference. That changes
the spec's intent, so it is an **Ask Alpha** item. Proposal:

- Add a setting **"Judgement placement: Screen (osu!mania) / Track (Chunithm)"**. Default to Screen per the current
  SPEC until Alpha answers. Recommend Track as the Chunithm preset.
- **Track mode:**
  - One text per lane: a new result in a lane replaces that lane's old text, which bounds clutter to the key count.
  - Placed at the lane centre at a fixed depth, with a setting for the four Chunithm positions: Bottom d = 0.15,
    Middle 0.30, Top 0.40, Heaven 0.50 (fractions of the visible track length) *(tune)*.
  - Drawn upright as a billboard, as in UMIGURI, and scaled by the projection's scale at that depth, so it is "larger
    towards the bottom". Clamp to a minimum of 14 px cap height so 10K stays readable (our addition).
  - Animation (ours; the arcade's is unverified): pop from 1.2x to 1.0x in 60 ms `OutQuad`, hold, fade out over the
    last 150 ms, 450 ms total. No travel: the GIF shows none.
- **Per-tier visibility**, in both modes: "Show judgement text for: All / Hide Perfect / Hide Perfect and Great".
  The default is **Hide Perfect and Great**, Alpha's stable habit [S31], close to Chunithm's per-tier OFF [S6].
  - Our own judgement containers filter by `HitResult` before taking from the pool.
  - A legacy skin with transparent `hit300g`/`hit300` images already hides those tiers in Screen mode through the
    skin. The setting covers the built-in highway skin and Track mode.
- **FAST/LATE:** a small line under the word, **off by default**, matching Chunithm's default and Alpha's use of the
  hit error meter instead.
  - When on: shown for Great/Good/Ok/Meh, and optionally for Perfect (Chunithm NEW).
  - Modes "FAST/LATE only" and "Off" as in [S6]. "FAST/LATE only" lets a hidden tier still show its timing direction.
  - Colours are ours: FAST `#6EC6FF`, LATE `#FF8A65`.
  - Compute it from `JudgementResult.TimeOffset`.
- **Screen mode** keeps mania's single aggregated judgement (`Stage.cs:213-220`).
- **Implementation:** Track mode needs our own per-lane `JudgementPooler` in the ortho playfield, not edits to mania
  (invariant 4).

### P4. Combo (SPEC 7, same conflict)

- **Track mode:**
  - Draw "COMBO" and the number on the track plane, centred: number centre at d = 0.28, height 0.10 of the track
    length; label at d = 0.40, height 0.035 *(tune)*.
  - White/silver at 50% opacity, in a layer between the track background and the notes, so notes draw over it as in
    UMIGURI.
  - Pop to 1.05x and back over 200 ms `OutQuad` on each increment (Seaurchin's values).
  - Hide at combo 0 (consistent with the GIF).
- **Screen mode:** keep osu!'s combo counter.
- Do **not** add milestone effects or AJ/FC colour states until a source confirms Chunithm has them. Alpha plays
  with combo burst and combo fire off [S31], so none of this is wanted by default anyway.
- Add a "combo pop" toggle, on in the Chunithm preset and off in an Alpha-style preset.

### P5. Lane beam on key press (SPEC 5: "SDVX-style beam rising from the judgement line")

- Drawn on the track plane, one lane wide.
- White, or the lane colour lightened by 0.8, at **30% alpha** at the line, fading linearly to 0 at d = 0.5. This sits
  between UMIGURI (a uniform 30% column to the far end) and Seaurchin (100% -> 0 over 0.27 of the length).
- Fade in over 30 ms, hold while pressed, fade out over 150 ms `OutQuad` on release. The arcade's decay is unverified;
  Seaurchin is instant; mania Argon uses 70 ms in and 800 ms out (`ArgonKeyArea.cs:188, 232-239`).
- Also brighten that lane's segment of the judgement line while pressed.

### P6. Holds in progress

- Bind to `DrawableHoldNote.IsHolding` (`DrawableHoldNote.cs:37`). While true:
  - Raise the lane beam to 45% alpha.
  - Brighten the hold body by +25%. Mania Argon pulses its hitting layer every 80 ms
    (`ArgonHoldNoteHittingLayer.cs:35-53`).
  - Emit a small ring and 2-3 sparkles at **Chunithm's tick cadence**: 1/16 beat below 120 BPM, 1/8 from 120 to 240,
    1/4 at 240 and above [S11][S13]. Take the BPM from the beatmap's timing point at the current time.
- **No per-tick judgement text or combo.** Mania holds have no tick judgements (the body is `IgnoreHit`/`ComboBreak`,
  `HoldNoteBodyJudgement.cs:10-11`), so showing Chunithm's per-tick "JUSTICE CRITICAL" would misreport the score.

### P7. Miss

Show the grey MISS text only (P3), with no burst. Unhit hold bodies keep scrolling past the line, as both mania and
Chunithm already do.

### P8. Spawn, bar lines, background (SPEC 3, 4, 6)

- Notes appear opaque at the far edge or wall, with **no fade-in**, matching Chunithm and SPEC 2.
- Bar lines: thin white lines at about 35% opacity across the track, with no animation. Make them a setting: on in
  the Chunithm preset, off in an Alpha-style preset (Alpha's skin hides them [S31]).
- Judgement line: white core with gold rim (`#FFF289`), **no beat pulse**.
- Add a "track opacity" setting modelled on Field Brightness 0 to -5: track background alpha from 0.6 (translucent)
  to 1.0 (black). Default 0.85 *(tune)*. At Alpha's background dim of 100 the result is black either way.

### P9. In-play clear banner (optional, later)

- After the last judgement, show a screen-space banner for 1.5 s before osu!'s results screen:
  - **FULL COMBO:** no Miss and no hold `ComboBreak`.
  - **AJ analogue:** only Perfect and Great.
  - **AJC analogue:** only Perfect.
- Derive it from the score processor's statistics, read-only. The banner names need Alpha's branding call.

### P10. Not worth copying

- ExTAP background sweeps: mania has no accent note, and at Alpha's background dim of 100 [S31] a background effect
  would not be seen. If we ever want them, an optional kiai-start sweep using one of the 8 directions, off by default.
- Skills, characters, gauge lamps and LED output.
- The arcade's guide sound: osu! already has hitsounds and a metronome-free design.

### Summary: where each effect is drawn

| Layer | Effects |
|---|---|
| Track plane (projected) | ring, core flash, lane beam, hold glow, judgement line, bar lines, combo (Track mode), wall |
| Screen-space billboard at a projected point, scaled by depth | sparkles, judgement text and FAST/LATE (Track mode) |
| Flat HUD | osu! score/accuracy/pp/progress, hit error meter, aggregated judgement and combo (Screen mode), clear banner |

## Unverified / open questions

- Hit-effect differences between JUSTICE CRITICAL, JUSTICE and ATTACK, and all real effect durations. These need
  frame-accurate video (ideally from a 120 fps Gold cabinet, NEW or later).
- Judgement text animation (pop, rise, fade) and lifetime. Where FAST/LATE sits, and its colours.
- MISS text colour in play: grey (GIF) vs blue (one user answer [S15]).
- Combo milestone effects, AJ/FC colour states, and the combo-break animation.
- Touch-light colour and decay, and what the slider LEDs show during play.
- Whether judgement text lies on the floor or stands upright in the arcade (UMIGURI's stands upright).
- The version that added in-play FAST/LATE for JUSTICE and ATTACK.
- **Blocked from this environment** (egress policy, not retried): wikiwiki.jp/chunithmwiki (ゲームシステム1, 小ネタ集,
  用語集), silentblue/RemyWiki, YouTube, images on chunithm.org, info-chunithm.sega.jp, chunithm.sega.com and
  twimg/hatena. These are the next places to look, together with video measured frame by frame.

## Sources

- [S1] [Settings, Chunithm English Guide](https://chunithm.org/basic/settings/): judgement position semantics and
  perspective note, judgement detail indicator, field brightness, guidelines, field wall, field indicator, guide sound,
  judgement sound trigger, version markers.
- [S2] [Gameplay and Note Types, chunithm.org](https://chunithm.org/basic/gameplay/): note colours, the AIR-miss
  graphic, the LUMINOUS ExTAP-over-hold fix.
- [S3] [Game Screen, chunithm.org](https://chunithm.org/basic/gamescreen/): HUD layout (score tracker,
  character), side LEDs on the last chart.
- [S4] [Timing Windows, chunithm.org](https://chunithm.org/basic/timing/): TAP window JC < 33.33 ms, J < 66.67 ms,
  ATTACK < 83.33 ms; hold ticks.
- [S5] [Judgements, chunithm.org](https://chunithm.org/basic/judgements/): 101/100/50/0% weights, FULL COMBO and ALL
  JUSTICE definitions.
- [S6] [『CHUNITHM NEW』で追加される新機能について, SEGA](https://info-chunithm.sega.jp/1098/): FAST/LATE for JC,
  "FAST/LATE only" and "OFF" modes, judgement sound, FAST/LATE result graph.
- [S7] [Others, CHUNITHM International (SEGA)](https://chunithm.sega.com/play/star_function.html): field wall,
  "information ... added above the combo display", FAST/LATE on results.
- [S8] [How to play, CHUNITHM International (SEGA)](https://chunithm.sega.com/play/): basic note descriptions.
- [S9] [チュウニズム, Wikipedia (ja)](https://ja.wikipedia.org/wiki/%E3%83%81%E3%83%A5%E3%82%A6%E3%83%8B%E3%82%BA%E3%83%A0):
  version history (AIR PLUS grey MISS, STAR PLUS background-changing ExTAP, AMAZON PLUS "天", CRYSTAL FAST/LATE, NEW
  120 fps, AIR-SLIDE colour change, LUMINOUS widths), the skills-only-affect-gauge rule.
- [S10] [CHUNITHM, ピクシブ百科事典](https://dic.pixiv.net/a/CHUNITHM): ExTAP hit triggers a 演出, STAR PLUS
  official names "やべーEXTAP"/"イカすSLIDE", AJC mark, X-VERSE stage backgrounds.
- [S11] [ゲームシステム・詳細, CHUNITHM攻略Wiki改 (gamerch)](https://gamerch.com/chunithm-puls/286117): judgement
  positions and perspective, judgement detail for J/ATTACK, ExTAP patterns increased in STAR PLUS, hold tick cadence
  by BPM, gauge values, field colour, guidelines.
- [S12] [CHUNITHM, ゲームカタログ@Wiki](https://w.atwiki.jp/gcmatome/pages/7349.html): JC keeps appearing while
  holding, AIR-HOLD line red/blue, side lamps red on the final song, gauge lamps.
- [S13] [ウニの裏技・マイナー仕様15選, 京音メンバーの日記](https://keionkakimasen.hatenadiary.com/entry/2023/02/01/200156):
  hold combo cadence, "赤J" for JUSTICE, 32 slider sensors.
- [S14] [ExTAPについて話す, 京音メンバーの日記](https://keionkakimasen.hatenadiary.com/entry/aboutextap): ExTAP has
  only JC and MISS; burst-like success sound.
- [S15] [チュウニズムのJ-CRITICALって何ですか, Yahoo!知恵袋](https://detail.chiebukuro.yahoo.co.jp/qa/question_detail/q11172780276):
  one user lists JUSTICE orange, ATTACK green, MISS blue (low weight).
- [S16] [初心者～紫レート攻略法, 今日も、つらつらと、ゲーム。](https://reoleotoge.hatenablog.com/entry/2022/03/02/172624):
  players move judgements to Top/Heaven or hide JC; field colour 0 is translucent; background effect on hit.
- [S17] [I want to teach you the fun of Gekichumai (CHUNITHM), note.com](https://note.com/ebicha0803/n/nad3cc94d0fb3?hl=en):
  "the background is dazzling" on ExTAP hits; AIR-CRUSH effect.
- [S18] [CHUNITHM, TV Tropes](https://tvtropes.org/pmwiki/pmwiki.php/VideoGame/CHUNITHM): "ALL JUSTICE!" message,
  AJC in LUMINOUS, version colour themes, Gold 120 Hz vs Silver 60 Hz.
- [S19] [Suprnova/Chunithm-Research @6604cc1](https://github.com/Suprnova/Chunithm-Research/tree/6604cc1310cb37a554792e927d8216d068ba591b)
  (Unlicense, public domain): General.md:5 (written for PARADISE), General.md:13 (FC/AJ graphic after the last note),
  Charting.md:88 (thin measure lines), Charting.md:182-185 (ExTAP extra field), Charting.md:276-297 (Cyaegha excerpt),
  `_assets/cyaegha example.gif` (65 frames, 450x284, real game, no input; frames inspected and measured with
  ImageMagick).
- [S20] [ChuniPingu/PenguinTools docs @ef1adcd](https://github.com/ChuniPingu/PenguinTools/tree/ef1adcdf7c6fb1b6a4ba527a20dadc7e5ed7d959/docs/formats)
  (MIT): c2s.md:39-42 (CHR Effect field), 49-52 (ExHold), 179-190 (8 ExTAP effect codes); ugc.md:120-126 (UGC effect
  letters, ExTAP over hold sets its effect, flick autoplay direction).
- [S21] [Umiguri Chart v8 仕様, inonote (gist)](https://gist.github.com/inonote/5c01e73781cab17765a1d93641d52298):
  ExTAP `d` effect values U/D/C/A/W/L/R/I.
- [S22] [ノーツ属性の変更, Margrete docs](https://umgr.inonote.jp/margrete/getting-started/note-attributes/): ExTAP
  attribute is "背景エフェクト方向" (background effect direction).
- [S23] [nazawa/Seaurchin @fffa136](https://github.com/nazawa/Seaurchin/tree/fffa13602ce6ee4413cd34c845293dd7ecd0a375)
  (MIT, Seaurchin 0.82, 2017):
  - `Resources/Skins/Default/Skin.as:48-52`: animation frame counts and frame times.
  - `Seaurchin/ScenePlayer.Draw.cpp:60-62, 81-109, 136-256`: combo in the field texture, effect spawning,
    billboarding, slide loop.
  - `Seaurchin/ScenePlayer.cpp:245-251`: combo pop.
  - `Seaurchin/ScoreProcessor.cpp:228-232, 256, 264-288, 522-535`: effects per note type, stub judging, hold ticks,
    touch light.
  - `Seaurchin/ScenePlayer.h:70-73`: field buffer.
  - `Resources/Skins/Default/Scripts/Play.as:142-174`: gauge and counter tweens.
  - Images `Effect-*.png`, `Judge-*.png`, `Lane-HoldLight.png`, `Lane-JudgeLine.png`: viewed and sampled.
- [S24] [inonote/UmiguriSampleLedServer `res/img.png` @60cd13b](https://github.com/inonote/UmiguriSampleLedServer/blob/60cd13b9f2bc76319b8b3bf843c2e43fcd473041/res/img.png)
  (MIT): UMIGURI Beta v2.02.7 gameplay screenshot (998x575) with a simulated slider-LED strip; measured and sampled
  with ImageMagick.
- [S25] [UMIGURI LED Controller Protocol v1, inonote (gist)](https://gist.github.com/inonote/00251fed881a82c9df1e505eef1722bc):
  16 lands and 15 borders, RGB, plus 3 AIR colours; sent every 33 ms.
- [S26] [PatrickkikALT/ChuniVR `Assets/Scripts/LEDs/LEDParser.cs` @3563356](https://github.com/PatrickkikALT/ChuniVR/blob/3563356b849e68ab4045d05b69bcbd21221f1c4b/Assets/Scripts/LEDs/LEDParser.cs)
  (MIT): the arcade game sends BRG triplets per LED over the LED board pipe.
- [S27] [Nat-Lab/chunithm-vcontroller screenshot](https://raw.githubusercontent.com/Nat-Lab/chunithm-vcontroller/master/doc/screenshot.png)
  (licence not checked; viewed only): slider LED colours mirror menu button functions.
- [S28] [ノーツ描画の仕様, いののて雑記](https://inonote.hatenablog.com/entry/2022/12/03/003138): notes move at 1x
  after their judgement time; draw order (narrower notes on top).
- [S29] [CHUNITHM, ニコニコ大百科](https://dic.nicovideo.jp/a/chunithm): ExTAP has a different sound, DAMAGE note
  behaviour.
- [S30] [CHUNITHM VERSEの新要素まとめ, SEGA](https://info-chunithm.sega.jp/8930/): VERSE result layout,
  skill-dependent clear marks, OVER POWER effects.
- [S31] `docs/ortho/research/baseline-alpha.md`: Alpha's stable setup (MAX/300 judgements hidden, hit lighting on,
  combo burst/fire off, background dim 100, bar lines hidden, hit error meter, scroll speed 29 ≈ 396 ms, 4K first).
- osu! code in this repository (MIT): `osu.Game.Rulesets.Mania/UI/Column.cs:95,176-185`, `UI/Stage.cs:213-220`,
  `UI/PoolableHitExplosion.cs:16`, `UI/DefaultManiaJudgementPiece.cs:44-74`, `Scoring/ManiaHitWindows.cs:12-17`,
  `Skinning/Argon/ArgonKeyArea.cs:188,232-239`, `Skinning/Argon/ArgonHoldNoteHittingLayer.cs:35-53`,
  `Objects/Drawables/DrawableHoldNote.cs:37`, `Judgements/HoldNoteBodyJudgement.cs:10-11`.
