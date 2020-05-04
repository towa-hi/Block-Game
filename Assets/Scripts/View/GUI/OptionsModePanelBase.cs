using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class OptionsModePanelBase : SerializedMonoBehaviour {
    public IntPickerBase parIntPickerBase;
    public InputField levelTitleField;

    public void OnLoadButtonClicked() {

    }

    public void OnSaveButtonClicked() {
        EditManager.Instance.SaveLevel(levelTitleField.text, parIntPickerBase.GetCurrentInt());
    }

    public void OnPlaytestButtonClicked() {
        
    }
}
