using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class EditPanelBase : SerializedMonoBehaviour {
    public GameObject pickerPanel;
    public GameObject editPanel;
    public GameObject optionsPanel;
    public GameObject activeBotPanel;
    public Button pickerButton;
    public Button editButton;
    public Button optionsButton;

    public Button optionsModeSaveButton;
    // public EditManager editManager;
    public EditManager editManager;
    Button[] topPanelArray;
    GameObject[] botPanelArray;
    
    void Awake() {
        this.topPanelArray = new Button[] {this.pickerButton, this.editButton, this.optionsButton};
        this.botPanelArray = new GameObject[] {this.pickerPanel, this.editPanel, this.optionsPanel};
        SetBotPanel(EditModeEnum.PICKER);
    }

    public void OnEditTabClick(int aPanelInt) {
        SetBotPanel((EditModeEnum)aPanelInt);
    }

    public void SetBotPanel(EditModeEnum aEditMode) {
        switch (aEditMode) {
            case EditModeEnum.PICKER:
                this.activeBotPanel = this.pickerPanel;
                this.pickerButton.interactable = false;
                this.editButton.interactable = true;
                this.optionsButton.interactable = true;
                break;
            case EditModeEnum.EDIT:
                this.activeBotPanel = this.editPanel;
                this.pickerButton.interactable = true;
                this.editButton.interactable = false;
                this.optionsButton.interactable = true;
                break;
            case EditModeEnum.OPTIONS:
                this.activeBotPanel = this.optionsPanel;
                this.pickerButton.interactable = true;
                this.editButton.interactable = true;
                this.optionsButton.interactable = false;
                break;
        }
        foreach (GameObject botPanel in botPanelArray) {
            botPanel.SetActive(botPanel == this.activeBotPanel);
        }
        this.editManager.SetEditMode(aEditMode);
    }

    // from EditManager

    public void SetEditModeEntity(EntityData aEntityData) {
        this.editPanel.GetComponent<EditModePanelBase>().SetEntity(aEntityData);
    }

    // picker mode

    public void OnPickerModeItemClick(EntitySchema aEntitySchema) {
        this.editManager.OnPickerModeItemClick(aEntitySchema);
    }

    // edit mode

    public void OnEditModeColorPickerClick(Color aColor) {
        this.editManager.OnEditModeColorPickerClick(aColor);

    }
    public void OnEditModeFixedToggle(bool aIsFixed) {
        this.editManager.OnEditModeFixedToggle(aIsFixed);
    }
    
    public void OnEditModeExtraButtonClick() {
        this.editManager.OnEditModeExtraButtonClick();
    }

    public void OnEditModeFlipButtonClick() {
        this.editManager.OnEditModeFlipButtonClick();
    }

    public void OnEditModeDeleteButtonClick() {
        this.editManager.OnEditModeDeleteButtonClick();
    }

    // options mode

    public void OnOptionsModeTitleChange(string aValue) {
        this.optionsModeSaveButton.interactable = (aValue.Length != 0);
        this.editManager.OnOptionsModeTitleChange(aValue);
    }

    public void OnOptionsModeParIntPickerChange(int aValue) {
        this.editManager.OnOptionsModeParIntPickerChange(aValue);
    }

    public void OnOptionsModeLoadButtonClick() {
        this.editManager.OnOptionsModeLoadButtonClick();
    }

    public void OnOptionsModeSaveButtonClick() {
        this.editManager.OnOptionsModeSaveButtonClick();
    }

    public void OnOptionsModePlaytestButtonClick() {
        this.editManager.OnOptionsModePlaytestButtonClick();
    }
}
