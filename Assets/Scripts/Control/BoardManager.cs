using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Schema;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

public delegate void OnUpdateBoardStateHandler(BoardState aBoardState);

public class BoardManager : SerializedMonoBehaviour {
    [SerializeField] BoardState boardState;
    public BoardState currentState {
        get { return this.boardState; }
    }
    // public Dictionary<Vector2Int, BoardCell> boardCellDict;
    public event OnUpdateBoardStateHandler OnUpdateBoardState;
    public Dictionary<int, EntityBase> entityBaseDict;

    #region Initialization

    // called by GM start
    public void InitializeStartingBoard() {
        InitBoard();
        AddBoundaryEntities();
    }
    
    // called by editManager or when coming back from playtest 
    public void LoadBoardStateFromFile(string aFilename = "PlayTestTemp.json") {
        print("attempting to load" + aFilename);
        BoardState loadedBoardState = GM.LoadBoardStateJson(aFilename);
        print(aFilename + "contains entity count of " + loadedBoardState.entityDict.Count);
        InitBoard(loadedBoardState);
        Debug.Log("loaded " + aFilename + " successfully");
    }

    void InitBoard(BoardState? aBoardState = null) {
        if (this.entityBaseDict != null) {
            foreach (EntityBase entityBase in this.entityBaseDict.Values) {
                Destroy(entityBase.gameObject);
            }
        }
        this.entityBaseDict = new Dictionary<int, EntityBase>();
        BoardState newBoardState = aBoardState ?? BoardState.GenerateBlankBoard();
        UpdateBoardState(newBoardState);
        foreach (EntityState entityState in newBoardState.entityDict.Values) {
            CreateEntityBase(entityState);
        }
    }

    // void InitBoard(BoardState? aBoardState = null) {
    //     if (this.entityBaseDict != null) {
    //         foreach (EntityBase entityBase in this.entityBaseDict.Values) {
    //             Destroy(entityBase.gameObject);
    //         }
    //     }
    //     this.boardCellDict = new Dictionary<Vector2Int, BoardCell>();
    //     foreach (Vector2Int pos in Util.V2IInRect(Vector2Int.zero, Constants.MAXBOARDSIZE)) {
    //         BoardCell currentCell = new BoardCell(pos);
    //         this.boardCellDict[pos] = currentCell;
    //     }
    //
    //     this.entityBaseDict = new Dictionary<int, EntityBase>();
    //     BoardState newBoardState = aBoardState ?? BoardState.GenerateBlankBoard();
    //     this.boardCellDict = new Dictionary<Vector2Int, BoardCell>();
    //     foreach (Vector2Int pos in Util.V2IInRect(Vector2Int.zero, newBoardState.size)) {
    //         this.boardCellDict[pos] = new BoardCell(pos);
    //     }
    //     UpdateBoardState(newBoardState);
    //     foreach (EntityState entityState in newBoardState.entityDict.Values) {
    //         CreateEntityBase(entityState);
    //     }
    // }
    //
    void AddBoundaryEntities() {
        EntitySchema tallBoy = Resources.Load<EntitySchema>("ScriptableObjects/Entities/Blocks/1x11 block");
        EntitySchema longBoy = Resources.Load<EntitySchema>("ScriptableObjects/Entities/Blocks/20x1 block");
        AddEntityFromSchema(longBoy, new Vector2Int(0, 0), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);
        AddEntityFromSchema(longBoy, new Vector2Int(20, 0), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);

        AddEntityFromSchema(longBoy, new Vector2Int(0, 23), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);
        AddEntityFromSchema(longBoy, new Vector2Int(20, 23), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);

        AddEntityFromSchema(tallBoy, new Vector2Int(0, 1), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);
        AddEntityFromSchema(tallBoy, new Vector2Int(0, 12), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);

        AddEntityFromSchema(tallBoy, new Vector2Int(39, 1), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);
        AddEntityFromSchema(tallBoy, new Vector2Int(39, 12), Constants.DEFAULTFACING, Constants.DEFAULTCOLOR, true, true);
    }
    
    #endregion

    #region Listeners

    // special function called by GM.OnUpdateGameState delegate
    public void OnUpdateGameState(GameState aGameState) {
        
    }

