// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using osu.Framework.Platform;

namespace osu.Game.Rulesets.ManiaOrtho.Scoring
{
    /// <summary>
    /// <c>maniaortho/plays.csv</c> in the game's data folder: one row per pass or fail, for spreadsheets and for rating
    /// plays again when osu!mania's calculators change (the raw judgement counts and each mod's settings are kept). Each
    /// play's replay is saved next to it in <see cref="REPLAY_DIRECTORY"/>, because osu! keeps no replay for a ruleset
    /// that is not one of its four own.
    /// </summary>
    public static class PlayLog
    {
        public const string DIRECTORY = "maniaortho";

        public const string FILENAME = "plays.csv";

        public const string REPLAY_DIRECTORY = "replays";

        public static readonly string[] COLUMNS =
        {
            "time", "result", "rank", "artist", "title", "difficulty", "beatmap_md5", "keys", "mods",
            "score", "accuracy", "max_combo", "perfect", "great", "good", "ok", "meh", "miss",
            "stars", "pp", "difficulty_version", "preset", "camera", "wall", "lift", "wall_keeps_visible_time",
            "scroll_speed", "mods_json", "replay", "score_id",
        };

        private static readonly object write_lock = new object();

        private static readonly char[] needs_quotes = { ',', '"', '\r', '\n' };

        /// <summary>
        /// Appends <paramref name="values"/> (in <see cref="COLUMNS"/> order) to the log in <paramref name="storage"/>. A new file
        /// starts with a byte-order mark (so spreadsheets read titles as UTF-8) and the header. A file whose header differs (an
        /// older set of columns) is kept under a dated name and a new one is started.
        /// </summary>
        public static void Append(Storage storage, IReadOnlyList<string?> values)
        {
            lock (write_lock)
            {
                string path = storage.GetFullPath(FILENAME, true);
                string header = FormatRow(COLUMNS);

                if (File.Exists(path) && readFirstLine(path) is string existing && existing.Length > 0 && existing != header)
                    File.Move(path, storage.GetFullPath($"plays until {DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss", CultureInfo.InvariantCulture)}.csv"));

                // A plain append: the storage's own write streams may replace the file rather than add to it.
                using (var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    bool fresh = stream.Length == 0;

                    using (var writer = new StreamWriter(stream, new UTF8Encoding(fresh)))
                    {
                        if (fresh)
                            writer.Write(header + "\r\n");

                        writer.Write(FormatRow(values) + "\r\n");
                    }
                }
            }
        }

        /// <summary>
        /// One CSV line (RFC 4180): fields with a comma, quote or line break are quoted, quotes doubled; null is empty.
        /// </summary>
        public static string FormatRow(IReadOnlyList<string?> values)
        {
            var row = new StringBuilder();

            for (int i = 0; i < values.Count; i++)
            {
                if (i > 0)
                    row.Append(',');

                string value = values[i] ?? string.Empty;

                if (value.IndexOfAny(needs_quotes) < 0)
                    row.Append(value);
                else
                    row.Append('"').Append(value.Replace("\"", "\"\"")).Append('"');
            }

            return row.ToString();
        }

        private static string? readFirstLine(string path)
        {
            // The reader skips the byte-order mark.
            using (var reader = new StreamReader(path, Encoding.UTF8, true))
                return reader.ReadLine();
        }
    }
}
