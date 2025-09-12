# Gate of Ore-thority

**Boss-gated teleport restrictions with lore themed flavour text and server sync capabilities.**  
Crafted by Stayk, straight up this was vibe coded with CoPilot. I have no idea what I am doing when it comes to programming, but I wanted to learn.
I sincerly hope this doesn't offend you, even these metadata files I used CoPilot to create

## 🧙 Features

- 🔐 **Teleport Restrictions**: Prevents teleporting with specific items until the corresponding boss is defeated.
- 📜 **Flavorful Feedback**: Custom messages for each item, themed to its biome and boss.
- ⚙️ **Configurable**: Easily add or remove gated items via config or code.
- 🧪 **Console Commands**: Grant, revoke, list, or reset progression keys for testing and admin control.
- 🧠 **Multiplayer-Aware**: Works in both solo and multiplayer environments.

## 🌍 World Modifier Requirement: Casual Portals

Gate of Ore-thority assumes that your world uses the Casual Portals modifier, which allows players to teleport with ores and other heavy items. This is essential for the mod’s teleport restriction system to function meaningfully—without it, players would be blocked by vanilla mechanics before your mod even runs its checks.

## 🛠 Console Commands

| Command                  | Description                                      |
|--------------------------|--------------------------------------------------|
| `ore_grantkey <key>`     | Grants a progression key to the local player     |
| `ore_revokekey <key>`    | Removes a progression key from the local player  |
| `ore_listkeys`           | Lists all progression keys the player has        |
| `ore_grantallkeys`       | Grants all progression keys                      |
| `ore_resetkeys`          | Removes all progression keys from the player     |

### 🗝️ Teleport Restriction Keys
Gate of Ore-thority uses boss progression flags to restrict teleporting with specific items. These flags are stored in each player's custom data and are typically granted upon defeating the corresponding boss.
| Item Name                 | Required Boss Flag       | Notes                      | 
|-------------------------- |--------------------------|----------------------------| 
| CopperOre                 | defeated_elder           |                            | 
| TinOre                    | defeated_elder           |                            | 
| Copper                    | defeated_elder           |                            | 
| Tin                       | defeated_elder           |                            | 
| Bronze                    | defeated_elder           |                            | 
| IronScrap                 | defeated_bonemass        |                            | 
| Iron                      | defeated_bonemass        |                            | 
| Ironpit                   | defeated_bonemass        |                            | 
| SilverOre                 | defeated_moder           |                            | 
| Silver                    | defeated_moder           |                            | 
| BlackMetalScrap           | defeated_yagluth         |                            | 
| BlackMetal                | defeated_yagluth         |                            | 
| MechanicalSpring          | defeated_queen           |                            | 
| DvergrNeedle              | defeated_queen           |                            | 
| IronOre                   | defeated_queen           | Not currently used in-game | 
| BronzeScrap               | defeated_queen           |                            | 
| CharredCogwheel           | defeated_fader           |                            | 
| FlametalOre               | defeated_fader           | Not currently used in-game | 
| Flametal                  | defeated_fader           | Not currently used in-game | 
| FlametalNew               | defeated_fader           |                            | 
| FlametalOreNew            | defeated_fader           |                            |

## 🧙 Multiplayer Boss Flag Behavior

Gate of Ore-thority relies on boss progression flags (e.g. defeated_bonemass) stored in each player’s custom data to determine teleport eligibility. These flags are typically granted when a boss is defeated—but how they’re distributed in multiplayer depends on proximity.

### 🔑 How Boss Flags Are Granted
- When a boss is killed, all players who are logged in and nearby will receive the corresponding boss flag.
- The flag is stored in each player’s m_customData and used by this mod to gate teleport access.
- The flag is not limited to the player who lands the killing blow.

### 📍 What Counts as “Nearby”?
- Players within approximately 100–150 meters of the boss at the moment of death are considered nearby.
- This includes anyone actively participating in the fight or spectating within render/combat range.
- Players who are far away (e.g. in another biome or dungeon) or offline will not receive the flag automatically.

### 🛠️ Mod Behavior
- Gate of Ore-thority checks each player’s m_customData for the required boss flag before allowing teleportation with gated items.
- If a player did not receive the flag due to distance or timing, they will be blocked from teleporting with those items until the flag is granted manually or via another mod.

## 🔐 Server Override

If EnableServerOverride is enabled in the config, clients will use the server-defined boss flags for teleport restrictions. This ensures consistent enforcement across multiplayer sessions.

## 🔄 Server Settings Sync

Gate of Ore-thority supports server-enforced teleport restrictions to ensure consistent gameplay across multiplayer sessions.
### 🧩 How It Works
- When EnableServerOverride = true on the server, all connected clients will receive the server’s item-to-boss flag mappings.
- These mappings are sent via custom RPC when a client connects.
- Clients will override their local config values with the server’s enforced settings.
- This ensures that teleport restrictions are consistent, even if players have modified their local configs.
### 📦 Synced Data
The following data is synced from server to client:
- Item prefab names (e.g. CopperOre, BlackMetalScrap)
- Required boss flags (e.g. defeated_elder, defeated_yagluth)
### 🔐 Enforcement Logic
- On the client, teleport checks use the server-synced values if available.
- If no server sync is received, the client falls back to local config values.
- This behavior is controlled by the EnableServerOverride flag.
### 🛠️ Config Flag
[General]
EnableServerOverride = true

Set this to true on the server to activate config sync. Clients do not need to set this manually—it will be respected automatically when the server sends its settings.

## 📦 Installation

Requires [BepInExPack Valheim](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/).  
Place the `GateOfOre-thority.dll` inside your `BepInEx/plugins` folder.

## 🧩 Compatibility

- Compatible with most mods that use `Player.m_customData` for progression tracking.
- Designed to be extensible for custom bosses and items.

## 🐾 Credits

Idea by **Stayk**  
Work by CoPilot
Special thanks to the Valheim modding community for prefab insights and biome lore inspiration.