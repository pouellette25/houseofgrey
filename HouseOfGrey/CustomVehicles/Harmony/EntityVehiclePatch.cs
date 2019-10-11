using DMT;
using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


class EntityVehiclePatch : IHarmony
{
    public void Start()
    {
        var patchName = GetType().ToString();

        Debug.Log(" Loading Patch: " + patchName);
        var harmony = HarmonyInstance.Create(patchName);
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    //[HarmonyPatch(typeof(XUiC_WorkstationGrid))]
    //[HarmonyPatch("OnOpen")]
    //private class PatchXUiC_WorkstationGridOnOpen
    //{
    //    static bool Prefix(XUiC_WorkstationGrid __instance, XUiM_Workstation ___workstationData, XUiWindowGroup ___windowGroup)
    //    {
    //        if (___windowGroup.Controller is XUiC_WorkstationWindowGroup) { return true; }

    //        if (__instance.ViewComponent != null && !__instance.ViewComponent.IsVisible)
    //        {
    //            Debug.Log("PatchXUiC_WorkstationGridOnOpen Prefix: ViewComponent is good");
    //            __instance.ViewComponent.OnOpen();
    //            __instance.ViewComponent.IsVisible = true;
    //        }

    //        Debug.Log("PatchXUiC_WorkstationGridOnOpen Prefix: before cast");

    //        //___workstationData = ((XUiC_VehicleWorkbenchWindowGroup)___windowGroup.Controller).WorkstationData;

    //        var test = ((XUiC_VehicleWorkbenchWindowGroup)___windowGroup.Controller);

    //        Debug.Log(test);

    //        __instance.IsDirty = true;
    //        __instance.IsDormant = false;

    //        return false;
    //    }
    //}

    [HarmonyPatch(typeof(EntityFactory), "addEntityToGameObject")]
    private class PatchEntityFactoryAddEntityToGameObject
    {
        public static Entity Postfix(Entity __result, GameObject _gameObject, string _className)
        {
            if (__result == null)
            {
                Type type = Type.GetType(_className + ", Mods");

                Debug.Log("custom entity type: " + type);

                if (type != null)
                {
                    return (Entity)_gameObject.AddComponent(type);
                }
            }

            return __result;
        }
    }
}
