// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ManiaOrtho.ScoreServerContract;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OrthoServer.Live;
using OrthoServer.Scoring;
using OrthoServer.Storage;
using OrthoServer.Web;

namespace OrthoServer
{
    public class ServerOptions
    {
        /// <summary>
        /// Where plays, maps and replays are kept.
        /// </summary>
        public string DataPath { get; set; } = "server-data";

        /// <summary>
        /// The port on 127.0.0.1 (this computer only). 0 picks a free one.
        /// </summary>
        public int Port { get; set; } = DEFAULT_PORT;

        /// <summary>
        /// Whether to check which calculator version osu! runs live (on GitHub, at start and twice a day).
        /// </summary>
        public bool CheckLiveVersion { get; set; } = true;

        public const int DEFAULT_PORT = 7727;
    }

    /// <summary>
    /// The web server: the profile page and the JSON endpoints the game uploads plays to. It listens on 127.0.0.1 only.
    /// </summary>
    public static class ServerHost
    {
        public static WebApplication Create(ServerOptions options)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { ContentRootPath = AppContext.BaseDirectory });

            builder.WebHost.UseUrls($"http://127.0.0.1:{options.Port}");
            builder.Logging.SetMinimumLevel(LogLevel.Warning);

            var service = new PlayService(new PlayStore(options.DataPath));
            var live = new LiveCheck(options.CheckLiveVersion);

            builder.Services.AddSingleton(service);
            builder.Services.AddSingleton(live);

            var app = builder.Build();

            app.Lifetime.ApplicationStarted.Register(() => live.Start(app.Lifetime.ApplicationStopping));

            // Only the game posts: a page in a browser cannot send this header without a preflight, which nothing here allows.
            app.Use(async (context, next) =>
            {
                if (HttpMethods.IsPost(context.Request.Method) && !context.Request.Headers.ContainsKey(ScoreServerProtocol.CLIENT_HEADER))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new { error = $"Requests that change anything need the {ScoreServerProtocol.CLIENT_HEADER} header." })
                                 .ConfigureAwait(false);
                    return;
                }

