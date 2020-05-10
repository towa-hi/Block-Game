using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public struct EntitySaveData {
    public string name;
    public Vector2Int pos;
    public Vector2Int size;
    public EntityTypeEnum type;
    public Color defaultColor;
    public bool isFixed;
    public bool isBoundary;

    public HashSet<Vector2Int> upNodes;
    public HashSet<Vector2Int> downNodes;

    public EntitySaveData(EntityData aEntityData, EntityBase aEntityBase) {
        this.name = aEntityData.name;
        this.pos = aEntityData.pos;
        this.size = aEntityData.size;
        this.type = aEntityData.type;
        this.defaultColor = aEntityData.defaultColor;
        this.isFixed = aEntityData.isFixed;
        this.isBoundary = aEntityData.isBoundary;
        INodal nodal = aEntityBase.GetComponent<INodal>();
        if (nodal != null) {
            this.upNodes = nodal.upNodes;
            this.downNodes = nodal.downNodes;
        } else {
            upNodes = null;
            downNodes = null;
        }
    }
}

public struct BoardSaveData {
    // public GameGrid gameGrid;
    public HashSet<EntitySaveData> entityDataSet;
    public string title;
    public string creator;
    public int par;
    public Vector2Int size;
    public int attempts;

    public BoardSaveData(BoardData aBoardData) {
        // this.gameGrid = BoardData.GetGameGrid();
        this.entityDataSet = new HashSet<EntitySaveData>();
        foreach (EntityData entityData in aBoardData.entityDataSet) {
            EntitySaveData entitySaveData = new EntitySaveData(entityData, entityData.entityBase);
            this.entityDataSet.Add(entitySaveData);
        }
        this.title = aBoardData.title;
        this.creator = aBoardData.creator;
        this.par = aBoardData.par;
        this.size = aBoardData.size;
        this.attempts = aBoardData.attempts;
    }
}

// public struct GameGridSaveData {
//     Dictionary<Vector2Int, GameCellSaveData> gridDict;
    
// }
public class LevelSaveData {
    public BoardSaveData boardSaveData;
    
    public LevelSaveData() {
        this.boardSaveData = new BoardSaveData(GM.boardData);
    }

    public static void UnpackLevelSaveData() {
        
    }
}
