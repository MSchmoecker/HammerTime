using UnityEngine;

namespace HammerTime {
    public class PieceItem {
        public GameObject gameObject;
        public Piece piece;
        public int nameHash;
        public string modName;
        public Piece.PieceCategory originalCategory;
        public string overrideCategory;

        public PieceItem(GameObject gameObject, Piece piece, string modName, string overrideCategory) {
            this.gameObject = gameObject;
            this.piece = piece;
            nameHash = gameObject.name.GetStableHashCode();
            originalCategory = piece.m_category;
            this.overrideCategory = overrideCategory;
            this.modName = modName;
        }
    }
}
