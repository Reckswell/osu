// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using NUnit.Framework;
using osu.Framework.Platform;
using osu.Game.IO;

namespace osu.Game.Rulesets.ManiaOrtho.Tests.Playtest
{
    /// <summary>
    /// What <c>scripts/ortho/play.ps1</c> relies on to keep the full game's data in a folder of its choosing: osu!'s own
    /// storage.ini ("custom data location") in the game's default storage moves the data there, and the first start only
    /// passes without a question when that folder is not empty, which is why the script leaves a note in it.
    /// </summary>
    /// <remarks>
    /// The other half, framework.ini next to the executable making the build folder the default storage, is
    /// osu-framework's (<c>DesktopGameHost.GetDefaultGameStorage</c>) and is checked on Alpha's PC: the script fails if
    /// <c>%APPDATA%\osu-development</c> changes during a run.
    /// </remarks>
    [TestFixture]
    public class PlayScriptDataFolderTest
    {
        private string root = null!;
        private string build = null!;
        private string data = null!;
        private HeadlessGameHost host = null!;

        [SetUp]
        public void SetUp()
        {
            root = Path.Combine(Path.GetTempPath(), $"ortho-play-script-{Guid.NewGuid():N}");
            build = Directory.CreateDirectory(Path.Combine(root, "build")).FullName;
            // A space and a "!" as in Documents\Projects\osu!\ortho-data.
            data = Path.Combine(root, "osu! data");
            host = new HeadlessGameHost(nameof(PlayScriptDataFolderTest));
        }

        [TearDown]
        public void TearDown()
        {
            host.Dispose();

            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }

        [Test]
        public void TestStorageIniMovesTheData()
        {
            Directory.CreateDirectory(data);
            File.WriteAllText(Path.Combine(data, "ortho-data.txt"), "note");
            writeStorageIni();

            var storage = new OsuStorage(host, host.GetStorage(build));
            storage.GetStorageForDirectory("screenshots");

            Assert.That(storage.Error, Is.EqualTo(OsuStorageError.None));
            Assert.That(storage.GetFullPath("."), Is.SamePath(data));
            Assert.That(Directory.Exists(Path.Combine(data, "screenshots")), Is.True);
            Assert.That(Directory.GetFileSystemEntries(build), Is.EquivalentTo(new[] { Path.Combine(build, "storage.ini") }));
        }

        [Test]
        public void TestAnEmptyDataFolderAsksOnTheFirstStart()
        {
            Directory.CreateDirectory(data);
            writeStorageIni();

            var storage = new OsuStorage(host, host.GetStorage(build));

            Assert.That(storage.Error, Is.EqualTo(OsuStorageError.AccessibleButEmpty));
        }

        /// <summary>
        /// The line play.ps1 writes.
        /// </summary>
        private void writeStorageIni() => File.WriteAllText(Path.Combine(build, "storage.ini"), $"FullPath = {data}\r\n");
    }
}
