using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace HammerTime {
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInProcess("valheim.exe")]
    public class Plugin : BaseUnityPlugin {
        public const string ModName = "HammerTime";
        public const string ModGuid = "com.maxsch.valheim.HammerTime";
        public const string ModVersion = "0.1.2";

        public static Plugin Instance { get; private set; }
        private Harmony harmony;

        private void Awake() {
            Instance = this;

            harmony = new Harmony(ModGuid);
            harmony.PatchAll();
        }

        private void Start() {
            if (Chainloader.PluginInfos.ContainsKey("randyknapp.mods.auga")) {
                harmony.PatchAll(typeof(AugaCompat));
            }

            if (Chainloader.PluginInfos.ContainsKey("marcopogo.PlanBuild")) {
                harmony.PatchAll(typeof(PlanBuildPatch));
            } else {
                harmony.PatchAll(typeof(ObjectDBPatch));
            }
        }

        public class PlanBuildPatch {
            [HarmonyPatch("PlanBuild.Plans.PlanDB, PlanBuild", "ScanPieceTables"), HarmonyPostfix]
            public static void ScanPieceTables() {
                Patches.IndexItems();
            }
        }

        public class ObjectDBPatch {
            [HarmonyPatch(typeof(DungeonDB), nameof(DungeonDB.Start)), HarmonyPostfix, HarmonyPriority(Priority.Last)]
            public static void ObjectDBAwake() {
                Patches.IndexItems();
            }
        }
    }
}
