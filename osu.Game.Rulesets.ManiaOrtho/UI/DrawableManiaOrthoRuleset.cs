// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.ManiaOrtho.UI
{
    /// <summary>
    /// The drawable ruleset for <see cref="ManiaOrthoRuleset"/>.
    /// </summary>
    /// <remarks>
    /// PLACEHOLDER: this currently renders exactly like osu!mania. The tilted highway playfield replaces this
    /// (see docs/ortho/BACKLOG.md, milestone M1). Judgement and input handling must stay identical to mania.
    /// </remarks>
    public partial class DrawableManiaOrthoRuleset : DrawableManiaRuleset
    {
        public DrawableManiaOrthoRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod>? mods = null)
            : base(ruleset, beatmap, mods)
        {
        }
    }
}
