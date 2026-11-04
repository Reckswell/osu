// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration.Tracking;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Overlays;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.ManiaOrtho.Configuration;
using osu.Game.Rulesets.ManiaOrtho.Highway;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.ManiaOrtho.Tests.Configuration
{
    /// <summary>
    /// Settings plumbing: values persist, presets fill in values, editing a value makes the preset Custom, and the
    /// settings subsection loads. Headless under <c>dotnet test</c>; run it through the playtest runner with
    /// <c>--scene TestSceneHighwaySettings --screenshot out.png</c> to see the panel.
    /// </summary>
    public partial class TestSceneHighwaySettings : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private SettingsStore store = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create settings store", () => store = new SettingsStore(Realm));
            AddStep("reset highway settings", () =>
            {
                using (var config = create())
                {
                    config.Highway.SetValue(HighwaySetting.Preset, HighwayPreset.Chunithm);
                    config.Highway.SetValue(HighwaySetting.WallKeepsVisibleTime, true);
                }
            });
        }

        [Test]
        public void TestValuesPersist()
        {
            AddStep("write custom values", () =>
            {
                using (var config = create())
                {
                    config.Highway.SetValue(HighwaySetting.FarWidthRatio, 0.33f);
                    config.Highway.SetValue(HighwaySetting.Wall, 0.25f);
                    config.Highway.SetValue(HighwaySetting.NoteThickness, 0.04f);
                    config.Highway.SetValue(HighwaySetting.WallKeepsVisibleTime, false);
                    config.SetValue(ManiaRulesetSetting.ScrollSpeed, 29.0);
                }
            });

            AddAssert("values read back", () =>
            {
                using (var config = create())
                {
                    var current = config.Highway.Current;
                    return near(current.FarWidthRatio, 0.33f)
                           && near(current.Wall, 0.25f)
                           && near(current.NoteThickness, 0.04f)
                           && !current.WallKeepsVisibleTime
                           && config.Highway.Get<HighwayPreset>(HighwaySetting.Preset) == HighwayPreset.Custom
                           && config.Get<double>(ManiaRulesetSetting.ScrollSpeed) == 29.0;
                }
            });
        }

        [Test]
        public void TestNoteThicknessReachesBarsOfAlphasSkin()
        {
            // Alpha's 4K note bars are 6.4% of the scroll length (research/game-osu-mania.md, finding 7); under the Parallel
            // camera the setting is the same measure, so the range must reach it.
            AddAssert("8% is the most", () =>
            {
                using (var config = create())
                    return ((BindableFloat)config.Highway.GetBindable<float>(HighwaySetting.NoteThickness)).MaxValue;
            }, () => Is.EqualTo(0.08f));

            AddStep("set 6.4%", () =>
            {
                using (var config = create())
                    config.Highway.SetValue(HighwaySetting.NoteThickness, 0.064f);
            });

            AddAssert("kept, not clamped", () =>
            {
                using (var config = create())
                    return config.Highway.Get<float>(HighwaySetting.NoteThickness);
            }, () => Is.EqualTo(0.064f).Within(1e-6f));
        }

        /// <summary>
        /// Alpha's skin port request: pick a skin and play. Skins without legacy mania images fall back to the highway's look.
        /// </summary>
        [Test]
        public void TestNotesDrawnFromThePortedSkinByDefault()
        {
            AddAssert("settings default", () => new HighwaySettings().NoteStyle, () => Is.EqualTo(HighwayNoteStyle.Ported));
            AddAssert("config default", () =>
            {
                using (var config = create())
                    return config.Highway.GetBindable<HighwayNoteStyle>(HighwaySetting.NoteStyle).Default;
            }, () => Is.EqualTo(HighwayNoteStyle.Ported));
        }

        [Test]
        public void TestPortedTrackByDefault()
        {
            AddAssert("settings default", () => new HighwaySettings().Track, () => Is.EqualTo(HighwayTrackStyle.Ported));
            AddAssert("config default", () =>
            {
                using (var config = create())
                    return config.Highway.GetBindable<HighwayTrackStyle>(HighwaySetting.Track).Default;
            }, () => Is.EqualTo(HighwayTrackStyle.Ported));

            AddAssert("a preset leaves it alone", () =>
            {
                using (var config = create())
                {
                    config.Highway.SetValue(HighwaySetting.Track, HighwayTrackStyle.Skin);
                    config.Highway.SetValue(HighwaySetting.Preset, HighwayPreset.Voltex);
                    return config.Highway.Current.Track;
                }
            }, () => Is.EqualTo(HighwayTrackStyle.Skin));
        }

        /// <summary>
        /// An install from before the skin porter stored "Highway" for the three styles the first time it ran; they move to
        /// the porter once, a style chosen otherwise is kept, and choosing "Highway" again afterwards sticks.
        /// </summary>
        [Test]
        public void TestOldDefaultsMoveToThePorterOnce()
        {
            AddStep("store the old defaults", () =>
            {
                using (var config = create())
                {
                    config.Highway.SetValue(HighwaySetting.NoteStyle, HighwayNoteStyle.Highway);
                    config.Highway.SetValue(HighwaySetting.HitEffects, HighwayHitEffectStyle.Highway);
                    config.Highway.SetValue(HighwaySetting.Track, HighwayTrackStyle.Skin);
                    config.Highway.SetValue(HighwaySetting.SkinPortDefaultsApplied, false);
                }
            });

            AddAssert("old defaults moved, a chosen style kept", () =>
            {
                using (var config = create())
                {
                    var current = config.Highway.Current;
                    return current.NoteStyle == HighwayNoteStyle.Ported
                           && current.HitEffects == HighwayHitEffectStyle.Ported
                           && current.Track == HighwayTrackStyle.Skin
                           && config.Highway.Get<bool>(HighwaySetting.SkinPortDefaultsApplied);
                }
            });

            AddStep("choose the highway's notes again", () =>
            {
                using (var config = create())
                    config.Highway.SetValue(HighwaySetting.NoteStyle, HighwayNoteStyle.Highway);
            });

            AddAssert("kept", () =>
            {
                using (var config = create())
                    return config.Highway.Get<HighwayNoteStyle>(HighwaySetting.NoteStyle);
            }, () => Is.EqualTo(HighwayNoteStyle.Highway));
        }

        [Test]
        public void TestWallChangesShowAToast()
        {
            AddStep("move the wall", () =>
            {
                using (var config = create())
                {
                    var tracked = config.Highway.CreateTrackedSettings();
                    SettingDescription? shown = null;

                    config.Highway.LoadInto(tracked);
                    tracked.SettingChanged += description => shown = description;

                    config.Highway.SetValue(HighwaySetting.Wall, 0.05f);
                    Assert.That(shown?.Name.ToString(), Is.EqualTo("Wall"));
                    Assert.That(shown?.Value.ToString(), Is.EqualTo("5% covered"));
                    Assert.That(shown?.Shortcut.ToString(), Is.EqualTo(HighwayConfigManager.WALL_SHORTCUT));

                    config.Highway.SetValue(HighwaySetting.Wall, 0f);
                    Assert.That(shown?.Value.ToString(), Is.EqualTo("Off"));

                    tracked.Unload();
                }
            });
        }

        [Test]
        public void TestPresets()
        {
            AddAssert("default is Chunithm", () =>
            {
                using (var config = create())
                    return same(config.Highway.Current, HighwayPresets.Get(HighwayPreset.Chunithm));
            });

            AddAssert("choosing a preset applies it", () =>
            {
                using (var config = create())
                {
                    config.Highway.SetValue(HighwaySetting.Preset, HighwayPreset.Voltex);
                    var voltex = HighwayPresets.Get(HighwayPreset.Voltex);
                    return same(config.Highway.Current, voltex) && config.Highway.Get<HighwayPreset>(HighwaySetting.Preset) == HighwayPreset.Voltex;
                }
            });

            AddAssert("Voltex notes look about as tall at the line as Chunithm's", () => heightAtLine(HighwayPreset.Voltex) / heightAtLine(HighwayPreset.Chunithm),
                () => Is.InRange(0.85f, 1.25f));

            AddAssert("note thickness is part of a preset", () =>
            {
                using (var config = create())
                {
                    config.Highway.SetValue(HighwaySetting.NoteThickness, 0.03f);
                    return config.Highway.Get<HighwayPreset>(HighwaySetting.Preset) == HighwayPreset.Custom;
                }
            });

            AddAssert("editing a value makes it Custom", () =>
            {
                using (var config = create())
                {
                    config.Highway.SetValue(HighwaySetting.TrackWidth, 0.9f);
                    return config.Highway.Get<HighwayPreset>(HighwaySetting.Preset) == HighwayPreset.Custom
                           && near(config.Highway.Get<float>(HighwaySetting.TrackWidth), 0.9f);
                }
            });

            AddAssert("Classic switches to flat", () =>
            {
                using (var config = create())
                {
                    config.Highway.SetValue(HighwaySetting.Preset, HighwayPreset.Classic);
                    return config.Highway.Current.CameraMode == HighwayCameraMode.Flat;
                }
            });
        }

        [Test]
        public void TestSettingsPanel()
        {
            AddStep("show settings", () =>
            {
                Child = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 480,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new Box { RelativeSizeAxes = Axes.Both, Colour = colourProvider.Background4 },
                        new ManiaOrthoSettingsSubsection(new ManiaOrthoRuleset()),
                    }
                };
            });

            AddUntilStep("panel loaded", () => Child.IsLoaded);
        }

        private ManiaOrthoRulesetConfigManager create() => new ManiaOrthoRulesetConfigManager(store, new ManiaOrthoRuleset().RulesetInfo);

        // Bindables round to their precision (0.01), so compare with a tolerance.
        private static bool near(float a, float b) => System.Math.Abs(a - b) < 1e-4f;

        /// <summary>
        /// How tall a note on the judgement line is drawn with <paramref name="preset"/>, as a share of the screen height.
        /// </summary>
        private static float heightAtLine(HighwayPreset preset)
        {
            var settings = HighwayPresets.Get(preset);
            var projection = new HighwayProjection(settings.ToLayout(), new osuTK.Vector2(1920, 1080));

            return (projection.YAt(0) - projection.YAt(settings.NoteThickness)) / 1080;
        }

        private static bool same(HighwaySettings a, HighwaySettings b)
            => a.CameraMode == b.CameraMode
               && near(a.FarWidthRatio, b.FarWidthRatio)
               && near(a.JudgementLineHeight, b.JudgementLineHeight)
               && near(a.FarEdgeHeight, b.FarEdgeHeight)
               && near(a.TrackWidth, b.TrackWidth)
               && near(a.Wall, b.Wall)
               && near(a.NoteThickness, b.NoteThickness);
    }
}
