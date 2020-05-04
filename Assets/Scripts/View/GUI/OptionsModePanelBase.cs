using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class OptionsModePanelBase : SerializedMonoBehaviour {
    public IntPickerBase parIntPickerBase;
    public InputField levelTitleField;
    public Button loadButton;
    public Button saveButton;
    public Button playtestButton;

    public void OnLevelTitleFieldChanged(string aText) {
        this.saveButton.interactable = (aText.Length != 0);
    }
    public void OnLoadButtonClicked() {
        EditManager.Instance.StartFilePicker();
    }

    public void OnSaveButtonClicked() {
        EditManager.Instance.SaveLevel(levelTitleField.text, parIntPickerBase.GetCurrentInt());
    }

    public void OnPlaytestButtonClicked() {
        
    }
}
