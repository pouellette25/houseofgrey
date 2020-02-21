using GUI_2;
using HouseOfGrey.MoveableStorage.Scripts;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

class XUiC_StorageBoxLabelWindow : XUiController
{
    private XUiC_TextInput textInput;
    private BlockEntityData _blockEntityData;
    TileEntitySecureLootContainer _tileEntity;
    private TextMesh[] _textMeshes;
    private BoxLabelTextService _boxLabelTextService;

    public override void Init()
    {
        base.Init();
        XUiV_Button viewComponent = (XUiV_Button)GetChildById("clickable").ViewComponent;

        _boxLabelTextService = new BoxLabelTextService();

        textInput = GetChildByType<XUiC_TextInput>();
        textInput.OnChangeHandler += new XUiEvent_InputOnChangedEventHandler(TextInput_OnChangeHandler);
        textInput.OnSubmitHandler += new XUiEvent_InputOnSubmitEventHandler(TextInput_OnSubmitHandler);
        textInput.OnInputAbortedHandler += new XUiEvent_InputOnAbortedEventHandler(TextInput_OnInputAbortedHandler);
        viewComponent.Controller.OnPress += new XUiEvent_OnPressEventHandler(closeButton_OnPress);
    }

    public override void OnOpen()
    {
        base.OnOpen();
        textInput.OnChangeHandler -= new XUiEvent_InputOnChangedEventHandler(TextInput_OnChangeHandler);
        textInput.OnChangeHandler += new XUiEvent_InputOnChangedEventHandler(TextInput_OnChangeHandler);

        textInput.Text = _boxLabelTextService.ReadText(_tileEntity.GetHashCode());

        if (PlayerInputManager.Instance.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
        {
            textInput.SetSelected(true);
        }
        else
        {
            textInput.ShowVirtualKeyboard();
        }

        SetText(textInput.Text, true);

        xui.playerUI.entityPlayer.PlayOneShot("open_sign");
    }

    public override void OnClose()
    {
        _tileEntity.SetModified();
        _tileEntity.SetUserAccessing(false);
        GameManager.Instance.TEUnlockServer(_tileEntity.GetChunk().ClrIdx, _tileEntity.ToWorldPos(), _tileEntity.entityId);
        base.OnClose();
        xui.playerUI.entityPlayer.PlayOneShot("close_sign");

        if (!string.IsNullOrEmpty(textInput.Text))
        {
            SetText(textInput.Text, true);
        }
        else
        {
            SetText("Storage", true);
        }
    }

    public void SetTileEntity(TileEntitySecureLootContainer tileEntity)
    {
        _tileEntity = tileEntity;
        var chunk = tileEntity.GetChunk();
        var blockEntityData = chunk != null ? chunk.GetBlockEntity(tileEntity.ToWorldPos()) : null;

        _blockEntityData = blockEntityData;
        _textMeshes = _blockEntityData.transform.GetComponentsInChildren<TextMesh>();
    }

    public void OnBlockEntityTransformAfterActivated(int blockPosHash)
    {
        var text = _boxLabelTextService.ReadText(blockPosHash);
        SetText(text, false);
    }

    public void OnBlockOnBlockDestroyedBy(int blockPosHash)
    {
        _boxLabelTextService.RemoveNode(blockPosHash);
    }

    public void OnBlockRemoved(int blockPosHash)
    {
        _boxLabelTextService.RemoveNode(blockPosHash);
    }

    private void TextInput_OnInputAbortedHandler(XUiController _sender)
    {
        xui.playerUI.windowManager.Close(WindowGroup.ID);
    }

    private void TextInput_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
    {
        SetText(_text, false);
    }

    private void TextInput_OnSubmitHandler(XUiController _sender, string _text)
    {
        SetText(_text, true);
        xui.playerUI.windowManager.Close(WindowGroup.ID);
    }

    private void closeButton_OnPress(XUiController _sender, OnPressEventArgs _e)
    {
        xui.playerUI.windowManager.Close(WindowGroup.ID);
    }

    private void SetText(string text, bool saveText)
    {
        foreach (var textMesh in _textMeshes)
        {
            textMesh.text = text;
        }

        if (saveText)
        {
            Debug.Log(_tileEntity.ToWorldPos().GetHashCode());

            _boxLabelTextService.WriteText(_tileEntity.ToWorldPos().GetHashCode(), text);
        }
    }
}
