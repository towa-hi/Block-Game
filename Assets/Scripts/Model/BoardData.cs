using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BoardData {
    
    public GameGrid gameGrid;
    public HashSet<EntityData> entityDataSet;
    public BackgroundData backgroundData;
    public EntityData playerEntityData;
    public string title;
    public string creator;
    public int par;
    public Vector2Int size;
    public int attempts;
    // public bool isInitialized;

    public BoardData() {
        this.entityDataSet = new HashSet<EntityData>();
        this.backgroundData = new BackgroundData(this);
        this.title = "largeBlankBoard";
        this.creator = Config.USERNAME;
        this.par = 5;
        this.size = new Vector2Int(40, 24);
        this.gameGrid = new GameGrid(this.size);
        this.attempts = 0;
        
    }
    
    public void SetPlayerEntity(EntityData aEntityData) {
        this.playerEntityData = aEntityData;
    }

    // adds aEntityData to the entityDataSet and have the cells register it
    // does not remove self from board if already in
    public void RegisterEntityData(EntityData aEntityData) {
        this.entityDataSet.Add(aEntityData);
        foreach (Vector2Int currentPos in aEntityData.GetOccupiedPos()) {
            this.gameGrid.GetCell(currentPos).entityData = aEntityData;
        }
        // Debug.Log("BoardData - RegisterEntity: " + aEntityData.name);
    }

    // removes aEntityData from the entityDataSet and have the cells register it
    // does not remove entityBase from the actual game
    public void UnRegisterEntityData(EntityData aEntityData) {
        this.entityDataSet.Remove(aEntityData);
        foreach (Vector2Int currentPos in aEntityData.GetOccupiedPos()) {
            this.gameGrid.GetCell(currentPos).entityData = null;
        }
        // Debug.Log("BoardData - UnRegisterEntity: " + aEntityData.name);
    }

    // removes this aEntityData from cells in gameGrid, usually before moving or killing it
    public void BanishEntity(EntityData aEntityData) {
        foreach (Vector2Int currentPos in aEntityData.GetOccupiedPos()) {
            this.gameGrid.GetCell(currentPos).entityData = null;
        }
    }

    // gets EntityData at aPos
    public EntityData GetEntityDataAtPos(Vector2Int aPos) {
        if (IsPosInBoard(aPos)) {
            return this.gameGrid.GetCell(aPos).entityData;
        } else {
            return null;
        }
    }

    // removes aEntityData from gameGrid temporarily then sets entityData to new pos and adds it back in
    // does not change position of EntityView
    public void MoveEntity(Vector2Int aPos, EntityData aEntityData) {
        if (IsRectEmpty(aPos, aEntityData.size, aEntityData)) {
            BanishEntity(aEntityData);
            aEntityData.SetPos(aPos);
            foreach (Vector2Int currentPos in aEntityData.GetOccupiedPos()) {
                this.gameGrid.GetCell(currentPos).entityData = aEntityData;
            }
        } else {
            throw new System.Exception("MoveEntity tried to move to an non-empty position");
        }
    }

    // returns dict of positions and EntityData in rect. basically a slice of gameGrid.gridDict
    public Dictionary<Vector2Int, EntityData> EntityDataDictInRect(Vector2Int aOrigin, Vector2Int aSize) {
        Dictionary<Vector2Int, EntityData> entityDataInRect = new Dictionary<Vector2Int, EntityData>();
        foreach (Vector2Int currentPos in Util.V2IInRect(aOrigin, aSize)) {
            if (IsPosInBoard(currentPos)) {
                entityDataInRect[currentPos] = GetEntityDataAtPos(currentPos);
            } else {
                Debug.Log("EntityDataDictInRect - checked out of bounds pos");
            }
        }
        return entityDataInRect;
    }

    public Dictionary<Vector2Int, GameCell> GetCellSlice(Vector2Int aOrigin, Vector2Int aSize) {
        return this.gameGrid.GetSlice(aOrigin, aSize);
    }

    public HashSet<EntityData> GetEntitiesInRect(Vector2Int aOrigin, Vector2Int aSize, EntityData aIgnoreEntity = null) {
        HashSet<EntityData> entityDataSet = new HashSet<EntityData>();
        foreach (KeyValuePair<Vector2Int, GameCell> kvp in this.gameGrid.GetSlice(aOrigin, aSize)) {
            if (kvp.Value.entityData != null) {
                entityDataSet.Add(kvp.Value.entityData);
            }
        }
        entityDataSet.Remove(aIgnoreEntity);
        return entityDataSet;
    }

    // returns false if rect is out of bounds 
    public bool IsRectEmpty(Vector2Int aOrigin, Vector2Int aSize, EntityData aIgnoreEntity = null) {
        if (!IsRectInBoard(aOrigin, aSize)) {
            return false;
        }
        foreach (KeyValuePair<Vector2Int, GameCell> kvp in this.gameGrid.GetSlice(aOrigin, aSize)) {
            if (kvp.Value.entityData != null) {
                if (aIgnoreEntity != null) {
                    if (aIgnoreEntity != kvp.Value.entityData) {
                        return false;
                    }
                } else {
                    return false;
                }
            }
        }
        return true;
    }

    // returns false if rect is out of bounds but with set of entities to ignore
    public bool IsRectEmpty(Vector2Int aOrigin, Vector2Int aSize, HashSet<EntityData> aIgnoreEntitySet) {
        Debug.Assert(aIgnoreEntitySet != null);
        if (!IsRectInBoard(aOrigin, aSize)) {
            return false;
        }
        foreach (KeyValuePair<Vector2Int, GameCell> kvp in this.gameGrid.GetSlice(aOrigin, aSize)) {
            if (kvp.Value.entityData != null) {
                if (!aIgnoreEntitySet.Contains(kvp.Value.entityData)) {
                    return false;
                }
            }
        }
        return true;
    }

    // check if any entities exist below aEntityData when its in aPos
    public bool IsEntityPosFloating(Vector2Int aPos, EntityData aEntityData, HashSet<EntityData> aIgnoreEntitySet = null) {
        if (aIgnoreEntitySet != null) {
            // add self to list of exceptions
            aIgnoreEntitySet.Add(aEntityData);
            return IsRectEmpty(aPos + Vector2Int.down, aEntityData.size, aIgnoreEntitySet);
        } else {
            return IsRectEmpty(aPos + Vector2Int.down, aEntityData.size, aEntityData);
        }
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

    public bool EmptyStraightBetween(Vector2Int aStart, Vector2Int aEnd, EntityData aIgnore = null) {
        if (aStart.x == aEnd.x) {
            int minY = System.Math.Min(aStart.y, aEnd.y);
            int maxY = System.Math.Max(aStart.y, aEnd.y);
            for (int y = minY + 1; y < maxY; y++) {
                Vector2Int currentPos = new Vector2Int(aStart.x, y);
                EntityData maybeAEntity = GetEntityDataAtPos(currentPos);
                if (maybeAEntity != null && maybeAEntity != aIgnore) {
                    Util.DebugAreaPulse(currentPos, new Vector2Int(1,1), Color.red);
                    return false;
                } else {
                    Util.DebugAreaPulse(currentPos, new Vector2Int(1,1), Color.green);
                }
            }
            return true;
        } else if (aStart.y == aEnd.y) {
            int minX = System.Math.Min(aStart.x, aEnd.x);
            int maxX = System.Math.Max(aStart.x, aEnd.x);
            for (int x = minX + 1; x < maxX; x++) {
                Vector2Int currentPos = new Vector2Int(x, aStart.y);
                EntityData maybeAEntity = GetEntityDataAtPos(currentPos);
                if (maybeAEntity != null && maybeAEntity != aIgnore) {
                    Util.DebugAreaPulse(currentPos, new Vector2Int(1,1), Color.red);
                    return false;
                } else {
                    Util.DebugAreaPulse(currentPos, new Vector2Int(1,1), Color.green);
                }
            }
            return true;
        } else {
            throw new System.Exception("EmptyStraightBetween given invalid points");
        }
    }
}
