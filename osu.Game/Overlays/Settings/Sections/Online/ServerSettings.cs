// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online;
using osu.Game.Online.API;

namespace osu.Game.Overlays.Settings.Sections.Online
{
    /// <summary>
    /// The private server to sign in to instead of osu!'s (<see cref="OsuSetting.ServerAddress"/>). It takes effect at the next start.
    /// </summary>
    public partial class ServerSettings : SettingsSubsection
    {
        public const string OSU_SERVER = @"osu!'s own server";

        protected override LocalisableString Header => @"Server";

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private Bindable<string> address = null!;
        private SettingsItemV2 addressItem = null!;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            address = config.GetBindable<string>(OsuSetting.ServerAddress);

            FormTextBox textBox;

            Children = new Drawable[]
            {
                addressItem = new SettingsItemV2(textBox = new FormTextBox
                {
                    Caption = @"Server address",
                    HintText = @"A private server to sign in to instead of osu!'s, such as http://127.0.0.1:7727. Empty for osu!'s own. "
                               + @"osu!'s addresses (ppy.sh) are not taken here. The game connects at its next start.",
                    PlaceholderText = OSU_SERVER,
                    Current = address,
                })
                {
                    Keywords = new[] { @"private", @"custom", @"url", @"devserver", @"login", @"sign in" },
                },
            };

            // Typed and confirmed: a saved sign-in belongs to the server that issued it and must not go to another one at the next start.
            textBox.OnCommit += (_, _) =>
            {
                if (TargetAddress(address.Value) != ConnectedAddress(api) && api.State.Value != APIState.Offline)
                    api.Logout();
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            address.BindValueChanged(_ => addressItem.Note.Value = Describe(address.Value, ConnectedAddress(api)), true);
        }

        /// <summary>
        /// The address the game is connected to now, or <see langword="null"/> for osu!'s.
        /// </summary>
        public static string? ConnectedAddress(IAPIProvider api) => (api.Endpoints as CustomEndpointConfiguration)?.Address;

        /// <summary>
        /// The server an entered address connects to at the next start, or <see langword="null"/> for osu!'s.
        /// </summary>
        public static string? TargetAddress(string? entered) => CustomEndpointConfiguration.TryCreate(entered, out var custom) ? custom.Address : null;

        public static SettingsNote.Data? Describe(string? entered, string? connected)
        {
            string? target = TargetAddress(entered);

            if (target == null && !string.IsNullOrWhiteSpace(entered))
            {
                return new SettingsNote.Data(@"The game cannot use this address and connects to " + OSU_SERVER + @" instead. "
                                             + @"Enter a full address such as http://127.0.0.1:7727 (osu!'s own ppy.sh addresses are not taken here).",
                    SettingsNote.Type.Critical);
            }

            if (target != connected)
                return new SettingsNote.Data($@"Restart the game to connect to {target ?? OSU_SERVER}.", SettingsNote.Type.Warning);

            if (connected != null)
            {
                return new SettingsNote.Data($@"Connected to {connected}. Sign in with an account on this server; Register in the sign-in panel makes one.",
                    SettingsNote.Type.Informational);
            }

            return null;
        }
    }
}
