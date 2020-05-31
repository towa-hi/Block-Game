// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Sirenix.OdinInspector;

// public class BgData {
//     public BgBase bgBase {
//         get {
//             foreach (BgBase bgBase in GM.boardManager.bgBaseList) {
//                 if (bgBase.bgData == this) {
//                     return bgBase;
//                 }
//             }
//             return null;
//         }
//     }
//     public Vector2Int pos;
//     public Vector2Int size;
//     public Color defaultColor;
//     public string prefabPath;
//     public string name;
//     public bool isBlocking;

//     public BgData(BgSchema aBgSchema, Vector2Int aPos, Color aDefaultColor) {
//         this.pos = aPos;
//         this.size = aBgSchema.size;
//         this.defaultColor = aDefaultColor;
//         this.name = aBgSchema.name;
//         this.prefabPath = aBgSchema.prefabPath;
//         this.isBlocking = aBgSchema.isBlocking;
//     }

//     public List<Vector2Int> GetOccupiedPos() {
//         return Util.V2IInRect(this.pos, this.size);
//     }

//     public void SetPos(Vector2Int aPos) {
//         this.pos = aPos;
//     }

//     public void SetDefaultColor(Color aDefaultColor) {
//         this.defaultColor = aDefaultColor;
//         this.bgBase.SetColor(aDefaultColor);
//     }
// }
