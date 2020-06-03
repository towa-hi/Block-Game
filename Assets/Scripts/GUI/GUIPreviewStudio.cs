using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GUIPreviewStudio : SerializedMonoBehaviour {
    public int id;
    int originalLayer;
    public Camera myCamera;
    EntityBase oldEntityBase;
    
    public void SetTarget(EntityState aEntityState) {
        if (this.oldEntityBase != null) {
            Util.SetLayerRecursively(this.oldEntityBase.gameObject, this.originalLayer);
        }
        this.id = aEntityState.data.id;
        EntityBase entityBase = aEntityState.entityBase;
        this.originalLayer = entityBase.gameObject.layer;
        this.transform.position = entityBase.transform.position;
        Util.SetLayerRecursively(entityBase.gameObject, 8);
        this.oldEntityBase = entityBase;
    }
}
