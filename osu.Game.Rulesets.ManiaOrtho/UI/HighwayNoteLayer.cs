// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Utils;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.ManiaOrtho.Highway;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI.Scrolling.Algorithms;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.ManiaOrtho.UI
{
    /// <summary>
    /// Draws mania's notes natively on the track (SPEC 6, research/game-chunithm.md P5): flat bars with a fixed thickness in
    /// track units, so they keep Chunithm's proportions at every depth and stay sharp, instead of mania's skin pieces warped
    /// with the stage.
    /// </summary>
    /// <remarks>
    /// Mania's drawable hit objects stay in the stage and do all the judging; only their drawing is taken over (the column
    /// hit object containers are proxied away, see <see cref="HighwayPlayfieldAdjustmentContainer"/>). Every frame this reads
    /// where they are from the hit objects' times and mania's own scroll algorithm, and their state (alpha, colour, held,
    /// broken) from the drawables.
    /// </remarks>
    public partial class HighwayNoteLayer : Drawable
    {
        /// <summary>
        /// Note thickness in track depth: Chunithm's TAP is 2.55% of the visible track (research P5).
        /// </summary>
        public const float NOTE_THICKNESS = 0.0255f;

        /// <summary>
        /// A hold's tail cap, in track depth.
        /// </summary>
        public const float TAIL_THICKNESS = NOTE_THICKNESS / 2;

        /// <summary>
        /// The gap on each side of a note, as a fraction of its lane (a 98% wide note).
        /// </summary>
        public const float NOTE_GAP = 0.01f;

        /// <summary>
        /// Hold bodies are narrower than their heads, as a fraction of the note width.
        /// </summary>
        public const float BODY_WIDTH = 0.8f;

        /// <summary>
        /// The bright rim at a note's near edge, as a fraction of its thickness.
        /// </summary>
        private const float rim_fraction = 0.2f;

        private static readonly Color4 broken_colour = new Color4(0.45f, 0.45f, 0.45f, 1);

        /// <summary>
        /// The built-in colours, after Alpha's own skin (research/baseline-alpha.md): white outer lanes, pink inner lanes,
        /// counted inwards from both edges, and amber for mania's special (centre) column.
        /// </summary>
        public static readonly Color4 OUTER_COLOUR = Color4.White;

        public static readonly Color4 INNER_COLOUR = Color4Extensions.FromHex("#ff8290");

        public static readonly Color4 SPECIAL_COLOUR = Color4Extensions.FromHex("#ffc528");

        /// <summary>
        /// Use each note's timing-based (snap) colour, as mania's "Timing-based note colouring" setting does.
        /// </summary>
        public bool SnapColours { get; set; }

        private HighwayLayout layout = new HighwayLayout();

        public HighwayLayout Layout
        {
            get => layout;
            set
            {
                if (layout == value)
                    return;

                layout = value;
                Invalidate(Invalidation.DrawNode);
            }
        }

        private NoteQuad[] quads = new NoteQuad[256];

        /// <summary>
        /// The number of quads gathered this frame.
        /// </summary>
        public int Count { get; private set; }

        public NoteQuad GetQuad(int index) => quads[index];

        private IShader shader = null!;

        public HighwayNoteLayer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders)
        {
            shader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE);
        }

        /// <summary>
        /// Collects every alive note of <paramref name="playfield"/>, in the lanes <paramref name="columnLanes"/> gives for each
        /// column (in playfield column order, lane space), at the depths <paramref name="scroll"/> maps their times to.
        /// </summary>
        public void Gather(ManiaPlayfield playfield, ReadOnlySpan<Vector2> columnLanes, IScrollAlgorithm algorithm, double timeRange, HighwayScroll scroll,
                           double currentTime)
        {
            Count = 0;
            Invalidate(Invalidation.DrawNode);

            var stages = playfield.Stages;
            int columnIndex = 0;

            for (int i = 0; i < stages.Count; i++)
            {
                var columns = stages[i].Columns;

                for (int j = 0; j < columns.Length; j++, columnIndex++)
                {
                    if (columnIndex >= columnLanes.Length)
                        return;

                    var column = columns[j];

                    Vector2 lanes = columnLanes[columnIndex];
                    float inset = (lanes.Y - lanes.X) * NOTE_GAP;
                    float lane0 = lanes.X + inset;
                    float lane1 = lanes.Y - inset;
                    Color4 colour = ColumnColour(column.AccentColour.Value, j, columns.Length, column.IsSpecial);

                    foreach (var entry in column.HitObjectContainer.AliveEntries)
                    {
                        switch (entry.Value)
                        {
                            case DrawableHoldNote hold:
                                addHold(hold, lane0, lane1, colour, algorithm, timeRange, scroll, currentTime);
                                break;

                            case DrawableNote note:
                                addNote(note, lane0, lane1, colour, algorithm, timeRange, scroll, currentTime);
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// A column's note colour: the skin's column colour when it is meant for notes (argon and triangles colour their
        /// notes with it), otherwise the built-in colours. Legacy skins' column colour is the lane background, usually
        /// black or faint, and their notes are textures, so it says nothing about how notes should look.
        /// </summary>
        public static Color4 ColumnColour(Color4 accent, int index, int columnCount, bool isSpecial)
        {
            if (accent.A >= 0.5f && Math.Max(accent.R, Math.Max(accent.G, accent.B)) >= 0.35f)
                return accent.Opacity(1f);

            if (isSpecial)
                return SPECIAL_COLOUR;

            int fromEdge = Math.Min(index, columnCount - 1 - index);
            return fromEdge % 2 == 0 ? OUTER_COLOUR : INNER_COLOUR;
        }

        /// <summary>
        /// The colour <paramref name="snapSource"/>'s note is drawn in: its snap colour when timing-based colouring is on
        /// (mania sets it as the drawable's colour, white otherwise), else its column's colour.
        /// </summary>
        private Color4 noteColour(DrawableNote snapSource, Color4 columnColour)
            => SnapColours ? snapSource.Colour.TopLeft.SRGB.Opacity(1f) : columnColour;

        private void addNote(DrawableNote note, float lane0, float lane1, Color4 columnColour, IScrollAlgorithm algorithm, double timeRange, HighwayScroll scroll,
                             double currentTime)
        {
            if (note.Alpha <= 0)
                return;

            float depth = scroll.DepthAt(algorithm, note.HitObject.StartTime, currentTime, timeRange);
            addBar(note.HitObject, NoteQuadKind.Note, lane0, lane1, depth, NOTE_THICKNESS, noteColour(note, columnColour), note.Alpha);
        }

        private void addHold(DrawableHoldNote hold, float lane0, float lane1, Color4 columnColour, IScrollAlgorithm algorithm, double timeRange, HighwayScroll scroll,
                             double currentTime)
        {
            if (hold.Alpha <= 0)
                return;

            double startTime = hold.HitObject.StartTime;

            float headDepth = scroll.DepthAt(algorithm, startTime, currentTime, timeRange);
            float tailDepth = headDepth + scroll.LengthBetween(algorithm, startTime, hold.HitObject.EndTime, timeRange);

            // As in mania, a held note's head stops at the judgement line and the body shortens from there, unless it was hit
            // early (it waits for its time) or the hold was dropped. Held past its end, the head follows the tail below the line.
            if (currentTime >= startTime && hold.Head.IsHit && !hold.Result.DroppedHoldAfter(startTime))
                headDepth = Math.Max(headDepth, Math.Min(0, tailDepth));

            bool broken = hold.MissingStartTime.Value != null;
            Color4 colour = broken ? broken_colour : noteColour(hold.Head, columnColour);

            float bodyInset = (lane1 - lane0) * (1 - BODY_WIDTH) / 2;
            float bodyAlpha = broken ? 0.35f : hold.IsHolding.Value ? 0.85f : 0.55f;
            float bodyStart = headDepth + NOTE_THICKNESS / 2;

            if (tailDepth > bodyStart)
            {
                Color4 body = colour.Opacity(bodyAlpha * hold.Alpha);
                add(new NoteQuad(hold.HitObject, NoteQuadKind.HoldBody, lane0 + bodyInset, lane1 - bodyInset, bodyStart, tailDepth, body, body));
            }

            addBar(hold.HitObject, NoteQuadKind.HoldTail, lane0, lane1, tailDepth, TAIL_THICKNESS, colour, hold.Alpha * hold.Tail.Alpha);
            addBar(hold.HitObject, NoteQuadKind.HoldHead, lane0, lane1, headDepth, NOTE_THICKNESS, colour, hold.Alpha * hold.Head.Alpha);
        }

        /// <summary>
        /// A note-like bar from <paramref name="depth"/> (its leading edge, which crosses the judgement line at its time) to
        /// <paramref name="depth"/> + <paramref name="thickness"/>, with a bright rim at the near edge.
        /// </summary>
        private void addBar(HitObject hitObject, NoteQuadKind kind, float lane0, float lane1, float depth, float thickness, Color4 colour, float alpha)
        {
            if (alpha <= 0)
                return;

            float rim = depth + thickness * rim_fraction;

            add(new NoteQuad(hitObject, kind, lane0, lane1, rim, depth + thickness, colour.Lighten(0.25f).Opacity(alpha), colour.Opacity(alpha)));
            add(new NoteQuad(hitObject, kind, lane0, lane1, depth, rim, Color4.White.Opacity(alpha), Color4.White.Opacity(alpha)));
        }

        private void add(in NoteQuad quad)
        {
            if (Count == quads.Length)
                Array.Resize(ref quads, quads.Length * 2);

            quads[Count++] = quad;
        }

        protected override DrawNode CreateDrawNode() => new HighwayNoteLayerDrawNode(this);

        public enum NoteQuadKind
        {
            Note,
            HoldHead,
            HoldBody,
            HoldTail,
        }

        /// <summary>
        /// One flat quad on the track, from <see cref="Depth0"/> (near) to <see cref="Depth1"/> (far).
        /// </summary>
        public readonly record struct NoteQuad(HitObject HitObject, NoteQuadKind Kind, float Lane0, float Lane1, float Depth0, float Depth1, Color4 Near, Color4 Far);

        private class HighwayNoteLayerDrawNode : DrawNode
        {
            protected new HighwayNoteLayer Source => (HighwayNoteLayer)base.Source;

            private IShader shader = null!;
            private HighwayProjection projection;
            private NoteQuad[] quads = Array.Empty<NoteQuad>();
            private int count;
            private float alpha;

            public HighwayNoteLayerDrawNode(HighwayNoteLayer source)
                : base(source)
            {
            }

            public override void ApplyState()
            {
                base.ApplyState();

                shader = Source.shader;
                projection = new HighwayProjection(Source.layout, Source.DrawSize);
                alpha = DrawColourInfo.Colour.MaxAlpha;

                count = Source.Count;
                if (quads.Length < count)
                    quads = new NoteQuad[Source.quads.Length];
                Array.Copy(Source.quads, quads, count);
            }

            protected override void Draw(IRenderer renderer)
            {
                base.Draw(renderer);

                if (alpha <= 0 || count == 0)
                    return;

                shader.Bind();

                // Bodies first, so heads and tails are drawn over them.
                for (int i = 0; i < count; i++)
                {
                    if (quads[i].Kind == NoteQuadKind.HoldBody)
                        draw(renderer, quads[i]);
                }

                for (int i = 0; i < count; i++)
                {
                    if (quads[i].Kind != NoteQuadKind.HoldBody)
                        draw(renderer, quads[i]);
                }

                shader.Unbind();
            }

            private void draw(IRenderer renderer, in NoteQuad quad)
            {
                // Notes are drawn from the judgement line to the far edge: past the line they are gone, as in Chunithm.
                float depth0 = Math.Max(quad.Depth0, 0);
                float depth1 = Math.Min(quad.Depth1, 1);

                if (depth1 <= depth0)
                    return;

                // Keep the colours where they were on the unclipped quad.
                float length = quad.Depth1 - quad.Depth0;
                Color4 near = length > 0 ? Interpolation.ValueAt(depth0, quad.Near, quad.Far, quad.Depth0, quad.Depth1) : quad.Near;
                Color4 far = length > 0 ? Interpolation.ValueAt(depth1, quad.Near, quad.Far, quad.Depth0, quad.Depth1) : quad.Far;

                var screen = projection.ToScreen(quad.Lane0, quad.Lane1, depth0, depth1);

                renderer.DrawQuad(renderer.WhitePixel, screen * DrawInfo.Matrix,
                    ColourInfo.GradientVertical(far.Opacity(far.A * alpha), near.Opacity(near.A * alpha)));
            }
        }
    }
}
