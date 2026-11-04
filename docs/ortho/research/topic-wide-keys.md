# Wide key counts: keeping 7K-10K readable on a tilted track

Last checked: 2026-11-04

## Summary

Alpha's choice (one track width for every key count, DECISIONS "One track width for every key count (Alpha)", SPEC 5)
has two precedents among the references:

- **Chunithm** always draws 16 cells on the same 0.61W field.
- **YARG** draws 5-lane guitar and 10-white-key pro keys on the same 2-unit track, with one camera per player profile.

Only StepMania/Etterna widens the field with the key count. The games that put many lanes on one width keep them
readable with four devices:

1. **A second, coarser tier of markers.** Chunithm's default guides split its 16 cells into 4 groups. YARG tints
   half-octave groups and outlines their edges. StepMania 5's kb7 leaves a gap around the centre column. IIDX has the
   scratch lane. Pop'n has a red centre button.
2. **Adjacent lanes never look alike**, by colour, width or both, and the pattern is mirror-symmetric: IIDX white and
   blue, pop'n's 9 colours, argon's palettes.
3. **Notes nearly fill their lane**: 98% in Chunithm, 100% in beatoraja's skin and in Alpha's own skin.
4. **The smallest, farthest part of the track can be hidden.** Chunithm players lower the wall to make notes bigger,
   and YARG fades the far end.

Pixel budget at 1920×1080:

- **Chunithm preset, 10K.** Lanes are 117 px at the line and 28 px at the far edge. That is wider at every depth than
  Chunithm's own 1-cell lanes (73 / 17.6 px).
- **Voltex preset, 7K.** Far lanes are 13 px, about SDVX's own BT lanes at the far edge (14.8 px).
- **Voltex preset, 10K.** Far lanes are 9 px, below every reference.

Proposals:

- dividers in two tiers, grouped by mania's own default hand split (3|3, 3|1|3, 4|4, 4|1|4, 5|5);
- a 1 px screen-space floor on the gap between notes;
- an optional faint lane tint;
- a playtest of Voltex at 10K with the wall.

The camera does not change by key count.

## Scope and evidence

This is a cross-game topic note. It reuses the measurements of the per-game notes (`game-chunithm.md`,
`game-sdvx.md`, `game-yarg-clonehero.md`, `game-stepmania-etterna.md`, `baseline-alpha.md`). It adds:

- beatoraja's default skins (GPL-3.0: numbers read, nothing copied);
- RemyWiki on IIDX;
- this repo's code (MIT): the argon skin, mania's default bindings and our highway.

All pixel figures are computed for a 1920×1080 screen from each note's geometry. The formula is the plane homography
our `HighwayProjection` uses: a lane w₀ px wide at the line is w₀ / (1 + (1/r − 1)·d) px wide at track depth d, where
r is the far/near width ratio.

## Our current state

### Presets

The table gives the width of one equal lane, in px at 1080p, at track depth d = 0 (the line), 0.25, 0.5 and 1 (the far
edge). Preset values come from `osu.Game.Rulesets.ManiaOrtho/Highway/HighwaySettings.cs:208-247`.

| Preset | Track at line | Far/near | Outer-edge lean (computed) | 4K | 7K | 10K |
|---|---|---|---|---|---|---|
| Chunithm | 0.61W | 0.24 | 24.0° | 293 / 163 / 113 / 70 | 167 / 93 / 65 / 40 | 117 / 65 / 45 / 28 |
| Voltex | 0.435W | 0.106 | 20.2° | 209 / 67 / 40 / 22 | 119 / 38 / 23 / 13 | 84 / 27 / 16 / 9 |
| Parallel | 0.45W | 1 | 0° | 216 everywhere | 123 everywhere | 86 everywhere |
| Classic | mania's own stage | — | — | skin | skin | skin |

- **Lean.** The angle is between the track's outer edge and vertical. It is computed from the judgement line height
  and the far edge at the top of the screen, and it is the same at every key count. The arcade's own Chunithm lean is
  28.4°, because its far edge sits at 0.163H (`game-chunithm.md`).
