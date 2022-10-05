# HammerTime

## About
Moves all pieces from custom hammers to the vanilla hammer.

![hammertime](https://raw.githubusercontent.com/MSchmoecker/HammerTime/master/Docs/HammerTimePreview.gif)

It is not possible to have the same pieces into different hammers with different categories.
That means the original hammers will be empty.


### Config
All config options are generated automatically after the first world loading for every custom hammer individually.
They can be changed at runtime.

Following Options are available:
- Combine Mod Categories: Combines all categories from this custom hammer into one category
- Disable PieceTable: Disables moving pieces from this custom hammer into one the vanilla hammer


## Installation
This mod requires BepInEx and Jötunn.
Extract all content of `HammerTime` into the `BepInEx/plugins` folder.

This is a client side only mod and does not execute on a server.


## Development
See [contributing](https://github.com/MSchmoecker/HammerTime/blob/master/CONTRIBUTING.md).


## Links
- Thunderstore: https://valheim.thunderstore.io/package/MSchmoecker/HammerTime/
- Github: https://github.com/MSchmoecker/HammerTime
- Discord: Margmas#9562


## Changelog
0.1.4:
- Fixed conflict with ChickenBoo

0.1.3:
- Added Auga Hud compatibility
- Tabs are cleaned up on config change and don't need a reload anymore
- Fixed plan pieces in PlanBuild were not available
- Improved performance of moving pieces to the hammers

0.1.2:
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
