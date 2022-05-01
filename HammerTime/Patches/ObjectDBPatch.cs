using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HammerTime.Patches {
    [HarmonyPatch]
    public static class ObjectDBPatch {
        private static readonly Dictionary<string, bool> CombineCategories = new Dictionary<string, bool>();
        private static readonly Dictionary<string, bool> DeactivatedHammers = new Dictionary<string, bool>();

        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
        public static void ObjectDBAwake(ObjectDB __instance) {
            if (SceneManager.GetActiveScene().name != "main") {
                return;
            }

            Dictionary<string, PieceTable> pieceTables = PrefabManager.Cache.GetPrefabs(typeof(PieceTable)).ToDictionary(k => k.Key, v => (PieceTable)v.Value);

            foreach (KeyValuePair<string, PieceTable> table in pieceTables) {
                if (table.Key == "_HammerPieceTable" || table.Key == "_HoePieceTable" || table.Key == "_CultivatorPieceTable") {
                    continue;
                }

                List<string> customCategories = PieceManager.Instance.GetPieceCategories();
                Dictionary<int, string> customCategoriesMap = customCategories.ToDictionary(i => 4 + customCategories.IndexOf(i), i => i);

                for (int index = 0; index < (int)Piece.PieceCategory.Max; index++) {
                    customCategoriesMap[index] = ((Piece.PieceCategory)index).ToString();
                }

                foreach (GameObject pieceGameObject in table.Value.m_pieces) {
                    Piece piece;
                    string tabName;
                    string category;
                    bool combine;
                    bool disabled;
                    CustomPiece customPiece = PieceManager.Instance.GetPiece(pieceGameObject.name);

                    if (customPiece != null) {
                        piece = customPiece.Piece;
                        tabName = customPiece.SourceMod.Name;
                        combine = CombineModCategories(table.Key, customPiece.SourceMod.Name);
                        disabled = IsHammerDeactivated(table.Key, customPiece.SourceMod.Name);
                    } else {
                        piece = pieceGameObject.GetComponent<Piece>();
                        tabName = CleanTableName(table.Key);
                        combine = CombineModCategories(table.Key, "Unrecognized Mod");
                        disabled = IsHammerDeactivated(table.Key, "Unrecognized Mod");
                    }

                    if (disabled) {
                        break;
                    }

                    int categoryId = (int)piece.m_category;

                    if (piece.m_category == Piece.PieceCategory.All) {
                        category = "All";

                        if (pieceTables["_HammerPieceTable"].m_pieces.Any(i => i.name == pieceGameObject.name)) {
                            continue;
                        }
                    } else {
                        if (customPiece != null) {
                            if (!combine && customCategoriesMap.ContainsKey(categoryId)) {
                                category = $"{tabName} {customCategoriesMap[categoryId]}";
                            } else {
                                category = tabName;
                            }
                        } else {
                            if (!combine && Enum.IsDefined(typeof(Piece.PieceCategory), categoryId)) {
                                category = $"{tabName} {((Piece.PieceCategory)categoryId).ToString()}";
                            } else {
                                category = $"{tabName}";
                            }
                        }
                    }

                    piece.m_category = PieceManager.Instance.AddPieceCategory("_HammerPieceTable", category);
                    pieceTables["_HammerPieceTable"].m_pieces.Add(pieceGameObject);
                }
            }
        }

        private static string CleanTableName(string tableName) {
            return tableName.Replace("PieceTable", "").Replace("HammerTable", "").Replace("_", "");
        }

        private static bool CombineModCategories(string pieceTable, string modName) {
            if (!CombineCategories.ContainsKey(pieceTable)) {
                string description = $"{modName}. Combines all categories from this custom hammer into one category. Requires a relog to take effect";
                ConfigEntry<bool> combine = Plugin.Instance.Config.Bind("Combine Mod Categories", $"Combine Categories of {pieceTable}", false, description);
                CombineCategories.Add(pieceTable, combine.Value);

                combine.SettingChanged += (sender, args) => { CombineCategories[pieceTable] = combine.Value; };
            }

            return CombineCategories[pieceTable];
        }

        private static bool IsHammerDeactivated(string pieceTable, string modName) {
            // Disable PlanBuild & Rune Magic by default
            bool defaultDisabled = modName != "PlanBuild" && pieceTable != "_RuneFocusPieceTable";

            if (!DeactivatedHammers.ContainsKey(pieceTable)) {
                string description = $"{modName}. Disables moving pieces from this custom hammer into one the vanilla hammer. Requires a relog to take effect";
                ConfigEntry<bool> combine = Plugin.Instance.Config.Bind("Disable PieceTable", $"Disable PieceTable of {pieceTable}", !defaultDisabled, description);
                DeactivatedHammers.Add(pieceTable, combine.Value);

                combine.SettingChanged += (sender, args) => { DeactivatedHammers[pieceTable] = combine.Value; };
            }

            return DeactivatedHammers[pieceTable];
        }
    }
}
