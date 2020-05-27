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
    public GameObject editModeVerticalContainer;
    public GameObject editModeNullTextContainer;
    public INodalPanelBase editModeINodalPanel;
    public Text editModeNameText;
    public GameObject editModeHorizontalContainer;
    public Toggle editModeIsFixedToggle;
    public Button editModeDeleteButton;
    public Button editModeFlipButton;
    [Header("Options Panel")]
    public GameObject optionsModePanel;
    public InputField titleInputField;
    public Text parPickerText;
    public GameObject filePicker;

    HashSet<GameObject> botPanels;

    public override void OnEnable() {
        this.botPanels = new HashSet<GameObject>();
        this.botPanels.Add(pickerModePanel);
        this.botPanels.Add(editModePanel);
        this.botPanels.Add(optionsModePanel);
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
        this.previewStudioBase.Init();
        base.OnEnable();
    }

    public override void OnDisable() {
        foreach (Transform child in this.pickerScrollRectContent.transform) {
            Destroy(child.gameObject);
        }
        base.OnDisable();
    }

    public override void OnUpdateState() {
        EditorState currentState = GM.editManager2.GetState();
        SetActiveBotPanel(currentState);
        // TODO: could be optimized to not setPickerItems if on another tab
        
        
        SetTitleField(currentState.title);
    }

    void SetActiveBotPanel(EditorState aState) {
        foreach (GameObject botPanel in this.botPanels) {
            botPanel.SetActive(false);
        }
        switch(aState.activeTab) {
            case EditTabEnum.PICKER:
                this.pickerModePanel.SetActive(true);
                SetPickerItems(aState.isFront, aState.selectedSchema);
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
                SetParPickerText(aState.par);
                SetTitleField(aState.title);
                break;
        }
    }

    void SetPickerItems(bool aIsFront, Object aSchema) {
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

    void SetParPickerText(int aPar) {
        this.parPickerText.text = aPar.ToString();
    }

    void SetTitleField(string aTitle) {
        this.titleInputField.SetTextWithoutNotify(aTitle);
    }

    void SetEditModePanel(EntityData aEntityData) {
        // TODO: remove fixed checkbox from mobs and stuff
        if (aEntityData != null) {
            this.previewStudioBase.SetEntity(aEntityData);
            this.editModeVerticalContainer.SetActive(true);
            this.editModeNullTextContainer.SetActive(false);
            this.editModeNameText.text = aEntityData.name;
            this.editModeIsFixedToggle.gameObject.SetActive(true);
            this.editModeIsFixedToggle.SetIsOnWithoutNotify(aEntityData.isFixed);
            this.editModeINodalPanel.gameObject.SetActive(aEntityData.entityBase.GetCachedIComponent<INodal>() != null);
            this.editModeHorizontalContainer.SetActive(true);

            this.editModeFlipButton.interactable = !aEntityData.isBoundary;
            this.editModeIsFixedToggle.interactable = !aEntityData.isBoundary;
            this.editModeDeleteButton.interactable = !aEntityData.isBoundary;
        } else {
            this.previewStudioBase.ClearEntity();
            this.editModeVerticalContainer.SetActive(false);
            this.editModeNullTextContainer.SetActive(true);
            this.editModeHorizontalContainer.SetActive(false);
        }
    }

    void SetEditModePanel(BgData aBgData) {
        if (aBgData != null) {
            this.previewStudioBase.SetBg(aBgData);
            this.editModeVerticalContainer.SetActive(true);
            this.editModeNullTextContainer.SetActive(false);
            this.editModeNameText.text = aBgData.name;
            this.editModeIsFixedToggle.gameObject.SetActive(false);
            this.editModeINodalPanel.gameObject.SetActive(false);
            this.editModeHorizontalContainer.SetActive(true);
            this.editModeFlipButton.interactable = false;
            this.editModeDeleteButton.interactable = true;
        } else {
            this.previewStudioBase.ClearEntity();
            this.editModeVerticalContainer.SetActive(false);
            this.editModeNullTextContainer.SetActive(true);
            this.editModeHorizontalContainer.SetActive(false);
        }
    }


    // called by UI

    public void OnIsFixedToggle(bool aIsOn) {
        EditorState currentState = GM.editManager2.GetState();
        Debug.Assert(currentState.selectedEntityData != null);
        currentState.selectedEntityData.isFixed = aIsOn;
        GM.editManager2.UpdateState(currentState);
    }

    public void OnEditColorButtonClick(Color aColor) {
        print(aColor);
        if (GM.editManager2.GetState().isFront) {
            GM.editManager2.GetState().selectedEntityData.SetDefaultColor(aColor);
        } else {
            GM.editManager2.GetState().selectedBgData.SetDefaultColor(aColor);
        }
    }

    public void OnEditDeleteButtonClick() {
        EditorState currentState = GM.editManager2.GetState();
        Debug.Assert(currentState.selectedEntityData != null);
        this.previewStudioBase.ClearEntity();
        if (currentState.isFront) {
            GM.boardManager.DestroyEntity(currentState.selectedEntityData);
        } else {
            GM.boardManager.DestroyBg(currentState.selectedBgData);
        }
        GM.editManager2.UpdateState(EditorState.ClearSelection(currentState));
    }

    public void OnEditFlipButtonClick() {
        EditorState currentState = GM.editManager2.GetState();
        Debug.Assert(currentState.selectedEntityData != null);
        GM.boardManager.FlipEntityAndView(currentState.selectedEntityData);
    }


    public void OnOptionsTitleField(string aTitle) {
        EditorState newState = EditorState.SetLevelTitle(GM.editManager2.GetState(), aTitle);
        GM.editManager2.UpdateState(newState);
    }

    public void OnOptionsParButtonClick(bool aIsUp) {
        int newPar = GM.boardData.par;
        if (aIsUp) {
            newPar += 1;
        } else {
            newPar -= 1;
        }
        EditorState newState = EditorState.SetPar(GM.editManager2.GetState(), newPar);
        GM.editManager2.UpdateState(newState);
    }

    public void OnOptionsLoadButtonClick() {
        ActivateFilePicker();

    }

    public void OnOptionsSaveButtonClick() {
        if (SaveLoad.IsValidSave(GM.boardData)) {
            SaveLoad.SaveBoard(GM.boardData);
        } else {
            print("invalid save");
        }
    }

    public void OnOptionsPlaytestButtonClick() {
        if (SaveLoad.IsValidSave(GM.boardData)) {
            SaveLoad.SaveBoard(GM.boardData, true);
            GM.I.SetGameMode(GameModeEnum.PLAYTESTING);
        } else {
            print("invalid save");
        }
    }

    void ActivateFilePicker() {
        GM.I.ToggleFullPauseGame(true);
        this.filePicker.SetActive(true);
    }
}
