// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Online.API;

namespace ManiaOrtho.ScoreServerContract
{
    /// <summary>
    /// The JSON part (<c>play</c>) of an upload from the game to the score server. Compiled into both the game's ruleset and
    /// the server, so neither can rename a field alone; <c>play-upload.example.json</c> documents it, and both test suites
    /// check the example still reads as this type.
    /// </summary>
    public class PlayUpload
    {
        /// <summary>
        /// The ruleset the play was made in (<c>maniaortho</c>). Plays are rated as osu!mania.
        /// </summary>
        [JsonProperty("client")]
        public string Client { get; set; } = string.Empty;

        [JsonProperty("client_version")]
        public string? ClientVersion { get; set; }

        /// <summary>
        /// The game's own id for the play. Sending the same play twice (a retry after a lost answer) stores it once.
        /// </summary>
        [JsonProperty("score_id")]
        public string? ScoreId { get; set; }

        [JsonProperty("played_at")]
        public DateTimeOffset PlayedAt { get; set; }

        [JsonProperty("passed")]
        public bool Passed { get; set; }

        [JsonProperty("rank")]
        public string? Rank { get; set; }

        [JsonProperty("beatmap")]
        public BeatmapUpload Beatmap { get; set; } = new BeatmapUpload();

        [JsonProperty("mods")]
        public List<APIMod> Mods { get; set; } = new List<APIMod>();

        /// <summary>
        /// Judgement counts by <see cref="osu.Game.Rulesets.Scoring.HitResult"/> name (<c>Perfect</c>, <c>Great</c>, ...).
        /// </summary>
        [JsonProperty("statistics")]
        public Dictionary<string, int> Statistics { get; set; } = new Dictionary<string, int>();

        [JsonProperty("maximum_statistics")]
        public Dictionary<string, int> MaximumStatistics { get; set; } = new Dictionary<string, int>();

        [JsonProperty("max_combo")]
        public int MaxCombo { get; set; }

        /// <summary>
        /// 0 to 1.
        /// </summary>
        [JsonProperty("accuracy")]
        public double Accuracy { get; set; }

        [JsonProperty("total_score")]
        public long TotalScore { get; set; }

        /// <summary>
        /// The pp the game showed, from its own copy of the calculators. Kept to show when the game and osu!'s live version differ.
        /// </summary>
        [JsonProperty("client_pp")]
        public double? ClientPp { get; set; }

        [JsonProperty("client_stars")]
        public double? ClientStars { get; set; }

        [JsonProperty("client_difficulty_version")]
        public int? ClientDifficultyVersion { get; set; }

        /// <summary>
        /// The highway settings the play was made with (preset, camera, wall, ...), as the game's play log writes them.
        /// Stored for the player's own records; they never change the rating.
        /// </summary>
        [JsonProperty("settings")]
        public Dictionary<string, string?> Settings { get; set; } = new Dictionary<string, string?>();
    }

    public class BeatmapUpload
    {
        /// <summary>
        /// MD5 of the <c>.osu</c> file, lower-case hex. The uploaded file must match it.
        /// </summary>
        [JsonProperty("md5")]
        public string Md5 { get; set; } = string.Empty;

        [JsonProperty("online_id")]
        public int OnlineId { get; set; }

        [JsonProperty("set_online_id")]
        public int SetOnlineId { get; set; }

        /// <summary>
        /// The beatmap's status on osu! as the game knows it (<see cref="osu.Game.Beatmaps.BeatmapOnlineStatus"/> name).
        /// Only <c>Ranked</c> and <c>Approved</c> maps award pp, as on osu!.
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("artist")]
        public string Artist { get; set; } = string.Empty;

        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("version")]
        public string Version { get; set; } = string.Empty;

        [JsonProperty("creator")]
        public string Creator { get; set; } = string.Empty;

        [JsonProperty("keys")]
        public int Keys { get; set; }
    }

    /// <summary>
    /// The server's answer to an upload. Field names are pinned by <c>ortho-server/contract/play-result.example.json</c>.
    /// </summary>
    public class PlayResult
    {
        [JsonProperty("play_id")]
        public long PlayId { get; set; }

        /// <summary>
        /// The play had been received before (a retry); nothing was added.
        /// </summary>
        [JsonProperty("already_stored")]
        public bool AlreadyStored { get; set; }

        [JsonProperty("pp")]
        public double? Pp { get; set; }

        [JsonProperty("stars")]
        public double? Stars { get; set; }

        [JsonProperty("awards_pp")]
        public bool AwardsPp { get; set; }

        [JsonProperty("reason")]
        public string? Reason { get; set; }

        /// <summary>
        /// Where the play stands among the profile's best plays (1 is the best), if it counts.
        /// </summary>
        [JsonProperty("top_position")]
        public int? TopPosition { get; set; }

        [JsonProperty("weight")]
        public double? Weight { get; set; }

        [JsonProperty("total_pp")]
        public double TotalPp { get; set; }

        [JsonProperty("total_pp_before")]
        public double TotalPpBefore { get; set; }

        [JsonProperty("accuracy")]
        public double Accuracy { get; set; }

        [JsonProperty("calculator_version")]
        public string CalculatorVersion { get; set; } = string.Empty;

        /// <summary>
        /// The calculator version osu! runs live, when the server has checked and it differs from <see cref="CalculatorVersion"/>.
        /// </summary>
        [JsonProperty("live_calculator_version")]
        public string? LiveCalculatorVersion { get; set; }

        [JsonProperty("client_pp")]
        public double? ClientPp { get; set; }
    }

    public static class ScoreServerProtocol
    {
        /// <summary>
        /// A header every <c>POST</c> to the server must carry (any value). A web page cannot add it without the browser first
        /// asking the server (a CORS preflight, which the server never allows), so a page open in a browser cannot post plays
        /// or start a recalculation.
        /// </summary>
        public const string CLIENT_HEADER = "X-ManiaOrtho";
    }
}
