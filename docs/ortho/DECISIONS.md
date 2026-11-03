# Decisions

Append-only log of decisions made without asking Alpha. Newest at the bottom. Each entry: date, decision, why,
alternatives considered, how to reverse. If a later decision overrides one, add a new entry that says so.

## 2026-11-03: Separate ruleset that delegates to mania

- **Decision**: `ManiaOrthoRuleset : Ruleset` holds a static `ManiaRuleset` instance (`MANIA`) and forwards
  conversion, difficulty, performance, scoring, health, mods, skins, key bindings and statistics to it. Only the
  drawable ruleset is its own.
- **Why**: subclassing `ManiaRuleset` would inherit `ILegacyRuleset` (legacy ID 3), which would collide with mania in
  the ruleset store and make scores submittable. Delegation keeps every number identical to mania by construction.
- **Alternatives**: subclass (rejected for the reason above); copy mania (rejected: drift).
- **Reverse**: n/a, foundational.

## 2026-11-03: Converter and difficulty calculator get the mania ruleset

- **Decision**: `CreateBeatmapConverter` passes `MANIA`; `CreateDifficultyCalculator` uses `MANIA`'s.
- **Why**: `ManiaBeatmapConverter` treats a beatmap as native only if its ruleset equals the one passed in, and
  `ManiaDifficultyCalculator` compares online IDs. Passing `this` turns every mania map into an osu!standard-style
  convert (wrong columns and notes) and changes star rating. Verified by `RulesetParityTest`.

## 2026-11-03: Working name `maniaortho`

- **Decision**: short name `maniaortho`, description "mania (ortho)", assembly `osu.Game.Rulesets.ManiaOrtho`.
- **Why**: matches the project name; the assembly must start with `osu.Game.Rulesets.` to be discovered.
- **Reverse**: the public name is Alpha's call (QUESTIONS.md). Renaming the short name later orphans local settings and
  scores stored under it, so rename before anyone plays seriously.

## 2026-11-03: Placeholder drawable ruleset subclasses `DrawableManiaRuleset`

- **Decision**: until M1 lands, `DrawableManiaOrthoRuleset` is a plain subclass, so it renders exactly like mania.
- **Why**: gives the harness and parity tests a real target from day one, and keeps mania's visibility mods working.

## 2026-11-03: Settings subsection deferred

- **Decision**: no `CreateSettings()` yet. `ManiaSettingsSubsection` requires a `ManiaRuleset` instance, and passing
  `MANIA` would bind to mania's settings rather than this ruleset's. In-game scroll speed hotkeys still work.
- **Reverse**: backlog M0 "Settings plumbing".

## 2026-11-03: Chunithm is the primary reference; perspective + Chunithm preset by default

- **Decision**: default camera is perspective with the Chunithm preset. SDVX becomes a secondary reference (the
  Voltex preset). Research priority: Chunithm viewfield and Chunithm animations/feedback first.
- **Why**: Alpha, in the project thread: Chunithm is the "game i want to mirror most", including animations and the
  gameplay viewfield. Chunithm's field is a perspective view, so this also answers Q1.
- **Reverse**: the camera mode and preset are settings; only the defaults change.

## 2026-11-03: Mania maps list natively under `maniaortho` via `Ruleset.PlaysBeatmapsOfRuleset`

- **Decision**: a new virtual on `Ruleset` names another ruleset whose beatmaps it plays as its own;
  `AllowGameplayWithRuleset` honours it, and both `PresentBeatmap` paths use `AllowGameplayWithRuleset(..., false)`
  instead of ruleset equality so presenting a mania map keeps `maniaortho` selected.
- **Why**: the alternatives were hard-coding `maniaortho` in osu.Game (worse: leaks our name into core) or relying on
  "show converted beatmaps" (does not work: converts only cover osu!standard maps). The lookup creates each ruleset
  once and caches the answer, so song select filtering stays allocation-free per beatmap.
- **Evidence**: `BeatmapCompatibilityTest` (10 cases, including upstream behaviour unchanged); osu!mania suite 840/840;
  osu.Game filtering + present-beatmap tests 269/269.

## 2026-11-03: Alpha's osu! stable install is the gameplay baseline, read-only

- **Decision**: use `C:\Users\alpha\AppData\Local\osu!` (stable, not lazer) as the reference for how the game should
  run: settings summarised in `research/baseline-alpha.md`; individual `.osu` files staged into the cloud container for
  playtests, never committed (copyright); skins only looked at, never copied. Nothing is ever written to that folder.
- **Why**: Alpha offered it as the baseline. It is stable, so its mania settings are translated to lazer equivalents.

## 2026-11-03: Camera is set in screen terms (far-edge width, heights, track width), not tilt + FOV

- **Decision**: the highway is defined by where it lands on screen: judgement-line height, far-edge height, track width
  at the line and far/near width ratio. Tilt and field of view are not settings.
- **Why**: once the track's near and far edges are pinned to screen positions (SPEC 2: notes enter at a visible far
  edge), a pinhole camera's tilt and FOV only matter through one number, `tan(tilt) / focal length`, which fixes the
  far/near width ratio. Two sliders for one degree of freedom would confuse; the width ratio is also what research can
  measure directly from Chunithm/SDVX frames. The maths is the plane homography (see `HighwayProjection`).
- **Reverse**: a tilt/FOV pair can be added later as a derived input that sets the ratio.

## 2026-11-03: Highway settings live in a second config manager under the same ruleset

- **Decision**: `ManiaOrthoRulesetConfigManager` extends mania's (so `DrawableManiaRuleset` keeps working) and owns a
  `HighwayConfigManager` with its own enum. Choosing a preset writes its values; editing a value makes it Custom; a named
  preset re-applies its values on load so tuning a preset reaches everyone using it.
- **Why**: a `RulesetConfigManager` has one enum, and mania's cannot be extended. Keys are stored by name and the two
  enums share none.
