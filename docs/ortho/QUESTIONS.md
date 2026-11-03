# Questions for Alpha

The loop never waits on these: it continues with the stated default where the step can be undone, and skips to other
tasks where it cannot. Alpha answers by editing this file (write under **Answer**) or in the project thread; the loop
reads this file at the start of every iteration, applies answers, and moves them to the bottom section.

## Open

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
  either way; the question is which one the Chunithm preset uses.
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

## Answered

### Q1. Default camera
Which camera should the gameplay default to: SDVX-style perspective, parallel tilt, or flat?
- **Answer** (2026-11-03, in the project thread): Chunithm is the game to mirror most, so the default is perspective
  with the Chunithm preset. Applied in SPEC section 2 and DECISIONS.

### Q4. Where is the reference install?
- **Answer** (2026-11-03): `C:\Users\alpha\AppData\Local\osu!` (osu! stable), read-only. Summary of what it shows in
  `research/baseline-alpha.md`.
