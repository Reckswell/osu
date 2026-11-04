// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.ManiaOrtho.Highway
{
    /// <summary>
    /// The hand groups of a stage, marked by stronger guide lines from 6K up (research/topic-wide-keys.md W1): 3|3, 3|1|3,
    /// 4|4, 4|1|4 and 5|5, mania's default split between the hands, with the odd centre key on its own. Up to 5K a stage
    /// is one group. From the key count alone: key bindings are per variant and need not follow the default.
    /// </summary>
    public static class LaneGroups
    {
        /// <summary>
        /// Whether column <paramref name="column"/> (from 1 to <paramref name="keys"/> − 1) of a stage with
        /// <paramref name="keys"/> columns starts a new group, so the boundary to its left divides two groups.
        /// </summary>
        public static bool StartsGroup(int keys, int column) => keys switch
        {
            6 => column == 3,
            7 => column == 3 || column == 4,
            8 => column == 4,
            9 => column == 4 || column == 5,
            10 => column == 5,
            _ => false,
        };
    }
}
