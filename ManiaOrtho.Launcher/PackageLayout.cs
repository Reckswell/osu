// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;

namespace ManiaOrtho.Launcher
{
    /// <summary>
    /// Where the packaged game and its data live. The launcher sits in the package folder with the game in
    /// <see cref="GAME_FOLDER"/> below it; the data is in <see cref="DATA_FOLDER"/> next to the package folder, so a new
    /// package (or a fresh clone) keeps it:
    /// <code>
    /// Documents\Projects\osu!\osu\osu.exe        the launcher
    /// Documents\Projects\osu!\osu\game\osu!.exe  the game
    /// Documents\Projects\osu!\ortho-data\        the game's data, also used by scripts\ortho\play.ps1
    /// </code>
    /// </summary>
    public sealed class PackageLayout
    {
        public const string GAME_FOLDER = "game";
        public const string GAME_EXECUTABLE = "osu!.exe";
        public const string DATA_FOLDER = "ortho-data";

        /// <summary>
        /// The folder the launcher is in.
        /// </summary>
        public string Package { get; }

        /// <summary>
        /// The game's data folder (database, settings, imported files, replays, logs).
        /// </summary>
        public string Data { get; }

        /// <summary>
        /// osu!stable's install, where screenshots go.
        /// </summary>
        public string Stable { get; }

        public string Game => Path.Combine(Package, GAME_FOLDER);

        public string GameExecutable => Path.Combine(Game, GAME_EXECUTABLE);

        /// <summary>
        /// Temporary files of the game, kept inside the package folder.
        /// </summary>
        public string Temp => Path.Combine(Package, "temp");

        public string Log => Path.Combine(Package, "launcher.log");

        public PackageLayout(string package, string data, string stable)
        {
            Package = trim(Path.GetFullPath(package));
            Data = trim(Path.GetFullPath(data));
            Stable = trim(Path.GetFullPath(stable));
        }

        /// <summary>
        /// The layout around a launcher in <paramref name="package"/>, with osu!stable at its default place unless given.
        /// </summary>
        public static PackageLayout ForPackage(string package, string? stable = null)
        {
            string full = trim(Path.GetFullPath(package));
            string parent = Path.GetDirectoryName(full) ?? full;

            return new PackageLayout(full, Path.Combine(parent, DATA_FOLDER),
                stable ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "osu!"));
        }

        private static string trim(string path)
        {
            string root = Path.GetPathRoot(path) ?? string.Empty;
            return path.Length > root.Length ? path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) : path;
        }
    }
}
