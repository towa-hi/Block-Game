using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class GUILayerTabButton : EditorStateListener {
    public bool isFront;
    Button button;

    void Awake() {
        this.button = GetComponent<Button>();
        this.button.onClick.AddListener(OnClick);
    }
    public void OnClick() {
        GM.editManager.SetLayer(isFront);
    }

    protected override void OnUpdateEditorState(EditorState aEditorState) {
        this.button.interactable = (this.isFront != aEditorState.isFront);
    }
}
