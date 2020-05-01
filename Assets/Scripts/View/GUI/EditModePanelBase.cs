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

    void Start() {
        SetEntity(null);
    }

    public void SetEntity(EntityBase aEntityBase) {
        print(aEntityBase);
        this.previewStudioBase.SetEntity(aEntityBase);
        this.editModeVerticalLayout.SetActive(aEntityBase != null);
        this.editModeNullText.SetActive(aEntityBase == null);
        if (aEntityBase != null) {
            this.nameText.text = aEntityBase.name;
            this.isFixedToggle.isOn = aEntityBase.isFixed;
        }
    }

    public void OnColorButtonClick(Color aColor) {
        EditManager.Instance.OnEditModeColorPicker(aColor);
    }

    public void OnFixedToggleClick(bool aIsOn) {
        EditManager.Instance.OnEditModeFixedToggleClick(aIsOn);
    }
}
