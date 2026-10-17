using HarmonyLib;
using UnityEngine;

namespace GateOfOrethority.Patches
{
    // VERIFY IN DNSPY: Character.OnDeath() is the long-standing hook point for "something died"
    // in Valheim modding, and Character carries a public bool m_boss field used for boss-specific
    // UI (health bar banner, etc). Confirm both still exist under those exact names in your game
    // version before compiling - names have been stable for years but always double check.
    [HarmonyPatch(typeof(Character), "OnDeath")]
    public static class BossDeathPatch
    {
        private const float ProximityRadius = 50f;
        private const string BossKilledRpcName = "GateOfOrethority_BossKilled";

        // How key granting works now:
        //   1. Whichever machine runs Character.OnDeath for the boss (on a dedicated server
        //      that is usually a player's client that owns the boss, NOT the server) sets the
        //      world key and broadcasts ONE message to everybody: "boss X died at position P".
        //   2. Every client that receives it checks its OWN distance to P using its own real
        //      position, and if it is within 50m it grants the key to its OWN local player.
        // Nothing here needs to look up other players' connections, so it doesn't matter
        // which machine detects the death. Granting is idempotent, so duplicate broadcasts
        // (e.g. if OnDeath runs on more than one machine) are harmless.
        [HarmonyPatch(typeof(ZNet), "Awake")]
        private static class RegisterBossKilledRpcPatch
        {
            private static void Postfix()
            {
                ZRoutedRpc.instance.Register<ZPackage>(BossKilledRpcName, OnBossKilledRpc);
            }
        }

        private static void OnBossKilledRpc(long sender, ZPackage pkg)
        {
            if (pkg == null) return;
            string globalKey = pkg.ReadString();
            Vector3 bossPos = pkg.ReadVector3();
            HandleBossKilled(globalKey, bossPos);
        }

        // Runs on every machine that receives the broadcast. The dedicated server has no
        // local player, so it simply does nothing here.
        private static void HandleBossKilled(string globalKey, Vector3 bossPos)
        {
            if (!Plugin.ModEnabled.Value) return;

            var local = Player.m_localPlayer;
            if (local == null) return; // dedicated server, or this player is dead/not spawned

            float dist = Vector3.Distance(local.transform.position, bossPos);
            bool inRange = dist <= ProximityRadius;
            Debug.Log($"[GateOfOrethority] Boss key '{globalKey}' broadcast received. Distance to boss: {dist:F1}m, in range: {inRange}");

            if (!inRange) return;
            KeyManager.GrantPersonalKey(local, globalKey);
            Debug.Log($"[GateOfOrethority] Granted personal key '{globalKey}' to local player.");
        }

        // Prefix so we read position/HP before the object potentially gets torn down.
        [HarmonyPrefix]
        public static void Prefix(Character __instance)
        {
            if (!Plugin.ModEnabled.Value) return;
            if (__instance == null || !__instance.m_boss) return;

            string globalKey = ResolveGlobalKeyForBoss(__instance);
            if (string.IsNullOrEmpty(globalKey)) return; // unmapped boss - skip silently

            Vector3 bossPos = __instance.transform.position;
            Debug.Log($"[GateOfOrethority] Boss '{__instance.name}' died at {bossPos}; broadcasting key '{globalKey}'. IsServer={ZNet.instance != null && ZNet.instance.IsServer()}");

            // Mark world-wide progress exactly like vanilla does for its own boss triggers.
            if (ZoneSystem.instance != null)
            {
                ZoneSystem.instance.SetGlobalKey(globalKey);
            }

            if (ZRoutedRpc.instance != null)
            {
                var pkg = new ZPackage();
                pkg.Write(globalKey);
                pkg.Write(bossPos);
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, BossKilledRpcName, pkg);
            }

            // Also handle it locally in case the broadcast doesn't loop back to the sender.
            // Harmless if it does: granting the same key twice changes nothing.
            HandleBossKilled(globalKey, bossPos);
        }

        // Maps a dying boss Character to the global key your config expects.
        // VERIFY IN DNSPY: match on __instance.name (prefab name, e.g. "Eikthyr", "gd_king",
        // "Bonemass", "Dragon", "GoblinKing", "SeekerQueen", "Fader") - exact prefab names
        // vary slightly and some have version-specific casing. Adjust this switch to match
        // what you actually see in the decompiler / in-game prefab list.
        //
        // Deep North entry is a PLACEHOLDER - shipped Sept 9 2026 with 1.0, boss identity
        // and prefab name not yet confirmed in public sources as of this writing. Update
        // "DeepNorthBoss" and "defeated_deepnorth_boss" once verified.
        private static string ResolveGlobalKeyForBoss(Character boss)
        {
            string n = boss.name.Replace("(Clone)", "");
            switch (n)
            {
                case "Eikthyr": return "defeated_eikthyr";
                case "gd_king": return "defeated_gdking";
                case "Bonemass": return "defeated_bonemass";
                case "Dragon": return "defeated_dragon";
                case "GoblinKing": return "defeated_goblinking";
                case "SeekerQueen": return "defeated_queen";
                case "Fader": return "defeated_fader";
                case "DeepNorthBoss": return "defeated_deepnorth_boss"; // PLACEHOLDER - confirm real prefab name
                default: return null;
            }
        }
    }
}
