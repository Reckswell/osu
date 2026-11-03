// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;

namespace osu.Game.Rulesets.ManiaOrtho.Highway
{
    public enum HighwayCameraMode
    {
        /// <summary>
        /// True perspective: lanes converge toward a vanishing point above the far edge (Chunithm, SDVX).
        /// </summary>
        Perspective,

        /// <summary>
        /// Foreshortened track with parallel lanes and constant on-screen speed.
        /// </summary>
        [Description("Parallel tilt")]
        ParallelTilt,

        /// <summary>
        /// osu!mania's own layout, unchanged.
        /// </summary>
        Flat,
    }

    public enum HighwayPreset
    {
        Chunithm,
        Voltex,
        Parallel,
        Classic,
        Custom,
    }

    /// <summary>
    /// The user-facing highway settings, as one value. Fractions are of the playfield size.
    /// </summary>
    public readonly record struct HighwaySettings
    {
        public HighwayCameraMode CameraMode { get; init; }

        /// <summary>
        /// Far-edge width over judgement-line width (perspective only).
        /// </summary>
        public float FarWidthRatio { get; init; }

        /// <summary>
        /// The judgement line's height above the bottom of the playfield (IIDX "LIFT" is a higher value).
        /// </summary>
        public float JudgementLineHeight { get; init; }

        /// <summary>
        /// The far edge's distance below the top of the playfield.
        /// </summary>
        public float FarEdgeHeight { get; init; }

        /// <summary>
        /// The track's width at the judgement line.
        /// </summary>
        public float TrackWidth { get; init; }

        /// <summary>
        /// The fraction of the track, from the far edge, hidden behind the wall (Chunithm's field wall, IIDX SUDDEN+).
        /// </summary>
        public float Wall { get; init; }

        /// <summary>
        /// When true (default), moving the wall keeps the visible time: notes slow down on the track. When false, notes keep
        /// their speed and are simply visible for less time.
        /// </summary>
        public bool WallKeepsVisibleTime { get; init; }

        public HighwaySettings()
        {
            CameraMode = HighwayCameraMode.Perspective;
            FarWidthRatio = 0.5f;
            JudgementLineHeight = 0.12f;
            FarEdgeHeight = 0;
            TrackWidth = 0.5f;
            Wall = 0;
            WallKeepsVisibleTime = true;
        }

        /// <summary>
        /// Where the track sits on screen. Not meaningful for <see cref="HighwayCameraMode.Flat"/>, which keeps mania's layout.
        /// </summary>
        public HighwayLayout ToLayout() => new HighwayLayout
        {
            JudgementY = 1 - JudgementLineHeight,
            FarY = FarEdgeHeight,
            NearWidth = TrackWidth,
            FarWidthRatio = CameraMode == HighwayCameraMode.Perspective ? FarWidthRatio : 1,
            CentreX = 0.5f,
        };

        /// <summary>
        /// How time maps onto the track, given the wall.
        /// </summary>
        public HighwayScroll ToScroll() => new HighwayScroll(Wall, WallKeepsVisibleTime);
    }

    /// <summary>
    /// Named starting points for the highway settings. Values marked (tune) are placeholders until the research notes in
    /// docs/ortho/research/ give measured values; every change is recorded in DECISIONS.md.
    /// </summary>
    public static class HighwayPresets
    {
        public const HighwayPreset DEFAULT = HighwayPreset.Chunithm;

        public static HighwaySettings Get(HighwayPreset preset)
        {
            switch (preset)
            {
                case HighwayPreset.Chunithm:
                    // (tune) Gentler perspective than SDVX with a wide far edge (SPEC 2).
                    return new HighwaySettings
                    {
                        CameraMode = HighwayCameraMode.Perspective,
                        FarWidthRatio = 0.45f,
                        JudgementLineHeight = 0.12f,
                        FarEdgeHeight = 0.05f,
                        TrackWidth = 0.6f,
                    };

                case HighwayPreset.Voltex:
                    // (tune) Strong perspective, narrow far end (SPEC 2).
                    return new HighwaySettings
                    {
                        CameraMode = HighwayCameraMode.Perspective,
                        FarWidthRatio = 0.3f,
                        JudgementLineHeight = 0.15f,
                        FarEdgeHeight = 0,
                        TrackWidth = 0.5f,
                    };

                case HighwayPreset.Parallel:
                    return new HighwaySettings
                    {
                        CameraMode = HighwayCameraMode.ParallelTilt,
                        FarWidthRatio = 1,
                        JudgementLineHeight = 0.12f,
                        FarEdgeHeight = 0,
                        TrackWidth = 0.45f,
                    };

                case HighwayPreset.Classic:
                    return new HighwaySettings { CameraMode = HighwayCameraMode.Flat };

                default:
                    throw new ArgumentOutOfRangeException(nameof(preset), preset, "Custom has no fixed values.");
            }
        }
    }
}
