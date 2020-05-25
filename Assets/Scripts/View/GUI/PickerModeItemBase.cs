using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class PickerModeItemBase : SerializedMonoBehaviour {
    EntitySchema entitySchema;
    BgSchema bgSchema;
    PickerModePanelBase pickerModePanelBase;
    Button button;
    bool isBg;
    // set by editor
    public Text text; 

    public void Init(EntitySchema aEntitySchema, PickerModePanelBase aPickerModePanelBase) {
        this.entitySchema = aEntitySchema;
        this.text.text = this.entitySchema.name;
        this.pickerModePanelBase = aPickerModePanelBase;
        this.button = this.gameObject.GetComponent<Button>();
        this.isBg = false;
    }

    public void Init(BgSchema aBgSchema, PickerModePanelBase aPickerModePanelBase) {
        this.bgSchema = aBgSchema;
        this.text.text = aBgSchema.name;
        this.pickerModePanelBase = aPickerModePanelBase;
        this.button = this.gameObject.GetComponent<Button>();
        this.isBg = true;
    }

    // TODO: remove this nasty update later
    // void Update() {
    //     if (!isBg) {
    //         if (this.entitySchema.type == EntityTypeEnum.PLAYER) {
    //             this.button.interactable = (GM.boardData.playerEntityData == null);
    //         }
    //     }
        
    // }

    public void OnClick() {
        if (isBg) {
            this.pickerModePanelBase.OnBgPickerModeItemClicked(this.bgSchema);
        } else {
            this.pickerModePanelBase.OnPickerModeItemClicked(this.entitySchema);
        }
    }
}
