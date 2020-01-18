using DMT;
using Harmony;
using InControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

class SphereII_PlayerMoveController_Patch
{
    public class SphereII_PlayerMoveController_Logger
    {
        public static bool blDisplayLog = false;

        public static void Log(String strMessage)
        {
            if (blDisplayLog)
                UnityEngine.Debug.Log(strMessage);
        }
    }

    public class SphereII_PlayerMoveController_Init : IHarmony
    {
        public void Start()
        {
            SphereII_PlayerMoveController_Logger.Log(" Loading Patch: " + GetType().ToString());
            var harmony = HarmonyInstance.Create(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }


    // This patches various constructors in Inventory from initial values to 12
    [HarmonyPatch(typeof(Inventory), MethodType.Constructor)]
    [HarmonyPatch("Inventory")]
    [HarmonyPatch(new Type[] { typeof(IGameManager), typeof(EntityAlive) })]
    public class SphereII_PatchInventoryInventory
    {
        static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_S)
                {
                    codes[i].opcode = OpCodes.Ldc_I4;
                    codes[i].operand = 11;
                }
            }
            return codes.AsEnumerable();
        }
    }

    // This is to make slots 9 and 10 accessable with keys or mouse scroll
    [HarmonyPatch(typeof(PlayerMoveController))]
    [HarmonyPatch("Update")]
    public class SphereII_PlayerMoveControllerUpdate
    {
        // Loops around the instructions and removes the return condition.
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SphereII_PlayerMoveController_Logger.Log("Patching Update()");

            int counter = 0;

            // Grab all the instructions
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_7)
                {
                    SphereII_PlayerMoveController_Logger.Log("Adjusting 7 to 9");
                    codes[i].opcode = OpCodes.Ldc_I4; // convert to the right thing
                    codes[i].operand = 9;
                    SphereII_PlayerMoveController_Logger.Log("Done with 7 to 9");
                }
                if (codes[i].opcode == OpCodes.Ldc_I4_8)
                {
                    counter++;
                    SphereII_PlayerMoveController_Logger.Log("Adjusting 8 to 10");

                    codes[i].opcode = OpCodes.Ldc_I4; // convert to the right thing
                    codes[i].operand = 10;
                    SphereII_PlayerMoveController_Logger.Log("Done with 8 to 10");
                }

            }
            SphereII_PlayerMoveController_Logger.Log("Done With Patching Update()");

            return codes.AsEnumerable();
        }

        // In addition to the transpiler above, we also want to see whether out inventory actions has the expected amount here. Hopefully this will only ever execute once.
        public static bool Prefix(PlayerMoveController __instance)
        {
            if (__instance.playerInput.InventoryActions.Count == 9)
            {
                SphereII_PlayerMoveController_Logger.Log("Player Action is 9. Adding Additional Slots");

                PlayerActionsLocal temp = __instance.playerInput;
                temp.InventorySlot10 = temp.CreatePlayerAction("Inventory10");
                temp.InventorySlot10.UserData = new PlayerActionsBase.ActionUserData("inpActInventorySlot9Name", null, PlayerActionsBase.GroupToolbelt, PlayerActionsBase.EAppliesToInputType.KbdMouseOnly, true);

                temp.InventorySlot11 = temp.CreatePlayerAction("Inventory11");
                temp.InventorySlot11.UserData = new PlayerActionsBase.ActionUserData("inpActInventorySlot10Name", null, PlayerActionsBase.GroupToolbelt, PlayerActionsBase.EAppliesToInputType.KbdMouseOnly, true);

                temp.InventoryActions.Add(temp.InventorySlot10);
                temp.InventoryActions.Add(temp.InventorySlot11);

                temp.InventorySlot10.AddDefaultBinding(new InControl.Key[]
                {
                    InControl.Key.Key0
                });

            }
            return true;
        }
    }

    // This stops the toolbelt resetting to slot 1 if the player logs out with 9 or 10 selected	
    [HarmonyPatch(typeof(PlayerMoveController))]
    [HarmonyPatch("Start")]
    public class SphereII_PlayerMoveControllerStart
    {
        public static bool Prefix(PlayerMoveController __instance, EntityPlayerLocal ___entityPlayerLocal)
        {
            if (___entityPlayerLocal.inventory.GetFocusedItemIdx() < 0 || ___entityPlayerLocal.inventory.GetFocusedItemIdx() > 10)
            {
                ___entityPlayerLocal.inventory.SetFocusedItemIdx(0);
                ___entityPlayerLocal.inventory.SetHoldingItemIdx(0);
            }
            return false;
        }
    }



    [HarmonyPatch(typeof(PlayerActionsLocal))]
    [HarmonyPatch("get_InventorySlotWasPressed")]
    public class SphereII_PlayerActionsLocalInventorySlotWasPressed
    {
        // Loops around the instructions and removes the return condition.
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int counter = 0;

            // Grab all the instructions
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_8)
                {
                    counter++;
                    SphereII_PlayerMoveController_Logger.Log("Adjusting 8 to 10");

                    codes[i].opcode = OpCodes.Ldc_I4; // convert to the right thing
                    codes[i].operand = 10;
                    SphereII_PlayerMoveController_Logger.Log("Done with 8 to 10");
                }

            }
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(PlayerActionsLocal))]
    [HarmonyPatch("get_InventorySlotWasReleased")]
    public class SphereII_PlayerActionsLocalInventorySlotWasReleased
    {
        // Loops around the instructions and removes the return condition.
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int counter = 0;

            // Grab all the instructions
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_8)
                {
                    counter++;
                    SphereII_PlayerMoveController_Logger.Log("Adjusting 8 to 10");

                    codes[i].opcode = OpCodes.Ldc_I4; // convert to the right thing
                    codes[i].operand = 10;
                    SphereII_PlayerMoveController_Logger.Log("Done with 8 to 10");
                }

            }
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(PlayerActionsLocal))]
    [HarmonyPatch("get_InventorySlotIsPressed")]
    public class SphereII_PlayerActionsLocalInventorySlotIsPressed
    {
        // Loops around the instructions and removes the return condition.
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int counter = 0;

            // Grab all the instructions
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_8)
                {
                    counter++;
                    SphereII_PlayerMoveController_Logger.Log("Adjusting 8 to 10");

                    codes[i].opcode = OpCodes.Ldc_I4; // convert to the right thing
                    codes[i].operand = 10;
                    SphereII_PlayerMoveController_Logger.Log("Done with 8 to 10");
                }

            }
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(ItemActionEntryUse))]
    [HarmonyPatch("RefreshEnabled")]
    public class SphereII_ItemActionEntryUseRefreshEnabled
    {
        // Loops around the instructions and removes the return condition.
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SphereII_PlayerMoveController_Logger.Log("Patching RefreshEnabled()");

            int counter = 0;

            // Grab all the instructions
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_8)
                {
                    counter++;
                    SphereII_PlayerMoveController_Logger.Log("Adjusting 8 to 10");

                    codes[i].opcode = OpCodes.Ldc_I4; // convert to the right thing
                    codes[i].operand = 10;
                    SphereII_PlayerMoveController_Logger.Log("Done with 8 to 10");
                }

            }

            SphereII_PlayerMoveController_Logger.Log("Done With Patching RefreshEnabled()");

            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(ItemActionEntryUse))]
    [HarmonyPatch("OnActivated")]
    public class SphereII_ItemActionEntryUseOnActivated
    {
        // Token: 0x060021E4 RID: 8676 RVA: 0x000DAB5C File Offset: 0x000D8D5C
        public static IEnumerator SimulateActionExecution(Inventory inventory, int _actionIdx, ItemStack _itemStack, Action onComplete)
        {
            int TempSlot = 10;
            inventory.SetItem(TempSlot, new ItemStack(_itemStack.itemValue.Clone(), 2));
            yield return new WaitForSeconds(0.1f);
            inventory.SetHoldingItemIdx(TempSlot);
            yield return new WaitForSeconds(0.1f);
            inventory.CallOnToolbeltChangedInternal();
            yield return new WaitForSeconds(0.1f);
            while (inventory.IsHolsterDelayActive())
            {
                yield return new WaitForSeconds(0.1f);
            }
            inventory.Execute(_actionIdx, false, null);
            yield return new WaitForSeconds(0.1f);
            inventory.Execute(_actionIdx, true, null);
            if (_itemStack.itemValue.ItemClass != null && _itemStack.itemValue.ItemClass.Actions.Length > _actionIdx && _itemStack.itemValue.ItemClass.Actions[_actionIdx] != null)
            {
                inventory.slots[TempSlot].itemStack.itemValue.ItemClass.Actions[_actionIdx].OnHoldingUpdate(inventory.GetItemActionDataInSlot(TempSlot, _actionIdx));
                while (inventory.IsHoldingItemActionRunning())
                {
                    yield return new WaitForSeconds(0.1f);
                }
                ItemStack itemStack = new ItemStack(_itemStack.itemValue.Clone(), 1);
                if (itemStack.itemValue.ItemClass.Actions[_actionIdx] is ItemActionEat)
                {
                    if (((ItemActionEat)itemStack.itemValue.ItemClass.Actions[_actionIdx]).Consume)
                    {
                        if (itemStack.count > 1)
                        {
                            itemStack.count--;
                        }
                        else if (!itemStack.itemValue.HasQuality || itemStack.itemValue.MaxUseTimes == 0)
                        {
                            itemStack = ItemStack.Empty.Clone();
                        }
                        else if (itemStack.itemValue.MaxUseTimes > 0)
                        {
                            itemStack.itemValue.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, itemStack.itemValue, 1f, inventory.entity, null, itemStack.itemValue.ItemClass.ItemTags, true, true, true, true, 1, true);
                        }
                    }
                    else if (inventory.entity as EntityPlayerLocal != null)
                    {
                        (inventory.entity as EntityPlayerLocal).PlayerUI.xui.PlayerInventory.AddItem(itemStack, true);
                    }
                }
            }
            while (inventory.IsHolsterDelayActive())
            {
                yield return new WaitForSeconds(0.1f);
            }
            onComplete();
            yield break;
            yield break;
        }

        public static bool Prefix(ItemActionEntryUse __instance, ItemActionEntryUse.ConsumeType ___consumeType, ref int ___oldToolbeltFocusID)
        {
            if (__instance.ItemController.xui.isUsingItemActionEntryUse)
            {
                return false;
            }
            XUiC_ItemStack stackControl = (XUiC_ItemStack)__instance.ItemController;
            if (!stackControl.ItemStack.itemValue.ItemClass.CanExecuteAction(0, __instance.ItemController.xui.playerUI.entityPlayer, stackControl.ItemStack.itemValue) || !stackControl.ItemStack.itemValue.ItemClass.CanExecuteAction(1, __instance.ItemController.xui.playerUI.entityPlayer, stackControl.ItemStack.itemValue))
            {
                GameManager.ShowTooltipWithAlert(__instance.ItemController.xui.playerUI.entityPlayer, "You cannot use that at this time.", "ui_denied");
                return false;
            }
            __instance.ItemController.xui.isUsingItemActionEntryUse = true;
            ItemStack itemStack = new ItemStack(stackControl.ItemStack.itemValue.Clone(), 1);
            ItemStack itemStack2 = new ItemStack(stackControl.ItemStack.itemValue.Clone(), stackControl.ItemStack.count - 1);
            if (itemStack2.count == 0)
            {
                itemStack2 = ItemStack.Empty.Clone();
            }
            Inventory inventory = __instance.ItemController.xui.PlayerInventory.Toolbelt;
            if (___consumeType == ItemActionEntryUse.ConsumeType.Quest)
            {
                __instance.ItemController.xui.FindWindowGroupByName("questOffer").GetChildByType<XUiC_QuestOfferWindow>().ItemStackController = stackControl;
                stackControl.QuestLock = true;
            }
            else
            {
                stackControl.HiddenLock = true;
            }
            stackControl.WindowGroup.Controller.SetAllChildrenDirty();
            __instance.RefreshEnabled();
            ___oldToolbeltFocusID = inventory.GetFocusedItemIdx();
            int num = 0;
            if (stackControl.ItemStack.itemValue.ItemClass != null)
            {
                for (int i = 0; i < stackControl.ItemStack.itemValue.ItemClass.Actions.Length; i++)
                {
                    bool flag = false;
                    switch (___consumeType)
                    {
                        case ItemActionEntryUse.ConsumeType.Eat:
                        case ItemActionEntryUse.ConsumeType.Drink:
                        case ItemActionEntryUse.ConsumeType.Heal:
                            if (stackControl.ItemStack.itemValue.ItemClass.Actions[i] != null)
                            {
                                flag = true;
                            }
                            break;
                        case ItemActionEntryUse.ConsumeType.Read:
                            if (stackControl.ItemStack.itemValue.ItemClass.Actions[i] is ItemActionLearnRecipe)
                            {
                                flag = true;
                            }
                            break;
                        case ItemActionEntryUse.ConsumeType.Quest:
                            if (stackControl.ItemStack.itemValue.ItemClass.Actions[i] is ItemActionQuest)
                            {
                                flag = true;
                            }
                            break;
                        case ItemActionEntryUse.ConsumeType.Open:
                            if (stackControl.ItemStack.itemValue.ItemClass.Actions[i] is ItemActionOpenBundle)
                            {
                                flag = true;
                            }
                            break;
                    }
                    if (flag)
                    {
                        num = i;
                        break;
                    }
                }
            }
            if (___consumeType != ItemActionEntryUse.ConsumeType.Quest)
            {
                stackControl.ItemStack = itemStack2;
            }
            if (!itemStack.itemValue.ItemClass.Actions[num].UseAnimation && itemStack.itemValue.ItemClass.Actions[num].ExecuteInstantAction(__instance.ItemController.xui.playerUI.entityPlayer, itemStack, false, stackControl))
            {
                if (___consumeType != ItemActionEntryUse.ConsumeType.Quest)
                {
                    stackControl.HiddenLock = false;
                    stackControl.WindowGroup.Controller.SetAllChildrenDirty();
                }
                __instance.ItemController.xui.isUsingItemActionEntryUse = false;
                return false;
            }

            int oldSlot = ___oldToolbeltFocusID;
            Inventory tempInventory = __instance.ItemController.xui.playerUI.entityPlayer.inventory;
            GameManager.Instance.StartCoroutine(SphereII_ItemActionEntryUseOnActivated.SimulateActionExecution(tempInventory, num, itemStack, delegate
            {
                stackControl.WindowGroup.Controller.SetAllChildrenDirty();
                inventory.SetHoldingItemIdx(oldSlot);
                inventory.SetItem(10, ItemStack.Empty.Clone());
                inventory.OnUpdate();
                GameManager.Instance.StartCoroutine(__instance.switchBack(inventory));
            }));

            return false;
        }
    }

    [HarmonyPatch(typeof(Inventory))]
    [HarmonyPatch("CanStack")]
    public class SphereII_InventoryCanStack
    {
        public static bool Prefix(Inventory __instance, bool __result, ItemStack _itemStack, ref ItemInventoryData[] ___slots)
        {
            __result = false;
            foreach (ItemInventoryData slot in ___slots)
            {
                if (slot.itemStack.IsEmpty() || slot.itemStack.CanStackWith(_itemStack))
                    __result = true;

            }
            return false;
        }

    }

    [HarmonyPatch(typeof(Inventory))]
    [HarmonyPatch("CanStackNoEmpty")]
    public class SphereII_InventoryCanStackNoEmpty
    {
        public static bool Prefix(Inventory __instance, bool __result, ItemStack _itemStack, ref ItemInventoryData[] ___slots)
        {
            __result = false;
            foreach (ItemInventoryData slot in ___slots)
            {
                if (slot.itemStack.CanStackPartlyWith(_itemStack))
                    __result = true;

            }
            return false;
        }

    }

    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("AttachToEntity")]
    public class SphereII_EntityAliveAttachToEntity
    {
        // Loops around the instructions and removes the return condition.
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SphereII_PlayerMoveController_Logger.Log("Patching AttachToEntity()");

            int counter = 0;

            // Grab all the instructions
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_8)
                {
                    counter++;
                    SphereII_PlayerMoveController_Logger.Log("Adjusting 8 to 10");

                    codes[i].opcode = OpCodes.Ldc_I4; // convert to the right thing
                    codes[i].operand = 10;
                    SphereII_PlayerMoveController_Logger.Log("Done with 8 to 10");
                }

            }

            SphereII_PlayerMoveController_Logger.Log("Done With Patching AttachToEntity()");

            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(EntityVehicle))]
    [HarmonyPatch("hasGasCan")]
    public class SphereII_EntityVehiclehasGasCan
    {
        // Loops around the instructions and removes the return condition.
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SphereII_PlayerMoveController_Logger.Log("Patching hasGasCan()");

            int counter = 0;

            // Grab all the instructions
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_8)
                {
                    counter++;
                    SphereII_PlayerMoveController_Logger.Log("Adjusting 8 to 10");

                    codes[i].opcode = OpCodes.Ldc_I4; // convert to the right thing
                    codes[i].operand = 10;
                    SphereII_PlayerMoveController_Logger.Log("Done with 8 to 10");
                }

            }
            SphereII_PlayerMoveController_Logger.Log("Done With Patching hasGasCan()");

            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(EntityVehicle.VehicleInventory))] // This is a nested class so mainclass.nestedclass
    [HarmonyPatch("SetupSlots")]
    public class SphereII_VehicleInventorySetupSlots
    {
        // Loops around the instructions and removes the return condition.
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SphereII_PlayerMoveController_Logger.Log("Patching SetupSlots()");

            int counter = 0;

            // Grab all the instructions
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_S)
                {
                    codes[i].opcode = OpCodes.Ldc_I4;
                    codes[i].operand = 11;
                }
            }
            SphereII_PlayerMoveController_Logger.Log("Done With Patching SetupSlots()");

            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(Inventory))]
    [HarmonyPatch("Clear")]
    public class SphereII_InventoryClear
    {
        public static void Prefix(Inventory __instance, ref int ___m_HoldingItemIdx, ref int ___m_LastDrawnHoldingItemIndex, ItemInventoryData[] ___slots, ItemInventoryData ___emptyItem)
        {
            for (int i = 0; i < ___slots.Length; i++)
            {
                ___slots[i] = ___emptyItem;
            }
            ___slots = new ItemInventoryData[11];
            __instance.models = new Transform[11];
            ___m_HoldingItemIdx = 0;
        }
    }

    [HarmonyPatch(typeof(EntityPlayerLocal))]
    [HarmonyPatch("dropBackpack")]
    public class SphereII_EntityPlayerLocaldropBackpack
    {
        // Loops around the instructions and removes the return condition.
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SphereII_PlayerMoveController_Logger.Log("Patching dropBackpack()");

            int counter = 0;

            // Grab all the instructions
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_8)
                {
                    counter++;
                    SphereII_PlayerMoveController_Logger.Log("Adjusting 8 to 10");

                    codes[i].opcode = OpCodes.Ldc_I4; // convert to the right thing
                    codes[i].operand = 10;
                    SphereII_PlayerMoveController_Logger.Log("Done with 8 to 10");
                }

            }
            SphereII_PlayerMoveController_Logger.Log("Done With Patching dropBackpack()");

            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(ItemActionQuest))]
    [HarmonyPatch("OnHoldingUpdate")]
    public class SphereII_ItemActionQuestOnHoldingUpdate
    {
        // Loops around the instructions and removes the return condition.
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SphereII_PlayerMoveController_Logger.Log("Patching OnHoldingUpdate()");

            int counter = 0;

            // Grab all the instructions
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_8)
                {
                    counter++;
                    SphereII_PlayerMoveController_Logger.Log("Adjusting 8 to 10");

                    codes[i].opcode = OpCodes.Ldc_I4; // convert to the right thing
                    codes[i].operand = 10;
                    SphereII_PlayerMoveController_Logger.Log("Done with 8 to 10");
                }

            }

            SphereII_PlayerMoveController_Logger.Log("Done With Patching OnHoldingUpdate()");

            return codes.AsEnumerable();
        }
    }
	
    [HarmonyPatch(typeof(ItemActionQuest))]
    [HarmonyPatch("ExecuteAction")]
    public class SphereII_ItemActionQuestExecuteAction
    {
        // Loops around the instructions and removes the return condition.
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SphereII_PlayerMoveController_Logger.Log("Patching ExecuteAction()");

            int counter = 0;

            // Grab all the instructions
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_8)
                {
                    counter++;
                    SphereII_PlayerMoveController_Logger.Log("Adjusting 8 to 10");

                    codes[i].opcode = OpCodes.Ldc_I4; // convert to the right thing
                    codes[i].operand = 10;
                    SphereII_PlayerMoveController_Logger.Log("Done with 8 to 10");
                }

            }

            SphereII_PlayerMoveController_Logger.Log("Done With Patching ExecuteAction()");

            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(XUiC_Toolbelt))]
    [HarmonyPatch("Update")]
    public class SphereII_ToolbeltUpdate
    {
        // Loops around the instructions and removes the return condition.
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SphereII_PlayerMoveController_Logger.Log("Patching Update()");

            int counter = 0;

            // Grab all the instructions
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_8)
                {
                    counter++;
                    SphereII_PlayerMoveController_Logger.Log("Adjusting 8 to 10");

                    codes[i].opcode = OpCodes.Ldc_I4; // convert to the right thing
                    codes[i].operand = 10;
                    SphereII_PlayerMoveController_Logger.Log("Done with 8 to 10");
                }

            }

            SphereII_PlayerMoveController_Logger.Log("Done With Patching Update()");

            return codes.AsEnumerable();
        }
    }
	
    [HarmonyPatch(typeof(XUiC_Toolbelt))]
    [HarmonyPatch("OnOpen")]
    public class SphereII_ToolbeltOnOpen
    {
        // Loops around the instructions and removes the return condition.
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SphereII_PlayerMoveController_Logger.Log("Patching OnOpen()");

            int counter = 0;

            // Grab all the instructions
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_8)
                {
                    counter++;
                    SphereII_PlayerMoveController_Logger.Log("Adjusting 8 to 10");

                    codes[i].opcode = OpCodes.Ldc_I4; // convert to the right thing
                    codes[i].operand = 10;
                    SphereII_PlayerMoveController_Logger.Log("Done with 8 to 10");
                }

            }

            SphereII_PlayerMoveController_Logger.Log("Done With Patching OnOpen()");

            return codes.AsEnumerable();
        }
    }
	
    [HarmonyPatch(typeof(XUiC_Toolbelt))]
    [HarmonyPatch("OnClose")]
    public class SphereII_ToolbeltOnClose
    {
        // Loops around the instructions and removes the return condition.
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SphereII_PlayerMoveController_Logger.Log("Patching OnClose()");

            int counter = 0;

            // Grab all the instructions
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_8)
                {
                    counter++;
                    SphereII_PlayerMoveController_Logger.Log("Adjusting 8 to 10");

                    codes[i].opcode = OpCodes.Ldc_I4; // convert to the right thing
                    codes[i].operand = 10;
                    SphereII_PlayerMoveController_Logger.Log("Done with 8 to 10");
                }

            }

            SphereII_PlayerMoveController_Logger.Log("Done With Patching OnClose()");

            return codes.AsEnumerable();
        }
    }

}