using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Jotunn;

namespace HammerTime {
    public static class Config {
        private static readonly Dictionary<string, bool> CombineCategories = new Dictionary<string, bool>();
        private static readonly Dictionary<string, bool> DeactivatedHammers = new Dictionary<string, bool>();

        public static ConfigEntry<bool> disableRecipes;

        public static void InitBaseConfig() {
            const string description = "Disables hammer recipes that are moved to the vanilla hammer. " +
                                       "This only deactivates the recipes, existing items will not be removed. " +
                                       "Can be changed at runtime. ";

            disableRecipes = Plugin.Instance.Config.Bind("General", "Disable Hammer Recipes", true, description.Trim());
            disableRecipes.SettingChanged += (sender, args) => {
                Plugin.UpdateDisabledRecipes();
            };
        }

        public static bool CombineModCategories(string pieceTable, string modName, Action settingChanged) {
            if (!CombineCategories.ContainsKey(pieceTable)) {
                string description = $"{modName}. Combines all categories from this custom hammer into one category. Can be changed at runtime.";
                ConfigEntry<bool> combine = Plugin.Instance.Config.Bind("Combine Mod Categories", $"Combine Categories of {pieceTable}", false, description);
                CombineCategories.Add(pieceTable, combine.Value);

                combine.SettingChanged += (sender, args) => {
                    CombineCategories[pieceTable] = combine.Value;
                    settingChanged?.Invoke();
                };
            }

            return CombineCategories[pieceTable];
        }

        public static bool IsHammerDeactivated(string pieceTable, string modName, Action settingChanged) {
            if (!DeactivatedHammers.ContainsKey(pieceTable)) {
                // Disable PlanBuild & Rune Magic by default
                bool defaultDisabled = modName != "PlanBuild" && pieceTable != "_RuneFocusPieceTable";

                string description = $"{modName}. Disables moving pieces from this custom hammer into the vanilla hammer. Can be changed at runtime.";
                ConfigEntry<bool> combine = Plugin.Instance.Config.Bind("Disable PieceTable", $"Disable PieceTable of {pieceTable}", !defaultDisabled, description);
                DeactivatedHammers.Add(pieceTable, combine.Value);

                combine.SettingChanged += (sender, args) => {
                    DeactivatedHammers[pieceTable] = combine.Value;
                    settingChanged?.Invoke();
                };
            }

            return DeactivatedHammers[pieceTable];
        }

        public static bool IsHammerDeactivated(string pieceTable) {
            if (DeactivatedHammers.TryGetValue(pieceTable, out bool deactivated)) {
                return deactivated;
            }

            Logger.LogWarning($"Config 'Disable PieceTable' for {pieceTable} not found");
            return false;
        }
    }
}
