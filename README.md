# HammerTime

## About
Moves all pieces from custom hammers to the vanilla hammer.

![hammertime](https://raw.githubusercontent.com/MSchmoecker/HammerTime/master/Docs/HammerTimePreview.gif)

It is not possible to have the same pieces into different hammers with different categories.
That means the original hammers will be empty.


### Config
All config options are generated automatically after the first world loading for every custom hammer individually.\
Following Options are available:
- Combine Mod Categories: Combines all categories from this custom hammer into one category
- Disable PieceTable: Disables moving pieces from this custom hammer into one the vanilla hammer


## Installation
This mod requires BepInEx and Jötunn.
Extract all content of `HammerTime` into the `BepInEx/plugins` folder.

This is a client side only mod and does not execute on a server.


## Development
BepInEx must be setup at manual or with r2modman/Thunderstore Mod Manager.
Jötunn must be installed.

Create a file called `Environment.props` inside the project root.
Copy the example and change the Valheim install path to your location.
If you use r2modman/Tunderstore Mod Manager you can set the path too, but this is optional.


```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <PropertyGroup>
        <!-- Needs to be your path to the base Valheim folder -->
        <VALHEIM_INSTALL>E:\Programme\Steam\steamapps\common\Valheim</VALHEIM_INSTALL>
        <!-- Optional, needs to be the path to a r2modmanPlus profile folder -->
        <R2MODMAN_INSTALL>C:\Users\<user>\AppData\Roaming\r2modmanPlus-local\Valheim\profiles\Develop</R2MODMAN_INSTALL>
        <USE_R2MODMAN_AS_DEPLOY_FOLDER>false</USE_R2MODMAN_AS_DEPLOY_FOLDER>
    </PropertyGroup>
</Project>
```

## Links
- Thunderstore: https://valheim.thunderstore.io/package/MSchmoecker/HammerTime/
- Github: https://github.com/MSchmoecker/HammerTime
- Discord: Margmas#9562


## Changelog
0.1.1
- Added option to disable moving pieces for every custom hammer individually
- Unified category names and combining for Jotunn and Non-Jotunn mods
- Changed config options to refer to pieceTable names instead of mod names.
  This mean old config files are not used anymore and have to be reconfigured.

0.1.0
- Release
