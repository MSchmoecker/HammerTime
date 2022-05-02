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
        private static Dictionary<string, PieceTable> pieceTables;
        private static Dictionary<int, string> categoryIdToName;
        private static Dictionary<string, List<PieceItem>> pieces;

        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
        public static void ObjectDBAwake(ObjectDB __instance) {
            if (SceneManager.GetActiveScene().name != "main") {
                return;
            }

            GetPieceTables();
            GetCategories();
            pieces = new Dictionary<string, List<PieceItem>>();

            foreach (KeyValuePair<string, PieceTable> table in pieceTables) {
                if (table.Key == "_HammerPieceTable" || table.Key == "_HoePieceTable" || table.Key == "_CultivatorPieceTable") {
                    continue;
                }

                pieces.Add(table.Key, new List<PieceItem>());

                foreach (GameObject pieceGameObject in table.Value.m_pieces) {
                    CustomPiece customPiece = PieceManager.Instance.GetPiece(pieceGameObject.name);

                    if (customPiece != null) {
                        string mod = customPiece.SourceMod.Name;
                        pieces[table.Key].Add(new PieceItem(pieceGameObject, customPiece.Piece, mod, mod));
                    } else {
                        Piece piece = pieceGameObject.GetComponent<Piece>();
                        pieces[table.Key].Add(new PieceItem(pieceGameObject, piece, "Unrecognized Mod", CleanTableName(table.Key)));
                    }
                }
            }

            foreach (string pieceTable in pieces.Keys) {
                UpdatePieceTable(pieceTable);
            }
        }

        private static void GetCategories() {
            List<string> customCategories = PieceManager.Instance.GetPieceCategories();
            categoryIdToName = customCategories.ToDictionary(i => 4 + customCategories.IndexOf(i), i => i);

            for (int index = 0; index < (int)Piece.PieceCategory.Max; index++) {
                categoryIdToName[index] = ((Piece.PieceCategory)index).ToString();
            }
        }

        private static void GetPieceTables() {
            pieceTables = PrefabManager.Cache.GetPrefabs(typeof(PieceTable)).ToDictionary(k => k.Key, v => (PieceTable)v.Value);
        }

        private static void UpdatePieceTable(string pieceTable) {
            List<PieceItem> pieceMap = pieces[pieceTable];

            if (pieceMap.Count == 0) {
                return;
            }

            bool combine = CombineModCategories(pieceTable, pieceMap[0].modName);
            bool disabled = IsHammerDeactivated(pieceTable, "Unrecognized Mod");

            if (disabled) {
                return;
            }

            foreach (PieceItem pieceItem in pieceMap) {
                string category = GetCategory(pieceItem, combine);
                pieceItem.piece.m_category = PieceManager.Instance.AddPieceCategory("_HammerPieceTable", category);

                if (pieceTables[pieceTable].m_pieces.Contains(pieceItem.gameObject)) {
                    pieceTables[pieceTable].m_pieces.Remove(pieceItem.gameObject);
                }

                if (pieceTables["_HammerPieceTable"].m_pieces.All(i => i.name != pieceItem.gameObject.name)) {
                    pieceTables["_HammerPieceTable"].m_pieces.Add(pieceItem.gameObject);
                }
            }
        }

        private static string GetCategory(PieceItem pieceItem, bool combine) {
            if (pieceItem.piece.m_category == Piece.PieceCategory.All) {
                return "All";
            }

            if (!combine && categoryIdToName.ContainsKey((int)pieceItem.originalCategory)) {
                return $"{pieceItem.tabName} {categoryIdToName[(int)pieceItem.originalCategory]}";
            }

            return pieceItem.tabName;
        }

        private struct PieceItem {
            public GameObject gameObject;
            public Piece piece;
            public string modName;
            public string tabName;
            public Piece.PieceCategory originalCategory;

            public PieceItem(GameObject gameObject, Piece piece, string modName, string tabName) {
                this.gameObject = gameObject;
                this.piece = piece;
                originalCategory = piece.m_category;
                this.modName = modName;
                this.tabName = tabName;
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

                combine.SettingChanged += (sender, args) => {
                    CombineCategories[pieceTable] = combine.Value;
                    UpdatePieceTable(pieceTable);
                };
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

                combine.SettingChanged += (sender, args) => {
                    DeactivatedHammers[pieceTable] = combine.Value;
                    UpdatePieceTable(pieceTable);
                };
            }

            return DeactivatedHammers[pieceTable];
        }
    }
}
