using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class GUIPickerItem : SerializedMonoBehaviour {
    public EntitySchema entitySchema;
    public Button button;
    public Text text;
    void Awake() {
        this.button.onClick.AddListener(() => GM.editManager.SetSelectedSchema(this.entitySchema));
    }

    public void Init(EntitySchema aEntitySchema) {
        this.entitySchema = aEntitySchema;
        this.text.text = aEntitySchema.name;
    }
}
