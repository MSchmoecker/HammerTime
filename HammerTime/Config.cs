using System;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace HammerTime {
    public static class Config {
        private static readonly Dictionary<string, bool> CombineCategories = new Dictionary<string, bool>();
        private static readonly Dictionary<string, bool> DeactivatedHammers = new Dictionary<string, bool>();

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

                string description = $"{modName}. Disables moving pieces from this custom hammer into one the vanilla hammer. Can be changed at runtime.";
                ConfigEntry<bool> combine = Plugin.Instance.Config.Bind("Disable PieceTable", $"Disable PieceTable of {pieceTable}", !defaultDisabled, description);
                DeactivatedHammers.Add(pieceTable, combine.Value);

                combine.SettingChanged += (sender, args) => {
                    DeactivatedHammers[pieceTable] = combine.Value;
                    settingChanged?.Invoke();
                };
            }

            return DeactivatedHammers[pieceTable];
        }
    }
}
