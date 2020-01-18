using DMT;
using Harmony;
using System;
using System.Collections.Generic;
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
                        
                        if(localPlayer != null)
                        {
                            var holdingItem = localPlayer.inventory.holdingItem;

                            if (holdingItem.Properties.Contains("DeathForceMultiplier"))
                            {
                                var multiplier = holdingItem.Properties.GetFloat("DeathForceMultiplier");

                                var difficulty = GameStats.GetInt(EnumGameStats.GameDifficulty);
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
