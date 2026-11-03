// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.ManiaOrtho.Configuration;
using osu.Game.Rulesets.ManiaOrtho.Highway;
using osu.Game.Rulesets.ManiaOrtho.Tests.Playtest;
using osu.Game.Rulesets.ManiaOrtho.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.ManiaOrtho.Tests.Highway
{
    /// <summary>
    /// Autoplays a synthetic chart on the highway and checks what is drawn where (SPEC 1, 2 and 7).
    /// </summary>
    public partial class TestSceneHighwayPlayfield : PlayerTestScene
    {
        protected override bool Autoplay => true;

        protected override Ruleset CreatePlayerRuleset() => new ManiaOrthoRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => PlaytestBeatmaps.Load("synthetic-4k");

        private DrawableManiaOrthoRuleset drawableRuleset => (DrawableManiaOrthoRuleset)Player.DrawableRuleset;

        private HighwayPlayfieldAdjustmentContainer highway => drawableRuleset.Highway.AsNonNull();

        private HighwayJudgementLayer layer => highway.JudgementLayers.Single();

        private HighwayConfigManager highwayConfig => ((ManiaOrthoRulesetConfigManager)RulesetConfigs.GetConfigFor(CreatePlayerRuleset()).AsNonNull()).Highway;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            AddStep("Chunithm preset", () =>
            {
                highwayConfig.SetValue(HighwaySetting.Preset, HighwayPreset.Custom);
                highwayConfig.SetValue(HighwaySetting.Preset, HighwayPreset.Chunithm);
            });

            base.SetUpSteps();
        }

        [Test]
        public void TestJudgementsAreUprightOverTheTrack()
        {
            waitForJudgement();

            AddAssert("one display per stage", () => highway.JudgementLayers.Count == drawableRuleset.Playfield.Stages.Count);
            AddAssert("mania's own display is hidden", () => maniaJudgements().Alpha == 0);

            AddAssert("judgement is upright", () =>
            {
                var quad = layer.CurrentJudgement.AsNonNull().Piece.AsNonNull().ScreenSpaceDrawQuad;
                return Math.Abs(quad.TopLeft.Y - quad.TopRight.Y) < 0.5f && Math.Abs(quad.TopLeft.X - quad.BottomLeft.X) < 0.5f;
            });

            AddAssert("judgement is above the line and over the track", () =>
            {
                var projection = highway.Projection;
                Vector2 centre = highway.ToLocalSpace(layer.CurrentJudgement.AsNonNull().Piece.AsNonNull().ScreenSpaceDrawQuad.Centre);

                if (centre.Y >= projection.YAt(0) || centre.Y <= projection.YAt(1))
                    return false;

                float depth = projection.DepthAtY(centre.Y);
                return centre.X > projection.ToScreen(0, depth).X && centre.X < projection.ToScreen(1, depth).X;
            });

            AddAssert("judgement is shrunk by the perspective", () =>
            {
                // Mania's (hidden) judgement area is flat and unstretched vertically, so the height ratio is the highway's scale.
                float ratio = layer.JudgementArea.ScreenSpaceDrawQuad.Height / maniaJudgements().ScreenSpaceDrawQuad.Height;
                return ratio < 1 && ratio > highway.Projection.Layout.FarWidthRatio;
            });
        }

        [Test]
        public void TestFlatCameraPlacesJudgementsWhereManiaDoes()
        {
            AddStep("flat camera", () => highwayConfig.SetValue(HighwaySetting.CameraMode, HighwayCameraMode.Flat));
            waitForJudgement();

            AddAssert("same area as mania's", () =>
            {
                // Compare the layout areas rather than the pieces: mania's display is hidden, so its animations may not run.
                // Mania's area also spans the stage's 1px borders; skins centre judgements in it, so compare centre and height.
                var ours = layer.JudgementArea.ScreenSpaceDrawQuad;
                var mania = maniaJudgements().ScreenSpaceDrawQuad;

                return Vector2.Distance(ours.Centre, mania.Centre) < 1
                       && Math.Abs(ours.TopLeft.Y - mania.TopLeft.Y) < 1
                       && Math.Abs(ours.BottomLeft.Y - mania.BottomLeft.Y) < 1;
            });
        }

        [Test]
        public void TestRewindWhileJudgementShown()
        {
            waitForJudgement();

            // The playtest harness seeks back a few ms before every capture; judgements on screen must survive that.
            for (int i = 0; i < 5; i++)
            {
                AddStep("seek back 20ms", () => Player.GameplayClockContainer.Seek(Player.GameplayClockContainer.CurrentTime - 20));
                AddWaitStep("let it settle", 1);
            }

            AddStep("seek back 2s", () => Player.GameplayClockContainer.Seek(Player.GameplayClockContainer.CurrentTime - 2000));
            AddUntilStep("judgements shown again", () => layer.CurrentJudgement?.Result?.IsHit == true);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestWallStandsWhereNotesAppear(bool keepsVisibleTime)
        {
            AddAssert("no wall by default", () => !highway.Wall.IsPresent);

            AddStep("wall at 30%", () =>
            {
                highwayConfig.SetValue(HighwaySetting.WallKeepsVisibleTime, keepsVisibleTime);
                highwayConfig.SetValue(HighwaySetting.Wall, 0.3f);
            });

            AddUntilStep("wall shown", () => highway.Wall.IsPresent);

            AddAssert("lower edge at 70% of the track", () =>
            {
                var projection = highway.Projection;
                var wall = highway.ToLocalSpace(highway.Wall.ScreenSpaceDrawQuad.BottomLeft);
                var corner = projection.ToScreen(0, 0.7f);

                return Vector2.Distance(wall, corner) < 1;
            });

            AddAssert("covers the track beyond it", () =>
            {
                var projection = highway.Projection;
                var wall = highway.Wall.ScreenSpaceDrawQuad;

                return wall.Contains(highway.ToScreenSpace(projection.ToScreen(0.01f, 0.71f)))
                       && wall.Contains(highway.ToScreenSpace(projection.ToScreen(0.99f, 0.99f)));
            });

            AddAssert("notes enter at the wall", () =>
            {
                // Mania's scrolling area is mapped onto the track up to the scroll length: the wall when keeping visible
                // time, the far edge (behind the wall) when keeping speed.
                return Math.Abs(highway.Warp.Scroll.ScrollLength - (keepsVisibleTime ? 0.7f : 1)) < 1e-4f;
            });

            AddStep("flat camera", () => highwayConfig.SetValue(HighwaySetting.CameraMode, HighwayCameraMode.Flat));
            AddUntilStep("no wall when flat", () => !highway.Wall.IsPresent);
        }

        [Test]
        public void TestManiaDrawablesSeeThisRulesetsConfig()
        {
            ConfigProbe probe = null!;

            AddStep("add a probe inside the playfield", () => highway.Overlay.Add(probe = new ConfigProbe()));
            AddUntilStep("probe loaded", () => probe.IsLoaded);

            // Mania's notes and columns resolve this type; without it, settings such as timing-based colouring do nothing.
            AddAssert("mania's config type resolves to ours", () => probe.Config is ManiaOrthoRulesetConfigManager);
            AddAssert("scroll direction is forced down", () => probe.Config!.Get<ManiaScrollingDirection>(ManiaRulesetSetting.ScrollDirection) == ManiaScrollingDirection.Down);
        }

        [Test]
        public void TestScrollSpeedChangesApply()
        {
            ScrollingProbe probe = null!;

            AddStep("add a probe inside the playfield", () => highway.Overlay.Add(probe = new ScrollingProbe()));
            AddUntilStep("probe loaded", () => probe.IsLoaded);

            // The synthetic chart has 0.5x and 1.75x SV sections (PlaytestBeatmaps.CreateSynthetic). Mania keeps SV for its own
            // beatmaps; a note in those sections must travel the same distance as in mania, relative to a 1x section.
            AddAssert("0.5x section scrolls at half speed", () => lengthRatio(probe, 24000), () => Is.EqualTo(0.5).Within(1e-3));
            AddAssert("1.75x section scrolls faster", () => lengthRatio(probe, 30000), () => Is.EqualTo(1.75).Within(1e-3));
            AddAssert("1x after the SV sections", () => lengthRatio(probe, 36000), () => Is.EqualTo(1).Within(1e-3));
        }

        /// <summary>
        /// How far a note travels over 100ms starting at <paramref name="time"/>, relative to the same span in the 1x lead-in.
        /// </summary>
        private static double lengthRatio(ScrollingProbe probe, double time)
        {
            var algorithm = probe.ScrollingInfo.Algorithm.Value;
            const double time_range = 1000;

            return algorithm.GetLength(time, time + 100, time_range, 1) / algorithm.GetLength(2000, 2100, time_range, 1);
        }

        private void waitForJudgement()
        {
            AddUntilStep("a judgement is shown", () => layer.CurrentJudgement?.Piece?.IsLoaded == true && layer.CurrentJudgement.Result?.IsHit == true);

            // Let the highway place it at least once.
            AddWaitStep("let it settle", 2);
        }

        private Drawable maniaJudgements() => drawableRuleset.Playfield.Stages.Single().ChildrenOfType<JudgementContainer<DrawableManiaJudgement>>().Single();

        private partial class ScrollingProbe : Drawable
        {
            [Resolved]
            public IScrollingInfo ScrollingInfo { get; private set; } = null!;
        }

        private partial class ConfigProbe : Drawable
        {
            [Resolved(CanBeNull = true)]
            public ManiaRulesetConfigManager? Config { get; private set; }
        }
    }
}
