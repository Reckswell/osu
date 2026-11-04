// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.ManiaOrtho.Configuration;
using osu.Game.Rulesets.ManiaOrtho.Highway;
using osuTK.Input;

namespace osu.Game.Rulesets.ManiaOrtho.UI
{
    /// <summary>
    /// Gameplay hotkeys for the wall, next to mania's scroll speed keys (F3 and F4, which already work here): Shift+F3 and
    /// Shift+F4 move it by one step of the chosen <see cref="WallScale"/> (5%, one Chunithm step, 50 white number or 5% of
    /// the lane; <see cref="WallScales.KeyStep"/>), and a toast shows the new value (<see cref="HighwayToasts"/>) as it does
    /// for the scroll speed.
    /// </summary>
    /// <remarks>
    /// It only watches the keys and never handles them, so every key press goes on to exactly what it reaches in mania (a
    /// column bound to F4 is still pressed by Shift+F4). It sits on the drawable ruleset, outside the ruleset's input
    /// manager, so nothing of it reaches a replay. Shift is read from the keyboard state, not from the key events this
    /// receives, so the keys also work while watching a replay, where osu! takes Shift itself (to fast-forward). The keys are
    /// fixed: osu!'s key binding settings list only global actions and the ruleset's own, and adding one to the ruleset's
    /// would put it into mania's replays. Unlike the scroll speed, the wall can be moved at any point of a song, as lane
    /// covers can in the arcade games; it never changes what is judged.
    /// </remarks>
    public partial class HighwayHotkeys : Component
    {
        /// <summary>
        /// How far one press moves the wall with <see cref="WallScale.Percent"/>, as a share of the track's length.
        /// </summary>
        public const float WALL_STEP = 0.05f;

        private readonly HighwayConfigManager config;
        private readonly Bindable<float> wall;

        public HighwayHotkeys(HighwayConfigManager config)
        {
            this.config = config;
            wall = config.GetBindable<float>(HighwaySetting.Wall);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            // Held keys repeat, as F3 and F4 do for the scroll speed.
            if (!e.ShiftPressed || e.ControlPressed || e.AltPressed || e.SuperPressed)
                return false;

            // The setting's own range clamps the wall (none, to the most the scroll allows).
            int steps;

            switch (e.Key)
            {
                case Key.F3:
                    steps = -1;
                    break;

                case Key.F4:
                    steps = 1;
                    break;

                default:
                    return false;
            }

            var settings = config.Current;
            wall.Value = WallScales.Step(settings.WallScale, wall.Value, steps, settings.ToLayout().FarWidthRatio);

            // Seen, never handled: the key goes on to everything it would reach in mania.
            return false;
        }
    }
}
