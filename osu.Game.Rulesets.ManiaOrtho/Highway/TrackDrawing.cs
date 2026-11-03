// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace osu.Game.Rulesets.ManiaOrtho.Highway
{
    /// <summary>
    /// Draws textured rectangles that lie on the track, for use inside a <c>DrawNode</c>.
    /// </summary>
    /// <remarks>
    /// The GPU interpolates texture coordinates linearly across each triangle, which is wrong under perspective: a
    /// trapezoid drawn as two triangles shows a visible bend along its diagonal (about a quarter of the width change
    /// across the quad, in pixels). Splitting the rectangle into a grid of small quads, each nearly a parallelogram,
    /// brings that error under a pixel without a custom shader. Each cell is placed exactly by the projection, so cell
    /// edges stay on the true perspective lines.
    /// </remarks>
    public static class TrackDrawing
    {
        /// <summary>
        /// Target height of one strip on screen, in pixels.
        /// </summary>
        public const float STRIP_HEIGHT = 8;

        public const int MAX_STRIPS = 256;

        /// <summary>
        /// Target width change across one cell, in pixels. The bend error is about a quarter of this.
        /// </summary>
        public const float MAX_WIDTH_CHANGE_PER_CELL = 1;

        public const int MAX_COLUMNS = 32;

        /// <summary>
        /// Draws <paramref name="texture"/> (or its <paramref name="textureRect"/>, in texture pixels) over the track rectangle
        /// lanes <paramref name="lane0"/>..<paramref name="lane1"/>, depths <paramref name="depth0"/> (near) to
        /// <paramref name="depth1"/> (far). The texture's top edge maps to the far end.
        /// </summary>
        /// <param name="renderer">The renderer.</param>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="projection">Projects track space into the drawable's local space.</param>
        /// <param name="toScreenSpace">The drawable's local-to-screen matrix (<c>DrawInfo.Matrix</c>).</param>
        /// <param name="lane0">Left edge, in lane space.</param>
        /// <param name="lane1">Right edge, in lane space.</param>
        /// <param name="depth0">Near edge depth.</param>
        /// <param name="depth1">Far edge depth.</param>
        /// <param name="colour">Colour, interpolated over the whole rectangle (top = far end).</param>
        /// <param name="textureRect">The part of the texture to draw, in texture pixels; the whole texture if null.</param>
        /// <param name="subdivide">Whether to subdivide. Small or thin rectangles (a note) can skip it.</param>
        public static void DrawTrackRectangle(IRenderer renderer, Texture texture, in HighwayProjection projection, Matrix3 toScreenSpace,
                                              float lane0, float lane1, float depth0, float depth1, ColourInfo colour,
                                              RectangleF? textureRect = null, bool subdivide = true)
        {
            // Anything nearer than the camera plane cannot be drawn.
            float minDepth = projection.MinDepth;
            if (depth1 <= minDepth || depth1 <= depth0)
                return;

            float fullDepth0 = depth0;
            depth0 = Math.Max(depth0, minDepth);

            var fullRect = textureRect ?? new RectangleF(0, 0, texture.DisplayWidth, texture.DisplayHeight);

            int strips = 1;
            int columns = 1;

            if (subdivide)
            {
                float screenHeight = projection.YAt(depth0) - projection.YAt(depth1);
                strips = Math.Clamp((int)MathF.Ceiling(screenHeight / STRIP_HEIGHT), 1, MAX_STRIPS);

                float widthChangePerStrip = (projection.WidthAt(depth0) - projection.WidthAt(depth1)) * Math.Abs(lane1 - lane0) / strips;
                columns = Math.Clamp((int)MathF.Ceiling(widthChangePerStrip / MAX_WIDTH_CHANGE_PER_CELL), 1, MAX_COLUMNS);
            }

            float totalDepth = depth1 - fullDepth0;

            for (int s = 0; s < strips; s++)
            {
                float dNear = depth0 + (depth1 - depth0) * s / strips;
                float dFar = depth0 + (depth1 - depth0) * (s + 1) / strips;

                // Fractions along the full rectangle, 0 at the near edge.
                float tNear = (dNear - fullDepth0) / totalDepth;
                float tFar = (dFar - fullDepth0) / totalDepth;

                for (int c = 0; c < columns; c++)
                {
                    float uLeft = (float)c / columns;
                    float uRight = (float)(c + 1) / columns;

                    float laneLeft = lane0 + (lane1 - lane0) * uLeft;
                    float laneRight = lane0 + (lane1 - lane0) * uRight;

                    var quad = projection.ToScreen(laneLeft, laneRight, dNear, dFar) * toScreenSpace;

                    var coords = new RectangleF(
                        fullRect.Left + fullRect.Width * uLeft,
                        fullRect.Top + fullRect.Height * (1 - tFar),
                        fullRect.Width * (uRight - uLeft),
                        fullRect.Height * (tFar - tNear));

                    ColourInfo cellColour = colour.HasSingleColour
                        ? colour
                        : new ColourInfo
                        {
                            TopLeft = colour.Interpolate(new Vector2(uLeft, 1 - tFar)),
                            TopRight = colour.Interpolate(new Vector2(uRight, 1 - tFar)),
                            BottomLeft = colour.Interpolate(new Vector2(uLeft, 1 - tNear)),
                            BottomRight = colour.Interpolate(new Vector2(uRight, 1 - tNear)),
                        };

                    renderer.DrawQuad(texture, quad, cellColour, textureRect: fullRect, textureCoords: coords);
                }
            }
        }
    }
}
