using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class EditPanelBase2 : GUIBase {

    // public GameObject activeBotPanel;
    [Header("Set In Editor")]
    public GameObject pickerItemMaster;
    public GameObject pickerScrollRectContent;
    public GameObject pickerModePanel;
    public GameObject editModePanel;
    public GameObject optionsModePanel;
    HashSet<GameObject> botPanels;
    List<EditorPickerItem> editorPickerItems;

    public override void OnEnable() {
        
        // instantiate both frontContentList and backContentList as game objects
        // hide them depending on isFront
        this.editorPickerItems = new List<EditorPickerItem>();
        EditorState currentState = GM.editManager2.GetState();
        foreach (BgSchema bgSchema in currentState.backContentList) {
            GameObject pickerItem = Instantiate(this.pickerItemMaster, this.pickerScrollRectContent.transform);
            EditorPickerItem editorPickerItem = pickerItem.GetComponent<EditorPickerItem>();
            editorPickerItem.Init(bgSchema);
            this.editorPickerItems.Add(editorPickerItem);
        }
        foreach(EntitySchema entitySchema in currentState.frontContentList) {
            GameObject pickerItem = Instantiate(this.pickerItemMaster, this.pickerScrollRectContent.transform);
            EditorPickerItem editorPickerItem = pickerItem.GetComponent<EditorPickerItem>();
            editorPickerItem.Init(entitySchema);
            this.editorPickerItems.Add(editorPickerItem);
        }
        base.OnEnable();
    }

    public override void OnDisable() {
        foreach (Transform child in this.pickerScrollRectContent.transform) {
            Destroy(child.gameObject);
        }
        base.OnDisable();
    }

    public void SetPickerItems(bool aIsFront) {
        foreach (EditorPickerItem pickerItem in this.editorPickerItems) {
            if (aIsFront) {
                pickerItem.gameObject.SetActive(pickerItem.schema is EntitySchema);
            } else {
                pickerItem.gameObject.SetActive(pickerItem.schema is BgSchema);
            }
            pickerItem.SetInteractable(pickerItem.schema != GM.editManager2.GetState().selectedSchema);
        }
    }

    void Awake() {
        this.botPanels = new HashSet<GameObject>();
        this.botPanels.Add(pickerModePanel);
        this.botPanels.Add(editModePanel);
        this.botPanels.Add(optionsModePanel);
        // SetActivePanel(this.pickerModePanel);
    }    

    public override void OnUpdateState() {
        EditorState currentState = GM.editManager2.GetState();
        SetActiveBotPanel(currentState.activeTab);
        // TODO: could be optimized to not setPickerItems if on another tab
        SetPickerItems(currentState.isFront);
    }

    void SetActiveBotPanel(EditTabEnum aActiveTab) {
        foreach (GameObject botPanel in this.botPanels) {
            botPanel.SetActive(false);
        }
        switch(aActiveTab) {
            case EditTabEnum.PICKER:
                this.pickerModePanel.SetActive(true);
                break;
            case EditTabEnum.EDIT:
                this.editModePanel.SetActive(true);
                break;
            case EditTabEnum.OPTIONS:
                this.optionsModePanel.SetActive(true);
                break;
        }
    }
}
