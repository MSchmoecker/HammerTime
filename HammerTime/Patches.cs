using HarmonyLib;

namespace HammerTime {
    [HarmonyPatch]
    public static class Patches {
        [HarmonyPatch(typeof(Game), nameof(Game.Logout)), HarmonyPostfix]
        public static void GameShutdown() {
            Plugin.Undo();
        }

        public class ObjectDBPatch {
            [HarmonyPatch(typeof(DungeonDB), nameof(DungeonDB.Start)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
            public static void ObjectDBAwake() {
                Plugin.IndexPrefabs();
            }
        }
    }
}
