// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Online;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.Settings.Sections.Online;

namespace osu.Game.Rulesets.ManiaOrtho.Tests.Online
{
    /// <summary>
    /// The core patch that lets the game sign in to a private server (README core patches: "Private server"):
    /// which addresses <see cref="CustomEndpointConfiguration"/> takes, where it sends each service, when it may use plain
    /// http, and what Settings &gt; Online &gt; Server says about an address.
    /// </summary>
    [TestFixture]
    public class CustomServerTest
    {
        [TestCase("http://127.0.0.1:7727", "http://127.0.0.1:7727")]
        [TestCase("http://127.0.0.1:7727/", "http://127.0.0.1:7727")]
        [TestCase("  http://127.0.0.1:7727//  ", "http://127.0.0.1:7727")]
        [TestCase("127.0.0.1:7727", "http://127.0.0.1:7727")]
        [TestCase("localhost:7727", "http://localhost:7727")]
        [TestCase("HTTP://LocalHost:7727", "http://localhost:7727")]
        [TestCase("http://[::1]:7727", "http://[::1]:7727")]
        [TestCase("https://scores.example.com", "https://scores.example.com")]
        [TestCase("https://example.com/osu/", "https://example.com/osu")]
        // Another computer is reached over https only, so a sign-in never crosses a network unencrypted.
        [TestCase("http://scores.example.com", "https://scores.example.com")]
        [TestCase("scores.example.com:8080", "https://scores.example.com:8080")]
        [TestCase("http://192.168.1.2:7727/osu", "https://192.168.1.2:7727/osu")]
        public void TestTakesAddress(string entered, string expected)
        {
            Assert.That(CustomEndpointConfiguration.TryCreate(entered, out var custom), Is.True);
            Assert.That(custom!.Address, Is.EqualTo(expected));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("/")]
        [TestCase("ftp://127.0.0.1:7727")]
        [TestCase("file:///home/alpha")]
        [TestCase("http://alpha:secret@127.0.0.1:7727")]
        [TestCase("http://127.0.0.1:7727/?server=1")]
        [TestCase("http://127.0.0.1:7727/#top")]
        [TestCase("http://")]
        [TestCase("http:///")]
        [TestCase("http://:7727")]
        [TestCase("not an address")]
        public void TestRefusesAddress(string? entered)
        {
            Assert.That(CustomEndpointConfiguration.TryCreate(entered, out var custom), Is.False);
            Assert.That(custom, Is.Null);
        }

        /// <summary>
        /// osu!'s own servers are never a private server, whatever way the address is written (invariant 2).
        /// </summary>
        [TestCase("ppy.sh")]
        [TestCase("osu.ppy.sh")]
        [TestCase("https://osu.ppy.sh")]
        [TestCase("https://OSU.PPY.SH/")]
        [TestCase("https://osu.ppy.sh./")]
        [TestCase("https://ppy.sh.")]
        [TestCase("http://dev.ppy.sh")]
        [TestCase("https://lazer.ppy.sh:443")]
        [TestCase("https://spectator.ppy.sh/signalr")]
        [TestCase("https://osu.ｐｐｙ.sh")]
        public void TestRefusesOsuServers(string entered)
        {
            Assert.That(CustomEndpointConfiguration.TryCreate(entered, out _), Is.False);
            Assert.That(ServerSettings.TargetAddress(entered), Is.Null);
        }

        [TestCase("https://notppy.sh")]
        [TestCase("https://ppy.sh.example.com")]
        [TestCase("https://ppy-sh.example.com")]
        public void TestOtherHostsAreNotOsu(string entered)
        {
            Assert.That(CustomEndpointConfiguration.IsOsuHost(new Uri(entered)), Is.False);
            Assert.That(CustomEndpointConfiguration.TryCreate(entered, out _), Is.True);
        }

        /// <summary>
        /// Every service sits at the server, as private lazer servers lay them out; nothing points at osu!.
        /// </summary>
        [Test]
        public void TestServicesAtTheServer()
        {
            CustomEndpointConfiguration.TryCreate("http://127.0.0.1:7727/", out var custom);

            Assert.Multiple(() =>
            {
                Assert.That(custom!.APIUrl, Is.EqualTo("http://127.0.0.1:7727"));
                Assert.That(custom.WebsiteUrl, Is.EqualTo("http://127.0.0.1:7727"));
                Assert.That(custom.SpectatorUrl, Is.EqualTo("http://127.0.0.1:7727/signalr/spectator"));
                Assert.That(custom.MultiplayerUrl, Is.EqualTo("http://127.0.0.1:7727/signalr/multiplayer"));
                Assert.That(custom.MetadataUrl, Is.EqualTo("http://127.0.0.1:7727/signalr/metadata"));
                Assert.That(custom.BeatmapSubmissionServiceUrl, Is.Null);
                Assert.That(custom.LivenessProbeUrl, Is.Null);
                Assert.That(custom.APIClientID, Is.EqualTo(CustomEndpointConfiguration.CLIENT_ID));
                Assert.That(custom.APIClientSecret, Is.EqualTo(CustomEndpointConfiguration.CLIENT_ID));
            });
        }

