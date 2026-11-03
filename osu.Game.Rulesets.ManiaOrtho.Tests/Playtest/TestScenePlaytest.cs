// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.ManiaOrtho.Configuration;
using osu.Game.Rulesets.ManiaOrtho.Highway;
using osu.Game.Rulesets.ManiaOrtho.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;
using SixLabors.ImageSharp;

namespace osu.Game.Rulesets.ManiaOrtho.Tests.Playtest
{
    /// <summary>
    /// Plays a beatmap with autoplay in a real window, freezing at chosen times to save screenshots, and writes a
    /// summary.json with frame rate and (optionally) final judgement statistics.
    /// Configure with environment variables (<see cref="PlaytestOptions"/>), or run via scripts/ortho/playtest.sh.
    /// When no window exists (headless <c>dotnet test</c>) it still plays, which makes it a smoke test.
    /// </summary>
    public partial class TestScenePlaytest : PlayerTestScene
    {
        private static readonly JsonSerializerOptions json_options = new JsonSerializerOptions { WriteIndented = true };

        private readonly PlaytestOptions options = PlaytestOptions.FromEnvironment();

        private readonly List<string> captures = new List<string>();
        private readonly List<object> clockAtCapture = new List<object>();
        private readonly List<double> fpsSamples = new List<double>();
        private Task? captureTask;
        private double[] times = Array.Empty<double>();

        [Resolved]
        private GameHost host { get; set; } = null!;

        [Resolved]
        private SkinManager skins { get; set; } = null!;

        protected override bool Autoplay => true;

        // The player is loaded below, with the chosen mods.
        protected override bool HasCustomSteps => true;

        protected override Ruleset CreatePlayerRuleset() => options.CreateRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => PlaytestBeatmaps.Load(options.Beatmap);

        [SetUpSteps]
        public override void SetUpSteps()
        {
            AddStep("apply ruleset settings", () =>
            {
                skins.CurrentSkinInfo.Value = options.CreateSkinInfo().ToLiveUnmanaged();

                var config = (ManiaRulesetConfigManager)RulesetConfigs.GetConfigFor(CreatePlayerRuleset()).AsNonNull();

                if (options.ScrollSpeed is double speed)
                    config.SetValue(ManiaRulesetSetting.ScrollSpeed, speed);

                if (config is ManiaOrthoRulesetConfigManager ortho)
                {
                    // Set the preset twice so its values are re-applied even if it is already selected.
                    ortho.Highway.SetValue(HighwaySetting.Preset, HighwayPreset.Custom);
                    ortho.Highway.SetValue(HighwaySetting.Preset, options.Preset);

                    if (options.Wall is float wall)
                        ortho.Highway.SetValue(HighwaySetting.Wall, wall);

                    ortho.Highway.SetValue(HighwaySetting.WallKeepsVisibleTime, !options.SpeedLock);
                    ortho.Highway.SetValue(HighwaySetting.NoteStyle, options.NoteStyle);
                    options.ApplyHighwayValues(ortho.Highway);
                }
            });

            base.SetUpSteps();

            AddStep("load player", () => LoadPlayer(options.CreateMods(CreatePlayerRuleset())));
            AddUntilStep("player loaded", () => Player.IsLoaded && Player.Alpha == 1);
        }

        [Test]
        public void TestCapture()
        {
            AddStep("choose capture times", () =>
            {
                times = options.Times.Length > 0 ? options.Times : defaultTimes(Player.GameplayState.Beatmap);
            });

            AddUntilStep("gameplay running", () => Player.GameplayClockContainer.IsRunning);

            for (int i = 0; i < 8; i++)
            {
                int index = i;

                // Steps are created up-front, so allow up to 8 capture points and skip unused ones.
                AddStep($"advance to capture point {index}", () =>
                {
                    if (index >= times.Length)
                        return;

                    double target = times[index];
                    var clock = Player.GameplayClockContainer;

                    // Skip ahead when far away, leaving 1.5s of real play so judgements and hit effects are on screen.
                    if (clock.CurrentTime < target - 1500)
                        clock.Seek(target - 1500);
                });
                // Wait on the frame-stable clock, which is what the playfield draws from. After a long seek it catches up
                // a limited number of frames per update, so it can lag the gameplay clock by tens of seconds on slow renderers.
                AddUntilStep($"reach capture point {index}", () => index >= times.Length || Player.DrawableRuleset.FrameStableClock.CurrentTime >= times[index]);
                AddStep($"freeze for capture {index}", () =>
                {
                    if (index >= times.Length)
                        return;

                    // Stop, then seek back the few ms overshot so every ruleset is captured at exactly the same time.
                    Player.GameplayClockContainer.Stop();
                    Player.GameplayClockContainer.Seek(times[index]);
                });
                AddUntilStep($"frame-stable clock at capture point {index}",
                    () => index >= times.Length || Math.Abs(Player.DrawableRuleset.FrameStableClock.CurrentTime - times[index]) < 1);
                AddWaitStep("let the frame settle", 2);
                AddStep($"capture {index}", () =>
                {
                    if (index >= times.Length)
                    {
                        captureTask = Task.CompletedTask;
                        return;
                    }

                    double timeRange = (Player.DrawableRuleset as IDrawableScrollingRuleset)?.ScrollingInfo.TimeRange.Value ?? 0;

                    clockAtCapture.Add(new
                    {
                        target = times[index],
                        gameplay = Math.Round(Player.GameplayClockContainer.CurrentTime, 1),
                        frameStable = Math.Round(Player.DrawableRuleset.FrameStableClock.CurrentTime, 1),
                        combo = Player.ScoreProcessor.Combo.Value,
                        judged = Player.ScoreProcessor.JudgedHits,
                        // How long a note takes from the top of mania's stage to the line, and (maniaortho) from the far edge or
                        // wall to the line: the time range, shorter with the speed lock.
                        timeRange = Math.Round(timeRange, 1),
                        visibleTime = (Player.DrawableRuleset as DrawableManiaOrthoRuleset)?.Highway?.Warp.Scroll.VisibleTime(timeRange) is double visible
                            ? Math.Round(visible, 1)
                            : (double?)null,
                    });
                    captureTask = capture($"{options.Ruleset}-{index}-{times[index]:0}ms");
                });
                AddUntilStep($"capture {index} written", () => captureTask?.IsCompleted == true);
                AddStep($"resume after capture {index}", () =>
                {
                    if (captureTask?.IsFaulted == true)
                        throw captureTask.Exception!;

                    if (index < times.Length)
                        Player.GameplayClockContainer.Start();
                });
            }

            if (options.PlayToEnd)
                AddUntilStep("play to the end", () => Player.GameplayState.HasCompleted);

            AddStep("write summary.json", writeSummary);
        }

