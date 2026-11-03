// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.ManiaOrtho.Configuration;
using osu.Game.Rulesets.ManiaOrtho.Highway;
using osu.Game.Rulesets.ManiaOrtho.Tests.Playtest;
using osu.Game.Rulesets.ManiaOrtho.UI;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.ManiaOrtho.Tests.Parity
{
    /// <summary>
    /// The core gameplay invariant: the new playfield is visual only, so the same inputs must give the same
    /// judgements, score and accuracy as osu!mania. Plays one humanised replay through both rulesets and compares.
    /// </summary>
    [HeadlessTest]
    public partial class TestSceneReplayParity : RateAdjustedBeatmapTestScene
    {
        private ReplayPlayer currentPlayer = null!;
        private GameplayClockContainer clock = null!;
        private readonly List<JudgementResult> results = new List<JudgementResult>();

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [TestCase(PlaytestBeatmaps.REAL_4K, 1)]
        [TestCase("synthetic-4k", 2)]
        [TestCase("synthetic-7k", 3)]
        public void TestJudgementParity(string source, int seed) => testParity(source, seed, HighwayNoteStyle.Skin);

        /// <summary>
        /// Native notes take over drawing mania's notes (their containers are proxied away); judging must not notice.
        /// </summary>
        [TestCase("synthetic-7k", 4)]
        public void TestJudgementParityWithNativeNotes(string source, int seed) => testParity(source, seed, HighwayNoteStyle.Highway);

        /// <summary>
        /// Hidden, Fade In and Cover wrap mania's note containers, which native notes read to fade the same way. Flashlight is
        /// replaced by one that draws on the highway.
        /// </summary>
        [TestCase("synthetic-7k", 5, "HD")]
        [TestCase("synthetic-7k", 6, "FI")]
        [TestCase("synthetic-7k", 7, "CO")]
        [TestCase("synthetic-7k", 8, "FL")]
        public void TestJudgementParityWithVisibilityMods(string source, int seed, string mod) => testParity(source, seed, HighwayNoteStyle.Highway, mod);

        private void testParity(string source, int seed, HighwayNoteStyle noteStyle, params string[] mods)
        {
            PlayRecord mania = null!;
            PlayRecord ortho = null!;

            AddAssert("game discovered the ruleset", () => rulesets.GetRuleset(ManiaOrthoRuleset.SHORT_NAME), () => Is.Not.Null);
            AddStep($"notes: {noteStyle}", () => ((ManiaOrthoRulesetConfigManager)RulesetConfigs.GetConfigFor(new ManiaOrthoRuleset()).AsNonNull())
                                               .Highway.SetValue(HighwaySetting.NoteStyle, noteStyle));

            play(new ManiaRuleset(), source, seed, mods, r => mania = r);
            play(new ManiaOrthoRuleset(), source, seed, mods, r => ortho = r);

            AddAssert("ortho run used the ortho playfield", () => ortho.DrawableRulesetType, () => Is.EqualTo(nameof(DrawableManiaOrthoRuleset)));
            AddAssert($"ortho run drew notes as {noteStyle}", () => ortho.NativeNotes, () => Is.EqualTo(noteStyle == HighwayNoteStyle.Highway));
            AddAssert("both runs played with the mods", () => new[] { mania.Mods, ortho.Mods }, () => Has.All.EquivalentTo(mods));
            AddAssert("replay produced a realistic mix of judgements", () => mania.Judgements.Select(j => j.Type).Distinct().Count(), () => Is.GreaterThanOrEqualTo(3));
            AddAssert("same judgement count", () => ortho.Judgements.Count, () => Is.EqualTo(mania.Judgements.Count));
            AddAssert("same judgement sequence", () => ortho.Judgements, () => Is.EqualTo(mania.Judgements));
            AddAssert("same total score", () => ortho.TotalScore, () => Is.EqualTo(mania.TotalScore));
            AddAssert("same accuracy", () => ortho.Accuracy, () => Is.EqualTo(mania.Accuracy));
        }

        private void play(Ruleset ruleset, string source, int seed, string[] mods, System.Action<PlayRecord> store)
        {
            AddStep($"[{ruleset.ShortName}] set beatmap", () => Beatmap.Value = CreateWorkingBeatmap(PlaytestBeatmaps.Load(source)));
            AddStep($"[{ruleset.ShortName}] set ruleset", () => Ruleset.Value = ruleset.RulesetInfo);
            AddStep($"[{ruleset.ShortName}] set mods", () => SelectedMods.Value = mods.Select(m => ruleset.CreateModFromAcronym(m).AsNonNull()).ToArray());
            AddStep($"[{ruleset.ShortName}] push replay player", () =>
            {
                var replay = HumanisedReplayGenerator.Generate(PlaytestBeatmaps.Load(source), seed);
                var player = new ReplayPlayer(new Score { Replay = replay, ScoreInfo = new ScoreInfo { Ruleset = ruleset.RulesetInfo } });

                player.OnLoadComplete += _ => player.GameplayState.ScoreProcessor.NewJudgement += result =>
                {
                    if (currentPlayer == player)
                        results.Add(result);
                };

                results.Clear();
                LoadScreen(currentPlayer = player);
            });

            AddUntilStep($"[{ruleset.ShortName}] player loaded", () => currentPlayer.IsCurrentScreen());
            AddStep($"[{ruleset.ShortName}] skip intro", () =>
            {
                clock = currentPlayer.ChildrenOfType<GameplayClockContainer>().Single();

                if (clock.CurrentTime < 0)
                    currentPlayer.Seek(0);
            });

            // Until-steps time out after 10s of game time, so wait for long maps in 8s checkpoints.
            double end = PlaytestBeatmaps.Load(source).HitObjects.Max(h => h.GetEndTime());

            for (double checkpoint = 8000; checkpoint < end; checkpoint += 8000)
            {
                double target = checkpoint;
                AddUntilStep($"[{ruleset.ShortName}] reach {target / 1000:0}s", () => clock.CurrentTime >= target);
            }

            AddUntilStep($"[{ruleset.ShortName}] play to completion", () => currentPlayer.GameplayState.HasCompleted);

            AddStep($"[{ruleset.ShortName}] record", () =>
            {
                var processor = currentPlayer.GameplayState.ScoreProcessor;

                store(new PlayRecord(
                    results.Select(r => new JudgementRecord(r.HitObject.StartTime, r.HitObject.GetType().Name, r.Type, System.Math.Round(r.TimeOffset, 3))).ToList(),
                    processor.TotalScore.Value,
                    processor.Accuracy.Value,
                    currentPlayer.ChildrenOfType<osu.Game.Rulesets.UI.DrawableRuleset>().Single().GetType().Name,
                    currentPlayer.ChildrenOfType<HighwayPlayfieldAdjustmentContainer>().SingleOrDefault()?.NativeNotes == true,
                    currentPlayer.GameplayState.Mods.Select(m => m.Acronym).ToArray()));
            });

            AddStep($"[{ruleset.ShortName}] exit player", () => currentPlayer.Exit());
            AddUntilStep($"[{ruleset.ShortName}] player exited", () => !currentPlayer.IsCurrentScreen());
            AddStep($"[{ruleset.ShortName}] dispose player", () => currentPlayer.Dispose());
        }

        private record JudgementRecord(double ObjectTime, string ObjectType, HitResult Type, double Offset);

        private record PlayRecord(List<JudgementRecord> Judgements, long TotalScore, double Accuracy, string DrawableRulesetType, bool NativeNotes, string[] Mods);
    }
}
