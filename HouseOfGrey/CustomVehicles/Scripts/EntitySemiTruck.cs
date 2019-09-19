using Audio;
using InControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class EntitySemiTruck : EntityVJeep
{
    public override void Init(int _entityClass)
    {
        try
        {
            base.Init(_entityClass);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }

        //var workBench = new BlockWorkstation();
        //workBench.SetBlockName(string.Format("{0}_{1}", GetType().Name, "workbench"));
        //workBench.Init();
    }

    public override EntityActivationCommand[] GetActivationCommands(Vector3i _tePos, EntityAlive _entityFocusing)
    {
        return base.GetActivationCommands(_tePos, _entityFocusing);

        var commandsList = base.GetActivationCommands(_tePos, _entityFocusing).ToList();

        string _steamId = GamePrefs.GetString(EnumGamePrefs.PlayerId);

        var workBenchCommand = new EntityActivationCommand("Work Bench", "wrench", true);

        commandsList.Add(workBenchCommand);

        return commandsList.ToArray();
    }

    public override bool OnEntityActivated(int _indexInBlockActivationCommands, Vector3i _tePos, EntityAlive _entityFocusing)
    {
        if (_indexInBlockActivationCommands == 10) // Open Workbench
        {
            EntityPlayerLocal entityPlayerLocal = _entityFocusing as EntityPlayerLocal;
            LocalPlayerUI uiForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);

            try
            {
                var workstationName = string.Format("{0}_{1}", GetType().Name, "workbench");
                WorkstationData workstationData = CraftingManager.GetWorkstationData(workstationName);
                Debug.Log(string.Format("Workstation Data {0}", workstationData));
                if (workstationData == null)
                    return false;

                var te = world.GetTileEntity(0, _tePos);

                string _windowName = !(workstationData.WorkstationWindow != string.Empty) ? string.Format("workstation_{0}", workstationName) : workstationData.WorkstationWindow;
                if (uiForPlayer.windowManager.Contains(_windowName))
                {
                    //((XUiC_WorkstationWindowGroup)((XUiWindowGroup)uiForPlayer.windowManager.GetWindow(_windowName)).Controller).SetTileEntity(te);
                    //Debug.Log("Set Entity Called");
                    try
                    {
                        uiForPlayer.windowManager.Open(_windowName, true, false, true);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Error opening window: " + ex.StackTrace);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }

            return true;
        }

        return base.OnEntityActivated(_indexInBlockActivationCommands, _tePos, _entityFocusing);
    }
}
