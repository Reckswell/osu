// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Replays;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.ManiaOrtho.Configuration;
using osu.Game.Rulesets.ManiaOrtho.Highway;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.ManiaOrtho.Scoring
{
    /// <summary>
    /// Adds the play to <see cref="PlayLog"/> once it is passed or failed, and saves its replay as an osu!mania replay
    /// (<c>.osr</c>). Quits, replays and autoplay are not logged. It only reads the score processor, the replay and the
    /// settings, so it cannot change judging or scoring (invariant 1).
    /// </summary>
    public partial class PlayLogRecorder : Component
    {
        private readonly IBindable<bool> hasReplayLoaded;
        private readonly ManiaOrthoRulesetConfigManager? config;

        [Resolved]
        private GameplayState? gameplayState { get; set; }

        [Resolved]
        private BeatmapDifficultyCache? difficultyCache { get; set; }

        [Resolved]
        private Storage storage { get; set; } = null!;

        private bool recorded;

        /// <summary>
        /// The write of this play's row and replay, once it has started.
        /// </summary>
        public Task? Writing { get; private set; }

        public PlayLogRecorder(IBindable<bool> hasReplayLoaded, ManiaOrthoRulesetConfigManager? config)
        {
            this.hasReplayLoaded = hasReplayLoaded;
            this.config = config;
        }

        protected override void Update()
        {
            base.Update();

            if (recorded || gameplayState == null || !(gameplayState.HasPassed || gameplayState.HasFailed))
                return;

            recorded = true;

            if (hasReplayLoaded.Value)
                return;

            try
            {
                Writing = record(gameplayState, gameplayState.HasPassed);
            }
            catch (Exception e)
            {
                // Never take the game down at the end of a play over the log.
                Logger.Error(e, $"This play could not be added to {PlayLog.DIRECTORY}/{PlayLog.FILENAME}");
            }
        }

        /// <summary>
        /// Captures the play on the update thread, as it is at its pass or fail, then rates it and writes it in the background.
        /// </summary>
        private Task record(GameplayState state, bool passed)
        {
            var beatmap = state.Score.ScoreInfo.BeatmapInfo ?? state.Beatmap.BeatmapInfo;
            var ruleset = state.Ruleset;
            var score = new ScoreInfo(beatmap, ruleset.RulesetInfo)
            {
                User = state.Score.ScoreInfo.User,
                Mods = state.Mods.ToArray(),
                Date = DateTimeOffset.Now,
            };

            state.ScoreProcessor.PopulateScore(score);

            // Populating shares the processor's list of hit events; the copies below must not.
            score.HitEvents = new List<HitEvent>(score.HitEvents);

            if (!passed)
            {
                score.Rank = ScoreRank.F;
                score.Passed = false;
            }

            // The recorder keeps adding frames until the player leaves.
            var replay = new Replay { Frames = state.Score.Replay.Frames.ToList() };

            string?[] row = new string?[PlayLog.COLUMNS.Length];

            set(row, "time", score.Date.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            set(row, "result", passed ? "pass" : "fail");
            set(row, "rank", score.Rank.ToString());
            set(row, "artist", beatmap.Metadata.Artist);
            set(row, "title", beatmap.Metadata.Title);
            set(row, "difficulty", beatmap.DifficultyName);
            set(row, "beatmap_md5", beatmap.MD5Hash);
            set(row, "keys", (state.Beatmap as ManiaBeatmap)?.TotalColumns.ToString(CultureInfo.InvariantCulture));
            set(row, "mods", string.Join(" ", score.Mods.Select(m => m.Acronym)));
            set(row, "score", score.TotalScore.ToString(CultureInfo.InvariantCulture));
            // More digits than osu! shows (it rounds down to two), so a 99.996% play does not read as 100%.
            set(row, "accuracy", (score.Accuracy * 100).ToString("0.0000", CultureInfo.InvariantCulture));
            set(row, "max_combo", score.MaxCombo.ToString(CultureInfo.InvariantCulture));
            set(row, "perfect", count(score, HitResult.Perfect));
            set(row, "great", count(score, HitResult.Great));
            set(row, "good", count(score, HitResult.Good));
            set(row, "ok", count(score, HitResult.Ok));
            set(row, "meh", count(score, HitResult.Meh));
            set(row, "miss", count(score, HitResult.Miss));
            set(row, "difficulty_version", ruleset.CreateDifficultyCalculator(new FlatWorkingBeatmap(state.Beatmap)).Version.ToString(CultureInfo.InvariantCulture));
            set(row, "mods_json", JsonConvert.SerializeObject(score.APIMods));
            set(row, "score_id", state.Score.ScoreInfo.ID.ToString());

            if (config != null)
            {
                var highway = config.Highway;

                set(row, "preset", highway.Get<HighwayPreset>(HighwaySetting.Preset).ToString());
                set(row, "camera", highway.Get<HighwayCameraMode>(HighwaySetting.CameraMode).ToString());
                set(row, "wall", highway.Get<float>(HighwaySetting.Wall).ToString("0.00", CultureInfo.InvariantCulture));
                set(row, "lift", highway.Get<float>(HighwaySetting.JudgementLineHeight).ToString("0.000", CultureInfo.InvariantCulture));
                set(row, "wall_keeps_visible_time", highway.Get<bool>(HighwaySetting.WallKeepsVisibleTime) ? "yes" : "no");
                set(row, "scroll_speed", config.Get<double>(ManiaRulesetSetting.ScrollSpeed).ToString(CultureInfo.InvariantCulture));
            }

            // Started here, as the results screen does: the lookup copies the play's mods, whose bindables belong to this thread.
            var difficulty = difficultyCache?.GetDifficultyAsync(beatmap, ruleset.RulesetInfo, score.Mods) ?? Task.FromResult<StarDifficulty?>(null);
            var log = storage.GetStorageForDirectory(PlayLog.DIRECTORY);
            var playable = state.Beatmap;

            return Task.Run(async () =>
            {
                try
                {
                    set(row, "replay", saveReplay(log, score, replay, playable));
                }
                catch (Exception e)
                {
                    Logger.Error(e, "This play's replay could not be saved");
                }

                try
                {
                    var rating = await difficulty.ConfigureAwait(false);

                    if (rating?.DifficultyAttributes != null)
                    {
                        set(row, "stars", rating.Value.Stars.ToString("0.0000", CultureInfo.InvariantCulture));
                        set(row, "pp", ruleset.CreatePerformanceCalculator()?.Calculate(score, rating.Value.DifficultyAttributes).Total.ToString("0.00", CultureInfo.InvariantCulture));
                    }

                    PlayLog.Append(log, row);
                }
                catch (Exception e)
                {
                    Logger.Error(e, $"This play could not be added to {PlayLog.DIRECTORY}/{PlayLog.FILENAME} (is it open in another program?)");
                }
            });
        }

        /// <summary>
        /// Saves the replay as an osu!mania replay: the judgements are mania's, and osu! can open no other kind for this
        /// ruleset. It plays back in osu!mania's flat stage.
        /// </summary>
        /// <returns>Its path in the log's folder.</returns>
        private static string saveReplay(Storage log, ScoreInfo score, Replay replay, IBeatmap playable)
        {
            var mania = score.DeepClone();
            mania.Ruleset = ManiaOrthoRuleset.MANIA.RulesetInfo;

            var replays = log.GetStorageForDirectory(PlayLog.REPLAY_DIRECTORY);
            string name = $"{score.Date.ToLocalTime().ToString("yyyy-MM-dd HH-mm-ss", CultureInfo.InvariantCulture)} {score.BeatmapInfo?.GetDisplayTitle()}".GetValidFilename();
            string filename = name + ".osr";

            for (int i = 2; replays.Exists(filename); i++)
                filename = $"{name} ({i}).osr";

            using (var stream = new FileStream(replays.GetFullPath(filename, true), FileMode.CreateNew, FileAccess.Write))
                new LegacyScoreEncoder(new Score { ScoreInfo = mania, Replay = replay }, playable).Encode(stream);

            return $"{PlayLog.REPLAY_DIRECTORY}/{filename}";
        }

        private static string count(ScoreInfo score, HitResult result) => score.Statistics.GetValueOrDefault(result).ToString(CultureInfo.InvariantCulture);

        private static void set(string?[] row, string column, string? value) => row[Array.IndexOf(PlayLog.COLUMNS, column)] = value;
    }
}
