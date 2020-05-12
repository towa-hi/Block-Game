using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BoardData {

    public GameGrid gameGrid;
    public HashSet<EntityData> entityDataSet;
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

    public void InitEmptyBoard() {
        EntityData leftBoundary = new EntityData(EntityPrefabEnum.BLOCKPREFAB, new Vector2Int(1, 20), EntityTypeEnum.BLOCK, new Vector2Int(0, 0), Vector2Int.right, Constants.DEFAULTCOLOR, true, true);
        EntityData rightBoundary = new EntityData(EntityPrefabEnum.BLOCKPREFAB, new Vector2Int(1, 20), EntityTypeEnum.BLOCK, new Vector2Int(39, 0), Vector2Int.right, Constants.DEFAULTCOLOR, true, true);
        EntityData upBoundary = new EntityData(EntityPrefabEnum.BLOCKPREFAB, new Vector2Int(38, 1), EntityTypeEnum.BLOCK, new Vector2Int(1, 0), Vector2Int.right, Constants.DEFAULTCOLOR, true, true);
        EntityData downBoundary = new EntityData(EntityPrefabEnum.BLOCKPREFAB, new Vector2Int(38, 1), EntityTypeEnum.BLOCK, new Vector2Int(1, 19), Vector2Int.right, Constants.DEFAULTCOLOR, true, true);
        RegisterEntityData(leftBoundary);
        RegisterEntityData(rightBoundary);
        RegisterEntityData(upBoundary);
        RegisterEntityData(downBoundary);
        this.isInitialized = true;
    }

    public void RegisterEntityData(EntityData aEntityData) {
        this.entityDataSet.Add(aEntityData);
        foreach (Vector2Int currentPos in aEntityData.GetOccupiedPos()) {
            this.gameGrid.GetCell(currentPos).entityData = aEntityData;
        }
        Debug.Log("BoardData - RegisterEntity: " + aEntityData.name);
    }

    public void UnRegisterEntityData(EntityData aEntityData) {
        entityDataSet.Remove(aEntityData);
        foreach (Vector2Int currentPos in aEntityData.GetOccupiedPos()) {
            this.gameGrid.GetCell(currentPos).entityData = null;
        }
        Debug.Log("BoardData - UnRegisterEntity: " + aEntityData.name);
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
    
    public bool IsEntityPosGroundedAndValid(Vector2Int aPos, EntityData aEntityData) {
        if (IsEntityPosValid(aPos, aEntityData)) {
            // Debug.Log("entity pos is valid" + aPos);
            if (!IsRectEmpty(aPos + Vector2Int.down, aEntityData.size, aEntityData)) {
                // Debug.Log("entity pos is grounded returning true" + aPos);
                return true;
            } else {
                // Debug.Log("entity pos is not grounded" + aPos);
            }
        } else {
            // Debug.Log("entity pos is invalid" + aPos);
        }
        return false;
    }
    public GameGrid GetGameGrid() {
        return this.gameGrid;
    }

}
