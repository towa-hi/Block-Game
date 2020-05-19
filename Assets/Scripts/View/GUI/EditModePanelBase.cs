using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

// TODO: figure out why longclick delete button doesnt work after going back from playtest mode
public class EditModePanelBase : SerializedMonoBehaviour {
    // set by editor
    public PreviewStudioBase previewStudioBase;
    public GameObject editModeVerticalLayout;
    public GameObject editModeNullText;
    public INodalPanelBase iNodalPanelBase;
    public Text nameText;
    public Toggle isFixedToggle;
    public Button deleteButton;
    public Button extraButton;
    public Button flipButton;
    public void Init() {
        SetEntity(null);
    }

    public void SetEntity(EntityData aEntityData) {
        this.previewStudioBase.SetEntity(aEntityData);
        this.editModeVerticalLayout.SetActive(aEntityData != null);
        this.editModeNullText.SetActive(aEntityData == null);
        this.extraButton.interactable = (aEntityData != null);
        this.flipButton.interactable = (aEntityData != null);
        this.deleteButton.interactable = (aEntityData != null);
        if (aEntityData != null) {
            this.nameText.text = aEntityData.name;
            this.isFixedToggle.SetIsOnWithoutNotify(aEntityData.isFixed);
            this.isFixedToggle.interactable = !aEntityData.isBoundary;
            this.deleteButton.interactable = !aEntityData.isBoundary;
        } else {
            this.isFixedToggle.interactable = true;
        }
        this.iNodalPanelBase.SetEntity(aEntityData);
    }
}
