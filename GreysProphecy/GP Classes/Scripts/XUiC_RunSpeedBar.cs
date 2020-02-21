using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


class XUiC_RunSpeedBar : XUiC_HUDStatBar
{
    private XUiV_Label textContent;

    public override bool GetBindingValue(ref string value, BindingItem binding)
    {
        base.GetBindingValue(ref value, binding);
        return true;
    }

    public override bool ParseAttribute(string name, string value, XUiController _parent)
    {
        Debug.Log("ParseAttribute: " + name);
        Debug.Log("ParseAttribute: " + value);
        return true;
    }

    public override void Init()
    {
        base.Init();
        XUiController childById2 = this.GetChildById("TextContent");
        if (childById2 != null)
            this.textContent = (XUiV_Label)childById2.ViewComponent;
    }

    public override void Update(float _dt)
    {
        base.Update(_dt);

        textContent.Text = LocalPlayer.GetVelocityPerSecond().ToString();
    }
}

