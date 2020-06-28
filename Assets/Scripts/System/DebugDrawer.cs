﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class DebugDrawer : SerializedMonoBehaviour {

    public bool drawGizmos;

    void OnDrawGizmos() {
        if (this.drawGizmos) {
            BoardCell[,] boardCellArray = GM.boardManager.currentState.boardCellArray;
            for (int x = 0; x < boardCellArray.GetLength(0); x++) {
                for (int y = 0; y < boardCellArray.GetLength(1); y++) {
                    BoardCell boardCell = boardCellArray[x, y];
                    if (boardCell.frontEntityId.HasValue) {
                        Gizmos.color = Color.white;
                        Gizmos.DrawSphere(Util.V2IOffsetV3(boardCell.pos, new Vector2Int(1, 1)) + new Vector3(0, 0, -1f), 0.2f);
                    }
                }
            }
            //
            // foreach (BoardCell boardCell in GM.boardManager.currentState.boardCellArray) {
            //     if (boardCell.frontEntityId.HasValue) {
            //         // if (boardCell.frontEntityId.Value.entityType == EntityTypeEnum.MOB) {
            //         //     Gizmos.color = Color.red;
            //         // }
            //         // else {
            //         //     Gizmos.color = Color.white;
            //         // }
            //
            //         Gizmos.color = Color.white;
            //         Gizmos.DrawSphere(Util.V2IOffsetV3(boardCell.pos, new Vector2Int(1, 1)) + new Vector3(0, 0, -1f), 0.2f);
            //     }
            // }
        }
    }
}
