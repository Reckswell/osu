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
using osu.Game.Database;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mania.Skinning.Legacy;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.ManiaOrtho.Configuration;
using osu.Game.Rulesets.ManiaOrtho.Highway;
using osu.Game.Rulesets.ManiaOrtho.Tests.Playtest;
using osu.Game.Rulesets.ManiaOrtho.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
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

        [Resolved]
        private SkinManager skins { get; set; } = null!;

        private HighwayConfigManager highwayConfig => ((ManiaOrthoRulesetConfigManager)RulesetConfigs.GetConfigFor(CreatePlayerRuleset()).AsNonNull()).Highway;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            AddStep("Chunithm preset", () =>
            {
                highwayConfig.SetValue(HighwaySetting.Preset, HighwayPreset.Custom);
                highwayConfig.SetValue(HighwaySetting.Preset, HighwayPreset.Chunithm);
                highwayConfig.SetValue(HighwaySetting.LaneGuides, true);
                highwayConfig.SetValue(HighwaySetting.SkinHitTarget, false);
                highwayConfig.SetValue(HighwaySetting.Track, HighwayTrackStyle.Highway);
                skins.CurrentSkinInfo.SetDefault();
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

        /// <summary>
        /// SPEC 4: the bottom lift raises the judgement line, and the judgement shown above it goes up with it, keeping its
        /// height above the line.
        /// </summary>
        [Test]
        public void TestJudgementsFollowTheLift()
        {
            float gap = 0;

            waitForJudgement();
            AddStep("freeze with a judgement shown", () => Player.GameplayClockContainer.Stop());
            AddWaitStep("let it settle", 2);
            AddStep("measure its height above the line", () => gap = highway.Projection.YAt(0) - judgementCentre().Y);
            AddAssert("it is above the line", () => gap, () => Is.GreaterThan(1));

            AddStep("lift the line to 30%", () => highwayConfig.SetValue(HighwaySetting.JudgementLineHeight, 0.3f));
            AddUntilStep("the line is raised", () => highway.Projection.YAt(0), () => Is.EqualTo(0.7f * highway.DrawHeight).Within(0.5f));
            AddWaitStep("let the judgement follow", 2);

            AddAssert("the judgement keeps its height above the raised line", () => highway.Projection.YAt(0) - judgementCentre().Y, () => Is.EqualTo(gap).Within(0.5f));
            AddAssert("and stays over the track", () =>
            {
                var projection = highway.Projection;
                Vector2 centre = judgementCentre();
                float depth = projection.DepthAtY(centre.Y);

                return centre.X > projection.ToScreen(0, depth).X && centre.X < projection.ToScreen(1, depth).X;
            });
        }

        private Vector2 judgementCentre() => highway.ToLocalSpace(layer.CurrentJudgement.AsNonNull().Piece.AsNonNull().ScreenSpaceDrawQuad.Centre);

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
        public void TestGuidesFollowTheColumns()
        {
            AddUntilStep("hit targets found", () => highway.HitTargets.Count == 4);

            AddAssert("guides and judgement line shown", () => highway.Guides.IsPresent && highway.Guides.ShowLaneLines && highway.Guides.ShowJudgementLine);
            AddAssert("skin hit targets hidden", () => highway.HitTargets.All(t => t.Alpha == 0));

            // Two edges and three dividers for 4K, on the warped column boundaries: equal lanes in the default skin.
            AddAssert("one line per column boundary", () => highway.Guides.LineCount == 5);
            AddAssert("lines at the column boundaries", () => Enumerable.Range(0, 5).All(i =>
            {
                var line = highway.Guides.GetLine(i);
                return Math.Abs(line.Lane - i / 4f) < 0.01f && line.IsEdge == (i == 0 || i == 4);
            }));
        }

        [Test]
        public void TestGuideSettings()
        {
            AddUntilStep("hit targets found", () => highway.HitTargets.Count == 4);

            AddStep("use the skin's hit target", () => highwayConfig.SetValue(HighwaySetting.SkinHitTarget, true));
            AddUntilStep("skin hit targets shown", () => highway.HitTargets.All(t => t.Alpha == 1));
            AddAssert("no highway judgement line", () => !highway.Guides.ShowJudgementLine);
            AddAssert("lane guides stay", () => highway.Guides.ShowLaneLines);

            AddStep("lane guides off", () => highwayConfig.SetValue(HighwaySetting.LaneGuides, false));
            AddUntilStep("no lane guides", () => !highway.Guides.ShowLaneLines);

            AddStep("reset", () =>
            {
                highwayConfig.SetValue(HighwaySetting.SkinHitTarget, false);
                highwayConfig.SetValue(HighwaySetting.LaneGuides, true);
            });
            AddUntilStep("skin hit targets hidden", () => highway.HitTargets.All(t => t.Alpha == 0));
        }

        [Test]
        public void TestLegacySkinHitTargetHidden()
        {
            AddStep("classic skin", () => skins.CurrentSkinInfo.Value = DefaultLegacySkin.CreateInfo().ToLiveUnmanaged());

            // The highway's track hides the stage background this hit target is drawn in, but keeps it current.
            AddAssert("on the highway's track", () => highway.TrackShown);

            // Legacy skins with key textures draw their hit target (stage hint and judgement line) in the stage background.
            AddUntilStep("legacy hit target found", () => highway.HitTargets.OfType<LegacyHitTarget>().Any());
            AddAssert("every legacy hit target is tracked", () => legacyHitTargets().All(highway.HitTargets.Contains));
            AddAssert("and hidden", () => legacyHitTargets().All(t => t.Alpha == 0));

            AddStep("use the skin's hit target", () => highwayConfig.SetValue(HighwaySetting.SkinHitTarget, true));
            AddUntilStep("legacy hit target shown", () => legacyHitTargets().All(t => t.Alpha == 1));

            // The highway's track hides the stage background the legacy hit target is drawn in.
            AddAssert("on the highway's track the highway's line stays", () => highway.TrackShown && highway.Guides.ShowJudgementLine);
            AddStep("skin's track", () => highwayConfig.SetValue(HighwaySetting.Track, HighwayTrackStyle.Skin));
            AddUntilStep("on the skin's track the skin's line replaces it", () => !highway.TrackShown && !highway.Guides.ShowJudgementLine);
            AddStep("highway's track", () => highwayConfig.SetValue(HighwaySetting.Track, HighwayTrackStyle.Highway));

            AddStep("default skin", () => skins.CurrentSkinInfo.SetDefault());
            AddUntilStep("legacy hit target gone", () => !highway.HitTargets.OfType<LegacyHitTarget>().Any());
        }

        [Test]
        public void TestFlatCameraKeepsManiasLook()
        {
            AddUntilStep("hit targets found", () => highway.HitTargets.Count == 4);

            AddStep("flat camera", () => highwayConfig.SetValue(HighwaySetting.CameraMode, HighwayCameraMode.Flat));
            AddUntilStep("no guides", () => !highway.Guides.IsPresent);
            AddAssert("skin hit targets shown", () => highway.HitTargets.All(t => t.Alpha == 1));
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

        [Test]
        public void TestConstantSpeedIgnoresSv()
        {
            ScrollingProbe probe = null!;

            // Mania's Constant Speed mod swaps the scroll algorithm for a constant one; the highway places notes with the
            // algorithm mania hands out, so the SV sections must scroll at 1x (research/game-osu-mania.md, finding 2).
            AddStep("play with Constant Speed", () => LoadPlayer(new Mod[] { new ManiaModConstantSpeed() }));
            AddUntilStep("player loaded", () => Player.IsLoaded);
            AddStep("add a probe inside the playfield", () => highway.Overlay.Add(probe = new ScrollingProbe()));
            AddUntilStep("probe loaded", () => probe.IsLoaded);

            AddAssert("0.5x section scrolls at 1x", () => lengthRatio(probe, 24000), () => Is.EqualTo(1).Within(1e-3));
            AddAssert("1.75x section scrolls at 1x", () => lengthRatio(probe, 30000), () => Is.EqualTo(1).Within(1e-3));
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

        private LegacyHitTarget[] legacyHitTargets() => drawableRuleset.Playfield.ChildrenOfType<LegacyHitTarget>().ToArray();

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
