using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PreviewStudioBase : SerializedMonoBehaviour {
    public EntityData entityData;
    // set by editor
    public Camera myCamera;

    void Awake() {
        this.entityData = null;
    }
    
    public void SetEntity(EntityData aEntityData) {
        // TODO: make this remember the last layer it was
        if (this.entityData != null) {
            Util.SetLayerRecursively(this.entityData.entityBase.gameObject, 0);
        } 

        if (aEntityData != null) {
            this.transform.position = aEntityData.entityBase.transform.position;
            Util.SetLayerRecursively(aEntityData.entityBase.gameObject, 8);
        }
        this.entityData = aEntityData;
    }
}
