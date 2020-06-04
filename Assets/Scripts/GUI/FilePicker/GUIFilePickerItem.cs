using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace FilePicker {
    public delegate void OnFilePickerItemClick(GUIFilePickerItem aFilePickerItem);
    
    public class GUIFilePickerItem : SerializedMonoBehaviour {
        public string filename;
        Button button;
        Image buttonImage;
        Color originalColor;
        Text text;
        bool isSelected;

        void Awake() {
            this.button = GetComponent<Button>();
            this.text = GetComponentInChildren<Text>();
            this.filename = "Uninitialized";
            this.isSelected = false;
            this.buttonImage = GetComponent<Image>();
            this.originalColor = this.buttonImage.color;
        }
        
        public void Init(string aFilename, OnFilePickerItemClick aCallback) {
            this.filename = aFilename;
            this.text.text = aFilename;
            this.button.onClick.AddListener(() => aCallback(this));
            
        }

        public void SetSelection(bool aIsSelected) {
            this.isSelected = aIsSelected;
            this.button.enabled = !aIsSelected;
            this.buttonImage.color = aIsSelected ? Color.green : this.originalColor;
        }
    }
}
