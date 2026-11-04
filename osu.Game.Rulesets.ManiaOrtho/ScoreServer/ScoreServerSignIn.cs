// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Overlays.Settings;

namespace osu.Game.Rulesets.ManiaOrtho.ScoreServer
{
    /// <summary>
    /// When the game is signed in to the score server itself (Settings &gt; Online &gt; Server is the same address as the score
    /// server's), plays are sent with that sign-in, so they go to the signed-in account. osu!'s own sign-in is never sent:
    /// only a private server's (<see cref="CustomEndpointConfiguration"/>), and only to that same server.
    /// </summary>
    public static class ScoreServerSignIn
    {
        /// <summary>
        /// The game's access token for <paramref name="server"/>, or null when the game is not signed in to exactly that server
        /// (same scheme, host and port).
        /// </summary>
        public static string? TokenFor(IAPIProvider? api, Uri server)
        {
            if (api?.Endpoints is not CustomEndpointConfiguration custom || api.State.Value != APIState.Online)
                return null;

            if (!Uri.TryCreate(custom.Address, UriKind.Absolute, out Uri? signedIn) || !SameServer(signedIn, server))
                return null;

            string token = api.AccessToken;
            return string.IsNullOrEmpty(token) ? null : token;
        }

        /// <summary>
        /// What the score server address setting says about the game's sign-in: whose account plays go to, or that the game is
        /// signed in to another address. Nothing while the game is on osu!'s servers.
        /// </summary>
        public static SettingsNote.Data? Describe(IAPIProvider? api, string? scoreServerAddress)
        {
            if (api?.Endpoints is not CustomEndpointConfiguration custom
                || ScoreServerClient.ParseAddress(scoreServerAddress, out _) is not Uri server
                || !Uri.TryCreate(custom.Address, UriKind.Absolute, out Uri? signedIn))
                return null;

            if (!SameServer(signedIn, server))
            {
                return new SettingsNote.Data($"The game is connected to {custom.Address}, not to this address, so plays are sent without your "
                                             + "sign-in and go to the score server's first account. Use the same address in both places.",
                    SettingsNote.Type.Warning);
            }

            return api.State.Value == APIState.Online
                ? new SettingsNote.Data($"Plays go to {api.LocalUser.Value.Username}'s account on this server.", SettingsNote.Type.Informational)
                : new SettingsNote.Data("Sign in (top right) to send plays under your account; until then they go to the server's first account.",
                    SettingsNote.Type.Informational);
        }

        public static bool SameServer(Uri a, Uri b)
            => a.Scheme.Equals(b.Scheme, StringComparison.OrdinalIgnoreCase)
               && a.Authority.Equals(b.Authority, StringComparison.OrdinalIgnoreCase)
               && !CustomEndpointConfiguration.IsOsuHost(b);
    }
}