        protected override void Update()
        {
            base.Update();

            if (Player?.GameplayClockContainer?.IsRunning == true)
                fpsSamples.Add(host.DrawThread.Clock.FramesPerSecond);
        }

        private static double[] defaultTimes(IBeatmap beatmap)
        {
            double start = beatmap.HitObjects.First().StartTime;
            double end = beatmap.HitObjects.Max(h => h.GetEndTime());
            return new[] { 0.2, 0.45, 0.7, 0.9 }.Select(f => Math.Round(start + (end - start) * f)).ToArray();
        }

        private Task capture(string name)
        {
            // No window means no real renderer (headless test run): nothing to capture.
            if (options.OutputDirectory == null || host.Window == null)
                return Task.CompletedTask;

            Directory.CreateDirectory(options.OutputDirectory);
            string path = Path.Combine(options.OutputDirectory, name + ".png");

            return Task.Run(async () =>
            {
                using (var image = await host.TakeScreenshotAsync().ConfigureAwait(false))
                    await image.SaveAsPngAsync(path).ConfigureAwait(false);

                lock (captures)
                    captures.Add(path);
            });
        }

        private void writeSummary()
        {
            if (options.OutputDirectory == null)
                return;

            var beatmap = Player.GameplayState.Beatmap;
            var scoreProcessor = Player.ScoreProcessor;
            double[] sortedFps = fpsSamples.OrderBy(f => f).ToArray();

            var summary = new
            {
                ruleset = options.Ruleset,
                drawableRuleset = Player.DrawableRuleset.GetType().Name,
                beatmap = options.Beatmap,
                columns = (beatmap as ManiaBeatmap)?.TotalColumns,
                hitObjects = beatmap.HitObjects.Count,
                window = host.Window == null ? "headless" : $"{host.Window.ClientSize.Width}x{host.Window.ClientSize.Height}",
                renderer = host.ResolvedRenderer.ToString(),
                scrollSpeed = options.ScrollSpeed,
                preset = options.Ruleset == ManiaOrthoRuleset.SHORT_NAME ? options.Preset.ToString() : null,
                wall = options.Wall,
                speedLock = options.Ruleset == ManiaOrthoRuleset.SHORT_NAME ? options.SpeedLock : (bool?)null,
                // What the highway played with, after the preset, wall and --set values (out-of-range values are clamped).
                highway = (Player.DrawableRuleset as DrawableManiaOrthoRuleset)?.Highway?.Settings is HighwaySettings used
                    ? new
                    {
                        camera = used.CameraMode.ToString(),
                        used.FarWidthRatio,
                        used.JudgementLineHeight,
                        used.FarEdgeHeight,
                        used.TrackWidth,
                        used.Wall,
                        used.WallKeepsVisibleTime,
                        notes = used.NoteStyle.ToString(),
                    }
                    : null,
                skin = options.Skin,
                notes = options.Ruleset == ManiaOrthoRuleset.SHORT_NAME ? options.NoteStyle.ToString() : null,
                mods = Player.GameplayState.Mods.Select(m => m.Acronym).ToArray(),
                captureTimes = times,
                captures = captures.Select(Path.GetFileName).ToArray(),
                clockAtCapture,
                fps = sortedFps.Length == 0
                    ? null
                    : new
                    {
                        mean = Math.Round(sortedFps.Average(), 1),
                        p5 = Math.Round(sortedFps[(int)(sortedFps.Length * 0.05)], 1),
                        min = Math.Round(sortedFps[0], 1),
                        samples = sortedFps.Length,
                    },
                completed = Player.GameplayState.HasCompleted,
                statistics = scoreProcessor.Statistics.Where(kvp => kvp.Value > 0).ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value),
                accuracy = Math.Round(scoreProcessor.Accuracy.Value, 6),
                totalScore = scoreProcessor.TotalScore.Value,
                maxCombo = scoreProcessor.HighestCombo.Value,
            };

            Directory.CreateDirectory(options.OutputDirectory);
            File.WriteAllText(Path.Combine(options.OutputDirectory, $"summary-{options.Ruleset}.json"),
                JsonSerializer.Serialize(summary, json_options));
        }
    }
}
