# Covers and walls across games

Last checked: 2026-11-04

## Summary

Every keyboard VSRG and both arcade "wall" games let the player hide the far end of the lane, but they differ in two
ways that matter for SPEC 4.

**What moving the cover keeps.**

- Only IIDX with Floating Hi-Speed (and beatoraja, which copies it) **keeps visible time**: it re-computes the speed
  when the cover moves.
- Every other cover **keeps on-screen speed**, so visible time shrinks: Chunithm's field wall, USC, Quaver, Etterna,
  osu!mania's mods, and the Sonolus Sekai engine.
- Chunithm players restore the visible time by hand, by lowering their speed.

**When it can be moved.**

- IIDX and beatoraja: any time, with START + turntable, while showing both numbers.
- Etterna: during play only in its customise mode.
- Chunithm, Quaver and osu!mania: only in menus.

Our wall (constant visible time by default, any time with Shift+F3 and Shift+F4) is IIDX's design on a tilted track.

## Camera and track

Not covered here; see each game's note. Two camera facts matter for covers:

- On a flat lane (IIDX, Quaver, Etterna, mania) a cover's share of the lane is both its share of the visible time and
  its share of the screen.
- On a tilted track they split apart (table under "What this means for us").

## Note approach and speed

See `topic-speed.md`. This note only records what each cover does to speed.

## Judgement line and receptors

| Game | Raise the line? | Below the line |
|---|---|---|
| IIDX | LIFT: white number 0-830, up to 600 of 723 px; set on song select, moved with START + turntable [`game-iidx.md`] | A LIFT image (lane-cover art), customisable since EPOLIS |
| beatoraja | Lift 0-1, off by default (0.1 when on); keeps visible time [B1] | Skin's lift graphic |
| Chunithm | No LIFT in any source (unverified negative) [`game-chunithm.md`] | — |
| Quaver | Bottom lane cover 0-75%. It does not move the line. | Cover sprite |
| osu!mania | Skin `HitPosition` only. Not a gameplay setting. | Skin stage |

## Lane covers / walls

### Every option found

