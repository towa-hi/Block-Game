using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class EditPanelBase : SerializedMonoBehaviour {
    public GameObject pickerPanel;
    public GameObject editPanel;
    public GameObject optionsPanel;
    public GameObject activeBotPanel;
    public EditManager editManager;
    
    void Awake() {
        SetBotPanel(EditModeEnum.PICKER);
    }
    
    // TODO: change the colors of the other tabs when you select
    public void OnEditTabClick(int aPanelInt) {
        SetBotPanel((EditModeEnum)aPanelInt);
    }

    public void SetBotPanel(EditModeEnum aEditMode) {
        switch (aEditMode) {
            case EditModeEnum.PICKER:
                this.activeBotPanel = this.pickerPanel;
                break;
            case EditModeEnum.EDIT:
                this.activeBotPanel = this.editPanel;
                break;
            case EditModeEnum.OPTIONS:
                this.activeBotPanel = this.optionsPanel;
                break;
        }
        GameObject[] botPanelArray = {this.pickerPanel, this.editPanel, this.optionsPanel};
        foreach (GameObject botPanel in botPanelArray) {
            botPanel.SetActive(botPanel == this.activeBotPanel);
        }
        this.editManager.SetEditMode(aEditMode);
    }
}
