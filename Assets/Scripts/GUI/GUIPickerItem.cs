using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public delegate void OnPickerItemClick(GUIPickerItem aPickerItem);

public class GUIPickerItem : SerializedMonoBehaviour {
    public EntitySchema entitySchema;
    Button button;
    Image buttonImage;
    Color originalColor;
    Text text;

    void Awake() {
        this.button = GetComponent<Button>();
        this.buttonImage = GetComponent<Image>();
        this.originalColor = this.buttonImage.color;
        this.text = GetComponentInChildren<Text>();
    }
    
    public void Init(EntitySchema aEntitySchema, OnPickerItemClick aCallback) {
        this.entitySchema = aEntitySchema;
        this.text.text = aEntitySchema.name;
        this.button.onClick.AddListener(() => aCallback(this));
    }

    public void SetSelection(bool aIsSelected) {
        this.button.enabled = !aIsSelected;
        this.buttonImage.color = aIsSelected ? Color.green : this.originalColor;
    }
}
