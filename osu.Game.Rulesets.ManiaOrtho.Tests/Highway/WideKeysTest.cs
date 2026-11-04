// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Rulesets.ManiaOrtho.Highway;
using osu.Game.Rulesets.ManiaOrtho.UI;

namespace osu.Game.Rulesets.ManiaOrtho.Tests.Highway
{
    /// <summary>
    /// L0 checks for the wide-key markings (research/topic-wide-keys.md W1 and W3): hand groups and the note gap's floor.
    /// </summary>
    [TestFixture]
    public class WideKeysTest
    {
        [TestCase(1, new[] { 1 })]
        [TestCase(4, new[] { 4 })]
        [TestCase(5, new[] { 5 })]
        [TestCase(6, new[] { 3, 3 })]
        [TestCase(7, new[] { 3, 1, 3 })]
        [TestCase(8, new[] { 4, 4 })]
        [TestCase(9, new[] { 4, 1, 4 })]
        [TestCase(10, new[] { 5, 5 })]
        public void TestHandGroups(int keys, int[] groups) => Assert.That(groupsOf(keys), Is.EqualTo(groups));

        [Test]
        public void TestGapIsAShareOfTheLaneWhereThatIsAPixel()
        {
            // A 4K lane under Chunithm at the line: 0.25 of a 1200 px track, so 1% is 3 px.
            Assert.That(HighwayNoteLayer.NoteGap(0.25f, 1200), Is.EqualTo(0.25f * HighwayNoteLayer.NOTE_GAP).Within(1e-7f));
        }

        [Test]
        public void TestGapIsNeverUnderAPixel()
        {
            // A 10K lane at Voltex's far edge: 0.1 of a 90 px track, 9 px, so 1% would be 0.09 px.
            Assert.That(HighwayNoteLayer.NoteGap(0.1f, 90) * 90, Is.EqualTo(HighwayNoteLayer.MIN_GAP_PIXELS).Within(1e-5f));
        }

        [Test]
        public void TestGapLeavesANoteOnATinyLane()
        {
            // A 2 px lane: a pixel each side would leave nothing, so each side takes a quarter.
            Assert.That(HighwayNoteLayer.NoteGap(0.1f, 20), Is.EqualTo(0.025f).Within(1e-7f));
            Assert.That(HighwayNoteLayer.NoteGap(0.1f, 0), Is.EqualTo(0.1f * HighwayNoteLayer.NOTE_GAP).Within(1e-7f), "no screen scale known");
        }

        private static int[] groupsOf(int keys)
        {
            var groups = new List<int>();
            int size = 1;

            for (int column = 1; column < keys; column++)
            {
                if (LaneGroups.StartsGroup(keys, column))
                {
                    groups.Add(size);
                    size = 0;
                }

                size++;
            }

            groups.Add(size);
            return groups.ToArray();
        }
    }
}