                await next().ConfigureAwait(false);
            });

            app.MapGet("/", () =>
            {
                var recent = service.Store.All().OrderByDescending(p => p.Id).Take(50).ToList();
                return Results.Content(ProfilePage.Render(service.Profile(), recent, PlayCalculator.CalculatorVersion, live.NewerVersion), "text/html; charset=utf-8");
            });

            app.MapGet("/api/status", () => json(new Dictionary<string, object?>
            {
                ["calculator_version"] = PlayCalculator.CalculatorVersion,
                ["live_calculator_version"] = live.Latest?.CalculatorVersion,
                ["live_release_tag"] = live.Latest?.ReleaseTag,
                ["live_checked_at"] = live.CheckedAt,
                ["plays"] = service.Store.All().Count,
            }));

            app.MapGet("/api/profile", () =>
            {
                var profile = service.Profile();

                return json(new Dictionary<string, object?>
                {
                    ["total_pp"] = profile.TotalPp,
                    ["bonus_pp"] = profile.BonusPp,
                    ["accuracy"] = profile.Accuracy,
                    ["counted_scores"] = profile.CountedScores,
                    ["calculator_version"] = PlayCalculator.CalculatorVersion,
                    ["top"] = profile.Top.Select(t => new Dictionary<string, object?>
                    {
                        ["play_id"] = t.Play.Id,
                        ["beatmap_md5"] = t.Play.BeatmapMd5,
                        ["beatmap_id"] = t.Play.BeatmapId,
                        ["artist"] = t.Play.Artist,
                        ["title"] = t.Play.Title,
                        ["version"] = t.Play.Version,
                        ["mods"] = t.Play.Mods,
                        ["accuracy"] = t.Play.Accuracy,
                        ["pp"] = t.Play.Pp,
                        ["weight"] = t.Weight,
                        ["played_at"] = t.Play.PlayedAt,
                    }),
                });
            });

            app.MapGet("/api/plays", (int? limit) => json(service.Store.All().OrderByDescending(p => p.Id).Take(Math.Clamp(limit ?? 50, 1, 1000))));

            app.MapPost("/api/plays", async (HttpRequest request) =>
            {
                if (!request.HasFormContentType)
                    return Results.BadRequest(new { error = "Send the play as multipart/form-data: play (JSON), beatmap (.osu) and optionally replay (.osr)." });

                var form = await request.ReadFormAsync().ConfigureAwait(false);

                string? playJson = form["play"];
                var beatmapFile = form.Files["beatmap"];

                if (string.IsNullOrEmpty(playJson) || beatmapFile == null)
                    return Results.BadRequest(new { error = "An upload needs a \"play\" field and a \"beatmap\" file." });

                try
                {
                    var upload = JsonConvert.DeserializeObject<PlayUpload>(playJson) ?? throw new InvalidDataException("The play is empty.");
                    byte[] osuFile = await readAll(beatmapFile).ConfigureAwait(false);
                    byte[]? replay = form.Files["replay"] is IFormFile replayFile ? await readAll(replayFile).ConfigureAwait(false) : null;

                    var result = service.Submit(upload, osuFile, replay);
                    result.LiveCalculatorVersion = live.NewerVersion;
                    return json(result);
                }
                catch (Exception e) when (e is InvalidDataException or JsonException or FormatException)
                {
                    return Results.BadRequest(new { error = e.Message });
                }
            });

            app.MapPost("/api/recalculate", () =>
            {
                var (rated, failed) = service.Recalculate(onlyOutdated: false);
                return json(new Dictionary<string, object?> { ["rated"] = rated, ["failed"] = failed, ["calculator_version"] = PlayCalculator.CalculatorVersion });
            });

            return app;
        }

        private static readonly JsonSerializerSettings json_settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() },
        };

        private static IResult json(object value) => Results.Content(JsonConvert.SerializeObject(value, json_settings), "application/json; charset=utf-8");

        private static async Task<byte[]> readAll(IFormFile file)
        {
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream).ConfigureAwait(false);
                return stream.ToArray();
            }
        }
    }

    /// <summary>
    /// Keeps the latest answer of <see cref="LiveCalculatorVersion"/>. A failed check (no network) keeps the previous answer.
    /// </summary>
    public class LiveCheck
    {
        private readonly bool enabled;

        public LiveCalculatorVersion.Result? Latest { get; private set; }

        public DateTimeOffset? CheckedAt { get; private set; }

        /// <summary>
        /// The live calculator version when it differs from this server's, else null.
        /// </summary>
        public string? NewerVersion => Latest != null && LiveCalculatorVersion.Differs(PlayCalculator.CalculatorVersion, Latest.CalculatorVersion) ? Latest.CalculatorVersion : null;

        public LiveCheck(bool enabled)
        {
            this.enabled = enabled;
        }

        public void Start(CancellationToken stopping)
        {
            if (!enabled)
                return;

            _ = Task.Run(async () =>
            {
                using (var http = CreateHttpClient())
                {
                    while (!stopping.IsCancellationRequested)
                    {
                        try
                        {
                            Latest = await LiveCalculatorVersion.FetchAsync(http, stopping).ConfigureAwait(false);
                            CheckedAt = DateTimeOffset.Now;

                            if (NewerVersion != null)
                                Console.WriteLine($"osu! now rates scores with {NewerVersion} (score processor {Latest.ReleaseTag}); this server uses {PlayCalculator.CalculatorVersion}. Run scripts/ortho/server-update.");
                        }
                        catch (Exception e) when (e is not OperationCanceledException)
                        {
                            Console.WriteLine($"Could not check osu!'s live calculator version: {e.Message}");
                        }

                        try
                        {
                            await Task.Delay(TimeSpan.FromHours(12), stopping).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            return;
                        }
                    }
                }
            }, stopping);
        }

        public static HttpClient CreateHttpClient()
        {
            var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            http.DefaultRequestHeaders.UserAgent.ParseAdd("maniaortho-score-server");
            return http;
        }
    }
}
