# Speed scales: each game's speed number as visible time

Last checked: 2026-11-07

## Summary

For a "Speed scale" setting, each game needs four things: the number its players set, that number's range and step,
the values players actually use, and a formula from the number to the time a note takes to reach the judgement line.
`topic-speed.md` covers osu!mania, Chunithm, IIDX, USC/KSM, Sekai (Sonolus), YARG, StepMania and Quaver. This note
fills the gaps with new sources:

- **SDVX.** Since EXCEED GEAR the setting is **lane speed 1.00-10.00**, which is the old BPM × hi-speed number
  divided by 100 (670 → 6.70) [SS1][SS2]. Players use 6-8, some 9+ [SS2]. The arcade's time per lane speed is still
  not measured. The clones give T = 4800 / L ms (USC) or 4489 / L ms (KSM).
- **Arcaea.** T = 3333 / s ms is **confirmed by an independent frame-stepped capture** of the official game: 200 / s
  frames at 60 Hz [SS13]. That matches ArcCreate's drop rate = speed × 30.
- **DJMAX RESPECT V.** **Speed no longer depends on BPM.** The 2020-08-06 patch (v568) replaced the BPM-multiplied
  speed with a fixed speed [SS16][SS17]. With a fan calculator's 1080p measurement, T ≈ 2693 / s ms [SS21][D15].
  `game-djmax.md`'s 323,143 / (speed × BPM) describes the PS4 game and V before that patch.
- **Chunithm.** 1.00-15.00 in **0.25** steps, then 16, 17, 18, 19 and SONIC. A 2024 player confirms the 0.25 step
  [SS23]. Typical speeds are 7.5-11.5.
- **BanG Dream! Girls Band Party.** Speed 1.0-12.0 in 0.1 steps, default 5.0 [SS26]. The MIT Sonolus recreation
  maps it to T = (12 − s) × 500 ms up to 11, then (16 − s) × 100 ms [SS25]. Typical speeds are 10-11.
- **Project Sekai.** A guide now sources the official range: 1.0-12.0, default 5.0 [SS29]. Steps have been 0.01
  since 2024-04 (`game-sekai.md`).

## Summary table

T is the time in ms from a note appearing (the far end of the lane) to its hit time at the line. Confidence:
**verified** means the formula or value is in code or a measurement of the real game; **derived** means our own
algebra on a clone's or a fan tool's constants; **unverified** means no source checks it.

| Game | Setting (as shown) | Range | Step | Default | Typical | Formula to ms | BPM | Confidence | Sources |
|---|---|---|---|---|---|---|---|---|---|
| osu!mania (lazer) | Scroll speed | 1.0-40.0 | 0.1 (F3/F4: 1) | 8.0 | Alpha 29 | 11485 / s (label); real T × (768 − H)/643.2 | relative to the most common beat length | verified | `game-osu-mania.md` |
| Chunithm (NEW to X-VERSE-X) | スピード (speed) | 1.00-15.00, then 16-19, SONIC | 0.25 to 15, then 1 | 6.0 | 7.5-8.5 [C9]; 8.00-11.50 [SS23] | 5486 / s (s ≤ 15); 16-19 = 313/274/244/219; SONIC 73 | none | formula verified (decode + screenshot check); step verified to 2024; typical verified | [C1][C21][SS22][SS23] |
| SDVX EG / ∇ | レーンスピード (lane speed) | 1.00-10.00 | 0.01 (finer mode); coarse step unverified | unverified | 6.00-8.50 [SS2][SS8][SS9] | 4800 / L (USC constant); 4489 / L (KSM) | relative to the chart's main BPM; BPM changes scale it | setting verified; formula **derived** from clones, arcade **unverified** (±7%) | [SS1][SS2][SS3][SS8][U1][K1] |
| SDVX III-VI | Hi-SPEED (multiplier), shown and saved as BPM × HS | multiplier up to 7.0 (III), 8.0 (IV); BPM × HS = 100 × L | multiplier 0.1 [SS10]; BPM × HS set directly with START twice | unverified | 550-900, mostly 650-850 [SS9][SS10] | 480,000 / (BPM × HS) (USC) | xmod: BPM × multiplier; saved value matched to the main BPM | as above | [SS5][SS6][SS9][SS10] |
| IIDX | green number (GN), shown while adjusting | n/a (a time; set through hi-speed and covers) | 1 | n/a | GN 250-330 (417-550 ms) | GN × 5/3 | FHS: start BPM; changes with BPM | verified | `game-iidx.md` |
| Project Sekai | ノーツの速さ (note speed) | 1.0-12.0 | 0.01 (since 2024-04; 0.1 before) | 5.0 | 10.0-11.6 | 350 + 3650 × ((12 − s)/11)^1.31 (exponential approach) | none | range verified [SS29]; formula derived (MIT engine), fits official captures within 1.5% | [SS29], `game-sekai.md` [P1][K3][K5] |
| BanG Dream! GBP | リズムアイコンの速さ (rhythm icon speed) | 1.0-12.0 | 0.1 (arrows 0.5) | 5.0 | 10.0-11.0 | (12 − s) × 500 for s ≤ 11; (16 − s) × 100 above (exponential approach) | unverified (none in the engine) | range verified (guide); formula **derived** (MIT engine), official **unverified** | [SS25][SS26][SS27] |
| Arcaea | 譜面速度 (note speed) | 1.0-6.5 | 0.1 (56 levels) | unverified | "4+" [A9] | 3333 / s | relative to the chart's base BPM (one 2017 guide says × BPM) | **verified** (frame capture + ArcCreate code) | [SS13][SS14][A1][A9] |
| DJMAX RESPECT V | SPEED | 1.0-9.0 | 0.1 | 2.0 | 4.0-7.0 [SS20] | 2693 / s (1080p default gear) | **none** since 2020-08-06 (fixed speed); BPM changes inside a chart still scale it | BPM rule verified (two namu pages); formula **derived** from a fan measurement | [SS16][SS17][SS18][SS20][SS21][D2][D15] |
| USC / KSM | hi-speed / mod-speed | KSM mod-speed 25-2000 [SS11] | n/a | USC mod-speed 300 | 300-400 new [SS12]; 600-1000 high [SS11] | 480,000 / (BPM × HS) (USC); 448,900 / (BPM × HS) (KSM) | mod-speed normalised | verified (code) | `game-usc.md` |
| Quaver | Scroll speed | 5.0-100.0 | 0.1 (F3/F4: 1) | 15.0 | n/a | 13528 / S (default 4K skin) | own charts none | verified | `game-quaver.md` |
| StepMania / Etterna | C-mod | any | n/a | n/a | n/a | 360,000 / C (default receptor position) | C: none | verified (code) | `game-stepmania-etterna.md` |
| YARG | note speed, highway length | units/s | n/a | 5 (new profiles) | n/a | 5000 × length / speed | none | verified (code) | `game-yarg-clonehero.md` |

