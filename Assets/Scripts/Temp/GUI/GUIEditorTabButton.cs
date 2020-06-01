using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class GUIEditorTabButton : EditorStateListener {
    public EditTabEnum editTabEnum;
    Button button;

    void Awake() {
        this.button = GetComponent<Button>();
        this.button.onClick.AddListener(() => GM.editManager.SetActiveTab(this.editTabEnum));
    }

    protected override void OnUpdateEditorState(EditorState aEditorState) {
        this.button.interactable = (this.editTabEnum != aEditorState.activeTab);
    }
}