using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Resources/ScriptableObjects", menuName = "ScriptableObjects/BoardData", order = 1)]
public class BoardData : SerializedScriptableObject {

    private static GameGrid gameGrid;
    public static HashSet<EntityData> entityDataSet;
    public static string title;
    public static string creator;
    public static int par;
    public static Vector2Int size;
    public static int attempts;

    public void Init(LevelData aLevelData) {
        BoardData.gameGrid = new GameGrid(aLevelData.levelSchema.size);
        BoardData.entityDataSet = new HashSet<EntityData>();
        BoardData.title = aLevelData.levelSchema.title;
        BoardData.creator = aLevelData.levelSchema.creator;
        BoardData.par = aLevelData.levelSchema.par;
        BoardData.size = aLevelData.levelSchema.size;
        BoardData.attempts = aLevelData.attempts;

        foreach (EntityData entityData in aLevelData.levelSchema.entityList) {
            RegisterEntityData(entityData);
        }
    }

    public static void RegisterEntityData(EntityData aEntityData) {
        // TODO: remove this later
        if (aEntityData.name == null) {
            aEntityData.name = aEntityData.GenerateName();
        }
        BoardData.entityDataSet.Add(aEntityData);
        foreach (Vector2Int currentPos in aEntityData.GetOccupiedPos()) {
            BoardData.gameGrid.GetCell(currentPos).entityData = aEntityData;
        }
        // Debug.Log("BoardData - RegisterEntity: " + aEntityData.entitySchema.name);
    }

    public static void UnRegisterEntityData(EntityData aEntityData) {
        entityDataSet.Remove(aEntityData);
        foreach (Vector2Int currentPos in aEntityData.GetOccupiedPos()) {
            BoardData.gameGrid.GetCell(currentPos).entityData = null;
        }
        // Debug.Log("BoardData - UnRegisterEntity: " + aEntityData.entitySchema.name);
    }

    public static EntityData GetEntityDataAtPos(Vector2Int aPos) {
        if (IsPosInBoard(aPos)) {
            return BoardData.gameGrid.GetCell(aPos).entityData;
        } else {
            return null;
        }
    }

    public static void MoveEntity(Vector2Int aPos, EntityData aEntityData) {
        if (IsPosInBoard(aPos)) {
            UnRegisterEntityData(aEntityData);
            aEntityData.SetPos(aPos);
            RegisterEntityData(aEntityData);
        }
    }

    public static Dictionary<Vector2Int, EntityData> EntityDataDictInRect(Vector2Int aOrigin, Vector2Int aSize) {
        Dictionary<Vector2Int, EntityData> entityDataInRect = new Dictionary<Vector2Int, EntityData>();
        foreach (Vector2Int currentPos in Util.V2IInRect(aOrigin, aSize)) {
            entityDataInRect[currentPos] = GetEntityDataAtPos(currentPos);
        }
        return entityDataInRect;
    }

    public static bool IsRectEmpty(Vector2Int aOrigin, Vector2Int aSize) {
        // Util.DebugAreaPulse(aOrigin, aSize);
        foreach (Vector2Int currentPos in Util.V2IInRect(aOrigin, aSize)) {
            if (GetEntityDataAtPos(currentPos) != null) {
                return false;
            }
        }
        return true;
    }

    public static bool IsPosInBoard(Vector2Int aPos) {
        return Util.IsInside(aPos, Vector2Int.zero, BoardData.size);
    }

    public static bool IsRectInBoard(Vector2Int aOrigin, Vector2Int aSize) {
        return Util.IsRectInside(aOrigin, aSize, Vector2Int.zero, BoardData.size);
    }
    
    public static GameGrid GetGameGrid() {
        return BoardData.gameGrid;
    }
}