## SDVX

### What the player sets

**EXCEED GEAR (2021) and ∇ (2024): lane speed.**

- "今作ではレーンスピードの設定（いわゆるハイスピ設定）が変更となり、従来の「BPM×n倍」形式から「BPMに左右されないレーンスピード値」形式に変更"
  (the hi-speed setting changed from "BPM × n" to a lane-speed value not tied to BPM) [SS1].
- "このレーンスピード値は、前作までのレーンスピード値/100になっています。例えば私は前作までは「BPM×倍速 = 670」に合わせていましたので、今作はLS6.70でプレイしています。"
  (the lane speed is the previous games' value / 100: BPM × multiplier = 670 became LS 6.70) [SS1].
- "This setting goes from 1-10 with two decimal places" [SS2]. RemyWiki: "displayed in hundredths (i.e. 1.03 speed)"
  [S1 in `game-sdvx.md`].
- Adjusted with START + a knob; "Double tapping and holding START will allow for finer control" [SS3]. The official
  Konasute EG page calls it "HI-SPEED設定" (START + analog) and "HI-SPEED保存" (START + numpad 6) [SS7].
- Numpad 6 locks it, so the game starts every song at that lane speed [SS2].
- **BPM changes scale the displayed number:** "楽曲のBPMが半分になったら、レーンスピードの数値も半分になります
  （例えばLS6.50の設定のまま楽曲のBPMが半減した場合、LS3.25という表示になります）" [SS1]. The playfield shows the
  incoming lane speed before a BPM change [SS3].
- Two modes: xmod keeps the lane speed as a multiplier of BPM (green number), mmod keeps a constant number (red
  number). Konasute EG uses mmod by default [SS4].

**III to VI (2014-2020): Hi-SPEED multiplier, with the BPM × HS value shown.**

- Turning a knob with START held changes a multiplier, and the screen also shows multiplier × BPM (since II)
  [SS5].
- III: "LANE SPEED, BPM * HI-SPEED". Pressing START twice sets the lane speed directly. START + numpad 6 saves it as a
  red number. On variable-BPM songs it is matched to the song's main BPM, not its start BPM [SS5].
- The maximum multiplier was 6.0 in II, 7.0 in III and 8.0 in IV [SS6].
- III's visible lane is shorter than II's, so the community advised setting the lane speed "about 40 lower" than in
  II (`전작 대비 약 40 정도 낮게`) [SS5]. That is about 6% at 700: **the time per lane speed changed between
  versions** (our inference from that advice).
- K-Shoot MANIA's guide confirms the same two arcade controls: the knob is "Type X0.5-9.9", which multiplies the BPM,
  and double-tapping START is like "Type 25-2000", which sets a base value [SS11].

### Typical values

| Source | Values (lane speed; BPM × HS / 100) |
|---|---|
| sdvx.org [SS2] | start around 2-4; "Some people read 9+ lane speed while others stay between 6 to 8" |
| voltexes, EG options guide [SS8] | beginners 3.00-4.00; advanced 7.00-8.20 |
| nvspace [SS9] (undated, BPM × HS units) | start at about 400, then 600+; "700未満でプレーしているプレーヤーは少なく、半数程度が800以上"; author 830, the ranker えつや 660, MINI about 850 |
| Yahoo! Chiebukuro, 2016 (III era) [SS10] | 550-600, 580 → 640-680, 650-690, 750-800, 790-900 |
| KSM guide [SS11] | start 400-500, step 25; "Most higher level players 600-1000" |
| sdvx.org simulators [SS12] | USC and KSM: 300-400 for new players |

So most experienced players sit at **6.50-8.50** (650-850).

### Visible time

- **No measurement of the arcade or Konasute exists in the sources found.** No official or fan page gives the time per
  lane speed, and no calculator converts it.
- **Clone constants** (code, `game-usc.md`):
  - USC: T = 480,000 / (BPM × HS) ms, so **T = 4800 / L ms** in lane speed. USC calls it "8 beats (2 measures) in view
    at 1x".
  - KSM v2: about 448,900 / (BPM × HS), so 4489 / L ms (7% shorter).
- The two clones differ by 7%, and the arcade changed its lane length between II and III by about 6% [SS5]. The real
  constant is therefore uncertain by roughly ±7%.

| Lane speed | 2.00 | 4.00 | 6.00 | 6.50 | 7.00 | 7.50 | 8.00 | 8.50 | 9.00 | 10.00 |
|---|---|---|---|---|---|---|---|---|---|---|
| T, USC constant (ms) | 2400 | 1200 | 800 | 738 | 686 | 640 | 600 | 565 | 533 | 480 |
| T, KSM constant (ms) | 2244 | 1122 | 748 | 691 | 641 | 599 | 561 | 528 | 499 | 449 |
| osu!mania label (11485 / T, USC) | 4.8 | 9.6 | 14.4 | 15.6 | 16.7 | 17.9 | 19.1 | 20.3 | 21.5 | 23.9 |

- Alpha's 405 ms is lane speed 11.85 by the USC constant, beyond the arcade's 10.00.

### BPM

- Lane speed is set against one BPM, the main BPM in III [SS5]. BPM changes inside the chart scale the speed, and the
  display follows them [SS1][SS3].
- That is osu!mania's own model (speed relative to the most common beat length), so the conversion is the same for
  every chart.

## Arcaea

### Range, step, typical

- "1.0~6.5まで0.1刻みの56段階" (1.0-6.5 in 56 steps of 0.1) [SS14].
- Default: not found (unverified).
- Typical: "most players use 4+" [A9]. A 2017 Chinese guide shows 3.6 (on a 180 BPM song) from when the maximum was
  5.0 [SS15].

### Visible time: verified

- **Measurement of the official game** [SS13] (TapTap, 2021-01-04, updated 2021-02-13):
  - an iPad Air 3 at 60 Hz, recording Ignotus at speeds in 0.5 steps;
  - frame-stepped in Kinovea, timing a beat line or arc from its appearance to the judgement line;
  - speed 5: frames 443 → 483, so **40 frames**;
  - the author fitted "y=200/x": "花费时间=200/音符流速/60（秒）", so speed 6.5 = 30.77 frames (0.51 s) and speed 1 =
    200 frames (3.33 s);
  - the 4.5-6.5 captures of the first attempt were inconsistent and were redone; the fit uses 1.5-4 plus the speed 5
    check.
- **ArcCreate** (GPL-3.0, read only) shows speed = drop rate / 30, and notes travel 100 units at drop rate per 1000 ms
  [A1]. So T = 100,000 / (30 s) = **3333 / s ms**.
- The two are independent and agree. **T = 3333 / s ms is verified** to about one frame (17 ms) at speed 5.

| Speed | 1.0 | 2.0 | 3.0 | 4.0 | 4.5 | 5.0 | 5.5 | 6.0 | 6.5 |
|---|---|---|---|---|---|---|---|---|---|
| T (ms) | 3333 | 1667 | 1111 | 833 | 741 | 667 | 606 | 556 | 513 |

- Alpha's 405 ms is Arcaea 8.2, beyond the game's 6.5.
- `game-arcaea.md` doubted this scale because it gives Arcaea players 8-13 H/s at the line, against 4-5 for Chunithm
  and Sekai players. The measurement settles it: Arcaea players do accept a faster near-line speed. The reason (very
  wide, thick near notes, or touch play) is our inference.

### BPM

- "基本的にどの譜面も同じ速度で流れてきますが、一部譜面では通常とは異なる速度が設定されている" (charts basically scroll
  at the same speed, but some are set differently) [SS14]. Timing changes inside a chart change the speed [A8].
- So speed is relative to each chart's base BPM, as in mania.
- **Conflict:** a 2017 GamerSky guide says "面板里的速度 * 曲目bpm=实际下落速度" (panel speed × song BPM = actual falling
  speed) [SS15]. It predates the 6.5 maximum and contradicts the wiki. The TapTap capture used one song only, so it
  cannot decide.

## DJMAX RESPECT V

### Speed no longer depends on BPM

- "2020년 8월 6일 패치를 기점으로 배속 시스템이 고정 배속 시스템으로 변경되었다" (from the 2020-08-06 patch, the speed system
  became a fixed-speed system) [SS16].
  - Its footnote: "GITADORA 시리즈에서 사용되던 배속 시스템으로, 곡의 BPM에 따라서 노트 내려오는 속도가 달랐던 기존 시스템과
    달리, 모든 BPM에서 배속에 따라 노트가 일정한 속도로 내려온다" (the system used in GITADORA: unlike the old system, where
    notes fell at different speeds depending on the song's BPM, notes now fall at a constant speed for a given setting
    at every BPM) [SS16].
