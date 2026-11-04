// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets.ManiaOrtho.Configuration;
using osu.Game.Rulesets.ManiaOrtho.Highway;
using osu.Game.Rulesets.ManiaOrtho.Tests.Playtest;
using osu.Game.Rulesets.ManiaOrtho.UI;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.ManiaOrtho.Tests.SkinPort
{
    /// <summary>
    /// Autoplays a synthetic chart with osu!'s classic (legacy) skin and checks that the skin porter draws it on the highway
    /// from the skin's own images, and that skins without legacy mania images keep the highway's look.
    /// </summary>
    public partial class TestSceneSkinPort : PlayerTestScene
    {
        protected override bool Autoplay => true;

        protected override Ruleset CreatePlayerRuleset() => new ManiaOrthoRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => PlaytestBeatmaps.Load("synthetic-4k");

        private HighwayPlayfieldAdjustmentContainer highway => ((DrawableManiaOrthoRuleset)Player.DrawableRuleset).Highway.AsNonNull();

        [Resolved]
        private SkinManager skins { get; set; } = null!;

        private HighwayConfigManager highwayConfig => ((ManiaOrthoRulesetConfigManager)RulesetConfigs.GetConfigFor(CreatePlayerRuleset()).AsNonNull()).Highway;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            AddStep("Chunithm preset, ported skin", () =>
            {
                highwayConfig.SetValue(HighwaySetting.Preset, HighwayPreset.Custom);
                highwayConfig.SetValue(HighwaySetting.Preset, HighwayPreset.Chunithm);
                highwayConfig.SetValue(HighwaySetting.NoteStyle, HighwayNoteStyle.Ported);
                highwayConfig.SetValue(HighwaySetting.HitEffects, HighwayHitEffectStyle.Ported);
                highwayConfig.SetValue(HighwaySetting.Track, HighwayTrackStyle.Ported);
                highwayConfig.SetValue(HighwaySetting.KeyArea, SkinKeyArea.Panel);
                highwayConfig.SetValue(HighwaySetting.StageFit, SkinStageFit.SkinPosition);
                highwayConfig.SetValue(HighwaySetting.SkinNoteProportions, true);
                skins.CurrentSkinInfo.Value = DefaultLegacySkin.CreateInfo().ToLiveUnmanaged();
            });

            base.SetUpSteps();
        }

        [TearDownSteps]
        public override void TearDownSteps()
        {
            base.TearDownSteps();

            AddStep("default skin and track", () =>
            {
                skins.CurrentSkinInfo.SetDefault();
                highwayConfig.SetValue(HighwaySetting.KeyArea, SkinKeyArea.Panel);
            });
        }

        [Test]
        public void TestClassicSkinIsPorted()
        {
            AddUntilStep("ported", () => highway.Port != null);
            AddAssert("one ported column per mania column", () => highway.Port!.Columns.Length, () => Is.EqualTo(4));
            AddAssert("every column has a note image", () => highway.Port!.Columns.All(c => c.Note.Length > 0));
            AddAssert("every column has a key image", () => highway.Port!.Columns.All(c => c.KeyUp != null));

            AddUntilStep("ported track, notes and lighting shown", () => highway.PortedTrackShown && highway.PortedNotesShown && highway.PortedLightingShown);
            AddAssert("the highway's own track hidden", () => !highway.TrackShown);
            AddAssert("the flat stage was measured", () => highway.FlatStage.IsValid);

            AddUntilStep("notes drawn from the skin's images", () =>
            {
                for (int i = 0; i < highway.NoteLayer.Count; i++)
                {
                    if (highway.NoteLayer.GetQuad(i).Texture != null)
                        return true;
                }

                return false;
            });
        }

        [Test]
        public void TestPanelLiftsTheLineToTheSkinsHitPosition()
        {
            float panelLine = 0;

            AddUntilStep("ported", () => highway.Port != null && highway.FlatStage.IsValid);
            AddWaitStep("let it settle", 2);
            AddStep("record the panel's line", () => panelLine = highway.Projection.Layout.JudgementY);

            // The classic skin's hit position (402 of 480) is above Chunithm's judgement line.
            AddStep("squeezed key area", () => highwayConfig.SetValue(HighwaySetting.KeyArea, SkinKeyArea.Squeezed));
            AddUntilStep("the camera's line is kept, lower on screen", () => highway.Projection.Layout.JudgementY, () => Is.GreaterThan(panelLine + 0.01f));

            AddStep("panel key area", () => highwayConfig.SetValue(HighwaySetting.KeyArea, SkinKeyArea.Panel));
            AddUntilStep("lifted again", () => highway.Projection.Layout.JudgementY, () => Is.EqualTo(panelLine).Within(1e-4f));
        }

        [Test]
        public void TestPortedLightingTakesOverHitLighting()
        {
            AddUntilStep("ported lighting shown", () => highway.Port != null && highway.PortedLightingShown);

            AddUntilStep("a hit was lit", () =>
            {
                foreach (double time in highway.PortedLighting.HitTimes)
                {
                    if (!double.IsNegativeInfinity(time))
                        return true;
                }

                return false;
            });

            AddStep("highway hit effects", () => highwayConfig.SetValue(HighwaySetting.HitEffects, HighwayHitEffectStyle.Highway));
            AddUntilStep("ported lighting off", () => !highway.PortedLightingShown);
        }

        [Test]
        public void TestSkinWithoutLegacyManiaImagesKeepsTheHighwayLook()
        {
            AddUntilStep("ported", () => highway.Port != null);

            AddStep("default skin", () => skins.CurrentSkinInfo.SetDefault());
            AddUntilStep("no port", () => highway.Port == null);
            AddUntilStep("the highway's track", () => highway.TrackShown && !highway.PortedTrackShown);
            AddAssert("the highway's notes", () => highway.NativeNotes && !highway.PortedNotesShown);

            AddStep("classic skin", () => skins.CurrentSkinInfo.Value = DefaultLegacySkin.CreateInfo().ToLiveUnmanaged());
            AddUntilStep("ported again", () => highway.Port != null && highway.PortedTrackShown);
        }
    }
}
