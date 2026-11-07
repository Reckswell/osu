#!/bin/bash
# Is this fork's osu!mania judgement, scoring and difficulty code the code osu!'s servers rate plays with? (CLAUDE.md
# invariant 1; BACKLOG M4.) osu!'s pp and star ratings come from osu-queue-score-statistics, which pins a release of
# ppy.osu.Game.Rulesets.Mania. This compares the files that decide judgements, score, accuracy, star rating and pp
# between that release of ppy/osu and HEAD. docs/ortho/live-mania-version.txt names the release last checked; the
# daily workflow (.github/workflows/ortho-live-mania.yml) opens a pull request when osu!'s servers move to another.
#
#   scripts/ortho/live-mania.sh                    # exit 0: the same code; 3: different (the files are listed)
#   scripts/ortho/live-mania.sh --live-version V   # as if osu!'s servers used release V (a simulated bump)
#   scripts/ortho/live-mania.sh --apply            # bring this branch to the release (below) and record it
#   scripts/ortho/live-mania.sh --apply --no-merge # the same, taking only the files, never merging
#   scripts/ortho/live-mania.sh --record           # only record the release as checked (differences reviewed and kept)
#   scripts/ortho/live-mania.sh --pr-body FILE     # write a pull request description of what was done to FILE
#   scripts/ortho/live-mania.sh --upstream URL     # where releases come from (default https://github.com/ppy/osu)
#
# --apply merges the release when this branch lacks it (a merge commit), then stages those files as the release's
# wherever they still differ (changes made upstream after the release, which osu! has not deployed, or this fork's
# own). A merge that conflicts is aborted and only the files are taken. Both --apply and --record stage the release in
# docs/ortho/live-mania-version.txt; nothing else is committed.
set -euo pipefail
cd "$(git rev-parse --show-toplevel)"

LIVE_CSPROJ=https://raw.githubusercontent.com/ppy/osu-queue-score-statistics/master/osu.Server.Queues.ScoreStatisticsProcessor/osu.Server.Queues.ScoreStatisticsProcessor.csproj
PIN=docs/ortho/live-mania-version.txt

# What decides the judgements, score, accuracy, star rating and pp of an osu!mania play: mania's own rules (with the
# order notes are judged in), and the shared code they build on: beatmap decoding and conversion, timing, hit objects,
# mods and their rates, judgements, scoring and health, hit window ranges and the difficulty calculators. Files are
# named one by one where their folder holds other things: osu.Game/Beatmaps also holds the core patch
# BeatmapInfoExtensions.cs (docs/ortho/README.md#core-patches), which --apply must never revert.
PATHS=(
  osu.Game.Rulesets.Mania/ManiaRuleset.cs
  osu.Game.Rulesets.Mania/Beatmaps
  osu.Game.Rulesets.Mania/Difficulty
  osu.Game.Rulesets.Mania/Judgements
  osu.Game.Rulesets.Mania/MathUtils
  osu.Game.Rulesets.Mania/Mods
  osu.Game.Rulesets.Mania/Objects
  osu.Game.Rulesets.Mania/Scoring
  osu.Game.Rulesets.Mania/UI/Column.cs
  osu.Game.Rulesets.Mania/UI/OrderedHitPolicy.cs
  osu.Game/Beatmaps/BeatmapConverter.cs
  osu.Game/Beatmaps/BeatmapDifficulty.cs
  osu.Game/Beatmaps/BeatmapProcessor.cs
  osu.Game/Beatmaps/ControlPoints
  osu.Game/Beatmaps/Formats
  osu.Game/Beatmaps/IBeatmapDifficultyInfo.cs
  osu.Game/Beatmaps/WorkingBeatmap.cs
  osu.Game/Rulesets/Difficulty
  osu.Game/Rulesets/Judgements
  osu.Game/Rulesets/Mods
  osu.Game/Rulesets/Objects
  osu.Game/Rulesets/Scoring
  osu.Game/Utils/LegacyRandom.cs
  osu.Game/Utils/ModUtils.cs
)

live=""
apply=0
merge=1
record=0
pr_body=""
upstream=https://github.com/ppy/osu
while [ $# -gt 0 ]; do
  case "$1" in
    --live-version) live="$2"; shift ;;
    --apply) apply=1 ;;
    --no-merge) merge=0 ;;
    --record) record=1 ;;
    --pr-body) pr_body="$2"; shift ;;
    --upstream) upstream="$2"; shift ;;
    *) echo "unknown option $1" >&2; exit 2 ;;
  esac
  shift
done