- The patch list: v568 (2020-08-06), "Double speed system changed to fixed speed" [SS17].
- "PC판인 V에서는 BPM에 상관없이 모든 곡의 속도가 일정한 고정 배속 시스템을 사용한다" (V uses a fixed speed for every song,
  whatever its BPM) [SS18]. But "BPM이 변하는 음악, 즉 변속곡이 있으면 BPM에 따라 배속이 달라진다. 이는 고정배속을 쓰는
  V도 동일하다" (in songs whose BPM changes, the speed changes with the BPM, in V too) [SS18].
- The PS4 RESPECT used x0.5-x5.0 in 0.25 steps, multiplied by the song's BPM [SS18][D4]. **`game-djmax.md` [D4] is a
  PS4 page**, which is where its "speed multiplies BPM" came from.

### Range, step, default, typical

- 1.00-9.00 in 0.10 steps, default 2.00, one value for every song, changeable during play [D2][SS18].
- Start at 3.5-4 on PC (2.5 on PS4) [SS19]. "처음이라면 3.0~4.5 정도를 추천"; "주로 사람들은 배속을 4.0~7.0 두고 씀"
  (most people use 4.0-7.0) [SS20].

### Visible time: derived

- A fan calculator (MIT) [D15] stores lane height 754 and bar length 560 at speed 1.
- Its author's post explains them: the bar spacing was measured in pixels at 1080p with 120 BPM songs at speed 1, and
  the "same time" mode uses the time "from when a note becomes visible to the judgement line" [SS21]. So 754 px is the
  visible lane and 560 px one 4/4 bar (2 s at 120 BPM).
