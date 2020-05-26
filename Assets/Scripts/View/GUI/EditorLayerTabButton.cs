using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class EditorLayerTabButton : GUIBase {
    public bool isFront;

    public void OnClick() {
        EditorState newState = EditorState.SetIsFront(GM.editManager2.GetState(), this.isFront);
        GM.editManager2.UpdateState(newState);
    }

    public override void OnUpdateState() {
        this.GetComponent<Button>().interactable = (this.isFront != GM.editManager2.GetState().isFront);
    }
}
