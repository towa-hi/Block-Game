using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class ColorPickerBase : SerializedMonoBehaviour {
    Color[] colorArray;
    // set in editor
    public EditModePanelBase editModePanelBase;
    void Awake() {
        
    }

    public void OnColorButtonClick(Image aImage) {
        editModePanelBase.OnColorButtonClick(aImage.color);
    }
}
