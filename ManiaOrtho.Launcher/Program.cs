// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace ManiaOrtho.Launcher
{
    /// <summary>
    /// <c>osu.exe</c>: double-click to play the packaged game with maniaortho (<see cref="GameLauncher"/>).
    /// </summary>
    public static partial class Program
    {
        /// <summary>
        /// The log is started again past this size.
        /// </summary>
        private const long max_log_bytes = 1024 * 1024;

        [STAThread]
        public static int Main()
        {
            var layout = PackageLayout.ForPackage(AppContext.BaseDirectory);

            try
            {
                return GameLauncher.Run(layout, message => log(layout, message));
            }
            catch (Exception e)
            {
                log(layout, e.ToString());
                showError($"{e.Message}\n\nDetails: {layout.Log}");
                return 1;
            }
        }

        private static void log(PackageLayout layout, string message)
        {
            try
            {
                if (File.Exists(layout.Log) && new FileInfo(layout.Log).Length > max_log_bytes)
                    File.Delete(layout.Log);

                File.AppendAllText(layout.Log, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)} {message}\r\n");
            }
            catch (IOException)
            {
                // The log is a convenience; never fail a start over it.
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        private static void showError(string message)
        {
            // A windowed app has no console to print to.
            if (OperatingSystem.IsWindows())
                _ = messageBox(IntPtr.Zero, message, "osu! (maniaortho)", 0x10 /* MB_ICONERROR */);
            else
                Console.Error.WriteLine(message);
        }

        [LibraryImport("user32.dll", EntryPoint = "MessageBoxW", StringMarshalling = StringMarshalling.Utf16)]
        private static partial int messageBox(IntPtr window, string text, string caption, uint type);
    }
}
