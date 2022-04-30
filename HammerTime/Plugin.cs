using BepInEx;
using HarmonyLib;

namespace HammerTime {
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInProcess("valheim.exe")]
    public class Plugin : BaseUnityPlugin {
        public const string ModName = "HammerTime";
        public const string ModGuid = "com.maxsch.valheim.HammerTime";
        public const string ModVersion = "0.0.0";

        public static Plugin Instance { get; private set; }
        private Harmony harmony;

        private void Awake() {
            Instance = this;

            harmony = new Harmony(ModGuid);
            harmony.PatchAll();
        }
    }
}
