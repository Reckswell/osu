// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Overlays;
using osu.Game.Rulesets.ManiaOrtho.Configuration;
using osuTK.Input;

namespace osu.Game.Rulesets.ManiaOrtho.UI
{
    /// <summary>
    /// Gameplay hotkeys for the wall, next to mania's scroll speed keys (F3 and F4, which already work here): Shift+F3 and
    /// Shift+F4 move it by <see cref="WALL_STEP"/>, and a toast shows the new value as it does for the scroll speed.
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
        /// How far one press moves the wall, as a share of the track's length.
        /// </summary>
        public const float WALL_STEP = 0.05f;

        private readonly HighwayConfigManager config;
        private readonly Bindable<float> wall;

        private IDisposable? configTracker;

        [Resolved]
        private OnScreenDisplay? onScreenDisplay { get; set; }

        public HighwayHotkeys(HighwayConfigManager config)
        {
            this.config = config;

            wall = config.GetBindable<float>(HighwaySetting.Wall);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Toasts for the wall, as osu! shows for the scroll speed while a ruleset's config is tracked.
            configTracker = onScreenDisplay?.BeginTracking(this, config);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            // Held keys repeat, as F3 and F4 do for the scroll speed.
            if (!e.ShiftPressed || e.ControlPressed || e.AltPressed || e.SuperPressed)
                return false;

            // The setting's own range clamps the wall (none, to the most the scroll allows).
            switch (e.Key)
            {
                case Key.F3:
                    wall.Value -= WALL_STEP;
                    break;

                case Key.F4:
                    wall.Value += WALL_STEP;
                    break;
            }

            // Seen, never handled: the key goes on to everything it would reach in mania.
            return false;
        }

        protected override void Dispose(bool isDisposing)
        {
            configTracker?.Dispose();
            configTracker = null;

            base.Dispose(isDisposing);
        }
    }
}
