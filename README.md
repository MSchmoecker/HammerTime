# HammerTime


## About
Moves all pieces from custom hammers to the vanilla hammer.

![hammertime](https://raw.githubusercontent.com/MSchmoecker/HammerTime/master/Docs/HammerTimePreview.gif)

It is not possible to have the same pieces into different hammers with different categories.
That means the original hammers will be empty.

Vanilla tools (Hoe, Cultivator and Serving Tray) are excluded to not break the progression of the game.


## Settings
All config options can be changed at runtime with the BepInEx Configuration Manager or similar tools.

General:
- Disable Hammer Recipes: Disables crafting recipes of custom hammers that are enabled in this mod.
  Only deactivates the recipes, existing items will not be removed. Enabled by default.

Other config options are generated automatically after the first world loading for every custom hammer individually:
- Enable Hammer: Enables moving pieces from this hammer into the vanilla hammer
- Combine Categories: Combines all categories from this custom hammer into one category
- Combined Category Name: Custom category into which pieces are moved, if 'Combine Categories' is turned on
- Category Name 'Original Category Name': Custom category into which pieces from this original category are moved, if 'Combine Categories' is turned off


### Custom Categories
As mentioned above, it is possible to set custom category names from different source categories.
Using vanilla names (Misc, Crafting, Building, Furniture) places the pieces into these categories, while using a custom name creates a new category.
Using the same name for multiple categories groups the pieces together.

If no piece uses a vanilla category, it will be disabled automatically.


## Compatibility

WackysDatabase ingame reloading is not supported, the game must be restarted if categories are changed.
Also note that changing piece categories will create new config options (possibly in new segments) as they are grouped by mod and category name.


## Manual Installation
This mod requires BepInEx and Jötunn.
Extract all content of `HammerTime` into the `BepInEx/plugins` folder.

This is a client side only mod and does not execute on a server.


## Development
See [contributing](https://github.com/MSchmoecker/HammerTime/blob/master/CONTRIBUTING.md).


## Links
- [Thunderstore](https://valheim.thunderstore.io/package/MSchmoecker/HammerTime/)
- [Nexus](https://www.nexusmods.com/valheim/mods/1864)
- [Github](https://github.com/MSchmoecker/HammerTime)
- Discord: Margmas. Feel free to DM or ping me about feedback or questions, for example in the [Jötunn discord](https://discord.gg/DdUt6g7gyA)


## Changelog
See [changelog](https://github.com/MSchmoecker/HammerTime/blob/master/CHANGELOG.md).
