using System.Collections.Generic;

namespace GateOfOrethority
{
    public static class KeyManager
    {
        // ---- World-level (server-wide) boss defeat check ----
        // Reuses Valheim's own global key system - the same one vanilla uses
        // to unlock recipes/events after a boss dies. No custom storage needed.
        public static bool WorldHasDefeated(string globalKey)
        {
            if (string.IsNullOrEmpty(globalKey)) return true; // no requirement configured
            return ZoneSystem.instance != null && ZoneSystem.instance.GetGlobalKey(globalKey);
        }

        // ---- Personal (per-character) boss defeat check ----
        // Stored in Player.m_customData, which is serialized with the character's
        // save file / profile - persists across sessions, per-character, not per-world.
        public static bool PlayerHasPersonalKey(Player player, string globalKey)
        {
            if (player == null || string.IsNullOrEmpty(globalKey)) return true;
            string dataKey = Plugin.PersonalKeyPrefix + globalKey;
            return player.m_customData != null
                   && player.m_customData.TryGetValue(dataKey, out var val)
                   && val == "true";
        }

        public static void GrantPersonalKey(Player player, string globalKey)
        {
            if (player == null || string.IsNullOrEmpty(globalKey)) return;
            string dataKey = Plugin.PersonalKeyPrefix + globalKey;
            if (player.m_customData == null) return; // defensive - should always exist on a live Player
            player.m_customData[dataKey] = "true";
        }

        // Combined check: BOTH personal and world proof required.
        public static bool IsCleared(Player player, string globalKey)
        {
            return WorldHasDefeated(globalKey) && PlayerHasPersonalKey(player, globalKey);
        }

        // Given the exact item names/tokens the player is carrying, returns every
        // BlockedItemRule that currently applies and is NOT yet cleared. Exact match
        // against Plugin.BlockedItems (case-insensitive) - a rule for "$item_iron" will
        // NOT match "$item_pickaxe_iron", so crafted tools/gear are unaffected.
        public static List<BlockedItemRule> GetUnmetRules(Player player, IEnumerable<string> carriedItemNames)
        {
            var unmet = new List<BlockedItemRule>();
            foreach (var itemName in carriedItemNames)
            {
                if (Plugin.BlockedItems.TryGetValue(itemName, out var rule) && !IsCleared(player, rule.RequiredKey))
                {
                    unmet.Add(rule);
                }
            }
            return unmet;
        }
    }
}
