using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class InfoPanelBase : SerializedMonoBehaviour {
    public EntityBase entityBase;
    // set by editor
    public PreviewStudioBase previewStudioBase;

    void Awake() {
        this.entityBase = null;
    }

    public void SetEntity(EntityBase aEntityBase) {
        this.entityBase = aEntityBase;
        if (this.entityBase != null) {
            previewStudioBase.SetEntity(aEntityBase);
        }
    }


}
