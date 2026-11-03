// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.ManiaOrtho.Highway
{
    /// <summary>
    /// Where the highway sits on screen, in fractions of the playfield's draw size (y measured down from the top).
    /// This is everything <see cref="HighwayProjection"/> needs; it holds no game state.
    /// </summary>
    public readonly record struct HighwayLayout
    {
        /// <summary>
        /// The judgement line's height, as a fraction of the playfield height from the top (0.88 = 12% above the bottom).
        /// </summary>
        public float JudgementY { get; init; }

        /// <summary>
        /// The far edge of the track (where notes first appear when no wall is set), as a fraction of the playfield height
        /// from the top. Must be above <see cref="JudgementY"/>.
        /// </summary>
        public float FarY { get; init; }

        /// <summary>
        /// The track's width at the judgement line, as a fraction of the playfield width.
        /// </summary>
        public float NearWidth { get; init; }

        /// <summary>
        /// The track's width at the far edge divided by its width at the judgement line, in (0, 1].
        /// 1 means lanes stay parallel (no perspective); smaller values mean a stronger perspective.
        /// </summary>
        public float FarWidthRatio { get; init; }

        /// <summary>
        /// The horizontal centre of the track, as a fraction of the playfield width.
        /// </summary>
        public float CentreX { get; init; }

        public HighwayLayout()
        {
            JudgementY = 0.88f;
            FarY = 0;
            NearWidth = 0.5f;
            FarWidthRatio = 0.5f;
            CentreX = 0.5f;
        }

        /// <summary>
        /// The smallest supported <see cref="FarWidthRatio"/>. Below this the far edge is close to a point and far notes
        /// become unreadable.
        /// </summary>
        public const float MIN_FAR_WIDTH_RATIO = 0.05f;

        /// <summary>
        /// Returns a copy with every value clamped to a range the projection can handle.
        /// </summary>
        public HighwayLayout Sanitised()
        {
            float judgementY = Math.Clamp(JudgementY, 0.05f, 1f);

            return this with
            {
                JudgementY = judgementY,
                FarY = Math.Clamp(FarY, 0f, judgementY - 0.05f),
                NearWidth = Math.Clamp(NearWidth, 0.05f, 1f),
                FarWidthRatio = Math.Clamp(FarWidthRatio, MIN_FAR_WIDTH_RATIO, 1f),
                CentreX = Math.Clamp(CentreX, 0f, 1f),
            };
        }
    }
}
