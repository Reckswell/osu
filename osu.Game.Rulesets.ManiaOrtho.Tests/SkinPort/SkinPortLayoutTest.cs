// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.ManiaOrtho.Highway;
using osu.Game.Rulesets.ManiaOrtho.SkinPort;
using osuTK;

namespace osu.Game.Rulesets.ManiaOrtho.Tests.SkinPort
{
    /// <summary>
    /// How the skin porter fits a skin's flat layout to the camera, and where it draws the skin's rows.
    /// </summary>
    [TestFixture]
    public class SkinPortLayoutTest
    {
        private static readonly Vector2 size = new Vector2(1920, 1080);

        /// <summary>
        /// A 4K stage like Minty's: hit position 400 (stable), 180 units wide, at 1080p (1.6 × 1080/768 px per stable unit).
        /// </summary>
        private static FlatStage minty()
        {
            const float px_per_stable_unit = 1080f / 480;
            float hitY = 400 * px_per_stable_unit;
            return new FlatStage(hitY, 0, 180 * px_per_stable_unit, 1080f / 768, size);
        }

        private static HighwayLayout chunithm() => HighwayPresets.Get(HighwayPreset.Chunithm).ToLayout();

        [TestCase(SkinKeyArea.Panel, true)]
        [TestCase(SkinKeyArea.Floor, true)]
        [TestCase(SkinKeyArea.Squeezed, false)]
        [TestCase(SkinKeyArea.Receptors, false)]
        [TestCase(SkinKeyArea.Hidden, false)]
        public void TestLiftedLineSitsAtTheSkinsHitPosition(SkinKeyArea keyArea, bool lifts)
        {
            var stage = minty();
            var layout = SkinPortLayout.Adjust(chunithm(), keyArea, SkinStageFit.Camera, stage, null);

            Assert.That(layout.JudgementY, Is.EqualTo(lifts ? stage.HitY / size.Y : chunithm().JudgementY).Within(1e-5f));
        }

        [Test]
        public void TestLineIsNeverLoweredBelowTheCameras()
        {
            // Ākala 4K: hit position at the very bottom of the screen.
            var stage = minty() with { HitY = size.Y };
            var layout = SkinPortLayout.Adjust(chunithm(), SkinKeyArea.Panel, SkinStageFit.Camera, stage, null);

            Assert.That(layout.JudgementY, Is.EqualTo(chunithm().JudgementY));
        }

        [Test]
        public void TestCameraFitKeepsTheCamerasTrack()
        {
            var layout = SkinPortLayout.Adjust(chunithm(), SkinKeyArea.Hidden, SkinStageFit.Camera, minty(), 200);

            Assert.That(layout.CentreX, Is.EqualTo(0.5f));
            Assert.That(layout.NearWidth, Is.EqualTo(chunithm().NearWidth));
        }

        [Test]
        public void TestSkinPositionCentresTheTrackWhereStableCentresTheStage()
        {
            // R Skin 4K: ColumnStart 200, 208 units wide: centred at 304 of 853 stable units at 16:9.
            const float px_per_stable_unit = 1080f / 480;
            var stage = minty() with { Width = 208 * px_per_stable_unit };

            var layout = SkinPortLayout.Adjust(chunithm(), SkinKeyArea.Hidden, SkinStageFit.SkinPosition, stage, 200);

            Assert.That(layout.CentreX, Is.EqualTo(304f / (480 * 16f / 9)).Within(1e-4f));
            Assert.That(layout.NearWidth, Is.EqualTo(chunithm().NearWidth), "the camera's width is kept");
        }

        [Test]
        public void TestSkinPositionKeepsTheTrackOnScreen()
        {
            // A stage pushed all the way to the left keeps the camera's wide track inside the screen.
            var layout = SkinPortLayout.Adjust(chunithm(), SkinKeyArea.Hidden, SkinStageFit.SkinPosition, minty(), 0);

            Assert.That(layout.CentreX, Is.EqualTo(chunithm().NearWidth / 2).Within(1e-5f));
        }

        [Test]
        public void TestCentredSkinStaysCentred()
        {
            var layout = SkinPortLayout.Adjust(chunithm(), SkinKeyArea.Hidden, SkinStageFit.SkinPosition, minty(), null);
            Assert.That(layout.CentreX, Is.EqualTo(0.5f));
        }

        [Test]
        public void TestSkinLayoutUsesTheSkinsWidth()
        {
            var stage = minty();
            var layout = SkinPortLayout.Adjust(chunithm(), SkinKeyArea.Hidden, SkinStageFit.SkinLayout, stage, null);

            Assert.That(layout.NearWidth, Is.EqualTo(stage.Width / size.X).Within(1e-5f));
        }

