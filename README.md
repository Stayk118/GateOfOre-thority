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