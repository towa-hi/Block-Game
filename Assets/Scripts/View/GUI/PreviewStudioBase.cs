using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PreviewStudioBase : SerializedMonoBehaviour {
    public EntityBase entityBase;
    // set by editor
    public Camera myCamera;

    void Awake() {
        this.entityBase = null;
    }
    
    public void SetEntity(EntityBase aEntityBase) {
        // TODO: make this remember the last layer it was

        if (this.entityBase != null) {
            Util.SetLayerRecursively(this.entityBase.gameObject, 0);
        } 

        if (aEntityBase != null) {
            this.transform.position = aEntityBase.transform.position;
            Util.SetLayerRecursively(aEntityBase.gameObject, 8);
        }
        this.entityBase = aEntityBase;
    }
}
