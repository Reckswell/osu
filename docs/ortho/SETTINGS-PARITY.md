# Settings parity: osu!mania and maniaortho

Alpha asked on 2026-11-04 that every lazer setting that affects gameplay works in maniaortho as it does in osu!mania, with
hit lighting and key bindings named. This file lists every such setting, where it acts, what maniaortho does with it and
how that was checked. Update it when a setting is added to osu! or when maniaortho changes how it draws something a
setting touches.

How to read the "Checked" column:

- **Shared code** means the setting acts outside the playfield (the player screen, the HUD, the clock or the audio), or
  through mania's own drawables, which maniaortho keeps unchanged. Both rulesets run the same code, and the code path
  is named.
- **Test** names an automated test that runs in `scripts/ortho/check.sh`.
- **Capture** means mania and maniaortho were played with the setting changed (`scripts/ortho/playtest.sh --game-set ...`)
  and the screenshots compared side by side. The 2026-11-04 captures, both modes at the same score and combo in each:
  `/mnt/project-files/ortho-agent/settings-parity/mania-vs-highway-settings.png`.

## osu!mania's own settings (Settings > Game modes)

maniaortho keeps its own copy of these, as osu! does for every mode.

| Setting | maniaortho | Checked |
|---|---|---|
| Scroll speed | Same setting, stored finer (0.0001) and up to 60 instead of 40, so other games' speed numbers each land on their own speed ("Speed scale", 2026-11-07); the osu!mania slider still moves in 0.1. F3/F4 change it in play by one, or by one step of the chosen game. Shown as the time notes are visible on the highway. | Test: `TestScrollSpeedChangesApply`, `TestSceneHighwayHotkeys.TestF3AndF4StillChangeTheScrollSpeed`, `TestSceneVisibleTime` |
| Scroll direction | Not offered: the camera sets the direction (SPEC 3, DECISIONS "Scroll direction"). | By design |
| Timing-based note colouring | Highway notes and ported skin notes take the snap colour mania gives each note. With lazer's default skin, mania multiplies that colour into the column's colour (its note bodies come out darker or redder; the arrow and edge show the snap colour itself), and the highway draws the snap colour as is. | Test: `TestSceneHighwayNativeNotes`; capture with argon and the ported classic skin at 7K |
| Touch overlay | **Was missing** from maniaortho's settings; added. The same buttons appear along the bottom, and a touch presses its column and lights its lane. | Test: `TestSceneHighwaySettings.TestTouchOverlayOffered` (the box sets it), `TestSceneSettingsParity.TestTouchOverlay` (both rulesets) |
| Mobile layout | Only shown on phones and tablets, where it changes mania's stage. Not applicable to the highway. | n/a |

## Key bindings (Settings > Input > Key bindings)

