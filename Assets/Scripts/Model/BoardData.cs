using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BoardData {

    public GameGrid gameGrid;
    public HashSet<EntityData> entityDataSet;
    public HashSet<EntityData> graveyardSet;

    public EntityData playerEntityData;
    public string title;
    public string creator;
    public int par;
    public Vector2Int size;
    public int attempts;
    public bool isInitialized;

    public BoardData() {
        this.entityDataSet = new HashSet<EntityData>();
        this.title = "largeBlankBoard";
        this.creator = Config.USERNAME;
        this.par = 5;
        this.size = new Vector2Int(40, 20);
        this.gameGrid = new GameGrid(this.size);
        this.attempts = 0;
        
    }

    public void RegisterEntityData(EntityData aEntityData) {
        this.entityDataSet.Add(aEntityData);
        foreach (Vector2Int currentPos in aEntityData.GetOccupiedPos()) {
            this.gameGrid.GetCell(currentPos).entityData = aEntityData;
        }
        Debug.Log("BoardData - RegisterEntity: " + aEntityData.name);
    }

    public void UnRegisterEntityData(EntityData aEntityData) {
        this.entityDataSet.Remove(aEntityData);
        foreach (Vector2Int currentPos in aEntityData.GetOccupiedPos()) {
            this.gameGrid.GetCell(currentPos).entityData = null;
        }
        Debug.Log("BoardData - UnRegisterEntity: " + aEntityData.name);
    }

    public void BanishEntity(EntityData aEntityData) {
        foreach (Vector2Int currentPos in aEntityData.GetOccupiedPos()) {
            this.gameGrid.GetCell(currentPos).entityData = null;
        }
    }

    public void AddToGraveyard(EntityData aEntityData) {
        UnRegisterEntityData(aEntityData);
        this.graveyardSet.Add(aEntityData);
    }

    public EntityData GetEntityDataAtPos(Vector2Int aPos) {
        if (IsPosInBoard(aPos)) {
            return this.gameGrid.GetCell(aPos).entityData;
        } else {
            return null;
        }
    }

    public void MoveEntity(Vector2Int aPos, EntityData aEntityData) {
        if (IsPosInBoard(aPos)) {
            foreach (Vector2Int currentPos in aEntityData.GetOccupiedPos()) {
                this.gameGrid.GetCell(currentPos).entityData = null;
            }
            aEntityData.SetPos(aPos);
            foreach (Vector2Int currentPos in aEntityData.GetOccupiedPos()) {
                this.gameGrid.GetCell(currentPos).entityData = aEntityData;
            }
        }
    }

    public Dictionary<Vector2Int, EntityData> EntityDataDictInRect(Vector2Int aOrigin, Vector2Int aSize) {
        Dictionary<Vector2Int, EntityData> entityDataInRect = new Dictionary<Vector2Int, EntityData>();
        foreach (Vector2Int currentPos in Util.V2IInRect(aOrigin, aSize)) {
            entityDataInRect[currentPos] = GetEntityDataAtPos(currentPos);
        }
        return entityDataInRect;
    }

    public bool IsRectEmpty(Vector2Int aOrigin, Vector2Int aSize, EntityData aIgnoreEntity = null) {
        foreach (Vector2Int currentPos in Util.V2IInRect(aOrigin, aSize)) {
            if (IsPosInBoard(currentPos)) {
                if (GetEntityDataAtPos(currentPos) != null) {
                    if (aIgnoreEntity != null) {
                        if (GetEntityDataAtPos(currentPos) != aIgnoreEntity) {
                            return false;
                        }
                    } else {
                        return false;
                    }
                }
            } else {
                return false;
            }
            
        }
        return true;
    }

    public bool IsEntityPosValid(Vector2Int aPos, EntityData aEntityData) {
        return IsRectEmpty(aPos, aEntityData.size, aEntityData);
    }

    public bool IsEntityPosFloating(Vector2Int aPos, EntityData aEntityData) {
        return IsRectEmpty(aPos, aEntityData.size, aEntityData);
    }
    public bool IsEntityOffsetValid(Vector2Int aOffset, EntityData aEntityData) {
        return IsRectEmpty(aEntityData.pos + aOffset, aEntityData.size, aEntityData);
    }

    public bool IsEntityFloating(EntityData aEntityData) {
        return IsEntityOffsetValid(Vector2Int.down, aEntityData);
    }

    public bool IsPosInBoard(Vector2Int aPos) {
        return Util.IsInside(aPos, Vector2Int.zero, this.size);
    }

    public bool IsRectInBoard(Vector2Int aOrigin, Vector2Int aSize) {
        return Util.IsRectInside(aOrigin, aSize, Vector2Int.zero, this.size);
    }
    
    public HashSet<EntityData> SetOfEntitiesInRect(Vector2Int aOrigin, Vector2Int aSize) {
        HashSet<EntityData> entitySet = new HashSet<EntityData>();
        foreach (Vector2Int pos in Util.V2IInRect(aOrigin, aSize)) {
            if (GM.boardData.IsPosInBoard(pos)) {
                EntityData maybeAEntity = GM.boardData.GetEntityDataAtPos(pos);
                if (maybeAEntity != null) {
                    entitySet.Add(GM.boardData.GetEntityDataAtPos(pos));
                }
            }
        }
        return entitySet;
    }

    public GameGrid GetGameGrid() {
        return this.gameGrid;
    }

}
