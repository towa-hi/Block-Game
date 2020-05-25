using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class EditorPickerItem : SerializedMonoBehaviour {
    public Object schema;
    Button button;
    Renderer myRenderer;

    public void Init(Object aSchema) {
        this.button = GetComponent<Button>();
        this.schema = aSchema;
        Text text = GetComponentInChildren<Text>();
        text.text = aSchema.name;
    }

    public void OnClick() {
        EditorState newState = EditorState.SetCurrentSchema(GM.editManager2.GetState(), this.schema);
        GM.editManager2.UpdateState(newState);
    }

    public void SetInteractable(bool aInteractable) {
        this.button.interactable = aInteractable;
    }

}
