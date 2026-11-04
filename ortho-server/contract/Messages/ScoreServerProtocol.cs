// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

// Shared by the game and the server, so one namespace for both projects.
// ReSharper disable once CheckNamespace
namespace ManiaOrtho.ScoreServerContract
{
    public static class ScoreServerProtocol
    {
        /// <summary>
        /// A header every <c>POST</c> to the server must carry (any value). A web page cannot add it without the browser first
        /// asking the server (a CORS preflight, which the server never allows), so a page open in a browser cannot post plays
        /// or start a recalculation.
        /// </summary>
        public const string CLIENT_HEADER = "X-ManiaOrtho";
    }
}
