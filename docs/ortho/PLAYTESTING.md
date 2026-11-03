# Playtesting

What the agent can check by itself, what it cannot, and how to do each level. Run the cheapest level that can catch
the problem, and always run L0 + L1 (`scripts/ortho/check.sh`) before committing.

## L0: unit tests (seconds)

Pure maths and data: projection, visible time, wall maths, settings, SR/pp parity. Live in
`osu.Game.Rulesets.ManiaOrtho.Tests`, plain NUnit `[TestFixture]` classes. Write these first for any geometry change.

## L1: replay parity (about 1 minute, headless)

`Parity/TestSceneReplayParity` plays one seeded, humanised replay (autoplay with ~30ms Gaussian timing error, so it
produces PERFECT through MISS) through osu!mania and through `maniaortho`, then requires the same judgement sequence,
score and accuracy. Sources: `real-4k` (a real ranked map, 1419 objects), `synthetic-4k`, `synthetic-7k` (stairs,
chords, jacks, long notes, SV 0.5x and 1.75x).

Add a case when you add a code path that touches input or judgement, or when Alpha sends a replay (section L4).

## L2: screenshots (2-4 minutes per chart)

```bash
scripts/ortho/playtest.sh                                    # synthetic-7k and real-4k, mania vs maniaortho
scripts/ortho/playtest.sh --beatmap synthetic-4k --times 9000,18000 --scroll-speed 20
scripts/ortho/playtest.sh --beatmap "/path/to/Songs/123 Artist - Title/Artist - Title (Mapper) [Diff].osu"
scripts/ortho/playtest.sh --preset Parallel --wall 0.3 --skin classic             # highway settings; built-in skins: argon, argon-pro, triangles, classic
```

The runner (`dotnet <tests dll> playtest ...`) opens a real window, plays the chart with autoplay, and at each capture
time stops the clock at exactly that time and saves the frame. Both rulesets are captured at identical times, then
joined into `compare-*.png` (mania left, maniaortho right). `summary-<ruleset>.json` holds the renderer, window size,
fps samples and, with `--play-to-end`, final statistics.

In a cloud container this runs under Xvfb with Mesa's software OpenGL. The frames are real renders of the game. Frame
rates there (about 10-15 fps) mean nothing.

To capture a custom scene (for example a projection test pattern), write a `TestScene` in the tests project with a
`[Test]` method, and run `dotnet <tests dll> playtest --scene TestSceneYourScene --out dir`. Copy the capture step
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
git fetch; git checkout <branch>
pwsh scripts/ortho/playtest.ps1                                        # same harness, real GPU, 1920x1080
pwsh scripts/ortho/playtest.ps1 -Beatmap "$env:LOCALAPPDATA\osu!\Songs\...\map.osu"   # Alpha's own maps (osu!stable library)
dotnet run --project osu.Desktop                                       # full game, for song-select and settings checks
```

Report: the compare sheets, the `fps` block of each summary (mean and 5th percentile), and anything that differs from
the cloud screenshots. Never copy Alpha's beatmaps, audio or skins into the repository.

## L4: Alpha plays

Only Alpha can judge feel. When asking for an L4 session, give Alpha a short list of what to try (for example "7K at
your usual speed, wall at 30%, tell me if outer lanes are readable") and ask for:

- Free-form notes, answered into `QUESTIONS.md` or the project thread.
- Exported replays (`.osr`) of a few plays, placed in `osu.Game.Rulesets.ManiaOrtho.Tests/Resources/Replays/` only if
  Alpha agrees, together with the matching `.osu` file. A parity test can then replay them through both rulesets,
  which turns real human input into a regression test.
