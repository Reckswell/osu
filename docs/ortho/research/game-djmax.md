# DJMAX RESPECT V: gear, speed and key modes

> **Correction (2026-11-07, `topic-speed-scales.md`):** RESPECT V's speed has not depended on the BPM since patch
> v568 (2020-08-06); T ≈ 2693 / speed ms. The BPM-scaled formula below and [D4] describe the PS4 RESPECT.

Last checked: 2026-11-04

## Summary

DJMAX RESPECT V (Neowiz / Rocky Studio; PC 2020, from the PS4 DJMAX RESPECT of 2017) is a flat 2D lane game. It
matters to us for three things, and has no camera to measure:

**1. Key modes.**

- DJMAX has 4B, 5B and 6B lanes. 5B's centre lane is played with two keys.
- Every mode adds two **side tracks** on Left and Right Shift. These are light-blue (or green) hold-only notes covering
  the left or right half of the gear.
- **8B is not eight lanes in a row.** It is 6B plus **L/R notes**, played with C and `,`. A guide calls them "thin pink
  bars" [D1]; a player guide says "6B+L/Rノーツ" (6B plus L/R notes), not one straight row [D9].
- So DJMAX handles extra inputs by giving them a different **look** (colour, thickness, half-gear width) on top of the
  normal lanes, not by adding equal columns.

**2. Speed.**

- 1.00 to 9.00 in 0.10 steps, default 2.00, one value for every song. It can be changed at any time during play, held
  for fast steps [D2].
- The setting **multiplies the song's BPM** [D4]. So the same number scrolls faster on a faster song, unlike mania,
  whose speed is BPM-independent.
- An MIT speed calculator models it as a 754-unit lane with a 560-unit bar at speed 1 [D15]. That gives **T = 323,143 /
  (speed × BPM) ms**: 2.00 at 120 BPM = 1346 ms; 5.00 = 539 ms; Alpha's 405 ms = 6.6 at 120 BPM or 5.3 at 150 BPM.

**3. Visibility options.**

- **No adjustable lane cover.** The visibility options are the FADER presets:
  - FADE IN: notes hidden at the top, fading in around the top third of the lane;
  - FADE OUT: notes fading out over the bottom third;
  - BLINK, FOG, PIXEL, BLIND.
- Gear transparency: 0-100% in 10% steps, default 30%.
- Key beam: Half or Quarter length.
- FAST/SLOW: shown only below MAX 90%, at the top, middle or bottom of the gear, in front of or behind the notes.

**What we already match, and what we could add:**

