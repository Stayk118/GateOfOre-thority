using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace GateOfOrethority
{
    /// <summary>
    /// Console commands for inspecting and editing personal GateOfOrethority boss-defeat
    /// keys on the current character, for testing/admin use.
    ///
    /// Usage in-game: press F5 to open the console.
    ///   gateoforethority_keys                 - lists your personal keys
    ///   gateoforethority_addkey <key>         - grants a personal key (tab-complete: configured keys)
    ///   gateoforethority_removekey <key>      - removes a personal key (tab-complete: keys you have)
    ///
    /// None of these are cheat-flagged, so no "devcommands" needed first.
    ///
    /// Confirmed via dnSpy on this game version: Terminal.ConsoleCommand's action
    /// parameter must be a void-returning Terminal.ConsoleEvent (not the bool-returning
    /// Terminal.ConsoleEventFailable overload), and optionsFetcher is a zero-argument
    /// Terminal.ConsoleOptionsFetcher (List<string> Method()), not Func&lt;Terminal, List&lt;string&gt;&gt;.
    /// </summary>
    public static class Commands
    {
        private static bool registered;

        [HarmonyPatch(typeof(Terminal), "InitTerminal")]
        private static class RegisterCommandsPatch
        {
            private static void Postfix()
            {
                if (registered) return;
                registered = true;

                new Terminal.ConsoleCommand(
                    "gateoforethority_keys",
                    "Lists your character's personal GateOfOrethority boss-defeat keys.",
                    args =>
                    {
                        var player = Player.m_localPlayer;
                        if (player == null || player.m_customData == null)
                        {
                            args.Context.AddString("[GateOfOrethority] No local player found.");
                            return;
                        }

                        var keys = PersonalKeys(player).ToList();

                        if (keys.Count == 0)
                        {
                            args.Context.AddString("[GateOfOrethority] No personal boss keys set on this character yet.");
                            return;
                        }

                        args.Context.AddString($"[GateOfOrethority] {keys.Count} personal key(s) on this character:");
                        foreach (var shortKey in keys)
                        {
                            args.Context.AddString($"  {shortKey} = true");
                        }
                    },
                    isCheat: false
                );

                new Terminal.ConsoleCommand(
                    "gateoforethority_addkey",
                    "Grants a personal boss-defeat key to your current character. Usage: gateoforethority_addkey <key>",
                    args =>
                    {
                        var player = Player.m_localPlayer;
                        if (player == null || player.m_customData == null)
                        {
                            args.Context.AddString("[GateOfOrethority] No local player found.");
                            return;
                        }

                        if (args.Length < 2)
                        {
                            args.Context.AddString("[GateOfOrethority] Usage: gateoforethority_addkey <key>");
                            return;
                        }

                        string key = args[1];
                        KeyManager.GrantPersonalKey(player, key);
                        args.Context.AddString($"[GateOfOrethority] Granted personal key: {key}");
                    },
                    isCheat: false,
                    optionsFetcher: () => KnownConfiguredKeys().ToList()
                );

                new Terminal.ConsoleCommand(
                    "gateoforethority_removekey",
                    "Removes a personal boss-defeat key from your current character. Usage: gateoforethority_removekey <key>",
                    args =>
                    {
                        var player = Player.m_localPlayer;
                        if (player == null || player.m_customData == null)
                        {
                            args.Context.AddString("[GateOfOrethority] No local player found.");
                            return;
                        }

                        if (args.Length < 2)
                        {
                            args.Context.AddString("[GateOfOrethority] Usage: gateoforethority_removekey <key>");
                            return;
                        }

                        string key = args[1];
                        string dataKey = Plugin.PersonalKeyPrefix + key;

                        if (!player.m_customData.ContainsKey(dataKey))
                        {
                            args.Context.AddString($"[GateOfOrethority] Key not set: {key}");
                            return;
                        }

                        player.m_customData.Remove(dataKey);
                        args.Context.AddString($"[GateOfOrethority] Removed personal key: {key}");
                    },
                    isCheat: false,
                    optionsFetcher: () => PersonalKeys(Player.m_localPlayer).ToList()
                );
            }
        }

        // Keys currently stamped on a given player's character, with the storage
        // prefix stripped back off for display/removal purposes.
        private static IEnumerable<string> PersonalKeys(Player player)
        {
            if (player?.m_customData == null) return Enumerable.Empty<string>();
            return player.m_customData.Keys
                .Where(k => k.StartsWith(Plugin.PersonalKeyPrefix))
                .Select(k => k.Substring(Plugin.PersonalKeyPrefix.Length));
        }

        // Every RequiredKey currently referenced by the configured item rules -
        // used as addkey's tab-complete suggestions so you're not guessing spellings.
        private static IEnumerable<string> KnownConfiguredKeys()
        {
            return Plugin.BlockedItems.Values
                .Select(r => r.RequiredKey)
                .Distinct();
        }
    }
}
