// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.ManiaOrtho.Configuration;
using osu.Game.Rulesets.ManiaOrtho.Highway;
using osu.Game.Rulesets.ManiaOrtho.Tests.Playtest;
using osu.Game.Rulesets.ManiaOrtho.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;
using osuTK.Graphics;

namespace osu.Game.Rulesets.ManiaOrtho.Tests.Highway
{
    /// <summary>
    /// Notes drawn natively on the track (SPEC 6): they must sit exactly where mania's own notes would be drawn on the warped
    /// stage, keep a fixed thickness in track units, and follow mania's hold behaviour.
    /// </summary>
    public partial class TestSceneHighwayNativeNotes : PlayerTestScene
    {
        private const float depth_tolerance = 0.003f;

        protected override bool Autoplay => true;

        protected override Ruleset CreatePlayerRuleset() => new ManiaOrthoRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => PlaytestBeatmaps.Load("synthetic-4k");

        private HighwayPlayfieldAdjustmentContainer highway => ((DrawableManiaOrthoRuleset)Player.DrawableRuleset).Highway.AsNonNull();

        private ManiaOrthoRulesetConfigManager rulesetConfig => (ManiaOrthoRulesetConfigManager)RulesetConfigs.GetConfigFor(CreatePlayerRuleset()).AsNonNull();

        private HighwayConfigManager highwayConfig => rulesetConfig.Highway;

        [Resolved]
        private SkinManager skins { get; set; } = null!;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            AddStep("Chunithm preset, native notes, default skin", () =>
            {
                skins.CurrentSkinInfo.SetDefault();
                rulesetConfig.SetValue(ManiaRulesetSetting.TimingBasedNoteColouring, false);
                highwayConfig.SetValue(HighwaySetting.Preset, HighwayPreset.Custom);
                highwayConfig.SetValue(HighwaySetting.Preset, HighwayPreset.Chunithm);
                highwayConfig.SetValue(HighwaySetting.NoteStyle, HighwayNoteStyle.Highway);
            });

            base.SetUpSteps();

            AddUntilStep("notes gathered", () => highway.NoteLayer.Count > 0);
        }

        [TearDownSteps]
        public override void TearDownSteps()
        {
            AddStep("skin notes, default skin", () =>
            {
                highwayConfig.SetValue(HighwaySetting.NoteStyle, HighwayNoteStyle.Skin);
                rulesetConfig.SetValue(ManiaRulesetSetting.TimingBasedNoteColouring, false);
                skins.CurrentSkinInfo.SetDefault();
            });
            base.TearDownSteps();
        }

        [Test]
        public void TestManiaNotesAreNotDrawnInTheStage()
        {
            AddAssert("native notes on", () => highway.NativeNotes && highway.NoteLayer.IsPresent);
            AddAssert("every column's notes are proxied away", () => columns.All(c => c.HitObjectContainer.HasProxy));
            AddAssert("each proxy sits beside its notes", () => columns.Select((c, i) => highway.NoteProxyHolders[i].Parent == c.HitObjectContainer.Parent).All(b => b));
            AddAssert("proxies hidden", () => highway.NoteProxyHolders.All(h => !h.IsPresent));

            AddStep("flat camera", () => highwayConfig.SetValue(HighwaySetting.CameraMode, HighwayCameraMode.Flat));
            AddUntilStep("no native notes when flat", () => !highway.NoteLayer.IsPresent);
            AddAssert("mania's notes drawn through their proxies", () => highway.NoteProxyHolders.Count == columns.Length && highway.NoteProxyHolders.All(h => h.IsPresent));
        }

