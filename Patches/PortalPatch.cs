using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace GateOfOrethority.Patches
{
    // Confirmed via dnSpy on this game version: the actual portal-use entry point is
    // TeleportWorld.Teleport(Player player) - NOT Interact(Humanoid, bool, bool), which
    // only handles the portal-naming dialog. Teleport() is where vanilla's own metal
    // restriction lives (player.IsTeleportable(this.m_allowAllItems)) and where the
    // teleport actually executes. We prefix it so we can run our OWN check first and
    // either:
    //   a) let vanilla continue untouched (our requirements are satisfied - vanilla will
    //      still run its own IsTeleportable check separately, which is fine/orthogonal), or
    //   b) show our custom lore message and skip the teleport ourselves entirely
    [HarmonyPatch(typeof(TeleportWorld), "Teleport")]
    public static class PortalPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TeleportWorld __instance, Player player)
        {
            // TEMPORARY DIAGNOSTIC - confirms the prefix is actually being reached.
            // Safe to remove once behavior is confirmed working end-to-end.
            UnityEngine.Debug.Log($"[GateOfOrethority] Teleport prefix called for player={player?.GetPlayerName()}");

            if (!Plugin.ModEnabled.Value) return true; // mod off - vanilla behavior only
            if (player == null) return true;

            var inventory = player.GetInventory();
            if (inventory == null) return true;

            // Pull the exact item names/tokens of everything in the inventory - matched
            // exactly against Plugin.BlockedItems, not as a substring, so crafted tools
            // made from a restricted material are never caught by a rule written for
            // the raw material itself.
            var carriedNames = inventory.GetAllItems()
                .Select(item => item.m_shared != null ? item.m_shared.m_name : item.m_dropPrefab?.name ?? "")
                .Where(n => !string.IsNullOrEmpty(n))
                .ToList();

            var unmet = KeyManager.GetUnmetRules(player, carriedNames);

            UnityEngine.Debug.Log($"[GateOfOrethority] carried item names: {string.Join(", ", carriedNames)}");
            UnityEngine.Debug.Log($"[GateOfOrethority] unmet item rules: {string.Join(", ", unmet.Select(r => r.ItemId))}");

            if (unmet.Count == 0) return true; // nothing restricted, or all requirements satisfied

            // Block the teleport ourselves and show our custom lore message instead of vanilla's.
            player.Message(MessageHud.MessageType.Center, Messages.Combined(unmet));
            return false; // skip the original Teleport entirely - no __result needed, method is void
        }
    }
}
