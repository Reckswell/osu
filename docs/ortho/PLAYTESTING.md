# Playtesting

What the agent can check by itself, what it cannot, and how to do each level. Run the cheapest level that can catch
the problem, and always run L0 + L1 (`scripts/ortho/check.sh`) before committing.

## L0: unit tests (seconds)

Pure maths and data: projection, visible time, wall maths, settings, SR/pp parity. Live in
`osu.Game.Rulesets.ManiaOrtho.Tests`, plain NUnit `[TestFixture]` classes. Write these first for any geometry change.

## L1: replay parity (about 1 minute, headless)

`Parity/TestSceneReplayParity` plays one seeded, humanised replay (autoplay with ~30ms Gaussian timing error, so it
produces PERFECT through MISS) through osu!mania and through `maniaortho`, then requires the same judgement sequence,
score, accuracy and live pp (the HUD's pp counter). `Parity/TestSceneResultsPerformanceParity` checks the results
screen's pp the same way, with mods. Sources: `real-4k` (a real ranked map, 1419 objects), `synthetic-4k`, `synthetic-7k` (stairs,
chords, jacks, long notes, SV 0.5x and 1.75x).

Add a case when you add a code path that touches input or judgement, or when Alpha sends a replay (section L4).

## L2: screenshots (2-4 minutes per chart)

```bash
scripts/ortho/playtest.sh                                    # the default set (below), mania vs maniaortho
scripts/ortho/playtest.sh --beatmap synthetic-4k --times 9000,18000 --scroll-speed 20
scripts/ortho/playtest.sh --beatmap "/path/to/Songs/123 Artist - Title/Artist - Title (Mapper) [Diff].osu"
scripts/ortho/playtest.sh --preset Parallel --wall 0.3 --skin classic             # highway settings; built-in skins: argon, argon-pro, triangles, classic
```

