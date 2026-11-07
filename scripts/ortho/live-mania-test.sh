#!/bin/bash
# Offline test of scripts/ortho/live-mania.sh against throwaway repositories: an "upstream" with releases and a fork of
# it with work of its own. Run by scripts/ortho/check.sh. Needs only git.
set -euo pipefail
script="$(cd "$(dirname "$0")" && pwd)/live-mania.sh"
root="$(mktemp -d -t ortho-live-mania.XXXXXX)"
trap 'rm -rf "$root"' EXIT

# Only this test's settings: a global commit.gpgsign or merge.ff=only would change what git does.
export GIT_CONFIG_GLOBAL=/dev/null GIT_CONFIG_NOSYSTEM=1
export GIT_AUTHOR_NAME=test GIT_AUTHOR_EMAIL=test@example.invalid GIT_COMMITTER_NAME=test GIT_COMMITTER_EMAIL=test@example.invalid
unset GITHUB_OUTPUT

fail() { echo "live-mania test FAILED: $*" >&2; exit 1; }
g() { git -c init.defaultBranch=master "$@" >/dev/null 2>&1; }
commit() { g add -A && g commit -q -m "$1"; }
content() { cat "$root/fork/$1" 2>/dev/null || echo "(none)"; }
expect() { [ "$(content "$1")" = "$2" ] || fail "$3: $1 is \"$(content "$1")\", expected \"$2\""; }

# Runs the script in the fork; sets $output and $code.
run() {
  set +e
  output="$(cd "$root/fork" && bash "$script" --upstream "$root/upstream" "$@" 2>&1)"
  code=$?
  set -e
}

mkdir -p "$root/upstream" && cd "$root/upstream"
g init -q
mkdir -p osu.Game.Rulesets.Mania/Difficulty osu.Game.Rulesets.Mania/MathUtils osu.Game/Rulesets/Scoring osu.Game/Screens osu.Game/Beatmaps
echo "strain 1" > osu.Game.Rulesets.Mania/Difficulty/Strain.cs
echo "sort 1" > osu.Game.Rulesets.Mania/MathUtils/LegacySortHelper.cs
echo "extensions" > osu.Game/Beatmaps/BeatmapInfoExtensions.cs
echo "score 1" > osu.Game/Rulesets/Scoring/ScoreProcessor.cs
echo "old" > osu.Game/Rulesets/Scoring/Old.cs
echo "menu 1" > osu.Game/Screens/Menu.cs
commit "release 2026.101.0" && g tag 2026.101.0

g clone -q "$root/upstream" "$root/fork" && cd "$root/fork"
echo "highway" > Highway.cs
echo "menu, patched" > osu.Game/Screens/Menu.cs
echo "extensions, patched" > osu.Game/Beatmaps/BeatmapInfoExtensions.cs
commit "the fork's work, and core patches outside scoring (one next to compared files)"

run --live-version 2026.101.0
[ "$code" = 0 ] && [[ "$output" == *"same code"* ]] || fail "same release: exit $code: $output"

# A release the fork lacks, that merges cleanly: a strain changed, a skill added, a scoring file removed, a file outside
# scoring added. The fork also has a scoring file of its own.
cd "$root/upstream"
echo "strain 2" > osu.Game.Rulesets.Mania/Difficulty/Strain.cs
echo "sort 2" > osu.Game.Rulesets.Mania/MathUtils/LegacySortHelper.cs
git rm -q osu.Game/Rulesets/Scoring/Old.cs
echo "new skill" > osu.Game.Rulesets.Mania/Difficulty/Skill.cs
echo "other" > osu.Game/Other.cs
commit "release 2026.201.0" && g tag 2026.201.0
cd "$root/fork"
echo "fork only" > osu.Game/Rulesets/Scoring/ForkOnly.cs
commit "a scoring file only the fork has"

