# beatmania IIDX: lane covers, LIFT and the green number

Last checked: 2026-11-04

## Summary

IIDX is a flat, vertical 7-key + turntable game. It has no camera, but it is where SPEC 3 and SPEC 4 come from: the
**green number** (visible time) and **SUDDEN+** (a top cover the player moves during a song).

- **Green number (GN).** It counts the time from a note appearing below the cover to reaching the judgement line, in
  units of **1/600 s**: GN 300 = 500 ms [I3][I9].
- **White number (WN).** It gives the cover's size in thousandths of the lane [I3][I4].
- **Adjusting.** The player holds START and turns the turntable. This works before and during a song, and both numbers
  show while START is held [I1].
- **What FHS keeps.** With **Floating Hi-Speed** (FHS, since tricoro), moving SUDDEN+ re-computes the hi-speed so that
  the green number, which is visible time, stays fixed [I2][I3]. With **Classic** hi-speed, moving the cover keeps the
  hi-speed, so visible time shrinks.
- **LIFT and HIDDEN+.** LIFT raises the judgement line, with a cover image below it. HIDDEN+ is a bottom cover that
  few players use.

Our wall's default (constant visible time) is what FHS does, and its "keep speed" toggle is what Classic hi-speed does.

## Camera and track

- A flat 2D lane, with no perspective.
- The fan-made gear-change simulator treats the note lane as **723 px** tall and measures every cover in pixels of
  that lane [I3].

## Note approach and speed

### Hi-speed modes and their history

From iidx.org's feature history [I5], unless marked otherwise:

| Version | Change |
|---|---|
| 12 HAPPY SKY | Hi-speed 0.5-4.0 in 0.5 steps; SUDDEN+ and HIDDEN+ added |
| 13 DistorteD | Hi-speed up to 5.0 |
| 16 EMPRESS | Green number display added; LIFT added as a hidden option |
| 17 SIRIUS | LIFT available by default |
| 20 tricoro | **Floating Hi-Speed** added; plain SUDDEN/HIDDEN removed |
| 21 SPADA | Hi-speed can be changed only on the gameplay screen [I6] |
| 26 Rootage | NORMAL HI-SPEED (BPM-independent levels) added; min and max green number shown |
| 27 HEROIC VERSE | NORMAL HI-SPEED expanded to 20 levels; CLASSIC HI-SPEED option; FHS steps 0.25 instead of 0.5 |
| 31 EPOLIS | The "LIFT image" can be customised |
| 34 ZINRAI | Alternate floating (double tap START) |

**Normal hi-speed (NHS).**

- It is the default since BISTROVER, BPM-independent, and has levels 1-20 [I7].
- Each level sets the green number for the whole lane [I3]:
  - levels 1-3: GN 1200, 1000, 800;
  - levels 4-8: GN 700 to 500 in steps of 50;
  - levels 9-20: GN 480 to 260 in steps of 20.

**Classic hi-speed (CHS).**

- A multiplier from the list 1.00, 1.50, 2.00, 2.25, … 4.00 [I3].
- It is BPM-dependent [I7].

**Floating hi-speed (FHS).**

