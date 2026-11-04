// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.ManiaOrtho.Configuration;
using osu.Game.Rulesets.ManiaOrtho.Highway;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring.Legacy;
using osu.Game.Skinning;
using osu.Game.Utils;

namespace osu.Game.Rulesets.ManiaOrtho
{
    /// <summary>
    /// This ruleset's section in the settings overlay: mania's scroll speed and note colouring, then the highway.
    /// Mania's scroll direction is left out on purpose: the camera decides the direction (SPEC section 3).
    /// </summary>
    public partial class ManiaOrthoSettingsSubsection : RulesetSettingsSubsection
    {
        [Resolved]
        private ISkinSource? skin { get; set; }

        [Resolved]
        private IBindable<WorkingBeatmap>? beatmap { get; set; }

        [Resolved]
        private IBindable<IReadOnlyList<Mod>>? mods { get; set; }

        private HighwayConfigManager highway = null!;
        private FormSliderBar<double> scrollSpeed = null!;

        /// <summary>
        /// The values the scroll speed's visible time depends on, kept to show it again when one changes.
        /// </summary>
        private readonly List<IUnbindable> visibleTimeInputs = new List<IUnbindable>();

        public ManiaOrthoSettingsSubsection(ManiaOrthoRuleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (ManiaOrthoRulesetConfigManager)Config;
            highway = config.Highway;

            Children = new Drawable[]
            {
                new SettingsItemV2(scrollSpeed = new FormSliderBar<double>
                {
                    Caption = RulesetSettingsStrings.ScrollSpeed,
                    HintText = "Shown as the time notes are visible, from the wall (or the far end) to the judgement line, with the "
                               + "current skin, wall and camera at the selected beatmap's key count. In gameplay, F3 and F4 change it.",
                    Current = config.GetBindable<double>(ManiaRulesetSetting.ScrollSpeed),
                    KeyboardStep = 1,
                    LabelFormat = scrollSpeedLabel,
                }),
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = RulesetSettingsStrings.TimingBasedColouring,
                    Current = config.GetBindable<bool>(ManiaRulesetSetting.TimingBasedNoteColouring),
                })
                {
                    Keywords = new[] { "color" },
                },
                new SettingsItemV2(new FormEnumDropdown<HighwayPreset>
                {
                    Caption = "Highway preset",
                    HintText = "Chunithm, Voltex (SDVX), Parallel or Classic (osu!mania's layout). Changing any value below makes it Custom.",
                    Current = highway.GetBindable<HighwayPreset>(HighwaySetting.Preset),
                })
                {
                    Keywords = new[] { "camera", "chunithm", "sdvx", "voltex" },
                },
                new SettingsItemV2(new FormEnumDropdown<HighwayCameraMode>
                {
                    Caption = "Camera",
                    Current = highway.GetBindable<HighwayCameraMode>(HighwaySetting.CameraMode),
                })
                {
                    Keywords = new[] { "perspective", "tilt", "flat" },
                },
                new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = "Far edge width",
                    HintText = "Width of the far end of the track compared to the judgement line. Lower is a stronger perspective.",
                    Current = highway.GetBindable<float>(HighwaySetting.FarWidthRatio),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                }),
                new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = "Track width",
                    HintText = "Width of the track at the judgement line, as a share of the screen width.",
                    Current = highway.GetBindable<float>(HighwaySetting.TrackWidth),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                }),
                new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = "Judgement line height",
                    HintText = "Distance of the judgement line above the bottom of the screen (lift).",
                    Current = highway.GetBindable<float>(HighwaySetting.JudgementLineHeight),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                })
                {
                    Keywords = new[] { "lift", "hit position" },
                },
                new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = "Far edge height",
                    HintText = "Distance of the far end of the track below the top of the screen.",
                    Current = highway.GetBindable<float>(HighwaySetting.FarEdgeHeight),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                }),
                new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = "Wall",
                    HintText = "How much of the far end of the track is covered. Notes appear at the wall. In gameplay, Shift+F3 "
                               + "and Shift+F4 move it (F3 and F4 change the scroll speed).",
                    Current = highway.GetBindable<float>(HighwaySetting.Wall),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                })
                {
                    Keywords = new[] { "sudden", "cover", "field wall" },
                },
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = "Wall keeps visible time",
                    HintText = "On: moving the wall slows notes so they stay visible for the same time. Off: notes keep their speed.",
                    Current = highway.GetBindable<bool>(HighwaySetting.WallKeepsVisibleTime),
                }),
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = "Lane guides",
                    HintText = "Thin lines along the lane boundaries that fade into the distance, as in Chunithm.",
                    Current = highway.GetBindable<bool>(HighwaySetting.LaneGuides),
                })
                {
                    Keywords = new[] { "guidelines", "dividers", "column lines" },
                },
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = "Use skin hit target",
                    HintText = "On: the skin's hit target marks the judgement line. Off: a Chunithm-style judgement line does.",
                    Current = highway.GetBindable<bool>(HighwaySetting.SkinHitTarget),
                })
                {
                    Keywords = new[] { "judgement line", "receptor", "stage hint" },
                },
                new SettingsItemV2(new FormEnumDropdown<HighwayNoteStyle>
                {
                    Caption = "Notes",
                    HintText = "Skin (ported): your skin's note images, redrawn sharp on the track. Skin (warped): your skin's "
                               + "notes tilted with the rest of its stage. Highway: notes drawn on the track at the preset's thickness, in "
                               + "your skin's colours or white and pink lanes. Ported needs a stable (legacy) skin. Applies from the next play.",
                    Current = highway.GetBindable<HighwayNoteStyle>(HighwaySetting.NoteStyle),
                })
                {
                    Keywords = new[] { "note skin", "thickness", "native" },
                },
                new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = "Note thickness",
                    HintText = "How deep Highway notes are, as a share of the track's length. Chunithm's notes are 2.55%.",
                    Current = highway.GetBindable<float>(HighwaySetting.NoteThickness),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.0005f,
                })
                {
                    Keywords = new[] { "note size", "note height" },
                },
                new SettingsItemV2(new FormEnumDropdown<HighwayHitEffectStyle>
                {
                    Caption = "Hit effects",
                    HintText = "Highway: bursts on the track at the judgement line, gold for Perfect and orange for Great, and a glow "
                               + "while you hold a note. Skin (ported): your skin's hit and hold lighting, upright at the line. Skin "
                               + "(warped): your skin's hit explosions, tilted with the track.",
                    Current = highway.GetBindable<HighwayHitEffectStyle>(HighwaySetting.HitEffects),
                })
                {
                    Keywords = new[] { "hit lighting", "explosion", "burst", "glow" },
                },
                new SettingsItemV2(new FormEnumDropdown<HighwayTrackStyle>
                {
                    Caption = "Track",
                    HintText = "Highway: a dark floor with teal side rails, and a beam rising from the judgement line in each lane you "
                               + "press. Skin (ported): your skin's stage rebuilt on the track from its images, with its keys under the line. "
                               + "Skin (warped): your skin's stage, tilted with the track.",
                    Current = highway.GetBindable<HighwayTrackStyle>(HighwaySetting.Track),
                })
                {
                    Keywords = new[] { "stage", "lane background", "column", "receptor", "key area", "floor", "beam", "lane lighting", "port" },
                },
                new SettingsItemV2(new FormEnumDropdown<SkinKeyArea>
                {
                    Caption = "Skin key area",
                    HintText = "Where a ported skin's keys go when its judgement line sits high (IIDX-style skins). Panel and Floor lift "
                               + "the line to the skin's height; the others keep the camera's line.",
                    Current = highway.GetBindable<SkinKeyArea>(HighwaySetting.KeyArea),
                })
                {
                    Keywords = new[] { "keys", "receptor", "iidx", "hit position", "lift", "panel" },
                },
                new SettingsItemV2(new FormEnumDropdown<SkinStageFit>
                {
                    Caption = "Skin stage position",
                    HintText = "Camera: the preset's track, centred. Skin position: centred where the skin puts its stage. Skin "
                               + "position and width: the skin's own stage width too.",
                    Current = highway.GetBindable<SkinStageFit>(HighwaySetting.StageFit),
                })
                {
                    Keywords = new[] { "column start", "stage width", "centre", "center", "left" },
                },
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = "Skin note proportions",
                    HintText = "On: ported notes are as tall at the judgement line, for their width, as your skin draws them. Off: they "
                               + "use the note thickness above.",
                    Current = highway.GetBindable<bool>(HighwaySetting.SkinNoteProportions),
                })
                {
                    Keywords = new[] { "note height", "note size", "thickness" },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            showVisibleTimeOnChange(highway.GetBindable<float>(HighwaySetting.Wall));
            showVisibleTimeOnChange(highway.GetBindable<bool>(HighwaySetting.WallKeepsVisibleTime));
            showVisibleTimeOnChange(highway.GetBindable<HighwayCameraMode>(HighwaySetting.CameraMode));

            if (beatmap != null)
                showVisibleTimeOnChange(beatmap.GetBoundCopy());
            if (mods != null)
                showVisibleTimeOnChange(mods.GetBoundCopy());

            if (skin != null)
                skin.SourceChanged += showVisibleTime;
        }

        private void showVisibleTimeOnChange<T>(IBindable<T> input)
        {
            visibleTimeInputs.Add(input);
            input.BindValueChanged(_ => showVisibleTime());
        }

        // Setting the label's format shows the value again. Once, however many inputs changed while the panel was hidden.
        private void showVisibleTime() => Scheduler.AddOnce(formatScrollSpeed);

        private void formatScrollSpeed() => scrollSpeed.LabelFormat = scrollSpeedLabel;

        /// <summary>
        /// Mania's label ("405ms (speed 29.0)"), with how long notes are visible on the highway in place of
        /// <c>11485 / speed</c>: the skin's hit position for the selected beatmap's key count (and key mods) changes it as in
        /// mania, and the wall when it keeps the speed. A beatmap's own skin can change it in gameplay too; the toast there shows
        /// the exact value.
        /// </summary>
        private LocalisableString scrollSpeedLabel(double speed)
        {
            var settings = highway.Current;
            var selectedMods = mods?.Value ?? Array.Empty<Mod>();
            float hitPosition = skin == null ? Stage.HIT_TARGET_POSITION : VisibleTime.HitPosition(skin, keyCount(selectedMods));
            double visible = VisibleTime.Of(DrawableManiaRuleset.ComputeScrollTime(speed), hitPosition, ModUtils.CalculateRateWithMods(selectedMods), settings.ToScroll(),
                settings.CameraMode == HighwayCameraMode.Flat);

            return RulesetSettingsStrings.ScrollSpeedTooltip((int)Math.Round(visible), speed);
        }

        /// <summary>
        /// The selected beatmap's key count as mania converts it (4 with none selected).
        /// </summary>
        private int keyCount(IReadOnlyList<Mod> selectedMods)
        {
            if (beatmap == null || beatmap.Value is DummyWorkingBeatmap)
                return 4;

            return ManiaBeatmapConverter.GetColumnCount(LegacyBeatmapConversionDifficultyInfo.FromBeatmapInfo(beatmap.Value.BeatmapInfo), selectedMods);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (skin != null)
                skin.SourceChanged -= showVisibleTime;

            // Held in a list, which the drawable's own unbinding does not reach.
            foreach (var input in visibleTimeInputs)
                input.UnbindAll();

            base.Dispose(isDisposing);
        }
    }
}