    #endregion
    
    #region BoardState

    void UpdateBoardState(BoardState aBoardState) {
        if (Config.PRINTLISTENERUPDATES) {
            print("BoardManager - Updating BoardState for " + OnUpdateBoardState?.GetInvocationList().Length + " delegates");
        }
        this.boardState = aBoardState;
        OnUpdateBoardState?.Invoke(this.currentState);
    }

    // void UpdateBoardState(BoardState aBoardState, HashSet<int> aEntitiesToUpdate = null) {
    //     if (Config.PRINTLISTENERUPDATES) {
    //         print("BoardManager - Updating BoardState for " + OnUpdateBoardState?.GetInvocationList().Length + " delegates");
    //     }
    //     if (aEntitiesToUpdate == null) {
    //         this.boardState = aBoardState;
    //         SetBoardCellDict(aBoardState);
    //         OnUpdateBoardState?.Invoke(this.currentState);
    //     }
    //     else {
    //         BoardState oldBoardState = this.boardState;
    //         foreach (int id in aEntitiesToUpdate) {
    //             EntityState oldEntityState = oldBoardState.entityDict[id];
    //             HashSet<Vector2Int> oldPosSet = Util.V2IInRect(oldEntityState.pos, oldEntityState.size).ToHashSet();
    //             foreach (Vector2Int pos in oldPosSet) {
    //                 BoardCell currentCell = this.boardCellDict[pos];
    //                 if (oldEntityState.isFront) {
    //                     currentCell.frontEntityId = null;
    //                 }
    //                 else {
    //                     currentCell.backEntityId = null;
    //                 }
    //                 this.boardCellDict[pos] = currentCell;
    //             }
    //         }
    //         foreach (int id in aEntitiesToUpdate) {
    //             EntityState newEntityState = aBoardState.entityDict[id];
    //             HashSet<Vector2Int> newPosSet = Util.V2IInRect(newEntityState.pos, newEntityState.size).ToHashSet();
    //             foreach (Vector2Int pos in newPosSet) {
    //                 BoardCell currentCell = this.boardCellDict[pos];
    //                 if (newEntityState.isFront) {
    //                     currentCell.frontEntityId = id;
    //                 }
    //                 else {
    //                     currentCell.backEntityId = id;
    //                 }
    //                 print("set pos " + pos + " to " + newEntityState.id);
    //                 this.boardCellDict[pos] = currentCell;
    //             }
    //         }
    //         this.boardState = aBoardState;
    //         foreach (int id in aEntitiesToUpdate) {
    //             GetEntityBaseById(id).OnUpdateBoardState(this.currentState);
    //         }
    //     }
    // }

    void UpdateEntityAndBoardState(EntityState aEntityState, HashSet<int> aEntitiesToUpdate = null) {
        BoardState newBoardState = BoardState.UpdateEntity(this.currentState, aEntityState);
        UpdateBoardState(newBoardState);
    }

    // void UpdateEntityAndBoardState(EntityState aEntityState, HashSet<int> aEntitiesToUpdate = null) {
    //     BoardState newBoardState = BoardState.UpdateEntity(this.currentState, aEntityState);
    //     UpdateBoardState(newBoardState, aEntitiesToUpdate);
    // }

    // void SetBoardCellDict(BoardState aBoardState) {
    //     List<Vector2Int> keys = this.boardCellDict.Keys.ToList();
    //     foreach (Vector2Int pos in keys) {
    //         BoardCell currentCell = this.boardCellDict[pos];
    //         currentCell.frontEntityId = null;
    //         currentCell.backEntityId = null;
    //         this.boardCellDict[pos] = currentCell;
    //     }
    //
    //     foreach (EntityState currentEntity in aBoardState.entityDict.Values) {
    //         foreach (Vector2Int currentPos in Util.V2IInRect(currentEntity.pos, currentEntity.size)) {
    //             BoardCell currentCell = this.boardCellDict[currentPos];
    //             if (currentEntity.isFront) {
    //                 currentCell.frontEntityId = currentEntity.id;
    //             }
    //             else {
    //                 currentCell.backEntityId = currentEntity.id;
    //             }
    //             print("set pos " + currentPos + " to " + currentEntity.id);
    //             this.boardCellDict[currentPos] = currentCell;
    //         }
    //     }
    // }