        [Test]
        public void TestServerElsewhereOnlyOverHttps()
        {
            CustomEndpointConfiguration.TryCreate("http://scores.example.com", out var custom);

            Assert.Multiple(() =>
            {
                Assert.That(custom!.APIUrl, Is.EqualTo("https://scores.example.com"));
                Assert.That(custom.SpectatorUrl, Is.EqualTo("https://scores.example.com/signalr/spectator"));
                Assert.That(custom.MultiplayerUrl, Is.EqualTo("https://scores.example.com/signalr/multiplayer"));
                Assert.That(custom.MetadataUrl, Is.EqualTo("https://scores.example.com/signalr/metadata"));
            });
        }

        /// <summary>
        /// The framework turns http into https unless a request allows plain http. Only a server on this computer gets it,
        /// and only for its own address, so a password never crosses a network unencrypted.
        /// </summary>
        [TestCase("http://127.0.0.1:7727", "http://127.0.0.1:7727/oauth/token", true)]
        [TestCase("http://127.0.0.1:7727", "http://127.0.0.1:7727/api/v2/me/mania", true)]
        [TestCase("http://localhost:7727", "http://LOCALHOST:7727/users", true)]
        [TestCase("http://[::1]:7727", "http://[::1]:7727/users", true)]
        [TestCase("http://127.0.0.1:7727", "http://127.0.0.1:7728/oauth/token", false)]
        [TestCase("http://127.0.0.1:7727", "http://192.168.1.2:7727/oauth/token", false)]
        [TestCase("http://127.0.0.1:7727", "https://127.0.0.1:7727/oauth/token", false)]
        [TestCase("http://127.0.0.1:7727", "http://osu.ppy.sh/oauth/token", false)]
        [TestCase("http://127.0.0.1:7727", "not a url", false)]
        [TestCase("http://192.168.1.2:7727", "http://192.168.1.2:7727/oauth/token", false)]
        [TestCase("http://scores.example.com", "http://scores.example.com/oauth/token", false)]
        [TestCase("http://scores.example.com", "https://scores.example.com/oauth/token", false)]
        [TestCase("https://127.0.0.1:7727", "http://127.0.0.1:7727/oauth/token", false)]
        public void TestPlainHttpOnlyToThisComputer(string server, string url, bool allowed)
        {
            CustomEndpointConfiguration.TryCreate(server, out var custom);
            Assert.That(custom!.AllowsPlainHttp(url), Is.EqualTo(allowed));
        }

        [Test]
        public void TestOsuServersNeverUsePlainHttp()
        {
            Assert.Multiple(() =>
            {
                foreach (var endpoints in new EndpointConfiguration[] { new ProductionEndpointConfiguration(), new DevelopmentEndpointConfiguration() })
                {
                    Assert.That(endpoints.AllowsPlainHttp($"{endpoints.APIUrl}/oauth/token"), Is.False);
                    Assert.That(endpoints.AllowsPlainHttp($"{endpoints.APIUrl.Replace("https://", "http://", StringComparison.Ordinal)}/oauth/token"), Is.False);
                    Assert.That(endpoints.AllowsPlainHttp("http://127.0.0.1:7727/oauth/token"), Is.False);
                }
            });
        }

        [Test]
        public void TestNotes()
        {
            Assert.Multiple(() =>
            {
                // osu!'s server, nothing entered: nothing to say.
                Assert.That(ServerSettings.Describe("", null), Is.Null);
                Assert.That(ServerSettings.Describe(null, null), Is.Null);

                // An address the game cannot use, whatever it is connected to.
                Assert.That(ServerSettings.Describe("https://osu.ppy.sh", null)?.Type, Is.EqualTo(SettingsNote.Type.Critical));
                Assert.That(ServerSettings.Describe("ftp://127.0.0.1", "http://127.0.0.1:7727")?.Type, Is.EqualTo(SettingsNote.Type.Critical));

                // A change that takes effect at the next start.
                var connect = ServerSettings.Describe("127.0.0.1:7727", null);
                Assert.That(connect?.Type, Is.EqualTo(SettingsNote.Type.Warning));
                Assert.That(connect?.Text.ToString(), Is.EqualTo("Restart the game to connect to http://127.0.0.1:7727."));

                var back = ServerSettings.Describe("", "http://127.0.0.1:7727");
                Assert.That(back?.Type, Is.EqualTo(SettingsNote.Type.Warning));
                Assert.That(back?.Text.ToString(), Is.EqualTo($"Restart the game to connect to {ServerSettings.OSU_SERVER}."));

                Assert.That(ServerSettings.Describe("http://127.0.0.1:7728", "http://127.0.0.1:7727")?.Type, Is.EqualTo(SettingsNote.Type.Warning));

                // Connected to the server entered (written another way is the same server).
                var connected = ServerSettings.Describe("127.0.0.1:7727/", "http://127.0.0.1:7727");
                Assert.That(connected?.Type, Is.EqualTo(SettingsNote.Type.Informational));
                Assert.That(connected?.Text.ToString(), Does.StartWith("Connected to http://127.0.0.1:7727."));
            });
        }
    }
}
