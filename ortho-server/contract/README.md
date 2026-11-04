# Score server contract

What the game (`osu.Game.Rulesets.ManiaOrtho/ScoreServer`) and the score server (`ortho-server/OrthoServer`) agree on.
Both test suites read these files, so a change on one side that the other does not follow fails a test.

| File | What |
|---|---|
| `ScoreServerMessages.cs` | The upload (`PlayUpload`) and answer (`PlayResult`) types, compiled into both the game's ruleset and the server. |
| `play-upload.example.json` | The `play` part of `POST /api/plays` (multipart: `play` JSON, `beatmap` the `.osu` file, `replay` the `.osr`, optional). |
| `play-result.example.json` | The server's answer. |
| `pp-parity.json` | Star rating and pp of synthetic plays. The game's calculators and osu!'s published ones must both give these. |
| `SyntheticManiaMap.cs` | The synthetic maps those plays are on (compiled into both test projects). |
