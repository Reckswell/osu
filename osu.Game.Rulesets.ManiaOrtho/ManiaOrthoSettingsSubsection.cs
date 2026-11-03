// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.ManiaOrtho.Configuration;
using osu.Game.Rulesets.ManiaOrtho.Highway;

namespace osu.Game.Rulesets.ManiaOrtho
{
    /// <summary>
    /// This ruleset's section in the settings overlay: mania's scroll speed and note colouring, then the highway.
    /// Mania's scroll direction is left out on purpose: the camera decides the direction (SPEC section 3).
    /// </summary>
    public partial class ManiaOrthoSettingsSubsection : RulesetSettingsSubsection
    {
        public ManiaOrthoSettingsSubsection(ManiaOrthoRuleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (ManiaOrthoRulesetConfigManager)Config;
            var highway = config.Highway;

            Children = new Drawable[]
            {
                new SettingsItemV2(new FormSliderBar<double>
                {
                    Caption = RulesetSettingsStrings.ScrollSpeed,
                    Current = config.GetBindable<double>(ManiaRulesetSetting.ScrollSpeed),
                    KeyboardStep = 1,
                    LabelFormat = v => RulesetSettingsStrings.ScrollSpeedTooltip((int)DrawableManiaRuleset.ComputeScrollTime(v), v),
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
                    HintText = "How much of the far end of the track is covered. Notes appear at the wall.",
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
            };
        }
    }
}
