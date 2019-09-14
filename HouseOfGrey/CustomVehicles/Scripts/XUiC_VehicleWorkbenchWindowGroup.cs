﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class XUiC_VehicleWorkbenchWindowGroup : XUiC_CraftingWindowGroup
{
    public override void OnOpen()
    {
        try
        {
            base.OnOpen();
        }
        catch (InvalidCastException ex)
        {
            Debug.LogError("XUiC_VehicleWorkbenchWindowGroup.OnOpen Invalid Case" + ex.Message + " Stack Trace:  " + ex.StackTrace);
        }
        catch(Exception ex)
        {
            Debug.LogError("XUiC_VehicleWorkbenchWindowGroup.OnOpen " + ex.Message + " Stack Trace:  " + ex.StackTrace);
        }
    }
}