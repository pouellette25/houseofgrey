using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ItemActionStaff : ItemActionCatapult
{
    public override ItemActionData CreateModifierData(
          ItemInventoryData _invData,
          int _indexInEntityOfAction)
    {
        return new ItemActionDataStaff(_invData, _indexInEntityOfAction);
    }

    public override void SwapAmmoType(EntityAlive _entity, int _selectedIndex = -1)
    {
        ItemActionDataStaff actionDataRanged = (ItemActionDataStaff)_entity.inventory.holdingItemData.actionData[0];

        for (var index = 0; index < actionDataRanged.projectileInstance.Count; index++)
        {
            Transform transform = actionDataRanged.projectileInstance[index];
            if (transform != null)
            {
                Object.Destroy(transform.gameObject);
            }
        }

        actionDataRanged.projectileInstance.Clear();

        base.SwapAmmoType(_entity, _selectedIndex);
    }

    protected class ItemActionDataStaff : ItemActionDataCatapult
    {
        public ItemActionDataStaff(ItemInventoryData _invData, int _indexInEntityOfAction)
          : base(_invData, _indexInEntityOfAction)
        {
        }
    }
}
