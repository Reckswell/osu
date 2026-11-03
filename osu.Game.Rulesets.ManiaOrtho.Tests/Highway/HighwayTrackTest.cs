// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.ManiaOrtho.Highway;
using osu.Game.Rulesets.ManiaOrtho.UI;
using osuTK;

namespace osu.Game.Rulesets.ManiaOrtho.Tests.Highway
{
    /// <summary>
    /// L0 checks for the highway's track (research game-chunithm-feedback P5): how a lane beam fades in and out, and where
    /// the track continues below the judgement line.
    /// </summary>
    [TestFixture]
    public class HighwayTrackTest
    {
        private const float tolerance = 1e-4f;

        [Test]
        public void TestBeamFadesInLinearly()
        {
            Assert.That(HighwayTrack.StepBeam(0, true, HighwayTrack.BEAM_FADE_IN / 2), Is.EqualTo(0.5f).Within(tolerance));
            Assert.That(HighwayTrack.StepBeam(0.5f, true, HighwayTrack.BEAM_FADE_IN / 2), Is.EqualTo(1).Within(tolerance));
            Assert.That(HighwayTrack.StepBeam(1, true, 1000), Is.EqualTo(1));
        }

        [Test]
        public void TestBeamFadesOutEasingOut()
        {
            // (1 - p)² at progress p, the shape of an OutQuad fade, whatever the frame steps.
            Assert.That(HighwayTrack.StepBeam(1, false, HighwayTrack.BEAM_FADE_OUT / 2), Is.EqualTo(0.25f).Within(tolerance));

            float level = 1;
            for (int i = 0; i < 15; i++)
                level = HighwayTrack.StepBeam(level, false, HighwayTrack.BEAM_FADE_OUT / 2 / 15);
            Assert.That(level, Is.EqualTo(0.25f).Within(tolerance));

            Assert.That(HighwayTrack.StepBeam(0.25f, false, HighwayTrack.BEAM_FADE_OUT / 2), Is.EqualTo(0).Within(tolerance));
            Assert.That(HighwayTrack.StepBeam(1, false, HighwayTrack.BEAM_FADE_OUT), Is.Zero);
        }

        [Test]
        public void TestBeamHoldsWhileTimeStands()
        {
            Assert.That(HighwayTrack.StepBeam(0.4f, true, 0), Is.EqualTo(0.4f));
            Assert.That(HighwayTrack.StepBeam(0.4f, false, 0), Is.EqualTo(0.4f).Within(tolerance));
        }

        [TestCase(-16.0)]
        [TestCase(double.NaN)]
        public void TestRewindSettlesTheBeam(double elapsed)
        {
            Assert.That(HighwayTrack.StepBeam(0.3f, true, elapsed), Is.EqualTo(1));
            Assert.That(HighwayTrack.StepBeam(0.3f, false, elapsed), Is.Zero);
        }

        [Test]
        public void TestRailsAreAShareOfTheirStage()
        {
            var single = new[] { new Vector2(0, 1) };
            HighwayTrack.GetRailWidths(single, 0, out float left, out float right);
            Assert.That(left, Is.EqualTo(HighwayTrack.RAIL_WIDTH).Within(tolerance));
            Assert.That(right, Is.EqualTo(HighwayTrack.RAIL_WIDTH).Within(tolerance));

            // Dual stages far apart: each rail is a sixteenth of its own stage.
            var apart = new[] { new Vector2(0, 0.4f), new Vector2(0.6f, 1) };
            HighwayTrack.GetRailWidths(apart, 1, out left, out right);
            Assert.That(left, Is.EqualTo(0.4f * HighwayTrack.RAIL_WIDTH).Within(tolerance));
            Assert.That(right, Is.EqualTo(0.4f * HighwayTrack.RAIL_WIDTH).Within(tolerance));
        }

        [Test]
        public void TestRailsNeverCoverTheNextStage()
        {
            var close = new[] { new Vector2(0, 0.495f), new Vector2(0.505f, 1) };

            HighwayTrack.GetRailWidths(close, 0, out float outer, out float inner0);
            HighwayTrack.GetRailWidths(close, 1, out float inner1, out _);

            Assert.That(outer, Is.EqualTo(0.495f * HighwayTrack.RAIL_WIDTH).Within(tolerance));
            Assert.That(inner0, Is.EqualTo(0.005f).Within(tolerance));
            Assert.That(close[0].Y + inner0, Is.LessThanOrEqualTo(close[1].X - inner1 + tolerance));

            var touching = new[] { new Vector2(0, 0.5f), new Vector2(0.5f, 1) };
            HighwayTrack.GetRailWidths(touching, 0, out _, out inner0);
            Assert.That(inner0, Is.Zero);
        }

        [TestCase(0f, 1f)]
        [TestCase(0.25f, 0.5f)]
        [TestCase(-1 / 16f, 0f)]
        public void TestBelowTheLineFollowsTheLaneEdges(float lane0, float lane1)
        {
            var p = new HighwayProjection(new HighwayLayout
            {
                JudgementY = 0.926f,
                FarY = 0,
                NearWidth = 0.61f,
                FarWidthRatio = 0.24f,
                CentreX = 0.5f,
            }, new Vector2(1920, 1080));

            float height = 1080 - p.YAt(0);
            var quad = TrackDrawing.GetBelowLine(p, lane0, lane1, height);

            Assert.That(Vector2.Distance(quad.TopLeft, p.ToScreen(lane0, 0)), Is.LessThan(1e-2f));
            Assert.That(Vector2.Distance(quad.TopRight, p.ToScreen(lane1, 0)), Is.LessThan(1e-2f));
            Assert.That(quad.BottomLeft.Y, Is.EqualTo(1080).Within(1e-2f));
            Assert.That(quad.BottomRight.Y, Is.EqualTo(1080).Within(1e-2f));

            // Each side continues the lane edge's straight line on the track.
            Assert.That(cross(p.ToScreen(lane0, 0.5f), p.ToScreen(lane0, 0), quad.BottomLeft), Is.LessThan(1e-3f));
            Assert.That(cross(p.ToScreen(lane1, 0.5f), p.ToScreen(lane1, 0), quad.BottomRight), Is.LessThan(1e-3f));
        }

        /// <summary>
        /// How far <paramref name="c"/> is from the line through <paramref name="a"/> and <paramref name="b"/>, in pixels.
        /// </summary>
        private static float cross(Vector2 a, Vector2 b, Vector2 c)
        {
            Vector2 ab = b - a;
            Vector2 ac = c - a;
            return System.Math.Abs(ab.X * ac.Y - ab.Y * ac.X) / ab.Length;
        }
    }
}
