using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class BoardCell {
    public Vector2Int pos;
    public EntityState? frontEntityState;
    public EntityState? backEntityState;
    
    public BoardCell(Vector2Int aPos) {
        this.pos = aPos;
        this.frontEntityState = null;
        this.backEntityState = null;
    }

    public void Reset() {
        this.frontEntityState = null;
    }

    public bool IsExit() {
        if (this.backEntityState.HasValue) {
            if (this.backEntityState.Value.data.isExit) {
                return true;
            }
        }
        return false;
    }
}
