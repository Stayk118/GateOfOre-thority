using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace GateOfOrethority
{
    // One configured rule: a specific item is blocked from portal use unless the
    // player clears RequiredKey (both personally and world-wide). Message is optional -
    // falls back to a generic line if not set.
    public class BlockedItemRule
    {
        public string ItemId;
        public string RequiredKey;
        public string Message;
    }

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = "Stayk.valheim.gateoforethority";
        public const string PluginName = "Gate of Ore-thority";
        public const string PluginVersion = "1.0.7";

        private readonly Harmony harmony = new Harmony(PluginGUID);

        // Server-authoritative config is handled by our own ConfigSync.cs - see that file
        // for what it does and doesn't cover compared to the ServerSync library.

        // ---- Config ----
        public static ConfigEntry<bool> ModEnabled;
        public static ConfigEntry<bool> ServerAuthoritative; // toggles whether ALL configs below sync from server

        // Personal-key prefix stored in Player.m_customData
        public const string PersonalKeyPrefix = "GateOfOrethority_";

        // Each known item gets its own three config entries (Enabled/RequiredKey/Message),
        // grouped under a section named after the item - this is what makes each item
        // individually toggleable/editable in a config-manager GUI instead of one big
        // text blob. Add a new item by adding one line to KnownItems below.
        private class ItemConfigEntries
        {
            public string ItemId;
            public ConfigEntry<bool> Enabled;
            public ConfigEntry<string> RequiredKey;
            public ConfigEntry<string> Message;
        }

        private static readonly List<ItemConfigEntries> ItemConfigs = new List<ItemConfigEntries>();

        // Parsed/live lookup built from ItemConfigs at runtime: exact item id -> rule.
        // Everything else in the mod (KeyManager, Commands, PortalPatch) reads from
        // this dictionary and doesn't care how it was populated.
        public static readonly Dictionary<string, BlockedItemRule> BlockedItems =
            new Dictionary<string, BlockedItemRule>(StringComparer.OrdinalIgnoreCase);

        // (ItemId, default RequiredKey, default Message) - the starting set of known items.
        // ItemId must be the item's exact name/token (e.g. "$item_iron"), not a substring -
        // crafted tools/gear made from a material will NOT match a rule for the raw material.
        // Defaults below use "$item_iron" as confirmed via in-game diagnostic logging on
        // this game version. The others are best-known guesses at token spelling - VERIFY
        // each one against your own LogOutput.log before relying on it; a rule with a
        // wrong ItemId simply never matches anything (no error, silently inert).
        private static readonly (string ItemId, string RequiredKey, string Message)[] KnownItems =
        {
            ("$item_copper", "defeated_gdking", ""),
            ("$item_copperore", "defeated_gdking", ""),
            ("$item_tin", "defeated_gdking", ""),
            ("$item_tinore", "defeated_gdking", ""),
            ("$item_bronze", "defeated_gdking", ""),
            ("$item_iron", "defeated_bonemass", ""),
            ("$item_ironpit", "defeated_bonemass", ""),
            ("$item_ironscrap", "defeated_bonemass", ""),
            ("$item_ironore", "defeated_bonemass", "Leeches will feed upon the fish that gave this treasure"),
            ("$item_dragonegg", "defeated_dragon", "Moder binds her clutch to the mountain realm"),
            ("$item_silver", "defeated_dragon", ""),
            ("$item_silverore", "defeated_dragon", ""),
            ("$item_blackmetalscrap", "defeated_goblinking", ""),
            ("$item_blackmetal", "defeated_goblinking", ""),
            ("$item_mechanicalspring", "defeated_goblinking", ""),
            ("$item_dvergrneedle", "defeated_queen", "The extractor trembles, the Dvergr do not yet recognise your ability"),
            ("$item_bronzescrap", "defeated_queen", ""),
            ("$item_flametal_old", "defeated_fader", "Fader things you should try the new Flametal"),
            ("$item_flametalore_old", "defeated_fader", "Fader says you spawned the wrong ore"),
            ("$item_flametalore", "defeated_fader", ""),
            ("$item_flametal", "defeated_fader", ""),
            ("$item_charredcogwheel", "defeated_fader", "The cogwheel crackles with emerald flame. Fader’s wrath must be extinguished"),
        };

        private void Awake()
        {
            BindConfig();
            RebuildBlockedItems();

            harmony.PatchAll();

            foreach (var method in harmony.GetPatchedMethods())
            {
                Logger.LogInfo($"[GateOfOrethority] Harmony reports patched: {method.DeclaringType?.FullName}.{method.Name}");
            }

            Logger.LogInfo($"{PluginName} v{PluginVersion} loaded.");
        }

        private void BindConfig()
        {
            ModEnabled = Config.Bind("General", "Enabled", true,
                "Master on/off switch for the mod.");

            ServerAuthoritative = Config.Bind("General", "ServerAuthoritative", true,
                "If true, clients joining a server running this mod use the SERVER's config values below, not their own local ones.");

            var syncedEntries = new List<ConfigEntryBase> { ModEnabled };

            foreach (var (itemId, defaultKey, defaultMessage) in KnownItems)
            {
                // Section name = the item id itself, so each item gets its own clearly
                // labeled group of fields in a config-manager GUI.
                var entries = new ItemConfigEntries
                {
                    ItemId = itemId,
                    Enabled = Config.Bind(itemId, "Enabled", true,
                        "Whether this item is blocked at all. Set false to remove the rule without deleting it."),
                    RequiredKey = Config.Bind(itemId, "RequiredKey", defaultKey,
                        "The global key that must be defeated (both world-wide AND personally) to carry this item through a portal."),
                    Message = Config.Bind(itemId, "Message", defaultMessage,
                        "Shown when this item blocks the portal. Leave blank to fall back to a generic message for that boss.")
                };

                ItemConfigs.Add(entries);
                syncedEntries.Add(entries.Enabled);
                syncedEntries.Add(entries.RequiredKey);
                syncedEntries.Add(entries.Message);
            }

            // Register every entry to be overwritten by the server's value on join.
            foreach (var entry in syncedEntries)
            {
                ConfigSync.Register(entry);
            }

            // NOTE: ServerAuthoritative is deliberately NOT synced - it's read on the
            // server to decide whether to push at all, and a client's local copy is
            // irrelevant (a client can't opt out of receiving the server's values).
        }

        // Rebuilds BlockedItems from the live config entries. Call again if config changes at runtime.
        public static void RebuildBlockedItems()
        {
            BlockedItems.Clear();

            foreach (var entries in ItemConfigs)
            {
                if (!entries.Enabled.Value) continue; // disabled - skip without deleting the entry

                string requiredKey = entries.RequiredKey.Value?.Trim();
                if (string.IsNullOrEmpty(requiredKey)) continue;

                BlockedItems[entries.ItemId] = new BlockedItemRule
                {
                    ItemId = entries.ItemId,
                    RequiredKey = requiredKey,
                    Message = string.IsNullOrWhiteSpace(entries.Message.Value) ? null : entries.Message.Value.Trim()
                };
            }
        }
    }
}