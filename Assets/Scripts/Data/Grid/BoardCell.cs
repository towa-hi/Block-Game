using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public struct BoardCell {
    public Vector2Int pos;
    public int? frontEntityId;
    public int? backEntityId;

    public BoardCell(Vector2Int aPos) {
        this.pos = aPos;
        this.frontEntityId = null;
        this.backEntityId = null;
    }

    public bool IsBackEntityStateExit() {
        if (this.backEntityId.HasValue) {
            return GM.boardManager.GetEntityById(this.backEntityId.Value).isExit;
        }
        return false;
    }

    public int? GetCellId(bool aIsFront) {
        return aIsFront ? this.frontEntityId : this.backEntityId;
    }
}
