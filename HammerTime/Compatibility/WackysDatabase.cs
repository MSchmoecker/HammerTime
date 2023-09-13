using System;
using System.Reflection;

namespace HammerTime.Compatibility {
    public static class WackysDatabase {
        public static void InitCompat() {
            EventInfo onAllReloaded = Type.GetType("wackydatabase.SetData.Reload, WackysDatabase").GetEvent("OnAllReloaded");
            onAllReloaded.AddEventHandler(null, new Action(() => {
                Plugin.IndexPrefabs();
                Plugin.UpdatePieceTables();
            }));
        }
    }
}
