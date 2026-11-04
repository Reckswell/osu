# maniaortho score server

Your own score server for maniaortho, on your own computer. The game sends it each play you pass or fail (once you turn
on **Settings > maniaortho > Send plays to my score server**), and it keeps them, rates them with osu!'s own osu!mania pp
and shows an unofficial total at <http://127.0.0.1:7727/>. It is not osu!: it does not speak osu!'s protocols, nobody
logs in to it, and it never sends anything to osu!'s servers.

## Run it

```powershell
powershell -ExecutionPolicy Bypass -File scripts\ortho\server.ps1          # Windows
scripts/ortho/server.sh                                                     # Linux/macOS
```

It listens on `127.0.0.1:7727` (this computer only). Options: `-Port`, `-Data`, `-NoLiveCheck` (`--port`, `--data`,
`--no-live-check` for the shell script). Data lives in `ortho-scores` next to the clone:

| Path | What |
|---|---|
| `plays.db` | Every play (SQLite): judgements, mods, map, the highway settings it was played with, its rating. |
| `beatmaps/<md5>.osu` | The map file of each play, so plays can be rated again later. |
| `replays/<id>.osr` | Each play's replay (osu!mania format, opens in osu!mania). |

Every play is first written to `ortho-data\maniaortho\outbox` and leaves it only once the server has stored it. If the
server is not running, the address is wrong or the server fails, the play waits there and goes, in order, with a later
play. A play the server refuses (or fails on five times) is kept in `outbox refused` with the reason in `error.txt`;
move its folder back to `outbox` to send it again. The game never follows a redirect and never contacts a `ppy.sh`
address, whatever the setting says.

## How plays are rated

Exactly as osu! rates an osu!mania score made in lazer, with osu!'s published calculators (NuGet
`ppy.osu.Game.Rulesets.Mania`) at the version osu!'s live score processor uses
(`ppy/osu-queue-score-statistics`, newest release). The version is in `calculator-version.txt`.

- **Which plays award pp**: passes on maps that are Ranked or Approved (as the game knows the map's status), with only
  mods osu! awards pp for at their default settings (DT at 1.5x, not 1.3x). Classic does not count (lazer scores),
  Loved and Qualified maps do not. Other plays are still rated ("80pp if it counted") and listed.
- **Total pp**: per map and mod combination, the best-pp play and the best-score play are kept (osu! deletes the
  others); the best 1000 by pp; per map only the best counts; weighted 0.95^n; plus the bonus
  `(417 - 1/3) * (1 - 0.995^n)` over the kept plays. Accuracy is weighted the same way.
- **Small differences from osu!**: osu! stores stars and difficulty attributes as floats from a separate calculator
  deploy, so its pp can differ from the server's in the second decimal; a few maps on osu!'s pp blacklist would award
  pp here; osu! deletes a play the preserve rule drops for good, the server keeps it and applies the rule again after
  each rework; the map's status is whatever your game last saw (a newly ranked map counts once the game knows it).
- **The game's pp and the server's**: the game shows pp from its own copy of the calculators. `contract/pp-parity.json`
  pins both to the same values on synthetic plays (4K and 7K, DT, HT, NF, EZ, HD, MR); the toast after a play says if
  they ever differ.

## When osu! changes pp

The server checks osu!'s live version at start and every 12 hours (it reads the score processor's newest release from
GitHub; that is its only outside request). When it differs, the page shows a banner and the toast after each play says so.
To move:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\ortho\server-update.ps1   # or scripts/ortho/server-update.sh
```

It writes the new version into `calculator-version.txt`, rebuilds the server and runs its tests. Restart the server: it
rates every stored play again with the new version before it starts serving, and your total follows. If only the
`Parity:` tests fail, osu! now rates differently from this fork's copy of the calculators: the server is right, the
game's in-game pp is behind until the fork takes the same osu! update.

## API (for the game; all JSON fields are snake_case)

| Method | Path | What |
|---|---|---|
| `GET` | `/` | The score page. |
| `GET` | `/api/status` | Calculator version, osu!'s live version, number of plays. |
| `GET` | `/api/profile` | Total pp, accuracy, bonus, best plays with weights. |
| `GET` | `/api/plays?limit=50` | Latest plays. |
| `POST` | `/api/plays` | Multipart: `play` (`contract/play-upload.example.json`), `beatmap` (the `.osu` file), `replay` (optional). Answers `contract/play-result.example.json`, or 400 `{"error": ...}`. The same `score_id` twice is stored once. |
| `POST` | `/api/recalculate` | Rate every play again. |

Every `POST` needs an `X-ManiaOrtho` header (any value); without it the server answers 403. Browsers cannot add it
without asking first, so a web page you visit cannot post plays to your server.

`OrthoServer recalculate --data <folder>` and `OrthoServer check-live [--write calculator-version.txt]` do the same from
the command line.

## Layout and tests

| Path | What |
|---|---|
| `OrthoServer/` | The server (ASP.NET Core minimal API, SQLite). Builds against NuGet `ppy.osu.Game*`, not this repository's projects. |
| `OrthoServer.Tests/` | Profile maths, pp rules, the HTTP API on a real port, recalculation, the live-version check, parity. |
| `contract/` | Shared with the game (`osu.Game.Rulesets.ManiaOrtho/ScoreServer`): the upload and answer types, their examples, the synthetic maps and `pp-parity.json`. |

`scripts/ortho/check.sh` runs the server's tests after the game's. To regenerate `pp-parity.json` from the game's
calculators (only when a calculator change is intended): `ORTHO_WRITE_PP_PARITY=1 dotnet test
osu.Game.Rulesets.ManiaOrtho.Tests --filter ScoreServerParityTest`.