if [ -z "$live" ]; then
  live="$(curl -fsSL "$LIVE_CSPROJ" | sed -n 's/.*Include="ppy\.osu\.Game\.Rulesets\.Mania" Version="\([^"]*\)".*/\1/p' | head -1)"
  [ -n "$live" ] || { echo "could not read the osu!mania release from $LIVE_CSPROJ" >&2; exit 1; }
fi
# Release tags look like 2026.1026.0 (the workflow also names a branch after it).
[[ "$live" =~ ^[0-9]{4}\.[0-9]{1,4}\.[0-9]{1,3}(-[a-z]+)?$ ]] || { echo "\"$live\" is not a release version" >&2; exit 2; }
pinned="$(tr -d '[:space:]' < "$PIN" 2>/dev/null || true)"

ref="refs/ortho-live/$live"
# A shallow clone fetches one commit, enough to compare files. A full clone fetches the release with its history, so
# its commit (usually an ancestor of HEAD) is not made a shallow root that cuts the history short.
depth=()
[ "$(git rev-parse --is-shallow-repository)" = true ] && depth=(--depth=1)
git fetch -q --no-tags "${depth[@]}" "$upstream" "+refs/tags/$live:$ref"

# Whether this branch has the release: "yes" (differences are changes made after it), "no" (it lacks changes the
# release has) or "unknown" (a shallow clone whose history does not reach it).
if git merge-base --is-ancestor "$ref" HEAD 2>/dev/null; then
  has_release=yes
elif [ "$(git rev-parse --is-shallow-repository)" = true ]; then
  has_release=unknown
else
  has_release=no
fi

# The compared files that differ, as "status<TAB>path" from the release to HEAD: A only here, D only in the release.
differing=()
read_differing() {
  local listing
  listing="$(git diff --no-renames --name-status "$ref" HEAD -- "${PATHS[@]}")"
  differing=()
  [ -z "$listing" ] || mapfile -t differing <<< "$listing"
}
read_differing
found=("${differing[@]}")

echo "osu!'s servers: osu!mania $live (last checked: ${pinned:-none}); this branch has that release: $has_release"
if [ ${#found[@]} -eq 0 ]; then
  echo "same code"
else
  echo "different in ${#found[@]} file(s):"
  printf '  %s\n' "${found[@]}"
fi
if [ -n "${GITHUB_OUTPUT:-}" ]; then
  { echo "live=$live"; echo "pinned=$pinned"; echo "differs=$([ ${#found[@]} -gt 0 ] && echo true || echo false)"; } >> "$GITHUB_OUTPUT"
fi

# What --apply did: merged, conflict (merge aborted, files taken), files (files taken, no merge tried) or none.
outcome=none
if [ "$apply" = 1 ] && [ ${#found[@]} -gt 0 ]; then
  outcome=files
  if [ "$merge" = 1 ] && [ "$has_release" = no ]; then
    if git merge -q --no-edit -m "Merge ppy/osu $live, the osu!mania release osu!'s servers use" "$ref"; then
      outcome=merged
      echo "merged the release"
    else
      # Only a merge that started can be aborted; one refused outright (a file in the way) left nothing to undo.
      git rev-parse -q --verify MERGE_HEAD > /dev/null && git merge --abort
      outcome=conflict
      echo "merging the release failed: taking only the files"
    fi
    read_differing
  fi

  for entry in "${differing[@]}"; do
    status="${entry%%$'\t'*}"
    file="${entry#*$'\t'}"
    if [ "$status" = A ]; then
      git rm -q -- "$file"            # only here: the release has no such file
    else
      git checkout "$ref" -- "$file"  # changed here, or missing here
    fi
  done
fi

if [ "$apply" = 1 ] || [ "$record" = 1 ]; then
  mkdir -p "$(dirname "$PIN")"
  echo "$live" > "$PIN"
  git add -- "$PIN"
  echo "staged: $PIN names $live$([ "$outcome" = none ] || echo ", and the files are the release's")"
fi

if [ -n "$pr_body" ]; then
  {
    echo "osu!'s servers now rate osu!mania plays with \`ppy.osu.Game.Rulesets.Mania\` **$live**, pinned by"
    echo "[osu-queue-score-statistics](https://github.com/ppy/osu-queue-score-statistics) (last checked here: ${pinned:-none})."
    echo
    if [ ${#found[@]} -eq 0 ]; then
      echo "The files that decide judgements, score, accuracy, star rating and pp are already the same as that release's,"
      echo "so this only records it in \`$PIN\`."
    else
      case "$outcome" in
        merged)
          echo "This branch lacked changes in that release. This merges it, and makes any of the files below that"
          echo "decide judgements, score, accuracy, star rating or pp the release's where they still differed." ;;
        conflict)
          echo "This branch lacks changes in that release, and merging it conflicts, so this only makes the files below"
          echo "that decide judgements, score, accuracy, star rating or pp the release's. If it does not build, merge"
          echo "ppy/osu's \`$live\` tag by hand instead." ;;
        *)
          if [ "$has_release" = yes ]; then
            echo "These files, which decide judgements, score, accuracy, star rating or pp, differ from that release here:"
            echo "changes upstream made after it, which osu! has not deployed yet, or this fork's own. This makes them the"
            echo "release's, so maniaortho rates plays as osu! does (CLAUDE.md invariant 1)."
          else
            echo "These files, which decide judgements, score, accuracy, star rating or pp, differ from that release here."
            echo "This makes them the release's, so maniaortho rates plays as osu! does (CLAUDE.md invariant 1), without"
            echo "merging the rest of the release. If it does not build, merge ppy/osu's \`$live\` tag by hand instead."
          fi ;;
      esac
      echo
      echo "| File | Here, before this change |"
      echo "|---|---|"
      for entry in "${found[@]}"; do
        status="${entry%%$'\t'*}"
        file="${entry#*$'\t'}"
        case "$status" in
          A) what="only here (removed)" ;;
          D) what="missing (added)" ;;
          *) what="changed" ;;
        esac
        echo "| \`$file\` | $what |"
      done
      echo
      echo "A change that cannot affect a play's result (a replay format, say), or that the branch does not build"
      echo "without: take that file out of this pull request, keep the rest and \`$PIN\`, and note it in"
      echo "\`docs/ortho/DECISIONS.md\`. Closing this instead leaves the release unchecked."
    fi
    echo
    echo "Before merging, run \`scripts/ortho/check.sh --mania\` (maniaortho's parity tests and osu!mania's own suite)."
    echo
    echo "Opened by \`.github/workflows/ortho-live-mania.yml\` (\`scripts/ortho/live-mania.sh\`)."
  } > "$pr_body"
fi

[ ${#found[@]} -eq 0 ] && exit 0
exit 3
