using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class EditPanelBase2 : GUIBase {

    // public GameObject activeBotPanel;
    [Header("Set In Editor")]
    [Header("PickerPanel")]
    public GameObject pickerModePanel;
    public GameObject pickerItemMaster;
    public GameObject pickerScrollRectContent;
    [SerializeField]
    List<EditorPickerItem> editorPickerItems;
    [Header("Edit Panel")]
    public GameObject editModePanel;
    public PreviewStudioBase previewStudioBase;
    public GameObject editModeVerticalLayout;
    public GameObject editModeNullTextContainer;
    public INodalPanelBase editModeINodalPanel;
    public Text editModeNameText;
    public Toggle editModeIsFixedToggle;
    public Button editModeDeleteButton;
    public Button editModeFlipButton;
    [Header("Options Panel")]
    public GameObject optionsModePanel;
    public InputField titleInputField;
    public Text parPickerText;

    HashSet<GameObject> botPanels;

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

    public void SetPickerItems(bool aIsFront, Object aSchema) {
        foreach (EditorPickerItem pickerItem in this.editorPickerItems) {
            if (aIsFront) {
                pickerItem.gameObject.SetActive(pickerItem.schema is EntitySchema);
            } else {
                pickerItem.gameObject.SetActive(pickerItem.schema is BgSchema);
            }
            if (aSchema != null) {
                pickerItem.SetInteractable(pickerItem.schema != aSchema);
            } else {
                pickerItem.SetInteractable(true);
            }
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
        SetActiveBotPanel(currentState);
        // TODO: could be optimized to not setPickerItems if on another tab
        SetPickerItems(currentState.isFront, currentState.selectedSchema);
        SetParPickerText(currentState.par);
        SetTitleField(currentState.title);
    }

    void SetActiveBotPanel(EditorState aState) {
        foreach (GameObject botPanel in this.botPanels) {
            botPanel.SetActive(false);
        }
        switch(aState.activeTab) {
            case EditTabEnum.PICKER:
                this.pickerModePanel.SetActive(true);
                break;
            case EditTabEnum.EDIT:
                this.editModePanel.SetActive(true);
                if (aState.isFront) {
                    SetEditModePanel(aState.selectedEntityData);
                } else {
                    SetEditModePanel(aState.selectedBgData);
                }
                
                break;
            case EditTabEnum.OPTIONS:
                this.optionsModePanel.SetActive(true);
                break;
        }
    }

    void SetParPickerText(int aPar) {
        this.parPickerText.text = aPar.ToString();
    }

    void SetTitleField(string aTitle) {
        this.titleInputField.SetTextWithoutNotify(aTitle);
    }
    // called by UI

    public void OnParPickerButtonClick(bool aIsUp) {
        int newPar = GM.boardData.par;
        if (aIsUp) {
            newPar += 1;
        } else {
            newPar -= 1;
        }
        EditorState newState = EditorState.SetPar(GM.editManager2.GetState(), newPar);
        GM.editManager2.UpdateState(newState);
    }

    public void OnTitleField(string aTitle) {
        EditorState newState = EditorState.SetLevelTitle(GM.editManager2.GetState(), aTitle);
        GM.editManager2.UpdateState(newState);
    }

    public void OnEditColorButtonClick(Color aColor) {
        print(aColor);
        if (GM.editManager2.GetState().isFront) {
            GM.editManager2.GetState().selectedEntityData.SetDefaultColor(aColor);
        } else {
            GM.editManager2.GetState().selectedBgData.SetDefaultColor(aColor);
        }
        
    }

    public void SetEditModePanel(EntityData aEntityData) {
        if (aEntityData != null) {
            this.previewStudioBase.SetEntity(aEntityData);
            this.editModeVerticalLayout.SetActive(true);
            this.editModeNullTextContainer.SetActive(false);
            this.editModeNameText.text = aEntityData.name;
            this.editModeIsFixedToggle.gameObject.SetActive(true);
            this.editModeIsFixedToggle.SetIsOnWithoutNotify(aEntityData.isFixed);
            this.editModeIsFixedToggle.interactable = !aEntityData.isBoundary;
            this.editModeDeleteButton.interactable = !aEntityData.isBoundary;
        } else {
            this.previewStudioBase.ClearEntity();
            this.editModeVerticalLayout.SetActive(false);
            this.editModeNullTextContainer.SetActive(true);
        }
    }

    public void SetEditModePanel(BgData aBgData) {
        if (aBgData != null) {
            this.previewStudioBase.SetBg(aBgData);
            this.editModeVerticalLayout.SetActive(true);
            this.editModeNullTextContainer.SetActive(false);
            this.editModeNameText.text = aBgData.name;
            this.editModeIsFixedToggle.gameObject.SetActive(false);
        } else {
            this.previewStudioBase.ClearEntity();
            this.editModeVerticalLayout.SetActive(false);
            this.editModeNullTextContainer.SetActive(true);
        }
    }

}
