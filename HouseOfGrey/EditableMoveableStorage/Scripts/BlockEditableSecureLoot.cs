using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

class BlockEditableSecureLoot : BlockSecureLoot
{
    private float _takeDelay = 2f;
    private readonly BlockActivationCommand[] _activationCommands = new BlockActivationCommand[]
      {
            new BlockActivationCommand("Search", "search", false),
            new BlockActivationCommand("lock", "lock", false),
            new BlockActivationCommand("unlock", "unlock", false),
            new BlockActivationCommand("keypad", "keypad", false),
            new BlockActivationCommand("take", "hand", false),
            new BlockActivationCommand("edit", "pen", false)
      };

    public override void Init()
    {
        base.Init();

        _takeDelay = !Properties.Values.ContainsKey("TakeDelay") ? 2f : StringParsers.ParseFloat(Properties.Values["TakeDelay"], 0, -1, NumberStyles.Any);
    }

    public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        var tileEntity = _world.GetTileEntity(_clrIdx, _blockPos);
        var secureLootEntity = tileEntity as TileEntitySecureLootContainer;

        if (secureLootEntity == null)
            return new BlockActivationCommand[0];
        string _steamID = GamePrefs.GetString(EnumGamePrefs.PlayerId);
        PersistentPlayerData playerData = _world.GetGameManager().GetPersistentPlayerList().GetPlayerData(secureLootEntity.GetOwner());
        bool flag = !secureLootEntity.IsOwner(_steamID) && (playerData != null && playerData.ACL != null && playerData.ACL.Contains(_steamID));
        _activationCommands[0].enabled = true;
        _activationCommands[1].enabled = !secureLootEntity.IsLocked() && (secureLootEntity.IsOwner(_steamID) || flag);
        _activationCommands[2].enabled = secureLootEntity.IsLocked() && secureLootEntity.IsOwner(_steamID);
        _activationCommands[3].enabled = !secureLootEntity.IsUserAllowed(_steamID) && secureLootEntity.HasPassword() && secureLootEntity.IsLocked() || secureLootEntity.IsOwner(_steamID);
        bool isMyLandProtectedBlock = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer(), false);
        _activationCommands[4].enabled = isMyLandProtectedBlock && secureLootEntity.IsOwner(_steamID) && _takeDelay > 0.0;

        _activationCommands[5].enabled = secureLootEntity.IsOwner(_steamID);

        return _activationCommands;
    }

    public override bool OnBlockActivated(int _indexInBlockActivationCommands, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        EntityPlayerLocal entityPlayerLocal = _player as EntityPlayerLocal;
        LocalPlayerUI uiForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
        var tileEntity = _world.GetTileEntity(_cIdx, _blockPos) as TileEntitySecureLootContainer;

        if (_indexInBlockActivationCommands == 4)
        {
            if (!tileEntity.IsEmpty())
            {
                GameManager.ShowTooltipWithAlert(entityPlayerLocal, Localization.Get("ttEmptyLootContainerBeforePickup", string.Empty), "ui_denied");
                return false;
            }
            TakeItemWithTimer(_cIdx, _blockPos, _blockValue, _player);
            return true;
        }

        if(_indexInBlockActivationCommands == 5)
        {
            if (uiForPlayer != null)
            {
                ((XUiWindowGroup)uiForPlayer.windowManager.GetWindow("storage_box")).Controller.GetChildByType<XUiC_StorageBoxLabelWindow>().SetTileEntity(tileEntity);
                uiForPlayer.windowManager.Open("storage_box", true, false, true);
            }
            return true;
        }

        return base.OnBlockActivated(_indexInBlockActivationCommands, _world, _cIdx, _blockPos, _blockValue, _player);
    }

    public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
    {
        var controller = GetStorageWindowController();

        if(controller != null)
        {
            var tileEntity = _world.GetTileEntity(_cIdx, _blockPos);

            controller.SetTileEntity(tileEntity as TileEntitySecureLootContainer);
            controller.OnBlockEntityTransformAfterActivated(_blockPos.GetHashCode());
        }

        base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
    }

    public override bool OnBlockDestroyedBy(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
    {
        var controller = GetStorageWindowController();

        if(controller != null)
        {
            controller.OnBlockOnBlockDestroyedBy(_blockPos.GetHashCode());
        }

        return base.OnBlockDestroyedBy(_world, _clrIdx, _blockPos, _blockValue, _entityId, _bUseHarvestTool);
    }

    public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        var controller = GetStorageWindowController();
        if (controller != null)
        {
            controller.OnBlockRemoved(_blockPos.GetHashCode());
        }

        base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
    }

    public void TakeItemWithTimer(int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        if (_blockValue.damage > 0)
        {
            GameManager.ShowTooltipWithAlert(_player as EntityPlayerLocal, Localization.Get("ttRepairBeforePickup", string.Empty), "ui_denied");
        }
        else
        {
            LocalPlayerUI playerUi = (_player as EntityPlayerLocal).PlayerUI;
            playerUi.windowManager.Open("timer", true, false, true);
            XUiC_Timer childByType = playerUi.xui.GetChildByType<XUiC_Timer>();
            TimerEventData _eventData = new TimerEventData
            {
                Data = new object[4]
                {
                     _cIdx,
                     _blockValue,
                     _blockPos,
                     _player
                }
            };
            _eventData.Event += new TimerEventHandler(this.EventData_Event);
            childByType.SetTimer(_takeDelay, _eventData);
        }
    }

    private void EventData_Event(object obj)
    {
        World world = GameManager.Instance.World;
        object[] objArray = (object[])obj;
        int _clrIdx = (int)objArray[0];
        BlockValue blockValue = (BlockValue)objArray[1];
        Vector3i vector3i = (Vector3i)objArray[2];
        BlockValue block = world.GetBlock(vector3i);
        EntityPlayerLocal entityPlayerLocal = objArray[3] as EntityPlayerLocal;
        if (block.damage > 0)
            GameManager.ShowTooltipWithAlert(entityPlayerLocal, Localization.Get("ttRepairBeforePickup", string.Empty), "ui_denied");
        else if (block.type != blockValue.type)
        {
            GameManager.ShowTooltipWithAlert(entityPlayerLocal, Localization.Get("ttBlockMissingPickup", string.Empty), "ui_denied");
        }
        else
        {
            TileEntitySecureLootContainer tileEntity = world.GetTileEntity(_clrIdx, vector3i) as TileEntitySecureLootContainer;
            if (tileEntity.IsUserAccessing())
            {
                GameManager.ShowTooltipWithAlert(entityPlayerLocal, Localization.Get("ttCantPickupInUse", string.Empty), "ui_denied");
            }
            else
            {
                LocalPlayerUI uiForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
                ItemStack itemStack = new ItemStack(block.ToItemValue(), 1);
                if (!uiForPlayer.xui.PlayerInventory.AddItem(itemStack, true))
                    uiForPlayer.xui.PlayerInventory.DropItem(itemStack);
                world.SetBlockRPC(_clrIdx, vector3i, BlockValue.Air);
            }
        }
    }

    // TODO: Move this to a helper class, and make it generic
    private XUiC_StorageBoxLabelWindow GetStorageWindowController()
    {
        var windowManager = (GUIWindowManager)UnityEngine.Object.FindObjectOfType(typeof(GUIWindowManager));

        if (windowManager == null || !windowManager.Contains("storage_box"))
        {
            return null;
        }

        var editWindow = windowManager.GetWindow("storage_box");

        if (editWindow != null)
        {
            var controller = (editWindow as XUiWindowGroup).Controller;

            return controller != null ? controller.GetChildByType<XUiC_StorageBoxLabelWindow>() : null;
        }

        return null;
    }
}
