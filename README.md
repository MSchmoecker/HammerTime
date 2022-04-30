# HammerTime

## About
Moves all pieces from custom hammers to the vanilla hammer.

![hammertime](https://raw.githubusercontent.com/MSchmoecker/HammerTime/master/Docs/HammerTimePreview.gif)

It can be configured if all categories from a custom hammers should be combined into one.
This option is automatically generated after the first world loading.
If other mods use Jotunn they can be toggled individually.

It is not possible to have the same pieces into different hammers with different categories.
That means the original hammers will be empty. 

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
