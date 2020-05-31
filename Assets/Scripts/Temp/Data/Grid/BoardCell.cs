using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public struct BoardCell {
    public Vector2Int pos;
    public EntityState? entityState;

    public BoardCell(Vector2Int aPos, EntityState? aEntityState = null) {
        this.pos = aPos;
        this.entityState = aEntityState;
    }
}