        [Test]
        public void TestProxiesStayInsideCoverMods()
        {
            AddStep("load with Fade In", () => LoadPlayer(new Mod[] { new ManiaModFadeIn() }));
            AddUntilStep("player loaded", () => Player.IsLoaded && Player.Alpha == 1);
            AddUntilStep("notes gathered", () => highway.NoteLayer.Count > 0);

            AddAssert("each proxy is inside its column's cover, drawn under it", () => highway.NoteProxyHolders.All(h =>
                h.Parent is BufferedContainer cover && cover.Parent is PlayfieldCoveringWrapper && cover.Children[0] == h));
        }

        [Test]
        public void TestNotesUseTheSkinsColumnColours()
        {
            AddAssert("argon's column colours", () => columns.All(c => quadsInColumn(c).All(q => sameColour(q.Far, c.AccentColour.Value))));
        }

        [Test]
        public void TestLegacySkinNotesUseTheBuiltInColours()
        {
            AddStep("classic skin", () => skins.CurrentSkinInfo.Value = DefaultLegacySkin.CreateInfo().ToLiveUnmanaged());
            AddUntilStep("column colours are black", () => columns.All(c => c.AccentColour.Value.R + c.AccentColour.Value.G + c.AccentColour.Value.B < 0.1f));

            AddUntilStep("white outer lanes, pink inner", () =>
            {
                Color4[] expected = { HighwayNoteLayer.OUTER_COLOUR, HighwayNoteLayer.INNER_COLOUR, HighwayNoteLayer.INNER_COLOUR, HighwayNoteLayer.OUTER_COLOUR };
                var notes = columns.Select(quadsInColumn).ToArray();

                return notes.Any(n => n.Length > 0) && notes.Select((n, i) => n.All(q => sameColour(q.Far, expected[i]))).All(b => b);
            });
        }

        [Test]
        public void TestTimingBasedColouring()
        {
            AddStep("timing-based colouring", () => rulesetConfig.SetValue(ManiaRulesetSetting.TimingBasedNoteColouring, true));

            AddUntilStep("notes take their snap colour", () =>
            {
                int checkedNotes = 0;

                foreach (var note in columns.SelectMany(c => c.HitObjectContainer.AliveObjects).OfType<DrawableNote>())
                {
                    var quads = quadsFor(note.HitObject).Where(q => q.Kind == HighwayNoteLayer.NoteQuadKind.Note && q.Far.A > 0).ToArray();

                    if (quads.Length == 0)
                        continue;

                    if (!sameColour(quads.MaxBy(q => q.Depth1).Far, note.Colour.TopLeft.SRGB))
                        return false;

                    checkedNotes++;
                }

                return checkedNotes > 0;
            });

        }

        [Test]
        public void TestNotesSitWhereTheWarpedNotesWould()
        {
            AddAssert("leading edges match the warped stage", () =>
            {
                int checkedNotes = 0;

                foreach (var note in drawableRuleset.Playfield.Stages.SelectMany(s => s.Columns).SelectMany(c => c.HitObjectContainer.AliveObjects).OfType<DrawableNote>())
                {
                    if (note.Alpha <= 0 || note.Judged)
                        continue;

                    float expected = warpedDepth(note);
                    var quad = quadsFor(note.HitObject).Where(q => q.Kind == HighwayNoteLayer.NoteQuadKind.Note).MinBy(q => q.Depth0);

                    if (Math.Abs(quad.Depth0 - expected) > depth_tolerance)
                        throw new AssertionException($"note at {note.HitObject.StartTime}: native {quad.Depth0}, warped {expected}");

                    checkedNotes++;
                }

                return checkedNotes > 0;
            });
        }

        [Test]
        public void TestNotesHaveAFixedThicknessOnTheTrack()
        {
            AddAssert("every note is the same depth thick", () =>
            {
                var notes = Enumerable.Range(0, highway.NoteLayer.Count).Select(highway.NoteLayer.GetQuad).Where(q => q.Kind == HighwayNoteLayer.NoteQuadKind.Note)
                                      .GroupBy(q => q.HitObject).ToArray();

                return notes.Length > 0 && notes.All(g => Math.Abs(g.Max(q => q.Depth1) - g.Min(q => q.Depth0) - HighwayNoteLayer.NOTE_THICKNESS) < 1e-4f);
            });
        }

