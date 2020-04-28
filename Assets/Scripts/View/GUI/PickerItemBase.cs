using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class PickerItemBase : SerializedMonoBehaviour {
    public EntitySchema entitySchema;
    // set by editor
    public Text text; 

    public void Init(EntitySchema aEntitySchema) {
        this.entitySchema = aEntitySchema;
        this.text.text = GenPickerItemText(this.entitySchema);

    }

    string GenPickerItemText(EntitySchema aEntitySchema) {
        return aEntitySchema.name;
    }

    public void OnClick() {
        EditManager.Instance.OnPickerItemSelect(this.entitySchema);
    }

}
