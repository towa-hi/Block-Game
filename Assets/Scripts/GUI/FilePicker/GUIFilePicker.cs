using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FilePicker {
    // TODO: make this use a callback on enable and on load button so it's more reusable
    
    public class GUIFilePicker : SerializedMonoBehaviour {
        public DirectoryInfo currentDirectory;
        GUIFilePickerItem selectedItem;
        public List<GUIFilePickerItem> itemList;
        public GameObject content;
        public GameObject filePickerItemPrefab;
        public Button loadButton;
        public Button cancelButton;
        public Text titleText;
        
        
        void OnEnable() {
            this.currentDirectory = new DirectoryInfo(Config.PATHTOBOARDS);
            this.titleText.text = this.currentDirectory.FullName;
            FileInfo[] info = this.currentDirectory.GetFiles("*.json");
            foreach (Transform child in this.content.transform) {
                Destroy(child.gameObject);
            }

            this.itemList = new List<GUIFilePickerItem>();
            foreach (FileInfo fileInfo in info) {
                GameObject filePickerItemGameObject = Instantiate(this.filePickerItemPrefab, this.content.transform);
                GUIFilePickerItem filePickerItem = filePickerItemGameObject.GetComponent<GUIFilePickerItem>();
                filePickerItem.Init(fileInfo.Name, OnFilePickerItemClick);
                this.itemList.Add(filePickerItem);
            }
            this.selectedItem = null;
            this.loadButton.onClick.AddListener(OnLoadButtonClick);
            this.cancelButton.onClick.AddListener(OnCancelButtonClick);
        }

        void OnFilePickerItemClick(GUIFilePickerItem aFilePickerItem) {
            if (this.selectedItem != null) {
                this.selectedItem.SetSelection(false);
            }
            this.selectedItem = aFilePickerItem;
            aFilePickerItem.SetSelection(true);
        }

        void OnLoadButtonClick() {
            GM.instance.SetFilePickerActive(false);
            GM.editManager.OnFilePickerLoadButtonClicked(this.selectedItem.filename);
        }

        void OnCancelButtonClick() {
            GM.instance.SetFilePickerActive(false);
        }
    }
}
