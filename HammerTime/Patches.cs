using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HammerTime {
    [HarmonyPatch]
    public static class Patches {
        private static Dictionary<string, PieceTable> pieceTables;
        public static Dictionary<int, string> categoryIdToName;
        private static Dictionary<string, List<PieceItem>> pieces;

        public static void IndexItems() {
            if (SceneManager.GetActiveScene().name != "main") {
                return;
            }

            pieceTables = Helper.GetPieceTables();
            categoryIdToName = Helper.GetCategories();
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
                        pieces[table.Key].Add(new PieceItem(pieceGameObject, piece, "Unrecognized Mod", Helper.CleanTableName(table.Key)));
                    }
                }
            }

            foreach (string pieceTable in pieces.Keys) {
                UpdatePieceTable(pieceTable);
            }
        }

        [HarmonyPatch(typeof(Game), nameof(Game.Logout)), HarmonyPostfix]
        public static void GameShutdown() {
            foreach (KeyValuePair<string, List<PieceItem>> pieces in pieces) {
                foreach (PieceItem pieceItem in pieces.Value) {
                    string category;

                    if (categoryIdToName.ContainsKey((int)pieceItem.originalCategory)) {
                        category = categoryIdToName[(int)pieceItem.originalCategory];
                    } else {
                        category = pieceItem.originalCategory.ToString();
                    }

                    MovePieceItemToTable(pieceItem, "_HammerPieceTable", pieces.Key, category);
                }
            }
        }

        private static void UpdatePieceTable(string pieceTable) {
            List<PieceItem> pieceMap = pieces[pieceTable];

            if (pieceMap.Count == 0) {
                return;
            }

            bool combine = Config.CombineModCategories(pieceTable, pieceMap[0].modName, () => UpdatePieceTable(pieceTable));
            bool disabled = Config.IsHammerDeactivated(pieceTable, pieceMap[0].modName, () => UpdatePieceTable(pieceTable));

            foreach (PieceItem pieceItem in pieceMap) {
                string category;
                string categoryUnCombined = Helper.GetCategory(pieceItem, false);
                string categoryCombined = Helper.GetCategory(pieceItem, true);

                if (disabled) {
                    PieceManager.Instance.RemovePieceCategory("_HammerPieceTable", categoryUnCombined);
                    PieceManager.Instance.RemovePieceCategory("_HammerPieceTable", categoryCombined);

                    category = categoryIdToName[(int)pieceItem.originalCategory];
                    MovePieceItemToTable(pieceItem, "_HammerPieceTable", pieceTable, category);
                    continue;
                }

                if (combine) {
                    PieceManager.Instance.RemovePieceCategory("_HammerPieceTable", categoryUnCombined);
                } else {
                    PieceManager.Instance.RemovePieceCategory("_HammerPieceTable", categoryCombined);
                }

                category = Helper.GetCategory(pieceItem, combine);
                MovePieceItemToTable(pieceItem, pieceTable, "_HammerPieceTable", category);
            }
        }

        private static void MovePieceItemToTable(PieceItem pieceItem, string pieceTableFrom, string pieceTableTo, string category) {
            bool hasPiece = pieceTables[pieceTableTo].m_pieces.Any(i => i.name == pieceItem.gameObject.name);

            if (!hasPiece) {
                pieceItem.piece.m_category = PieceManager.Instance.AddPieceCategory(pieceTableTo, category);
                pieceTables[pieceTableTo].m_pieces.Add(pieceItem.gameObject);
            }

            if (pieceTables[pieceTableFrom].m_pieces.Contains(pieceItem.gameObject)) {
                pieceTables[pieceTableFrom].m_pieces.Remove(pieceItem.gameObject);
            }
        }
    }
}
