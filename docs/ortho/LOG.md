# Work log

One entry per loop iteration, newest at the bottom. Format:
`YYYY-MM-DD HH:MM | task | result (done / partial / failed) | evidence (commit, test counts, screenshot dir) | next`

2026-11-03 02:40 | M0 scaffold + harness + parity | done | 16 ruleset tests pass (parity on real 4K map, synthetic 4K/7K); playtest captures of mania and maniaortho at the same time are pixel-identical apart from fading effects; full osu.Desktop registers the ruleset | next: harness polish, AllowGameplayWithRuleset patch, research baseline
2026-11-03 03:10 | M0 core patch: mania maps under maniaortho | done | BeatmapCompatibilityTest 10/10, ruleset tests 26/26, osu!mania 840/840, osu.Game filtering+present 269/269; Alpha's stable install read (research/baseline-alpha.md) | next: projection maths (L0), settings, highway playfield; Chunithm research running
2026-11-03 03:20 | M0 settings + harness polish, M1 projection maths | done | ruleset tests 79/79 (projection 32, scroll 21, settings 4); settings panel captured in a real window with no cursor | next: highway playfield (rendering approach pending Alpha's card)