| Game | Option | Hides | Range | Default | Adjust in gameplay | Moving it keeps | Shown while adjusting |
|---|---|---|---|---|---|---|---|
| beatmania IIDX | SUDDEN+ | top of the lane, opaque art | WN 42-999 of 1000 (1 px of 723) | off (Beginner Option) | START + turntable, during play or before; double tap START hides or shows it | **visible time with FHS** (HS auto-adjusted); speed with Classic HS | white number (cover) and green number (visible time, 1/600 s) |
| | HIDDEN+ | bottom of the lane | (unverified) | off | like SUDDEN+ | — (judgement unaffected) | — |
| | LIFT | raises the judgement line | WN 0-830 | off | START + turntable when it is the only cover; not togglable mid-song | speed with CHS; FHS (unverified) | white number |
| beatoraja (IIDX clone) | lane cover, lift, hidden | top, raise, bottom | 0-1 each | cover **on at 0.2**; lift and hidden off (0.1) | START + scratch: 0.1% per tick; digital: 0.1% per 50 ms, then 1% per 50 ms after 0.5 s; double tap START toggles | **visible time** (HS fix on by default, main BPM); lift keeps visible time too | skin numbers: WN = cover × 1000, GN = ms × 3/5 |
| SOUND VOLTEX (arcade) | none found | — | — | — | — | — | — (unverified negative: the EG option guide lists none [S2]) |
| USC (SDVX clone) | Hidden/Sudden | notes fade near the line / far end, 0.2 fade band | cut-offs 0-1 of the track length | off | (unverified) | speed | — [`game-usc.md`] |
| Chunithm | Field wall | far part of the track, opaque vertical panel | 0-16; 5.82% of the visible track per step up to 11 | 0 | options menu between songs only (unverified that it is locked in play) | **speed** (players lower speed by hand) | — [`game-chunithm.md`] |
| DJMAX RESPECT V | Fader: Fade In 1/2, Fade Out 1/2, Blink 1/2, Fog, Blind | notes, by fixed patterns | fixed presets | off | no (equipment tab) | speed | — [D1] |
| | Lane cover | — | not found (unverified) | — | — | — | — |
| Project Sekai (official) | none found | — | — | — | — | — | — (unverified negative) |
| Sonolus Sekai engine (fan, MIT) | Stage cover (vertical) | screen above a line from the lane top to the judgement line | 0-1 in 0.01 steps (`lerp(lane top, line, v)`) | 0 | engine option, before play | speed (a drawn cover; note timing unchanged) | — [P1] |
| | Hidden | the last part of each note's approach | 0-1 of the visible time, 0.05 steps | 0 | before play | — | — [P1] |
| StepMania / Etterna (engine) | Sudden, Hidden, SuddenOffset, HiddenOffset | fade bands in field pixels: centre line 160 px from the receptors, fade 40 px, offsets move it; they tilt with the field | modifier 0-100% plus offset | off | mods (set before; themes may change them) | speed | — [SM1] (`game-stepmania-etterna.md`) |
| Etterna, Til' Death theme | Lane cover (Sudden or Hidden type) | a quad over the field from the screen edge | height in px | **off**, 10 px | Select + EffectUp/EffectDown **only with "Customize gameplay" on**: 0.1 px per update while held | **speed** | white: height (px); green: effective speed, e.g. scroll × field / (field − cover) [E1] |
| Quaver | Lane cover top / bottom | top or bottom of the playfield, skin sprite | 0-75% each, 1% steps | **off**, 25% | no: options only; built at load | speed | — [Q1] |
| osu!mania (lazer) | Cover, Hidden, Fade In mods | Cover: either end (default the far end); Hidden: near end; Fade In: far end; each with a 25% gradient | Cover 0.2-0.8 in 0.1 steps; HD/FI grow with combo, 0 in breaks | Cover 0.5 when selected | no (mods, fixed for the play) | speed (time range unchanged) | — (`game-osu-mania.md`, "Lane covers / walls") |
| YARG | none; the highway's far end fades over 1.25 units | — | "highway length" multiplier | 1 | no (profile) | speed (longer highway, more visible time) | — (`game-yarg-clonehero.md`) |
| Clone Hero | none documented | — | highway length (since v1) | — | — | — | — (`game-yarg-clonehero.md`, unverified negative) |

### Three patterns

1. **Speed-keeping covers.** Chunithm, Quaver, Etterna, mania, USC and Sekai (Sonolus) put a mask over a lane whose
   timing is untouched. Visible time = visible fraction × full-lane time.
   - Chunithm players keep visible time about constant by lowering the speed when they raise the wall. Examples: wall
     3 at 8.5, wall 4 at 8.0, no wall at 10.0, all about 530-550 ms [`game-chunithm.md`].
2. **Visible-time-keeping covers.** In IIDX FHS and beatoraja, moving the cover rescales the hi-speed so the green
   number stays fixed. The player's speed choice *is* the visible time, and the cover only changes where notes appear
   and how fast they move on screen.
3. **Display.** Only the IIDX family and Etterna's Til' Death show numbers while the cover moves:
   - IIDX shows the cover size and the visible time.
   - Etterna shows the cover height and an effective speed.

### Step sizes and repeat while held

| Game | One step | Held |
|---|---|---|
| IIDX | analog turntable; 1 px of a 723 px lane (0.14%) | analog |
| beatoraja | 0.1% (analog tick or digital) | 0.1% / 50 ms = 2%/s, then 1% / 50 ms = 20%/s after 0.5 s |
| Etterna Til' Death | — | 0.1 px per update (frame-rate dependent) |
| Chunithm | 5.82% of the visible track (steps 0-11; larger above) | menu |
| osu!mania Cover | 10% of the playfield | mod setting |
| Sonolus Sekai stage cover | 1% | engine option |
| **maniaortho** | **5% of the track depth** | osu-framework's repeat: 250 ms, then every 70 ms → about 70%/s (corrected 2026-11-04; was "OS key repeat, about 30 Hz → about 150%/s") |

## Notes and holds, key count, feedback

Not relevant to covers.

## What this means for us

**C1. Semantics (SPEC 4): the default matches IIDX, the reference SPEC names.**

