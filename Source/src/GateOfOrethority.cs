using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using BepInEx.Configuration;
using System.Collections.Generic;

[BepInPlugin("com.stayk.gateoforethority", "Gate of Ore-thority", "1.0.3")]
public class GateOfOrethority : BaseUnityPlugin
{
    public static ManualLogSource Log;
    public static ConfigEntry<bool> EnableServerOverride;
    public static Dictionary<string, string> ItemBossMap = new(); // Enforced values only

    public static readonly List<string> AllBossKeys = new()
    {
        "defeated_eikthyr",
        "defeated_elder",
        "defeated_bonemass",
        "defeated_moder",
        "defeated_yagluth",
        "defeated_queen",
        "defeated_fader"
    };

    private Dictionary<string, ConfigEntry<string>> RawConfigEntries = new();

    private void Awake()
    {
        Log = Logger;

        EnableServerOverride = Config.Bind("General", "EnableServerOverride", false,
            "If true, clients will use server-enforced boss flags for teleport restrictions.");

        RegisterItem("CopperOre", "defeated_elder");
        RegisterItem("TinOre", "defeated_elder");
        RegisterItem("Copper", "defeated_elder");
        RegisterItem("Tin", "defeated_elder");
        RegisterItem("Bronze", "defeated_elder");

        RegisterItem("IronScrap", "defeated_bonemass");
        RegisterItem("Iron", "defeated_bonemass");
        RegisterItem("Ironpit", "defeated_bonemass");

        RegisterItem("SilverOre", "defeated_moder");
        RegisterItem("Silver", "defeated_moder");

        RegisterItem("BlackMetalScrap", "defeated_yagluth");
        RegisterItem("BlackMetal", "defeated_yagluth");

        RegisterItem("MechanicalSpring", "defeated_queen");
        RegisterItem("DvergrNeedle", "defeated_queen");
        RegisterItem("IronOre", "defeated_queen");
        RegisterItem("BronzeScrap", "defeated_queen");

        RegisterItem("CharredCogwheel", "defeated_fader");
        RegisterItem("FlametalOre", "defeated_fader");
        RegisterItem("Flametal", "defeated_fader");
        RegisterItem("FlametalNew", "defeated_fader");
        RegisterItem("FlametalOreNew", "defeated_fader");

        EnforceServerConfig();

        Harmony.CreateAndPatchAll(typeof(GateOfOrethority).Assembly, null);
        Log.LogInfo("Gate of Ore-thority initialized with server override and player key support.");
    }

    private void RegisterItem(string itemName, string defaultBossKey)
    {
        var entry = Config.Bind("TeleportRestrictions", itemName, defaultBossKey,
            $"Boss flag required to teleport with {itemName}.");
        RawConfigEntries[itemName] = entry;
    }

    private void EnforceServerConfig()
    {
        bool isServer = ZNet.instance != null && ZNet.instance.IsServer();
        bool overrideEnabled = EnableServerOverride.Value;

        foreach (var kvp in RawConfigEntries)
        {
            string item = kvp.Key;
            string bossKey = kvp.Value.Value;

            if (!isServer && overrideEnabled)
            {
                // Enforce server-side values on clients
                bossKey = kvp.Value.DefaultValue.ToString();
                Log.LogInfo($"[Ore-thority] Overriding client config for '{item}' → '{bossKey}'");
            }

            ItemBossMap[item] = bossKey;
        }

        if (!isServer && overrideEnabled)
        {
            Log.LogInfo("[Ore-thority] Server override is active. Client config values have been replaced.");
        }
        else
        {
            Log.LogInfo("[Ore-thority] Using local config values.");
        }
    }
}