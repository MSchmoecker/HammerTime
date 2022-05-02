using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HammerTime.Patches {
    [HarmonyPatch]
    public static class ObjectDBPatch {
        private static Dictionary<string, PieceTable> pieceTables;
        public static Dictionary<int, string> categoryIdToName;
        private static Dictionary<string, List<PieceItem>> pieces;

        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
        public static void ObjectDBAwake(ObjectDB __instance) {
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

        private static void UpdatePieceTable(string pieceTable) {
            List<PieceItem> pieceMap = pieces[pieceTable];

            if (pieceMap.Count == 0) {
                return;
            }

            bool combine = Config.CombineModCategories(pieceTable, pieceMap[0].modName, () => UpdatePieceTable(pieceTable));
            bool disabled = Config.IsHammerDeactivated(pieceTable, pieceMap[0].modName, () => UpdatePieceTable(pieceTable));

            if (disabled) {
                return;
            }

            foreach (PieceItem pieceItem in pieceMap) {
                string category = Helper.GetCategory(pieceItem, combine);
                pieceItem.piece.m_category = PieceManager.Instance.AddPieceCategory("_HammerPieceTable", category);

                if (pieceTables[pieceTable].m_pieces.Contains(pieceItem.gameObject)) {
                    pieceTables[pieceTable].m_pieces.Remove(pieceItem.gameObject);
                }

                if (pieceTables["_HammerPieceTable"].m_pieces.All(i => i.name != pieceItem.gameObject.name)) {
                    pieceTables["_HammerPieceTable"].m_pieces.Add(pieceItem.gameObject);
                }
            }
        }
    }
}
