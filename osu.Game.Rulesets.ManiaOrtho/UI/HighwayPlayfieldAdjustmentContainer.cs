// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.ManiaOrtho.Configuration;
using osu.Game.Rulesets.ManiaOrtho.Highway;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.ManiaOrtho.UI
{
    /// <summary>
    /// Holds mania's own playfield layout inside a <see cref="HighwayWarpContainer"/>, keeps the warp in step with the
    /// highway settings and the stage's position, and offers a screen-space <see cref="Overlay"/> for things that must not
    /// tilt (SPEC section 7).
    /// </summary>
    public partial class HighwayPlayfieldAdjustmentContainer : PlayfieldAdjustmentContainer
    {
        /// <summary>
        /// Limits on how far the stage is stretched sideways inside the buffer.
        /// </summary>
        private const float min_stage_scale = 0.25f;

        private const float max_stage_scale = 4;

        protected override Container<Drawable> Content => mania;

        /// <summary>
        /// Drawn on top of the highway, untilted, in the same space as the highway.
        /// </summary>
        public Container Overlay { get; }

        public HighwayWarpContainer Warp { get; }

        /// <summary>
        /// The current highway settings, refreshed every frame.
        /// </summary>
        public HighwaySettings Settings { get; private set; } = new HighwaySettings();

        /// <summary>
        /// The current projection in this container's local space.
        /// </summary>
        public HighwayProjection Projection { get; private set; }

        private readonly DrawableManiaOrthoRuleset drawableRuleset;
        private readonly ManiaPlayfieldAdjustmentContainer mania;

        private HighwayConfigManager? config;

        public HighwayPlayfieldAdjustmentContainer(DrawableManiaOrthoRuleset drawableRuleset)
        {
            this.drawableRuleset = drawableRuleset;

            InternalChildren = new Drawable[]
            {
                Warp = new HighwayWarpContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = mania = new ManiaPlayfieldAdjustmentContainer(drawableRuleset)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                },
                Overlay = new Container { RelativeSizeAxes = Axes.Both },
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            // The drawable ruleset's dependencies (and so its config) are set up before its children load.
            config = drawableRuleset.HighwayConfig;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (config != null)
                Settings = config.Current;

            bool flat = Settings.CameraMode == HighwayCameraMode.Flat;

            Projection = new HighwayProjection(Settings.ToLayout(), DrawSize);

            Warp.Warp = !flat;
            Warp.Layout = Projection.Layout;
            Warp.Scroll = Settings.ToScroll();

            if (!drawableRuleset.IsLoaded || drawableRuleset.Playfield.Stages.Count == 0)
                return;

            RectangleF stage = stageBounds();
            Warp.Source = stage;

            // Render the stage in the buffer at about the width it will have at the judgement line, so the near end is
            // not magnified (blurry). The far end is then minified, which filters well.
            float currentScale = mania.Scale.X;
            float targetScale = 1;

            if (!flat && stage.Width > 0)
            {
                float unscaledWidth = stage.Width / currentScale;
                targetScale = Math.Clamp(Projection.WidthAt(0) / unscaledWidth, min_stage_scale, max_stage_scale);
            }

            if (Math.Abs(targetScale - currentScale) > 1e-3f)
                mania.Scale = new Vector2(targetScale, 1);
        }

        /// <summary>
        /// The columns of every stage, in this container's local space: left and right edges, the top of the scrolling area
        /// and the judgement line.
        /// </summary>
        private RectangleF stageBounds()
        {
            float left = float.MaxValue, right = float.MinValue, top = float.MaxValue, bottom = float.MinValue;

            foreach (var stage in drawableRuleset.Playfield.Stages)
            {
                foreach (var column in stage.Columns)
                {
                    var quad = column.HitObjectContainer.ScreenSpaceDrawQuad;

                    Vector2 topLeft = Warp.ToLocalSpace(quad.TopLeft);
                    Vector2 bottomRight = Warp.ToLocalSpace(quad.BottomRight);

                    left = Math.Min(left, topLeft.X);
                    right = Math.Max(right, bottomRight.X);
                    top = Math.Min(top, topLeft.Y);
                    bottom = Math.Max(bottom, bottomRight.Y);
                }
            }

            return left > right ? RectangleF.Empty : new RectangleF(left, top, right - left, bottom - top);
        }
    }
}
