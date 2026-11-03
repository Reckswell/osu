// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Online.API;

namespace osu.Game.Rulesets.ManiaOrtho.Tests.Playtest
{
    /// <summary>
    /// Runs one visual test scene, unattended, in a real window, then exits.
    /// This is how the agent "plays" the game: on a desktop it uses the GPU, in a cloud container it runs under Xvfb
    /// with Mesa's software renderer. Screenshots come from steps inside the scene (see <see cref="TestScenePlaytest"/>).
    /// </summary>
    /// <example>
    /// dotnet run --project osu.Game.Rulesets.ManiaOrtho.Tests -- playtest --beatmap synthetic-7k --ruleset maniaortho --out playtest-output
    /// </example>
    public static partial class PlaytestRunner
    {
        public static int Run(string[] args)
        {
            var parsed = parse(args);

            string sceneName = parsed.GetValueOrDefault("scene", nameof(TestScenePlaytest));
            var sceneType = typeof(PlaytestRunner).Assembly.GetTypes()
                                                  .SingleOrDefault(t => typeof(TestScene).IsAssignableFrom(t) && (t.Name == sceneName || t.FullName == sceneName))
                            ?? throw new ArgumentException($"No test scene named {sceneName} in {typeof(PlaytestRunner).Assembly.GetName().Name}.");

            // Catching up after a long seek can take longer than the default 10s step timeout on software renderers.
            // Must be set before osu-framework reads its environment.
            Environment.SetEnvironmentVariable("OSU_TESTS_NO_TIMEOUT", "1");

            // Options reach the scene through the environment so it can also be run from the test browser.
            setEnv(PlaytestOptions.ENV_BEATMAP, parsed.GetValueOrDefault("beatmap"));
            setEnv(PlaytestOptions.ENV_RULESET, parsed.GetValueOrDefault("ruleset"));
            setEnv(PlaytestOptions.ENV_TIMES, parsed.GetValueOrDefault("times"));
            setEnv(PlaytestOptions.ENV_OUT, parsed.GetValueOrDefault("out"));
            setEnv(PlaytestOptions.ENV_SCROLL, parsed.GetValueOrDefault("scroll-speed"));
            setEnv(PlaytestOptions.ENV_FULL, parsed.ContainsKey("play-to-end") ? "1" : null);

            int[] size = parsed.GetValueOrDefault("size", "1280x720").Split('x').Select(int.Parse).ToArray();

            using (var host = Host.GetSuitableDesktopHost(@"ortho-playtest", new HostOptions { FriendlyGameName = "ortho playtest" }))
            {
                var game = new PlaytestGame(sceneType, parsed.GetValueOrDefault("method"), new Size(size[0], size[1]));
                host.Run(game);

                Console.WriteLine(game.Error == null ? "PLAYTEST PASSED" : $"PLAYTEST FAILED: {game.Error}");
                return game.Error == null ? 0 : 1;
            }
        }

        private static void setEnv(string name, string? value)
        {
            if (value != null)
                Environment.SetEnvironmentVariable(name, value);
        }

        private static Dictionary<string, string> parse(string[] args)
        {
            var result = new Dictionary<string, string>();

            for (int i = 0; i < args.Length; i++)
            {
                if (!args[i].StartsWith("--", StringComparison.Ordinal))
                    throw new ArgumentException($"Unexpected argument {args[i]}");

                string key = args[i][2..];
                bool hasValue = i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal);
                result[key] = hasValue ? args[++i] : "true";
            }

            return result;
        }

        private partial class PlaytestGame : OsuGameBase
        {
            private readonly Type sceneType;
            private readonly string? methodName;
            private readonly Size windowSize;

            public string? Error { get; private set; } = "did not complete";

            public PlaytestGame(Type sceneType, string? methodName, Size windowSize)
            {
                this.sceneType = sceneType;
                this.methodName = methodName;
                this.windowSize = windowSize;

                // Never talk to osu! servers from the harness.
                API = new DummyAPIAccess();
            }

            [BackgroundDependencyLoader]
            private void load(FrameworkConfigManager frameworkConfig)
            {
                frameworkConfig.SetValue(FrameworkSetting.WindowMode, WindowMode.Windowed);
                frameworkConfig.SetValue(FrameworkSetting.WindowedSize, windowSize);
            }

            public override void SetHost(GameHost host)
            {
                base.SetHost(host);
                host.Window.CursorState |= CursorState.Hidden;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                var scene = (TestScene)Activator.CreateInstance(sceneType)!;

                // Hide the test scene's step list by pushing it off the left edge of the window.
                var wrapper = new SceneWrapper(scene);
                Add(wrapper);

                LoadComponentAsync(scene, _ =>
                {
                    wrapper.Add(scene);

                    foreach (var setUp in methodsWith<SetUpAttribute>())
                        scene.AddStep("[SetUp]", () => setUp.Invoke(scene, null));

                    foreach (var setUpSteps in methodsWith<SetUpStepsAttribute>())
                        setUpSteps.Invoke(scene, null);

                    var tests = methodsWith<TestAttribute>().Where(m => methodName == null || m.Name == methodName).ToArray();
                    if (tests.Length == 0)
                        throw new ArgumentException($"No [Test] method {methodName} on {sceneType.Name}.");

                    tests[0].Invoke(scene, null);

                    scene.RunAllSteps(() =>
                    {
                        Error = null;
                        Exit();
                    }, (step, e) =>
                    {
                        Error = $"step \"{step}\" failed: {e}";
                        Exit();
                    });
                });
            }

            private IEnumerable<MethodInfo> methodsWith<T>() where T : Attribute
                => sceneType.GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(m => m.GetCustomAttribute<T>(true) != null);

            private partial class SceneWrapper : Container
            {
                // Matches TestScene's internal steps panel width.
                private const float steps_width = 180;

                private readonly TestScene scene;

                public SceneWrapper(TestScene scene)
                {
                    this.scene = scene;
                    RelativeSizeAxes = Axes.Both;
                }

                protected override void Update()
                {
                    base.Update();

                    scene.RelativeSizeAxes = Axes.Y;
                    scene.Width = DrawWidth + steps_width;
                    scene.X = -steps_width;
                }
            }
        }
    }
}