The default set is synthetic-7k and real-4k at speed 29 (Alpha's speed: 405 ms visible on lazer's skins) and
synthetic-7k at speed 14 (840 ms, where Chunithm and Sekai players' notes move at the line: `game-sekai.md` K3). Runs
without `--scroll-speed` use 29: the harness keeps its settings between runs, so an unset speed would be whatever an
earlier run left. Each run's folder is `<chart>-speed<N>`, and its summary records the real visible time.

The runner (`dotnet <tests dll> playtest ...`) opens a real window, plays the chart with autoplay, and at each capture
time stops the clock at exactly that time and saves the frame. Both rulesets are captured at identical times, then
joined into `compare-*.png` (mania left, maniaortho right). `summary-<ruleset>.json` holds the renderer, window size,
fps samples and, with `--play-to-end`, final statistics.

In a cloud container this runs under Xvfb with Mesa's software OpenGL. The frames are real renders of the game. Frame
rates there (about 10-15 fps) mean nothing.

Short-lived effects (hit bursts, explosions, judgements) only show in a capture if the clock never runs past the
capture time: stopping late and seeking back drops every pooled effect that ended in between, in both rulesets, and a
software-rendered frame moves the clock 100 ms or more. So the runner stops the clock when the next frame would pass the
capture point and seeks forward onto it, and reaches a capture point up to 500 ms after the previous one by seeking
forward without playing. `clockAtCapture[]` in the summary records `jumped` (the forward seek, up to about one frame) and
`overshoot` (should be 0; anything larger means effects that ended in that window are missing). Elements on the
game-wide clock (the skin's press lighting and key area) keep real time, so a forward jump of one frame can leave a
press beam slightly behind; this is the same for both rulesets.

To capture a custom scene (for example a projection test pattern), write a `TestScene` in the tests project with a
`[Test]` method, and run `dotnet <tests dll> playtest --scene TestSceneYourScene --method TestName --screenshot out.png`
(for example `TestSceneHitBurst`, a contact sheet of hit bursts at several ages). Copy the capture step
pattern from `TestScenePlaytest`.

### Screenshot rubric

The `playtester` subagent answers each item with pass / fail / n.a. and points at the image. Anything failing goes
back to the implementer.

1. **Approach**: notes come from the top edge of the screen (or the wall), not from a point mid-screen.
2. **Judgement line**: horizontal, near the bottom, and every note at its hit time is on it (check the frame frozen at
   a note's exact time).
3. **Lanes**: correct count, equal widths at any given depth, dividers straight, nothing cut off at the screen edge.
4. **Perspective**: in perspective mode lanes converge consistently toward one vanishing point; in parallel mode they
   stay parallel; flat mode matches the mania baseline layout.
5. **Notes**: shrink with distance in perspective mode, no stretching or bending of textures, colours by lane as
   specified.
6. **Holds**: head, body and tail aligned at every depth; body width matches the lane at each depth; no gaps.
7. **Readability**: at 7K the outer lanes are still readable; far notes are distinguishable from each other.
8. **Wall**: when on, notes appear exactly at its edge and nothing is drawn beyond it.
9. **HUD**: score, combo, judgement text and progress are visible and not covered by the track.
10. **Parity of state**: the mania and maniaortho frames show the same combo and score at the same time.

## L3: on Alpha's PC (milestone gates)

The cloud cannot measure performance or show the GPU path. At milestone gates the loop asks Alpha (in the project
thread) to approve a Remote Control session on their Windows PC, then runs there:

```powershell
# in C:\Users\alpha\Documents\Projects\osu!\ortho (Alpha wants every file inside Documents\Projects\osu!)
git pull
powershell -ExecutionPolicy Bypass -File scripts\ortho\playtest.ps1    # same harness, real GPU, 1920x1080
powershell -ExecutionPolicy Bypass -File scripts\ortho\playtest.ps1 -Beatmap "playtest-output\maps\<set>\<map>.osu" -PlayToEnd
```

The full game runs through `scripts\ortho\play.ps1` (L4 below), which keeps its data inside the folder too. Run
`osu.Desktop` any other way only with Alpha's say-so: on its own it keeps its data in `%APPDATA%\osu-development`.

Report: the compare sheets, the `fps` and `frameTimeMs` blocks of each summary, and anything that differs from the
cloud screenshots. The harness measures uncapped (frame sync unlimited, 1000 Hz even when its window lacks focus,
`frameLimits` and `windowActive` in the summary) and skips the half second after each seek or capture pause, when the
playfield catches up. `frameTimeMs` p99 and max show stutter that a mean fps hides. Never copy Alpha's beatmaps, audio
or skins into the repository.

Keep everything inside the clone: the harness is portable (settings and realm next to its build output), and
`playtest.ps1` puts NuGet's caches, the .NET CLI home and temp files in `.ortho-cache`. Copy Alpha's maps into
`playtest-output\maps` (gitignored) and run them from there. The harness never contacts osu!'s servers (dummy API, no
metadata cache download, no online images).

## L4: Alpha plays

Alpha plays the full game (menus, song select, settings, replays) by double-clicking
`Documents\Projects\osu!\osu\osu.exe`. To ship changes, package it again (the game must be closed):

```powershell
# in C:\Users\alpha\Documents\Projects\osu!\ortho
git pull
powershell -ExecutionPolicy Bypass -File scripts\ortho\package.ps1       # osu\game (the build) and osu\osu.exe (the launcher)
powershell -ExecutionPolicy Bypass -File scripts\ortho\play.ps1          # for development: build the clone and start it
```

`osu.exe` (`ManiaOrtho.Launcher`) does for the package what `play.ps1` does for the clone's build: the ini files,
temp files in `osu\temp`, no update checks or crash reports, screenshots moved to stable, and `osu\launcher.log`. It
starts nothing when the packaged game is already running. `osu\version.txt` names the commit it was packaged from.

- **Data**: `Documents\Projects\osu!\ortho-data`, next to the clone, so a fresh clone or `git clean` keeps it. The
  script writes `framework.ini` (portable: the profile's data folders are never opened) and `storage.ini` (osu!'s own
  custom data location) next to the Debug build. No core patch.
- **Stable's files**: on the first start, osu!'s first-run setup offers "Import from osu!stable" (later: Settings >
  Maintenance). On the same drive it hard-links maps, skins and scores instead of copying them, and never changes
  stable's files (it only writes and deletes a `_hard_link_test` file in stable's folder to check that links work).
- **New maps**: download them from the osu! website and open them in stable as usual, then run Settings > Maintenance >
  "Import from osu!stable" again (already imported maps are skipped). Dragging an `.osz` onto the window also works but
  keeps a separate copy. Lazer's in-game beatmap listing does not need supporter (in the client only the listing's
  "played" and "rank achieved" filters do), but it needs a login to osu!'s servers, which this build does not use.
