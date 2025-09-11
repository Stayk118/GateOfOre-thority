using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;

[HarmonyPatch(typeof(Player), nameof(Player.TeleportTo))]
public class TeleportBlocker
{
    static bool Prefix(Player __instance, Vector3 pos, Quaternion rot, bool distantTeleport)
    {
        var inventory = __instance.GetInventory();
        var items = inventory.GetAllItems();

        foreach (var kvp in GateOfOrethority.ItemBossMap)
        {
            string itemName = kvp.Key;
            string requiredKey = kvp.Value.Value;

            foreach (var item in items)
            {
                string prefabName = item.m_dropPrefab?.name ?? item.m_shared.m_name;
                bool hasKey = __instance.m_customData.ContainsKey(requiredKey);

                GateOfOrethority.Log.LogInfo($"Teleport check: item='{prefabName}', requiredKey='{requiredKey}', playerHasKey={hasKey}");

                if ((prefabName == itemName || item.m_shared.m_name == itemName) && !hasKey)
                {
                    string message = FlavorText.Get(itemName);

                    if (__instance == Player.m_localPlayer && MessageHud.instance != null)
                    {
                        MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, message);
                    }
                    else
                    {
                        GateOfOrethority.Log.LogWarning($"[TeleportBlocker] Could not show message: {message}");
                    }

                    return false;
                }
            }
        }

        return true;
    }
}