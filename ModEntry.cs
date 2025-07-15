using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;

namespace FriendshipInsight
{
    /// <summary>
    /// Mod that logs daily friendship point changes for each NPC.
    /// </summary>
    internal sealed class ModEntry : Mod
    {
        // Stores friendship points at day start
        private readonly Dictionary<string, int> initialFriendship = new();

        // Stores friendship deltas for the previous day
        private readonly Dictionary<string, int> yesterdayDelta = new();

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.DayEnding += OnDayEnding;

            Monitor.Log(Messages.Activation, LogLevel.Info);
        }

        /// <summary>
        /// Fired at day start: shows yesterday’s summary and captures current points.
        /// </summary>
        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.player?.friendshipData == null)
                return;

            try
            {
                ShowPreviousDaySummary();
                initialFriendship.Clear();

                foreach (var (npc, friendship) in Game1.player.friendshipData.Pairs)
                    initialFriendship[npc] = friendship.Points;
            }
            catch (Exception ex)
            {
                Monitor.Log($"⚠️ Error al iniciar el día: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Fired at day end: computes deltas between start and end points.
        /// </summary>
        private void OnDayEnding(object? sender, DayEndingEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.player?.friendshipData == null)
                return;

            try
            {
                yesterdayDelta.Clear();

                foreach (var (npc, friendship) in Game1.player.friendshipData.Pairs)
                {
                    int before = initialFriendship.TryGetValue(npc, out var pts) ? pts : 0;
                    int now   = friendship.Points;
                    int delta = now - before;

                    if (delta != 0)
                        yesterdayDelta[npc] = delta;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"⚠️ Error al finalizar el día: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Logs the friendship changes from the previous day.
        /// </summary>
        private void ShowPreviousDaySummary()
        {
            if (yesterdayDelta.Count == 0)
            {
                Monitor.Log(Messages.NoChanges, LogLevel.Debug);
                return;
            }

            Monitor.Log(Messages.SummaryHeader, LogLevel.Info);
            foreach (var (npc, delta) in yesterdayDelta)
                Monitor.Log(FormatDeltaMessage(npc, delta), LogLevel.Info);
        }

        /// <summary>
        /// Builds the log message for a single NPC’s delta.
        /// </summary>
        private static string FormatDeltaMessage(string npc, int delta)
        {
            string emoji = delta > 0 ? "❤️" : "💔";
            string sign  = delta > 0 ? "+" : "";
            string note  = delta < 0 ? " ¡Ouch!" : "";
            return $"- {sign}{delta} {emoji} con {npc}{note}";
        }
    }

    /// <summary>
    /// Centralizes all user‐facing text for easy maintenance & localization.
    /// </summary>
    internal static class Messages
    {
        public const string Activation    = "📘 Friendship Insight activado. ¡Listo para analizar corazones!";
        public const string NoChanges     = "📊 Ayer no hubo cambios emocionales. Todo tranquilo en Pueblo Pelícano 🌙";
        public const string SummaryHeader = "📊 Ayer ganaste:";
    }
}
