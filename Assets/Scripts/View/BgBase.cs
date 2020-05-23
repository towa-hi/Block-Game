using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BgBase : SerializedMonoBehaviour {
    public BgData bgData;
    public bool isInTempPos;

    public void Init(BgData aBgData) {
        this.bgData = aBgData;
        ResetViewPosition();
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
}
