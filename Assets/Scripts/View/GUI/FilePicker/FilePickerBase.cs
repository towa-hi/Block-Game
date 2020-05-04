using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class FilePickerBase : SerializedMonoBehaviour {
    public string currentFileName;
    public List<FilePickerItemPanelBase> itemBaseList;
    // set by editor
    public GameObject content;
    public GameObject filePickerItemPrefabMaster;
    public Button loadButton;
    
    public void Awake() {
        DirectoryInfo dir = new DirectoryInfo(Config.PATHTOLEVELJSON);
        FileInfo[] info = dir.GetFiles("*.json");
        this.itemBaseList = new List<FilePickerItemPanelBase>();
        foreach (FileInfo f in info) {
            GameObject filePickerItem = Instantiate(filePickerItemPrefabMaster, content.transform);
            FilePickerItemPanelBase newItem = filePickerItem.GetComponent<FilePickerItemPanelBase>();
            newItem.Init(f.Name, this);
            this.itemBaseList.Add(newItem);
        }
        this.currentFileName = null;
        this.loadButton.interactable = false;
    }

    public void OnFilePickerItemClick(string aFileName, FilePickerItemPanelBase aItemBase) {
        this.loadButton.interactable = true;
        this.currentFileName = aFileName;
        foreach (FilePickerItemPanelBase itemBase in itemBaseList) {
            itemBase.Highlight(itemBase == aItemBase);
        }
    }

    public void OnLoadButtonClick() {
        EditManager.Instance.LoadLevelFromFilePicker(currentFileName);
    }

    public void OnCancelButtonClick() {
        EditManager.Instance.EndFilePicker();
    }
}
