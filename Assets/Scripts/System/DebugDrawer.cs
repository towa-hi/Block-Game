using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class DebugDrawer : SerializedMonoBehaviour {

    public bool drawGizmos;

    void OnDrawGizmos() {
        if (this.drawGizmos) {
            foreach (BoardCell boardCell in GM.boardManager.boardCellDict.Values) {
                if (boardCell.frontEntityState.HasValue) {
                    if (boardCell.frontEntityState.Value.data.entityType == EntityTypeEnum.MOB) {
                        Gizmos.color = Color.red;
                    }
                    else {
                        Gizmos.color = Color.white;
                    }

                    Gizmos.DrawSphere(Util.V2IOffsetV3(boardCell.pos, new Vector2Int(1, 1)) + new Vector3(0, 0, -1f), 0.2f);
                }
            }
        }
    }
}
