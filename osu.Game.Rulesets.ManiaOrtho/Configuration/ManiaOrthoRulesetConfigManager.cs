// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Configuration.Tracking;
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

            keepFineScrollSpeed();

            // The camera defines the direction (SPEC 3): notes always come down the track, whatever was stored.
            SetValue(ManiaRulesetSetting.ScrollDirection, ManiaScrollingDirection.Down);
            GetOriginalBindable<ManiaScrollingDirection>(ManiaRulesetSetting.ScrollDirection).Disabled = true;
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            // Finer and up to faster than osu!mania's (0.1, 1-40), so every speed scale's numbers land on their own speeds
            // (SpeedScale). Display only: the time range is all a speed changes.
            var speed = (BindableDouble)GetOriginalBindable<double>(ManiaRulesetSetting.ScrollSpeed);
            speed.MaxValue = SpeedScales.MAX_MANIA_SPEED;
            speed.Precision = SpeedScales.MANIA_SPEED_PRECISION;
        }

        /// <summary>
        /// Brings back the stored speed at maniaortho's precision and range, and keeps a copy of it from now on.
        /// </summary>
        /// <remarks>
        /// osu!mania's defaults read the stored speed into its own limits (0.1, up to 40) before <see cref="InitialiseDefaults"/>
        /// widens them, so the finer, faster speed would be lost on every start. The copy is used only while osu!mania's limits
        /// would read it as the stored speed, so a speed stored without one is kept.
        /// </remarks>
        private void keepFineScrollSpeed()
        {
            var speed = GetOriginalBindable<double>(ManiaRulesetSetting.ScrollSpeed);
            double fine = Highway.Get<double>(HighwaySetting.FineScrollSpeed);

            // Within one of osu!mania's 0.1 steps: its rounding of a half step may go either way.
            if (fine >= 1 && Math.Abs(Math.Min(fine, mania_max_speed) - speed.Value) < 0.1 + 1e-6)
                speed.Value = fine;

            speed.BindValueChanged(e => Highway.SetValue(HighwaySetting.FineScrollSpeed, e.NewValue), true);
        }

        /// <summary>
        /// osu!mania's own fastest speed.
        /// </summary>
        private const double mania_max_speed = 40;

        /// <summary>
        /// None: mania's scroll speed toast shows <c>11485 / speed</c>, not the time notes are visible on the highway. A play's
        /// <see cref="UI.HighwayToasts"/> shows the speed and wall toasts instead, with the real visible time.
        /// </summary>
        public override TrackedSettings CreateTrackedSettings() => new TrackedSettings();

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
            GetOriginalBindable<float>(HighwaySetting.NoteThickness).ValueChanged += _ => onPresetValueChanged();

            void onPresetValueChanged()
            {
                if (!applyingPreset && preset.Value != HighwayPreset.Custom && !matches(HighwayPresets.Get(preset.Value)))
                    preset.Value = HighwayPreset.Custom;
            }

            // Choosing a game's wall scale also gives the wall that game's speed behaviour (Chunithm and osu!mania keep the
            // speed, IIDX the visible time); the toggle can still be changed afterwards.
            GetOriginalBindable<WallScale>(HighwaySetting.WallScale).ValueChanged += e =>
            {
                if (WallScales.KeepsVisibleTime(e.NewValue) is bool keepsVisibleTime)
                    SetValue(HighwaySetting.WallKeepsVisibleTime, keepsVisibleTime);
            };

            ApplySkinPortDefaults();
            applySkinLayoutDefault();
        }

        /// <summary>
        /// Once per install: the note, hit effect and track styles were stored as "Highway" (the default before the skin
        /// porter) the first time this ruleset ran, so the new "Skin (ported)" default would never reach an existing install.
        /// Styles still on "Highway" move to "Skin (ported)" (skins without legacy mania images keep the highway's look
        /// anyway); a style set to anything else is left alone. Runs once, so choosing "Highway" again sticks.
        /// </summary>
        public void ApplySkinPortDefaults()
        {
            var applied = GetOriginalBindable<bool>(HighwaySetting.SkinPortDefaultsApplied);

            if (applied.Value)
                return;

            if (Get<HighwayNoteStyle>(HighwaySetting.NoteStyle) == HighwayNoteStyle.Highway)
                SetValue(HighwaySetting.NoteStyle, HighwayNoteStyle.Ported);
            if (Get<HighwayHitEffectStyle>(HighwaySetting.HitEffects) == HighwayHitEffectStyle.Highway)
                SetValue(HighwaySetting.HitEffects, HighwayHitEffectStyle.Ported);
            if (Get<HighwayTrackStyle>(HighwaySetting.Track) == HighwayTrackStyle.Highway)
                SetValue(HighwaySetting.Track, HighwayTrackStyle.Ported);

            applied.Value = true;
        }

        /// <summary>
        /// Once per install, for the same reason as the styles above: the stage fit's default moved from the camera's width
        /// ("Skin position") to the skin's own ("Skin position and width"), so an install still on the old default moves; a
        /// fit chosen afterwards sticks.
        /// </summary>
        private void applySkinLayoutDefault()
        {
            var applied = GetOriginalBindable<bool>(HighwaySetting.SkinLayoutDefaultApplied);

            if (applied.Value)
                return;

            if (Get<SkinStageFit>(HighwaySetting.StageFit) == SkinStageFit.SkinPosition)
                SetValue(HighwaySetting.StageFit, SkinStageFit.SkinLayout);

            applied.Value = true;
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
            SetDefault(HighwaySetting.JudgementLineHeight, defaults.JudgementLineHeight, 0f, HighwaySettings.MAX_JUDGEMENT_LINE_HEIGHT, precision);
            SetDefault(HighwaySetting.FarEdgeHeight, defaults.FarEdgeHeight, 0f, 0.5f, precision);
            SetDefault(HighwaySetting.TrackWidth, defaults.TrackWidth, 0.2f, 1f, precision);
            // Fine enough for every scale's units (a white number is a thousandth of the lane on screen).
            SetDefault(HighwaySetting.Wall, defaults.Wall, 0f, HighwayScroll.MAX_WALL, 0.0001f);
            SetDefault(HighwaySetting.NoteThickness, defaults.NoteThickness, 0.005f, 0.08f, 0.0005f);
            SetDefault(HighwaySetting.WallKeepsVisibleTime, true);
            SetDefault(HighwaySetting.LaneGuides, true);
            SetDefault(HighwaySetting.SkinHitTarget, false);
            SetDefault(HighwaySetting.NoteStyle, defaults.NoteStyle);
            SetDefault(HighwaySetting.HitEffects, defaults.HitEffects);
            SetDefault(HighwaySetting.Track, defaults.Track);
            SetDefault(HighwaySetting.KeyArea, defaults.KeyArea);
            SetDefault(HighwaySetting.StageFit, defaults.StageFit);
            SetDefault(HighwaySetting.SideStagePull, defaults.SideStagePull, 0f, 1f, 0.01f);
            SetDefault(HighwaySetting.SideStageLean, defaults.SideStageLean, 0f, 1f, 0.01f);
            SetDefault(HighwaySetting.SkinLayoutDefaultApplied, false);
            SetDefault(HighwaySetting.SkinNoteProportions, defaults.SkinNoteProportions);
            SetDefault(HighwaySetting.FarFade, defaults.FarFade, 0f, HighwaySettings.MAX_FAR_FADE, 0.01f);
            SetDefault(HighwaySetting.HoldOpacity, defaults.HoldOpacity, HighwaySettings.MIN_HOLD_OPACITY, 1f, 0.01f);
            SetDefault(HighwaySetting.TrackOpacity, defaults.TrackOpacity, 0f, 1f, 0.01f);
            SetDefault(HighwaySetting.KeyBeam, defaults.KeyBeam);
            SetDefault(HighwaySetting.KiaiSway, defaults.KiaiSway, 0f, 1f, 0.01f);
            SetDefault(HighwaySetting.WallScale, defaults.WallScale);
            SetDefault(HighwaySetting.ShareManiaKeys, true);
            SetDefault(HighwaySetting.SpeedScale, defaults.SpeedScale);
            SetDefault(HighwaySetting.WidthMode, defaults.WidthMode);
            SetDefault(HighwaySetting.LaneWidth, defaults.LaneWidth, HighwaySettings.MIN_LANE_WIDTH, HighwaySettings.MAX_LANE_WIDTH, 0.001f);
            SetDefault(HighwaySetting.SkinPortDefaultsApplied, false);
            SetDefault(HighwaySetting.FineScrollSpeed, 0.0, 0, SpeedScales.MAX_MANIA_SPEED, SpeedScales.MANIA_SPEED_PRECISION);
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
            NoteThickness = Get<float>(HighwaySetting.NoteThickness),
            WallKeepsVisibleTime = Get<bool>(HighwaySetting.WallKeepsVisibleTime),
            LaneGuides = Get<bool>(HighwaySetting.LaneGuides),
            SkinHitTarget = Get<bool>(HighwaySetting.SkinHitTarget),
            NoteStyle = Get<HighwayNoteStyle>(HighwaySetting.NoteStyle),
            HitEffects = Get<HighwayHitEffectStyle>(HighwaySetting.HitEffects),
            Track = Get<HighwayTrackStyle>(HighwaySetting.Track),
            KeyArea = Get<SkinKeyArea>(HighwaySetting.KeyArea),
            StageFit = Get<SkinStageFit>(HighwaySetting.StageFit),
            SideStagePull = Get<float>(HighwaySetting.SideStagePull),
            SideStageLean = Get<float>(HighwaySetting.SideStageLean),
            SkinNoteProportions = Get<bool>(HighwaySetting.SkinNoteProportions),
            FarFade = Get<float>(HighwaySetting.FarFade),
            HoldOpacity = Get<float>(HighwaySetting.HoldOpacity),
            TrackOpacity = Get<float>(HighwaySetting.TrackOpacity),
            KeyBeam = Get<KeyBeamLength>(HighwaySetting.KeyBeam),
            KiaiSway = Get<float>(HighwaySetting.KiaiSway),
            WallScale = Get<WallScale>(HighwaySetting.WallScale),
            SpeedScale = Get<SpeedScale>(HighwaySetting.SpeedScale),
            WidthMode = Get<HighwayWidthMode>(HighwaySetting.WidthMode),
            LaneWidth = Get<float>(HighwaySetting.LaneWidth),
        };

        /// <summary>
        /// Writes a preset's values into the individual settings. Leaves <see cref="HighwaySetting.WallKeepsVisibleTime"/>,
        /// <see cref="HighwaySetting.LaneGuides"/>, <see cref="HighwaySetting.SkinHitTarget"/>,
        /// <see cref="HighwaySetting.NoteStyle"/>, <see cref="HighwaySetting.HitEffects"/>, <see cref="HighwaySetting.Track"/>,
        /// <see cref="HighwaySetting.FarFade"/>, <see cref="HighwaySetting.HoldOpacity"/>, <see cref="HighwaySetting.TrackOpacity"/>,
        /// <see cref="HighwaySetting.KeyBeam"/>, <see cref="HighwaySetting.KiaiSway"/>, <see cref="HighwaySetting.WallScale"/> and the
        /// skin porter's settings alone (preferences, not part of a camera).
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
                SetValue(HighwaySetting.NoteThickness, values.NoteThickness);
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
                   && near(current.Wall, preset.Wall)
                   && near(current.NoteThickness, preset.NoteThickness);

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
        NoteThickness,
        HitEffects,
        Track,
        KeyArea,
        StageFit,
        SkinNoteProportions,
        FarFade,
        HoldOpacity,
        TrackOpacity,
        KeyBeam,
        KiaiSway,
        WallScale,
        SideStagePull,
        SideStageLean,

        /// <summary>
        /// The units the scroll speed is set and shown in (<see cref="Highway.SpeedScale"/>); the speed itself stays osu!mania's
        /// scroll speed setting.
        /// </summary>
        SpeedScale,

        /// <summary>
        /// Whether the track width or the lane width sets the highway track's width (<see cref="HighwaySettings.WidthMode"/>).
        /// </summary>
        WidthMode,

        /// <summary>
        /// One lane's width at the judgement line in lane mode (<see cref="HighwaySettings.LaneWidth"/>).
        /// </summary>
        LaneWidth,

        /// <summary>
        /// Whether maniaortho plays with the keys bound for osu!mania, the two key binding sections kept equal
        /// (<see cref="KeyBindingMirror"/>), or with its own.
        /// </summary>
        ShareManiaKeys,

        /// <summary>
        /// Not shown: whether <see cref="HighwayConfigManager.ApplySkinPortDefaults"/> has run on this install.
        /// </summary>
        SkinPortDefaultsApplied,

        /// <summary>
        /// Not shown: osu!mania's scroll speed at maniaortho's precision and range (0 before it is first stored), which
        /// osu!mania's limits would round and cap as they read it (<see cref="ManiaOrthoRulesetConfigManager"/>).
        /// </summary>
        FineScrollSpeed,

        /// <summary>
        /// Not shown: whether the one-time move of a stored "Skin position" stage fit to the skin's own width
        /// (<c>applySkinLayoutDefault</c>, Alpha 2026-11-04) has run on this install.
        /// </summary>
        SkinLayoutDefaultApplied,
    }
}
