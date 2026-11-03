// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Configuration;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.ManiaOrtho.Highway;

namespace osu.Game.Rulesets.ManiaOrtho.Configuration
{
    /// <summary>
    /// This ruleset's settings: osu!mania's own (scroll speed, note colouring), which <c>DrawableManiaRuleset</c> reads
    /// from this type, plus the highway settings in <see cref="Highway"/>.
    /// </summary>
    /// <remarks>
    /// Highway settings need their own enum, and a <see cref="RulesetConfigManager{TLookup}"/> has exactly one, so they live
    /// in a second manager stored under the same ruleset. Keys are enum member names and the two enums share none.
    /// None of these settings affect judgement or scoring (CLAUDE.md invariant 3).
    /// </remarks>
    public class ManiaOrthoRulesetConfigManager : ManiaRulesetConfigManager
    {
        public HighwayConfigManager Highway { get; }

        public ManiaOrthoRulesetConfigManager(SettingsStore? settings, RulesetInfo ruleset, int? variant = null)
            : base(settings, ruleset, variant)
        {
            Highway = new HighwayConfigManager(settings, ruleset, variant);

            // The camera defines the direction (SPEC 3): notes always come down the track, whatever was stored.
            SetValue(ManiaRulesetSetting.ScrollDirection, ManiaScrollingDirection.Down);
            GetOriginalBindable<ManiaScrollingDirection>(ManiaRulesetSetting.ScrollDirection).Disabled = true;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (isDisposing)
                Highway.Dispose();
        }
    }

    public class HighwayConfigManager : RulesetConfigManager<HighwaySetting>
    {
        private bool applyingPreset;

        public HighwayConfigManager(SettingsStore? settings, RulesetInfo ruleset, int? variant = null)
            : base(settings, ruleset, variant)
        {
            var preset = GetOriginalBindable<HighwayPreset>(HighwaySetting.Preset);

            // A named preset always shows its current values, so tuning a preset in code reaches everyone using it.
            ApplyPreset(preset.Value);

            preset.ValueChanged += e => ApplyPreset(e.NewValue);

            // Changing any value by hand turns the preset into "Custom".
            GetOriginalBindable<HighwayCameraMode>(HighwaySetting.CameraMode).ValueChanged += _ => onPresetValueChanged();
            GetOriginalBindable<float>(HighwaySetting.FarWidthRatio).ValueChanged += _ => onPresetValueChanged();
            GetOriginalBindable<float>(HighwaySetting.JudgementLineHeight).ValueChanged += _ => onPresetValueChanged();
            GetOriginalBindable<float>(HighwaySetting.FarEdgeHeight).ValueChanged += _ => onPresetValueChanged();
            GetOriginalBindable<float>(HighwaySetting.TrackWidth).ValueChanged += _ => onPresetValueChanged();
            GetOriginalBindable<float>(HighwaySetting.Wall).ValueChanged += _ => onPresetValueChanged();

            void onPresetValueChanged()
            {
                if (!applyingPreset && preset.Value != HighwayPreset.Custom && !matches(HighwayPresets.Get(preset.Value)))
                    preset.Value = HighwayPreset.Custom;
            }
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            var defaults = HighwayPresets.Get(HighwayPresets.DEFAULT);

            // Fine enough for measured preset values such as a 7.4% judgement line.
            const float precision = 0.001f;

            SetDefault(HighwaySetting.Preset, HighwayPresets.DEFAULT);
            SetDefault(HighwaySetting.CameraMode, defaults.CameraMode);
            SetDefault(HighwaySetting.FarWidthRatio, defaults.FarWidthRatio, HighwayLayout.MIN_FAR_WIDTH_RATIO, 1f, precision);
            SetDefault(HighwaySetting.JudgementLineHeight, defaults.JudgementLineHeight, 0f, 0.4f, precision);
            SetDefault(HighwaySetting.FarEdgeHeight, defaults.FarEdgeHeight, 0f, 0.5f, precision);
            SetDefault(HighwaySetting.TrackWidth, defaults.TrackWidth, 0.2f, 1f, precision);
            SetDefault(HighwaySetting.Wall, defaults.Wall, 0f, HighwayScroll.MAX_WALL, 0.01f);
            SetDefault(HighwaySetting.WallKeepsVisibleTime, true);
            SetDefault(HighwaySetting.LaneGuides, true);
            SetDefault(HighwaySetting.SkinHitTarget, false);
            SetDefault(HighwaySetting.NoteStyle, HighwayNoteStyle.Highway);
        }

