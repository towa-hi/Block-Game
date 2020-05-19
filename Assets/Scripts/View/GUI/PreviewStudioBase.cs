using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PreviewStudioBase : SerializedMonoBehaviour {
    public EntityData entityData;
    int originalLayer;
    // set by editor
    public Camera myCamera;
    
    public void SetEntity(EntityData aEntityData) {
        if (this.entityData != null) {
            Util.SetLayerRecursively(this.entityData.entityBase.gameObject, this.originalLayer);
        } 
        if (aEntityData != null) {
            this.originalLayer = aEntityData.entityBase.gameObject.layer;
            this.transform.position = aEntityData.entityBase.transform.position;
            Util.SetLayerRecursively(aEntityData.entityBase.gameObject, 8);
        }
        
        this.entityData = aEntityData;
    }
}
