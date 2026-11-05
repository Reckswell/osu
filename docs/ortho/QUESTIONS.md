# Questions for Alpha

The loop never waits on these: it continues with the stated default where the step can be undone, and skips to other
tasks where it cannot. Alpha answers by editing this file (write under **Answer**) or in the project thread; the loop
reads this file at the start of every iteration, applies answers, and moves them to the bottom section.

## Open

### Q23. Turn on the daily check that pp match osu!'s servers
`.github/workflows/ortho-live-mania.yml` (README "Live osu!mania check") opens a pull request when osu!'s servers move to
a new osu!mania release. It cannot run yet: GitHub Actions are off for this fork, and a scheduled workflow runs only
from the default branch, which is still plain upstream (all maniaortho work is on `claude/*` branches).
- **Options**: (a) enable Actions for `reckswell/osu` (Settings > Actions > General, and under Workflow permissions tick
  "Allow GitHub Actions to create and approve pull requests") and merge the maniaortho branch into `master`; upstream's own workflows then run too (`ci.yml` on every push; deploy, Sentry and mod-definition jobs need
  ppy's secrets and should be disabled under Actions > each workflow > Disable). (b) A daily Claude routine that runs
  the script and opens the pull request instead: works now, but starts a session every day. (c) Neither: the work loop
  runs the check at the start of every iteration.
- **Default until answered**: (c). Merging to `master` is yours to do.
- **Answer**:

### Q22. Three differences from osu!mania that are not settings
Found during the settings check (`SETTINGS-PARITY.md`, last section): a HUD layout customised for osu!mania in the skin
editor does not carry over to maniaortho; when you fail, mania's notes tumble while the highway's slow to a stop; and
tapping a lane on a touch screen without the touch overlay can press the wrong column.
- **Options**: make any of them match osu!mania (each is its own task), or leave them.
- **Default until answered**: leave them; listed in `SETTINGS-PARITY.md`.
- **Answer**:

### Q20. How a side-placed skin stage is made easier to play
Some skins put their stage to one side with `ColumnStart` (R Skin's stages at 29-38% of the screen width, Aristia's at
52-54%; an IIDX 1P layout would be near 17%). The tilted track keeps the skin's centre exactly at the judgement line
(a track too wide for that spot is narrowed, never moved). First asked as three convergence choices (above the stage,
halfway, screen centre; `research/topic-off-centre-fields.md` O2). Alpha (2026-11-04) worried that the upright stage may
hurt gameplay and asked to "change the scaling at which the lanes are offset so that its easier for gameplay, rather
than a complete shift". Now two sliders: "Side stage lean" (the far end leans towards the screen's centre, 0-100%) and
"Side stage pull" (the whole stage moves towards it, 0-100%). Screenshots:
`/mnt/project-files/ortho-agent/skin-port/lanes-and-lean/`. Asked on a decision card (lean less / pull in / both).
- **Default until answered**: lean 50%, pull 0% (the stage stays at the skin's x, its far lanes come halfway in).
- **Answer**:

### Q19. An "Arcaea" camera preset, with notes that grow with distance
Arcaea's ground track (fan reimplementations' camera, `research/game-arcaea.md` R2, R3): judgement line 14% up, far
edge 7% below the top, track 85% of the screen wide, far lanes 12% of the near width, notes approaching linearly as
ours do. Its notes get about 5× thicker towards the far end, so far notes stay visible (96 px near, 7 px far at
1080p). SPEC 6 asks for one fixed thickness in track units; with it, this camera draws far notes about 2 px tall.
- **Options**: (a) the preset, with a "Far note size" setting (1-6, default 1 everywhere else) that it sets to 5;
  (b) the preset with fixed thickness; (c) no Arcaea preset.
- **Default until answered**: (c). Nothing is built; the other presets and SPEC 6 are unchanged.
- **Answer** (Alpha, 2026-11-04): (c), no Arcaea preset.

### Q2. Public name
The client must not be called "osu!" anything (trademark). What should the game and the ruleset be called?
- **Default until answered**: internal working name `maniaortho`, no public branding yet. M3 rebrand is blocked.
- **Answer**:

### Q3. Dual stage maps
Maps with two stages (e.g. 7K+7K): two highways side by side, or one wide highway?
- **Default until answered**: two highways side by side.
- **Answer**:

