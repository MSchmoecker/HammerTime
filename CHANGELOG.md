## Changelog

0.3.10
- Fixed vanilla serving tray not being excluded from HammerTime

0.3.9
- Removed the process filter that prevented the mod from loading on linux devices

0.3.8
- Fixed relogging without a restart caused tabs to not show up

0.3.7
- Fixed an compatibility error with Wacky's Database 2.0 update. Ingame reload of Wacky's Database is no longer supported in HammerTime
- Fixed an error if a piece on a hammer has no piece script attached

0.3.6
- Fixed an error if a mod uses special characters in a piece table name which aren't allowed in a BepInEx config section

0.3.5
- Updated for Jotunn 2.12.0 because internal category ids are now different and caused errors
- Added debug files (HammerTime.dll.mdb) to the release, this will make finding future issues easier
- Fixed errors if a piece category has an empty name

0.3.4
- Fixed an incompatibility with Auga, requires Auga 1.2.8 and Jotunn 2.11.4 or newer

0.3.3
- Fixed unused categories were not disabled if a disabled hammer uses the same category name
- Fixed an error that occured if a piece table has a null entry

0.3.2
- Fixed that a category could be disabled if two hammers use the same category name and one of them is disabled
- Improved startup time with many mods

0.3.1
- Added support for [CookieMilk](https://valheim.thunderstore.io/package/CookieMilk/) pieces
- Automatically enabled Combine Categories of a mod with less then 60 pieces, this only applies to newly generated configs

0.3.0
- Reworked the config again to allow new features, the old config should be deleted and regenerated
- Added configuration of custom category names to organize your hammer individually
- Added configuration of mods that use the vanilla hammer
- Improved compatibility with WackysDB, HammerTime now reloads after WackysDB to move pieces into the correct hammer/category

0.2.1
- Fixed an error that could occur with some modded items, which caused the recipe disabling to not work properly

0.2.0
- Added option to disable crafting recipes of hammer that are active in this mod. Enabled by default
- Mod names for every mod are now are shown in the settings
- Changed config options to be more clear.
  The old config values will not apply and new config options will be generated.
  It is advised to delete the old config to reduce clutter

0.1.4
- Fixed conflict with ChickenBoo

0.1.3
- Added Auga Hud compatibility
- Tabs are cleaned up on config change and don't need a reload anymore
- Fixed plan pieces in PlanBuild were not available
- Improved performance of moving pieces to the hammers

0.1.2
- Fixed a rare bug that caused an index error. This resulted in the player not being able to spawn
- Fixed tab names were doubled when relogging within the same game session
- Config options can now be changed at runtime, although old tabs are not cleaned up until restart

0.1.1
- Added option to disable moving pieces for every custom hammer individually
- Unified category names and combining for Jotunn and Non-Jotunn mods
- Changed config options to refer to pieceTable names instead of mod names.
  This mean old config files are not used anymore and have to be reconfigured.

0.1.0
- Release
