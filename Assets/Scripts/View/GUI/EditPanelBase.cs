using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class EditPanelBase : SerializedMonoBehaviour {
    Button[] topPanelArray;
    GameObject[] botPanelArray;
    GameObject activeBotPanel;
    [Header("Set In Editor")]
    public GameObject pickerPanel;
    public GameObject editPanel;
    public GameObject optionsPanel;
    public Button pickerButton;
    public Button editButton;
    public Button optionsButton;
    public InputField titleInput;
    public Button optionsModeSaveButton;
    
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
        GM.editManager.SetEditMode(aEditMode);
    }

    public void SetOptionsModeTitleField(string aValue) {
        this.titleInput.SetTextWithoutNotify(aValue);
    }

    // from EditManager

    public void SetEditModeEntity(EntityData aEntityData) {
        this.editPanel.GetComponent<EditModePanelBase>().SetEntity(aEntityData);
    }

    // picker mode

    public void OnPickerModeItemClick(EntitySchema aEntitySchema) {
        GM.editManager.OnPickerModeItemClick(aEntitySchema);
    }

    // edit mode

    public void OnEditModeColorPickerClick(Color aColor) {
        GM.editManager.OnEditModeColorPickerClick(aColor);

    }
    public void OnEditModeFixedToggle(bool aIsFixed) {
        GM.editManager.OnEditModeFixedToggle(aIsFixed);
    }
    
    public void OnEditModeNodeToggle(NodeToggleStruct aNodeToggleStruct) {
        GM.editManager.OnEditModeNodeToggle(aNodeToggleStruct);
    }
    public void OnEditModeExtraButtonClick() {
        GM.editManager.OnEditModeExtraButtonClick();
    }

    public void OnEditModeFlipButtonClick() {
        GM.editManager.OnEditModeFlipButtonClick();
    }

    public void OnEditModeDeleteButtonClick() {
        GM.editManager.OnEditModeDeleteButtonClick();
    }

    // options mode

    public void OnOptionsModeTitleChange(string aValue) {
        this.optionsModeSaveButton.interactable = (aValue.Length != 0);
        GM.editManager.OnOptionsModeTitleChange(aValue);
    }

    public void OnOptionsModeParIntPickerChange(int aValue) {
        GM.editManager.OnOptionsModeParIntPickerChange(aValue);
    }

    public void OnOptionsModeLoadButtonClick() {
        GM.editManager.OnOptionsModeLoadButtonClick();
    }

    public void OnOptionsModeSaveButtonClick() {
        GM.editManager.OnOptionsModeSaveButtonClick();
    }

    public void OnOptionsModePlaytestButtonClick() {
        GM.editManager.OnOptionsModePlaytestButtonClick();
    }
}
