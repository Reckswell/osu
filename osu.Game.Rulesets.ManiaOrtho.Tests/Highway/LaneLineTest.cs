// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.ManiaOrtho.Highway;
using osuTK;

namespace osu.Game.Rulesets.ManiaOrtho.Tests.Highway
{
    /// <summary>
    /// L0 checks for <see cref="TrackDrawing.GetLaneLine"/>, which places the guide lines (research/game-chunithm.md P4):
    /// fixed width in track space, so they thin with distance, but never thinner than a minimum on screen.
    /// </summary>
    [TestFixture]
    public class LaneLineTest
    {
        private const float tolerance = 1e-3f;

        private static HighwayProjection create(float farWidthRatio) => new HighwayProjection(new HighwayLayout
        {
            JudgementY = 0.926f,
            FarY = 0,
            NearWidth = 0.61f,
            FarWidthRatio = farWidthRatio,
            CentreX = 0.5f,
        }, new Vector2(1920, 1080));

        [TestCase(0f)]
        [TestCase(0.25f)]
        [TestCase(0.5f)]
        [TestCase(1f)]
        public void TestStartsOnTheLaneAtTheJudgementLine(float lane)
        {
            var p = create(0.24f);
            const float width = 0.0022f;

            TrackDrawing.GetLaneLine(p, lane, width, 1, 1, out var near, out _, out _);

            Vector2 expected = p.ToScreen(lane, 0);

            Assert.That((near.BottomLeft + near.BottomRight).X / 2, Is.EqualTo(expected.X).Within(tolerance));
            Assert.That(near.BottomLeft.Y, Is.EqualTo(expected.Y).Within(tolerance));
            Assert.That(near.BottomRight.X - near.BottomLeft.X, Is.EqualTo(width * p.WidthAt(0)).Within(tolerance));
        }

        [Test]
        public void TestThinsWithDistanceButNotBelowTheMinimum()
        {
            var p = create(0.24f);

            // 0.22% of the track is ~2.6 px at the line and ~0.6 px at the far edge: the far part is held at 1 px.
            TrackDrawing.GetLaneLine(p, 0.25f, 0.0022f, 1, 1, out var near, out var far, out float split);

            Assert.That(split, Is.GreaterThan(0).And.LessThan(1));

            Assert.That(width(near.TopLeft, near.TopRight), Is.EqualTo(1).Within(tolerance));
            Assert.That(width(far.BottomLeft, far.BottomRight), Is.EqualTo(1).Within(tolerance));
            Assert.That(width(far.TopLeft, far.TopRight), Is.EqualTo(1).Within(tolerance));

            // The parts meet, and the far part ends on the lane at the far edge.
            assertNear(near.TopLeft, far.BottomLeft);
            assertNear((far.TopLeft + far.TopRight) / 2, p.ToScreen(0.25f, 1));

            // Where the near part narrows to the minimum, the exact projected width is the minimum too.
            float y = near.TopLeft.Y;
            Assert.That(0.0022f * p.WidthAtY(y), Is.EqualTo(1).Within(tolerance));
        }

        [Test]
        public void TestNearPartFollowsTheProjectedWidth()
        {
            var p = create(0.24f);

            TrackDrawing.GetLaneLine(p, 0.75f, 0.01f, 1, 1, out var near, out _, out float split);

            Assert.That(split, Is.EqualTo(1), "a 1% line stays above 1 px all the way");

            // Width is linear in screen y on a projected plane, so the trapezoid's width at any height is exact.
            for (float t = 0; t <= 1; t += 0.25f)
            {
                float y = near.BottomLeft.Y + (near.TopLeft.Y - near.BottomLeft.Y) * t;
                float left = near.BottomLeft.X + (near.TopLeft.X - near.BottomLeft.X) * t;
                float right = near.BottomRight.X + (near.TopRight.X - near.BottomRight.X) * t;

                Assert.That(right - left, Is.EqualTo(0.01f * p.WidthAtY(y)).Within(tolerance));
                Assert.That((left + right) / 2, Is.EqualTo(p.XAt(0.75f, y)).Within(tolerance));
            }
        }

        [Test]
        public void TestParallelLinesKeepTheirWidth()
        {
            var p = create(1);

            TrackDrawing.GetLaneLine(p, 0.5f, 0.0022f, 1, 1, out var near, out _, out float split);

            Assert.That(split, Is.EqualTo(1));
            Assert.That(width(near.TopLeft, near.TopRight), Is.EqualTo(width(near.BottomLeft, near.BottomRight)).Within(tolerance));
        }

        [Test]
        public void TestAcrossTrackSpansTheTrack()
        {
            var p = create(0.24f);
            float y = p.YAt(0);

            Quad band = TrackDrawing.GetAcrossTrack(p, y - 5, y + 5);

            Assert.That(band.TopLeft.X, Is.EqualTo(p.XAt(0, y - 5)).Within(tolerance));
            Assert.That(band.BottomRight.X, Is.EqualTo(p.XAt(1, y + 5)).Within(tolerance));
            Assert.That((band.TopLeft.Y + band.BottomLeft.Y) / 2, Is.EqualTo(y).Within(tolerance));
        }

        private static float width(Vector2 left, Vector2 right) => right.X - left.X;

        private static void assertNear(Vector2 actual, Vector2 expected)
        {
            Assert.That(actual.X, Is.EqualTo(expected.X).Within(tolerance));
            Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(tolerance));
        }
    }
}
