// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Framework.Testing;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.ManiaOrtho.Configuration;
using osu.Game.Rulesets.ManiaOrtho.Highway;
using osu.Game.Rulesets.Mods;
using osu.Game.Skinning;
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

        [Resolved]
        private SkinManager skins { get; set; } = null!;

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
                    config.Highway.GetBindable<float>(HighwaySetting.HoldOpacity).SetDefault();
                    config.Highway.GetBindable<float>(HighwaySetting.TrackOpacity).SetDefault();
                    config.Highway.GetBindable<KeyBeamLength>(HighwaySetting.KeyBeam).SetDefault();
                }
            });
        }

        /// <summary>
        /// Sekai's Long Note Transparency and Lane Transparency (research/game-sekai.md K6, K9): preferences that start at
        /// the look as designed, outside the presets.
        /// </summary>
        [Test]
        public void TestOpacities()
        {
            AddAssert("both default to full", () => new[] { new HighwaySettings().HoldOpacity, new HighwaySettings().TrackOpacity }, () => Is.EqualTo(new[] { 1f, 1f }));

            AddAssert("config defaults to full", () =>
            {
                using (var config = create())
                    return new[] { config.Highway.Current.HoldOpacity, config.Highway.Current.TrackOpacity };
            }, () => Is.EqualTo(new[] { 1f, 1f }));

            AddAssert("holds never vanish, the track can", () =>
            {
                using (var config = create())
                {
                    config.Highway.SetValue(HighwaySetting.HoldOpacity, 0f);
                    config.Highway.SetValue(HighwaySetting.TrackOpacity, 0f);
                    return new[] { config.Highway.Current.HoldOpacity, config.Highway.Current.TrackOpacity };
                }
            }, () => Is.EqualTo(new[] { HighwaySettings.MIN_HOLD_OPACITY, 0f }).Within(1e-6f));

            AddAssert("changing them keeps the preset", () =>
            {
                using (var config = create())
                {
                    config.Highway.SetValue(HighwaySetting.HoldOpacity, 0.7f);
                    config.Highway.SetValue(HighwaySetting.TrackOpacity, 0.7f);
                    return config.Highway.Get<HighwayPreset>(HighwaySetting.Preset);
                }
            }, () => Is.EqualTo(HighwayPreset.Chunithm));

            AddAssert("choosing a preset leaves them", () =>
            {
                using (var config = create())
                {
                    config.Highway.SetValue(HighwaySetting.HoldOpacity, 0.4f);
                    config.Highway.SetValue(HighwaySetting.TrackOpacity, 0.5f);
                    config.Highway.SetValue(HighwaySetting.Preset, HighwayPreset.Voltex);
                    return new[] { config.Highway.Current.HoldOpacity, config.Highway.Current.TrackOpacity };
                }
            }, () => Is.EqualTo(new[] { 0.4f, 0.5f }).Within(1e-4f));
        }

        /// <summary>
        /// DJMAX's beam lengths (research/game-djmax.md R2): a preference that starts at today's long beam, outside the presets.
        /// </summary>
        [Test]
        public void TestKeyBeam()
        {
            AddAssert("defaults to long", () =>
            {
                using (var config = create())
                    return new[] { new HighwaySettings().KeyBeam, config.Highway.Current.KeyBeam };
            }, () => Is.EqualTo(new[] { KeyBeamLength.Long, KeyBeamLength.Long }));

            AddAssert("kept, and keeps the preset", () =>
            {
                using (var config = create())
                    config.Highway.SetValue(HighwaySetting.KeyBeam, KeyBeamLength.Quarter);

                using (var config = create())
                    return (config.Highway.Current.KeyBeam, config.Highway.Get<HighwayPreset>(HighwaySetting.Preset));
            }, () => Is.EqualTo((KeyBeamLength.Quarter, HighwayPreset.Chunithm)));

            AddAssert("choosing a preset leaves it", () =>
            {
                using (var config = create())
                {
                    config.Highway.SetValue(HighwaySetting.Preset, HighwayPreset.Voltex);
                    return config.Highway.Current.KeyBeam;
                }
            }, () => Is.EqualTo(KeyBeamLength.Quarter));
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

        /// <summary>
        /// The scroll speed's label shows how long notes are visible on the highway, not mania's <c>11485 / speed</c>: the
        /// skin's hit position changes it (the default skin's line is 14.8 units higher than the classic skin's, +2.3%), and
        /// so does a wall that keeps the speed, except under the flat camera (no wall). Toasts in gameplay:
        /// <see cref="ManiaOrtho.Tests.Highway.TestSceneVisibleTime"/>.
        /// </summary>
        [Test]
        public void TestScrollSpeedShowsTheVisibleTime()
        {
            AddStep("speed 29, no wall, default skin", () =>
            {
                skins.CurrentSkinInfo.Value = ArgonSkin.CreateInfo().ToLiveUnmanaged();
                speedWithoutWall();
            });
            showPanel();
            AddUntilStep("405 ms (mania's label: 396)", speedLabel, () => Is.EqualTo("405ms (speed 29.0)"));

            AddStep("40% wall keeping the speed", () => gameConfig.Highway.SetValue(HighwaySetting.Wall, 0.4f));
            AddUntilStep("243 ms", speedLabel, () => Is.EqualTo("243ms (speed 29.0)"));

            AddStep("flat camera", () => gameConfig.Highway.SetValue(HighwaySetting.CameraMode, HighwayCameraMode.Flat));
            AddUntilStep("405 ms: no wall when flat", speedLabel, () => Is.EqualTo("405ms (speed 29.0)"));

            // The classic skin keeps osu!'s default line (HitPosition 402), where mania's label is right.
            AddStep("classic skin", () => skins.CurrentSkinInfo.Value = DefaultLegacySkin.CreateInfo().ToLiveUnmanaged());
            AddUntilStep("396 ms", speedLabel, () => Is.EqualTo("396ms (speed 29.0)"));
        }

        /// <summary>
        /// A legacy skin gives a hit position per key count, so the label uses the selected beatmap's, as mania converts it
        /// (key mods included): here a skin whose 7K line is at the bottom of the stage (19.4% longer) and whose others are
        /// osu!'s default.
        /// </summary>
        [Test]
        public void TestScrollSpeedUsesTheBeatmapsKeyCount()
        {
            AddStep("speed 29, no wall", speedWithoutWall);
            AddStep("a 7K beatmap", () => Beatmap.Value = CreateWorkingBeatmap(maniaBeatmap(7)));
            showPanel(new SevenKeyLineAtTheBottom());
            AddUntilStep("473 ms", speedLabel, () => Is.EqualTo("473ms (speed 29.0)"));

            AddStep("a 4K beatmap", () => Beatmap.Value = CreateWorkingBeatmap(maniaBeatmap(4)));
            AddUntilStep("396 ms", speedLabel, () => Is.EqualTo("396ms (speed 29.0)"));

            AddStep("an osu! beatmap with 7K selected", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(new Beatmap { BeatmapInfo = { Ruleset = new RulesetInfo("osu", "osu!", string.Empty, 0) } });
                SelectedMods.Value = new Mod[] { new ManiaModKey7() };
            });
            AddUntilStep("473 ms", speedLabel, () => Is.EqualTo("473ms (speed 29.0)"));

            static IBeatmap maniaBeatmap(int keys) => new ManiaBeatmap(new StageDefinition(keys))
            {
                BeatmapInfo = { Ruleset = new ManiaRuleset().RulesetInfo, Difficulty = { CircleSize = keys } },
            };
        }

        [TearDownSteps]
        public void TearDownSteps()
        {
            AddStep("default skin, highway, beatmap and mods", () =>
            {
                skins.CurrentSkinInfo.SetDefault();
                gameConfig.GetBindable<double>(ManiaRulesetSetting.ScrollSpeed).SetDefault();
                gameConfig.Highway.GetBindable<bool>(HighwaySetting.WallKeepsVisibleTime).SetDefault();
                gameConfig.Highway.GetBindable<HighwayPreset>(HighwaySetting.Preset).SetDefault();
                Beatmap.SetDefault();
                SelectedMods.SetDefault();
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
            showPanel();
        }

        /// <param name="skin">A skin to show the panel with, in place of the game's.</param>
        private void showPanel(ISkinSource? skin = null)
        {
            AddStep("show settings", () =>
            {
                var panel = new Container
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

                Child = skin == null
                    ? panel
                    : new DependencyProvidingContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        CachedDependencies = new (Type, object)[] { (typeof(ISkinSource), skin) },
                        Child = panel,
                    };
            });

            AddUntilStep("panel loaded", () => Child.IsLoaded);
        }

        /// <summary>
        /// The game's config for this ruleset, which the settings panel shows.
        /// </summary>
        private ManiaOrthoRulesetConfigManager gameConfig => (ManiaOrthoRulesetConfigManager)RulesetConfigs.GetConfigFor(new ManiaOrthoRuleset()).AsNonNull();

        private void speedWithoutWall()
        {
            gameConfig.SetValue(ManiaRulesetSetting.ScrollSpeed, 29.0);
            gameConfig.Highway.SetValue(HighwaySetting.Preset, HighwayPreset.Custom);
            gameConfig.Highway.SetValue(HighwaySetting.Preset, HighwayPreset.Chunithm);
            gameConfig.Highway.SetValue(HighwaySetting.WallKeepsVisibleTime, false);
        }

        private string speedLabel() => this.ChildrenOfType<OsuSpriteText>().Select(t => t.Current.Value).FirstOrDefault(t => t.Contains("(speed ", StringComparison.Ordinal)) ?? string.Empty;

        /// <summary>
        /// A legacy skin's mania settings, as far as the speed label asks: HitPosition 480 (the line at the bottom) for 7K,
        /// osu!'s default for every other key count.
        /// </summary>
        private class SevenKeyLineAtTheBottom : ISkinSource
        {
            public event Action? SourceChanged
            {
                add { }
                remove { }
            }

            public IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup)
                where TLookup : notnull
                where TValue : notnull
            {
                if (lookup is LegacyManiaSkinConfigurationLookup { Lookup: LegacyManiaSkinConfigurationLookups.HitPosition } mania && typeof(TValue) == typeof(float))
                    return (IBindable<TValue>)(object)new Bindable<float>(mania.TotalColumns == 7 ? 0 : LegacyManiaSkinConfiguration.DEFAULT_HIT_POSITION);

                return null;
            }

            public Drawable? GetDrawableComponent(ISkinComponentLookup lookup) => null;

            public Texture? GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) => null;

            public ISample? GetSample(ISampleInfo sampleInfo) => null;

            public ISkin? FindProvider(Func<ISkin, bool> lookupFunction) => lookupFunction(this) ? this : null;

            public IEnumerable<ISkin> AllSources => new ISkin[] { this };
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
