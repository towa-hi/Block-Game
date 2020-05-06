using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class PickerModeItemBase : SerializedMonoBehaviour {
    public EntitySchema entitySchema;
    public PickerModePanelBase pickerModePanelBase;
    // set by editor
    public Text text; 

    public void Init(EntitySchema aEntitySchema, PickerModePanelBase aPickerModePanelBase) {
        this.entitySchema = aEntitySchema;
        this.text.text = GenPickerItemText(this.entitySchema);
        this.pickerModePanelBase = aPickerModePanelBase;
    }

    string GenPickerItemText(EntitySchema aEntitySchema) {
        return aEntitySchema.name;
    }

    public void OnClick() {
        this.pickerModePanelBase.OnPickerModeItemClicked(this.entitySchema);
    }
}
