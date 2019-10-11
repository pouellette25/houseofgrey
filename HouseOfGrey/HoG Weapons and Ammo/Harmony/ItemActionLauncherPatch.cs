using DMT;
using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using UnityEngine;

namespace HoG_Weapons_and_Ammo.Harmony
{
    public class ItemActionLauncherPatch : IHarmony
    {
        public void Start()
        {
            var patchName = GetType().ToString();

            Debug.Log(" Loading Patch: " + patchName);
            var harmony = HarmonyInstance.Create(patchName);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        [HarmonyPatch(typeof(ItemActionLauncher), "instantiateProjectile")]
        private class PatchItemActionLauncherInstantiateProjectile
        {
            public static Transform Postfix(Transform __result, ItemActionLauncher __instance, ItemActionData _actionData, Vector3 _positionOffset)
            {
                if(__instance is ItemActionStaff)
                {
                    __result.gameObject.SetActive(false);
                }

                return __result;
            }
        }
    }
}