- We already match:
  - FADE IN in time (our "Far fade" goes up to 30% of visible time, DJMAX's FADE IN acts on about 33%);
  - "Track opacity";
  - mid-song visibility changes (our wall keys).
- Proposals:
  - a key-beam length option (Long / Half / Quarter);
  - for mania maps flagged `SpecialStyle` (N+1 keys), marking the special column as its own group (low priority).

No official screenshot or frame was measured. Lane colours per mode, long-note look and the judgement line's exact
height are unverified.

## Scope, evidence and versions

**Versions.**

- DJMAX RESPECT: PS4, Asia 2017-07-28, worldwide 2018-03-06.
- DJMAX RESPECT V: Windows, full release 2020-03-12; Xbox 2022-07-07 [D14].
- Developer Rocky Studio, a Neowiz subsidiary [D14].
- Settings below are for RESPECT V (PC) unless marked PS4.

**Evidence, strongest first:**

1. **A fan summary of the official X feature posts** [D6]. A GitHub gist quoting each option.
2. **A Japanese wiki's option and guide pages** [D2][D3] (atwiki djmaxinfo), and a PS4 strategy site [D4][D5].
3. **A Steam Community guide** [D1], already cited by `topic-covers.md` as its [D1].
4. **Player articles:**
   - note.com [D7][D8][D9];
   - s11ina.com [D10];
   - a blog [D11].
5. **Wikis:** TV Tropes [D12], Wikipedia [D13][D14], Giant Bomb [D16].
6. **A speed converter** between DJMAX and EZ2ON, MIT [D15]. Its constants are its author's measurements; how they
   were taken is not stated.

**Could not be read:**

- namu.wiki (the most detailed system page);
- GameFAQs, PSNProfiles, TrueAchievements and Steam discussions;
- X posts.

No video was frame-stepped and no image was downloaded.

## Camera and track

**The gear** [D1][D2][D4][D6]:

- One flat, vertical lane block ("gear"). There is no camera, tilt or perspective.
- **Position:** GEAR POSITION is Left, Center (default) or Right [D2][D6].
- **Transparency:** GEAR TRANSPARENCY is the lane background's transparency over the background video, 0-100% in 10%
  steps, **default 30%** [D2]:
  - 0% is fully opaque and hides the video in the lane area [D1][D2];
  - a PS4 strategy writer recommends a low value and uses 10% [D4].
- **Skins:** the gear and note skins are chosen separately from unlocked items [D4][D6]. "Skins change the design around
  the judgment line" [D7].

**Size on screen** (unverified):

- The speed calculator's constants are lane height 754 and bar length 560 at speed 1, with a commented-out width of 502
  [D15 `app/core/calculator.ts:5-7`].
- If those are pixels of a 1080p frame, the lane is 0.70H tall and the gear 0.26W wide. The units are not stated.

## Note approach and speed

**The setting** [D1][D2]:

- 1.00 to 9.00 in 0.10 steps, **default 2.00**.
- One value for all songs: "設定値はすべての曲に適用されます" (the value applies to every song) [D2].
- Not applied while a SPEED effector (CHAOS) is on [D2].

**Changing it during play** [D2][D4][D8]:

- Keys: [1] and [2] on the keyboard, LT and RT on a controller.
- It can be changed during play and while paused: "演奏中・演奏ポーズ中にも変更可能" [D4].
- Holding the key steps quickly [D2], with different sounds for up and down [D2].

**Speed multiplies the song's BPM:**

- "曲ごとにBPM（スピード）が設定されており、そこにこの倍率をかけたものが実際のスピードとなる" (each song has a BPM
  (speed) set, and that times this multiplier is the actual speed) [D4].
- The scroll BPM is per song and not always the music's BPM [D12]:
  - DJMAX Online's "ON" showed 72 BPM but scrolled as if 144 (RESPECT displays 144);
  - collaboration songs over 200 BPM were halved "so their scroll speeds stayed manageable".
- Whether the scroll follows BPM changes inside a chart: unverified.

**Visible time** (from the calculator's model [D15]):

- `calculateNoteTime = height / (bar × speed) × (120 / 60)`: the lane is 754/560 = 1.346 bars at speed 1, and a 4/4
  bar lasts 2 s at 120 BPM.
- In general, T = 1.346 × 4 × 60000 / (speed × BPM) = **323,143 / (speed × BPM) ms** (our generalisation).

| Speed | 1.0 | 2.0 (default) | 3.0 | 4.0 | 5.0 | 6.0 | 7.0 | 8.0 | 9.0 |
|---|---|---|---|---|---|---|---|---|---|
| T at 120 BPM (ms) | 2693 | 1346 | 898 | 673 | 539 | 449 | 385 | 337 | 299 |
| T at 150 BPM (ms) | 2154 | 1077 | 718 | 539 | 431 | 359 | 308 | 269 | 239 |
| T at 180 BPM (ms) | 1795 | 898 | 598 | 449 | 359 | 299 | 256 | 224 | 199 |
| Mania label with the same T, 120 BPM (11485 / T) | 4.3 | 8.5 | 12.8 | 17.1 | 21.3 | 25.6 | 29.9 | 34.1 | 38.4 |

- Alpha's 405 ms is speed 6.6 at 120 BPM, 5.3 at 150 and 4.4 at 180.
- Typical player speeds: no numbers found. One player runs "a quite high scroll speed for 4B and 6B" [D11].

**Approach.** Notes move at constant speed on a flat lane: visible time and on-screen distance are proportional.

**Effectors that change speed** (CHAOS) [D1][D6]:

- CHAOS W: speed swings mid-song.
- SLIDE UP / SLIDE DOWN: speed changes with combo and resets on BREAK.
- CHAOS X: random per lane.
- REVERSE: the gear upside down.

## Judgement line and receptors

- **Where the line is.** Notes are hit "when they overlap the judgment line", which is "at the bottom of the screen"
  [D7]. Its exact height is unverified.
- **Visual offset.** "Adjust Judgment Time" moves the judgement timing "without changing the note position" (ノートの
  位置を変更せずに判定位置の調整をします) [D6][D1]. "Adjust Note Output Time" shifts the notes against the music
  instead [D1][D10]. Both are timing offsets, not a line-height setting. No line-height option is listed [D6].
- **Receptors** are part of the gear skin [D7]. Their look is unverified.

## Lane covers / walls

**No adjustable lane cover.** None is listed in:

- the official feature summary [D6];
- the Steam guide ("The guide doesn't mention a lane cover option") [D1];
- the wiki options page [D2];
- the PS4 options page [D4].

This settles `topic-covers.md`'s "Unverified" bullet for DJMAX.

**FADER** (default OFF) [D1][D2][D6]:

| Fader | What it does |
|---|---|
| FADE IN | "Notes will be invisible at the top of the screen and will slowly fade into view as they scroll down" [D1]; it acts "around the top 33% of the lane" [D2] |
| FADE IN 2 | The same, stronger [D1] |
| FADE OUT | Notes fade out as they near the line, around the bottom 33% [D1][D2] |
| FADE OUT 2 | The same, stronger [D1] |
| BLINK | Notes blink at about half the song's BPM; BLINK 2 also blinks the gear [D2] |
| FOG | A large cloud of smoke over the gear near the line [D1][D6] |
| PIXEL, PIXEL 2 | The screen pixelates more with combo and resets on BREAK [D1][D6] |
| BLIND | No notes at all [D1][D2] |

- Each fader is a fixed preset: the option list gives no value to adjust [D2].
- They are chosen before play in the equipment tab [D1]. A mid-play change was not found (unverified).

**Background dim.** On PS4 the touchpad dims the background video during play [D4][D12].

## Notes and holds (shapes, rendering)

- **Taps.** Bars across the lane, in note skins chosen per player [D6]. Per-lane colours are unverified:
  - the only source names "blue notes" on the ↑ and △ pad buttons of a default layout [D5];
  - that fits blue inner lanes (inference);
  - the full colour pattern per mode was not found.
- **Long notes.** The head and the tail are both judged [D10]. "Press and hold when the long note reaches the judge
  line, and keep holding until its end passes the line" [D3].
  - Holding past the end gives MAX 1%, not a BREAK [D10][D12].
  - Score uses only the start and end, so length does not change the score [D10].
  - The release window is forgiving [D10].
  - Body look: unverified.
- **Side tracks** [D7][D10]:
  - "横半分を覆う水色のロングノーツ" (light-blue long notes covering half the width); one player calls them green [D8].
    Their colour is adjustable in PLAY HUD settings [D10].
  - Hold-only, with no tap version [D10].
  - A missed side track counts as MAX 1%, not a miss, so the combo continues [D10].
- **Note effect.** The hit effect is selectable or off, but not resizable [D8].

## Key count and lane layout

| Mode | Main lanes | Default keys (keyboard) [D1] | Extra inputs |
|---|---|---|---|
| 4B | 4 | A S ; ' | side tracks L/R Shift (each covers 2 lanes) [D7] |
| 5B | 5; the centre lane ("TRACK3") can be bound to two keys [D6], one per hand [D11] | — | side tracks |
| 6B | 6 | A S D L ; ' | side tracks (each covers 3 lanes) [D7] |
| 8B | 6 | as 6B | side tracks, plus **L/R notes** on C and `,` [D1] |

**Side tracks in every mode.**

- "○BUTTON" uses "○+2 keys" [D7][D10]; side tracks exist in "all of DJMAX's modes" [D11].
- They are not on every chart [D7].
- A warning appears on the lane before one starts, "SIDE TRACK ALARM", which can be set to BACK or turned off [D3][D6][D8][D10].
- They come from DJMAX Portable 3's REMIX tracks (3.2T / 4.2T / 6.2T, "two additional tracks"). RESPECT puts these
  "remix lanes ... within the main lanes" [D13].

**8B's L/R notes.**

- Sources describe them two ways:
  - "Eight lanes, including two FX lanes (thin pink bars)" [D1];
  - "本作における8Bは横並び一列ではなく、6B+L/Rノーツという少し変わった配置" (8B in this game is not one straight row
    but the slightly unusual layout of 6B plus L/R notes), played with the index fingers [D9].
- Colour: one player calls them "red left/right notes", used almost only in 8B [D8].
- A player finds that "having a button that is a cornerstone of a chart and is not in line with the other buttons throws
  off my judgement" [D11].
- Inference: they are drawn over the six lanes as thin coloured bars, not in two extra columns. Whether each spans one
  half of the gear like a side track is unverified.

**History.**

- In DJMAX Portable Black Square the mode was 6BFX: D-pad and face buttons plus the L and R shoulders. It was
  "formally known as "8B Mode" in previous DJMAX iterations" [D16][D12].
- 4BFX (4B plus L/R) existed in Clazziquai Edition and Black Square [D9][D16].

**Mode switching.** The 4, 5, 6 and 8 keys or Tab switch modes in song select [D1][D3]. Each mode keeps its own key
bindings [D6].

**Randomisation.** Side tracks and 8B's L/R notes are excluded from POSITION CHANGE (random and mirror) [D2].

**Gear width per mode** (whether 4B and 6B gears are equally wide): unverified.

## Feedback (judgement display, hit effects, lane lighting)

**Judgements:**

- MAX 100%, 90%, 80%, … 10%, then MAX 1%: "11 levels". Anything worse is BREAK, a miss [D7].
- Score follows the rate. Combo, and FEVER's combo multiplier (up to ×5), do not change the score in RESPECT V [D7].

**PLAY HUD settings** (F10 → GAME) [D6][D8][D10]:

- **JUDGEMENT:** judgement display on or off [D8].
- **Combo and judgement position:** adjustable [D10].
- **DETAILS / DISPLAY AVERAGE RATE:** the current rate in % [D6][D8].
- **FAST/SLOW:**
  - shown only for results at MAX 90% or worse: "90%以下の場合にFAST/SLOWを…表示" [D6];
  - placed at the top, middle or bottom of the gear (上段、中段、下段) [D6];
  - FRONT draws it over the notes, BACK under them [D8][D10].
- **KEYBEAM:** Half or Quarter length. Quarter is for players "used to very short IIDX beams" [D8].
- **NOTE EFFECT:** choose one or turn it off [D8].
- **FEVER GAUGE:** can be hidden [D10].

## What this means for us

All items are visual settings or data points: settings, not mods (invariants 1 and 3).

### R1. Where we already match (no change)

- **No cover, fade instead.**
  - DJMAX has no cover; its FADE IN hides the top third of a flat lane, which is about a third of the visible time.
  - Our "Far fade" (0-0.30 of track depth, linear in time like DJMAX's flat lane) covers the same share of time. Under
    perspective it covers less of the screen: 0.30 of depth is 9% of the track's on-screen height under the Chunithm
    preset and 4% under Voltex (computed from each preset's far/near ratio as a plain homography).
  - That is intended: the fade hides the far end, not the top third.
- **Track opacity.**
  - DJMAX's gear transparency (default 30% see-through) is the same one-slider idea as our "Track opacity" (0-100%,
    default 100%).
  - The background behind our track is osu!'s own background dim, so our default needs no change.
- **Changing visibility during play.**
  - DJMAX changes speed during play at any time. Mania's F3/F4 work only until 8 s after the first note and in breaks
    (DECISIONS 2026-11-04, "Correction to Gameplay hotkeys"); that is mania's rule in `osu.Game`, not ours.
  - Our wall keys (Shift+F3/F4) work at any time, which is DJMAX's freedom applied to the wall.
- **FAST/SLOW and judgement placement.**
  - DJMAX shows FAST/SLOW only for non-top results, with a position choice. Mania's HUD and SPEC 7 already decide our
    early/late display.
  - Nothing new for Q5: DJMAX's judgement is one centred display on a flat gear, which is SPEC 7's default.

### R2. Key beam length (SPEC 5), optional, default unchanged

- **Our beam today.** 30% alpha at the line, fading linearly in track depth to nothing at **50% of the depth**
  (`UI/HighwayTrack.cs:50-78`). Under perspective that is far more than half of the track's height on screen:
  - **81%** under the Chunithm preset;
  - 90% under Voltex.
- **DJMAX** offers Half and Quarter lengths. Players coming from IIDX prefer the short beam [D8].
- **Proposal:** a "Key beam" setting, **Long (today) / Half / Quarter**, measured in on-screen track height so it means
  the same in every preset. In track depth:

  | Preset | Half | Quarter |
  |---|---|---|
  | Chunithm (r 0.24) | d = 0.194 | d = 0.074 |
  | Voltex (r 0.106) | d = 0.096 | d = 0.034 |

  Formula: d = f / (1 + (1 − f) × (1/r − 1)) for an on-screen fraction f.
- **Default:** Long, so nothing changes until a playtest says otherwise. Low priority. Ties to the backlog's
  "Optional ... judgement line lit per lane" item, which touches the same drawing code.

### R3. Mark the special column of `SpecialStyle` maps (SPEC 5; extends `topic-wide-keys.md` W1), low priority

**The DJMAX pattern.** DJMAX gives inputs that are not ordinary lanes a different look (thin red/pink L/R bars,
half-gear side tracks) rather than an equal column. IIDX does the same with its red scratch lane (`topic-wide-keys.md`).

**What mania does today:**

- Mania marks only the centre column of odd key counts as special (`osu.Game.Rulesets.Mania/Beatmaps/StageDefinition.cs:32`).
- The beatmap's `SpecialStyle` flag (stable's N+1 layout: 8K as 7 + 1 scratch) is read and saved (`LegacyBeatmapDecoder.cs:285-286`;
  `BeatmapConverter.cs:77`) but not used for layout.
- The skin's `SpecialStyle` Left/Right is parsed (`LegacyManiaSkinDecoder.cs:139-141`; `LegacyManiaSkinConfiguration.cs:80-85`)
  but likewise unused.

**Proposal:** when a map has `SpecialStyle` set, W1's group dividers become **1 | N−1**, with the special column on the
skin's side (Left by default) instead of N/2 | N/2.

- Visual only: keys still go to mania's columns.
- It needs no core change: the flag is on the beatmap.
- Which column stable treats as special for each skin setting is unverified. Read stable's `SkinMania.cs` (linked from
  `LegacyManiaSkinConfiguration.cs:79`) before implementing.
- Worth doing only if Alpha plays N+1 maps; check their library first.

**Checked 2026-11-05** (stable's `SkinMania.cs` is not readable from here: `peppy/osu-stable-reference` is not public
to this session, so these are documentation and forum sources):

- The skin's `[Mania]` `SpecialStyle`: 0 none, 1 "left (SP) or outer (DP) lane", 2 "right (SP) or inner (DP) lane",
  for even key counts above 4; "for DP, the centre between the stages is used for determining positions"
  ([skinship.xyz ini reference](https://skinship.xyz/wiki/ini)).
- "When using SpecialStyle the furthest left or right lane will use the special key and note with the other keys
  ordered like in 5K"; the old `SpecialStyleLeft` line was replaced by `SpecialStyle` 0/1/2
  ([osu! forum, mania skinning](https://osu.ppy.sh/forum/p/6884683)).
- Stable "defaults to Normal Style for most keycounts, except for 8K, 10K and 18K where it defaults to Left Special
  Style" (2015, [osu! forum](https://osu.ppy.sh/community/forums/topics/309587)); the osu! wiki's key tables mark the
  special key S1 in 6K(L), 6K(R), 8K(L) and 8K(R) ([osu!mania](https://osu.ppy.sh/wiki/Osu!mania)).
- The beatmap's `SpecialStyle`: "Whether or not the "N+1" style key layout is used for osu!mania", 0 or 1, default 0
  ([.osu file format](https://osu.ppy.sh/wiki/Osu)). How stable picks the side for a flagged map when the skin names
  none is not documented; we use the left (stable's 8K and 10K default).
- Licences: documentation and forum posts, read for facts only; nothing copied.

### R4. Speed: a data point, no change (SPEC 3)

- **DJMAX's speed multiplies the song's BPM.** At one setting a 180 BPM song shows notes for 2/3 of the time a 120 BPM
  song does.
- **Mania and SPEC 3 keep visible time fixed** across songs (speed relative to each map's most common beat length).
  DJMAX players make up for this by changing speed during play.
- No change: SPEC 3's visible time is the better model for a speed that "feels the same in every camera mode".
- **Speed hint.** If the speed setting's hint ever names other games, DJMAX's equivalent needs the map's BPM:
  s = 323,143 / (T × BPM). That makes it a poor fit for a fixed label (inference).

### R5. Not applicable

- **Side tracks, 8B's L/R notes and FEVER:** mania maps have no such notes or mechanics.
- **Gear position Left/Right:** our track is centred per preset, and the skin's `ColumnStart` already moves it under
  the skin porter (SPEC 8).
- **Faders FADE OUT, BLINK, FOG, PIXEL and BLIND, and CHAOS:** these change what the player can see of the notes
  themselves, which mania does with mods (Hidden, Flashlight). They stay mods per SPEC 9, not settings.

### R6. Corrections for other notes (for the main session)

- **`topic-covers.md`, "Unverified".** DJMAX has no adjustable lane cover (official feature summary [D6], option pages
  [D2][D4], Steam guide [D1]). Arcaea has none either (`game-arcaea.md`).
- **`topic-speed.md`.** It has no DJMAX row. A row could say:
  - 1.00-9.00, 0.10 steps, default 2.00, one global value;
  - multiplies the song's (scroll) BPM;
  - T ≈ 323,143 / (speed × BPM) ms by an MIT calculator's constants [D15];
  - changeable during play.

### Where SPEC.md's assumptions about DJMAX hold or fail

1. **SPEC 4 / topic-covers, "DJMAX has lane covers".** Fails: DJMAX has fader presets, not a cover. INDEX's old "Why it
   matters" line said "lane covers".
2. **SPEC 3, "speed as visible time".** DJMAX does not define speed that way: its speed scales with BPM.
3. **SPEC 5, "one key per lane".** DJMAX breaks it in 5B (two keys, one lane) and with its overlay inputs. Mania does
   not, so nothing changes.

## Unverified / open questions

- **Gear geometry.** Judgement line height, gear width per mode, and whether the calculator's 754 / 560 / 502 are 1080p
  pixels.
- **Look.**
  - Per-lane note colours in 4B, 5B, 6B and 8B (only "blue notes" on two pad buttons is sourced [D5]);
  - the long-note body look;
  - where 8B's L/R notes sit and how wide they are.
- **Speed.**
  - Whether scroll speed follows BPM changes inside a chart;
  - typical player speeds;
  - how the calculator's constants were measured.
- **KEYBEAM.** Whether there is a full-length option besides Half and Quarter, and the default.
- **FADER.**
  - FADE IN's and FADE OUT's exact extents ("around 33%" [D2]);
  - whether faders can be changed mid-play.

## Sources

Official feature summaries and wikis:

- [D1] Steam Community guide, "DJMAX RESPECT V 100% Achievements, Walkthrough, and Unlock Guide":
  https://steamcommunity.com/sharedfiles/filedetails/?id=2362889860. Speed 1.0-9.0 in 0.1 steps; FADER list and
  descriptions; CHAOS; gear position and transparency (0% opaque, 100% see-through); judgement and note timing
  adjustments; no lane cover; 4B/6B/8B default keys, FX notes "thin pink bars" on C and `,`; side tracks on Shift.
- [D2] DJMAX/TAPSONIC攻略wiki (atwiki), ゲームプレイ - DJMAX RESPECT V: https://w.atwiki.jp/djmaxinfo/pages/176.html.
  - Speed 1.00-9.00 [0.10], default 2.00, all songs, not with SPEED effectors, changeable during play, hold for fast,
    [1]/[2] and LT/RT.
  - Side tracks on Shift / L, R.
  - Side tracks and 8-key L/R excluded from POSITION CHANGE.
  - GEAR POSITION (default CENTER); GEAR TRANSPARENCY 0-100% in 10% steps (default 30%; 0% hides the BGA in the lane).
  - FADER default OFF; FADE IN top ~33%, FADE OUT bottom ~33%, BLINK at ~half BPM, BLINK 2 also the gear, FOG, PIXEL,
    PIXEL 2, BLIND.
  - FEVER default ON.
- [D3] DJMAX/TAPSONIC攻略wiki (atwiki), GUIDE - DJMAX RESPECT V: https://w.atwiki.jp/djmaxinfo/pages/175.html. 4/5/6/8
  BUTTON TUNES; side notes preceded by a guide sign; long notes held until their end passes the line.
- [D4] DJMAX RESPECT 攻略の缶詰 (PS4), オプション設定: https://ds-can.com/djmax_respect/system/option.html. Speed = song
  BPM × multiplier, changeable during play and pause; gear position (3) and transparency; fader list; touchpad dims the
  BGA.
- [D5] DJMAX RESPECT 攻略の缶詰 (PS4), パッド: https://ds-can.com/djmax_respect/other/pad.html. 8B with L1 and ×;
  "青のノート" (blue notes) on ↑ and △.
- [D6] reindex-ot, "DJMAXの公式Xが復活したと同時にDJMAX Respect Vの機能の説明があったんでメモ" (a fan summary of the
  official X posts): https://gist.github.com/reindex-ot/0a0bc7c91f7c559617ce5fcc889a9594. GEAR TRANSPARENCY and
  POSITION; DISPLAY AVERAGE RATE; DISPLAY FAST/SLOW for ≤90% at 上段/中段/下段; FADE IN, BLIND, FOG, PIXEL; judgement
  adjustment without moving notes; per-mode key settings; 5B TRACK3 on two keys; side track warning; CHAOS SLIDE
  UP/DOWN. No lane cover listed.

Player articles:

- [D7] おーえん, note.com: https://note.com/unknown3620/n/nf470e9bcc996. "○BUTTON" = ○+2 keys; side tracks on Shift
  covering the left or right half (2 lanes in 4B, 3 in 6B), not on every chart; 4B keys A S ; :; MAX 100% … MAX 1% in
  11 levels, BREAK; judgement line at the bottom of the screen; skins change the design around the line.
- [D8] waseda_bbd, note.com: https://note.com/waseda_bbd/n/n24e730ebabba. Red L/R notes used only in 8B; green side
  tracks; SPEED UP/DOWN in play; FAST/SLOW GUIDE; DISPLAY BACK/FRONT/OFF; KEYBEAM Half/Quarter; NOTE EFFECT;
  DETAILS; JUDGEMENT; SIDETRACK ALARM; HUD POSITION.
- [D9] mugenxbullet, note.com: https://note.com/mugenxbullet/n/nf0cfc8b3f778. 8B = 6B + L/R notes, not one row, from the
  PSP era, index fingers; 4BFX in Clazziquai Edition and Black Square; side tracks since the original RESPECT.
- [D10] s11ina.com, DJMAX RESPECT Vの遊び方: https://s11ina.com/how-to-play-djmax-respect-v-3/. Side tracks: light-blue
  half-width hold notes, +2 buttons, warning (can be off), hold-only, miss = MAX 1%; long notes held past the end
  MAX 1%, start and end only; [1]/[2] speed; PLAY HUD: FEVER GAUGE off, FAST/SLOW FRONT/BACK and POSITION, combo and
  judgement positions, side track colour; judgement and note output timing.
- [D11] wish, "Returning to DJMAX RESPECT V over a break": https://www.unix.dog/~wish/posts/djmax_postseason_13/. 5B
  centre on two keys; 6B = 6K plus side track; 8B button "not in line with the other buttons"; side tracks in all
  modes.

Encyclopaedic:

- [D12] TV Tropes, DJMAX: https://tvtropes.org/pmwiki/pmwiki.php/VideoGame/DJMAX. Scroll BPM vs displayed BPM (ON 72 →
  144; >200 BPM halved); 8B renamed 6BFX in Black Square; RESPECT over-held notes MAX 1%; Pixel fader; background dim.
- [D13] Wikipedia, DJMax: https://en.wikipedia.org/wiki/DJMax. 2-8 buttons, 4B easiest, 8B hardest; Portable 3 REMIX
  modes 3.2T/4.2T/6.2T; RESPECT's remix lanes within the main lanes.
- [D14] Wikipedia, DJMax Respect: https://en.wikipedia.org/wiki/DJMax_Respect. Developer Rocky Studio (Neowiz); release
  dates; scoring like Technika 2/3; base score 1,000,000 in RESPECT V.
- [D16] Giant Bomb, DJMAX Portable Black Square: https://giantbomb.com/wiki/Games/DJMAX_Portable_Black_Square. 4BFX and
  6BFX button use; "formally known as "8B Mode"".

Code:

- [D15] johypark97, ez2djmax-speed-calculator, **MIT**, commit 6544d2eb52977ec3d9131ca05850534200ce4098 (2026-04-07):
  https://github.com/johypark97/ez2djmax-speed-calculator. `app/core/calculator.ts:1-45` (`DJMAX_BAR = 560`,
  `DJMAX_HEIGHT = 754`, commented `DJMAX_WIDTH = 502`; `calculateNoteTime`).

Ours and the existing notes:

- `topic-covers.md`, `topic-speed.md`, `topic-wide-keys.md` (W1), `game-iidx.md`, `game-arcaea.md`.
- `osu.Game.Rulesets.ManiaOrtho/UI/HighwayTrack.cs:50-78` (beam alpha, length, fades);
  `Configuration/ManiaOrthoRulesetConfigManager.cs:133-135` (Far fade, Hold and Track opacity ranges).
- `osu.Game.Rulesets.Mania/Beatmaps/StageDefinition.cs:32`; `osu.Game/Beatmaps/Formats/LegacyBeatmapDecoder.cs:285-286`;
  `osu.Game/Beatmaps/BeatmapConverter.cs:77`; `osu.Game/Skinning/LegacyManiaSkinDecoder.cs:139-141`;
  `osu.Game/Skinning/LegacyManiaSkinConfiguration.cs:79-85`.
