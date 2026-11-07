# Research notes

Written by the `vsrg-researcher` subagent (or by hand). One file per reference game or per cross-game topic, named
`game-<name>.md` or `topic-<name>.md`. List every note in `INDEX.md`.

## Rules

- **Cite every claim.** A claim without a source is marked `(unverified)`. Prefer, in order: the game's official
  manual or site, open-source code (link to the file and line), community wikis (RemyWiki, silentblue, fandom),
  frame-accurate video with a timestamp, forum posts.
- **Separate observation from inference.** "SDVX's judgement line sits about 15% above the screen bottom (measured on
  [video] at 1:23)" is an observation. "So ours should too" goes under *What this means for us*.
- **Numbers over adjectives.** Angles, fractions of screen, milliseconds, ratios. Say how you measured them.
- **Licences.** For open-source references, note the licence. Code from GPL/AGPL/unlicensed projects may be read for
  ideas but never copied into this repository (it is MIT).
- **No copyrighted assets** (screenshots, sprites, audio) committed. Link to them instead.

## Template

```markdown
# <Game or topic>

Last checked: YYYY-MM-DD

## Summary
Three to five sentences.

## Camera and track
## Note approach and speed
## Judgement line and receptors
## Lane covers / walls
## Notes and holds (shapes, rendering)
## Key count and lane layout
## Feedback (judgement display, hit effects, lane lighting)

## What this means for us
Concrete proposals, each tied to a SPEC.md section, with suggested default values.

## Sources
- [title](url): what it supports
```
