# Research index

Status: `todo`, `draft` (some claims unverified), `done` (every claim sourced).

| Reference | Why it matters | Open source? | Note | Status |
|---|---|---|---|---|
| osu!mania (lazer) | The base: scrolling algorithms, scroll speed to time range, hit position, skins | Yes, this repo (MIT) | `game-osu-mania.md` | todo |
| Sound Voltex | Primary visual reference: fixed button lanes on a tilted highway | No | `game-sdvx.md` | todo |
| Unnamed SDVX Clone (USC) | Open-source SDVX-style track rendering and camera | Yes: https://github.com/Drewol/unnamed-sdvx-clone (licence to check) | `game-usc.md` | todo |
| Chunithm | Alpha's primary reference: track geometry (judgement 0.926H, far edge 0.163H, far/near 0.24), field wall, speed to visible time, guide lines, note thickness | No (read: Seaurchin, MIT; Chunithm-Studio decodes, no licence, numbers only) | `game-chunithm.md` | draft |
| Chunithm: animations and feedback | Alpha's main reference: hit effects, per-note judgement text and combo on the field, lane lighting, end-of-chart banners | No (fan reimplementations read: Seaurchin, MIT; UMIGURI screenshot, MIT repo) | `game-chunithm-feedback.md` | draft |
| Project Sekai | Note art, hit effects, steep perspective lane | No | `game-sekai.md` | todo |
| beatmania IIDX | SUDDEN+ / HIDDEN+ / LIFT, "green number" visible-time speed | No | `game-iidx.md` | todo |
| StepMania / Etterna | Perspective mods (Distant, Hallway, Overhead, Space, Incoming, Tilt) on keyboard charts | Yes: stepmania/stepmania, etternagame/etterna (licences to check) | `game-stepmania-etterna.md` | todo |
| Quaver | Modern keyboard VSRG; scroll speed and skinning choices | Yes: https://github.com/Quaver/Quaver (licence to check) | `game-quaver.md` | todo |
| YARG / Clone Hero | Fixed-lane perspective highway with camera FOV, height and angle options | YARG open source (licence to check); Clone Hero closed | `game-yarg-clonehero.md` | todo |
| Arcaea | 4 ground lanes at steep tilt: readability reference | No | `game-arcaea.md` | todo |
| DJMAX Respect V | Flat keyboard VSRG with gear options, lane covers | No | `game-djmax.md` | todo |

## Cross-game topics

| Topic | Question to answer | Note | Status |
|---|---|---|---|
| Speed under perspective | How do perspective games define and display speed so it feels constant? | `topic-speed.md` | todo |
| Covers and walls | Every cover/wall option across games, with defaults and adjustment UX | `topic-covers.md` | todo |
| Wide key counts | How 7K-10K stays readable on a tilted track (any game that does it) | `topic-wide-keys.md` | todo |
| Projected texture mapping | Perspective-correct quads in a 2D engine; how USC/YARG/StepMania draw the track | `topic-rendering.md` | todo |

The design doc's reference table (https://claude.ai/artifact/GKF11mXTNjtLSg3aWkochk) is a starting point; it marks its
reference-game details as unverified, so each needs a source before it drives a decision.
