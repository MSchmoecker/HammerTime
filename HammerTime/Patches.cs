using HarmonyLib;

namespace HammerTime {
    [HarmonyPatch]
    public static class Patches {
        [HarmonyPatch(typeof(Game), nameof(Game.Logout)), HarmonyPostfix]
        public static void GameShutdown() {
            Plugin.Undo();
        }

        public class PlanBuildPatch {
            [HarmonyPatch("PlanBuild.Plans.PlanDB, PlanBuild", "ScanPieceTables"), HarmonyPostfix]
            public static void ScanPieceTables() {
                Plugin.IndexPrefabs();
            }
        }

        public class ObjectDBPatch {
            [HarmonyPatch(typeof(DungeonDB), nameof(DungeonDB.Start)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
            public static void ObjectDBAwake() {
                Plugin.IndexPrefabs();
            }
        }

        public static class WackyDBPatch {
            [HarmonyPatch("wackydatabase.WMRecipeCust, WackysDatabase", "LoadAllRecipeData"), HarmonyPostfix]
            public static void WackyDBLoadAllRecipeData() {
                Plugin.UpdatePieceTables();
            }
        }
    }
}
