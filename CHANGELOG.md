# Changelog

## 1.0.3 - Dungeon logic fix
- Thanks to DaCylops for submitting a bug fix stopping players from accessing dungeons with a blocked material

## 1.0.2 - README Update
- Added information to readme

## 1.0.1 — Bugfix Release
- Fixed teleport blocker config sync casting error
- Improved server-side enforcement logic
- Cleaned up logging and prefab matching

## 1.0.0 — Initial Release

- Added teleport gating based on boss progression keys
- Integrated custom flavor text for blocked items
- Console commands for key management:
  - `ore_grantkey`, `ore_revokekey`, `ore_listkeys`, `ore_grantallkeys`, `ore_resetkeys`
- Support for legacy and new item variants (e.g. FlametalNew, BronzeScrap)
- Fixed prefab name mismatches for Eikthyr, SeekerQueen, DvergrNeedle
- Centralized key list for safe and scoped resets
- Multiplayer-safe messaging and logging