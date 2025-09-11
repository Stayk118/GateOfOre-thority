using HarmonyLib;

[HarmonyPatch(typeof(Character), "OnDeath")]
public class BossDeathTracker
{
    static void Postfix(Character __instance)
    {
        string prefabName = Utils.GetPrefabName(__instance.gameObject);
        GateOfOrethority.Log.LogInfo($"Boss died: prefab = {prefabName}");

        string bossKey = prefabName switch
        {
            "Eikthyr" => "defeated_eikthyr",
            "deer" => "defeated_eikthyr", // fallback alias
            "gd_king" => "defeated_elder",
            "Bonemass" => "defeated_bonemass",
            "Dragon" => "defeated_moder",
            "GoblinKing" => "defeated_yagluth",
            "SeekerQueen" => "defeated_queen",
            "Queen" => "defeated_queen", // optional fallback
            "Fader" => "defeated_fader",
            _ => null
        };

        if (bossKey == null) return;

        foreach (var player in Player.GetAllPlayers())
        {
            if (!player.m_customData.ContainsKey(bossKey))
            {
                player.m_customData[bossKey] = "true";
                GateOfOrethority.Log.LogInfo($"Set player key '{bossKey}' for {player.GetPlayerName()}");
            }
        }
    }
}