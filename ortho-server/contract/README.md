# Score server contract

What the game (`osu.Game.Rulesets.ManiaOrtho/ScoreServer`) and the score server (orthomania's `server/OrthoServer`) agree
on. Both test suites read these files, so a change on one side that the other does not follow fails a test. This is a
copy of orthomania's `server/contract`: change it there first, then copy it here.

| File | What |
|---|---|
| `Messages/` | The upload (`PlayUpload`, `BeatmapUpload`), answer (`PlayResult`), link (`LinkStart`, `LinkStatus`) and protocol (`ScoreServerProtocol`) types, one per file, compiled into both the game's ruleset and the server. |
| `play-upload.example.json` | The `play` part of `POST /api/plays` (multipart: `play` JSON, `beatmap` the `.osu` file, `replay` the `.osr`, optional). |
| `play-result.example.json` | The server's answer. |
| `link-start.example.json`, `link-status.example.json` | Linking the game to an account by signing in with osu!: the answers to `POST /api/link` and `POST /api/link/poll`. |
| `pp-parity.json` | Star rating and pp of synthetic plays. The game's calculators and osu!'s published ones must both give these. |
| `SyntheticManiaMap.cs` | The synthetic maps those plays are on (compiled into both test projects). |
