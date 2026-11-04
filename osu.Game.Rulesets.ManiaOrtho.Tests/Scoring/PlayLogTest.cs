// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.ManiaOrtho.Scoring;

namespace osu.Game.Rulesets.ManiaOrtho.Tests.Scoring
{
    [TestFixture]
    public class PlayLogTest
    {
        [Test]
        public void TestFieldsThatNeedItAreQuoted()
        {
            string row = PlayLog.FormatRow(new[] { "plain", "a, b", "say \"hi\"", null, "two\nlines" });

            Assert.That(row, Is.EqualTo("plain,\"a, b\",\"say \"\"hi\"\"\",,\"two\nlines\""));
            Assert.That(ParseRow(row), Is.EqualTo(new[] { "plain", "a, b", "say \"hi\"", "", "two\nlines" }));
        }

        [Test]
        public void TestHeaderAndMarkOnlyStartAFile()
        {
            using (var storage = new TemporaryNativeStorage("maniaortho-play-log"))
            {
                PlayLog.Append(storage, row("first"));
                PlayLog.Append(storage, row("second"));

                byte[] bytes = File.ReadAllBytes(storage.GetFullPath(PlayLog.FILENAME));
                byte[] mark = Encoding.UTF8.GetPreamble();
                string text = Encoding.UTF8.GetString(bytes, mark.Length, bytes.Length - mark.Length);

                Assert.That(bytes.Take(mark.Length), Is.EqualTo(mark), "starts with a byte-order mark");
                // By character: culture-aware string search ignores a zero-width mark and finds it anywhere.
                Assert.That(text.IndexOf('\uFEFF'), Is.EqualTo(-1), "only one mark");

                string[] lines = text.Split("\r\n");

                Assert.That(lines, Has.Length.EqualTo(4), "header, two rows, and the final line break");
                Assert.That(lines[0], Is.EqualTo(string.Join(",", PlayLog.COLUMNS)));
                Assert.That(ParseRow(lines[1])[0], Is.EqualTo("first"));
                Assert.That(ParseRow(lines[2])[0], Is.EqualTo("second"));
                Assert.That(lines[3], Is.Empty);
            }
        }

        [Test]
        public void TestAnOlderHeaderStartsANewFile()
        {
            using (var storage = new TemporaryNativeStorage("maniaortho-play-log-columns"))
            {
                File.WriteAllText(storage.GetFullPath(PlayLog.FILENAME, true), "time,result\r\nold,pass\r\n", new UTF8Encoding(true));

                PlayLog.Append(storage, row("new"));

                string[] kept = storage.GetFiles(".", "plays until *.csv").ToArray();

                Assert.That(kept, Has.Length.EqualTo(1), "the old file is kept under a dated name");
                Assert.That(File.ReadAllText(storage.GetFullPath(kept[0])), Is.EqualTo("time,result\r\nold,pass\r\n"));
                Assert.That(File.ReadAllLines(storage.GetFullPath(PlayLog.FILENAME)), Is.EqualTo(new[] { string.Join(",", PlayLog.COLUMNS), PlayLog.FormatRow(row("new")) }));
            }
        }

        private static string?[] row(string time)
        {
            string?[] values = new string?[PlayLog.COLUMNS.Length];
            values[0] = time;
            return values;
        }

        /// <summary>
        /// Splits one CSV line written by <see cref="PlayLog.FormatRow"/>.
        /// </summary>
        public static string[] ParseRow(string line)
        {
            var fields = new List<string>();
            var field = new StringBuilder();
            bool quoted = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (quoted)
                {
                    if (c == '"' && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else if (c == '"')
                        quoted = false;
                    else
                        field.Append(c);
                }
                else if (c == '"')
                    quoted = true;
                else if (c == ',')
                {
                    fields.Add(field.ToString());
                    field.Clear();
                }
                else
                    field.Append(c);
            }

            fields.Add(field.ToString());
            return fields.ToArray();
        }
    }
}
