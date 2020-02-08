using DMT;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HoG_Weapons_and_Ammo.Harmony
{
    public class EModelBasePatch : IHarmony
    {
        public void Start()
        {
            var patchName = GetType().ToString();

            Debug.Log(" Loading Patch: " + patchName);
            var harmony = HarmonyInstance.Create(patchName);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        [HarmonyPatch(typeof(EModelBase), "DoRagdoll")]
        [HarmonyPatch(new Type[] { typeof(DamageResponse), typeof(float) })]
        private class EModelBasePatchDoRagdoll
        {
            public static bool Prefix(EModelBase __instance, DamageResponse dr, float stunTime = 999999f)
            {
                float strength = dr.Strength;
                DamageSource source = dr.Source;

                var _difficultyModifiers = new Dictionary<int, float>()
                    {
                        {0, 0.5f },
                        {1, 0.75f },
                        {3, 1.5f },
                        {4, 2.0f },
                        {5, 2.5f },
                    };

                if (strength > 0.0 && source != null)
                {                    
                    float angle = GameManager.Instance.World.GetGameRandom().RandomRange(-20f, 50f);
                    float num1 = strength * 0.5f;
                    if (source.damageType == EnumDamageTypes.Bashing)
                        num1 *= 2.5f;
                    if (dr.Critical)
                    {
                        angle += 25f;
                        num1 *= 2f;
                    }
                    if (dr.HitBodyPart == EnumBodyPartHit.Head)
                        num1 *= 0.45f;
                    float num2 = Utils.FastMin(20f + num1, 500f);
                    Vector3 direction = source.getDirection();
                    Vector3 axis = Vector3.Cross(direction, Vector3.up);
                    Vector3 forceVec = Quaternion.AngleAxis(angle, axis) * direction * num2;

                    var damageSourceEntity = GameManager.Instance.World.GetEntity(source.getEntityId());

                    if(damageSourceEntity != null && dr.Fatal) // only run this logic if the hit was a kill
                    {
                        var localPlayer = (damageSourceEntity as EntityPlayerLocal);

                        if (localPlayer != null && localPlayer.inventory != null) // there appears to be a strange timing issue when the inventory is null and throws an exception
                        {
                            var multiplier = 0.0f;

                            var holdingItem = localPlayer.inventory.holdingItem;
                            var holdingItemData = localPlayer.inventory.holdingItemData;

                            if (holdingItemData != null && holdingItemData.itemValue != null && holdingItemData.itemValue.Modifications != null)
                            {
                                foreach (var mod in localPlayer.inventory.holdingItemData.itemValue.Modifications)
                                {
                                    if (mod.IsMod && mod.ItemClass != null)
                                    {
                                        if (mod.ItemClass.Properties != null && mod.ItemClass.Properties.Contains("DeathForceMultiplier"))
                                        {
                                            var modMultiplier = mod.ItemClass.Properties.GetFloat("DeathForceMultiplier");

                                            // get the highest multiplier from any mods
                                            if(modMultiplier > multiplier)
                                            {
                                                multiplier = modMultiplier;
                                            }
                                        }
                                    }
                                }
                            }

                            if (holdingItem.Properties.Contains("DeathForceMultiplier"))
                            {
                                var itemMultiplier = holdingItem.Properties.GetFloat("DeathForceMultiplier");
                                if(itemMultiplier > multiplier)
                                {
                                    multiplier = itemMultiplier;
                                }
                            }

                            if(multiplier > 0)
                            {
                                var difficulty = GameStats.GetInt(EnumGameStats.GameDifficulty);

                                Debug.Log(multiplier);
                                forceVec *= multiplier * _difficultyModifiers[difficulty];
                            }
                        }
                    }

                    __instance.DoRagdoll(stunTime, dr.HitBodyPart, forceVec, source.getHitTransformPosition(), false);
                }
                else
                    __instance.DoRagdoll(stunTime, dr.HitBodyPart, Vector3.zero, Vector3.zero, false);

                return false;
            }
        }

        //[HarmonyPatch(typeof(EModelBase), "DoRagdoll")]
        //[HarmonyPatch(new Type[] { typeof(float), typeof(EnumBodyPartHit), typeof(Vector3), typeof(Vector3), typeof(bool) })]
        //private class EModelBasePatchOtherDoRagdoll
        //{
        //    public static void Postfix(float stunTime, EnumBodyPartHit bodyPart, Vector3 forceVec, Vector3 forceWorldPos, bool isRemote)
        //    {
        //        forceVec *= 200;
        //    }
        //}
    }
}
