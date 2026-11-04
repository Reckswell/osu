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
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Skinning.Legacy;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.ManiaOrtho.Configuration;
using osu.Game.Rulesets.ManiaOrtho.Highway;
using osu.Game.Rulesets.ManiaOrtho.Tests.Playtest;
using osu.Game.Rulesets.ManiaOrtho.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
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

        private string chart = "synthetic-4k";

        /// <summary>
        /// Played instead of <see cref="chart"/> when set.
        /// </summary>
        private IBeatmap? beatmap;

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => beatmap ?? PlaytestBeatmaps.Load(chart);

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
                chart = "synthetic-4k";
                beatmap = null;
                highwayConfig.GetBindable<HighwayNoteStyle>(HighwaySetting.NoteStyle).SetDefault();
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

            AddAssert("one cover, from the first column's left edge to the last column's right edge", () => highway.Wall.Stages.ToArray(),
                () => Is.EqualTo(new[] { new Vector2(highway.ColumnLanes[0].X, highway.ColumnLanes[^1].Y) }));

            AddAssert("lower edge at 70% of the track, on the stage's left edge", () =>
            {
                var projection = highway.Projection;
                var wall = highway.ToLocalSpace(highway.Wall.GetScreenSpaceCover(0).BottomLeft);
                var corner = projection.ToScreen(highway.Wall.Stages[0].X, 0.7f);

                return Vector2.Distance(wall, corner) < 3;
            });

            AddAssert("covers the lanes beyond it", () =>
            {
                var projection = highway.Projection;
                var stage = highway.Wall.Stages[0];
                var wall = highway.Wall.GetScreenSpaceCover(0);

                return wall.Contains(highway.ToScreenSpace(projection.ToScreen(stage.X + 0.01f, 0.71f)))
                       && wall.Contains(highway.ToScreenSpace(projection.ToScreen(stage.Y - 0.01f, 0.99f)));
            });

            AddAssert("and the stage's edge lines", () =>
            {
                var projection = highway.Projection;
                var stage = highway.Wall.Stages[0];
                var wall = highway.Wall.GetScreenSpaceCover(0);

                // The outer edges of the stage's edge lines (as wide as the guides draw them, at least a pixel), at and
                // beyond the wall's lower edge.
                return new[] { 0.72f, 0.85f, 0.99f }.All(depth =>
                {
                    float half = Math.Max(HighwayGuides.LINE_WIDTH * projection.WidthAt(depth), 1) / 2;
                    return wall.Contains(highway.ToScreenSpace(projection.ToScreen(stage.X, depth) - new Vector2(half, 0)))
                           && wall.Contains(highway.ToScreenSpace(projection.ToScreen(stage.Y, depth) + new Vector2(half, 0)));
                });
            });

            AddAssert("but nothing beside or above them", () =>
            {
                var projection = highway.Projection;
                var stage = highway.Wall.Stages[0];
                var wall = highway.Wall.GetScreenSpaceCover(0);

                return !wall.Contains(highway.ToScreenSpace(projection.ToScreen(stage.X, 0.95f) - new Vector2(10, 0)))
                       && !wall.Contains(highway.ToScreenSpace(projection.ToScreen(stage.Y, 0.95f) + new Vector2(10, 0)))
                       && !wall.Contains(highway.ToScreenSpace(projection.ToScreen(0.5f * (stage.X + stage.Y), 1) - new Vector2(0, 5)));
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

        /// <summary>
        /// From 6K, the boundaries between mania's hand groups are group dividers (research/topic-wide-keys.md W1): the centre
        /// key of 7K stands alone, 10K splits 5|5.
        /// </summary>
        [TestCase(7, new[] { 3, 4 })]
        [TestCase(10, new[] { 5 })]
        public void TestGroupDividers(int keys, int[] groupStarts)
        {
            AddStep($"play {keys}K", () =>
            {
                chart = $"synthetic-{keys}k";
                LoadPlayer();
            });
            AddUntilStep("player loaded", () => Player.IsLoaded && Player.Alpha == 1 && highway.Guides.LineCount == keys + 1);

            AddAssert("edges, group dividers and lane lines", () => Enumerable.Range(0, keys + 1).Select(i => highway.Guides.GetLine(i).Kind),
                () => Is.EqualTo(expectedKinds(keys, groupStarts)));
        }

        /// <summary>
        /// Each stage of a dual-stage play is grouped on its own: two 7K stages each show 7K's centre key.
        /// </summary>
        [Test]
        public void TestGroupDividersPerStage()
        {
            AddStep("play an osu! beatmap as 7K dual stages", () =>
            {
                // Dual Stages applies to converts only.
                beatmap = new Beatmap
                {
                    BeatmapInfo = { Ruleset = new OsuRuleset().RulesetInfo },
                    HitObjects = Enumerable.Range(0, 32).Select(i => (HitObject)new HitCircle { StartTime = 1000 + i * 250, Position = new Vector2(i * 61 % 512, 192) }).ToList(),
                };

                var ruleset = CreatePlayerRuleset();
                LoadPlayer(new[] { ruleset.CreateModFromAcronym("7K").AsNonNull(), ruleset.CreateModFromAcronym("DS").AsNonNull() });
            });
            AddUntilStep("player loaded", () => Player.IsLoaded && Player.Alpha == 1 && drawableRuleset.Playfield.Stages.Count == 2 && highway.Guides.LineCount == 16);

            AddAssert("each stage grouped 3|1|3", () => Enumerable.Range(0, 16).Select(i => highway.Guides.GetLine(i).Kind),
                () => Is.EqualTo(expectedKinds(7, new[] { 3, 4 }).Concat(expectedKinds(7, new[] { 3, 4 }))));
        }

        private static HighwayGuides.GuideLineKind[] expectedKinds(int keys, int[] groupStarts) => Enumerable.Range(0, keys + 1)
            .Select(i => i == 0 || i == keys ? HighwayGuides.GuideLineKind.Edge
                : groupStarts.Contains(i) ? HighwayGuides.GuideLineKind.Group
                : HighwayGuides.GuideLineKind.Lane)
            .ToArray();

        /// <summary>
        /// Notes keep at least a pixel of gap on each side on screen (research/topic-wide-keys.md W3): 1% of a 10K lane is far
        /// under a pixel at Voltex's far end. Where 1% is more, as for 4K near the line under Chunithm, it stays 1%. Checked once
        /// a note is near the line, so far and near notes are both on screen.
        /// </summary>
        [TestCase(HighwayPreset.Voltex, 10, false)]
        [TestCase(HighwayPreset.Chunithm, 4, true)]
        public void TestNotesKeepAPixelOfGap(HighwayPreset preset, int keys, bool someWiderThanAPixel)
        {
            playWithHighwayNotes(preset, keys);
            AddUntilStep("a note near the line", () => noteBars().Any(b => b.Depth0 > 0 && b.Depth0 < 0.2f));

            int floored = 0;
            int share = 0;

            AddStep("measure the gaps", () =>
            {
                var noteLayer = highway.NoteLayer;
                var projection = new HighwayProjection(noteLayer.Layout, noteLayer.DrawSize);
                float pixelsPerUnit = MathF.Sqrt(noteLayer.DrawInfo.Matrix.M11 * noteLayer.DrawInfo.Matrix.M11 + noteLayer.DrawInfo.Matrix.M12 * noteLayer.DrawInfo.Matrix.M12);

                floored = share = 0;

                // Each bar's gap is measured at its far side.
                foreach (var bar in noteBars().Where(q => q.Depth1 >= 0))
                {
                    var lane = highway.ColumnLanes[((ManiaHitObject)bar.HitObject).Column];
                    float width = lane.Y - lane.X;
                    float trackPixels = projection.WidthAt(bar.Depth1) * pixelsPerUnit;
                    float floor = HighwayNoteLayer.MIN_GAP_PIXELS / trackPixels;
                    float expected = Math.Min(Math.Max(width * HighwayNoteLayer.NOTE_GAP, floor), width / 4);

                    foreach (float gap in new[] { bar.Lane0 - lane.X, lane.Y - bar.Lane1 })
                    {
                        if (gap * trackPixels < HighwayNoteLayer.MIN_GAP_PIXELS - 1e-3f)
                            throw new AssertionException($"under a pixel at depth {bar.Depth1}: {gap * trackPixels} px");

                        if (Math.Abs(gap - expected) * trackPixels > 1e-3f)
                            throw new AssertionException($"gap {gap * trackPixels} px at depth {bar.Depth1}, expected {expected * trackPixels} px");
                    }

                    if (floor > width * HighwayNoteLayer.NOTE_GAP)
                        floored++;
                    else
                        share++;
                }
            });

            AddAssert("some gaps held at a pixel", () => floored, () => Is.GreaterThan(0));
            if (someWiderThanAPixel)
                AddAssert("some gaps 1% of the lane", () => share, () => Is.GreaterThan(0));
        }

        /// <summary>
        /// A hold's body is never wider than its head or tail (SPEC 6), also where the gap's pixel floor narrows them: 10K under
        /// Voltex at the test window's size, where far lanes are a few pixels wide and a pixel is more than the body's own inset.
        /// </summary>
        [Test]
        public void TestHoldBodiesStayWithinTheirEnds()
        {
            playWithHighwayNotes(HighwayPreset.Voltex, 10);

            double time = 0;

            AddStep("stop among the long notes", () =>
            {
                // Bars 12-15 of the synthetic chart: holds every beat, 1.5 beats long.
                time = drawableRuleset.Objects.First(h => h is HoldNote).StartTime + 1000;

                Player.GameplayClockContainer.Stop();
                Player.GameplayClockContainer.Seek(time);
            });
            AddUntilStep("playfield at that time", () => drawableRuleset.FrameStableClock.CurrentTime, () => Is.EqualTo(time).Within(1));
            AddWaitStep("let the highway gather", 2);

            int holds = 0;
            int narrowedByTheirEnds = 0;

            AddStep("compare each body with its ends", () =>
            {
                var noteLayer = highway.NoteLayer;
                var quads = Enumerable.Range(0, noteLayer.Count).Select(noteLayer.GetQuad).ToArray();

                holds = narrowedByTheirEnds = 0;

                foreach (var hold in quads.Where(q => q.Kind == HighwayNoteLayer.NoteQuadKind.HoldBody).GroupBy(q => q.HitObject))
                {
                    var body = hold.First();
                    var ends = quads.Where(q => q.HitObject == hold.Key && q.Kind is HighwayNoteLayer.NoteQuadKind.HoldHead or HighwayNoteLayer.NoteQuadKind.HoldTail).ToArray();

                    foreach (var end in ends)
                    {
                        if (body.Lane0 < end.Lane0 - 1e-6f || body.Lane1 > end.Lane1 + 1e-6f)
                            throw new AssertionException($"body {body.Lane0}-{body.Lane1} wider than its {end.Kind} {end.Lane0}-{end.Lane1} at depth {end.Depth1}");
                    }

                    holds++;
                    if (ends.Any(e => Math.Abs(body.Lane0 - e.Lane0) < 1e-6f))
                        narrowedByTheirEnds++;
                }
            });

            AddAssert("holds compared", () => holds, () => Is.GreaterThan(0));
            AddAssert("some bodies narrowed by a pixel-wide gap", () => narrowedByTheirEnds, () => Is.GreaterThan(0));
        }

        private void playWithHighwayNotes(HighwayPreset preset, int keys)
        {
            AddStep($"{preset}, {keys}K, highway notes", () =>
            {
                highwayConfig.SetValue(HighwaySetting.Preset, preset);
                highwayConfig.SetValue(HighwaySetting.NoteStyle, HighwayNoteStyle.Highway);
                chart = $"synthetic-{keys}k";
                LoadPlayer();
            });
            AddUntilStep("highway notes shown", () => Player.IsLoaded && Player.Alpha == 1 && highway.NativeNotes && highway.ColumnLanes.Length == keys);
        }

        /// <summary>
        /// Each tap note's bar as the highway draws it: the quad reaching farthest of its rim and body (they share their lanes).
        /// </summary>
        private HighwayNoteLayer.NoteQuad[] noteBars()
        {
            var noteLayer = highway.NoteLayer;

            return Enumerable.Range(0, noteLayer.Count).Select(noteLayer.GetQuad)
                             .Where(q => q.Kind == HighwayNoteLayer.NoteQuadKind.Note)
                             .GroupBy(q => q.HitObject)
                             .Select(g => g.MaxBy(q => q.Depth1))
                             .ToArray();
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
