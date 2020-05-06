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

    public BoardData(LevelData aLevelData) {
        this.gameGrid = new GameGrid(aLevelData.levelSchema.size);
        this.entityDataSet = new HashSet<EntityData>();
        this.title = aLevelData.levelSchema.title;
        this.creator = aLevelData.levelSchema.creator;
        this.par = aLevelData.levelSchema.par;
        this.size = aLevelData.levelSchema.size;
        this.attempts = aLevelData.attempts;

        foreach (EntityData entityData in aLevelData.levelSchema.entityList) {
            RegisterEntityData(entityData);
        }
    }

    public void RegisterEntityData(EntityData aEntityData) {
        // TODO: remove this later
        if (aEntityData.name == null) {
            aEntityData.name = aEntityData.GenerateName();
        }
        this.entityDataSet.Add(aEntityData);
        foreach (Vector2Int currentPos in aEntityData.GetOccupiedPos()) {
            this.gameGrid.GetCell(currentPos).entityData = aEntityData;
        }
        Debug.Log("BoardData - RegisterEntity: " + aEntityData.entitySchema.name);
    }

    public void UnRegisterEntityData(EntityData aEntityData) {
        entityDataSet.Remove(aEntityData);
        foreach (Vector2Int currentPos in aEntityData.GetOccupiedPos()) {
            this.gameGrid.GetCell(currentPos).entityData = null;
        }
        Debug.Log("BoardData - UnRegisterEntity: " + aEntityData.entitySchema.name);
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

    
}
