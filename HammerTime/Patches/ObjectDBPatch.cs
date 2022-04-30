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

                foreach (GameObject pieceGameObject in table.Value.m_pieces) {
                    Piece piece;
                    string modName;
                    string category;

                    CustomPiece customPiece = PieceManager.Instance.GetPiece(pieceGameObject.name);

                    if (customPiece != null) {
                        piece = customPiece.Piece;
                        modName = customPiece.SourceMod.Name;
                    } else {
                        piece = pieceGameObject.GetComponent<Piece>();
                        modName = "Mods";
                    }

                    bool combine = CombineModCategories(modName);
                    int categoryId = (int)piece.m_category;

                    if (customPiece != null) {
                        if (!combine && !Enum.IsDefined(typeof(Piece.PieceCategory), categoryId) && customCategoriesMap.ContainsKey(categoryId)) {
                            category = $"{modName} {customCategoriesMap[categoryId]}";
                        } else {
                            category = modName;
                        }
                    } else {
                        if (!combine && Enum.IsDefined(typeof(Piece.PieceCategory), categoryId)) {
                            if (piece.m_category == Piece.PieceCategory.All) {
                                category = "All";
                            } else {
                                category = $"Mods {((Piece.PieceCategory)categoryId).ToString()}";
                            }
                        } else {
                            category = "Mods";
                        }
                    }

                    if (piece.m_category == Piece.PieceCategory.All && pieceTables["_HammerPieceTable"].m_pieces.Any(i => i.name == pieceGameObject.name)) {
                        continue;
                    }

                    PieceManager.Instance.RegisterPieceInPieceTable(pieceGameObject, "_HammerPieceTable", category);
                }
            }
        }

        private static bool CombineModCategories(string modName) {
            if (!CombineCategories.ContainsKey(modName)) {
                const string description = "Combines all categories from a custom hammer into one mod category";
                ConfigEntry<bool> combine = Plugin.Instance.Config.Bind("Combine Mod Categories", $"Combine Categories of {modName}", false, description);
                CombineCategories.Add(modName, combine.Value);

                combine.SettingChanged += (sender, args) => { CombineCategories[modName] = combine.Value; };
            }

            return CombineCategories[modName];
        }
    }
}
