using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Object = UnityEngine.Object;

public class GUIEditorPanel : EditorStateListener {
    [Header("Set In Editor")]
    [Header("PickerPanel")]
    public GameObject pickerModePanel;
    public GameObject pickerItemMaster;
    public GameObject pickerScrollRectContent;
    bool isPickerItemsInitialized;
    [SerializeField] private List<GUIPickerItem> pickerItemList;
    HashSet<EntitySchema> fullSchemaList;
    [Header("Edit Panel")]
    public GameObject editModePanel;
    public GUIPreviewStudio previewStudio;
    public GameObject editModeVerticalContainer;
    public GameObject editModeHorizontalContainer;
    public GameObject editModeNullTextContainer;
    public Text editModeNameText;
    public Toggle editModeIsFixedToggle;
    public Button editModeFlipButton;
    public Button editModeDeleteButton;
    [Header("Options Panel")]
    public GameObject optionsModePanel;
    public InputField titleInputField;
    public Text parPickerText;
    public GameObject filePicker;

    HashSet<GameObject> botPanels;
    EditorState oldEditorState;
    EditTabEnum activeTab;
    
    private void Awake()
    {
        this.pickerItemList = new List<GUIPickerItem>();
        this.fullSchemaList = new HashSet<EntitySchema>();
        HashSet<GameObject> newBotPanels = new HashSet<GameObject> {pickerModePanel, editModePanel, optionsModePanel};
        this.botPanels = newBotPanels;
        this.isPickerItemsInitialized = false;
    }

    protected override void OnUpdateEditorState(EditorState aNewEditorState) {
        if (!this.isPickerItemsInitialized) {
            CreatePickerItems(aNewEditorState);
            this.isPickerItemsInitialized = true;
        }
        SetEditorStateVars(aNewEditorState);
        this.oldEditorState = aNewEditorState;
    }

    void CreatePickerItems(EditorState aEditorState) {
        foreach (EntitySchema entitySchema in aEditorState.frontContentList) {
            GameObject pickerItemGameObject = Instantiate(this.pickerItemMaster, this.pickerScrollRectContent.transform);
            GUIPickerItem pickerItem = pickerItemGameObject.GetComponent<GUIPickerItem>();
            pickerItem.Init(entitySchema);
            this.pickerItemList.Add(pickerItem);
        }
    }
    
    void SetPickerItems(EditorState aEditorState) {
        foreach (GUIPickerItem pickerItem in this.pickerItemList) {
            pickerItem.gameObject.SetActive(pickerItem.entitySchema.isFront == aEditorState.isFront);
        }
    }

    void SetParPickerText(int aPar) {
        this.parPickerText.text = aPar.ToString();
    }

    void SetTitleField(string aTitle) {
        this.titleInputField.SetTextWithoutNotify(aTitle);
    }

    void SetEditorStateVars(EditorState aEditorState) {
        
        foreach (GameObject botPanel in this.botPanels) {
            botPanel.SetActive(false);
        }
        this.activeTab = aEditorState.activeTab;
        switch (aEditorState.activeTab) {
            case EditTabEnum.PICKER:
                this.pickerModePanel.SetActive(true);
                SetPickerItems(aEditorState);
                break;
            case EditTabEnum.EDIT:
                this.editModePanel.SetActive(true);
                break;
            case EditTabEnum.OPTIONS:
                this.optionsModePanel.SetActive(true);
                SetTitleField(GM.boardManager.currentState.title);
                SetParPickerText(GM.boardManager.currentState.par);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void OnTitleFieldSet(string aInput) {
        GM.boardManager.SetTitle(aInput);
    }
    
    // void SetEditModePanel(EntityState? aEntityState) {
    //     if (aEntityState != null) {
    //         this.previewStudio.SetEntity(aEntityState);
    //         this.editModeVerticalContainer.SetActive(true);
    //         this.editModeNullTextContainer.SetActive(false);
    //         this.editModeNameText.text = aEntityState.pos;
    //     }
    // }
}