        /// <summary>
        /// The current values as one snapshot.
        /// </summary>
        public HighwaySettings Current => new HighwaySettings
        {
            CameraMode = Get<HighwayCameraMode>(HighwaySetting.CameraMode),
            FarWidthRatio = Get<float>(HighwaySetting.FarWidthRatio),
            JudgementLineHeight = Get<float>(HighwaySetting.JudgementLineHeight),
            FarEdgeHeight = Get<float>(HighwaySetting.FarEdgeHeight),
            TrackWidth = Get<float>(HighwaySetting.TrackWidth),
            Wall = Get<float>(HighwaySetting.Wall),
            WallKeepsVisibleTime = Get<bool>(HighwaySetting.WallKeepsVisibleTime),
            LaneGuides = Get<bool>(HighwaySetting.LaneGuides),
            SkinHitTarget = Get<bool>(HighwaySetting.SkinHitTarget),
            NoteStyle = Get<HighwayNoteStyle>(HighwaySetting.NoteStyle),
        };

        /// <summary>
        /// Writes a preset's values into the individual settings. Leaves <see cref="HighwaySetting.WallKeepsVisibleTime"/>,
        /// <see cref="HighwaySetting.LaneGuides"/>, <see cref="HighwaySetting.SkinHitTarget"/> and
        /// <see cref="HighwaySetting.NoteStyle"/> alone (preferences, not part of a camera).
        /// </summary>
        public void ApplyPreset(HighwayPreset preset)
        {
            if (preset == HighwayPreset.Custom)
                return;

            var values = HighwayPresets.Get(preset);

            applyingPreset = true;

            try
            {
                SetValue(HighwaySetting.CameraMode, values.CameraMode);
                SetValue(HighwaySetting.FarWidthRatio, values.FarWidthRatio);
                SetValue(HighwaySetting.JudgementLineHeight, values.JudgementLineHeight);
                SetValue(HighwaySetting.FarEdgeHeight, values.FarEdgeHeight);
                SetValue(HighwaySetting.TrackWidth, values.TrackWidth);
                SetValue(HighwaySetting.Wall, values.Wall);
            }
            finally
            {
                applyingPreset = false;
            }
        }

        private bool matches(HighwaySettings preset)
        {
            var current = Current;

            // Flat ignores the geometry, so any geometry matches the Classic preset.
            if (preset.CameraMode == HighwayCameraMode.Flat)
                return current.CameraMode == HighwayCameraMode.Flat;

            return current.CameraMode == preset.CameraMode
                   && near(current.FarWidthRatio, preset.FarWidthRatio)
                   && near(current.JudgementLineHeight, preset.JudgementLineHeight)
                   && near(current.FarEdgeHeight, preset.FarEdgeHeight)
                   && near(current.TrackWidth, preset.TrackWidth)
                   && near(current.Wall, preset.Wall);

            static bool near(float a, float b) => Math.Abs(a - b) < 1e-4f;
        }
    }

    public enum HighwaySetting
    {
        Preset,
        CameraMode,
        FarWidthRatio,
        JudgementLineHeight,
        FarEdgeHeight,
        TrackWidth,
        Wall,
        WallKeepsVisibleTime,
        LaneGuides,
        SkinHitTarget,
        NoteStyle,
    }
}
