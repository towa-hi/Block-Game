using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class GUIColorPicker : SerializedMonoBehaviour {
    public ColorPicker colorPicker;
    public Button submitButton;
    

    public void SetStartingColor(Color aColor) {
        this.colorPicker = GetComponentInChildren<ColorPicker>();
        this.colorPicker.CurrentColor = aColor;
    }

    public void OnSubmitButtonClick() {
        GM.editManager.OnColorSubmit(this.colorPicker.CurrentColor);
    }
}