- The post is from 2023-04, after the fixed-speed patch, so the 120 BPM measurement holds at every BPM:
  T = 754 / 560 × 2000 / s = **2693 / s ms**.

| Speed | 1.0 | 2.0 (default) | 3.0 | 4.0 | 5.0 | 6.0 | 6.5 | 7.0 | 8.0 | 9.0 |
|---|---|---|---|---|---|---|---|---|---|---|
| T (ms) | 2693 | 1346 | 898 | 673 | 539 | 449 | 414 | 385 | 337 | 299 |
| osu!mania label (11485 / T) | 4.3 | 8.5 | 12.8 | 17.1 | 21.3 | 25.6 | 27.7 | 29.9 | 34.1 | 38.4 |

- These are the old table's 120 BPM row; the BPM dependence is gone. Alpha's 405 ms is DJMAX 6.65.
- Caveats:
  - one person's pixel measurement, not code;
  - the default gear only (other gears may have other lane heights);
  - the post does not say whether 754 px reaches the lane's top edge or the point notes fade in.

## Chunithm

- **Range and step.**
  - 1.00-15.00, then 16, 17, 18, 19 and SONIC. Steps are 0.50 up to PARADISE LOST and 0.25 from NEW (2021-11) [C1].
  - The executable decode of NEW and later agrees: table = display × 0.8 in 0.25 steps [C21].
  - A 2024 player (LUMINOUS PLUS era): raise the speed in 0.25 increments [SS23].
  - wikiwiki, last modified 2025-12-06: "1.0~20.0(SONIC)まで設定できます" [SS22].
  - The current version is X-VERSE-X (2025-12-11) [SS24]. No source reports a step change after NEW, but none from
    VERSE or later states the step either (unverified for those versions).
