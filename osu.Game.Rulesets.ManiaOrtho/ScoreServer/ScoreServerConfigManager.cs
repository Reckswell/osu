// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Configuration;
using osu.Game.Rulesets.Configuration;

namespace osu.Game.Rulesets.ManiaOrtho.ScoreServer
{
    /// <summary>
    /// Whether, and where, plays go to the player's own score server (<c>ortho-server</c>). Stored under this ruleset next to
    /// mania's and the highway's settings, so its keys share no name with theirs. Off unless the player turns it on.
    /// </summary>
    public class ScoreServerConfigManager : RulesetConfigManager<ScoreServerSetting>
    {
        /// <summary>
        /// Where <c>scripts/ortho/server</c> runs the server: this computer only.
        /// </summary>
        public const string DEFAULT_ADDRESS = "http://127.0.0.1:7727";

        public ScoreServerConfigManager(SettingsStore? settings, RulesetInfo ruleset, int? variant = null)
            : base(settings, ruleset, variant)
        {
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            SetDefault(ScoreServerSetting.ScoreServerEnabled, false);
            SetDefault(ScoreServerSetting.ScoreServerAddress, DEFAULT_ADDRESS);
        }
    }

    public enum ScoreServerSetting
    {
        ScoreServerEnabled,
        ScoreServerAddress,
    }
}
