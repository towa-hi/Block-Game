using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class FilePickerItemPanelBase : SerializedMonoBehaviour {
    public string fileName;
    public Text text;
    FilePickerBase filePickerBase;

    public void Init(string aFileName, FilePickerBase aFilePickerBase) {
        this.fileName = aFileName;
        this.text.text = aFileName;
        this.filePickerBase = aFilePickerBase;
    }

    public void OnClick() {
        this.filePickerBase.OnFilePickerItemClick(this.fileName, this);
    }

    public void Highlight(bool aHighlighted) {
        if (aHighlighted) {
            GetComponent<Image>().color = Color.green;
        } else {
            GetComponent<Image>().color = Color.white;
        }
    }
}
