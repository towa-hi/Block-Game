using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class EditModePanelBase : SerializedMonoBehaviour {
    // set by editor
    public PreviewStudioBase previewStudioBase;
    public GameObject editModeVerticalLayout;
    public GameObject editModeNullText;
    public Text nameText;
    public Toggle isFixedToggle;
    public LongClickButton deleteButton;

    void Start() {
        SetEntity(null);
    }

    public void SetEntity(EntityData aEntityData) {
        this.previewStudioBase.SetEntity(aEntityData);
        this.editModeVerticalLayout.SetActive(aEntityData != null);
        this.editModeNullText.SetActive(aEntityData == null);
        if (aEntityData != null) {
            this.nameText.text = aEntityData.name;
            this.isFixedToggle.isOn = aEntityData.isFixed;
            this.isFixedToggle.interactable = !aEntityData.isBoundary;
            this.deleteButton.interactable = !aEntityData.isBoundary;
        }
    }
}
