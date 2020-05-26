using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BackgroundData {
    public HashSet<BgData> bgDataSet;
    public BoardData boardData;
    public Vector2Int size {
        get {return this.boardData.size;}
    }
    public GameGrid gameGrid {
        get {return this.boardData.gameGrid;}
    }

    public BackgroundData(BoardData aBoardData) {
        this.boardData = aBoardData;
        this.bgDataSet = new HashSet<BgData>();
    }

    public void RegisterBgData(BgData aBgData) {
        this.bgDataSet.Add(aBgData);
        foreach (Vector2Int currentPos in aBgData.GetOccupiedPos()) {
            this.gameGrid.GetCell(currentPos).bgData = aBgData;
        }
    }

    public void UnRegisterBgData(BgData aBgData) {
        this.bgDataSet.Remove(aBgData);
        foreach (Vector2Int currentPos in aBgData.GetOccupiedPos()) {
            this.gameGrid.GetCell(currentPos).bgData = null;
        }
    }

    public void MoveBg(Vector2Int aPos, BgData aBgData) {
        foreach (Vector2Int currentPos in aBgData.GetOccupiedPos()) {
            this.gameGrid.GetCell(currentPos).bgData = null;
        }
        aBgData.SetPos(aPos);
        foreach (Vector2Int currentPos in aBgData.GetOccupiedPos()) {
            this.gameGrid.GetCell(currentPos).bgData = aBgData;
        }
    }

    
    public BgData GetBgDataAtPos(Vector2Int aPos) {
        if (IsPosInBoard(aPos)) {
            return this.gameGrid.GetCell(aPos).bgData;
        } else {
            return null;
        }
    }

    public bool IsPosInBoard(Vector2Int aPos) {
        return Util.IsInside(aPos, Vector2Int.zero, this.size);
    }

    public bool IsRectEmpty(Vector2Int aOrigin, Vector2Int aSize, BgData aIgnoreEntity = null) {
        if (!this.boardData.IsRectInBoard(aOrigin, aSize)) {
            return false;
        }
        foreach (KeyValuePair<Vector2Int, GameCell> kvp in this.gameGrid.GetSlice(aOrigin, aSize)) {
            if (kvp.Value.bgData != null) {
                if (aIgnoreEntity != null) {
                    if (aIgnoreEntity != kvp.Value.bgData) {
                        return false;
                    }
                } else {
                    return false;
                }
            }
        }
        return true;
    }

    public bool CanEditorPlaceBgSchema(Vector2Int aPos, BgSchema aBgSchema) {
        if (IsRectEmpty(aPos, aBgSchema.size)) {
            return true;
        } else {
            return false;
        }
    }

}