- Constant visible time is IIDX FHS's rule (official INFINITAS manual: the green number stays fixed while SUDDEN+
  moves).
- Chunithm, the visual reference, keeps speed instead. Its players re-create our default by hand.
- **No change.** Q8 (what the Chunithm preset should use) stays open for Alpha. The "keep speed" toggle covers
  Chunithm, Quaver, Etterna and mania.

**C2. Mid-song adjustment: matches IIDX and beatoraja.** Most keyboard games allow only menus. **Keep "any time".**

- IIDX also lets Classic hi-speed players move SUDDEN+ mid-song, which changes their visible time. So the reference
  does not require mania's F3/F4 gate (until 8 s after the first note, breaks, never paused) even with "keep speed"
  on.
- `game-osu-mania.md` proposal 5 (gate only when "keep speed" is on) is a fairness choice, not a reference behaviour.

**C3. Show what changed.** IIDX, beatoraja and Etterna show the cover size *and* the speed outcome.

- **Proposal:** the wall toast reads "Wall: 40% covered · 405 ms visible".
- With "keep speed": "Wall: 40% covered · 243 ms visible (speed kept)".
- At 0%: "Wall: off · 405 ms visible".
- The ms must be the highway's real visible time: mania's `TimeRange` after the skin hit-position and rate factors,
  times (1 − wall) when speed is kept. Mania's label `11485 / speed` is not that value (`game-osu-mania.md`,
  proposal 1). The examples are speed 29 on argon.