run --live-version 2026.201.0
[ "$code" = 3 ] || fail "new release: exit $code: $output"
[[ "$output" == *"has that release: no"* ]] || fail "new release: not seen as missing: $output"
for expected in "M	osu.Game.Rulesets.Mania/Difficulty/Strain.cs" "D	osu.Game.Rulesets.Mania/Difficulty/Skill.cs" "A	osu.Game/Rulesets/Scoring/ForkOnly.cs" "A	osu.Game/Rulesets/Scoring/Old.cs" "M	osu.Game.Rulesets.Mania/MathUtils/LegacySortHelper.cs"; do
  [[ "$output" == *"$expected"* ]] || fail "new release: \"$expected\" not listed: $output"
done
[[ "$output" != *"Menu.cs"* && "$output" != *"Other.cs"* && "$output" != *"BeatmapInfoExtensions"* ]] || fail "new release: a file outside scoring listed: $output"
[ -z "$(git status --porcelain)" ] || fail "a check without --apply changed files"

run --live-version 2026.201.0 --apply --pr-body "$root/pr.md"
[ "$code" = 3 ] && [[ "$output" == *"merged the release"* ]] || fail "apply (clean merge): exit $code: $output"
grep -q '\*\*2026.201.0\*\*' "$root/pr.md" || fail "the description does not name the release"
grep -q 'This merges it' "$root/pr.md" || fail "the description does not say it merged"
grep -q '`osu.Game/Rulesets/Scoring/ForkOnly.cs` | only here (removed)' "$root/pr.md" || fail "the description does not list the fork's file"
expect osu.Game.Rulesets.Mania/Difficulty/Strain.cs "strain 2" "apply (clean merge)"
expect osu.Game.Rulesets.Mania/Difficulty/Skill.cs "new skill" "apply (clean merge)"
expect osu.Game/Other.cs "other" "apply (clean merge) brings the rest of the release"
expect osu.Game/Rulesets/Scoring/ForkOnly.cs "(none)" "apply (clean merge) removes the fork's scoring file"
expect osu.Game/Rulesets/Scoring/Old.cs "(none)" "apply (clean merge) takes the release's removal"
expect osu.Game.Rulesets.Mania/MathUtils/LegacySortHelper.cs "sort 2" "apply (clean merge)"
expect osu.Game/Screens/Menu.cs "menu, patched" "apply (clean merge) keeps the core patch"
expect osu.Game/Beatmaps/BeatmapInfoExtensions.cs "extensions, patched" "apply (clean merge) keeps the core patch beside compared files"
expect Highway.cs "highway" "apply (clean merge) keeps the fork's work"
expect docs/ortho/live-mania-version.txt "2026.201.0" "apply (clean merge) records the release"
[ -z "$(git diff --name-only)" ] || fail "apply: changes left unstaged"
commit "match 2026.201.0"
run --live-version 2026.201.0
[ "$code" = 0 ] || fail "after the clean merge: exit $code: $output"

# Upstream changes the strain again after the release, and the fork takes upstream's master: osu!'s servers do not run
# it yet, so the file goes back to the release's, without a merge.
cd "$root/upstream"
echo "strain 2.1, not deployed" > osu.Game.Rulesets.Mania/Difficulty/Strain.cs
commit "after the release"
cd "$root/fork"
g pull -q --no-rebase --no-edit origin master || fail "the fork could not take upstream's master"
run --live-version 2026.201.0 --pr-body "$root/pr.md"
[ "$code" = 3 ] && [[ "$output" == *"has that release: yes"* ]] || fail "newer here: exit $code: $output"
grep -q 'has not deployed yet' "$root/pr.md" || fail "the description does not explain changes newer than the release"
head="$(git rev-parse HEAD)"
run --live-version 2026.201.0 --apply
[ "$code" = 3 ] && [ "$(git rev-parse HEAD)" = "$head" ] || fail "apply (newer here) made a commit or failed: exit $code: $output"
expect osu.Game.Rulesets.Mania/Difficulty/Strain.cs "strain 2" "apply (newer here)"
commit "back to 2026.201.0"

