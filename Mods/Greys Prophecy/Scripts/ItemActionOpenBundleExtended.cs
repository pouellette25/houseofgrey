using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;

public class ItemActionOpenBundleExtended : ItemActionOpenBundle
{
    private List<int> _originalCreateItemCount;

    public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
    {
        base.ExecuteAction(_actionData, _bReleased);
    }

    public override void ReadFrom(DynamicProperties _props)
    {
        base.ReadFrom(_props);
        _originalCreateItemCount = new List<int>();
        _originalCreateItemCount.AddRange(CreateItemCount.Select(cic => int.Parse(cic)));
    }

    public override bool ExecuteInstantAction(EntityAlive ent, ItemStack stack, bool isHeldItem, XUiC_ItemStack stackController)
    {
        if (Properties.Values.ContainsKey("OutputCountMultiplier") && Properties.Params1.ContainsKey("OutputCountMultiplier"))
        {
            var progressionName = Properties.Values["OutputCountMultiplier"];

            if (!string.IsNullOrEmpty(progressionName))
            {
                var progression = ent.Progression.GetProgressionValue(progressionName);

                if(progression == null)
                {
                    Debug.Log("Could not find the progression item from the value attribute. Multiplier will not be applied");
                    return base.ExecuteInstantAction(ent, stack, isHeldItem, stackController);
                }

                var countString = Properties.Params1["OutputCountMultiplier"];
                if (string.IsNullOrEmpty(countString))
                {
                    Debug.Log("The params1 value was empty, or the OutputCountMultiplier element was not found. Multiplier will not be applied");
                    return base.ExecuteInstantAction(ent, stack, isHeldItem, stackController);
                }

                if(progression.Level == 0)
                {
                    Debug.Log("The progression level is 0. This shouldn't happen in normal gameplay. Multiplier will not be applied");
                    return base.ExecuteInstantAction(ent, stack, isHeldItem, stackController);
                }
                else
                {
                    var counts = countString.Split(',');
                    var multiplier = 1;

                    if (counts.Length > 1)
                    {
                        for(var i = 0; i < counts.Length; i++)
                        {
                            if(i == progression.Level - 1)
                            {
                                multiplier = int.Parse(counts[i]);
                            }
                        }
                    }
                    else
                    {
                        multiplier = int.Parse(counts[0]);
                    }

                    for(var i = 0; i < _originalCreateItemCount.Count; i++)
                    {
                        var itemCount = _originalCreateItemCount[i];
                        var newItemCount = itemCount * multiplier;

                        CreateItemCount[i] = newItemCount.ToString();
                    }
                }
            }
        }

        return base.ExecuteInstantAction(ent, stack, isHeldItem, stackController);
    }
}
