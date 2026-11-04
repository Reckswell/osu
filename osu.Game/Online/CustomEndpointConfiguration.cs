// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics.CodeAnalysis;
using osu.Game.Configuration;

namespace osu.Game.Online
{
    /// <summary>
    /// Every online service at one private server (<see cref="OsuSetting.ServerAddress"/>), as private lazer servers
    /// lay them out: the web API, OAuth and the website at its root, the SignalR hubs under <c>/signalr</c>.
    /// Never an osu! (<c>ppy.sh</c>) address: those are osu!'s own configurations. Plain http only on this computer.
    /// </summary>
    public class CustomEndpointConfiguration : EndpointConfiguration
    {
        /// <summary>
        /// Private servers accept any OAuth client; this one names the game it signs in from.
        /// </summary>
        public const string CLIENT_ID = @"lazer-custom-server";

        /// <summary>
        /// The server's address as entered, without a trailing slash (for example <c>http://127.0.0.1:7727</c>).
        /// </summary>
        public string Address { get; }

        private CustomEndpointConfiguration(string address)
        {
            Address = address;
            WebsiteUrl = APIUrl = address;
            APIClientID = CLIENT_ID;
            APIClientSecret = CLIENT_ID;
            SpectatorUrl = $@"{address}/signalr/spectator";
            MultiplayerUrl = $@"{address}/signalr/multiplayer";
            MetadataUrl = $@"{address}/signalr/metadata";
            BeatmapSubmissionServiceUrl = null;
            LivenessProbeUrl = null;
        }

        /// <summary>
        /// Plain http only to this server when it runs on this computer (as the maniaortho score server does); a server elsewhere
        /// needs https, so a password never crosses a network unencrypted.
        /// </summary>
        public override bool AllowsPlainHttp(string url)
            => Uri.TryCreate(Address, UriKind.Absolute, out Uri? server)
               && server.Scheme == Uri.UriSchemeHttp
               && server.IsLoopback
               && Uri.TryCreate(url, UriKind.Absolute, out Uri? target)
               && target.Scheme == Uri.UriSchemeHttp
               && target.Authority.Equals(server.Authority, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// The configuration for a server address, or <see langword="false"/> when it is empty, not an http(s) address,
        /// or one of osu!'s own (any <c>ppy.sh</c> host).
        /// </summary>
        public static bool TryCreate(string? address, [NotNullWhen(true)] out CustomEndpointConfiguration? configuration)
        {
            configuration = null;
            address = address?.Trim();

            if (string.IsNullOrEmpty(address))
                return false;

            if (!address.Contains(@"://", StringComparison.Ordinal))
                address = $@"http://{address}";

            if (!Uri.TryCreate(address, UriKind.Absolute, out Uri? uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                || string.IsNullOrEmpty(uri.Host)
                || !string.IsNullOrEmpty(uri.UserInfo)
                || uri.Query.Length > 0
                || uri.Fragment.Length > 0
                || IsOsuHost(uri))
                return false;

            // Plain http only on this computer (see AllowsPlainHttp): a server elsewhere is reached over https, the hubs included,
            // so a sign-in never crosses a network unencrypted.
            if (uri.Scheme == Uri.UriSchemeHttp && !uri.IsLoopback)
                uri = new UriBuilder(uri) { Scheme = Uri.UriSchemeHttps, Port = uri.IsDefaultPort ? -1 : uri.Port }.Uri;

            configuration = new CustomEndpointConfiguration(uri.GetLeftPart(UriPartial.Path).TrimEnd('/'));
            return true;
        }

        /// <summary>
        /// A saved sign-in belongs to the server that issued it. When the game starts with another server than the one that issued
        /// <see cref="OsuSetting.Token"/> (the address changed since), the token is forgotten rather than sent to the new server.
        /// A token saved before private servers could be chosen is osu!'s.
        /// </summary>
        public static void ForgetSignInFromOtherServer(OsuConfigManager config, EndpointConfiguration endpoints)
        {
            string issuer = config.Get<string>(OsuSetting.TokenServer);

            if (issuer == endpoints.APIUrl)
                return;

            if (issuer.Length > 0 || endpoints is CustomEndpointConfiguration)
                config.SetValue(OsuSetting.Token, string.Empty);

            config.SetValue(OsuSetting.TokenServer, endpoints.APIUrl);
        }

        /// <summary>
        /// Whether an address belongs to osu! itself (<c>ppy.sh</c> or any of its subdomains), by its host as written and as IDN.
        /// </summary>
        public static bool IsOsuHost(Uri uri) => isOsuHost(uri.Host) || isOsuHost(uri.IdnHost);

        private static bool isOsuHost(string host)
        {
            host = host.TrimEnd('.');
            return host.Equals(@"ppy.sh", StringComparison.OrdinalIgnoreCase) || host.EndsWith(@".ppy.sh", StringComparison.OrdinalIgnoreCase);
        }
    }
}
