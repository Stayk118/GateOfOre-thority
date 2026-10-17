using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace GateOfOrethority
{
    /// <summary>
    /// Minimal one-directional config sync: the server pushes its values to each client
    /// on connect; clients apply them and restore their own local values on disconnect.
    ///
    /// Deliberately does NOT support client->server pushes (no live admin editing).
    /// That's what keeps this small AND safe: the server never accepts config from a
    /// client, so there is no admin-permission check to get wrong.
    ///
    /// Trade-offs vs. the full ServerSync library:
    ///   - no mod version-mismatch warnings/disconnects
    ///   - no live in-game config editing (server must restart to change values)
    ///   - no arbitrary synced value types beyond BepInEx ConfigEntry
    /// </summary>
    public static class ConfigSync
    {
        private const string RpcName = "GateOfOrethority_ConfigSync";

        // Entries registered for syncing, keyed by "Section.Key" for wire transport.
        private static readonly Dictionary<string, ConfigEntryBase> Registered =
            new Dictionary<string, ConfigEntryBase>();

        // The client's own on-disk values, stashed before server values overwrite them,
        // so single-player/other-server sessions aren't polluted by one server's config.
        private static readonly Dictionary<string, object> LocalBackup =
            new Dictionary<string, object>();

        private static bool serverValuesApplied;

        /// <summary>Register a config entry to be overwritten by the server's value.</summary>
        public static void Register(ConfigEntryBase entry)
        {
            string id = Id(entry);
            if (!Registered.ContainsKey(id)) Registered[id] = entry;
        }

        private static string Id(ConfigEntryBase entry) =>
            entry.Definition.Section + "." + entry.Definition.Key;

        // ---------------------------------------------------------------
        // Serialization
        // ---------------------------------------------------------------

        // BepInEx's TomlTypeConverter is what writes/reads .cfg files, so round-tripping
        // through it means any type BepInEx can persist, we can sync - no custom per-type code.
        private static ZPackage BuildPackage()
        {
            var pkg = new ZPackage();
            pkg.Write(Registered.Count);
            foreach (var kvp in Registered)
            {
                pkg.Write(kvp.Key);
                pkg.Write(TomlTypeConverter.ConvertToString(
                    kvp.Value.BoxedValue, kvp.Value.SettingType));
            }
            return pkg;
        }

        private static void ApplyPackage(ZPackage pkg)
        {
            int count = pkg.ReadInt();
            for (int i = 0; i < count; i++)
            {
                string id = pkg.ReadString();
                string raw = pkg.ReadString();

                if (!Registered.TryGetValue(id, out var entry)) continue; // unknown/removed setting

                try
                {
                    // Stash the local value once, the first time we overwrite it.
                    if (!LocalBackup.ContainsKey(id)) LocalBackup[id] = entry.BoxedValue;

                    entry.BoxedValue = TomlTypeConverter.ConvertToValue(raw, entry.SettingType);
                }
                catch (Exception ex)
                {
                    // A malformed/incompatible value shouldn't take the whole connection down.
                    UnityEngine.Debug.LogWarning($"[GateOfOrethority] Could not apply synced config '{id}': {ex.Message}");
                }
            }

            serverValuesApplied = true;
            Plugin.RebuildBlockedItems(); // config changed - refresh the derived lookup
        }

        private static void RestoreLocalValues()
        {
            if (!serverValuesApplied) return;

            foreach (var kvp in LocalBackup)
            {
                if (Registered.TryGetValue(kvp.Key, out var entry))
                {
                    try { entry.BoxedValue = kvp.Value; }
                    catch { /* entry type changed underneath us - ignore */ }
                }
            }

            LocalBackup.Clear();
            serverValuesApplied = false;
            Plugin.RebuildBlockedItems();
        }

        // ---------------------------------------------------------------
        // Networking
        // ---------------------------------------------------------------

        // VERIFY IN DNSPY: ZNet.Awake, ZRoutedRpc.instance.Register<T>(string, Action<long,T>),
        // ZNet.instance.IsServer(), ZNet.m_peers / ZNetPeer.m_rpc / ZNetPeer.m_uid, and the
        // RPC_PeerInfo signature below are the version-sensitive touchpoints in this file.
        // They've been stable for a long time but confirm against your game build before
        // trusting this in multiplayer.

        [HarmonyPatch(typeof(ZNet), "Awake")]
        private static class RegisterRpcPatch
        {
            private static void Postfix()
            {
                ZRoutedRpc.instance.Register<ZPackage>(RpcName, OnReceiveConfig);
            }
        }

        // Handler runs on BOTH sides, so the server must ignore anything a client sends.
        // This single guard is the entire security model for this sync implementation.
        private static void OnReceiveConfig(long sender, ZPackage pkg)
        {
            if (ZNet.instance == null) return;
            if (ZNet.instance.IsServer()) return; // never accept config FROM a client
            if (pkg == null) return;

            ApplyPackage(pkg);
        }

        // Server side: once a peer has finished handshaking, push the config to them.
        // VERIFY IN DNSPY: RPC_PeerInfo is private; if its signature changed, adjust the
        // patch target. We resolve the calling peer by scanning ZNet.m_peers for a
        // matching m_rpc rather than via ZNet.GetPeer, since that method's available
        // overloads vary by game version and may not accept a ZRpc directly.
        [HarmonyPatch(typeof(ZNet), "RPC_PeerInfo")]
        private static class SendOnPeerConnectPatch
        {
            private static void Postfix(ZNet __instance, ZRpc rpc)
            {
                if (!__instance.IsServer()) return;
                if (!Plugin.ServerAuthoritative.Value) return; // server opted out of enforcing its config

                ZNetPeer peer = null;
                foreach (var p in __instance.GetPeers())
                {
                    if (p.m_rpc == rpc) { peer = p; break; }
                }
                if (peer == null) return;

                ZRoutedRpc.instance.InvokeRoutedRPC(peer.m_uid, RpcName, BuildPackage());
            }
        }

        // Client side: leaving a server must undo whatever that server imposed, or the
        // values leak into the next single-player session / next server.
        [HarmonyPatch(typeof(ZNet), "Shutdown")]
        private static class RestoreOnDisconnectPatch
        {
            private static void Prefix()
            {
                RestoreLocalValues();
            }
        }
    }
}
