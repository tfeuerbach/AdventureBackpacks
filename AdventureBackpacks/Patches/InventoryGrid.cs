using AdventureBackpacks.Extensions;
using HarmonyLib;
using JetBrains.Annotations;

namespace AdventureBackpacks.Patches;

public class InventoryGridPatches
{
    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.UpdateGui))]
    public static class UpdateGuiPatch
    {
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

        [UsedImplicitly]
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(InventoryGrid __instance)
        {
            if (!__instance.name.Equals("ContainerGrid")) return true;

            // Secondary dual-open panel clones ContainerGrid; leave its sizing alone.
            if (__instance.transform != null && __instance.transform.parent != null &&
                __instance.transform.root != null)
            {
                var t = __instance.transform;
                while (t != null)
                {
                    if (t.name == "AB_BackpackSecondary")
                        return true;
                    t = t.parent;
                }
            }

            if (__instance.m_elements.Count >= __instance.m_inventory.m_inventory.Count) return true;
            
            if ((__instance.m_width != __instance.m_inventory.m_width) ||
                (__instance.m_height != __instance.m_inventory.m_height)) return true;
            
            __instance.m_width = __instance.m_inventory.m_width + 1;
            __instance.m_height = __instance.m_inventory.m_height + 1;

            return true;
        }
    }
}