using System;
using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace HammerTime {
    public static class AugaCompat {
        [HarmonyPatch(typeof(Hud), nameof(Hud.Awake)), HarmonyAfter("randyknapp.mods.auga"), HarmonyPostfix]
        public static void AugaHudPatch(Hud __instance) {
            Transform tabs = __instance.m_pieceCategoryRoot.transform.Find("Tabs");
            if (tabs) {
                __instance.m_pieceCategoryRoot = tabs.gameObject;
            }

            Object.Destroy(__instance.m_pieceCategoryRoot.GetComponent<HorizontalLayoutGroup>());
            Object.Destroy(__instance.m_pieceCategoryRoot.GetComponent<ContentSizeFitter>());

            PieceManager.PieceCategorySettings.HeaderWidth = 550f;
            PieceManager.PieceCategorySettings.TabSizePerCharacter = 8.5f;
            PieceManager.PieceCategorySettings.TabMargin = 70f;
            PieceManager.PieceCategorySettings.MinTabSize = 90f;
        }
    }
}