- **Default and presets.**
  - Presets "初心者向け(2.0)" "中級者向け(4.0)" "上級者向け(6.0)" [SS22][C1].
  - One 2024 answer calls the advanced preset 8.00 [SS23]. That contradicts wikiwiki; keep 6.0.
- **Speed is saved per cabinet type** since NEW [SS22].
- **Typical speeds:**
  - "7.5~8.5の人が多い" [C9];
  - 8.00 "a common scroll speed for MASTER charts" [C1];
  - "大体の人は、8.00〜11.50までの間に収まります" (most people fall within 8.00-11.50) [SS23];
  - 10.00-10.75 suggested for a player at rating 14.5 [SS23].
- **Wall.** Lower the speed by about 0.50 per wall step [SS23], as `game-chunithm.md` found.
- **T = 5486 / s ms** (s ≤ 15), unchanged (`game-chunithm.md`).

| Chunithm speed | 8.00 | 9.00 | 10.00 | 10.75 | 11.00 | 11.50 |
|---|---|---|---|---|---|---|
| T (ms) | 686 | 610 | 549 | 510 | 499 | 477 |

## BanG Dream! Girls Band Party

- **Range and step:**
  - "リズムアイコンのスピードは「1.0~12.0」の間なら0.1刻みで設定可能"; the "<" and ">" buttons move 0.5 [SS26] (guide
    last modified 2021-11-04).
  - At launch (2017) the maximum was 10 in 0.5 steps. An options trick unlocked 0.1 steps up to 11.0, and by 2019
    "現在は設定で速度を11まで上げることが可能" [SS27]. So the range grew 10 → 11 → 12.
- **Default:** 5.0 [SS26].
- **Typical speeds:**
  - beginners 8.0, experienced rhythm-game players 10.0, arcade players 9.0+;
  - the guide's author plays EXPERT at 11.0 or 10.9 [SS26];
  - one player's settings post: "ガルパ：速度10.6" [SS28] (search snippet only).
