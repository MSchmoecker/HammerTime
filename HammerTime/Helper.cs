using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using UnityEngine;

namespace HammerTime {
    public class Helper {
        public static Dictionary<GameObject, int> nameCache = new Dictionary<GameObject, int>();

        public static Dictionary<int, string> GetCategories() {
            List<string> customCategories = PieceManager.Instance.GetPieceCategories();
            Dictionary<int, string> categoryIdToName = customCategories.ToDictionary(i => 4 + customCategories.IndexOf(i), i => i);

            for (int index = 0; index < (int)Piece.PieceCategory.Max; index++) {
                categoryIdToName[index] = ((Piece.PieceCategory)index).ToString();
            }

            categoryIdToName[(int)Piece.PieceCategory.All] = Piece.PieceCategory.All.ToString();

            return categoryIdToName;
        }

        public static Dictionary<string, PieceTable> GetPieceTables() {
            return PrefabManager.Cache.GetPrefabs(typeof(PieceTable)).ToDictionary(k => k.Key, v => (PieceTable)v.Value);
        }

        public static string CleanTableName(string tableName) {
            if (tableName == "_BJORKSNASPieceTable") {
                return "VALKEA";
            }

            if (tableName == "OP_HammerTable") {
                return "Bamboozled";
            }

            return tableName.Replace("PieceTable", "")
                            .Replace("HammerTable", "")
                            .Replace("_", "");
        }

        public static string GetCategory(PieceItem pieceItem, bool combine) {
            if (pieceItem.piece.m_category == Piece.PieceCategory.All) {
                return "All";
            }

            if (!combine && Plugin.categoryIdToName.ContainsKey((int)pieceItem.originalCategory)) {
                return $"{pieceItem.tabName} {Plugin.categoryIdToName[(int)pieceItem.originalCategory]}";
            }

            return pieceItem.tabName;
        }

        public static int GetPieceNameHash(GameObject piece) {
            return nameCache.TryGetValue(piece, out int nameHash) ? nameHash : piece.name.GetHashCode();
        }
    }
}
