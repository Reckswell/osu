// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using ManiaOrtho.Launcher;
using NUnit.Framework;
using osu.Framework.Platform;
using osu.Game.IO;

namespace osu.Game.Rulesets.ManiaOrtho.Tests.Playtest
{
    /// <summary>
    /// The osu.exe launcher (<c>ManiaOrtho.Launcher</c>), without starting a game: where it keeps things, what it writes,
    /// and that osu! then uses the data folder.
    /// </summary>
    [TestFixture]
    public class GameLauncherTest
    {
        private string root = null!;
        private PackageLayout layout = null!;

        [SetUp]
        public void SetUp()
        {
            // A space and a "!" as in Documents\Projects\osu!.
            root = Path.Combine(Path.GetTempPath(), $"ortho-launcher-{Guid.NewGuid():N}", "osu! projects");
            layout = new PackageLayout(Path.Combine(root, "osu"), Path.Combine(root, PackageLayout.DATA_FOLDER), Path.Combine(root, "stable"));
            Directory.CreateDirectory(layout.Game);
        }

        [TearDown]
        public void TearDown()
        {
            TemporaryFolder.Delete(Path.GetDirectoryName(root)!);
        }

        [Test]
        public void TestDataSitsNextToThePackage()
        {
            var found = PackageLayout.ForPackage(Path.Combine(root, "osu") + Path.DirectorySeparatorChar);

            Assert.That(found.Package, Is.EqualTo(Path.Combine(root, "osu")));
            Assert.That(found.Data, Is.EqualTo(Path.Combine(root, "ortho-data")));
            Assert.That(found.GameExecutable, Is.EqualTo(Path.Combine(root, "osu", "game", "osu!.exe")));
            Assert.That(found.Temp, Is.EqualTo(Path.Combine(root, "osu", "temp")));
            Assert.That(found.Stable, Does.EndWith("osu!"));
        }

        [Test]
        public void TestPreparedFoldersMoveOsuToTheData()
        {
            GameLauncher.PrepareData(layout);
            GameLauncher.PrepareGame(layout);

            Assert.That(File.ReadAllText(Path.Combine(layout.Game, "framework.ini")), Is.Empty);
            Assert.That(File.ReadAllText(Path.Combine(layout.Game, "storage.ini")), Is.EqualTo($"FullPath = {layout.Data}\r\n"));
            Assert.That(File.Exists(Path.Combine(layout.Data, GameLauncher.DATA_NOTE)), Is.True);

            using (var host = new HeadlessGameHost(nameof(GameLauncherTest)))
            {
                var storage = new OsuStorage(host, host.GetStorage(layout.Game));

                Assert.That(storage.Error, Is.EqualTo(OsuStorageError.None));
                Assert.That(storage.GetFullPath("."), Is.SamePath(layout.Data));
            }
        }

        [Test]
        public void TestPrepareKeepsExistingFiles()
        {
            File.WriteAllText(Path.Combine(layout.Game, "framework.ini"), "WindowMode = Fullscreen\r\n");
            Directory.CreateDirectory(layout.Data);
            File.WriteAllText(Path.Combine(layout.Data, GameLauncher.DATA_NOTE), "mine");

            GameLauncher.PrepareData(layout);
            GameLauncher.PrepareGame(layout);

            Assert.That(File.ReadAllText(Path.Combine(layout.Game, "framework.ini")), Is.EqualTo("WindowMode = Fullscreen\r\n"));
            Assert.That(File.ReadAllText(Path.Combine(layout.Data, GameLauncher.DATA_NOTE)), Is.EqualTo("mine"));
        }

        [Test]
        public void TestScreenshotsMoveToStableWithoutReplacingAny()
        {
            string from = Directory.CreateDirectory(Path.Combine(layout.Data, "screenshots")).FullName;
            string to = Directory.CreateDirectory(Path.Combine(layout.Stable, "Screenshots")).FullName;

            File.WriteAllText(Path.Combine(from, "osu_2026-11-04_05-00-00.png"), "new");
            File.WriteAllText(Path.Combine(from, "same-name.png"), "ours");
            File.WriteAllText(Path.Combine(to, "same-name.png"), "stable's");

            Assert.That(GameLauncher.MoveScreenshots(layout), Is.EqualTo(1));

            Assert.That(File.ReadAllText(Path.Combine(to, "osu_2026-11-04_05-00-00.png")), Is.EqualTo("new"));
            Assert.That(File.Exists(Path.Combine(from, "osu_2026-11-04_05-00-00.png")), Is.False);
            Assert.That(File.ReadAllText(Path.Combine(to, "same-name.png")), Is.EqualTo("stable's"));
            Assert.That(File.ReadAllText(Path.Combine(from, "same-name.png")), Is.EqualTo("ours"));
        }

        [Test]
        public void TestScreenshotsStayWithoutStable()
        {
            string from = Directory.CreateDirectory(Path.Combine(layout.Data, "screenshots")).FullName;
            File.WriteAllText(Path.Combine(from, "shot.png"), "new");

            Assert.That(GameLauncher.MoveScreenshots(layout), Is.Zero);
            Assert.That(File.Exists(Path.Combine(from, "shot.png")), Is.True);
        }

        [Test]
        public void TestGameStartsFromItsFolderWithoutUpdatesOrReports()
        {
            var info = GameLauncher.CreateStartInfo(layout);

            Assert.That(info.FileName, Is.EqualTo(layout.GameExecutable));
            Assert.That(info.WorkingDirectory, Is.EqualTo(layout.Game));
            Assert.That(info.Environment["TEMP"], Is.EqualTo(layout.Temp));
            Assert.That(info.Environment["TMP"], Is.EqualTo(layout.Temp));
            Assert.That(info.Environment["OSU_EXTERNAL_UPDATE_PROVIDER"], Is.Not.Empty);
            Assert.That(info.Environment["OSU_DISABLE_ERROR_REPORTING"], Is.EqualTo("1"));
        }

        [Test]
        public void TestMissingGameNamesThePackageScript()
        {
            var error = Assert.Throws<FileNotFoundException>(() => GameLauncher.Run(layout, _ => { }));

            Assert.That(error!.Message, Does.Contain("package.ps1"));
        }
    }
}
