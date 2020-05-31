﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class GUIEditorTabButton : EditorStateListener {
    public EditTabEnum editTabEnum;
    Button button;

    public override void OnEnable() {
        this.button = GetComponent<Button>();
        this.button.onClick.AddListener(OnClick);
        base.OnEnable();
    }

    void OnClick() {
        GM.editManager.SetActiveTab(editTabEnum);
    }

    public override void OnUpdateEditorState(EditorState aEditorState) {
        this.button.interactable = (this.editTabEnum != aEditorState.activeTab);
    }
}