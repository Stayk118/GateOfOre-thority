using HarmonyLib;
using System;
using System.Collections.Generic;
using BepInEx.Configuration;

public static class ConfigSync
{
    public static bool UseServerConfig = false;
    public static Dictionary<string, object> ServerItemBossMap = new();

    [HarmonyPatch(typeof(ZNet), "OnNewConnection")]
    public class ServerSendConfig
    {
        static void Postfix(ZNet __instance, ZNetPeer peer)
        {
            if (!ZNet.instance.IsServer()) return;

            var pkg = new ZPackage();
            foreach (var kvp in GateOfOrethority.ItemBossMap)
            {
                pkg.Write(kvp.Key);
                pkg.Write(kvp.Value.Value);
            }

            peer.m_rpc.Invoke("GateOfOrethority_ReceiveConfig", pkg);
        }
    }

    [HarmonyPatch(typeof(ZNet), "Awake")]
    public class ClientRegisterRPC
    {
        static void Postfix(ZNet __instance)
        {
            if (!ZNet.instance.IsServer())
            {
                ZRoutedRpc.instance.Register("GateOfOrethority_ReceiveConfig", new Action<long, ZPackage>(WrappedReceiveConfig));
            }
        }

        private static void WrappedReceiveConfig(long sender, ZPackage pkg)
        {
            ReceiveConfig(pkg);
        }
    }

    public static void ReceiveConfig(ZPackage pkg)
    {
        ServerItemBossMap.Clear();
        while (pkg.GetPos() < pkg.Size())
        {
            string item = pkg.ReadString();
            string flag = pkg.ReadString();
            ServerItemBossMap[item] = flag;
        }

        UseServerConfig = GateOfOrethority.EnableServerOverride.Value;
    }
}