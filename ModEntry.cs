using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;

namespace FriendshipInsight;

internal sealed class ModEntry : Mod
{
    private Dictionary<string, int> amistadInicial = new();
    private Dictionary<string, int> amistadDeltaAyer = new();

    public override void Entry(IModHelper helper)
    {
        Monitor.Log("📘 Friendship Insight activado. ¡Listo para analizar corazones!", LogLevel.Info);

        helper.Events.GameLoop.DayStarted += OnDayStarted;
        helper.Events.GameLoop.DayEnding += OnDayEnding;
    }

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        MostrarResumenDelDiaAnterior();

        amistadInicial.Clear();
        foreach (var kvp in Game1.player.friendshipData)
        {
            amistadInicial[kvp.Key] = kvp.Value.Points;
        }
    }

    private void OnDayEnding(object? sender, DayEndingEventArgs e)
    {
        amistadDeltaAyer.Clear();

        foreach (var kvp in Game1.player.friendshipData)
        {
            string npc = kvp.Key;
            int puntosAntes = amistadInicial.ContainsKey(npc) ? amistadInicial[npc] : 0;
            int puntosAhora = kvp.Value.Points;
            int delta = puntosAhora - puntosAntes;

            if (delta != 0)
                amistadDeltaAyer[npc] = delta;
        }
    }

    private void MostrarResumenDelDiaAnterior()
    {
        if (amistadDeltaAyer.Count == 0)
        {
            Monitor.Log("📊 Ayer no hubo cambios emocionales. Todo tranquilo en Pueblo Pelícano 🌙", LogLevel.Debug);
            return;
        }

        Monitor.Log("📊 Ayer ganaste:", LogLevel.Info);

        foreach (var kvp in amistadDeltaAyer)
        {
            string npc = kvp.Key;
            int delta = kvp.Value;
            string emoji = delta > 0 ? "❤️" : "💔";
            string texto = delta > 0 ? $"+{delta}" : $"{delta}";
            string adorno = delta < 0 ? " ¡Ouch!" : "";

            Monitor.Log($"- {texto} {emoji} con {npc}{adorno}", LogLevel.Info);
        }
    }
}
