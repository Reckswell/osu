// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.IO;

namespace ManiaOrtho.Launcher
{
    /// <summary>
    /// What <c>osu.exe</c> does around the packaged game, as <c>scripts/ortho/play.ps1</c> does around the clone's build:
    /// it keeps the game's data in <see cref="PackageLayout.Data"/> and its temporary files in the package, and moves
    /// screenshots into osu!stable's folder after each run. See docs/ortho/PLAYTESTING.md, "L4".
    /// </summary>
    public static class GameLauncher
    {
        public const string DATA_NOTE = "ortho-data.txt";

        /// <summary>
        /// Readable by <c>OsuGameDesktop.IsPackageManaged</c>: no installer hooks (Velopack) and no update checks, since
        /// this build is updated by packaging it again.
        /// </summary>
        public const string UPDATE_PROVIDER = "maniaortho package (scripts/ortho/package.ps1)";

        /// <summary>
        /// Makes the game folder a portable install with its data in <see cref="PackageLayout.Data"/>: an empty
        /// framework.ini next to the executable makes osu-framework keep the game's storage there (and never open the
        /// profile's data folders), and storage.ini there is osu!'s own custom data location.
        /// </summary>
        public static void PrepareGame(PackageLayout layout)
        {
            string frameworkIni = Path.Combine(layout.Game, "framework.ini");

            // osu! keeps its framework settings with the data (the host's storage is the game's), so this one stays empty.
            if (!File.Exists(frameworkIni))
                File.WriteAllText(frameworkIni, string.Empty);

            File.WriteAllText(Path.Combine(layout.Game, "storage.ini"), $"FullPath = {layout.Data}\r\n");
        }

        /// <summary>
        /// Creates the data folder. osu! asks what to do with an empty custom data folder on its first start, so a note is
        /// left in it.
        /// </summary>
        public static void PrepareData(PackageLayout layout)
        {
            Directory.CreateDirectory(layout.Data);

            string note = Path.Combine(layout.Data, DATA_NOTE);

            if (!File.Exists(note))
            {
                File.WriteAllText(note, "Data of the maniaortho build of osu! (osu.exe, or scripts\\ortho\\play.ps1 in the clone).\r\n"
                                        + "Imported maps and skins here are hard links to osu!stable's files: deleting this folder frees no space "
                                        + "and leaves stable's files as they are.\r\n");
            }
        }

        /// <summary>
        /// Moves screenshots from the data folder into osu!stable's Screenshots folder, never replacing a file there.
        /// Moved rather than linked: a junction to stable's folder would be followed by tools that delete recursively.
        /// </summary>
        /// <returns>How many were moved. Nothing moves when either folder is missing.</returns>
        public static int MoveScreenshots(PackageLayout layout)
        {
            string from = Path.Combine(layout.Data, "screenshots");
            string to = Path.Combine(layout.Stable, "Screenshots");

            if (!Directory.Exists(from) || !Directory.Exists(to))
                return 0;

            int moved = 0;

            foreach (string file in Directory.GetFiles(from))
            {
                string target = Path.Combine(to, Path.GetFileName(file));

                if (File.Exists(target))
                    continue;

                File.Move(file, target);
                moved++;
            }

            return moved;
        }

        /// <summary>
        /// How the game is started: from its folder, with temporary files in the package, no update checks and no crash
        /// reports.
        /// </summary>
        public static ProcessStartInfo CreateStartInfo(PackageLayout layout)
        {
            var info = new ProcessStartInfo(layout.GameExecutable)
            {
                WorkingDirectory = layout.Game,
                UseShellExecute = false,
            };

            info.Environment["TEMP"] = layout.Temp;
            info.Environment["TMP"] = layout.Temp;
            info.Environment["OSU_EXTERNAL_UPDATE_PROVIDER"] = UPDATE_PROVIDER;
            info.Environment["OSU_DISABLE_ERROR_REPORTING"] = "1";

            return info;
        }

        /// <summary>
        /// Whether this package's game is already running (osu!stable's own osu!.exe does not count).
        /// </summary>
        public static bool IsRunning(PackageLayout layout)
        {
            foreach (var process in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(PackageLayout.GAME_EXECUTABLE)))
            {
                using (process)
                {
                    try
                    {
                        if (string.Equals(process.MainModule?.FileName, layout.GameExecutable, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                    catch (Exception)
                    {
                        // Another user's or an elevated process: not ours.
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Prepares the folders, starts the game and waits for it to close, then moves its screenshots.
        /// </summary>
        /// <returns>The game's exit code, or 0 when it was already running.</returns>
        public static int Run(PackageLayout layout, Action<string> log)
        {
            if (!File.Exists(layout.GameExecutable))
                throw new FileNotFoundException($"No game in {layout.Game}. Package it with scripts\\ortho\\package.ps1 in the clone.", layout.GameExecutable);

            if (IsRunning(layout))
            {
                log("already running");
                return 0;
            }

            PrepareData(layout);
            PrepareGame(layout);
            Directory.CreateDirectory(layout.Temp);

            int leftOver = MoveScreenshots(layout);
            if (leftOver > 0)
                log($"moved {leftOver} screenshot(s) left from an earlier run to {layout.Stable}");

            log($"starting {layout.GameExecutable} (data: {layout.Data})");

            using (var game = Process.Start(CreateStartInfo(layout)) ?? throw new InvalidOperationException($"Could not start {layout.GameExecutable}."))
            {
                game.WaitForExit();

                int moved = MoveScreenshots(layout);
                log($"closed with code {game.ExitCode}; moved {moved} screenshot(s)");

                return game.ExitCode;
            }
        }
    }
}
