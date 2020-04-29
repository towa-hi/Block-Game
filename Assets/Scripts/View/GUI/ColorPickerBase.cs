using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class ColorPickerBase : SerializedMonoBehaviour {
    Color[] colorArray;
    public InfoPanelBase infoPanelBase;
    void Awake() {
        
    }

    public void OnColorButtonClick(Image aImage) {
        infoPanelBase.entityBase.entityView.SetColor(aImage.color);
    }
}
