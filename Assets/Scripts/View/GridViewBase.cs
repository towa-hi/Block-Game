using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

// isn't actually visible. just a container for gameGrid that reads it 
// instantiates a bunch of CellViewBases as child gameObjects
public class GridViewBase : SerializedMonoBehaviour {
    public GameGrid gameGrid;
    // set by editor
    public CellViewBase cellViewMaster;
    
    public void Init() {

        this.gameGrid = GM.boardData.GetGameGrid();
        this.gameGrid.gridView = this;
        foreach (KeyValuePair<Vector2Int, GameCell> kvp in this.gameGrid.gridDict) {
            CellViewBase newCellViewBase = Instantiate(cellViewMaster, Util.V2IOffsetV3(kvp.Key, new Vector2Int(1, 1)), Quaternion.identity, this.transform);
            newCellViewBase.Init(gameGrid.gridDict[kvp.Key]);
            gameGrid.gridDict[kvp.Key].cellViewBase = newCellViewBase;
        }
    }
}
