using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using Jotunn;
using Jotunn.Managers;

namespace HammerTime {
    public static class Config {
        public static ConfigEntry<bool> disableRecipes;

        private static readonly Dictionary<string, bool> EnabledHammers = new Dictionary<string, bool>();
        private static readonly Dictionary<string, bool> CombineCategories = new Dictionary<string, bool>();
        private static readonly Dictionary<string, string> CategoryNames = new Dictionary<string, string>();

        private const string CategoryDescription = "A new category is getting created if it is not a vanilla value. " +
                                                   "If two categories have the same name the pieces will be combined, even from different hammers. " +
                                                   "Can be changed at runtime. ";

        // matches all '=', '\n', '\t', '\\', '"', '\'', '[', ']'
        private static Regex invalidConfigCharsRegex = new Regex($"[=\n\t\\\\\"\'\\[\\]]*");

        public static void InitBaseConfig() {
            const string section = "1 - General";
            const string key = "Disable Hammer Recipes";
            const string description = "Disables crafting recipes of custom hammers that are enabled in this mod. " +
                                       "Only deactivates the recipes, existing items will not be removed. " +
                                       "Can be changed at runtime. ";

            disableRecipes = Plugin.Instance.Config.Bind(section, key, true, description.Trim());
            disableRecipes.SettingChanged += (sender, args) => Plugin.UpdateDisabledRecipes();
        }

        public static bool CombineModCategories(string pieceTable, string modName, bool combineByDefault, Action settingChanged) {
            string cacheKey = $"{pieceTable}_{modName}";

            if (!CombineCategories.ContainsKey(cacheKey)) {
                string section = CleanKeySection($"{modName} {pieceTable}");
                const string key = "Combine Categories";
                const string description = "Combines all categories from this custom hammer into one category. " +
                                           "Can be changed at runtime. ";

                ConfigurationManagerAttributes attributes = new ConfigurationManagerAttributes {
                    Order = 4
                };

                ConfigEntry<bool> combine = Plugin.Instance.Config.Bind(section, key, combineByDefault, new ConfigDescription(description.Trim(), null, attributes));
                CombineCategories.Add(cacheKey, combine.Value);

                combine.SettingChanged += (sender, args) => {
                    CombineCategories[cacheKey] = combine.Value;
                    settingChanged?.Invoke();
                };
            }

            return CombineCategories[cacheKey];
        }

        public static bool IsHammerEnabled(string pieceTable, string modName, Action settingChanged) {
            string cacheKey = $"{pieceTable}_{modName}";

            if (!EnabledHammers.ContainsKey(cacheKey)) {
                bool defaultDisabled = modName == "PlanBuild" || pieceTable == "_RuneFocusPieceTable" || Helper.IsVanillaPieceTable(pieceTable);

                string section = CleanKeySection($"{modName} {pieceTable}");
                const string key = "Enable Hammer";
                const string description = "Enables moving pieces from this custom hammer into the vanilla hammer. " +
                                           "Can be changed at runtime. ";

                ConfigurationManagerAttributes attributes = new ConfigurationManagerAttributes {
                    Order = 5
                };

                ConfigEntry<bool> enabled = Plugin.Instance.Config.Bind(section, key, !defaultDisabled, new ConfigDescription(description.Trim(), null, attributes));
                EnabledHammers.Add(cacheKey, enabled.Value);

                enabled.SettingChanged += (sender, args) => {
                    EnabledHammers[cacheKey] = enabled.Value;
                    settingChanged?.Invoke();
                };
            }

            return EnabledHammers[cacheKey];
        }

        public static bool IsHammerEnabled(string pieceTable) {
            if (Helper.IsVanillaPieceTable(pieceTable)) {
                return true;
            }

            foreach (KeyValuePair<string, bool> hammer in EnabledHammers) {
                if (hammer.Key.StartsWith(pieceTable)) {
                    return hammer.Value;
                }
            }

            Logger.LogWarning($"Config 'Enable Hammer' for {pieceTable} not found");
            return false;
        }

        public static string GetCategoryName(string pieceTable, string modName, string originalCategory, Action settingChanged) {
            if (originalCategory == "All") {
                return "All";
            }

            string cacheKey = $"Single_{pieceTable}_{modName}_{originalCategory}";

            if (!CategoryNames.ContainsKey(cacheKey)) {
                string section = CleanKeySection($"{modName} {pieceTable}");
                string key = CleanKeySection($"Category Name {originalCategory}");
                string category = $"{modName} {originalCategory}";
                string description = $"Used category name if categories are not combined. {CategoryDescription}".Trim();

                if (modName == "Vanilla") {
                    category = originalCategory;
                }

                ConfigEntry<string> entry = Plugin.Instance.Config.Bind(section, key, category, description);
                InitCategoryName(cacheKey, entry, settingChanged);
            }

            return CategoryNames[cacheKey];
        }

        public static string GetCombinedCategoryName(string pieceTable, string modName, Action settingChanged) {
            string cacheKey = $"Combined_{pieceTable}_{modName}";

            if (!CategoryNames.ContainsKey(cacheKey)) {
                string section = CleanKeySection($"{modName} {pieceTable}");
                string key = $"Combined Category Name";

                ConfigurationManagerAttributes attributes = new ConfigurationManagerAttributes {
                    Order = 3
                };
                ConfigDescription description = new ConfigDescription($"Used category name if categories are combined. {CategoryDescription}".Trim(), null, attributes);

                ConfigEntry<string> entry = Plugin.Instance.Config.Bind(section, key, $"{modName}", description);
                InitCategoryName(cacheKey, entry, settingChanged);
            }

            return CategoryNames[cacheKey];
        }

        private static void InitCategoryName(string key, ConfigEntry<string> entry, Action settingChanged) {
            entry.SettingChanged += (sender, args) => {
                string oldValue = CategoryNames[key];
                CategoryNames[key] = entry.Value;

                PieceManager.Instance.RemovePieceCategory("_HammerPieceTable", oldValue);
                settingChanged?.Invoke();
            };

            CategoryNames[key] = entry.Value;
        }

        private static string CleanKeySection(string section) {
            return invalidConfigCharsRegex.Replace(section, "").Trim();
        }
    }
}
