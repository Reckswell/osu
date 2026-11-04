// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Mania;

namespace osu.Game.Rulesets.ManiaOrtho.Tests.Playtest
{
    /// <summary>
    /// The harness's scene always adds No Fail (the humanised replay misses a few notes); a second one fails the player's load.
    /// </summary>
    [TestFixture]
    public class PlaytestOptionsTest
    {
        [TestCase(null)]
        [TestCase(4)]
        public void TestNoFailIsLeftToTheScene(int? humanisedSeed)
        {
            var mods = new PlaytestOptions { HumanisedSeed = humanisedSeed, Mods = new[] { "HD", "nf" } }.CreateMods(new ManiaRuleset());

            Assert.That(mods.Select(m => m.Acronym), Is.EquivalentTo(new[] { "HD" }));
        }

        [Test]
        public void TestModsAsGiven()
        {
            var mods = new PlaytestOptions { Mods = new[] { "HD", "FI" } }.CreateMods(new ManiaRuleset());

            Assert.That(mods.Select(m => m.Acronym), Is.EquivalentTo(new[] { "HD", "FI" }));
        }
    }
}