    public void SaveBoardState(bool aIsPlaytestTemp) {
        print("SaveBoardState");
        GM.SaveBoardStateJson(this.currentState, aIsPlaytestTemp);
    }

    public bool SetTitle(string aTitle) {
        if (0 < aTitle.Length && aTitle.Length <= Constants.MAXTITLELENGTH) {
            // TODO: validate titles so they can be valid filenames here and make input less hardass
            if (aTitle.IndexOfAny(Path.GetInvalidFileNameChars()) == -1) {
                BoardState newBoardState = BoardState.SetTitle(this.currentState, aTitle);
                UpdateBoardState(newBoardState);
                return true;
            }
            else {
                return false;
            }
        }
        else {
            throw new Exception("SetTitle - invalid title");
        }
    }

    public void SetPar(int aPar) {
        if (0 < aPar && aPar <= Constants.MAXPAR) {
            BoardState newBoardState = BoardState.SetPar(this.currentState, aPar);
            UpdateBoardState(newBoardState);
        }
    }

    #endregion
    
    #region Entity

    public void AddEntityFromSchema(EntitySchema aEntitySchema, Vector2Int aPos, Vector2Int aFacing, Color aDefaultColor, bool aIsFixed = false, bool aIsBoundary = false) {
        // if the area isn't clear, throw an exception
        if (!IsRectEmpty(aPos, aEntitySchema.size, null, aEntitySchema.isFront)) {
            throw new Exception("AddEntity - Position is invalid");
        }
        // generate a fresh entityState without an ID
        EntityState newEntityStateWithoutId = EntityState.CreateEntityState(aEntitySchema, aPos, aFacing, aDefaultColor, aIsFixed, aIsBoundary);
        // add it to the board and get the new boardState and entityState with ID back
        (BoardState newBoard, EntityState newEntityStateWithId) = BoardState.AddEntity(this.currentState, newEntityStateWithoutId);
        // update the boardState
        UpdateBoardState(newBoard);
        CreateEntityBase(newEntityStateWithId);
    }
    
    void CreateEntityBase(EntityState aEntityState) {
        if (this.entityBaseDict.ContainsKey(aEntityState.id)) {
            throw new Exception("TRIED TO OVERWRITE EXISTING ENTITYSTATE");
        }
        // set the new EntityPosition
        Vector3 newEntityPosition = Util.V2IOffsetV3(aEntityState.pos, aEntityState.size, aEntityState.isFront);
        // instantiate a new gameObject entityPrefab from the schemas prefabPath
        GameObject entityPrefab = Instantiate(GM.LoadEntityPrefabByFilename(aEntityState.prefabPath), newEntityPosition, Quaternion.identity,  this.transform);
        // get the entityBase
        EntityBase entityBase = entityPrefab.GetComponent<EntityBase>();
        // add it to the entityBaseDict

        this.entityBaseDict[aEntityState.id] = entityBase;
        // initialize entityBase with the newest state
        entityBase.Init(aEntityState);
    }
    
    public void RemoveEntity(int aId, bool aRemoveEntityBase = false) {
        if (aRemoveEntityBase) {
            EntityBase entityBase = this.entityBaseDict[aId];
            this.entityBaseDict.Remove(aId);
            Destroy(entityBase.gameObject);
        }
        // remove entity from boardstate
        BoardState newBoard = BoardState.RemoveEntity(this.currentState, aId);
        // update the boardState
        UpdateBoardState(newBoard);
    }

    public void RemoveEntityBase(int aId) {
        EntityBase entityBase = this.entityBaseDict[aId];
        this.entityBaseDict.Remove(aId);
        Destroy(entityBase.gameObject);
    }
    
