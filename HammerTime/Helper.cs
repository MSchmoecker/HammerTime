using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;

namespace HammerTime {
    public class Helper {
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

        public static bool IsVanillaPieceTable(string pieceTable) {
            return pieceTable == "_HammerPieceTable" || pieceTable == "_HoePieceTable" || pieceTable == "_CultivatorPieceTable";
        }
    }
}
