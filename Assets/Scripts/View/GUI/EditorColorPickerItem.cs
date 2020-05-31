using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class EditorColorPickerItem : SerializedMonoBehaviour {
    [SerializeField]
    Color color;
    Button button;
    // EditorColorPicker parent;
    // EditPanelBase2 editPanelBase;

    // public void Init(Color aColor, EditPanelBase2 aEditPanelBase) {
    //     this.color = aColor;
    //     this.button = GetComponent<Button>();
    //     this.editPanelBase = aEditPanelBase;
    //     this.button.onClick.AddListener(delegate{ColorButtonHandler();});
    //     Image image = GetComponent<Image>();
    //     image.color = aColor;
    // }

    void ColorButtonHandler() {
        // this.editPanelBase.OnEditColorButtonClick(this.color);
    }
}
