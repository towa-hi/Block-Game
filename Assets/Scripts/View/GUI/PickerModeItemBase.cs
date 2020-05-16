using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class PickerModeItemBase : SerializedMonoBehaviour {
    EntitySchema entitySchema;
    PickerModePanelBase pickerModePanelBase;
    Button button;
    // set by editor
    public Text text; 

    public void Init(EntitySchema aEntitySchema, PickerModePanelBase aPickerModePanelBase) {
        this.entitySchema = aEntitySchema;
        this.text.text = GenPickerItemText(this.entitySchema);
        this.pickerModePanelBase = aPickerModePanelBase;
        this.button = this.gameObject.GetComponent<Button>();
    }

    // TODO: remove this nasty update later
    void Update() {
        if (entitySchema.type == EntityTypeEnum.PLAYER) {
            this.button.interactable = (GM.boardData.playerEntityData == null);
        }
    }
    string GenPickerItemText(EntitySchema aEntitySchema) {
        return aEntitySchema.name;
    }

    public void OnClick() {
        this.pickerModePanelBase.OnPickerModeItemClicked(this.entitySchema);
    }
}