    public void MoveEntity(int aId, Vector2Int aPos, bool aMoveEntityBase = false) {
        EntityState entityState = GetEntityById(aId);
        HashSet<int> ignoreSet = new HashSet<int> {aId};
        if (IsRectEmpty(aPos, entityState.size, ignoreSet, entityState.isFront)) {
            UpdateEntityAndBoardState(EntityState.SetPos(entityState, aPos), new HashSet<int>{aId});
            if (aMoveEntityBase) {
                entityState.entityBase.ResetView();
            }
        } else {
            throw new Exception("MoveEntity - invalid move id: " + aId + " to pos: " + aPos);
        }
    }
    public void MoveEntityBatch(HashSet<int> aEntityIdSet, Vector2Int aOffset, bool aMoveEntityBase = false) {
        Dictionary<int, EntityState> entityStateDict = new Dictionary<int, EntityState>();
        foreach (int id in aEntityIdSet) {
            EntityState movingEntity = this.currentState.entityDict[id];
            EntityState movedEntity = EntityState.SetPos(movingEntity, movingEntity.pos + aOffset);
            entityStateDict[id] = movedEntity;
        }
        UpdateBoardState(BoardState.UpdateEntityBatch(this.currentState, entityStateDict));
        if (aMoveEntityBase) {
            foreach (int id in aEntityIdSet) {
                GetEntityBaseById(id).ResetView();
            }
        }
    }
    // public void MoveEntityBatch(HashSet<int> aEntityIdSet, Vector2Int aOffset, bool aMoveEntityBase = false) {
    //     // BoardState newBoardState = BoardState.GetClone(this.currentState);
    //     Dictionary<int, EntityState> entityStateDict = new Dictionary<int, EntityState>();
    //     foreach (int id in aEntityIdSet) {
    //         EntityState movingEntity = this.currentState.entityDict[id];
    //         EntityState movedEntity = EntityState.SetPos(movingEntity, movingEntity.pos + aOffset);
    //         // newBoardState = BoardState.UpdateEntity(newBoardState, movedEntity);
    //         entityStateDict[id] = movedEntity;
    //     }
    //     UpdateBoardState(BoardState.UpdateEntityBatch(this.currentState, entityStateDict), aEntityIdSet);
    //     if (aMoveEntityBase) {
    //         foreach (int id in aEntityIdSet) {
    //             GetEntityBaseById(id).ResetView();
    //         }
    //     }
    // }

    public void SetEntityFacing(int aId, Vector2Int aFacing) {
        EntityState entityState = GetEntityById(aId);
        if (Util.IsDirection(aFacing)) {
            UpdateEntityAndBoardState(EntityState.SetFacing(entityState, aFacing), new HashSet<int>{aId});
        } else {
            throw new Exception("SetEntityFacing - invalid facing direction");
        }
    }

    public void SetEntityDefaultColor(int aId, Color aColor) {
        UpdateEntityAndBoardState(EntityState.SetDefaultColor(GetEntityById(aId), aColor), new HashSet<int>{aId});
    }

    public void SetEntityIsFixed(int aId, bool aIsFixed) {
        UpdateEntityAndBoardState(EntityState.SetIsFixed(GetEntityById(aId), aIsFixed), new HashSet<int>{aId});
    }

    public void SetEntityIsFixedBatch(IEnumerable<int> aIdSet, bool aIsFixed) {
        BoardState newBoardState = this.currentState;
        foreach (int id in aIdSet) {
            EntityState entityState = GetEntityById(id);
            entityState.isFixed = aIsFixed;
            newBoardState.entityDict = newBoardState.entityDict.SetItem(id, entityState);
            // newBoardState.entityDict[id] = entityState;
        }
        UpdateBoardState(newBoardState);
    }
    
    public void SetEntityTeam(int aId, TeamEnum aTeam) {
        UpdateEntityAndBoardState(EntityState.SetTeam(GetEntityById(aId), aTeam), new HashSet<int>{aId});
    }

    public void SetEntityTouchDefense(int aId, int aTouchDefense) {
        EntityState entityState = GetEntityById(aId);
        if (0 <= aTouchDefense && aTouchDefense <= 999) {
            UpdateEntityAndBoardState(EntityState.SetTouchDefense(entityState, aTouchDefense), new HashSet<int>{aId});
        }
        else {
            throw new Exception("SetEntityTouchDefense - invalid touchDefense");
        }
    }

