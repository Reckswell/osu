// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.Settings.Sections.Online;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.ManiaOrtho.Tests.Online
{
    /// <summary>
    /// Settings &gt; Online &gt; Server: the address is saved as typed, a confirmed change of server signs out (a sign-in belongs
    /// to the server that issued it), and the note says what happens at the next start.
    /// </summary>
    public partial class TestSceneServerSettings : OsuManualInputManagerTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        private ServerSettings settings = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("osu!'s server, signed in", () =>
            {
                config.SetValue(OsuSetting.ServerAddress, string.Empty);

                var dummy = (DummyAPIAccess)API;
                dummy.SessionVerificationMethod = null;
                dummy.Login("alpha", "password");
            });

            AddStep("show the settings", () => Child = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Width = 0.5f,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Child = settings = new ServerSettings(),
            });

            AddUntilStep("loaded", () => settings.IsLoaded);
            AddAssert("signed in", () => API.State.Value, () => Is.EqualTo(APIState.Online));
            AddAssert("no note", () => note, () => Is.Null);
        }

        [TearDownSteps]
        public void TearDownSteps()
        {
            AddStep("back to osu!'s server", () => config.SetValue(OsuSetting.ServerAddress, string.Empty));
        }

        [Test]
        public void TestConfirmingAnotherServerSignsOut()
        {
            enter("127.0.0.1:7727");
            AddAssert("still signed in while typing", () => API.State.Value, () => Is.EqualTo(APIState.Online));
            AddAssert("saved as typed", () => config.Get<string>(OsuSetting.ServerAddress), () => Is.EqualTo("127.0.0.1:7727"));
            AddAssert("restart to connect", () => note?.Text.ToString(), () => Is.EqualTo("Restart the game to connect to http://127.0.0.1:7727."));

            confirm();
            AddAssert("signed out", () => API.State.Value, () => Is.EqualTo(APIState.Offline));
        }

        [Test]
        public void TestConfirmingTheSameServerKeepsSignIn()
        {
            enter(string.Empty);
            confirm();
            AddAssert("still signed in", () => API.State.Value, () => Is.EqualTo(APIState.Online));
        }

        /// <summary>
        /// An osu! address is not taken, so the game stays on osu!'s server and the sign-in stays valid.
        /// </summary>
        [Test]
        public void TestOsuAddressIsRefused()
        {
            enter("https://osu.ppy.sh");
            confirm();
            AddAssert("still signed in", () => API.State.Value, () => Is.EqualTo(APIState.Online));
            AddAssert("refused", () => note?.Type, () => Is.EqualTo(SettingsNote.Type.Critical));
        }

        private SettingsNote.Data? note => settings.ChildrenOfType<SettingsItemV2>().Single().Note.Value;

        private void enter(string address) => AddStep($"type {(address.Length > 0 ? address : "nothing")}", () =>
        {
            InputManager.MoveMouseTo(settings.ChildrenOfType<OsuTextBox>().Single());
            InputManager.Click(MouseButton.Left);
            settings.ChildrenOfType<FormTextBox>().Single().Current.Value = address;
        });

        private void confirm() => AddStep("press enter", () => InputManager.Key(Key.Enter));
    }
}
