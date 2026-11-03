// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.Skinning.Legacy;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.ManiaOrtho.Configuration;
using osu.Game.Rulesets.ManiaOrtho.Highway;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
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
        /// Notes drawn natively on the track, in <see cref="Overlay"/> over the guides and under the wall. Only used with
        /// <see cref="HighwayNoteStyle.Highway"/>.
        /// </summary>
        public HighwayNoteLayer NoteLayer { get; }

        /// <summary>
        /// Whether this play draws notes natively (<see cref="HighwayNoteStyle.Highway"/>), fixed when gameplay loads.
        /// </summary>
        public bool NativeNotes { get; private set; }

        /// <summary>
        /// When notes are drawn natively: one holder per column with a proxy of its hit object container, so mania's own
        /// notes are not drawn in the stage. Each holder sits right behind the container it proxies, in the same parent, so
        /// under the flat camera (the only time holders are shown) the notes are drawn exactly where mania draws them: under
        /// a cover mod's cover, inside the column's masking, under the key area and explosions.
        /// </summary>
        public IReadOnlyList<Drawable> NoteProxyHolders => noteProxyHolders;

        private readonly List<Drawable> noteProxyHolders = new List<Drawable>();

        /// <summary>
        /// Holds proxies whose container is not in a <see cref="Container{T}"/> (none known; a later mod might do it).
        /// </summary>
        private readonly Container fallbackNoteProxies;

        private bool noteProxiesCreated;

        /// <summary>
        /// Each column's cover from a cover mod (Hidden, Fade In, Cover), in playfield column order, or empty without one.
        /// </summary>
        private CoverBoxes?[] coverBoxes = Array.Empty<CoverBoxes?>();

        /// <summary>
        /// Each column's cover in track depth this frame, in playfield column order. Empty without a cover mod.
        /// </summary>
        public ReadOnlySpan<DepthCover> ColumnCovers => columnCovers;

        private DepthCover[] columnCovers = Array.Empty<DepthCover>();

        /// <summary>
        /// Each column's left and right edge in lane space, in playfield column order.
        /// </summary>
        private Vector2[] columnLanes = Array.Empty<Vector2>();

        [Resolved]
        private IScrollingInfo scrollingInfo { get; set; } = null!;

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

        /// <summary>
        /// How close (in track depth) a cover's edge must be to an end of the column to count as reaching it.
        /// </summary>
        private const float cover_edge_tolerance = 1e-3f;

        /// <summary>
        /// Rows far above the stage map towards the vanishing point; past this depth they are all drawn there.
        /// </summary>
        private const float max_mapped_depth = 1000;

        private readonly Bindable<bool> timingBasedColouring = new Bindable<bool>();

        public HighwayPlayfieldAdjustmentContainer(DrawableManiaOrthoRuleset drawableRuleset)
        {
            this.drawableRuleset = drawableRuleset;

            InternalChildren = new Drawable[]
            {
                Warp = new HighwayWarpContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        mania = new ManiaPlayfieldAdjustmentContainer(drawableRuleset)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        fallbackNoteProxies = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                    },
                },
                Overlay = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        Guides = new HighwayGuides(),
                        NoteLayer = new HighwayNoteLayer { Alpha = 0 },
                        Wall = new HighwayWall(),
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skinSource, ManiaRulesetConfigManager? maniaConfig)
        {
            // The drawable ruleset's dependencies (and so its config) are set up before its children load.
            config = drawableRuleset.HighwayConfig;

            this.skinSource = skinSource;
            skinSource.SourceChanged += onSkinSourceChanged;

            NativeNotes = config?.Current.NoteStyle == HighwayNoteStyle.Highway;

            maniaConfig?.BindWith(ManiaRulesetSetting.TimingBasedNoteColouring, timingBasedColouring);

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

            if (NativeNotes && !noteProxiesCreated)
            {
                noteProxiesCreated = true;
                createNoteProxies();
            }

            for (int i = 0; i < noteProxyHolders.Count; i++)
                noteProxyHolders[i].Alpha = flat ? 1 : 0;

            NoteLayer.Alpha = NativeNotes && !flat ? 1 : 0;

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

            if (NativeNotes && !flat)
            {
                NoteLayer.Layout = Projection.Layout;
                NoteLayer.SnapColours = timingBasedColouring.Value;
                measureCovers(stage);
                NoteLayer.Gather(drawableRuleset.Playfield, columnLanes, columnCovers, scrollingInfo.Algorithm.Value, scrollingInfo.TimeRange.Value, Warp.Scroll,
                    Time.Current);
            }

            for (int i = 0; i < judgementLayers.Count; i++)
                judgementLayers[i].Place(Projection, flat, stage, mania.Scale.X);
        }

        /// <summary>
        /// Mania's notes keep judging in the stage; proxying their containers away only stops them being drawn there. Runs
        /// after mods have been applied, so a cover mod's wrapper is already in place.
        /// </summary>
        private void createNoteProxies()
        {
            var stages = drawableRuleset.Playfield.Stages;
            var covers = new List<CoverBoxes?>();

            for (int i = 0; i < stages.Count; i++)
            {
                foreach (var column in stages[i].Columns)
                {
                    var hitObjects = column.HitObjectContainer;
                    covers.Add(findCover(hitObjects));

                    var holder = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        Child = hitObjects.CreateProxy(),
                    };

                    if (hitObjects.Parent is Container<Drawable> parent)
                    {
                        // Drawn just before the container itself, which draws nothing now: before anything added after it,
                        // such as a cover.
                        holder.Depth = MathF.BitIncrement(hitObjects.Depth);
                        parent.Add(holder);
                    }
                    else
                        fallbackNoteProxies.Add(holder);

                    noteProxyHolders.Add(holder);
                }
            }

            if (covers.Any(c => c != null))
            {
                coverBoxes = covers.ToArray();
                columnCovers = new DepthCover[coverBoxes.Length];
            }
        }

        /// <summary>
        /// The two boxes of a <see cref="PlayfieldCoveringWrapper"/>'s cover: the gradient (opaque at its own bottom edge) and
        /// the fully covered part.
        /// </summary>
        private readonly record struct CoverBoxes(Drawable Gradient, Drawable Filled);

        /// <summary>
        /// Mania's cover mods wrap each column's notes in a <see cref="PlayfieldCoveringWrapper"/>: a buffer holding the notes
        /// and the cover, a container of a gradient box and a filled box that the wrapper sizes every frame. Reading the drawn
        /// boxes keeps native notes covered exactly as mania covers its own (coverage changes with combo, eases in, legacy
        /// hit positions, direction), without copying any of that.
        /// </summary>
        private static CoverBoxes? findCover(Drawable hitObjects)
        {
            if (hitObjects.Parent is not Container<Drawable> buffer || buffer.Parent is not PlayfieldCoveringWrapper)
                return null;

            foreach (var child in buffer.Children)
            {
                if (child is not Container<Drawable> cover || cover.Children.Count != 2 || !cover.Children.All(c => c is Box))
                    continue;

                var gradient = cover.Children.FirstOrDefault(c => !c.Colour.TryExtractSingleColour(out _));
                var filled = cover.Children.FirstOrDefault(c => c.Colour.TryExtractSingleColour(out _));

                if (gradient != null && filled != null)
                    return new CoverBoxes(gradient, filled);
            }

            return null;
        }

        /// <summary>
        /// The track depth the warp gives the flat stage's row at <paramref name="flatY"/> (this container's space; mania's
        /// stage is laid out flat in it before the warp): 0 at the hit position, the scroll length at the top of the
        /// scrolling area, linearly in between, so notes keep mania's timing. Negative below the line.
        /// </summary>
        public float DepthAtFlatY(float flatY)
        {
            var source = Warp.Source;
            return source.Height > 0 ? (source.Bottom - flatY) / source.Height * Warp.Scroll.ScrollLength : 0;
        }

        /// <summary>
        /// Where the flat stage's row at <paramref name="flatY"/> is drawn, in this container's space: on the track at its
        /// depth above the judgement line, at its flat height below it (as the warp draws the receptor area), unchanged under
        /// the flat camera. Never decreases as <paramref name="flatY"/> grows.
        /// </summary>
        public float DrawnYOfFlatY(float flatY)
        {
            var source = Warp.Source;

            if (!Warp.Warp || source.Height <= 0)
                return flatY;

            if (flatY > source.Bottom)
                return Projection.YAt(0) + (flatY - source.Bottom);

            return Projection.YAt(Math.Min(DepthAtFlatY(flatY), max_mapped_depth));
        }

        /// <summary>
        /// Converts each column's cover, as mania draws it in the flat stage, to track depth through the warp's mapping (the
        /// stage's rows map linearly to depth 0 at the hit position up to the scroll length at the top).
        /// </summary>
        private void measureCovers(RectangleF source)
        {
            if (source.Height <= 0)
                return;

            float scrollLength = Warp.Scroll.ScrollLength;

            for (int i = 0; i < coverBoxes.Length; i++)
            {
                if (coverBoxes[i] is not CoverBoxes boxes)
                {
                    columnCovers[i] = DepthCover.None;
                    continue;
                }

                float coveredFrom = 0, coveredTo = 0;

                if (boxes.Filled.IsPresent && boxes.Filled.DrawHeight > 0)
                {
                    var quad = boxes.Filled.ScreenSpaceDrawQuad;
                    float a = DepthAtFlatY(Warp.ToLocalSpace(quad.TopLeft).Y);
                    float b = DepthAtFlatY(Warp.ToLocalSpace(quad.BottomLeft).Y);

                    coveredFrom = Math.Min(a, b);
                    coveredTo = Math.Max(a, b);

                    // A cover reaching an end of the column also covers what is past that end: mania's column masks it.
                    if (coveredFrom <= cover_edge_tolerance)
                        coveredFrom = float.MinValue;
                    if (coveredTo >= scrollLength - cover_edge_tolerance)
                        coveredTo = float.MaxValue;
                }

                float fadeFrom = 0, fadeTo = 0, fadeAlpha = 0;

                if (boxes.Gradient.IsPresent && boxes.Gradient.DrawHeight > 0)
                {
                    var gradient = boxes.Gradient;

                    fadeFrom = DepthAtFlatY(Warp.ToLocalSpace(gradient.ToScreenSpace(new Vector2(0, gradient.DrawHeight))).Y);
                    fadeTo = DepthAtFlatY(Warp.ToLocalSpace(gradient.ToScreenSpace(Vector2.Zero)).Y);
                    fadeAlpha = gradient.Alpha;
                }

                columnCovers[i] = new DepthCover(coveredFrom, coveredTo, fadeFrom, fadeTo, fadeAlpha);
            }
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

            if (columnLanes.Length != count - stages.Count)
                columnLanes = new Vector2[count - stages.Count];

            if (source.Width <= 0)
                return;

            int index = 0;
            int columnIndex = 0;

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

                    columnLanes[columnIndex++] = new Vector2(left, right);
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
