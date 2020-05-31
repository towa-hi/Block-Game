using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    [Header("Options Panel")]
    public GameObject optionsModePanel;

    public override void OnUpdateEditorState(EditorState aEditorState) {

    }
}

