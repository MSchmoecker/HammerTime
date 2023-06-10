using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;

namespace HammerTime {
    public class Helper {
        public static Dictionary<Piece.PieceCategory, string> GetCategories() {
            return PieceManager.Instance.GetPieceCategoriesMap();
        }

        public static Dictionary<string, PieceTable> GetPieceTables() {
            return PrefabManager.Cache.GetPrefabs(typeof(PieceTable)).ToDictionary(k => k.Key, v => (PieceTable)v.Value);
        }

        public static bool IsVanillaPieceTable(string pieceTable) {
            return pieceTable == "_HammerPieceTable" || pieceTable == "_HoePieceTable" || pieceTable == "_CultivatorPieceTable";
        }
    }
}
