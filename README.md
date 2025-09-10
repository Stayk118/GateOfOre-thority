# Gate of Ore-thority

**Boss-gated teleport restrictions with rich flavor text and full config flexibility.**  
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

## 🔮 Supported Items

Includes support for:
- Copper, Tin, Bronze
- Iron, Silver, Black Metal
- Flametal (and FlametalNew variants)
- Mistlands items like BronzeScrap, DvergrNeedle
- Custom items like CharredCogwheel

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
