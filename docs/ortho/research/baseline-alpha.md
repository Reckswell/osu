# Baseline: how Alpha plays osu!mania today

Last checked: 2026-11-03

Source: Alpha's own osu! **stable** install (`C:\Users\alpha\AppData\Local\osu!`), read-only. Read: the user config
(credential lines redacted before reading), `osu!.db` (parsed for beatmap metadata only), the active skin's `skin.ini`
and a few of its textures. Nothing was written to that folder. No beatmaps, audio or skin files are committed; the
playtest copies of `.osu` files live outside the repository.

## Summary

Alpha is a 4K-first mania player on stable at 2560x1440: fast scroll (speed 29, about 400 ms visible), judgement
line at the very bottom of the screen, flat "bar" notes, and an error meter. The library has 1,155 mania difficulties
(969 4K, 151 7K, a few 5K/6K/8K) against 122 for all other modes, so 4K readability is the first thing to get right and
7K the second.

## Settings that matter for the highway

| Setting (stable) | Value | lazer / maniaortho equivalent |
|---|---|---|
| `ManiaSpeed` | 29, `ManiaSpeedBPMScale = 0`, no per-beatmap speed | mania ScrollSpeed 29 = `11485 / 29` ≈ 396 ms time range (`DrawableManiaRuleset.ComputeScrollTime`). Use 29 in playtests. |
| Resolution | 2560x1440 fullscreen, frame limiter unlimited | Playtest compare sheets at 16:9; device runs at 1440p. |
| `Skin` | ĀkalaMania! (Bars) | See below. `IgnoreBeatmapSkins = 1`. |
| `ScoreMeter` | Error (hit error bar), scale 1 | Keep the osu! hit error meter in screen space (SPEC 7). |
| `HitLighting` | on | Hit lighting at the judgement line expected. |
| `KeyOverlay` | on | |
| `DimLevel` | 100 (background fully dimmed) | Highway on black is the familiar look; any Chunithm field background should be subtle. |
| `ComboBurst` / `ComboFire` | off | No combo bursts. |
| 4K keys | `S F J L` | Set in lazer's key bindings; not a ruleset default. |
| 5K keys | `Z C Space M .` | |

## Skin: ĀkalaMania! (Bars)

From `skin.ini` `[Mania]` sections (stable 640x480 units; lazer converts with `LegacyManiaSkinDecoder`):

| | 4K | 7K |
|---|---|---|
| `HitPosition` | 500, which stable and lazer clamp to 480: **the judgement line is at the very bottom edge** (`LegacyManiaSkinDecoder.cs:94`) | 460: 20/480 ≈ 4% above the bottom |
| `ColumnWidth` | 60 each (stage 240/853 ≈ 28% of a 16:9 screen) | 45 each (315/853 ≈ 37%) |
| Column lines | 2 px between inner columns, none on the outside | none |
| `JudgementLine` | on | on |
| `BarlineHeight` | 0 (bar lines hidden) | 0 |
| Column colour | black at alpha 233/255 | |
| `KeysUnderNotes` | 1 | 1 |

Notes, hold heads, bodies and tails are all the same flat rectangle per column (`Bars/bar1.png` and `bar2.png`,
296x152, solid colour, no outline): **white on the outer columns, pink `#FF8290` on the inner columns** in 4K. Receptor
images (`key.png`, `keyD.png`) are 1x1 transparent: no visible receptors. `mania-hit300g.png` and `mania-hit300.png` are
transparent: **MAX and 300 judgements show nothing**; only 200/100/50/miss show text.

## What this means for us

- **Readability target** (SPEC 5): 4K first. Alpha reads flat, full-lane-width bars at about 400 ms visible time with
  the line at the bottom edge. The Chunithm preset must still be readable at that speed; check compare sheets at
  scroll speed 29 as well as the default.
- **Judgement line height** (SPEC 2, 4): Alpha's habit is 0% above the bottom. Offer down to 0 in settings (lift = 0),
  even if the Chunithm preset itself uses a higher line.
- **Notes** (SPEC 6, 8): the built-in highway skin should support plain flat bars filling the lane (Alpha's style), with
  an outer/inner column colour pattern like this skin. Do not rely on receptors being visible.
- **Feedback** (SPEC 7): make "show nothing for perfect hits" possible (Alpha hides 300g/300). Chunithm-style judgement
  text should be an option that respects this.
- **Bar lines** hidden in Alpha's skin; keep them optional on the highway.
- **Playtest set** (staged from the library, never committed): 4K LeaF - Armageddon [Lunatic] (4.1*), OISHII - ONIGIRI
  FREEWAY [Ikura Onigiri SC] (4.4*, 39% LN), rei sirose - Lost Umbrella [STEPDWI's Deluge] (6.2*, OD10), DECO*27 - Rabbit
  Hole [Hyper] (3.4*); 5K ESTi - HELIX [5K MX]; 6K onoken - felys final remix [BK's 6K EXPERT]; 7K Team Grimoire -
  C18H27NO3 [nowsmart's 7K EXHAUST] (5.1*, 25% LN), Ryu* - We're so Happy [KK's 7K MX], xi - FREEDOM DiVE [EXTRA]
  (6.8*, 6,114 objects); 8K sakuzyo - Imprinting [8K Lv.12].

## Sources

- Alpha's `osu!.<user>.cfg`, `osu!.db` and `Skins/ĀkalaMania! (Bars)/skin.ini` (local, read-only, 2026-11-03).
- `osu.Game/Skinning/LegacyManiaSkinDecoder.cs:94`: HitPosition clamp to 240-480.
- `osu.Game.Rulesets.Mania/UI/DrawableManiaRuleset.cs:183`: `ComputeScrollTime(speed) = 11485 / speed`.
- osu!.db layout: https://github.com/ppy/osu/wiki/Legacy-database-file-structure