- **Lanes follow the skin's columns.** The guides and lane spans are read from each column's
  `HitObjectContainer.ScreenSpaceDrawQuad` (`UI/HighwayPlayfieldAdjustmentContainer.cs:733-752`).
  - Alpha's legacy skin has equal columns: 60 each at 4K, 45 at 7K (`baseline-alpha.md`).
  - Argon (lazer's default skin) makes the special column twice as wide (`ManiaArgonSkinTransformer.cs:167-172`). The
    special column is the middle one of an odd key count (`StageDefinition.cs:32`).
  - Argon at 7K is 60-60-60-120-60-60-60. Under the Chunithm preset that gives about 146 px lanes and a 293 px centre
    lane at the line (ignoring argon's 1-unit column spacing, `ManiaArgonSkinTransformer.cs:159-161`).
  - Argon at 9K gives about 117 px lanes and a 234 px centre lane.
  - Those argon 7K proportions are exactly osu_2_mgxc's 7K → Chunithm cell grouping, 2-2-2-4-2-2-2
    (`game-chunithm.md`, Key count).
- **Guides** (`UI/HighwayGuides.cs:30-53, 188-227`):
  - one tier of lines at every lane boundary;
  - 0.0022 track widths wide, at least 1 px: 2.6 px at the line and 0.6 px (drawn at 1 px) at the far edge under
    Chunithm, 1.8 px and 0.2 px (drawn at 1 px) under Voltex;
  - teal #45D9C8, alpha 0.8 near to 0.3 far;
  - edge lines #1F6763.
- **Native notes** (`UI/HighwayNoteLayer.cs:49, 67-71, 147-150, 175-185`; the Notes setting defaults to Highway,
  `Configuration/ManiaOrthoRulesetConfigManager.cs:96`):
  - inset 1% of the lane per side, so notes are 98% of the lane;
  - colour is the skin's column colour when it is a note colour (argon);
  - otherwise white/pink by `fromEdge % 2`, with amber for the special column.
  - Notes are drawn after the guides (`HighwayPlayfieldAdjustmentContainer.cs:238-239`), so they cover them.
- **Adjacent lanes with the same colour.** With the built-in colours the two centre lanes of every even key count
  share a colour:
  - 4K white, pink, pink, white;
  - 6K white, pink, white, white, pink, white;
  - 8K centre pair pink;
  - 10K centre pair white.

  Argon never gives two adjacent lanes the same colour at 4-10K (`ManiaArgonSkinTransformer.cs:183-375`).
  - At the far edge under Chunithm at 10K, the 1% inset is 0.28 px per side, so a chord on the centre pair draws as
    one 56 px white bar (computed).

## Camera and track

| Game | What changes with the lane count | Source |
|---|---|---|
| Chunithm | Nothing. Always 16 cells on 0.611W, far/near 0.24. | `game-chunithm.md` (Key count) |
| YARG | Nothing. One 2-unit track. Lane i of n is centred at 2(i + 1)/n − 1 − 1/n, and lane decorations scale to 2/n. The camera preset is a single field of the player profile, applied by every track player. | `game-yarg-clonehero.md`; [Y1] `TrackElement.cs:66`, `LaneElement.cs:31-46`, `TrackPlayer.cs:544`; [Y2] `YargProfile.cs:71` |
| StepMania 5 / Etterna | The field widens: 64 px per column, 4K 0.30W, 7K 0.525W (Etterna) or 0.56W (SM5), shrunk only to fit the screen. Under Hallway the outer-edge lean grows 3.7° (4K), 6.4° (7K), 6.8° (10K pump-double). | `game-stepmania-etterna.md` |
| IIDX (flat) | 8 columns (7 keys and scratch). beatoraja's default 7K skin is 390/1280 = 0.305W wide. | RemyWiki [R1]; [B1] `play7main.lua:150-157` |
| Pop'n (flat, beatoraja skin) | 9 lanes, 590/1280 = 0.461W. | [B1] `play9.json:347-355` |
| SDVX | Fixed: 4 BT lanes in the central 0.290W, 2 FX lanes over BT pairs, laser margins. | `game-sdvx.md` |

Observation: both perspective references with many lanes (Chunithm, YARG) keep one width and one camera. The
reference that varies width (StepMania) is the one with the mildest perspective (far/near 0.80 under Hallway).

### Narrowest lanes players read

Widths in px at 1080p, at the line, at mid-depth (d = 0.5) and at the far edge. Under perspective a note spends half
its visible time beyond d = 0.5, since d is linear in time (SPEC 3).

| Lane | Line | d = 0.5 | Far edge | How obtained |
|---|---|---|---|---|
| Chunithm 1-cell lane | 73 | 28 | 17.6 | 0.611W / 16, far/near 0.24 (`game-chunithm.md`) |
| SDVX BT lane (USC landscape camera) | 139 | 27 | 14.8 | 0.290W / 4, far/near 0.106 (`game-sdvx.md`) |
| YARG pro keys white-key lane | 56 | 35 | 25 | Default camera (`game-yarg-clonehero.md`) |
| StepMania Hallway lane | 138 | 123 | 111 | 0.288W / 4, far/near 0.80 (`game-stepmania-etterna.md`) |
| beatoraja 7K, white / black / scratch (flat) | 75 / 60 / 105 | same | same | 50 / 40 / 70 at 720p [B1] |
| beatoraja pop'n 9, wide / narrow (flat) | 105 / 90 | same | same | 70 / 60 at 720p [B1] |
| Alpha's flat skin, 4K / 7K | 135 / 101 | same | same | 60 / 45 of 853 (`baseline-alpha.md`) |
| **Ours, Chunithm 7K / 10K** | 167 / 117 | 65 / 45 | 40 / 28 | table above |
| **Ours, Voltex 7K / 10K** | 119 / 84 | 23 / 16 | 13 / 9 | table above |
| **Ours, Parallel 10K** | 86 | 86 | 86 | table above |

Inferences:

- **The Chunithm preset is safe up to 10K.** At every depth a 10K lane is wider than a Chunithm 1-cell note, which
  arcade players read routinely.
- **Voltex 7K matches SDVX's own BT lanes** at the far edge (13 vs 14.8 px) and is a little narrower at mid-depth
  (23 vs 27 px).
- **Voltex 10K is the one combination below every reference**: 9 px at the far edge and 16 px at mid-depth.
- How narrow a note can get and still read is not measured anywhere we found (unverified). The table is only what
  players are known to cope with.

## Note approach and speed

- **No reference changes speed with the lane count.**
  - StepMania: 64 px of field per beat for every style (`game-stepmania-etterna.md`).
  - YARG: note speed is a profile setting (`game-yarg-clonehero.md`; [Y2] `YargProfile.cs:34-35`).
  - Chunithm: speed is per player (`game-chunithm.md`).
- SPEC 3 (speed as visible time) already makes this true for us.

## Judgement line and receptors

- **Chunithm and SDVX draw no receptors.** The physical slider and buttons are the receptors (`game-chunithm.md`,
  `game-sdvx.md`).
- **StepMania** keeps per-column receptors on the field plane (`game-stepmania-etterna.md`).
- **Alpha's skin** has transparent receptors at every key count (`baseline-alpha.md`).
- Nothing about receptors changes with the key count in any reference, so this note proposes nothing here.

## Lane covers / walls

- **Chunithm players use the wall to make notes bigger.** They lower the wall and the speed together so notes "look
  bigger", and keep visible time about constant by hand (`game-chunithm.md`, [C13][C16] there).
  - That removes exactly the far part, where lanes are narrowest.
- **YARG fades the far 25%** of its Default track (`game-yarg-clonehero.md`).
- **Lane width at the wall's edge** (computed, px at 1080p). This is where notes now enter.

  | Hidden by the wall | 0 | 0.25 | 0.35 | 0.5 |
  |---|---|---|---|---|
  | Chunithm 7K | 40 | 50 | 55 | 65 |
  | Chunithm 10K | 28 | 35 | 38 | 45 |
  | Voltex 7K | 13 | 16 | 18 | 23 |
  | Voltex 10K | 9 | 11 | 13 | 16 |

  - Voltex 7K needs a 16% wall to reach SDVX's 14.8 px at entry. Voltex 10K needs 45%.
  - Because SPEC 4 keeps visible time constant by default, a wall here costs no reading time.

## Notes and holds (shapes, rendering)

| Source | Note width as a share of the lane |
|---|---|
| Chunithm | 98-99% of the cell span; narrower notes draw on top (`game-chunithm.md`, decoded meshes) |
| beatoraja default 7K and 24K skins | 100%: note width equals lane width, except 24K "hybrid" black notes (30 px on 8 px black lanes, overlapping the white lanes) ([B1] `play7main.lua:150-157`; `play24main.lua:141-185`) |
| Alpha's skin | 100%: flat bars filling the column, no outline (`baseline-alpha.md`) |
| Ours | 98% (1% inset per side) |

- So the gap between two notes in adjacent lanes is mostly the lane line, or nothing at all. Colour does the
  separating.
- That stops working where two adjacent lanes share a colour and the inset is sub-pixel (see Our current state).

## Key count and lane layout: how many lanes stay readable

### Chunithm: 16 cells, guides in groups

- The guide options are OFF, 2-, 4- (default), 8- and 16-split (`game-chunithm.md`).
  - **The default marks groups of 4 cells, not every cell.**
  - The lines are teal #45D9C8, 2-3 px near the line and 1 px at mid-screen, and they fade toward the far end.
  - The edge line is darker (#1F6763).
- Mania converters map 7K to 2-2-2-4-2-2-2 cells, which is a wide centre lane (osu_2_mgxc), or to 2 cells each.
  Only 1, 2, 4, 8 and 16 lanes tile 16 cells evenly (`game-chunithm.md`).

### YARG pro keys: 10 white keys on the guitar track

All from `game-yarg-clonehero.md` and [Y1] `ProKeys/ProKeysTrackOverlay.cs:35-138`, `ProKeysPlayer.cs:56-57`.

- 10 white-key lanes at 0.2 units on the same 2-unit track. Black-key notes sit between them, on the white-lane
  boundaries.
- **Group tint.** Each key's lane overlay is tinted by its half-octave group (C-E, F-B), cycling red, yellow, blue,
  green and orange, at **5% alpha**.
- **Group edges.** The outer keys of each group (C and F, E and B) carry an edge-gradient texture, which outlines
  every group of 3 or 4 white keys.
- **Notes.** White notes for white keys, black notes for black keys, both with a white rim.
- **Press highlight.** A held key shows a 30%-alpha highlight in its group's colour, rising along the track.
- **Range shifts.** The track slides sideways when the chart shifts range. Mania lanes are fixed, so this does not
  apply to us.

### StepMania 5 and Etterna keyboard modes

All from `game-stepmania-etterna.md`.

- **kb7 in StepMania 5** places columns at ±3.25, ±2.25, ±1.25 and 0 spacings. That is a quarter-column gap on each
  side of the centre (space-bar) column, so the field reads **3 | 1 | 3**.
  - **Etterna made kb7 uniform** (±3, ±2, ±1, 0).
  - The "beat" style puts the scratch half a column away from the keys.
- **Etterna's key modes:**
  - 3K-10K, 12K and 16K;
  - note field width, height and spacing are **saved per key mode** (defaults 1.0, 1.0, 0.0);
  - an optional combo-break column flash at 40% alpha over 0.25 s.

### IIDX (flat, for reference)

- **Keys.** The controller has seven keys, four white and three black, plus a turntable. On screen, "Keys 1, 3, 5,
  and 7 are white, 2, 4, and 6 are blue, and the scratch lane is red" [R1].
- **beatoraja's default 7K skin** (an IIDX-style BMS player):
  - lanes 50 px (white key), 40 px (black key) and 70 px (scratch) at 1280×720 [B1 `play7main.lua:129, 150-157`];
  - so black-key lanes are 20% narrower than white-key lanes, in addition to their colour.
  - Whether the arcade uses the same proportions is unverified.
- **beatoraja's 24-key (keyboard) skin** offers three layouts [B1 `play24main.lua:141-185`]:
  - "half": white lanes only, black notes drawn over them;
  - "hybrid": white 32 px, black 8 px lanes with 30 px black notes;
  - "separate": white 40 px, black 32 px, 950/1280 = 74% of the screen.

### Pop'n music (flat, beatoraja skin)

- The 9 lanes alternate 70 and 60 px [B1 `play9.json:347-355`].
- The note colours are white, yellow, green, blue, red, blue, green, yellow, white [B1 `play9.json:332`]: mirror
  symmetric, with a red centre.
- That they match the arcade buttons is inferred from the skin only (unverified).

### osu!mania argon (this repo)

- Argon uses one palette per key count, mirror-symmetric, with no two adjacent lanes alike
  (`ManiaArgonSkinTransformer.cs:183-375`). For example:
  - 7K: pink, orange, pink, purple (special, double width), pink, orange, pink;
  - 10K: purple, pink, orange, yellow, green, cyan, yellow, orange, pink, purple.

### Mania's own hand split

- Default bindings put n/2 keys under the left hand and n/2 under the right, with Space for the centre column of an
  odd count (`osu.Game.Rulesets.Mania/VariantMappingGenerator.cs:38-63`; keys in
  `SingleStageVariantGenerator.cs:20-37`).

  | Keys | Default layout | Split |
  |---|---|---|
  | 6K | S D F / J K L | 3 \| 3 |
  | 7K | S D F / Space / J K L | 3 \| 1 \| 3 |
  | 8K | A S D F / J K L ; | 4 \| 4 |
  | 9K | A S D F / Space / J K L ; | 4 \| 1 \| 4 |
  | 10K | A S D F V / N J K L ; | 5 \| 5 |

### The common pattern (inference)

Across these games, lanes that the eye cannot count at a glance are grouped:

- in 4s (Chunithm's default guides);
- in 3s and 4s (YARG's half-octaves);
- around a centre (kb7's gaps, pop'n's red centre, argon's double centre).

The grouping marks are drawn more strongly than the lane boundaries, or in a different way.

## Feedback (judgement display, hit effects, lane lighting)

- **YARG:** a held pro key lights its lane at 30% in its group colour.
- **Etterna (Til Death):** a combo break can flash the column at 40%.
- Neither scales with the lane count.
- Our hit rings grow to 1.3 lane widths, capped at 30% of the track (DECISIONS "Hit lighting is drawn by the
  highway"). At 10K that is 13% of the track, under the cap, so a burst stays within about one lane of its own.
- Nothing to add.

## What this means for us

One track width per preset stays (Alpha, SPEC 5). These proposals add markers and contrast instead.

- **W1. Group dividers: a second guide tier** (SPEC 5; the "Lane guides" setting, DECISIONS "Lane guides (on)").
  - **Where.** Boundaries that separate mania's default hand groups:

    | Keys | 6K | 7K | 8K | 9K | 10K |
    |---|---|---|---|---|---|
    | Groups | 3 \| 3 | 3 \| 1 \| 3 | 4 \| 4 | 4 \| 1 \| 4 | 5 \| 5 |

    - 1K-5K keep a single tier. At 4K this is Chunithm's default 4-split exactly (`game-chunithm.md` P4).
    - Dual stages group each stage on its own.
  - **Style.**
    - same teal #45D9C8;
    - **0.0044 track widths** (twice a lane line), **at least 2 px**, so they still differ from the 1 px lane lines
      at the far end;
    - alpha **1.0 near to 0.45 far** (lane lines 0.8 to 0.3).
  - **Rules.**
    - Drawn only when Lane guides is on, and never under the flat camera.
    - The grouping comes from the key count, not the user's bindings: bindings are per variant and may not follow
      the default (inference).
    - Different from `game-chunithm.md` P4's optional "16-cell grid": that one follows cells, this one follows hands.
  - **Defaults** *(tune)*: on. Playtest 7K and 10K: the playtester checks that the centre column of 7K and the hand
    split of 10K can be found at a glance, at the line and at mid-depth.
- **W2. Optional lane tint** (SPEC 5, 6).
  - Each lane's floor is tinted with its note colour at **5% alpha** (YARG's group tint).
    - On our floor (#060606, `UI/HighwayTrack.cs:34`) that is about #121212 for a white lane and #120C0D for a pink
      one (computed).
    - It makes the white/pink (or argon) alternation visible even where no note is.
  - Setting "Lane tint": **Off / 6K and up / Always**, default **6K and up** *(tune)*.
    - Alpha's 7K skin has no column lines and a near-black floor (`baseline-alpha.md`), so if the playtest finds the
      tint distracting, change the default to Off.
- **W3. Screen-space floor on the note gap** (SPEC 6).
  - Keep the 1% inset per side, but **never less than 1 px per side on screen**. Use the mechanism of the guides'
    `MIN_LINE_PIXELS` (`HighwayGuides.cs:188`).
  - Where this changes anything:
    - at the line, nothing under Chunithm (lanes 117 px and up), and at most 0.16 px per side at 10K under Voltex
      and Parallel (84-86 px lanes);
    - from mid-depth on at 7K+;
    - at the far edge under Chunithm at 10K, notes become 26 of 28 px;
    - at the far edge under Voltex at 10K, notes become 7 of 9 px.
  - It fixes the merged centre-pair chord (Our current state) without touching Alpha's "bars fill the lane" look at
    the line.
- **W4. Colours: no change** (SPEC 6, 8).
  - The built-in white/pink pattern is mirror-symmetric, like every reference above, and argon never repeats a colour
    in adjacent lanes.
  - With the built-in colours, the even-count centre pair sharing a colour marks the hand split. That agrees with W1's
    centre divider. With W3 the pair no longer merges into one bar.
- **W5. Voltex at 7K and up: a playtest item, not a code change** (SPEC 2, 4, 5).
  - At 7K the Voltex far lanes (13 px) equal SDVX's own. At 10K (9 px) they are below every reference.
  - Levers that keep one width:
    - the wall: 16% (7K) or 45% (10K) brings entry widths to SDVX's 14.8 px at no cost in visible time (SPEC 4);
    - the optional far fade proposed in `game-yarg-clonehero.md` Y3;
    - choosing the Chunithm or Parallel preset, which SPEC 2 already names "readability at high key counts" for
      Parallel.
  - Do not change the camera per key count (Alpha, Q10). If the playtest finds 10K Voltex unreadable, the
    preset's description could recommend a wall for 9K+ (Ask Alpha: it touches default feel).
- **W6. Optional "Lane widths: Skin / Equal"** (SPEC 5), default **Skin** *(tune)*.
  - Argon's double-width centre lane at 7K/9K is a strong centre marker (and matches Chunithm converters'
    2-2-2-4-2-2-2).
  - It narrows the other six 7K lanes from 167 to 146 px under Chunithm. "Equal" lays the track out in equal
    fractions whatever the skin says.
  - Low priority. It would be visual only: keys go to mania's columns, never to drawn lane positions. It would
    change only the spans that `updateGuideLines` hands to the guides and the note layer (inference; check before
    implementing).
- **W7. Keep the camera fixed per preset.**
  - With one width, the outer edge leans the same at every key count: 24.0° under Chunithm, 20.2° under Voltex.
  - Inner dividers lean less toward the centre.
  - This confirms Alpha's reason in DECISIONS: nothing to change.

## Unverified / open questions

- The narrowest note width players can still read under perspective: no study or measurement found. The table above
  is a list of known-workable cases, not a limit.
- Whether IIDX's arcade lane proportions match beatoraja's 50 / 40 / 70: only a skin was read.
- Whether pop'n's arcade button colours match beatoraja's skin names: not checked against an official source.
- The width of YARG's black-key notes: not measured.
- Whether Chunithm's guide alpha fades with distance by design or only appears to (thin lines minified):
  `game-chunithm.md` infers fixed track width from screenshots.
- W1 and W2 values are first guesses for playtesting. No reference uses exactly a two-tier line weight; YARG uses
  tints and edge gradients instead of thicker lines.

## Sources

- `game-chunithm.md` (this folder): Chunithm field, guides, cells, note width, wall habits; its [C13], [C16] and
  converter sources (osu_2_mgxc, osu2ugc, both MIT).
- `game-sdvx.md` (this folder): SDVX lanes and the USC landscape camera numbers.
- `game-yarg-clonehero.md` (this folder): YARG camera, lanes and pro keys. Underlying code read:
  - [Y1] YARG, LGPL-3.0, `8e5c4a81f4ed80ae4360605017d71273584e7c86`:
    https://github.com/YARC-Official/YARG/tree/8e5c4a81f4ed80ae4360605017d71273584e7c86
    (`Assets/Script/Gameplay/Visuals/TrackElements/TrackElement.cs:66`, `LaneElement.cs:31-46`,
    `ProKeys/ProKeysTrackOverlay.cs:35-138`; `Assets/Script/Gameplay/Player/TrackPlayer.cs:544`,
    `ProKeysPlayer.cs:56-57`).
  - [Y2] YARG.Core, LGPL-3.0, `2f3900cf91aa141149d60cb122e88876e1709abb`:
    https://github.com/YARC-Official/YARG.Core/tree/2f3900cf91aa141149d60cb122e88876e1709abb
    (`YARG.Core/Game/YargProfile.cs:34-35, 71`).
  - Ideas and numbers only; nothing copied.
- `game-stepmania-etterna.md` (this folder): kb7 spacing, Etterna key modes, Hallway lean, field width per key count.
  StepMania and Etterna are MIT.
- `baseline-alpha.md` (this folder): Alpha's skin column widths, lines and note style.
- [B1] beatoraja, GPL-3.0 (`LICENSE`), `9340253a866a2f0091d44c6fa9de7637ce229ef9`:
  https://github.com/exch-bms2/beatoraja/tree/9340253a866a2f0091d44c6fa9de7637ce229ef9.
  - `skin/default/play/play7main.lua:129, 150-157, 187-194`: 7K lane and note widths.
  - `skin/default/play24main.lua:141-185`: 24-key half, hybrid and separate layouts.
  - `skin/default/play9.json:332, 347-355`: 9-button colours and lane widths.
  - Numbers read only, nothing copied.
- [R1] RemyWiki, beatmania IIDX: https://remywiki.com/Beatmania_IIDX. The controller has seven keys (four white,
  three black) and a turntable; on screen keys 1, 3, 5 and 7 are white, 2, 4 and 6 blue, and scratch red.
- This repo (MIT):
  - `osu.Game.Rulesets.Mania/Skinning/Argon/ManiaArgonSkinTransformer.cs:167-172, 183-375`: argon widths and palettes.
  - `osu.Game.Rulesets.Mania/Beatmaps/StageDefinition.cs:32`: special column.
  - `osu.Game.Rulesets.Mania/VariantMappingGenerator.cs:38-63` and `SingleStageVariantGenerator.cs:20-37`: default
    bindings.
  - `osu.Game.Rulesets.ManiaOrtho/Highway/HighwaySettings.cs:208-247`: presets.
  - `osu.Game.Rulesets.ManiaOrtho/UI/HighwayGuides.cs:30-53, 188-227`: guides. `UI/HighwayTrack.cs:34`: floor colour.
  - `osu.Game.Rulesets.ManiaOrtho/UI/HighwayNoteLayer.cs:49, 67-71, 147-150, 175-185`: native notes.
  - `osu.Game.Rulesets.ManiaOrtho/UI/HighwayPlayfieldAdjustmentContainer.cs:238-239, 733-752`: layer order and lane
    spans.
