using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public abstract class EditorStateListener : SerializedMonoBehaviour {
    public virtual void OnEnable() {
        GM.editManager.OnUpdateEditorState += OnUpdateEditorState;
        if (GM.editManager.currentState.isInitialized) {
            OnUpdateEditorState(GM.editManager.currentState);
        }
        else {
            print("EditState was not initialized");
        }
    }

    public virtual void OnDisable() {
        GM.editManager.OnUpdateEditorState -= OnUpdateEditorState;
    }

    protected abstract void OnUpdateEditorState(EditorState aEditorState);
}
