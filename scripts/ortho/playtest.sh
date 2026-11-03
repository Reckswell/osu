#!/bin/bash
# Plays charts with autoplay in a real window and saves gameplay screenshots, for both osu!mania (baseline)
# and the new ruleset, then builds side-by-side comparison images and a summary per run.
#
#   scripts/ortho/playtest.sh                                   # synthetic-7k and real-4k, both rulesets
#   scripts/ortho/playtest.sh --beatmap ~/osu/Songs/x/y.osu     # any .osu file (repeatable)
#   scripts/ortho/playtest.sh --times 5000,12000 --scroll-speed 24 --size 1920x1080 --play-to-end --no-baseline
#
# On Linux without a display it runs under Xvfb with Mesa's software renderer: frames are real, frame rates are not.
# Output: playtest-output/<timestamp>/ (git-ignored), with compare-*.png, the raw captures and summary-*.json.
set -euo pipefail
cd "$(git rev-parse --show-toplevel)"

beatmaps=()
extra=()
baseline=1
out=""
size="1280x720"

while [ $# -gt 0 ]; do
  case "$1" in
    --beatmap) beatmaps+=("$2"); shift ;;
    --times|--scroll-speed) extra+=("$1" "$2"); shift ;;
    --size) size="$2"; shift ;;
    --play-to-end) extra+=("$1") ;;
    --no-baseline) baseline=0 ;;
    --out) out="$2"; shift ;;
    *) echo "unknown option $1" >&2; exit 2 ;;
  esac
  shift
done
[ ${#beatmaps[@]} -eq 0 ] && beatmaps=(synthetic-7k real-4k)

rulesets=(maniaortho)
[ "$baseline" = 1 ] && rulesets=(mania maniaortho)

if [ -z "$out" ]; then
  out="playtest-output/$(date +%Y%m%d-%H%M%S)"
  mkdir -p playtest-output
  [ -e playtest-output/.gitignore ] || echo '*' > playtest-output/.gitignore
fi
mkdir -p "$out"

echo "== build"
dotnet build osu.Game.Rulesets.ManiaOrtho.Tests -c Debug -v q -nologo >/dev/null
dll="osu.Game.Rulesets.ManiaOrtho.Tests/bin/Debug/net10.0/osu.Game.Rulesets.ManiaOrtho.Tests.dll"

launcher=()
if [ "$(uname)" = "Linux" ] && [ -z "${DISPLAY:-}" ]; then
  w="${size%x*}"; h="${size#*x}"
  launcher=(xvfb-run -a -s "-screen 0 ${w}x${h}x24")
fi

failed=0
for beatmap in "${beatmaps[@]}"; do
  name="$(basename "$beatmap" .osu | sed 's/[^A-Za-z0-9._-]/_/g')"
  dir="$out/$name"
  for ruleset in "${rulesets[@]}"; do
    echo "== $beatmap ($ruleset)"
    if ! timeout 600 "${launcher[@]}" dotnet "$dll" playtest --beatmap "$beatmap" --ruleset "$ruleset" --out "$dir" --size "$size" "${extra[@]}" >"$dir-$ruleset.log" 2>&1; then
      echo "   FAILED, see $dir-$ruleset.log"; tail -5 "$dir-$ruleset.log"; failed=1
    fi
    grep -E "PLAYTEST (PASSED|FAILED)" "$dir-$ruleset.log" | sed 's/^/   /' || true
  done

  if [ "$baseline" = 1 ] && command -v montage >/dev/null 2>&1; then
    for ortho in "$dir"/maniaortho-*.png; do
      [ -e "$ortho" ] || continue
      suffix="${ortho#"$dir"/maniaortho-}"
      mania="$dir/mania-$suffix"
      [ -e "$mania" ] && montage "$mania" "$ortho" -tile 2x1 -geometry +4+0 -background '#222' "$out/compare-$name-$suffix"
    done
  fi
done

echo
echo "Output: $out"
ls "$out" | sed 's/^/  /'
exit $failed
