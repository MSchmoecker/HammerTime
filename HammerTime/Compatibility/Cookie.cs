using System.Collections.Generic;
using BepInEx.Bootstrap;
using Jotunn.Utils;
using UnityEngine;

namespace HammerTime.Compatibility {
    public class Cookie {
        public static void IndexPrefabs(Dictionary<string, List<PieceItem>> piecesByTable) {
            Transform hammerUI = Chainloader.ManagerObject.transform.Find("CookieHammerUI");

            if (!hammerUI) {
                return;
            }

            Transform pieceParent = hammerUI.transform.Find("Canvas/MainTab/ItemList/ListPanel/Scroll View/Viewport/Content");

            if (!pieceParent) {
                Plugin.Log.LogWarning("CookieMilk HammerUI found but no piece parent");
                return;
            }

            foreach (Transform categoryTransform in pieceParent) {
                if (!categoryTransform) {
                    continue;
                }

                string category = categoryTransform.name;

                foreach (Transform pieceTransform in categoryTransform) {
                    if (!pieceTransform) {
                        continue;
                    }

                    string prefabName = pieceTransform.name;
                    IModPrefab modPrefab = ModQuery.GetPrefab(prefabName);

                    if (modPrefab == null) {
                        Plugin.Log.LogWarning($"CookieMilk Prefab {prefabName} not found");
                        continue;
                    }

                    string mod = modPrefab.SourceMod.Name;

                    if (!piecesByTable.TryGetValue(mod, out List<PieceItem> modPieces)) {
                        modPieces = new List<PieceItem>();
                        piecesByTable.Add(mod, modPieces);
                    }

                    Piece piece = modPrefab.Prefab.GetComponent<Piece>();
                    modPieces.Add(new PieceItem(modPrefab.Prefab, piece, mod, category));
                }
            }
        }
    }
}