**C4. Step and repeat.** Ours is 25-75× faster than beatoraja's held rate, at the mercy of the OS repeat rate.
*Corrected 2026-11-04:* the repeat is osu-framework's (250 ms delay, then every 70 ms, real time), about 70%/s: 3.5× beatoraja's
fast rate, independent of the OS. The timer below was tried and dropped (DECISIONS, "Held wall keys keep
osu-framework's repeat").

- **Proposal:** keep **5% per press**, close to Chunithm's 5.82% step, so round values stay reachable.
- Repeat at **one step per 100 ms** (50%/s) from a timer of our own while the key is held, ignoring OS repeats.
- A 1% fine step is optional and needs a free key combination; check osu!'s global bindings first.

**C5. Range.** IIDX goes to 999/1000, Chunithm to 16 (past the line), mania Cover to 80%, Quaver to 75%. Ours is 0-90%
of the depth. **Keep**, since Chunithm players use up to wall 14 (88.5%).

**C6. Screen position versus depth.** Under the Chunithm camera, equal depth steps are very unequal on screen. Base of
the wall, fraction of screen height from the top, our Chunithm preset with the far edge stretched to the top
(r = 0.24, line 0.926H, vanishing point −0.292H):

| Wall (depth) | 0 | 10% | 20% | 30% | 40% | 50% | 60% | 70% | 80% | 90% |
|---|---|---|---|---|---|---|---|---|---|---|
| Base y | 0 | 0.024 | 0.052 | 0.086 | 0.128 | 0.179 | 0.245 | 0.332 | 0.454 | 0.633 |

- A 5% step moves the base 1.2% of H near 0 and 8-10% of H near 80-90%.
- The arcade has the same profile (steps 1.0% → 8.2% of H with its 0.163H far edge).
- Depth steps are what Chunithm uses, so **keep depth**.
- If playtests find the low end too fine, make the held repeat skip faster there rather than switching to screen steps.

**C7. No bottom cover setting.** HIDDEN+ is "rarely used" in IIDX, and mania's Hidden (combo-driven) is already
projected (DECISIONS). **Not proposed.**

**C8. Below a lifted line.** IIDX draws a cover image under LIFT; beatoraja draws the skin's lift graphic. Ours keeps
the receptor area and then the background (DECISIONS, bottom lift). **Keep.** An optional lift cover can come with the
highway skin.

**C9. Cover look.** IIDX, Quaver and Etterna covers are opaque panels in the lane plane. Chunithm's is a vertical panel
standing on the track. Ours is a vertical panel (DECISIONS). **No change.**

## Unverified

- Whether SDVX, official Project Sekai, Arcaea or DJMAX have any adjustable lane cover. None was found in their option
  guides.
- Whether Chunithm's wall can change during a song.
- USC's in-game cover controls.
- IIDX: HIDDEN+ range; FHS with LIFT.

## Sources

- IIDX facts: `game-iidx.md` [I1]-[I13].
- [B1] beatoraja, GPL-3.0, read only, commit 9340253a. Details and line numbers in `game-iidx.md`.
- Chunithm field wall: `game-chunithm.md` ("Lane covers / walls").
- USC: `game-usc.md` ("USC: lanes, notes, speed, covers"). SDVX: `game-sdvx.md` [S2].
- [D1] Steam Community guide "DJMAX RESPECT V 100% Achievements, Walkthrough, and Unlock Guide":
  https://steamcommunity.com/sharedfiles/filedetails/?id=2362889860. Speed 1.0-9.0 in 0.1 steps; Fader options; no
  lane cover listed.
- [P1] sonolus-pjsekai-engine (NonSpicyBurrito), **MIT**, commit 0f6f37fc9b512b9d156ec4c00d998fe3916b157d:
  https://github.com/NonSpicyBurrito/sonolus-pjsekai-engine/tree/0f6f37fc9b512b9d156ec4c00d998fe3916b157d.
  - `shared/src/engine/configuration/options.ts:15-24` (hidden), `:118-127` (stageCover).
  - `play/src/engine/playData/archetypes/Stage.ts:126-136` (cover drawn from the screen top to
    `lerp(lane.t, 1, stageCover)`).
  - `play/src/engine/playData/archetypes/notes/flatNotes/FlatNote.ts:84` (hidden time = hit − duration × hidden).
- [SM1] StepMania, MIT-style licence (`Docs/Licenses.txt`), commit 825467bcd81c812b33ad684dc04dd151b2d5dec3:
  https://github.com/stepmania/stepmania/blob/825467bcd81c812b33ad684dc04dd151b2d5dec3/src/ArrowEffects.cpp#L1018-L1106
  (`CENTER_LINE_Y 160`, `FADE_DIST_Y 40`, Sudden/Hidden lines and offsets).
- [E1] Etterna, **MIT**, commit 2d706147cc4bcf95a8ceb24dc4f4129be207f803:
  https://github.com/etternagame/etterna/tree/2d706147cc4bcf95a8ceb24dc4f4129be207f803.
  - `Themes/Til Death/BGAnimations/ScreenGameplay overlay/lanecover.lua`: lines 13 and 178 (input only with
    CustomizeGameplay), 84-93 (effective speed = speed × h / (h − cover)), 100-103 (an unused "IIDX green number"
    helper), 151-160 (Select + EffectUp/Down), 270-293 (0.1 px per update, white and green text).
  - `Themes/Til Death/Scripts/01 player_config.lua:87-88,100` (LaneCover 0, height 10, CustomizeGameplay false).
- [Q1] Quaver, **MPL-2.0**, commit e9b54a66c5274b1651328c66e9ea3846f8614508:
  https://github.com/Quaver/Quaver/tree/e9b54a66c5274b1651328c66e9ea3846f8614508.
  - `Quaver.Shared/Config/ConfigManager.cs:1244-1249` (LaneCoverTop/BottomHeight 25, 0-75; both off).
  - `Quaver.Shared/Screens/Gameplay/Rulesets/Keys/Playfield/GameplayPlayfieldKeysStage.cs:758-790` (cover sprites
    built at load).
- osu!mania: `game-osu-mania.md` ("Lane covers / walls", "F3 / F4 speed keys", proposals 1 and 5).
- StepMania and Etterna: `game-stepmania-etterna.md` ("Lane covers / walls").
- YARG and Clone Hero: `game-yarg-clonehero.md`.
- Ours: `osu.Game.Rulesets.ManiaOrtho/Highway/HighwayScroll.cs:18,39-49`, `UI/HighwayHotkeys.cs:33,61-75`,
  `Configuration/ManiaOrthoRulesetConfigManager.cs:91-93,176-180`.
- Wall-base numbers: our own homography with r = 0.24, line 0.926H, far edge 0 or 0.163H. Same method as
  `game-chunithm.md`; scratch script outside the repository.
