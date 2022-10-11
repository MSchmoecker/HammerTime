using UnityEngine;

namespace HammerTime {
    public struct PieceItem {
        public GameObject gameObject;
        public Piece piece;
        public int nameHash;
        public string modName;
        public Piece.PieceCategory originalCategory;

        public PieceItem(GameObject gameObject, Piece piece, string modName) {
            this.gameObject = gameObject;
            this.piece = piece;
            nameHash = gameObject.name.GetStableHashCode();
            originalCategory = piece.m_category;
            this.modName = modName;
        }
    }
}
