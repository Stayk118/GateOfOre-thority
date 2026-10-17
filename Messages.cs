using System.Collections.Generic;

namespace GateOfOrethority
{
    public static class Messages
    {
        // One lore line per boss global-key. Fall back message covers anything unmapped.
        private static readonly Dictionary<string, string> BossMessages = new Dictionary<string, string>
        {
            { "defeated_eikthyr",    "The forest spirits reject this metal - Eikthyr's antlers still guard the old ways." },
            { "defeated_gdking",     "The roots hold tight and the Elder denies your passage" },
            { "defeated_bonemass",   "Iron groans with the weight of decay. Bonemass’s curse clings to its core." },
            { "defeated_dragon",     "The metal remembers the cold breath of Moder. You are not yet worthy." },
            { "defeated_goblinking", "Black metal hums with scorched defiance. Yagluth’s embers burn too bright to pass." },
            { "defeated_queen",     "The metal clinks with defiance. The Queen’s will must be broken." },
            { "defeated_fader",     "The metal smolders with unspent vengeance. Fader’s dominion endures." },
            { "defeated_deepnorth_boss", "The frozen north does not yet release its gold to you." } // PLACEHOLDER
        };

        public static string ForBossKey(string globalKey)
        {
            if (BossMessages.TryGetValue(globalKey, out var msg)) return msg;
            return "An old ward blocks this metal from the portal - a great foe must fall first.";
        }

        // Combines messages when multiple items are unmet at once. Each rule's own
        // custom message wins if it set one; otherwise falls back to the generic
        // per-boss line above.
        public static string Combined(List<BlockedItemRule> unmetRules)
        {
            if (unmetRules.Count == 1)
            {
                var rule = unmetRules[0];
                return !string.IsNullOrEmpty(rule.Message) ? rule.Message : ForBossKey(rule.RequiredKey);
            }
            return "The portal rejects your burden - several old wards still hold your items fast.";
        }
    }
}