- **Screenshots** land in `ortho-data\screenshots` and the script moves them into stable's `Screenshots` folder when
  the game closes (and at the next start). Moved, not linked: PowerShell 5.1's `Remove-Item -Recurse` follows a
  junction and would empty stable's folder, and osu!'s own "move data folder" would copy all of it.
- **Servers**: the Debug build only knows osu!'s development server (dev.ppy.sh); Alpha should not log in. Like every
  lazer it downloads osu!'s public beatmap metadata file (`online.db`, about 86 MB) and the main menu's news banner
  from assets.ppy.sh. Discord rich presence connects but shows nothing while logged out. No update
  checks (`OSU_EXTERNAL_UPDATE_PROVIDER`), no crash reports (`OSU_DISABLE_ERROR_REPORTING`, and Sentry is off in
  local builds anyway). maniaortho scores are never submittable (invariant 2).
- **Outside the folder**, as with release lazer: osu.Desktop resets the NVIDIA driver's "threaded optimisation" for
  `osu!.exe` to Auto on start (stable sets it Off for itself).
- `-QuitAfter 30` closes the game after 30 s, for a remote check (two close requests: the first opens osu!'s exit
  dialog, the second confirms it). The script fails if `%APPDATA%\osu-development` changed during the run.
- osu-framework's own settings (window, renderer, frame limiter) are in `ortho-data\framework.ini`: the host's
  storage is the game's, so the empty `framework.ini` next to the build only marks it portable.
- **pp**: the results screen shows each play's pp (mania's calculators, so the same number as osu!mania). During play,
  lazer's own skins show a live pp counter; a stable skin has none in its layout, so add one with the skin editor
  (Ctrl+Shift+S during play): "LegacyPerformancePointsCounter" in the components list, on the global HUD layer (every
  ruleset) or the maniaortho layer (only here; one added on mania's layer does not show here), then save. Lazer saves
  that layout with its copy of the skin, as new files in its own store; stable's skin files are not changed.
- **Plays log**: every maniaortho pass or fail adds a row to `ortho-data\maniaortho\plays.csv` (map, mods, judgement
  counts, accuracy, stars, pp, camera and wall) and saves its replay as an osu!mania `.osr` in
  `ortho-data\maniaortho\replays`. osu! itself keeps no replay of maniaortho plays (it only stores replays of its four
  rulesets), so these files are the only ones. Quits, replays and autoplay are skipped. Close the CSV in Excel before
  playing: a locked file loses that play's row (the game shows an error).
- **Speed numbers**: the scroll speed setting and the F3/F4 and Shift+F3/F4 toasts show how long notes are visible on
  the highway with the current skin, wall and camera, not osu!mania's `11485 / speed`. Same speed, larger number than
  in osu!mania's settings: at speed 29, 405 ms with lazer's own skins (mania's label says 396), more with a stable skin
  whose judgement line is low (about 473 ms with Alpha's 4K skin). Notes move exactly as fast as in osu!mania.

Only Alpha can judge feel. When asking for an L4 session, give Alpha a short list of what to try (for example "7K at
your usual speed, wall at 30%, tell me if outer lanes are readable") and ask for:

- Free-form notes, answered into `QUESTIONS.md` or the project thread.
- Replays (`.osr`, from `ortho-data\maniaortho\replays`) of a few plays, placed in `osu.Game.Rulesets.ManiaOrtho.Tests/Resources/Replays/` only if
  Alpha agrees, together with the matching `.osu` file. A parity test can then replay them through both rulesets,
  which turns real human input into a regression test.
