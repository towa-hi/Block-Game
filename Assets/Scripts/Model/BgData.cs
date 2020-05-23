﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BgData {
    public BgBase bgBase {
        get {
            foreach (BgBase bgBase in GM.boardManager.bgBaseList) {
                if (bgBase.bgData == this) {
                    return bgBase;
                }
            }
            return null;
        }
    }
    public Vector2Int pos;
    public Vector2Int size;
    public Color defaultColor;
    public string prefabPath;
    public string name;

    public List<Vector2Int> GetOccupiedPos() {
        return Util.V2IInRect(this.pos, this.size);
    }

    public void SetPos(Vector2Int aPos) {
        this.pos = aPos;
    }
}