# A release whose merge conflicts with the fork's core patch: the merge is aborted and only the scoring files taken.
cd "$root/upstream"
echo "strain 3" > osu.Game.Rulesets.Mania/Difficulty/Strain.cs
echo "menu 3" > osu.Game/Screens/Menu.cs
commit "release 2026.301.0" && g tag 2026.301.0
cd "$root/fork"
head="$(git rev-parse HEAD)"
run --live-version 2026.301.0 --apply --pr-body "$root/pr.md"
[ "$code" = 3 ] && [[ "$output" == *"merging the release failed"* ]] || fail "apply (conflict): exit $code: $output"
grep -q 'merging it conflicts' "$root/pr.md" || fail "the description does not say the merge conflicted"
[ "$(git rev-parse HEAD)" = "$head" ] && [ ! -e .git/MERGE_HEAD ] || fail "apply (conflict) left a merge behind"
expect osu.Game.Rulesets.Mania/Difficulty/Strain.cs "strain 3" "apply (conflict)"
expect osu.Game/Screens/Menu.cs "menu, patched" "apply (conflict) keeps the core patch"
expect docs/ortho/live-mania-version.txt "2026.301.0" "apply (conflict) records the release"
commit "match 2026.301.0"

# The same code under a newer release: the check says so, and --apply only records it.
cd "$root/upstream"
echo "menu 4" > osu.Game/Screens/Menu.cs
commit "release 2026.401.0" && g tag 2026.401.0
cd "$root/fork"
run --live-version 2026.401.0
[ "$code" = 0 ] && [[ "$output" == *"last checked: 2026.301.0"* ]] || fail "same code, newer release: exit $code: $output"
head="$(git rev-parse HEAD)"
run --live-version 2026.401.0 --apply --pr-body "$root/pr.md"
[ "$code" = 0 ] && [ "$(git rev-parse HEAD)" = "$head" ] || fail "apply (same code) merged or failed: exit $code: $output"
expect osu.Game/Screens/Menu.cs "menu, patched" "apply (same code)"
expect docs/ortho/live-mania-version.txt "2026.401.0" "apply (same code) records the release"
grep -q 'only records it' "$root/pr.md" || fail "the description does not say it only records the release"
commit "record 2026.401.0"

# A release with a scoring change, taken without merging (the workflow does this when the merge would bring workflow
# files its token cannot push): only the files change, and the description says the rest was not merged.
cd "$root/upstream"
echo "score 5" > osu.Game/Rulesets/Scoring/ScoreProcessor.cs
echo "other 5" > osu.Game/Other.cs
commit "release 2026.501.0" && g tag 2026.501.0
cd "$root/fork"
head="$(git rev-parse HEAD)"
run --live-version 2026.501.0 --apply --no-merge --pr-body "$root/pr.md"
[ "$code" = 3 ] && [ "$(git rev-parse HEAD)" = "$head" ] || fail "apply --no-merge merged or failed: exit $code: $output"
expect osu.Game/Rulesets/Scoring/ScoreProcessor.cs "score 5" "apply --no-merge"
expect osu.Game/Other.cs "other" "apply --no-merge leaves the rest of the release"
grep -q 'without' "$root/pr.md" && grep -q 'merging the rest of the release' "$root/pr.md" || fail "the description does not say the rest was not merged"
g reset -q --hard "$head"

# Differences reviewed and kept: --record only names the release.
run --live-version 2026.501.0 --record
[ "$code" = 3 ] || fail "record: exit $code: $output"
expect osu.Game/Rulesets/Scoring/ScoreProcessor.cs "score 1" "record leaves the files"
expect docs/ortho/live-mania-version.txt "2026.501.0" "record"

run --live-version not-a-version
[ "$code" = 2 ] || fail "a bad version: exit $code: $output"

echo "live-mania test passed"
