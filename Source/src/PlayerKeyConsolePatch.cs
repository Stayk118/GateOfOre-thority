using HarmonyLib;

[HarmonyPatch(typeof(Terminal), nameof(Terminal.TryRunCommand))]
public class PlayerKeyConsolePatch
{
    static bool Prefix(Terminal __instance, string text)
    {
        if (!text.StartsWith("ore_")) return true;

        var player = Player.m_localPlayer;
        if (player == null) return true;

        var args = text.Split(' ');
        var command = args[0];

        switch (command)
        {
            case "ore_grantkey":
                if (args.Length < 2) return false;
                player.m_customData[args[1]] = "true";
                __instance.AddString($"[Ore-thority] Granted key '{args[1]}' to {player.GetPlayerName()}");
                return false;

            case "ore_revokekey":
                if (args.Length < 2) return false;
                player.m_customData.Remove(args[1]);
                __instance.AddString($"[Ore-thority] Revoked key '{args[1]}' from {player.GetPlayerName()}");
                return false;

            case "ore_listkeys":
                __instance.AddString($"[Ore-thority] Keys for {player.GetPlayerName()}:");
                foreach (var kvp in player.m_customData)
                {
                    __instance.AddString($"  {kvp.Key} = {kvp.Value}");
                }
                return false;

            case "ore_grantallkeys":
                foreach (var key in GateOfOrethority.AllBossKeys)
                {
                    player.m_customData[key] = "true";
                }
                __instance.AddString($"[Ore-thority] Granted all progression keys to {player.GetPlayerName()}");
                return false;

            case "ore_resetkeys":
                foreach (var key in GateOfOrethority.AllBossKeys)
                {
                    player.m_customData.Remove(key);
                }
                __instance.AddString($"[Ore-thority] Reset all progression keys for {player.GetPlayerName()}");
                return false;

            default:
                __instance.AddString($"[Ore-thority] Unknown command: {command}");
                return false;
        }
    }
}