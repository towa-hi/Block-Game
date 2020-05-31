﻿using System.Collections;
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
    
    void OnEnable() {
        DirectoryInfo dir = new DirectoryInfo(Config.PATHTOBOARDS);
        FileInfo[] info = dir.GetFiles("*.board");
        // destroy any filepickeritem objects that might already exist
        foreach (Transform child in content.transform) {
            Destroy(child.gameObject);
        }
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

    // public void OnLoadButtonClick() {
    //     if (this.currentFileName != null) {
    //         GM.I.ToggleFullPauseGame(false);
    //         this.gameObject.SetActive(false);
    //         GM.I.LoadBoard(SaveLoad.LoadBoard(this.currentFileName));
    //     }
        
    // }

    // public void OnCancelButtonClick() {
    //     // GM.editManager.EndFilePicker();
    //     GM.I.ToggleFullPauseGame(false);
    //     this.gameObject.SetActive(false);
    // }
}
