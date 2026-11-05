// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Overlays.Settings;

namespace osu.Game.Rulesets.ManiaOrtho.ScoreServer
{
    /// <summary>
    /// Whose account plays go to. When the game is linked to the score server by signing in with osu!
    /// (<see cref="ScoreServerLinking"/>), plays carry the server's token for this game, to that server only. Otherwise, when the
    /// game is signed in to the score server itself (Settings &gt; Online &gt; Server is the same address as the score server's,
    /// this fork only), plays carry that sign-in. osu!'s own sign-in is never sent: only a private server's
    /// (<see cref="CustomEndpointConfiguration"/>), and only to that same server.
    /// </summary>
    public static class ScoreServerSignIn
    {
        /// <summary>
        /// The token plays to <paramref name="server"/> carry: the game's link to that server, else the game's sign-in to that
        /// very server, else none.
        /// </summary>
        public static string? TokenFor(ScoreServerConfigManager? config, IAPIProvider? api, Uri server)
            => ScoreServerLinking.TokenFor(config, server) ?? TokenFor(api, server);

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
        /// What the score server address setting says about whose account plays go to: the account the game is linked to, or
        /// the one it is signed in to on that server, or that neither applies to this address.
        /// </summary>
        public static SettingsNote.Data? Describe(ScoreServerConfigManager? config, IAPIProvider? api, string? scoreServerAddress)
        {
            var server = ScoreServerClient.ParseAddress(scoreServerAddress, out _);

            if (ScoreServerLinking.Current(config) is { } link && server != null)
            {
                if (!SameServer(link.Server, server))
                {
                    return new SettingsNote.Data($"The game is linked to {link.Server.Authority}, not to this address, so plays sent here go to the score "
                                                 + "server's first account. Link with osu! again for this address.", SettingsNote.Type.Warning);
                }

                // Signed in to osu! in the game as someone else than the account the link signed in with.
                if (api != null && api.Endpoints is not CustomEndpointConfiguration && api.State.Value == APIState.Online && link.OsuUserId > 0
                    && api.LocalUser.Value.Id != link.OsuUserId)
                {
                    return new SettingsNote.Data($"Plays go to {link.Username}'s account, but the game is signed in to osu! as {api.LocalUser.Value.Username}. "
                                                 + "Unlink and link with osu! again to use that account.", SettingsNote.Type.Warning);
                }

                return new SettingsNote.Data($"Plays go to {link.Username}'s account (linked with osu!).", SettingsNote.Type.Informational);
            }

            return describeSignIn(api, server)
                   ?? (server == null
                       ? null
                       : new SettingsNote.Data("Not linked: plays go to the score server's first account. Press \"Link with osu!\" to send them to yours.",
                           SettingsNote.Type.Informational));
        }

        private static SettingsNote.Data? describeSignIn(IAPIProvider? api, Uri? server)
        {
            if (api?.Endpoints is not CustomEndpointConfiguration custom
                || server == null
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
