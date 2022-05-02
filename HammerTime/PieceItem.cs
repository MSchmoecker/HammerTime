using UnityEngine;

namespace HammerTime {
    public struct PieceItem {
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
}
