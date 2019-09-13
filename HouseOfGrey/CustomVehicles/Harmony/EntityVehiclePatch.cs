using DMT;
using Harmony;
using System.Linq;
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

    [HarmonyPatch(typeof(EntityVehicle))]
    [HarmonyPatch("GetActivationCommands")]
    private class PatchGetActivationCommands
    {
        static EntityActivationCommand[] Postfix(EntityActivationCommand[] __result, EntityVehicle __instance)
        {
            var commandsList = __result.ToList();

            string _steamId = GamePrefs.GetString(EnumGamePrefs.PlayerId);

            var workBenchCommand = new EntityActivationCommand("Work Bench", "wrench", true);

            var workBench = new BlockWorkstation();
            workBench.SetBlockName(string.Format("{0}_{1}", __instance.EntityName.ToLower().Replace(" ", "_"), "workbench"));
            workBench.Init();

            Debug.Log(workBench.blockID);

            commandsList.Add(workBenchCommand);

            return commandsList.ToArray();
        }
    }

    [HarmonyPatch(typeof(EntityVehicle))]
    [HarmonyPatch("OnEntityActivated")]
    private class PatchOnEntityActivated
    {
        static bool Postfix(bool __result, EntityVehicle __instance, int _indexInBlockActivationCommands, Vector3i _tePos, EntityAlive _entityFocusing)
        {
            if(_indexInBlockActivationCommands == 10) // Open Workbench
            {
                EntityPlayerLocal entityPlayerLocal = _entityFocusing as EntityPlayerLocal;
                LocalPlayerUI uiForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);

                try
                {
                    var workstationName = string.Format("{0}_{1}", __instance.EntityName.ToLower().Replace(" ", "_"), "workbench");
                    WorkstationData workstationData = CraftingManager.GetWorkstationData(workstationName);
                    if (workstationData == null)
                        return false;

                    var te = __instance.world.GetTileEntity(0, _tePos);

                    string _windowName = !(workstationData.WorkstationWindow != string.Empty) ? string.Format("workstation_{0}", workstationName) : workstationData.WorkstationWindow;
                    if (uiForPlayer.windowManager.Contains(_windowName))
                    {
                        Debug.Log("Window Exists");
                        Debug.Log(te);
                        //((XUiC_WorkstationWindowGroup)((XUiWindowGroup)uiForPlayer.windowManager.GetWindow(_windowName)).Controller).SetTileEntity(te);
                        //Debug.Log("Set Entity Called");
                        try
                        {
                            uiForPlayer.windowManager.Open(_windowName, true, false, true);
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogError("Error opening window: " + ex.StackTrace);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.Log(ex.Message);
                }
            }


            return __result;
        }
    }

    [HarmonyPatch(typeof(GameManager))]
    [HarmonyPatch("workstationOpened")]
    private class TempPatchWorkstationOpened
    {
        static void Postfix(GameManager __instance, TileEntityWorkstation _te, LocalPlayerUI _playerUI)
        {
            if (!(_playerUI != null))
                return;
            BlockValue block = __instance.World.GetBlock(_te.ToWorldPos());
            string blockName = Block.list[block.type].GetBlockName();
            Debug.Log(blockName);
            WorkstationData workstationData = CraftingManager.GetWorkstationData(blockName);
            if (workstationData == null)
                return;
            string _windowName = !(workstationData.WorkstationWindow != string.Empty) ? string.Format("workstation_{0}", blockName) : workstationData.WorkstationWindow;

            Debug.Log(_windowName);
            Debug.Log(_te);
        }
    }

    [HarmonyPatch(typeof(GameManager))]
    [HarmonyPatch("TELockServer")]
    private class TempPatchTELockServer
    {
        static void Postfix(GameManager __instance, int _clrIdx, Vector3i _blockPos, int _lootEntityId, int _entityIdThatOpenedIt)
        {
            if (__instance.World == null)
                return;
            TileEntity tileEntity = _lootEntityId != -1 ? __instance.World.GetTileEntity(_lootEntityId) : __instance.World.GetTileEntity(_clrIdx, _blockPos);
            Debug.Log(_lootEntityId);
            Debug.Log(tileEntity);
            Debug.Log(_clrIdx);
        }
    }
}
