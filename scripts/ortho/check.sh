#!/bin/bash
# Fast gate for every change: style-enforced build plus the ruleset's own tests (parity + unit + headless smoke).
#   scripts/ortho/check.sh            # ~2-3 min warm
#   scripts/ortho/check.sh --mania    # also run osu!mania's own test suite (do this when touching mania or osu.Game)
#   scripts/ortho/check.sh --filter X # pass an NUnit filter through to the ruleset tests
set -euo pipefail
cd "$(git rev-parse --show-toplevel)"

run_mania=0
filter=""
while [ $# -gt 0 ]; do
  case "$1" in
    --mania) run_mania=1 ;;
    --filter) filter="$2"; shift ;;
    *) echo "unknown option $1" >&2; exit 2 ;;
  esac
  shift
done

echo "== scripts"
bash scripts/ortho/live-mania-test.sh

echo "== build (warnings as errors, code style enforced)"
dotnet build osu.Game.Rulesets.ManiaOrtho.Tests -c Debug -warnaserror -v q -nologo
# Incremental builds skip analyzers, so re-run them on this fork's own projects every time (about 20s).
for project in osu.Game.Rulesets.ManiaOrtho osu.Game.Rulesets.ManiaOrtho.Tests ManiaOrtho.Launcher; do
  dotnet build "$project" -c Debug -warnaserror -p:EnforceCodeStyleInBuild=true --no-incremental --no-dependencies -v q -nologo
done

echo "== ruleset tests"
# A playtest killed mid-run leaves its framework.ini in the build output; with it, NUnit's headless hosts take that folder
# as their storage and delete it on cleanup, and every later test fails on missing assemblies (PlaytestRunner).
rm -f osu.Game.Rulesets.ManiaOrtho.Tests/bin/Debug/net10.0/framework.ini
args=(osu.Game.Rulesets.ManiaOrtho.Tests --no-build --logger "console;verbosity=normal")
[ -n "$filter" ] && args+=(--filter "$filter")
log="$(mktemp -t ortho-check.XXXXXX.log)"
set +e
dotnet test "${args[@]}" -- NUnit.ConsoleOut=0 >"$log" 2>&1
status=$?
set -e
grep -E "^\s+(Passed|Failed) |Error Message|Expected|But was|^\s+at osu\.Game\.Rulesets\.ManiaOrtho|Total tests|Passed!|Failed!" "$log" || true
if [ $status -ne 0 ]; then
  echo "RULESET TESTS FAILED (full log: $log)"
  exit 1
fi

if [ "$run_mania" = 1 ]; then
  echo "== osu!mania tests"
  dotnet test osu.Game.Rulesets.Mania.Tests -nologo -- NUnit.ConsoleOut=0 2>&1 | tail -3
fi

echo "CHECK PASSED"
