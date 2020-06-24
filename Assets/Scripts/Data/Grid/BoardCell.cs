using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BoardCell {
    public Vector2Int pos;
    // public EntityState? frontEntityState;
    // public EntityState? backEntityState;
    public int? frontEntityId;
    public int? backEntityId;

    public BoardCell(Vector2Int aPos) {
        this.pos = aPos;
        // this.frontEntityState = null;
        // this.backEntityState = null;
        this.frontEntityId = null;
        this.backEntityId = null;
    }

    // public bool IsBackEntityStateExit() {
    //     if (this.backEntityState.HasValue) {
    //         if (this.backEntityState.Value.isExit) {
    //             return true;
    //         }
    //     }
    //     return false;
    // }

    public bool IsBackEntityStateExit() {
        if (this.backEntityId.HasValue) {
            return GM.boardManager.GetEntityById(this.backEntityId.Value).isExit;
        }
        return false;
    }

    public int? GetCellId(bool aIsFront) {
        return aIsFront ? this.frontEntityId : this.backEntityId;
    }

    // public EntityState? GetEntityState(bool aIsFront) {
    //     return aIsFront ? this.frontEntityState : this.backEntityState;
    // }
}
