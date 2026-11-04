// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using ManiaOrtho.ScoreServerContract;
using OrthoServer.Storage;

namespace OrthoServer.Scoring
{
    /// <summary>
    /// Takes plays in, rates them and keeps the profile. One at a time: plays arrive one by one from one player.
    /// </summary>
    public class PlayService
    {
        public PlayStore Store { get; }

        private readonly object gate = new object();

        public PlayService(PlayStore store)
        {
            Store = store;
        }

        /// <summary>
        /// Rates and stores an uploaded play. A play already received (same <see cref="PlayUpload.ScoreId"/>) is not stored
        /// again; its stored result is returned.
        /// </summary>
        /// <exception cref="InvalidDataException">The upload is not a play this server can rate.</exception>
        public PlayResult Submit(PlayUpload upload, byte[] osuFile, byte[]? replay)
        {
            if (!string.Equals(upload.Client, "maniaortho", StringComparison.Ordinal))
                throw new InvalidDataException($"Plays from \"{upload.Client}\" are not accepted; only maniaortho plays are.");

            string md5 = PlayCalculator.Md5(osuFile);

            if (!string.Equals(md5, upload.Beatmap.Md5, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"The beatmap file does not match the play (its checksum is {md5}, the play's is {upload.Beatmap.Md5}).");

            lock (gate)
            {
                if (!string.IsNullOrEmpty(upload.ScoreId) && Store.FindByClientScoreId(upload.ScoreId) is PlayRecord existing)
                    return result(existing, ProfileCalculator.Calculate(Store.All()), alreadyStored: true);

                var before = ProfileCalculator.Calculate(Store.All());

                var play = new PlayRecord
                {
                    ClientScoreId = string.IsNullOrEmpty(upload.ScoreId) ? null : upload.ScoreId,
                    PlayedAt = upload.PlayedAt,
                    ReceivedAt = DateTimeOffset.Now,
                    Passed = upload.Passed,
                    Rank = upload.Rank,
                    BeatmapMd5 = md5,
                    BeatmapId = upload.Beatmap.OnlineId,
                    BeatmapSetId = upload.Beatmap.SetOnlineId,
                    Status = upload.Beatmap.Status,
                    Artist = upload.Beatmap.Artist,
                    Title = upload.Beatmap.Title,
                    Version = upload.Beatmap.Version,
                    Creator = upload.Beatmap.Creator,
                    Keys = upload.Beatmap.Keys,
                    Mods = upload.Mods,
                    Statistics = upload.Statistics,
                    MaximumStatistics = upload.MaximumStatistics,
                    MaxCombo = upload.MaxCombo,
                    Accuracy = upload.Accuracy,
                    TotalScore = upload.TotalScore,
                    ClientPp = upload.ClientPp,
                    ClientStars = upload.ClientStars,
                    ClientDifficultyVersion = upload.ClientDifficultyVersion,
                    Settings = upload.Settings,
                };

                PlayCalculator.Rate(play, osuFile);

                Store.SaveBeatmap(md5, osuFile);
                Store.Insert(play);

                if (replay != null)
                {
                    File.WriteAllBytes(Store.ReplayPath(play.Id), replay);
                    play.ReplayFile = Path.GetFileName(Store.ReplayPath(play.Id));
                    Store.SetReplayFile(play.Id, play.ReplayFile);
                }

                var after = ProfileCalculator.Calculate(Store.All());

                return result(play, after, alreadyStored: false, totalPpBefore: before.TotalPp);
            }
        }

        public Profile Profile()
        {
            lock (gate)
                return ProfileCalculator.Calculate(Store.All());
        }

        /// <summary>
        /// Rates every stored play again with this server's calculators.
        /// </summary>
        /// <param name="onlyOutdated">Only plays rated by another calculator version.</param>
        /// <param name="progress">Called with (done, total) after each play.</param>
        /// <returns>How many plays were rated again, and how many could not be (their map file is missing or unreadable).</returns>
        public (int rated, int failed) Recalculate(bool onlyOutdated, Action<int, int>? progress = null)
        {
            lock (gate)
            {
                var plays = Store.All().Where(p => !onlyOutdated || p.CalculatorVersion != PlayCalculator.CalculatorVersion).ToList();
                int rated = 0, failed = 0;

                for (int i = 0; i < plays.Count; i++)
                {
                    var play = plays[i];
                    byte[]? osuFile = Store.ReadBeatmap(play.BeatmapMd5);

                    try
                    {
                        if (osuFile == null)
                            throw new FileNotFoundException("The map file is missing.", Store.BeatmapPath(play.BeatmapMd5));

                        PlayCalculator.Rate(play, osuFile);
                        Store.UpdateRating(play);
                        rated++;
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine($"Play {play.Id} ({play.Artist} - {play.Title} [{play.Version}]) could not be rated again: {e.Message}");
                        failed++;
                    }

                    progress?.Invoke(i + 1, plays.Count);
                }

                return (rated, failed);
            }
        }

        private static PlayResult result(PlayRecord play, Profile profile, bool alreadyStored, double? totalPpBefore = null)
        {
            int index = -1;

            for (int i = 0; i < profile.Top.Count; i++)
            {
                if (profile.Top[i].Play.Id == play.Id)
                {
                    index = i;
                    break;
                }
            }

            return new PlayResult
            {
                PlayId = play.Id,
                AlreadyStored = alreadyStored,
                Pp = play.Pp,
                Stars = play.Stars,
                AwardsPp = play.AwardsPp,
                Reason = play.Reason,
                TopPosition = index >= 0 ? index + 1 : null,
                Weight = index >= 0 ? profile.Top[index].Weight : null,
                TotalPp = profile.TotalPp,
                TotalPpBefore = totalPpBefore ?? profile.TotalPp,
                Accuracy = profile.Accuracy,
                CalculatorVersion = play.CalculatorVersion,
                ClientPp = play.ClientPp,
            };
        }
    }
}