- **Visible time: derived from the Sonolus recreation** [SS25] (MIT, "A recreation of BanG Dream! Girls Band Party
  engine"):
  - `noteSpeed`: default 5, 1-15.9, step 0.1 (`shared/src/engine/configuration/options.ts:37-46`). The official game
    stops at 12.0 [SS26].
  - `duration` (`play/src/engine/playData/note.ts`):
    `noteSpeed <= 11 ? (12 - noteSpeed) / 2 : (16 - noteSpeed) / 10` seconds.
  - So **T = (12 − s) × 500 ms for s ≤ 11, (16 − s) × 100 ms above**. It is continuous at 11 (500 ms) and 5× steeper
    below 11.
  - The approach is exponential in screen scale, like Sekai's: `approach = lerp(0.05, 1, 1.1^(50 × (u − 1)))`
    (`shared/src/engine/data/note.ts`). T runs from spawn to the line.
  - Whether the official game uses these durations is unverified. The engine's author also wrote the Sekai engine,
    whose duration curve fits official captures within 1.5% (`game-sekai.md`).

| Bandori speed | 5.0 (default) | 8.0 | 9.0 | 10.0 | 10.5 | 10.6 | 10.9 | 11.0 | 11.5 | 12.0 |
|---|---|---|---|---|---|---|---|---|---|---|
| T (ms) | 3500 | 2000 | 1500 | 1000 | 750 | 700 | 550 | 500 | 450 | 400 |
| osu!mania label (11485 / T) | 3.3 | 5.7 | 7.7 | 11.5 | 15.3 | 16.4 | 20.9 | 23.0 | 25.5 | 28.7 |

- Alpha's 405 ms is Bandori 11.95.
- BPM: the engine's duration does not use BPM. Whether official charts change scroll speed is unverified.

## Project Sekai: official range

- AppMedia's guide (2021-06-03): "リズムアイコンのスピードは「1.0~12.0」の間なら0.1刻みで設定可能"; default
  "５.０"; beginners 8.0, experienced 10.0; the author plays 11.0-11.5 and drops to about 10.0 for MASTER [SS29].
- So the official range is 1.0-12.0, which matches the Sonolus engine. Steps became 0.01 on 2024-04-24
  (`game-sekai.md` [K2]).
- Formula and typical speeds: unchanged from `game-sekai.md`.

## Other games checked

- **D4DJ Groovy Mix.** By default notes speed up as they near the line, and a shorter display area makes their speed
  more nearly constant [SS30]. No range or formula was found; skipped.
- **Fortnite Festival** (a highway). "Track Speed", moved in 0.25 steps; players quote 1.0x-2.5x, most 1.25-2.0
  [SS31]. No visible time is documented; not proposed.
- **maimai** is not a highway; skipped as asked.

## What this means for us

Proposals for the Speed scale setting (SPEC 3: speed is visible time; the scale changes only the numbers shown).

**1. SDVX: show lane speed, keep the USC constant, call it approximate.**

- EG and ∇, the two current versions, show lane speed L = BPM × HS / 100. Show "SDVX 7.00" (two decimals) rather
  than "SDVX 700". The code's current 480,000 / units with units in BPM × HS is the same mapping (T = 4800 / L).
- Range: 1.00-10.00 is the game's own. Allow up to 15.00 so Alpha's 405 ms (11.85) can still be shown; the current
  code's 100-1500 (= 1.00-15.00) already does.
- Precision 0.01; key step 0.10. Players quote speeds in steps of 10 BPM × HS (650, 670, 690, 830); the arcade's
  coarse knob step is unverified. KSM's 25 (0.25) is the alternative.
- The hint should say "≈ SDVX (simulator scale)". The arcade constant is unverified, within about ±7%.

**2. Arcaea: keep 3333 / s.** It is now verified [SS13]. Range 1.0-6.5 in 0.1 steps. Alpha's 405 ms is 8.2; show it
beyond 6.5 rather than clamp, as for SDVX, or clamp the slider and let the hint say "faster than Arcaea's 6.5".

**3. Add DJMAX: "DJMAX 5.0", 1.0-9.0 in 0.1 steps, T = 2693 / s ms, no BPM.**

- It is the only flat 4-8 key keyboard game here whose players think in a BPM-independent multiplier, and Alpha's 405
  ms lands in range at 6.65.
- Confidence is derived (one pixel measurement). Since V has no BPM dependence, the mapping is a single constant like
  Arcaea's.

**4. Add BanG Dream: "Bandori 10.5", 1.0-12.0 in 0.1 steps, the piecewise formula above.**

- It is Sekai's sister game with the same kind of number, and Alpha's 405 ms is 11.95, in range.
- Confidence is derived (MIT engine, not checked against the official game).

**5. Chunithm, Sekai: no change.** 0.25 steps to 15, then 16-19; Sekai 1.0-12.0 in 0.01 steps with 0.1 key steps
(the 0.1 key step is our choice).

**6. Corrections for other notes** (not applied here; this note was asked to edit only itself and INDEX):

- `game-djmax.md` and `topic-speed.md`'s DJMAX row:
  - V's speed has not depended on BPM since 2020-08-06 [SS16][SS17][SS18];
  - T ≈ 2693 / s ms;
  - [D4] describes the PS4 game.
- `topic-speed.md` "Unverified" and `game-arcaea.md`: the Arcaea scale (3333 / s) is verified by [SS13].
- `game-sdvx.md`:
  - lane speed = old BPM × HS / 100 is now sourced [SS1];
  - the range is 1-10 [SS2];
  - III matched the saved speed to the main BPM [SS5];
  - II → III shortened the visible lane [SS5].
- `game-sekai.md`: the official range 1.0-12.0 now has a source [SS29].

## Unverified

- SDVX: the arcade's time per lane speed (no measurement found); EG's default lane speed and coarse step; whether EG
  normalises to the main BPM as III did.
- Arcaea: the default speed; whether speed ever depended on song BPM (2017 guide vs wiki).
- DJMAX: the visible lane length on other gears; a second measurement of the constant.
- Bandori: the official durations (only the recreation's); whether official charts change scroll speed.
- Chunithm: the step in VERSE, X-VERSE and X-VERSE-X (no change reported).
- D4DJ and Fortnite Festival: no formula.

## Sources

New in this note:

- [SS1] ぴこ師匠の音ゲーばなし #7, "【SDVX】EXCEED GEAR稼働おめでとう回 Pt.2" (2021-02-27):
  https://picoquality.hatenablog.com/entry/2021/02/27/110000. Lane speed replaces BPM × n; value = old / 100 (670 →
  6.70); display halves when the BPM halves.
- [SS2] sdvx.org, Options Menu: https://www.sdvx.org/startingout/options. Lane speed definition, 1-10 with two
  decimals, numpad 6 lock, start at 2-4, 6-8 or 9+.
- [SS3] sdvx.org, Gameplay Screens: https://www.sdvx.org/en/startingout/gameplay-screens. START + knob; double-tap for
  finer control; incoming BPM change shows the new lane speed.
- [SS4] mobiuslau, Sound Voltex Controls: https://mobiuslau.github.io/sdvx/controls/. xmod (green multiplier) and mmod
  (red number); Konasute EG uses mmod by default.
- [SS5] namu.moe (NamuWiki mirror), SDVX III: https://namu.moe/w/SDVX%20III. "LANE SPEED, BPM * HI-SPEED"; about 40
  lower than II because the visible lane is shorter; START twice for direct setting; START + 6 saves a red number;
  main BPM for variable-BPM songs; multiplier maximum 6.0 → 7.0. Also w.atwiki.jp/sdvx/pages/46.html: since II the
  multiplier × BPM is shown.
- [SS6] RemyWiki: https://remywiki.com/AC_SDVX_III (maximum 6.0 → 7.0; "the Hi-SPEED x BPM value" saved),
  https://remywiki.com/AC_SDVX_IV (7.0 → 8.0), https://remywiki.com/AC_SDVX_EG (hundredths; "does not pertain to the
  BPM"; red/blue values at BPM changes).
- [SS7] KONAMI, Konasute SOUND VOLTEX EXCEED GEAR controls: https://p.eagate.573.jp/game/eacsdvx/vi/howto/play.html.
  HI-SPEED設定 (START + analog), HI-SPEED保存 (START + numpad 6).
- [SS8] voltexes.com, EXCEED GEAR play options guide: https://voltexes.com/exceed-gear-play-options-guide/ (`game-sdvx.md`
  [S2]). Beginners 3.00-4.00, advanced 7.00-8.20.
