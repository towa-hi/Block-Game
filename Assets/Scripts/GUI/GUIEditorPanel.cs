using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Schema;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class GUIEditorPanel : EditorStateListener {
    [Header("Set In Editor")]
    [Header("PickerPanel")]
    public GameObject pickerModePanel;
    public GameObject pickerItemMaster;
    public GameObject pickerScrollRectContent;
    [SerializeField] GUIPickerItem selectedItem;
    [SerializeField] bool isPickerItemsInitialized;
    [SerializeField] string searchString;
    [SerializeField] List<GUIPickerItem> pickerItemList;
    [Header("Edit Panel")]
    public GameObject editModePanel;
    public GUIPreviewStudio previewStudio;
    public GameObject editModeVerticalContainer;
    public GameObject editModeHorizontalContainer;
    public GameObject editModeNullTextContainer;
    public Text editModeNameText;
    public Toggle editModeIsFixedToggle;
    public GUIColorPicker editModeColorPicker;
    public GUINodeEditor editModeNodeEditor;
    public Button editModeFlipButton;
    public Button editModeDeleteButton;
    [Header("Options Panel")]
    public GameObject optionsModePanel;
    public InputField titleInputField;
    public Text parPickerText;

    new void OnEnable() {
        base.OnEnable();
        this.pickerItemList = new List<GUIPickerItem>();
        this.isPickerItemsInitialized = false;
        this.searchString = "";
    }

    protected override void OnUpdateEditorState(EditorState aNewEditorState) {
        if (!this.isPickerItemsInitialized) {
            CreatePickerItems(aNewEditorState);
            this.isPickerItemsInitialized = true;
        }
        SetEditorStateVars(aNewEditorState);
    }

    void CreatePickerItems(EditorState aEditorState) {
        foreach (EntitySchema entitySchema in aEditorState.frontContentList) {
            GameObject pickerItemGameObject = Instantiate(this.pickerItemMaster, this.pickerScrollRectContent.transform);
            GUIPickerItem pickerItem = pickerItemGameObject.GetComponent<GUIPickerItem>();
            pickerItem.Init(entitySchema, OnPickerItemClick);
            this.pickerItemList.Add(pickerItem);
        }

        foreach (EntitySchema entitySchema in aEditorState.backContentList) {
            GameObject pickerItemGameObject = Instantiate(this.pickerItemMaster, this.pickerScrollRectContent.transform);
            GUIPickerItem pickerItem = pickerItemGameObject.GetComponent<GUIPickerItem>();
            pickerItem.Init(entitySchema, OnPickerItemClick);
            this.pickerItemList.Add(pickerItem);
        }
        SetPickerItems(aEditorState.isFront);
    }
    
    void SetPickerItems(bool aIsFront) {
        List<GUIPickerItem> newPickerList = this.pickerItemList
            .Where(item => item.entitySchema.isFront == aIsFront)
            .OrderBy(item => item.entitySchema.name)
            .ToList();

        if (this.searchString.Length > 0) {
            List<GUIPickerItem> sortedPickerList = newPickerList
                .Where(item => item.entitySchema.name.Contains(this.searchString))
                .OrderBy(item => item.entitySchema.name.StartsWith(this.searchString)
                    ? (item.entitySchema.name == this.searchString ? 0 : 1)
                    : 2)
                .ToList();
            newPickerList = sortedPickerList;
        }
        
        foreach (GUIPickerItem pickerItem in this.pickerItemList) {
            pickerItem.gameObject.SetActive(newPickerList.Contains(pickerItem));
        }

        foreach (GUIPickerItem pickerItem in newPickerList) {
            pickerItem.GetComponent<RectTransform>().SetAsFirstSibling();
        }
    }

    void SetParPickerText(int aPar) {
        this.parPickerText.text = aPar.ToString();
    }

    void SetTitleField(string aTitle) {
        this.titleInputField.SetTextWithoutNotify(aTitle);
    }

    void SetEditorStateVars(EditorState aEditorState) {
        switch (aEditorState.activeTab) {
            case EditTabEnum.PICKER:
                if (!this.pickerModePanel.activeInHierarchy) {
                    if (aEditorState.selectedSchema == null) {
                        this.selectedItem.SetSelection(false);
                        this.selectedItem = null;
                    }
                    this.pickerModePanel.SetActive(true);
                    this.editModePanel.SetActive(false);
                    this.optionsModePanel.SetActive(false);
                }
                SetPickerItems(aEditorState.isFront);
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
                if (!this.optionsModePanel.activeInHierarchy) {
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
    
    public void SetSearchString(string aString) {
        this.searchString = aString;
        SetPickerItems(GM.editManager.currentState.isFront);
        if (this.selectedItem != null) {
            GM.editManager.ClearSelectedSchema();
            this.selectedItem.SetSelection(false);
            this.selectedItem = null;
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
            this.editModeIsFixedToggle.interactable = !selectedEntity.data.isBoundary;
            this.editModeColorPicker.SetStartingColor(selectedEntity.defaultColor);
            this.editModeNodeEditor.gameObject.SetActive(selectedEntity.hasNodes);
            if (selectedEntity.hasNodes) {
                this.editModeNodeEditor.SetEntity(selectedEntity);
            }
            this.editModeDeleteButton.interactable = !selectedEntity.data.isBoundary;
            this.editModeFlipButton.interactable = !selectedEntity.data.isBoundary;
            
        }
    }

    public void OnPickerItemClick(GUIPickerItem aPickerItem) {
        if (this.selectedItem != null) {
            this.selectedItem.SetSelection(false);
        }
        this.selectedItem = aPickerItem;
        GM.editManager.SetSelectedSchema(aPickerItem.entitySchema);
        aPickerItem.SetSelection(true);
    }
}

