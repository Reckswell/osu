// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Tests;

namespace osu.Game.Rulesets.ManiaOrtho.Tests.Online
{
    /// <summary>
    /// The game reads Settings &gt; Online &gt; Server when it starts (<see cref="OsuGameBase.CreateEndpoints"/>): an address it
    /// can use replaces osu!'s endpoints, anything else (empty, or an osu! address) leaves osu!'s in place. A saved sign-in
    /// is kept only for the server that issued it (<see cref="CustomEndpointConfiguration.ForgetSignInFromOtherServer"/>).
    /// </summary>
    [TestFixture]
    public partial class CustomServerGameTest
    {
        private const string local_server = "http://127.0.0.1:7727";

        [Test]
        public void TestServerFromSettings()
        {
            var start = startGame("127.0.0.1:7727");

            Assert.That(start.Endpoints, Is.TypeOf<CustomEndpointConfiguration>());
            Assert.That(start.Endpoints.APIUrl, Is.EqualTo(local_server));
        }

        [TestCase("")]
        [TestCase("https://osu.ppy.sh")]
        [TestCase("ftp://127.0.0.1:7727")]
        public void TestOsuServersOtherwise(string address)
        {
            var start = startGame(address);

            Assert.That(start.Endpoints, Is.Not.InstanceOf<CustomEndpointConfiguration>());
            Assert.That(new Uri(start.Endpoints.APIUrl).Host, Does.EndWith("ppy.sh"));
        }

        /// <summary>
        /// osu!'s saved sign-in never reaches a private server: switching to one forgets it, even one saved before the setting existed.
        /// </summary>
        [Test]
        public void TestOsuSignInStaysWithOsu()
        {
            var start = startGame(local_server, token: "osu-token", tokenServer: string.Empty);

            Assert.That(start.Token, Is.Empty);
            Assert.That(start.TokenServer, Is.EqualTo(local_server));
        }

        /// <summary>
        /// And the private server's sign-in never reaches osu! when the address is emptied again.
        /// </summary>
        [Test]
        public void TestServerSignInStaysWithServer()
        {
            var start = startGame(string.Empty, token: "server-token", tokenServer: local_server);

            Assert.That(start.Token, Is.Empty);
            Assert.That(start.TokenServer, Is.EqualTo(start.Endpoints.APIUrl));
        }

        [Test]
        public void TestSignInKeptOnSameServer()
        {
            Assert.Multiple(() =>
            {
                Assert.That(startGame("127.0.0.1:7727/", token: "server-token", tokenServer: local_server).Token, Is.EqualTo("server-token"));
                Assert.That(startGame(local_server, token: "server-token", tokenServer: "http://127.0.0.1:7728").Token, Is.Empty);
            });
        }

        /// <summary>
        /// A token saved before private servers could be chosen is osu!'s, and stays signed in on osu!.
        /// </summary>
        [Test]
        public void TestOsuSignInKeptOnOsu()
        {
            var start = startGame(string.Empty, token: "osu-token", tokenServer: string.Empty);

            Assert.That(start.Token, Is.EqualTo("osu-token"));
            Assert.That(start.TokenServer, Is.EqualTo(start.Endpoints.APIUrl));
            Assert.That(startGame(string.Empty, token: "osu-token", tokenServer: start.Endpoints.APIUrl).Token, Is.EqualTo("osu-token"));
        }

        private static Start startGame(string address, string token = "", string tokenServer = "")
        {
            using (var host = new CleanRunHeadlessGameHost(callingMethodName: $"{nameof(CustomServerGameTest)}-{Guid.NewGuid()}"))
            {
                var game = new StartingGame(address, token, tokenServer);
                var run = Task.Factory.StartNew(state => ((GameHost)state!).Run(game), host, TaskCreationOptions.LongRunning);

                try
                {
                    var deadline = DateTime.UtcNow.AddSeconds(60);

                    while (!game.IsLoaded && !run.IsCompleted && DateTime.UtcNow < deadline)
                        Thread.Sleep(50);

                    if (run.IsFaulted)
                        throw run.Exception!;

                    Assert.That(game.IsLoaded, "the game did not start");
                    return new Start(game.Endpoints!, game.Config.Get<string>(OsuSetting.Token), game.Config.Get<string>(OsuSetting.TokenServer));
                }
                finally
                {
                    host.Exit();
                    run.Wait(TimeSpan.FromSeconds(30));
                }
            }
        }

        private record Start(EndpointConfiguration Endpoints, string Token, string TokenServer);

        /// <summary>
        /// The game as it starts, with a server address and a sign-in saved in its settings and a stand-in API, so nothing goes online.
        /// </summary>
        private partial class StartingGame : OsuGameBase
        {
            private readonly string address;
            private readonly string token;
            private readonly string tokenServer;

            public volatile EndpointConfiguration? Endpoints;

            public OsuConfigManager Config => LocalConfig;

            public StartingGame(string address, string token, string tokenServer)
            {
                this.address = address;
                this.token = token;
                this.tokenServer = tokenServer;
                API = new DummyAPIAccess();
            }

            public override void SetHost(GameHost host)
            {
                base.SetHost(host);
                LocalConfig.SetValue(OsuSetting.ServerAddress, address);
                LocalConfig.SetValue(OsuSetting.Token, token);
                LocalConfig.SetValue(OsuSetting.TokenServer, tokenServer);
            }

            public override EndpointConfiguration CreateEndpoints() => Endpoints = base.CreateEndpoints();
        }
    }
}