    public void SetEntityFallDefense(int aId, int aFallDefense) {
        EntityState entityState = GetEntityById(aId);
        if (0 <= aFallDefense && aFallDefense <= 999) {
            UpdateEntityAndBoardState(EntityState.SetFallDefense(entityState, aFallDefense), new HashSet<int>{aId});
        }
        else {
            throw new Exception("SetEntityFallDefense - invalid touchDefense");
        }
    }

    #endregion

    #region Utility

    public EntityState? GetEntityAtPos(Vector2Int aPos, bool aIsFront = true) {
        return this.currentState.GetEntityAtPos(aPos, aIsFront);
    }

    // public EntityState? GetEntityAtPos(Vector2Int aPos, bool aIsFront = true) {
    //     if (!IsPosInBoard(aPos)) {
    //         return null;
    //     }
    //     BoardCell posCell = this.boardCellDict[aPos];
    //     if (aIsFront) {
    //         if (posCell.frontEntityId != null) {
    //             EntityState? entityState = GetEntityById(posCell.frontEntityId.Value);
    //             return entityState;
    //         }
    //         else {
    //             return null;
    //         }
    //     }
    //     else {
    //         if (posCell.backEntityId != null) {
    //             EntityState? entityState = GetEntityById(posCell.backEntityId.Value);
    //             return entityState;
    //         }
    //         else {
    //             return null;
    //         }
    //     }
    // }
    
    public EntityState? GetEntityAtMousePos(bool aIsFront = true) {
        if (this.currentState.IsPosOnBoard(GM.inputManager.mousePosV2)) {
            return GetEntityAtPos(GM.inputManager.mousePosV2, aIsFront);
        }
        else {
            return null;
        }
    }

    public EntityState GetEntityById(int aId) {
        return this.currentState.entityDict[aId];;
    }

    public EntityBase GetEntityBaseById(int aId) {
        return this.entityBaseDict[aId];
    }
    
    public bool CanEditorPlaceSchema(Vector2Int aPos, EntitySchema aEntitySchema) {
        // TODO: finish this
        if (IsRectEmpty(aPos, aEntitySchema.size, null, aEntitySchema.isFront)) {
            return true;
        }
        return false;
    }

    public bool IsPosInBoard(Vector2Int aPos) {
        return Util.IsInside(aPos, Vector2Int.zero, this.currentState.size);
    }

    public bool IsRectInBoard(Vector2Int aOrigin, Vector2Int aSize) {
        return Util.IsRectInside(aOrigin, aSize, Vector2Int.zero, this.currentState.size);
    }

    public Dictionary<Vector2Int, BoardCell> GetBoardGridSlice(Vector2Int aOrigin, Vector2Int aSize) {
        return this.currentState.GetBoardCellSlice(aOrigin, aSize);
    }

    // public Dictionary<Vector2Int, BoardCell> GetBoardGridSlice(Vector2Int aOrigin, Vector2Int aSize) {
    //     if (IsRectInBoard(aOrigin, aSize)) {
    //         Dictionary<Vector2Int, BoardCell> sliceDict = new Dictionary<Vector2Int, BoardCell>();
    //         foreach (Vector2Int currentPos in Util.V2IInRect(aOrigin, aSize)) {
    //             if (this.boardCellDict.ContainsKey(currentPos)) {
    //                 sliceDict[currentPos] = this.boardCellDict[currentPos];
    //             }
    //         }
    //         return sliceDict;
    //     }
    //     throw new ArgumentOutOfRangeException("GetBoardGridSlice - rect not in board" + aOrigin + aSize);
    // }

    public bool IsRectEmpty(Vector2Int aOrigin, Vector2Int aSize, HashSet<int> aIgnoreSet = null, bool aIsFront = true) {
        try {
            foreach (BoardCell boardCell in GetBoardGridSlice(aOrigin, aSize).Values) {
                int? id = aIsFront ? boardCell.frontEntityId : boardCell.backEntityId;
                if (id.HasValue) {
                    if (aIgnoreSet == null) {
                        return false;
                    }
                    if (!aIgnoreSet.Contains(id.Value)) {
                        return false;
                    }
                }
            }
            return true;
        }
        catch (ArgumentOutOfRangeException) {
            return false;
        }
    }

    #endregion
}