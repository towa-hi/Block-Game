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

    public BoardData() {
        this.entityDataSet = new HashSet<EntityData>();
        this.title = "largeBlankBoard";
        this.creator = Config.USERNAME;
        this.par = 5;
        this.size = new Vector2Int(40, 20);
        this.gameGrid = new GameGrid(this.size);
        this.attempts = 0;
        EntityData leftBoundary = new EntityData(EntityPrefabEnum.BLOCKPREFAB, new Vector2Int(1, 20), EntityTypeEnum.BLOCK, new Vector2Int(0, 0), Vector2Int.right, Constants.DEFAULTCOLOR, true, true);
        EntityData rightBoundary = new EntityData(EntityPrefabEnum.BLOCKPREFAB, new Vector2Int(1, 20), EntityTypeEnum.BLOCK, new Vector2Int(39, 0), Vector2Int.right, Constants.DEFAULTCOLOR, true, true);
        EntityData upBoundary = new EntityData(EntityPrefabEnum.BLOCKPREFAB, new Vector2Int(38, 1), EntityTypeEnum.BLOCK, new Vector2Int(1, 0), Vector2Int.right, Constants.DEFAULTCOLOR, true, true);
        EntityData downBoundary = new EntityData(EntityPrefabEnum.BLOCKPREFAB, new Vector2Int(38, 1), EntityTypeEnum.BLOCK, new Vector2Int(1, 19), Vector2Int.right, Constants.DEFAULTCOLOR, true, true);
        RegisterEntityData(leftBoundary);
        RegisterEntityData(rightBoundary);
        RegisterEntityData(upBoundary);
        RegisterEntityData(downBoundary);
    }

    public void RegisterEntityData(EntityData aEntityData) {
        this.entityDataSet.Add(aEntityData);
        foreach (Vector2Int currentPos in aEntityData.GetOccupiedPos()) {
            this.gameGrid.GetCell(currentPos).entityData = aEntityData;
        }
        // Debug.Log("BoardData - RegisterEntity: " + aEntityData.entitySchema.name);
    }

    public void UnRegisterEntityData(EntityData aEntityData) {
        entityDataSet.Remove(aEntityData);
        foreach (Vector2Int currentPos in aEntityData.GetOccupiedPos()) {
            this.gameGrid.GetCell(currentPos).entityData = null;
        }
        // Debug.Log("BoardData - UnRegisterEntity: " + aEntityData.entitySchema.name);
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
            UnRegisterEntityData(aEntityData);
            aEntityData.SetPos(aPos);
            RegisterEntityData(aEntityData);
        }
    }

    public Dictionary<Vector2Int, EntityData> EntityDataDictInRect(Vector2Int aOrigin, Vector2Int aSize) {
        Dictionary<Vector2Int, EntityData> entityDataInRect = new Dictionary<Vector2Int, EntityData>();
        foreach (Vector2Int currentPos in Util.V2IInRect(aOrigin, aSize)) {
            entityDataInRect[currentPos] = GetEntityDataAtPos(currentPos);
        }
        return entityDataInRect;
    }

    public bool IsRectEmpty(Vector2Int aOrigin, Vector2Int aSize) {
        // Util.DebugAreaPulse(aOrigin, aSize);
        foreach (Vector2Int currentPos in Util.V2IInRect(aOrigin, aSize)) {
            if (GetEntityDataAtPos(currentPos) != null) {
                return false;
            }
        }
        return true;
    }

    public bool IsPosInBoard(Vector2Int aPos) {
        return Util.IsInside(aPos, Vector2Int.zero, this.size);
    }

    public bool IsRectInBoard(Vector2Int aOrigin, Vector2Int aSize) {
        return Util.IsRectInside(aOrigin, aSize, Vector2Int.zero, this.size);
    }
    
    public GameGrid GetGameGrid() {
        return this.gameGrid;
    }
}
