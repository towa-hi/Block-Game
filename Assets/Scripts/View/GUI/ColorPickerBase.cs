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
    // set in editor
    public OnColorPickerClick onColorPickerClick = new OnColorPickerClick();

    public void OnColorButtonClick(Image aImage) {
        onColorPickerClick.Invoke(aImage.color);
    }
}
