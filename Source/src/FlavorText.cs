using System.Collections.Generic;

public static class FlavorText
{
    private static readonly Dictionary<string, string> itemMessages = new()
    {
        { "CopperOre", "The copper resists your passage, bound by roots older than stone." },
        { "TinOre", "The forge-born tin trembles, awaiting the Elder’s fall." },
        { "Copper", "The copper resists your passage, bound by roots older than stone." },
        { "Tin", "The forge-born tin trembles, awaiting the Elder’s fall." },
        { "Bronze", "Bronze and tin echo with ancient rites. The forest’s guardian still watches." },

        { "IronScrap", "Iron groans with the weight of decay. Bonemass’s curse clings to its core." },
        { "Iron", "Iron groans with the weight of decay. Bonemass’s curse clings to its core." },
        { "Ironpit", "The pit reeks of rot and resistance. The swamp’s champion must be vanquished." },

        { "SilverOre", "Silver shivers with frostbitten fury. The mountain’s dragon still reigns." },
        { "Silver", "The metal remembers the cold breath of Moder. You are not yet worthy." },

        { "BlackMetalScrap", "The scrap pulses with flame-forged pride. The plains demand tribute." },
        { "BlackMetal", "Black metal hums with scorched defiance. Yagluth’s embers burn too bright to pass." },

        { "MechanicalSpring", "Mechanical spring resists the warp, loyal to the Mistlands’ sovereign." },
        { "BronzeScrap", "The bronze scrap clinks with defiance. The Queen’s will must be broken." },
        { "DvergrNeedle", "The extractor trembles, bound by regal decree. The Queen’s dominion remains unbroken." },
        { "IronOre", "The Queen is curious to what you are going to do with this." },

        { "CharredCogwheel", "The cogwheel crackles with emerald flame. Fader’s wrath must be extinguished." },
        { "FlametalOre", "Fader says you spawned the wrong ore" },
        { "Flametal", "The Emerald Flame thinks you should try the New flametal" },
        { "FlametalOreNew", "The ore smolders with unspent vengeance. Fader’s dominion endures." },
        { "FlametalNew", "Flametal recoils from your grasp, forged in fire yet loyal to ash." }
    };

    public static string Get(string itemName) =>
        itemMessages.TryGetValue(itemName, out var msg)
            ? msg
            : "The metal resists your passage. Its guardian still stands.";
}