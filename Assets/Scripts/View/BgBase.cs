using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BgBase : SerializedMonoBehaviour {
    public BgData bgData;
    public bool isInTempPos;
    Renderer myRenderer;
    
    public void Init(BgData aBgData) {
        this.myRenderer = GetComponent<Renderer>();
        this.bgData = aBgData;
        ResetViewPosition();
        SetColor(aBgData.defaultColor);
    }
    public void SetViewPosition(Vector2Int aPos) {
        Vector3 zOffset = new Vector3(0, 0, 2f);
        this.transform.position = Util.V2IOffsetV3(aPos, this.bgData.size) + zOffset;
        this.isInTempPos = true;
    }

    public void ResetViewPosition() {
        Vector3 zOffset = new Vector3(0, 0, 2f);
        this.transform.position = Util.V2IOffsetV3(this.bgData.pos, this.bgData.size) + zOffset;
        this.isInTempPos = false;
    }

    // TODO: stinky code
    public void SetColor(Color aColor) {
        foreach (Transform child in this.transform) {
            child.gameObject.GetComponent<Renderer>().material.color = aColor;
        }
        // NOTE: doesnt set children colors yet
    }
}
