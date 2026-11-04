#!/bin/bash
# Plays charts with autoplay in a real window and saves gameplay screenshots, for both osu!mania (baseline)
# and the new ruleset, then builds side-by-side comparison images and a summary per run.
#
#   scripts/ortho/playtest.sh                                   # the default set (below), both rulesets
#   scripts/ortho/playtest.sh --beatmap ~/osu/Songs/x/y.osu     # any .osu file (repeatable)
#   scripts/ortho/playtest.sh --times 5000,12000 --scroll-speed 24 --size 1920x1080 --play-to-end --no-baseline
#   scripts/ortho/playtest.sh --preset Voltex --wall 0.3   (maniaortho highway settings; mania ignores them)
#   scripts/ortho/playtest.sh --wall 0.4 --speed-lock      (the wall keeps the note speed instead of the visible time)
#   scripts/ortho/playtest.sh --set JudgementLineHeight=0.3,TrackWidth=0.7   (any highway setting, after the preset)
#   scripts/ortho/playtest.sh --skin classic                (built-in skin: argon, argon-pro, triangles, classic)
#   scripts/ortho/playtest.sh --skin "path/to/Skins/My Skin" --port   (a stable skin folder, drawn by the skin porter)
#   scripts/ortho/playtest.sh --notes skin                  (maniaortho notes: highway, the default, or skin)
#   scripts/ortho/playtest.sh --mods HD                     (mod acronyms, comma separated, besides autoplay and no fail)
#
# The default set: synthetic-7k and real-4k at speed 29 (Alpha's: 405 ms visible on lazer's skins), and synthetic-7k at
# speed 14 (840 ms), where Chunithm and Sekai players' notes move at the line (research/game-sekai.md K3). Without
# --scroll-speed every run uses 29: the harness keeps its settings between runs, so an unset speed would be whatever an
# earlier run left. Each run's summary records the real visible time.
#
# On Linux without a display it runs under Xvfb with Mesa's software renderer: frames are real, frame rates are not.
# Output: playtest-output/<timestamp>/ (git-ignored), with compare-*.png, the raw captures and summary-*.json.
set -euo pipefail
cd "$(git rev-parse --show-toplevel)"

beatmaps=()
extra=()
speed=""
baseline=1
out=""
size="1280x720"

while [ $# -gt 0 ]; do
  case "$1" in
    --beatmap) beatmaps+=("$2"); shift ;;
    --scroll-speed) speed="$2"; shift ;;
    --times|--preset|--wall|--skin|--notes|--mods|--set) extra+=("$1" "$2"); shift ;;
    --size) size="$2"; shift ;;
    --play-to-end|--speed-lock|--port) extra+=("$1") ;;
    --no-baseline) baseline=0 ;;
    --out) out="$2"; shift ;;
    *) echo "unknown option $1" >&2; exit 2 ;;
  esac
  shift
done
# Runs as "beatmap|speed".
runs=()
if [ ${#beatmaps[@]} -eq 0 ]; then
  runs=("synthetic-7k|${speed:-29}" "real-4k|${speed:-29}")
  [ -z "$speed" ] && runs+=("synthetic-7k|14")
else
  for beatmap in "${beatmaps[@]}"; do runs+=("$beatmap|${speed:-29}"); done
fi

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
for run in "${runs[@]}"; do
  beatmap="${run%|*}"
  run_speed="${run##*|}"
  name="$(basename "$beatmap" .osu | sed 's/[^A-Za-z0-9._-]/_/g')-speed$run_speed"
  dir="$out/$name"
  for ruleset in "${rulesets[@]}"; do
    echo "== $beatmap at speed $run_speed ($ruleset)"
    if ! timeout 600 "${launcher[@]}" dotnet "$dll" playtest --beatmap "$beatmap" --ruleset "$ruleset" --out "$dir" --size "$size" --scroll-speed "$run_speed" "${extra[@]}" >"$dir-$ruleset.log" 2>&1; then
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
