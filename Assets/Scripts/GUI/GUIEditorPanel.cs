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
        if (aEditorState.isFront != this.oldEditorState.isFront) {
            print("refreshing picker items");
            foreach (GUIPickerItem pickerItem in this.pickerItemList) {
                pickerItem.gameObject.SetActive(pickerItem.entitySchema.isFront == aEditorState.isFront);
            }
        }
    }

    void SetParPickerText(int aPar) {
        this.parPickerText.text = aPar.ToString();
    }

    void SetTitleField(string aTitle) {
        this.titleInputField.SetTextWithoutNotify(aTitle);
    }

    void SetEditorStateVars(EditorState aEditorState) {
        this.activeTab = aEditorState.activeTab;
        switch (aEditorState.activeTab) {
            case EditTabEnum.PICKER:
                if (!this.pickerModePanel.activeInHierarchy) {
                    this.pickerModePanel.SetActive(true);
                    this.editModePanel.SetActive(false);
                    this.optionsModePanel.SetActive(false);
                }
                SetPickerItems(aEditorState);
                break;
            case EditTabEnum.EDIT:
                if (!this.editModePanel.activeInHierarchy) {
                    this.pickerModePanel.SetActive(false);
                    this.editModePanel.SetActive(true);
                    this.optionsModePanel.SetActive(false);
                }
                EditModePanelSetup(aEditorState);
                break;
            case EditTabEnum.OPTIONS:
                if (!this.editModePanel.activeInHierarchy) {
                    this.pickerModePanel.SetActive(false);
                    this.editModePanel.SetActive(false);
                    this.optionsModePanel.SetActive(true);
                }
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

    public void EditModePanelSetup(EditorState aEditorState) {
        Debug.Assert(aEditorState.activeTab == EditTabEnum.EDIT);
        this.editModeNullTextContainer.SetActive(!aEditorState.hasSelectedEntity);
        this.editModeHorizontalContainer.SetActive(aEditorState.hasSelectedEntity);
        this.editModeVerticalContainer.SetActive(aEditorState.hasSelectedEntity);
        if (aEditorState.hasSelectedEntity) {
            EntityState selectedEntity = GM.editManager.GetSelectedEntity();
            this.previewStudio.SetTarget(selectedEntity);
            this.editModeNameText.text = selectedEntity.entityBase.name;
            this.editModeIsFixedToggle.SetIsOnWithoutNotify(selectedEntity.isFixed);
            this.editModeIsFixedToggle.interactable = !selectedEntity.isBoundary;
            
        }
        
    }
}

