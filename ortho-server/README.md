# Score server: moved to orthomania

The score server now lives in the orthomania repository (`reckswell/orthomania`), in `server/`: its code, tests, scripts
(`server/scripts/server.ps1`, `server-update.ps1`, `setup-osu-sign-in.ps1`) and README. The patcher there can also start it
with the game for development.

What stays here is `contract/`, the messages and examples the game and the server agree on, because this fork's ruleset
compiles them. orthomania's `server/contract` is the original: change it there first, then copy the change here (with this
repository's licence header). Both test suites read their copy, and `pp-parity.json` must be the same file in both.