- [SS9] nvspace, "ボルテのハイスピってどのくらいがいいの？【SDVX】" (undated): https://nvspace.net/sdvx-highspeed/.
  400 → 600+; few under 700, half 800+; 660 (えつや), 830 (author), about 850 (MINI).
- [SS10] Yahoo! Chiebukuro, 2016-04-05, "皆さんボルテのハイスピードどれくらいにしてますか？":
  https://detail.chiebukuro.yahoo.co.jp/qa/question_detail/q13157848883. Five answers, 550-900 BPM × HS.
- [SS11] grumpybutter and Ted_the_Red, "A Beginner's guide to K-Shoot Mania" (Google Doc):
  https://docs.google.com/document/d/1jPako8p8U1lA_EFvpJ8Ho4OvaR5o2LS_u-kyiFiUsnE. Arcade knob = BPM multiplier;
  double-tap START = base value; start 400-500 in steps of 25; high-level players 600-1000.
- [SS12] sdvx.org, Simulators: https://www.sdvx.org/setup/simulators. USC/KSM: 300-400 for new players.
- [SS13] TapTap Arcaea forum, "对“音符流速”的简单研究" (2021-01-04, updated 2021-02-13):
  https://www.taptap.cn/moment/95351829461733020. 60 Hz frame-stepped capture of Ignotus; 200 / speed frames; speed 5 =
  40 frames.
- [SS14] Arcaea Wiki* (wikiwiki.jp), ルール: https://wikiwiki.jp/arcaea/%E3%83%AB%E3%83%BC%E3%83%AB (`game-arcaea.md`
  [A6]). 1.0-6.5 in 0.1 steps; same speed across charts except some.
- [SS15] GamerSky, "arcaea调速教学 调速计算方法" (2017-03-29): https://wap.gamersky.com/sygl/Content-885525.html. Claims
  panel speed × BPM; maximum 5.0 at the time; 180 × 3.6 = 648.
- [SS16] namu.moe, DJMAX RESPECT V: https://namu.moe/w/DJMAX%20RESPECT%20V. Fixed speed from the 2020-08-06 patch;
  footnote 22 (GITADORA-style, constant at every BPM).
- [SS17] NamuWiki (English machine translation), DJMAX RESPECT V/Patch Notes:
  https://en.namu.wiki/w/DJMAX%20RESPECT%20V/%ED%8C%A8%EC%B9%98%EB%85%B8%ED%8A%B8. v568 (2020-08-06): "Double speed
  system changed to fixed speed".
- [SS18] namu.moe, DJMAX RESPECT/시스템: https://namu.moe/w/DJMAX%20RESPECT/%EC%8B%9C%EC%8A%A4%ED%85%9C. PS4 x0.5-x5.0
  in 0.25; V x1.0-x9.0 in 0.1; V fixed speed regardless of BPM; BPM changes inside a song still change it.
- [SS19] namu.moe, DJMAX RESPECT/게임 관련 정보:
  https://namu.moe/w/DJMAX%20RESPECT/%EA%B2%8C%EC%9E%84%20%EA%B4%80%EB%A0%A8%20%EC%A0%95%EB%B3%B4. Start at 2.5 (PS4) or
  3.5-4 (PC).
