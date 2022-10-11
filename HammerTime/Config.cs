using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Jotunn;
using Jotunn.Managers;

namespace HammerTime {
    public static class Config {
        public static ConfigEntry<bool> disableRecipes;

        private static readonly Dictionary<string, bool> CombineCategories = new Dictionary<string, bool>();
        private static readonly Dictionary<string, bool> EnabledHammers = new Dictionary<string, bool>();
        private static readonly Dictionary<string, ConfigEntry<string>> CategoryNames = new Dictionary<string, ConfigEntry<string>>();

        private const string CategoryDescription = "A new category is getting created if it is not a vanilla value. " +
                                                   "If two categories have the same name the pieces will be combined, even from different hammers. " +
                                                   "Can be changed at runtime. ";

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
                string key = $"Combine {pieceTable}";
                const string description = "Combines all categories from this custom hammer into one category. " +
                                           "Can be changed at runtime. ";

                ConfigurationManagerAttributes attributes = new ConfigurationManagerAttributes {
                    Order = 4
                };

                ConfigEntry<bool> combine = Plugin.Instance.Config.Bind($"{modName} {pieceTable}", key, false, new ConfigDescription(description.Trim(), null, attributes));
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

                string key = $"Enable {pieceTable}";
                const string description = "Enables moving pieces from this custom hammer into the vanilla hammer. " +
                                           "Can be changed at runtime. ";

                ConfigurationManagerAttributes attributes = new ConfigurationManagerAttributes {
                    Order = 5
                };

                ConfigEntry<bool> enabled = Plugin.Instance.Config.Bind($"{modName} {pieceTable}", key, !defaultDisabled, new ConfigDescription(description.Trim(), null, attributes));
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

        public static string GetCategoryName(string pieceTable, string modName, Piece.PieceCategory originalCategory, Action settingChanged) {
            if (originalCategory == Piece.PieceCategory.All) {
                return "All";
            }

            string cacheKey = $"Single_{pieceTable}_{originalCategory}";

            if (!CategoryNames.ContainsKey(cacheKey)) {
                string key = $"Category Name {Plugin.categoryIdToName[(int)originalCategory]}";
                string category = $"{modName} {Plugin.categoryIdToName[(int)originalCategory]}";
                string description = $"Used category name if categories are not combined. {CategoryDescription}";

                ConfigEntry<string> entry = Plugin.Instance.Config.Bind($"{modName} {pieceTable}".Trim(), key, category, description);
                InitCategoryName(cacheKey, entry, settingChanged);
            }

            return CategoryNames[cacheKey].Value;
        }

        public static string GetCombinedCategoryName(string pieceTable, string modName, Action settingChanged) {
            string cacheKey = $"Combined_{pieceTable}";

            if (!CategoryNames.ContainsKey(cacheKey)) {
                string key = $"Combined Category Name";

                ConfigurationManagerAttributes attributes = new ConfigurationManagerAttributes {
                    Order = 3
                };
                ConfigDescription description = new ConfigDescription($"Used category name if categories are combined. {CategoryDescription}".Trim(), null, attributes);

                ConfigEntry<string> entry = Plugin.Instance.Config.Bind($"{modName} {pieceTable}", key, $"{modName}", description);
                InitCategoryName(cacheKey, entry, settingChanged);
            }

            return CategoryNames[cacheKey].Value;
        }

        private static void InitCategoryName(string key, ConfigEntry<string> entry, Action settingChanged) {
            entry.SettingChanged += (sender, args) => {
                string oldValue = CategoryNames[key].Value;
                string newValue = entry.Value;
                CategoryNames[key].Value = newValue;

                PieceManager.Instance.RemovePieceCategory("_HammerPieceTable", oldValue);
                settingChanged?.Invoke();
            };

            CategoryNames[key] = entry;
        }
    }
}
