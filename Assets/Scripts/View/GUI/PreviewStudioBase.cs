using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PreviewStudioBase : SerializedMonoBehaviour {
    public EntityData entityData;
    BgData bgData;
    int originalLayer;
    // set by editor
    public Camera myCamera;
    
    public void Init() {
        this.entityData = null;
        this.bgData = null;
    }
    
    public void SetEntity(EntityData aEntityData) {
        ResetLastEntity();
        if (aEntityData != null) {
            this.originalLayer = aEntityData.entityBase.gameObject.layer;
            this.transform.position = aEntityData.entityBase.transform.position;
            Util.SetLayerRecursively(aEntityData.entityBase.gameObject, 8);
        }
        this.entityData = aEntityData;
    }

    public void SetBg(BgData aBgData) {
        ResetLastEntity();
        if (aBgData != null) {
            this.originalLayer = aBgData.bgBase.gameObject.layer;
            this.transform.position = aBgData.bgBase.transform.position;
            Util.SetLayerRecursively(aBgData.bgBase.gameObject, 8);
        }
        this.bgData = aBgData;
    }

    public void ClearEntity() {
        ResetLastEntity();
        this.entityData = null;
        this.bgData = null;
    }

    void ResetLastEntity() {
        if (this.entityData != null) {
            Util.SetLayerRecursively(this.entityData.entityBase.gameObject, this.originalLayer);
        }
        if (this.bgData != null) {
            Util.SetLayerRecursively(this.bgData.bgBase.gameObject, this.originalLayer);
        }
    }
}
