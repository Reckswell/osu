// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using NUnit.Framework;

namespace osu.Game.Rulesets.ManiaOrtho.Tests.Playtest
{
    /// <summary>
    /// The playtest harness runs portable from the test build output and must not leave it portable: with framework.ini
    /// there, NUnit's headless test hosts take that folder as their storage and delete it when they clean up
    /// (<see cref="PlaytestRunner.RemovePortableMarker"/>).
    /// </summary>
    [TestFixture]
    public class PortableMarkerTest
    {
        private string folder = null!;

        [SetUp]
        public void SetUp() => folder = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"ortho-portable-{Guid.NewGuid():N}")).FullName;

        [TearDown]
        public void TearDown() => Directory.Delete(folder, true);

        [Test]
        public void TestMarkerIsRemovedAndTheBuildStays()
        {
            File.WriteAllText(Path.Combine(folder, PlaytestRunner.PORTABLE_MARKER), "WindowMode = Windowed\r\n");
            File.WriteAllText(Path.Combine(folder, "client.realm"), string.Empty);

            PlaytestRunner.RemovePortableMarker(folder);

            Assert.That(File.Exists(Path.Combine(folder, PlaytestRunner.PORTABLE_MARKER)), Is.False);
            Assert.That(File.Exists(Path.Combine(folder, "client.realm")), Is.True, "only the marker goes");
        }

        [Test]
        public void TestNoMarkerIsFine() => Assert.DoesNotThrow(() => PlaytestRunner.RemovePortableMarker(folder));
    }
}
