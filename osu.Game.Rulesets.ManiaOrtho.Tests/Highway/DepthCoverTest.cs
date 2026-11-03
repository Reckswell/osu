// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.ManiaOrtho.Highway;

namespace osu.Game.Rulesets.ManiaOrtho.Tests.Highway
{
    /// <summary>
    /// L0 checks for <see cref="DepthCover"/>, which covers native notes the way mania's Hidden, Fade In and Cover cover a
    /// column.
    /// </summary>
    [TestFixture]
    public class DepthCoverTest
    {
        private const float tolerance = 1e-5f;

        /// <summary>
        /// Fade In / Cover along scroll: the far half covered, fading in over the quarter below it.
        /// </summary>
        private static readonly DepthCover far_half = new DepthCover(0.5f, float.MaxValue, 0.5f, 0.25f, 1);

        /// <summary>
        /// Hidden / Cover against scroll: the near part covered, fading out over the part above it.
        /// </summary>
        private static readonly DepthCover near_part = new DepthCover(float.MinValue, 0.2f, 0.2f, 0.45f, 1);

        [Test]
        public void TestNoneShowsEverything()
        {
            Assert.That(DepthCover.None.IsActive, Is.False);
            Assert.That(DepthCover.None.VisibilityAt(0.3f), Is.EqualTo(1));
            Assert.That(DepthCover.None.NextCut(0.1f, 0.4f), Is.EqualTo(0.4f));
        }

        [TestCase(0.0f, 1)]
        [TestCase(0.25f, 1)]
        [TestCase(0.375f, 0.5f)]
        [TestCase(0.5f, 0)]
        [TestCase(0.9f, 0)]
        [TestCase(3f, 0)]
        public void TestFarHalf(float depth, float visibility)
        {
            Assert.That(far_half.VisibilityAt(depth), Is.EqualTo(visibility).Within(tolerance));
        }

        [TestCase(-0.5f, 0)]
        [TestCase(0.1f, 0)]
        [TestCase(0.325f, 0.5f)]
        [TestCase(0.45f, 1)]
        [TestCase(0.8f, 1)]
        public void TestNearPart(float depth, float visibility)
        {
            Assert.That(near_part.VisibilityAt(depth), Is.EqualTo(visibility).Within(tolerance));
        }

        [Test]
        public void TestCutsStopAtEveryEdge()
        {
            Assert.That(far_half.NextCut(0, 1), Is.EqualTo(0.25f), "fade start");
            Assert.That(far_half.NextCut(0.49f, 1), Is.EqualTo(0.5f).Within(tolerance), "cover start");
            Assert.That(far_half.NextCut(0.5f, 1), Is.EqualTo(1), "nothing after the cover starts");
            Assert.That(far_half.NextCut(0, 0.1f), Is.EqualTo(0.1f), "the piece's own end");
        }

        [Test]
        public void TestFadeIsCutIntoShortRamps()
        {
            float from = 0.25f;
            int pieces = 0;

            while (from < 0.5f)
            {
                float to = far_half.NextCut(from, 1);

                Assert.That(to - from, Is.LessThanOrEqualTo(DepthCover.MAX_FADE_STEP + tolerance));
                Assert.That(to, Is.GreaterThan(from));

                from = to;
                pieces++;
            }

            Assert.That(from, Is.EqualTo(0.5f).Within(tolerance));
            Assert.That(pieces, Is.EqualTo(13));
        }

        [Test]
        public void TestPieceStartingJustBelowTheFadeIsStillCutShort()
        {
            var nearPart = new DepthCover(float.MinValue, 0.2f, 0.2f, 0.45f, 1);

            Assert.That(nearPart.NextCut(0.2f - 5e-6f, 1) - (0.2f - 5e-6f), Is.LessThanOrEqualTo(DepthCover.MAX_FADE_STEP + tolerance));
        }

        [Test]
        public void TestStepWhileTheFadeEasesIn()
        {
            // While mania's cover fades in, the gradient is only partly opaque next to the fully covered part.
            var easing = new DepthCover(0.5f, float.MaxValue, 0.5f, 0.25f, 0.6f);

            Assert.That(easing.VisibilityAt(0.5f, 0.45f), Is.EqualTo(0.4f).Within(tolerance), "fade side of the edge");
            Assert.That(easing.VisibilityAt(0.5f, 0.55f), Is.EqualTo(0), "covered side of the edge");
        }
    }
}