- [SS20] DC Inside DJMAX RESPECT V gallery, "대충 알아보는 배속" (2023-03-20):
  https://gall.dcinside.com/mgallery/board/view/?id=djmaxrespect&no=295306. Beginners 3.0-4.5; most players 4.0-7.0.
- [SS21] DC Inside DJMAX RESPECT V gallery, "DJMAX - EZ2ON 노트 속도 계산기" (2023-04-12):
  https://gall.dcinside.com/mgallery/board/view/?id=djmaxrespect&no=309589. Bar spacing measured in px at 1080p, 120
  BPM, speed 1; "same time" mode = time from visible to the line.
- [SS22] CHUNITHM攻略 Wiki* (wikiwiki.jp), これからCHUNITHMを始める人へ (last modified 2025-12-06):
  https://wikiwiki.jp/chunithmwiki/これからCHUNITHMを始める人へ. 1.0-20.0 (SONIC); presets 2.0/4.0/6.0; speed per cabinet
  type since NEW.
- [SS23] Yahoo! Chiebukuro, 2024-06-13, "チュウニズムのおすすめ設定教えてください":
  https://detail.chiebukuro.yahoo.co.jp/qa/question_detail/q10299535415. 8.00-11.50 for most people; 0.25 increments;
  10.00-10.75 at rating 14.5; −0.50 per wall step.
- [SS24] SEGA, CHUNITHM X-VERSE-X: https://www.sega.jp/arcade/detail/chunithm-x-verse-x/. Release 2025-12-11.
- [SS25] sonolus-bandori-engine (NonSpicyBurrito), **MIT**, main branch as read on 2026-11-07 (159 commits; hash not
  recorded): https://github.com/NonSpicyBurrito/sonolus-bandori-engine.
  - `shared/src/engine/configuration/options.ts:37-46` (`noteSpeed` default 5, 1-15.9, step 0.1);
  - `play/src/engine/playData/note.ts` and `watch/src/engine/watchData/note.ts` (`duration`);
  - `shared/src/engine/data/note.ts` (`approach`, `note.h`).
  - Read through GitHub code search fragments.
- [SS26] AppMedia, バンドリ！ガルパ, リズムアイコンのスピード (modified 2021-11-04): https://appmedia.jp/bang_dream/719605.
  1.0-12.0 in 0.1; "<"/">" 0.5; default 5.0; 8.0 / 10.0 / 9.0+; author 11.0 or 10.9.
- [SS27] AppMedia, ノーツ速度を11.0にする裏技 (2017-04-14, updated 2019-09-05): https://appmedia.jp/bang_dream/752748.
  Launch maximum 10 in 0.5 steps; trick for 0.1 steps to 11.0; 11 now official.
- [SS28] 湯田 雅 (@egg_yuda) on X: https://x.com/egg_yuda/status/2104075373873188918. "ガルパ：速度10.6" (search snippet
  only; date not read).
- [SS29] AppMedia, プロセカ ノーツの速さ (2021-06-03): https://appmedia.jp/?p=5344381. 1.0-12.0 in 0.1; default 5.0;
  8.0 / 10.0; author 11.0-11.5, about 10.0 for MASTER.
- [SS30] gamepedia.jp, D4DJ Groovy Mix (2020-02-21): https://gamepedia.jp/archives/41251. Notes accelerate toward the
  line by default; shorter display area makes speed more constant.
- [SS31] r/FortniteFestival, "What's the recommended speed to play at in expert" (2024-07-04, via a redlib mirror):
  https://lr.eu.psf.lt/r/FortniteFestival/comments/1dv3eut/whats_the_recommended_speed_to_play_at_in_expert. Track
  Speed; 0.25 steps; 1.0x-2.5x quoted.

From existing notes:

- [A1] ArcCreate (GPL-3.0, read only), [A8], [A9]: see `game-arcaea.md` and `topic-speed.md`.
- [C1] chunithm.org settings (mirror: https://chunithm-org.pages.dev/basic/settings/), [C9], [C21]: see
  `game-chunithm.md`.
- [D2] atwiki DJMAX/TAPSONIC攻略wiki, [D4] (PS4), [D15] ez2djmax-speed-calculator (MIT, `app/core/calculator.ts`:
  `DJMAX_BAR = 560`, `DJMAX_HEIGHT = 754`; `calculateNoteTime`): see `game-djmax.md`.
- [U1] USC, [K1] KSM v2 (both MIT): see `game-usc.md` (`Game.cpp:866-875`, `HighwayScroll.cpp:11-16,36-50,317-322`).
- [S1] RemyWiki SDVX EG and [S2] voltexes: see `game-sdvx.md`.
- [P1] [K2] [K3] [K5]: see `game-sekai.md`.
- Tables: computed with a scratch Python one-liner outside the repository.
