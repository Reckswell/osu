// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.ManiaOrtho.UI;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.ManiaOrtho.Tests.Highway
{
    /// <summary>
    /// Which burst each mania result gets (research game-chunithm-feedback P1 and P2), and the hold glow's pulse rate (P6).
    /// </summary>
    [TestFixture]
    public class HitBurstStyleTest
    {
        [Test]
        public void TestEveryHitHasABurstAndAMissHasNone()
        {
            var hitWindows = new ManiaHitWindows();
            var results = System.Enum.GetValues<HitResult>().Where(r => r.IsBasic() && hitWindows.IsHitResultAllowed(r)).ToArray();

            Assert.That(results, Does.Contain(HitResult.Perfect).And.Contain(HitResult.Meh));

            foreach (var result in results)
            {
                if (result == HitResult.Miss)
                    Assert.That(HitBurstStyle.For(result), Is.Null, "a miss must not light the line");
                else
                    Assert.That(HitBurstStyle.For(result), Is.Not.Null, $"{result} has no burst");
            }

            Assert.That(HitBurstStyle.For(HitResult.IgnoreHit), Is.Null);
        }

        [Test]
        public void TestPerfectAndGreatAreEasyToTellApart()
        {
            var perfect = HitBurstStyle.For(HitResult.Perfect)!.Value;
            var great = HitBurstStyle.For(HitResult.Great)!.Value;

            Assert.That(perfect.Colour, Is.EqualTo(HitBurstStyle.PERFECT_COLOUR));
            Assert.That(great.Colour, Is.EqualTo(HitBurstStyle.GREAT_COLOUR));
            Assert.That(perfect.Sparkles, Is.GreaterThan(great.Sparkles));
            Assert.That(perfect.Sparkles, Is.LessThanOrEqualTo(HighwayHitBurst.MAX_SPARKLES));
        }

        [Test]
        public void TestLowerResultsAreSmallerAndDimmer()
        {
            var great = HitBurstStyle.For(HitResult.Great)!.Value;
            var good = HitBurstStyle.For(HitResult.Good)!.Value;
            var meh = HitBurstStyle.For(HitResult.Meh)!.Value;

            Assert.That(HitBurstStyle.For(HitResult.Ok), Is.EqualTo(good));
            Assert.That(good.RingEnd, Is.LessThan(great.RingEnd));
            Assert.That(good.Sparkles, Is.Zero);
            Assert.That(good.Core, Is.False);
            Assert.That(meh.Alpha, Is.LessThan(good.Alpha));
        }

        [TestCase(500, 250)] // 120 BPM: 1/8 notes.
        [TestCase(600, 150)] // 100 BPM: 1/16 notes.
        [TestCase(352.941, 176.4705)] // 170 BPM: 1/8 notes.
        [TestCase(240, 240)] // 250 BPM: 1/4 notes.
        [TestCase(0, 125)] // No usable timing.
        [TestCase(6, 192)] // 10000 BPM gimmick: 6 ms doubled into range, not a flicker.
        [TestCase(60000, 234.375)] // 1 BPM: 15 s halved into range, not a stopped pulse.
        public void TestHoldTicksAtChunithmsCadence(double beatLength, double interval)
        {
            Assert.That(HighwayHitEffectLayer.HoldTickInterval(beatLength), Is.EqualTo(interval).Within(1e-3));
        }

        [Test]
        public void TestHoldTickPhaseFollowsTheBeatGrid()
        {
            // 170 BPM from 1000 ms: a tick every half beat (176.47 ms).
            var timing = new TimingControlPoint { Time = 1000, BeatLength = 60000 / 170.0 };
            double interval = HighwayHitEffectLayer.HoldTickInterval(timing.BeatLength);

            Assert.That(HighwayHitEffectLayer.HoldTickPhase(1000, timing), Is.EqualTo(0).Within(1e-4));
            Assert.That(HighwayHitEffectLayer.HoldTickPhase(1000 + interval * 0.25, timing), Is.EqualTo(0.25f).Within(1e-4));
            Assert.That(HighwayHitEffectLayer.HoldTickPhase(1000 + interval * 7.5, timing), Is.EqualTo(0.5f).Within(1e-4));

            // Before the timing point (a hold under the first one): still on the grid, in [0, 1).
            Assert.That(HighwayHitEffectLayer.HoldTickPhase(1000 - interval * 0.25, timing), Is.EqualTo(0.75f).Within(1e-4));
        }
    }
}