| Binding | maniaortho | Checked |
|---|---|---|
| Keys per key count (4K, 7K, ...) | **Were separate**: keys changed under osu!mania did not reach maniaortho. maniaortho now plays with osu!mania's keys. Its own section is still listed, because osu! lists every mode and hiding one would need a patch to osu! itself. Its stored keys are kept equal to osu!mania's both ways (`KeyBindingMirror`): change either, and gameplay in both modes uses it at once. osu!'s key binding panel reads its keys once, so the other mode's section shows the change after osu! restarts. Keys set for maniaortho before this, where osu!mania's were still the defaults, moved to osu!mania (at the first maniaortho play or settings opening); where both had been changed, osu!mania's were kept and maniaortho's are in osu!'s log. Since 2026-11-07 this is the switch "Share osu!mania's keys" (on by default): off, maniaortho plays with its own section's keys, which start as osu!mania's, and the mirror stops; on again, the two settle as above. | Test: `TestSceneKeyBindings` (with sharing off, maniaortho's own key presses and osu!mania's does nothing, nothing is merged, and turning it on in play switches at once; a rebound key presses the same column in both rulesets and lights that lane; maniaortho's old keys do nothing; keys set for maniaortho move to osu!mania as it loads; which side wins; the mirror both ways, a second key, and a copy that leaves other columns alone). `TestSceneHighwaySettings.TestKeySharingSwitch`: the panel mirrors while on, not while off, and settles on osu!mania's when turned on again. Mutation: without the input manager change the maniaortho case fails, and without the load-time merge the moved-keys case does. |
| Decrease / Increase scroll speed (F3 / F4) | Same. | Test: `TestSceneHighwayHotkeys.TestF3AndF4StillChangeTheScrollSpeed` |
| Wall down / up (Shift with the scroll speed keys) | **Were fixed** to Shift+F3/F4. They now follow the scroll speed bindings: rebinding "Increase scroll speed" to F6 makes the wall's key Shift+F6, and the wall's toast names it. A scroll speed binding that itself uses Shift cannot move the wall. | Test: `TestSceneHighwayHotkeys.TestKeysFollowTheScrollSpeedBindings` |
| Pause, quick retry, quick exit, skip, toggle HUD (Shift+Tab), hold for HUD, seek and pause in replays, screenshot, offset keys, volume | The player screen handles these for every mode. | Shared code (`Player`, `ReplayPlayer`, `GlobalActionContainer`) |

## Gameplay (Settings > Gameplay)

| Setting | maniaortho | Checked |
|---|---|---|
| Hit lighting | Only osu! and osu!catch read it (`DrawableOsuJudgement`, `ArgonMainCirclePiece`, `Catcher`). osu!mania ignores it and so does maniaortho: mania's lighting comes from the skin, and the highway's from "Hit effects". | Shared code; no mania code reads `OsuSetting.HitLighting` |
| Score display mode | Same counter and results. | Capture (Classic) |
| Star fountains | Kiai fountains around the screen. | Shared code (`KiaiGameplayFountains`) |
| HUD visibility mode | Same HUD, hidden the same way. maniaortho's judgements and hit effects belong to the playfield, as mania's do, so they stay. | Capture (Never) |
| Show health display when you can't fail | Same. | Capture (off) |
| Always show key overlay | Same counters, one per column in order, counting the same keys. | Test: `TestSceneSettingsParity.TestKeyOverlayCounts` (both rulesets); capture |
| Show leaderboard, hold for menu button, replay settings | HUD. | Shared code |
| Background dim, blur, lighten during breaks | The background behind the playfield; the highway's track lies over it as mania's stage does. | Shared code (`Player`, `BeatmapBackground`) |
| Fade playfield to red when health is low | Red layer over the screen (`FailingLayer`). | Shared code |
| Positional hitsounds | mania pans each note by its column's index (`DrawableManiaHitObject.SamplePlaybackPosition`). maniaortho plays mania's own notes, so it is identical. | Shared code |
| Always play first combo break sound | `ComboEffects`. | Shared code |
| Beatmap skins | The highway and the skin porter read the same skin chain as mania, beatmap skin first. A beatmap skin's [Mania] section is ignored in lazer (`LegacyBeatmapSkin.AllowManiaConfigLookups`), and the porter ignores it too. | Shared code; `PortedSkin.ReadColumnStart` |
| Beatmap colours | Combo colours, which osu!mania does not use. | n/a |
| Beatmap hitsounds | Same samples, played by mania's notes. | Shared code |
| Storyboard / video | Behind the playfield. | Shared code |
| Increase first object visibility | Only osu!'s Hidden reads it. | n/a |
| Cursor size and rotation, cursor during touch | osu! only. | n/a |
| Disable Windows key | Whole game. | Shared code |

## Input, graphics and audio

| Setting | maniaortho | Checked |
|---|---|---|
| Disable mouse buttons during gameplay; touch: disable gameplay taps | Read by the ruleset's input manager, which drops those presses before they reach mania's actions. maniaortho uses mania's input manager. | Shared code (`RulesetInputManager.HandleInputStateChange`) |
| Disable mouse wheel during gameplay | The player's wheel handling (volume and seek). | Shared code (`GameplayScrollWheelHandling`) |
| Confine mouse cursor | Whole game. | Shared code |
| Scaling mode "Gameplay", size and position | The player scales its gameplay area (`Player.createGameplayComponents`), highway included. The highway draws with its own transform, so it scales with it. | Capture (60%, moved left) |
| UI scale | HUD and menus. | Shared code |
| Audio offset, beatmap offset | The gameplay clock. The highway draws from mania's frame-stable clock, so notes reach the line at the same moment. | Shared code |
| Hitsound volume | Audio. | Shared code |

## Differences found, not changed yet

These are not settings, but are part of what a player sees change between the two modes. Each is Alpha's call
(QUESTIONS.md):

- **HUD layout from the skin editor.** osu! keeps a HUD layout per mode. maniaortho starts from osu!mania's default
  layout, but a layout you customised for osu!mania does not carry over, and editing it in maniaortho saves a separate
  copy. Sharing it means wrapping mania's skin transformer, which other osu! code checks by type, so it needs care.
- **Failing.** When you fail, osu!mania's notes tumble and shrink (`FailAnimationContainer.dropOffScreen` moves mania's
  drawables). The highway draws its notes from their times, so they slow to a stop with the music instead. The rest of
  the fail (slow-down, sound, fade, red flash) is the same.
- **Touching the lanes directly.** Without the touch overlay, osu!mania lets you tap a column on the stage. On the
  highway, mania's columns still sit where mania lays them out flat, so a tap on a drawn lane can miss or press a
  different column. The touch overlay works.
