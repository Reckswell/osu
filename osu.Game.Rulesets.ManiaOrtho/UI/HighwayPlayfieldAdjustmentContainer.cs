// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Testing;
using osu.Game.Rulesets.Mania.Skinning.Legacy;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.ManiaOrtho.Configuration;
using osu.Game.Rulesets.ManiaOrtho.Highway;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
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
        /// The field wall, in <see cref="Overlay"/> under the judgements.
        /// </summary>
        public HighwayWall Wall { get; }

        /// <summary>
        /// The lane guides and judgement line, in <see cref="Overlay"/> under the wall.
        /// </summary>
        public HighwayGuides Guides { get; }

        /// <summary>
        /// The skin's hit targets: one wrapper per column, plus legacy skins' own (see <see cref="updateHitTargets"/>).
        /// </summary>
        public IReadOnlyList<Drawable> HitTargets => hitTargets;

        private readonly List<Drawable> hitTargets = new List<Drawable>();

        /// <summary>
        /// Frames left in which to look for hit targets again. A skin change rebuilds them.
        /// </summary>
        private int hitTargetSearches = 1;

        private ISkinSource? skinSource;

        /// <summary>
        /// One upright judgement display per stage, in <see cref="Overlay"/>.
        /// </summary>
        public IReadOnlyList<HighwayJudgementLayer> JudgementLayers => judgementLayers;

        private readonly List<HighwayJudgementLayer> judgementLayers = new List<HighwayJudgementLayer>();

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
                Overlay = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        Guides = new HighwayGuides(),
                        Wall = new HighwayWall(),
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skinSource)
        {
            // The drawable ruleset's dependencies (and so its config) are set up before its children load.
            config = drawableRuleset.HighwayConfig;

            this.skinSource = skinSource;
            skinSource.SourceChanged += onSkinSourceChanged;

            // The playfield (and so its stages) is created with this container, before it loads.
            foreach (var stage in drawableRuleset.Playfield.Stages)
            {
                var layer = new HighwayJudgementLayer(stage);
                judgementLayers.Add(layer);
                Overlay.Add(layer);
            }
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
            Wall.Place(Projection, Warp.Scroll, flat);

            Guides.Layout = Projection.Layout;
            Guides.Alpha = flat ? 0 : 1;
            Guides.ShowLaneLines = Settings.LaneGuides;
            Guides.ShowJudgementLine = !Settings.SkinHitTarget;

            if (!drawableRuleset.IsLoaded || drawableRuleset.Playfield.Stages.Count == 0)
                return;

            updateHitTargets(flat || Settings.SkinHitTarget);

            RectangleF stage = stageBounds();

            // Render the stage in the buffer at about the width it will have at the judgement line, so the near end is
            // not magnified (blurry). The far end is minified instead (see DECISIONS on far-end shimmer).
            float currentScale = mania.Scale.X;
            float targetScale = 1;

            if (!flat && stage.Width > 0)
            {
                float unscaledWidth = stage.Width / currentScale;
                targetScale = Math.Clamp(Projection.WidthAt(0) / unscaledWidth, min_stage_scale, max_stage_scale);
            }

            if (Math.Abs(targetScale - currentScale) > 1e-3f)
            {
                // Measure again at the new stretch, so this frame is not drawn from a stale stage rectangle.
                mania.Scale = new Vector2(targetScale, 1);
                stage = stageBounds();
            }

            Warp.Source = stage;

            updateGuideLines(stage);

            for (int i = 0; i < judgementLayers.Count; i++)
                judgementLayers[i].Place(Projection, flat, stage, mania.Scale.X);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (skinSource != null)
                skinSource.SourceChanged -= onSkinSourceChanged;

            base.Dispose(isDisposing);
        }

        // Skinnable drawables rebuild in a scheduled call on the same event; look again after they have (this frame and the
        // next, in case the rebuild lands a frame later).
        private void onSkinSourceChanged() => Schedule(() => hitTargetSearches = 2);

        /// <summary>
        /// Shows or hides the skin's hit targets. Under a tilted camera the warp magnifies them several times near the line
        /// (they sit just above it in the flat stage), so the highway draws its own judgement line instead by default.
        /// </summary>
        /// <remarks>
        /// Each column has a skinnable hit target, the only skinnable drawable directly inside its hit object area. Legacy
        /// skins with key textures leave that empty and draw a <see cref="LegacyHitTarget"/> (stage hint and judgement
        /// line) inside their stage background instead, which is rebuilt when the skin changes.
        /// </remarks>
        private void updateHitTargets(bool show)
        {
            if (hitTargetSearches > 0)
            {
                hitTargetSearches--;
                hitTargets.Clear();

                var stages = drawableRuleset.Playfield.Stages;

                for (int i = 0; i < stages.Count; i++)
                {
                    foreach (var column in stages[i].Columns)
                    {
                        foreach (var skinnable in column.HitObjectArea.ChildrenOfType<SkinnableDrawable>())
                        {
                            if (skinnable.Parent == column.HitObjectArea)
                                hitTargets.Add(skinnable);
                        }
                    }

                    hitTargets.AddRange(stages[i].ChildrenOfType<LegacyHitTarget>());
                }
            }

            float alpha = show ? 1 : 0;

            for (int i = 0; i < hitTargets.Count; i++)
                hitTargets[i].Alpha = alpha;
        }

        /// <summary>
        /// Puts a guide line on each stage edge and between each pair of neighbouring columns, where the warp draws them.
        /// </summary>
        private void updateGuideLines(RectangleF source)
        {
            var stages = drawableRuleset.Playfield.Stages;

            int count = 0;

            for (int i = 0; i < stages.Count; i++)
                count += stages[i].Columns.Length + 1;

            Guides.SetLineCount(count);

            if (source.Width <= 0)
                return;

            int index = 0;

            for (int i = 0; i < stages.Count; i++)
            {
                var columns = stages[i].Columns;
                float previousRight = 0;

                for (int j = 0; j < columns.Length; j++)
                {
                    var quad = columns[j].HitObjectContainer.ScreenSpaceDrawQuad;
                    float left = toLane(quad.TopLeft);
                    float right = toLane(quad.BottomRight);

                    // The left edge of the stage, then the middle of each gap between columns.
                    Guides.SetLine(index++, j == 0 ? left : (previousRight + left) / 2, j == 0);
                    previousRight = right;
                }

                Guides.SetLine(index++, previousRight, true);
            }

            float toLane(Vector2 screenSpace) => (Warp.ToLocalSpace(screenSpace).X - source.Left) / source.Width;
        }

        /// <summary>
        /// The columns of every stage, in this container's local space: left and right edges, the top of the scrolling area
        /// and the judgement line.
        /// </summary>
        private RectangleF stageBounds()
        {
            float left = float.MaxValue, right = float.MinValue, top = float.MaxValue, bottom = float.MinValue;

            var stages = drawableRuleset.Playfield.Stages;

            // Indexed loops: enumerating an IReadOnlyList boxes its enumerator, and this runs every frame.
            for (int i = 0; i < stages.Count; i++)
            {
                var columns = stages[i].Columns;

                for (int j = 0; j < columns.Length; j++)
                {
                    var quad = columns[j].HitObjectContainer.ScreenSpaceDrawQuad;

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
