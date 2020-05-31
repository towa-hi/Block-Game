using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class GUIEditorPanel : EditorStateListener {
    [Header("Set In Editor")]
    [Header("PickerPanel")]
    public GameObject pickerModePanel;
    public GameObject pickerItemMaster;
    public GameObject pickerScrollRectContent;
    [SerializeField]
    List<GUIPickerItem> pickerItemList;
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

    void Awake() {
        HashSet<GameObject> newBotPanels = new HashSet<GameObject>();
        newBotPanels.Add(pickerModePanel);
        newBotPanels.Add(editModePanel);
        newBotPanels.Add(optionsModePanel);
        this.botPanels = newBotPanels;
    }
    public override void OnUpdateEditorState(EditorState aNewEditorState) {
        SetActiveBotPanel(aNewEditorState);
        this.oldEditorState = aNewEditorState;
    }

    void SetActiveBotPanel(EditorState aEditorState) {
        foreach (GameObject botPanel in this.botPanels) {
            botPanel.SetActive(false);
        }
        switch (aEditorState.activeTab) {
            case EditTabEnum.PICKER:
                this.pickerModePanel.SetActive(true);
                SetPickerItems(aEditorState.isFront, aEditorState.selectedSchema);
                break;
            case EditTabEnum.EDIT:
                this.editModePanel.SetActive(true);
                break;
            case EditTabEnum.OPTIONS:
                this.optionsModePanel.SetActive(true);
                break;
        }
    }

    void SetPickerItems(bool aIsFront, Object aSchema) {
        foreach (GUIPickerItem pickerItem in this.pickerItemList) {
            // function to turn picker items on and off depending on aIsFront
        }
    }

    void SetParPickerText(int aPar) {
        this.parPickerText.text = aPar.ToString();
    }

    void SetTitleField(string aTitle) {
        this.titleInputField.SetTextWithoutNotify(aTitle);
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

