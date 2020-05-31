using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public abstract class EditorStateListener : SerializedMonoBehaviour {
    public virtual void OnEnable() {
        GM.editManager.OnUpdateEditorState += OnUpdateEditorState;
        OnUpdateEditorState(GM.editManager.editorState);
    }

    public virtual void OnDisable() {
        GM.editManager.OnUpdateEditorState -= OnUpdateEditorState;
    }

    public abstract void OnUpdateEditorState(EditorState aEditorState);
}
