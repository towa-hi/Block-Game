using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Sirenix.OdinInspector;

[System.Serializable]
public class OnColorPickerClick : UnityEvent<Color>{};

public class ColorPickerBase : SerializedMonoBehaviour {
    Color[] colorArray;
    public GameObject colorPickerBar;
    public GameObject colorButtonMaster;
    [Header("Set In Editor")]
    public OnColorPickerClick onColorPickerClick = new OnColorPickerClick();

    public void OnColorButtonClick(Image aImage) {
        this.onColorPickerClick.Invoke(aImage.color);
    }
}
