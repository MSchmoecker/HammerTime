using HarmonyLib;

namespace HammerTime.Compatibility {
    public static class PlanBuild {
        public static void InitCompat() {
            Plugin.Instance.harmony.PatchAll(typeof(PlanBuildPatches));
        }

        private static class PlanBuildPatches {
            [HarmonyPatch("PlanBuild.Plans.PlanDB, PlanBuild", "ScanPieceTables"), HarmonyPostfix]
            public static void ScanPieceTables() {
                Plugin.IndexPrefabs();
            }
        }
    }
}