### Q5. Judgement text and combo: on the track (Chunithm) or flat on screen?
Chunithm shows judgement text per note in that note's lane, on the field, and the combo in the field centre under the
notes (`research/game-chunithm-feedback.md`). SPEC 1 and 7 currently keep both flat in screen space for readability.
- **Default until answered**: screen space (current SPEC). A "Judgement placement: Screen / Track" setting is planned
  either way; the question is which one the Chunithm preset uses. Arcaea adds a third way: upright text per lane,
  rising over 0.5 s (`research/game-arcaea.md` R5), which the setting could offer as "Lane (upright)".
- **Answer**:

### Q6. Chunithm's judgement names
Should judgement text use Chunithm's names (JUSTICE CRITICAL, JUSTICE, ATTACK, MISS)? They are SEGA's terms, so this
is a branding call.
- **Default until answered**: osu!mania's names in a Chunithm-like style.
- **Answer**:

### Q7. Where do notes appear with the Chunithm preset?
The arcade's track ends 16% below the top of the screen, hidden under its HUD frame (`research/game-chunithm.md`).
SPEC 2 and Alpha's brief say notes come from the top of the screen.
- **Options**: (a) keep the arcade's line height, width and far/near ratio but stretch the track to the top edge;
  (b) exact arcade geometry, far edge visible 16% below the top; (c) exact geometry with a dark frame over the top 16%
  hiding the far edge, as in the arcade.