        [Test]
        public void TestHeldNoteStopsAtTheLine()
        {
            DrawableHoldNote held = null!;

            AddUntilStep("a hold note is being held", () =>
            {
                held = drawableRuleset.Playfield.Stages.SelectMany(s => s.Columns).SelectMany(c => c.HitObjectContainer.AliveObjects).OfType<DrawableHoldNote>()
                                      .FirstOrDefault(h => h.IsHolding.Value && h.Time.Current > h.HitObject.StartTime + 50 && h.Time.Current < h.HitObject.EndTime - 200)!;

                // Freeze it there, so the following checks see the same frame.
                if (held != null)
                    Player.GameplayClockContainer.Stop();

                return held != null;
            });

            AddWaitStep("let the highway gather", 2);

            AddAssert("its head sits on the line", () => quadsFor(held.HitObject).Where(q => q.Kind == HighwayNoteLayer.NoteQuadKind.HoldHead).Min(q => q.Depth0),
                () => Is.EqualTo(0).Within(1e-4));

            AddAssert("its body starts under the head", () => quadsFor(held.HitObject).Single(q => q.Kind == HighwayNoteLayer.NoteQuadKind.HoldBody).Depth0,
                () => Is.EqualTo(HighwayNoteLayer.NOTE_THICKNESS / 2).Within(1e-4));

            AddAssert("its tail is where the warped tail is", () =>
            {
                float tail = quadsFor(held.HitObject).Where(q => q.Kind == HighwayNoteLayer.NoteQuadKind.HoldTail).Min(q => q.Depth0);
                return Math.Abs(tail - warpedDepth(held.Tail)) < depth_tolerance;
            });
        }

        private DrawableManiaOrthoRuleset drawableRuleset => (DrawableManiaOrthoRuleset)Player.DrawableRuleset;

        private Column[] columns => drawableRuleset.Playfield.Stages.SelectMany(s => s.Columns).ToArray();

        /// <summary>
        /// The main (coloured) quad of every visible tap note in <paramref name="column"/>: the far part of the bar, after
        /// the white rim.
        /// </summary>
        private HighwayNoteLayer.NoteQuad[] quadsInColumn(Column column)
            => column.HitObjectContainer.AliveObjects.OfType<DrawableNote>().Where(n => n is not DrawableHoldNoteHead)
                     .Select(n => quadsFor(n.HitObject).Where(q => q.Kind == HighwayNoteLayer.NoteQuadKind.Note).ToArray())
                     .Where(q => q.Length == 2 && q[0].Far.A > 0)
                     .Select(q => q.MaxBy(x => x.Depth1)).ToArray();

        private static bool sameColour(Color4 drawn, Color4 expected)
            => Math.Abs(drawn.R - expected.R) < 0.01f && Math.Abs(drawn.G - expected.G) < 0.01f && Math.Abs(drawn.B - expected.B) < 0.01f;

        private HighwayNoteLayer.NoteQuad[] quadsFor(HitObject hitObject)
            => Enumerable.Range(0, highway.NoteLayer.Count).Select(highway.NoteLayer.GetQuad).Where(q => q.HitObject == hitObject).ToArray();

        /// <summary>
        /// Where the warp would draw <paramref name="drawable"/>'s leading edge: mania places it in the flat stage, and the warp
        /// maps the stage's scrolling area linearly onto the track up to the scroll length.
        /// </summary>
        private float warpedDepth(DrawableManiaHitObject drawable)
        {
            var source = highway.Warp.Source;
            float y = highway.Warp.ToLocalSpace(drawable.ScreenSpaceDrawQuad.BottomLeft).Y;

            return (source.Bottom - y) / source.Height * highway.Warp.Scroll.ScrollLength;
        }
    }
}
