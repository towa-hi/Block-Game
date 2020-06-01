using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public abstract class EditorStateListener : SerializedMonoBehaviour {
    public virtual void OnEnable() {
        GM.editManager.OnUpdateEditorState += OnUpdateEditorState;
        OnUpdateEditorState(GM.editManager.currentState);
    }

    public virtual void OnDisable() {
        GM.editManager.OnUpdateEditorState -= OnUpdateEditorState;
    }

    protected abstract void OnUpdateEditorState(EditorState aEditorState);
}
