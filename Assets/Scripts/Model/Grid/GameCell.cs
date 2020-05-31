// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Sirenix.OdinInspector;

// // holds all the information regarding one particular cell in the level grid
// public class GameCell {
//     // set by constructor
//     public Vector2Int pos;
//     public EntityData entityData;
//     public BgData bgData;
//     public Vector2Int push;
//     public bool panelVisible;
//     public Color panelColor;
//     public CellViewBase cellViewBase {
//         get {
//             return GM.gridViewBase.GetCellView(pos);
//         }
//     }

//     public GameCell(Vector2Int aPos) {
//         this.pos = aPos;
//         this.push = Vector2Int.down;
//         this.panelVisible = false;
//         this.panelColor = Color.white;
//     }

//     public void RegisterEntityData(EntityData aEntityData) {
//         this.entityData = aEntityData;
//     }

//     public void SetPanelVisibility(bool aPanelVisible) {
//         this.panelVisible = aPanelVisible;
//         cellViewBase.SetPanelVisbility(aPanelVisible);
//     }

//     public void SetColor(Color aColor) {
//         this.panelColor = aColor;
//         cellViewBase.SetColor(aColor);
//     }
// }
