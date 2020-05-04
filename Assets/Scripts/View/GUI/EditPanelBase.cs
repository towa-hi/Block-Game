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
}
