# Survey: stable osu!mania skins, and what the skin porter must handle

Last checked: 2026-11-04

Source: the seven skins in Alpha's osu! **stable** install (`C:\Users\alpha\AppData\Local\osu!\Skins`), read-only: each
skin's `skin.ini` `[Mania]` sections and its mania images (sizes measured, contact sheets looked at). Nothing was written
to that folder and no skin file is committed: playtest copies live outside the repository. How osu! (lazer) lays the
same values out comes from this repository's `osu.Game.Rulesets.Mania/Skinning/Legacy/*` and
`osu.Game/Skinning/LegacySkin.cs`, `LegacyManiaSkinDecoder.cs` (MIT).

Units: `skin.ini` values are stable units, a 480-unit tall screen (853 wide at 16:9). osu! (lazer) multiplies them by
1.6 into its 768-unit tall mania playfield. "Centre" is the stage's centre at 16:9, from `ColumnStart` plus the stage
width; 427 is the middle of the screen.

## Summary

Every skin is a **stage of columns plus a key area under a hit position**; they differ in five ways the porter has to
carry over:

1. **Hit position** (where notes are hit): from the very bottom of the screen (ĀkalaMania 4K: 480) to a fifth of the
   way up (Minty 5K+: 385). Most sit at 400-402, the stable default: the judgement line is 16-17% above the bottom, with
   the keys below it. This is the "notes do not end at the bottom of the screen" case, and it is the common one, not an
   exception. IIDX-themed skins are the extreme: a tall key panel with the line well above it.
2. **Stage position** (`ColumnStart`): most skins set it so the stage is centred at 16:9 (centre 426-430). R Skin puts
   it on the **left** (centre 250-320 by key count), Aristia's 6K-8K sit **right** (446-462: "cascaded to the right"),
   Starcruiser slightly left (343-388), Kitsune 10K+ slightly left (407). osu! (lazer) parses `ColumnStart` but always
   centres the stage, so flat lazer already loses this.
3. **Stage width**: 4K stages are 21-36% of a 16:9 screen (Kitsune 160, Minty and Aristia 180, R Skin 208, Ākala 240,
   Flow 306 units); wide key counts grow to most of it (Kitsune 18K: 720/853 = 84%, "fullscreen"). Column widths vary
   per column for IIDX layouts (Aristia 8K: a 45-unit scratch lane then 28/25 alternating).
4. **Note shape**: flat bars (Ākala 296x152, R Skin 256x150, Minty 256x109), framed bars (Aristia 256x82), glossy
   pills (Starcruiser), circles (Flow 75x75). Hold bodies are usually plain or edge-striped columns, sometimes animated
   (Starcruiser 3 frames); heads and tails are often blank (R Skin heads 1000x1, Flow 128x128 blank).
5. **Key area art**: from nothing (Ākala 1x1 transparent keys, `KeysUnderNotes`), through a thin receptor line (Minty,
   Starcruiser), to full panels (Aristia dark panels with a lit receptor; Flow ring receptors 150x250; IIDX turntable
   images). Plus lane lights (`mania-stage-light`), a hit target band (`mania-stage-hint`), stage side images
   (`mania-stage-left/right`, from thin lines to 800x770 decorations) and a stage bottom logo (`mania-stage-bottom`).

## Per skin

| Skin | Keys | Hit position | Stage (centre / width at 4K) | Notes | Key area | Stage art |
|---|---|---|---|---|---|---|
| ĀkalaMania! (Bars), Alpha's | 4K, 7K | 4K 500 (clamped to 480: line at the bottom), 7K 460 | centred, 240 (60 per column) | bars 296x152: pink, white, yellow | none (1x1 keys), `KeysUnderNotes`, `Colour1` 0,0,0,233 (dark lanes), 2 px inner lines at 4K | hint 210x25, side lines @2x |
| - Minty | 1K, 4K-10K | 4K 400, 5K+ 385 | centred, 180 | flat mint and white bars 256x109 | keys 50x192: a thin receptor line, a gradient when pressed | thin side lines (800x770), light 64x640, lighting 210x240, 12 frames |
| Aristia(Edit) | 4K-8K | 400 | 4K centred; 6K-8K right of centre (446-462) | pink framed bars 256x82 with H and L variants | keys 50x192: dark panels with a blue receptor, a blue box when pressed; 8K `SpecialStyle 1` (IIDX: 45-unit scratch lane, 28/25 lanes), turntable images (`mania-keyTURN`, not referenced: the ini names `Mania-keyT`) | hint 179x128, bottom logo 200x24, large side images 800x770 and 640x770, coloured lights per lane |
| Kitsune v1 | 1K-18K | 400 (1K 480) | centred to 9K, then slightly left; 160 at 4K, 720 at 18K (84% of the screen) | rounded red and white bars 256x109 | keys 50x192, transparent, a grey block when pressed | red and white side lines 500x770, 1x1 lighting |
| R Skin v3.0 (Bars) | 1K-18K | 440 | **left**: centre 304 at 4K (`ColumnStart` 200) | solid bars 256x150: white, cyan, green, yellow; blank heads (1000x1), red-edged bodies 256x82, black tails | keys 50x100, transparent | pink hint 210x25, bottom 209x29, `SpecialStyle Left` at 8K |
| Starcruiser Skin | 4K-9K | 402 (default) | slightly left (343-388), default 30-unit columns | glossy green, blue, pink pills (@2x), 3-frame animated bodies | keys 50x117 with star icons, a blue glow when pressed | thin side lines, light 50x250 gradient, hint 210x26 blue line |
| (599 kB) Mania Flow Mode | 4K-10K | 450 | centred, 306 (75 per column, the widest 4K) | white **circles** 75x75 (@2x) | ring receptors 150x250 (`keyD` blank) | no judgement line, no column lines, no bar lines; images in `Mania/` subfolders |

## Layout types

- **Bottom line, no keys** (Ākala 4K): the flat look most like a highway already: notes run to the bottom edge.
- **Standard key area** (Minty, Kitsune, Starcruiser, R Skin, Aristia 4K-7K; hit position 400-450): a strip of keys or
  receptors 6-17% of the screen tall below the line.
- **IIDX panel** (Aristia 8K and IIDX-themed skins): a scratch lane, uneven lane widths and a tall key panel.
- **Off-centre** (R Skin left, Aristia 6K+ right, Starcruiser and Kitsune 10K+ slightly left).
- **Fullscreen** (Kitsune 14K-18K): the stage spans most of the screen width.

## What this means for the porter (decisions in DECISIONS.md, 2026-11-04)

- Read everything through osu!'s own mania lookups (same image names, fallbacks and per-column values as mania's legacy
  pieces), so every skin osu! (lazer) can draw flat, the porter can draw tilted, including fallbacks to the default
  classic skin's images. `ColumnStart` is the one value lazer does not use; it is read from the parsed configuration.
- Measure the flat stage mania lays out (hit position, unit size, stage width) instead of recomputing it, so the port
  always agrees with the flat game.
- The key area is the hard part: a camera's judgement line (7.4% for Chunithm) is lower than most skins' (16-17%).
  Five placements are offered (setting "Skin key area"); see the variant screenshots in the porter's report.
- Notes keep the skin's proportions at the line by default (a 296x152 bar is as tall for its width at the line as it is
  flat), which matches what Alpha already plays (bars about 6% of the track); circle notes become ellipses lying on the
  track.
- Hit lighting keeps its flat size (lighting images are 210x240 for 35-50 unit columns: three to five columns wide), so
  it is not grown with a wider track.
