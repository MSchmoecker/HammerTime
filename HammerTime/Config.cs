using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Jotunn;

namespace HammerTime {
    public static class Config {
        private static readonly Dictionary<string, bool> CombineCategories = new Dictionary<string, bool>();
        private static readonly Dictionary<string, bool> EnabledHammers = new Dictionary<string, bool>();

        public static ConfigEntry<bool> disableRecipes;

        public static void InitBaseConfig() {
            const string section = "1 - General";
            const string key = "Disable Hammer Recipes";
            const string description = "Disables crafting recipes of custom hammers that are enabled in this mod. " +
                                       "Only deactivates the recipes, existing items will not be removed. " +
                                       "Can be changed at runtime. ";

            disableRecipes = Plugin.Instance.Config.Bind(section, key, true, description.Trim());
            disableRecipes.SettingChanged += (sender, args) => Plugin.UpdateDisabledRecipes();
        }

        public static bool CombineModCategories(string pieceTable, string modName, Action settingChanged) {
            if (!CombineCategories.ContainsKey(pieceTable)) {
                const string section = "3 - Combine Hammer Categories";
                string key = $"Combine {pieceTable}";
                string description = $"{modName}. " +
                                     $"Combines all categories from this custom hammer into one category. " +
                                     $"Can be changed at runtime. ";

                ConfigEntry<bool> combine = Plugin.Instance.Config.Bind(section, key, false, description.Trim());
                CombineCategories.Add(pieceTable, combine.Value);

                combine.SettingChanged += (sender, args) => {
                    CombineCategories[pieceTable] = combine.Value;
                    settingChanged?.Invoke();
                };
            }

            return CombineCategories[pieceTable];
        }

        public static bool IsHammerEnabled(string pieceTable, string modName, Action settingChanged) {
            if (!EnabledHammers.ContainsKey(pieceTable)) {
                bool defaultDisabled = modName == "PlanBuild" || pieceTable == "_RuneFocusPieceTable";

                const string section = "2 - Enable Hammer";
                string key = $"Enable {pieceTable}";
                string description = $"{modName}. " +
                                     $"Enables moving pieces from this custom hammer into the vanilla hammer. " +
                                     $"Can be changed at runtime. ";

                ConfigEntry<bool> enabled = Plugin.Instance.Config.Bind(section, key, !defaultDisabled, description.Trim());
                EnabledHammers.Add(pieceTable, enabled.Value);

                enabled.SettingChanged += (sender, args) => {
                    EnabledHammers[pieceTable] = enabled.Value;
                    settingChanged?.Invoke();
                };
            }

            return EnabledHammers[pieceTable];
        }

        public static bool IsHammerEnabled(string pieceTable) {
            if (EnabledHammers.TryGetValue(pieceTable, out bool enabled)) {
                return enabled;
            }

            Logger.LogWarning($"Config 'Enable Hammer' for {pieceTable} not found");
            return false;
        }
    }
}
