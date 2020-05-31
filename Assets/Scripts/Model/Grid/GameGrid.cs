// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Sirenix.OdinInspector;

// // data structure to represent a grid based state of the game in runtime
// // only holds references to entities
// public class GameGrid {
//     // holds all the GameCells representing coodinates
//     public Dictionary<Vector2Int, GameCell> gridDict;
//     // set in constructor
//     public Vector2Int size;

//     public GameGrid(Vector2Int aSize) {
//         this.gridDict = new Dictionary<Vector2Int, GameCell>();
//         this.size = aSize;
//         for (int x = 0; x < this.size.x; x++) {
//             for (int y = 0; y < this.size.y; y++) {
//                 Vector2Int currentPos = new Vector2Int(x, y);
//                 GameCell newCell = new GameCell(currentPos);
//                 gridDict[currentPos] = newCell;
//             }
//         }
//     }

//     public GameCell GetCell(Vector2Int aPos) {
//         return this.gridDict[aPos];
//     }

//     public Dictionary<Vector2Int, GameCell> GetSlice(Vector2Int aOrigin, Vector2Int aSize) {
//         Dictionary<Vector2Int, GameCell> sliceDict = new Dictionary<Vector2Int, GameCell>();
//         foreach (Vector2Int pos in Util.V2IInRect(aOrigin, aSize)) {
//             if (this.gridDict.ContainsKey(pos)) {
//                 sliceDict[pos] = this.gridDict[pos];
//             }
//         }
//         return sliceDict;
//     }
// }