- A multiplier from 0.50 to 10.00, stored with more decimals than the two it displays [I3].
- START + a key moves it by 0.5 [I3], or by 0.25 since HEROIC VERSE [I5]. The two sources disagree on the current step.
- Turning the turntable with no cover shown fine-tunes it, by about 1/300 per tick [I3] ("one-hundredth decimal
  intervals" in [I6]).

### Green number

- **Definition.** "The display time from when the chart appears at the top of the lane until it reaches the judgement
  line" [I8]. A smaller number is faster [I8].
- **Unit.** "GN 10 reaches the judgement line in 1/60 s" [I3], so GN 300 = 0.5 s and 600 = 1 s.
  - beatoraja (below) stores visible time in ms and displays its green number as ms × 3/5 [B1], which is the same
    unit.
- **Formula.** Reverse-engineered by [I3], who calls it inferred from in-game checks:
  - GN = K / HS / BPM × (visible px / 723), where visible px = 723 − SUD+ px − LIFT px.
  - K = 174728 on the arcade (32 Pinky Crush, Lightning Model) and 174758 on INFINITAS.
  - The fhs calculator uses K = 174000 with (1000 − WN − lift) / 1000 [I10].
- **In ms.** The whole lane takes **T = 291,213 / (BPM × HS) ms**. USC's SDVX constant is 480,000 (`game-usc.md`).
- **Typical values.** Players prefer GN about 250 ("blazing fast") to 330 ("very slow") [I4], which is 417-550 ms.
  - That is mania speed 27.6-20.9 by the label 11485 / T, or about 28.2-21.4 for the same real visible time with argon
    (+2.3%, `game-osu-mania.md`).
  - Alpha's mania speed 29 is labelled 396 ms (GN 238). Its real visible time in lazer is 405 ms with argon (GN 243)
    and 473 ms with Alpha's own 4K skin (GN 284), because mania scales the time range by the skin's hit position
    (`game-osu-mania.md`, "Scroll speed to time range"). With his skin, he plays inside IIDX's usual range.
- **BPM.** FHS re-computes the green number from the **BPM at the start of the song** [I4].
  - A BPM change mid-song changes the green number. Players "re-float" by touching the turntable while holding START,
    or by double-tapping START twice [I4].
  - Since Rootage, songs with BPM changes show a GN range [I5][I8].

### What moving the cover does to speed

| Mode | Cover moved | Effect | Source |
|---|---|---|---|
| CHS | SUD+ or LIFT | HS kept, GN shrinks with the visible lane | formula [I3] |
| FHS | SUD+ | GN kept: "with SUDDEN+ applied, you can change SUDDEN+'s display area while keeping the note speed (green number) fixed"; "the hi-speed value is adjusted automatically" | official INFINITAS manual [I2] |
| FHS | LIFT | **unverified** | — |

For the FHS + SUD+ row, [I3] gives the formula: HS = HS at FHS start × (visible px now / visible px at FHS start),
clamped to 0.50-10.00. Past the clamp, the green number is no longer held.

For the FHS + LIFT row, the sources only say this much:

- [I4] says floating "requires SUDDEN+ or LIFT".
- [I3] lists the moments when HS is re-computed: START + scratch with SUD+ shown, START + keys, and toggling SUD+ while
  LIFT is shown or hidden. Moving LIFT alone is not in that list.

## Judgement line and receptors

- **LIFT** "literally lifts the judgement line" [I11].
  - Players use it when the screen sits too low for their eye level [I11].
  - It is set on song select and cannot be toggled during play [I12].
  - START + turntable moves it when it is the only cover active [I1][I12].
  - Range: white number 0-830, which is up to 600 of 723 px [I3].
- **Below a lifted line** the game draws a cover image.
  - Lane-cover artwork "can also be used as pacemaker and LIFT backgrounds" [I13].
  - Since EPOLIS the LIFT image is customisable [I5].
  - This settles the "(unverified)" in DECISIONS "The bottom lift is the judgement line height setting".
- **Receptors** are the physical keys. Key beams light the lane.

## Lane covers / walls

### Options

The official option list for 32 Pinky Crush, the 2024 arcade version [I1]:

| Option | Official description | Range (white number) | Notes |
|---|---|---|---|
| SUDDEN+ | "Hides the top of the chart" | 42-999 (30-723 px of 723) [I3] | — |
| HIDDEN+ | "Hides the bottom of the chart" | (unverified) | "Cosmetic", doesn't move the judgement line, rarely used [I11]; adjusted the same way as SUDDEN+ [I6] |
| SUD+ & HID+ | Both | — | "Few players use HIDDEN+" [I11] |
| LIFT | Moves the judgement line | 0-830 [I3] | — |
| LIFT & SUD+ | Both | — | With both on, the turntable moves SUD+ only. To move LIFT: double-tap START to hide SUD+, adjust, bring it back [I11][I12] |

- **Default.** The out-of-the-box option set is the Beginner Option [I1], which has no cover. "Almost everyone" uses
  SUDDEN+ with FHS [I11].
- **White number.** It is the cover height in thousandths of the lane (227 = 22.7%) [I4].
  - The game stores the cover in pixels of the 723 px lane, at 1 px resolution, and converts with
    WN = ceil(px × 999 / 723). That is why white numbers skip values [I3].
- **Cover art** is opaque artwork, with over 200 designs. Some are dark only up to a given white number, so players
  pick a cover to suit the size they use [I13].

### Adjusting during play

**Arcade.**

- Hold START and turn the turntable to move the cover. This works "during play (or before the song starts)" [I1].
- While START is held, the cover position shows in white and the note display time in green [I1].
- Double-tapping START quickly hides the cover; double-tapping again brings it back [I1]. The position is kept.

**INFINITAS (PC)** [I2]:

| Action | Keyboard | Controller |
|---|---|---|
| Cover on/off | Enter ×2 | E1 ×2 |
| Display area (with a cover) or judgement line (LIFT) | Enter + Up/Down | E1 + turntable |
| Hi-speed step | Enter + key | E1 + key |
| Switch to FHS | Enter + BS | E1 + E2 |

**Hi-speed.** Hold START: a black key is faster, a white key is slower [I7][I12].

**Step and repeat.**

- The turntable is analog, at 1 px of 723 (0.14% of the lane).
- How far the cover moves per turntable tick is not documented (unverified).
- Whether a held key or arrow repeats is not documented (unverified).

### beatoraja (IIDX-style BMS player, GPL-3.0: read for behaviour only)

beatoraja copies IIDX's model and is open source, so its exact rules can be read [B1]:

- **Visible time.** "duration" (ms) is the stored speed, default **500 ms**.
- **Hi-speed fix.** It defaults to the **main BPM**; the other choices are off, start, max and min.
- **Covers.** Lane cover 0-1, on by default at **0.2**. Lift 0-1, off by default, 0.1 when enabled. Hidden 0-1, off,
  0.1.
- **Moving the cover keeps visible time** whenever hi-speed fix is on:
  - `setLanecover` calls `resetHispeed`, which sets HS = 240000 / (BPM × duration) × (1 − cover).
  - Visible time is then region × (1 − cover), where region = 240000 / BPM / HS (`LaneRenderer.java:206-214,321,330`).
- **Lift keeps visible time.** One measure spans the lifted lane × HS (`LaneRenderer.java:325-326`), so lifting shortens
  the lane on screen and slows the notes.
- **Adjusting** (`ControlInputProcessor.java:231-268`):
  - START + scratch moves the cover by 0.001 per analog tick.
  - On digital keys it moves 0.001 every 50 ms, then 0.01 every 50 ms once the key has been held 500 ms. That is
    2%/s, then 20%/s (`PlayConfig.java:87-95`).
  - A double tap of START within 500 ms toggles the cover (line 165).
  - START + keys change HS by 0.25 × the base HS.
  - SELECT + keys change the duration by 1 ms.
- **Display.** The white number is cover × 1000. The green number is the current duration × 3/5
  (`IntegerPropertyFactory.java:70,305-310`).

### Notes and holds, key count, feedback

Out of scope for this note. IIDX is 7 keys + scratch (14 + 2 in DP) on a flat lane.

## What this means for us

1. **Wall default (SPEC 3, 4): matches.** IIDX FHS + SUDDEN+ keeps visible time when the cover moves [I2], and so do
   beatoraja's defaults [B1]. Our default (`WallKeepsVisibleTime = true`, `HighwayScroll.ScrollLength = 1 − wall`) is
   the same rule on a tilted track: raising the wall slows the notes on screen, as FHS lowers HS. The "keep speed"
   toggle is IIDX Classic and Chunithm. **No change.**
2. **Wall as a fraction (SPEC 4): matches.** The white number is a fraction of the lane, which on a flat lane is both
   depth and screen. Our depth fraction is the right analogue. Under the Chunithm camera, 40% of the depth is only
   12.8% of the screen (`topic-covers.md`).
3. **Mid-song adjustment: matches.** IIDX moves the cover "during play (or before the song starts)" [I1], under FHS and
   under Classic hi-speed alike; under Classic that changes the visible time mid-song. Our Shift+F3 and Shift+F4 work at
   any time. **No change.**
   - `game-osu-mania.md` (proposal 5) suggests applying mania's F3/F4 gate when "keep speed" is on. IIDX has no such
     gate, so the reference does not require it. It is a fairness choice for Alpha, low risk either way because the
     ruleset is never submittable.
4. **The toast should show the green number.** IIDX shows the white and green numbers together while START is held
   [I1]. Our toast shows only "Wall: 40% covered".
   - **Proposal:** add the visible time, for example "Wall: 40% covered · 405 ms".
   - With "keep speed" on, show the shrinking time, for example "· 243 ms (speed kept)".
   - Use ms, not GN, since mania's speed tooltip is already in ms.
   - Show the highway's real visible time: mania's `TimeRange` after the hit-position and rate factors, times the wall
     factor when speed is kept. Mania's label `11485 / speed` is 2.3-19% short, depending on the skin
     (`game-osu-mania.md`, proposal 1).
5. **Step and repeat are much coarser than IIDX.**
   - IIDX is analog at 0.14% of the lane. beatoraja goes 0.1% per step at 2%/s, then 20%/s. Ours is 5% per press
     plus the OS key repeat, about 30 Hz (unverified on Alpha's machine): about **150%/s**, so the whole 0-90% passes in
     about 0.6 s and overshoot is likely.
   - **Proposal:** keep **5% per press**, which is close to Chunithm's 5.8% wall step and lands on round numbers. Drive
     held repeats from our own timer at **one step per 100 ms (50%/s)**, independent of the OS rate.
   - A 1% fine step is optional. It needs a free key combination.
6. **No cover toggle.** IIDX's double-tap START hides and restores the cover, keeping its size.
   - **Not proposed now.** It is cheap, but needs a free key, and the arcade games with a fixed wall (Chunithm) have no
     toggle.
7. **Lift (SPEC 4).**
   - IIDX draws a cover image under a lifted line [I13][I5]. Ours draws the receptor area and then the background.
     Keep that, since the receptors are what the player reads there. An optional dark "lift cover" below the key area
     can come with the highway skin.
   - IIDX's LIFT is its own option, independent of hi-speed and persistent [I1]. Ours is the preset's judgement-line
     height, so picking a preset resets it. Already noted for the M2 report; this research confirms IIDX keeps it
     separate.
   - Visible time with a lift is kept in beatoraja [B1]. Under IIDX FHS it is unverified, and under CHS it is not
     kept. Ours keeps it. **No change.**
8. **Speed display (SPEC 3).** Keep ms. For IIDX players, an optional tooltip hint "≈ GN 243" (GN = 0.6 × the real
   visible time in ms) costs nothing. Low priority.
9. **BPM (SPEC 3).** FHS fixes the green number at the start BPM [I4]; beatoraja fixes it at the main BPM by default
   [B1]. Lazer mania scales relative to the most common beat length (`RelativeScaleBeatLengths`), which is the
   beatoraja default. **No change.** SPEC 3 keeps mania's SV.
10. **Range.** IIDX SUDDEN+ goes to 999/1000 of the lane and LIFT to 830. Our wall stops at 90% of the depth and the
    lift at 40% of the screen. Both are enough for the uses in the sources. **No change.**

## Unverified

- FHS behaviour when LIFT alone is moved.
- The FHS step per key: 0.5 [I3] or 0.25 since HEROIC VERSE [I5].
- The HIDDEN+ range and how it combines with SUD+ on the turntable.
- The cover's movement per turntable tick, and whether held keys repeat (INFINITAS arrows).
- Why SUD+ cannot go below white number 42 [I3].
- The constant K was reverse-engineered by one player [I3]. The fhs calculator's 174000 [I10] agrees within 0.4%.

## Sources

- [I1] KONAMI, beatmania IIDX 32 Pinky Crush, ゲームオプションについて: https://p.eagate.573.jp/game/2dx/32/howto/play/option.html.
  SUDDEN+, HIDDEN+, SUD+&HID+, LIFT, LIFT&SUD+. START + turntable during play or before the song. White and green
  numbers while START is held. Double tap START hides and shows the cover. Default is Beginner Option.
- [I2] KONAMI, beatmania IIDX INFINITAS 操作方法: https://p.eagate.573.jp/game/infinitas/2/howto/play.html.
  Keyboard and controller bindings for hi-speed, FHS, cover and LIFT. "※SUDDEN+適用時は、ノーツの速度（緑数字）を固定したまま、SUDDEN+の表示エリアを変更することができます";
  "※ハイスピードの数値は自動で調整されます".
- [I3] まさお, IIDXのハイスピードの仕様を再現したギアチェンシミュレータを公開しました (note.com, 2025-08-21):
  https://note.com/masaoblue/n/n02ecb7426a76. Reverse-engineered formulas: GN unit, K, white-number conversion, ranges,
  NHS table, FHS recalculation rules. The author marks them as inferred. Simulator:
  https://masaoblue.github.io/iidx-gear-change-simulator/ (no licence stated; numbers only).
- [I4] iidx.org, Floating Hi-Speed: https://iidx.org/beginner/floating. WN out of 1000; GN 250-330; floating restores
  GN at the start BPM; re-floating; floating needs SUDDEN+ or LIFT.
- [I5] iidx.org, History of Notable Changes: https://iidx.org/compendium/feature_history. Version table above.
- [I6] RemyWiki, What is beatmania IIDX: https://remywiki.com/What_is_beatmania_IIDX. HIDDEN+ adjusted like SUDDEN+;
  FHS fine steps; "Since SPADA, the HI-SPEED setting can only be changed on the main gameplay screen".
- [I7] iidx.org, Hi-Speed Options: https://iidx.org/beginner/hispeed. NHS default since BISTROVER, 1-20,
  BPM-independent; START + black key faster, white key slower; CHS BPM-dependent.
- [I8] もやしのひとこと, 初心者のためのbeatmania IIDXハイスピ講座 (2021): https://rhychris.hatenablog.com/entry/2021/12/12/202238.
  GN definition; smaller is faster; GN range shown for BPM changes.
- [I9] beatoraja `IntegerPropertyFactory.java:305-310`: green number = ms × 3/5. Same unit as [I3].
- [I10] veliusiidx/fhs (2017, no licence; numbers only): https://github.com/veliusiidx/fhs, commit 300d586.
  README formula GN = 174000 / (BPM × HS × 1000 / (1000 − WN − lift)).
- [I11] iidx.org, Options Menu: https://iidx.org/beginner/option. SUDDEN+ with FHS used by almost everyone; HIDDEN+
  cosmetic and rare; LIFT for eye level; LIFT & SUD+ procedure.
- [I12] iidx.org, In-game Controls: https://iidx.org/beginner/ingame. Double tap START toggles SUDDEN+; LIFT cannot be
  toggled during play; with SUD+ and LIFT the turntable moves SUD+.
- [I13] iidx.org, Lane Covers: https://iidx.org/customize/lanecover. Covers used as LIFT backgrounds; covers dark up to
  given white numbers.
- `game-osu-mania.md`: mania's real visible time = (11485 / speed) × (768 − H) / 643.2, F3/F4 gate, proposals 1 and 5.
- [B1] beatoraja (exch-bms2), **GPL-3.0, read only, nothing copied**, commit 9340253a866a2f0091d44c6fa9de7637ce229ef9:
  https://github.com/exch-bms2/beatoraja/tree/9340253a866a2f0091d44c6fa9de7637ce229ef9. `src/bms/player/beatoraja/`:
  - `PlayConfig.java:24,43,54,62-99` (defaults);
  - `play/LaneRenderer.java:206-214,321-330` (hi-speed fix, cover, lift, duration);
  - `play/ControlInputProcessor.java:165,231-268` (double tap, cover input steps);
  - `skin/property/IntegerPropertyFactory.java:70,305-310` (white and green numbers).