- **Default until answered**: (a). Asked on a decision card in the thread.
- **Same for Voltex**: SDVX's track ends at a narrow visible far edge about 28% below the top, with the vanishing point
  just above it (USC's camera, `research/game-sdvx.md` P1). Stretching it to the top edge, as now, flattens the lanes'
  lean from 28 to 20 degrees and makes notes rush about 43% faster near the line. The Voltex preset follows whatever
  Q7 resolves to; the research recommends the faithful edge (0.284) for Voltex.
- **Answer**:

### Q8. Wall behaviour in the Chunithm preset
In the arcade the field wall is off by default, goes up to about 90% of the track, and keeps note speed (visible time
shrinks; players lower their speed by hand). SPEC 4 asks for constant visible time by default and a 0-80% range.
- **Default until answered**: wall off and keeps visible time (SPEC); "keep speed" stays a toggle. The range is already
  0-90% like the arcade (DECISIONS, "Wall is a fraction of track depth, up to 90%").
- **Answer**:

### Q9. Approve three more core files
Listing mania maps under maniaortho needed small edits outside the ruleset that the backlog did not name: a new
virtual property in `osu.Game/Rulesets/Ruleset.cs`, and `PresentBeatmap` in `SongSelect.cs` and `OsuGame.cs` (so
opening or importing a mania map keeps maniaortho selected instead of switching to mania). Each is a one-line
condition, behaves identically for upstream rulesets, and is tested (`BeatmapCompatibilityTest`,
`TestScenePresentBeatmapUnderOrtho`, osu!mania and song select suites). Listed in `README.md#core-patches`.
- **Default until answered**: keep them (reversible).
- **Answer**:

### Q11. What the tilted track looks like by default
The highway can draw its own track (near-black floor, teal side rails, beams on pressed lanes) and hide the skin's lane
backgrounds, receptors and stage edges, or keep the skin's stage warped onto the track. Asked on a decision card in
the thread; captures in `/mnt/project-files/ortho-agent/track/`.
- **Options**: (a) the highway's track by default, the skin's as a setting; (b) the skin's by default, the highway's as a
  setting.
- **Default until answered**: (a), "Track: Highway". Switching is one default value (DECISIONS, 2026-11-03).
- **Note** (2026-11-04): Alpha's skin port request ("port over skins and play maps without needing to adjust any
  textures") makes the ported skin the default track; skins without legacy mania images keep the highway's track.
  The warped skin stays a setting. Change back if Alpha prefers the highway's look by default.
- **Answer**:

### Q12. The HUD's key counter over the outer lanes
In replays and autoplay (or with osu!'s key overlay on), argon's key counter sits at the bottom right over the near end
of the two or three rightmost lanes at 7K (MG2 captures, `/mnt/project-files/ortho-agent/mg2/`). DECISIONS
(2026-11-03, "The HUD's key counter stays where the skin puts it") leaves it, since SPEC 7 keeps the osu! HUD.
- **Options**: (a) leave it; Alpha moves it per ruleset in the skin editor if wanted; (b) a maniaortho default HUD
  layout that moves it (and the combo counter) off the track, mania's layout untouched.
- **Default until answered**: (a).
- **Answer**:

### Q14. Four small feel choices from the research baseline
Each is a default-feel choice, so it is yours; nothing changes until you answer. Notes in `research/`.
- **(a) Missed notes.** In mania a missed note slides past the line for its 150 ms fade, the "late" cue mania players
  read; ours stop at the line, as in Chunithm (`game-osu-mania.md` P4). Options: keep Chunithm's; slide like mania.
- **(b) Wall keys with "keep speed when moving the wall" on.** Then Shift+F3/F4 change the visible time mid-song,
  which mania forbids for F3/F4 after the first 8 s. IIDX allows it (`topic-covers.md` C2). Options: any time, like
  IIDX; mania's gate only when "keep speed" is on.
- **(c) Lane tint.** A 5% tint of each lane's note colour on the floor, so lanes read apart where no note is
  (`topic-wide-keys.md` W2). Options: off; from 6K up; always.
- **(d) Voltex at 9K and up.** Its far lanes are 9 px at 10K, below every reference game. Options: leave it; the preset
  suggests a wall at 9K+ (`topic-wide-keys.md` W5).
- **Default until answered**: (a) keep Chunithm's, (b) any time, (c) off, (d) leave it.
- **Answer**:

### Q17. A "Sekai" camera preset with Project Sekai's eased approach
Project Sekai has the steepest perspective of any reference (line 21% up, far lanes 7% of the near width, notes from
the top edge) and stays readable because notes slow down less far away: their approach is not linear in track depth,
which SPEC 3 rules out. Copying its camera without that curve would make far notes crawl and near notes rush (189×
speed difference). `research/game-sekai.md` K2 has the values and the curve (it keeps hit times, visible time and SV).
- **Options**: (a) a Sekai preset with the eased approach, other presets stay linear; (b) (a) plus an "Approach:
  Linear / Eased" setting on every preset; (c) no Sekai preset.
- **Default until answered**: nothing is built. If (a) or (b), Chunithm (linear) and Sekai (eased) captures at 405 ms,
  4K and 7K come first.
- **Answer**:

### Q18. Replays of your maniaortho plays
osu! stores replays only for its four own rulesets, so it keeps none of your maniaortho plays: nothing to watch from
the leaderboard and nothing to export. Each logged play's replay is now saved as an osu!mania `.osr` in
`ortho-data\maniaortho\replays`. It opens in osu! as a mania replay (flat stage, same judgements), and the parity
tests can replay it on the highway.
- **Options**: (a) keep saving them there; (b) don't save them; (c) also let osu! store and play them back on the
  highway, which needs a patch to osu!'s own code (core patch).
- **Default until answered**: (a).
- **Answer**:

## Answered

### Q24. Do you play maps with a scratch column (8K as 7 + 1)?
Some osu!mania maps set `SpecialStyle`: in stable an 8K map is then played as 7 keys plus one scratch column on the
skin's side. Mania (lazer) reads the flag but lays the columns out as usual. The research suggests marking that column
with a stronger divider (1 | 7 instead of the hand split 4 | 4), visual only (`game-djmax.md` R3).
- **Options**: (a) yes, mark the special column; (b) no, leave it.
- **Default until answered**: (b). Nothing is built.
- **Answer** (Alpha, 2026-11-05): "no, but include it as a feature regardless". Built: N+1 maps are split 1 | N−1
  beside their special column (DECISIONS 2026-11-05).

### Q21. Share osu!mania's key bindings
maniaortho kept its own keys (osu! stores them per mode), so keys changed under osu!mania did not carry over. Asked on a
decision card (share / keep separate).
- **Default until answered**: share. Gameplay reads osu!mania's keys; maniaortho's section in the key binding settings
  stays listed (osu! lists every mode, hiding one needs a patch to osu!) and is kept equal to osu!mania's both ways (a
  change from the other side shows in the panel after a restart). Keys set for maniaortho while osu!mania's were at their
  defaults moved to osu!mania; where both had been changed, osu!mania's were kept and maniaortho's are in osu!'s log
  ("its own for variant N are replaced").
  Undo: drop `DrawableManiaOrthoRuleset.CreateInputManager`, its `Merge` call and the mirror; maniaortho then plays with
  its section's keys, which equal osu!mania's at that point.
- **Answer** (Alpha, 2026-11-04, in the thread after the settings report): "i think this looks good, continue". Share,
  as built; the card is closed with it.

### Q16. Where a ported skin's key area goes by default
Most stable skins put the hit position 16-17% above the bottom with keys or receptors below it; IIDX-style skins put a
tall key panel there. The camera's judgement line is lower (Chunithm 7.4%). Five placements are built ("Skin key
area"): Panel (line lifted to the skin's hit position, keys upright below it), Floor (same line, keys lying on the
floor), Squeezed (camera's line, keys squeezed under it), Receptors (camera's line, keys cropped by the screen edge),
Hidden (camera's line, no keys). Screenshots: `/mnt/project-files/ortho-agent/skin-port/`. Asked on a decision card.
- **Default until answered**: Panel. Switching is one default value.
- **Answer** (Alpha, 2026-11-04, on the decision card): Panel. Already the default; nothing changes.

### Q15. How maniaortho gets mania maps inside official lazer
Alpha (2026-11-04): the mod has to install on lazer, sharing files the way lazer does with stable. Lazer loads a
custom ruleset as a DLL from its data folder's `rulesets` directory and shares stable's files through its "Import from
osu!stable" (hard links on the same drive). But official lazer offers a custom ruleset only its own maps and
osu!standard converts (`BeatmapInfoExtensions.AllowGameplayWithRuleset`, unchanged on ppy's master); our fork's three
core patches (README "Core patches") are what show mania maps under maniaortho. Upstream request: ppy/osu#9263
("allow conversion of non-osu! beatmaps"; #12662 closed as its duplicate). Asked on a decision card in the thread.
- **Options**: (a) a small general hook as a pull request to ppy (Alpha approves it before it goes out); until merged,
  Alpha plays the fork build, which imports from stable the same way; (b) the DLL patches lazer in memory (Harmony):
  works now, may break on each lazer update, and a modified client while logged in risks Alpha's account; (c) both,
  the patch removed once ppy merges the hook.
- **Default until answered**: (a); keep the fork build (`scripts/ortho/play.ps1`) for playing meanwhile.
- **Answer** (2026-11-04, in the thread): keep it an isolated project for now; do not involve peppy or other osu!
  developers. So no upstream pull request, and the card was withdrawn. Alpha plays the fork build, which shares
  stable's files through lazer's own import; mania maps inside official lazer are parked (only (b) is left, and it
  needs Alpha again). Map downloads stay external (osu! website), see PLAYTESTING L4.

### Q13. Where the full game keeps its data on Alpha's PC (for the L4 session)
Alpha wants every file kept inside `Documents\Projects\osu!`. The playtest harness is portable now, but the full game
(`osu.Desktop`, needed for Alpha to play: song select, settings, replays) keeps its data in `%APPDATA%\osu-development`.
- **Options**: (a) a small portable switch in `osu.Desktop` (data next to the build, inside the folder; a core patch
  outside `osu.Game/`, listed in README); (b) let the dev build use `%APPDATA%\osu-development`.
- **Default until answered**: (a) prepared, not used until Alpha says.
- **Answer** (2026-11-04, in the thread): keep development isolated to `Documents\Projects\osu!`, but point the actual
  osu! files (maps, skins, screenshots, user data) at the original osu! install (stable, `AppData\Local\osu!`) as much
  as possible. Long term the product should be a lightweight mod: maniaortho as a separate game mode attached to the
  main osu! engine, using the original install's files; standalone is fine while developing. See DECISIONS
  2026-11-04 "Product direction".

### Q10. Track width at 7K and 10K
SPEC 5 planned a track that widens with key count (about 45% of the screen at 4K to 70% at 10K) and less tilt at 7K+.
Chunithm's field keeps one width for everything, and the Chunithm and Voltex presets do the same; with one width the
outer lanes lean the same at every key count. Asked on a decision card in the thread, with captures of 4K/7K/10K both
ways (`/mnt/project-files/ortho-agent/presets/key-counts-same-vs-spec-width.png`).
- **Options**: (a) one width for all key counts; (b) wider for more keys (SPEC 5); (c) one width, far end opened out
  from 7K up.
- **Default until answered**: (a). No code change needed for it.
- **Answer** (2026-11-03, decision card): (a) one width for all key counts, as in the arcade.

### Q1. Default camera
Which camera should the gameplay default to: SDVX-style perspective, parallel tilt, or flat?
- **Answer** (2026-11-03, in the project thread): Chunithm is the game to mirror most, so the default is perspective
  with the Chunithm preset. Applied in SPEC section 2 and DECISIONS.

### Q4. Where is the reference install?
- **Answer** (2026-11-03): `C:\Users\alpha\AppData\Local\osu!` (osu! stable), read-only. Summary of what it shows in
  `research/baseline-alpha.md`.
