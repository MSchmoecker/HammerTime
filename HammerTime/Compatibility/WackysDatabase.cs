using System;
using System.Reflection;

namespace HammerTime.Compatibility {
    public static class WackysDatabase {
        public static void InitCompat() {
            // disabled as indexing pieces twice causes issues with hot reloading
            // SubscribeToReload();
        }

        private static void SubscribeToReload() {
            EventInfo onAllReloaded = Type.GetType("wackydatabase.SetData.Reload, WackysDatabase").GetEvent("OnAllReloaded");
            onAllReloaded.AddEventHandler(null, new Action(() => {
                Plugin.IndexPrefabs();
                Plugin.UpdatePieceTables();
            }));
        }
    }
}
