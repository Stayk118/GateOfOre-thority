# Gate of Ore-thority

**Boss-gated teleport restrictions with lore themed flavour text and server sync capabilities.**  
**Disclaimer** this was vibe-coded using Copilot & Claude, I know a lot of people don't want to use AI content.
However, I don't program and this mod has been a bit of a learning experience while also getting some settings that my group really wanted.

## 🧙 Features

- 🔐 **Teleport Restrictions**: Prevents teleporting with specific items until the corresponding boss is defeated.
- 📜 **Flavorful Feedback**: Custom messages for each item, themed to its biome and boss.
- 🧪 **Console Commands**: Grant, revoke, list, or reset progression keys for testing and admin control.
- 🧠 **Multiplayer-Aware**: Works in both solo and multiplayer environments.

## 🌍 World Modifier Requirement: Casual Portals

Gate of Ore-thority assumes that your world uses the Casual Portals modifier, which allows players to teleport with ores and other heavy items.

## 🛠 Console Commands

| Command                     | Description                                      |
|-----------------------------|--------------------------------------------------|
| `gateoforethority_addkey`   | Grants a progression key to the local player     |
| `gateoforethority_removekey`| Removes a progression key from the local player  |
| `gateoforethority_keys`     | Lists all progression keys the player has        |

### 🗝️ Teleport Restriction Keys
Gate of Ore-thority uses boss progression flags to restrict teleporting with specific items. These flags are stored in each player's custom data and are typically granted upon defeating the corresponding boss.
| Item Name                 | Required Boss Flag       | Notes                      | 
|-------------------------- |--------------------------|----------------------------| 
| CopperOre                 | defeated_gdking          |                            | 
| TinOre                    | defeated_gdking          |                            | 
| Copper                    | defeated_gdking          |                            | 
| Tin                       | defeated_gdking          |                            | 
| Bronze                    | defeated_gdking          |                            | 
| IronScrap                 | defeated_bonemass        |                            | 
| Iron                      | defeated_bonemass        |                            | 
| Ironpit                   | defeated_bonemass        |                            |
| IronOre                   | defeated_bonemass        |                            | 
| SilverOre                 | defeated_moder           |                            | 
| Silver                    | defeated_moder           |                            | 
| BlackMetalScrap           | defeated_yagluth         |                            | 
| BlackMetal                | defeated_yagluth         |                            | 
| MechanicalSpring          | defeated_queen           |                            | 
| DvergrNeedle              | defeated_queen           |                            |  
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
- Players within 50 meters of the boss at the moment of death are considered nearby.
- This includes anyone actively participating in the fight or spectating within render/combat range.
- Players who are far away (e.g. in another biome or dungeon) or offline will not receive the key.

### 🛠️ Mod Behavior
- Gate of Ore-thority checks each player’s m_customData for the required boss flag before allowing teleportation with gated items.
- If a player did not receive the flag due to distance or timing, they will be blocked from teleporting with those items until the flag is granted manually or via another mod.

## 🔐 Server Override

If Server authoritative is enabled in the config, clients will use the server-defined boss flags for teleport restrictions. This ensures consistent enforcement across multiplayer sessions.

## 📦 Installation

Requires [BepInExPack Valheim](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/).  
Place the `GateOfOrethority.dll` inside your `BepInEx/plugins` folder.

## 🧩 Compatibility

- Compatible with most mods that use `Player.m_customData` for progression tracking.
- Designed to be extensible for custom bosses and items.

## 🐾 Credits

Idea by **Stayk**  
Work by Claude & Copilot
Special thanks to the Valheim modding community for prefab insights and biome lore inspiration.