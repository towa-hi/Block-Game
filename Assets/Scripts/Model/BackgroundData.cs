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

    public void MoveBg(BgData aBgData, Vector2Int aPos) {
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
}