        [Test]
        public void TestInvalidStageChangesNothing()
        {
            var layout = SkinPortLayout.Adjust(chunithm(), SkinKeyArea.Panel, SkinStageFit.SkinLayout, default, 200);
            Assert.That(layout, Is.EqualTo(chunithm()));
        }

        [TestCase(HighwayPreset.Chunithm)]
        [TestCase(HighwayPreset.Voltex)]
        [TestCase(HighwayPreset.Parallel)]
        public void TestNoteDepthKeepsTheImagesProportionsAtTheLine(HighwayPreset preset)
        {
            var projection = new HighwayProjection(HighwayPresets.Get(preset).ToLayout(), size);
            const float lane = 0.25f;
            const float aspect = 152f / 296; // Ākala's bars.

            float depth = SkinPortLayout.NoteDepth(projection, lane, aspect, 1);

            float height = projection.YAt(0) - projection.YAt(depth);
            float width = lane * projection.WidthAt(0);

            Assert.That(height / width, Is.EqualTo(aspect).Within(1e-3f));
        }

        [Test]
        public void TestPlacementMapsRowsAsTheWarpDoes()
        {
            var stage = minty();
            var projection = new HighwayProjection(chunithm(), size);
            var placement = new PortPlacement(projection, stage, SkinKeyArea.Receptors, 1.2f, 1);

            Assert.That(placement.DepthOfFlatY(stage.HitY), Is.Zero);
            Assert.That(placement.DepthOfFlatY(stage.ScrollTop), Is.EqualTo(1.2f).Within(1e-5f));
            Assert.That(placement.ScreenYBelow(stage.HitY + 10), Is.EqualTo(placement.LineY + 10).Within(1e-3f), "kept at its size below the line");
            Assert.That(placement.BelowScale, Is.EqualTo(1));
        }

        [Test]
        public void TestSqueezedKeyAreaFitsBelowTheLine()
        {
            var stage = minty();
            var projection = new HighwayProjection(chunithm(), size);
            var placement = new PortPlacement(projection, stage, SkinKeyArea.Squeezed, 1, 1);

            Assert.That(placement.ScreenYBelow(size.Y), Is.EqualTo(size.Y).Within(1e-2f), "the bottom of the key area at the bottom of the screen");
        }

        [Test]
        public void TestPanelHasUprightSides()
        {
            var projection = new HighwayProjection(chunithm(), size);
            var placement = new PortPlacement(projection, minty(), SkinKeyArea.Panel, 1, 1);

            Assert.That(placement.XBelow(0, size.Y), Is.EqualTo(projection.XAt(0, placement.LineY)).Within(1e-3f));

            var floor = new PortPlacement(projection, minty(), SkinKeyArea.Floor, 1, 1);
            Assert.That(floor.XBelow(0, size.Y), Is.LessThan(projection.XAt(0, placement.LineY)), "a floor widens below the line");
        }

        [Test]
        public void TestReadingColumnStartStillWorks()
        {
            // ColumnStart is read from an internal field of osu!'s legacy skin; this fails if osu! renames it.
            Assert.That(PortedSkin.CanReadColumnStart, Is.True);
        }

        [Test]
        public void TestLaneLightFadesAfterRelease()
        {
            Assert.That(PortedTrack.LightLevel(true, double.PositiveInfinity), Is.EqualTo(1));
            Assert.That(PortedTrack.LightLevel(false, 0), Is.EqualTo(1));
            Assert.That(PortedTrack.LightLevel(false, PortedTrack.LIGHT_FADE / 2), Is.EqualTo(0.5f).Within(1e-5f));
            Assert.That(PortedTrack.LightLevel(false, PortedTrack.LIGHT_FADE), Is.Zero);
            Assert.That(PortedTrack.LightLevel(false, -10), Is.Zero, "rewound before the release");
        }

        [Test]
        public void TestHitLightingFadesInThenOut()
        {
            Assert.That(PortedLighting.HitAlpha(-1), Is.Zero);
            Assert.That(PortedLighting.HitAlpha(PortedLighting.FADE_IN / 2), Is.EqualTo(0.5f).Within(1e-5f));
            Assert.That(PortedLighting.HitAlpha(PortedLighting.FADE_IN), Is.EqualTo(1).Within(1e-5f));
            Assert.That(PortedLighting.HitAlpha(PortedLighting.FADE_IN + PortedLighting.FADE_OUT), Is.Zero);
        }
    }
}
