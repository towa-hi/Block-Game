using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

// data structure to represent a grid based state of the game in runtime
// only holds references to entities
public class GameGrid {
    // holds all the GameCells representing coodinates
    public Dictionary<Vector2Int, GameCell> gridDict;
    // set in constructor
    public Vector2Int size;
    // set in editor but doesn't have to be
    public GridViewBase gridView;

    public GameGrid(Vector2Int aSize) {
        this.gridDict = new Dictionary<Vector2Int, GameCell>();
        this.size = aSize;
        for (int x = 0; x < this.size.x; x++) {
            for (int y = 0; y < this.size.y; y++) {
                Vector2Int currentPos = new Vector2Int(x, y);
                GameCell newCell = new GameCell(currentPos);
                gridDict[currentPos] = newCell;
            }
        }
    }

    // tells all the GameCells that entityBase claims to occupy to set their reference to it
    // to unregister an entity, just set null as the param
    public void RegisterEntity(EntityBase aEntityBase) {
        foreach (Vector2Int pos in aEntityBase.GetOccupiedPos()) {
            this.gridDict[pos].RegisterEntity(aEntityBase);
        }
    }

    public EntityBase GetEntityAtPos(Vector2Int aPos) {
        if (IsPosInGrid(aPos)) {
            return gridDict[aPos].entityBase;
        } else {
            return null;
        }
        
    }

    public bool IsPosInGrid(Vector2Int aPos) {
        return Util.IsInside(aPos, Vector2Int.zero, this.size);
    }

    public bool IsRectInGrid(Vector2Int aPos, Vector2Int aSize) {
        return Util.IsRectInside(aPos, aSize, Vector2Int.zero, this.size);
    }
    public bool HasEntitiesBetweenPos(Vector2Int aPos, Vector2Int aSize) {
        for (int x = aPos.x; x < aPos.x + aSize.x; x++) {
            for (int y = aPos.y; y < aPos.y + aSize.y; y++) {
                Vector2Int currentPos = new Vector2Int(x, y);
                if (IsPosInGrid(currentPos)) {
                    if (GetEntityAtPos(currentPos)) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
