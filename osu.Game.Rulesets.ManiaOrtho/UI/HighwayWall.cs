// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.ManiaOrtho.Highway;
using osuTK;

namespace osu.Game.Rulesets.ManiaOrtho.UI
{
    /// <summary>
    /// The field wall (SPEC 4, Chunithm's field wall): an opaque panel standing on the track at the depth where notes
    /// appear, hiding everything beyond it. Notes come out from under its lower edge.
    /// </summary>
    /// <remarks>
    /// Drawn as a vertical panel, as in the arcade (research/game-chunithm.md). Its image is a rectangle as wide as the track
    /// at the wall's depth, reaching up to the top of the screen; the track only narrows beyond it, so the rectangle covers it.
    /// Under the kiai sway and a ported side stage's lean it leans with the track, sheared about its lower edge.
    /// </remarks>
    public partial class HighwayWall : CompositeDrawable
    {
        /// <summary>
        /// The lower edge, marking where notes appear.
        /// </summary>
        private const float edge_thickness = 3;

        public HighwayWall()
        {
            Alpha = 0;
            // Placed by its lower edge, the one on the track, so a lean (the kiai sway) shears it about that edge.
            Origin = Anchor.BottomLeft;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#06060c"), Color4Extensions.FromHex("#14141f")),
                },
                new Box
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = edge_thickness,
                    // The arcade's guide-line teal (research/game-chunithm.md).
                    Colour = Color4Extensions.FromHex("#45d9c8"),
                },
            };
        }

        /// <summary>
        /// Moves the wall to where <paramref name="scroll"/> puts it. Hidden when there is no wall, or when the highway is
        /// drawn flat (mania's own Cover mod covers that case).
        /// </summary>
        public void Place(in HighwayProjection projection, HighwayScroll scroll, bool flat)
        {
            if (flat || scroll.Wall <= 0)
            {
                Alpha = 0;
                return;
            }

            float depth = scroll.AppearDepth;
            float bottom = projection.YAt(depth);

            Position = new Vector2(projection.ToScreen(0, depth).X, bottom);
            Size = new Vector2(projection.WidthAt(depth), bottom);
            // The track behind it leans with the kiai sway and any lean: lean with it, so the lanes beyond stay covered.
            Shear = new Vector2(bottom > 0 ? (projection.SwayAtY(0) - projection.SwayAtY(bottom)) / bottom : 0, 0);
            Alpha = 1;
        }
    }
}
