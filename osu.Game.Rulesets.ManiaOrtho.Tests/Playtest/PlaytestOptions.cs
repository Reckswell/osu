// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using System.Linq;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.ManiaOrtho.Highway;
using osu.Game.Rulesets.Mods;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.ManiaOrtho.Tests.Playtest
{
    /// <summary>
    /// Options for <see cref="TestScenePlaytest"/>. Read from environment variables so the same scene works from
    /// the capture runner (<c>playtest</c> command), the visual test browser and <c>dotnet test</c>.
    /// </summary>
    public class PlaytestOptions
    {
        public const string ENV_BEATMAP = "ORTHO_PLAYTEST_BEATMAP";
        public const string ENV_RULESET = "ORTHO_PLAYTEST_RULESET";
        public const string ENV_TIMES = "ORTHO_PLAYTEST_TIMES";
        public const string ENV_OUT = "ORTHO_PLAYTEST_OUT";
        public const string ENV_SCROLL = "ORTHO_PLAYTEST_SCROLL_SPEED";
        public const string ENV_FULL = "ORTHO_PLAYTEST_PLAY_TO_END";
        public const string ENV_PRESET = "ORTHO_PLAYTEST_PRESET";
        public const string ENV_WALL = "ORTHO_PLAYTEST_WALL";
        public const string ENV_SKIN = "ORTHO_PLAYTEST_SKIN";
        public const string ENV_NOTES = "ORTHO_PLAYTEST_NOTES";
        public const string ENV_MODS = "ORTHO_PLAYTEST_MODS";

        /// <summary>See <see cref="PlaytestBeatmaps.Load"/>.</summary>
        public string Beatmap { get; init; } = "synthetic-7k";

        /// <summary><c>maniaortho</c> or <c>mania</c> (for side-by-side baselines).</summary>
        public string Ruleset { get; init; } = ManiaOrthoRuleset.SHORT_NAME;

        /// <summary>Gameplay times (ms) at which to freeze and screenshot. Empty means 20/45/70/90% through the chart.</summary>
        public double[] Times { get; init; } = Array.Empty<double>();

        /// <summary>Where screenshots and summary.json are written. Null disables capture (e.g. headless runs).</summary>
        public string? OutputDirectory { get; init; }

        /// <summary>Mania scroll speed setting (1-40). Null keeps the default.</summary>
        public double? ScrollSpeed { get; init; }

        /// <summary>Whether to keep playing to the end and record final statistics.</summary>
        public bool PlayToEnd { get; init; }

        /// <summary>
        /// The highway preset to play with (maniaortho only). Defaults to the ruleset's default preset.
        /// </summary>
        public HighwayPreset Preset { get; init; } = HighwayPresets.DEFAULT;

        /// <summary>
        /// The wall position, if overriding the preset's (maniaortho only).
        /// </summary>
        public float? Wall { get; init; }

        /// <summary>
        /// One of the built-in skins: argon (the default), argon-pro, triangles or classic (the legacy skin).
        /// </summary>
        public string Skin { get; init; } = "argon";

        /// <summary>
        /// How maniaortho draws notes (skin or highway).
        /// </summary>
        public HighwayNoteStyle NoteStyle { get; init; } = HighwayNoteStyle.Skin;

        /// <summary>
        /// Mod acronyms to play with besides autoplay (and no fail), such as HD, FI or CO.
        /// </summary>
        public string[] Mods { get; init; } = Array.Empty<string>();

        public Mod[] CreateMods(Ruleset ruleset) => Mods.Select(acronym => ruleset.CreateModFromAcronym(acronym)
                                                                   ?? throw new ArgumentException($"{ruleset.Description} has no mod \"{acronym}\".")).ToArray();

        public SkinInfo CreateSkinInfo() => Skin switch
        {
            "argon" => ArgonSkin.CreateInfo(),
            "argon-pro" => ArgonProSkin.CreateInfo(),
            "triangles" => TrianglesSkin.CreateInfo(),
            "classic" => DefaultLegacySkin.CreateInfo(),
            _ => throw new ArgumentException($"Unknown skin \"{Skin}\" (expected argon, argon-pro, triangles or classic)."),
        };

        public Ruleset CreateRuleset() => Ruleset switch
        {
            ManiaOrthoRuleset.SHORT_NAME => new ManiaOrthoRuleset(),
            "mania" => new ManiaRuleset(),
            _ => throw new ArgumentException($"Unknown ruleset \"{Ruleset}\" (expected {ManiaOrthoRuleset.SHORT_NAME} or mania)."),
        };

        public static PlaytestOptions FromEnvironment()
        {
            string? get(string name) => string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name)) ? null : Environment.GetEnvironmentVariable(name);

            var defaults = new PlaytestOptions();

            return new PlaytestOptions
            {
                Beatmap = get(ENV_BEATMAP) ?? defaults.Beatmap,
                Ruleset = get(ENV_RULESET) ?? defaults.Ruleset,
                Times = get(ENV_TIMES)?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                      .Select(t => double.Parse(t, CultureInfo.InvariantCulture))
                                      .OrderBy(t => t)
                                      .ToArray() ?? defaults.Times,
                OutputDirectory = get(ENV_OUT),
                ScrollSpeed = get(ENV_SCROLL) is string s ? double.Parse(s, CultureInfo.InvariantCulture) : null,
                PlayToEnd = get(ENV_FULL) == "1",
                Preset = get(ENV_PRESET) is string p ? Enum.Parse<HighwayPreset>(p, true) : defaults.Preset,
                Wall = get(ENV_WALL) is string w ? float.Parse(w, CultureInfo.InvariantCulture) : null,
                Skin = get(ENV_SKIN) ?? defaults.Skin,
                NoteStyle = get(ENV_NOTES) is string n ? Enum.Parse<HighwayNoteStyle>(n, true) : defaults.NoteStyle,
                Mods = get(ENV_MODS)?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? defaults.Mods,
            };
        }
    }
}
